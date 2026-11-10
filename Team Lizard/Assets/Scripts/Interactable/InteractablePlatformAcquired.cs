using System.Collections.Generic;
using Unity.VisualScripting;
using UnityEngine;

namespace Interactable
{
    public class InteractablePlatformAcquired : Interactable
    {
        /// <summary>
        /// clicking a button is likely going to have some effect on another object. This is that object.
        /// The use of InteractableTarget allows this class to call its interface method
        /// </summary>
        [SerializeField]
        [Tooltip("The GameObject that will be affected by the trigger area when the player enters.")]
        private List<MonoBehaviour> m_platformActivatedObjects;


        public void OnPlatformAcquired()
        {
            m_resultObjects = m_platformActivatedObjects;

            if (m_resultObjects != null)
            {
                OnInteract();
            }
        }
    }
}