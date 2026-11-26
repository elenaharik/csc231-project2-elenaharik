using UnityEngine;

// One small effect piece (a dust puff, a leaf, a sparkle).
// It moves, grows or shrinks, fades out, and removes itself.
// Effects.cs creates these, you don't add this script by hand.
public class SimpleParticle : MonoBehaviour
{
    public Vector2 velocity;
    public float gravity;          // positive = falls, negative = floats up
    public float drag;             // slows it down over time
    public float life = 0.5f;      // seconds before it disappears
    public float startSize = 1f;
    public float endSize = 1f;
    public float spin;             // degrees per second
    public float sway;             // side to side movement (leaves)
    public bool useWind;           // pushed by the forest wind
    public float fadeSpeed = 1f;   // 1 = fades the whole time, 4 = only fades at the very end
    public bool fadeIn;            // also fades in at the start (wind streaks)
    public float flutter;          // leaves: how fast it flips over while falling (0 = no flipping)
    public float stretch = 1f;     // makes it longer than it is tall (wind streaks)
    public float wave;             // gentle up and down wave (wind streaks)

    private SpriteRenderer sr;
    private float startAlpha;
    private float age;
    private float swayOffset;

    void Start()
    {
        sr = GetComponent<SpriteRenderer>();
        startAlpha = sr.color.a;
        swayOffset = Random.Range(0f, 10f);
    }

    void Update()
    {
        age += Time.deltaTime;
        if (age >= life)
        {
            Destroy(gameObject);
            return;
        }

        float progress = age / life; // 0 -> 1

        // movement
        velocity.y -= gravity * Time.deltaTime;
        velocity /= 1f + drag * Time.deltaTime;

        Vector2 move = velocity;
        move.x += Mathf.Cos(age * 2.5f + swayOffset) * sway;
        move.y += Mathf.Sin(age * 6f + swayOffset) * wave;
        if (useWind)
        {
            move.x += Effects.wind;
        }
        transform.position += (Vector3)(move * Time.deltaTime);

        // size and spin
        float size = Mathf.Lerp(startSize, endSize, progress);
        float width = size * stretch;
        if (flutter > 0)
        {
            // squash the width from full to thin and back = the leaf looks like it flips over
            width *= Mathf.Cos(age * flutter + swayOffset);
        }
        transform.localScale = new Vector3(width, size, 1f);
        transform.Rotate(0, 0, spin * Time.deltaTime);

        // fade out (and fade in, if turned on)
        float alpha = Mathf.Clamp01((1f - progress) * fadeSpeed);
        if (fadeIn)
        {
            alpha *= Mathf.Clamp01(progress * 4f);
        }
        Color c = sr.color;
        c.a = startAlpha * alpha;
        sr.color = c;
    }
}
