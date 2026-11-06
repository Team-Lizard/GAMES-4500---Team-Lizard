using UnityEngine;
using System;
using System.Collections.Generic;

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

        [SerializeField]
        [Tooltip("Condition that's required for this trigger to work. Setting no condition makes the trigger always work.")]
        protected VariableCondition m_condition;

        protected List<MonoBehaviour> m_resultObjects;

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
            bool cooldownPassed = m_canReactivate && (Time.time - m_lastPressedTime < m_cooldown);
            bool conditionMet = (m_condition == null) ? true : m_condition.IsConditionMet();
            return !(m_hasActivated && (!m_canReactivate || cooldownPassed)) && conditionMet;
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
        protected virtual void OnInteract()
        {
            if (!CanBeActivated())
            {
                return;
            }
            SetActivatedState();

            foreach (MonoBehaviour resultObject in m_resultObjects)
            {
                if (resultObject == null)
                {
                    Debug.Log(" resultObject is not yet assigned. Look at InteractableButton.cs");
                    return;
                }

                if (resultObject is IInteractableTarget target)
                {
                    target.OnActivate();
                    return;
                }

                Debug.Log(" resultObject is not implementing IInteractableTarget.cs");
            }
        }
    }

}
