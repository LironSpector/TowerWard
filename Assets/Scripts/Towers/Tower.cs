//using UnityEngine;

//public class Tower : MonoBehaviour
//{
//    public TowerData towerData; // Reference to the tower's data
//    public int level = 1;       // Current level
//    public int maxLevel;        // Maximum level (set from towerData)

//    protected BaseTowerShooting towerShooting;  // We'll reference the base shooting script
//    protected CircleCollider2D rangeCollider;   // For shooting range detection

//    public Vector2 towerGridPosition; // Track this tower's grid position
//    public GameObject rangeIndicator;

//    protected virtual void Awake()
//    {
//        // Find the RangeIndicator child object
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

//    protected virtual void Start()
//    {
//        if (towerData == null)
//        {
//            Debug.LogError("TowerData is not assigned for " + gameObject.name);
//            return;
//        }

//        maxLevel = towerData.levels.Length;

//        towerShooting = GetComponent<BaseTowerShooting>();

//        // Get the range collider for shooting range
//        rangeCollider = GetComponent<CircleCollider2D>();
//        if (rangeCollider != null)
//        {
//            rangeCollider.isTrigger = true;
//        }

//        ApplyLevelStats();
//    }

//    protected virtual void Update()
//    {
//        //Debug.Log("From within Tower.cs:" + towerGridPosition);
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
//        return 0; // No cost if max level reached
//    }

//    public virtual void Upgrade()
//    {
//        if (CanUpgrade())
//        {
//            level++;
//            ApplyLevelStats();
//        }
//    }

//    protected virtual void ApplyLevelStats()
//    {
//        if (towerShooting != null && towerData != null && level <= towerData.levels.Length)
//        {
//            TowerLevelData levelData = towerData.levels[level - 1];
//            towerShooting.range = levelData.range;
//            towerShooting.fireRate = levelData.fireRate;
//            towerShooting.damage = levelData.damage;
//            UpdateRangeIndicator();
//        }
//    }

//    void DetectTowerSelection()
//    {
//        if (GameManager.Instance.isGameOver)
//            return;

//        if (Input.GetMouseButtonDown(0))
//        {
//            Vector2 mousePosition = Camera.main.ScreenToWorldPoint(Input.mousePosition);
//            Vector2 gridPosition = GridManager.Instance.SnapToGrid(mousePosition);

//            Debug.Log("gridPosition: " + gridPosition + ", towerGridPosition: " + towerGridPosition);
//            if (gridPosition == towerGridPosition)
//            {
//                UIManager.Instance.ShowTowerPanel(this);
//            }
//        }
//    }

//    public int GetSellValue()
//    {
//        int totalCost = 0;
//        // Add the cost of the initial tower
//        totalCost += towerData.levels[0].upgradeCost;
//        // Add the cost of upgrades
//        for (int i = 1; i < level; i++)
//        {
//            totalCost += towerData.levels[i].upgradeCost;
//        }
//        return totalCost / 2;
//    }

//    protected void UpdateRangeIndicator()
//    {
//        if (rangeCollider != null)
//        {
//            rangeCollider.radius = towerShooting.range;
//        }

//        if (rangeIndicator != null)
//        {
//            float diameter = towerShooting.range * 2f;
//            rangeIndicator.transform.localScale = new Vector3(diameter, diameter, 1f);
//        }
//    }

//    public void ShowRangeIndicator()
//    {
//        if (rangeIndicator != null)
//        {
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
    public TowerData towerData;
    public int level = 1;
    public int maxLevel;

    // We remove the towerShooting reference from here!
    protected CircleCollider2D rangeCollider;

    public Vector2 towerGridPosition;
    public GameObject rangeIndicator;

    public bool isFullyPlaced = false;

    protected virtual void Awake()
    {
        // Find the RangeIndicator child object
        if (rangeIndicator == null)
        {
            Transform rangeIndicatorTransform = transform.Find("RangeIndicator");
            if (rangeIndicatorTransform != null)
            {
                rangeIndicator = rangeIndicatorTransform.gameObject;
                rangeIndicator.SetActive(false);
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

        // If we want a range for AoE or something, set up a circle collider
        rangeCollider = GetComponent<CircleCollider2D>();
        if (rangeCollider != null)
        {
            rangeCollider.isTrigger = true;
        }

        // Let the base tower apply stats or do something minimal
        ApplyLevelStats();
    }

    protected virtual void Update()
    {
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
        return 0;
    }

    public virtual void Upgrade()
    {
        if (CanUpgrade())
        {
            level++;
            ApplyLevelStats();
        }
    }

    /// <summary>
    /// Base method for applying tower level stats.
    /// Projectile towers override it to set shooting stats, 
    /// Non-projectile towers might override it if needed for special logic.
    /// </summary>
    protected virtual void ApplyLevelStats()
    {
        // By default, do nothing or maybe set rangeCollider
        if (towerData == null || level > towerData.levels.Length) return;

        TowerLevelData levelData = towerData.levels[level - 1];

        // For towers that do area-of-effect or aura, we might still want to do:
        if (rangeCollider != null)
        {
            rangeCollider.radius = levelData.range;
        }

        // Update range indicator visuals
        if (rangeIndicator != null)
        {
            float diameter = levelData.range * 2f;
            rangeIndicator.transform.localScale = new Vector3(diameter, diameter, 1f);
        }
    }

    void DetectTowerSelection()
    {
        if (GameManager.Instance.isGameOver) return;

        if (Input.GetMouseButtonDown(0))
        {
            Vector2 mousePosition = Camera.main.ScreenToWorldPoint(Input.mousePosition);
            Vector2 gridPosition = GridManager.Instance.SnapToGrid(mousePosition);

            // If the user clicked exactly on this tower's grid position
            if (gridPosition == towerGridPosition)
            {
                UIManager.Instance.ShowTowerPanel(this);
            }
        }
    }

    public int GetSellValue()
    {
        int totalCost = 0;
        totalCost += towerData.levels[0].upgradeCost;
        for (int i = 1; i < level; i++)
        {
            totalCost += towerData.levels[i].upgradeCost;
        }
        return totalCost / 2;
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
