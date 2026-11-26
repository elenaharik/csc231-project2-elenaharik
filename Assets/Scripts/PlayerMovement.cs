using UnityEngine;
using UnityEngine.InputSystem;

// Moves the player left and right, and lets her jump.
// Signature move: double jump (she can jump one more time in the air).
public class PlayerMovement : MonoBehaviour
{
    [Header("Movement")]
    public float moveSpeed = 6f;
    public float jumpForce = 12f;
    public int extraJumps = 1; // 1 = double jump

    [Header("Gravity")]
    public float normalGravity = 3f;
    public float fallGravity = 5f; // falls faster than she rises, feels less floaty

    [Header("Ground Check")]
    public Transform groundCheck;
    public float groundCheckRadius = 0.15f;
    public LayerMask groundLayer;

    [Header("Jump Help")]
    public float coyoteTime = 0.1f;     // can still jump a moment after walking off a ledge
    public float jumpBufferTime = 0.1f; // jump pressed a moment before landing still counts

    private Rigidbody2D rb;
    private float moveInput;
    private bool isGrounded;
    private bool facingRight = true;
    private int extraJumpsLeft;
    private float coyoteCounter;
    private float jumpBufferCounter;

    void Start()
    {
        rb = GetComponent<Rigidbody2D>();
        rb.gravityScale = normalGravity;
    }

    void Update()
    {
        // ----- read keyboard -----
        moveInput = 0;
        if (Keyboard.current.aKey.isPressed || Keyboard.current.leftArrowKey.isPressed)
        {
            moveInput -= 1;
        }
        if (Keyboard.current.dKey.isPressed || Keyboard.current.rightArrowKey.isPressed)
        {
            moveInput += 1;
        }

        bool jumpPressed = Keyboard.current.spaceKey.wasPressedThisFrame
                        || Keyboard.current.wKey.wasPressedThisFrame
                        || Keyboard.current.upArrowKey.wasPressedThisFrame;

        bool jumpReleased = Keyboard.current.spaceKey.wasReleasedThisFrame
                         || Keyboard.current.wKey.wasReleasedThisFrame
                         || Keyboard.current.upArrowKey.wasReleasedThisFrame;

        // ----- ground check -----
        isGrounded = Physics2D.OverlapCircle(groundCheck.position, groundCheckRadius, groundLayer);

        if (isGrounded && rb.linearVelocity.y <= 0.01f)
        {
            coyoteCounter = coyoteTime;
            extraJumpsLeft = extraJumps;
        }
        else
        {
            coyoteCounter -= Time.deltaTime;
        }

        // ----- jump -----
        if (jumpPressed)
        {
            jumpBufferCounter = jumpBufferTime;
        }
        else
        {
            jumpBufferCounter -= Time.deltaTime;
        }

        if (jumpBufferCounter > 0)
        {
            if (coyoteCounter > 0)
            {
                // normal jump from the ground
                Jump();
                coyoteCounter = 0;
            }
            else if (extraJumpsLeft > 0)
            {
                // double jump in the air
                Jump();
                extraJumpsLeft--;
            }
        }

        // let go of jump early = smaller jump
        if (jumpReleased && rb.linearVelocity.y > 0)
        {
            rb.linearVelocity = new Vector2(rb.linearVelocity.x, rb.linearVelocity.y * 0.5f);
        }

        // fall faster than rising
        if (rb.linearVelocity.y < 0)
        {
            rb.gravityScale = fallGravity;
        }
        else
        {
            rb.gravityScale = normalGravity;
        }

        // ----- face the way she is walking -----
        if (moveInput > 0 && !facingRight)
        {
            Flip();
        }
        else if (moveInput < 0 && facingRight)
        {
            Flip();
        }
    }

    void FixedUpdate()
    {
        // physics movement goes in FixedUpdate
        rb.linearVelocity = new Vector2(moveInput * moveSpeed, rb.linearVelocity.y);
    }

    void Jump()
    {
        rb.linearVelocity = new Vector2(rb.linearVelocity.x, jumpForce);
        jumpBufferCounter = 0;
    }

    void Flip()
    {
        facingRight = !facingRight;
        Vector3 scale = transform.localScale;
        scale.x *= -1;
        transform.localScale = scale;
    }

    // other scripts (like the animation script) can read these
    public bool IsGrounded()
    {
        return isGrounded;
    }

    public float GetMoveInput()
    {
        return moveInput;
    }

    // draws the ground check circle in the Scene view so you can see it
    void OnDrawGizmosSelected()
    {
        if (groundCheck != null)
        {
            Gizmos.color = Color.yellow;
            Gizmos.DrawWireSphere(groundCheck.position, groundCheckRadius);
        }
    }
}
