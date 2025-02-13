using UnityEngine;
using TMPro;
using UnityEngine.UI;
using System.IO;
using System;

public class UIManager : MonoBehaviour
{
    public static UIManager Instance;

    public GameObject towerPanel;
    public TextMeshProUGUI towerNameText;
    public TextMeshProUGUI towerLevelText;
    public TextMeshProUGUI upgradeCostText;
    public TextMeshProUGUI upgradeStatsText;
    public Button upgradeButton;
    public Button sellButton;


    private Tower selectedTower;

    public RawImage opponentSnapshotImage; // Assign in Inspector
    public GameObject opponentSnapshotPanel; // Assign in Inspector


    void Awake()
    {
        if (Instance == null)
            Instance = this;
        else
            Destroy(gameObject);

        towerPanel.SetActive(false);
    }

    public void ShowTowerPanel(Tower tower)
    {
        if (selectedTower != null)
        {
            selectedTower.HideRangeIndicator(); // Hide previous tower's range indicator
        }

        selectedTower = tower;
        selectedTower.ShowRangeIndicator(); // Show selected tower's range indicator
        UpdateTowerPanel();
        towerPanel.SetActive(true);
    }

    public void HideTowerPanel()
    {
        if (selectedTower != null)
        {
            selectedTower.HideRangeIndicator(); // Hide selected tower's range indicator
            selectedTower = null;
        }

        towerPanel.SetActive(false);
    }



    public void OnUpgradeButtonClicked()
    {
        if (GameManager.Instance.isGameOver)
            return;

        if (selectedTower != null && selectedTower.CanUpgrade())
        {
            int cost = selectedTower.GetUpgradeCost();
            if (GameManager.Instance.CanAfford(cost))
            {
                GameManager.Instance.SpendCurrency(cost);
                selectedTower.Upgrade();
                UpdateTowerPanel();
            }
            else
            {
                Debug.Log("Not enough currency to upgrade.");
                // Optionally display a message to the player
            }
        }
    }

    public void OnSellButtonClicked()
    {
        if (GameManager.Instance.isGameOver)
            return;

        if (selectedTower != null)
        {
            // Calculate the refund amount
            int refundAmount = selectedTower.GetSellValue();

            // Add currency to the player
            GameManager.Instance.AddCurrency(refundAmount);

            // Free the occupied cell
            GameManager.Instance.cellManager.FreeCell(selectedTower.towerGridPosition);

            // Destroy the tower
            Destroy(selectedTower.gameObject);

            // Hide the tower panel
            HideTowerPanel();
        }
    }

    void UpdateTowerPanel()
    {
        if (selectedTower != null)
        {
            towerNameText.text = selectedTower.towerData.towerName;
            towerLevelText.text = "Level: " + selectedTower.level;

            if (selectedTower.CanUpgrade())
            {
                upgradeCostText.text = "Upgrade Cost: " + selectedTower.GetUpgradeCost();
                upgradeButton.interactable = true;
                upgradeButton.GetComponentInChildren<TextMeshProUGUI>().text = "Upgrade";

                // --- NEW CODE: show upgrade stats ---
                ShowUpgradeDifferences(selectedTower);
            }
            else
            {
                upgradeCostText.text = "Max Level Reached";
                upgradeButton.interactable = false;
                upgradeButton.GetComponentInChildren<TextMeshProUGUI>().text = "Max Level";

                // If max level, clear stats or show something like "No further upgrades"
                if (upgradeStatsText != null)
                {
                    upgradeStatsText.text = "No upgrades Available. Your tower has reaches the maximum improvements!";
                }
            }

            // Update the Sell Button text to show refund amount
            int sellValue = selectedTower.GetSellValue();
            sellButton.GetComponentInChildren<TextMeshProUGUI>().text = "Sell ($" + sellValue + ")";
        }
        else
        {
            // No tower selected
            if (upgradeStatsText != null)
            {
                upgradeStatsText.text = "";
            }
        }
    }


