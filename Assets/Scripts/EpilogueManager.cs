using UnityEngine;
using UnityEngine.UI;
using UnityEngine.SceneManagement;
using System.Collections;

public class EpilogueManager : MonoBehaviour
{
    [Header("Personagens")]
    public Transform player;
    public Transform euridice;
    public Animator playerAnimator;
    public Animator euridiceAnimator;

    [Header("Câmera")]
    public Camera epilogueCamera;
    public float cameraOffsetX = -4f; // player fica na esquerda da tela
    public float cameraSpeed = 5f;

    [Header("Textos")]
    public Text[] epilogueTexts; // textos em ordem
    public float[] textTriggerPositions; // posição X do player que aciona cada texto
    public float textFadeDuration = 0.8f;
    public float textDisplayDuration = 2f;

    [Header("Encontro Final")]
    public float meetingPositionX; // posição X do meio da tela onde se encontram
    public float euridiceMoveSpeed = 2f;
    public float playerMoveSpeed = 2f;
    public string menuSceneName = "Menu";

    private bool[] textShown;
    private bool finalSequenceStarted = false;
    private bool cameraFollowing = true;

    void Start()
    {
        textShown = new bool[epilogueTexts.Length];

        // Esconde todos os textos
        foreach (Text t in epilogueTexts)
            t.color = new Color(t.color.r, t.color.g, t.color.b, 0f);

        // Euridice começa invisível no final da plataforma
        SpriteRenderer euridiceSprite = euridice.GetComponent<SpriteRenderer>();
        euridiceSprite.color = new Color(1f, 1f, 1f, 0f);
    }

    void LateUpdate()
    {
        // Câmera segue o player mantendo ele na esquerda
        if (cameraFollowing)
        {
            Vector3 targetPos = new Vector3(
                player.position.x - cameraOffsetX,
                epilogueCamera.transform.position.y,
                epilogueCamera.transform.position.z
            );
            epilogueCamera.transform.position = Vector3.Lerp(
                epilogueCamera.transform.position,
                targetPos,
                cameraSpeed * Time.deltaTime
            );
        }

        // Verifica triggers de texto
        for (int i = 0; i < epilogueTexts.Length; i++)
        {
            if (!textShown[i] && player.position.x >= textTriggerPositions[i])
            {
                textShown[i] = true;
                StartCoroutine(ShowText(epilogueTexts[i]));
            }
        }
    }

    private IEnumerator ShowText(Text text)
    {
        // Fade in
        float elapsed = 0f;
        while (elapsed < textFadeDuration)
        {
            elapsed += Time.deltaTime;
            float alpha = Mathf.Lerp(0f, 1f, elapsed / textFadeDuration);
            text.color = new Color(text.color.r, text.color.g, text.color.b, alpha);
            yield return null;
        }

        yield return new WaitForSeconds(textDisplayDuration);

        // Fade out
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

    // Chamado por um trigger no final da plataforma
    public void StartFinalSequence()
    {
        if (finalSequenceStarted) return;
        finalSequenceStarted = true;
        StartCoroutine(FinalSequence());
    }

    private IEnumerator FinalSequence()
    {
        // 1. Para o player
        Player playerScript = player.GetComponent<Player>();
        if (playerScript != null)
        {
            playerScript.canMove = false;
            playerScript.movement = 0f;
            playerScript.GetComponent<Rigidbody2D>().linearVelocity = Vector2.zero;
        }

        // 2. Euridice faz fade in
        yield return StartCoroutine(EuridiceFadeIn());

        yield return new WaitForSeconds(0.5f);

        // 3. Os dois caminham um em direção ao outro até o centro
        yield return StartCoroutine(WalkToMeeting());

        yield return new WaitForSeconds(1f);

        // 4. Olham um para o outro
        player.eulerAngles = new Vector3(0, 0, 0);   // player olha para direita
        euridice.eulerAngles = new Vector3(0, 180, 0); // euridice olha para esquerda

        yield return new WaitForSeconds(2f);

        // 5. Fade e volta ao menu
        yield return StartCoroutine(FadeToMenu());
    }

    private IEnumerator EuridiceFadeIn()
    {
        SpriteRenderer euridiceSprite = euridice.GetComponent<SpriteRenderer>();
        float elapsed = 0f;
        float duration = 1.5f;

        while (elapsed < duration)
        {
            elapsed += Time.deltaTime;
            float alpha = Mathf.Lerp(0f, 1f, elapsed / duration);
            euridiceSprite.color = new Color(1f, 1f, 1f, alpha);
            yield return null;
        }
    }

    private IEnumerator WalkToMeeting()
    {
        cameraFollowing = false; // para a câmera durante o encontro

        // Anima os dois caminhando
        if (playerAnimator != null) playerAnimator.SetInteger("transition", 1);
        if (euridiceAnimator != null) euridiceAnimator.SetInteger("transition", 1);

        float meetingX = meetingPositionX;

        while (Mathf.Abs(player.position.x - meetingX) > 0.1f ||
               Mathf.Abs(euridice.position.x - meetingX) > 0.1f)
        {
            // Move o player para a direita
            if (player.position.x < meetingX)
                player.position = Vector3.MoveTowards(player.position,
                    new Vector3(meetingX, player.position.y, player.position.z),
                    playerMoveSpeed * Time.deltaTime);

            // Move a euridice para a esquerda
            if (euridice.position.x > meetingX)
                euridice.position = Vector3.MoveTowards(euridice.position,
                    new Vector3(meetingX, euridice.position.y, euridice.position.z),
                    euridiceMoveSpeed * Time.deltaTime);

            yield return null;
        }

        // Para as animações
        if (playerAnimator != null) playerAnimator.SetInteger("transition", 0);
        if (euridiceAnimator != null) euridiceAnimator.SetInteger("transition", 0);
    }

    private IEnumerator FadeToMenu()
    {
        // Reutiliza o fadeImage do Canvas se houver
        yield return new WaitForSeconds(1f);
        SceneManager.LoadScene(menuSceneName);
    }
}