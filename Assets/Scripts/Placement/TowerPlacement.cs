using UnityEngine;
using System.Collections.Generic;
using UnityEngine.Tilemaps;
using System.Runtime.CompilerServices;

/// <summary>
/// Description:
/// Handles tower placement logic in the game. This class manages the instantiation of a pending tower
/// when the player selects a tower type, moves the pending tower with the mouse, validates placement according to grid bounds,
/// path restrictions, and occupied cells, and finalizes the placement by deducting currency and marking the grid cell as occupied.
/// </summary>
public class TowerPlacement : MonoBehaviour
{
    /// <summary>
    /// Singleton instance of TowerPlacement.
    /// </summary>
    public static TowerPlacement Instance;

    [Header("Tower Prefabs")]
    /// <summary>
    /// List of tower prefab GameObjects. Assign these in the Inspector.
    /// </summary>
    public List<GameObject> towerPrefabs;

    [Header("Layers & Camera")]
    /// <summary>
    /// Layer mask for placement (e.g., ground).
    /// </summary>
    public LayerMask placementLayerMask;
    /// <summary>
    /// Layer mask for tower selection cells.
    /// </summary>
    public LayerMask towerSelectionLayerMask;
    /// <summary>
    /// Layer mask for the balloon path.
    /// </summary>
    public LayerMask balloonPathLayerMask;
    /// <summary>
    /// Layer mask for towers once placed.
    /// </summary>
    public LayerMask towerLayerMask;
    /// <summary>
    /// Main camera used for converting mouse screen position to world position.
    /// </summary>
    public Camera mainCamera;

    [Header("Grid Settings")]
    /// <summary>
    /// Number of grid cells horizontally.
    /// </summary>
    public int gridWidth = 26;
    /// <summary>
    /// Number of grid cells vertically.
    /// </summary>
    public int gridHeight = 14;
    /// <summary>
    /// Tilemap component representing the grid.
    /// </summary>
    public Tilemap tilemap;

    // Pending tower data during placement.
    private GameObject pendingTower;
    private bool isPlacing = false;
    private int selectedTowerIndex = -1; // No tower selected by default

    /// <summary>
    /// Public read-only property indicating whether a tower placement is in progress.
    /// </summary>
    public bool IsPlacing => isPlacing;

    /// <summary>
    /// Predefined list of positions (as grid cell coordinates) that represent the balloon path.
    /// Towers cannot be placed on these cells.
    /// </summary>
    public List<Vector3Int> balloonPathPositions = new List<Vector3Int>
    {
        // Path from waypoint0 (-10, 2, 0) to waypoint1 (9, 2, 0)
        new Vector3Int(-10, 2, 0), new Vector3Int(-9, 2, 0), new Vector3Int(-8, 2, 0), new Vector3Int(-7, 2, 0), new Vector3Int(-6, 2, 0),
        new Vector3Int(-5, 2, 0), new Vector3Int(-4, 2, 0), new Vector3Int(-3, 2, 0), new Vector3Int(-2, 2, 0), new Vector3Int(-1, 2, 0),
        new Vector3Int(0, 2, 0), new Vector3Int(1, 2, 0), new Vector3Int(2, 2, 0), new Vector3Int(3, 2, 0), new Vector3Int(4, 2, 0),
        new Vector3Int(5, 2, 0), new Vector3Int(6, 2, 0), new Vector3Int(7, 2, 0), new Vector3Int(8, 2, 0), new Vector3Int(9, 2, 0),

        // Path from waypoint1 (9, 2, 0) to waypoint2 (9, -3, 0)
        new Vector3Int(9, 1, 0), new Vector3Int(9, 0, 0), new Vector3Int(9, -1, 0), new Vector3Int(9, -2, 0), new Vector3Int(9, -3, 0),

        // Path from waypoint2 (9, -3, 0) to waypoint3 (3, -3, 0)
        new Vector3Int(8, -3, 0), new Vector3Int(7, -3, 0), new Vector3Int(6, -3, 0), new Vector3Int(5, -3, 0), new Vector3Int(4, -3, 0),
        new Vector3Int(3, -3, 0),

        // Path from waypoint3 (3, -3, 0) to waypoint4 (3, 0, 0)
        new Vector3Int(3, -2, 0), new Vector3Int(3, -1, 0), new Vector3Int(3, 0, 0),

        // Path from waypoint4 (3, 0, 0) to waypoint5 (-8, 0, 0)
        new Vector3Int(2, 0, 0), new Vector3Int(1, 0, 0), new Vector3Int(0, 0, 0), new Vector3Int(-1, 0, 0), new Vector3Int(-2, 0, 0),
        new Vector3Int(-3, 0, 0), new Vector3Int(-4, 0, 0), new Vector3Int(-5, 0, 0), new Vector3Int(-6, 0, 0), new Vector3Int(-7, 0, 0),
        new Vector3Int(-8, 0, 0),

        // Path from waypoint5 (-8, 0, 0) to waypoint6 (-8, -4, 0)
        new Vector3Int(-8, -1, 0), new Vector3Int(-8, -2, 0), new Vector3Int(-8, -3, 0), new Vector3Int(-8, -4, 0),

        // Path from waypoint6 (-8, -4, 0) to waypoint7 (0, -4, 0)
        new Vector3Int(-7, -4, 0), new Vector3Int(-6, -4, 0), new Vector3Int(-5, -4, 0), new Vector3Int(-4, -4, 0), new Vector3Int(-3, -4, 0),
        new Vector3Int(-2, -4, 0), new Vector3Int(-1, -4, 0), new Vector3Int(0, -4, 0),

        // Path from waypoint7 (0, -4, 0) to waypoint8 (0, -7, 0)
        new Vector3Int(0, -5, 0), new Vector3Int(0, -6, 0), new Vector3Int(0, -7, 0),

        // Path from waypoint8 (0, -7, 0) to waypoint9 (-10, -7, 0)
        new Vector3Int(-1, -7, 0), new Vector3Int(-2, -7, 0), new Vector3Int(-3, -7, 0), new Vector3Int(-4, -7, 0), new Vector3Int(-5, -7, 0),
        new Vector3Int(-6, -7, 0), new Vector3Int(-7, -7, 0), new Vector3Int(-8, -7, 0), new Vector3Int(-9, -7, 0), new Vector3Int(-10, -7, 0)
    };

