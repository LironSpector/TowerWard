using UnityEngine;
using System.Collections.Generic;

/// <summary>
/// Manages all music and sound effects in the game.
/// </summary>
public class AudioManager : MonoBehaviour
{
    public static AudioManager Instance;

    // ========== MUSIC SOURCES ==========
    [Header("Music Sources")]
    public AudioSource mainMenuMusicSource;   // Loop for main menu
    public AudioSource gameMusicSource;       // Loop for game
    public AudioSource oneShotMusicSource;    // For playing one-shot music (win/lose) if needed

    // ========== SFX SOURCE ==========
    [Header("Sound Effects")]
    public AudioSource sfxSource; // a single AudioSource for all SFX, or you can do a pool

    // ========== CLIPS ==========

    [Header("Music Clips")]
    public AudioClip mainMenuMusicClip;   // loop in main menu
    public AudioClip gameMusicClip;       // loop in game
    public AudioClip winMusicClip;        // one-shot after game is won
    public AudioClip loseMusicClip;       // one-shot after game is lost

    [Header("SFX Clips")]
    public AudioClip balloonPopClip;      // balloon pop
    public AudioClip towerPanelChooseClip;
    public AudioClip towerPlacementClip;
    public AudioClip towerSelectionClip;
    // Suppose we want an array/dictionary for projectile shots:
    public List<AudioClip> projectileShotClips; // or a Dictionary<ProjectileType, AudioClip>

    // ========== VOLUMES & MUTE SETTINGS ==========

    [Range(0f, 1f)] public float mainMenuVolume = 1f;          // 0=muted, 1=full
    [Range(0f, 1f)] public float backgroundGameVolume = 1f;    // 0=muted, 1=full
    [Range(0f, 1f)] public float sfxVolume = 1f;               // 0=muted, 1=full
    public bool isMuted = false;

    void Awake()
    {
        // Singleton pattern
        if (Instance == null)
        {
            Instance = this;
            DontDestroyOnLoad(gameObject); // if you want it persistent across scenes
        }
        else
        {
            Destroy(gameObject);
            return;
        }

        // OPTIONAL: Initialize your AudioSources if needed
        // e.g. mainMenuMusicSource.loop = true;
        // e.g. gameMusicSource.loop = true;
    }

    void Start()
    {
        // Optionally: Start playing main menu music if you're in main menu scene
        // mainMenuMusicSource.clip = mainMenuMusicClip;
        // mainMenuMusicSource.loop = true;
        // mainMenuMusicSource.Play();
        RefreshVolumes();
    }

    // ========== PUBLIC METHODS ==========

    /// <summary>
    /// Call this if we transition to the Main Menu scene, to start looping main menu music.
    /// </summary>
    public void PlayMainMenuMusic()
    {
        StopGameMusic();
        if (mainMenuMusicSource != null && mainMenuMusicClip != null)
        {
            mainMenuMusicSource.clip = mainMenuMusicClip;
            mainMenuMusicSource.loop = true;
            mainMenuMusicSource.Play();
        }
    }

    /// <summary>
    /// Call this if we transition to the Game scene, to start looping game music.
    /// </summary>
    public void PlayGameMusic()
    {
        StopMainMenuMusic();
        if (gameMusicSource != null && gameMusicClip != null)
        {
            gameMusicSource.clip = gameMusicClip;
            gameMusicSource.loop = true;
            gameMusicSource.Play();
        }
    }

    public void StopMainMenuMusic()
    {
        if (mainMenuMusicSource != null) mainMenuMusicSource.Stop();
    }

    public void StopGameMusic()
    {
        if (gameMusicSource != null) gameMusicSource.Stop();
    }

    /// <summary>
    /// Plays the "win" music once, then stops.
    /// </summary>
    public void PlayWinMusic()
    {
        if (oneShotMusicSource != null && winMusicClip != null)
        {
            oneShotMusicSource.Stop(); // ensure no overlap
            oneShotMusicSource.loop = false;
            oneShotMusicSource.clip = winMusicClip;
            oneShotMusicSource.Play();
        }
    }

    /// <summary>
    /// Plays the "lose" music once, then stops.
    /// </summary>
    public void PlayLoseMusic()
    {
        if (oneShotMusicSource != null && loseMusicClip != null)
        {
            oneShotMusicSource.Stop();
            oneShotMusicSource.loop = false;
            oneShotMusicSource.clip = loseMusicClip;
            oneShotMusicSource.Play();
        }
    }

    // ========== SFX Methods ==========

    /// <summary>
    /// Called when a balloon pops.
    /// </summary>
    public void PlayBalloonPop()
    {
        if (balloonPopClip != null) PlaySFX(balloonPopClip);
    }

    /// <summary>
    /// Called when the user chooses tower from tower panel.
    /// </summary>
    public void PlayTowerPanelChoose()
    {
        if (towerPanelChooseClip != null) PlaySFX(towerPanelChooseClip);
    }

    /// <summary>
    /// Called when a tower is placed on the map.
    /// </summary>
    public void PlayTowerPlacement()
    {
        if (towerPlacementClip != null) PlaySFX(towerPlacementClip);
    }

    /// <summary>
    /// Called when a tower on the map is selected.
    /// </summary>
    public void PlayTowerSelection()
    {
        if (towerSelectionClip != null) PlaySFX(towerSelectionClip);
    }

    /// <summary>
    /// Called when a projectile is shot. We can pick based on the tower or projectile ID.
    /// </summary>
    public void PlayProjectileShot(int projectileIndex = 0)
    {
        if (projectileShotClips != null && projectileIndex >= 0 && projectileIndex < projectileShotClips.Count)
        {
            PlaySFX(projectileShotClips[projectileIndex]);
        }
    }

    /// <summary>
    /// Generic method to play an SFX clip with the current sfxVolume.
    /// </summary>
    private void PlaySFX(AudioClip clip)
    {
        if (sfxSource == null || clip == null) return;
        if (isMuted) return; // If fully muted => skip

        // PlayOneShot so multiple sfx can overlap
        sfxSource.PlayOneShot(clip, sfxVolume);
    }

    // ========== VOLUME & MUTE LOGIC ==========

    /// <summary>
    /// Called when the user changes volume settings in the Settings panel.
    /// e.g. mainMenuVolume, backgroundGameVolume, sfxVolume, isMuted
    /// </summary>
    public void RefreshVolumes()
    {
        // If muted => all set to 0, else set to the slider values
        float mainMenuVol = isMuted ? 0f : mainMenuVolume;
        float gameVol = isMuted ? 0f : backgroundGameVolume;
        float sfxVol = isMuted ? 0f : sfxVolume;

        if (mainMenuMusicSource != null) mainMenuMusicSource.volume = mainMenuVol;
        if (gameMusicSource != null) gameMusicSource.volume = gameVol;

        // For SFX, we won't set sfxSource.volume because we do PlayOneShot(clip, sfxVolume).
        // But if you want to do it, you can set sfxSource.volume = sfxVol for baseline.
        if (sfxSource != null) sfxSource.volume = sfxVol;

        // For the oneShotMusicSource => typically used for the win/lose clip
        if (oneShotMusicSource != null) oneShotMusicSource.volume = sfxVol;
        // or you might want a separate volume for that
    }
}
