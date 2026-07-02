using UnityEngine;

public class EpilogueTrigger : MonoBehaviour
{
    public EpilogueManager epilogueManager;

    private bool triggered = false;

    void OnTriggerEnter2D(Collider2D other)
    {
        Debug.Log($"Trigger acionado por: {other.gameObject.name} — Tag: {other.tag}");
        if (triggered || !other.CompareTag("EpiloguePlayer")) return;
        triggered = true;
        epilogueManager.StartFinalSequence();
    }
}