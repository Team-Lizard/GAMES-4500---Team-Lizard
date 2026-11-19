using UnityEngine;
using UnityEngine.InputSystem;

public class GrappleController : MonoBehaviour
{
    [Header("Targeting")]
        [SerializeField]
        [Tooltip("Camera used to aim and to project the target indicator.")]
        private Camera m_camera;

        [SerializeField]
        [Tooltip("Layers containing objects that can be grappled to.")]
        private LayerMask m_grappleMask;

        [SerializeField]
        [Tooltip("Layers that block line of sight to a target (walls, floors, etc.).")]
        private LayerMask m_blockMask;

        [SerializeField]
        [Tooltip("Furthest you can grapple.")]
        private float m_maxRange = 30f;

        [SerializeField]
        [Tooltip("Max angle in degrees between the camera's forward and a target for it to be selectable. Larger is more forgiving.")]
        private float m_aimConeAngle = 12f;

    [Header("Movement")]
        [SerializeField]
        [Tooltip("How fast the rope shortens, in units per second.")]
        private float m_reelSpeed = 14f;

        [SerializeField]
        [Tooltip("How quickly the player accelerates toward reel speed, in units per second squared.")]
        private float m_reelAcceleration = 40f;

        [SerializeField]
        [Tooltip("Grapple auto-releases when the player is this close to the anchor.")]
        private float m_releaseDistance = 2f;

    [Header("Visuals")]
        [SerializeField]
        [Tooltip("UI element drawn over the current grapple target.")]
        private RectTransform m_indicator;

        [SerializeField]
        [Tooltip("Line renderer used to draw the rope.")]
        private LineRenderer m_rope;

        [SerializeField]
        [Tooltip("Where the rope starts (e.g. the hand or grapple gun).")]
        private Transform m_ropeOrigin;

        [SerializeField]
        [Tooltip("Seconds for the rope to travel from the origin to the anchor. Reeling starts once it arrives.")]
        private float m_ropeShootTime = 0.15f;

        [SerializeField]
        [Tooltip("Sideways wobble amplitude of the rope while it shoots out. Fades to zero on arrival.")]
        private float m_ropeWobble = 0.4f;

        [SerializeField]
        [Tooltip("Number of points in the rope line. Higher is smoother.")]
        private int m_ropeSegments = 24;

    /// <summary>
    /// Indicates whether player is grappling or not.
    /// </summary>
    public bool IsGrappling
    {
        get;
        private set;
    }

    /// <summary>
    /// Position of the object we are attached to.
    /// </summary>
    public Vector3 Anchor
    {
        get;
        private set;
    }

    /// <summary>
    /// The current length of the grapple.
    /// </summary>
    public float RopeLength
    {
        get;
        private set;
    }

    /// <summary>
    /// Whether or not the grappling hook is attached to something.
    /// </summary>
    public bool IsAttached => IsGrappling && m_attached;

    /// <summary>
    /// State of the grappling hook.
    /// </summary>
    public GrappleState State => new GrappleState
    {
        IsActive = IsAttached,
        Anchor = Anchor,
        RopeLength = RopeLength,
        ReelSpeed = m_reelSpeed,
        ReelAcceleration = m_reelAcceleration
    };

    private Collider m_target;
    private float m_timer;
    private bool m_attached;
    private readonly Collider[] m_results = new Collider[16];

    /// <summary>
    /// Called by InputController every frame, before building the movement request. Attempt to grapple.
    /// </summary>
    /// <param name="playerPosition">The player's current position.</param>
    /// <param name="deltaTime">The time between frames.</param>
    public void Tick(Vector3 playerPosition, float deltaTime)
    {
        if (!IsGrappling)
        {
            m_target = FindTarget();
            return;
        }

        m_timer += deltaTime;
        m_attached = m_timer >= m_ropeShootTime;

        if (!m_attached)
        {
            return;
        }

        float distance = Vector3.Distance(playerPosition, Anchor);
        if (distance < m_releaseDistance)
        {
            Release();
            return;
        }

        RopeLength = Mathf.Min(RopeLength, distance) - m_reelSpeed * deltaTime;
    }

    /// <summary>
    /// Input callback for the grapple action. Starts a grapple on press if a target is in range, and releases it when the button is let go.
    /// </summary>
    /// <param name="context">Information about the input being passed to the controller.</param>
    public void OnGrapple(InputAction.CallbackContext context)
    {
        if (context.started && m_target != null)
        {
            Anchor = m_target.bounds.center;
            RopeLength = Vector3.Distance(transform.position, Anchor);
            m_timer = 0f;
            m_attached = false;
            IsGrappling = true;
        }
        else if (context.canceled)
        {
            Release();
        }
    }

    /// <summary>
    /// Release the grappling hook.
    /// </summary>
    public void Release()
    {
        IsGrappling = false;
        m_attached = false;
    }

    /// <summary>
    /// Finds the relevant grapplable object and returns it.
    /// </summary>
    /// <returns>Object to grapple to.</returns>
    private Collider FindTarget()
    {
        Vector3 camPos = m_camera.transform.position;
        int count = Physics.OverlapSphereNonAlloc(camPos, m_maxRange, m_results, m_grappleMask);

        Collider best = null;
        float bestAngle = m_aimConeAngle;
        for (int i = 0; i < count; i++)
        {
            Vector3 point = m_results[i].bounds.center;
            float angle = Vector3.Angle(m_camera.transform.forward, point - camPos);
            if (angle >= bestAngle || Physics.Linecast(camPos, point, m_blockMask))
            {
                continue;
            }

            best = m_results[i];
            bestAngle = angle;
        }
        return best;
    }

    /// <summary>
    /// Updates the aim indicator and rope visuals after all movement for the frame has finished.
    /// </summary>
    private void LateUpdate()
    {
        UpdateIndicator();
        UpdateRope();
    }

    /// <summary>
    /// Shows the indicator over the current grapple target while not grappling, and hides it otherwise.
    /// </summary>
    private void UpdateIndicator()
    {
        if (m_indicator == null)
        {
            return;
        }

        bool show = m_target != null && !IsGrappling;
        m_indicator.gameObject.SetActive(show);

        if (show)
        {
            m_indicator.position = m_camera.WorldToScreenPoint(m_target.bounds.center);
        }
    }

    /// <summary>
    /// Draws the rope from the rope origin toward the anchor. The rope shoots out over its travel time, with a wobble that fades as it arrives.
    /// </summary>
    private void UpdateRope()
    {
        m_rope.enabled = IsGrappling;
        if (!IsGrappling)
        {
            return;
        }

        float t = Mathf.Clamp01(m_timer / m_ropeShootTime);
        Vector3 start = m_ropeOrigin.position;
        Vector3 end = Vector3.Lerp(start, Anchor, t);
        Vector3 side = Vector3.Cross((end - start).normalized, m_camera.transform.up);

        m_rope.positionCount = m_ropeSegments;
        for (int i = 0; i < m_ropeSegments; i++)
        {
            float f = i / (float)(m_ropeSegments - 1);
            float amp = (1f - t) * m_ropeWobble * Mathf.Sin(f * Mathf.PI) * Mathf.Sin(f * 12f + Time.time * 25f);
            m_rope.SetPosition(i, Vector3.Lerp(start, end, f) + side * amp);
        }
    }
}
