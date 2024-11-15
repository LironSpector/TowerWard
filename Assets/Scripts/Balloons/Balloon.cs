using UnityEngine;
using System;


public class Balloon : MonoBehaviour
{
    public int health = 1;
    public float speed = 2f;
    public int reward = 10;
    public GameObject nextBalloonPrefab; // For higher-tier balloons

    public bool immuneToFreeze = false;
    public bool immuneToPoison = false;
    public bool isFrozen = false;
    public bool isPoisoned = false;

    private float freezeDuration = 0f;
    private float poisonDuration = 0f;
    private float poisonTickInterval = 1f; // Damage every second
    private float poisonTickTimer = 0f;

    private BalloonMovement movement; // Reference to BalloonMovement script

    // New event to notify when the balloon is destroyed
    public event Action<Balloon> OnDestroyed;
    // New event to notify when the balloon reaches the end of the path
    public event Action<Balloon> OnEndReached;

    void Start()
    {
        movement = GetComponent<BalloonMovement>();
    }

    public void TakeDamage(int damage)
    {
        health -= damage;
        if (health <= 0)
        {
            Pop();
        }
    }

    public void Freeze(float duration)
    {
        if (immuneToFreeze)
            return;

        isFrozen = true;
        freezeDuration = duration;
        // Optionally, change appearance to indicate freezing
    }

    public void Poison(float duration)
    {
        if (immuneToPoison)
            return;

        isPoisoned = true;
        poisonDuration = duration;
        poisonTickTimer = poisonTickInterval;
        // Optionally, change appearance to indicate poisoning
    }

    void Update()
    {
        // Handle freezing
        if (isFrozen)
        {
            freezeDuration -= Time.deltaTime;
            if (freezeDuration <= 0f)
            {
                isFrozen = false;
                // Optionally, revert appearance
            }
        }

        // Handle poisoning
        if (isPoisoned)
        {
            poisonDuration -= Time.deltaTime;
            poisonTickTimer -= Time.deltaTime;

            if (poisonTickTimer <= 0f)
            {
                poisonTickTimer = poisonTickInterval;
                TakeDamage(1);
            }

            if (poisonDuration <= 0f)
            {
                isPoisoned = false;
                // Optionally, revert appearance
            }
        }
    }

    void Pop()
    {
        // Spawn next balloon if applicable
        if (nextBalloonPrefab != null)
        {
            // Instantiate the next balloon at the current balloon's position
            GameObject nextBalloon = Instantiate(nextBalloonPrefab, transform.position, Quaternion.identity);

            // Set up the next balloon's movement
            BalloonMovement movement = nextBalloon.GetComponent<BalloonMovement>();
            BalloonMovement currentMovement = GetComponent<BalloonMovement>();

            movement.waypoints = currentMovement.waypoints;
            movement.waypointIndex = currentMovement.waypointIndex;
            movement.startFromExactPosition = true; // A flag to indicate the next balloon will start from the exact position of the popped balloon (and not from
                                                    // the next waypoint)
        }

        // Reward the player
        GameManager.Instance.AddCurrency(reward);

        // Notify listeners that this balloon is destroyed
        OnDestroyed?.Invoke(this);

        // Destroy the balloon
        Destroy(gameObject);
    }

    public void ReachEnd()
    {
        // Notify listeners that this balloon reached the end of the path
        OnEndReached?.Invoke(this);

        // Destroy the balloon
        Destroy(gameObject);
    }
}
