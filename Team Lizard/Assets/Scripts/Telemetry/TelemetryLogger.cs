using System;
using System.IO;
using System.Text;
using UnityEngine;

public class TelemetryLogger : MonoBehaviour
{
    [Header("Tracking Settings")]
    [Tooltip("How often (in seconds) to record the player's position.")]
    [SerializeField]
    private float m_sampleInterval = 0.5f;

    [SerializeField]
    private bool m_recordMovement = true;

    private string m_filePath;
    private float m_timer;
    private StreamWriter m_writer;

    public static TelemetryLogger Instance { 
        get; 
        private set;
    }

    void Awake()
    {
        if (Instance != null && Instance != this)
        {
            Destroy(gameObject);
            return;
        }
        Instance = this;

        string folder = Path.Combine(Application.persistentDataPath, "TelemetryLogs");
        Directory.CreateDirectory(folder);

        string timestamp = DateTime.Now.ToString("yyyy-MM-dd-HH-mm-ss");
        m_filePath = Path.Combine(folder, $"Session_{timestamp}.csv");

        m_writer = new StreamWriter(m_filePath, false, Encoding.UTF8);
        m_writer.WriteLine("Time, Event, X, Y, Z");
        m_writer.Flush();

        Debug.Log($"[Telemetry] Recording locally to {m_filePath}");
    }

    // Update is called once per frame
    void Update()
    {
        if (!m_recordMovement)
        {
            return;
        }

        m_timer += Time.deltaTime;
        if (m_timer >= m_sampleInterval)
        {
            m_timer = 0f;
            Record("Position", transform.position);
        }
    }

    private void Record(string eventName, Vector3 pos)
    {
        if (m_writer == null) {
            return;
        }

        // Invariant culture ensures decimals always use '.' regardless of OS region settings
        string line = string.Format(
            System.Globalization.CultureInfo.InvariantCulture,
            "{0:F2},{1},{2:F3},{3:F3},{4:F3}",
            Time.timeSinceLevelLoad, eventName, pos.x, pos.y, pos.z
        );
        m_writer.WriteLine(line);
    }

    private void OnApplicationQuit()
    {
        CloseWriter();
    }

    private void OnDestroy()
    {
        CloseWriter();
    }

    private void CloseWriter()
    {
        if (m_writer != null)
        {
            m_writer.Flush();
            m_writer.Close();
            m_writer = null;
        }
    }
}
