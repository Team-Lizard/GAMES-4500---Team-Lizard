using UnityEngine;
using UnityEngine.ProBuilder.MeshOperations;

namespace Interactable
{
    public class InteractableTargetVariable : MonoBehaviour, IInteractableTarget
    {

        [Tooltip("Name of variable to set.")]
        [SerializeField]
        private string m_variableName;

        [Tooltip("What to set variable to.")]
        [SerializeField]
        private float m_value;

        [Tooltip("If true, value is added to the variable instead of directly set.")]
        private bool m_shouldIncrement = true;

        public void OnActivate()
        {
            if (m_shouldIncrement)
            {
                VariableManager.Instance().IncrementVariable(m_variableName, m_value);
            }
            else
            {
                VariableManager.Instance().SetVariable(m_variableName, m_value);
            }
            
        }
    }
}
