//using UnityEngine;

///// <summary>
///// ProjectileTower inherits from the base Tower and adds the logic for shooting projectiles.
///// </summary>
//public class ProjectileTower : Tower
//{
//    protected BaseTowerShooting towerShooting; // Moved here from the old base Tower

//    protected override void Start()
//    {
//        base.Start();

//        // Now we initialize the shooting reference
//        towerShooting = GetComponent<BaseTowerShooting>();
//        // If there's a circle collider for range, we already got it in Tower.cs, but let's confirm:
//        // rangeCollider = GetComponent<CircleCollider2D>(); // (already done in base if needed)
//        ApplyLevelStats();
//    }

//    public override void ApplyLevelStats()
//    {
//        if (towerData == null || level > towerData.levels.Length) return;

//        TowerLevelData levelData = towerData.levels[level - 1];

//        // 1) We can set the tower's "rangeCollider" radius to match levelData.range if we want
//        if (rangeCollider != null)
//        {
//            rangeCollider.radius = levelData.range;
//        }

//        // 2) Update the range indicator visuals
//        if (rangeIndicator != null)
//        {
//            float diameter = levelData.range * 2f;
//            rangeIndicator.transform.localScale = new Vector3(diameter, diameter, 1f);
//        }

//        // 3) For projectile-shooting towers, we pass stats into the TowerShooting script
//        if (towerShooting != null)
//        {
//            //towerShooting.range = levelData.range;
//            //towerShooting.fireRate = levelData.fireRate;
//            towerShooting.range = levelData.range * GameManager.Instance.rangeBuffFactor;
//            towerShooting.fireRate = levelData.fireRate * GameManager.Instance.fireRateBuffFactor;
//            towerShooting.damage = levelData.damage;
//        }

//        base.ApplyImageStatus(levelData);
//    }

//    // In ProjectileTower.cs (or Tower.cs)
//    public void UpdateRangeIndicatorVisualOnly(float newRange)
//    {
//        if (rangeIndicator != null)
//        {
//            float diameter = newRange * 2f;
//            rangeIndicator.transform.localScale = new Vector3(diameter, diameter, 1f);
//        }

//        // If you also have a CircleCollider2D representing the tower's attack range:
//        if (rangeCollider != null)
//        {
//            rangeCollider.radius = newRange;
//        }
//    }

//}




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

        // 4) If you have a level-specific sprite
        base.ApplyImageStatus(levelData);
    }

    public void UpdateRangeIndicatorVisualOnly(float newRange)
    {
        // If you want to update range on the fly
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
