using UnityEngine;
using System;

/// <summary>
/// Description:
/// Manages the behavior of a balloon in the game including its movement, health, and status effects such as freeze, poison, and slow.
/// Also handles visual updates based on its state and triggers rewards and events upon damage or when reaching the end.
/// </summary>
[RequireComponent(typeof(SpriteRenderer))]
public class Balloon : MonoBehaviour
{
    #region Public Fields

    [Header("Current Balloon State")]
    /// <summary>
    /// The current health of the balloon.
    /// </summary>
    public int health = 1;
    /// <summary>
    /// The movement speed of the balloon. Used by BalloonMovement.
    /// </summary>
    public float speed = 2f;
    /// <summary>
    /// The reward awarded to the player when the balloon is popped.
    /// </summary>
    public int reward = 5;

    [Header("Immunities")]
    /// <summary>
    /// If true, the balloon is immune to freezing effects.
    /// </summary>
    public bool immuneToFreeze = false;
    /// <summary>
    /// If true, the balloon is immune to poisoning effects.
    /// </summary>
    public bool immuneToPoison = false;

    [Header("Status Effects")]
    /// <summary>
    /// Indicates whether the balloon is currently frozen.
    /// </summary>
    public bool isFrozen = false;
    /// <summary>
    /// Indicates whether the balloon is currently poisoned.
    /// </summary>
    public bool isPoisoned = false;
    /// <summary>
    /// Indicates whether the balloon is currently slowed.
    /// </summary>
    public bool isSlowed = false;

    [Header("Balloon Sprites")]
    /// <summary>
    /// Sprite for a red balloon.
    /// </summary>
    public Sprite redBalloonSprite;
    /// <summary>
    /// Sprite for a blue balloon.
    /// </summary>
    public Sprite blueBalloonSprite;
    /// <summary>
    /// Sprite for a green balloon.
    /// </summary>
    public Sprite greenBalloonSprite;
    /// <summary>
    /// Sprite for a yellow balloon.
    /// </summary>
    public Sprite yellowBalloonSprite;
    /// <summary>
    /// Sprite for a pink balloon.
    /// </summary>
    public Sprite pinkBalloonSprite;
    /// <summary>
    /// Sprite for a black balloon.
    /// </summary>
    public Sprite blackBalloonSprite;
    /// <summary>
    /// Sprite for a white balloon.
    /// </summary>
    public Sprite whiteBalloonSprite;
    /// <summary>
    /// Sprite for a basic strong balloon.
    /// </summary>
    public Sprite strongBalloonSprite;
    /// <summary>
    /// Sprite for a stronger balloon.
    /// </summary>
    public Sprite strongerBalloonSprite;
    /// <summary>
    /// Sprite for a very strong balloon.
    /// </summary>
    public Sprite veryStrongBalloonSprite;
    /// <summary>
    /// Sprite for a small boss balloon.
    /// </summary>
    public Sprite smallBossBalloonSprite;
    /// <summary>
    /// Sprite for a medium boss balloon.
    /// </summary>
    public Sprite mediumBossBalloonSprite;
    /// <summary>
    /// Sprite for a big boss balloon.
    /// </summary>
    public Sprite bigBossBalloonSprite;

    // Newly added sprites for visual effects.
    /// <summary>
    /// Sprite used when the balloon is frozen.
    /// </summary>
    public Sprite frozenBalloonSprite;
    /// <summary>
    /// Sprite used when the balloon is poisoned.
    /// </summary>
    public Sprite poisonedBalloonSprite;

    [Header("Wave Logic")]
    /// <summary>
    /// Indicates whether the balloon is part of a wave.
    /// </summary>
    public bool isWaveBalloon = false;
    /// <summary>
    /// The wave number this balloon belongs to. Default is -1 when not in a wave.
    /// </summary>
    public int waveNumber = -1;

    #endregion

    #region Events

    /// <summary>
    /// Event triggered when the balloon is destroyed (popped).
    /// </summary>
    public event Action<Balloon> OnDestroyed;
    /// <summary>
    /// Event triggered when the balloon reaches the end.
    /// </summary>
    public event Action<Balloon> OnEndReached;

    #endregion

    #region Private Fields

