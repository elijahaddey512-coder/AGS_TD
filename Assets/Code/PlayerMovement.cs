
using UnityEngine;

[RequireComponent(typeof(CharacterController))]
public class PlayerMovement : MonoBehaviour
{
    [Header("Movement")]
    [SerializeField] private float moveSpeed = 5f;
    [SerializeField] private float gravity = -9.81f;

    [Header("Camera")]
    [SerializeField] private Transform cameraTransform;

    [Header("Animation")]
    [SerializeField] private Animator animator;
    [SerializeField] private string runTrigger = "Run";
    [SerializeField] private string idleTrigger = "Idle";

    private CharacterController controller;
    private Vector3 verticalVelocity;

    private bool isRunning = false;

    private void Awake()
    {
        controller = GetComponent<CharacterController>();

        if (cameraTransform == null && Camera.main != null)
        {
            cameraTransform = Camera.main.transform;
        }

        if (animator == null)
        {
            animator = GetComponentInChildren<Animator>();
        }
    }

    private void Update()
    {
        HandleMovement();
        HandleGravity();
    }

    private void HandleMovement()
    {
        float horizontal = Input.GetAxisRaw("Horizontal");
        float vertical = Input.GetAxisRaw("Vertical");

        Vector3 inputDirection =
            new Vector3(horizontal, 0f, vertical).normalized;

        bool hasMovementInput = inputDirection.magnitude >= 0.1f;

        if (hasMovementInput)
        {
            Vector3 cameraForward = cameraTransform != null
                ? cameraTransform.forward
                : Vector3.forward;

            Vector3 cameraRight = cameraTransform != null
                ? cameraTransform.right
                : Vector3.right;

            cameraForward.y = 0f;
            cameraRight.y = 0f;

            cameraForward.Normalize();
            cameraRight.Normalize();

            Vector3 moveDirection =
                cameraForward * inputDirection.z +
                cameraRight * inputDirection.x;

            controller.Move(
                moveDirection.normalized * moveSpeed * Time.deltaTime
            );

            // Rotate player towards movement direction.
            transform.rotation = Quaternion.Slerp(
                transform.rotation,
                Quaternion.LookRotation(moveDirection),
                12f * Time.deltaTime
            );

            PlayRunAnimation();
        }
        else
        {
            PlayIdleAnimation();
        }
    }

    private void HandleGravity()
    {
        if (controller.isGrounded && verticalVelocity.y < 0f)
        {
            verticalVelocity.y = -2f;
        }

        verticalVelocity.y += gravity * Time.deltaTime;

        controller.Move(verticalVelocity * Time.deltaTime);
    }

    private void PlayRunAnimation()
    {
        if (isRunning)
            return;

        isRunning = true;

        if (animator != null)
        {
            animator.ResetTrigger(idleTrigger);
            animator.SetTrigger(runTrigger);
        }
    }

    private void PlayIdleAnimation()
    {
        if (!isRunning)
            return;

        isRunning = false;

        if (animator != null)
        {
            animator.ResetTrigger(runTrigger);
            animator.SetTrigger(idleTrigger);
        }
    }
}


