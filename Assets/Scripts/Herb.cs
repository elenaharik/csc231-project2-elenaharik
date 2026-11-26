using System.Collections;
using UnityEngine;
using UnityEngine.Rendering.Universal;

// A herb she can pick up.
// While it waits on the ground, a soft glow behind it pulses brighter and dimmer.
// When she picks it up, it flashes, grows a bit and fades away.
// The collider on this object must have "Is Trigger" ticked.
public class Herb : MonoBehaviour
{
    [Header("Glow")]
    public Color glowColor = new Color(1f, 0.9f, 0.4f); // yellow. Use white for a white glow
    public float glowSize = 1.5f;      // how big the glow circle is
    public float glowMin = 0.05f;      // dimmest
    public float glowMax = 0.35f;      // brightest
    public float glowSpeed = 3f;
    public Light2D glowLight;          // optional: a Light 2D child, it pulses too

    [Header("Pickup")]
    public float pickupTime = 0.2f;    // how long the pickup flash lasts
    public GameObject pickupEffect;    // optional: sparkles (we add these in the polish step)

    private bool collected;
    private float glowOffset;
    private SpriteRenderer sr;
    private SpriteRenderer glow;

    private static Sprite glowSprite;  // made once and shared by all herbs

    void Start()
    {
        sr = GetComponent<SpriteRenderer>();

        // each herb starts its glow at a different moment, so they don't all blink together
        glowOffset = Random.Range(0f, 10f);

        MakeGlow();
    }

    // makes a soft circle behind the herb
    void MakeGlow()
    {
        if (glowSprite == null)
        {
            glowSprite = CreateGlowSprite();
        }

        GameObject glowObject = new GameObject("Glow");
        glowObject.transform.SetParent(transform, false);

        glow = glowObject.AddComponent<SpriteRenderer>();
        glow.sprite = glowSprite;
        // drawn just in front of the herb, so nothing (ground, trees, background) can hide it
        glow.sortingLayerID = sr.sortingLayerID;
        glow.sortingOrder = sr.sortingOrder + 1;

        // keep the same size in the world even if the herb was scaled
        float scale = glowSize / Mathf.Max(0.01f, Mathf.Abs(transform.lossyScale.x));
        glowObject.transform.localScale = new Vector3(scale, scale, 1f);
    }

    // a 64x64 white circle that fades out to the edges
    static Sprite CreateGlowSprite()
    {
        int size = 64;
        Texture2D texture = new Texture2D(size, size, TextureFormat.RGBA32, false);
        float center = (size - 1) / 2f;

        for (int y = 0; y < size; y++)
        {
            for (int x = 0; x < size; x++)
            {
                float distance = Vector2.Distance(new Vector2(x, y), new Vector2(center, center)) / center; // 0 middle, 1 edge
                float alpha = Mathf.Clamp01(1f - distance);
                alpha = alpha * alpha; // softer edge
                texture.SetPixel(x, y, new Color(1f, 1f, 1f, alpha));
            }
        }
        texture.Apply();

        // pixels per unit = size, so the sprite is 1 unit wide
        return Sprite.Create(texture, new Rect(0, 0, size, size), new Vector2(0.5f, 0.5f), size);
    }

    void Update()
    {
        if (collected)
        {
            return;
        }

        // Sin goes from -1 to 1, this turns it into 0 to 1
        float t = (Mathf.Sin(Time.time * glowSpeed + glowOffset) + 1f) / 2f;

        Color c = glowColor;
        c.a = Mathf.Lerp(glowMin, glowMax, t);
        glow.color = c;

        if (glowLight != null)
        {
            glowLight.intensity = Mathf.Lerp(0.3f, 1.2f, t);
        }
    }

    void OnTriggerEnter2D(Collider2D other)
    {
        if (collected || !other.CompareTag("Player"))
        {
            return;
        }

        collected = true;
        AudioManager.Play(Sound.Collect);

        if (GameManager.instance == null)
        {
            Debug.LogError("No GameManager in the scene! Add the GameManager script to the GameManager object.");
        }
        else
        {
            GameManager.instance.AddHerb();
        }

        if (pickupEffect != null)
        {
            Instantiate(pickupEffect, transform.position, Quaternion.identity);
        }

        StartCoroutine(PickupFlash());
    }

    // bright flash, grow a little and fade out, then disappear
    IEnumerator PickupFlash()
    {
        GetComponent<Collider2D>().enabled = false;

        Vector3 startScale = transform.localScale;
        float time = 0;

        while (time < pickupTime)
        {
            time += Time.deltaTime;
            float progress = time / pickupTime; // 0 -> 1

            transform.localScale = startScale * (1f + progress * 0.6f);
            sr.color = new Color(1f, 1f, 1f, 1f - progress);

            // the glow flashes bright, then fades
            Color c = glowColor;
            c.a = 1f - progress;
            glow.color = c;

            if (glowLight != null)
            {
                glowLight.intensity = 3f * (1f - progress);
            }

            yield return null;
        }

        Destroy(gameObject);
    }
}
