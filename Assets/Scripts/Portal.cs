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

    // [Header("Áudio")]
    // public AudioClip portalLoopSound;
    // public float soundDelay = 0.5f; // tempo pra runa "terminar" antes do loop começar
    // private AudioSource loopSource;
    // public float fadeInDuration = 1f;
    // public float loopVolume = 0.6f;

    // private AudioSource GetLoopSource()
    // {
    //     if (loopSource == null)
    //     {
    //         loopSource = gameObject.AddComponent<AudioSource>();
    //         loopSource.volume = 0.6f;
    //         loopSource.playOnAwake = false;
    //         loopSource.loop = true;
    //         loopSource.clip = portalLoopSound;
    //     }
    //     return loopSource;
    // }

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

        // StartCoroutine(FadeInLoop());
    }

    // private IEnumerator FadeInLoop()
    // {
    //     if (portalLoopSound == null) yield break;

    //     var src = GetLoopSource();
    //     src.volume = 0f;
    //     src.Play();

    //     float elapsed = 0f;
    //     while (elapsed < fadeInDuration)
    //     {
    //         elapsed += Time.deltaTime;
    //         src.volume = Mathf.Lerp(0f, loopVolume, elapsed / fadeInDuration);
    //         yield return null;
    //     }

    //     src.volume = loopVolume;
    // }

    // private IEnumerator PlayLoopAfterDelay()
    // {
    //     yield return new WaitForSeconds(soundDelay);
    //     if (portalLoopSound != null && !loopSource.isPlaying)
    //         loopSource.Play();
    // }

    public void Close(bool instant)
    {
        if (!isOpen && !instant) return;
        isOpen = false;

        // GetLoopSource().Stop();

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
    Debug.Log($"isCoolingDown: {isCoolingDown}");
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
        yield return new WaitForSeconds(0.3f);
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