using System;
using System.Collections.Generic;
using UnityEngine;

[Serializable]
public struct ParkourState
{
    public ParkourBehavior CurrentAction;
    public float AnimationTime;
    public float AnimationLength;
    public Vector3 StartingPosition;
    public Vector3 EndingPosition;
    public float ObstacleHeight;
}

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

    /// <summary>
    /// Check whether a parkour action is possible.
    /// </summary>
    /// <returns>ParkourState struct representing what action should be performed. The CurrentAction field will be null is no action is possible.</returns>
    public ParkourState CheckParkourAction()
    {
        ParkourState parkourState = new ParkourState
        {
            AnimationTime = 0f,
            StartingPosition = transform.position
        };

        HitInfo hitInfo = m_obstacleSensor.DetectObstacle();

        if (hitInfo.HitObstacle)
        {
            foreach (ParkourBehavior action in m_parkourBehaviors)
            {
                if (action.IsParkourActionPossible(hitInfo))
                {
                    // Parkour action is possible, fill in data about action.
                    parkourState.CurrentAction = action;
                    parkourState.ObstacleHeight = hitInfo.ObstacleHeight;
                    parkourState.AnimationLength = action.AnimationLength;

                    // We want to end up on other side of the object. This code could be adjusted if we want different behavior.
                    parkourState.EndingPosition = parkourState.StartingPosition + action.HorizontalDistance * hitInfo.ForwardDirection;
                }
            }
        }

        return parkourState;
    }

    /// <summary>
    /// Checks for a wall-runnable surface to either side of the player.
    /// </summary>
    public WallHitInfo CheckWall()
    {
        return m_obstacleSensor.DetectWall(transform.right);
    }
}
