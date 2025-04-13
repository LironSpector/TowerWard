using UnityEngine;
using System.Collections;
using System.Collections.Generic;

public class SurpriseBoxManager : MonoBehaviour
{
    public static SurpriseBoxManager Instance;

    [Header("Surprise Box Prefab")]
    public GameObject surpriseBoxPrefab;

    [Header("Timing Settings")]
    public float minSpawnTime = 90f;   // 1.5 min
    public float maxSpawnTime = 150f;  // 2.5 min
    public float boxLifetime = 5f;     // box remains for 5s

    private Coroutine spawnRoutine;

    void Awake()
    {
        if (Instance == null) Instance = this;
        else Destroy(gameObject);
    }

    void Start()
    {
        spawnRoutine = StartCoroutine(SpawnCycle());
    }

    private IEnumerator SpawnCycle()
    {
        while (!GameManager.Instance.isGameOver)
        {
            // wait random time
            float waitTime = Random.Range(minSpawnTime, maxSpawnTime);
            yield return new WaitForSeconds(waitTime);

            // If game ended during wait, break
            if (GameManager.Instance.isGameOver) yield break;

            // Attempt to spawn the box
            SpawnBox();
        }
    }

    private void SpawnBox()
    {
        // 1) find random free cell
        Vector2? freeCell = FindRandomFreeCell();
        if (freeCell == null)
        {
            Debug.Log("No free cell found for surprise box. Skipping this round.");
            return;
        }

        // 2) spawn
        Vector2 cellPos = freeCell.Value;
        GameObject boxGO = Instantiate(surpriseBoxPrefab, cellPos, Quaternion.identity);
        SurpriseBox sb = boxGO.GetComponent<SurpriseBox>();

        // 3) schedule a self-despawn after boxLifetime if not claimed
        StartCoroutine(DespawnAfterDelay(sb, boxLifetime));
    }

    private IEnumerator DespawnAfterDelay(SurpriseBox sb, float delay)
    {
        yield return new WaitForSeconds(delay);
        if (sb != null && !sb.isClaimed)
        {
            // box expired
            sb.ForceExpire();
            // That frees the cell and destroys the box
        }
    }

    public void OnBoxClaimed(SurpriseBox box)
    {
        // The user clicked the box => it's destroyed => do nothing special here
        // the cycle continues
        Debug.Log("Box was claimed by player!");
    }

    // We gather all possible free cells ignoring path or occupancy
    private Vector2? FindRandomFreeCell()
    {
        List<Vector2> possibleCells = CollectAllAvailableCells();

        if (possibleCells.Count == 0)
            return null;

        // pick random from possibleCells
        int randIndex = Random.Range(0, possibleCells.Count);
        return possibleCells[randIndex];
    }

    private List<Vector2> CollectAllAvailableCells()
    {
        List<Vector2> possibleCells = new List<Vector2>();

        float gridOriginXRectifiedPosition = -5.5f;
        float gridOriginYRectifiedPosition = -2.5f;
        float cSize = GridManager.Instance.cellSize; // typically 0.5

        int w = TowerPlacement.Instance.gridWidth;  // 26
        int h = TowerPlacement.Instance.gridHeight; // 14

        for (int gx = 0; gx < w; gx++)
        {
            for (int gy = 0; gy < h; gy++)
            {
                // Compute the center in world coords
                float worldX = gridOriginXRectifiedPosition + gx * cSize + cSize / 2f;
                float worldY = gridOriginYRectifiedPosition + gy * cSize + cSize / 2f;

                Vector2 cellPos = new Vector2(worldX, worldY);

                // 1) Check if it's within the bounding rect (via IsWithinGrid)
                if (!GridManager.Instance.IsWithinGrid(cellPos, w, h))
                    continue;

                // 2) Check if it's on the balloon path
                if (TowerPlacementUtils.IsOnBalloonPath(cellPos, TowerPlacement.Instance.tilemap, TowerPlacement.Instance.balloonPathPositions))
                    continue;

                // 3) Check if occupied by tower or box
                if (GameManager.Instance.cellManager.IsCellOccupiedForAnyReason(cellPos))
                    continue;

                // If we reach here => it's a valid free cell
                possibleCells.Add(cellPos);
            }
        }

        return possibleCells;
    }

}
