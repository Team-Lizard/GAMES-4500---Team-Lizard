using Unity.VectorGraphics;
using UnityEngine;
using UnityEngine.SceneManagement;

public class SceneLoader : MonoBehaviour
{
    [Tooltip("Name of scene to load when touching this object.")]
    [SerializeField]
    private string m_sceneName;

    private void OnTriggerEnter(Collider other)
    {
        if (other.TryGetComponent(out FirstPersonController controller))
        {
            SceneManager.LoadScene(m_sceneName);
        }
    }
}

