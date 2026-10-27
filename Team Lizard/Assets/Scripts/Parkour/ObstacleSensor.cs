using Unity.VisualScripting;
using UnityEngine;
public struct HitInfo
{
    public bool HitObstacle;
    public RaycastHit HitData;
    public float ObstacleHeight;
    public Vector3 ForwardDirection;
}

public struct WallHitInfo
{
    public bool HitWall;
    public Vector3 Normal;
}

public class ObstacleSensor : MonoBehaviour
{
    [SerializeField]
    [Tooltip("Height to cast ray from.")]
    private float m_rayHeight = -0.5f;

    [SerializeField]
    [Tooltip("Length of the ray in the forward direction.")]
    private float m_rayLength = 1.75f;

    [Header("Wall Running")]
        [SerializeField]
        [Tooltip("Distance to check for a wall to the player's side.")]
        private float m_wallCheckDistance = 0.7f;

    private Vector3 m_rayOrigin;
    private LayerMask m_obstacleLayerMask;
    private LayerMask m_wallRunnableLayerMask;
    private HitInfo m_hitInfo = new HitInfo();
    private WallHitInfo m_wallHitInfo = new WallHitInfo();

    // Start is called once before the first execution of Update after the MonoBehaviour is created
    void Start()
    {
        m_obstacleLayerMask = LayerMask.GetMask("Obstacle");
        m_wallRunnableLayerMask = LayerMask.GetMask("Wallrunnable");
    }

    /// <summary>
    /// Checks if an obstacle is in front of the player.
    /// </summary>
    /// <returns>HitInfo struct representing info about what obstacle is in front of the player.</returns>
    public HitInfo DetectObstacle()
    {
        m_rayOrigin = transform.position + Vector3.up * m_rayHeight;

        m_hitInfo.HitObstacle = Physics.Raycast(m_rayOrigin, transform.forward, out m_hitInfo.HitData, m_rayLength, m_obstacleLayerMask);

        if (m_hitInfo.HitObstacle)
        {
            Collider hitCollider = m_hitInfo.HitData.collider;
            float distanceToTop = hitCollider.bounds.max.y - transform.position.y - m_rayHeight;
            m_hitInfo.ObstacleHeight = distanceToTop;
            m_hitInfo.ForwardDirection = -m_hitInfo.HitData.normal;
        }

        return m_hitInfo;
    }

    /// <summary>
    /// Checks for a wall-runnable surface on either side of the player.
    /// </summary>
    /// <param name="right">The player's right-facing direction.</param>
    /// <returns>WallHitInfo struct representing info about a nearby wall.</returns>
    public WallHitInfo DetectWall(Vector3 right)
    {
        if (Physics.Raycast(transform.position, right, out RaycastHit rightHit, m_wallCheckDistance, m_wallRunnableLayerMask))
        {
            m_wallHitInfo.HitWall = true;
            m_wallHitInfo.Normal = rightHit.normal;
            return m_wallHitInfo;
        }
        if (Physics.Raycast(transform.position, -right, out RaycastHit leftHit, m_wallCheckDistance, m_wallRunnableLayerMask))
        {
            m_wallHitInfo.HitWall = true;
            m_wallHitInfo.Normal = leftHit.normal;
            return m_wallHitInfo;
        }
        m_wallHitInfo.HitWall = false;
        return m_wallHitInfo;
    }
}
