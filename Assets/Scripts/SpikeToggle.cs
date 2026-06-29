using UnityEngine;
using System.Collections;

public class SpikeToggle : MonoBehaviour
{
    [Header("Configuração")]
    public bool startsVisible = true;
    public float animationDuration = 0.3f;
    public float toggleInterval = 0.5f;

    [Header("Detecção")]
    public Transform detectionZone;

    public float minX = -1.5f;
    public float maxX = 1.5f;
    public float minY = -1f;
    public float maxY = 1f;

    public float detectionAngle = 0f;

    [Header("Direção da animação")]
    public bool growFromBottom = true;

    private Vector3 originalScale;
    private Vector3 originalPosition;
    private bool isVisible;
    private Coroutine scaleCoroutine;
    private Coroutine toggleCoroutine;
    private Collider2D spikeCollider;
    private Player player;

    void Awake()
    {
        originalScale = transform.localScale;
        originalPosition = transform.localPosition; // posição relativa ao pai — correta para filho de plataforma móvel
        spikeCollider = GetComponent<Collider2D>();
        isVisible = startsVisible;

        if (!startsVisible)
        {
            transform.localScale = Vector3.zero;
            transform.localPosition = HiddenPosition();
            spikeCollider.enabled = false;
        }
    }

    Vector3 HiddenPosition()
    {
        float offset = originalScale.y / 2f;
        // Usa localPosition para ser relativo ao pai (plataforma móvel)
        return originalPosition + new Vector3(0, growFromBottom ? -offset : offset, 0);
    }

    void Update()
    {
        Vector2 center = detectionZone != null
            ? (Vector2)detectionZone.position
            : (Vector2)transform.position;

        Vector2 boxCenter = center + new Vector2(
            (minX + maxX) / 2f,
            (minY + maxY) / 2f
        );

        Vector2 boxSize = new Vector2(
            maxX - minX,
            maxY - minY
        );

        Collider2D hit = Physics2D.OverlapBox(
            boxCenter,
            boxSize,
            detectionAngle,
            LayerMask.GetMask("Player")
        );

        if (hit != null && player == null)
            player = hit.GetComponent<Player>();

        bool playerMoving = player != null && Mathf.Abs(player.movement) > 0.01f;

        if (playerMoving && toggleCoroutine == null)
            toggleCoroutine = StartCoroutine(ToggleRoutine());
        else if (!playerMoving && toggleCoroutine != null)
        {
            StopCoroutine(toggleCoroutine);
            toggleCoroutine = null;
        }
    }

    private IEnumerator ToggleRoutine()
    {
        while (true)
        {
            Toggle();
            yield return new WaitForSeconds(toggleInterval);
        }
    }

    void Toggle()
    {
        isVisible = !isVisible;

        if (scaleCoroutine != null) StopCoroutine(scaleCoroutine);

        if (isVisible)
            scaleCoroutine = StartCoroutine(ScaleTo(originalScale));
        else
            scaleCoroutine = StartCoroutine(ScaleTo(Vector3.zero));
    }

    private IEnumerator ScaleTo(Vector3 target)
    {
        float elapsed = 0f;
        Vector3 startScale = transform.localScale;
        Vector3 startLocalPos = transform.localPosition;
        // targetPos sempre relativo ao pai
        Vector3 targetLocalPos = target == Vector3.zero ? HiddenPosition() : originalPosition;

        if (target != Vector3.zero)
            spikeCollider.enabled = true;

        while (elapsed < animationDuration)
        {
            elapsed += Time.deltaTime;
            float t = elapsed / animationDuration;
            float smooth = target == Vector3.zero
                ? Mathf.Pow(t, 3f)
                : 1f - Mathf.Pow(1f - t, 3f);

            transform.localScale = Vector3.Lerp(startScale, target, smooth);
            transform.localPosition = Vector3.Lerp(startLocalPos, targetLocalPos, smooth);
            yield return null;
        }

        transform.localScale = target;
        transform.localPosition = targetLocalPos;

        if (target == Vector3.zero)
            spikeCollider.enabled = false;
    }

    void OnDrawGizmosSelected()
    {
        Gizmos.color = Color.red;

        Vector2 center = detectionZone != null
            ? (Vector2)detectionZone.position
            : (Vector2)transform.position;

        Vector2 boxCenter = center + new Vector2(
            (minX + maxX) / 2f,
            (minY + maxY) / 2f
        );

        Vector2 boxSize = new Vector2(
            maxX - minX,
            maxY - minY
        );

        Matrix4x4 rotMatrix = Matrix4x4.TRS(
            boxCenter,
            Quaternion.Euler(0, 0, detectionAngle),
            Vector3.one
        );

        Gizmos.matrix = rotMatrix;
        Gizmos.DrawWireCube(Vector3.zero, boxSize);
        Gizmos.matrix = Matrix4x4.identity;
    }
}