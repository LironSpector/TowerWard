using UnityEngine;

/// <summary>
/// Description:
/// Represents a tower selection cell in the UI. This component stores an index corresponding to a specific tower prefab
/// in the TowerSelectionUI's towerPrefabs list, which is used to identify the tower type when a cell is selected.
/// </summary>
public class TowerSelectionCell : MonoBehaviour
{
    /// <summary>
    /// The index of the tower in the towerPrefabs list.
    /// </summary>
    public int towerIndex;
}