    // Durations and timers for status effects.
    private float freezeDuration = 0f;
    private float poisonDuration = 0f;
    private float poisonTickInterval = 1f;
    private float poisonTickTimer = 0f;
    private float slowDuration = 0f;
    private float slowFactor = 0.5f;
    private float originalSpeed = 2f;  // Used to store the base speed for slow calculations.

    // References to required components.
    private BalloonMovement movement;
    private SpriteRenderer spriteRenderer;

    // The balloon's normal (non-affected) sprite.
    private Sprite normalSprite;

    #endregion

    #region Unity Methods

    /// <summary>
    /// Awake is called when the script instance is being loaded.
    /// Retrieves required components.
    /// </summary>
    void Awake()
    {
        movement = GetComponent<BalloonMovement>();
        spriteRenderer = GetComponent<SpriteRenderer>();
    }

    /// <summary>
    /// Start is called before the first frame update.
    /// Recalculates the balloon's attributes based on its health.
    /// </summary>
    void Start()
    {
        RecalculateAttributesBasedOnHealth();
    }

    /// <summary>
    /// Update is called once per frame.
    /// Processes status effects, updates movement speed, and refreshes the visual sprite based on current status.
    /// </summary>
    void Update()
    {
        UpdateStatusEffects();
        UpdateMovementSpeed();
        UpdateVisualSprite();
    }

    #endregion

    #region Status Effects

    /// <summary>
    /// Updates all active status effects (freeze, poison, slow) on the balloon.
    /// </summary>
    private void UpdateStatusEffects()
    {
        HandleFreeze();
        HandlePoison();
        HandleSlow();
    }

    /// <summary>
    /// Processes the freeze effect. Decreases the freeze duration and unfreezes the balloon if the duration expires.
    /// </summary>
    private void HandleFreeze()
    {
        if (isFrozen)
        {
            freezeDuration -= Time.deltaTime;
            if (freezeDuration <= 0f)
            {
                isFrozen = false;
            }
        }
    }

    /// <summary>
    /// Processes the poison effect. Decreases poison duration and applies damage on each tick.
    /// Unpoisons the balloon when the duration expires.
    /// </summary>
    private void HandlePoison()
    {
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
    }

    /// <summary>
    /// Processes the slow effect by decreasing its duration and resetting the status when the duration expires.
    /// </summary>
    private void HandleSlow()
    {
        if (isSlowed)
        {
            slowDuration -= Time.deltaTime;
            if (slowDuration <= 0f)
            {
                isSlowed = false;
            }
        }
    }

    #endregion

    #region Movement and Visual Updates

    /// <summary>
    /// Updates the balloon's movement speed based on its status effects such as freeze and slow.
    /// </summary>
    private void UpdateMovementSpeed()
    {
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
    }

