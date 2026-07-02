using UnityEngine;

public class ShowTextOnTouch : MonoBehaviour
{
    [Header("Objeto de Texto na Interface")]
    [Tooltip("Arraste o painel ou texto do Canvas para cá")]
    public GameObject textObject;

    void Start()
    {
        // Garante que o texto comece invisível quando o jogo iniciar
        if (textObject != null)
        {
            textObject.SetActive(false);
        }
    }

    private void OnTriggerEnter2D(Collider2D other)
    {
        // Verifica se quem entrou na área foi o Player
        if (other.CompareTag("Player"))
        {
            if (textObject != null)
            {
                textObject.SetActive(true); // Liga o texto
            }
        }
    }

    private void OnTriggerExit2D(Collider2D other)
    {
        // Verifica se o Player saiu da área
        if (other.CompareTag("Player"))
        {
            if (textObject != null)
            {
                textObject.SetActive(false); // Desliga o texto
            }
        }
    }
}