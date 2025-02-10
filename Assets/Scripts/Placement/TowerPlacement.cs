//New - after making structure better
using UnityEngine;
using System.Collections.Generic;
using UnityEngine.Tilemaps;
using System.Runtime.CompilerServices;

public class TowerPlacement : MonoBehaviour
{
    public static TowerPlacement Instance;

    [Header("Tower Prefabs")]
    public List<GameObject> towerPrefabs;

    [Header("Layers & Camera")]
    public LayerMask placementLayerMask;
    public LayerMask towerSelectionLayerMask; // Layer mask for tower selection cells
    public LayerMask balloonPathLayerMask;
    public LayerMask towerLayerMask;
    public Camera mainCamera;

    [Header("Grid Settings")]
    public int gridWidth = 26;
    public int gridHeight = 14;
    public Tilemap tilemap;

    private GameObject pendingTower;
    private bool isPlacing = false;
    private int selectedTowerIndex = -1; // No tower selected by default

    public bool IsPlacing => isPlacing; // Public getter for isPlacing

    // For storing the path cells in one list
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

    void Awake()
    {
        if (Instance == null) Instance = this;
        else Destroy(gameObject);
    }

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
        // else
        // {
        //     if (Input.GetMouseButtonDown(0))
        //     {
        //         DetectTowerSelection();
        //     }
        // }
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
        var towerPrefab = towerPrefabs[selectedTowerIndex];
        int towerCost = towerPrefab.GetComponent<Tower>().towerData.levels[0].upgradeCost;

        // Check if the player can afford the tower
        if (!GameManager.Instance.CanAfford(towerCost))
        {
            Debug.Log("Not enough currency to place this tower.");
            selectedTowerIndex = -1;
            return;
        }

        // Instantiate
        isPlacing = true;
        pendingTower = Instantiate(towerPrefab);
        pendingTower.layer = LayerMask.NameToLayer("PendingTower"); // After instantiating the pending tower, set its layer to "PendingTower"
        TowerPlacementUtils.SetLayerRecursively(pendingTower, LayerMask.NameToLayer("PendingTower")); // Additionally, set the layer for all child objects (if any)

