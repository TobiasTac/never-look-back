using UnityEngine;

public class SpotLightFollow : MonoBehaviour
{
    public Transform player;
    public Vector3 offset = new Vector3(0, 0.5f, 0);

    void LateUpdate()
    {
        if (player != null)
            transform.position = new Vector3(player.position.x + offset.x,
                                             player.position.y + offset.y,
                                             transform.position.z);
    }
}