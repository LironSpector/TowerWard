using UnityEngine;

/// <summary>
/// Description:
/// A simple script for testing purposes that calculates and displays the current frames per second (FPS) on screen.
/// The FPS is computed by smoothing the time delta between frames, and the result is drawn using OnGUI.
/// </summary>
public class FPSDisplay : MonoBehaviour
{
    /// <summary>
    /// The smoothed delta time used for calculating FPS.
    /// </summary>
    private float deltaTime = 0.0f;

    /// <summary>
    /// Called once per frame.
    /// Smooths out the delta time to calculate a more stable FPS.
    /// </summary>
    void Update()
    {
        if (GameManager.Instance.isGameOver) return;

        // Smooth the delta time using an exponential moving average.
        deltaTime += (Time.deltaTime - deltaTime) * 0.1f;

        // Calculate FPS: Uncomment the line below to print FPS to the console for debugging.
        // float fps = 1.0f / deltaTime;
        // Debug.Log($"FPS: {fps:F1}");
    }

    /// <summary>
    /// Called for rendering and handling GUI events.
    /// Displays the FPS as a label in the upper-left corner of the screen.
    /// </summary>
    void OnGUI()
    {
        if (GameManager.Instance.isGameOver)
            return;

        // Get the current screen dimensions.
        int width = Screen.width, height = Screen.height;

        // Create a GUI style for the FPS display.
        GUIStyle style = new GUIStyle();
        Rect rect = new Rect(0, 0, width, height * 2 / 100);
        style.alignment = TextAnchor.UpperLeft;
        style.fontSize = height * 2 / 100;
        style.normal.textColor = Color.white;

        // Compute the current FPS.
        float fps = 1.0f / deltaTime;
        string text = $"FPS: {fps:F1}";

        // Render the FPS label on screen.
        GUI.Label(rect, text, style);
    }
}
