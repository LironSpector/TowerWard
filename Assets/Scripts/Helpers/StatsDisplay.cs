using UnityEngine;
using TMPro;

public class StatsDisplay : MonoBehaviour
{
    public GameObject fpsPanel;       // parent or TMP text that shows FPS
    public TextMeshProUGUI fpsText;   // the actual text component

    private float timer;

    void Update()
    {
        // Update FPS ~ once per second or so
        timer += Time.deltaTime;
        if (timer >= 1f)
        {
            float fps = 1f / Time.deltaTime; // or compute average
            fpsText.text = "FPS: " + Mathf.RoundToInt(fps);

            timer = 0f;
        }

        // Show/hide based on GlobalSettings.StatsDisplayMode
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
