using TMPro;
using UnityEngine;
using UnityEngine.UI;

// The HUD shown during the game: health bar, herb counter and lives.
// Put this on the Canvas.
public class HUD : MonoBehaviour
{
    public PlayerHealth player;
    public Image healthFill;    // the red bar. Image Type must be "Filled", Fill Method "Horizontal"
    public TMP_Text herbText;   // e.g. "3 / 10"
    public TMP_Text livesText;  // e.g. "x3"

    void Update()
    {
        if (player != null && healthFill != null)
        {
            // fillAmount goes from 0 (empty) to 1 (full)
            healthFill.fillAmount = (float)player.GetHealth() / player.GetMaxHealth();
        }

        if (herbText != null && GameManager.instance != null)
        {
            herbText.text = "Herbs " + GameManager.instance.GetHerbs() + " / " + GameManager.instance.GetTotalHerbs();
        }

        if (livesText != null && player != null)
        {
            livesText.text = "x" + player.GetLives();
        }
    }
}
