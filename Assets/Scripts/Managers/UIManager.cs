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
        Debug.Log("------ Image bytes - before updating (end): ---------> " + BitConverter.ToString(imageBytes));

        Texture2D texture = new Texture2D(2, 2); //(2, 2) is an initial value of 2*2 pixels, but it is override by the dimentions of "imageBytes" in "texture.LoadImage(imageBytes)"
        texture.LoadImage(imageBytes);
        Debug.Log("Texture's width: " +  texture.width);
        Debug.Log("Texture's height: " +  texture.height);

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
