using UnityEngine;
using UnityEngine.UI;
using TMPro;
using System.Linq;

/// <summary>
/// Description:
/// Manages the UI for tower selection, including the initialization of tower buttons with their
/// corresponding prices, handling button clicks to start tower placement, and refreshing button states
/// based on the player's current currency. This class interacts with the TowerPlacement and AudioManager
/// to start placement and play associated sounds.
/// </summary>
public class TowerSelectionUI : MonoBehaviour
{
    [Header("Tower Buttons")]
    /// <summary>
    /// Button for selecting the Basic Tower.
    /// </summary>
    public Button basicTowerButton;
    /// <summary>
    /// Button for selecting the Sniper Tower.
    /// </summary>
    public Button sniperTowerButton;
    /// <summary>
    /// Button for selecting the Freezing Tower.
    /// </summary>
    public Button freezingTowerButton;
    /// <summary>
    /// Button for selecting the Slowing Tower.
    /// </summary>
    public Button slowingTowerButton;
    /// <summary>
    /// Button for selecting the Poison Tower.
    /// </summary>
    public Button poisonTowerButton;
    /// <summary>
    /// Button for selecting the Super Tower.
    /// </summary>
    public Button superTowerButton;
    /// <summary>
    /// Button for selecting the Money Tower.
    /// </summary>
    public Button moneyTowerButton;
    /// <summary>
    /// Button for selecting the Destructive Energy Tower.
    /// </summary>
    public Button destructiveEnergyTowerButton;
    /// <summary>
    /// Button for selecting the Area Damage Tower.
    /// </summary>
    public Button areaDamageTowerButton;
    /// <summary>
    /// Button for selecting the Village Tower.
    /// </summary>
    public Button villageTowerButton;

    /// <summary>
    /// Index for the Basic Tower in the tower prefab list.
    /// </summary>
    public int basicTowerIndex = 0;
    /// <summary>
    /// Index for the Sniper Tower in the tower prefab list.
    /// </summary>
    public int sniperTowerIndex = 1;
    /// <summary>
    /// Index for the Freezing Tower in the tower prefab list.
    /// </summary>
    public int freezingTowerIndex = 2;
    /// <summary>
    /// Index for the Slowing Tower in the tower prefab list.
    /// </summary>
    public int slowingTowerIndex = 3;
    /// <summary>
    /// Index for the Poison Tower in the tower prefab list.
    /// </summary>
    public int poisonTowerIndex = 4;
    /// <summary>
    /// Index for the Super Tower in the tower prefab list.
    /// </summary>
    public int superTowerIndex = 5;
    /// <summary>
    /// Index for the Money Tower in the tower prefab list.
    /// </summary>
    public int moneyTowerIndex = 6;
    /// <summary>
    /// Index for the Destructive Energy Tower in the tower prefab list.
    /// </summary>
    public int destructiveEnergyTowerIndex = 7;
    /// <summary>
    /// Index for the Area Damage Tower in the tower prefab list.
    /// </summary>
    public int areaDamageTowerIndex = 8;
    /// <summary>
    /// Index for the Village Tower in the tower prefab list.
    /// </summary>
    public int villageTowerIndex = 9;

    // Text components for displaying the price of each tower button.
    public TextMeshProUGUI basicTowerPriceText;
    public TextMeshProUGUI sniperTowerPriceText;
    public TextMeshProUGUI freezingTowerPriceText;
    public TextMeshProUGUI slowingTowerPriceText;
    public TextMeshProUGUI poisonTowerPriceText;
    public TextMeshProUGUI superTowerPriceText;
    public TextMeshProUGUI moneyTowerPriceText;
    public TextMeshProUGUI destructiveEnergyTowerPriceText;
    public TextMeshProUGUI areaDamageTowerPriceText;
    public TextMeshProUGUI villageTowerPriceText;

    /// <summary>
    /// Called on the first frame. Sets up event listeners for tower buttons, initializes the price text for each tower,
    /// and refreshes the button states based on the player's current currency.
    /// </summary>
    private void Start()
    {
        // Set up the OnClick listeners for each tower button using their corresponding indices.
        basicTowerButton.onClick.AddListener(() => OnTowerButtonClicked(basicTowerIndex));
        sniperTowerButton.onClick.AddListener(() => OnTowerButtonClicked(sniperTowerIndex));
        freezingTowerButton.onClick.AddListener(() => OnTowerButtonClicked(freezingTowerIndex));
        slowingTowerButton.onClick.AddListener(() => OnTowerButtonClicked(slowingTowerIndex));
        poisonTowerButton.onClick.AddListener(() => OnTowerButtonClicked(poisonTowerIndex));
        superTowerButton.onClick.AddListener(() => OnTowerButtonClicked(superTowerIndex));
        moneyTowerButton.onClick.AddListener(() => OnTowerButtonClicked(moneyTowerIndex));
        destructiveEnergyTowerButton.onClick.AddListener(() => OnTowerButtonClicked(destructiveEnergyTowerIndex));
        areaDamageTowerButton.onClick.AddListener(() => OnTowerButtonClicked(areaDamageTowerIndex));
        villageTowerButton.onClick.AddListener(() => OnTowerButtonClicked(villageTowerIndex));

        // Initialize each tower button's price text.
        InitTowerButton(basicTowerIndex, basicTowerPriceText);
        InitTowerButton(sniperTowerIndex, sniperTowerPriceText);
        InitTowerButton(freezingTowerIndex, freezingTowerPriceText);
        InitTowerButton(slowingTowerIndex, slowingTowerPriceText);
        InitTowerButton(poisonTowerIndex, poisonTowerPriceText);
        InitTowerButton(superTowerIndex, superTowerPriceText);
        InitTowerButton(moneyTowerIndex, moneyTowerPriceText);
        InitTowerButton(destructiveEnergyTowerIndex, destructiveEnergyTowerPriceText);
        InitTowerButton(areaDamageTowerIndex, areaDamageTowerPriceText);
        InitTowerButton(villageTowerIndex, villageTowerPriceText);

        // Perform an initial refresh of the tower buttons to adjust colors and interactivity based on the player's currency.
        RefreshTowerButtons();
    }

