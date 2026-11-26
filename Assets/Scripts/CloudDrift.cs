using UnityEngine;

// Makes a cloud slowly drift across the sky.
// When it goes too far, it comes back from the other side.
// Works best when the cloud is a child of the sky layer.
public class CloudDrift : MonoBehaviour
{
    public float speed = 0.3f;          // how fast it drifts (negative = drifts left)
    public float wrapDistance = 20f;    // how far it goes before coming back

    private float startX;

    void Start()
    {
        startX = transform.localPosition.x;

        // small random speed difference so the clouds don't all move together
        speed *= Random.Range(0.7f, 1.3f);
    }

    void Update()
    {
        Vector3 position = transform.localPosition;
        position.x += speed * Time.deltaTime;

        // went too far: come back from the other side
        if (position.x > startX + wrapDistance)
        {
            position.x -= wrapDistance * 2;
        }
        else if (position.x < startX - wrapDistance)
        {
            position.x += wrapDistance * 2;
        }

        transform.localPosition = position;
    }
}
