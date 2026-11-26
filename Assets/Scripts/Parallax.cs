using UnityEngine;

// Put this on each background layer.
// Far layers move slowly, near layers move faster = feeling of depth.
// The script makes a copy of the picture on the left and on the right,
// and moves them along with the camera, so the background never runs out.
//
// Parallax Effect: 0 = stays glued to the camera (very far away, like the sky)
//                  1 = doesn't move with the camera (as close as the ground)
[DefaultExecutionOrder(100)] // runs after the camera moves, so it doesn't shake
public class Parallax : MonoBehaviour
{
    [Range(0f, 1f)]
    public float parallaxEffect = 0.5f;

    private Transform cam;
    private float startX;
    private float offsetY;   // distance between this layer and the camera, up and down
    private float length;    // width of one copy of the picture

    void Start()
    {
        cam = Camera.main.transform;
        startX = transform.position.x;
        offsetY = transform.position.y - cam.position.y;

        SpriteRenderer sr = GetComponent<SpriteRenderer>();
        sr.drawMode = SpriteDrawMode.Simple; // the copies do the repeating
        length = sr.sprite.bounds.size.x * transform.localScale.x;

        // one copy on the left and one on the right
        MakeCopy(sr, -1);
        MakeCopy(sr, 1);
    }

    void MakeCopy(SpriteRenderer original, int side)
    {
        GameObject copy = new GameObject(name + " copy");
        copy.transform.SetParent(transform);
        copy.transform.localPosition = new Vector3(side * original.sprite.bounds.size.x, 0, 0);
        copy.transform.localScale = Vector3.one;

        SpriteRenderer copySr = copy.AddComponent<SpriteRenderer>();
        copySr.sprite = original.sprite;
        copySr.sortingLayerID = original.sortingLayerID;
        copySr.sortingOrder = original.sortingOrder;
        copySr.color = original.color;
        copySr.sharedMaterial = original.sharedMaterial;
    }

    void LateUpdate()
    {
        // move left/right slower than the camera
        float distance = cam.position.x * (1 - parallaxEffect);
        float movedFromCamera = cam.position.x * parallaxEffect;

        // always stay at the same height on the screen
        float y = cam.position.y + offsetY;

        transform.position = new Vector3(startX + distance, y, transform.position.z);

        // when the camera goes past one picture width, jump the layer so it repeats
        if (movedFromCamera > startX + length)
        {
            startX += length;
        }
        else if (movedFromCamera < startX - length)
        {
            startX -= length;
        }
    }
}
