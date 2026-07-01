using UnityEngine;

public class EpilogueTrigger : MonoBehaviour
{
    public EpilogueManager epilogueManager;

    private bool triggered = false;

    void OnTriggerEnter2D(Collider2D other)
    {
        if (triggered || !other.CompareTag("Player")) return;
        triggered = true;
        epilogueManager.StartFinalSequence();
    }
}