using UnityEngine;

/// <summary>
/// Represents the data for a tower in the game. This includes general tower information such as its name
/// and default sprite, as well as an array of level-specific data detailing the tower's stats and upgrade parameters.
/// This ScriptableObject can be created from the Unity Editor via the "Tower Defense/Tower Data" menu item.
/// </summary>
[CreateAssetMenu(fileName = "New Tower Data", menuName = "Tower Defense/Tower Data")]
public class TowerData : ScriptableObject
{
    /// <summary>
    /// The name of the tower.
    /// </summary>
    public string towerName;

    /// <summary>
    /// The default sprite representing the tower.
    /// </summary>
    public Sprite towerSprite;

    /// <summary>
    /// An array of level-specific data that defines the tower's stats, upgrade cost, and special effect durations at each level.
    /// </summary>
    public TowerLevelData[] levels;
}

/// <summary>
/// Contains the data specific to an individual level of a tower.
/// This includes parameters such as upgrade cost, range, fire rate, damage, and effect durations for abilities
/// like freeze, slow, and poison. It also includes a level-specific sprite for visual differentiation.
/// </summary>
[System.Serializable]
public class TowerLevelData
{
    /// <summary>
    /// The cost required to upgrade the tower to this level.
    /// </summary>
    public int upgradeCost;

    /// <summary>
    /// The effective range of the tower at this level.
    /// </summary>
    public float range;

    /// <summary>
    /// The fire rate (shots per second) of the tower at this level.
    /// </summary>
    public float fireRate;

    /// <summary>
    /// The damage dealt by the tower's attacks at this level.
    /// </summary>
    public int damage;

    /// <summary>
    /// The interval for executing a special ability for towers that do not use projectile attacks.
    /// </summary>
    public float specialInterval;

    /// <summary>
    /// The special value associated with the tower's special ability.
    /// </summary>
    public int specialValue;

    [Header("Effect Durations (For Freeze/Slow/Poison)")]
    /// <summary>
    /// The duration of the freeze effect applied by the tower.
    /// </summary>
    public float freezeDuration;       // e.g., 2.0f

    /// <summary>
    /// The duration of the slow effect applied by the tower.
    /// </summary>
    public float slowDuration;         // e.g., 3.0f

    /// <summary>
    /// The factor by which the tower slows its target.
    /// </summary>
    public float slowFactor;           // e.g., 0.5f

    /// <summary>
    /// The duration of the poison effect applied by the tower.
    /// </summary>
    public float poisonDuration;       // e.g., 2.5f

    /// <summary>
    /// The interval between consecutive poison damage ticks.
    /// </summary>
    public float poisonTickInterval;   // e.g., 0.75f

    [Header("Level-specific sprite")]
    /// <summary>
    /// The sprite representing the tower at this specific level.
    /// </summary>
    public Sprite towerLevelSprite;
}
