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
    public GameObject smallBossBalloonPrefab;
    public GameObject mediumBossBalloonPrefab;
    public GameObject bigBossBalloonPrefab;

    [MenuItem("Tools/Wave Generator")]
    public static void ShowWindow()
    {
        GetWindow<WaveGeneratorEditor>("Wave Generator");
    }

    private void OnGUI()
    {
        GUILayout.Label("Balloon Prefabs", EditorStyles.boldLabel);

        // Assign balloon prefabs through the editor
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
        smallBossBalloonPrefab = (GameObject)EditorGUILayout.ObjectField("Small Boss Balloon", smallBossBalloonPrefab, typeof(GameObject), false);
        mediumBossBalloonPrefab = (GameObject)EditorGUILayout.ObjectField("Medium Boss Balloon", mediumBossBalloonPrefab, typeof(GameObject), false);
        bigBossBalloonPrefab = (GameObject)EditorGUILayout.ObjectField("Big Boss Balloon", bigBossBalloonPrefab, typeof(GameObject), false);

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

        // Individual counters for each balloon type
        int moreRedCount = 0, moreBlueCount = 0, moreGreenCount = 0, moreYellowCount = 0;
        int morePinkCount = 0, moreBlackCount = 0, moreWhiteCount = 0;
        int moreStrongCount = 0, moreStrongerCount = 0, moreVeryStrongCount = 0;
        int moreSmallBossCount = 0, moreMediumBossCount = 0, moreBigBossCount = 0;

        for (int i = 1; i <= 150; i++)
        {
            WaveData wave = ScriptableObject.CreateInstance<WaveData>();
            List<GameObject> balloons = new List<GameObject>();

            // Unique waves for new balloon types
            if (i == 1) balloons.AddRange(Enumerable.Repeat(redBalloonPrefab, 5));
            else if (i == 3) balloons.AddRange(Enumerable.Repeat(blueBalloonPrefab, 3));
            else if (i == 6) balloons.AddRange(Enumerable.Repeat(greenBalloonPrefab, 4));
            else if (i == 10) balloons.AddRange(Enumerable.Repeat(yellowBalloonPrefab, 5));
            else if (i == 14) balloons.AddRange(Enumerable.Repeat(pinkBalloonPrefab, 5));
            else if (i == 19) balloons.AddRange(Enumerable.Repeat(blackBalloonPrefab, 4).Concat(Enumerable.Repeat(whiteBalloonPrefab, 4)));
            else if (i == 25) balloons.AddRange(Enumerable.Repeat(strongBalloonPrefab, 8));
            else if (i == 31) balloons.AddRange(Enumerable.Repeat(strongerBalloonPrefab, 8));
            else if (i == 37) balloons.AddRange(Enumerable.Repeat(veryStrongBalloonPrefab, 8));
            else if (i == 47) balloons.Add(smallBossBalloonPrefab);
            else if (i == 57) balloons.Add(mediumBossBalloonPrefab);
            else if (i == 75) balloons.Add(bigBossBalloonPrefab);
            else if (i == 17)
                balloons.AddRange(Enumerable.Repeat(yellowBalloonPrefab, 5).Concat(Enumerable.Repeat(pinkBalloonPrefab, 5)));
            else if (i == 23)
                balloons.AddRange(Enumerable.Repeat(greenBalloonPrefab, 30).Concat(Enumerable.Repeat(blackBalloonPrefab, 4)).Concat(Enumerable.Repeat(whiteBalloonPrefab, 4)));
            else if (i == 28)
                balloons.AddRange(Enumerable.Repeat(strongBalloonPrefab, 6).Concat(Enumerable.Repeat(pinkBalloonPrefab, 10)));
            else if (i == 33)
                balloons.AddRange(Enumerable.Repeat(blueBalloonPrefab, 20).Concat(Enumerable.Repeat(pinkBalloonPrefab, 15)));
            else if (i == 39)
                balloons.AddRange(Enumerable.Repeat(yellowBalloonPrefab, 40).Concat(Enumerable.Repeat(veryStrongBalloonPrefab, 8)));
            else if (i == 45)
                balloons.AddRange(Enumerable.Repeat(strongerBalloonPrefab, 8).Concat(Enumerable.Repeat(veryStrongBalloonPrefab, 8)));
            else if (i == 52)
                balloons.AddRange(Enumerable.Repeat(veryStrongBalloonPrefab, 15).Concat(Enumerable.Repeat(greenBalloonPrefab, 60)));
            else if (i == 55)
                balloons.AddRange(Enumerable.Repeat(blackBalloonPrefab, 30).Concat(Enumerable.Repeat(whiteBalloonPrefab, 30)));
            else if (i == 60)
                balloons.AddRange(Enumerable.Repeat(smallBossBalloonPrefab, 5).Concat(Enumerable.Repeat(whiteBalloonPrefab, 50)));
            else if (i == 67)
                balloons.AddRange(Enumerable.Repeat(yellowBalloonPrefab, 50).Concat(Enumerable.Repeat(mediumBossBalloonPrefab, 5)));
            else if (i == 78)
                balloons.AddRange(Enumerable.Repeat(veryStrongBalloonPrefab, 75).Concat(Enumerable.Repeat(mediumBossBalloonPrefab, 15)));
            else if (i == 76)
                balloons.AddRange(Enumerable.Repeat(bigBossBalloonPrefab, 3));
            else if (i == 78)
                balloons.AddRange(Enumerable.Repeat(bigBossBalloonPrefab, 5));
            else if (i == 80)
                balloons.AddRange(Enumerable.Repeat(bigBossBalloonPrefab, 1).Concat(Enumerable.Repeat(smallBossBalloonPrefab, 35)));
            else if (i == 85)
                balloons.AddRange(Enumerable.Repeat(bigBossBalloonPrefab, 10));
            else if (i == 90)
                balloons.AddRange(Enumerable.Repeat(bigBossBalloonPrefab, 13).Concat(Enumerable.Repeat(mediumBossBalloonPrefab, 35)));
            else
            {
                // Add balloons based on progression
                if (i >= 1) balloons.AddRange(Enumerable.Repeat(redBalloonPrefab, Mathf.Min(5 + moreRedCount++ + i / 3, 14)));
                if (i >= 3) balloons.AddRange(Enumerable.Repeat(blueBalloonPrefab, Mathf.Min(3 + moreBlueCount++ + i / 5, 12)));
                if (i >= 6) balloons.AddRange(Enumerable.Repeat(greenBalloonPrefab, Mathf.Min(2 + moreGreenCount++ + i / 7, 10)));
                if (i >= 10) balloons.AddRange(Enumerable.Repeat(yellowBalloonPrefab, Mathf.Min(2 + moreYellowCount++ + i / 8, 8)));
                if (i >= 14) balloons.AddRange(Enumerable.Repeat(pinkBalloonPrefab, Mathf.Min(2 + morePinkCount++ + i / 8, 8)));
                if (i >= 19)
                {
                    balloons.AddRange(Enumerable.Repeat(blackBalloonPrefab, Mathf.Min(1 + moreBlackCount++ + i / 8, 8)));
                    balloons.AddRange(Enumerable.Repeat(whiteBalloonPrefab, Mathf.Min(1 + moreWhiteCount++ + i / 8, 8)));
                }
                if (i >= 25) balloons.AddRange(Enumerable.Repeat(strongBalloonPrefab, Mathf.Min(1 + moreStrongCount++ + i / 8, 8)));
                if (i >= 31) balloons.AddRange(Enumerable.Repeat(strongerBalloonPrefab, Mathf.Min(1 + moreStrongerCount++ + i / 8, 8)));
                if (i >= 37) balloons.AddRange(Enumerable.Repeat(veryStrongBalloonPrefab, Mathf.Min(1 + moreVeryStrongCount++ + i / 8, 8)));
                if (i >= 47) balloons.AddRange(Enumerable.Repeat(smallBossBalloonPrefab, Mathf.Min(1 + moreSmallBossCount++ + i / 15, 40)));
                if (i >= 57) balloons.AddRange(Enumerable.Repeat(mediumBossBalloonPrefab, Mathf.Min(1 + moreMediumBossCount++ + i / 20, 40)));
                if (i >= 75) balloons.AddRange(Enumerable.Repeat(bigBossBalloonPrefab, Mathf.Min(1 + moreBigBossCount++ + i / 30, 75)));
            }

            // Shuffle balloons
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

        EditorUtility.DisplayDialog("Wave Generation Complete", "All 150 waves have been generated successfully.", "OK");
    }
}






