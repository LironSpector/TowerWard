using UnityEngine;
using UnityEngine.UI;
using TMPro;

/// <summary>
/// Description:
/// Manages the settings panel UI that allows the player to adjust audio settings (volumes and mute),
/// select the stats display mode, and toggle extreme speed mode. This class handles both saving and reverting
/// changes based on user input from the panel's controls.
/// </summary>
public class SettingsPanel : MonoBehaviour
{
    [Header("UI Controls - Audio")]
    /// <summary>
    /// Slider for adjusting the main menu volume.
    /// </summary>
    public Slider mainMenuVolumeSlider;
    /// <summary>
    /// Slider for adjusting the background game volume.
    /// </summary>
    public Slider backgroundGameVolumeSlider;
    /// <summary>
    /// Slider for adjusting the sound effects volume.
    /// </summary>
    public Slider sfxVolumeSlider;
    /// <summary>
    /// Toggle for muting all audio.
    /// </summary>
    public Toggle muteAllToggle;

    [Header("UI Controls - Stats")]
    /// <summary>
    /// Toggle (radio button) for disabling stats display.
    /// </summary>
    public Toggle statsNoneToggle;
    /// <summary>
    /// Toggle (radio button) for displaying FPS.
    /// </summary>
    public Toggle statsFpsToggle;

    [Header("UI Controls - Extreme Speed")]
    /// <summary>
    /// Toggle for enabling extreme speed mode.
    /// </summary>
    public Toggle extremeSpeedToggle;

    /// <summary>
    /// Button to close the settings panel without saving changes.
    /// </summary>
    public Button closeButton;
    /// <summary>
    /// Button to save settings changes and close the settings panel.
    /// </summary>
    public Button saveAndCloseButton;

    // Private fields to store initial settings values for reverting if necessary.
    private float initialMainMenuVol;
    private float initialBackgroundGameVol;
    private float initialSfxVol;
    private bool initialIsMuted;
    private int initialStatsMode;      // 0=none, 1=fps, 2=advanced (if applicable)
    private bool initialExtremeSpeed;

    /// <summary>
    /// Initializes the settings panel by adding listeners to the close and save buttons.
    /// </summary>
    void Start()
    {
        closeButton.onClick.AddListener(OnCloseClicked);
        saveAndCloseButton.onClick.AddListener(OnSaveAndCloseClicked);
    }

    /// <summary>
    /// Called externally when the settings panel is opened.
    /// Records current settings, updates UI controls with these values, and displays the panel.
    /// </summary>
    public void ShowSettings()
    {
        // Record current audio settings from AudioManager.
        initialMainMenuVol = AudioManager.Instance.mainMenuVolume;
        initialBackgroundGameVol = AudioManager.Instance.backgroundGameVolume;
        initialSfxVol = AudioManager.Instance.sfxVolume;
        initialIsMuted = AudioManager.Instance.isMuted;

        // Record current stats display mode.
        initialStatsMode = GlobalSettings.StatsDisplayMode; // e.g., 0=none, 1=fps, etc.

        // Record current extreme speed setting.
        initialExtremeSpeed = GlobalSettings.ExtremeSpeedEnabled;

        // Set UI control values to the recorded settings.
        mainMenuVolumeSlider.value = initialMainMenuVol;
        backgroundGameVolumeSlider.value = initialBackgroundGameVol;
        sfxVolumeSlider.value = initialSfxVol;
        muteAllToggle.isOn = initialIsMuted;

        statsNoneToggle.isOn = (initialStatsMode == 0);
        statsFpsToggle.isOn = (initialStatsMode == 1);

        extremeSpeedToggle.isOn = initialExtremeSpeed;

        // Activate the settings panel.
        gameObject.SetActive(true);
    }

    /// <summary>
    /// Reverts any changes made in the settings panel and closes the panel.
    /// Audio and global settings are restored to their initial values.
    /// </summary>
    private void OnCloseClicked()
    {
        // Revert audio settings.
        AudioManager.Instance.mainMenuVolume = initialMainMenuVol;
        AudioManager.Instance.backgroundGameVolume = initialBackgroundGameVol;
        AudioManager.Instance.sfxVolume = initialSfxVol;
        AudioManager.Instance.isMuted = initialIsMuted;
        AudioManager.Instance.RefreshVolumes();

        // Revert stats display and extreme speed settings.
        GlobalSettings.StatsDisplayMode = initialStatsMode;
        GlobalSettings.ExtremeSpeedEnabled = initialExtremeSpeed;
        GlobalSettings.ApplyTimeScaleIfPossible();

        // Hide the settings panel.
        gameObject.SetActive(false);
    }

    /// <summary>
    /// Saves the changes made in the settings panel and closes the panel.
    /// Updates AudioManager and GlobalSettings with the new values, and applies volume and timescale adjustments.
    /// </summary>
    private void OnSaveAndCloseClicked()
    {
        // Save audio settings.
        AudioManager.Instance.mainMenuVolume = mainMenuVolumeSlider.value;
        AudioManager.Instance.backgroundGameVolume = backgroundGameVolumeSlider.value;
        AudioManager.Instance.sfxVolume = sfxVolumeSlider.value;
        AudioManager.Instance.isMuted = muteAllToggle.isOn;
        AudioManager.Instance.RefreshVolumes();

        // Update stats display mode based on UI toggles.
        if (statsNoneToggle.isOn)
            GlobalSettings.StatsDisplayMode = 0;
        else if (statsFpsToggle.isOn)
            GlobalSettings.StatsDisplayMode = 1;

        // Update extreme speed setting.
        GlobalSettings.ExtremeSpeedEnabled = extremeSpeedToggle.isOn;

        // Apply timescale logic based on new settings.
        GlobalSettings.ApplyTimeScaleIfPossible();

        // Hide the settings panel.
        gameObject.SetActive(false);
    }
}
