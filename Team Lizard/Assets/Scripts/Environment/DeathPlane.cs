using UnityEngine;

public class DeathPlane : MonoBehaviour
{
    private void OnTriggerEnter(Collider other)
    {
        if (other.TryGetComponent(out CheckpointManager manager))
        {
            manager.Respawn();
        }
    }
}
