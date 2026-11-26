using System.Collections;
using UnityEngine;

// Health for any enemy. The player's sword calls TakeDamage().
// If the enemy has an Animator with "Hit" and "Die" states, they play too.
public class EnemyHealth : MonoBehaviour
{
    public int maxHealth = 3;
    public float knockbackForce = 1.5f;
    public float hurtTime = 0.3f;        // enemy stops moving for this long after a hit
    public float destroyDelay = 1.5f;    // time for the death animation before it disappears

    private int health;
    private bool isDead;
    private float hurtTimer;
    private SpriteRenderer sr;
    private Rigidbody2D rb;
    private Animator anim;

    void Start()
    {
        health = maxHealth;
        sr = GetComponent<SpriteRenderer>();
        rb = GetComponent<Rigidbody2D>();
        anim = GetComponent<Animator>();
    }

    void Update()
    {
        if (hurtTimer > 0)
        {
            hurtTimer -= Time.deltaTime;
        }
    }

    public void TakeDamage(int damage, Vector2 attackerPosition)
    {
        if (isDead)
        {
            return;
        }

        health -= damage;
        hurtTimer = hurtTime;
        AudioManager.Play(Sound.EnemyHit);
        StartCoroutine(FlashRed());

        // push the enemy away from the player
        if (rb != null && rb.bodyType == RigidbodyType2D.Dynamic)
        {
            float direction = Mathf.Sign(transform.position.x - attackerPosition.x);
            rb.linearVelocity = new Vector2(direction * knockbackForce, knockbackForce * 0.5f);
        }

        if (health <= 0)
        {
            Die();
        }
        else
        {
            PlayAnimation("Hit");
        }
    }

    void Die()
    {
        isDead = true;
        PlayAnimation("Die");
        Effects.Dust(transform.position, 8, 0.3f, 1.5f, 1.2f); // poof

        // stop it from hurting the player or blocking her
        foreach (Collider2D col in GetComponents<Collider2D>())
        {
            col.enabled = false;
        }
        if (rb != null)
        {
            rb.linearVelocity = Vector2.zero;
            rb.bodyType = RigidbodyType2D.Kinematic;
        }

        Destroy(gameObject, destroyDelay);
    }

    void PlayAnimation(string state)
    {
        if (anim != null && anim.HasState(0, Animator.StringToHash(state)))
        {
            anim.Play(state, 0, 0f);
        }
    }

    IEnumerator FlashRed()
    {
        sr.color = Color.red;
        yield return new WaitForSeconds(0.15f);
        sr.color = Color.white;
    }

    // enemy movement scripts use these to stop moving when hurt or dead
    public bool IsDead()
    {
        return isDead;
    }

    public bool IsHurt()
    {
        return hurtTimer > 0;
    }
}
