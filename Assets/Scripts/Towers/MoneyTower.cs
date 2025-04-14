using UnityEngine;

/// <summary>
/// Represents a Money Tower that periodically adds currency to the player's total.
/// The tower gives money at intervals defined by its level-specific data, and upon giving money,
/// it spawns a visual money effect to provide feedback to the player.
/// </summary>
public class MoneyTower : Tower
{
    /// <summary>
    /// Accumulates the elapsed time since the last money reward.
    /// </summary>
    private float timer = 0f;

    /// <summary>
    /// The interval (in seconds) between money rewards, as defined in the tower's level-specific data.
    /// </summary>
    private float currentInterval = 5f;

    /// <summary>
    /// The amount of currency given to the player each time the reward is triggered.
    /// This value is defined in the tower's level-specific data.
    /// </summary>
    private int currentMoneyGive = 10;

    /// <summary>
    /// A prefab that represents the money effect (e.g., a floating and fading money sprite).
    /// This should be assigned in the Inspector.
    /// </summary>
    public GameObject moneyFxPrefab;

    /// <summary>
    /// Initializes the Money Tower by calling the base Start method and applying level-specific stats.
    /// </summary>
    protected override void Start()
    {
        base.Start();
        ApplyLevelStats();
    }

    /// <summary>
    /// Called once per frame.
    /// If the tower is fully placed, updates the timer. When the timer reaches the current interval,
    /// it resets the timer and triggers the money reward.
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
            GiveMoney();
        }
    }

    /// <summary>
    /// Applies level-specific statistics to the Money Tower.
    /// This includes setting the money reward interval and the amount of currency to give,
    /// based on the values defined in TowerData for the current level.
    /// </summary>
    public override void ApplyLevelStats()
    {
        base.ApplyLevelStats();

        if (towerData != null && level <= towerData.levels.Length)
        {
            TowerLevelData lvl = towerData.levels[level - 1];
            currentInterval = lvl.specialInterval;
            currentMoneyGive = lvl.specialValue;
        }
    }

    /// <summary>
    /// Gives money to the player by adding currency to their total,
    /// and spawns a money effect animation for visual feedback.
    /// </summary>
    private void GiveMoney()
    {
        // 1) Add the reward to the player's currency.
        GameManager.Instance.AddCurrency(currentMoneyGive);

        // 2) Spawn the money effect animation (if available) slightly above the tower.
        if (moneyFxPrefab != null)
        {
            Vector3 spawnPos = transform.position + new Vector3(0, 0.5f, 0);
            Instantiate(moneyFxPrefab, spawnPos, Quaternion.identity);
        }
    }
}
