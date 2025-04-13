using UnityEngine;

/// <summary>
/// Description:
/// Represents a Surprise Box in the game that, when clicked by the user,
/// awards a random prize such as currency, resetting special ability cooldowns, or destroying all balloons.
/// The SurpriseBox automatically snaps to the grid on start and occupies the corresponding cell.
/// When claimed or expired, it frees the occupied cell and notifies the SurpriseBoxManager.
/// </summary>
public class SurpriseBox : MonoBehaviour
{
    /// <summary>
    /// Indicates if the surprise box has already been claimed.
    /// </summary>
    public bool isClaimed = false;

    // The grid position occupied by this box (stored for later release).
    private Vector2 cellPosition;

    /// <summary>
    /// Called when the script instance is first loaded.
    /// Snaps the SurpriseBox to the grid and marks the corresponding cell as occupied.
    /// </summary>
    void Start()
    {
        // Snap to the nearest cell center.
        cellPosition = GridManager.Instance.SnapToGrid(transform.position);

        // Occupy the cell in the GameManager's grid, so no other object can be placed there.
        GameManager.Instance.cellManager.OccupyCellWithBox(cellPosition, this);
    }

    /// <summary>
    /// Called when the user clicks on the SurpriseBox.
    /// If not already claimed and if the game is still in progress, the box applies a random prize,
    /// frees its grid cell, notifies the SurpriseBoxManager, and destroys itself.
    /// </summary>
    void OnMouseDown()
    {
        if (!isClaimed && !GameManager.Instance.isGameOver)
        {
            isClaimed = true;
            // Apply a random prize effect.
            DoRandomPrize();

            // Free the grid cell occupied by this box.
            GameManager.Instance.cellManager.FreeCellFromBox(cellPosition);

            // Notify the SurpriseBoxManager that this box has been claimed.
            SurpriseBoxManager.Instance.OnBoxClaimed(this);

            // Destroy the SurpriseBox.
            Destroy(gameObject);
        }
    }

    /// <summary>
    /// Forces the expiration of the SurpriseBox (e.g., when timed out).
    /// Frees the grid cell if the box has not been claimed, then destroys the box.
    /// </summary>
    public void ForceExpire()
    {
        if (!isClaimed)
        {
            // Free the grid cell.
            GameManager.Instance.cellManager.FreeCellFromBox(cellPosition);
        }
        Destroy(gameObject);
    }

    /// <summary>
    /// Determines and applies a random prize based on the following probabilities:
    /// - 80% chance to reward currency.
    /// - 10% chance to reset all special ability cooldowns.
    /// - 10% chance to destroy all balloons on the map.
    /// </summary>
    void DoRandomPrize()
    {
        // Generate a random value between 0 and 1.
        float roll = Random.value;

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

    /// <summary>
    /// Rewards the player with currency.
    /// The reward amount is determined by a random base value between 50 and 100 multiplied by a wave-based multiplier.
    /// </summary>
    void RewardMoney()
    {
        // Get the current wave index.
        int waveIndex = BalloonSpawner.Instance.GetCurrentWaveIndex();

        // Generate a random base value between 50 and 100.
        int randomVal = Random.Range(50, 101);
        // Calculate a multiplier based on the current wave.
        float multiplier = waveIndex * 0.5f;
        if (multiplier < 1f) multiplier = 1f; // Fallback for early waves.

        int finalMoney = Mathf.RoundToInt(randomVal * multiplier);

        // Add the rewarded currency to the player's total.
        GameManager.Instance.AddCurrency(finalMoney);
        Debug.Log($"SurpriseBox => Gave {finalMoney} currency!");
    }

    /// <summary>
    /// Resets all special ability cooldowns by calling the appropriate method on the SpecialAbilitiesManager.
    /// </summary>
    void ResetAllAbilities()
    {
        Debug.Log("SurpriseBox => Reset all special ability cooldowns!");
        SpecialAbilitiesManager.Instance.ResetAllCooldowns();
    }

    /// <summary>
    /// Destroys all balloons currently on the map by invoking the Pop method on each balloon.
    /// </summary>
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
