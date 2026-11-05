using System.Collections.Generic;
using UnityEngine;

public class VariableManager : ScriptableObject
{

    private static VariableManager s_instance;

    private Dictionary<string, float> m_variableStorage;

    private void Awake()
    {
        m_variableStorage = new Dictionary<string, float>();
    }

    public void SetVariable(string name, float value)
    {
        if (m_variableStorage.ContainsKey(name))
        {
            m_variableStorage[name] = value;
        }
        else
        {
            m_variableStorage.Add(name, value);
        }
        
        Debug.Log(GetVariableValue(name));
    }

    public float GetVariableValue(string name)
    {
        if (m_variableStorage.ContainsKey(name))
        {
            return m_variableStorage[name];
        }
        else
        {
            return 0;
        }
        
    }

    public void IncrementVariable(string name, float amount = 1)
    {
        SetVariable(name, GetVariableValue(name) + amount);
    }

    public static VariableManager Instance()
    {
        if (s_instance != null)
        {
            return s_instance;
        }

        s_instance = ScriptableObject.CreateInstance<VariableManager>();
        return s_instance;
    }
}
