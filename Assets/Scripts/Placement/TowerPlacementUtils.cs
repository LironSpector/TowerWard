using UnityEngine;
using System.Collections.Generic;
using UnityEngine.Tilemaps;

/// <summary>
/// Description:
/// Provides utility methods for managing tower placement logic,
/// such as setting the Unity layer on a GameObject and its children recursively,
/// and checking if a given world position lies on the predefined balloon path.
/// </summary>
public static class TowerPlacementUtils
{
    /// <summary>
    /// Recursively sets the Unity layer for the specified GameObject and all its children.
    /// </summary>
    /// <param name="obj">The GameObject whose layer is to be set.</param>
    /// <param name="newLayer">The new layer to assign.</param>
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
    /// Determines whether a specified world position corresponds to any of the predefined balloon path cells.
    /// Uses the given Tilemap to convert the world position to a cell position,
    /// and then checks if that cell is contained in the provided list of balloon path positions.
    /// </summary>
    /// <param name="position">The world position to check.</param>
    /// <param name="tilemap">The Tilemap used to convert the world position to a cell position.</param>
    /// <param name="balloonPathPositions">A list of Vector3Int cell positions that represent the balloon path.</param>
    /// <returns>
    /// True if the converted cell position is in the balloonPathPositions list; otherwise, false.
    /// </returns>
    public static bool IsOnBalloonPath(Vector2 position, Tilemap tilemap, List<Vector3Int> balloonPathPositions)
    {
        Vector3Int cellPosition = tilemap.WorldToCell(position);
        return balloonPathPositions.Contains(cellPosition);
    }
}
