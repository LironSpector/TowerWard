using UnityEngine;

public class GridManager : MonoBehaviour
{
    //Centralized class for managing grid-related actions

    public static GridManager Instance;

    public float gridOriginX = 0f;
    public float gridOriginY = 0f;
    public float cellSize = 0.5f;
    //public float cellSize = 0.5625f;

    void Awake()
    {
        if (Instance == null)
            Instance = this;
        else
            Destroy(gameObject);
    }

    public Vector2 SnapToGrid(Vector2 position)
    {
        //Debug.Log("division is: " + (position.x - gridOriginX) / cellSize);
        //Debug.Log("division floored is: " + Mathf.Floor((position.x - gridOriginX) / cellSize));
        //Debug.Log("division floored is: " + (Mathf.Floor((position.x - gridOriginX) / cellSize) * cellSize));
        float snappedX = Mathf.Floor((position.x - gridOriginX) / cellSize) * cellSize + cellSize / 2 + gridOriginX;
        float snappedY = Mathf.Floor((position.y - gridOriginY) / cellSize) * cellSize + cellSize / 2 + gridOriginY;
        //Debug.Log("SnappedX: " + snappedX + ", SnappedY" + snappedY);

        //return new Vector2(snappedX + 0.185f, snappedY + 0.28125f);
        return new Vector2(snappedX, snappedY);
    }


    // Optional: You can add methods to get grid boundaries, check if a position is within the grid, etc.
    public bool IsWithinGrid(Vector2 position, int gridWidth, int gridHeight)
    {
        //float gridOriginXRectifiedPosition = -1f; //Used to be -5f before changing the map location.
        //float gridOriginYRectifiedPosition = -0.5f; //Used to be -2.5f before changing the map location.

        //float gridOriginXRectifiedPosition = -5.6f; //Used to be -5f before changing the map location.
        //float gridOriginYRectifiedPosition = -1.05f; //Used to be -2.5f before changing the map location.
        float gridOriginXRectifiedPosition = -5.5f; //Used to be -5f before changing the map location.
        float gridOriginYRectifiedPosition = -2.5f; //Used to be -2.5f before changing the map location.


        float gridMinX = gridOriginXRectifiedPosition;
        float gridMaxX = gridOriginXRectifiedPosition + (gridWidth * cellSize);
        float gridMinY = gridOriginYRectifiedPosition;
        float gridMaxY = gridOriginYRectifiedPosition + (gridHeight * cellSize);
        Debug.Log("Grid checks:");
        Debug.Log("position: " + position);
        Debug.Log("gridMinX: " + gridMinX + ", gridMaxX: " + gridMaxX + ", gridMinY: " + gridMinY + ", gridMaxY: " + gridMaxY);

        return position.x >= gridMinX && position.x <= gridMaxX && position.y >= gridMinY && position.y <= gridMaxY;
    }
}
