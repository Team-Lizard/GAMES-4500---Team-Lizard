using System.Collections.Generic;
using System.Globalization;
using System.IO;
using UnityEngine;
using Unity.VisualScripting;
using System.ComponentModel;



#if UNITY_EDITOR
using UnityEditor;
#endif

public class HeatmapVisualizer : MonoBehaviour
{
    [Header("Filter & Grid Settings")]
    [Tooltip("Exact event name to visualize (i.e., Position, Death, Jump). Leave empty for all.")]
    [SerializeField]
    private string m_eventFilter = "Position";

    [Tooltip("Size of each 3D heatmap cell in meters.")]
    [SerializeField]
    private float m_cellSize = 1.0f;

    [Tooltip("Min size.")]
    [Range(0.2f, 3.0f)]
    [SerializeField]
    private float m_minPointScale = 0.5f;

    [Tooltip("Max increase in size.")]
    [Range(0.2f, 3.0f)]
    [SerializeField]
    private float m_maxPointScaleIncrease = 0.5f;

    private int m_loadedSessions = 0;
    private int m_totalPoints = 0;
    private Dictionary<Vector3Int, int> m_densityGrid = new Dictionary<Vector3Int, int>();
    private int m_maxDensity = 1;

    public string DefaultLogFolder => Path.Combine(Application.persistentDataPath, "TelemetryLogs");

    public void LoadLogsFromFolder(string folderPath)
    {
        m_densityGrid.Clear();
        m_loadedSessions = 0;
        m_totalPoints = 0;
        m_maxDensity = 1;

        if (!Directory.Exists(folderPath))
        {
            Debug.LogWarning($"[Heatmap] Folder not found: {folderPath}");
            return;
        }

        string[] files = Directory.GetFiles(folderPath, "*.csv");
        foreach (string file in files)
        {
            m_loadedSessions++;
            string[] lines = File.ReadAllLines(file);

            // Skip header row (index 0)
            for (int i = 1; i < lines.Length; i++)
            {
                string[] parts = lines[i].Split(',');
                if (parts.Length < 5) {
                    continue;
                }

                string evt = parts[1].Trim();
                if (!string.IsNullOrEmpty(m_eventFilter) && evt != m_eventFilter)
                {
                    continue;
                } 

                // Help from https://discussions.unity.com/t/creating-a-heat-map-to-track-where-players-go/15861
                if (float.TryParse(parts[2], NumberStyles.Float, CultureInfo.InvariantCulture, out float x) &&
                    float.TryParse(parts[3], NumberStyles.Float, CultureInfo.InvariantCulture, out float y) &&
                    float.TryParse(parts[4], NumberStyles.Float, CultureInfo.InvariantCulture, out float z))
                {
                    Vector3Int cell = new Vector3Int(
                        Mathf.FloorToInt(x / m_cellSize),
                        Mathf.FloorToInt(y / m_cellSize),
                        Mathf.FloorToInt(z / m_cellSize)
                    );

                    if (!m_densityGrid.ContainsKey(cell)) 
                    {
                        m_densityGrid[cell] = 0;
                    }

                    m_densityGrid[cell]++;
                    m_totalPoints++;

                    if (m_densityGrid[cell] > m_maxDensity)
                    {
                        m_maxDensity = m_densityGrid[cell];
                    }
                        
                }
            }
        }

        Debug.Log($"[Heatmap] Loaded {m_totalPoints} points across {m_loadedSessions} CSV files.");
    }

    private void OnDrawGizmos()
    {
        if (m_densityGrid == null || m_densityGrid.Count == 0) {
            return;
        }

        foreach (var kvp in m_densityGrid)
        {
            float normalized = Mathf.Clamp01((float)kvp.Value / m_maxDensity);
            Color heatColor = Color.HSVToRGB(Mathf.Lerp(0.66f, 0f, normalized), 0.95f, 1f);
            heatColor.a = Mathf.Lerp(0.1f, 0.5f, normalized);

            Gizmos.color = heatColor;
            // Find center of cell for each key.
            Vector3 worldCenter = new Vector3(
                (kvp.Key.x + 0.5f) * m_cellSize,
                (kvp.Key.y + 0.5f) * m_cellSize,
                (kvp.Key.z + 0.5f) * m_cellSize
            );

            float scale = m_minPointScale + normalized * m_maxPointScaleIncrease;
            Gizmos.DrawCube(worldCenter, new Vector3(scale, scale, scale));
        }
    }
}

// Add buttons to inspector.
#if UNITY_EDITOR
[CustomEditor(typeof(HeatmapVisualizer))]
public class SpatialHeatmapVisualizerEditor : Editor
{
    public override void OnInspectorGUI()
    {
        DrawDefaultInspector();
        HeatmapVisualizer viz = (HeatmapVisualizer)target;

        GUILayout.Space(10);

        // Button to load playtest logs.
        if (GUILayout.Button("Load Playtest Logs (Default Folder)", GUILayout.Height(30)))
        {
            viz.LoadLogsFromFolder(viz.DefaultLogFolder);
            SceneView.RepaintAll();
        }

        // Button to load playtest logs from external source.
        if (GUILayout.Button("Load From Custom Folder (e.g., USB Drive)..."))
        {
            string selected = EditorUtility.OpenFolderPanel("Select Telemetry CSV Folder", "", "");
            if (!string.IsNullOrEmpty(selected))
            {
                viz.LoadLogsFromFolder(selected);
                SceneView.RepaintAll();
            }
        }

        // Open folder that logs are stored in.
        if (GUILayout.Button("Open Local Log Folder in Explorer/Finder"))
        {
            Directory.CreateDirectory(viz.DefaultLogFolder);
            EditorUtility.RevealInFinder(viz.DefaultLogFolder);
        }
    }
}
#endif