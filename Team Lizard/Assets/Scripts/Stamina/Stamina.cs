using UnityEngine;
using System;

/// <summary>
/// Responsible for storing stamina state and providing an API for others to use.
/// NOT responsible for updating the values outside the API definition.
/// </summary>
public class Stamina : ScriptableObject
{
    private static Stamina s_instance;
    private float m_maxStamina;
    private float m_currentStamina;


    // Other objects can subscribe to this, but only Stamina can invoke it.
    public event Action<float> StaminaGained;
    public event Action<float> StaminaSpent;

    /// <summary>
    /// Called once when the component is first loaded; this initializes all needed values
    /// </summary>
    private void Awake()
    {
        m_maxStamina = 100;
        m_currentStamina = m_maxStamina;
    }

    /// <summary>
    /// Attempts to spend a given amount of stamina
    /// </summary>
    /// <param name="amount"> the amount being spent; the 'cost' of an action </param>
    /// <returns> true if there is enough stamina to spend the specified amount, false otherwise </returns>
    public bool TrySpendStamina(float amount)
    {
        if (m_currentStamina < amount)
        {
            return false;
        }

        m_currentStamina -= amount;

        // Firing an event that other objects can listen to
        StaminaSpent?.Invoke(m_currentStamina);
        return true;
    }

    public void RegenerateStamina(float amount)
    {
        if (m_currentStamina == m_maxStamina)
        {
            return;
        }

        m_currentStamina = Math.Min(m_currentStamina + amount, m_maxStamina);
        StaminaGained?.Invoke(m_currentStamina);
    }

    /// <summary>
    /// gets the current amount of stamina left as a percentage
    /// </summary>
    /// <returns> the current amount of stamina left as a percentage </returns>
    public static float GetCurrentStaminaPercent()
    {
        return Instance().m_currentStamina;
    }

    /// <summary>
    /// Accesses the singleton instance of the Stamina class used in the above functions
    /// </summary>
    /// <returns> the singleton instance of the Stamina class </returns>
    public static Stamina Instance()
    {
        if (s_instance != null)
        {
            return s_instance;
        }

        s_instance = ScriptableObject.CreateInstance<Stamina>();
        return s_instance;
    }
}
