using UnityEngine;
using UnityEngine.ProBuilder.MeshOperations;

namespace Interactable
{
    public class InteractableTargetSetStaminaCost : MonoBehaviour, IInteractableTarget
    {

        [Tooltip("Ability to set stamina cost of.")]
        [SerializeField]
        private Ability m_ability;

        [Tooltip("Stamina cost to set.")]
        [SerializeField]
        private float m_value;

        [Tooltip("Player input controller to activate ability on.")]
        [SerializeField]
        private StaminaManager m_staminaManager;

        public void OnActivate()
        {
            m_staminaManager.ChangeAbilityStamina(m_ability, m_value);
        }
    }
}