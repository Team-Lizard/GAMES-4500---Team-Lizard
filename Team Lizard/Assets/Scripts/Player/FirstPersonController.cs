using UnityEngine;
using UnityEngine.InputSystem;

public class FirstPersonController : MonoBehaviour
{
    [Header("Movement")]
        [SerializeField]
        [Tooltip("Speed at which player should move.")]
        private float m_moveSpeed = 5.0f;

        [SerializeField]
        [Tooltip("Amount that speed is multiplied by when player is sprinting.")]
        private float m_sprintMultiplier = 1.8f;

        [SerializeField]
        [Tooltip("Height that player can jump.")]
        private float m_jumpHeight = 1.5f;

        [SerializeField]
        [Tooltip("Number of jumps the player can perform while not touching the ground.")]
        private int m_maxExtraJumps = 1;

        [SerializeField]
        [Tooltip("Force of gravity applied to player.")]
        private float m_gravity = -9.81f;

    [Header("Camera")]
        [SerializeField]
        [Tooltip("Mouse sensitivity when controlling the camera.")]
        private float m_lookSensitivity = 1.5f;

        [SerializeField]
        [Tooltip("Transform of object parenting camera.")]
        private Transform m_cameraTransform;

    private CharacterController m_characterController;

    private Vector2 m_moveInput;
    private Vector2 m_lookInput;
    private bool m_isSprintHeld;
    private bool m_isJumping;
    private int m_extraJumps;

    private Vector3 m_playerVelocity;
    private bool m_isGrounded;
    private float m_pitch = 0f;

    private void Awake() 
    {
        m_characterController = gameObject.AddComponent<CharacterController>();
    }

    private void Start()
    {
        Cursor.lockState = CursorLockMode.Locked;
        m_extraJumps = m_maxExtraJumps;
    }

    // Update is called once per frame
    private void Update()
    {
        HandleLook();

        // Check ground
        m_isGrounded = m_characterController.isGrounded;
        if (m_isGrounded && m_playerVelocity.y < 0) 
        {
            m_extraJumps = m_maxExtraJumps;
            m_playerVelocity.y = 0f;
        }

        // Movement
        Vector3 move = transform.forward * m_moveInput.y + transform.right * m_moveInput.x;
        // Make sure diagonal movement isn't faster than unidirectional movement.
        move = Vector3.ClampMagnitude(move, 1f);

        // Jump
        if (m_isGrounded)
        {
            HandleJump();
        }
        else if (m_extraJumps > 0 && HandleJump()) 
        {
            m_extraJumps--;
            
        }

        // Gravity
        m_playerVelocity.y += m_gravity * Time.deltaTime;

        // Combine horizontal and vertical movement
        float finalSpeed = m_isSprintHeld ? m_moveSpeed * m_sprintMultiplier : m_moveSpeed;
        Vector3 finalMove = (move * finalSpeed) + (m_playerVelocity.y * Vector3.up);
        m_characterController.Move(finalMove * Time.deltaTime);
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

    /// -----------------
    /// Helper Functions
    /// -----------------
    
    private void HandleLook() {
        // Take the movement of the mouse and scale it by the look sensitivity. We'll calculate rotations additively, so we just need to know how far the mouse moved.
        float mouseX = m_lookInput.x * m_lookSensitivity;
        float mouseY = m_lookInput.y * m_lookSensitivity;

        transform.Rotate(Vector3.up * mouseX);

        m_pitch -= mouseY;
        // Camera shouldn't be able to rotate further than straight up or straight down.
        m_pitch = Mathf.Clamp(m_pitch, -85f, 85f);
        m_cameraTransform.localRotation = Quaternion.Euler(m_pitch, 0, 0);
    }

    /// <summary>
    /// Handles jump physics and cancels input. Assumes that the requirements to jump have already been met.
    /// </summary>
    /// <returns>True if the player successfully jumped.</returns>
    private bool HandleJump() {
        if (m_isJumping) 
        {
            // Jump calculation from Unity Documentation for Character Controller
            m_playerVelocity.y = Mathf.Sqrt(m_jumpHeight * -2.0f * m_gravity);
            m_isJumping = false;

            return true;
        }
        return false;
        
    }
}
