using UnityEngine;
using System.Linq;
using System.Collections.Generic;

public class FreezingTowerShooting : BaseTowerShooting
{
    protected override void Update() // Imporatnt for the tower to always pick balloons which aren't frozen yet before frozen ones
    {
        // If we currently have a target and it's actually frozen, we don't want it.
        if (targetBalloon != null && targetBalloon.isFrozen)
        {
            targetBalloon = null;
        }

        // Then call the base update, which does "if (targetBalloon == null) AcquireTarget()"
        base.Update();
    }

    protected override void AcquireTarget()
    {
        // 1) Remove null balloons
        balloonsInRange.RemoveAll(b => b == null);

        // 2) Try to find a balloon that is NOT frozen first
        List<Balloon> notFrozen = balloonsInRange.Where(b => !b.isFrozen).ToList();

        if (notFrozen.Count > 0)
        {
            // Among the not-frozen, pick the furthest along path
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
            // If all are frozen, fallback to base logic 
            base.AcquireTarget();
        }
    }

    protected override Projectile Shoot()
    {
        // 1) Call base to do the standard predictive shot 
        Projectile projectile = base.Shoot();
        if (projectile == null) return null;

        // 2) Mark it as a freeze projectile
        projectile.effectType = ProjectileEffectType.Freeze;

        // 3) Grab the freeze duration from TowerData
        Tower tower = GetComponent<Tower>();
        if (tower != null && tower.towerData != null)
        {
            float freezeDur = tower.towerData.levels[tower.level - 1].freezeDuration;
            projectile.effectDuration = freezeDur;
        }

        return projectile;
    }
}
