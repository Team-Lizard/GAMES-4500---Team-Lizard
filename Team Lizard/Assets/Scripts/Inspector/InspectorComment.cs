using UnityEngine;

public class InspectorComment : MonoBehaviour
{
    [TextArea(3, 10)]
    public string note;

    // Automatically removes the component when the game starts
    private void Awake()
    {
        Destroy(this);
    }
}
