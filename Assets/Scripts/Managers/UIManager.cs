using UnityEngine;
using UnityEngine.UI;
using TMPro;
using System.IO;
using System;
using System.Collections;
using System.Collections.Generic;
using Newtonsoft.Json;
using Newtonsoft.Json.Linq;
using System.Linq;

/// <summary>
/// Description:
/// Manages UI elements for various game functions, including tower panel display and updates, player panel updates,
/// handling opponent snapshot display in multiplayer mode, and processing UI button clicks such as exit and settings.
/// It also decompresses and updates the opponent's snapshot image using compressed data.
/// </summary>
public class UIManager : MonoBehaviour
{
    /// <summary>
    /// Singleton instance of the UIManager.
    /// </summary>
    public static UIManager Instance;

    [Header("Tower Panel References")]
    /// <summary>
    /// Panel containing tower details and controls.
    /// </summary>
    public GameObject towerPanel;
    /// <summary>
    /// Text element showing the tower's name.
    /// </summary>
    public TextMeshProUGUI towerNameText;
    /// <summary>
    /// Text element displaying the tower's current level.
    /// </summary>
    public TextMeshProUGUI towerLevelText;
    /// <summary>
    /// Text element for the upgrade cost.
    /// </summary>
    public TextMeshProUGUI upgradeCostText;
    /// <summary>
    /// Text element for showing upgrade differences/statistics.
    /// </summary>
    public TextMeshProUGUI upgradeStatsText;
    /// <summary>
    /// Button to upgrade the selected tower.
    /// </summary>
    public Button upgradeButton;
    /// <summary>
    /// Button to sell the selected tower.
    /// </summary>
    public Button sellButton;
    /// <summary>
    /// Image displaying the tower icon.
    /// </summary>
    public Image towerIconImage;

    [Header("Player Panel References")]
    /// <summary>
    /// Panel containing general player controls.
    /// </summary>
    public GameObject playerPanel;
    /// <summary>
    /// Text element displaying the current wave number.
    /// </summary>
    public TextMeshProUGUI waveNumberText;
    /// <summary>
    /// Button to exit the game.
    /// </summary>
    public Button exitGameButton;
    /// <summary>
    /// Button to open the settings panel.
    /// </summary>
    public Button settingsButton;
    /// <summary>
    /// Reference to the settings panel.
    /// </summary>
    public SettingsPanel settingsPanel;

    // The currently selected tower.
    private Tower selectedTower;

    [Header("Opponent Snapshot")]
    /// <summary>
    /// RawImage used to display the opponent's snapshot.
    /// </summary>
    public RawImage opponentSnapshotImage; // Assign in Inspector
    /// <summary>
    /// Panel that contains the opponent's snapshot UI.
    /// </summary>
    public GameObject opponentSnapshotPanel; // Assign in Inspector

    /// <summary>
    /// Awake is called when the script instance is loaded.
    /// Implements the singleton pattern and initializes UI panel states.
    /// </summary>
    void Awake()
    {
        if (Instance == null)
            Instance = this;
        else
            Destroy(gameObject);

        // Show the Player Panel and hide the Tower Panel by default.
        playerPanel.SetActive(true);
        towerPanel.SetActive(false);

        // Hook up button events.
        if (exitGameButton != null)
            exitGameButton.onClick.AddListener(OnExitGameButtonClicked);
        if (settingsButton != null)
            settingsButton.onClick.AddListener(OnSettingsButtonClicked);
    }

    /// <summary>
    /// Shows the tower panel for the specified tower, updating UI elements with its data.
    /// Hides the player panel and displays the tower's range indicator.
    /// </summary>
    /// <param name="tower">The tower to display information for.</param>
    public void ShowTowerPanel(Tower tower)
    {
        // Hide Player Panel.
        if (playerPanel != null)
            playerPanel.SetActive(false);

        // Hide range indicator of any previously selected tower.
        if (selectedTower != null)
            selectedTower.HideRangeIndicator();

        selectedTower = tower;
        selectedTower.ShowRangeIndicator();
        UpdateTowerPanel();

        // Show Tower Panel.
        towerPanel.SetActive(true);
    }

