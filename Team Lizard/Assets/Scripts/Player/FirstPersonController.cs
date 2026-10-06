using System;
using Unity.Multiplayer.PlayMode;
using Unity.VisualScripting;
using UnityEditor.Build.Pipeline;
using UnityEngine;
using UnityEngine.InputSystem;

public class FirstPersonController : MonoBehaviour
{
    [Header("Physics Constants")]
        [SerializeField]
        [Tooltip("Force of gravity applied to player.")]
        private float m_gravity = -9.81f;

        [SerializeField]
        [Tooltip("Force of ground friction horizontally on player.")]
        private float m_horizontalGroundFriction = 17f;

        [SerializeField]
        [Tooltip("Force of air friction horizontally on player.")]
        private float m_horizontalAirFriction = 4f;

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

    private float m_wallRunTimer;
    private bool m_wasWallRunning;
    private float m_roll;

    private CharacterController m_characterController;

    private float m_playerVelocityY;
    private float m_pitch = 0f;
    private Vector3 m_inertia;
    private float m_standHeight;
    private float m_standCameraY;
    private Vector3 m_standCenter;
    private bool m_isCrouched = false;
    private MovingPlatform m_currentPlatform;

    /// <summary>
    /// Is the player currently touching the ground?
    /// </summary>
    public bool IsGrounded
    {
        get;
        private set;
    }

    private void Awake()
    {
        m_characterController = gameObject.GetComponent<CharacterController>();
        m_standHeight = m_characterController.height;
        m_standCameraY = m_cameraTransform.localPosition.y;
        m_standCenter = m_characterController.center;
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

        // If player is standing on a moving platform, apply that platform's movement to the player.
        if (m_currentPlatform != null)
        {
            ApplyPlatformMovement();
        }
        
        // Apply friction if the player doesn't want to move, apply their movement otherwise.
        if (request.DesiredVelocity == Vector3.zero)
        {
            ApplyFriction();
        }
        else
        {
            ApplyMove(request.DesiredVelocity);
        }
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
                m_playerVelocityY = 0f;
                m_wallRunTimer = 0f;
            }

            m_wallRunTimer += Time.deltaTime;

            // 0 during the hang, then ramps up to the wall-run multiplier.
            float wallRunProgress = Mathf.InverseLerp(m_wallRunHangTime, m_wallRunHangTime + m_wallRunSlideRampTime, m_wallRunTimer);
            gravity = m_gravity * Mathf.Lerp(0f, m_wallRunGravityMultiplier, wallRunProgress);
        }
        m_wasWallRunning = isWallRunning;

        IsGrounded = m_characterController.isGrounded;
        if (IsGrounded && m_playerVelocityY < 0)
        {
            // Player needs to be pressed into the ground for isGrounded to work correctly.
            m_playerVelocityY = m_gravity * Time.deltaTime;
        }
        else
        {
            // Player is not on ground, accelerate downwards.
            m_playerVelocityY += gravity * Time.deltaTime;
            
            // Player also isn't touching a platform
            m_currentPlatform = null;
        }
    }

    /// <summary>
    /// Move player by whatever amount the platform they're standing on is moving.
    /// </summary>
    private void ApplyPlatformMovement()
    {
        Vector3 delta = m_currentPlatform.GetTotalDelta();
        m_characterController.Move(delta);
    }

    /// <summary>
    /// Player's momentum is preserved after they stop moving. Friction interpolates this momentum down to zero.
    /// </summary>
    private void ApplyFriction()
    {
        if (IsGrounded)
        {
            m_inertia = Vector3.MoveTowards(m_inertia, Vector3.zero, m_horizontalGroundFriction * Time.deltaTime);
        }
        else
        {
            m_inertia = Vector3.MoveTowards(m_inertia, Vector3.zero, m_horizontalAirFriction * Time.deltaTime);
        }
        m_characterController.Move(new Vector3(m_inertia.x, m_playerVelocityY, m_inertia.z) * Time.deltaTime);
    }

    /// <summary>
    /// Move player according to their requested input, to the best of our ability.
    /// </summary>
    /// <param name="moveInput">Vector3 representing requested movement.</param>
    private void ApplyMove(Vector3 moveInput)
    {
        if (moveInput.y != 0)
        {
            // If player wants to jump, override vertical velocity.
            m_playerVelocityY = moveInput.y * Mathf.Sqrt(-1*m_gravity);
        }

        m_characterController.Move(new Vector3(moveInput.x, m_playerVelocityY, moveInput.z) * Time.deltaTime);
        m_inertia = new Vector3(moveInput.x, 0, moveInput.z);
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

        m_characterController.height = m_isCrouched ? m_slideHeight : m_standHeight;
        m_characterController.center = m_standCenter + Vector3.down * ((m_standHeight - m_characterController.height) * 0.5f);

        float intendedCameraHeight = CalculateIntendedCameraHeight();
        Vector3 currentCameraPosition = m_cameraTransform.localPosition;
        currentCameraPosition.y = Mathf.Lerp(currentCameraPosition.y, intendedCameraHeight, m_slideCameraLerp * Time.deltaTime);
        m_cameraTransform.localPosition = currentCameraPosition;
    }

    /// <summary>
    /// Calculate the height of the camera based on if the player is sliding or standing.
    /// </summary>
    /// <returns>Height of the camera</returns>
    private float CalculateIntendedCameraHeight()
    {
        float bottomY = m_standCenter.y - m_standHeight * 0.5f;
        float cameraRelativeToBottom = m_standCameraY - bottomY;
        float crouchedCameraY = bottomY + cameraRelativeToBottom * (m_slideHeight / m_standHeight);
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
}
