using UnityEngine;
using System.Collections.Generic;
using UnityEngine.Tilemaps;

/// <summary>
/// Utility methods for tower placement logic.
/// </summary>
public static class TowerPlacementUtils
{
    /// <summary>
    /// Recursively sets the Unity layer for a GameObject and all its children.
    /// </summary>
    public static void SetLayerRecursively(GameObject obj, int newLayer)
    {
        if (obj == null) return;

        obj.layer = newLayer;
        foreach (Transform child in obj.transform)
        {
            if (child == null) continue;
            SetLayerRecursively(child.gameObject, newLayer);
        }
    }

    /// <summary>
    /// Checks if a world position corresponds to any of the predefined path cells (using the balloonPathPositions list).
    /// </summary>
    public static bool IsOnBalloonPath(Vector2 position, Tilemap tilemap, List<Vector3Int> balloonPathPositions)
    {
        Vector3Int cellPosition = tilemap.WorldToCell(position);
        return balloonPathPositions.Contains(cellPosition);
    }
}
