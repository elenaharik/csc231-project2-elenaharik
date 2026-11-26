using System.Collections;
using UnityEngine;

// Player health: gets hurt, blinks for a moment (can't be hit again),
// dies, and respawns at the last checkpoint.
public class PlayerHealth : MonoBehaviour
{
    public int maxHealth = 5;
    public int lives = 3;                  // lose them all = Game Over
    public float invincibleTime = 1f;      // safe time after being hit
    public float knockbackForce = 6f;
    public float knockbackTime = 0.2f;
    public float respawnDelay = 1.5f;      // time to show the death animation
    public float fallDeathY = -20f;        // falling below this = death

    private int health;
    private bool isDead;
    private bool isInvincible;
    private Vector3 respawnPoint;

    private SpriteRenderer sr;
    private Rigidbody2D rb;
    private PlayerMovement movement;
    private PlayerAnimation playerAnim;
    private PlayerSword sword;

    void Start()
    {
        health = maxHealth;
        respawnPoint = transform.position; // start position until she reaches a checkpoint

        sr = GetComponent<SpriteRenderer>();
        rb = GetComponent<Rigidbody2D>();
        movement = GetComponent<PlayerMovement>();
        playerAnim = GetComponent<PlayerAnimation>();
        sword = GetComponent<PlayerSword>();
    }

    void Update()
    {
        // fell off the level
        if (!isDead && transform.position.y < fallDeathY)
        {
            health = 0;
            Die();
        }
    }

    public void TakeDamage(int damage, Vector2 attackerPosition)
    {
        if (isDead || isInvincible)
        {
            return;
        }

        health -= damage;

        if (health <= 0)
        {
            health = 0;
            Die();
            return;
        }

        // push her away from whatever hit her
        float direction = Mathf.Sign(transform.position.x - attackerPosition.x);
        movement.Knockback(new Vector2(direction * knockbackForce, knockbackForce * 0.6f), knockbackTime);

        StartCoroutine(HurtBlink(true));
    }

    // instant death (water, bottomless pits)
    public void Kill()
    {
        if (isDead)
        {
            return;
        }
        health = 0;
        Die();
    }

    IEnumerator HurtBlink(bool flashRed)
    {
        isInvincible = true;

        // flash red first (only when hit, not when respawning)
        if (flashRed)
        {
            sr.color = Color.red;
        }
        yield return new WaitForSeconds(0.1f);
        sr.color = Color.white;

        // then blink on and off
        float timer = 0.1f;
        while (timer < invincibleTime)
        {
            sr.enabled = !sr.enabled;
            yield return new WaitForSeconds(0.1f);
            timer += 0.1f;
        }

        sr.enabled = true;
        isInvincible = false;
    }

    void Die()
    {
        isDead = true;
        StopAllCoroutines();
        sr.enabled = true;
        sr.color = Color.white;

        movement.canMove = false;
        sword.enabled = false;
        rb.linearVelocity = Vector2.zero;

        playerAnim.PlayAction("Death", respawnDelay + 1f);

        lives--;
        if (lives > 0)
        {
            Invoke("Respawn", respawnDelay);
        }
        else
        {
            Invoke("ShowGameOver", respawnDelay);
        }
    }

    void ShowGameOver()
    {
        GameManager.instance.GameOver();
    }

    void Respawn()
    {
        transform.position = respawnPoint;
        rb.linearVelocity = Vector2.zero;
        health = maxHealth;
        isDead = false;

        movement.canMove = true;
        sword.enabled = true;
        playerAnim.StopAction();

        // a short safe time after coming back
        StartCoroutine(HurtBlink(false));
    }

    // the checkpoint calls this
    public void SetRespawnPoint(Vector3 point)
    {
        respawnPoint = point;
    }

    // healing items or the HUD can use these
    public int GetHealth()
    {
        return health;
    }

    public int GetMaxHealth()
    {
        return maxHealth;
    }

    public int GetLives()
    {
        return lives;
    }

    public bool IsDead()
    {
        return isDead;
    }
}
