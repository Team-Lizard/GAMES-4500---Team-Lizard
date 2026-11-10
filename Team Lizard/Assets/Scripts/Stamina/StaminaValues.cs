using System;
using System.Collections.Generic;
using UnityEngine;

[CreateAssetMenu(fileName = "StaminaValues", menuName = "Scriptable Objects/StaminaValues")]
public class StaminaValues : ScriptableObject
{
    [Serializable]
    public enum StaminaActions
    {
        Run,
        Jump,
        Double_Jump,
        Wall_Jump,
        Wall_Run,
        Slide
    }

    [SerializeField]
    public Dictionary<StaminaActions, float> values = new Dictionary<StaminaActions, float>();
    
}
