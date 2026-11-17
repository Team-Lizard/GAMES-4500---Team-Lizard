using System;
using Interactable;
using Unity.Multiplayer.PlayMode;
using Unity.VisualScripting;
using UnityEditor.Build.Pipeline;
using UnityEngine;
using UnityEngine.Assemblies;
using UnityEngine.InputSystem;

public class PlayerStateManager : MonoBehaviour
{
    [Tooltip("Starting health of the player.")]
    [SerializeField]
    private int m_startingHealth;

    private int m_currentHealth;

    void Start()
    {
        m_currentHealth = m_startingHealth;
    }

    public bool TryKillPlayer()
    {
        Debug.Log(m_currentHealth - 1);
        return --m_currentHealth <= 0;
    }

    public void Respawn()
    {
        m_currentHealth = m_startingHealth;
    }
}
