using UnityEngine;
using System.Collections;

public class WindZone : MonoBehaviour
{
    public enum WindType
    {
        Increasing,  // Plataforma 1 — vento cresce gradualmente
        Constant,    // Plataforma 2 — vento constante
        Changing     // Plataforma 3 — vento muda de direção e cresce
    }

    [Header("Tipo de vento")]
    public WindType windType = WindType.Constant;

    [Header("Configuração")]
    public Vector2 windDirection = Vector2.left; // direção base do vento
    public float minWindForce = 2f;
    public float maxWindForce = 10f;
    public float windChangeDuration = 3f; // tempo para crescer ou mudar direção

    private Rigidbody2D playerRb;
    private bool playerInZone = false;
    private float currentForce = 0f;
    private Vector2 currentDirection;
    private Coroutine windCoroutine;

    void Start()
    {
        currentDirection = windDirection.normalized;
    }

    void FixedUpdate()
    {
        if (!playerInZone || playerRb == null) return;

        // Para o vento se o player estiver morto
        Player player = playerRb.GetComponent<Player>();
        if (player != null && player.IsDead) return;

        playerRb.AddForce(currentDirection * currentForce, ForceMode2D.Force);
    }

    void OnTriggerEnter2D(Collider2D other)
    {
        if (!other.CompareTag("Player")) return;

        playerRb = other.GetComponent<Rigidbody2D>();
        playerInZone = true;

        if (windCoroutine != null) StopCoroutine(windCoroutine);

        switch (windType)
        {
            case WindType.Increasing:
                windCoroutine = StartCoroutine(IncreasingWindRoutine());
                break;
            case WindType.Constant:
                currentForce = maxWindForce;
                break;
            case WindType.Changing:
                windCoroutine = StartCoroutine(ChangingWindRoutine());
                break;
        }
    }

    void OnTriggerExit2D(Collider2D other)
    {
        if (!other.CompareTag("Player")) return;

        playerInZone = false;
        playerRb = null;
        currentForce = 0f;
        currentDirection = windDirection.normalized;

        if (windCoroutine != null)
        {
            StopCoroutine(windCoroutine);
            windCoroutine = null;
        }
    }

    // Plataforma 1 — cresce de min até max enquanto atravessa
    private IEnumerator IncreasingWindRoutine()
    {
        currentForce = minWindForce;
        float elapsed = 0f;

        while (playerInZone)
        {
            elapsed += Time.fixedDeltaTime;
            float t = Mathf.Clamp01(elapsed / windChangeDuration);
            currentForce = Mathf.Lerp(minWindForce, maxWindForce, t);
            yield return new WaitForFixedUpdate();
        }
    }

    // Plataforma 3 — cresce e muda de direção ciclicamente
    private IEnumerator ChangingWindRoutine()
    {
        currentForce = minWindForce;

        while (playerInZone)
        {
            // Cresce na direção atual
            float elapsed = 0f;
            Vector2 startDir = currentDirection;

            while (elapsed < windChangeDuration && playerInZone)
            {
                elapsed += Time.fixedDeltaTime;
                float t = Mathf.Clamp01(elapsed / windChangeDuration);
                currentForce = Mathf.Lerp(minWindForce, maxWindForce, t);
                yield return new WaitForFixedUpdate();
            }

            // Inverte a direção
            currentDirection = -currentDirection;
            currentForce = minWindForce;
        }
    }

    // Mostra a área e direção do vento no editor
    void OnDrawGizmosSelected()
    {
        Gizmos.color = Color.cyan;

        // Desenha a área
        Collider2D col = GetComponent<Collider2D>();
        if (col is BoxCollider2D box)
        {
            Gizmos.DrawWireCube(transform.position + (Vector3)box.offset, box.size);
        }

        // Desenha a direção do vento
        Gizmos.color = Color.blue;
        Gizmos.DrawRay(transform.position, windDirection.normalized * 2f);
    }
}