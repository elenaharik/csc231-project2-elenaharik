using TMPro;
using UnityEngine;

// The portal at the end of the level.
// When she walks into it, the level is complete.
// It can need a number of herbs first: a message above the portal tells the player.
// The collider on this object must have "Is Trigger" ticked.
public class Goal : MonoBehaviour
{
    public int herbsNeeded = 0;                         // 0 = she can finish without herbs
    public Vector3 messageOffset = new Vector3(0, 2.2f, 0); // where the message floats above the portal

    private bool reached;
    private TextMeshPro message;
    private SpriteRenderer box;
    private string lastText;
    private float warningTimer; // the message turns red for a moment when she isn't ready

    void Start()
    {
        if (herbsNeeded > 0)
        {
            message = TutorialSign.MakeWorldText(transform, messageOffset, "", 3.5f);
            box = TutorialSign.MakeBox(message);
        }
    }

    void Update()
    {
        if (message == null || GameManager.instance == null)
        {
            return;
        }

        int herbs = GameManager.instance.GetHerbs();
        string newText;

        if (herbs < herbsNeeded)
        {
            newText = "Find all " + herbsNeeded + " herbs!  (" + herbs + " / " + herbsNeeded + ")";
        }
        else
        {
            newText = "The portal is open!";
        }

        // only rebuild the box when the words change
        if (newText != lastText)
        {
            message.text = newText;
            TutorialSign.FitBox(message, box);
            lastText = newText;
        }

        // color: red warning, green when open, cream otherwise
        if (warningTimer > 0)
        {
            warningTimer -= Time.deltaTime;
            message.color = new Color(1f, 0.4f, 0.35f);
        }
        else if (herbs >= herbsNeeded)
        {
            message.color = new Color(0.6f, 1f, 0.6f);
        }
        else
        {
            message.color = new Color(1f, 0.95f, 0.82f);
        }
    }

    void OnTriggerEnter2D(Collider2D other)
    {
        if (reached || !other.CompareTag("Player"))
        {
            return;
        }

        if (GameManager.instance.GetHerbs() < herbsNeeded)
        {
            warningTimer = 1f; // not enough herbs: flash the message red
            return;
        }

        reached = true;
        GameManager.instance.LevelComplete();
    }
}
