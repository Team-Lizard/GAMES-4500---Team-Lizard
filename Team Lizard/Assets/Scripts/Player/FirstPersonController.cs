using System;
using Unity.Multiplayer.PlayMode;
using Unity.VisualScripting;
using UnityEditor.Build.Pipeline;
using UnityEngine;
using UnityEngine.Assemblies;
using UnityEngine.InputSystem;

public class FirstPersonController : MonoBehaviour
{
    [Header("Physics Constants")]
        [SerializeField]
        [Tooltip("Force of gravity applied to player.")]
        private float m_gravity = -9.81f;

    [Header("Friction")]
        [SerializeField]
        [Tooltip("Force of ground friction horizontally on player.")]
        private float m_groundFriction = 17f;

        [SerializeField]
        [Tooltip("Force of air friction horizontally on player.")]
        private float m_airFriction = 4f;

    [Header("Acceleration")]
        [SerializeField]
        [Tooltip("Speed of acceleration when on ground.")]
        private float m_groundAcceleration = 60f;

        [SerializeField]
        [Tooltip("Speed of acceleration when in air.")]
        private float m_airAcceleration = 20f;

    [Header("Camera")]
        [SerializeField]
        [Tooltip("Transform of object parenting camera.")]
        private Transform m_cameraTransform;

    [Header("Sliding Constants")]
        [SerializeField]
        [Tooltip("Height of sliding player in percentage of standing height.")]
        private float m_slideHeight = 0.5f;

        [SerializeField]
        [Tooltip("How fast to transition to and from slide.")]
        private float m_slideCameraLerp = 12f;

    [Header("Wall Running")]
        [SerializeField]
        [Tooltip("Multiplier applied to gravity while wall running (lower = floatier).")]
        private float m_wallRunGravityMultiplier = 0.2f;

        [SerializeField]
        [Tooltip("Degrees to roll the camera while wall running.")]
        private float m_wallRunCameraRoll = 15f;

        [SerializeField]
        [Tooltip("How fast the camera rolls into/out of a wall run.")]
        private float m_wallRunRollLerp = 10f;

        [SerializeField]
        [Tooltip("Seconds the player holds their height after latching onto a wall.")]
        private float m_wallRunHangTime = 0.7f;

        [SerializeField]
        [Tooltip("Seconds over which gravity ramps from zero to full wall-run gravity after the hang.")]
        private float m_wallRunSlideRampTime = 0.5f;

    [Header("Wwise Object")]
        [SerializeField]
        private AK.Wwise.Event m_jumpEvent;

    private float m_movementThreshold = 0.0001f;

    [Header("Snap Distance")]
        [SerializeField]
        [Tooltip("Distance to snap to ground")]
        private float m_snapDistance = 0.3f;

    private float m_wallRunTimer;
    private bool m_wasWallRunning;
    private float m_roll;

    private CharacterController m_characterController;

    private float m_verticalVelocity;
    private float m_pitch = 0f;
    private Vector3 m_horizontalVelocity;
    private float m_standHeight;
    private float m_standCameraY;
    private float m_standRadius;
    private Vector3 m_standCenter;
    private bool m_isCrouched = false;
    private bool m_snappedLastFrame;

    // item-specific data
    private MovingPlatform m_currentPlatform;
    private event Action m_interactableAction;

    /// <summary>
    /// Is the player currently touching the ground?
    /// </summary>
    public bool IsGrounded
    {
        get;
        private set;
    }

    /// <summary>
    /// A 3d vector representing the angle of the ground beneath the player.
    /// </summary>
    public Vector3 GroundNormal
    {
        get;
        private set;
    } = Vector3.up;

    /// <summary>
    /// Magnitude of gravity.
    /// </summary>
    public float GravityMagnitude => Mathf.Abs(m_gravity);

    private void Awake()
    {
        m_characterController = gameObject.GetComponent<CharacterController>();
        m_standHeight = m_characterController.height;
        m_standRadius = m_characterController.radius;
        m_standCameraY = m_cameraTransform.localPosition.y;
        m_standCenter = m_characterController.center;

        // empty action initialization
        m_interactableAction = new Action(() => { });
    }

