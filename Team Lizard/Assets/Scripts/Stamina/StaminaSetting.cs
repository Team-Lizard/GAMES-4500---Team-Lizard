using System;
using UnityEngine;

[Serializable]
public class StaminaSetting
{
    [SerializeField]
    [Tooltip("Stamina cost to activate this ability.")]
    private float m_cost;
    [SerializeField]
    [Tooltip("Is this ability accessible when the game starts?")]
    private bool m_isEnabledOnStart;

    private bool m_isEnabled = false;

    /// <summary>
    /// Makes this ability accessible.
    /// </summary>
    public void Enable()
    {
        m_isEnabled = true;
    }

    /// <summary>
    /// Cost of ability, cost is above max stamina is ability isn't enabled.
    /// </summary>
    /// <returns></returns>
    public float GetCost()
    {
        // Return more than the max stamina if not enabled.
        return (m_isEnabled || m_isEnabledOnStart) ? m_cost : 101;
    }
}
