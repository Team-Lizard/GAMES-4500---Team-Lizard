using UnityEngine;

namespace Interactable
{
    public class InteractableTargetDoor : MonoBehaviour, IInteractableTarget
    {
        public void OnActivate()
        {
            Destroy(gameObject);
        }
    }
}