    /// <summary>
    /// Attempts to move character according to requested movement and look vectors.
    /// </summary>
    /// <param name="request">MovementRequest struct holding a movement vector and a look vector.</param>
    public void ApplyMovement(MovementRequest request)
    {
       UpdateSlidePose(request.IsSliding);

        UpdateWallRunRoll(request.IsWallRunning, request.WallNormal);

        ApplyLook(request.LookDelta);

        ApplyGravity(request.IsWallRunning);

        Vector3 desired = request.DesiredVelocity;
        Vector3 target = new Vector3(desired.x, 0f, desired.z);
        UpdateHorizontalVelocity(target);

        // If player wants to jump, override vertical velocity.
        if (desired.y != 0)
        {
            m_verticalVelocity = desired.y * Mathf.Sqrt(-1*m_gravity);
            m_jumpEvent.Post(gameObject);
        }

        // Inertia comes from player input and friction.
        Vector3 displacement = new Vector3(m_horizontalVelocity.x, m_verticalVelocity, m_horizontalVelocity.z) * Time.deltaTime;
        if (m_currentPlatform != null)
        {
            displacement += m_currentPlatform.GetTotalDelta();
            m_currentPlatform.ActivatePlatform();
        }

        bool wasGrounded = IsGrounded;
        m_snappedLastFrame = false;
        m_characterController.Move(displacement);

        if (wasGrounded && !m_characterController.isGrounded
            && desired.y == 0f && !request.IsWallRunning && m_verticalVelocity <= 0f)
        {
            m_snappedLastFrame = TrySnap();
        }

        UpdateGroundNormal();

        HandleInteractions(request.IsInteracting);
    }

    /// <summary>
    /// Reset velocity of player for when they die.
    /// </summary>
    public void ResetVelocity()
    {
        m_horizontalVelocity = Vector3.zero;
        m_verticalVelocity = 0f;
    }

    /// <summary>
    /// Checks if player is touching ground and applies gravity otherwise.
    /// </summary>
    private void ApplyGravity(bool isWallRunning)
    {
        float gravity = m_gravity;

        if (isWallRunning)
        {
            if (!m_wasWallRunning)
            {
                // Just latched: stop all vertical motion and restart the timer.
                m_verticalVelocity = 0f;
                m_wallRunTimer = 0f;
            }

            m_wallRunTimer += Time.deltaTime;

            // 0 during the hang, then ramps up to the wall-run multiplier.
            float wallRunProgress = Mathf.InverseLerp(m_wallRunHangTime, m_wallRunHangTime + m_wallRunSlideRampTime, m_wallRunTimer);
            gravity = m_gravity * Mathf.Lerp(0f, m_wallRunGravityMultiplier, wallRunProgress);
        }
        m_wasWallRunning = isWallRunning;

        IsGrounded = m_characterController.isGrounded || m_snappedLastFrame;
        if (IsGrounded && m_verticalVelocity < 0)
        {
            // Player needs to be pressed into the ground for isGrounded to work correctly.
            m_verticalVelocity = m_gravity * Time.deltaTime;
        }
        else
        {
            // Player is not on ground, accelerate downwards.
            m_verticalVelocity += gravity * Time.deltaTime;

            // Player also isn't touching a platform
            m_currentPlatform = null;
        }
    }

    /// <summary>
    /// Move player velocity towards the desired velocity. If there is no player input, move towards no velocity.
    /// </summary>
    /// <param name="target">Target direction and velocity to reach.</param>
    private void UpdateHorizontalVelocity(Vector3 target)
    {
        bool hasInput = target.sqrMagnitude > m_movementThreshold;
        float dt = Time.deltaTime;
        
        if (IsGrounded)
        {
            float rate = hasInput ? m_groundAcceleration : m_groundFriction;
            m_horizontalVelocity = Vector3.MoveTowards(m_horizontalVelocity, target, rate * dt);
        }
        else if (hasInput)
        {
            // Don't let player input get faster than the magnitude we defined in the input controller.
            float speedCap = Mathf.Max(m_horizontalVelocity.magnitude, target.magnitude);

            // Move towards direction we have input in, clamping if we go beyond the max.
            m_horizontalVelocity += target.normalized * (m_airAcceleration * dt);
            m_horizontalVelocity = Vector3.ClampMagnitude(m_horizontalVelocity, speedCap);
        }
        else
        {
            // This is separate because we don't need to clamp our velocity as we move towards zero.
            m_horizontalVelocity = Vector3.MoveTowards(m_horizontalVelocity, Vector3.zero, m_airFriction * dt);
        }
    }

    /// <summary>
    /// Rotate player according to their requested input, to the best of our ability.
    /// </summary>
    /// <param name="lookInput">Vector2 representing how far the player would like to look in the yaw and pitch directions.</param>
    private void ApplyLook(Vector2 lookInput)
    {
        transform.Rotate(Vector3.up * lookInput.x);

        m_pitch -= lookInput.y;
        // Camera shouldn't be able to rotate further than straight up or straight down.
        m_pitch = Mathf.Clamp(m_pitch, -85f, 85f);

        m_cameraTransform.localRotation = Quaternion.Euler(m_pitch, 0, m_roll);
    }

    /// <summary>
    /// Rolls the camera toward or away from the wall while wall running.
    /// </summary>
    private void UpdateWallRunRoll(bool isWallRunning, Vector3 wallNormal)
    {
        float targetRoll = 0f;
        if (isWallRunning)
        {
            float side = Vector3.Dot(wallNormal, transform.right);
            targetRoll = -side * m_wallRunCameraRoll;
        }

        m_roll = Mathf.Lerp(m_roll, targetRoll, m_wallRunRollLerp * Time.deltaTime);
    }

