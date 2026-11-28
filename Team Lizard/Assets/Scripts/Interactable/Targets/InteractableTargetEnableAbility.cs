using UnityEngine;
using UnityEngine.ProBuilder.MeshOperations;

namespace Interactable
{
    public class InteractableTargetEnableAbility : MonoBehaviour, IInteractableTarget
    {

        [Tooltip("Ability to activate.")]
        [SerializeField]
        private Ability m_ability;

        [Tooltip("Player input controller to activate ability on.")]
        [SerializeField]
        private StaminaManager m_staminaManager;

        public void OnActivate()
        {
            m_staminaManager.ActivateAbility(m_ability);
        }
    }
}