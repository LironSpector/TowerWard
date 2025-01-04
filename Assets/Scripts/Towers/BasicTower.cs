using UnityEngine;

public class BasicTower : ProjectileTower
{
    // Basic tower has no special logic beyond the base tower
    // but you can still override if you want custom logic
    protected override void Awake()
    {
        base.Awake();
    }

    protected override void Start()
    {
        base.Start();
    }
}
