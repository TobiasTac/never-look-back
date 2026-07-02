using UnityEngine;
using TMPro;
using UnityEngine.SceneManagement;
using System.Collections;

public class EpilogueManager : MonoBehaviour
{
    [Header("Personagens")]
    public EpiloguePlayer epiloguePlayer;
    public Transform euridice;
    public Animator playerAnimator;
    public Animator euridiceAnimator;

    [Header("Câmera")]
    public Camera epilogueCamera;
    public float cameraOffsetX = -4f;
    public float cameraSpeed = 5f;

    [Header("Textos")]
    public TMP_Text[] epilogueTexts;
    [Tooltip("Posição X onde o texto vai APARECER")]
    public float[] textTriggerPositions;
    [Tooltip("Posição X onde o texto vai SUMIR")]
    public float[] textHidePositions; 
    public float textFadeDuration = 0.8f;
    // Removi o textDisplayDuration, pois agora não usaremos mais tempo!

    [Header("Encontro Final")]
    public float playerMeetingX;
    public float euridiceMeetingX;
    public float euridiceMoveSpeed = 2f;
    public float playerMoveSpeed = 2f;
    public string menuSceneName = "Menu";

    [Header("Fade Final")]
    public TMP_Text fadeImage; 
    public UnityEngine.UI.Image fadePanel; 

    private bool[] textShown;
    private bool[] textHidden; // Nova variável para saber se o texto já sumiu
    private bool finalSequenceStarted = false;
    private bool cameraFollowing = true;
    private Rigidbody2D playerRb;
    private Transform playerTransform;

    void Start()
    {
        playerRb = epiloguePlayer.GetComponent<Rigidbody2D>();
        playerTransform = epiloguePlayer.transform;

        // Inicializa os arrays de controle
        textShown = new bool[epilogueTexts.Length];
        textHidden = new bool[epilogueTexts.Length];

        // Deixa todos os textos invisíveis no começo
        foreach (TMP_Text t in epilogueTexts)
            t.color = new Color(t.color.r, t.color.g, t.color.b, 0f);

        Collider2D euridiceCollider = euridice.GetComponent<Collider2D>();
        if (euridiceCollider != null) euridiceCollider.enabled = false;

        if (fadePanel != null)
            fadePanel.color = new Color(0f, 0f, 0f, 0f);
    }

    void LateUpdate()
    {
        if (finalSequenceStarted) return;

        // Lógica da Câmera
        if (cameraFollowing)
        {
            Vector3 targetPos = new Vector3(
                playerTransform.position.x - cameraOffsetX,
                epilogueCamera.transform.position.y,
                epilogueCamera.transform.position.z
            );
            epilogueCamera.transform.position = Vector3.Lerp(
                epilogueCamera.transform.position,
                targetPos,
                cameraSpeed * Time.deltaTime
            );
        }

        // Lógica de Aparecer e Sumir Textos
        for (int i = 0; i < epilogueTexts.Length; i++)
        {
            // Verifica se o texto deve APARECER
            if (i < textTriggerPositions.Length && !textShown[i] && playerTransform.position.x >= textTriggerPositions[i])
            {
                textShown[i] = true;
                StartCoroutine(FadeInText(epilogueTexts[i]));
            }

            // Verifica se o texto deve SUMIR
            if (i < textHidePositions.Length && textShown[i] && !textHidden[i] && playerTransform.position.x >= textHidePositions[i])
            {
                textHidden[i] = true;
                StartCoroutine(FadeOutText(epilogueTexts[i]));
            }
        }
    }

    // --- NOVAS COROUTINES DE TEXTO ---

    private IEnumerator FadeInText(TMP_Text text)
    {
        float elapsed = 0f;
        while (elapsed < textFadeDuration)
        {
            elapsed += Time.deltaTime;
            float alpha = Mathf.Lerp(0f, 1f, elapsed / textFadeDuration);
            text.color = new Color(text.color.r, text.color.g, text.color.b, alpha);
            yield return null;
        }
        
        // Garante que a opacidade fique em 100% no final
        text.color = new Color(text.color.r, text.color.g, text.color.b, 1f);
    }

    private IEnumerator FadeOutText(TMP_Text text)
    {
        float elapsed = 0f;
        while (elapsed < textFadeDuration)
        {
            elapsed += Time.deltaTime;
            float alpha = Mathf.Lerp(1f, 0f, elapsed / textFadeDuration);
            text.color = new Color(text.color.r, text.color.g, text.color.b, alpha);
            yield return null;
        }

        // Garante que a opacidade fique em 0% no final
        text.color = new Color(text.color.r, text.color.g, text.color.b, 0f);
    }

    // ---------------------------------

    public void StartFinalSequence()
    {
        if (finalSequenceStarted) return;
        finalSequenceStarted = true;
        StartCoroutine(FinalSequence());
    }

    private IEnumerator FinalSequence()
    {
        epiloguePlayer.Freeze();
        yield return new WaitForSeconds(0.5f);
        yield return StartCoroutine(WalkToMeeting());
        yield return new WaitForSeconds(1f);
        yield return StartCoroutine(FadeToMenu());
    }

    private IEnumerator WalkToMeeting()
    {
        cameraFollowing = false;

        if (playerAnimator != null) playerAnimator.SetInteger("transition", 1);
        if (euridiceAnimator != null) euridiceAnimator.SetInteger("transition", 1);

        while (Mathf.Abs(playerTransform.position.x - playerMeetingX) > 0.1f ||
               Mathf.Abs(euridice.position.x - euridiceMeetingX) > 0.1f)
        {
            if (playerTransform.position.x < playerMeetingX)
                playerRb.MovePosition(Vector2.MoveTowards(playerRb.position,
                    new Vector2(playerMeetingX, playerRb.position.y),
                    playerMoveSpeed * Time.fixedDeltaTime));

            if (euridice.position.x > euridiceMeetingX)
                euridice.position = Vector3.MoveTowards(euridice.position,
                    new Vector3(euridiceMeetingX, euridice.position.y, euridice.position.z),
                    euridiceMoveSpeed * Time.deltaTime);

            yield return null;
        }

        if (playerAnimator != null) playerAnimator.SetInteger("transition", 0);
        if (euridiceAnimator != null) euridiceAnimator.SetInteger("transition", 0);
    }

    private IEnumerator FadeToMenu()
    {
        yield return new WaitForSeconds(1f);

        if (fadePanel != null)
        {
            float elapsed = 0f;
            float duration = 1.5f;

            while (elapsed < duration)
            {
                elapsed += Time.deltaTime;
                float alpha = Mathf.Lerp(0f, 1f, elapsed / duration);
                fadePanel.color = new Color(0f, 0f, 0f, alpha);
                yield return null;
            }

            fadePanel.color = new Color(0f, 0f, 0f, 1f);
            yield return new WaitForSeconds(0.5f);
        }

        SceneManager.LoadScene(menuSceneName);
    }
}