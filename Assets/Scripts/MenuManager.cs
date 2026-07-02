using UnityEngine;
using UnityEngine.SceneManagement;
using System.Collections;

public class MenuManager : MonoBehaviour
{
    [Header("Painéis")]
    public GameObject mainPanel;
    public GameObject levelSelectPanel;
    public GameObject optionsPanel;

    public float fadeDuration = 0.3f;

    void Start()
    {
        ShowPanel(mainPanel);
    }

    public void OnPlayClick()
    {
        ShowPanel(levelSelectPanel);
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
}