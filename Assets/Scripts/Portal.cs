using UnityEngine;
using System.Collections;

public class Portal : MonoBehaviour
{
    [Header("Entrance Settings")]
    public Transform exitPoint;
    public float pullSpeed = 5f;
    public float activateDistance = 2f;

    [Header("Exit Settings")]
    public Vector2 throwDirection = new Vector2(0, 1);
    public float throwForce = 15f;
    public bool faceRightOnExit = true;
    public float freezeTimeOnExit = 0.3f;

    [Header("Scale Animation")]
    public float openDuration = 0.4f;
    public float closeDuration = 0.3f;
    [SerializeField] private Vector3 originalScale;
    private bool isCoolingDown = false;
    private Coroutine scaleCoroutine;
    private bool isOpen = false;

    void Awake()
    {
        // Só captura se ainda não foi salvo pelo Inspector
        if (originalScale == Vector3.zero)
            originalScale = transform.localScale;
    }
    public void Open()
    {
        if (isOpen) return;
        isOpen = true;

        gameObject.SetActive(true);

        if (scaleCoroutine != null) StopCoroutine(scaleCoroutine);
        scaleCoroutine = StartCoroutine(ScaleTo(originalScale, openDuration));
    }

    public void Close(bool instant)
    {
        if (!isOpen && !instant) return;
        isOpen = false;

        if (instant)
        {
            transform.localScale = Vector3.zero;
            gameObject.SetActive(false);
            return;
        }

        if (scaleCoroutine != null) StopCoroutine(scaleCoroutine);
        scaleCoroutine = StartCoroutine(ScaleToZero(closeDuration));
    }

    private void OnTriggerStay2D(Collider2D other)
    {
        if (isCoolingDown || !other.CompareTag("Player")) return;

        Rigidbody2D playerRb = other.GetComponent<Rigidbody2D>();
        Player player = other.GetComponent<Player>();

        Vector2 direction = (transform.position - other.transform.position);
        float distance = direction.magnitude;

        if (distance < activateDistance)
        {
            if (player != null)
                player.canMove = false;

            playerRb.linearVelocity = direction.normalized * pullSpeed;

            if (distance < 0.2f)
            {
                if (exitPoint != null)
                {
                    other.transform.position = exitPoint.position;

                    Portal exitPortal = exitPoint.GetComponent<Portal>();
                    if (exitPortal != null)
                    {
                        exitPortal.StartCooldown();

                        if (player != null)
                        {
                            Vector2 throwVelocity = exitPortal.throwDirection.normalized * exitPortal.throwForce;
                            player.ExitPortal(throwVelocity, exitPortal.faceRightOnExit, exitPortal.freezeTimeOnExit);
                        }
                    }
                    else
                    {
                        playerRb.linearVelocity = Vector2.zero;
                        if (player != null) player.canMove = true;
                    }
                }
                else
                {
                    playerRb.linearVelocity = Vector2.zero;
                    if (player != null) player.canMove = true;
                }

                StartCooldown();
            }
        }
    }

    private void OnTriggerExit2D(Collider2D other)
    {
        if (other.CompareTag("Player") && !isCoolingDown)
        {
            Player player = other.GetComponent<Player>();
            if (player != null)
                player.canMove = true;
        }
    }

    public void StartCooldown()
    {
        StartCoroutine(CooldownRoutine());
    }

    private IEnumerator CooldownRoutine()
    {
        isCoolingDown = true;
        yield return new WaitForSeconds(1f);
        isCoolingDown = false;
    }

    private IEnumerator ScaleTo(Vector3 target, float duration)
    {
        float elapsed = 0f;
        Vector3 start = transform.localScale;

        while (elapsed < duration)
        {
            elapsed += Time.deltaTime;
            float t = elapsed / duration;
            float smooth = 1f - Mathf.Pow(1f - t, 3f);
            transform.localScale = Vector3.Lerp(start, target, smooth);
            yield return null;
        }

        transform.localScale = target;
    }

    private IEnumerator ScaleToZero(float duration)
    {
        float elapsed = 0f;
        Vector3 start = transform.localScale;

        while (elapsed < duration)
        {
            elapsed += Time.deltaTime;
            float t = elapsed / duration;
            float smooth = Mathf.Pow(t, 3f);
            transform.localScale = Vector3.Lerp(start, Vector3.zero, smooth);
            yield return null;
        }

        transform.localScale = Vector3.zero;
        gameObject.SetActive(false);
    }
}