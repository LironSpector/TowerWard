using UnityEngine;

[CreateAssetMenu(fileName = "New Tower Data", menuName = "Tower Defense/Tower Data")]
public class TowerData : ScriptableObject
{
    public string towerName;
    public Sprite towerSprite;
    public TowerLevelData[] levels; //The first item in the list (level 1) are the values used for a newly placed tower on the board, so in that case
                                    //the "upgradeCost" is how much the tower placement costs (and not an actual upgrade)
}

[System.Serializable]
public class TowerLevelData
{
    public int upgradeCost;
    public float range;
    public float fireRate;
    public int damage;
}
