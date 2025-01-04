using UnityEngine;
using System.Linq;
using System.Collections.Generic;

public class PoisonTowerShooting : BaseTowerShooting
{
    protected override void Update() // Imporatnt for the tower to always pick balloons which aren't poisoned yet before poisoned ones
    {
        if (targetBalloon != null && targetBalloon.isPoisoned)
        {
            targetBalloon = null;
        }
        base.Update();
    }


    protected override void AcquireTarget()
    {
        balloonsInRange.RemoveAll(b => b == null);

        // 1) Try to find a balloon that is NOT poisoned
        List<Balloon> notPoisoned = balloonsInRange
            .Where(b => !b.isPoisoned)
            .ToList();

        if (notPoisoned.Count > 0)
        {
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
            // All are already poisoned, fallback
            base.AcquireTarget();
        }
    }

    protected override Projectile Shoot()
    {
        Projectile projectile = base.Shoot();
        if (projectile == null) return null;

        projectile.effectType = ProjectileEffectType.Poison;

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
