using UnityEngine;

/// <summary>
/// Description:
/// Provides utility methods for balloons, such as obtaining the appropriate statistics
/// (movement speed, reward, immunities, and default sprite) based on the balloon's health.
/// The GetStatsForHealth method uses predefined health thresholds to determine which attributes
/// to return in a BalloonStats struct.
/// </summary>
public static class BalloonUtils
{
    /// <summary>
    /// Returns a <see cref="BalloonStats"/> struct containing the speed, reward, immunities,
    /// and normal sprite for a balloon of a given health.
    /// The method uses a series of conditional checks against health thresholds to populate the stats.
    /// </summary>
    /// <param name="balloon">
    /// A reference to the <see cref="Balloon"/> instance. It provides access to the sprite references
    /// that are used to set the balloon's visual appearance.
    /// </param>
    /// <param name="health">
    /// The current health value of the balloon.
    /// </param>
    /// <returns>
    /// A <see cref="BalloonStats"/> struct with values appropriate for the provided health.
    /// </returns>
    public static BalloonStats GetStatsForHealth(Balloon balloon, int health)
    {
        // Initialize the stats object.
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