        // Show the range indicator while “pending”
        Tower pendingTowerScript = pendingTower.GetComponent<Tower>();
        pendingTowerScript.ShowRangeIndicator();
    }

    private void MovePendingTowerToMouse()
    {
        Vector2 mousePosition = mainCamera.ScreenToWorldPoint(Input.mousePosition);
        Vector2 gridPosition = GridManager.Instance.SnapToGrid(mousePosition);
        pendingTower.transform.position = gridPosition;
    }

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

    private void PerformPlacement(Vector2 placementPos)
    {
        Vector2 gridPosition = GridManager.Instance.SnapToGrid(placementPos);

        Tower towerScript = pendingTower.GetComponent<Tower>();

        // Finalize position & grid data
        towerScript.towerGridPosition = gridPosition;
        towerScript.HideRangeIndicator();
        towerScript.isFullyPlaced = true;

        // Spend currency
        int cost = towerScript.towerData.levels[0].upgradeCost;
        GameManager.Instance.SpendCurrency(cost);

        // Mark cell as occupied
        GameManager.Instance.OccupyCell(gridPosition, towerScript);

        // Set final layer "Tower"
        pendingTower.layer = LayerMask.NameToLayer("Tower");
        TowerPlacementUtils.SetLayerRecursively(pendingTower, LayerMask.NameToLayer("Tower"));

        // If it's a VillageTower, refresh the towers in range
        VillageTower vTower = towerScript as VillageTower;
        if (vTower != null)
        {
            // This one-time call ensures we buff any towers that were already inside the collider
            vTower.RefreshTowersInRange();
        }

        AudioManager.Instance.PlayTowerPlacement();

        // Clear pending
        isPlacing = false;
        pendingTower = null;
    }

    private bool IsValidPlacement(Vector2 position)
    {
        // 1) Check if within grid bounds
        if (!GridManager.Instance.IsWithinGrid(position, gridWidth, gridHeight))
        {
            Debug.Log("Position is outside the playable grid.");
            return false;
        }

        // 2) Check if on balloon path
        bool onPath = TowerPlacementUtils.IsOnBalloonPath(position, tilemap, balloonPathPositions);
        if (onPath)
        {
            Debug.Log("Cannot place tower on the balloon path.");
            return false;
        }

        // 3) Check if cell is already occupied
        Vector2 gridPos = GridManager.Instance.SnapToGrid(position);
        if (GameManager.Instance.IsCellOccupiedForAnyReason(gridPos))
        {
            Debug.Log("This cell is already occupied.");
            return false;
        }

        return true;
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










//Old - before making structure better
//using UnityEngine;
//using System.Collections.Generic;
//using System.Runtime.CompilerServices;
//using UnityEngine.Tilemaps;

//public class TowerPlacement : MonoBehaviour
//{
//    public static TowerPlacement Instance;

//    public List<GameObject> towerPrefabs; // List of tower prefabs
//    public LayerMask placementLayerMask;
//    public LayerMask towerSelectionLayerMask; // Layer mask for tower selection cells
//    public LayerMask balloonPathLayerMask;
//    public LayerMask towerLayerMask;
//    public Camera mainCamera;

//    private GameObject pendingTower;
//    private bool isPlacing = false;
//    private int selectedTowerIndex = -1; // No tower selected by default

//    //public int gridWidth = 16; // Exclude the tower selection column
//    //public int gridHeight = 10;
//    public int gridWidth = 26; // Exclude the tower selection column
//    public int gridHeight = 14;

//    public Tilemap tilemap; // Reference to the tilemap

//    public bool IsPlacing => isPlacing; // Public getter for isPlacing

//    void Awake()
//    {
//        if (Instance == null)
//            Instance = this;
//        else
//            Destroy(gameObject);
//    }

//    void Update()
//    {
//        if (GameManager.Instance.isGameOver)
//            return;

//        if (isPlacing)
//        {
//            MovePendingTowerToMouse();
//            if (Input.GetMouseButtonDown(0))
//            {
//                PlaceTower();
//            }
//            else if (Input.GetMouseButtonDown(1))
//            {
//                CancelPlacement();
//            }
//        }
//        //else
//        //{
//        //    if (Input.GetMouseButtonDown(0))
//        //    {
//        //        DetectTowerSelection();
//        //    }

//        //}
//    }

//    void DetectTowerSelection()
//    {
//        Vector2 mousePosition = mainCamera.ScreenToWorldPoint(Input.mousePosition);
//        Collider2D hitCollider = Physics2D.OverlapPoint(mousePosition, towerSelectionLayerMask);
//        if (hitCollider != null)
//        {
//            TowerSelectionCell selectionCell = hitCollider.GetComponent<TowerSelectionCell>();
//            if (selectionCell != null)
//            {
//                StartPlacement(selectionCell.towerIndex);
//            }
//        }
//    }


//    public void StartPlacement(int towerIndex)
//    {
//        if (towerIndex < 0 || towerIndex >= towerPrefabs.Count)
//        {
//            Debug.LogError("Invalid tower index");
//            return;
//        }

//        selectedTowerIndex = towerIndex;

//        // Get the tower cost
//        //int towerCost = towerPrefabs[selectedTowerIndex].GetComponent<Tower>().cost;
//        int towerCost = towerPrefabs[selectedTowerIndex].GetComponent<Tower>().towerData.levels[0].upgradeCost;

//        // Check if the player can afford the tower
//        if (!GameManager.Instance.CanAfford(towerCost))
//        {
//            Debug.Log("Not enough currency to place this tower.");
//            // Optionally, provide visual feedback
//            selectedTowerIndex = -1;
//            return;
//        }

//        isPlacing = true;
//        pendingTower = Instantiate(towerPrefabs[selectedTowerIndex]);

//        // Set the pending tower to the "PendingTower" layer
//        pendingTower.layer = LayerMask.NameToLayer("PendingTower"); // After instantiating the pending tower, set its layer to "PendingTower"
//        SetLayerRecursively(pendingTower, LayerMask.NameToLayer("PendingTower")); // Additionally, set the layer for all child objects (if any):


//        // Show the range indicator
//        Tower pendingTowerScript = pendingTower.GetComponent<Tower>();
//        pendingTowerScript.ShowRangeIndicator();
//        //Debug.Log("pendingTowerScript: " + pendingTowerScript.rangeIndicator.active);
//    }

//    void SetLayerRecursively(GameObject obj, int newLayer)
//    {
//        if (obj == null)
//            return;

//        obj.layer = newLayer;

//        foreach (Transform child in obj.transform)
//        {
//            if (child == null)
//                continue;
//            SetLayerRecursively(child.gameObject, newLayer);
//        }
//    }

//    void MovePendingTowerToMouse()
//    {
//        Vector2 mousePosition = mainCamera.ScreenToWorldPoint(Input.mousePosition);
//        //Debug.Log("Mouse position is: " + mousePosition);

//        //Debug.Log("gridPosition: " + GridManager.Instance);
//        Vector2 gridPosition = GridManager.Instance.SnapToGrid(mousePosition);
//        //Debug.Log("gridPosition: " + gridPosition);

//        pendingTower.transform.position = gridPosition;
//    }

//    void PlaceTower()
//    {
//        Vector2 gridPosition = GridManager.Instance.SnapToGrid(pendingTower.transform.position);
//        Debug.Log("gridPosition: " + gridPosition);


//        // Check if the position is valid
//        if (IsValidPlacement(pendingTower.transform.position))
//        {
//            Tower towerScript = pendingTower.GetComponent<Tower>();
//            Debug.Log("Valid placement in indeed. Storing in Tower: " + towerScript);
//            //Debug.Log("Is he null? " + (towerScript == null));
//            towerScript.towerGridPosition = gridPosition; // Store the grid position in the tower
//            Debug.Log("Stored position: " + towerScript.towerGridPosition);

//            // Hide the range indicator
//            towerScript.HideRangeIndicator();

//            GameManager.Instance.SpendCurrency(towerScript.towerData.levels[0].upgradeCost);

//            // Mark the cell as occupied with the tower instance
//            GameManager.Instance.OccupyCell(gridPosition, towerScript);

//            pendingTower.layer = LayerMask.NameToLayer("Tower");

//            SetLayerRecursively(pendingTower, LayerMask.NameToLayer("Tower"));

//            towerScript.isFullyPlaced = true;

//            AudioManager.Instance.PlayTowerPlacement();

//            // If it's a VillageTower, refresh the towers in range
//            VillageTower vTower = towerScript as VillageTower;
//            if (vTower != null)
//            {
//                // This one-time call ensures we buff any towers that were already inside the collider
//                vTower.RefreshTowersInRange();
//            }

//            isPlacing = false;
//            pendingTower = null;
//        }
//        else
//        {
//            Debug.Log("Invalid placement location!");
//        }
//    }

//    bool IsValidPlacement(Vector2 position)
//    {
//        Vector2 gridPosition = GridManager.Instance.SnapToGrid(position);

//        // Check if position is within the playable grid
//        if (!GridManager.Instance.IsWithinGrid(position, gridWidth, gridHeight))
//        {
//            Debug.Log("Position is outside the playable grid.");
//            return false;
//        }

//        // Check if the position is on the balloon path
//        if (IsOnBalloonPath(position))
//        {
//            Debug.Log("Cannot place tower on the balloon path.");
//            return false;
//        }


//        if (GameManager.Instance.IsCellOccupiedForAnyReason(gridPosition))
//        {
//            Debug.Log("This cell is already occupied by another tower.");
//            return false;
//        }

//        //if (GameManager.Instance.IsCellOccupied(gridPosition))
//        //{
//        //    Debug.Log("This cell is already occupied by another tower.");
//        //    return false;
//        //}

//        return true;
//    }

//    //bool IsOnBalloonPath(Vector2 position)
//    //{
//    //    // Convert world position to cell position
//    //    Vector3Int cellPosition = tilemap.WorldToCell(position);
//    //    Debug.Log("Cell position is this: " + cellPosition);
//    //    Debug.Log("Cell position type: " + cellPosition.GetType());

//    //    // Get the tile at the cell position
//    //    TileBase tile = tilemap.GetTile(cellPosition);

//    //    // Check if the tile is a PathTile
//    //    if (tile is PathTile)
//    //    {
//    //        return true; // Position is on the balloon path
//    //    }

//    //    return false; // Position is not on the balloon path
//    //}

//    public bool IsOnBalloonPath(Vector2 position)
//    {
//        List<Vector3Int> balloonPathPositions = new List<Vector3Int>
//        {
//            // Path from waypoint0 (-10, 2, 0) to waypoint1 (9, 2, 0)
//            new Vector3Int(-10, 2, 0), new Vector3Int(-9, 2, 0), new Vector3Int(-8, 2, 0), new Vector3Int(-7, 2, 0), new Vector3Int(-6, 2, 0),
//            new Vector3Int(-5, 2, 0), new Vector3Int(-4, 2, 0), new Vector3Int(-3, 2, 0), new Vector3Int(-2, 2, 0), new Vector3Int(-1, 2, 0),
//            new Vector3Int(0, 2, 0), new Vector3Int(1, 2, 0), new Vector3Int(2, 2, 0), new Vector3Int(3, 2, 0), new Vector3Int(4, 2, 0),
//            new Vector3Int(5, 2, 0), new Vector3Int(6, 2, 0), new Vector3Int(7, 2, 0), new Vector3Int(8, 2, 0), new Vector3Int(9, 2, 0),

//            // Path from waypoint1 (9, 2, 0) to waypoint2 (9, -3, 0)
//            new Vector3Int(9, 1, 0), new Vector3Int(9, 0, 0), new Vector3Int(9, -1, 0), new Vector3Int(9, -2, 0), new Vector3Int(9, -3, 0),

//            // Path from waypoint2 (9, -3, 0) to waypoint3 (3, -3, 0)
//            new Vector3Int(8, -3, 0), new Vector3Int(7, -3, 0), new Vector3Int(6, -3, 0), new Vector3Int(5, -3, 0), new Vector3Int(4, -3, 0),
//            new Vector3Int(3, -3, 0),

//            // Path from waypoint3 (3, -3, 0) to waypoint4 (3, 0, 0)
//            new Vector3Int(3, -2, 0), new Vector3Int(3, -1, 0), new Vector3Int(3, 0, 0),

//            // Path from waypoint4 (3, 0, 0) to waypoint5 (-8, 0, 0)
//            new Vector3Int(2, 0, 0), new Vector3Int(1, 0, 0), new Vector3Int(0, 0, 0), new Vector3Int(-1, 0, 0), new Vector3Int(-2, 0, 0),
//            new Vector3Int(-3, 0, 0), new Vector3Int(-4, 0, 0), new Vector3Int(-5, 0, 0), new Vector3Int(-6, 0, 0), new Vector3Int(-7, 0, 0),
//            new Vector3Int(-8, 0, 0),

//            // Path from waypoint5 (-8, 0, 0) to waypoint6 (-8, -4, 0)
//            new Vector3Int(-8, -1, 0), new Vector3Int(-8, -2, 0), new Vector3Int(-8, -3, 0), new Vector3Int(-8, -4, 0),

//            // Path from waypoint6 (-8, -4, 0) to waypoint7 (0, -4, 0)
//            new Vector3Int(-7, -4, 0), new Vector3Int(-6, -4, 0), new Vector3Int(-5, -4, 0), new Vector3Int(-4, -4, 0), new Vector3Int(-3, -4, 0),
//            new Vector3Int(-2, -4, 0), new Vector3Int(-1, -4, 0), new Vector3Int(0, -4, 0),

//            // Path from waypoint7 (0, -4, 0) to waypoint8 (0, -7, 0)
//            new Vector3Int(0, -5, 0), new Vector3Int(0, -6, 0), new Vector3Int(0, -7, 0),

//            // Path from waypoint8 (0, -7, 0) to waypoint9 (-10, -7, 0)
//            new Vector3Int(-1, -7, 0), new Vector3Int(-2, -7, 0), new Vector3Int(-3, -7, 0), new Vector3Int(-4, -7, 0), new Vector3Int(-5, -7, 0),
//            new Vector3Int(-6, -7, 0), new Vector3Int(-7, -7, 0), new Vector3Int(-8, -7, 0), new Vector3Int(-9, -7, 0), new Vector3Int(-10, -7, 0)
//        };

//        Vector3Int cellPosition = tilemap.WorldToCell(position);
//        //Debug.Log("Cell position is this: " + cellPosition);
//        //Debug.Log("Cell position type: " + cellPosition.GetType());

//        return balloonPathPositions.Contains(cellPosition);
//    }



//    void CancelPlacement()
//    {
//        isPlacing = false;
//        if (pendingTower != null)
//        {
//            // Hide the range indicator
//            Tower pendingTowerScript = pendingTower.GetComponent<Tower>();
//            pendingTowerScript.HideRangeIndicator();

//            Destroy(pendingTower);
//            pendingTower = null;
//        }
//    }
//}