using System.Collections.Generic;
using System.Diagnostics;
using UnityEngine;
using System;
using System.IO;

/// <summary>
/// Description:
/// Manages the occupancy status of grid cells in the game. This includes tracking which cells
/// are occupied by towers and which are occupied by surprise boxes. The class provides functionalities
/// to occupy or free cells, check if a cell is occupied for any reason, retrieve a tower by cell position,
/// and refresh tower stats across all occupied cells.
/// </summary>
public class CellOccupationManager : MonoBehaviour
{
    // Dictionaries to track occupied cells by towers and surprise boxes.
    private Dictionary<Vector2, Tower> occupiedCells = new Dictionary<Vector2, Tower>();
    private Dictionary<Vector2, SurpriseBox> occupiedBoxCells = new Dictionary<Vector2, SurpriseBox>();

    /// <summary>
    /// Checks if the specified cell is occupied by a tower.
    /// </summary>
    /// <param name="position">The grid position to check.</param>
    /// <returns>
    /// True if the cell is occupied by a tower; otherwise, false.
    /// </returns>
    private bool IsCellOccupied(Vector2 position)
    {
        return occupiedCells.ContainsKey(position);
    }

    /// <summary>
    /// Determines whether the specified cell is occupied for any reason,
    /// either by a tower or by a surprise box.
    /// </summary>
    /// <param name="position">The grid position to check.</param>
    /// <returns>
    /// True if the cell is occupied by either a tower or a surprise box; otherwise, false.
    /// </returns>
    public bool IsCellOccupiedForAnyReason(Vector2 position)
    {
        return IsCellOccupied(position) || occupiedBoxCells.ContainsKey(position);
    }

    /// <summary>
    /// Marks the specified cell as occupied by a tower.
    /// </summary>
    /// <param name="position">The grid position to occupy.</param>
    /// <param name="tower">The tower that is occupying the cell.</param>
    public void OccupyCell(Vector2 position, Tower tower)
    {
        // Associate the tower with the given grid position.
        occupiedCells[position] = tower;
    }

    /// <summary>
    /// Frees the specified cell from tower occupation.
    /// </summary>
    /// <param name="position">The grid position to free.</param>
    public void FreeCell(Vector2 position)
    {
        occupiedCells.Remove(position);
    }

    /// <summary>
    /// Retrieves the tower occupying the specified grid cell.
    /// </summary>
    /// <param name="position">The grid position to check for an occupying tower.</param>
    /// <returns>
    /// The <see cref="Tower"/> at the specified position if present; otherwise, null.
    /// </returns>
    public Tower GetTowerAtPosition(Vector2 position)
    {
        if (occupiedCells.ContainsKey(position))
        {
            return occupiedCells[position];
        }
        return null;
    }

    /// <summary>
    /// Marks the specified cell as occupied by a surprise box.
    /// </summary>
    /// <param name="position">The grid position to occupy.</param>
    /// <param name="box">The surprise box that will occupy the cell.</param>
    public void OccupyCellWithBox(Vector2 position, SurpriseBox box)
    {
        if (!occupiedBoxCells.ContainsKey(position))
        {
            occupiedBoxCells[position] = box;
        }
    }

    /// <summary>
    /// Frees the specified cell from being occupied by a surprise box.
    /// </summary>
    /// <param name="position">The grid position to free from a surprise box.</param>
    public void FreeCellFromBox(Vector2 position)
    {
        if (occupiedBoxCells.ContainsKey(position))
        {
            occupiedBoxCells.Remove(position);
        }
    }

    /// <summary>
    /// Refreshes and reapplies level stats to all towers occupying cells.
    /// This function is useful when global buffs or tower parameters change,
    /// ensuring all towers update their effective stats accordingly.
    /// </summary>
    public void RefreshAllTowersStats()
    {
        foreach (var kvp in occupiedCells)
        {
            Tower t = kvp.Value;
            t.ApplyLevelStats();
            // For ProjectileTowers, this recalculates the final range and fire rate using the current buff factors.
        }
    }
}
