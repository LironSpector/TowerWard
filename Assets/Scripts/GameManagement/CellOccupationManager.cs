using System.Collections.Generic;
using System.Diagnostics;
using UnityEngine;
using System;
using System.IO;

public class CellOccupationManager : MonoBehaviour
{
    //private GameManager gm;

    // We keep these dictionaries here
    private Dictionary<Vector2, Tower> occupiedCells = new Dictionary<Vector2, Tower>();
    private Dictionary<Vector2, SurpriseBox> occupiedBoxCells = new Dictionary<Vector2, SurpriseBox>();

    //void Awake()
    //{
    //    gm = GetComponent<GameManager>();
    //}

    // Public accessors
    private bool IsCellOccupied(Vector2 position)
    {
        return occupiedCells.ContainsKey(position);
    }

    public bool IsCellOccupiedForAnyReason(Vector2 position)
    {
        return IsCellOccupied(position) || occupiedBoxCells.ContainsKey(position);
    }

    public void OccupyCell(Vector2 position, Tower tower)
    {
        //Debug.Log("Position to occupy: " + position);
        occupiedCells[position] = tower;
    }

    public void FreeCell(Vector2 position)
    {
        occupiedCells.Remove(position);
    }

    // Method to retrieve the tower at a specific position - currently not used
    public Tower GetTowerAtPosition(Vector2 position)
    {
        if (occupiedCells.ContainsKey(position))
        {
            return occupiedCells[position];
        }
        return null;
    }

    // Surprise box logic
    // Occupy a cell with a surprise box
    public void OccupyCellWithBox(Vector2 position, SurpriseBox box)
    {
        if (!occupiedBoxCells.ContainsKey(position))
        {
            occupiedBoxCells[position] = box;
        }
    }

    public void FreeCellFromBox(Vector2 position)
    {
        if (occupiedBoxCells.ContainsKey(position))
        {
            occupiedBoxCells.Remove(position);
        }
    }

    // If we want to re-apply stats to all towers (so they pick up the new buff factors)
    public void RefreshAllTowersStats()
    {
        foreach (var kvp in occupiedCells)
        {
            Tower t = kvp.Value;
            t.ApplyLevelStats();
            // For ProjectileTowers, that’ll recalc final range/fireRate using the buff factors
        }
    }
}
