using UnityEngine;
using UnityEngine.InputSystem;

// Sword attack. Press J or left mouse click.
// Hits every enemy inside a small circle in front of the player.
public class PlayerSword : MonoBehaviour
{
    public Transform attackPoint;      // empty child in front of her
    public float attackRange = 0.6f;
    public LayerMask enemyLayer;
    public int damage = 1;

    public float attackDuration = 0.4f; // how long the attack animation lasts
    public float hitDelay = 0.15f;      // the sword hits a bit after the swing starts
    public float cooldown = 0.45f;      // time before she can attack again

    private PlayerAnimation playerAnim;
    private float cooldownTimer;

    void Start()
    {
        playerAnim = GetComponent<PlayerAnimation>();
    }

    void Update()
    {
        // no attacking while paused or on the win/game over screen
        if (Time.timeScale == 0f)
        {
            return;
        }

        cooldownTimer -= Time.deltaTime;

        bool attackPressed = Keyboard.current.jKey.wasPressedThisFrame
                          || Mouse.current.leftButton.wasPressedThisFrame;

        if (attackPressed && cooldownTimer <= 0)
        {
            cooldownTimer = cooldown;
            playerAnim.PlayAction("Attack", attackDuration);
            AudioManager.Play(Sound.Swing);
            Invoke("DealDamage", hitDelay);
        }
    }

    void DealDamage()
    {
        Collider2D[] hits = Physics2D.OverlapCircleAll(attackPoint.position, attackRange, enemyLayer);

        // the sword blade glints at the end of the swing
        Effects.Glint(attackPoint.position, 0.8f);

        foreach (Collider2D hit in hits)
        {
            EnemyHealth enemy = hit.GetComponentInParent<EnemyHealth>();
            if (enemy != null)
            {
                enemy.TakeDamage(damage, transform.position);

                // a bigger sparkle where the sword hit the enemy
                Effects.Glint(hit.ClosestPoint(attackPoint.position), 1.3f);
            }
        }
    }

    // red circle in the Scene view shows how far the sword reaches
    void OnDrawGizmosSelected()
    {
        if (attackPoint != null)
        {
            Gizmos.color = Color.red;
            Gizmos.DrawWireSphere(attackPoint.position, attackRange);
        }
    }
}
