using UnityEngine;

namespace Interactable
{
    public class InteractableButton : Interactable
    {

        /// <summary>
        /// clicking a button is likely going to have some effect on another object. This is that object.
        /// The use of InteractableTarget allows this class to call its interface method
        /// </summary>
        [SerializeField]
        [Tooltip("The GameObject that will be affected by the button.")]
        private MonoBehaviour m_resultObject;

        protected override void OnInteract()
        {
            if (!CanBeActivated())
            {
                return;
            }
            SetActivatedState();
            
            if (m_resultObject == null)
            {
                Debug.Log(" m_resultObject is not yet assigned. Look at InteractableButton.cs");
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
