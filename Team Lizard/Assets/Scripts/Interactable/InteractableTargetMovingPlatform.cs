using Unity.Transforms;
using UnityEditor.UI;
using UnityEngine;

namespace Interactable
{
    public class InteractableTargetMovingPlatform : MovingPlatform, IInteractableTarget
    {
        public void Start()
        {
            base.Start();

            m_canStartOnOwn = false;
        }
        public void OnActivate()
        {
            m_canStartOnOwn = true;
        }
    }
}