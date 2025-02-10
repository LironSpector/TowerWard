using UnityEngine;

/// <summary>
/// Holds the essential stats a balloon needs when given a certain health.
/// </summary>
public struct BalloonStats
{
    public float Speed;
    public int Reward;
    public bool ImmuneToFreeze;
    public bool ImmuneToPoison;
    public Sprite NormalSprite;
}
