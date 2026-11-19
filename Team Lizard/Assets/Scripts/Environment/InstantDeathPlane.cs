using UnityEngine;

public class InstantDeathPlane : MonoBehaviour
{
    private void OnTriggerEnter(Collider other)
    {
        if (other.TryGetComponent(out CheckpointManager manager))
        {
            manager.Respawn();
        }
    }
}
