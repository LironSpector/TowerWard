using UnityEngine;

/// <summary>
/// ProjectileTower inherits from the base Tower and adds the logic for shooting projectiles.
/// </summary>
public class ProjectileTower : Tower
{
    protected BaseTowerShooting towerShooting; // Moved here from the old base Tower

    protected override void Start()
    {
        base.Start();

        // Now we initialize the shooting reference
        towerShooting = GetComponent<BaseTowerShooting>();
        // If there's a circle collider for range, we already got it in Tower.cs, but let's confirm:
        // rangeCollider = GetComponent<CircleCollider2D>(); // (already done in base if needed)
        ApplyLevelStats();
    }

    protected override void ApplyLevelStats()
    {
        if (towerData == null || level > towerData.levels.Length) return;

        TowerLevelData levelData = towerData.levels[level - 1];

        // 1) We can set the tower's "rangeCollider" radius to match levelData.range if we want
        if (rangeCollider != null)
        {
            rangeCollider.radius = levelData.range;
        }

        // 2) Update the range indicator visuals
        if (rangeIndicator != null)
        {
            float diameter = levelData.range * 2f;
            rangeIndicator.transform.localScale = new Vector3(diameter, diameter, 1f);
        }

        // 3) For projectile-shooting towers, we pass stats into the TowerShooting script
        if (towerShooting != null)
        {
            towerShooting.range = levelData.range;
            towerShooting.fireRate = levelData.fireRate;
            towerShooting.damage = levelData.damage;
        }

        base.ApplyImageStatus(levelData);
    }

    // In ProjectileTower.cs (or Tower.cs)
    public void UpdateRangeIndicatorVisualOnly(float newRange)
    {
        if (rangeIndicator != null)
        {
            float diameter = newRange * 2f;
            rangeIndicator.transform.localScale = new Vector3(diameter, diameter, 1f);
        }

        // If you also have a CircleCollider2D representing the tower's attack range:
        if (rangeCollider != null)
        {
            rangeCollider.radius = newRange;
        }
    }

}
