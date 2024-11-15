using UnityEngine;

public class GridManager : MonoBehaviour
{
    //Centralized class for managing grid-related actions

    public static GridManager Instance;

    public float gridOriginX = 0f;
    public float gridOriginY = 0f;
    public float cellSize = 0.5f;

    void Awake()
    {
        if (Instance == null)
            Instance = this;
        else
            Destroy(gameObject);
    }

    public Vector2 SnapToGrid(Vector2 position)
    {
        float snappedX = Mathf.Floor((position.x - gridOriginX) / cellSize) * cellSize + cellSize / 2 + gridOriginX;
        float snappedY = Mathf.Floor((position.y - gridOriginY) / cellSize) * cellSize + cellSize / 2 + gridOriginY;
        return new Vector2(snappedX, snappedY);
    }

    // Optional: You can add methods to get grid boundaries, check if a position is within the grid, etc.
    public bool IsWithinGrid(Vector2 position, int gridWidth, int gridHeight)
    {
        float gridOriginXRectifiedPosition = -5f;
        float gridOriginYRectifiedPosition = -2.5f;

        float gridMinX = gridOriginXRectifiedPosition;
        float gridMaxX = gridOriginXRectifiedPosition + (gridWidth * cellSize);
        float gridMinY = gridOriginYRectifiedPosition;
        float gridMaxY = gridOriginYRectifiedPosition + (gridHeight * cellSize);

        return position.x >= gridMinX && position.x <= gridMaxX && position.y >= gridMinY && position.y <= gridMaxY;
    }
}
