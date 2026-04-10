using UnityEngine;
using UnityEngine.InputSystem;

[RequireComponent(typeof(CharacterController))]
public class PlayerMovement : MonoBehaviour
{
    public float moveSpeed = 5f;
    public float rotationSpeed = 10f;
    public float gravityMultiplier = 1f;
    public Transform cameraTransform;
    public InputActionReference moveAction;
    
    [Header("Configurações de Pulo e Gravidade")]
    public float baseMoveSpeed = 5f; // Mantido para backup se necessário
    public float jumpHeight = 2.5f;
    public float jumpDuration = 2f; // Tempo total no ar (subida + descida)
    
    private CharacterController controller;
    private Animator animator;
    private PlayerAttack playerAttack;
    public float velocityY; // Agora público para controle externo (ex: voar)

    void Start()
    {
        controller = GetComponent<CharacterController>();
        animator = GetComponentInChildren<Animator>();
        playerAttack = GetComponent<PlayerAttack>();

        if (cameraTransform == null)
        {
            cameraTransform = Camera.main.transform;
        }

        // Se quiser que a velocidade definida no inspector seja a base
        baseMoveSpeed = moveSpeed;
    }

    void Update()
    {
        // === FÍSICA E PULO ===
        if (controller.isGrounded && velocityY < 0f)
        {
            velocityY = -2f; // Mantém o jogador preso ao chão suavemente

            // Lógica do pulo se apertar Espaço e estiver no chão
            if (Keyboard.current != null && Keyboard.current.spaceKey.wasPressedThisFrame)
            {
                // Fórmula física: velocity = g * tempo_de_subida; onde g = gravidade customizada
                // Para ter um pulo de X m em Y seg (tempo subida = Y/2):
                float timeToPeak = jumpDuration / 2f;
                // Calculamos a vel. inicial baseada no tempo de subida e na altura
                velocityY = (2f * jumpHeight) / timeToPeak;
                
                // if (animator != null) animator.SetTrigger("Jump");
            }
        }

        // Calcula a gravidade customizada baseado na altura e tempo requerido
        // Isso fará seu pulo durar exatos 2 segundos subindo e descendo 2.5m
        float customGravity = (-8f * jumpHeight) / (jumpDuration * jumpDuration);
        
        velocityY += customGravity * Time.deltaTime;

        // === ATAQUE ===
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

        Vector2 inputVector = moveAction.action.ReadValue<Vector2>();
        Vector3 inputDirection = new Vector3(inputVector.x, 0f, inputVector.y).normalized;
        Vector3 moveDirection = Vector3.zero;

        if (inputDirection.magnitude >= 0.1f)
        {
            float targetAngle = Mathf.Atan2(inputDirection.x, inputDirection.z) * Mathf.Rad2Deg + cameraTransform.eulerAngles.y;
            Quaternion toRotation = Quaternion.Euler(0f, targetAngle, 0f);

            transform.rotation = Quaternion.Lerp(transform.rotation, toRotation, rotationSpeed * Time.deltaTime);

            moveDirection = Quaternion.Euler(0f, targetAngle, 0f) * Vector3.forward;

            // Verifica se o Shift está sendo sendo pressionado (Hold para correr)
            float currentSpeed = moveSpeed;
            if (Keyboard.current != null && Keyboard.current.shiftKey.isPressed)
            {
                currentSpeed = moveSpeed * 2f;
                // Se tiver animação de correr, você pode colocar aqui:
                // if (animator != null) animator.SetBool("IsRunning", true);
            }
            
            moveDirection = moveDirection.normalized * currentSpeed;

            if (animator != null)
            {
                animator.SetBool("IsWalking", true);
            }
        }
        else
        {
            if (animator != null)
            {
                animator.SetBool("IsWalking", false);
            }
        }

        moveDirection.y = velocityY;
        controller.Move(moveDirection * Time.deltaTime);
    }
}