using UnityEngine;
using System.Collections;

public class AudioManager : MonoBehaviour
{
    public static AudioManager Instance;

    public AudioSource musicSource;
    public AudioSource sfxSource;

    [Header("Músicas por cena")]
    public AudioClip[] sceneMusics;
    public string[] sceneNames;

    public float volumeFadeDuration = 0.3f;
    private Coroutine musicFadeCoroutine;
    private Coroutine sfxFadeCoroutine;

    [Header("Sliders")]
    public UnityEngine.UI.Slider musicSlider;
    public UnityEngine.UI.Slider sfxSlider;


    void Awake()
    {
        if (Instance == null)
        {
            Instance = this;
            DontDestroyOnLoad(gameObject);

            float music = PlayerPrefs.GetFloat("MusicVolume", 1f);
            float sfx = PlayerPrefs.GetFloat("SFXVolume", 1f);

            if (music <= 0f) music = 1f;
            if (sfx <= 0f) sfx = 1f;

            PlayerPrefs.SetFloat("MusicVolume", music);
            PlayerPrefs.SetFloat("SFXVolume", sfx);
            PlayerPrefs.Save();

            if (musicSource != null)
            {
                musicSource.volume = music;
                musicSource.Play();
            }
            if (sfxSource != null)
                sfxSource.volume = sfx;

            StartCoroutine(InitSliders(music, sfx));
        }
        else
        {
            Destroy(gameObject);
        }
    }


    private IEnumerator InitSliders(float music, float sfx)
    {
        yield return null;

        if (musicSlider != null)
        {
            musicSlider.SetValueWithoutNotify(music);
            musicSlider.onValueChanged.RemoveAllListeners();
            musicSlider.onValueChanged.AddListener(SetMusicVolume);
        }

        if (sfxSlider != null)
        {
            sfxSlider.SetValueWithoutNotify(sfx);
            sfxSlider.onValueChanged.RemoveAllListeners();
            sfxSlider.onValueChanged.AddListener(SetSFXVolume);
        }
    }

    IEnumerator CheckAudio()
    {
        yield return new WaitForSeconds(2f);
        Debug.Log($"Ainda tocando após 2s: {musicSource.isPlaying} — volume: {musicSource.volume} — time: {musicSource.time}");
    }

    void OnEnable()
    {
        UnityEngine.SceneManagement.SceneManager.sceneLoaded += OnSceneLoaded;
    }

    void OnDisable()
    {
        UnityEngine.SceneManagement.SceneManager.sceneLoaded -= OnSceneLoaded;
    }

    void OnSceneLoaded(UnityEngine.SceneManagement.Scene scene, UnityEngine.SceneManagement.LoadSceneMode mode)
    {
        for (int i = 0; i < sceneNames.Length; i++)
        {
            if (scene.name == sceneNames[i] && i < sceneMusics.Length)
            {
                if (sceneMusics[i] != null)
                {
                    musicSource.clip = sceneMusics[i];
                    musicSource.loop = true;
                    musicSource.volume = PlayerPrefs.GetFloat("MusicVolume", 1f);
                    musicSource.Play(); // <- sempre toca, independente de ser o mesmo clip
                }
                else
                    musicSource.Stop();
                break;
            }
        }
    }

    public void SetMusicVolume(float value)
    {
        PlayerPrefs.SetFloat("MusicVolume", value);
        if (musicFadeCoroutine != null) StopCoroutine(musicFadeCoroutine);
        musicFadeCoroutine = StartCoroutine(FadeVolume(musicSource, value));
    }

    public void SetSFXVolume(float value)
    {
        if (sfxSource != null) sfxSource.volume = value;
        PlayerPrefs.SetFloat("SFXVolume", value);

        foreach (AudioSource source in FindObjectsByType<AudioSource>(FindObjectsInactive.Exclude))
        {
            if (source != musicSource)
                source.volume = value;
        }
    }

    private IEnumerator FadeVolume(AudioSource source, float target)
    {
        if (source == null) yield break;

        float start = source.volume;
        float elapsed = 0f;

        while (elapsed < volumeFadeDuration)
        {
            elapsed += Time.unscaledDeltaTime;
            source.volume = Mathf.Lerp(start, target, elapsed / volumeFadeDuration);
            yield return null;
        }

        source.volume = target;
    }
}