    /// <summary>
    /// Updates the visual sprite of the balloon based on its current status.
    /// Priority: Frozen > Poisoned > Normal.
    /// </summary>
    private void UpdateVisualSprite()
    {
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
            spriteRenderer.sprite = normalSprite;
        }
    }

    #endregion

    #region Damage and Status Effect Application

    /// <summary>
    /// Applies damage to the balloon. If the health remains above zero, recalculates attributes and rewards currency if a layer is popped.
    /// If health drops to zero or below, pops the balloon.
    /// </summary>
    /// <param name="damage">The amount of damage to apply.</param>
    public void TakeDamage(int damage)
    {
        int oldHealth = health;
        health -= damage;

        if (health > 0)
        {
            // Capture the previous sprite and reward for comparison.
            Sprite previousSprite = spriteRenderer.sprite;
            int prevReward = reward;

            RecalculateAttributesBasedOnHealth();

            // If the sprite has changed, the balloon has "popped" a layer; reward the player.
            if (previousSprite != spriteRenderer.sprite)
            {
                GameManager.Instance.AddCurrency(prevReward);
            }
        }
        else
        {
            Pop();
        }
    }

    /// <summary>
    /// Pops the balloon by awarding currency, playing a pop sound, triggering the OnDestroyed event, and destroying the GameObject.
    /// </summary>
    public void Pop()
    {
        GameManager.Instance.AddCurrency(reward);
        AudioManager.Instance.PlayBalloonPop();

        OnDestroyed?.Invoke(this);
        Destroy(gameObject);
    }

    /// <summary>
    /// Applies a freeze effect to the balloon for the specified duration, if it is not immune.
    /// </summary>
    /// <param name="duration">The duration of the freeze effect in seconds.</param>
    public void Freeze(float duration)
    {
        if (immuneToFreeze) return;
        isFrozen = true;
        freezeDuration = duration;
    }

    /// <summary>
    /// Applies a poison effect to the balloon for the specified duration, if it is not immune.
    /// </summary>
    /// <param name="duration">The duration of the poison effect in seconds.</param>
    public void Poison(float duration)
    {
        if (immuneToPoison) return;
        isPoisoned = true;
        poisonDuration = duration;
        poisonTickTimer = poisonTickInterval;
    }

    /// <summary>
    /// Applies a slow effect to the balloon for the specified duration with the provided slow factor.
    /// </summary>
    /// <param name="duration">The duration of the slow effect in seconds.</param>
    /// <param name="factor">The factor by which the balloon's speed is reduced.</param>
    public void ApplySlow(float duration, float factor)
    {
        isSlowed = true;
        slowDuration = duration;
        slowFactor = factor;
    }

    #endregion

    #region Attribute Recalculation and End-of-Path

    /// <summary>
    /// Recalculates the balloon's attributes based on its current health by retrieving stats from BalloonUtils.
    /// Updates movement speed, reward, immunities, and the normal sprite.
    /// </summary>
    public void RecalculateAttributesBasedOnHealth()
    {
        if (health <= 0) return;

        BalloonStats stats = BalloonUtils.GetStatsForHealth(this, health);

        speed = stats.Speed;
        reward = stats.Reward;
        immuneToFreeze = stats.ImmuneToFreeze;
        immuneToPoison = stats.ImmuneToPoison;
        normalSprite = stats.NormalSprite;
        originalSpeed = stats.Speed;
    }

    /// <summary>
    /// Called when the balloon reaches the end of its path.
    /// Triggers the OnEndReached event and destroys the balloon.
    /// </summary>
    public void ReachEnd()
    {
        OnEndReached?.Invoke(this);
        Destroy(gameObject);
    }

    #endregion
}













////New - after making structure better
//using UnityEngine;
//using System;

//[RequireComponent(typeof(SpriteRenderer))]
//public class Balloon : MonoBehaviour
//{
//    [Header("Current Balloon State")]
//    public int health = 1;
//    public float speed = 2f; // Used by BalloonMovement
//    public int reward = 5;

//    [Header("Immunities")]
//    public bool immuneToFreeze = false;
//    public bool immuneToPoison = false;

//    [Header("Status Effects")]
//    public bool isFrozen = false;
//    public bool isPoisoned = false;
//    public bool isSlowed = false;

//    private float freezeDuration = 0f;
//    private float poisonDuration = 0f;
//    private float poisonTickInterval = 1f;
//    private float poisonTickTimer = 0f;
//    private float slowDuration = 0f;
//    private float slowFactor = 0.5f;
//    private float originalSpeed = 2f;  // Used to store base speed for slow calculations

//    private BalloonMovement movement;
//    private SpriteRenderer spriteRenderer;

//    [Header("Balloon Sprites")]
//    public Sprite redBalloonSprite;
//    public Sprite blueBalloonSprite;
//    public Sprite greenBalloonSprite;
//    public Sprite yellowBalloonSprite;
//    public Sprite pinkBalloonSprite;
//    public Sprite blackBalloonSprite;
//    public Sprite whiteBalloonSprite;
//    public Sprite strongBalloonSprite;
//    public Sprite strongerBalloonSprite;
//    public Sprite veryStrongBalloonSprite;
//    public Sprite smallBossBalloonSprite;
//    public Sprite mediumBossBalloonSprite;
//    public Sprite bigBossBalloonSprite;

//    // Newly added for visuals:
//    public Sprite frozenBalloonSprite;
//    public Sprite poisonedBalloonSprite;

//    // Wave logic
//    public bool isWaveBalloon = false;
//    public int waveNumber = -1;

//    // Events
//    public event Action<Balloon> OnDestroyed;
//    public event Action<Balloon> OnEndReached;

//    // Keep track of the “normal” (non-frozen, non-poisoned) sprite
//    private Sprite normalSprite;

//    void Awake()
//    {
//        movement = GetComponent<BalloonMovement>();
//        spriteRenderer = GetComponent<SpriteRenderer>();
//    }

//    void Start()
//    {
//        RecalculateAttributesBasedOnHealth();
//    }

