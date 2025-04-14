using UnityEngine;

/// <summary>
/// Description:
/// Represents a projectile-based tower that shoots projectiles to damage balloons.
/// The ProjectileTower class extends the base Tower class by integrating projectile shooting behavior,
/// which is managed by a BaseTowerShooting component. It applies level-specific statistics including
/// range, fire rate, and damage—modified by any global buffs—and updates its visual range indicator accordingly.
/// </summary>
public class ProjectileTower : Tower
{
    /// <summary>
    /// Reference to the BaseTowerShooting component that handles the tower's shooting logic.
    /// </summary>
    protected BaseTowerShooting towerShooting;

    /// <summary>
    /// Called before the first frame update.
    /// Initializes the ProjectileTower by calling the base Start method, retrieving the shooting component,
    /// and applying the level-specific statistics.
    /// </summary>
    protected override void Start()
    {
        base.Start();
        towerShooting = GetComponent<BaseTowerShooting>();
        ApplyLevelStats();
    }

    /// <summary>
    /// Applies the level-specific statistics for the projectile tower.
    /// This includes updating the tower's range (adjusted by the global range buff),
    /// firing rate (modified by the global fire rate buff), and damage.
    /// It also updates the visual representation of the range indicator and applies the appropriate level-specific sprite.
    /// </summary>
    public override void ApplyLevelStats()
    {
        if (towerData == null || level > towerData.levels.Length) return;
        TowerLevelData levelData = towerData.levels[level - 1];

        // Calculate the final range adjusted by the global buff factor.
        float finalRange = levelData.range * GameManager.Instance.rangeBuffFactor;

        // Update the range collider's radius.
        if (rangeCollider != null)
        {
            rangeCollider.radius = finalRange;
        }

        // Update the range indicator's scale.
        if (rangeIndicator != null)
        {
            float diameter = finalRange * 2f;
            rangeIndicator.transform.localScale = new Vector3(diameter, diameter, 1f);
        }

        // If a shooting component is available, update its stats.
        if (towerShooting != null)
        {
            towerShooting.range = finalRange;
            towerShooting.fireRate = levelData.fireRate * GameManager.Instance.fireRateBuffFactor;
            towerShooting.damage = levelData.damage;
        }

        // Apply the level-specific sprite.
        base.ApplyImageStatus(levelData);
    }

    /// <summary>
    /// Updates the visual representation of the tower's range indicator on the fly.
    /// Sets the range collider's radius and adjusts the range indicator scale based on the provided new range.
    /// </summary>
    /// <param name="newRange">The new range value to apply.</param>
    public void UpdateRangeIndicatorVisualOnly(float newRange)
    {
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