    /// <summary>
    /// Unity's Awake method. Initializes the singleton instance.
    /// </summary>
    void Awake()
    {
        if (Instance == null)
            Instance = this;
        else
            Destroy(gameObject);
    }

    /// <summary>
    /// Unity's Update method. If a tower is pending placement, moves it with the mouse cursor and
    /// listens for placement or cancellation input.
    /// </summary>
    void Update()
    {
        if (GameManager.Instance.isGameOver) return;

        if (isPlacing)
        {
            MovePendingTowerToMouse();
            if (Input.GetMouseButtonDown(0))
            {
                PlaceTower();
            }
            else if (Input.GetMouseButtonDown(1))
            {
                CancelPlacement();
            }
        }
    }

    /// <summary>
    /// Detects a tower selection by checking if the mouse is over a tower selection cell. 
    /// If a valid selection is found, starts the tower placement process.
    /// </summary>
    void DetectTowerSelection()
    {
        Vector2 mousePosition = mainCamera.ScreenToWorldPoint(Input.mousePosition);
        Collider2D hitCollider = Physics2D.OverlapPoint(mousePosition, towerSelectionLayerMask);
        if (hitCollider != null)
        {
            TowerSelectionCell selectionCell = hitCollider.GetComponent<TowerSelectionCell>();
            if (selectionCell != null)
            {
                StartPlacement(selectionCell.towerIndex);
            }
        }
    }

    /// <summary>
    /// Initiates the tower placement process for the given tower index.
    /// Validates the index, checks player currency, instantiates a pending tower, and displays its range indicator.
    /// </summary>
    /// <param name="towerIndex">Index of the tower prefab to place.</param>
    public void StartPlacement(int towerIndex)
    {
        if (towerIndex < 0 || towerIndex >= towerPrefabs.Count)
        {
            Debug.LogError("Invalid tower index");
            return;
        }

        selectedTowerIndex = towerIndex;
        var towerPrefab = towerPrefabs[selectedTowerIndex];
        int towerCost = towerPrefab.GetComponent<Tower>().towerData.levels[0].upgradeCost;

        // Check if the player can afford the tower.
        if (!GameManager.Instance.CanAfford(towerCost))
        {
            Debug.Log("Not enough currency to place this tower.");
            selectedTowerIndex = -1;
            return;
        }

        // Instantiate pending tower and mark placement mode as active.
        isPlacing = true;
        pendingTower = Instantiate(towerPrefab);
        pendingTower.layer = LayerMask.NameToLayer("PendingTower");
        TowerPlacementUtils.SetLayerRecursively(pendingTower, LayerMask.NameToLayer("PendingTower"));

        // Show the range indicator for the pending tower.
        Tower pendingTowerScript = pendingTower.GetComponent<Tower>();
        pendingTowerScript.ShowRangeIndicator();
    }

