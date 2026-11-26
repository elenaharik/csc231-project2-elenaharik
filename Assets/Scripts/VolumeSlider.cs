using UnityEngine;
using UnityEngine.UI;

// Put this on a UI Slider (in the pause menu).
// Moving the slider changes the game volume. The volume is saved.
[RequireComponent(typeof(Slider))]
public class VolumeSlider : MonoBehaviour
{
    void Start()
    {
        Slider slider = GetComponent<Slider>();
        slider.minValue = 0f;
        slider.maxValue = 1f;

        // start at the saved volume
        if (AudioManager.instance != null)
        {
            slider.value = AudioManager.instance.GetVolume();
        }

        slider.onValueChanged.AddListener(OnSliderChanged);
    }

    void OnSliderChanged(float value)
    {
        if (AudioManager.instance != null)
        {
            AudioManager.instance.SetVolume(value);
        }
    }
}
