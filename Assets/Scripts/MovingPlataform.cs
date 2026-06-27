using UnityEngine;

public class MovingPlatform : MonoBehaviour
{
    public float speed = 2f;
    public float distance = 3f;

    [Header("Configurações de Movimento")]
    [Tooltip("Defina a direção do movimento. Ex: X=1, Y=0 (Direita) | X=0, Y=1 (Cima)")]
    public Vector3 moveDirection = Vector3.right;

    private Vector3 startPos;
    private Vector3 lastPos;
    private int direction = 1;

    public Vector3 Movement { get; private set; }

    void Start()
    {
        startPos = transform.position;
        lastPos = transform.position;

        // Normalizamos o vetor para garantir que a velocidade seja constante, 
        // independente dos números que você colocar no Inspector
        moveDirection = moveDirection.normalized;
    }

    void FixedUpdate()
    {
        // Agora usamos o moveDirection no lugar do Vector3.right
        transform.position += moveDirection * direction * speed * Time.fixedDeltaTime;

        if (Vector3.Distance(startPos, transform.position) >= distance)
        {
            direction *= -1;
        }

        // Calcula exatamente quanto a plataforma andou neste frame de física
        Movement = transform.position - lastPos;
        lastPos = transform.position;
    }
}