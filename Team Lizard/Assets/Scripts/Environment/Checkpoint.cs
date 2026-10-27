using UnityEngine;

public class Checkpoint : MonoBehaviour
{
    [SerializeField]
    [Tooltip("Where the player respawns. If unset, uses this object's own transform.")]
    private Transform m_spawnpoint;

    [SerializeField]
    private int m_order;

    private void OnTriggerEnter(Collider other)
    {
        if (other.TryGetComponent(out CheckpointManager manager))
        {
            Transform spawn = m_spawnpoint != null ? m_spawnpoint : transform;
            manager.SetCheckpoint(spawn.position, spawn.rotation, m_order);
        }
    }
}
