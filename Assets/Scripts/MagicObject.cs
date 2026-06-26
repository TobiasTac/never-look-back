using UnityEngine;
using UnityEngine.Rendering.Universal;
using System.Collections;

public class MagicObject : MonoBehaviour
{
    [Header("Portais controlados por este objeto")]
    public Portal[] portals;

    [Header("Range")]
    public float activationRange = 5f;

    [Header("Light")]
    public Light2D ambientLight;
    public float targetIntensity = 1f;
    public float lightFadeDuration = 0.5f;

    private bool isActive = false;
    private Transform ghost;
    private Coroutine lightCoroutine;

    void Awake()
    {
        foreach (Portal portal in portals)
            portal.Close(instant: true);

        // Começa com a light apagada
        if (ambientLight != null)
            ambientLight.intensity = 0f;
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

        if (inRange && !isActive)
        {
            isActive = true;
            foreach (Portal portal in portals)
                portal.Open();

            if (lightCoroutine != null) StopCoroutine(lightCoroutine);
            lightCoroutine = StartCoroutine(FadeLight(targetIntensity));
        }
        else if (!inRange && isActive)
        {
            isActive = false;
            foreach (Portal portal in portals)
                portal.Close(instant: false);

            if (lightCoroutine != null) StopCoroutine(lightCoroutine);
            lightCoroutine = StartCoroutine(FadeLight(0f));
        }
    }

    private IEnumerator FadeLight(float target)
    {
        if (ambientLight == null) yield break;

        float elapsed = 0f;
        float start = ambientLight.intensity;

        while (elapsed < lightFadeDuration)
        {
            elapsed += Time.deltaTime;
            ambientLight.intensity = Mathf.Lerp(start, target, elapsed / lightFadeDuration);
            yield return null;
        }

        ambientLight.intensity = target;
    }

    void OnDrawGizmosSelected()
    {
        Gizmos.color = Color.cyan;
        Gizmos.DrawWireSphere(transform.position, activationRange);
    }
}