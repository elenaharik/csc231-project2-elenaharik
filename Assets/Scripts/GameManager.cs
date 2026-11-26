using TMPro;
using UnityEngine;
using UnityEngine.SceneManagement;

// Keeps track of the game: herbs collected, winning and losing.
// There is only one GameManager, and any script can reach it with GameManager.instance
// The buttons on the win and game over screens call RestartLevel() and LoadMainMenu().
public class GameManager : MonoBehaviour
{
    public static GameManager instance;

    [Header("Screens")]
    public GameObject winScreen;       // "Level Complete" panel
    public GameObject gameOverScreen;  // "Game Over" panel
    public TMP_Text winHerbsText;      // text on the win screen, e.g. "Herbs: 8 / 10"

    [Header("Scenes")]
    public string mainMenuScene = "MainMenu";

    private int herbsCollected;
    private int totalHerbs;
    private bool levelDone;

    void Awake()
    {
        instance = this;
    }

    void Start()
    {
        Time.timeScale = 1f; // make sure the game is running (after a restart)

        if (winScreen != null)
        {
            winScreen.SetActive(false);
        }
        if (gameOverScreen != null)
        {
            gameOverScreen.SetActive(false);
        }

        // count every herb placed in the level
        totalHerbs = FindObjectsByType<Herb>(FindObjectsSortMode.None).Length;
    }

    public void AddHerb()
    {
        herbsCollected++;
    }

    // the portal calls this
    public void LevelComplete()
    {
        if (levelDone)
        {
            return;
        }
        levelDone = true;

        if (winHerbsText != null)
        {
            winHerbsText.text = "Herbs: " + herbsCollected + " / " + totalHerbs;
        }
        if (winScreen != null)
        {
            winScreen.SetActive(true);
        }

        Time.timeScale = 0f; // freeze the game behind the win screen
    }

    // PlayerHealth calls this when she has no lives left
    public void GameOver()
    {
        if (levelDone)
        {
            return;
        }
        levelDone = true;

        if (gameOverScreen != null)
        {
            gameOverScreen.SetActive(true);
        }

        Time.timeScale = 0f;
    }

    // ----- buttons -----

    public void RestartLevel()
    {
        Time.timeScale = 1f;
        SceneManager.LoadScene(SceneManager.GetActiveScene().name);
    }

    public void LoadMainMenu()
    {
        Time.timeScale = 1f;
        SceneManager.LoadScene(mainMenuScene);
    }

    public void QuitGame()
    {
        Application.Quit();
    }

    // ----- other scripts read these -----

    public bool IsLevelDone()
    {
        return levelDone;
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
