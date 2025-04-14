using UnityEngine;

/// <summary>
/// Description:
/// Represents a Slowing Tower that applies slow effects to its targets.
/// Currently, it relies entirely on the base projectile tower behavior without additional overrides.
/// </summary>
public class SlowingTower : ProjectileTower
{
    /// <summary>
    /// Called when the script instance is being loaded.
    /// Invokes the base Awake implementation.
    /// </summary>
    protected override void Awake()
    {
        base.Awake();
    }

    /// <summary>
    /// Called before the first frame update.
    /// Invokes the base Start implementation.
    /// </summary>
    protected override void Start()
    {
        base.Start();
    }
}
