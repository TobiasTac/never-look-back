using UnityEngine;

public class MovingPlatform : MonoBehaviour
{
    public float speed = 2f;
    public float distance = 3f;

    private Vector3 startPos;
    private Vector3 lastPos;
    private int direction = 1;

    public Vector3 Movement { get; private set; }

    void Start()
    {
        startPos = transform.position;
        lastPos = transform.position;
    }

    void FixedUpdate()
    {
        // Atualiza a posição manualmente (Modo cinemático)
        transform.position += Vector3.right * direction * speed * Time.fixedDeltaTime;

        if (Vector3.Distance(startPos, transform.position) >= distance)
        {
            direction *= -1;
        }

        // Calcula exatamente quanto a plataforma andou neste frame de física
        Movement = transform.position - lastPos;
        lastPos = transform.position;
    }
}