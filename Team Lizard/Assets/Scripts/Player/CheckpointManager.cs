using UnityEngine;

public class CheckpointManager : MonoBehaviour
{
    [SerializeField]
    [Tooltip("Player spawn location before any checkpoints are reached.")]
    private Transform m_spawnPoint;

    private CharacterController m_characterController;
    private Vector3 m_checkpointPosition;
    private Quaternion m_checkpointRotation;
    private int m_checkpointOrder;

    private void Awake()
    {
        m_characterController = GetComponent<CharacterController>();
        m_checkpointPosition = m_spawnPoint.position;
        m_checkpointRotation = m_spawnPoint.rotation;
        m_checkpointOrder = -1;
    }

    /// <summary>
    /// Sets the new checkpoint. Skip the checkpoint reached if it is before intended checkpoint.
    /// </summary>
    /// <param name="position">Player's position on respawn.</param>
    /// <param name="rotation">Player's rotation on respawn.</param>
    /// <param name="order">Checkpoint's number.</param>
    public void SetCheckpoint(Vector3 position, Quaternion rotation, int order)
    {
        if (order <= m_checkpointOrder)
        {
            return;
        }

        m_checkpointPosition = position;
        m_checkpointRotation = rotation;
        m_checkpointOrder = order;
    }

    /// <summary>
    /// Teleports the player back to their last checkpoint.
    /// </summary>
    public void Respawn()
    {
        m_characterController.enabled = false;
        transform.SetPositionAndRotation(m_checkpointPosition, m_checkpointRotation);
        m_characterController.enabled = true;
    }
}
