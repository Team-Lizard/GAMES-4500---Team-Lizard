using System;
using UnityEngine;

[Serializable]
public class StaminaSetting
{
    [SerializeField]
    private float m_cost;
    [SerializeField]
    private bool m_isEnabledOnStart;

    private bool m_isEnabled = false;

    public void Enable()
    {
        m_isEnabled = true;
    }

    public float GetCost()
    {
        // Return more than the max stamina if not enabled.
        return (m_isEnabled || m_isEnabledOnStart) ? m_cost : 101;
    }
}
