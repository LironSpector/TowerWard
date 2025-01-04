//using UnityEngine;

//public class Tower : MonoBehaviour
//{
//    public TowerData towerData; // Reference to the tower's data
//    public int level = 1; // Current level
//    public int maxLevel; // Maximum level (set from towerData)

//    private TowerShooting towerShooting;
//    private CircleCollider2D rangeCollider; // For shooting range detection

//    public Vector2 towerGridPosition; // Track this tower's grid position

//    public GameObject rangeIndicator;


//    void Awake()
//    {
//        // Find the RangeIndicator child object (it's in the Awake() method and not Start() because it needs to happen at the very first moment
//        // after the object is created, and not even 1 frame afterwards)
//        if (rangeIndicator == null)
//        {
//            Transform rangeIndicatorTransform = transform.Find("RangeIndicator");
//            if (rangeIndicatorTransform != null)
//            {
//                rangeIndicator = rangeIndicatorTransform.gameObject;
//                rangeIndicator.SetActive(false); // Hide by default
//            }
//            else
//            {
//                Debug.LogWarning("RangeIndicator child object not found in Tower prefab.");
//            }
//        }
//    }

//    void Start()
//    {
//        if (towerData == null)
//        {
//            Debug.LogError("TowerData is not assigned for " + gameObject.name);
//            return;
//        }

//        maxLevel = towerData.levels.Length;
//        towerShooting = GetComponent<TowerShooting>();

//        // Get the range collider for shooting range
//        rangeCollider = GetComponent<CircleCollider2D>();
//        if (rangeCollider != null)
//        {
//            rangeCollider.isTrigger = true; // Ensure this is a trigger
//        }

//        ApplyLevelStats();
//    }

//    void Update()
//    {
//        DetectTowerSelection();
//    }

//    public bool CanUpgrade()
//    {
//        return level < maxLevel;
//    }

//    public int GetUpgradeCost()
//    {
//        if (CanUpgrade())
//        {
//            return towerData.levels[level].upgradeCost;
//        }
//        else
//        {
//            return 0; // No cost if max level reached
//        }
//    }

//    public void Upgrade()
//    {
//        if (CanUpgrade())
//        {
//            level++;
//            ApplyLevelStats();
//        }
//    }

//    void ApplyLevelStats()
//    {
//        if (towerShooting != null)
//        {
//            TowerLevelData levelData = towerData.levels[level - 1];
//            towerShooting.range = levelData.range;
//            towerShooting.fireRate = levelData.fireRate;
//            towerShooting.damage = levelData.damage;
//            UpdateRangeIndicator();
//        }
//    }

//    // Detect tower selection using the grid-based system
//    void DetectTowerSelection()
//    {
//        if (GameManager.Instance.isGameOver)
//            return;

//        if (Input.GetMouseButtonDown(0)) // Left-click
//        {
//            Vector2 mousePosition = Camera.main.ScreenToWorldPoint(Input.mousePosition);
//            Vector2 gridPosition = GridManager.Instance.SnapToGrid(mousePosition);
//            //Debug.Log("mousePosition: " + mousePosition + ", gridPosition: " + gridPosition + ", towerGridPosition: " + towerGridPosition);

//            // Check if this tower is located at the clicked position
//            if (gridPosition == towerGridPosition)
//            {
//                // If this tower is at the clicked grid position, show the upgrade panel
//                UIManager.Instance.ShowTowerPanel(this); // Show the upgrade panel for this tower
//            }
//        }
//    }

//    public int GetSellValue()
//    {
//        int totalCost = 0;

//        // Add the cost of the initial tower
//        totalCost += towerData.levels[0].upgradeCost;

//        // Add the cost of upgrades up to the current level
//        for (int i = 1; i < level; i++)
//        {
//            totalCost += towerData.levels[i].upgradeCost;
//        }

//        // Return half of the total cost (rounded down)
//        return totalCost / 2;
//    }


//    //void UpdateRangeIndicator()
//    //{
//    //    if (rangeCollider != null)
//    //    {
//    //        rangeCollider.radius = towerShooting.range;
//    //    }
//    //}

//    void UpdateRangeIndicator()
//    {
//        if (rangeCollider != null)
//        {
//            rangeCollider.radius = towerShooting.range;
//        }

//        if (rangeIndicator != null)
//        {
//            float diameter = towerShooting.range * 2f;
//            //Debug.Log("diameter: " + diameter);
//            rangeIndicator.transform.localScale = new Vector3(diameter, diameter, 1f);
//        }
//    }

