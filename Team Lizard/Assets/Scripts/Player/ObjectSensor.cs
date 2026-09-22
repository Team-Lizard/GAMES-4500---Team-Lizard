using UnityEngine;
using UnityEngine.Diagnostics;

public struct HitInfo
{
    public bool hitObstacle;
    public RaycastHit hitData;
    public float obstacleHeight;
    public Quaternion targetRotation;
}

public class ObjectSensor : MonoBehaviour
{
    [SerializeField]
    [Tooltip("Height to cast ray from.")]
    private float m_rayHeight = 0.1f;

    [SerializeField]
    [Tooltip("Length of the ray in the forward direction.")]
    private float m_rayLength = 1.75f;

    private Vector3 m_rayOrigin;
    private LayerMask m_obstacleLayerMask;
    private HitInfo m_hitInfo = new HitInfo();

    // Start is called once before the first execution of Update after the MonoBehaviour is created
    void Start()
    {
        m_obstacleLayerMask = LayerMask.GetMask("Obstacle");
    }

    // Update is called once per frame
    void Update()
    {
        
    }

    public HitInfo ObstacleDetected()
    {
        m_rayOrigin = transform.position + Vector3.up * m_rayHeight;

        m_hitInfo.hitObstacle = Physics.Raycast(m_rayOrigin, transform.forward, out m_hitInfo.hitData, m_rayLength, m_obstacleLayerMask);

        if (m_hitInfo.hitObstacle)
        {
            Debug.DrawRay(m_rayOrigin, transform.forward * m_rayLength, Color.green);

            Collider hitCollider = m_hitInfo.hitData.collider;
            m_hitInfo.obstacleHeight = hitCollider.bounds.size.y;
            m_hitInfo.targetRotation = Quaternion.LookRotation(-m_hitInfo.hitData.normal);

            Debug.Log($"Obstacle detected with height: {m_hitInfo.obstacleHeight}");
        }
        else
        {
            Debug.DrawRay(m_rayOrigin, transform.forward * m_rayLength, Color.red);
        }
        return m_hitInfo;
    }
}
