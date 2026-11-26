using UnityEngine;

// Small visual effects that any script can make with one line:
//   Effects.Dust(position, 6);     a puff of dust
//   Effects.Glint(position);       a sword sparkle
// The tiny pixel-art pictures are drawn here in code, so nothing needs to be set up in Unity.
public static class Effects
{
    public static float wind; // ForestAmbience changes this, leaves use it

    // sorting order: drawn in front of the ground and characters
    const int EffectsOrder = 5;

    static readonly Color dustColor = new Color(0.85f, 0.78f, 0.65f, 0.8f);
    static readonly Color glintColor = new Color(1f, 0.97f, 0.8f, 1f);

    static Sprite dotSprite;
    static Sprite starSprite;
    static Sprite leafSprite;
    static Sprite lineSprite;

    // ---------------------------------------------------------------
    // effects
    // ---------------------------------------------------------------

    // puffs of dust that spread sideways and float up a little
    public static void Dust(Vector2 position, int count, float spread = 0.3f, float speed = 1f, float size = 1f)
    {
        for (int i = 0; i < count; i++)
        {
            float side = Random.value < 0.5f ? -1f : 1f;
            Vector2 velocity = new Vector2(side * Random.Range(0.4f, 1f) * speed, Random.Range(0.2f, 0.7f) * speed);
            Vector2 start = position + new Vector2(Random.Range(-spread, spread), 0.05f);

            SimpleParticle p = Spawn(DotSprite, start, velocity, dustColor, EffectsOrder);
            p.life = Random.Range(0.3f, 0.5f);
            p.startSize = size * Random.Range(0.8f, 1.2f);
            p.endSize = p.startSize * 1.8f;
            p.gravity = -0.4f;
            p.drag = 4f;
        }
    }

    // a quick star sparkle with two tiny sparks
    public static void Glint(Vector2 position, float size = 1f)
    {
        SimpleParticle star = Spawn(StarSprite, position, Vector2.zero, glintColor, EffectsOrder + 1);
        star.life = 0.18f;
        star.startSize = size * 1.2f;
        star.endSize = size * 0.2f;
        star.spin = 300f;
        star.fadeSpeed = 3f;

        for (int i = 0; i < 2; i++)
        {
            Vector2 velocity = Random.insideUnitCircle.normalized * 2.5f;
            SimpleParticle spark = Spawn(DotSprite, position, velocity, glintColor, EffectsOrder + 1);
            spark.life = 0.2f;
            spark.startSize = size * 0.5f;
            spark.endSize = 0.1f;
            spark.drag = 6f;
        }
    }

    // ---------------------------------------------------------------
    // helpers
    // ---------------------------------------------------------------

    public static SimpleParticle Spawn(Sprite sprite, Vector2 position, Vector2 velocity, Color color, int order)
    {
        GameObject go = new GameObject("Effect");
        go.transform.position = position;
        go.transform.localScale = Vector3.zero; // hidden until it starts moving next frame

        SpriteRenderer sr = go.AddComponent<SpriteRenderer>();
        sr.sprite = sprite;
        sr.color = color;
        sr.sortingOrder = order;

        SimpleParticle p = go.AddComponent<SimpleParticle>();
        p.velocity = velocity;
        return p;
    }

    // tiny pixel pictures: '#' = solid, '+' = half see-through, '.' = empty
    public static Sprite DotSprite
    {
        get
        {
            if (dotSprite == null)
            {
                dotSprite = MakeSprite(new string[] {
                    ".##.",
                    "####",
                    "####",
                    ".##." });
            }
            return dotSprite;
        }
    }

    public static Sprite StarSprite
    {
        get
        {
            if (starSprite == null)
            {
                starSprite = MakeSprite(new string[] {
                    "...#...",
                    "...#...",
                    "..+#+..",
                    "#######",
                    "..+#+..",
                    "...#...",
                    "...#..." });
            }
            return starSprite;
        }
    }

    // a leaf: pointed tip on the right, little stem on the left,
    // darker middle vein (d) and a slightly darker bottom half (m)
    public static Sprite LeafSprite
    {
        get
        {
            if (leafSprite == null)
            {
                leafSprite = MakeSprite(new string[] {
                    "....####....",
                    "..########..",
                    ".##########.",
                    "sdddddddddd#",
                    ".mmmmmmmmmm.",
                    "..mmmmmmmm..",
                    "....mmmm...." }, 32);
            }
            return leafSprite;
        }
    }

    // a thin wind streak that is faint at both ends
    public static Sprite LineSprite
    {
        get
        {
            if (lineSprite == null)
            {
                lineSprite = MakeSprite(new string[] {
                    "..++++++############++++++....++++######++++.." }, 32);
            }
            return lineSprite;
        }
    }

    // turns the text picture into a sprite
    //   '#' = solid   '+' = half see-through   '.' = empty
    //   'm' = a bit darker   'd' = darker (vein)   's' = darkest (stem)
    // the color of the sprite renderer tints it (green leaf, white wind...)
    static Sprite MakeSprite(string[] rows, int pixelsPerUnit = 16)
    {
        int height = rows.Length;
        int width = rows[0].Length;
        Texture2D texture = new Texture2D(width, height, TextureFormat.RGBA32, false);
        texture.filterMode = FilterMode.Point;

        for (int y = 0; y < height; y++)
        {
            string row = rows[height - 1 - y]; // textures start at the bottom
            for (int x = 0; x < width; x++)
            {
                Color pixel = new Color(1f, 1f, 1f, 0f);
                switch (row[x])
                {
                    case '#': pixel = new Color(1f, 1f, 1f, 1f); break;
                    case '+': pixel = new Color(1f, 1f, 1f, 0.5f); break;
                    case 'm': pixel = new Color(0.82f, 0.82f, 0.82f, 1f); break;
                    case 'd': pixel = new Color(0.6f, 0.6f, 0.6f, 1f); break;
                    case 's': pixel = new Color(0.45f, 0.4f, 0.35f, 1f); break;
                }
                texture.SetPixel(x, y, pixel);
            }
        }
        texture.Apply();

        return Sprite.Create(texture, new Rect(0, 0, width, height), new Vector2(0.5f, 0.5f), pixelsPerUnit);
    }
}
