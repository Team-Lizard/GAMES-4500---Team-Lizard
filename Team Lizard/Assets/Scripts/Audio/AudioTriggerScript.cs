using UnityEngine;

public class AudioTriggerScript : MonoBehaviour
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
