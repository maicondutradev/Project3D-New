using UnityEngine;
using UnityEngine.InputSystem;
using System.Collections;

[RequireComponent(typeof(CharacterController))]
public class PlayerMovement : MonoBehaviour
{
    public float moveSpeed = 5f;
    public float rotationSpeed = 10f;
    public Transform cameraTransform;
    public InputActionReference moveAction;

    [Header("Configuracoes de Pulo")]
    public float jumpHeight = 1.5f;
    public float gravity = -15f;
    public float jumpDelay = 0.15f;

    private CharacterController controller;
    private Animator animator;
    private PlayerAttack playerAttack;

    private float velocityY;
    private bool isJumping = false;

    void Start()
    {
        controller = GetComponent<CharacterController>();
        animator = GetComponentInChildren<Animator>();
        playerAttack = GetComponent<PlayerAttack>();

        if (cameraTransform == null)
        {
            cameraTransform = Camera.main.transform;
        }
    }

    void Update()
    {
        bool isCurrentlyGrounded = controller.isGrounded && !isJumping;

        if (animator != null)
        {
            animator.SetBool("IsGrounded", isCurrentlyGrounded);

            AnimatorStateInfo stateInfo = animator.GetCurrentAnimatorStateInfo(0);
            if (stateInfo.IsName("Hit"))
            {
                velocityY += gravity * Time.deltaTime;
                controller.Move(new Vector3(0f, velocityY, 0f) * Time.deltaTime);
                return;
            }
        }

        Vector2 inputVector = Vector2.zero;

        if (moveAction != null && moveAction.action != null)
        {
            inputVector = moveAction.action.ReadValue<Vector2>();
        }

        bool isStandingStill = inputVector.magnitude < 0.1f;

        if (controller.isGrounded && velocityY < 0f && !isJumping)
        {
            velocityY = -2f;

            if (Keyboard.current != null && Keyboard.current.spaceKey.wasPressedThisFrame && isStandingStill)
            {
                isJumping = true;

                if (animator != null)
                {
                    animator.SetTrigger("Jump");
                }

                StartCoroutine(JumpRoutine());
            }
        }

        velocityY += gravity * Time.deltaTime;

        if (playerAttack != null && playerAttack.IsAttacking)
        {
            controller.Move(new Vector3(0f, velocityY, 0f) * Time.deltaTime);
            return;
        }

        if (moveAction == null || moveAction.action == null)
        {
            controller.Move(new Vector3(0f, velocityY, 0f) * Time.deltaTime);
            return;
        }

        Vector3 inputDirection = new Vector3(inputVector.x, 0f, inputVector.y).normalized;
        Vector3 moveDirection = Vector3.zero;

        if (inputDirection.magnitude >= 0.1f)
        {
            float targetAngle = Mathf.Atan2(inputDirection.x, inputDirection.z) * Mathf.Rad2Deg + cameraTransform.eulerAngles.y;
            Quaternion toRotation = Quaternion.Euler(0f, targetAngle, 0f);

            transform.rotation = Quaternion.Lerp(transform.rotation, toRotation, rotationSpeed * Time.deltaTime);

            moveDirection = Quaternion.Euler(0f, targetAngle, 0f) * Vector3.forward;

            float currentSpeed = moveSpeed;
            bool isRunningInput = Keyboard.current != null && Keyboard.current.shiftKey.isPressed;

            if (isRunningInput)
            {
                currentSpeed = moveSpeed * 2f;
            }

            if (animator != null)
            {
                animator.SetBool("IsRunning", isRunningInput && isCurrentlyGrounded);
                animator.SetBool("IsWalking", isCurrentlyGrounded);
            }

            moveDirection = moveDirection.normalized * currentSpeed;
        }
        else
        {
            if (animator != null)
            {
                animator.SetBool("IsWalking", false);
                animator.SetBool("IsRunning", false);
            }
        }

        moveDirection.y = velocityY;
        controller.Move(moveDirection * Time.deltaTime);
    }

    private IEnumerator JumpRoutine()
    {
        yield return new WaitForSeconds(jumpDelay);

        velocityY = Mathf.Sqrt(jumpHeight * -2f * gravity);

        yield return new WaitForSeconds(0.1f);
        isJumping = false;
    }
}