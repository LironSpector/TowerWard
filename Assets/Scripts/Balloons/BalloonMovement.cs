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
            GameManager.Instance.LoseLife();
            balloon.ReachEnd(); // Trigger the ReachEnd method
            //Destroy(gameObject);
        }
    }



    // Old Move() function with less accuracty 
    //void Move()
    //{
    //    if (waypointIndex < waypoints.Length)
    //    {
    //        Transform targetWaypoint = waypoints[waypointIndex];
    //        float movementStep = balloon.speed * Time.deltaTime;
    //        transform.position = Vector2.MoveTowards(transform.position, targetWaypoint.position, movementStep);

    //        if (Vector2.Distance(transform.position, targetWaypoint.position) < 0.1f)
    //        {
    //            waypointIndex++;
    //        }
    //    }
    //    else
    //    {
    //        // Balloon reached the end
    //        GameManager.Instance.LoseLife();
    //        Destroy(gameObject);
    //    }
    //}

}
