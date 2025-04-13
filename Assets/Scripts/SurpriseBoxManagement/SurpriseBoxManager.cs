using UnityEngine;
using System.Collections;
using System.Collections.Generic;

/// <summary>
/// Description:
/// Manages the lifecycle of Surprise Boxes that appear randomly on the map during gameplay.
/// This class handles the periodic spawning of surprise boxes, their self-despawn after a lifetime,
/// and provides functionality for when a box is claimed by the player.
/// </summary>
public class SurpriseBoxManager : MonoBehaviour
{
    /// <summary>
    /// Singleton instance of the SurpriseBoxManager.
    /// </summary>
    public static SurpriseBoxManager Instance;

    [Header("Surprise Box Prefab")]
    /// <summary>
    /// The prefab to use when spawning a surprise box.
    /// </summary>
    public GameObject surpriseBoxPrefab;

    [Header("Timing Settings")]
    /// <summary>
    /// Minimum time (in seconds) between spawns (e.g., 90 seconds = 1.5 minutes).
    /// </summary>
    public float minSpawnTime = 90f;
    /// <summary>
    /// Maximum time (in seconds) between spawns (e.g., 150 seconds = 2.5 minutes).
    /// </summary>
    public float maxSpawnTime = 150f;
    /// <summary>
    /// How long a spawned box remains active before expiring (if not claimed).
    /// </summary>
    public float boxLifetime = 5f;

    // Reference to the spawning coroutine.
    private Coroutine spawnRoutine;

    /// <summary>
    /// Awake is called when the script instance is being loaded.
    /// Implements the singleton pattern.
    /// </summary>
    void Awake()
    {
        if (Instance == null)
            Instance = this;
        else
            Destroy(gameObject);
    }

    /// <summary>
    /// Start is called before the first frame update.
    /// Begins the cycle of spawning surprise boxes.
    /// </summary>
    void Start()
    {
        spawnRoutine = StartCoroutine(SpawnCycle());
    }

    /// <summary>
    /// Coroutine that controls the spawn cycle of surprise boxes.
    /// It waits for a random duration between minSpawnTime and maxSpawnTime,
    /// then attempts to spawn a box if the game is still ongoing.
    /// </summary>
    /// <returns>An IEnumerator for the coroutine.</returns>
    private IEnumerator SpawnCycle()
    {
        while (!GameManager.Instance.isGameOver)
        {
            // Wait for a random interval before spawning the next box.
            float waitTime = Random.Range(minSpawnTime, maxSpawnTime);
            yield return new WaitForSeconds(waitTime);

            // If the game ended during the wait period, exit the coroutine.
            if (GameManager.Instance.isGameOver)
                yield break;

            // Attempt to spawn a surprise box.
            SpawnBox();
        }
    }

    /// <summary>
    /// Attempts to spawn a surprise box at a random free cell.
    /// If no free cell is found, it skips the spawn for this cycle.
    /// </summary>
    private void SpawnBox()
    {
        // Find a random free cell on the grid.
        Vector2? freeCell = FindRandomFreeCell();
        if (freeCell == null)
        {
            Debug.Log("No free cell found for surprise box. Skipping this round.");
            return;
        }

        // Spawn the surprise box at the chosen cell.
        Vector2 cellPos = freeCell.Value;
        GameObject boxGO = Instantiate(surpriseBoxPrefab, cellPos, Quaternion.identity);
        SurpriseBox sb = boxGO.GetComponent<SurpriseBox>();

        // Schedule automatic despawn if the box is not claimed within its lifetime.
        StartCoroutine(DespawnAfterDelay(sb, boxLifetime));
    }

    /// <summary>
    /// Waits for a specified delay, then forces the expiration of the given surprise box if it has not been claimed.
    /// </summary>
    /// <param name="sb">The SurpriseBox to potentially expire.</param>
    /// <param name="delay">The delay in seconds after which the box should expire.</param>
    /// <returns>An IEnumerator for the coroutine.</returns>
    private IEnumerator DespawnAfterDelay(SurpriseBox sb, float delay)
    {
        yield return new WaitForSeconds(delay);
        if (sb != null && !sb.isClaimed)
        {
            // Box expires; force it to free its cell and destroy itself.
            sb.ForceExpire();
        }
    }

    /// <summary>
    /// Called by a SurpriseBox when it is claimed by the player.
    /// Logs the event and continues the spawn cycle.
    /// </summary>
    /// <param name="box">The SurpriseBox that was claimed.</param>
    public void OnBoxClaimed(SurpriseBox box)
    {
        Debug.Log("Box was claimed by player!");
    }

    /// <summary>
    /// Finds a random free cell in which a surprise box can be spawned.
    /// </summary>
    /// <returns>A free cell position as a Vector2 if available; otherwise, null.</returns>
    private Vector2? FindRandomFreeCell()
    {
        List<Vector2> possibleCells = CollectAllAvailableCells();

        if (possibleCells.Count == 0)
            return null;

        // Randomly select a free cell from the available list.
        int randIndex = Random.Range(0, possibleCells.Count);
        return possibleCells[randIndex];
    }

    /// <summary>
    /// Collects all available cells on the grid where a surprise box can be spawned.
    /// A cell is considered available if it is within grid bounds, not on the balloon path,
    /// and not already occupied by a tower or another surprise box.
    /// </summary>
    /// <returns>A list of valid free cell positions as Vector2.</returns>
    private List<Vector2> CollectAllAvailableCells()
    {
        List<Vector2> possibleCells = new List<Vector2>();

        // The rectified grid origin values.
        float gridOriginXRectifiedPosition = -5.5f;
        float gridOriginYRectifiedPosition = -2.5f;
        float cSize = GridManager.Instance.cellSize; // Typically 0.5

        int w = TowerPlacement.Instance.gridWidth;  // e.g., 26
        int h = TowerPlacement.Instance.gridHeight; // e.g., 14

        // Loop through each grid cell based on grid dimensions.
        for (int gx = 0; gx < w; gx++)
        {
            for (int gy = 0; gy < h; gy++)
            {
                // Compute the center of the cell in world coordinates.
                float worldX = gridOriginXRectifiedPosition + gx * cSize + cSize / 2f;
                float worldY = gridOriginYRectifiedPosition + gy * cSize + cSize / 2f;
                Vector2 cellPos = new Vector2(worldX, worldY);

                // Verify that the cell is within the grid boundaries.
                if (!GridManager.Instance.IsWithinGrid(cellPos, w, h))
                    continue;

                // Exclude cells that are part of the balloon path.
                if (TowerPlacementUtils.IsOnBalloonPath(cellPos, TowerPlacement.Instance.tilemap, TowerPlacement.Instance.balloonPathPositions))
                    continue;

                // Exclude cells that are already occupied.
                if (GameManager.Instance.cellManager.IsCellOccupiedForAnyReason(cellPos))
                    continue;

                // If we reach here => it's a valid free cell
                possibleCells.Add(cellPos);
            }
        }

        return possibleCells;
    }
}
