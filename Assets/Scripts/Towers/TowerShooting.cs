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

    // "Shoot()" method before projectile code & behaviour changes:
    //void Shoot()
    //{
    //    if (projectilePrefab == null || targetBalloon == null)
    //        return;

    //    GameObject projectileGO = Instantiate(projectilePrefab, firePoint.position, Quaternion.identity);
    //    Projectile projectile = projectileGO.GetComponent<Projectile>();
    //    if (projectile != null)
    //    {
    //        projectile.Seek(targetBalloon.transform);
    //    }

    //}


    void Shoot()
    {
        if (projectilePrefab == null || targetBalloon == null)
            return;

        // If we want a "predictive shot" instead of a homing shot:
        BalloonMovement balloonMov = targetBalloon.GetComponent<BalloonMovement>();
        if (balloonMov == null)
        {
            // fallback - no movement script found
            return;
        }

        // 1) find intercept time
        float projectileSpeed = 5f; // or you can store in projectilePrefab
        float balloonSpeed = targetBalloon.speed; // or balloonMov.balloon.speed
        Vector2 towerPos = firePoint.position;

        float interceptTime = InterceptSolver.FindInterceptTime(
            towerPos,
            balloonMov,
            balloonSpeed,
            projectileSpeed
        );

        // 2) find intercept position
        Vector2 interceptPos = balloonMov.PredictPositionInFuture(interceptTime);

        // 3) spawn projectile
        GameObject projGO = Instantiate(projectilePrefab, firePoint.position, Quaternion.identity);

        Projectile projectile = projGO.GetComponent<Projectile>();
        projectile.damage = damage;
        // pass no "target transform"
        // Instead we do a pure velocity approach

        // 4) set projectile velocity to go in a straight line
        Vector2 dir = interceptPos - (Vector2)firePoint.position;
        dir.Normalize();

        float angle = Mathf.Atan2(dir.y, dir.x) * Mathf.Rad2Deg;
        projGO.transform.rotation = Quaternion.Euler(0, 0, angle);

        // e.g., if Projectile has a RigidBody2D:
        Rigidbody2D rb = projGO.GetComponent<Rigidbody2D>();
        if (rb != null)
        {
            rb.velocity = dir * projectileSpeed;
        }
        else
        {
            // or if you do a "manual movement" in Projectile.cs, store the direction
            projectile.InitDirection(dir, projectileSpeed);
        }
    }

}