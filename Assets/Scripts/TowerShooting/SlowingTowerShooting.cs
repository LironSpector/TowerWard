using UnityEngine;
using System.Linq;
using System.Collections.Generic;

/// <summary>
/// Description:
/// Implements the shooting behavior for a Slowing Tower by inheriting from BaseTowerShooting.
/// This tower prioritizes targeting balloons that are not already slowed. When firing, the projectile is
/// marked with a Slow effect, and its effect duration and slow factor are set based on the tower's TowerData.
/// </summary>
public class SlowingTowerShooting : BaseTowerShooting
{
    /// <summary>
    /// Overrides the Update method to ensure that the tower does not continue targeting balloons that are already slowed.
    /// If the current target is slowed, it is cleared so a new target can be acquired.
    /// Then, the base Update method is called to continue normal shooting behavior.
    /// </summary>
    protected override void Update()
    {
        if (targetBalloon != null && targetBalloon.isSlowed)
        {
            targetBalloon = null;
        }
        base.Update();
    }

    /// <summary>
    /// Overrides the AcquireTarget method to prioritize unslowed balloons.
    /// It filters the list of balloons in range to exclude those that are already slowed.
    /// If there are unslowed balloons available, it selects the one furthest along the path
    /// (i.e., with the highest waypoint index). Otherwise, it defaults to the base method.
    /// </summary>
    protected override void AcquireTarget()
    {
        // Remove any null references from the list.
        balloonsInRange.RemoveAll(b => b == null);

        // Filter out balloons that are slowed.
        List<Balloon> notSlowed = balloonsInRange.Where(b => b.isSlowed == false).ToList();

        if (notSlowed.Count > 0)
        {
            // Among the unslowed balloons, select the one furthest along (with the highest waypoint index).
            Balloon best = notSlowed[0];
            int maxIndex = best.GetComponent<BalloonMovement>().waypointIndex;
            foreach (Balloon balloon in notSlowed)
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
            // If all balloons in range are slowed, fall back to the default target acquisition.
            base.AcquireTarget();
        }
    }

    /// <summary>
    /// Overrides the Shoot method to add a Slow effect to the projectile.
    /// This method first calls the base Shoot method to perform predictive shooting,
    /// then marks the projectile with the Slow effect, setting its effect duration and slow factor
    /// from the tower's TowerData.
    /// </summary>
    /// <returns>
    /// A Projectile with the Slow effect applied, or null if no projectile was generated.
    /// </returns>
    protected override Projectile Shoot()
    {
        Projectile projectile = base.Shoot();
        if (projectile == null)
            return null;

        // Mark the projectile to apply the Slow effect.
        projectile.effectType = ProjectileEffectType.Slow;

        // Retrieve and set slow effect parameters from the tower's data.
        Tower tower = GetComponent<Tower>();
        if (tower != null && tower.towerData != null)
        {
            float slowDur = tower.towerData.levels[tower.level - 1].slowDuration;
            float slowFactor = tower.towerData.levels[tower.level - 1].slowFactor;
            projectile.effectDuration = slowDur;
            projectile.slowFactor = slowFactor;
        }

        return projectile;
    }
}
