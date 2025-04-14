using UnityEngine;

/// <summary>
/// Description:
/// Represents a Super Tower—a powerful tower type inheriting from ProjectileTower.
/// This class currently does not provide additional logic beyond calling the base methods.
/// </summary>
public class SuperTower : ProjectileTower
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
