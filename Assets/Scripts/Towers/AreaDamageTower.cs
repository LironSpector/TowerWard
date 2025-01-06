using UnityEngine;

public class AreaDamageTower : Tower
{
    private float timer = 0f;
    private float currentInterval = 1f; // from towerData.levels[level-1].specialInterval
    private int currentDamage = 1;      // from towerData.levels[level-1].damage or specialValue

    // Assign in Inspector: an electricity prefab that is shown each time AoE triggers
    public GameObject electricityFxPrefab;

    protected override void Start()
    {
        base.Start();
        ApplyLevelStats();
    }

    protected override void Update()
    {
        if (!isFullyPlaced) return;

        base.Update();

        timer += Time.deltaTime;
        if (timer >= currentInterval)
        {
            timer = 0f;
            DoAreaDamage();
        }
    }

    public override void ApplyLevelStats()
    {
        base.ApplyLevelStats();
        if (towerData != null && level <= towerData.levels.Length)
        {
            TowerLevelData lvl = towerData.levels[level - 1];
            currentInterval = lvl.specialInterval; // how often to do AoE
            currentDamage = lvl.specialValue;    // or you could use lvl.damage if you prefer
        }
    }

    private void DoAreaDamage()
    {
        // 1) Overlap all balloons in range
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

        // 2) Show an electricity effect
        ShowElectricityFx();
    }

    private void ShowElectricityFx()
    {
        if (electricityFxPrefab != null)
        {
            // For example, spawn it at the tower's position. 
            // The prefab might scale to cover the entire range visually.
            GameObject fx = Instantiate(electricityFxPrefab, transform.position, Quaternion.identity);

            // If we want it to scale with the range:
            if (rangeCollider != null)
            {
                float diameter = rangeCollider.radius * 2f;
                fx.transform.localScale = new Vector3(diameter, diameter, 1f);
            }

            // The electricity prefab can have its own short-lifetime script or ParticleSystem
        }
    }
}
