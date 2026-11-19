using UnityEngine;

namespace Interactable
{
    public class InteractableTargetDestroy : MonoBehaviour, IInteractableTarget
    {
        public void OnActivate()
        {
            Destroy(gameObject);
        }
    }
}
