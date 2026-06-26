using UnityEngine;

public class CloudLoop : MonoBehaviour
{
    public float speed = 2f;

    // Posição X onde a nuvem reaparece (à direita da tela)
    public float spawnX = 14f;

    // Posição X onde a nuvem é reposicionada (saiu pela esquerda)
    public float despawnX = -12f;

    void Update()
    {
        transform.position += Vector3.left * speed * Time.deltaTime;

        if (transform.position.x < despawnX)
        {
            transform.position = new Vector3(spawnX, transform.position.y, transform.position.z);
        }
    }
}