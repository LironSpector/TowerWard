using UnityEngine;

/// <summary>
/// Holds the essential stats for a balloon at a given health level.
/// Contains the movement speed, reward value, immunities (to freeze and poison), and the default sprite for the balloon.
/// </summary>
public struct BalloonStats
{
    /// <summary>
    /// The movement speed of the balloon.
    /// </summary>
    public float Speed;

    /// <summary>
    /// The reward (currency) awarded when the balloon is popped.
    /// </summary>
    public int Reward;

    /// <summary>
    /// Indicates whether the balloon is immune to freeze effects.
    /// </summary>
    public bool ImmuneToFreeze;

    /// <summary>
    /// Indicates whether the balloon is immune to poison effects.
    /// </summary>
    public bool ImmuneToPoison;

    /// <summary>
    /// The default sprite used to display the balloon when no special status effects are active.
    /// </summary>
    public Sprite NormalSprite;
}
