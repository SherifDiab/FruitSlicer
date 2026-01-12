using UnityEngine;

/// <summary>
/// Manages game audio including sound effects and music.
/// Singleton pattern for easy access from any script.
/// </summary>
public class AudioManager : MonoBehaviour
{
    public static AudioManager Instance { get; private set; }

    [Header("Audio Sources")]
    [SerializeField] private AudioSource sfxSource;
    [SerializeField] private AudioSource musicSource;

    [Header("Slice Sounds")]
    [SerializeField] private AudioClip[] sliceSounds;
    [SerializeField] private float sliceVolume = 0.5f;

    [Header("Bomb Sounds")]
    [SerializeField] private AudioClip bombSound;
    [SerializeField] private float bombVolume = 0.7f;

    [Header("UI Sounds")]
    [SerializeField] private AudioClip buttonClickSound;
    [SerializeField] private AudioClip gameOverSound;
    [SerializeField] private AudioClip comboSound;
    [SerializeField] private AudioClip newHighScoreSound;
    [SerializeField] private float uiVolume = 0.5f;

    [Header("Swoosh Sounds")]
    [SerializeField] private AudioClip[] swooshSounds;
    [SerializeField] private float swooshVolume = 0.3f;

    [Header("Background Music")]
    [SerializeField] private AudioClip menuMusic;
    [SerializeField] private AudioClip gameMusic;
    [SerializeField] private float musicVolume = 0.3f;

    private void Awake()
    {
        if (Instance != null && Instance != this)
        {
            Destroy(gameObject);
            return;
        }
        Instance = this;
        DontDestroyOnLoad(gameObject);

        // Create audio sources if not assigned
        if (sfxSource == null)
        {
            sfxSource = gameObject.AddComponent<AudioSource>();
            sfxSource.playOnAwake = false;
        }
        if (musicSource == null)
        {
            musicSource = gameObject.AddComponent<AudioSource>();
            musicSource.playOnAwake = false;
            musicSource.loop = true;
        }
    }

    #region Sound Effects

    public void PlaySliceSound()
    {
        if (sliceSounds != null && sliceSounds.Length > 0)
        {
            AudioClip clip = sliceSounds[Random.Range(0, sliceSounds.Length)];
            PlaySFX(clip, sliceVolume);
        }
    }

    public void PlayBombSound()
    {
        if (bombSound != null)
        {
            PlaySFX(bombSound, bombVolume);
        }
    }

    public void PlaySwooshSound()
    {
        if (swooshSounds != null && swooshSounds.Length > 0)
        {
            AudioClip clip = swooshSounds[Random.Range(0, swooshSounds.Length)];
            PlaySFX(clip, swooshVolume);
        }
    }

    public void PlayButtonClick()
    {
        if (buttonClickSound != null)
        {
            PlaySFX(buttonClickSound, uiVolume);
        }
    }

    public void PlayGameOver()
    {
        if (gameOverSound != null)
        {
            PlaySFX(gameOverSound, uiVolume);
        }
    }

    public void PlayComboSound()
    {
        if (comboSound != null)
        {
            PlaySFX(comboSound, uiVolume);
        }
    }

    public void PlayNewHighScore()
    {
        if (newHighScoreSound != null)
        {
            PlaySFX(newHighScoreSound, uiVolume);
        }
    }

    private void PlaySFX(AudioClip clip, float volume)
    {
        if (clip != null && sfxSource != null)
        {
            sfxSource.PlayOneShot(clip, volume);
        }
    }

    #endregion

    #region Music

    public void PlayMenuMusic()
    {
        PlayMusic(menuMusic);
    }

    public void PlayGameMusic()
    {
        PlayMusic(gameMusic);
    }

    public void StopMusic()
    {
        if (musicSource != null)
        {
            musicSource.Stop();
        }
    }

    private void PlayMusic(AudioClip clip)
    {
        if (musicSource != null)
        {
            if (musicSource.clip == clip && musicSource.isPlaying)
                return;

            musicSource.clip = clip;
            musicSource.volume = musicVolume;
            if (clip != null)
            {
                musicSource.Play();
            }
        }
    }

    #endregion

    #region Volume Control

    public void SetMusicVolume(float volume)
    {
        musicVolume = Mathf.Clamp01(volume);
        if (musicSource != null)
        {
            musicSource.volume = musicVolume;
        }
    }

    public void SetSFXVolume(float volume)
    {
        sliceVolume = Mathf.Clamp01(volume);
        swooshVolume = Mathf.Clamp01(volume * 0.6f);
        uiVolume = Mathf.Clamp01(volume);
    }

    public void ToggleMute(bool muted)
    {
        if (sfxSource != null) sfxSource.mute = muted;
        if (musicSource != null) musicSource.mute = muted;
    }

    #endregion
}