    /// <summary>
    /// Hides the tower panel and clears the currently selected tower.
    /// Restores the Player Panel.
    /// </summary>
    public void HideTowerPanel()
    {
        if (selectedTower != null)
        {
            selectedTower.HideRangeIndicator();
            selectedTower = null;
        }

        towerPanel.SetActive(false);

        // Show the Player Panel.
        if (playerPanel != null)
            playerPanel.SetActive(true);
    }

    /// <summary>
    /// Called when the Upgrade button is clicked.
    /// Upgrades the selected tower if affordable and available.
    /// </summary>
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
                // Optionally, display a message to the player.
            }
        }
    }

    /// <summary>
    /// Called when the Sell button is clicked.
    /// Sells the selected tower, refunds currency, frees its grid cell, and destroys the tower.
    /// </summary>
    public void OnSellButtonClicked()
    {
        if (GameManager.Instance.isGameOver)
            return;

        if (selectedTower != null)
        {
            // Calculate refund amount.
            int refundAmount = selectedTower.GetSellValue();
            GameManager.Instance.AddCurrency(refundAmount);

            // Free the grid cell occupied by the tower.
            GameManager.Instance.cellManager.FreeCell(selectedTower.towerGridPosition);

            // Destroy the tower GameObject.
            Destroy(selectedTower.gameObject);

            // Hide the Tower Panel.
            HideTowerPanel();
        }
    }

    /// <summary>
    /// Updates the tower panel UI with data from the selected tower.
    /// Displays tower name, level, upgrade cost, upgrade differences, and sell value.
    /// </summary>
    void UpdateTowerPanel()
    {
        if (selectedTower != null)
        {
            towerNameText.text = selectedTower.towerData.towerName;
            towerLevelText.text = "Level: " + selectedTower.level;

            // Display the main tower sprite.
            if (towerIconImage != null)
            {
                Debug.Log("Not null!");
                towerIconImage.sprite = selectedTower.towerData.towerSprite;
            }
            else
            {
                Debug.Log("Is null!");
            }

            // Update upgrade button state and text.
            if (selectedTower.CanUpgrade())
            {
                upgradeCostText.text = "Upgrade Cost: " + selectedTower.GetUpgradeCost();
                upgradeButton.interactable = true;
                upgradeButton.GetComponentInChildren<TextMeshProUGUI>().text = "Upgrade";
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
                    upgradeStatsText.text = "No upgrades available. Tower is at max level!";
                }
            }

            // Update the Sell button to show refund amount.
            int sellValue = selectedTower.GetSellValue();
            sellButton.GetComponentInChildren<TextMeshProUGUI>().text = "Sell ($" + sellValue + ")";
        }
        else
        {
            // No tower selected
            if (upgradeStatsText != null)
                upgradeStatsText.text = "";
            if (towerIconImage != null)
                towerIconImage.sprite = null;
        }
    }

    /// <summary>
    /// Displays upgrade differences between the current and next level for the selected tower.
    /// Uses a StringBuilder to create a multi-line string showing percentage changes for various stats.
    /// </summary>
    /// <param name="tower">The tower to display upgrade differences for.</param>
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

        System.Text.StringBuilder sb = new System.Text.StringBuilder();

        bool isMoneyTower = (tower is MoneyTower);
        bool isAreaDamageTower = (tower is AreaDamageTower);
        bool isDestructiveEnergy = (tower is DestructiveEnergyTower);
        bool isVillageTower = (tower is VillageTower);

        // Helper for float fields
        void AddUpgradeLineFloat(string key, float oldVal, float newVal)
        {
            if (Mathf.Approximately(oldVal, newVal)) return;
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

        // Add standard upgrade differences.
        AddUpgradeLineFloat("Range", curr.range, nxt.range);
        AddUpgradeLineFloat("Fire Rate", curr.fireRate, nxt.fireRate);
        AddUpgradeLineInt("Damage", curr.damage, nxt.damage);

        // Determine labels based on tower type.
        string intervalLabel = "specialInterval";
        string specialValueLabel = "specialValue";

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
            specialValueLabel = "Buff Percent";
            intervalLabel = "Interval (Unused)";
        }

        AddUpgradeLineFloat(intervalLabel, curr.specialInterval, nxt.specialInterval);
        AddUpgradeLineInt(specialValueLabel, curr.specialValue, nxt.specialValue);

        // Add upgrade differences for status effects.
        AddUpgradeLineFloat("Freeze Duration", curr.freezeDuration, nxt.freezeDuration);
        AddUpgradeLineFloat("Slow Duration", curr.slowDuration, nxt.slowDuration);
        AddUpgradeLineFloat("Slow Factor", curr.slowFactor, nxt.slowFactor);
        AddUpgradeLineFloat("Poison Duration", curr.poisonDuration, nxt.poisonDuration);
        AddUpgradeLineFloat("Poison TickInterval", curr.poisonTickInterval, nxt.poisonTickInterval);

        upgradeStatsText.text = sb.ToString();
    }

    /// <summary>
    /// Called once per frame.
    /// Hides the tower panel if the user clicks outside of it and updates the wave number displayed on the player panel.
    /// </summary>
    void Update()
    {
        // Hide the tower panel if the player clicks outside its bounds.
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

        // Update the wave number in the player panel continuously.
        if (playerPanel.activeSelf && waveNumberText != null)
        {
            int waveIndex = BalloonSpawner.Instance.GetCurrentWaveIndex();
            waveNumberText.text = waveIndex.ToString();
        }
    }

    /// <summary>
    /// Called when the Exit button is clicked in the player panel.
    /// Sends an update for last login, disconnects from the server, and quits the application.
    /// </summary>
    private void OnExitGameButtonClicked()
    {
        if (NetworkManager.Instance != null && NetworkManager.Instance.isConnected)
        {
            int userId = PlayerPrefs.GetInt("UserId", -1);
            if (userId != -1)
            {
                NetworkManager.Instance.messageSender.SendUpdateLastLogin(userId);
            }
            NetworkManager.Instance.DisconnectAndQuit();
        }
        Application.Quit();
    }

    /// <summary>
    /// Called when the Settings button is clicked in the player panel.
    /// Displays the settings panel.
    /// </summary>
    private void OnSettingsButtonClicked()
    {
        if (settingsPanel != null)
        {
            settingsPanel.ShowSettings();
        }
    }

    /// <summary>
    /// Sets the opponent snapshot panel's active state.
    /// Typically used to control snapshot display in multiplayer mode.
    /// </summary>
    /// <param name="isActive">True to show the opponent snapshot panel; false to hide it.</param>
    public void SetOpponentSnapshotPanel(bool isActive)
    {
        opponentSnapshotPanel.SetActive(isActive);
    }

    /// <summary>
    /// Decompresses data compressed with GZip.
    /// </summary>
    /// <param name="data">The compressed byte array.</param>
    /// <returns>The decompressed byte array.</returns>
    private byte[] DecompressData(byte[] data)
    {
        using (var input = new System.IO.MemoryStream(data))
        using (var gzip = new System.IO.Compression.GZipStream(input, System.IO.Compression.CompressionMode.Decompress))
        using (var output = new System.IO.MemoryStream())
        {
            gzip.CopyTo(output);
            return output.ToArray();
        }
    }

    /// <summary>
    /// Updates the opponent's snapshot image from a Base64 encoded and compressed image string.
    /// The method decompresses the image data, loads it into a Texture2D, and sets it on the RawImage.
    /// </summary>
    /// <param name="imageData">Base64 encoded string containing compressed image data.</param>
    public void UpdateOpponentSnapshot(string imageData)
    {
        if (string.IsNullOrEmpty(imageData))
            return;

        byte[] compressedBytes = Convert.FromBase64String(imageData);
        byte[] imageBytes = DecompressData(compressedBytes);

        Texture2D texture = new Texture2D(2, 2); //(2, 2) is an initial value of 2*2 pixels, but it is override by the
                                                 //dimentions of "imageBytes" in "texture.LoadImage(imageBytes)"
        texture.LoadImage(imageBytes);

        opponentSnapshotImage.texture = texture;
    }

    /// <summary>
    /// Sets the alpha value of the opponent snapshot image.
    /// </summary>
    /// <param name="alpha">Alpha value (0 to 1) to set on the snapshot image.</param>
    public void SetOpponentSnapshotAlpha(float alpha)
    {
        if (opponentSnapshotImage != null)
        {
            Color c = opponentSnapshotImage.color;
            c.a = alpha;
            opponentSnapshotImage.color = c;
        }
    }

    /// <summary>
    /// Called when this UIManager instance is destroyed.
    /// Clears references to the opponent snapshot image and panel to free memory.
    /// </summary>
    void OnDestroy()
    {
        opponentSnapshotImage = null;
        opponentSnapshotPanel = null;
    }
}
