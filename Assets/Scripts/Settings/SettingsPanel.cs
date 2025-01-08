using UnityEngine;
using UnityEngine.UI;
using TMPro;

public class SettingsPanel : MonoBehaviour
{
    [Header("UI Controls")]
    public Slider mainMenuVolumeSlider;
    public Slider backgroundGameVolumeSlider;
    public Slider sfxVolumeSlider;
    public Toggle muteAllToggle;

    public Button closeButton;
    public Button saveAndCloseButton;

    // We'll store the initial values so we can revert if the user just hits "Close"
    private float initialMainMenuVol;
    private float initialBackgroundGameVol;
    private float initialSfxVol;
    private bool initialIsMuted;

    void Start()
    {
        closeButton.onClick.AddListener(OnCloseClicked);
        saveAndCloseButton.onClick.AddListener(OnSaveAndCloseClicked);
    }

    // Called when we open the settings panel
    public void ShowSettings()
    {
        Debug.Log("Showing Settings!");

        // record current values
        initialMainMenuVol = AudioManager.Instance.mainMenuVolume;
        initialBackgroundGameVol = AudioManager.Instance.backgroundGameVolume;
        initialSfxVol = AudioManager.Instance.sfxVolume;
        initialIsMuted = AudioManager.Instance.isMuted;

        // set UI controls to reflect them
        mainMenuVolumeSlider.value = initialMainMenuVol;
        backgroundGameVolumeSlider.value = initialBackgroundGameVol;
        sfxVolumeSlider.value = initialSfxVol;
        muteAllToggle.isOn = initialIsMuted;

        gameObject.SetActive(true);
        Debug.Log("IsActive? " + gameObject.active);
    }

    private void OnCloseClicked()
    {
        // revert changes (the user doesn't want to save)
        AudioManager.Instance.mainMenuVolume = initialMainMenuVol;
        AudioManager.Instance.backgroundGameVolume = initialBackgroundGameVol;
        AudioManager.Instance.sfxVolume = initialSfxVol;
        AudioManager.Instance.isMuted = initialIsMuted;

        AudioManager.Instance.RefreshVolumes();

        // hide panel
        gameObject.SetActive(false);
    }

    private void OnSaveAndCloseClicked()
    {
        // read from the sliders/toggle
        AudioManager.Instance.mainMenuVolume = mainMenuVolumeSlider.value;
        AudioManager.Instance.backgroundGameVolume = backgroundGameVolumeSlider.value;
        AudioManager.Instance.sfxVolume = sfxVolumeSlider.value;
        AudioManager.Instance.isMuted = muteAllToggle.isOn;

        AudioManager.Instance.RefreshVolumes();

        // hide
        gameObject.SetActive(false);
    }
}
