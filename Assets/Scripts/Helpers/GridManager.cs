using UnityEngine;

/// <summary>
/// Description:
/// Centralized class for managing grid-related actions, such as snapping positions to a grid
/// and determining if a given position is within the bounds of the grid.
/// This class implements the singleton pattern for global access.
/// </summary>
public class GridManager : MonoBehaviour
{
    /// <summary>
    /// The singleton instance of the GridManager.
    /// </summary>
    public static GridManager Instance;

    /// <summary>
    /// The X coordinate for the grid origin.
    /// </summary>
    public float gridOriginX = 0f;

    /// <summary>
    /// The Y coordinate for the grid origin.
    /// </summary>
    public float gridOriginY = 0f;

    /// <summary>
    /// The size of each cell in the grid.
    /// </summary>
    public float cellSize = 0.5f;

    /// <summary>
    /// Awake is called when the script instance is being loaded.
    /// Implements the singleton pattern by ensuring that only one instance of GridManager exists.
    /// </summary>
    void Awake()
    {
        if (Instance == null)
            Instance = this;
        else
            Destroy(gameObject);
    }

    /// <summary>
    /// Snaps a given position to the center of the nearest grid cell.
    /// </summary>
    /// <param name="position">
    /// The original position (in world coordinates) to be snapped.
    /// </param>
    /// <returns>
    /// A <see cref="Vector2"/> representing the center position of the grid cell that the input position aligns with.
    /// </returns>
    public Vector2 SnapToGrid(Vector2 position)
    {
        float snappedX = Mathf.Floor((position.x - gridOriginX) / cellSize) * cellSize + cellSize / 2 + gridOriginX;
        float snappedY = Mathf.Floor((position.y - gridOriginY) / cellSize) * cellSize + cellSize / 2 + gridOriginY;

        return new Vector2(snappedX, snappedY);
    }

    /// <summary>
    /// Determines whether a given position is within the defined grid bounds.
    /// The grid bounds are defined by a "rectified" grid origin and the specified grid width and height.
    /// </summary>
    /// <param name="position">
    /// The position (in world coordinates) to check.
    /// </param>
    /// <param name="gridWidth">
    /// The number of cells in the grid horizontally.
    /// </param>
    /// <param name="gridHeight">
    /// The number of cells in the grid vertically.
    /// </param>
    /// <returns>
    /// True if the position is within the bounds of the grid; otherwise, false.
    /// </returns>
    public bool IsWithinGrid(Vector2 position, int gridWidth, int gridHeight)
    {
        // These rectified values account for adjustments after changing map location and size.
        float gridOriginXRectifiedPosition = -5.5f; // Used to be -5f before changing the map location and size.
        float gridOriginYRectifiedPosition = -2.5f; // Used to be -2.5f before changing the map location and size.

        float gridMinX = gridOriginXRectifiedPosition;
        float gridMaxX = gridOriginXRectifiedPosition + (gridWidth * cellSize);
        float gridMinY = gridOriginYRectifiedPosition;
        float gridMaxY = gridOriginYRectifiedPosition + (gridHeight * cellSize);

        return position.x >= gridMinX && position.x <= gridMaxX && position.y >= gridMinY && position.y <= gridMaxY;
    }
}
