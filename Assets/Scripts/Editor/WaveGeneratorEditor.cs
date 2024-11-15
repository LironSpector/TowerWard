using UnityEngine;
using UnityEditor;
using System.Collections.Generic;
using System.Linq;

public class WaveGeneratorEditor : EditorWindow
{
    // Balloon prefabs
    public GameObject redBalloonPrefab;
    public GameObject blueBalloonPrefab;
    public GameObject greenBalloonPrefab;
    public GameObject yellowBalloonPrefab;
    public GameObject pinkBalloonPrefab;
    public GameObject blackBalloonPrefab;
    public GameObject whiteBalloonPrefab;
    public GameObject strongBalloonPrefab;
    public GameObject strongerBalloonPrefab;
    public GameObject veryStrongBalloonPrefab;

    [MenuItem("Tools/Wave Generator")]
    public static void ShowWindow()
    {
        GetWindow<WaveGeneratorEditor>("Wave Generator");
    }

    private void OnGUI()
    {
        GUILayout.Label("Balloon Prefabs", EditorStyles.boldLabel);

        redBalloonPrefab = (GameObject)EditorGUILayout.ObjectField("Red Balloon", redBalloonPrefab, typeof(GameObject), false);
        blueBalloonPrefab = (GameObject)EditorGUILayout.ObjectField("Blue Balloon", blueBalloonPrefab, typeof(GameObject), false);
        greenBalloonPrefab = (GameObject)EditorGUILayout.ObjectField("Green Balloon", greenBalloonPrefab, typeof(GameObject), false);
        yellowBalloonPrefab = (GameObject)EditorGUILayout.ObjectField("Yellow Balloon", yellowBalloonPrefab, typeof(GameObject), false);
        pinkBalloonPrefab = (GameObject)EditorGUILayout.ObjectField("Pink Balloon", pinkBalloonPrefab, typeof(GameObject), false);
        blackBalloonPrefab = (GameObject)EditorGUILayout.ObjectField("Black Balloon", blackBalloonPrefab, typeof(GameObject), false);
        whiteBalloonPrefab = (GameObject)EditorGUILayout.ObjectField("White Balloon", whiteBalloonPrefab, typeof(GameObject), false);
        strongBalloonPrefab = (GameObject)EditorGUILayout.ObjectField("Strong Balloon", strongBalloonPrefab, typeof(GameObject), false);
        strongerBalloonPrefab = (GameObject)EditorGUILayout.ObjectField("Stronger Balloon", strongerBalloonPrefab, typeof(GameObject), false);
        veryStrongBalloonPrefab = (GameObject)EditorGUILayout.ObjectField("Very Strong Balloon", veryStrongBalloonPrefab, typeof(GameObject), false);

        if (GUILayout.Button("Generate Waves"))
        {
            GenerateWaves();
        }
    }

    void GenerateWaves()
    {
        string folderPath = "Assets/Waves";
        if (!AssetDatabase.IsValidFolder(folderPath))
        {
            AssetDatabase.CreateFolder("Assets", "Waves");
        }

        for (int i = 1; i <= 40; i++)
        {
            WaveData wave = ScriptableObject.CreateInstance<WaveData>();
            List<GameObject> balloons = new List<GameObject>();

            // Add balloons based on wave difficulty level
            if (i >= 1)
            {
                int redCount = Mathf.Min(5 + i / 3, 14);
                balloons.AddRange(Enumerable.Repeat(redBalloonPrefab, redCount));
            }
            if (i >= 3)
            {
                int blueCount = Mathf.Min(3 + i / 5, 12);
                balloons.AddRange(Enumerable.Repeat(blueBalloonPrefab, blueCount));
            }
            if (i >= 5)
            {
                int greenCount = Mathf.Min(2 + i / 7, 10);
                balloons.AddRange(Enumerable.Repeat(greenBalloonPrefab, greenCount));
            }
            if (i >= 8)
            {
                int yellowCount = Mathf.Min(2 + i / 8, 8);
                balloons.AddRange(Enumerable.Repeat(yellowBalloonPrefab, yellowCount));
            }
            if (i >= 12)
            {
                int pinkCount = Mathf.Min(2 + i / 8, 8);
                balloons.AddRange(Enumerable.Repeat(pinkBalloonPrefab, pinkCount));
            }
            if (i >= 15)
            {
                int blackCount = Mathf.Min(1 + i / 8, 8);
                balloons.AddRange(Enumerable.Repeat(blackBalloonPrefab, blackCount));
                int whiteCount = Mathf.Min(1 + i / 8, 8);
                balloons.AddRange(Enumerable.Repeat(whiteBalloonPrefab, whiteCount));
            }
            if (i >= 20)
            {
                int strongCount = Mathf.Min(1 + i / 8, 6);
                balloons.AddRange(Enumerable.Repeat(strongBalloonPrefab, strongCount));
            }
            if (i >= 25)
            {
                int strongerCount = Mathf.Min(1 + i / 8, 6);
                balloons.AddRange(Enumerable.Repeat(strongerBalloonPrefab, strongerCount));
            }
            if (i >= 30)
            {
                int veryStrongCount = Mathf.Min(1 + i / 8, 6);
                balloons.AddRange(Enumerable.Repeat(veryStrongBalloonPrefab, veryStrongCount));
            }

            // Shuffle the balloons
            balloons = balloons.OrderBy(x => Random.value).ToList();

            // Generate spawn instructions
            float currentTime = 0f;
            foreach (var balloonPrefab in balloons)
            {
                float spawnDelay = Random.Range(0.1f, 0.6f);
                currentTime += spawnDelay;
                wave.spawnInstructions.Add(new WaveData.SpawnInstruction
                {
                    balloonPrefab = balloonPrefab,
                    spawnDelay = spawnDelay
                });
            }

            // Save the WaveData asset
            AssetDatabase.CreateAsset(wave, $"{folderPath}/Wave{i}.asset");
        }

        AssetDatabase.SaveAssets();
        AssetDatabase.Refresh();

        EditorUtility.DisplayDialog("Wave Generation Complete", "All waves have been generated successfully.", "OK");
    }

}
