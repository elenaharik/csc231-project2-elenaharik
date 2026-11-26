using UnityEngine;

// Campfire checkpoint. It starts dark and still.
// When she touches it, the fire lights up and she will respawn here.
// The collider on this object must have "Is Trigger" ticked.
public class Checkpoint : MonoBehaviour
{
    public Color unlitColor = new Color(0.4f, 0.4f, 0.4f); // dark before it is lit
    public Vector3 respawnOffset = new Vector3(0, 1f, 0);   // so she appears above the ground, not inside it

    private bool isLit;
    private SpriteRenderer sr;
    private Animator anim;

    void Start()
    {
        sr = GetComponent<SpriteRenderer>();
        anim = GetComponent<Animator>();

        // not lit yet: dark and not moving
        sr.color = unlitColor;
        if (anim != null)
        {
            anim.enabled = false;
        }
    }

    void OnTriggerEnter2D(Collider2D other)
    {
        if (isLit || !other.CompareTag("Player"))
        {
            return;
        }

        isLit = true;
        AudioManager.Play(Sound.Checkpoint);
        other.GetComponent<PlayerHealth>().SetRespawnPoint(transform.position + respawnOffset);

        // light the fire
        sr.color = Color.white;
        if (anim != null)
        {
            anim.enabled = true;
        }

    }
}
