using UnityEngine;
using System.Collections.Generic;

/// <summary>
/// Description:
/// Implements the base shooting behavior for towers.
/// This class handles tracking balloons in range using a trigger collider, acquiring a target,
/// firing projectiles using predictive aiming (to hit moving balloons), and rotating the tower toward its target.
/// Derived classes can override methods (such as Shoot) to implement special projectile effects.
/// </summary>
public class BaseTowerShooting : MonoBehaviour
{
    /// <summary>
    /// The effective range of the tower.
    /// </summary>
    [HideInInspector] public float range = 2f;
    /// <summary>
    /// The firing rate of the tower (shots per second).
    /// </summary>
    [HideInInspector] public float fireRate = 1f;
    /// <summary>
    /// The damage inflicted by each projectile.
    /// </summary>
    [HideInInspector] public int damage = 1;

    /// <summary>
    /// The projectile prefab to spawn when shooting.
    /// </summary>
    public GameObject projectilePrefab;
    /// <summary>
    /// The transform from which projectiles are spawned.
    /// </summary>
    public Transform firePoint;

    /// <summary>
    /// Countdown timer to manage the delay between shots.
    /// </summary>
    protected float fireCountdown = 0f;
    /// <summary>
    /// List of balloons currently within the tower's range.
    /// </summary>
    protected List<Balloon> balloonsInRange = new List<Balloon>();
    /// <summary>
    /// The currently targeted balloon.
    /// </summary>
    protected Balloon targetBalloon;

    /// <summary>
    /// The circle collider used for detecting balloons in range.
    /// </summary>
    protected CircleCollider2D rangeCollider;

    /// <summary>
    /// Called when the script instance is being loaded.
    /// Ensures a CircleCollider2D is present and sets its radius to the tower's range.
    /// </summary>
    protected virtual void Start()
    {
        // Ensure a circle collider exists for range detection.
        rangeCollider = GetComponent<CircleCollider2D>();
        if (rangeCollider == null)
        {
            rangeCollider = gameObject.AddComponent<CircleCollider2D>();
            rangeCollider.isTrigger = true;
        }
        rangeCollider.radius = range;
    }

    /// <summary>
    /// Called once per frame.
    /// Acquires a target if one is not already set, handles firing projectiles at the target,
    /// and rotates the tower to face the target.
    /// </summary>
    protected virtual void Update()
    {
        // Acquire target if necessary.
        if (targetBalloon == null)
        {
            AcquireTarget();
        }

        if (targetBalloon != null)
        {
            if (fireCountdown <= 0f)
            {
                // Shoot a projectile at the target.
                Projectile spawnedProjectile = Shoot();
                fireCountdown = 1f / fireRate;
            }
            fireCountdown -= Time.deltaTime;

            // Rotate the tower toward the target.
            Vector2 direction = targetBalloon.transform.position - transform.position;
            float angle = Mathf.Atan2(direction.y, direction.x) * Mathf.Rad2Deg;
            int rotationOffset = -90; // Adjust rotation as needed.
            transform.rotation = Quaternion.Euler(0, 0, angle + rotationOffset);
        }
    }

    /// <summary>
    /// Acquires a target from the list of balloons currently in range.
    /// The default selection is the balloon with the highest waypoint index (i.e., furthest along the path).
    /// </summary>
    protected virtual void AcquireTarget()
    {
        // Remove any null entries that may exist due to destroyed balloons.
        balloonsInRange.RemoveAll(balloon => balloon == null);

        if (balloonsInRange.Count > 0)
        {
            // Default target is the first balloon.
            targetBalloon = balloonsInRange[0];
            int highestWP = targetBalloon.GetComponent<BalloonMovement>().waypointIndex;

            // Iterate through the list to find the balloon furthest along.
            foreach (Balloon b in balloonsInRange)
            {
                int wpIndex = b.GetComponent<BalloonMovement>().waypointIndex;
                if (wpIndex > highestWP)
                {
                    targetBalloon = b;
                    highestWP = wpIndex;
                }
            }
        }
    }

    /// <summary>
    /// Creates and launches a projectile towards the specified balloon using predictive aiming.
    /// The method calculates the intercept time and predicts the future position of the balloon,
    /// spawns a projectile, and sets its direction and velocity accordingly.
    /// </summary>
    /// <param name="balloon">The target balloon.</param>
    /// <returns>The spawned Projectile component, or null if spawning fails.</returns>
    protected virtual Projectile CreateAndLaunchProjectile(Balloon balloon)
    {
        if (projectilePrefab == null || balloon == null)
            return null;

        // (Optional) Play projectile shooting sound:
        // AudioManager.Instance.PlayProjectileShot();

        BalloonMovement balloonMov = balloon.GetComponent<BalloonMovement>();
        if (balloonMov == null)
            return null;

        float projectileSpeed = 5f;
        float balloonSpeed = balloon.speed;
        Vector2 towerPos = firePoint.position;

        // Calculate intercept time using predictive logic.
        float interceptTime = InterceptSolver.FindInterceptTime(
            towerPos,
            balloonMov,
            balloonSpeed,
            projectileSpeed
        );

        // Predict where the balloon will be after the calculated time.
        Vector2 interceptPos = balloonMov.PredictPositionInFuture(interceptTime);

        // Spawn the projectile at the fire point.
        GameObject projGO = Instantiate(projectilePrefab, firePoint.position, Quaternion.identity);
        Projectile p = projGO.GetComponent<Projectile>();
        p.damage = damage;

        // Calculate the direction for the projectile.
        Vector2 dir = interceptPos - (Vector2)firePoint.position;
        dir.Normalize();

        // Set the projectile's rotation to face its travel direction.
        float angle = Mathf.Atan2(dir.y, dir.x) * Mathf.Rad2Deg;
        projGO.transform.rotation = Quaternion.Euler(0, 0, angle);

        // Attempt to set the projectile's velocity using Rigidbody2D; if not present, use InitDirection.
        Rigidbody2D rb = projGO.GetComponent<Rigidbody2D>();
        if (rb != null)
        {
            rb.velocity = dir * projectileSpeed;
        }
        else
        {
            p.InitDirection(dir, projectileSpeed);
        }

        return p;
    }

    /// <summary>
    /// Initiates shooting by creating and launching a projectile towards the current target.
    /// This base implementation does not apply any special projectile effects.
    /// </summary>
    /// <returns>The spawned Projectile, or null if shooting fails.</returns>
    protected virtual Projectile Shoot()
    {
        return CreateAndLaunchProjectile(targetBalloon);
    }

    /// <summary>
    /// Called when a collider enters the tower's trigger area.
    /// If the collider belongs to a balloon, it adds the balloon to the list of targets in range.
    /// </summary>
    /// <param name="other">The collider that entered the trigger.</param>
    protected virtual void OnTriggerEnter2D(Collider2D other)
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

    /// <summary>
    /// Called when a collider exits the tower's trigger area.
    /// If the collider belongs to a balloon, it removes the balloon from the list of targets in range
    /// and clears the current target if it is no longer valid.
    /// </summary>
    /// <param name="other">The collider that exited the trigger.</param>
    protected virtual void OnTriggerExit2D(Collider2D other)
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
}
