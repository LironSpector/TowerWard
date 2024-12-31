//------- After balloon code & behaviour changes: -----------
using UnityEngine;
using System;

[RequireComponent(typeof(SpriteRenderer))]
public class Balloon : MonoBehaviour
{
    [Header("Current Balloon State")]
    public int health = 1;
    public float speed = 2f;    // Used by BalloonMovement
    public int reward = 5;

    [Header("Immunities")]
    public bool immuneToFreeze = false;
    public bool immuneToPoison = false;

    [Header("Status Effects")]
    public bool isFrozen = false;
    public bool isPoisoned = false;

    private float freezeDuration = 0f;
    private float poisonDuration = 0f;
    private float poisonTickInterval = 1f;
    private float poisonTickTimer = 0f;

    private BalloonMovement movement;
    private SpriteRenderer spriteRenderer;

    // We'll store references to different balloon sprites
    [Header("Balloon Sprites")]
    public Sprite redBalloonSprite;
    public Sprite blueBalloonSprite;
    public Sprite greenBalloonSprite;
    public Sprite yellowBalloonSprite;
    public Sprite pinkBalloonSprite;
    public Sprite blackBalloonSprite;
    public Sprite whiteBalloonSprite;
    public Sprite strongBalloonSprite;
    public Sprite strongerBalloonSprite;
    public Sprite veryStrongBalloonSprite;
    public Sprite smallBossBalloonSprite;
    public Sprite mediumBossBalloonSprite;
    public Sprite bigBossBalloonSprite;

    // Wave logic
    public bool isWaveBalloon = false;
    public int waveNumber = -1;        // The wave index this balloon belongs to

    // Events
    public event Action<Balloon> OnDestroyed;
    public event Action<Balloon> OnEndReached;

    void Awake()
    {
        movement = GetComponent<BalloonMovement>();
        spriteRenderer = GetComponent<SpriteRenderer>();
    }

    void Start()
    {
        // Right after we spawn, recalc sprite and stats from current health
        RecalculateAttributesBasedOnHealth();
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
                // Optionally revert appearance
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
                TakeDamage(1); // 1 damage per tick
            }

            if (poisonDuration <= 0f)
            {
                isPoisoned = false;
            }
        }
    }

    // ========================
    // Taking damage and popping
    // ========================
    public void TakeDamage(int damage)
    {
        int oldHealth = health;
        health -= damage;

        if (health > 0)
        {
            // Still alive => Recalc stats if crossing a threshold
            Sprite previousSprite = spriteRenderer.sprite;
            int prevReward = reward;
            RecalculateAttributesBasedOnHealth();
            if (previousSprite != spriteRenderer.sprite) //If the balloon has changed (for example from "StrongBalloon" to "WhiteBalloon"), add currency
            {
                GameManager.Instance.AddCurrency(prevReward);
            }
        }
        else
        {
            // Fully destroyed
            Pop();
        }
    }

    public void Pop()
    {
        // Reward the player
        GameManager.Instance.AddCurrency(reward);

        // Notify listeners
        OnDestroyed?.Invoke(this);

        // Actually destroy this balloon object
        Destroy(gameObject);
    }

    public void Freeze(float duration)
    {
        if (immuneToFreeze)
            return;

        isFrozen = true;
        freezeDuration = duration;
    }

    public void Poison(float duration)
    {
        if (immuneToPoison)
            return;

        isPoisoned = true;
        poisonDuration = duration;
        poisonTickTimer = poisonTickInterval;
    }

    // ========================
    // Recalculate stats based on Health
    // ========================
    public void RecalculateAttributesBasedOnHealth()
    {
        if (health <= 0) return;

        // (A) Identify which type the balloon is based on current health
        // (B) Set speed, reward, immunities, and sprite accordingly

        if (health == 1)
        {
            // Red
            speed = 2f;    // Movement speed
            reward = 5;
            immuneToFreeze = false;
            immuneToPoison = false;
            spriteRenderer.sprite = redBalloonSprite;
        }
        else if (health == 2)
        {
            // Blue
            speed = 2f;
            reward = 5;
            immuneToFreeze = false;
            immuneToPoison = false;
            spriteRenderer.sprite = blueBalloonSprite;
        }
        else if (health == 3)
        {
            // Green
            speed = 2f;
            reward = 5;
            immuneToFreeze = false;
            immuneToPoison = false;
            spriteRenderer.sprite = greenBalloonSprite;
        }
        else if (health == 4)
        {
            // Yellow
            speed = 2f;
            reward = 5;
            immuneToFreeze = false;
            immuneToPoison = false;
            spriteRenderer.sprite = yellowBalloonSprite;
        }
        else if (health == 5)
        {
            // Pink
            speed = 4f;
            reward = 5;
            immuneToFreeze = false;
            immuneToPoison = false;
            spriteRenderer.sprite = pinkBalloonSprite;
        }
        else if (health == 6)
        {
            // Black
            speed = 3f;
            reward = 5;
            immuneToFreeze = true;
            immuneToPoison = false;
            spriteRenderer.sprite = blackBalloonSprite;
        }
        else if (health == 7)
        {
            // White
            speed = 3f;
            reward = 5;
            immuneToFreeze = false;
            immuneToPoison = true;
            spriteRenderer.sprite = whiteBalloonSprite;
        }
        else if (health >= 8 && health <= 10)
        {
            // Strong
            speed = 2f;
            reward = 5;
            immuneToFreeze = false;
            immuneToPoison = false;
            // We might want “health = 3” for the actual leftover HP in this tier
            spriteRenderer.sprite = strongBalloonSprite;
        }
        else if (health >= 11 && health <= 16)
        {
            // Stronger
            speed = 1.5f;
            reward = 5;
            immuneToFreeze = false;
            immuneToPoison = false;
            // e.g., health = 6 in this range
            spriteRenderer.sprite = strongerBalloonSprite;
        }
        else if (health >= 17 && health <= 26)
        {
            // Very Strong
            speed = 1f;
            reward = 5;
            immuneToFreeze = false;
            immuneToPoison = false;
            spriteRenderer.sprite = veryStrongBalloonSprite;
        }
        else if (health >= 27 && health <= 126)
        {
            // Small Boss
            speed = 1.5f;
            reward = 5;
            immuneToFreeze = false;
            immuneToPoison = false;
            spriteRenderer.sprite = smallBossBalloonSprite;
        }
        else if (health >= 127 && health <= 626)
        {
            // Medium Boss
            speed = 1f;
            reward = 5;
            immuneToFreeze = false;
            immuneToPoison = false;
            spriteRenderer.sprite = mediumBossBalloonSprite;
        }
        else if (health >= 627 && health <= 3126)
        {
            // Big Boss
            speed = 0.5f;
            reward = 5;
            immuneToFreeze = false;
            immuneToPoison = false;
            spriteRenderer.sprite = bigBossBalloonSprite;
        }
    }

    // Reached the end of the path
    public void ReachEnd()
    {
        OnEndReached?.Invoke(this);
        Destroy(gameObject);
    }
}
