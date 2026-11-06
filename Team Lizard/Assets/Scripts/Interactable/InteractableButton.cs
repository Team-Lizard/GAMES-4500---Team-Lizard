using System.Collections.Generic;
using UnityEngine;

namespace Interactable
{
    public class InteractableButton : Interactable
    {
        [SerializeField]
        [Tooltip("Objects that will be activated by the button being pressed.")]
        private List<MonoBehaviour> m_buttonActivatedObjects;

        protected override void OnInteract()
        {
            m_resultObjects = m_buttonActivatedObjects;
            base.OnInteract();
        }
    }
}