// ------------------ Previous WaveGeneratorEditor: ------------------
//using UnityEngine;
//using UnityEditor;
//using System.Collections.Generic;
//using System.Linq;

//public class WaveGeneratorEditor : EditorWindow
//{
//    // Balloon prefabs
//    public GameObject redBalloonPrefab;
//    public GameObject blueBalloonPrefab;
//    public GameObject greenBalloonPrefab;
//    public GameObject yellowBalloonPrefab;
//    public GameObject pinkBalloonPrefab;
//    public GameObject blackBalloonPrefab;
//    public GameObject whiteBalloonPrefab;
//    public GameObject strongBalloonPrefab;
//    public GameObject strongerBalloonPrefab;
//    public GameObject veryStrongBalloonPrefab;

//    [MenuItem("Tools/Wave Generator")]
//    public static void ShowWindow()
//    {
//        GetWindow<WaveGeneratorEditor>("Wave Generator");
//    }

//    private void OnGUI()
//    {
//        GUILayout.Label("Balloon Prefabs", EditorStyles.boldLabel);

//        redBalloonPrefab = (GameObject)EditorGUILayout.ObjectField("Red Balloon", redBalloonPrefab, typeof(GameObject), false);
//        blueBalloonPrefab = (GameObject)EditorGUILayout.ObjectField("Blue Balloon", blueBalloonPrefab, typeof(GameObject), false);
//        greenBalloonPrefab = (GameObject)EditorGUILayout.ObjectField("Green Balloon", greenBalloonPrefab, typeof(GameObject), false);
//        yellowBalloonPrefab = (GameObject)EditorGUILayout.ObjectField("Yellow Balloon", yellowBalloonPrefab, typeof(GameObject), false);
//        pinkBalloonPrefab = (GameObject)EditorGUILayout.ObjectField("Pink Balloon", pinkBalloonPrefab, typeof(GameObject), false);
//        blackBalloonPrefab = (GameObject)EditorGUILayout.ObjectField("Black Balloon", blackBalloonPrefab, typeof(GameObject), false);
//        whiteBalloonPrefab = (GameObject)EditorGUILayout.ObjectField("White Balloon", whiteBalloonPrefab, typeof(GameObject), false);
//        strongBalloonPrefab = (GameObject)EditorGUILayout.ObjectField("Strong Balloon", strongBalloonPrefab, typeof(GameObject), false);
//        strongerBalloonPrefab = (GameObject)EditorGUILayout.ObjectField("Stronger Balloon", strongerBalloonPrefab, typeof(GameObject), false);
//        veryStrongBalloonPrefab = (GameObject)EditorGUILayout.ObjectField("Very Strong Balloon", veryStrongBalloonPrefab, typeof(GameObject), false);

