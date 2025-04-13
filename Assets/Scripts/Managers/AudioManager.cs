using UnityEngine;
using UnityEngine.SceneManagement;
using TMPro;
using System.Collections.Generic;

/// <summary>
/// Description:
/// Manages all music and sound effects in the game. This includes handling background music for the main menu 
/// and in-game scenarios, one-shot music clips for win/lose events, and various sound effects (SFX) such as balloon pops,
/// tower interactions, and projectile shots. It also controls volume and mute settings across different audio sources.
/// The AudioManager implements the singleton pattern for global access and persists across scene loads.
/// </summary>
public class AudioManager : MonoBehaviour
{
    /// <summary>
    /// Singleton instance of the AudioManager.
    /// </summary>
    public static AudioManager Instance;

    // ========== MUSIC SOURCES ==========
    [Header("Music Sources")]
    /// <summary>
    /// AudioSource used for looping main menu music.
    /// </summary>
    public AudioSource mainMenuMusicSource;
    /// <summary>
    /// AudioSource used for looping in-game music.
    /// </summary>
    public AudioSource gameMusicSource;
    /// <summary>
    /// AudioSource used for playing one-shot music clips (such as win or lose music).
    /// </summary>
    public AudioSource oneShotMusicSource;

    // ========== SFX SOURCE ==========
    [Header("Sound Effects")]
    /// <summary>
    /// AudioSource used for playing all sound effects (SFX).
    /// </summary>
    public AudioSource sfxSource;

    // ========== CLIPS ==========
    [Header("Music Clips")]
    /// <summary>
    /// AudioClip used for main menu music.
    /// </summary>
    public AudioClip mainMenuMusicClip;
    /// <summary>
    /// AudioClip used for in-game music.
    /// </summary>
    public AudioClip gameMusicClip;
    /// <summary>
    /// AudioClip played once when the game is won.
    /// </summary>
    public AudioClip winMusicClip;
    /// <summary>
    /// AudioClip played once when the game is lost.
    /// </summary>
    public AudioClip loseMusicClip;

    [Header("SFX Clips")]
    /// <summary>
    /// AudioClip played when a balloon pops.
    /// </summary>
    public AudioClip balloonPopClip;
    /// <summary>
    /// AudioClip played when a tower is chosen from the tower panel.
    /// </summary>
    public AudioClip towerPanelChooseClip;
    /// <summary>
    /// AudioClip played when a tower is placed on the map.
    /// </summary>
    public AudioClip towerPlacementClip;
    /// <summary>
    /// AudioClip played when a tower on the map is selected.
    /// </summary>
    public AudioClip towerSelectionClip;
    /// <summary>
    /// List of AudioClips for projectile shots. Alternate approach could be using a dictionary keyed by projectile type.
    /// </summary>
    public List<AudioClip> projectileShotClips;


    // ========== VOLUMES & MUTE SETTINGS ==========
    /// <summary>
    /// Volume level for main menu music (0 = muted, 1 = full volume).
    /// </summary>
    [Range(0f, 1f)] public float mainMenuVolume = 1f;
    /// <summary>
    /// Volume level for in-game background music.
    /// </summary>
    [Range(0f, 1f)] public float backgroundGameVolume = 1f;
    /// <summary>
    /// Volume level for sound effects.
    /// </summary>
    [Range(0f, 1f)] public float sfxVolume = 1f;
    /// <summary>
    /// Global mute flag. If true, all audio volumes are set to 0.
    /// </summary>
    public bool isMuted = false;




    /// <summary>
    /// Awake is called when the script instance is being loaded.
    /// Sets up the singleton instance and ensures persistence across scene loads.
    /// </summary>
    void Awake()
    {
        if (Instance == null)
        {
            Instance = this;
            DontDestroyOnLoad(gameObject); // Persist across scenes.
        }
        else
        {
            Destroy(gameObject);
            return;
        }
    }

    /// <summary>
    /// Start is called before the first frame update.
    /// Refreshes audio volumes according to current settings.
    /// </summary>
    void Start()
    {
        RefreshVolumes();
    }

    // ========== PUBLIC METHODS ==========

    /// <summary>
    /// Plays the looping main menu music. Also ensures that the game music is stopped.
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
    /// Plays the looping in-game music. Also ensures that the main menu music is stopped.
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

    /// <summary>
    /// Stops the main menu music, if it is playing.
    /// </summary>
    public void StopMainMenuMusic()
    {
        if (mainMenuMusicSource != null) mainMenuMusicSource.Stop();
    }

    /// <summary>
    /// Stops the in-game music, if it is playing.
    /// </summary>
    public void StopGameMusic()
    {
        if (gameMusicSource != null) gameMusicSource.Stop();
    }

    /// <summary>
    /// Plays the win music clip as a one-shot audio. Ensures no overlapping audio occurs.
    /// </summary>
    public void PlayWinMusic()
    {
        if (oneShotMusicSource != null && winMusicClip != null)
        {
            oneShotMusicSource.Stop(); // Ensure any currently playing clip is stopped.
            oneShotMusicSource.loop = false;
            oneShotMusicSource.clip = winMusicClip;
            oneShotMusicSource.Play();
        }
    }