    /// <summary>
    /// Handles the tower button click event.
    /// If a tower is not already being placed, initiates the placement process for the specified tower index and plays a selection sound.
    /// </summary>
    /// <param name="towerIndex">The index of the tower prefab selected for placement.</param>
    private void OnTowerButtonClicked(int towerIndex)
    {
        if (!TowerPlacement.Instance.IsPlacing)
        {
            TowerPlacement.Instance.StartPlacement(towerIndex);
            AudioManager.Instance.PlayTowerPanelChoose();
        }
    }

    /// <summary>
    /// Initializes a tower button's price text based on the tower prefab's initial upgrade cost.
    /// </summary>
    /// <param name="towerIndex">The index of the tower prefab in the TowerPlacement's list.</param>
    /// <param name="priceText">The TextMeshProUGUI component to set the price on.</param>
    private void InitTowerButton(int towerIndex, TextMeshProUGUI priceText)
    {
        if (towerIndex < 0 || towerIndex >= TowerPlacement.Instance.towerPrefabs.Count)
        {
            Debug.LogError("Invalid tower index in InitTowerButton");
            return;
        }

        GameObject towerPrefab = TowerPlacement.Instance.towerPrefabs[towerIndex];
        Tower towerScript = towerPrefab.GetComponent<Tower>();
        if (towerScript != null && towerScript.towerData != null && towerScript.towerData.levels.Length > 0)
        {
            int cost = towerScript.towerData.levels[0].upgradeCost;
            priceText.text = "$" + cost;
        }
        else
        {
            priceText.text = "$???";
        }
    }

    /// <summary>
    /// Refreshes the visual state of each tower button based on whether the player can afford the corresponding tower.
    /// Updates button interactivity and text colors accordingly.
    /// </summary>
    public void RefreshTowerButtons()
    {
        UpdateTowerButton(basicTowerIndex, basicTowerButton, basicTowerPriceText);
        UpdateTowerButton(sniperTowerIndex, sniperTowerButton, sniperTowerPriceText);
        UpdateTowerButton(freezingTowerIndex, freezingTowerButton, freezingTowerPriceText);
        UpdateTowerButton(slowingTowerIndex, slowingTowerButton, slowingTowerPriceText);
        UpdateTowerButton(poisonTowerIndex, poisonTowerButton, poisonTowerPriceText);
        UpdateTowerButton(superTowerIndex, superTowerButton, superTowerPriceText);
        UpdateTowerButton(moneyTowerIndex, moneyTowerButton, moneyTowerPriceText);
        UpdateTowerButton(destructiveEnergyTowerIndex, destructiveEnergyTowerButton, destructiveEnergyTowerPriceText);
        UpdateTowerButton(areaDamageTowerIndex, areaDamageTowerButton, areaDamageTowerPriceText);
        UpdateTowerButton(villageTowerIndex, villageTowerButton, villageTowerPriceText);
    }

    /// <summary>
    /// Updates a specific tower button's state based on whether the player can afford the tower's cost.
    /// Sets the button's interactability and adjusts the button and price text colors.
    /// </summary>
    /// <param name="towerIndex">The index of the tower prefab in the TowerPlacement's list.</param>
    /// <param name="btn">The Button component of the tower button.</param>
    /// <param name="priceText">The TextMeshProUGUI component that displays the tower's price.</param>
    private void UpdateTowerButton(int towerIndex, Button btn, TextMeshProUGUI priceText)
    {
        if (towerIndex < 0 || towerIndex >= TowerPlacement.Instance.towerPrefabs.Count)
            return;

        Tower towerScript = TowerPlacement.Instance.towerPrefabs[towerIndex].GetComponent<Tower>();
        int cost = towerScript.towerData.levels[0].upgradeCost;
        bool canAfford = (GameManager.Instance.currency >= cost);

        // Obtain the Image component attached to the button.
        Image btnImage = btn.GetComponent<Image>();

        // If the player can afford the tower, set button to normal appearance; otherwise, gray it out.
        if (canAfford)
        {
            // Normal color
            btn.interactable = true;
            btnImage.color = Color.white;
            priceText.color = Color.white;
        }
        else
        {
            btn.interactable = false;
            btnImage.color = new Color(0.7f, 0.7f, 0.7f); // Light gray.
            priceText.color = new Color(0.2f, 0.2f, 0.2f);  // Darker gray.
        }
    }
}
