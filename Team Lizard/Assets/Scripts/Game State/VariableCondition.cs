using Unity.GraphToolkit.Editor;
using Unity.VisualScripting;
using UnityEngine;
using System;

public enum ConditionType
{
    EQ,
    LT,
    LEQ,
    GT,
    GEQ,
    NEQ
}

[Serializable]
public class VariableCondition
{
    public string VariableName;
    public ConditionType condition;
    public float Value;

    public bool IsConditionMet()
    {
        
        switch (condition)
        {
            case ConditionType.LT:
                return VariableManager.Instance().GetVariableValue(VariableName) < Value;
            case ConditionType.LEQ:
                return VariableManager.Instance().GetVariableValue(VariableName) <= Value;
            case ConditionType.GT:
                return VariableManager.Instance().GetVariableValue(VariableName) > Value;
            case ConditionType.GEQ:
                return VariableManager.Instance().GetVariableValue(VariableName) >= Value;
            case ConditionType.NEQ:
                return VariableManager.Instance().GetVariableValue(VariableName) != Value;
            case ConditionType.EQ:
            default:
                return VariableManager.Instance().GetVariableValue(VariableName) == Value;
        }

        
    }
}
