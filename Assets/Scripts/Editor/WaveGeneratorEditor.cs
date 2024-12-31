using UnityEngine;
using UnityEditor;
using System.Collections.Generic;
using System.Linq;

/// <summary>
/// WaveGeneratorEditor that generates 150 waves exactly like your original code,
/// but uses a single universalBalloonPrefab + a dictionary of balloonName => health.
/// No references to old prefab fields for red/blue/etc. We only store "RedBalloon" (string).
/// </summary>
public class WaveGeneratorEditor : EditorWindow
{
    [Header("Universal Balloon Prefab")]
    public GameObject universalBalloonPrefab;

    // Dictionary that maps a balloon "name" to the max health in its range
    private static readonly Dictionary<string, int> balloonHealthMap = new Dictionary<string, int>
    {
        { "RedBalloon",           1   },
        { "BlueBalloon",          2   },
        { "GreenBalloon",         3   },
        { "YellowBalloon",        4   },
        { "PinkBalloon",          5   },
        { "BlackBalloon",         6   },
        { "WhiteBalloon",         7   },
        { "StrongBalloon",        10  }, // range 8-10 => 10
        { "StrongerBalloon",      16  }, // range 11-16 => 16
        { "VeryStrongBalloon",    26  }, // range 17-26 => 26
        { "SmallBossBalloon",     126 }, // range 27-126 => 126
        { "MediumBossBalloon",    626 }, // range 127-626 => 626
        { "BigBossBalloon",       3126}  // range 627-3126 => 3126
    };

    [MenuItem("Tools/Wave Generator")]
    public static void ShowWindow()
    {
        GetWindow<WaveGeneratorEditor>("Wave Generator");
    }

    private void OnGUI()
    {
        GUILayout.Label("Single Universal Balloon Prefab", EditorStyles.boldLabel);
        universalBalloonPrefab = (GameObject)EditorGUILayout.ObjectField("Universal Balloon Prefab",
            universalBalloonPrefab, typeof(GameObject), false);

        GUILayout.Space(10);
        if (GUILayout.Button("Generate 150 Waves"))
        {
            GenerateWaves();
        }
    }

