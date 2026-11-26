using UnityEngine;

// Put this on the Main Camera.
// The camera smoothly follows the player, looks a bit ahead of where she is running,
// and can stay inside level limits. It can also shake (used later for effects).
//
// Up and down: the camera does NOT move for normal jumps (that feels shaky).
// It moves when she lands on a higher or lower platform, or falls far.
public class CameraFollow : MonoBehaviour
{
    public Transform target;                 // the player
    public float smoothTime = 0.15f;         // left/right: smaller = follows faster
    public float smoothTimeY = 0.3f;         // up/down: a bit slower, feels calmer
    public float lookAhead = 1.5f;           // looks ahead in the direction she runs

    [Tooltip("How high the camera is above her. Bigger = more ground under her feet... smaller = more sky.")]
    public float groundSpace = 3f;
    [Tooltip("If she goes this high above the platform she stood on, the camera follows her up.")]
    public float followUpHeight = 4f;

    [Header("Level Limits")]
    public bool useLimits = false;
    public float minX = -10f;
    public float maxX = 100f;
    public float minY = -5f;
    public float maxY = 20f;

    [Header("Fit To Background")]
    [Tooltip("Drag the tallest background layer here. The camera zooms so the background fills the screen from top to bottom.")]
    public SpriteRenderer fitToBackground;

    public static CameraFollow instance;     // so other scripts can call Shake()

    private Vector3 velocity;
    private float velocityY;
    private float currentLookAhead;
    private Rigidbody2D targetRb;
    private PlayerMovement targetMovement;
    private float groundY;                   // height of the last platform she stood on
    private bool firstFrame = true;
    private float shakeTimer;
    private float shakeStrength;

    void Awake()
    {
        instance = this;

        // zoom so the background fills the screen,
        // and start centered on the background so the layers line up
        // (done in Awake so the background layers see this camera height)
        if (fitToBackground != null)
        {
            Bounds bg = fitToBackground.bounds;
            GetComponent<Camera>().orthographicSize = bg.size.y / 2f;
            transform.position = new Vector3(transform.position.x, bg.center.y, -10f);
        }
    }

    void Start()
    {
        if (target != null)
        {
            targetRb = target.GetComponent<Rigidbody2D>();
            targetMovement = target.GetComponent<PlayerMovement>();
            groundY = target.position.y;

            // only move left/right here, up/down is set on the first frame
            transform.position = new Vector3(target.position.x, transform.position.y, -10f);
        }
    }

    void LateUpdate()
    {
        if (target == null)
        {
            return;
        }

        // ----- remember the height of the platform she is on -----
        bool grounded = targetMovement != null && targetMovement.IsGrounded();
        if (grounded)
        {
            groundY = target.position.y;
        }
        else if (target.position.y < groundY)
        {
            groundY = target.position.y;                     // falling down: follow her
        }
        else if (target.position.y > groundY + followUpHeight)
        {
            groundY = target.position.y - followUpHeight;    // very high: follow her up
        }

        // ----- look ahead in the direction she is moving -----
        float wantedLookAhead = 0;
        if (targetRb != null && Mathf.Abs(targetRb.linearVelocity.x) > 0.1f)
        {
            wantedLookAhead = Mathf.Sign(targetRb.linearVelocity.x) * lookAhead;
        }
        currentLookAhead = Mathf.Lerp(currentLookAhead, wantedLookAhead, 2f * Time.deltaTime);

        // ----- where the camera wants to be -----
        float wantedX = target.position.x + currentLookAhead;
        float wantedY = groundY + groundSpace;

        Vector3 newPosition = transform.position;
        if (firstFrame)
        {
            // jump straight there at the start, no sliding
            newPosition = new Vector3(wantedX, wantedY, -10f);
            firstFrame = false;
        }
        else
        {
            newPosition.x = Mathf.SmoothDamp(transform.position.x, wantedX, ref velocity.x, smoothTime);
            newPosition.y = Mathf.SmoothDamp(transform.position.y, wantedY, ref velocityY, smoothTimeY);
        }

        // stay inside the level
        if (useLimits)
        {
            newPosition.x = Mathf.Clamp(newPosition.x, minX, maxX);
            newPosition.y = Mathf.Clamp(newPosition.y, minY, maxY);
        }

        transform.position = newPosition;

        // screen shake: move the camera a tiny random amount
        if (shakeTimer > 0)
        {
            shakeTimer -= Time.unscaledDeltaTime;
            Vector2 shake = Random.insideUnitCircle * shakeStrength;
            transform.position += new Vector3(shake.x, shake.y, 0);
        }
    }

    // call with CameraFollow.instance.Shake(0.15f, 0.2f);
    public void Shake(float duration, float strength)
    {
        shakeTimer = duration;
        shakeStrength = strength;
    }

    // draws the level limits in the Scene view
    void OnDrawGizmosSelected()
    {
        if (useLimits)
        {
            Gizmos.color = Color.cyan;
            Vector3 center = new Vector3((minX + maxX) / 2, (minY + maxY) / 2, 0);
            Vector3 size = new Vector3(maxX - minX, maxY - minY, 0);
            Gizmos.DrawWireCube(center, size);
        }
    }
}
