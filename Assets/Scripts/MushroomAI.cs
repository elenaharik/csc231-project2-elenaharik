using UnityEngine;

// Mushroom enemy:
// - walks back and forth, turns around at edges and walls
// - when the player is close, it walks toward her
// - when she is in reach, it attacks
// Needs: Rigidbody2D, Animator (Idle, Run, Attack, Hit, Die), EnemyHealth
public class MushroomAI : MonoBehaviour
{
    [Header("Walking")]
    public float walkSpeed = 1.5f;
    public float chaseSpeed = 2.5f;
    public LayerMask groundLayer;
    public float edgeCheckDistance = 0.6f; // how far in front it looks for an edge or wall
    public float groundCheckDepth = 1.5f;  // how far down it looks for ground
    [Tooltip("Tick this if the mushroom picture faces right.")]
    public bool artFacesRight = false;

    [Header("Chase and Attack")]
    public float chaseRange = 5f;
    public float attackRange = 1.2f;
    public int damage = 1;
    public float attackDuration = 0.8f; // length of the Attack animation
    public float hitDelay = 0.5f;       // when in the animation the hit happens
    public float attackCooldown = 1f;

    private Transform player;
    private PlayerHealth playerHealth;
    private Rigidbody2D rb;
    private Animator anim;
    private EnemyHealth health;

    private int direction = 1; // 1 = right, -1 = left
    private float currentSpeed;
    private string currentState;
    private bool isAttacking;
    private bool hasHit;
    private float attackTimer;
    private float cooldownTimer;
    private float dustTimer;
    private Collider2D body;

    void Start()
    {
        rb = GetComponent<Rigidbody2D>();
        body = GetComponent<Collider2D>();
        anim = GetComponent<Animator>();
        health = GetComponent<EnemyHealth>();

        GameObject playerObject = GameObject.FindGameObjectWithTag("Player");
        if (playerObject != null)
        {
            player = playerObject.transform;
            playerHealth = playerObject.GetComponent<PlayerHealth>();
        }

        UpdateFacing();
    }

    void Update()
    {
        if (health.IsDead())
        {
            return;
        }

        // when hit: stop everything, EnemyHealth plays the Hit animation
        if (health.IsHurt())
        {
            isAttacking = false;
            currentState = ""; // so Run/Idle plays again after
            return;
        }

        cooldownTimer -= Time.deltaTime;

        // ----- attacking -----
        if (isAttacking)
        {
            currentSpeed = 0;
            attackTimer += Time.deltaTime;

            if (!hasHit && attackTimer >= hitDelay)
            {
                hasHit = true;
                TryHitPlayer();
            }

            if (attackTimer >= attackDuration)
            {
                isAttacking = false;
                cooldownTimer = attackCooldown;
            }
            return;
        }

        // ----- chasing -----
        if (CanSeePlayer())
        {
            // turn to face her
            if (player.position.x > transform.position.x)
            {
                direction = 1;
            }
            else
            {
                direction = -1;
            }
            UpdateFacing();

            float distance = Mathf.Abs(player.position.x - transform.position.x);

            if (distance <= attackRange)
            {
                currentSpeed = 0;
                if (cooldownTimer <= 0)
                {
                    StartAttack();
                    return;
                }
            }
            else if (GroundAhead())
            {
                currentSpeed = chaseSpeed;
            }
            else
            {
                currentSpeed = 0; // don't walk off the edge to follow her
            }
        }
        // ----- patrolling -----
        else
        {
            if (!GroundAhead() || WallAhead())
            {
                direction = -direction;
                UpdateFacing();
            }
            currentSpeed = walkSpeed;
        }

        // ----- animation -----
        if (currentSpeed > 0)
        {
            ChangeState("Run");

            // little dust behind its feet while walking
            dustTimer -= Time.deltaTime;
            if (dustTimer <= 0 && body != null)
            {
                dustTimer = currentSpeed > walkSpeed ? 0.15f : 0.3f; // more dust when chasing
                Vector2 feet = new Vector2(transform.position.x - direction * 0.3f, body.bounds.min.y);
                Effects.Dust(feet, 1, 0.05f, 0.5f, 0.8f);
            }
        }
        else
        {
            ChangeState("Idle");
        }
    }

    void FixedUpdate()
    {
        // don't move while dead or being knocked back
        if (health.IsDead() || health.IsHurt())
        {
            return;
        }

        rb.linearVelocity = new Vector2(direction * currentSpeed, rb.linearVelocity.y);
    }

    bool CanSeePlayer()
    {
        if (player == null || playerHealth.IsDead())
        {
            return false;
        }

        float distanceX = Mathf.Abs(player.position.x - transform.position.x);
        float distanceY = Mathf.Abs(player.position.y - transform.position.y);
        return distanceX < chaseRange && distanceY < 1.5f;
    }

    void StartAttack()
    {
        isAttacking = true;
        hasHit = false;
        attackTimer = 0;
        currentSpeed = 0;

        anim.Play("Attack", 0, 0f);
        currentState = "Attack";
    }

    void TryHitPlayer()
    {
        if (player == null)
        {
            return;
        }

        float distanceX = player.position.x - transform.position.x;
        float distanceY = Mathf.Abs(player.position.y - transform.position.y);

        // she must still be close and in front of the mushroom
        bool inReach = Mathf.Abs(distanceX) <= attackRange + 0.2f && distanceY < 1.5f;
        bool inFront = distanceX * direction > -0.2f;

        if (inReach && inFront)
        {
            playerHealth.TakeDamage(damage, transform.position);
        }
    }

    bool GroundAhead()
    {
        Vector2 origin = (Vector2)transform.position + new Vector2(direction * edgeCheckDistance, 0);
        return Physics2D.Raycast(origin, Vector2.down, groundCheckDepth, groundLayer);
    }

    bool WallAhead()
    {
        return Physics2D.Raycast(transform.position, new Vector2(direction, 0), edgeCheckDistance, groundLayer);
    }

    void UpdateFacing()
    {
        Vector3 scale = transform.localScale;
        bool lookRight = direction > 0;

        if (lookRight == artFacesRight)
        {
            scale.x = Mathf.Abs(scale.x);
        }
        else
        {
            scale.x = -Mathf.Abs(scale.x);
        }
        transform.localScale = scale;
    }

    void ChangeState(string newState)
    {
        if (newState == currentState)
        {
            return;
        }
        if (anim.HasState(0, Animator.StringToHash(newState)))
        {
            anim.Play(newState);
        }
        currentState = newState;
    }

    // Scene view: yellow = chase range, red = attack range, blue = edge check
    void OnDrawGizmosSelected()
    {
        Gizmos.color = Color.yellow;
        Gizmos.DrawWireSphere(transform.position, chaseRange);
        Gizmos.color = Color.red;
        Gizmos.DrawWireSphere(transform.position, attackRange);

        Gizmos.color = Color.blue;
        Vector3 origin = transform.position + new Vector3(direction * edgeCheckDistance, 0, 0);
        Gizmos.DrawLine(origin, origin + Vector3.down * groundCheckDepth);
    }
}
