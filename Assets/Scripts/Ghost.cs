using UnityEngine;
using UnityEngine.InputSystem;
using UnityEngine.SceneManagement;
using UnityEngine.UI;
using System.Collections;
using UnityEngine.Rendering.Universal;
using UnityEngine.SceneManagement;

public class Ghost : MonoBehaviour
{
    public float speed = 5f;
    public float fadeOutDuration = 1f;

    [Header("Light")]
    public Light2D globalLight;
    public float targetLightIntensity = 0f; // intensidade final
    public float lightFadeDuration = 0.6f;

    [Header("Slow Motion")]
    public float slowMotionScale = 0.1f;
    public float slowMotionDuration = 0.4f;

    [Header("UI")]
    public Text deathText; // arraste o Text do Canvas aqui no Inspector

    private Rigidbody2D rig;
    private Animator anim;
    private SpriteRenderer spriteRenderer;
    private Vector2 movement;
    private bool isDead = false;

    void Start()
    {
        rig = GetComponent<Rigidbody2D>();
        anim = GetComponent<Animator>();
        spriteRenderer = GetComponent<SpriteRenderer>();
        if (spriteRenderer == null)
            spriteRenderer = GetComponentInChildren<SpriteRenderer>();

        rig.gravityScale = 0f;

        var playerInput = GetComponent<PlayerInput>();
        playerInput.actions.Disable();
        playerInput.actions.FindActionMap("Player").Enable();

        if (deathText != null)
            deathText.color = new Color(deathText.color.r, deathText.color.g, deathText.color.b, 0f);
    }

    public void OnMove(InputValue value)
    {
        if (isDead) return;
        movement = value.Get<Vector2>();
    }

    void FixedUpdate()
    {
        if (isDead) return;
        Move();
        UpdateAnimations();
    }

    void Move()
    {
        rig.linearVelocity = new Vector2(movement.x * speed, movement.y * speed);

        if (movement.x > 0)
            transform.eulerAngles = new Vector3(0, 0, 0);
        else if (movement.x < 0)
            transform.eulerAngles = new Vector3(0, 180, 0);
    }

    void UpdateAnimations()
    {
        if (anim == null) return;

        if (Mathf.Abs(movement.x) > 0.01f || Mathf.Abs(movement.y) > 0.01f)
            anim.SetInteger("transition", 1);
        else
            anim.SetInteger("transition", 0);
    }

    public void Die()
    {
        if (isDead) return;

        // Garante que as referências existem, mesmo se Start() não rodou ainda
        if (rig == null) rig = GetComponent<Rigidbody2D>();
        if (anim == null) anim = GetComponent<Animator>();
        if (spriteRenderer == null) spriteRenderer = GetComponent<SpriteRenderer>();

        isDead = true;
        movement = Vector2.zero;
        rig.linearVelocity = Vector2.zero;

        if (anim != null)
            anim.SetInteger("transition", 3);

        StartCoroutine(DeathSequence());
    }

    void FreezeScene()
    {
        foreach (Rigidbody2D rb in FindObjectsByType<Rigidbody2D>(FindObjectsInactive.Exclude))
        {
            if (rb.gameObject != gameObject)
            {
                rb.linearVelocity = Vector2.zero;
                rb.angularVelocity = 0f;
                rb.bodyType = RigidbodyType2D.Static;
            }
        }

        foreach (Animator a in FindObjectsByType<Animator>(FindObjectsInactive.Exclude))
        {
            if (a.gameObject != gameObject)
                a.speed = 0f;
        }
    }

    public bool reloadOnDeath = true; // desmarque no Inspector na fase final

    private IEnumerator ReloadAfterDeath()
    {
        yield return new WaitForSecondsRealtime(1.5f);
        
        if (reloadOnDeath)
            SceneManager.LoadScene(SceneManager.GetActiveScene().buildIndex);
    }

    private IEnumerator DeathSequence()
    {
        yield return StartCoroutine(SlowMotionRoutine());

        FreezeScene();

        StartCoroutine(TextFadeRoutine());
        StartCoroutine(LightFadeRoutine());

        yield return new WaitForSecondsRealtime(0.2f);

        yield return StartCoroutine(GhostFadeAndScaleRoutine());

        yield return new WaitForSecondsRealtime(0.3f);

        Time.timeScale = 1f;

        // Usa o ReloadAfterDeath para respeitar o reloadOnDeath
        yield return StartCoroutine(ReloadAfterDeath());
    }

    private IEnumerator SlowMotionRoutine()
    {
        float elapsed = 0f;

        while (elapsed < slowMotionDuration)
        {
            elapsed += Time.unscaledDeltaTime;
            Time.timeScale = Mathf.Lerp(1f, slowMotionScale, elapsed / slowMotionDuration);
            yield return null;
        }

        Time.timeScale = slowMotionScale;
    }

    private IEnumerator TextFadeRoutine()
    {
        if (deathText == null) yield break;

        float elapsed = 0f;
        float fadeDuration = 0.5f;

        // Fade in do texto
        while (elapsed < fadeDuration)
        {
            elapsed += Time.unscaledDeltaTime;
            float alpha = Mathf.Lerp(0f, 1f, elapsed / fadeDuration);
            deathText.color = new Color(deathText.color.r, deathText.color.g, deathText.color.b, alpha);
            yield return null;
        }
    }

    private IEnumerator LightFadeRoutine()
    {
        if (globalLight == null) yield break;

        float elapsed = 0f;
        float originalIntensity = globalLight.intensity;

        while (elapsed < lightFadeDuration)
        {
            elapsed += Time.unscaledDeltaTime;
            globalLight.intensity = Mathf.Lerp(originalIntensity, targetLightIntensity, elapsed / lightFadeDuration);
            yield return null;
        }

        globalLight.intensity = targetLightIntensity;
    }

    private IEnumerator GhostFadeAndScaleRoutine()
    {
        float elapsed = 0f;
        Color originalColor = spriteRenderer.color;
        Vector3 originalScale = transform.localScale;
        Vector3 targetScale = originalScale * 1.5f; // cresce 50% enquanto some

        while (elapsed < fadeOutDuration)
        {
            elapsed += Time.unscaledDeltaTime;
            float t = elapsed / fadeOutDuration;

            // Fade
            float alpha = Mathf.Lerp(1f, 0f, t);
            spriteRenderer.color = new Color(originalColor.r, originalColor.g, originalColor.b, alpha);

            // Scale
            transform.localScale = Vector3.Lerp(originalScale, targetScale, t);

            yield return null;
        }

        spriteRenderer.color = new Color(originalColor.r, originalColor.g, originalColor.b, 0f);
        transform.localScale = targetScale;
    }
}