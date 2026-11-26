using UnityEngine;

// Keeps track of the game: how many herbs she collected.
// There is only one GameManager, and any script can reach it with GameManager.instance
public class GameManager : MonoBehaviour
{
    public static GameManager instance;

    private int herbsCollected;
    private int totalHerbs;

    void Awake()
    {
        instance = this;
    }

    void Start()
    {
        // count every herb placed in the level
        totalHerbs = FindObjectsByType<Herb>(FindObjectsSortMode.None).Length;
    }

    public void AddHerb()
    {
        herbsCollected++;
        Debug.Log("Herbs: " + herbsCollected + " / " + totalHerbs);
    }

    public int GetHerbs()
    {
        return herbsCollected;
    }

    public int GetTotalHerbs()
    {
        return totalHerbs;
    }
}
