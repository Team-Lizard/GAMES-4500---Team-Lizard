using UnityEngine;
using UnityEngine.InputSystem.Controls;

public class MovingPlatform : MonoBehaviour
{
    private enum ObjectMoveState
    {
        Waiting,
        Moving
    }

    [Tooltip("Ending position of object in world space.")]
    [SerializeField]
    private Vector3 m_initialPosition;

    [Tooltip("Ending position of object in world space.")]
    [SerializeField]
    private Vector3 m_finalPosition;

    [Tooltip("Position of control point along line.")]
    [SerializeField]
    private Vector3 m_controlPosition;

    [Tooltip("Time it should take for object to get from one point to another.")]
    [SerializeField]
    private float m_transitTime;

    [Tooltip("Time object should wait at either position before moving.")]
    [SerializeField]
    private float m_waitTime;

    private float m_currentTime;
    private int m_moveDirection;
    private ObjectMoveState m_moveState;
    private Vector3 m_lastPosition;

    private Vector3 m_delta;
    private Vector3 m_totalDelta;
    
    // Start is called once before the first execution of Update after the MonoBehaviour is created
    private void Start()
    {
        m_currentTime = 0;
        m_moveDirection = -1;
        m_moveState = ObjectMoveState.Waiting;
    }

    // FixedUpdate is called once per physics frame
    private void FixedUpdate()
    {
        // Update delta by however much platform moved last physics frame.
        m_delta = transform.position - m_lastPosition;
        m_lastPosition = transform.position;

        // Accumulate movement to be applied all at once.
        m_totalDelta += m_delta;
        
        m_currentTime += Time.deltaTime;
        switch (m_moveState)
        {
            // Wait at endpoints
            case ObjectMoveState.Waiting:
                if (m_currentTime > m_waitTime)
                {
                    m_currentTime = 0;

                    // Switch direction we should be moving in.
                    m_moveDirection *= -1;
                    m_moveState = ObjectMoveState.Moving;
                }
                break;
            
            // Move between endpoints
            case ObjectMoveState.Moving:
                float scaledT = m_currentTime / m_transitTime;

                // Invert t if platform is moving backwards.
                scaledT = m_moveDirection > 0 ?  scaledT : 1 - scaledT;

                transform.position = BezierCurve.EvaluatePoint(m_initialPosition, m_finalPosition, m_controlPosition, scaledT);
                
                if (m_currentTime > m_transitTime)
                {
                    m_currentTime = 0;
                    m_moveState = ObjectMoveState.Waiting;
                }
                break;
        }
    }

    /// <summary>
    /// Get amount to move entity by to keep them on the platform.
    /// </summary>
    /// <returns> Amount the platform has moved by since last calling this function.</returns>
    public Vector3 GetTotalDelta()
    {
        Vector3 totalDelta = m_totalDelta;
        m_totalDelta = Vector3.zero;
        return totalDelta;
    }
}
