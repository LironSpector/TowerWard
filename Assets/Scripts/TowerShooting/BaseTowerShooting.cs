using UnityEngine;
using System.Collections.Generic;

public class BaseTowerShooting : MonoBehaviour
{
    [HideInInspector] public float range = 2f;
    [HideInInspector] public float fireRate = 1f;
    [HideInInspector] public int damage = 1;

    public GameObject projectilePrefab;
    public Transform firePoint;

    protected float fireCountdown = 0f;
    protected List<Balloon> balloonsInRange = new List<Balloon>();
    protected Balloon targetBalloon;

    protected CircleCollider2D rangeCollider;

    protected virtual void Start()
    {
        // Ensure there's a circle collider for range
        rangeCollider = GetComponent<CircleCollider2D>();
        if (rangeCollider == null)
        {
            rangeCollider = gameObject.AddComponent<CircleCollider2D>();
            rangeCollider.isTrigger = true;
        }
        rangeCollider.radius = range;
    }

    protected virtual void Update()
    {
        if (targetBalloon == null)
        {
            AcquireTarget();
        }

        if (targetBalloon != null)
        {
            if (fireCountdown <= 0f)
            {
                Projectile spawnedProjectile = Shoot();
                fireCountdown = 1f / fireRate;
            }
            fireCountdown -= Time.deltaTime;

            // Rotate tower towards target
            Vector2 direction = targetBalloon.transform.position - transform.position;
            float angle = Mathf.Atan2(direction.y, direction.x) * Mathf.Rad2Deg;
            int rotationOffset = -90;
            transform.rotation = Quaternion.Euler(0, 0, angle + rotationOffset);
        }
    }

    protected virtual void AcquireTarget()
    {
        // Clean up null entries
        balloonsInRange.RemoveAll(balloon => balloon == null);

        if (balloonsInRange.Count > 0)
        {
            // Default: target balloon with the highest waypoint index
            targetBalloon = balloonsInRange[0];
            int highestWP = targetBalloon.GetComponent<BalloonMovement>().waypointIndex;

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

    protected virtual Projectile CreateAndLaunchProjectile(Balloon balloon)
    {
        // This method does the predictive logic and spawns a projectile
        if (projectilePrefab == null || balloon == null) return null;

        //AudioManager.Instance.PlayProjectileShot(); // pass an index if different towers have different clips

        BalloonMovement balloonMov = balloon.GetComponent<BalloonMovement>();
        if (balloonMov == null) return null;

        float projectileSpeed = 5f;
        float balloonSpeed = balloon.speed;
        Vector2 towerPos = firePoint.position;

        float interceptTime = InterceptSolver.FindInterceptTime(
            towerPos,
            balloonMov,
            balloonSpeed,
            projectileSpeed
        );

        Vector2 interceptPos = balloonMov.PredictPositionInFuture(interceptTime);

        // Spawn
        GameObject projGO = Instantiate(projectilePrefab, firePoint.position, Quaternion.identity);
        Projectile p = projGO.GetComponent<Projectile>();
        p.damage = damage;

        // Direction
        Vector2 dir = interceptPos - (Vector2)firePoint.position;
        dir.Normalize();

        float angle = Mathf.Atan2(dir.y, dir.x) * Mathf.Rad2Deg;
        projGO.transform.rotation = Quaternion.Euler(0, 0, angle);

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

    protected virtual Projectile Shoot()
    {
        // Base tower just spawns the projectile with no special effect
        return CreateAndLaunchProjectile(targetBalloon);
    }

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
