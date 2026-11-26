using UnityEngine;

// A herb she can pick up. Floats up and down so the player notices it.
// The collider on this object must have "Is Trigger" ticked.
public class Herb : MonoBehaviour
{
    public float floatHeight = 0.15f;
    public float floatSpeed = 3f;
    public GameObject pickupEffect; // optional: sparkles (we add these in the polish step)

    private Vector3 startPosition;
    private bool collected;

    void Start()
    {
        startPosition = transform.position;
    }

    void Update()
    {
        // gentle float up and down
        float offset = Mathf.Sin(Time.time * floatSpeed) * floatHeight;
        transform.position = startPosition + new Vector3(0, offset, 0);
    }

    void OnTriggerEnter2D(Collider2D other)
    {
        if (collected || !other.CompareTag("Player"))
        {
            return;
        }

        collected = true;

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

        Destroy(gameObject);
    }
}
