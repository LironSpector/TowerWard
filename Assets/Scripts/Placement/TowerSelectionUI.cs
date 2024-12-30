using UnityEngine;
using UnityEngine.UI;

public class TowerSelectionUI : MonoBehaviour
{
    public Button basicTowerButton;
    public Button sniperTowerButton;
    //public Button greenTowerButton;
    // etc.

    // Provide the tower indices that match `towerPrefabs` order in TowerPlacement
    public int basicTowerIndex = 0;
    public int sniperTowerIndex = 1;
    //public int greenTowerIndex = 2;
    // etc.

    void Start()
    {
        // Add listeners
        basicTowerButton.onClick.AddListener(() => OnTowerButtonClicked(basicTowerIndex));
        sniperTowerButton.onClick.AddListener(() => OnTowerButtonClicked(sniperTowerIndex));
        //greenTowerButton.onClick.AddListener(() => OnTowerButtonClicked(greenTowerIndex));
        // etc.
    }

    void OnTowerButtonClicked(int towerIndex)
    {
        // Call TowerPlacement to start placing that tower
        if (!TowerPlacement.Instance.IsPlacing) //Allow start placement only if currently there is no placement in process
        {
            TowerPlacement.Instance.StartPlacement(towerIndex);
        }
    }
}
