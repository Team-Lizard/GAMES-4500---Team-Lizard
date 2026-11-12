using UnityEngine;
using UnityEngine.ProBuilder.MeshOperations;

namespace Interactable
{
    public class InteractableTargetEnableAbility : MonoBehaviour, IInteractableTarget
    {

        [Tooltip("Ability to activate.")]
        [SerializeField]
        private Ability m_ability;

        [Tooltip("Ability to activate.")]
        [SerializeField]
        private InputController m_inputController;

        public void OnActivate()
        {
            m_inputController.ActivateAbility(m_ability);
        }
    }
}