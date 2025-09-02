using UnityEngine;
using UnityEngine.InputSystem;

[RequireComponent(typeof(CharacterController))]
public class FirstPersonController : MonoBehaviour
{
    [Header("Movement")]
    public float walkSpeed = 3f;
    public float runSpeed = 6f;
    public float crouchSpeed = 1.5f;
    public float jumpHeight = 1f;
    public float gravity = -9.81f;

    [Header("Mouse Look")]
    public float mouseSensitivity = 100f;
    public Transform playerCamera;
    public float xRotation = 0f;

    [Header("Ground Check")]
    public float groundDistance = 0.4f;
    public LayerMask groundMask = -1;
    public string groundTag = "Ground";

    [Header("Stealth")]
    public bool isCrouching = false;
    public bool isHidden = false;
    public float noiseLevel = 0f;

    private CharacterController controller;
    private Vector3 velocity;
    private bool isGrounded;
    private float currentSpeed;

    private PlayerInputActions inputActions;
    private Vector2 moveInput;
    private Vector2 lookInput;
    private bool isRunning;
    private bool jumpPressed;
    private bool crouchPressed;

    public System.Action<float> OnNoiseGenerated;
    public System.Action<bool> OnVisibilityChanged;

    private void Awake()
    {
        inputActions = new PlayerInputActions();

        if (!gameObject.CompareTag("Player"))
        {
            gameObject.tag = "Player";
        }
    }

    private void OnEnable()
    {
        inputActions.Player.Enable();

        inputActions.Player.Move.performed += OnMove;
        inputActions.Player.Move.canceled += OnMove;

        inputActions.Player.Look.performed += OnLook;
        inputActions.Player.Look.canceled += OnLook;

        inputActions.Player.Run.performed += OnRun;
        inputActions.Player.Run.canceled += OnRun;

        inputActions.Player.Jump.performed += OnJump;
        inputActions.Player.Crouch.performed += OnCrouch;
    }

    private void OnDisable()
    {
        inputActions.Player.Move.performed -= OnMove;
        inputActions.Player.Move.canceled -= OnMove;

        inputActions.Player.Look.performed -= OnLook;
        inputActions.Player.Look.canceled -= OnLook;

        inputActions.Player.Run.performed -= OnRun;
        inputActions.Player.Run.canceled -= OnRun;

        inputActions.Player.Jump.performed -= OnJump;
        inputActions.Player.Crouch.performed -= OnCrouch;

        inputActions.Player.Disable();
    }

    private void OnMove(InputAction.CallbackContext context)
    {
        moveInput = context.ReadValue<Vector2>();
    }

    private void OnLook(InputAction.CallbackContext context)
    {
        lookInput = context.ReadValue<Vector2>();
    }

    private void OnRun(InputAction.CallbackContext context)
    {
        isRunning = context.ReadValueAsButton();
    }

    private void OnJump(InputAction.CallbackContext context)
    {
        jumpPressed = context.performed;
    }

    private void OnCrouch(InputAction.CallbackContext context)
    {
        if (context.performed)
        {
            crouchPressed = true;
        }
    }

    private void Start()
    {
        controller = GetComponent<CharacterController>();
        Cursor.lockState = CursorLockMode.Locked;
        currentSpeed = walkSpeed;
    }

    private void Update()
    {
        if (GameManager.Instance.currentState != GameState.Playing) return;

        HandleMouseLook();
        HandleMovement();
        HandleStealth();

        jumpPressed = false;
        crouchPressed = false;
    }

    private void HandleMouseLook()
    {
        float mouseX = lookInput.x * mouseSensitivity * Time.deltaTime;
        float mouseY = lookInput.y * mouseSensitivity * Time.deltaTime;

        xRotation -= mouseY;
        xRotation = Mathf.Clamp(xRotation, -90f, 90f);

        playerCamera.localRotation = Quaternion.Euler(xRotation, 0f, 0f);
        transform.Rotate(Vector3.up * mouseX);
    }

    private void HandleMovement()
    {
        CheckGrounded();

        if (isGrounded && velocity.y < 0)
        {
            velocity.y = -2f;
        }

        float x = moveInput.x;
        float z = moveInput.y;

        Vector3 move = transform.right * x + transform.forward * z;

        // Determine current speed based on state
        if (isRunning && !isCrouching)
        {
            currentSpeed = runSpeed;
            GenerateNoise(0.8f);
        }
        else if (isCrouching)
        {
            currentSpeed = crouchSpeed;
            GenerateNoise(0.2f);
        }
        else
        {
            currentSpeed = walkSpeed;
            GenerateNoise(0.5f);
        }

        controller.Move(move * currentSpeed * Time.deltaTime);

        // Jumping
        if (jumpPressed && isGrounded && !isCrouching)
        {
            velocity.y = Mathf.Sqrt(jumpHeight * -2f * gravity);
            GenerateNoise(1f);
        }

        velocity.y += gravity * Time.deltaTime;
        controller.Move(velocity * Time.deltaTime);
    }

    private void CheckGrounded()
    {
        Vector3 spherePosition = new Vector3(transform.position.x, transform.position.y - controller.height / 2f, transform.position.z);

        if (Physics.CheckSphere(spherePosition, groundDistance, groundMask))
        {
            // Additional check using tag for more precise ground detection
            RaycastHit hit;
            if (Physics.SphereCast(spherePosition, groundDistance, Vector3.down, out hit, groundDistance, groundMask))
            {
                isGrounded = hit.collider.CompareTag(groundTag);
            }
            else
            {
                isGrounded = true; // Fallback to layer mask check
            }
        }
        else
        {
            isGrounded = false;
        }
    }

    private void HandleStealth()
    {
        // Crouching
        if (crouchPressed)
        {
            ToggleCrouch();
        }

        // Check if player is hidden (implement based on your hiding spots)
        CheckHiddenStatus();
    }

    private void ToggleCrouch()
    {
        isCrouching = !isCrouching;

        if (isCrouching)
        {
            controller.height = 1f;
            playerCamera.localPosition = new Vector3(0, 0.5f, 0);
        }
        else
        {
            controller.height = 2f;
            playerCamera.localPosition = new Vector3(0, 1f, 0);
        }
    }

    private void GenerateNoise(float amount)
    {
        if (moveInput.magnitude > 0.1f)
        {
            noiseLevel = amount;
            OnNoiseGenerated?.Invoke(noiseLevel);
        }
    }

    private void CheckHiddenStatus()
    {
        // Implement logic to check if player is behind cover or in hiding spots
        bool wasHidden = isHidden;

        // This would typically involve raycasting or trigger zones
        // For now, just a placeholder
        isHidden = false; // Implement your hiding logic here

        if (wasHidden != isHidden)
        {
            OnVisibilityChanged?.Invoke(isHidden);
        }
    }

    private void OnDrawGizmosSelected()
    {
        if (controller != null)
        {
            Vector3 spherePosition = new Vector3(transform.position.x, transform.position.y - controller.height / 2f, transform.position.z);
            Gizmos.color = isGrounded ? Color.green : Color.red;
            Gizmos.DrawWireSphere(spherePosition, groundDistance);
        }
    }
}