    /// <summary>
    /// Plays the lose music clip as a one-shot audio. Ensures no overlapping audio occurs.
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
    /// Plays the balloon pop sound effect.
    /// </summary>
    public void PlayBalloonPop()
    {
        if (balloonPopClip != null) PlaySFX(balloonPopClip);
    }

    /// <summary>
    /// Plays the tower panel selection sound effect.
    /// </summary>
    public void PlayTowerPanelChoose()
    {
        if (towerPanelChooseClip != null) PlaySFX(towerPanelChooseClip);
    }

    /// <summary>
    /// Plays the tower placement sound effect.
    /// </summary>
    public void PlayTowerPlacement()
    {
        if (towerPlacementClip != null) PlaySFX(towerPlacementClip);
    }

    /// <summary>
    /// Plays the tower selection sound effect.
    /// </summary>
    public void PlayTowerSelection()
    {
        if (towerSelectionClip != null) PlaySFX(towerSelectionClip);
    }

    /// <summary>
    /// Plays a projectile shot sound effect. The clip is chosen based on the provided projectile index.
    /// </summary>
    /// <param name="projectileIndex">Index of the projectile sound effect to play (default is 0).</param>
    public void PlayProjectileShot(int projectileIndex = 0)
    {
        if (projectileShotClips != null && projectileIndex >= 0 && projectileIndex < projectileShotClips.Count)
        {
            PlaySFX(projectileShotClips[projectileIndex]);
        }
    }

    /// <summary>
    /// Generic method to play a sound effect (SFX) clip using the sfxVolume.
    /// If the audio is muted or there is no sfxSource or clip, no sound is played.
    /// </summary>
    /// <param name="clip">The AudioClip to play.</param>
    private void PlaySFX(AudioClip clip)
    {
        if (sfxSource == null || clip == null) return;
        if (isMuted) return; // Skip playback if muted.

        // Play the clip as a one-shot to allow overlapping SFX.
        sfxSource.PlayOneShot(clip, sfxVolume);
    }

    // ========== VOLUME & MUTE LOGIC ==========

    /// <summary>
    /// Refreshes the volume settings for all audio sources based on current volume levels and mute status.
    /// If muted, all volume levels are set to 0.
    /// </summary>
    public void RefreshVolumes()
    {
        // Determine volume levels based on mute settings.
        float mainMenuVol = isMuted ? 0f : mainMenuVolume;
        float gameVol = isMuted ? 0f : backgroundGameVolume;
        float sfxVol = isMuted ? 0f : sfxVolume;

        if (mainMenuMusicSource != null) mainMenuMusicSource.volume = mainMenuVol;
        if (gameMusicSource != null) gameMusicSource.volume = gameVol;
        if (sfxSource != null) sfxSource.volume = sfxVol;
        if (oneShotMusicSource != null) oneShotMusicSource.volume = sfxVol;
    }
}















//using UnityEngine;
//using System.Collections.Generic;

///// <summary>
///// Manages all music and sound effects in the game.
///// </summary>
//public class AudioManager : MonoBehaviour
//{
//    public static AudioManager Instance;

//    // ========== MUSIC SOURCES ==========
//    [Header("Music Sources")]
//    public AudioSource mainMenuMusicSource;   // Loop for main menu
//    public AudioSource gameMusicSource;       // Loop for game
//    public AudioSource oneShotMusicSource;    // For playing one-shot music (win/lose)

//    // ========== SFX SOURCE ==========
//    [Header("Sound Effects")]
//    public AudioSource sfxSource; // a single AudioSource for all SFX

//    // ========== CLIPS ==========

//    [Header("Music Clips")]
//    public AudioClip mainMenuMusicClip;   // loop in main menu
//    public AudioClip gameMusicClip;       // loop in game
//    public AudioClip winMusicClip;        // one-shot after game is won
//    public AudioClip loseMusicClip;       // one-shot after game is lost

//    [Header("SFX Clips")]
//    public AudioClip balloonPopClip;
//    public AudioClip towerPanelChooseClip;
//    public AudioClip towerPlacementClip;
//    public AudioClip towerSelectionClip;

//    public List<AudioClip> projectileShotClips; // or a Dictionary<ProjectileType, AudioClip>

//    // ========== VOLUMES & MUTE SETTINGS ==========

//    [Range(0f, 1f)] public float mainMenuVolume = 1f;          // 0=muted, 1=full
//    [Range(0f, 1f)] public float backgroundGameVolume = 1f;    // 0=muted, 1=full
//    [Range(0f, 1f)] public float sfxVolume = 1f;               // 0=muted, 1=full
//    public bool isMuted = false;

//    void Awake()
//    {
//        // Singleton pattern
//        if (Instance == null)
//        {
//            Instance = this;
//            DontDestroyOnLoad(gameObject); // persistent across scenes
//        }
//        else
//        {
//            Destroy(gameObject);
//            return;
//        }
//    }

