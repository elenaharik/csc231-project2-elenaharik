using System.Collections.Generic;
using UnityEngine;

// Makes the forest feel alive: leaves slowly fall across the screen,
// and every few seconds a gust of wind blows them (with faint wind lines).
// Put this on the Main Camera.
public class ForestAmbience : MonoBehaviour
{
    [Header("Leaves")]
    public int maxLeaves = 14;
    public float leavesPerSecond = 1.2f;
    public Color[] leafColors = {
        new Color(0.45f, 0.65f, 0.25f),  // green
        new Color(0.7f, 0.75f, 0.25f),   // yellow green
        new Color(0.85f, 0.55f, 0.2f)    // orange
    };

    [Header("Wind")]
    public float gustEvery = 7f;      // seconds between gusts (about)
    public float gustStrength = 2.5f;
    public float gustLength = 2f;

    private Camera cam;
    private List<SimpleParticle> leaves = new List<SimpleParticle>();
    private float leafTimer;
    private float gustTimer;
    private float gustTimeLeft;

    void Start()
    {
        cam = GetComponent<Camera>();
        gustTimer = gustEvery;
        Effects.wind = 0;
    }

    void Update()
    {
        // forget leaves that are gone
        leaves.RemoveAll(leaf => leaf == null);

        // ----- falling leaves -----
        leafTimer -= Time.deltaTime;
        if (leafTimer <= 0 && leaves.Count < maxLeaves)
        {
            leafTimer = 1f / leavesPerSecond;
            SpawnLeaf();
        }

        // ----- wind gusts -----
        gustTimer -= Time.deltaTime;
        if (gustTimer <= 0)
        {
            gustTimer = gustEvery * Random.Range(0.7f, 1.3f);
            gustTimeLeft = gustLength;
            for (int i = 0; i < 3; i++)
            {
                SpawnWindLine();
            }
        }

        float targetWind = 0.3f; // a light breeze all the time
        if (gustTimeLeft > 0)
        {
            gustTimeLeft -= Time.deltaTime;
            targetWind = gustStrength;
        }
        Effects.wind = Mathf.Lerp(Effects.wind, targetWind, Time.deltaTime * 2f);
    }

    void SpawnLeaf()
    {
        float halfHeight = cam.orthographicSize;
        float halfWidth = halfHeight * cam.aspect;

        // start a little above the top of the screen, a bit more to the left (the wind blows right)
        float x = cam.transform.position.x + Random.Range(-halfWidth * 1.3f, halfWidth);
        float y = cam.transform.position.y + halfHeight + 0.5f;

        Color color = leafColors[Random.Range(0, leafColors.Length)];
        Vector2 velocity = new Vector2(Random.Range(-0.2f, 0.2f), Random.Range(-0.5f, -0.9f));

        SimpleParticle leaf = Effects.Spawn(Effects.LeafSprite, new Vector2(x, y), velocity, color, 3);
        leaf.life = 14f;
        leaf.startSize = Random.Range(0.8f, 1.3f);
        leaf.endSize = leaf.startSize;
        leaf.spin = Random.Range(-60f, 60f);
        leaf.sway = Random.Range(0.3f, 0.8f);
        leaf.flutter = Random.Range(2f, 4f); // flips over as it falls
        leaf.useWind = true;
        leaf.fadeSpeed = 6f; // stays visible, fades only at the end
        leaves.Add(leaf);
    }

    void SpawnWindLine()
    {
        float halfHeight = cam.orthographicSize;
        float halfWidth = halfHeight * cam.aspect;

        float x = cam.transform.position.x - halfWidth + Random.Range(0f, halfWidth);
        float y = cam.transform.position.y + Random.Range(-halfHeight * 0.6f, halfHeight * 0.8f);

        SimpleParticle line = Effects.Spawn(Effects.LineSprite, new Vector2(x, y), new Vector2(Random.Range(5f, 7f), 0), new Color(1f, 1f, 1f, 0.3f), 3);
        line.life = Random.Range(1.2f, 1.6f);
        line.startSize = 1f;              // stays 1 pixel thin
        line.endSize = 1f;
        line.stretch = Random.Range(2f, 3f); // only gets longer, not thicker
        line.wave = 0.3f;                 // gentle up and down wave
        line.fadeIn = true;               // appears softly and disappears softly
        line.fadeSpeed = 2f;
    }
}
