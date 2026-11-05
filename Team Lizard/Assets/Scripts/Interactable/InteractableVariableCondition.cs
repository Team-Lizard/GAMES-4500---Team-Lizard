using UnityEngine;

namespace Interactable
{
    public class InteractableVariableCondition : Interactable
    {
        [SerializeField]
        [Tooltip("The GameObject that will be affected by the condition being met.")]
        private MonoBehaviour m_resultObject;

        [SerializeField]
        private VariableCondition m_condition;

        protected override void OnInteract()
        {
            if (!CanBeActivated())
            {
                return;
            }
            SetActivatedState();

            if (m_resultObject == null)
            {
                Debug.Log(" m_resultObject is not yet assigned. Look at InteractableVariableCondition.cs");
                return;
            }

            if (m_resultObject is IInteractableTarget target)
            {
                target.OnActivate();
                return;
            }

            Debug.Log(" m_resultObject is not implementing IInteractableTarget.cs");


        }
    }
}
