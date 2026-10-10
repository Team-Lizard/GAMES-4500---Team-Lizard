using UnityEngine;

public class SoundTriggerScript : MonoBehaviour
{
    public AK.Wwise.Event soundEvent;
    private void OnTriggerEnter(Collider other)
    {
        if (other.CompareTag("Player"))
        {
            soundEvent.Post(gameObject);
        }
    }
}
