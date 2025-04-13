using UnityEngine;
using UnityEngine.UI;
using TMPro;

public class SettingsPanel : MonoBehaviour
{
    [Header("UI Controls - Audio")]
    public Slider mainMenuVolumeSlider;
    public Slider backgroundGameVolumeSlider;
    public Slider sfxVolumeSlider;
    public Toggle muteAllToggle;

    [Header("UI Controls - Stats")]
    public Toggle statsNoneToggle;      // Radio for "Don't show stats"
    public Toggle statsFpsToggle;       // Radio for "Show FPS"

    [Header("UI Controls - Extreme Speed")]
    public Toggle extremeSpeedToggle;

    public Button closeButton;
    public Button saveAndCloseButton;

    // We'll store the initial values so we can revert if "Close" is clicked
    private float initialMainMenuVol;
    private float initialBackgroundGameVol;
    private float initialSfxVol;
    private bool initialIsMuted;

    private int initialStatsMode;
    // 0=none, 1=fps, 2=advanced
    private bool initialExtremeSpeed;

    void Start()
    {
        closeButton.onClick.AddListener(OnCloseClicked);
        saveAndCloseButton.onClick.AddListener(OnSaveAndCloseClicked);
    }

    // Called externally when opening the panel
    public void ShowSettings()
    {
        // record current values from AudioManager
        initialMainMenuVol = AudioManager.Instance.mainMenuVolume;
        initialBackgroundGameVol = AudioManager.Instance.backgroundGameVolume;
        initialSfxVol = AudioManager.Instance.sfxVolume;
        initialIsMuted = AudioManager.Instance.isMuted;

        // record the stats display mode (from some global or static)
        initialStatsMode = GlobalSettings.StatsDisplayMode;
        // e.g. store 0=none, 1=fps, 2=advanced

        initialExtremeSpeed = GlobalSettings.ExtremeSpeedEnabled;

        // Set UI
        mainMenuVolumeSlider.value = initialMainMenuVol;
        backgroundGameVolumeSlider.value = initialBackgroundGameVol;
        sfxVolumeSlider.value = initialSfxVol;
        muteAllToggle.isOn = initialIsMuted;

        // Stats radio
        statsNoneToggle.isOn = (initialStatsMode == 0);
        statsFpsToggle.isOn = (initialStatsMode == 1);

        // Extreme speed
        extremeSpeedToggle.isOn = initialExtremeSpeed;

        gameObject.SetActive(true);
    }

    private void OnCloseClicked()
    {
        // revert changes
        AudioManager.Instance.mainMenuVolume = initialMainMenuVol;
        AudioManager.Instance.backgroundGameVolume = initialBackgroundGameVol;
        AudioManager.Instance.sfxVolume = initialSfxVol;
        AudioManager.Instance.isMuted = initialIsMuted;
        AudioManager.Instance.RefreshVolumes();

        GlobalSettings.StatsDisplayMode = initialStatsMode;
        GlobalSettings.ExtremeSpeedEnabled = initialExtremeSpeed;
        GlobalSettings.ApplyTimeScaleIfPossible(); // handle timescale logic

        gameObject.SetActive(false);
    }

    private void OnSaveAndCloseClicked()
    {
        // Audio
        AudioManager.Instance.mainMenuVolume = mainMenuVolumeSlider.value;
        AudioManager.Instance.backgroundGameVolume = backgroundGameVolumeSlider.value;
        AudioManager.Instance.sfxVolume = sfxVolumeSlider.value;
        AudioManager.Instance.isMuted = muteAllToggle.isOn;
        AudioManager.Instance.RefreshVolumes();

        // Stats radio => figure out which is on
        if (statsNoneToggle.isOn) GlobalSettings.StatsDisplayMode = 0;
        else if (statsFpsToggle.isOn) GlobalSettings.StatsDisplayMode = 1;

        // Extreme speed
        GlobalSettings.ExtremeSpeedEnabled = extremeSpeedToggle.isOn;

        // Apply timescale logic
        GlobalSettings.ApplyTimeScaleIfPossible();

        gameObject.SetActive(false);
    }
}