//    public void ShowRangeIndicator()
//    {
//        //Debug.Log("Showing RangeIndicator");
//        if (rangeIndicator != null)
//        {
//            //Debug.Log("and it's not null!");
//            rangeIndicator.SetActive(true);
//        }
//    }

//    public void HideRangeIndicator()
//    {
//        if (rangeIndicator != null)
//        {
//            rangeIndicator.SetActive(false);
//        }
//    }


//}













using UnityEngine;

public class Tower : MonoBehaviour
{
    public TowerData towerData; // Reference to the tower's data
    public int level = 1;       // Current level
    public int maxLevel;        // Maximum level (set from towerData)

    protected BaseTowerShooting towerShooting;  // We'll reference the base shooting script
    protected CircleCollider2D rangeCollider;   // For shooting range detection

    public Vector2 towerGridPosition; // Track this tower's grid position
    public GameObject rangeIndicator;

    protected virtual void Awake()
    {
        // Find the RangeIndicator child object
        if (rangeIndicator == null)
        {
            Transform rangeIndicatorTransform = transform.Find("RangeIndicator");
            if (rangeIndicatorTransform != null)
            {
                rangeIndicator = rangeIndicatorTransform.gameObject;
                rangeIndicator.SetActive(false); // Hide by default
            }
            else
            {
                Debug.LogWarning("RangeIndicator child object not found in Tower prefab.");
            }
        }
    }

    protected virtual void Start()
    {
        if (towerData == null)
        {
            Debug.LogError("TowerData is not assigned for " + gameObject.name);
            return;
        }

        maxLevel = towerData.levels.Length;

        towerShooting = GetComponent<BaseTowerShooting>();

        // Get the range collider for shooting range
        rangeCollider = GetComponent<CircleCollider2D>();
        if (rangeCollider != null)
        {
            rangeCollider.isTrigger = true;
        }

        ApplyLevelStats();
    }

    protected virtual void Update()
    {
        //Debug.Log("From within Tower.cs:" + towerGridPosition);
        DetectTowerSelection();
    }

    public bool CanUpgrade()
    {
        return level < maxLevel;
    }

    public int GetUpgradeCost()
    {
        if (CanUpgrade())
        {
            return towerData.levels[level].upgradeCost;
        }
        return 0; // No cost if max level reached
    }

    public virtual void Upgrade()
    {
        if (CanUpgrade())
        {
            level++;
            ApplyLevelStats();
        }
    }

    protected virtual void ApplyLevelStats()
    {
        if (towerShooting != null && towerData != null && level <= towerData.levels.Length)
        {
            TowerLevelData levelData = towerData.levels[level - 1];
            towerShooting.range = levelData.range;
            towerShooting.fireRate = levelData.fireRate;
            towerShooting.damage = levelData.damage;
            UpdateRangeIndicator();
        }
    }

    void DetectTowerSelection()
    {
        if (GameManager.Instance.isGameOver)
            return;

        if (Input.GetMouseButtonDown(0))
        {
            Vector2 mousePosition = Camera.main.ScreenToWorldPoint(Input.mousePosition);
            Vector2 gridPosition = GridManager.Instance.SnapToGrid(mousePosition);

            Debug.Log("gridPosition: " + gridPosition + ", towerGridPosition: " + towerGridPosition);
            if (gridPosition == towerGridPosition)
            {
                UIManager.Instance.ShowTowerPanel(this);
            }
        }
    }

    public int GetSellValue()
    {
        int totalCost = 0;
        // Add the cost of the initial tower
        totalCost += towerData.levels[0].upgradeCost;
        // Add the cost of upgrades
        for (int i = 1; i < level; i++)
        {
            totalCost += towerData.levels[i].upgradeCost;
        }
        return totalCost / 2;
    }

    protected void UpdateRangeIndicator()
    {
        if (rangeCollider != null)
        {
            rangeCollider.radius = towerShooting.range;
        }

        if (rangeIndicator != null)
        {
            float diameter = towerShooting.range * 2f;
            rangeIndicator.transform.localScale = new Vector3(diameter, diameter, 1f);
        }
    }

    public void ShowRangeIndicator()
    {
        if (rangeIndicator != null)
        {
            rangeIndicator.SetActive(true);
        }
    }

    public void HideRangeIndicator()
    {
        if (rangeIndicator != null)
        {
            rangeIndicator.SetActive(false);
        }
    }
}
