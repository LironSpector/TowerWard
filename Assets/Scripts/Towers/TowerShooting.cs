//using UnityEngine;
//using System.Collections.Generic;

//public class TowerShooting : MonoBehaviour
//{
//    public float range = 2f;
//    public float fireRate = 1f;
//    public GameObject projectilePrefab;
//    public Transform firePoint;

//    private float fireCountdown = 0f;
//    private List<Balloon> balloonsInRange = new List<Balloon>();
//    private Balloon targetBalloon;

//    void Start()
//    {
//        // Optional: Visualize the range
//        CircleCollider2D rangeCollider = gameObject.AddComponent<CircleCollider2D>();
//        rangeCollider.isTrigger = true;
//        rangeCollider.radius = range;
//    }

//    void Update()
//    {
//        if (targetBalloon == null)
//        {
//            AcquireTarget();
//        }

//        if (targetBalloon != null)
//        {
//            if (fireCountdown <= 0f)
//            {
//                Shoot();
//                fireCountdown = 1f / fireRate;
//            }
//            fireCountdown -= Time.deltaTime;

//            // Optionally rotate tower towards target
//            Vector2 direction = targetBalloon.transform.position - transform.position;
//            float angle = Mathf.Atan2(direction.y, direction.x) * Mathf.Rad2Deg;
//            int rotationOffset = -90; //By default my sprite images (like monkey images) are facing up, but the calculation in Quaternion.Euler is done
//                                      //based on the assumption that it's facing right by default, so a "rotationOffset" of 90 degrees to the right is needed.
//            transform.rotation = Quaternion.Euler(0, 0, angle + rotationOffset);
//        }
//    }

//    void AcquireTarget()
//    {
//        // Clean up null entries
//        balloonsInRange.RemoveAll(balloon => balloon == null);

//        if (balloonsInRange.Count > 0)
//        {
//            // Target the balloon with the highest waypoint index
//            targetBalloon = balloonsInRange[0];
//            int highestWaypointIndex = targetBalloon.GetComponent<BalloonMovement>().waypointIndex;

//            foreach (Balloon balloon in balloonsInRange)
//            {
//                int balloonWaypointIndex = balloon.GetComponent<BalloonMovement>().waypointIndex;
//                if (balloonWaypointIndex > highestWaypointIndex)
//                {
//                    targetBalloon = balloon;
//                    highestWaypointIndex = balloonWaypointIndex;
//                }
//            }
//        }
//    }

//    void OnTriggerEnter2D(Collider2D other)
//    {
//        if (other.CompareTag("Balloon"))
//        {
//            Balloon balloon = other.GetComponent<Balloon>();
//            if (balloon != null)
//            {
//                balloonsInRange.Add(balloon);
//            }
//        }
//    }

//    void OnTriggerExit2D(Collider2D other)
//    {
//        if (other.CompareTag("Balloon"))
//        {
//            Balloon balloon = other.GetComponent<Balloon>();
//            if (balloon != null)
//            {
//                balloonsInRange.Remove(balloon);
//                if (balloon == targetBalloon)
//                {
//                    targetBalloon = null;
//                }
//            }
//        }
//    }

//    void Shoot()
//    {
//        if (projectilePrefab == null || targetBalloon == null)
//            return;

//        GameObject projectileGO = Instantiate(projectilePrefab, firePoint.position, Quaternion.identity);
//        Projectile projectile = projectileGO.GetComponent<Projectile>();
//        if (projectile != null)
//        {
//            projectile.Seek(targetBalloon.transform);
//        }

//    }
//}




using UnityEngine;
using System.Collections.Generic;

public class TowerShooting : MonoBehaviour
{
    //Hard-coded values at first, but these are changed to the values from TowerData (this happens in "Tower.cs" script)
    [HideInInspector]
    public float range = 2f;
    [HideInInspector]
    public float fireRate = 1f;
    [HideInInspector]
    public int damage = 1;

    public GameObject projectilePrefab;
    public Transform firePoint;

    private float fireCountdown = 0f;
    private List<Balloon> balloonsInRange = new List<Balloon>();
    private Balloon targetBalloon;

    private CircleCollider2D rangeCollider;

    void Start()
    {
        // Get the range collider (assumed to be added in the Tower script)
        rangeCollider = GetComponent<CircleCollider2D>();
        if (rangeCollider == null)
        {
            rangeCollider = gameObject.AddComponent<CircleCollider2D>();
            rangeCollider.isTrigger = true;
        }
        rangeCollider.radius = range;
    }

    void Update()
    {
        if (targetBalloon == null)
        {
            AcquireTarget();
        }

        if (targetBalloon != null)
        {
            if (fireCountdown <= 0f)
            {
                Shoot();
                fireCountdown = 1f / fireRate;
            }
            fireCountdown -= Time.deltaTime;

            // Optionally rotate tower towards target
            Vector2 direction = targetBalloon.transform.position - transform.position;
            float angle = Mathf.Atan2(direction.y, direction.x) * Mathf.Rad2Deg;
            int rotationOffset = -90; //By default my sprite images (like monkey images) are facing up, but the calculation in Quaternion.Euler is done
                                      //based on the assumption that it's facing right by default, so a "rotationOffset" of 90 degrees to the right is needed.
            transform.rotation = Quaternion.Euler(0, 0, angle + rotationOffset);
        }
    }

    void AcquireTarget()
    {
        // Clean up null entries
        balloonsInRange.RemoveAll(balloon => balloon == null);

        if (balloonsInRange.Count > 0)
        {
            // Target the balloon with the highest waypoint index
            targetBalloon = balloonsInRange[0];
            int highestWaypointIndex = targetBalloon.GetComponent<BalloonMovement>().waypointIndex;

            foreach (Balloon balloon in balloonsInRange)
            {
                int balloonWaypointIndex = balloon.GetComponent<BalloonMovement>().waypointIndex;
                if (balloonWaypointIndex > highestWaypointIndex)
                {
                    targetBalloon = balloon;
                    highestWaypointIndex = balloonWaypointIndex;
                }
            }
        }
    }

    void OnTriggerEnter2D(Collider2D other)
    {
        if (other.CompareTag("Balloon"))
        {
            Balloon balloon = other.GetComponent<Balloon>();
            if (balloon != null)
            {
                balloonsInRange.Add(balloon);
            }
        }
    }

    void OnTriggerExit2D(Collider2D other)
    {
        if (other.CompareTag("Balloon"))
        {
            Balloon balloon = other.GetComponent<Balloon>();
            if (balloon != null)
            {
                balloonsInRange.Remove(balloon);
                if (balloon == targetBalloon)
                {
                    targetBalloon = null;
                }
            }
        }
    }

    void Shoot()
    {
        if (projectilePrefab == null || targetBalloon == null)
            return;

        GameObject projectileGO = Instantiate(projectilePrefab, firePoint.position, Quaternion.identity);
        Projectile projectile = projectileGO.GetComponent<Projectile>();
        if (projectile != null)
        {
            projectile.Seek(targetBalloon.transform);
        }

    }
}