//    void Start()
//    {
//        RefreshVolumes();
//    }

//    // ========== PUBLIC METHODS ==========

//    /// <summary>
//    /// Call this if we transition to the Main Menu scene, to start looping main menu music.
//    /// </summary>
//    public void PlayMainMenuMusic()
//    {
//        StopGameMusic();
//        if (mainMenuMusicSource != null && mainMenuMusicClip != null)
//        {
//            mainMenuMusicSource.clip = mainMenuMusicClip;
//            mainMenuMusicSource.loop = true;
//            mainMenuMusicSource.Play();
//        }
//    }

//    /// <summary>
//    /// Call this if we transition to the Game scene, to start looping game music.
//    /// </summary>
//    public void PlayGameMusic()
//    {
//        StopMainMenuMusic();
//        if (gameMusicSource != null && gameMusicClip != null)
//        {
//            gameMusicSource.clip = gameMusicClip;
//            gameMusicSource.loop = true;
//            gameMusicSource.Play();
//        }
//    }

//    public void StopMainMenuMusic()
//    {
//        if (mainMenuMusicSource != null) mainMenuMusicSource.Stop();
//    }

//    public void StopGameMusic()
//    {
//        if (gameMusicSource != null) gameMusicSource.Stop();
//    }

//    /// <summary>
//    /// Plays the "win" music once, then stops.
//    /// </summary>
//    public void PlayWinMusic()
//    {
//        if (oneShotMusicSource != null && winMusicClip != null)
//        {
//            oneShotMusicSource.Stop(); // ensure no overlap
//            oneShotMusicSource.loop = false;
//            oneShotMusicSource.clip = winMusicClip;
//            oneShotMusicSource.Play();
//        }
//    }

//    /// <summary>
//    /// Plays the "lose" music once, then stops.
//    /// </summary>
//    public void PlayLoseMusic()
//    {
//        if (oneShotMusicSource != null && loseMusicClip != null)
//        {
//            oneShotMusicSource.Stop();
//            oneShotMusicSource.loop = false;
//            oneShotMusicSource.clip = loseMusicClip;
//            oneShotMusicSource.Play();
//        }
//    }

//    // ========== SFX Methods ==========

//    /// <summary>
//    /// Called when a balloon pops.
//    /// </summary>
//    public void PlayBalloonPop()
//    {
//        if (balloonPopClip != null) PlaySFX(balloonPopClip);
//    }

//    /// <summary>
//    /// Called when the user chooses tower from tower panel.
//    /// </summary>
//    public void PlayTowerPanelChoose()
//    {
//        if (towerPanelChooseClip != null) PlaySFX(towerPanelChooseClip);
//    }

//    /// <summary>
//    /// Called when a tower is placed on the map.
//    /// </summary>
//    public void PlayTowerPlacement()
//    {
//        if (towerPlacementClip != null) PlaySFX(towerPlacementClip);
//    }

//    /// <summary>
//    /// Called when a tower on the map is selected.
//    /// </summary>
//    public void PlayTowerSelection()
//    {
//        if (towerSelectionClip != null) PlaySFX(towerSelectionClip);
//    }

//    /// <summary>
//    /// Called when a projectile is shot. We can pick based on the tower or projectile ID.
//    /// </summary>
//    public void PlayProjectileShot(int projectileIndex = 0)
//    {
//        if (projectileShotClips != null && projectileIndex >= 0 && projectileIndex < projectileShotClips.Count)
//        {
//            PlaySFX(projectileShotClips[projectileIndex]);
//        }
//    }

//    /// <summary>
//    /// Generic method to play an SFX clip with the current sfxVolume.
//    /// </summary>
//    private void PlaySFX(AudioClip clip)
//    {
//        if (sfxSource == null || clip == null) return;
//        if (isMuted) return; // If fully muted => skip

//        // PlayOneShot so multiple sfx can overlap
//        sfxSource.PlayOneShot(clip, sfxVolume);
//    }

//    // ========== VOLUME & MUTE LOGIC ==========

//    /// <summary>
//    /// Called when the user changes volume settings in the Settings panel.
//    /// e.g. mainMenuVolume, backgroundGameVolume, sfxVolume, isMuted
//    /// </summary>
//    public void RefreshVolumes()
//    {
//        // If muted => all set to 0, else set to the slider values
//        float mainMenuVol = isMuted ? 0f : mainMenuVolume;
//        float gameVol = isMuted ? 0f : backgroundGameVolume;
//        float sfxVol = isMuted ? 0f : sfxVolume;

//        if (mainMenuMusicSource != null) mainMenuMusicSource.volume = mainMenuVol;
//        if (gameMusicSource != null) gameMusicSource.volume = gameVol;

//        if (sfxSource != null) sfxSource.volume = sfxVol;

//        // For the oneShotMusicSource => used for the win/lose clip
//        if (oneShotMusicSource != null) oneShotMusicSource.volume = sfxVol;
//    }
//}
