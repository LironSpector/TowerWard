using UnityEngine;

public static class BalloonUtils
{
    /// <summary>
    /// Returns a BalloonStats struct containing speed, reward, immunities, and the
    /// sprite for a balloon of a given health.
    /// </summary>
    public static BalloonStats GetStatsForHealth(Balloon balloon, int health)
    {
        // We'll fill in a 'stats' object by referencing balloon's sprites
        // and your existing health thresholds.
        BalloonStats stats = new BalloonStats();

        if (health == 1)
        {
            stats.Speed = 2f;
            stats.Reward = 5;
            stats.ImmuneToFreeze = false;
            stats.ImmuneToPoison = false;
            stats.NormalSprite = balloon.redBalloonSprite;
        }
        else if (health == 2)
        {
            stats.Speed = 2f;
            stats.Reward = 5;
            stats.ImmuneToFreeze = false;
            stats.ImmuneToPoison = false;
            stats.NormalSprite = balloon.blueBalloonSprite;
        }
        else if (health == 3)
        {
            stats.Speed = 2f;
            stats.Reward = 5;
            stats.ImmuneToFreeze = false;
            stats.ImmuneToPoison = false;
            stats.NormalSprite = balloon.greenBalloonSprite;
        }
        else if (health == 4)
        {
            stats.Speed = 2f;
            stats.Reward = 5;
            stats.ImmuneToFreeze = false;
            stats.ImmuneToPoison = false;
            stats.NormalSprite = balloon.yellowBalloonSprite;
        }
        else if (health == 5)
        {
            stats.Speed = 4f;
            stats.Reward = 5;
            stats.ImmuneToFreeze = false;
            stats.ImmuneToPoison = false;
            stats.NormalSprite = balloon.pinkBalloonSprite;
        }
        else if (health == 6)
        {
            stats.Speed = 3f;
            stats.Reward = 5;
            stats.ImmuneToFreeze = true;
            stats.ImmuneToPoison = false;
            stats.NormalSprite = balloon.blackBalloonSprite;
        }
        else if (health == 7)
        {
            stats.Speed = 3f;
            stats.Reward = 5;
            stats.ImmuneToFreeze = false;
            stats.ImmuneToPoison = true;
            stats.NormalSprite = balloon.whiteBalloonSprite;
        }
        else if (health >= 8 && health <= 10)
        {
            stats.Speed = 2f;
            stats.Reward = 5;
            stats.ImmuneToFreeze = false;
            stats.ImmuneToPoison = false;
            stats.NormalSprite = balloon.strongBalloonSprite;
        }
        else if (health >= 11 && health <= 16)
        {
            stats.Speed = 1.5f;
            stats.Reward = 5;
            stats.ImmuneToFreeze = false;
            stats.ImmuneToPoison = false;
            stats.NormalSprite = balloon.strongerBalloonSprite;
        }
        else if (health >= 17 && health <= 26)
        {
            stats.Speed = 1f;
            stats.Reward = 5;
            stats.ImmuneToFreeze = false;
            stats.ImmuneToPoison = false;
            stats.NormalSprite = balloon.veryStrongBalloonSprite;
        }
        else if (health >= 27 && health <= 126)
        {
            stats.Speed = 1.5f;
            stats.Reward = 5;
            stats.ImmuneToFreeze = true;
            stats.ImmuneToPoison = true;
            stats.NormalSprite = balloon.smallBossBalloonSprite;
        }
        else if (health >= 127 && health <= 626)
        {
            stats.Speed = 1f;
            stats.Reward = 5;
            stats.ImmuneToFreeze = true;
            stats.ImmuneToPoison = true;
            stats.NormalSprite = balloon.mediumBossBalloonSprite;
        }
        else if (health >= 627 && health <= 3126)
        {
            stats.Speed = 0.5f;
            stats.Reward = 5;
            stats.ImmuneToFreeze = true;
            stats.ImmuneToPoison = true;
            stats.NormalSprite = balloon.bigBossBalloonSprite;
        }

        return stats;
    }
}
