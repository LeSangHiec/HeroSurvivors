using UnityEngine;
using System.Collections;

public class AudioManager : MonoBehaviour
{
    // Singleton
    public static AudioManager Instance { get; private set; }

    [Header("Audio Sources")]
    [SerializeField] private AudioSource musicSource;
    [SerializeField] private AudioSource sfxSource;

    [Header("Music Clips")]
    [SerializeField] private AudioClip menuMusic;
    [SerializeField] private AudioClip gameplayMusic;
    [SerializeField] private AudioClip bossMusic;
    [SerializeField] private AudioClip victoryMusic;
    [SerializeField] private AudioClip gameOverMusic;

    [Header("SFX Clips")]
    [SerializeField] private AudioClip buttonClick;
    [SerializeField] private AudioClip playerHit;
    [SerializeField] private AudioClip playerDeath;
    [SerializeField] private AudioClip enemyHit;
    [SerializeField] private AudioClip enemyDeath;
    [SerializeField] private AudioClip weaponShoot;
    [SerializeField] private AudioClip xpCollect;
    [SerializeField] private AudioClip levelUp;
    [SerializeField] private AudioClip explosion;

    [Header("Volume Settings")]
    [SerializeField] private float masterVolume = 1f;
    [SerializeField] private float musicVolume = 0.7f;
    [SerializeField] private float sfxVolume = 1f;

    [Header("Settings")]
    [SerializeField] private bool muteMusic = false;
    [SerializeField] private bool muteSFX = false;

    void Awake()
    {
        // Singleton setup
        if (Instance == null)
        {
            Instance = this;
            DontDestroyOnLoad(gameObject); // Giữ qua scenes
        }
        else
        {
            Destroy(gameObject);
            return;
        }

        // Validate AudioSources
        ValidateAudioSources();

        // Apply initial volumes
        UpdateVolumes();
    }

    void ValidateAudioSources()
    {
        if (musicSource == null)
        {
            Debug.LogError("AudioManager: Music AudioSource is missing!");
        }

        if (sfxSource == null)
        {
            Debug.LogError("AudioManager: SFX AudioSource is missing!");
        }
    }

    // ========== MUSIC METHODS ==========

    public void PlayMusic(AudioClip clip, bool loop = true)
    {
        if (musicSource == null || clip == null || muteMusic) return;

        // Nếu đang chơi clip này rồi, không play lại
        if (musicSource.clip == clip && musicSource.isPlaying)
        {
            return;
        }

        musicSource.clip = clip;
        musicSource.loop = loop;
        musicSource.Play();

        Debug.Log($"Playing music: {clip.name}");
    }

    public void StopMusic()
    {
        if (musicSource == null) return;

        musicSource.Stop();
    }

    public void PauseMusic()
    {
        if (musicSource == null) return;

        musicSource.Pause();
    }

    public void ResumeMusic()
    {
        if (musicSource == null) return;

        musicSource.UnPause();
    }

    public void FadeOutMusic(float duration)
    {
        if (musicSource == null) return;

        StartCoroutine(FadeOutMusicCoroutine(duration));
    }

    public void FadeInMusic(AudioClip clip, float duration, bool loop = true)
    {
        if (musicSource == null || clip == null) return;

        musicSource.clip = clip;
        musicSource.loop = loop;
        musicSource.volume = 0f;
        musicSource.Play();

        StartCoroutine(FadeInMusicCoroutine(duration));
    }

    IEnumerator FadeOutMusicCoroutine(float duration)
    {
        float startVolume = musicSource.volume;
        float elapsed = 0f;

        while (elapsed < duration)
        {
            elapsed += Time.deltaTime;
            musicSource.volume = Mathf.Lerp(startVolume, 0f, elapsed / duration);
            yield return null;
        }

        musicSource.Stop();
        musicSource.volume = startVolume;
    }

    IEnumerator FadeInMusicCoroutine(float duration)
    {
        float targetVolume = musicVolume * masterVolume;
        float elapsed = 0f;

        while (elapsed < duration)
        {
            elapsed += Time.deltaTime;
            musicSource.volume = Mathf.Lerp(0f, targetVolume, elapsed / duration);
            yield return null;
        }

        musicSource.volume = targetVolume;
    }

