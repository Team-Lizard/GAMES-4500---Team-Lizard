using UnityEngine;

public class Checkpoint : MonoBehaviour
{
    [SerializeField]
    [Tooltip("Order of this checkpoint in the level. Higher values override lower ones.")]
    private int m_order;

    private void OnTriggerEnter(Collider other)
    {
        if (other.TryGetComponent(out CheckpointManager manager))
        {
            manager.SetCheckpoint(transform.position, transform.rotation, m_order);
        }
    }
}
