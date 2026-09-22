using System.Collections.Generic;
using UnityEngine;

public class ParkourManager : MonoBehaviour
{
    [SerializeField]
    [Tooltip("Drag and drop all Parkour Behavior Scriptable Objects into this list.")]
    private List<ParkourBehavior> m_parkourBehaviors;

    private ObstacleSensor m_obstacleSensor;

    private void Awake()
    {
        m_obstacleSensor = GetComponent<ObstacleSensor>();
    }

    public ParkourBehavior CheckParkourAction()
    {
        HitInfo hitInfo = m_obstacleSensor.ObstacleDetected();

        if (hitInfo.hitObstacle)
        {
            //Debug.Log($"Obstacle hit was: {hitInfo.hitData.transform.name}  -   Its height is: {hitInfo.obstacleHeight}");

            foreach (ParkourBehavior action in m_parkourBehaviors)
            {
                if (action.IsParkourActionPossible(hitInfo, transform))
                {
                    Debug.Log($"Parkour action is possible.");
                    return action;
                }
            }
        }

        return null;
    }
}
