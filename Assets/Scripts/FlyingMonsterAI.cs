using UnityEngine;

// Flying monster:
// - flies left and right around where you place it, bobbing up and down
// - when the player walks under it, it winds up and SMASHES straight down
// - after landing it is stuck for a moment (good time to hit it!), then flies back up
// Needs: Rigidbody2D (Kinematic), Animator (Fly, SmashStart, SmashLoop, SmashEnd, Hit, Die), EnemyHealth
public class FlyingMonsterAI : MonoBehaviour
{
    [Header("Flying")]
    public float flySpeed = 2f;
    public float patrolDistance = 3f;   // flies this far left and right of its start
    public float bobHeight = 0.25f;
    public float bobSpeed = 3f;
    [Tooltip("Tick this if the monster picture faces right.")]
    public bool artFacesRight = false;

    [Header("Smash Attack")]
    public float detectWidth = 1f;      // how close (left/right) she must be to trigger the smash
    public float detectDepth = 8f;      // how far below it can see her
    public float windUpTime = 0.6f;     // warning before the smash, so the player can dodge
    public float smashSpeed = 12f;
    public float landTime = 0.8f;       // stuck on the ground for this long
    public float returnSpeed = 3f;
    public float cooldown = 1.5f;       // time between smashes
    public LayerMask groundLayer;
    public float groundDistance = 0.6f; // distance from its center to its bottom

    private enum State { Patrol, WindUp, Smash, Land, Return }
    private State state = State.Patrol;

    private Vector2 startPosition;
    private int direction = 1;
    private float timer;
    private float cooldownTimer;
    private string currentState;

    private Rigidbody2D rb;
    private Animator anim;
    private EnemyHealth health;
    private Transform player;
    private PlayerHealth playerHealth;

    void Start()
    {
        rb = GetComponent<Rigidbody2D>();
        anim = GetComponent<Animator>();
        health = GetComponent<EnemyHealth>();
        startPosition = transform.position;

        GameObject playerObject = GameObject.FindGameObjectWithTag("Player");
        if (playerObject != null)
        {
            player = playerObject.transform;
            playerHealth = playerObject.GetComponent<PlayerHealth>();
        }

        UpdateFacing();
    }

    void FixedUpdate()
    {
        if (health.IsDead() || health.IsHurt())
        {
            currentState = ""; // so the right animation plays again after the Hit
            return;
        }

        float dt = Time.fixedDeltaTime;
        Vector2 position = rb.position;

        switch (state)
        {
            case State.Patrol:
                // fly left and right
                position.x += direction * flySpeed * dt;
                if (position.x > startPosition.x + patrolDistance)
                {
                    direction = -1;
                    UpdateFacing();
                }
                else if (position.x < startPosition.x - patrolDistance)
                {
                    direction = 1;
                    UpdateFacing();
                }

                // bob up and down
                position.y = startPosition.y + Mathf.Sin(Time.time * bobSpeed) * bobHeight;

                ChangeState("Fly");

                cooldownTimer -= dt;
                if (cooldownTimer <= 0 && PlayerIsBelow())
                {
                    state = State.WindUp;
                    timer = 0;
                    ChangeState("SmashStart");
                }
                break;

            case State.WindUp:
                // stay still and get ready
                timer += dt;
                if (timer >= windUpTime)
                {
                    state = State.Smash;
                    ChangeState("SmashLoop");
                }
                break;

            case State.Smash:
                // fall fast until it hits the ground
                position.y -= smashSpeed * dt;

                bool hitGround = Physics2D.Raycast(position, Vector2.down, groundDistance, groundLayer);
                bool fellTooFar = position.y < startPosition.y - detectDepth - 2f;

                if (hitGround || fellTooFar)
                {
                    state = State.Land;
                    timer = 0;
                    ChangeState("SmashEnd");
                }
                break;

            case State.Land:
                // stuck for a moment
                timer += dt;
                if (timer >= landTime)
                {
                    state = State.Return;
                    ChangeState("Fly");
                }
                break;

            case State.Return:
                // fly back up to its normal height
                position.y += returnSpeed * dt;
                if (position.y >= startPosition.y)
                {
                    position.y = startPosition.y;
                    state = State.Patrol;
                    cooldownTimer = cooldown;
                }
                break;
        }

        rb.MovePosition(position);
    }

    bool PlayerIsBelow()
    {
        if (player == null || playerHealth.IsDead())
        {
            return false;
        }

        float distanceX = Mathf.Abs(player.position.x - transform.position.x);
        float distanceBelow = transform.position.y - player.position.y;
        return distanceX < detectWidth && distanceBelow > 0 && distanceBelow < detectDepth;
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
            anim.Play(newState, 0, 0f);
        }
        currentState = newState;
    }

    // Scene view: green = where it patrols, red = where it can see the player below
    void OnDrawGizmosSelected()
    {
        Vector3 center = Application.isPlaying ? (Vector3)startPosition : transform.position;

        Gizmos.color = Color.green;
        Gizmos.DrawLine(center + Vector3.left * patrolDistance, center + Vector3.right * patrolDistance);

        Gizmos.color = Color.red;
        Vector3 boxCenter = transform.position + Vector3.down * detectDepth / 2;
        Gizmos.DrawWireCube(boxCenter, new Vector3(detectWidth * 2, detectDepth, 0));
    }
}
