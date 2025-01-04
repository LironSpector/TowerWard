using UnityEngine;
using System.Collections.Generic;

public class VillageTower : Tower
{
    private Dictionary<ProjectileTower, TowerBuffData> buffedTowers = new Dictionary<ProjectileTower, TowerBuffData>();
    private float buffPercent = 0.1f;

    protected override void Start()
    {
        base.Start();
        ApplyLevelStats();
    }

    protected override void ApplyLevelStats()
    {
        base.ApplyLevelStats();
        if (towerData != null && level <= towerData.levels.Length)
        {
            TowerLevelData lvl = towerData.levels[level - 1];
            buffPercent = lvl.specialValue / 100f;
        }
    }

    // -------------------------
    // Trigger events
    // -------------------------
    void OnTriggerEnter2D(Collider2D other)
    {
        // If not fully placed, skip
        if (!isFullyPlaced) return;

        if (other.CompareTag("ProjectileTower"))
        {
            ProjectileTower pTower = other.GetComponent<ProjectileTower>();
            if (pTower != null && !buffedTowers.ContainsKey(pTower))
            {
                BuffTower(pTower);
            }
        }
    }

    void OnTriggerExit2D(Collider2D other)
    {
        if (!isFullyPlaced) return;

        if (other.CompareTag("ProjectileTower"))
        {
            ProjectileTower pTower = other.GetComponent<ProjectileTower>();
            if (pTower != null && buffedTowers.ContainsKey(pTower))
            {
                UnbuffTower(pTower);
            }
        }
    }

    private void BuffTower(ProjectileTower pTower)
    {
        BaseTowerShooting shooting = pTower.GetComponent<BaseTowerShooting>();
        if (shooting == null) return;

        TowerBuffData data = new TowerBuffData
        {
            baseFireRate = shooting.fireRate,
            baseRange = shooting.range
        };

        shooting.fireRate *= (1f + buffPercent);
        shooting.range *= (1f + buffPercent);

        pTower.UpdateRangeIndicatorVisualOnly(shooting.range);
        buffedTowers[pTower] = data;
    }

    private void UnbuffTower(ProjectileTower pTower)
    {
        BaseTowerShooting shooting = pTower.GetComponent<BaseTowerShooting>();
        if (shooting == null) return;

        if (buffedTowers.TryGetValue(pTower, out TowerBuffData original))
        {
            shooting.fireRate = original.baseFireRate;
            shooting.range = original.baseRange;

            pTower.UpdateRangeIndicatorVisualOnly(shooting.range);
            buffedTowers.Remove(pTower);
        }
    }

    // -------------------------
    // The key method to refresh towers in range
    // -------------------------
    public void RefreshTowersInRange()
    {
        if (rangeCollider == null) return;

        // 1) Find all objects in a circle around VillageTower
        Collider2D[] hits = Physics2D.OverlapCircleAll(transform.position, rangeCollider.radius);

        // 2) For each "Tower" we find, if it’s a ProjectileTower, we call BuffTower (unless already buffed)
        foreach (Collider2D c in hits)
        {
            if (c.CompareTag("ProjectileTower"))
            {
                ProjectileTower pTower = c.GetComponent<ProjectileTower>();
                if (pTower != null && !buffedTowers.ContainsKey(pTower))
                {
                    BuffTower(pTower);
                }
            }
        }
    }
}

public struct TowerBuffData
{
    public float baseFireRate;
    public float baseRange;
}
