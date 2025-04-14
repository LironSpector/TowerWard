using UnityEngine;

/// <summary>
/// Description:
/// Represents an Area Damage Tower that periodically deals damage to all balloons within its range.
/// In addition to inheriting general tower behavior from the base class, this tower periodically checks for balloons 
/// in range and applies area damage with a special effect. When triggering its effect, it also displays an electricity 
/// visual effect over the area.
/// </summary>
public class AreaDamageTower : Tower
{
    // Timer for tracking when to deal area damage.
    private float timer = 0f;

    // The interval between successive area damage bursts, set from TowerData.specialInterval.
    private float currentInterval = 1f;

    // The damage dealt to each balloon in range, set from TowerData.specialValue.
    private int currentDamage = 1;

    /// <summary>
    /// The prefab for the electricity visual effect displayed when area damage is applied.
    /// This should be assigned in the Inspector.
    /// </summary>
    public GameObject electricityFxPrefab;

    /// <summary>
    /// Initializes the Area Damage Tower by calling the base Start method and applying level-specific stats.
    /// </summary>
    protected override void Start()
    {
        base.Start();
        ApplyLevelStats();
    }

    /// <summary>
    /// Called once per frame. If the tower is fully placed, accumulates time and, once the time exceeds the current interval,
    /// checks for any balloons within range to apply area damage. Also rotates the tower as defined in the base class.
    /// </summary>
    protected override void Update()
    {
        if (!isFullyPlaced)
            return;

        base.Update();

        timer += Time.deltaTime;
        if (timer >= currentInterval)
        {
            timer = 0f;

            bool hasBalloonsInRange = false;
            // Check for balloons within the tower's effective range.
            Collider2D[] hits = Physics2D.OverlapCircleAll(transform.position, rangeCollider.radius);
            foreach (var hit in hits)
            {
                if (hit.CompareTag("Balloon"))
                {
                    hasBalloonsInRange = true;
                    break;
                }
            }

            if (hasBalloonsInRange)
            {
                DoAreaDamage();
            }
        }
    }

    /// <summary>
    /// Applies level-specific statistics to the tower, including the special interval and special value (damage)
    /// used for area damage calculations. This overrides the base ApplyLevelStats to update local variables.
    /// </summary>
    public override void ApplyLevelStats()
    {
        base.ApplyLevelStats();
        if (towerData != null && level <= towerData.levels.Length)
        {
            TowerLevelData lvl = towerData.levels[level - 1];
            currentInterval = lvl.specialInterval;
            currentDamage = lvl.specialValue;
        }
    }

    /// <summary>
    /// Deals area damage by checking all colliders within the tower's range. For each collider tagged "Balloon",
    /// the tower applies its current damage. After damaging balloons, it displays an electricity effect.
    /// </summary>
    private void DoAreaDamage()
    {
        if (rangeCollider == null) return;

        Collider2D[] hits = Physics2D.OverlapCircleAll(transform.position, rangeCollider.radius);
        foreach (var hit in hits)
        {
            if (hit.CompareTag("Balloon"))
            {
                Balloon b = hit.GetComponent<Balloon>();
                if (b != null)
                {
                    Debug.Log("Taking damage: " + currentDamage);
                    b.TakeDamage(currentDamage);
                }
            }
        }

        ShowElectricityFx();
    }

    /// <summary>
    /// Instantiates and displays the electricity visual effect at the tower's position.
    /// If a range collider is present, the effect is scaled to cover the entire range area.
    /// </summary>
    private void ShowElectricityFx()
    {
        if (electricityFxPrefab != null)
        {
            GameObject fx = Instantiate(electricityFxPrefab, transform.position, Quaternion.identity);

            if (rangeCollider != null)
            {
                float diameter = rangeCollider.radius * 2f;
                fx.transform.localScale = new Vector3(diameter, diameter, 1f);
            }
        }
    }
}
