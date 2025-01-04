using UnityEngine;

public class DestructiveEnergyTower : Tower
{
    private float timer = 0f;
    private float currentInterval = 60f;
    private int damageToBalloons = 1;

    // If you want a quick green overlay effect
    public GameObject greenFlashFxPrefab;

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
            DoDestructiveEnergy();
        }
    }

    protected override void ApplyLevelStats()
    {
        base.ApplyLevelStats();
        if (towerData != null && level <= towerData.levels.Length)
        {
            TowerLevelData lvl = towerData.levels[level - 1];
            currentInterval = lvl.specialInterval; // 60s, 45s, 30s, etc.
            damageToBalloons = lvl.specialValue;     // typically 1 damage
        }
    }

    private void DoDestructiveEnergy()
    {
        // 1) Damage all balloons
        Balloon[] allBalloons = GameObject.FindObjectsOfType<Balloon>();
        foreach (Balloon b in allBalloons)
        {
            b.TakeDamage(damageToBalloons);
        }

        // 2) Show a quick green flash
        ShowGreenFlash();
    }

    private void ShowGreenFlash()
    {
        if (greenFlashFxPrefab != null)
        {
            // One approach is to spawn a full-screen UI overlay or effect.
            // Or a single sprite that covers the camera. 
            // We'll just instantiate a prefab that self-destructs after a moment.

            Instantiate(greenFlashFxPrefab, Vector3.zero, Quaternion.identity);
        }
    }
}
