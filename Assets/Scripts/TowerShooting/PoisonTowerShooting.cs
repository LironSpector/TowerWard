using UnityEngine;
using System.Linq;
using System.Collections.Generic;

/// <summary>
/// Description:
/// Implements the shooting behavior for a Poison Tower by inheriting from BaseTowerShooting.
/// This tower prioritizes targeting balloons that are not already poisoned. When shooting,
/// the projectile is marked with a Poison effect, and its effect duration and tick interval are set
/// based on the tower's TowerData.
/// </summary>
public class PoisonTowerShooting : BaseTowerShooting
{
    /// <summary>
    /// Overrides the Update method to ensure that the tower does not continue targeting balloons that are already poisoned.
    /// If the current target is poisoned, it clears the target before proceeding with the normal update logic.
    /// </summary>
    protected override void Update()
    {
        if (targetBalloon != null && targetBalloon.isPoisoned)
        {
            targetBalloon = null;
        }
        base.Update();
    }

    /// <summary>
    /// Overrides the AcquireTarget method to prioritize unfrozen targets.
    /// First, filters the list of balloons in range to those that are not poisoned.
    /// If any exist, selects the one that is furthest along the path (i.e., with the highest waypoint index).
    /// If all balloons are already poisoned, falls back to the base target acquisition logic.
    /// </summary>
    protected override void AcquireTarget()
    {
        // Remove any null balloons.
        balloonsInRange.RemoveAll(b => b == null);

        // Filter out poisoned balloons first.
        List<Balloon> notPoisoned = balloonsInRange.Where(b => !b.isPoisoned).ToList();

        if (notPoisoned.Count > 0)
        {
            // Select the balloon furthest along the path.
            Balloon best = notPoisoned[0];
            int maxIndex = best.GetComponent<BalloonMovement>().waypointIndex;
            foreach (var balloon in notPoisoned)
            {
                int wp = balloon.GetComponent<BalloonMovement>().waypointIndex;
                if (wp > maxIndex)
                {
                    best = balloon;
                    maxIndex = wp;
                }
            }
            targetBalloon = best;
        }
        else
        {
            // If all available balloons are poisoned, fallback to the base acquisition logic.
            base.AcquireTarget();
        }
    }

    /// <summary>
    /// Overrides the Shoot method to add a Poison effect to the projectile.
    /// This method calls the base Shoot method for standard predictive shooting, then marks the projectile
    /// with a Poison effect and sets its effect duration and tick interval based on the tower's data.
    /// </summary>
    /// <returns>
    /// The spawned Projectile with the Poison effect applied, or null if shooting fails.
    /// </returns>
    protected override Projectile Shoot()
    {
        Projectile projectile = base.Shoot();
        if (projectile == null)
            return null;

        // Mark the projectile as having a Poison effect.
        projectile.effectType = ProjectileEffectType.Poison;

        // Retrieve poison settings from the tower's data.
        Tower tower = GetComponent<Tower>();
        if (tower != null && tower.towerData != null)
        {
            float poisonDur = tower.towerData.levels[tower.level - 1].poisonDuration;
            float tickInterval = tower.towerData.levels[tower.level - 1].poisonTickInterval;
            projectile.effectDuration = poisonDur;
            projectile.poisonTickInterval = tickInterval;
        }

        return projectile;
    }
}
