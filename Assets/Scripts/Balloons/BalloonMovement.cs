using UnityEngine;

public class BalloonMovement : MonoBehaviour
{
    [HideInInspector]
    public Transform[] waypoints;
    [HideInInspector]
    public int waypointIndex = 0;
    public bool startFromExactPosition = false; // New flag to control starting position


    private Balloon balloon;

    void Start()
    {
        balloon = GetComponent<Balloon>();

        // If this is a newly spawned balloon after popping the previous one, do not reset position to waypoint (skip the if statement and keep the exact position)
        if (!startFromExactPosition)
        {
            transform.position = waypoints[waypointIndex].position; // Normal starting point for balloons in the wave
        }
    }

    void Update()
    {
        Move();
    }

    void Move()
    {
        if (balloon.isFrozen)
            return; // Skip movement while frozen

        if (waypointIndex < waypoints.Length)
        {
            Transform targetWaypoint = waypoints[waypointIndex];
            Vector3 direction = targetWaypoint.position - transform.position;

            // Determine movement along X and Y separately
            float moveX = Mathf.Sign(direction.x) * balloon.speed * Time.deltaTime;
            float moveY = Mathf.Sign(direction.y) * balloon.speed * Time.deltaTime;

            // Move along X-axis
            if (Mathf.Abs(direction.x) > 0.01f)
            {
                transform.position += new Vector3(moveX, 0, 0);
            }
            // Move along Y-axis
            else if (Mathf.Abs(direction.y) > 0.01f)
            {
                transform.position += new Vector3(0, moveY, 0);
            }

            // Check if close enough to waypoint
            if (Vector2.Distance(transform.position, targetWaypoint.position) < 0.1f)
            {
                transform.position = targetWaypoint.position; // Snap to waypoint
                waypointIndex++;
            }
        }
        else
        {
            // Balloon reached the end
            int livesToDecrease = balloon.health;
            GameManager.Instance.LoseLife(livesToDecrease);
            balloon.ReachEnd(); // Trigger the ReachEnd method
            //Destroy(gameObject);
        }
    }


    /// <summary>
    /// Predict the balloon's position after 'timeAhead' seconds from now,
    /// assuming it continues forward along the same path, turning at waypoints, etc.
    /// We'll do a small piecewise simulation, ignoring large DT steps for better accuracy.
    /// </summary>
    public Vector2 PredictPositionInFuture(float timeAhead)
    {
        // We'll make a temp copy of balloon's path state and simulate forward
        Vector2 currentPos = transform.position;
        int currentIndex = waypointIndex;
        float remainTime = timeAhead;
        float speedB = balloon.speed;

        while (remainTime > 0f && currentIndex < waypoints.Length)
        {
            // distance to next waypoint
            Vector2 nextWaypointPos = waypoints[currentIndex].position;
            Vector2 dir = nextWaypointPos - currentPos;
            float dist = dir.magnitude;

            float timeToReach = dist / speedB;
            if (timeToReach <= remainTime)
            {
                // we reach the waypoint and maybe move on
                currentPos = nextWaypointPos;
                currentIndex++;
                remainTime -= timeToReach;
            }
            else
            {
                // we won't reach the next waypoint in remainTime
                currentPos += dir.normalized * (speedB * remainTime);
                remainTime = 0f;
            }
        }

        // If we run out of waypoints, we stay at the last position
        return currentPos;
    }

}
