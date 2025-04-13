using UnityEngine;
using UnityEngine.UI;
using TMPro;
using System.Collections;
using System;

/// <summary>
/// Description:
/// Manages the UI in the Waiting Scene that appears while a multiplayer match is being set up.
/// It handles the display of a waiting timer, continuously rotates a spinner for visual feedback,
/// and shows a random tip to the player. The waiting process can be stopped externally when a match is found
/// or if the user exits the scene.
/// </summary>
public class WaitingSceneManager : MonoBehaviour
{
    [Header("Timer UI")]
    /// <summary>
    /// Text element that displays the waiting time in minutes and seconds.
    /// </summary>
    public TextMeshProUGUI waitingTimeText; // assign in Inspector

    [Header("Spinner UI")]
    /// <summary>
    /// Image representing a spinner that rotates while waiting.
    /// </summary>
    public Image spinnerImage; // assign in Inspector
    /// <summary>
    /// Rotation speed of the spinner in degrees per second.
    /// </summary>
    public float spinnerSpeed = 100f;

    [Header("Tip UI")]
    /// <summary>
    /// Text element that displays a random tip to the user.
    /// </summary>
    public TextMeshProUGUI tipText; // assign in Inspector

    // Internal tracking of waiting time and state.
    private float waitingSeconds = 0f;
    private bool isWaiting = true;

    /// <summary>
    /// Start is called on the frame when the script is enabled.
    /// Initializes the waiting timer, starts the timer update coroutine, and displays a random tip.
    /// </summary>
    void Start()
    {
        // Initialize waiting time and start timer coroutine.
        waitingSeconds = 0f;
        StartCoroutine(UpdateTimer());

        // Spinner rotation is handled in the Update method.

        // Display a random tip in the UI.
        DisplayRandomTip();
    }

    /// <summary>
    /// Update is called once per frame.
    /// Continuously rotates the spinner image if waiting is active.
    /// </summary>
    void Update()
    {
        if (isWaiting && spinnerImage != null)
        {
            spinnerImage.transform.Rotate(0f, 0f, -spinnerSpeed * Time.deltaTime);
        }
    }

    /// <summary>
    /// Coroutine that updates the waiting timer once per second.
    /// Increments the waiting time and updates the displayed timer text accordingly.
    /// </summary>
    /// <returns>An IEnumerator required for coroutine execution.</returns>
    private IEnumerator UpdateTimer()
    {
        while (isWaiting)
        {
            yield return new WaitForSeconds(1f);
            waitingSeconds += 1f;
            UpdateTimerText();
        }
    }

    /// <summary>
    /// Updates the waiting time text UI element, formatting the time as mm:ss.
    /// </summary>
    private void UpdateTimerText()
    {
        int totalSeconds = Mathf.FloorToInt(waitingSeconds);
        int minutes = totalSeconds / 60;
        int seconds = totalSeconds % 60;
        string timeString = string.Format("{0:00}:{1:00}", minutes, seconds);

        if (waitingTimeText != null)
        {
            waitingTimeText.text = "Waiting for: " + timeString;
        }
    }

    /// <summary>
    /// Retrieves a list of tip messages from a separate tips provider and displays one randomly in the UI.
    /// </summary>
    private void DisplayRandomTip()
    {
        var tips = WaitingTips.GetMessages();
        if (tips.Count > 0 && tipText != null)
        {
            int randomIndex = UnityEngine.Random.Range(0, tips.Count);
            tipText.text = tips[randomIndex];
        }
    }

    /// <summary>
    /// Stops the waiting process by setting the waiting flag to false.
    /// This method is called externally when a match is found or the user exits the scene.
    /// </summary>
    public void StopWaiting()
    {
        isWaiting = false;
    }
}
