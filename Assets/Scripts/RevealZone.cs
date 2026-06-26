using UnityEngine;
using System.Collections;

public class RevealZone : MonoBehaviour
{
    [Header("Plataforma revelada")]
    public GameObject platform;
    public GameObject vfx; // arraste o VFX aqui
    public float revealDuration = 0.5f;
    public float hideDelay = 1f;

    [Header("Range")]
    public float activationRange = 3f;

    private bool isRevealed = false;
    private Transform ghost;
    private Coroutine revealCoroutine;
    private SpriteRenderer platformRenderer;
    private Collider2D platformCollider;

    void Awake()
    {
        if (platform != null)
        {
            platformRenderer = platform.GetComponent<SpriteRenderer>();
            platformCollider = platform.GetComponent<Collider2D>();
            SetPlatformAlpha(0f);
            platformCollider.enabled = false;
        }

        // VFX começa desativado
        if (vfx != null)
            vfx.SetActive(false);
    }

    void Update()
    {
        if (ghost == null)
        {
            Ghost g = FindAnyObjectByType<Ghost>();
            if (g != null) ghost = g.transform;
            return;
        }

        float distance = Vector2.Distance(transform.position, ghost.position);
        bool inRange = distance <= activationRange;

        if (inRange && !isRevealed)
        {
            isRevealed = true;
            if (revealCoroutine != null) StopCoroutine(revealCoroutine);
            revealCoroutine = StartCoroutine(RevealPlatform());
        }
        else if (!inRange && isRevealed)
        {
            isRevealed = false;
            if (revealCoroutine != null) StopCoroutine(revealCoroutine);
            revealCoroutine = StartCoroutine(HidePlatform());
        }
    }

    private IEnumerator RevealPlatform()
    {
        platformCollider.enabled = true;

        // Ativa o VFX junto com a plataforma
        if (vfx != null)
            vfx.SetActive(true);

        float elapsed = 0f;
        while (elapsed < revealDuration)
        {
            elapsed += Time.deltaTime;
            SetPlatformAlpha(Mathf.Lerp(0f, 1f, elapsed / revealDuration));
            yield return null;
        }

        SetPlatformAlpha(1f);
    }

    private IEnumerator HidePlatform()
    {
        yield return new WaitForSeconds(hideDelay);

        // Desativa o VFX junto com a plataforma
        if (vfx != null)
            vfx.SetActive(false);

        float elapsed = 0f;
        while (elapsed < revealDuration)
        {
            elapsed += Time.deltaTime;
            SetPlatformAlpha(Mathf.Lerp(1f, 0f, elapsed / revealDuration));
            yield return null;
        }

        SetPlatformAlpha(0f);
        platformCollider.enabled = false;
    }

    void SetPlatformAlpha(float alpha)
    {
        if (platformRenderer == null) return;
        Color c = platformRenderer.color;
        platformRenderer.color = new Color(c.r, c.g, c.b, alpha);
    }

    void OnDrawGizmosSelected()
    {
        Gizmos.color = Color.magenta;
        Gizmos.DrawWireSphere(transform.position, activationRange);
    }
}