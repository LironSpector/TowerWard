using UnityEngine;

/// <summary>
/// Description:
/// Handles the movement of a balloon along a predefined series of waypoints.
/// This script controls the balloon's position updates, ensuring it follows the designated path,
/// and adjusts its movement based on status effects (e.g., frozen). It also provides functionality
/// to predict the balloon's future position over a given time interval.
/// </summary>
public class BalloonMovement : MonoBehaviour
{
    #region Public Fields

    /// <summary>
    /// Array of waypoints defining the path for the balloon.
    /// </summary>
    [HideInInspector]
    public Transform[] waypoints;

    /// <summary>
    /// Current index in the waypoints array that the balloon is moving toward.
    /// </summary>
    [HideInInspector]
    public int waypointIndex = 0;

    /// <summary>
    /// Flag to control whether the balloon starts exactly at its current position or resets to the first waypoint.
    /// </summary>
    public bool startFromExactPosition = false;

    #endregion

    #region Private Fields

    /// <summary>
    /// Reference to the Balloon component attached to this GameObject.
    /// </summary>
    private Balloon balloon;

    #endregion

    #region Unity Methods

    /// <summary>
    /// Awake is called when the script instance is being loaded.
    /// Retrieves and caches the Balloon component.
    /// </summary>
    void Awake()
    {
        balloon = GetComponent<Balloon>();
    }

    /// <summary>
    /// Start is called before the first frame update.
    /// Initializes the balloon's starting position based on the provided waypoints unless startFromExactPosition is true.
    /// </summary>
    void Start()
    {
        // If this is not a newly spawned balloon (i.e., we want to reset its position),
        // set the starting position to the current waypoint.
        if (!startFromExactPosition)
        {
            transform.position = waypoints[waypointIndex].position; // Normal starting point for balloons in the wave
        }
    }

    /// <summary>
    /// Update is called once per frame.
    /// Invokes the Move method to update the balloon's position along the path.
    /// </summary>
    void Update()
    {
        Move();
    }

    #endregion

    #region Movement Methods

    /// <summary>
    /// Moves the balloon along its defined waypoints.
    /// Skips movement if the balloon is frozen. Moves separately along the X and Y axes.
    /// When the balloon reaches the end of the waypoints, it reduces the player's lives and triggers the balloon's end behavior.
    /// </summary>
    void Move()
    {
        // Do not move if the balloon is frozen.
        if (balloon.isFrozen)
            return;

        if (waypointIndex < waypoints.Length)
        {
            Transform targetWaypoint = waypoints[waypointIndex];
            Vector3 direction = targetWaypoint.position - transform.position;

            // Calculate movement increments based on direction and balloon's speed.
            float moveX = Mathf.Sign(direction.x) * balloon.speed * Time.deltaTime;
            float moveY = Mathf.Sign(direction.y) * balloon.speed * Time.deltaTime;

            // Move along the X-axis if needed.
            if (Mathf.Abs(direction.x) > 0.01f)
            {
                transform.position += new Vector3(moveX, 0, 0);
            }
            // Otherwise move along the Y-axis.
            else if (Mathf.Abs(direction.y) > 0.01f)
            {
                transform.position += new Vector3(0, moveY, 0);
            }

            // Snap to the waypoint if the balloon is close enough, then proceed to the next waypoint.
            if (Vector2.Distance(transform.position, targetWaypoint.position) < 0.1f)
            {
                transform.position = targetWaypoint.position;
                waypointIndex++;
            }
        }
        else
        {
            // Balloon has reached the end of its path.
            int livesToDecrease = balloon.health;
            GameManager.Instance.LoseLife(livesToDecrease);
            balloon.ReachEnd(); // Trigger the ReachEnd method.
        }
    }

    #endregion

    #region Prediction Methods

    /// <summary>
    /// Predicts the balloon's position after a specified period into the future, assuming it continues along the same path.
    /// The simulation is performed in small time increments for better accuracy.
    /// </summary>
    /// <param name="timeAhead">The number of seconds into the future for which to predict the position.</param>
    /// <returns>
    /// A Vector2 representing the predicted position of the balloon.
    /// If the balloon reaches the end of the waypoints before timeAhead elapses, the last waypoint position is returned.
    /// </returns>
    public Vector2 PredictPositionInFuture(float timeAhead)
    {
        // Make temporary copies of the current position and waypoint index for simulation.
        Vector2 currentPos = transform.position;
        int currentIndex = waypointIndex;
        float remainTime = timeAhead;
        float speedB = balloon.speed;

        // Simulate movement along the waypoints until the remaining time is exhausted.
        while (remainTime > 0f && currentIndex < waypoints.Length)
        {
            Vector2 nextWaypointPos = waypoints[currentIndex].position;
            Vector2 dir = nextWaypointPos - currentPos;
            float dist = dir.magnitude;

            // Calculate the time required to reach the next waypoint.
            float timeToReach = dist / speedB;
            if (timeToReach <= remainTime)
            {
                // Move to the waypoint and update the remaining time.
                currentPos = nextWaypointPos;
                currentIndex++;
                remainTime -= timeToReach;
            }
            else
            {
                // The balloon will not reach the next waypoint in the remaining time.
                currentPos += dir.normalized * (speedB * remainTime);
                remainTime = 0f;
            }
        }

        // If all waypoints are traversed, the balloon remains at the final waypoint position.
        return currentPos;
    }

    #endregion
}
