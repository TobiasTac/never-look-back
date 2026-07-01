using UnityEngine;

public class LavaRising : MonoBehaviour
{
    [Header("Configuração")]
    public float riseSpeed = 1f;
    public float topLimit; // posição Y onde a lava para

    private bool rising = true;

    void Update()
    {
        if (!rising) return;

        transform.position += Vector3.up * riseSpeed * Time.deltaTime;

        if (transform.position.y >= topLimit)
        {
            transform.position = new Vector3(transform.position.x, topLimit, transform.position.z);
            rising = false;
        }
    }

    void OnTriggerEnter2D(Collider2D other)
    {
        if (!other.CompareTag("Player")) return;

        Player player = other.GetComponent<Player>();
        if (player != null) player.TriggerDeath();
    }

    // Mostra o limite no editor
    void OnDrawGizmosSelected()
    {
        Gizmos.color = Color.red;
        Gizmos.DrawLine(
            new Vector3(transform.position.x - 5f, topLimit, 0),
            new Vector3(transform.position.x + 5f, topLimit, 0)
        );
    }
}