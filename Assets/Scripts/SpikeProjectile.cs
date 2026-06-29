using UnityEngine;

public class SpikeProjectile : MonoBehaviour
{
    [Header("Configuração")]
    public Vector2 fireDirection = Vector2.right;
    public float fireSpeed = 10f;
    public float detectionAngle = 0f;
    public Transform detectionZone;

    private bool fired = false;
    private Rigidbody2D rig;

    [Header("Área de Detecção")]
    public float minX = -2f;
    public float maxX = 2f;
    public float minY = -1f;
    public float maxY = 1f;

    void Awake()
    {
        rig = GetComponent<Rigidbody2D>();
        rig.gravityScale = 0f;
    }

    void Update()
    {
        if (fired) return;

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

        if (hit != null)
            Fire();
    }
    void Fire()
    {
        fired = true;
        rig.linearVelocity = fireDirection.normalized * fireSpeed;
    }

    void OnTriggerEnter2D(Collider2D other)
    {
        // Mata o player (isto funciona quer o espinho esteja parado ou em movimento)
        if (other.CompareTag("Player"))
        {
            Player player = other.GetComponent<Player>();
            if (player != null) player.TriggerDeath();
            Destroy(gameObject);
            return;
        }

        // CORREÇÃO: Só desaparece ao bater em paredes/chão SE já tiver sido disparado!
        if (fired && !other.CompareTag("Ghost"))
        {
            Destroy(gameObject);
        }
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

        Gizmos.color = Color.yellow;
        Gizmos.DrawRay(transform.position, fireDirection.normalized * 2f);
    }
}