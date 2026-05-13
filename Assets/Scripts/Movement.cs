using System.Collections;
using System.Collections.Generic;
using UnityEngine;

[RequireComponent(typeof(CharacterController))]
public class PlayerMovement : MonoBehaviour
{
    [SerializeField] private Transform playerCamera;
    [SerializeField] private float cameraHeight = 1.5f;
    [SerializeField] private float moveSpeed = 7f;
    [SerializeField] private float sprintSpeed = 20f;
    [SerializeField] private KeyCode sprintKey = KeyCode.LeftShift;
    [SerializeField] private KeyCode jumpKey = KeyCode.Space;
    [SerializeField] private float lookSensitivity = 2f;
    [SerializeField] private float minPitch = -80f;
    [SerializeField] private float maxPitch = 80f;
    [SerializeField] private float gravity = -9.81f;
    [SerializeField] private float jumpForce = 1.5f;

    private CharacterController characterController;
    private float cameraPitch;
    private float verticalVelocity;

    // Start is called before the first frame update
    void Start()
    {
        characterController = GetComponent<CharacterController>();

        Cursor.lockState = CursorLockMode.Locked;
        Cursor.visible = false;

        if (playerCamera == null)
        {
            Camera childCamera = GetComponentInChildren<Camera>();
            if (childCamera != null)
            {
                playerCamera = childCamera.transform;
            }
            else if (Camera.main != null)
            {
                playerCamera = Camera.main.transform;
            }
        }

        if (playerCamera != null)
        {
            if (playerCamera.parent != transform)
            {
                playerCamera.SetParent(transform, false);
            }

            playerCamera.localPosition = new Vector3(0f, cameraHeight, 0f);
        }
    }

    // Update is called once per frame
    void Update()
    {
        HandleMovement();
        HandleMouseLook();
    }

    private void HandleMovement()
    {
        float horizontal = Input.GetAxisRaw("Horizontal");
        float vertical = Input.GetAxisRaw("Vertical");
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
        {
            verticalVelocity = -2f;
        }
        if (characterController.isGrounded)
        {
            if (Input.GetKeyDown(jumpKey))
            {
                verticalVelocity = Mathf.Sqrt(jumpForce * -1.7f * gravity);
            }
        }

        verticalVelocity += gravity * Time.deltaTime;
        float currentSpeed = Input.GetKey(sprintKey) ? sprintSpeed : moveSpeed;
        Vector3 velocity = moveDirection * currentSpeed;
        velocity.y = verticalVelocity;

        characterController.Move(velocity * Time.deltaTime);
    }

    private void HandleMouseLook()
    {
        if (playerCamera == null)
        {
            return;
        }

        float mouseX = Input.GetAxis("Mouse X") * lookSensitivity;
        float mouseY = Input.GetAxis("Mouse Y") * lookSensitivity;

        transform.Rotate(0f, mouseX, 0f);
        cameraPitch -= mouseY;
        cameraPitch = Mathf.Clamp(cameraPitch, minPitch, maxPitch);
        playerCamera.localRotation = Quaternion.Euler(cameraPitch, 0f, 0f);
    }
}
