using UnityEngine;
using UnityEngine.SceneManagement;
using UnityEngine.UI; // OBRIGATÓRIO: Permite controlar a interatividade dos botões (UI)

public class MenuManager : MonoBehaviour
{
    [Header("Painéis")]
    public GameObject mainPanel;
    public GameObject levelSelectPanel;
    public GameObject optionsPanel;

    [Header("Botões de Fase")]
    // Arraste todos os botões de fases para cá no Inspector da Unity (em ordem: Fase 1, Fase 2, etc)
    public Button[] levelButtons; 

    public float fadeDuration = 0.3f;

    void Start()
    {
        ShowPanel(mainPanel);
    }

    public void OnPlayClick()
    {
        ShowPanel(levelSelectPanel);
        AtualizarBotoesDeFase(); // Checa o progresso sempre que abrir o painel de fases
    }

    public void OnControlsClick()
    {
        SceneManager.LoadScene("Tutorial");
    }

    public void OnOptionsClick()
    {
        ShowPanel(optionsPanel);
    }

    public void OnBackClick()
    {
        ShowPanel(mainPanel);
    }

    void ShowPanel(GameObject target)
    {
        mainPanel.SetActive(false);
        levelSelectPanel.SetActive(false);
        optionsPanel.SetActive(false);
        target.SetActive(true);
    }

    // --- NOVA FUNÇÃO DE DESBLOQUEIO ---
    void AtualizarBotoesDeFase()
    {
        int fasesDesbloqueadas = PlayerPrefs.GetInt("UnlockedLevel", 1); // <- mesma chave

        for (int i = 0; i < levelButtons.Length; i++)
        {
            levelButtons[i].interactable = (i + 1 <= fasesDesbloqueadas);
        }
    }
}