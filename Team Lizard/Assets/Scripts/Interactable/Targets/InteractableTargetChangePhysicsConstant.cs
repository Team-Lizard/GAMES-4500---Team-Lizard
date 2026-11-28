using Unity.VisualScripting;
using UnityEngine;
using UnityEngine.ProBuilder.MeshOperations;

namespace Interactable
{
    public class InteractableTargetChangePhysicsConstant : MonoBehaviour, IInteractableTarget
    {

        [Tooltip("Constant to change.")]
        [SerializeField]
        private PhysicsConstant m_physicsConstant;

        [Tooltip("Value to set constant to.")]
        [SerializeField]
        private float m_value;

        [Tooltip("Player controller to change constant on.")]
        [SerializeField]
        private FirstPersonController m_firstPersonController;

        public void OnActivate()
        {
            m_firstPersonController.ChangePhysicsConstant(m_physicsConstant, m_value);
        }
    }
}