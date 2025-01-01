using UnityEngine;

public class FPSDisplay : MonoBehaviour
{
    //I created it only for testing purposes

    private float deltaTime = 0.0f;

    //Remove the comments when FPS checking is desired
    void Update()
    {
        // Accumulate the delta time for FPS calculation
        deltaTime += (Time.deltaTime - deltaTime) * 0.1f;

        // Calculate FPS
        float fps = 1.0f / deltaTime;

        // Print FPS to the console
        //Debug.Log($"FPS: {fps:F1}");
    }


    void OnGUI()
    {
        // Display FPS on the screen
        int width = Screen.width, height = Screen.height;

        GUIStyle style = new GUIStyle();

        Rect rect = new Rect(0, 0, width, height * 2 / 100);
        style.alignment = TextAnchor.UpperLeft;
        style.fontSize = height * 2 / 100;
        style.normal.textColor = Color.white;
        float fps = 1.0f / deltaTime;
        string text = $"FPS: {fps:F1}";
        GUI.Label(rect, text, style);
    }
}
