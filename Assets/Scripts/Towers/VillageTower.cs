using UnityEngine;
using System.Collections.Generic;

/// <summary>
/// Description:
/// Represents a Village Tower that provides buffs to nearby projectile towers.
/// When a ProjectileTower enters its trigger area, this tower increases that tower’s fire rate and range
/// by a percentage defined by its own level-specific data (buffPercent). When the tower exits the area,
/// its stats revert to their original values. The VillageTower keeps track of buffed towers and their original stats
/// using a dictionary.
/// </summary>
public class VillageTower : Tower
{
    /// <summary>
    /// Dictionary that stores references to buffed ProjectileTowers along with their original fire rate and range.
    /// </summary>
    private Dictionary<ProjectileTower, TowerBuffData> buffedTowers = new Dictionary<ProjectileTower, TowerBuffData>();

    /// <summary>
    /// The percentage by which nearby towers are buffed (e.g., 0.1 for a 10% increase).
    /// This is determined from the special value in the tower’s data.
    /// </summary>
    private float buffPercent = 0.1f;

    /// <summary>
    /// Called before the first frame update.
    /// Initializes the VillageTower by invoking the base Start method and applying level-specific stats.
    /// </summary>
    protected override void Start()
    {
        base.Start();
        ApplyLevelStats();
    }

    /// <summary>
    /// Applies the VillageTower’s level-specific statistics.
    /// Sets the buff percentage based on the special value in the TowerData.
    /// </summary>
    public override void ApplyLevelStats()
    {
        base.ApplyLevelStats();
        if (towerData != null && level <= towerData.levels.Length)
        {
            TowerLevelData lvl = towerData.levels[level - 1];
            buffPercent = lvl.specialValue / 100f;
        }
    }

    #region Trigger Events

    /// <summary>
    /// Called when a collider enters the VillageTower's trigger area.
    /// If the collider belongs to a ProjectileTower and the tower is fully placed, 
    /// applies a buff to the tower.
    /// </summary>
    /// <param name="other">The collider that entered the trigger.</param>
    void OnTriggerEnter2D(Collider2D other)
    {
        if (!isFullyPlaced)
            return;

        if (other.CompareTag("ProjectileTower"))
        {
            ProjectileTower pTower = other.GetComponent<ProjectileTower>();
            if (pTower != null && !buffedTowers.ContainsKey(pTower))
            {
                BuffTower(pTower);
            }
        }
    }

    /// <summary>
    /// Called when a collider exits the VillageTower's trigger area.
    /// If the collider belongs to a buffed ProjectileTower, the buff is removed.
    /// </summary>
    /// <param name="other">The collider that exited the trigger.</param>
    void OnTriggerExit2D(Collider2D other)
    {
        if (!isFullyPlaced)
            return;

        if (other.CompareTag("ProjectileTower"))
        {
            ProjectileTower pTower = other.GetComponent<ProjectileTower>();
            if (pTower != null && buffedTowers.ContainsKey(pTower))
            {
                UnbuffTower(pTower);
            }
        }
    }

    #endregion

    #region Buffing Methods

    /// <summary>
    /// Applies the buff to a ProjectileTower by increasing its fire rate and range
    /// by the defined buff percentage. The original stats are stored in a dictionary for later reversion.
    /// </summary>
    /// <param name="pTower">The ProjectileTower to buff.</param>
    private void BuffTower(ProjectileTower pTower)
    {
        BaseTowerShooting shooting = pTower.GetComponent<BaseTowerShooting>();
        if (shooting == null)
            return;

        TowerBuffData data = new TowerBuffData
        {
            baseFireRate = shooting.fireRate,
            baseRange = shooting.range
        };

        shooting.fireRate *= (1f + buffPercent);
        shooting.range *= (1f + buffPercent);

        // Update the visual representation of the tower's range.
        pTower.UpdateRangeIndicatorVisualOnly(shooting.range);

        buffedTowers[pTower] = data;
    }

    /// <summary>
    /// Removes the buff from a ProjectileTower by restoring its original fire rate and range.
    /// Updates the tower's range indicator accordingly and removes it from the buff tracking dictionary.
    /// </summary>
    /// <param name="pTower">The ProjectileTower to unbuff.</param>
    private void UnbuffTower(ProjectileTower pTower)
    {
        BaseTowerShooting shooting = pTower.GetComponent<BaseTowerShooting>();
        if (shooting == null)
            return;

        if (buffedTowers.TryGetValue(pTower, out TowerBuffData original))
        {
            shooting.fireRate = original.baseFireRate;
            shooting.range = original.baseRange;

            pTower.UpdateRangeIndicatorVisualOnly(shooting.range);
            buffedTowers.Remove(pTower);
        }
    }

    #endregion

    #region Refresh Buffs

    /// <summary>
    /// Refreshes all projectile towers within the VillageTower's range by applying buffs where needed.
    /// Scans all colliders within the tower's range and applies the buff to any ProjectileTowers that are not already buffed.
    /// </summary>
    public void RefreshTowersInRange()
    {
        if (rangeCollider == null)
            return;

        // Find all colliders within the tower's range.
        Collider2D[] hits = Physics2D.OverlapCircleAll(transform.position, rangeCollider.radius);

        // For each "Tower" we find, if it’s a ProjectileTower, we call BuffTower (unless already buffed)
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

    #endregion
}

/// <summary>
/// Structure to store the original stats of a buffed ProjectileTower.
/// Used to revert the tower's stats when it exits the VillageTower's range.
/// </summary>
public struct TowerBuffData
{
    /// <summary>
    /// The original fire rate of the tower before buffing.
    /// </summary>
    public float baseFireRate;

    /// <summary>
    /// The original range of the tower before buffing.
    /// </summary>
    public float baseRange;
}
