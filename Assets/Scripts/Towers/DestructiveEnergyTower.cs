using UnityEngine;

/// <summary>
/// Description:
/// Represents a Destructive Energy Tower that periodically damages all balloons on the map.
/// This tower triggers a destructive energy burst at set intervals (configured per level via TowerData),
/// dealing a specified amount of damage to every balloon in the scene. In addition, a quick green flash visual
/// effect is displayed when the burst occurs.
/// </summary>
public class DestructiveEnergyTower : Tower
{
    /// <summary>
    /// Timer to track the elapsed time since the last destructive burst.
    /// </summary>
    private float timer = 0f;

    /// <summary>
    /// The current interval (in seconds) between successive destructive bursts,
    /// set from TowerData.levels[level - 1].specialInterval.
    /// </summary>
    private float currentInterval = 60f;

    /// <summary>
    /// The damage dealt to each balloon during a destructive burst,
    /// set from TowerData.levels[level - 1].specialValue.
    /// </summary>
    private int damageToBalloons = 1;

    /// <summary>
    /// Prefab for the green flash effect shown when the destructive burst is activated.
    /// </summary>
    public GameObject greenFlashFxPrefab;

    /// <summary>
    /// Initializes the tower by calling the base Start method and applying level-specific stats.
    /// </summary>
    protected override void Start()
    {
        base.Start();
        ApplyLevelStats();
    }

    /// <summary>
    /// Called once per frame.
    /// Updates the internal timer and, when the timer exceeds the current interval,
    /// resets the timer and triggers the destructive energy burst.
    /// </summary>
    protected override void Update()
    {
        if (!isFullyPlaced)
            return;

        base.Update();

        timer += Time.deltaTime;
        if (timer >= currentInterval)
        {
            timer = 0f;
            DoDestructiveEnergy();
        }
    }

    /// <summary>
    /// Applies level-specific statistics to the tower by updating the current interval and damage values.
    /// These values are retrieved from the TowerData for the current level.
    /// </summary>
    public override void ApplyLevelStats()
    {
        base.ApplyLevelStats();
        if (towerData != null && level <= towerData.levels.Length)
        {
            TowerLevelData lvl = towerData.levels[level - 1];
            currentInterval = lvl.specialInterval; // e.g., 60s, 45s, 30s, etc.
            damageToBalloons = lvl.specialValue;     // Typically 1 damage
        }
    }

    /// <summary>
    /// Performs the destructive energy burst.
    /// Damages all balloons in the scene by dealing the specified damage and then triggers a green flash effect.
    /// </summary>
    private void DoDestructiveEnergy()
    {
        // Damage all balloons in the scene.
        Balloon[] allBalloons = GameObject.FindObjectsOfType<Balloon>();
        foreach (Balloon b in allBalloons)
        {
            b.TakeDamage(damageToBalloons);
        }

        // Display the green flash effect.
        ShowGreenFlash();
    }

    /// <summary>
    /// Instantiates the green flash effect prefab at the origin.
    /// The prefab is expected to handle its own lifetime.
    /// </summary>
    private void ShowGreenFlash()
    {
        if (greenFlashFxPrefab != null)
        {
            Instantiate(greenFlashFxPrefab, Vector3.zero, Quaternion.identity);
        }
    }
}
