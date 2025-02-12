using UnityEngine;

public class SurpriseBox : MonoBehaviour
{
    // For internal tracking if it was claimed
    public bool isClaimed = false;

    // The grid position we occupy (we'll store it so we can free it later)
    private Vector2 cellPosition;

    void Start()
    {
        // Snap ourselves to the grid in case we placed by code
        cellPosition = GridManager.Instance.SnapToGrid(transform.position);

        // Occupy that cell in GameManager
        GameManager.Instance.cellManager.OccupyCellWithBox(cellPosition, this);
        //GameManager.Instance.OccupyCellWithBox(cellPosition, this);
    }

    // If user clicks the box
    void OnMouseDown()
    {
        if (!isClaimed && !GameManager.Instance.isGameOver)
        {
            isClaimed = true;
            // Apply the random effect
            DoRandomPrize();

            // Free the cell
            GameManager.Instance.cellManager.FreeCellFromBox(cellPosition);
            //GameManager.Instance.FreeCellFromBox(cellPosition);

            // Let the manager know
            SurpriseBoxManager.Instance.OnBoxClaimed(this);

            // Destroy box
            Destroy(gameObject);
        }
    }

    public void ForceExpire()
    {
        // Called if the box times out after 5s
        if (!isClaimed)
        {
            // Free the cell
            GameManager.Instance.cellManager.FreeCellFromBox(cellPosition);
            //GameManager.Instance.FreeCellFromBox(cellPosition);
        }
        Destroy(gameObject);
    }

    void DoRandomPrize()
    {
        // Probability: 80% money, 10% reset, 10% destroy all
        float roll = Random.value; // 0..1

        if (roll < 0.80f)
        {
            RewardMoney();
        }
        else if (roll < 0.90f)
        {
            ResetAllAbilities();
        }
        else
        {
            DestroyAllBalloons();
        }
    }

    void RewardMoney()
    {
        // formula => waveNumber * 0.5 * random[50..100]
        int waveIndex = BalloonSpawner.Instance.GetCurrentWaveIndex();
        // or however you track waves
        int randomVal = Random.Range(50, 101); // 50..100
        float multiplier = waveIndex * 0.5f;
        if (multiplier < 1f) multiplier = 1f; // if wave=0, fallback
        int finalMoney = Mathf.RoundToInt(randomVal * multiplier);

        GameManager.Instance.AddCurrency(finalMoney);
        Debug.Log($"SurpriseBox => Gave {finalMoney} currency!");
    }

    void ResetAllAbilities()
    {
        Debug.Log("SurpriseBox => Reset all special ability cooldowns!");
        SpecialAbilitiesManager.Instance.ResetAllCooldowns();
    }

    void DestroyAllBalloons()
    {
        Debug.Log("SurpriseBox => Destroying all balloons on the map!");
        Balloon[] allBalloons = FindObjectsOfType<Balloon>();
        foreach (var b in allBalloons)
        {
            b.Pop();
        }
    }
}
