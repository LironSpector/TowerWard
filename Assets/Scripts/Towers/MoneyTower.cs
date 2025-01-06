using UnityEngine;

public class MoneyTower : Tower
{
    private float timer = 0f;
    private float currentInterval = 5f;   // from towerData.levels[level-1].specialInterval
    private int currentMoneyGive = 10;    // from towerData.levels[level-1].specialValue

    // Assign in Inspector - a small prefab with a money sprite + a script that floats/fades out
    public GameObject moneyFxPrefab;

    protected override void Start()
    {
        base.Start();
        ApplyLevelStats();
    }

    protected override void Update()
    {
        if (!isFullyPlaced) return;

        base.Update();

        timer += Time.deltaTime;
        if (timer >= currentInterval)
        {
            timer = 0f;
            GiveMoney();
        }
    }

    public override void ApplyLevelStats()
    {
        base.ApplyLevelStats(); // sets the range, etc. from towerData

        if (towerData != null && level <= towerData.levels.Length)
        {
            TowerLevelData lvl = towerData.levels[level - 1];
            currentInterval = lvl.specialInterval;
            currentMoneyGive = lvl.specialValue;
        }
    }

    private void GiveMoney()
    {
        // 1) Actually give the money
        GameManager.Instance.AddCurrency(currentMoneyGive);

        // 2) Spawn the money floating animation
        if (moneyFxPrefab != null)
        {
            // For example, spawn at the tower's position or slightly above
            Vector3 spawnPos = transform.position + new Vector3(0, 0.5f, 0);
            Instantiate(moneyFxPrefab, spawnPos, Quaternion.identity);
        }
    }
}
