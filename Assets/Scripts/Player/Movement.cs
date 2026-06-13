using UnityEngine;

[RequireComponent(typeof(CharacterController))]
public class PlayerMovement : MonoBehaviour
{
    /// <summary>
    /// The active player, set on enable. A reliable, tag-independent handle so
    /// systems like <see cref="GameFlowManager"/>, the boundary respawn and the
    /// save/continue flow can always find the player (this project's player is built
    /// on an imported character prefab that isn't guaranteed to carry the Player tag).
    /// </summary>
    public static PlayerMovement Current { get; private set; }

    [SerializeField] private Transform playerCamera;
    [SerializeField] private float cameraHeight = 1.5f;
    [SerializeField] private float moveSpeed = 7f;
    [SerializeField] private float sprintSpeed = 20f;
    [SerializeField] private float minPitch = -80f;
    [SerializeField] private float maxPitch = 80f;
    [SerializeField] private float gravity = -9.81f;
    [SerializeField] private float jumpForce = 1.5f;

    private CharacterController characterController;
    private float cameraPitch;
    private float verticalVelocity;

    void Awake()
    {
        Current = this;
        // Guarantee the player is discoverable by tag too (safe: "Player" is a builtin tag).
        if (!CompareTag("Player")) tag = "Player";
    }

    void OnDestroy()
    {
        if (Current == this) Current = null;
    }

    void Start()
    {
        characterController = GetComponent<CharacterController>();

        Cursor.lockState = CursorLockMode.Locked;
        Cursor.visible = false;

        if (playerCamera == null)
        {
            Camera childCamera = GetComponentInChildren<Camera>();
            if (childCamera != null)
                playerCamera = childCamera.transform;
            else if (Camera.main != null)
                playerCamera = Camera.main.transform;
        }

        if (playerCamera != null)
        {
            if (playerCamera.parent != transform)
                playerCamera.SetParent(transform, false);

            playerCamera.localPosition = new Vector3(0f, cameraHeight, 0f);
        }
    }

    void Update()
    {
        // Freeze the player while a blocking UI (dialogue / mission mini-game / menu) is open.
        if (UIBlocker.IsBlocked) return;

        HandleMovement();
        HandleMouseLook();
    }

    private void HandleMovement()
    {
        // Movement axes come from the rebindable bindings in GameSettings.
        float horizontal = (GameSettings.Held(GameAction.MoveRight) ? 1f : 0f)
                         - (GameSettings.Held(GameAction.MoveLeft) ? 1f : 0f);
        float vertical = (GameSettings.Held(GameAction.MoveForward) ? 1f : 0f)
                       - (GameSettings.Held(GameAction.MoveBack) ? 1f : 0f);

        Vector3 forward = transform.forward;
        Vector3 right = transform.right;
        if (playerCamera != null)
        {
            forward = playerCamera.forward;
            right = playerCamera.right;
            forward.y = 0f;
            right.y = 0f;
            forward.Normalize();
            right.Normalize();
        }
        Vector3 moveDirection = (right * horizontal + forward * vertical).normalized;

        if (characterController.isGrounded && verticalVelocity < 0f)
            verticalVelocity = -2f;

        if (characterController.isGrounded && GameSettings.Pressed(GameAction.Jump))
            verticalVelocity = Mathf.Sqrt(jumpForce * -1.7f * gravity);

        verticalVelocity += gravity * Time.deltaTime;
        float currentSpeed = GameSettings.Held(GameAction.Sprint) ? sprintSpeed : moveSpeed;
        Vector3 velocity = moveDirection * currentSpeed;
        velocity.y = verticalVelocity;

        characterController.Move(velocity * Time.deltaTime);
    }

    private void HandleMouseLook()
    {
        if (playerCamera == null) return;

        float sensitivity = GameSettings.MouseSensitivity;
        float mouseX = Input.GetAxis("Mouse X") * sensitivity;
        float mouseY = Input.GetAxis("Mouse Y") * sensitivity;
        if (GameSettings.InvertY) mouseY = -mouseY;

        transform.Rotate(0f, mouseX, 0f);
        cameraPitch -= mouseY;
        cameraPitch = Mathf.Clamp(cameraPitch, minPitch, maxPitch);
        playerCamera.localRotation = Quaternion.Euler(cameraPitch, 0f, 0f);
    }
}
