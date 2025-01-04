using UnityEngine;
using System.Linq;
using System.Collections.Generic;

public class SlowingTowerShooting : BaseTowerShooting
{
    protected override void Update() // Imporatnt for the tower to always pick balloons which aren't slowed yet before slowed ones
    {
        if (targetBalloon != null && targetBalloon.isSlowed)
        {
            targetBalloon = null;
        }
        base.Update();
    }

    protected override void AcquireTarget()
    {
        // Remove nulls
        balloonsInRange.RemoveAll(b => b == null);

        // 1) Try to find a balloon that is NOT slowed
        List<Balloon> notSlowed = balloonsInRange
            .Where(b => b.isSlowed == false)
            .ToList();

        if (notSlowed.Count > 0)
        {
            // Among them, pick the balloon with the highest waypoint index
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
            // If all in range are slowed, pick normally
            base.AcquireTarget();
        }
    }

    protected override Projectile Shoot()
    {
        Projectile projectile = base.Shoot();
        if (projectile == null) return null;

        projectile.effectType = ProjectileEffectType.Slow;

        // Set duration + slowFactor from TowerData
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
