using UnityEngine;
using System.Collections.Generic;
using System.Runtime.CompilerServices;
using UnityEngine.Tilemaps;

public class TowerPlacement : MonoBehaviour
{
    public static TowerPlacement Instance;

    public List<GameObject> towerPrefabs; // List of tower prefabs
    public LayerMask placementLayerMask;
    public LayerMask towerSelectionLayerMask; // Layer mask for tower selection cells
    public LayerMask balloonPathLayerMask;
    public LayerMask towerLayerMask;
    public Camera mainCamera;

    private GameObject pendingTower;
    private bool isPlacing = false;
    private int selectedTowerIndex = -1; // No tower selected by default

    public int gridWidth = 16; // Exclude the tower selection column
    public int gridHeight = 10;

    public Tilemap tilemap; // Reference to the tilemap

    public bool IsPlacing => isPlacing; // Public getter for isPlacing

    void Awake()
    {
        if (Instance == null)
            Instance = this;
        else
            Destroy(gameObject);
    }

    void Update()
    {
        if (GameManager.Instance.isGameOver)
            return;

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
        else
        {
            if (Input.GetMouseButtonDown(0))
            {
                DetectTowerSelection();
            }

        }
    }

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


    public void StartPlacement(int towerIndex)
    {
        if (towerIndex < 0 || towerIndex >= towerPrefabs.Count)
        {
            Debug.LogError("Invalid tower index");
            return;
        }

        selectedTowerIndex = towerIndex;

        // Get the tower cost
        //int towerCost = towerPrefabs[selectedTowerIndex].GetComponent<Tower>().cost;
        int towerCost = towerPrefabs[selectedTowerIndex].GetComponent<Tower>().towerData.levels[0].upgradeCost;

        // Check if the player can afford the tower
        if (!GameManager.Instance.CanAfford(towerCost))
        {
            Debug.Log("Not enough currency to place this tower.");
            // Optionally, provide visual feedback
            selectedTowerIndex = -1;
            return;
        }

        isPlacing = true;
        pendingTower = Instantiate(towerPrefabs[selectedTowerIndex]);

        // Set the pending tower to the "PendingTower" layer
        pendingTower.layer = LayerMask.NameToLayer("PendingTower"); // After instantiating the pending tower, set its layer to "PendingTower"
        SetLayerRecursively(pendingTower, LayerMask.NameToLayer("PendingTower")); // Additionally, set the layer for all child objects (if any):


        // Show the range indicator
        Tower pendingTowerScript = pendingTower.GetComponent<Tower>();
        pendingTowerScript.ShowRangeIndicator();
        //Debug.Log("pendingTowerScript: " + pendingTowerScript.rangeIndicator.active);
    }

    void SetLayerRecursively(GameObject obj, int newLayer)
    {
        if (obj == null)
            return;

        obj.layer = newLayer;

        foreach (Transform child in obj.transform)
        {
            if (child == null)
                continue;
            SetLayerRecursively(child.gameObject, newLayer);
        }
    }

    void MovePendingTowerToMouse()
    {
        Vector2 mousePosition = mainCamera.ScreenToWorldPoint(Input.mousePosition);
        //Debug.Log("gridPosition: " + GridManager.Instance);
        Vector2 gridPosition = GridManager.Instance.SnapToGrid(mousePosition);

        pendingTower.transform.position = gridPosition;
    }

    void PlaceTower()
    {
        Vector2 gridPosition = GridManager.Instance.SnapToGrid(pendingTower.transform.position);

        // Check if the position is valid
        if (IsValidPlacement(pendingTower.transform.position))
        {
            Tower towerScript = pendingTower.GetComponent<Tower>();
            towerScript.towerGridPosition = gridPosition; // Store the grid position in the tower

            // Hide the range indicator
            towerScript.HideRangeIndicator();

            GameManager.Instance.SpendCurrency(towerScript.towerData.levels[0].upgradeCost);

            // Mark the cell as occupied with the tower instance
            GameManager.Instance.OccupyCell(gridPosition, towerScript);

            pendingTower.layer = LayerMask.NameToLayer("Tower");

            SetLayerRecursively(pendingTower, LayerMask.NameToLayer("Tower"));

            isPlacing = false;
            pendingTower = null;
        }
        else
        {
            Debug.Log("Invalid placement location!");
        }
    }

    bool IsValidPlacement(Vector2 position)
    {
        Vector2 gridPosition = GridManager.Instance.SnapToGrid(position);

        // Check if position is within the playable grid
        if (!GridManager.Instance.IsWithinGrid(position, gridWidth, gridHeight))
        {
            Debug.Log("Position is outside the playable grid.");
            return false;
        }

        // Check if the position is on the balloon path
        if (IsOnBalloonPath(position))
        {
            Debug.Log("Cannot place tower on the balloon path.");
            return false;
        }

        if (GameManager.Instance.IsCellOccupied(gridPosition))
        {
            Debug.Log("This cell is already occupied by another tower.");
            return false;
        }

        return true;
    }

    bool IsOnBalloonPath(Vector2 position)
    {
        // Convert world position to cell position
        Vector3Int cellPosition = tilemap.WorldToCell(position);

        // Get the tile at the cell position
        TileBase tile = tilemap.GetTile(cellPosition);

        // Check if the tile is a PathTile
        if (tile is PathTile)
        {
            return true; // Position is on the balloon path
        }

        return false; // Position is not on the balloon path
    }

    void CancelPlacement()
    {
        isPlacing = false;
        if (pendingTower != null)
        {
            // Hide the range indicator
            Tower pendingTowerScript = pendingTower.GetComponent<Tower>();
            pendingTowerScript.HideRangeIndicator();

            Destroy(pendingTower);
            pendingTower = null;
        }
    }
}