    private void ShowUpgradeDifferences(Tower tower)
    {
        if (upgradeStatsText == null) return;
        upgradeStatsText.text = "";

        if (!tower.CanUpgrade()) return;

        TowerData data = tower.towerData;
        int currentIndex = tower.level - 1;
        int nextIndex = tower.level;

        TowerLevelData curr = data.levels[currentIndex];
        TowerLevelData nxt = data.levels[nextIndex];

        // We'll store the lines in a StringBuilder
        System.Text.StringBuilder sb = new System.Text.StringBuilder();

        // Identify tower type for label mapping:
        bool isMoneyTower = (tower is MoneyTower);
        bool isAreaDamageTower = (tower is AreaDamageTower);
        bool isDestructiveEnergy = (tower is DestructiveEnergyTower);
        bool isVillageTower = (tower is VillageTower);

        // Helper for float fields
        void AddUpgradeLineFloat(string key, float oldVal, float newVal)
        {
            if (Mathf.Approximately(oldVal, newVal)) return; // no change
            float diff = newVal - oldVal;
            if (Mathf.Approximately(oldVal, 0f))
            {
                if (!Mathf.Approximately(newVal, 0f))
                    sb.AppendLine($"{key}: from 0 to {newVal}");
                return;
            }
            float pct = (diff / oldVal) * 100f;
            float rounded = Mathf.Round(pct);
            string sign = (pct >= 0) ? "+" : "";
            sb.AppendLine($"{key}: {sign}{rounded}%");
        }

        // Helper for int fields
        void AddUpgradeLineInt(string key, int oldVal, int newVal)
        {
            if (oldVal == newVal) return;
            int diff = newVal - oldVal;
            if (oldVal == 0)
            {
                if (newVal != 0)
                    sb.AppendLine($"{key}: from 0 to {newVal}");
                return;
            }
            float pct = (diff / (float)oldVal) * 100f;
            float rounded = Mathf.Round(pct);
            string sign = (pct >= 0) ? "+" : "";
            sb.AppendLine($"{key}: {sign}{rounded}%");
        }

        // Now we do each field, but rename specialInterval / specialValue if the tower is type-2

        // -- Range
        AddUpgradeLineFloat("Range", curr.range, nxt.range);

        // -- Fire Rate
        AddUpgradeLineFloat("Fire Rate", curr.fireRate, nxt.fireRate);

        // -- Damage
        AddUpgradeLineInt("Damage", curr.damage, nxt.damage);

        // Next we have "specialInterval" and "specialValue" but the label depends on tower type.
        string intervalLabel = "specialInterval"; // default
        string specialValueLabel = "specialValue";    // default

        if (isMoneyTower)
        {
            intervalLabel = "Money Interval";
            specialValueLabel = "Money Production";
        }
        else if (isAreaDamageTower)
        {
            intervalLabel = "Damage Interval";
            specialValueLabel = "Damage";
        }
        else if (isDestructiveEnergy)
        {
            intervalLabel = "Damage Interval";
            specialValueLabel = "Damage";
        }
        else if (isVillageTower)
        {
            // VillageTower only uses "specialValue" as "buffPercent" 
            // We can rename it:
            specialValueLabel = "Buff Percent";
            // If you want to hide specialInterval entirely if it’s not used,
            // you can just skip it. Or label it "Unused".
            intervalLabel = "Interval (Unused)"; // Or skip
        }

        // Then we add them as usual
        AddUpgradeLineFloat(intervalLabel, curr.specialInterval, nxt.specialInterval);
        AddUpgradeLineInt(specialValueLabel, curr.specialValue, nxt.specialValue);

        // For freeze/slow/poison fields, if you want them all the same logic
        AddUpgradeLineFloat("Freeze Duration", curr.freezeDuration, nxt.freezeDuration);
        AddUpgradeLineFloat("Slow Duration", curr.slowDuration, nxt.slowDuration);
        AddUpgradeLineFloat("Slow Factor", curr.slowFactor, nxt.slowFactor);
        AddUpgradeLineFloat("Poison Duration", curr.poisonDuration, nxt.poisonDuration);
        AddUpgradeLineFloat("Poison TickInterval", curr.poisonTickInterval, nxt.poisonTickInterval);

        // Finally set the text
        upgradeStatsText.text = sb.ToString();
    }


    void Update()
    {
        // Hide the panel if the player clicks outside of it
        if (towerPanel.activeSelf && Input.GetMouseButtonDown(0))
        {
            if (!RectTransformUtility.RectangleContainsScreenPoint(
                towerPanel.GetComponent<RectTransform>(),
                Input.mousePosition,
                null))
            {
                HideTowerPanel();
            }
        }

    }

    public void SetOpponentSnapshotPanel(bool isActive)  //Also used in intention to: "bool isMultiplayer"
    {
        // Only show opponent snapshot panel in Multiplayer mode
        opponentSnapshotPanel.SetActive(isActive);
    }


    private byte[] DecompressData(byte[] data)
    {
        using (var input = new MemoryStream(data))
        using (var gzip = new System.IO.Compression.GZipStream(input, System.IO.Compression.CompressionMode.Decompress))
        using (var output = new MemoryStream())
        {
            gzip.CopyTo(output);
            return output.ToArray();
        }
    }

    public void UpdateOpponentSnapshot(string imageData)
    {
        if (string.IsNullOrEmpty(imageData))
            return;

        byte[] compressedBytes = System.Convert.FromBase64String(imageData);
        byte[] imageBytes = DecompressData(compressedBytes);
        //byte[] imageBytes = System.Convert.FromBase64String(imageData);
        //Debug.Log("------ Image bytes - before updating (end): ---------> " + BitConverter.ToString(imageBytes));

        Texture2D texture = new Texture2D(2, 2); //(2, 2) is an initial value of 2*2 pixels, but it is override by the dimentions of "imageBytes" in "texture.LoadImage(imageBytes)"
        texture.LoadImage(imageBytes);
        //Debug.Log("Texture's width: " +  texture.width);
        //Debug.Log("Texture's height: " +  texture.height);

        opponentSnapshotImage.texture = texture;
    }

    public void SetOpponentSnapshotAlpha(float alpha)
    {
        if (opponentSnapshotImage != null)
        {
            Color c = opponentSnapshotImage.color;
            c.a = alpha;
            opponentSnapshotImage.color = c;
        }
    }

    void OnDestroy()
    {
        //To save memory - I am not sure if it's needed
        opponentSnapshotImage = null;
        opponentSnapshotPanel = null;
    }
}
