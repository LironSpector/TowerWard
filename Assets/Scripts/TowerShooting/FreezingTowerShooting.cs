using UnityEngine;
using System.Linq;
using System.Collections.Generic;

/// <summary>
/// Description:
/// Implements the shooting behavior for a Freezing Tower. This tower prioritizes targeting balloons
/// that are not already frozen. When shooting, the projectile is marked with a freeze effect,
/// and its duration is obtained from the tower's TowerData.
/// </summary>
public class FreezingTowerShooting : BaseTowerShooting
{
    /// <summary>
    /// Overrides Update to ensure the tower does not target balloons that are already frozen.
    /// If the current target becomes frozen, it is cleared so that a new, unfrozen target can be acquired.
    /// After handling this check, it calls the base Update() to perform standard shooting operations.
    /// </summary>
    protected override void Update()
    {
        // If the current target exists and is frozen, clear it so that a new target will be acquired.
        if (targetBalloon != null && targetBalloon.isFrozen)
        {
            targetBalloon = null;
        }

        // Proceed with the standard update routine (which includes acquiring a target, shooting, and rotating).
        base.Update();
    }

    /// <summary>
    /// Overrides the base AcquireTarget method to prioritize unfrozen balloons.
    /// First, it filters the list of balloons in range to exclude any that are frozen.
    /// If any unfrozen balloons exist, it selects the one that is furthest along the path (i.e., has the highest waypoint index).
    /// If all balloons are frozen, it falls back to the base acquisition logic.
    /// </summary>
    protected override void AcquireTarget()
    {
        // Remove null references.
        balloonsInRange.RemoveAll(b => b == null);

        // Filter out frozen balloons.
        List<Balloon> notFrozen = balloonsInRange.Where(b => !b.isFrozen).ToList();

        if (notFrozen.Count > 0)
        {
            // Among unfrozen balloons, choose the one furthest along the path.
            Balloon best = notFrozen[0];
            int bestWP = best.GetComponent<BalloonMovement>().waypointIndex;

            foreach (Balloon b in notFrozen)
            {
                int wp = b.GetComponent<BalloonMovement>().waypointIndex;
                if (wp > bestWP)
                {
                    best = b;
                    bestWP = wp;
                }
            }
            targetBalloon = best;
        }
        else
        {
            // If all balloons in range are frozen, fallback to the default targeting logic.
            base.AcquireTarget();
        }
    }

    /// <summary>
    /// Overrides the base Shoot method to add a freezing effect to the projectile.
    /// First, the base shooting behavior is invoked to create a projectile using predictive targeting.
    /// Then, the projectile is marked as having a Freeze effect, and its effect duration is set based on the tower's data.
    /// </summary>
    /// <returns>
    /// The Projectile instance with the freeze effect applied, or null if the base Shoot method failed.
    /// </returns>
    protected override Projectile Shoot()
    {
        // Invoke base shooting logic to create a projectile.
        Projectile projectile = base.Shoot();
        if (projectile == null)
            return null;

        // Mark the projectile to apply the Freeze effect upon impact.
        projectile.effectType = ProjectileEffectType.Freeze;

        // Retrieve and set the freeze duration from the tower's TowerData.
        Tower tower = GetComponent<Tower>();
        if (tower != null && tower.towerData != null)
        {
            float freezeDur = tower.towerData.levels[tower.level - 1].freezeDuration;
            projectile.effectDuration = freezeDur;
        }

        return projectile;
    }
}
