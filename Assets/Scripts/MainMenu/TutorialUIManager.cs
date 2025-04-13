using UnityEngine;
using UnityEngine.UI;
using TMPro;

/// <summary>
/// Description:
/// Manages the display and hiding of the tutorial UI panel.
/// It holds references to the main tutorial panel and its close button,
/// ensuring that the panel is hidden at startup and providing methods to show or hide the tutorial.
/// </summary>
public class TutorialUIManager : MonoBehaviour
{
    [Header("Tutorial UI References")]
    [Tooltip("The main panel that holds the tutorial (Scroll View + content)")]
    public GameObject tutorialPanel;

    [Tooltip("The close button in the tutorial panel")]
    public Button closeButton;

    /// <summary>
    /// Awake is called when the script instance is being loaded.
    /// Ensures the tutorial panel is hidden on start, and adds a listener to the close button.
    /// </summary>
    private void Awake()
    {
        // Hide the tutorial panel at startup.
        if (tutorialPanel != null)
        {
            tutorialPanel.SetActive(false);
        }

        // Add the HideTutorial method as a click listener to the close button.
        if (closeButton != null)
        {
            closeButton.onClick.AddListener(HideTutorial);
        }
    }

    /// <summary>
    /// Shows the tutorial panel.
    /// </summary>
    public void ShowTutorial()
    {
        if (tutorialPanel != null)
        {
            tutorialPanel.SetActive(true);
        }
    }

    /// <summary>
    /// Hides the tutorial panel.
    /// </summary>
    public void HideTutorial()
    {
        if (tutorialPanel != null)
        {
            tutorialPanel.SetActive(false);
        }
    }
}
