using UnityEngine;
using System;

[RequireComponent(typeof(SpriteRenderer))]
public class Balloon : MonoBehaviour
{
    [Header("Current Balloon State")]
    public int health = 1;
    public float speed = 2f; // Used by BalloonMovement
    public int reward = 5;

    [Header("Immunities")]
    public bool immuneToFreeze = false;
    public bool immuneToPoison = false;

    [Header("Status Effects")]
    public bool isFrozen = false;
    public bool isPoisoned = false;
    public bool isSlowed = false;

    private float freezeDuration = 0f;
    private float poisonDuration = 0f;
    private float poisonTickInterval = 1f;
    private float poisonTickTimer = 0f;
    private float slowDuration = 0f;
    private float slowFactor = 0.5f;
    private float originalSpeed = 2f;  // Used to store base speed for slow calculations

    private BalloonMovement movement;
    private SpriteRenderer spriteRenderer;

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

    // Newly added for visuals:
    public Sprite frozenBalloonSprite;
    public Sprite poisonedBalloonSprite;

    // Wave logic
    public bool isWaveBalloon = false;
    public int waveNumber = -1;

    // Events
    public event Action<Balloon> OnDestroyed;
    public event Action<Balloon> OnEndReached;

    // We store the “normal” sprite that corresponds to the current health
    private Sprite normalSprite;

    void Awake()
    {
        movement = GetComponent<BalloonMovement>();
        spriteRenderer = GetComponent<SpriteRenderer>();
    }

    void Start()
    {
        RecalculateAttributesBasedOnHealth();
    }

