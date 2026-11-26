using UnityEngine;

// A hazard the player must avoid.
// Water / pits: tick "Instant Kill" -> she dies and goes back to the checkpoint.
// Thorns / spikes: untick it -> she just loses health.
// The collider on this object must have "Is Trigger" ticked.
public class Hazard : MonoBehaviour
{
    public bool instantKill = true;
    public int damage = 1;

    void OnTriggerStay2D(Collider2D other)
    {
        if (!other.CompareTag("Player"))
        {
            return;
        }

        PlayerHealth player = other.GetComponent<PlayerHealth>();
        if (player == null)
        {
            return;
        }

        if (instantKill)
        {
            player.Kill();
        }
        else
        {
            player.TakeDamage(damage, transform.position);
        }
    }
}