//    void Update()
//    {
//        UpdateStatusEffects();
//        UpdateMovementSpeed();
//        UpdateVisualSprite();
//    }

//    // ============================
//    //  Break down the status updates
//    // ============================
//    private void UpdateStatusEffects()
//    {
//        HandleFreeze();
//        HandlePoison();
//        HandleSlow();
//    }

//    private void HandleFreeze()
//    {
//        if (isFrozen)
//        {
//            freezeDuration -= Time.deltaTime;
//            if (freezeDuration <= 0f)
//            {
//                isFrozen = false;
//            }
//        }
//    }

//    private void HandlePoison()
//    {
//        if (isPoisoned)
//        {
//            poisonDuration -= Time.deltaTime;
//            poisonTickTimer -= Time.deltaTime;

//            if (poisonTickTimer <= 0f)
//            {
//                poisonTickTimer = poisonTickInterval;
//                TakeDamage(1);
//            }
//            if (poisonDuration <= 0f)
//            {
//                isPoisoned = false;
//            }
//        }
//    }

//    private void HandleSlow()
//    {
//        if (isSlowed)
//        {
//            slowDuration -= Time.deltaTime;
//            if (slowDuration <= 0f)
//            {
//                isSlowed = false;
//            }
//        }
//    }

//    private void UpdateMovementSpeed()
//    {
//        if (isFrozen)
//        {
//            speed = 0f;
//        }
//        else if (isSlowed)
//        {
//            speed = originalSpeed * slowFactor * GameManager.Instance.allBalloonsSpeedFactor;
//        }
//        else
//        {
//            speed = originalSpeed * GameManager.Instance.allBalloonsSpeedFactor;
//        }
//    }

//    private void UpdateVisualSprite()
//    {
//        // Priority: Frozen -> Poisoned -> Normal
//        if (isFrozen && frozenBalloonSprite != null)
//        {
//            spriteRenderer.sprite = frozenBalloonSprite;
//        }
//        else if (isPoisoned && poisonedBalloonSprite != null)
//        {
//            spriteRenderer.sprite = poisonedBalloonSprite;
//        }
//        else
//        {
//            spriteRenderer.sprite = normalSprite;
//        }
//    }

//    // ========================
//    //  Taking damage and popping
//    // ========================
//    public void TakeDamage(int damage)
//    {
//        int oldHealth = health;
//        health -= damage;

//        if (health > 0)
//        {
//            // If we cross a threshold that changes the base sprite, we give reward
//            Sprite previousSprite = spriteRenderer.sprite;
//            int prevReward = reward;

//            RecalculateAttributesBasedOnHealth();

//            // If the sprite changed, it's as if we "popped" a layer.
//            if (previousSprite != spriteRenderer.sprite)
//            {
//                GameManager.Instance.AddCurrency(prevReward);
//            }
//        }
//        else
//        {
//            Pop();
//        }
//    }

//    public void Pop()
//    {
//        GameManager.Instance.AddCurrency(reward);
//        AudioManager.Instance.PlayBalloonPop();

//        OnDestroyed?.Invoke(this);
//        Destroy(gameObject);
//    }

//    public void Freeze(float duration)
//    {
//        if (immuneToFreeze) return;
//        isFrozen = true;
//        freezeDuration = duration;
//    }

//    public void Poison(float duration)
//    {
//        if (immuneToPoison) return;
//        isPoisoned = true;
//        poisonDuration = duration;
//        poisonTickTimer = poisonTickInterval;
//    }

//    public void ApplySlow(float duration, float factor)
//    {
//        isSlowed = true;
//        slowDuration = duration;
//        slowFactor = factor;
//    }

//    // ========================
//    // Recalculate stats based on Health
//    // ========================
//    public void RecalculateAttributesBasedOnHealth()
//    {
//        if (health <= 0) return;

//        // Retrieve balloon stats from our utility
//        BalloonStats stats = BalloonUtils.GetStatsForHealth(this, health);

//        speed = stats.Speed;
//        reward = stats.Reward;
//        immuneToFreeze = stats.ImmuneToFreeze;
//        immuneToPoison = stats.ImmuneToPoison;
//        normalSprite = stats.NormalSprite;
//        originalSpeed = stats.Speed;
//    }

//    public void ReachEnd()
//    {
//        OnEndReached?.Invoke(this);
//        Destroy(gameObject);
//    }
//}
