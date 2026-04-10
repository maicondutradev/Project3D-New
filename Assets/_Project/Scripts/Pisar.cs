using System.Collections;
using UnityEngine;

public class Pisar : MonoBehaviour
{
    [Header("Configurações da Porta")]
    public Transform porta; // O objeto que vai se mover
    public float tempoDeMovimento = 2f; // Quantos segundos leva para abrir/fechar

    [Header("Posições e Rotações")]
    // Valores baseados nas imagens enviadas
    public Vector3 posicaoInicial = new Vector3(-171.4f, -1.525f, 575f);
    public Vector3 rotacaoInicial = new Vector3(0f, -47.55f, 0f);

    public Vector3 posicaoFinal = new Vector3(-159.3f, -1.525f, 594.6f);
    public Vector3 rotacaoFinal = new Vector3(0f, 40.8f, 0f);

    private Coroutine movimentoAtual;
    private bool estaAberto = false;

    [Header("Identificação do Jogador para evitar falhas do Collider")]
    public bool ativarPorDistancia = true; // Ignora o tamanho gigante do Trigger e foca na distância real
    public float distanciaParaAtivar = 2.5f; // Distância (em metros) exata que considera pisar na pedra
    private Transform jogador;

    private void Start()
    {
        if (porta == null)
        {
            Debug.LogError("O script Pisar precisa de uma Porta atribuída no Inspector!");
        }

        // Tenta achar o jogador automaticamente pela tag caso seja por distância
        if (ativarPorDistancia)
        {
            GameObject playerObj = GameObject.FindGameObjectWithTag("Player");
            if (playerObj != null) jogador = playerObj.transform;
            else Debug.LogWarning("Jogador com Tag 'Player' não encontrado! O acionamento por distância não vai funcionar.");
        }
    }

    private void Update()
    {
        if (ativarPorDistancia && jogador != null)
        {
            // Calcula a distância real apenas no plano horizontal (X e Z) ou total
            float distanciaReal = Vector3.Distance(transform.position, jogador.position);

            if (distanciaReal <= distanciaParaAtivar && !estaAberto)
            {
                AbrirPorta();
            }
            else if (distanciaReal > distanciaParaAtivar && estaAberto && !voltarAutomaticamente)
            {
                FecharPorta();
            }
        }
    }

    private void AbrirPorta()
    {
        Debug.Log("Jogador chegou perto o suficiente! A porta vai abrir.");
        MoverPorta(posicaoFinal, QuantidadeParaQuaternion(rotacaoFinal));
        estaAberto = true;

        if (voltarAutomaticamente)
        {
            StartCoroutine(RotinaVoltarAutomaticamente());
        }
    }

    private void FecharPorta()
    {
        Debug.Log("Jogador se afastou da pedra! A porta vai fechar.");
        MoverPorta(posicaoInicial, QuantidadeParaQuaternion(rotacaoInicial));
        estaAberto = false;
    }

    [Header("Opções Extras")]
    public bool voltarAutomaticamente = false; // Se true, volta sozinho sem precisar sair
    public float tempoParaVoltar = 5f; // Segundos esperando aberto antes de voltar

    private void OnTriggerEnter(Collider other)
    {
        if (!ativarPorDistancia && other.CompareTag("Player"))
        {
            if (!estaAberto)
            {
                AbrirPorta();
            }
        }
    }

    private void OnTriggerExit(Collider other)
    {
        if (!ativarPorDistancia && other.CompareTag("Player"))
        {
            if (estaAberto && !voltarAutomaticamente)
            {
                FecharPorta();
            }
        }
    }

    private IEnumerator RotinaVoltarAutomaticamente()
    {
        // Espera o tempo de abrir + o tempo que ficará parado
        yield return new WaitForSeconds(tempoDeMovimento + tempoParaVoltar);
        
        if (estaAberto)
        {
            Debug.Log("Retornando automaticamente após o tempo acabar.");
            MoverPorta(posicaoInicial, QuantidadeParaQuaternion(rotacaoInicial));
            estaAberto = false;
        }
    }

    private void MoverPorta(Vector3 destino, Quaternion rotacaoDestino)
    {
        if (movimentoAtual != null)
        {
            StopCoroutine(movimentoAtual);
        }
        movimentoAtual = StartCoroutine(AnimarMovimento(destino, rotacaoDestino));
    }

    private IEnumerator AnimarMovimento(Vector3 destino, Quaternion rotacaoDestino)
    {
        Vector3 posPartida = porta.position;
        Quaternion rotPartida = porta.rotation;
        float tempoDecorrido = 0;

        while (tempoDecorrido < tempoDeMovimento)
        {
            porta.position = Vector3.Lerp(posPartida, destino, tempoDecorrido / tempoDeMovimento);
            porta.rotation = Quaternion.Slerp(rotPartida, rotacaoDestino, tempoDecorrido / tempoDeMovimento);
            tempoDecorrido += Time.deltaTime;
            yield return null;
        }

        porta.position = destino;
        porta.rotation = rotacaoDestino;
    }

    private Quaternion QuantidadeParaQuaternion(Vector3 euler)
    {
        return Quaternion.Euler(euler);
    }
}
