using UnityEngine;
using UnityEngine.InputSystem;

// Press Esc (or P) to pause. The buttons call Resume(), Restart() and QuitToMenu().
// Put this on the Canvas and drag the pause panel into "Pause Panel".
public class PauseMenu : MonoBehaviour
{
    public GameObject pausePanel;

    public static bool isPaused;

    void Start()
    {
        isPaused = false;
        pausePanel.SetActive(false);
    }

    void Update()
    {
        // can't pause on the win or game over screen
        if (GameManager.instance != null && GameManager.instance.IsLevelDone())
        {
            return;
        }

        if (Keyboard.current.escapeKey.wasPressedThisFrame || Keyboard.current.pKey.wasPressedThisFrame)
        {
            if (isPaused)
            {
                Resume();
            }
            else
            {
                Pause();
            }
        }
    }

    public void Pause()
    {
        isPaused = true;
        pausePanel.SetActive(true);
        Time.timeScale = 0f; // stops everything that uses time
    }

    public void Resume()
    {
        isPaused = false;
        pausePanel.SetActive(false);
        Time.timeScale = 1f;
    }

    public void Restart()
    {
        isPaused = false;
        GameManager.instance.RestartLevel();
    }

    public void QuitToMenu()
    {
        isPaused = false;
        GameManager.instance.LoadMainMenu();
    }
}
