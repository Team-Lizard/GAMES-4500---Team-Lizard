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
        [Tooltip("Height that player can jump.")]
        private float m_jumpHeight = 1.5f;

        [SerializeField]
        [Tooltip("Mouse sensitivity when controlling the camera.")]
        private float m_lookSensitivity = 1.5f;

        [SerializeField]
        [Tooltip("Number of jumps the player can perform while not touching the ground.")]
        private int m_maxExtraJumps = 1;
    
    private bool m_isSprintHeld;
    private bool m_isSlideHeld;
    private bool m_isJumping;
    private int m_extraJumps;
    private Vector2 m_moveInput;
    private Vector2 m_lookInput;
    private float m_currentSlideMultiplier;

    private FirstPersonController m_firstPersonController;

    private MovementRequest m_movementRequest;

    private void Awake()
    {
        m_firstPersonController = gameObject.GetComponent<FirstPersonController>();
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
        // Reset player's jumps if they are touching the ground.
        if (m_firstPersonController.IsGrounded)
        {
            m_extraJumps = m_maxExtraJumps;
        }

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

        Vector3 horizontalVelocity = move * finalSpeed;

        float verticalVelocity = 0;
        if (m_isJumping)
        {
            if (m_firstPersonController.IsGrounded)
            {
                verticalVelocity = HandleJump();
            }
            else if (m_extraJumps > 0) 
            {
                verticalVelocity = HandleJump();
                m_extraJumps--;
            }
        }
        
        // Combine horizontal and vertical velocities.
        m_movementRequest.DesiredVelocity = horizontalVelocity + verticalVelocity * Vector3.up;
        
        // Take the movement of the mouse and scale it by the look sensitivity. We'll calculate rotations additively, so we just need to know how far the mouse moved.
        m_movementRequest.LookDelta = new Vector2(m_lookInput.x, m_lookInput.y) * m_lookSensitivity;

        // Ask player controller to apply the movement the player wants.
        m_firstPersonController.ApplyMovement(m_movementRequest);
    }

    private float HandleSlide(float moveSpeed)
    {
        if (m_currentSlideMultiplier > 0)
        {
            m_currentSlideMultiplier -= 0.0025f;
        }

        return moveSpeed * m_currentSlideMultiplier;
    }

    private float HandleJump()
    {
        // Jump calculation from Unity Documentation for Character Controller
        float verticalVelocity = Mathf.Sqrt(m_jumpHeight * 2.0f);
        m_isJumping = false;
        return verticalVelocity;
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
