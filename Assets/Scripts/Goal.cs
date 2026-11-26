using UnityEngine;

// The portal at the end of the level.
// When she walks into it, the level is complete.
// You can make it need a number of herbs first.
// The collider on this object must have "Is Trigger" ticked.
public class Goal : MonoBehaviour
{
    public int herbsNeeded = 0; // 0 = she can finish without herbs

    private bool reached;

    void OnTriggerEnter2D(Collider2D other)
    {
        if (reached || !other.CompareTag("Player"))
        {
            return;
        }

        if (GameManager.instance.GetHerbs() < herbsNeeded)
        {
            Debug.Log("You need " + herbsNeeded + " herbs to go through the portal!");
            return;
        }

        reached = true;
        GameManager.instance.LevelComplete();
    }
}
