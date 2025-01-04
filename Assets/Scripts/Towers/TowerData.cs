using UnityEngine;

[CreateAssetMenu(fileName = "New Tower Data", menuName = "Tower Defense/Tower Data")]
public class TowerData : ScriptableObject
{
    public string towerName;
    public Sprite towerSprite;
    public TowerLevelData[] levels;
}

[System.Serializable]
public class TowerLevelData
{
    public int upgradeCost;
    public float range;
    public float fireRate;
    public int damage;
    public float specialInterval; //For type 2 of towers (not shooting projectiles type)
    public int specialValue; //For type 2 of towers (not shooting projectiles type)

    [Header("Effect Durations (For Freeze/Slow/Poison)")]
    public float freezeDuration;       // e.g., 2.0f
    public float slowDuration;         // e.g., 3.0f
    public float slowFactor;           // e.g., 0.5f
    public float poisonDuration;       // e.g., 2.5f
    public float poisonTickInterval;   // e.g., 0.75f
}










//Previous one - before adding tower types and effects (freeze, poison, slow):
//using UnityEngine;

//[CreateAssetMenu(fileName = "New Tower Data", menuName = "Tower Defense/Tower Data")]
//public class TowerData : ScriptableObject
//{
//    public string towerName;
//    public Sprite towerSprite;
//    public TowerLevelData[] levels; //The first item in the list (level 1) are the values used for a newly placed tower on the board, so in that case
//                                    //the "upgradeCost" is how much the tower placement costs (and not an actual upgrade)
//}

//[System.Serializable]
//public class TowerLevelData
//{
//    public int upgradeCost;
//    public float range;
//    public float fireRate;
//    public int damage;
//}
