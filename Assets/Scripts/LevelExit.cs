using UnityEngine;
using UnityEngine.SceneManagement;
using UnityEngine.UI;
using System.Collections;

public class LevelExit : MonoBehaviour
{
    [Header("Próxima fase")]
    public string nextSceneName;

    [Header("Fade")]
    public Image fadeImage;
    public float fadeDuration = 1f;

    [Header("Portal Enter")]
    public float moveToCenterDuration = 0.3f; // tempo para puxar ao centro
    public float enterDuration = 0.4f; // tempo para encolher

    private bool isExiting = false;
    
    [Header("Progresso")]
    public int currentLevelIndex;

    void Start()
    {
        if (fadeImage != null)
            fadeImage.color = new Color(0f, 0f, 0f, 0f);
    }


    private void OnTriggerEnter2D(Collider2D other)
    {
        if (!other.CompareTag("Player") || isExiting) return;

        // Desbloqueia a próxima fase
        LevelSelectManager.UnlockNextLevel(currentLevelIndex);

        Player player = other.GetComponent<Player>();
        Rigidbody2D playerRb = other.GetComponent<Rigidbody2D>();

        if (player != null) player.canMove = false;
        if (playerRb != null)
        {
            playerRb.linearVelocity = Vector2.zero;
            playerRb.bodyType = RigidbodyType2D.Kinematic;
        }

        isExiting = true;
        StartCoroutine(EnterPortalAndLoad(other.transform));
    }

    private IEnumerator EnterPortalAndLoad(Transform playerTransform)
    {
        // 1. Puxa o player até o centro do portal
        float elapsed = 0f;
        Vector3 startPos = playerTransform.position;
        Vector3 centerPos = new Vector3(transform.position.x, transform.position.y, playerTransform.position.z);

        while (elapsed < moveToCenterDuration)
        {
            elapsed += Time.deltaTime;
            float t = elapsed / moveToCenterDuration;
            float smooth = 1f - Mathf.Pow(1f - t, 3f); // ease out — desacelera ao chegar no centro
            playerTransform.position = Vector3.Lerp(startPos, centerPos, smooth);
            yield return null;
        }

        playerTransform.position = centerPos;

        // 2. Encolhe o player até zero
        elapsed = 0f;
        Vector3 originalScale = playerTransform.localScale;

        while (elapsed < enterDuration)
        {
            elapsed += Time.deltaTime;
            float t = elapsed / enterDuration;
            float smooth = Mathf.Pow(t, 3f); // ease in — acelera ao encolher
            playerTransform.localScale = Vector3.Lerp(originalScale, Vector3.zero, smooth);
            yield return null;
        }

        playerTransform.localScale = Vector3.zero;

        // 3. Fade de tela
        elapsed = 0f;
        while (elapsed < fadeDuration)
        {
            elapsed += Time.deltaTime;
            float alpha = Mathf.Lerp(0f, 1f, elapsed / fadeDuration);
            fadeImage.color = new Color(0f, 0f, 0f, alpha);
            yield return null;
        }

        fadeImage.color = new Color(0f, 0f, 0f, 1f);
        SceneManager.LoadScene(nextSceneName);
    }
}