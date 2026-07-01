using UnityEngine;

public class FinalPortal : MonoBehaviour
{
    public FinalSceneManager sceneManager;

    private bool triggered = false;

    void OnTriggerEnter2D(Collider2D other)
    {
        if (triggered || !other.CompareTag("Player")) return;
        triggered = true;
        StartCoroutine(sceneManager.FinalSequence());
    }
}