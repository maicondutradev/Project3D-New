using UnityEngine;

public class MoedaElevador : MonoBehaviour
{
    public string tagJogador = "Player";

    private void OnTriggerEnter(Collider outro)
    {
        // Se o jogador encostar na moeda
        if (outro.CompareTag(tagJogador))
        {
            // Adiciona o inventário ao jogador (se ele não tiver ainda)
            // Adiciona no "Root" (objeto principal do boneco) para evitar erros com múltiplas colisões de pernas/braços
            InventarioJogador inventario = outro.transform.root.GetComponent<InventarioJogador>();
            if (inventario == null)
            {
                inventario = outro.transform.root.gameObject.AddComponent<InventarioJogador>();
            }
            
            // Dá o passe livre do elevador para o jogador
            inventario.temMoedaDoElevador = true;
            Debug.Log("Moeda do Elevador Coletada! Vá até o elevador.");
            
            // Destrói (apaga) a moeda da tela
            Destroy(gameObject);
        }
    }
}

// Essa classe guarda o item no Jogador
public class InventarioJogador : MonoBehaviour
{
    public bool temMoedaDoElevador = false;
}
