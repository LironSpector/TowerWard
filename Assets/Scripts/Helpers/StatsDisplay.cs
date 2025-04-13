using UnityEngine;
using TMPro;

/// <summary>
/// Description:
/// Displays game statistics on-screen, such as frames per second (FPS).
/// This component updates a UI panel (via a TextMeshProUGUI component) every second
/// with the current FPS value. It also shows or hides the panel based on the global settings.
/// </summary>
public class StatsDisplay : MonoBehaviour
{
    /// <summary>
    /// The parent GameObject of the FPS display UI elements.
    /// </summary>
    public GameObject fpsPanel;

    /// <summary>
    /// The TextMeshProUGUI component that displays the FPS text.
    /// </summary>
    public TextMeshProUGUI fpsText;

    // Timer for updating FPS display.
    private float timer;

    /// <summary>
    /// Update is called once per frame.
    /// Accumulates delta time to update the FPS value roughly once per second,
    /// and toggles the visibility of the FPS panel based on GlobalSettings.StatsDisplayMode.
    /// </summary>
    void Update()
    {
        // Accumulate time to update FPS roughly every second.
        timer += Time.deltaTime;
        if (timer >= 1f)
        {
            // Calculate FPS based on current frame's delta time.
            float fps = 1f / Time.deltaTime;
            fpsText.text = "FPS: " + Mathf.RoundToInt(fps);

            // Reset timer.
            timer = 0f;
        }

        // Toggle visibility of the FPS panel based on GlobalSettings.StatsDisplayMode.
        // 0: Hide the panel, 1: Show the panel.
        switch (GlobalSettings.StatsDisplayMode)
        {
            case 0: // none
                fpsPanel.SetActive(false);
                break;
            case 1: // show fps
                fpsPanel.SetActive(true);
                break;
        }
    }
}
