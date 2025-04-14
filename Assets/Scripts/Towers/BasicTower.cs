using UnityEngine;

/// <summary>
/// Description:
/// Represents a Basic Tower that uses standard projectile shooting behavior as defined in the base class.
/// This class does not add any additional logic beyond what is provided by ProjectileTower.
/// </summary>
public class BasicTower : ProjectileTower
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
