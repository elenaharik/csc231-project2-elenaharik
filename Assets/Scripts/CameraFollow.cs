using UnityEngine;

// Put this on the Main Camera.
// The camera smoothly follows the player, looks a bit ahead of where she is running,
// and can stay inside level limits. It can also shake (used later for effects).
public class CameraFollow : MonoBehaviour
{
    public Transform target;                 // the player
    public Vector3 offset = new Vector3(0, 1f, -10f);
    public float smoothTime = 0.15f;         // smaller = follows faster
    public float lookAhead = 1.5f;           // looks ahead in the direction she runs

    [Header("Level Limits")]
    public bool useLimits = false;
    public float minX = -10f;
    public float maxX = 100f;
    public float minY = -5f;
    public float maxY = 20f;

    [Header("Fit To Background")]
    [Tooltip("Drag the tallest background layer here. The camera zooms so the background fills the screen from top to bottom, and stays at that height.")]
    public SpriteRenderer fitToBackground;

    public static CameraFollow instance;     // so other scripts can call Shake()

    private Vector3 velocity;
    private float currentLookAhead;
    private Rigidbody2D targetRb;
    private float shakeTimer;
    private float shakeStrength;
    private bool lockHeight;
    private float lockedY;

    void Awake()
    {
        instance = this;

        // zoom and place the camera to match the background exactly
        // (done in Awake so the background layers see the final camera height)
        if (fitToBackground != null)
        {
            Bounds bg = fitToBackground.bounds;
            GetComponent<Camera>().orthographicSize = bg.size.y / 2f;

            lockHeight = true;
            lockedY = bg.center.y;
            transform.position = new Vector3(transform.position.x, lockedY, transform.position.z);
        }
    }

    void Start()
    {
        if (target != null)
        {
            targetRb = target.GetComponent<Rigidbody2D>();
            Vector3 start = target.position + offset; // start on the player
            if (lockHeight)
            {
                start.y = lockedY;
            }
            transform.position = start;
        }
    }

    void LateUpdate()
    {
        if (target == null)
        {
            return;
        }

        // look ahead in the direction she is moving
        float wantedLookAhead = 0;
        if (targetRb != null && Mathf.Abs(targetRb.linearVelocity.x) > 0.1f)
        {
            wantedLookAhead = Mathf.Sign(targetRb.linearVelocity.x) * lookAhead;
        }
        currentLookAhead = Mathf.Lerp(currentLookAhead, wantedLookAhead, 2f * Time.deltaTime);

        // smooth follow
        Vector3 wantedPosition = target.position + offset + new Vector3(currentLookAhead, 0, 0);
        Vector3 newPosition = Vector3.SmoothDamp(transform.position, wantedPosition, ref velocity, smoothTime);

        // stay inside the level
        if (useLimits)
        {
            newPosition.x = Mathf.Clamp(newPosition.x, minX, maxX);
            newPosition.y = Mathf.Clamp(newPosition.y, minY, maxY);
        }

        // the camera stays at the background's height
        if (lockHeight)
        {
            newPosition.y = lockedY;
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
