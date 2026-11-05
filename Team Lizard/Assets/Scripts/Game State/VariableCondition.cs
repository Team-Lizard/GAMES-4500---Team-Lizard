using Unity.GraphToolkit.Editor;
using Unity.VisualScripting;
using UnityEngine;

public class VariableCondition : ScriptableObject
{
    public string VariableName;
    public float Value;

    public bool IsConditionMet()
    {
        return VariableManager.Instance().GetVariableValue(VariableName) == Value;
    }
}
