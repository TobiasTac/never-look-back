using UnityEngine;
using UnityEngine.SceneManagement;
using UnityEngine.UI;
using System.Collections;

public class PauseMenu : MonoBehaviour
{
    [Header("Painel")]
    public CanvasGroup pausePanel;
    public float fadeDuration = 0.3f;

    [Header("Sliders")]
    public Slider musicSlider;
    public Slider sfxSlider;

    private bool isPaused = false;

    void Start()
    {
        pausePanel.alpha = 0f;
        pausePanel.interactable = false;
        pausePanel.blocksRaycasts = false;

        // Seta os valores salvos sem disparar eventos
        if (musicSlider != null)
        {
            musicSlider.SetValueWithoutNotify(PlayerPrefs.GetFloat("MusicVolume", 1f));
            musicSlider.onValueChanged.RemoveAllListeners();
            musicSlider.onValueChanged.AddListener(value =>
            {
                if (AudioManager.Instance != null)
                    AudioManager.Instance.SetMusicVolume(value);
            });
        }

        if (sfxSlider != null)
        {
            sfxSlider.SetValueWithoutNotify(PlayerPrefs.GetFloat("SFXVolume", 1f));
            sfxSlider.onValueChanged.RemoveAllListeners();
            sfxSlider.onValueChanged.AddListener(value =>
            {
                if (AudioManager.Instance != null)
                    AudioManager.Instance.SetSFXVolume(value);
            });
        }
    }

    public void TogglePause()
    {
        if (isPaused) Resume();
        else Pause();
    }

    void Pause()
    {
        isPaused = true;
        Time.timeScale = 0f;
        StartCoroutine(FadePanel(1f));
    }

    public void Resume()
    {
        isPaused = false;
        Time.timeScale = 1f;
        StartCoroutine(FadePanel(0f));
    }

    public void QuitToMenu()
    {
        Time.timeScale = 1f;
        SceneManager.LoadScene("Menu");
    }

    private IEnumerator FadePanel(float target)
    {
        float start = pausePanel.alpha;
        float elapsed = 0f;

        pausePanel.interactable = target > 0f;
        pausePanel.blocksRaycasts = target > 0f;

        while (elapsed < fadeDuration)
        {
            elapsed += Time.unscaledDeltaTime;
            pausePanel.alpha = Mathf.Lerp(start, target, elapsed / fadeDuration);
            yield return null;
        }

        pausePanel.alpha = target;
    }
}