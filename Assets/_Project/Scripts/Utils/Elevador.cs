using UnityEngine;
using System.Collections;

public class Elevador : MonoBehaviour
{
    [Header("Configurações do Elevador")]
    public float velocidade = 5f;
    public string tagJogador = "Player";
    
    [Header("Pontos de Passagem (Na ordem: do chão até o topo)")]
    // Arraste os GameObjects vazios (Waypoints) que representam o caminho aqui
    public Transform[] pontosDePassagem;
    
    private bool estaMovendo = false;
    private bool estaNoTopo = false;

    private Transform transformJogadorRoot = null;

    private void Update()
    {
        // Se o jogador estiver parado no elevador lá embaixo e repente pegar a moeda, o elevador nota e sobe!
        if (transformJogadorRoot != null && !estaMovendo && !estaNoTopo)
        {
            InventarioJogador inventario = transformJogadorRoot.GetComponent<InventarioJogador>();
            if (inventario != null && inventario.temMoedaDoElevador)
            {
                Debug.Log("Moeda detectada enquanto estava no elevador! Subindo.");
                StartCoroutine(MoverElevador(true)); // Sobe!
            }
        }
    }

    private void OnTriggerEnter(Collider outro)
    {
        // Verifica se quem pisou foi o jogador
        if (outro.CompareTag(tagJogador))
        {
            // Salva a referência e faz ele ser filho para não cair
            transformJogadorRoot = outro.transform.root;
            transformJogadorRoot.SetParent(transform);

            // Verifica se já tem a moeda da primeira vez que pisa
            InventarioJogador inventario = transformJogadorRoot.GetComponent<InventarioJogador>();
            bool temMoeda = (inventario != null && inventario.temMoedaDoElevador);

            // Se não está se movendo, decide se sobe ou desce
            if (!estaMovendo)
            {
                if (!estaNoTopo) // Elevador está embaixo
                {
                    if (!temMoeda)
                    {
                        Debug.Log("Esperando pegar a moeda em cima do elevador...");
                    }
                }
                else // Elevador está no topo e o jogador subiu de novo nele
                {
                    Debug.Log("Descendo...");
                    StartCoroutine(MoverElevador(false)); // Desce!
                }
            }
        }
    }

    private void OnTriggerExit(Collider outro)
    {
        // Solta o jogador inteiro quando ele sai do elevador
        if (outro.CompareTag(tagJogador) && transformJogadorRoot != null)
        {
            transformJogadorRoot.SetParent(null);
            transformJogadorRoot = null;
        }
    }

    // Coroutine (rotina) que move o elevador por todos os pontos
    private IEnumerator MoverElevador(bool subindo)
    {
        estaMovendo = true;

        if (subindo)
        {
            // Sobe: Move do índice 1 até o último índice
            for (int i = 1; i < pontosDePassagem.Length; i++)
            {
                yield return StartCoroutine(MoverParaPonto(pontosDePassagem[i].position));
            }
            estaNoTopo = true;
        }
        else
        {
            // Desce: Move do penúltimo índice de volta até o índice 0 (Chão)
            for (int i = pontosDePassagem.Length - 2; i >= 0; i--)
            {
                yield return StartCoroutine(MoverParaPonto(pontosDePassagem[i].position));
            }
            estaNoTopo = false;
        }

        estaMovendo = false;
    }

    private IEnumerator MoverParaPonto(Vector3 destino)
    {
        // Enquanto a distância for maior que "0.01", continua movendo
        while (Vector3.Distance(transform.position, destino) > 0.01f)
        {
            transform.position = Vector3.MoveTowards(transform.position, destino, velocidade * Time.deltaTime);
            yield return null; // Espera o próximo frame
        }
        transform.position = destino; // Garante que cravou no lugar exato
    }
}
