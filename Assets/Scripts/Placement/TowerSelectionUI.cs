// ------------------ old TowerSelectionUI - before UI changes of the TowerSelection panel: ------------------
//using UnityEngine;
//using UnityEngine.UI;

//public class TowerSelectionUI : MonoBehaviour
//{
//    public Button basicTowerButton;
//    public Button sniperTowerButton;
//    //public Button greenTowerButton;
//    // etc.

//    // Provide the tower indices that match `towerPrefabs` order in TowerPlacement
//    public int basicTowerIndex = 0;
//    public int sniperTowerIndex = 1;
//    //public int greenTowerIndex = 2;
//    // etc.

//    void Start()
//    {
//        // Add listeners
//        basicTowerButton.onClick.AddListener(() => OnTowerButtonClicked(basicTowerIndex));
//        sniperTowerButton.onClick.AddListener(() => OnTowerButtonClicked(sniperTowerIndex));
//        //greenTowerButton.onClick.AddListener(() => OnTowerButtonClicked(greenTowerIndex));
//        // etc.
//    }

//    void OnTowerButtonClicked(int towerIndex)
//    {
//        // Call TowerPlacement to start placing that tower
//        if (!TowerPlacement.Instance.IsPlacing) //Allow start placement only if currently there is no placement in process
//        {
//            TowerPlacement.Instance.StartPlacement(towerIndex);
//        }
//    }
//}








using UnityEngine;
using UnityEngine.UI;
using TMPro;

public class TowerSelectionUI : MonoBehaviour
{
    [Header("Tower Buttons")]
    public Button basicTowerButton;
    public Button sniperTowerButton;
    // etc...

    // These indices must match TowerPlacement’s towerPrefabs order
    public int basicTowerIndex = 0;
    public int sniperTowerIndex = 1;
    // etc...

    // We’ll store references to the text for each button
    public TextMeshProUGUI basicTowerPriceText;
    public TextMeshProUGUI sniperTowerPriceText;
    // etc...

    // We’ll store references to the images for each button to gray them out
    //public Image basicTowerButtonImage;
    //public Image sniperTowerButtonImage;
    // etc...

    private void Start()
    {
        basicTowerButton.onClick.AddListener(() => OnTowerButtonClicked(basicTowerIndex));
        sniperTowerButton.onClick.AddListener(() => OnTowerButtonClicked(sniperTowerIndex));
        // etc.

        // Initialize the price text
        InitTowerButton(basicTowerIndex, basicTowerPriceText);
        InitTowerButton(sniperTowerIndex, sniperTowerPriceText);
        // etc.

        // Then do an initial refresh to set color based on current currency
        RefreshTowerButtons();
    }

    // Called when user clicks a tower button
    private void OnTowerButtonClicked(int towerIndex)
    {
        if (!TowerPlacement.Instance.IsPlacing)
        {
            TowerPlacement.Instance.StartPlacement(towerIndex);
        }
    }

    // Fetch tower cost from TowerPlacement => Tower => TowerData => levels[0].upgradeCost
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

    // Called whenever currency changes or after placement, etc.
    public void RefreshTowerButtons()
    {
        // For each tower button, check if the player can afford it
        //UpdateTowerButton(basicTowerIndex, basicTowerButton, basicTowerButtonImage, basicTowerPriceText);
        //UpdateTowerButton(sniperTowerIndex, sniperTowerButton, sniperTowerButtonImage, sniperTowerPriceText);
        UpdateTowerButton(basicTowerIndex, basicTowerButton, basicTowerPriceText);
        UpdateTowerButton(sniperTowerIndex, sniperTowerButton, sniperTowerPriceText);

        // etc. for other towers
    }

    //private void UpdateTowerButton(int towerIndex, Button btn, Image btnImage, TextMeshProUGUI priceText)

    private void UpdateTowerButton(int towerIndex, Button btn, TextMeshProUGUI priceText)
    {
        if (towerIndex < 0 || towerIndex >= TowerPlacement.Instance.towerPrefabs.Count)
            return;

        Tower towerScript = TowerPlacement.Instance.towerPrefabs[towerIndex].GetComponent<Tower>();
        int cost = towerScript.towerData.levels[0].upgradeCost;

        bool canAfford = (GameManager.Instance.currency >= cost);


        Image btnImage = btn.GetComponent<Image>();

        // If can afford => normal color, else => gray
        if (canAfford)
        {
            // Normal color
            btn.interactable = true;
            btnImage.color = Color.white; // or whatever normal color
            priceText.color = Color.white; // some dark text color
        }
        else
        {
            // Gray out
            btn.interactable = false; // Or keep it interactable but visually gray
            btnImage.color = new Color(0.7f, 0.7f, 0.7f); // light gray
            priceText.color = new Color(0.2f, 0.2f, 0.2f); // darker gray text
        }
    }
}
