using UnityEngine;
using UnityEngine.InputSystem;

public class PlayerFlying : MonoBehaviour
{
    [Header("Configurações do Voador")]
    public GameObject voadorPrefab; // O objeto (tapete, nuvem, etc)
    public Transform createPes; // O ponto nos pés do mago
    public float alturaDeVoo = 2.5f; // Metros acima do chão
    public InputActionReference shiftAction; // Referência para o Shift (Input System)

    private GameObject voadorInstanciado;
    private PlayerMovement playerMovement;
    private CharacterController controller;
    private float originalSpeed;
    private bool estaVoando = false;

    void Start()
    {
        playerMovement = GetComponent<PlayerMovement>();
        controller = GetComponent<CharacterController>();
        
        if (playerMovement != null)
        {
            originalSpeed = playerMovement.moveSpeed;
        }
    }

    void Update()
    {
        // Verifica se a tecla Shift foi pressionada
        bool apertouShift = false;

        if (shiftAction != null && shiftAction.action != null && shiftAction.action.triggered)
        {
            apertouShift = true;
        }
        else if (Keyboard.current != null && Keyboard.current.shiftKey.wasPressedThisFrame)
        {
            apertouShift = true;
        }

        if (apertouShift)
        {
            ToggleVoo();
        }
    }

    private void ToggleVoo()
    {
        estaVoando = !estaVoando;

        if (estaVoando)
        {
            AtivarVoo();
        }
        else
        {
            DesativarVoo();
        }
    }

    private void AtivarVoo()
    {
        // Verifica se as referências principais foram configuradas
        if (voadorPrefab == null)
        {
            Debug.LogError("ERRO: Você não colocou o Voador Prefab no script PlayerFlying!");
            estaVoando = false;
            return;
        }

        if (createPes == null)
        {
            Debug.LogError("ERRO: Você precisa arrastar o ponto 'create pes' para o campo Create Pes no script PlayerFlying!");
            estaVoando = false;
            return;
        }

        if (playerMovement != null)
        {
            // 1. Dobra a velocidade
            playerMovement.moveSpeed = originalSpeed * 2f;
            
            // 2. Desliga a gravidade e reseta a velocidade vertical
            playerMovement.gravityMultiplier = 0f;
            playerMovement.velocityY = 0f; // Para o mago não cair nem um pouco
        }

        // 3. Cria o objeto voador se ele não existir
        if (voadorInstanciado == null)
        {
            voadorInstanciado = Instantiate(voadorPrefab, createPes.position, createPes.rotation);
            voadorInstanciado.transform.SetParent(createPes); // Fixa nos pés
        }
        else
        {
            voadorInstanciado.SetActive(true);
        }

        // 4. Sobe o mago para a altura de voo
        if (controller != null)
        {
            controller.Move(Vector3.up * alturaDeVoo);
        }
    }

    private void DesativarVoo()
    {
        if (playerMovement != null)
        {
            // 1. Volta a velocidade original
            playerMovement.moveSpeed = originalSpeed;

            // 2. Liga a gravidade novamente
            playerMovement.gravityMultiplier = 1f;
        }

        // 3. Esconde o voador
        if (voadorInstanciado != null)
        {
            voadorInstanciado.SetActive(false);
        }
    }
}
