using UnityEngine;

// Picks which animation the player should play (Idle, Run, Jump, Fall).
// The state names here must match the animation clip names in the Animator.
public class PlayerAnimation : MonoBehaviour
{
    private Animator anim;
    private Rigidbody2D rb;
    private PlayerMovement movement;
    private string currentState;

    void Start()
    {
        anim = GetComponent<Animator>();
        rb = GetComponent<Rigidbody2D>();
        movement = GetComponent<PlayerMovement>();
    }

    void Update()
    {
        string newState;

        if (!movement.IsGrounded())
        {
            // in the air: going up = Jump, going down = Fall
            if (rb.linearVelocity.y > 0.1f)
            {
                newState = "Jump";
            }
            else
            {
                newState = "Fall";
            }
        }
        else if (movement.GetMoveInput() != 0)
        {
            newState = "Run";
        }
        else
        {
            newState = "Idle";
        }

        ChangeState(newState);
    }

    // only switch animation when the state actually changes,
    // otherwise the animation would restart every frame
    public void ChangeState(string newState)
    {
        if (newState == currentState)
        {
            return;
        }

        // tells you in the Console if a state is missing from the Animator
        if (!anim.HasState(0, Animator.StringToHash(newState)))
        {
            Debug.LogWarning("Animator has no state called " + newState);
            return;
        }

        anim.Play(newState);
        currentState = newState;
    }
}