    // ========== SFX METHODS ==========

    public void PlaySFX(AudioClip clip)
    {
        if (sfxSource == null || clip == null || muteSFX) return;

        sfxSource.PlayOneShot(clip, sfxVolume * masterVolume);
    }

    public void PlaySFX(AudioClip clip, float volumeScale)
    {
        if (sfxSource == null || clip == null || muteSFX) return;

        sfxSource.PlayOneShot(clip, volumeScale * sfxVolume * masterVolume);
    }

    // ========== PRESET MUSIC ==========

    public void PlayMenuMusic()
    {
        if (menuMusic != null)
        {
            PlayMusic(menuMusic);
        }
    }

    public void PlayGameplayMusic()
    {
        if (gameplayMusic != null)
        {
            PlayMusic(gameplayMusic);
        }
    }

    public void PlayBossMusic()
    {
        if (bossMusic != null)
        {
            PlayMusic(bossMusic);
        }
    }

    public void PlayVictoryMusic()
    {
        if (victoryMusic != null)
        {
            PlayMusic(victoryMusic, false); // Không loop
        }
    }

    public void PlayGameOverMusic()
    {
        if (gameOverMusic != null)
        {
            PlayMusic(gameOverMusic, false); // Không loop
        }
    }

    // ========== PRESET SFX ==========

    public void PlayButtonClick()
    {
        PlaySFX(buttonClick);
    }

    public void PlayPlayerHit()
    {
        PlaySFX(playerHit);
    }

    public void PlayPlayerDeath()
    {
        PlaySFX(playerDeath);
    }

    public void PlayEnemyHit()
    {
        PlaySFX(enemyHit);
    }

    public void PlayEnemyDeath()
    {
        PlaySFX(enemyDeath);
    }

    public void PlayWeaponShoot()
    {
        PlaySFX(weaponShoot);
    }

    public void PlayXPCollect()
    {
        PlaySFX(xpCollect);
    }

    public void PlayLevelUp()
    {
        PlaySFX(levelUp);
    }

    public void PlayExplosion()
    {
        PlaySFX(explosion);
    }

    // ========== VOLUME CONTROL ==========

    public void SetMasterVolume(float volume)
    {
        masterVolume = Mathf.Clamp01(volume);
        UpdateVolumes();
    }

    public void SetMusicVolume(float volume)
    {
        musicVolume = Mathf.Clamp01(volume);
        UpdateVolumes();
    }

    public void SetSFXVolume(float volume)
    {
        sfxVolume = Mathf.Clamp01(volume);
        UpdateVolumes();
    }

    void UpdateVolumes()
    {
        if (musicSource != null)
        {
            musicSource.volume = musicVolume * masterVolume;
        }

        // SFX volume áp dụng khi PlayOneShot
    }

    // ========== MUTE CONTROL ==========

    public void ToggleMuteMusic()
    {
        muteMusic = !muteMusic;

        if (musicSource != null)
        {
            musicSource.mute = muteMusic;
        }

        Debug.Log($"Music muted: {muteMusic}");
    }

    public void ToggleMuteSFX()
    {
        muteSFX = !muteSFX;

        Debug.Log($"SFX muted: {muteSFX}");
    }

    public void SetMuteMusic(bool mute)
    {
        muteMusic = mute;

        if (musicSource != null)
        {
            musicSource.mute = mute;
        }
    }

    public void SetMuteSFX(bool mute)
    {
        muteSFX = mute;
    }

    // ========== GETTERS ==========

    public float GetMasterVolume() => masterVolume;
    public float GetMusicVolume() => musicVolume;
    public float GetSFXVolume() => sfxVolume;
    public bool IsMusicMuted() => muteMusic;
    public bool IsSFXMuted() => muteSFX;
    public bool IsMusicPlaying() => musicSource != null && musicSource.isPlaying;

    // ========== DEBUG ==========

    
}