using UnityEngine;

// Hurts the player when she touches this object.
// Put it on enemies (their body hurts) and on hazards like thorns or water.
public class DamageOnTouch : MonoBehaviour
{
    public int damage = 1;

    // for solid colliders (enemy bodies)
    void OnCollisionStay2D(Collision2D collision)
    {
        HurtPlayer(collision.gameObject);
    }

    // for colliders with "Is Trigger" ticked (water, thorns)
    void OnTriggerStay2D(Collider2D other)
    {
        HurtPlayer(other.gameObject);
    }

    void HurtPlayer(GameObject other)
    {
        if (!other.CompareTag("Player"))
        {
            return;
        }

        PlayerHealth player = other.GetComponent<PlayerHealth>();
        if (player != null)
        {
            // PlayerHealth ignores this while she is still blinking
            player.TakeDamage(damage, transform.position);
        }
    }
}
