using System;
using System.Collections.Specialized;
using Unity.VisualScripting;
using UnityEditor.Build.Pipeline;
using UnityEngine;
using UnityEngine.InputSystem;

[Serializable]
public struct MovementRequest
{
    public Vector3 DesiredVelocity;
    public Vector2 LookDelta;
    public bool IsSliding;
    public bool IsWallRunning;
    public Vector3 WallNormal;
}

public class InputController : MonoBehaviour
{
    [Header("Movement")]
        [SerializeField]
        [Tooltip("Speed at which player should move.")]
        private float m_moveSpeed = 5.0f;

        [SerializeField]
        [Tooltip("Amount that speed is multiplied by when player is sprinting.")]
        private float m_sprintMultiplier = 1.8f;

        [SerializeField]
        [Tooltip("Amount that speed is multiplied by when player is sliding.")]
        private float m_slideMultiplier = 1.8f;

        [SerializeField]
        [Tooltip("Minimium speed while crouched as a multiplier.")]
        private float m_minimumSlideMultiplier = 0.5f;

        [SerializeField]
        [Tooltip("How fast slide decays.")]
        private float m_slideDecay = 1.8f;

        [SerializeField]
        [Tooltip("Height that player can jump.")]
        private float m_jumpHeight = 1.5f;

        [SerializeField]
        [Tooltip("Mouse sensitivity when controlling the camera.")]
        private float m_lookSensitivity = 1.5f;

        [SerializeField]
        [Tooltip("Number of jumps the player can perform while not touching the ground.")]
        private int m_maxExtraJumps = 1;

        [SerializeField]
        [Tooltip("Time in seconds after leaving the ground where the player can still jump.")]
        private float m_coyoteTime = 0.15f;

    [Header("Wall Running")]
        [SerializeField]
        [Tooltip("Speed multiplier applied while wall running.")]
        private float m_wallRunSpeedMultiplier = 1.2f;

        [SerializeField]
        [Tooltip("Outward force applied when jumping off a wall.")]
        private float m_wallJumpSideForce = 6f;

        [SerializeField]
        [Tooltip("Minimum forward input required to start or sustain a wall run.")]
        private float m_wallRunMinForwardInput = 0.3f;

        [SerializeField]
        [Tooltip("How long a wall run continues after the wall ray stops hitting.")]
        private float m_wallRunGraceTime = 0.2f;

        [SerializeField]
        [Tooltip("Time after a wall jump before the player can latch onto a wall again.")]
        private float m_wallJumpLockoutTime = 0.25f;

    private float m_wallGraceTimer;
    private float m_wallLockoutTimer;
    private bool m_isWallRunning;
    private Vector3 m_wallNormal;
    private Vector3 m_wallJumpVelocity;
    private bool m_isSprintHeld;
    private bool m_isSlideHeld;
    private bool m_isJumping;
    private bool m_isParkouring;
    private int m_extraJumps;
    private Vector2 m_moveInput;
    private Vector2 m_lookInput;
    private ParkourState m_parkourState;
    private float m_currentSlideMultiplier;
    private float m_coyoteTimer;

    private FirstPersonController m_firstPersonController;
    private ParkourManager m_parkourManager;

    private MovementRequest m_movementRequest;

    private void Awake()
    {
        m_firstPersonController = gameObject.GetComponent<FirstPersonController>();
        m_parkourManager = gameObject.GetComponent<ParkourManager>();
    }

    // Start is called once before the first execution of Update after the MonoBehaviour is created
    void Start()
    {
        Cursor.lockState = CursorLockMode.Locked;
        m_extraJumps = m_maxExtraJumps;
        m_currentSlideMultiplier = m_slideMultiplier;
    }

    // Update is called once per frame
    void Update()
    {
        Vector3 horizontalVelocity;
        float verticalVelocity;

        // If player is currently parkouring, override input and use input from parkour curve.
        if (m_parkourState.CurrentAction != null)
        {
            Vector3 curveDirection = m_parkourState.CurrentAction.Evaluate(
                m_parkourState.StartingPosition, 
                m_parkourState.EndingPosition, 
                m_parkourState.ObstacleHeight, 
                m_parkourState.AnimationTime);

            // Split found velocity into horizontal and vertical velocity (this will be recombined later.)
            horizontalVelocity = Vector3.forward * curveDirection.z + Vector3.right * curveDirection.x;
            verticalVelocity = curveDirection.y;

            // Step animation forward.
            m_parkourState.AnimationTime += Time.deltaTime;
            
            // Once the animation has played, clear the current parkour state.
            if (m_parkourState.AnimationTime > m_parkourState.AnimationLength)
            {
                m_parkourState.CurrentAction = null;
            }
            
        }
        // Otherwise, read player movement input.
        else
        {
            // Reset player's jumps if they are touching the ground.
            if (m_firstPersonController.IsGrounded)
            {
                m_extraJumps = m_maxExtraJumps;
                m_coyoteTimer = m_coyoteTime;
            }
            else
            {
                m_coyoteTimer -= Time.deltaTime;
            }

            if (m_isJumping)
            {
                m_parkourState = m_parkourManager.CheckParkourAction();
                if (m_parkourState.CurrentAction != null)
                {
                    m_isJumping = false;
                    return;
                }
            }

            HandleWallRun();

            // Calculate movement velocities.
            horizontalVelocity = HandleHorizontalMovement();
            verticalVelocity = HandleVerticalMovement();
        }

        // Combine horizontal and vertical velocities.
        m_movementRequest.DesiredVelocity = horizontalVelocity + verticalVelocity * Vector3.up;

        // Take the movement of the mouse and scale it by the look sensitivity. We'll calculate rotations additively, so we just need to know how far the mouse moved.
        m_movementRequest.LookDelta = new Vector2(m_lookInput.x, m_lookInput.y) * m_lookSensitivity;

        // Ask player controller to apply the movement the player wants.
        m_firstPersonController.ApplyMovement(m_movementRequest);
    }