    void Update()
    {
        // ==============
        // 1) Handle Timers
        // ==============

        // Freezing
        if (isFrozen)
        {
            freezeDuration -= Time.deltaTime;
            if (freezeDuration <= 0f)
            {
                isFrozen = false;
            }
        }

        // Poisoning
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
            }
        }

        // Slowing
        if (isSlowed)
        {
            slowDuration -= Time.deltaTime;
            if (slowDuration <= 0f)
            {
                isSlowed = false;
            }
        }

        // ==============
        // 2) Update Speed
        // ==============
        // Priority: If Frozen => speed=0, else if Slowed => speed=originalSpeed * slowFactor, else => speed=originalSpeed
        if (isFrozen)
        {
            speed = 0f;
        }
        else if (isSlowed)
        {
            speed = originalSpeed * slowFactor * GameManager.Instance.allBalloonsSpeedFactor;
        }
        else
        {
            speed = originalSpeed * GameManager.Instance.allBalloonsSpeedFactor;
        }

        // ==============
        // 3) Update Sprite
        // ==============
        // Priority: if Frozen => frozenBalloonSprite, else if Poisoned => poisonedBalloonSprite, else => normalSprite
        if (isFrozen && frozenBalloonSprite != null)
        {
            spriteRenderer.sprite = frozenBalloonSprite;
        }
        else if (isPoisoned && poisonedBalloonSprite != null)
        {
            spriteRenderer.sprite = poisonedBalloonSprite;
        }
        else
        {
            // revert to normal sprite based on current health
            spriteRenderer.sprite = normalSprite;
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
            // Recalc if crossing a threshold
            Sprite previousSprite = spriteRenderer.sprite;
            int prevReward = reward;
            RecalculateAttributesBasedOnHealth();

            // If the base sprite changed from e.g. “strong” to “white,” add currency
            if (previousSprite != spriteRenderer.sprite)
            {
                GameManager.Instance.AddCurrency(prevReward);
            }
        }
        else
        {
            // Destroy
            Pop();
        }
    }

    public void Pop()
    {
        GameManager.Instance.AddCurrency(reward);

        AudioManager.Instance.PlayBalloonPop();

        OnDestroyed?.Invoke(this);
        Destroy(gameObject);
    }

    public void Freeze(float duration)
    {
        if (immuneToFreeze) return;

        isFrozen = true;
        freezeDuration = duration;
    }

    public void Poison(float duration)
    {
        if (immuneToPoison) return;

        isPoisoned = true;
        poisonDuration = duration;
        poisonTickTimer = poisonTickInterval;
    }

    public void ApplySlow(float duration, float factor)
    {
        // If balloon is immune or something, skip
        isSlowed = true;
        slowDuration = duration;
        slowFactor = factor;
    }

    // ========================
    // Recalculate stats based on Health
    // ========================
    public void RecalculateAttributesBasedOnHealth()
    {
        if (health <= 0) return;

        // This sets "speed", "reward", "immuneToFreeze/Poison", and sets the sprite
        // We store the chosen sprite in “normalSprite”

        // For example:
        if (health == 1)
        {
            speed = 2f;
            reward = 5;
            immuneToFreeze = false;
            immuneToPoison = false;
            normalSprite = redBalloonSprite;
            originalSpeed = speed;
        }
        else if (health == 2)
        {
            speed = 2f;
            reward = 5;
            immuneToFreeze = false;
            immuneToPoison = false;
            normalSprite = blueBalloonSprite;
            originalSpeed = speed;
        }
        else if (health == 3)
        {
            speed = 2f;
            reward = 5;
            immuneToFreeze = false;
            immuneToPoison = false;
            normalSprite = greenBalloonSprite;
            originalSpeed = speed;
        }
        else if (health == 4)
        {
            speed = 2f;
            reward = 5;
            immuneToFreeze = false;
            immuneToPoison = false;
            normalSprite = yellowBalloonSprite;
            originalSpeed = speed;
        }
        else if (health == 5)
        {
            speed = 4f;
            reward = 5;
            immuneToFreeze = false;
            immuneToPoison = false;
            normalSprite = pinkBalloonSprite;
            originalSpeed = speed;
        }
        else if (health == 6)
        {
            speed = 3f;
            reward = 5;
            immuneToFreeze = true;
            immuneToPoison = false;
            normalSprite = blackBalloonSprite;
            originalSpeed = speed;
        }
        else if (health == 7)
        {
            speed = 3f;
            reward = 5;
            immuneToFreeze = false;
            immuneToPoison = true;
            normalSprite = whiteBalloonSprite;
            originalSpeed = speed;
        }
        else if (health >= 8 && health <= 10)
        {
            speed = 2f;
            reward = 5;
            immuneToFreeze = false;
            immuneToPoison = false;
            normalSprite = strongBalloonSprite;
            originalSpeed = speed;
        }
        else if (health >= 11 && health <= 16)
        {
            speed = 1.5f;
            reward = 5;
            immuneToFreeze = false;
            immuneToPoison = false;
            normalSprite = strongerBalloonSprite;
            originalSpeed = speed;
        }
        else if (health >= 17 && health <= 26)
        {
            speed = 1f;
            reward = 5;
            immuneToFreeze = false;
            immuneToPoison = false;
            normalSprite = veryStrongBalloonSprite;
            originalSpeed = speed;
        }
        else if (health >= 27 && health <= 126)
        {
            speed = 1.5f;
            reward = 5;
            immuneToFreeze = true;
            immuneToPoison = true;
            normalSprite = smallBossBalloonSprite;
            originalSpeed = speed;
        }
        else if (health >= 127 && health <= 626)
        {
            speed = 1f;
            reward = 5;
            immuneToFreeze = true;
            immuneToPoison = true;
            normalSprite = mediumBossBalloonSprite;
            originalSpeed = speed;
        }
        else if (health >= 627 && health <= 3126)
        {
            speed = 0.5f;
            reward = 5;
            immuneToFreeze = true;
            immuneToPoison = true;
            normalSprite = bigBossBalloonSprite;
            originalSpeed = speed;
        }
    }

    public void ReachEnd()
    {
        OnEndReached?.Invoke(this);
        Destroy(gameObject);
    }
}
