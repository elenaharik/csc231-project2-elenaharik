using UnityEngine;
using UnityEngine.InputSystem;

// Plays all sound effects and the music.
// Any script can play a sound with:  AudioManager.Play(Sound.Jump);
// Press M to mute / unmute. The volume is saved between games.
// Put ONE AudioManager in the MainMenu scene and one in the level.
// It stays alive when the scene changes, so the music doesn't restart.
public enum Sound { Jump, Land, Collect, Hurt, Swing, EnemyHit, Win, Checkpoint, Death }

public class AudioManager : MonoBehaviour
{
    public static AudioManager instance;

    [Header("Sound Effects")]
    public AudioClip jump;
    public AudioClip land;
    public AudioClip collect;
    public AudioClip hurt;
    public AudioClip swing;
    public AudioClip enemyHit;
    public AudioClip win;
    public AudioClip checkpoint;
    public AudioClip death;

    [Header("Music")]
    public AudioClip music;
    [Range(0f, 1f)] public float musicVolume = 0.4f; // music is quieter than the effects

    private AudioSource sfxSource;
    private AudioSource musicSource;
    private float masterVolume = 1f;
    private bool muted;

    void Awake()
    {
        // only one AudioManager can exist. If another one is already playing, remove this one
        if (instance != null)
        {
            Destroy(gameObject);
            return;
        }
        instance = this;
        DontDestroyOnLoad(gameObject);

        sfxSource = gameObject.AddComponent<AudioSource>();
        musicSource = gameObject.AddComponent<AudioSource>();
        musicSource.loop = true;
        musicSource.clip = music;

        // load the saved settings
        masterVolume = PlayerPrefs.GetFloat("volume", 1f);
        muted = PlayerPrefs.GetInt("muted", 0) == 1;
        ApplyVolume();

        if (music != null)
        {
            musicSource.Play();
        }
    }

    void Update()
    {
        if (Keyboard.current.mKey.wasPressedThisFrame)
        {
            ToggleMute();
        }
    }

    // ----- playing sounds -----

    public static void Play(Sound sound)
    {
        if (instance == null)
        {
            return; // no AudioManager in the scene, stay silent
        }
        instance.PlayClip(instance.GetClip(sound), sound);
    }

    void PlayClip(AudioClip clip, Sound sound)
    {
        if (clip == null)
        {
            return;
        }

        // small random pitch change so repeated sounds (jump, swing) don't sound robotic
        bool vary = sound == Sound.Jump || sound == Sound.Land || sound == Sound.Swing || sound == Sound.EnemyHit;
        sfxSource.pitch = vary ? Random.Range(0.93f, 1.07f) : 1f;
        sfxSource.PlayOneShot(clip);
    }

    AudioClip GetClip(Sound sound)
    {
        switch (sound)
        {
            case Sound.Jump: return jump;
            case Sound.Land: return land;
            case Sound.Collect: return collect;
            case Sound.Hurt: return hurt;
            case Sound.Swing: return swing;
            case Sound.EnemyHit: return enemyHit;
            case Sound.Win: return win;
            case Sound.Checkpoint: return checkpoint;
            case Sound.Death: return death;
        }
        return null;
    }

    // ----- music -----

    public static void StopMusic()
    {
        if (instance != null)
        {
            instance.musicSource.Stop();
        }
    }

    public static void StartMusic()
    {
        if (instance != null && !instance.musicSource.isPlaying)
        {
            instance.musicSource.Play();
        }
    }

    // ----- volume (the pause menu slider calls SetVolume) -----

    public void SetVolume(float volume)
    {
        masterVolume = volume;
        PlayerPrefs.SetFloat("volume", volume);
        ApplyVolume();
    }

    public float GetVolume()
    {
        return masterVolume;
    }

    public void ToggleMute()
    {
        muted = !muted;
        PlayerPrefs.SetInt("muted", muted ? 1 : 0);
        ApplyVolume();
    }

    void ApplyVolume()
    {
        float v = muted ? 0f : masterVolume;
        sfxSource.volume = v;
        musicSource.volume = v * musicVolume;
    }
}
