using UnityEngine;

public class ProjectileTower : Tower
{
    protected BaseTowerShooting towerShooting;

    protected override void Start()
    {
        base.Start();
        towerShooting = GetComponent<BaseTowerShooting>();
        ApplyLevelStats();
    }

    public override void ApplyLevelStats()
    {
        if (towerData == null || level > towerData.levels.Length) return;
        TowerLevelData levelData = towerData.levels[level - 1];

        // 1) Multiply the circle collider radius by the global buff factor
        float finalRange = levelData.range * GameManager.Instance.rangeBuffFactor;

        if (rangeCollider != null)
        {
            rangeCollider.radius = finalRange;
        }

        // 2) Multiply the range indicator scale as well
        if (rangeIndicator != null)
        {
            float diameter = finalRange * 2f;
            rangeIndicator.transform.localScale = new Vector3(diameter, diameter, 1f);
        }

        // 3) If we track 'shooting.range' for our AcquireTarget() or projectile speed logic, set it here:
        if (towerShooting != null)
        {
            towerShooting.range = finalRange;
            towerShooting.fireRate = levelData.fireRate * GameManager.Instance.fireRateBuffFactor;
            towerShooting.damage = levelData.damage;
        }

        // 4) A level-specific sprite
        base.ApplyImageStatus(levelData);
    }

    public void UpdateRangeIndicatorVisualOnly(float newRange)
    {
        // Update range on the fly
        if (rangeIndicator != null)
        {
            float diameter = newRange * 2f;
            rangeIndicator.transform.localScale = new Vector3(diameter, diameter, 1f);
        }
        if (rangeCollider != null)
        {
            rangeCollider.radius = newRange;
        }
    }
}
