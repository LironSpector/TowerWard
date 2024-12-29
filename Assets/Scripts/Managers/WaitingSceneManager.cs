using UnityEngine;
using UnityEngine.UI;
using TMPro;
using System.Collections;
using System;

public class WaitingSceneManager : MonoBehaviour
{
    [Header("Timer UI")] //For nice viewing in the inspector
    public TextMeshProUGUI waitingTimeText; // assign in Inspector

    [Header("Spinner UI")]
    public Image spinnerImage; // assign in Inspector
    public float spinnerSpeed = 100f; // degrees per second

    [Header("Tip UI")]
    public TextMeshProUGUI tipText; // assign in Inspector

    private float waitingSeconds = 0f;
    private bool isWaiting = true;

    void Start()
    {
        // 1) Start the timer
        waitingSeconds = 0f;
        StartCoroutine(UpdateTimer());

        // 2) Spinner will rotate each frame in Update()

        // 3) Random tip
        DisplayRandomTip();
    }

    void Update()
    {
        // 2) Continuously rotate spinner
        // If you want it rotating only while isWaiting is true:
        if (isWaiting && spinnerImage != null)
        {
            spinnerImage.transform.Rotate(0f, 0f, -spinnerSpeed * Time.deltaTime);
        }
    }

    private IEnumerator UpdateTimer()
    {
        while (isWaiting)
        {
            yield return new WaitForSeconds(1f);
            waitingSeconds += 1f;
            UpdateTimerText();
        }
    }

    private void UpdateTimerText()
    {
        // waitingSeconds is total seconds waited
        int totalSeconds = Mathf.FloorToInt(waitingSeconds);
        int minutes = totalSeconds / 60;
        int seconds = totalSeconds % 60;

        // Format mm:ss
        string timeString = string.Format("{0:00}:{1:00}", minutes, seconds);
        if (waitingTimeText != null)
        {
            waitingTimeText.text = timeString;
        }
    }

    private void DisplayRandomTip()
    {
        var tips = WaitingTips.GetMessages();
        // We'll define a separate static class or script for storing tips
        if (tips.Count > 0 && tipText != null)
        {
            int randomIndex = UnityEngine.Random.Range(0, tips.Count);
            tipText.text = tips[randomIndex];
        }
    }

    // Called if match is found or user leaves this scene
    public void StopWaiting()
    {
        isWaiting = false;
    }
}
