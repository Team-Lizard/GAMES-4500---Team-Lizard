using UnityEngine;
using System;

namespace Interactable
{   /// <summary>
    /// This is a class of GameObjects that holds an interaction collider
    /// </summary>
    public abstract class Interactable : MonoBehaviour
    {

        [SerializeField]
        [Tooltip("Whether the trigger area can be activated more than once.")]
        protected bool m_canReactivate;

        [SerializeField]
        [Tooltip("How long before the trigger area can be reactivated (only works if canReactivate is true).")]
        protected float m_cooldown;

        private bool m_hasActivated;
        private float m_lastPressedTime;

        protected virtual void Awake()
        {
            m_hasActivated = false;
        }

        /// <summary>
        /// run whenever another collider enters this trigger (mostly for checking if the player is within
        /// interaction distance of this item)
        /// </summary>
        /// <param name="other"> the other object (potentially a player) </param>
        protected virtual void OnTriggerEnter(Collider other)
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

        protected virtual void OnTriggerExit(Collider other)
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

        protected virtual bool CanBeActivated()
        {
            return !(m_hasActivated && (!m_canReactivate || (m_canReactivate && (Time.time - m_lastPressedTime < m_cooldown))));
        }

        protected virtual void SetActivatedState()
        {
            m_hasActivated = true;
            m_lastPressedTime = Time.time;
        }
        /// <summary>
        /// This function shouldn't be directly called, but is rather the behavior that is passed into the
        /// InteractionInput Action in the player controller.
        /// This defines the behavior for when this item is interacted with.
        /// </summary>
        protected abstract void OnInteract();
    }

}
