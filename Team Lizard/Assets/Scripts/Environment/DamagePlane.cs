using UnityEngine;

public class DamagePlane : MonoBehaviour
{
    [Tooltip("Damage applied by this object.")]
    [SerializeField]
    private int m_damage;

    [Tooltip("Cooldown until damage is tried again.")]
    [SerializeField]
    private float m_cooldown;

    private float m_lastDamage;

    private void OnTriggerStay(Collider other)
    {
        // Wait for cooldown to end and get player state manager.
        if (Time.time - m_lastDamage > m_cooldown && other.TryGetComponent(out PlayerStateManager playerStateManager))
        {
            // Update when the last damage was tried.
            m_lastDamage = Time.time;
            
            // Try to kill the player and respawn them.
            if (playerStateManager.TryKillPlayer(m_damage) && other.TryGetComponent(out CheckpointManager checkpointManager))
            {
                checkpointManager.Respawn();
            }
        }
    }
}
