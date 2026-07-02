using UnityEngine;
using UnityEngine.SceneManagement;
using UnityEngine.UI;
using TMPro;

public class LevelSelectManager : MonoBehaviour
{
    [Header("Botões das fases")]
    public Button[] levelButtons; // um por fase na ordem
    public string[] levelSceneNames; // nome exato de cada cena

    void Start()
    {
        int unlockedLevel = PlayerPrefs.GetInt("UnlockedLevel", 1);

        for (int i = 0; i < levelButtons.Length; i++)
        {
            bool unlocked = i + 1 <= unlockedLevel;
            levelButtons[i].interactable = unlocked;

            // Opacidade reduzida para fases bloqueadas
            CanvasGroup cg = levelButtons[i].GetComponent<CanvasGroup>();
            if (cg != null)
                cg.alpha = unlocked ? 1f : 0.4f;

            int index = i; // captura para o closure
            levelButtons[i].onClick.AddListener(() => LoadLevel(index));
        }
    }

    void LoadLevel(int index)
    {
        SceneManager.LoadScene(levelSceneNames[index]);
    }

    // Chame isso no LevelExit ao passar de fase
    public static void UnlockNextLevel(int currentLevelIndex)
    {
        int next = currentLevelIndex + 1;
        int saved = PlayerPrefs.GetInt("UnlockedLevel", 1);
        if (next > saved)
        {
            PlayerPrefs.SetInt("UnlockedLevel", next);
            PlayerPrefs.Save();
        }
    }
}