//        if (GUILayout.Button("Generate Waves"))
//        {
//            GenerateWaves();
//        }
//    }

//    void GenerateWaves()
//    {
//        string folderPath = "Assets/Waves";
//        if (!AssetDatabase.IsValidFolder(folderPath))
//        {
//            AssetDatabase.CreateFolder("Assets", "Waves");
//        }

//        for (int i = 1; i <= 40; i++)
//        {
//            WaveData wave = ScriptableObject.CreateInstance<WaveData>();
//            List<GameObject> balloons = new List<GameObject>();

//            // Add balloons based on wave difficulty level
//            if (i >= 1)
//            {
//                int redCount = Mathf.Min(5 + i / 3, 14);
//                balloons.AddRange(Enumerable.Repeat(redBalloonPrefab, redCount));
//            }
//            if (i >= 3)
//            {
//                int blueCount = Mathf.Min(3 + i / 5, 12);
//                balloons.AddRange(Enumerable.Repeat(blueBalloonPrefab, blueCount));
//            }
//            if (i >= 5)
//            {
//                int greenCount = Mathf.Min(2 + i / 7, 10);
//                balloons.AddRange(Enumerable.Repeat(greenBalloonPrefab, greenCount));
//            }
//            if (i >= 8)
//            {
//                int yellowCount = Mathf.Min(2 + i / 8, 8);
//                balloons.AddRange(Enumerable.Repeat(yellowBalloonPrefab, yellowCount));
//            }
//            if (i >= 12)
//            {
//                int pinkCount = Mathf.Min(2 + i / 8, 8);
//                balloons.AddRange(Enumerable.Repeat(pinkBalloonPrefab, pinkCount));
//            }
//            if (i >= 15)
//            {
//                int blackCount = Mathf.Min(1 + i / 8, 8);
//                balloons.AddRange(Enumerable.Repeat(blackBalloonPrefab, blackCount));
//                int whiteCount = Mathf.Min(1 + i / 8, 8);
//                balloons.AddRange(Enumerable.Repeat(whiteBalloonPrefab, whiteCount));
//            }
//            if (i >= 20)
//            {
//                int strongCount = Mathf.Min(1 + i / 8, 6);
//                balloons.AddRange(Enumerable.Repeat(strongBalloonPrefab, strongCount));
//            }
//            if (i >= 25)
//            {
//                int strongerCount = Mathf.Min(1 + i / 8, 6);
//                balloons.AddRange(Enumerable.Repeat(strongerBalloonPrefab, strongerCount));
//            }
//            if (i >= 30)
//            {
//                int veryStrongCount = Mathf.Min(1 + i / 8, 6);
//                balloons.AddRange(Enumerable.Repeat(veryStrongBalloonPrefab, veryStrongCount));
//            }

//            // Shuffle the balloons
//            balloons = balloons.OrderBy(x => Random.value).ToList();

//            // Generate spawn instructions
//            float currentTime = 0f;
//            foreach (var balloonPrefab in balloons)
//            {
//                float spawnDelay = Random.Range(0.1f, 0.6f);
//                currentTime += spawnDelay;
//                wave.spawnInstructions.Add(new WaveData.SpawnInstruction
//                {
//                    balloonPrefab = balloonPrefab,
//                    spawnDelay = spawnDelay
//                });
//            }

//            // Save the WaveData asset
//            AssetDatabase.CreateAsset(wave, $"{folderPath}/Wave{i}.asset");
//        }

//        AssetDatabase.SaveAssets();
//        AssetDatabase.Refresh();

//        EditorUtility.DisplayDialog("Wave Generation Complete", "All waves have been generated successfully.", "OK");
//    }

//}