    private void GenerateWaves()
    {
        string folderPath = "Assets/Waves";
        if (!AssetDatabase.IsValidFolder(folderPath))
        {
            AssetDatabase.CreateFolder("Assets", "Waves");
        }

        // Counters for dynamic wave generation logic, same as original
        int moreRedCount = 0, moreBlueCount = 0, moreGreenCount = 0, moreYellowCount = 0;
        int morePinkCount = 0, moreBlackCount = 0, moreWhiteCount = 0;
        int moreStrongCount = 0, moreStrongerCount = 0, moreVeryStrongCount = 0;
        int moreSmallBossCount = 0, moreMediumBossCount = 0, moreBigBossCount = 0;

        for (int i = 1; i <= 150; i++)
        {
            WaveData wave = ScriptableObject.CreateInstance<WaveData>();
            List<string> balloonNames = new List<string>();

            // EXACT same distribution logic as your old code, but using string IDs instead of prefabs:

            if (i == 1) balloonNames.AddRange(Enumerable.Repeat("RedBalloon", 5));
            else if (i == 3) balloonNames.AddRange(Enumerable.Repeat("BlueBalloon", 3));
            else if (i == 6) balloonNames.AddRange(Enumerable.Repeat("GreenBalloon", 4));
            else if (i == 10) balloonNames.AddRange(Enumerable.Repeat("YellowBalloon", 5));
            else if (i == 14) balloonNames.AddRange(Enumerable.Repeat("PinkBalloon", 5));
            else if (i == 19) balloonNames.AddRange(Enumerable.Repeat("BlackBalloon", 4)
                                                .Concat(Enumerable.Repeat("WhiteBalloon", 4)));
            else if (i == 25) balloonNames.AddRange(Enumerable.Repeat("StrongBalloon", 8));
            else if (i == 31) balloonNames.AddRange(Enumerable.Repeat("StrongerBalloon", 8));
            else if (i == 37) balloonNames.AddRange(Enumerable.Repeat("VeryStrongBalloon", 8));
            else if (i == 47) balloonNames.Add("SmallBossBalloon");
            else if (i == 57) balloonNames.Add("MediumBossBalloon");
            else if (i == 75) balloonNames.Add("BigBossBalloon");
            else if (i == 17)
                balloonNames.AddRange(Enumerable.Repeat("YellowBalloon", 5)
                                            .Concat(Enumerable.Repeat("PinkBalloon", 5)));
            else if (i == 23)
                balloonNames.AddRange(Enumerable.Repeat("GreenBalloon", 30)
                                            .Concat(Enumerable.Repeat("BlackBalloon", 4))
                                            .Concat(Enumerable.Repeat("WhiteBalloon", 4)));
            else if (i == 28)
                balloonNames.AddRange(Enumerable.Repeat("StrongBalloon", 6)
                                            .Concat(Enumerable.Repeat("PinkBalloon", 10)));
            else if (i == 33)
                balloonNames.AddRange(Enumerable.Repeat("BlueBalloon", 20)
                                            .Concat(Enumerable.Repeat("PinkBalloon", 15)));
            else if (i == 39)
                balloonNames.AddRange(Enumerable.Repeat("YellowBalloon", 40)
                                            .Concat(Enumerable.Repeat("VeryStrongBalloon", 8)));
            else if (i == 45)
                balloonNames.AddRange(Enumerable.Repeat("StrongerBalloon", 8)
                                            .Concat(Enumerable.Repeat("VeryStrongBalloon", 8)));
            else if (i == 52)
                balloonNames.AddRange(Enumerable.Repeat("VeryStrongBalloon", 15)
                                            .Concat(Enumerable.Repeat("GreenBalloon", 60)));
            else if (i == 55)
                balloonNames.AddRange(Enumerable.Repeat("BlackBalloon", 30)
                                            .Concat(Enumerable.Repeat("WhiteBalloon", 30)));
            else if (i == 60)
                balloonNames.AddRange(Enumerable.Repeat("SmallBossBalloon", 5)
                                            .Concat(Enumerable.Repeat("WhiteBalloon", 50)));
            else if (i == 67)
                balloonNames.AddRange(Enumerable.Repeat("YellowBalloon", 50)
                                            .Concat(Enumerable.Repeat("MediumBossBalloon", 5)));
            else if (i == 78)
                balloonNames.AddRange(Enumerable.Repeat("VeryStrongBalloon", 75)
                                            .Concat(Enumerable.Repeat("MediumBossBalloon", 15)));
            else if (i == 76)
                balloonNames.AddRange(Enumerable.Repeat("BigBossBalloon", 3));
            else if (i == 78)
                balloonNames.AddRange(Enumerable.Repeat("BigBossBalloon", 5));
            else if (i == 80)
                balloonNames.AddRange(Enumerable.Repeat("BigBossBalloon", 1)
                                            .Concat(Enumerable.Repeat("SmallBossBalloon", 35)));
            else if (i == 85)
                balloonNames.AddRange(Enumerable.Repeat("BigBossBalloon", 10));
            else if (i == 90)
                balloonNames.AddRange(Enumerable.Repeat("BigBossBalloon", 13)
                                            .Concat(Enumerable.Repeat("MediumBossBalloon", 35)));
            else
            {
                // Add balloons based on progression
                if (i >= 1) balloonNames.AddRange(Enumerable.Repeat("RedBalloon",
                    Mathf.Min(5 + moreRedCount++ + i / 3, 14)));
                if (i >= 3) balloonNames.AddRange(Enumerable.Repeat("BlueBalloon",
                    Mathf.Min(3 + moreBlueCount++ + i / 5, 12)));
                if (i >= 6) balloonNames.AddRange(Enumerable.Repeat("GreenBalloon",
                    Mathf.Min(2 + moreGreenCount++ + i / 7, 10)));
                if (i >= 10) balloonNames.AddRange(Enumerable.Repeat("YellowBalloon",
                    Mathf.Min(2 + moreYellowCount++ + i / 8, 8)));
                if (i >= 14) balloonNames.AddRange(Enumerable.Repeat("PinkBalloon",
                    Mathf.Min(2 + morePinkCount++ + i / 8, 8)));
                if (i >= 19)
                {
                    balloonNames.AddRange(Enumerable.Repeat("BlackBalloon",
                        Mathf.Min(1 + moreBlackCount++ + i / 8, 8)));
                    balloonNames.AddRange(Enumerable.Repeat("WhiteBalloon",
                        Mathf.Min(1 + moreWhiteCount++ + i / 8, 8)));
                }
                if (i >= 25) balloonNames.AddRange(Enumerable.Repeat("StrongBalloon",
                    Mathf.Min(1 + moreStrongCount++ + i / 8, 8)));
                if (i >= 31) balloonNames.AddRange(Enumerable.Repeat("StrongerBalloon",
                    Mathf.Min(1 + moreStrongerCount++ + i / 8, 8)));
                if (i >= 37) balloonNames.AddRange(Enumerable.Repeat("VeryStrongBalloon",
                    Mathf.Min(1 + moreVeryStrongCount++ + i / 8, 8)));
                if (i >= 47) balloonNames.AddRange(Enumerable.Repeat("SmallBossBalloon",
                    Mathf.Min(1 + moreSmallBossCount++ + i / 15, 40)));
                if (i >= 57) balloonNames.AddRange(Enumerable.Repeat("MediumBossBalloon",
                    Mathf.Min(1 + moreMediumBossCount++ + i / 20, 40)));
                if (i >= 75) balloonNames.AddRange(Enumerable.Repeat("BigBossBalloon",
                    Mathf.Min(1 + moreBigBossCount++ + i / 30, 75)));
            }

            // Shuffle
            balloonNames = balloonNames.OrderBy(x => Random.value).ToList();

            // Build the wave instructions
            float currentTime = 0f;
            foreach (string balloonName in balloonNames)
            {
                float spawnDelay = Random.Range(0.1f, 0.6f);
                currentTime += spawnDelay;

                // Look up final health from dictionary
                int mappedHealth = 1;
                if (!balloonHealthMap.TryGetValue(balloonName, out mappedHealth))
                {
                    Debug.LogWarning($"Balloon name '{balloonName}' not found in dictionary. Default to 1");
                    mappedHealth = 1;
                }

                WaveData.SpawnInstruction instruction = new WaveData.SpawnInstruction
                {
                    balloonPrefab = universalBalloonPrefab, // always the same prefab
                    spawnDelay = spawnDelay,
                    initialHealth = mappedHealth
                };

                // Add to wave
                wave.spawnInstructions.Add(instruction);
            }

            // Delay before wave begins
            if (i < 75)
            {
                wave.delayBeforeWaveBegins = 0.5f * i;
            }
            else
            {
                wave.delayBeforeWaveBegins = 0.5f * 75 + 5 * (i - 75);
            }

            // Save wave asset
            AssetDatabase.CreateAsset(wave, $"{folderPath}/Wave{i}.asset");
        }

        AssetDatabase.SaveAssets();
        AssetDatabase.Refresh();

        EditorUtility.DisplayDialog("Wave Generation Complete",
            "All 150 waves have been generated successfully using universalBalloonPrefab + health-based types.",
            "OK");
    }
}
