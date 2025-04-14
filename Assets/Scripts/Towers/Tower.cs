using UnityEngine;

/// <summary>
/// Description:
/// Represents a tower in the game. The Tower class holds essential information such as its current level,
/// associated TowerData, range, and grid position. It also manages tower upgrades, selling, and selection via UI.
/// This class is designed to be extended by specific tower types (e.g., projectile towers, non-projectile towers)
/// that may override methods like ApplyLevelStats() or Upgrade() to implement specialized behavior.
/// </summary>
public class Tower : MonoBehaviour
{
    /// <summary>
    /// Data that defines the tower's attributes across levels.
    /// </summary>
    public TowerData towerData;

    /// <summary>
    /// The current level of the tower. Levels start at 1.
    /// </summary>
    public int level = 1;

    /// <summary>
    /// The maximum level attainable by the tower, determined by the length of TowerData.levels.
    /// </summary>
    public int maxLevel;

    /// <summary>
    /// CircleCollider2D used to define the tower's range for detecting enemies.
    /// </summary>
    protected CircleCollider2D rangeCollider;

    /// <summary>
    /// The grid position where the tower is placed. Used to track occupancy on the grid.
    /// </summary>
    public Vector2 towerGridPosition;

    /// <summary>
    /// A visual indicator (usually a UI element or GameObject) that displays the tower's range.
    /// </summary>
    public GameObject rangeIndicator;

    /// <summary>
    /// Indicates whether the tower has been fully placed (as opposed to being in a pending placement state).
    /// </summary>
    public bool isFullyPlaced = false;

    /// <summary>
    /// Called when the script instance is being loaded.
    /// Finds and caches the RangeIndicator child object if not already assigned.
    /// </summary>
    protected virtual void Awake()
    {
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

    /// <summary>
    /// Called before the first frame update.
    /// Sets the maximum level based on TowerData, initializes the range collider for AoE or similar features,
    /// and applies initial level-specific stats.
    /// </summary>
    protected virtual void Start()
    {
        if (towerData == null)
        {
            Debug.LogError("TowerData is not assigned for " + gameObject.name);
            return;
        }

        maxLevel = towerData.levels.Length;

        // Initialize the circle collider for tower range (if present).
        rangeCollider = GetComponent<CircleCollider2D>();
        if (rangeCollider != null)
        {
            rangeCollider.isTrigger = true;
        }

        // Apply initial stats according to the current level.
        ApplyLevelStats();
    }

    /// <summary>
    /// Called once per frame.
    /// Checks for user input to detect tower selection, unless the game is over.
    /// </summary>
    protected virtual void Update()
    {
        DetectTowerSelection();
    }

    /// <summary>
    /// Determines if the tower can be upgraded based on its current level versus maximum level.
    /// </summary>
    /// <returns>True if the tower can be upgraded; otherwise, false.</returns>
    public bool CanUpgrade()
    {
        return level < maxLevel;
    }

    /// <summary>
    /// Retrieves the cost for upgrading to the next level.
    /// </summary>
    /// <returns>The upgrade cost from TowerData if upgrade is possible; otherwise, 0.</returns>
    public int GetUpgradeCost()
    {
        if (CanUpgrade())
        {
            return towerData.levels[level].upgradeCost;
        }
        return 0;
    }

    /// <summary>
    /// Upgrades the tower if possible, increments its level, and applies new level stats.
    /// </summary>
    public virtual void Upgrade()
    {
        if (CanUpgrade())
        {
            level++;
            ApplyLevelStats();
        }
    }

    /// <summary>
    /// Base method for applying level-specific stats to the tower.
    /// This method updates the tower's range (via collider and range indicator) and applies the level-specific sprite.
    /// </summary>
    public virtual void ApplyLevelStats()
    {
        if (towerData == null || level > towerData.levels.Length) return;

        TowerLevelData levelData = towerData.levels[level - 1];

        // Update the range collider's radius.
        if (rangeCollider != null)
        {
            rangeCollider.radius = levelData.range;
        }

        // Update the visual range indicator's scale.
        if (rangeIndicator != null)
        {
            float diameter = levelData.range * 2f;
            rangeIndicator.transform.localScale = new Vector3(diameter, diameter, 1f);
        }

        // Apply level-specific image settings.
        ApplyImageStatus(levelData);
    }

    /// <summary>
    /// Applies the level-specific sprite to the tower's SpriteRenderer.
    /// Derived classes may override this method to include additional visual effects.
    /// </summary>
    /// <param name="levelData">The TowerLevelData for the current level.</param>
    protected virtual void ApplyImageStatus(TowerLevelData levelData)
    {
        SpriteRenderer sr = GetComponent<SpriteRenderer>();
        if (sr != null && levelData.towerLevelSprite != null)
        {
            sr.sprite = levelData.towerLevelSprite;
        }
    }

    /// <summary>
    /// Detects if the player has clicked on this tower by comparing the mouse position, snapped to grid,
    /// with the tower's grid position. If clicked, it shows the tower panel and plays a selection sound.
    /// </summary>
    void DetectTowerSelection()
    {
        if (GameManager.Instance.isGameOver) return;

        if (Input.GetMouseButtonDown(0))
        {
            Vector2 mousePosition = Camera.main.ScreenToWorldPoint(Input.mousePosition);
            Vector2 gridPosition = GridManager.Instance.SnapToGrid(mousePosition);

            if (gridPosition == towerGridPosition)
            {
                UIManager.Instance.ShowTowerPanel(this);
                AudioManager.Instance.PlayTowerSelection();
            }
        }
    }

    /// <summary>
    /// Calculates the sell value of the tower by summing the upgrade costs paid for all levels and returning half of the total.
    /// </summary>
    /// <returns>The sell value as an integer.</returns>
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

    /// <summary>
    /// Enables the range indicator, making it visible.
    /// </summary>
    public void ShowRangeIndicator()
    {
        if (rangeIndicator != null)
        {
            rangeIndicator.SetActive(true);
        }
    }

    /// <summary>
    /// Disables the range indicator, making it invisible.
    /// </summary>
    public void HideRangeIndicator()
    {
        if (rangeIndicator != null)
        {
            rangeIndicator.SetActive(false);
        }
    }
}