    /// <summary>
    /// Moves the pending tower to the current mouse position, snapped to the grid.
    /// </summary>
    private void MovePendingTowerToMouse()
    {
        Vector2 mousePosition = mainCamera.ScreenToWorldPoint(Input.mousePosition);
        Vector2 gridPosition = GridManager.Instance.SnapToGrid(mousePosition);
        pendingTower.transform.position = gridPosition;
    }

    /// <summary>
    /// Attempts to place the pending tower at its current position.
    /// Validates the placement location and finalizes tower placement if valid.
    /// </summary>
    private void PlaceTower()
    {
        Vector2 placementPos = pendingTower.transform.position;
        if (IsValidPlacement(placementPos))
        {
            PerformPlacement(placementPos);
        }
        else
        {
            Debug.Log("Invalid placement location!");
        }
    }

    /// <summary>
    /// Finalizes tower placement: snaps to grid, updates tower properties, deducts cost,
    /// marks the grid cell as occupied, sets the tower layer, and plays placement audio.
    /// </summary>
    /// <param name="placementPos">The position where the tower is placed.</param>
    private void PerformPlacement(Vector2 placementPos)
    {
        Vector2 gridPosition = GridManager.Instance.SnapToGrid(placementPos);
        Tower towerScript = pendingTower.GetComponent<Tower>();

        // Finalize tower position and grid data.
        towerScript.towerGridPosition = gridPosition;
        towerScript.HideRangeIndicator();
        towerScript.isFullyPlaced = true;

        // Deduct cost from player's currency.
        int cost = towerScript.towerData.levels[0].upgradeCost;
        GameManager.Instance.SpendCurrency(cost);

        // Mark the grid cell as occupied.
        GameManager.Instance.cellManager.OccupyCell(gridPosition, towerScript);

        // Set final tower layer.
        pendingTower.layer = LayerMask.NameToLayer("Tower");
        TowerPlacementUtils.SetLayerRecursively(pendingTower, LayerMask.NameToLayer("Tower"));

        // If the tower is a VillageTower, refresh towers within its range.
        VillageTower vTower = towerScript as VillageTower;
        if (vTower != null)
        {
            // This one-time call ensures we buff any towers that were already inside the collider
            vTower.RefreshTowersInRange();
        }

        AudioManager.Instance.PlayTowerPlacement();

        // Clear pending placement mode.
        isPlacing = false;
        pendingTower = null;
    }

    /// <summary>
    /// Validates if the given position is a valid placement location for a tower.
    /// Checks grid bounds, ensures the position is not on the balloon path, and verifies the cell is unoccupied.
    /// </summary>
    /// <param name="position">The world position to validate.</param>
    /// <returns>True if placement is valid; otherwise, false.</returns>
    private bool IsValidPlacement(Vector2 position)
    {
        // 1) Check if the position is within grid bounds.
        if (!GridManager.Instance.IsWithinGrid(position, gridWidth, gridHeight))
        {
            Debug.Log("Position is outside the playable grid.");
            return false;
        }

        // 2) Check if the position is on the balloon path.
        bool onPath = TowerPlacementUtils.IsOnBalloonPath(position, tilemap, balloonPathPositions);
        if (onPath)
        {
            Debug.Log("Cannot place tower on the balloon path.");
            return false;
        }

        // 3) Check if the cell (snapped to grid) is already occupied.
        Vector2 gridPos = GridManager.Instance.SnapToGrid(position);
        if (GameManager.Instance.cellManager.IsCellOccupiedForAnyReason(gridPos))
        {
            Debug.Log("This cell is already occupied.");
            return false;
        }

        return true;
    }

    /// <summary>
    /// Cancels the current pending tower placement, hides any visual indicators, and destroys the pending tower GameObject.
    /// </summary>
    void CancelPlacement()
    {
        isPlacing = false;
        if (pendingTower != null)
        {
            Tower pendingTowerScript = pendingTower.GetComponent<Tower>();
            pendingTowerScript.HideRangeIndicator();
            Destroy(pendingTower);
            pendingTower = null;
        }
    }
}
