using System.Collections.Generic;
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
        private List<MonoBehaviour> m_resultObjectsEnter;

        [SerializeField]
        [Tooltip("The GameObject that will be affected by the trigger area when the player enters.")]
        private List<MonoBehaviour> m_resultObjectsExit;
        
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

                m_resultObjects = m_resultObjectsEnter;

                if (m_resultObjectsEnter != null)
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

            m_resultObjects = m_resultObjectsExit;

            if (m_resultObjectsExit != null)
            {
                OnInteract();
            }
            
        }
    }
}