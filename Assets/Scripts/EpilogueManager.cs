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
    public float[] textTriggerPositions;
    public float textFadeDuration = 0.8f;
    public float textDisplayDuration = 2f;

    [Header("Encontro Final")]
    public float playerMeetingX;
    public float euridiceMeetingX;
    public float euridiceMoveSpeed = 2f;
    public float playerMoveSpeed = 2f;
    public string menuSceneName = "Menu";

    [Header("Fade Final")]
    public TMP_Text fadeImage; // se usar TMP para o fade também
    public UnityEngine.UI.Image fadePanel; // painel preto do fade

    private bool[] textShown;
    private bool finalSequenceStarted = false;
    private bool cameraFollowing = true;
    private Rigidbody2D playerRb;
    private Transform playerTransform;

    void Start()
    {
        playerRb = epiloguePlayer.GetComponent<Rigidbody2D>();
        playerTransform = epiloguePlayer.transform;

        textShown = new bool[epilogueTexts.Length];

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

        for (int i = 0; i < epilogueTexts.Length; i++)
        {
            if (i >= textTriggerPositions.Length) break;

            if (!textShown[i] && playerTransform.position.x >= textTriggerPositions[i])
            {
                textShown[i] = true;
                StartCoroutine(ShowText(epilogueTexts[i]));
            }
        }
    }

    private IEnumerator ShowText(TMP_Text text)
    {
        float elapsed = 0f;
        while (elapsed < textFadeDuration)
        {
            elapsed += Time.deltaTime;
            float alpha = Mathf.Lerp(0f, 1f, elapsed / textFadeDuration);
            text.color = new Color(text.color.r, text.color.g, text.color.b, alpha);
            yield return null;
        }

        yield return new WaitForSeconds(textDisplayDuration);

        elapsed = 0f;
        while (elapsed < textFadeDuration)
        {
            elapsed += Time.deltaTime;
            float alpha = Mathf.Lerp(1f, 0f, elapsed / textFadeDuration);
            text.color = new Color(text.color.r, text.color.g, text.color.b, alpha);
            yield return null;
        }

        text.color = new Color(text.color.r, text.color.g, text.color.b, 0f);
    }

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