    /// <summary>
    /// Puts camera lower to the ground when they are sliding or under a roof.
    /// </summary>
    private void UpdateSlidePose(bool isSliding)
    {
        bool shouldCrouch = isSliding || (m_isCrouched && !CanStand());

        // If sliding or under a short roof, do not get up.
        m_isCrouched = shouldCrouch;

        float targetHeight = m_isCrouched ? m_standHeight * m_slideHeight : m_standHeight;
        m_characterController.height = targetHeight;
        m_characterController.radius = Mathf.Min(m_standRadius, targetHeight * 0.5f);
        m_characterController.center = m_standCenter + Vector3.down * ((m_standHeight - targetHeight) * 0.5f);

        float intendedCameraHeight = CalculateIntendedCameraHeight();
        Vector3 currentCameraPosition = m_cameraTransform.localPosition;
        currentCameraPosition.y = Mathf.Lerp(currentCameraPosition.y, intendedCameraHeight, m_slideCameraLerp * Time.deltaTime);
        m_cameraTransform.localPosition = currentCameraPosition;
    }

    /// <summary>
    /// Tries to snap player to slope or step.
    /// </summary>
    private bool TrySnap()
    {
        float half = m_characterController.height * 0.5f;
        Vector3 origin = transform.position + m_characterController.center;
        float snap = m_snapDistance + new Vector2(m_horizontalVelocity.x, m_horizontalVelocity.z).magnitude * Time.deltaTime;

        if (Physics.Raycast(origin, Vector3.down, out RaycastHit hit, half + snap)
            && Vector3.Angle(hit.normal, Vector3.up) <= m_characterController.slopeLimit)
        {
            m_characterController.Move(Vector3.down * (hit.distance - half));
            m_verticalVelocity = -2f;

            return true;
        }

        return false;
    }

    /// <summary>
    /// Calculate the height of the camera based on if the player is sliding or standing.
    /// </summary>
    /// <returns>Height of the camera</returns>
    private float CalculateIntendedCameraHeight()
    {
        float bottomY = m_standCenter.y - m_standHeight * 0.5f;
        float cameraRelativeToBottom = m_standCameraY - bottomY;
        float crouchedCameraY = bottomY + cameraRelativeToBottom * m_slideHeight;
        return m_isCrouched ? crouchedCameraY : m_standCameraY;
    }

    /// <summary>
    /// Determines if the player can stand or not.
    /// </summary>
    /// <returns>True if the player can stand, false otherwise.</returns>
    private bool CanStand()
    {
        Vector3 origin = transform.position + m_characterController.center;
        float distance = m_standHeight - m_characterController.height * 0.5f + 0.05f;
        return !Physics.Raycast(origin, Vector3.up, distance);
    }

    /// <summary>
    /// Gets the slope of the ground beneath the player.
    /// </summary>
    private void UpdateGroundNormal()
    {
        GroundNormal = Vector3.up;

        if (!(m_characterController.isGrounded || m_snappedLastFrame))
        {
            return;
        }

        float half = m_characterController.height * 0.5f;
        Vector3 origin = transform.position + m_characterController.center;
        if (Physics.Raycast(origin, Vector3.down, out RaycastHit hit, half + m_snapDistance))
        {
            GroundNormal = hit.normal;
        }
    }

    /// <summary>
    /// When the player touches a platform, check if that platform is moving and assign it to the player if so.
    /// </summary>
    /// <param name="hit">Collider that was just touched.</param>
    private void OnControllerColliderHit(ControllerColliderHit hit)
    {
        MovingPlatform platform = hit.collider.GetComponent<MovingPlatform>();
        if (platform != null)
        {
            m_currentPlatform = platform;
            m_currentPlatform.GetTotalDelta();
        }
    }

    /// <summary>
    /// If the player is currently pressing the interact key, we try to interact with our environment
    /// </summary>
    /// <param name="isInteracting"> whether or not the player is trying to interact </param>
    private void HandleInteractions(bool isInteracting)
    {
        if (isInteracting)
        {
            m_interactableAction?.Invoke();
        }
    }

    /// <summary>
    /// CALLED FROM ANOTHER OBJECT!!!!!!!!!!
    /// passes in a new behavior that should be run when the interact input is pressed.
    /// </summary>
    /// <param name="action"> the new behavior to be executed if the interact input is pressed. </param>
    public void SubscribeToOnInteract(Action action)
    {
        m_interactableAction += action;
    }

    /// <summary>
    /// CALLED FROM ANOTHER OBJECT!!!!!!!!!!
    /// passes in a behavior that should be no longer run when the interact input is pressed.
    /// </summary>
    /// <param name="action"> the new behavior to be no longer executed if the interact input is pressed. </param>
    public void UnsubscribeToOnInteract(Action action)
    {
        m_interactableAction -= action;
    }
}
