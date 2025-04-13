using UnityEngine;
using UnityEngine.UI;
using TMPro;

public class TutorialUIManager : MonoBehaviour
{
    [Header("Tutorial UI References")]
    [Tooltip("The main panel that holds the tutorial (Scroll View + content)")]
    public GameObject tutorialPanel;

    [Tooltip("The close button in the tutorial panel")]
    public Button closeButton;

    private void Awake()
    {
        // Ensure the panel is hidden on start
        if (tutorialPanel != null)
        {
            tutorialPanel.SetActive(false);
        }

        if (closeButton != null)
        {
            closeButton.onClick.AddListener(HideTutorial);
        }
    }

    /// <summary>
    /// Show the tutorial panel.
    /// </summary>
    public void ShowTutorial()
    {
        if (tutorialPanel != null)
        {
            tutorialPanel.SetActive(true);
        }
    }

    /// <summary>
    /// Hide the tutorial panel.
    /// </summary>
    public void HideTutorial()
    {
        if (tutorialPanel != null)
        {
            tutorialPanel.SetActive(false);
        }
    }
}