    /// <summary>
    /// Handles wall running logic by calculating variables.
    /// </summary>
    private void HandleWallRun()
    {
        m_wallLockoutTimer -= Time.deltaTime;

        bool canWallRun = !m_firstPersonController.IsGrounded
                          && m_moveInput.y >= m_wallRunMinForwardInput
                          && m_wallLockoutTimer <= 0f;

        if (canWallRun)
        {
            WallHitInfo wall = m_parkourManager.CheckWall();
            if (wall.HitWall)
            {
                m_wallGraceTimer = m_wallRunGraceTime;
                m_wallNormal = wall.Normal;
            }
            else
            {
                m_wallGraceTimer -= Time.deltaTime;
            }
        }
        else
        {
            m_wallGraceTimer = 0f;
        }

        m_isWallRunning = m_wallGraceTimer > 0f;
        if (!m_isWallRunning)
        {
            m_wallNormal = Vector3.zero;
        }

        m_movementRequest.IsWallRunning = m_isWallRunning;
        m_movementRequest.WallNormal = m_wallNormal;
    }

    /// <summary>
    /// Handles calculating slide speed.
    /// </summary>
    /// <param name="moveSpeed">How fast the player is moving originally.</param>
    /// <returns>Float representing slide speed.</returns>
    private float HandleSlide(float moveSpeed)
    {
        if (m_currentSlideMultiplier > m_minimumSlideMultiplier)
        {
            m_currentSlideMultiplier -= m_slideDecay * Time.deltaTime;
        }

        return moveSpeed * m_currentSlideMultiplier;
    }

    /// <summary>
    /// Handles calculating jump velocity and cancels player input.
    /// </summary>
    /// <returns>Float velocity of jump.</returns>
    private float HandleJump()
    {
        // Jump calculation from Unity Documentation for Character Controller
        float verticalVelocity = Mathf.Sqrt(m_jumpHeight * 2.0f);
        m_isJumping = false;
        return verticalVelocity;
    }

    /// <summary>
    /// Handles permissions for vertical movement.
    /// </summary>
    /// <returns>Float vertical velocity.</returns>
    private float HandleVerticalMovement()
    {
        float verticalVelocity = 0;
        if (m_isJumping)
        {
            if (m_firstPersonController.IsGrounded || m_coyoteTimer > 0f)
            {
                verticalVelocity = HandleJump();
            }
            else if (m_extraJumps > 0)
            {
                verticalVelocity = HandleJump();
                m_extraJumps--;
            }
        }

        return verticalVelocity;
    }

    /// <summary>
    /// Calculates movement direction and scales it by speed.
    /// </summary>
    /// <returns>Vector3 representing movement velocity.</returns>
    private Vector3 HandleHorizontalMovement()
    {
        // Create movement vector based on stored player input.
        Vector3 move = transform.forward * m_moveInput.y + transform.right * m_moveInput.x;

        // Make sure diagonal movement isn't faster than unidirectional movement.
        move = Vector3.ClampMagnitude(move, 1f);

        float finalSpeed;
        if (m_isSlideHeld && m_firstPersonController.IsGrounded)
        {
            m_movementRequest.IsSliding = true;
            finalSpeed = HandleSlide(m_moveSpeed);
        }
        else
        {
            m_movementRequest.IsSliding = false;
            finalSpeed = m_isSprintHeld ? m_moveSpeed * m_sprintMultiplier : m_moveSpeed;
        }

        return move * finalSpeed;
    }

    /// ---------------
    /// Input Handling
    /// ---------------

    /// <summary>
    /// Called whenever movement input is received. Passes movement data to controller.
    /// </summary>
    /// <param name="value">Information about input being passed to controller.</param>
    public void OnMove(InputAction.CallbackContext value)
    {
        m_moveInput = value.ReadValue<Vector2>();
    }

    /// <summary>
    /// Called whenever look input is received. Passes look data to controller.
    /// </summary>
    /// <param name="value">Information about input being passed to controller.</param>
    public void OnLook(InputAction.CallbackContext value)
    {
        m_lookInput = value.ReadValue<Vector2>();
    }

    /// <summary>
    /// Called whenever sprint input is received. Passes sprint data to controller.
    /// </summary>
    /// <param name="value">Information about input being passed to controller.</param>
    public void OnSprint(InputAction.CallbackContext value)
    {
       m_isSprintHeld = value.ReadValueAsButton();
    }

    /// <summary>
    /// Called whenever jump input is received. Passes jump data to controller.
    /// </summary>
    /// <param name="value">Information about input being passed to controller.</param>
    public void OnJump(InputAction.CallbackContext value)
    {
        if (value.started)
        {
            m_isJumping = true;
        }
        else if (value.canceled)
        {
            m_isJumping = false;
        }
    }

    /// <summary>
    /// Called whenever slide input is received. Passes slide data to controller.
    /// </summary>
    /// <param name="value">Information about input being passed to controller.</param>
    public void OnSlide(InputAction.CallbackContext value)
    {
        if (value.started)
        {
            m_isSlideHeld = true;
        }
        else if (value.canceled)
        {
            m_isSlideHeld = false;
            m_currentSlideMultiplier = m_slideMultiplier;
        }
    }
}
