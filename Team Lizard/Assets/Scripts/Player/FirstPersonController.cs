using System;
using Unity.VisualScripting;
using UnityEngine;

public class FirstPersonController : MonoBehaviour
{
    [Header("Movement")]
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


    private CharacterController m_characterController;
    private InputController m_inputController;


    private float m_playerVelocityY;
    private float m_pitch = 0f;
    private Vector3 m_inertia;

    public bool IsGrounded
    {
        get => m_characterController.isGrounded;
    }

    private void Awake() 
    {
        m_characterController = gameObject.AddComponent<CharacterController>();
        m_inputController = gameObject.GetComponent<InputController>();
    }

    // Start is called once before the first execution of Update after the MonoBehaviour is created
    private void Start()
    {
        
    }

    // Update is called once per frame
    private void Update()
    {
        MovementRequest state = m_inputController.MovementRequest;
        ApplyMovement(state);
    }

    private void ApplyMovement(MovementRequest state) {
        ApplyLook(state.LookDelta);

        ApplyGravity();

        if (state.DesiredVelocity == Vector3.zero) 
        {
            ApplyFriction();
        }
        else
        {
            ApplyMove(state.DesiredVelocity);
        }
        
        
    }

    private void ApplyGravity() {
        // Gravity
        m_playerVelocityY += m_gravity * Time.deltaTime;

        // Check ground
        if (IsGrounded && m_playerVelocityY < 0) 
        {
            m_playerVelocityY = 0f;
        }

        
    }

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

    private void ApplyMove(Vector3 moveInput)
    {
        if (moveInput.y != 0)
        {
            m_playerVelocityY = moveInput.y * Mathf.Sqrt(-1*m_gravity);
        }
        
        m_characterController.Move(new Vector3(moveInput.x, m_playerVelocityY, moveInput.z) * Time.deltaTime);
        m_inertia = new Vector3(moveInput.x, 0, moveInput.z);
        Debug.Log(m_playerVelocityY);
    }

    private void ApplyLook(Vector2 lookInput) 
    {
        transform.Rotate(Vector3.up * lookInput.x);

        m_pitch -= lookInput.y;
        // Camera shouldn't be able to rotate further than straight up or straight down.
        m_pitch = Mathf.Clamp(m_pitch, -85f, 85f);
        m_cameraTransform.localRotation = Quaternion.Euler(m_pitch, 0, 0);
    }
}
