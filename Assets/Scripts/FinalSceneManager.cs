using UnityEngine;
using UnityEngine.Rendering.Universal;
using System.Collections;
using UnityEngine.SceneManagement;

public class FinalSceneManager : MonoBehaviour
{
    [Header("Referências")]
    public Player player;
    public Ghost ghost;
    public Light2D globalLight;
    public Light2D spotLight; // spotlight que segue o player

    [Header("Configuração")]
    public float darkenDelay = 3f;       // tempo até escurecer
    public float darkenDuration = 1.5f;  // duração do escurecimento
    public float lightenDuration = 1f;   // duração do acender ao chegar no portal

    [Header("Próxima cena")]
    public string epilogue; // coloque o nome da cena do epílogo

    private void Start()
    {
        // Garante que a luz começa em 100%
        globalLight.intensity = 1f;

        // Ghost começa invisível
        ghost.gameObject.SetActive(false);

        StartCoroutine(DarkenSequence());
    }

    private IEnumerator DarkenSequence()
    {
        // Aguarda 3 segundos com luz normal
        yield return new WaitForSeconds(darkenDelay);

        // Escurece gradualmente
        float elapsed = 0f;
        while (elapsed < darkenDuration)
        {
            elapsed += Time.deltaTime;
            globalLight.intensity = Mathf.Lerp(1f, 0f, elapsed / darkenDuration);
            yield return null;
        }

        globalLight.intensity = 0f;
    }

    // Chamado pelo FinalPortal quando o player chega
    public IEnumerator FinalSequence()
    {
        // 1. Trava o movimento e para o personagem
        player.canMove = false;
        player.movement = 0f;
        player.GetComponent<Rigidbody2D>().linearVelocity = Vector2.zero;

        // 2. Acende a luz gradualmente
        float elapsed = 0f;
        while (elapsed < lightenDuration)
        {
            elapsed += Time.deltaTime;
            globalLight.intensity = Mathf.Lerp(0f, 1f, elapsed / lightenDuration);
            yield return null;
        }

        globalLight.intensity = 1f;

        yield return new WaitForSeconds(0.5f);

        // 3. Vira o player para trás
        int playerFacing = player.GetFacingDirection();
        player.transform.eulerAngles = new Vector3(0, playerFacing == 1 ? 180 : 0, 0);

        yield return new WaitForSeconds(0.8f);

        // 4. Ghost faz fade in atrás do player
        Vector3 ghostPos = player.transform.position + new Vector3(playerFacing == 1 ? -1.5f : 1.5f, 0, 0);
        ghost.transform.position = ghostPos;
        ghost.gameObject.SetActive(true);
        yield return StartCoroutine(GhostFadeIn());

        yield return new WaitForSeconds(1f);

        // 5. Ghost faz a animação de desaparecer
        yield return StartCoroutine(GhostDieSequence());

        yield return new WaitForSeconds(0.5f);

        // 6. Vai para o epílogo
        SceneManager.LoadScene(epilogue);
    }

    // Aguarda o ghost terminar a animação antes de trocar de cena
    private IEnumerator GhostDieSequence()
    {
        ghost.reloadOnDeath = false; // <- impede o ghost de recarregar a cena
        ghost.Die();
        yield return new WaitForSeconds(2.5f);
    }

    private IEnumerator GhostFadeIn()
    {
        SpriteRenderer ghostSprite = ghost.GetComponent<SpriteRenderer>();
        Color c = ghostSprite.color;

        float elapsed = 0f;
        float duration = 1f;

        ghostSprite.color = new Color(c.r, c.g, c.b, 0f);

        while (elapsed < duration)
        {
            elapsed += Time.deltaTime;
            float alpha = Mathf.Lerp(0f, 1f, elapsed / duration);
            ghostSprite.color = new Color(c.r, c.g, c.b, alpha);
            yield return null;
        }

        ghostSprite.color = new Color(c.r, c.g, c.b, 1f);
    }
}