using TMPro;
using UnityEngine;

// A floating hint in the level, like "SPACE to jump".
// It fades in when the player comes close and fades out when she walks away.
// Make an empty object, put this script on it, and type the message.
public class TutorialSign : MonoBehaviour
{
    [TextArea] public string message = "SPACE to jump";
    public bool alwaysVisible = true;                  // ticked = permanent sign, unticked = only shows when she is close
    public float showDistance = 4f;                    // how close she must be to see it (when not always visible)
    public float textSize = 3.5f;
    public Vector3 offset = new Vector3(0, 1.5f, 0);   // where the text floats

    private TextMeshPro text;
    private SpriteRenderer box;
    private Transform player;
    private float alpha;

    void Start()
    {
        GameObject playerObject = GameObject.FindGameObjectWithTag("Player");
        if (playerObject != null)
        {
            player = playerObject.transform;
        }

        text = MakeWorldText(transform, offset, message, textSize);
        box = MakeBox(text);
        SetAlpha(0);
    }

    void Update()
    {
        float target = 0;
        if (alwaysVisible)
        {
            target = 1;
        }
        else if (player != null && Vector2.Distance(player.position, transform.position) < showDistance)
        {
            target = 1;
        }

        alpha = Mathf.MoveTowards(alpha, target, Time.deltaTime * 3f);
        SetAlpha(alpha);
    }

    void SetAlpha(float a)
    {
        text.alpha = a;
        box.color = new Color(0f, 0f, 0f, 0.55f * a);
    }

    // ---------- helpers (the portal uses these too) ----------

    // text that lives in the level (not on the screen UI)
    public static TextMeshPro MakeWorldText(Transform parent, Vector3 localPosition, string message, float size)
    {
        GameObject go = new GameObject("HintText");
        go.transform.SetParent(parent, false);
        go.transform.localPosition = localPosition;

        // cancel the parent's scale so the text always has the same size
        Vector3 parentScale = parent.lossyScale;
        go.transform.localScale = new Vector3(1f / Mathf.Abs(parentScale.x), 1f / Mathf.Abs(parentScale.y), 1f);

        TextMeshPro t = go.AddComponent<TextMeshPro>();
        t.text = message;
        t.fontSize = size;
        t.alignment = TextAlignmentOptions.Center;
        t.color = new Color(1f, 0.95f, 0.82f);
        t.rectTransform.sizeDelta = new Vector2(8f, 2f);
        t.sortingOrder = 20; // in front of everything in the level
        return t;
    }

    // dark see-through box behind the text so it is easy to read
    public static SpriteRenderer MakeBox(TextMeshPro t)
    {
        GameObject go = new GameObject("HintBox");
        go.transform.SetParent(t.transform, false);

        SpriteRenderer sr = go.AddComponent<SpriteRenderer>();
        Texture2D white = Texture2D.whiteTexture;
        sr.sprite = Sprite.Create(white, new Rect(0, 0, white.width, white.height), new Vector2(0.5f, 0.5f), white.width); // 1x1 unit square
        sr.color = new Color(0f, 0f, 0f, 0.55f);
        sr.sortingOrder = 19; // just behind the text

        FitBox(t, sr);
        return sr;
    }

    // makes the box the same size as the text (call again when the text changes)
    public static void FitBox(TextMeshPro t, SpriteRenderer box)
    {
        t.ForceMeshUpdate();
        Bounds bounds = t.textBounds;
        box.transform.localPosition = bounds.center;
        box.transform.localScale = new Vector3(bounds.size.x + 0.4f, bounds.size.y + 0.25f, 1f);
    }

    // shows where the sign is in the Scene view
    void OnDrawGizmos()
    {
        Gizmos.color = Color.cyan;
        Gizmos.DrawWireSphere(transform.position + offset, 0.3f);
    }

    void OnDrawGizmosSelected()
    {
        Gizmos.color = new Color(0f, 1f, 1f, 0.3f);
        Gizmos.DrawWireSphere(transform.position, showDistance);
    }
}
