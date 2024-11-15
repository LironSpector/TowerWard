using UnityEngine;
using TMPro;
using UnityEngine.UI;

public class UIManager : MonoBehaviour
{
    public static UIManager Instance;

    public GameObject towerPanel;
    public TextMeshProUGUI towerNameText;
    public TextMeshProUGUI towerLevelText;
    public TextMeshProUGUI upgradeCostText;
    public Button upgradeButton;
    public Button sellButton;

    private Tower selectedTower;

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
            GameManager.Instance.FreeCell(selectedTower.towerGridPosition);

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
            }
            else
            {
                upgradeCostText.text = "Max Level Reached";
                upgradeButton.interactable = false;
                upgradeButton.GetComponentInChildren<TextMeshProUGUI>().text = "Max Level";
            }

            // Update the Sell Button text to show refund amount
            int sellValue = selectedTower.GetSellValue();
            sellButton.GetComponentInChildren<TextMeshProUGUI>().text = "Sell ($" + sellValue + ")";
        }
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
}
