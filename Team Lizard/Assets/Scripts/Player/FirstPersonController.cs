using System;
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

    private CharacterController m_characterController;

    private float m_playerVelocityY;
    private float m_pitch = 0f;
    private Vector3 m_inertia;
    private float m_standHeight;
    private float m_standCameraY;
    private Vector3 m_standCenter;
    private bool m_isCrouched = false;

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
    }

    /// <summary>
    /// Attempts to move character according to requested movement and look vectors.
    /// </summary>
    /// <param name="request">MovementRequest struct holding a movement vector and a look vector.</param>
    public void ApplyMovement(MovementRequest request) 
    {
        ApplyLook(request.LookDelta);

        ApplyGravity();

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
    private void ApplyGravity() 
    {
        IsGrounded = m_characterController.isGrounded;
        if (IsGrounded && m_playerVelocityY < 0) 
        {
            // Player needs to be pressed into the ground for isGrounded to work correctly.
            m_playerVelocityY = m_gravity * Time.deltaTime;
        }
        else
        {
            // Player is not on ground, accelerate downwards.
            m_playerVelocityY += m_gravity * Time.deltaTime;
        }

        
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

        m_cameraTransform.localRotation = Quaternion.Euler(m_pitch, 0, 0);
    }
}
