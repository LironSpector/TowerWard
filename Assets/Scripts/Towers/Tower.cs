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
        Debug.Log("At the very start!");
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
            Debug.Log("D");
            ApplyLevelStats();
            Debug.Log("E");
        }
    }

    /// <summary>
    /// Base method for applying tower level stats.
    /// Projectile towers override it to set shooting stats, 
    /// Non-projectile towers might override it if needed for special logic.
    /// </summary>
    public virtual void ApplyLevelStats()
    {
        Debug.Log("AAAAAAAAAAAAAAAAAAA");
        if (towerData == null || level > towerData.levels.Length) return;
        Debug.Log("BBBBBBBBBBBBBB");

        TowerLevelData levelData = towerData.levels[level - 1];

        // 1) Adjust range if you want
        if (rangeCollider != null)
        {
            rangeCollider.radius = levelData.range;
        }
        if (rangeIndicator != null)
        {
            float diameter = levelData.range * 2f;
            rangeIndicator.transform.localScale = new Vector3(diameter, diameter, 1f);
        }

        ApplyImageStatus(levelData);
    }

    protected virtual void ApplyImageStatus(TowerLevelData levelData)
    {
        // 2) Set the sprite (if this tower’s main GameObject has a SpriteRenderer)
        SpriteRenderer sr = GetComponent<SpriteRenderer>();
        Debug.Log("sr ==== null: " + (sr == null) + ", levelData.towerLevelSprite == null: " + (levelData.towerLevelSprite == null));
        if (sr != null && levelData.towerLevelSprite != null)
        {
            Debug.Log("Tempforme!");
            sr.sprite = levelData.towerLevelSprite;
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
