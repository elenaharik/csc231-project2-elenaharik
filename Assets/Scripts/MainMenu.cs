using UnityEngine;
using UnityEngine.SceneManagement;

// Title screen. The Play button calls PlayGame(), the Quit button calls QuitGame().
// Put this on the Canvas of the MainMenu scene.
public class MainMenu : MonoBehaviour
{
    public string levelScene = "SampleScene"; // the name of your level scene

    void Start()
    {
        Time.timeScale = 1f;
    }

    public void PlayGame()
    {
        SceneManager.LoadScene(levelScene);
    }

    public void QuitGame()
    {
        Application.Quit();
    }
}
