using UnityEngine;
using System;

namespace Interactable
{   /// <summary>
    /// This is a class of GameObjects that holds an interaction collider
    /// </summary>
    public abstract class Interactable : MonoBehaviour
    {

        /// <summary>
        /// run whenever another collider enters this trigger (mostly for checking if the player is within
        /// interaction distance of this item)
        /// </summary>
        /// <param name="other"> the other object (potentially a player) </param>
        private void OnTriggerEnter(Collider other)
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

            controller.SubscribeToOnInteract(OnInteract);
        }

        private void OnTriggerExit(Collider other)
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

            controller.UnsubscribeToOnInteract(OnInteract);
        }

        /// <summary>
        /// This function shouldn't be directly called, but is rather the behavior that is passed into the
        /// InteractionInput Action in the player controller.
        /// This defines the behavior for when this item is interacted with.
        /// </summary>
        protected abstract void OnInteract();
    }

}
