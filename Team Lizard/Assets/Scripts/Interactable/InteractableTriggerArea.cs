using Unity.VisualScripting;
using UnityEngine;

namespace Interactable
{
    public class InteractableTriggerArea : Interactable
    {
        /// <summary>
        /// clicking a button is likely going to have some effect on another object. This is that object.
        /// The use of InteractableTarget allows this class to call its interface method
        /// </summary>
        [SerializeField]
        [Tooltip("The GameObject that will be affected by the trigger area when the player enters.")]
        private MonoBehaviour m_resultObjectEnter;

        [SerializeField]
        [Tooltip("The GameObject that will be affected by the trigger area when the player enters.")]
        private MonoBehaviour m_resultObjectExit;

        private MonoBehaviour m_resultObject;



        protected override void OnTriggerEnter(Collider other)
        {
            if (!other.CompareTag("Player"))
                {
                    return;
                }

                FirstPersonController controller = other.GetComponent<FirstPersonController>();

                if (!controller)
                {
                    Debug.Log("FirstPersonController not found, Interactable.cs");
                    return;
                }

                m_resultObject = m_resultObjectEnter;

                if (m_resultObjectEnter != null)
                {
                    OnInteract();
                }
        }

        protected override void OnTriggerExit(Collider other)
        {
            if (!other.CompareTag("Player"))
            {
                return;
            }

            FirstPersonController controller = other.GetComponent<FirstPersonController>();

            if (!controller)
            {
                Debug.Log("FirstPersonController not found, Interactable.cs");
                return;
            }

            m_resultObject = m_resultObjectExit;

            if (m_resultObjectExit != null)
            {
                OnInteract();
            }
            
        }

        protected override void OnInteract()
        {
            if (!CanBeActivated())
            {
                return;
            }
            SetActivatedState();

            if (m_resultObject == null)
            {
                Debug.Log(" m_resultObject is not yet assigned. Look at InteractableTriggerArea.cs");
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
