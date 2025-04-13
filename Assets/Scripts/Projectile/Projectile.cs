using UnityEngine;

/// <summary>
/// Enumerates the possible effects that a projectile can apply to a balloon.
/// </summary>
public enum ProjectileEffectType
{
    /// <summary>
    /// No additional effect.
    /// </summary>
    None,

    /// <summary>
    /// Applies a freeze effect.
    /// </summary>
    Freeze,

    /// <summary>
    /// Applies a slow effect.
    /// </summary>
    Slow,

    /// <summary>
    /// Applies a poison effect.
    /// </summary>
    Poison
}

/// <summary>
/// Description:
/// Represents a projectile in the game that moves in a specified direction, deals damage upon collision with a balloon,
/// and can apply status effects such as Freeze, Slow, or Poison. The projectile destroys itself upon colliding with a balloon
/// or when it exits the defined grid boundaries.
/// </summary>
public class Projectile : MonoBehaviour
{
    /// <summary>
    /// Movement speed of the projectile.
    /// </summary>
    public float speed = 5f;

    /// <summary>
    /// Damage dealt by the projectile on impact.
    /// </summary>
    public int damage = 1;

    // Status effect parameters:
    /// <summary>
    /// Type of status effect to apply on impact.
    /// </summary>
    public ProjectileEffectType effectType = ProjectileEffectType.None;

    /// <summary>
    /// Duration of the status effect applied.
    /// </summary>
    public float effectDuration = 0f;

    /// <summary>
    /// Slow factor applied if the effect is Slow.
    /// </summary>
    public float slowFactor = 1f;

    /// <summary>
    /// Interval between poison ticks if the effect is Poison.
    /// </summary>
    public float poisonTickInterval = 1f;

    // Internal state:
    private Vector2 moveDir;         // Normalized movement direction.
    private bool isInitialized = false; // Indicates if the projectile has been initialized with a direction and speed.

    // Map grid dimensions for boundary checking.
    private int mapGridWidth = 26;
    private int mapGridHeight = 14;

    /// <summary>
    /// Initializes the projectile's movement direction and speed.
    /// </summary>
    /// <param name="direction">The desired direction of movement (will be normalized).</param>
    /// <param name="_speed">The speed of the projectile.</param>
    public void InitDirection(Vector2 direction, float _speed)
    {
        moveDir = direction.normalized;
        speed = _speed;
        isInitialized = true;
    }

    /// <summary>
    /// Called once per frame. Moves the projectile in the specified direction,
    /// rotates the projectile to match its movement direction, and destroys it if it exits the grid.
    /// </summary>
    void Update()
    {
        // Destroy the projectile if it is outside of grid bounds.
        if (!GridManager.Instance.IsWithinGrid(transform.position, mapGridWidth, mapGridHeight))
        {
            Destroy(gameObject);
            return;
        }

        // If not initialized, do nothing.
        if (!isInitialized)
            return;

        // Calculate the distance to move this frame.
        float distFrame = speed * Time.deltaTime;

        // Move the projectile in world space.
        transform.Translate(moveDir * distFrame, Space.World);

        // Rotate the projectile to face its direction of travel.
        float angle = Mathf.Atan2(moveDir.y, moveDir.x) * Mathf.Rad2Deg;
        transform.rotation = Quaternion.Euler(0, 0, angle);
    }

    /// <summary>
    /// Handles collision events with other colliders.
    /// If the projectile collides with a GameObject tagged as "Balloon", it applies damage
    /// and, if applicable, a status effect to the balloon, then destroys itself.
    /// </summary>
    /// <param name="other">The Collider2D that the projectile has collided with.</param>
    void OnTriggerEnter2D(Collider2D other)
    {
        if (other.CompareTag("Balloon"))
        {
            Balloon balloon = other.GetComponent<Balloon>();
            if (balloon != null)
            {
                // Apply direct damage to the balloon.
                balloon.TakeDamage(damage);

                // Apply the appropriate status effect based on the projectile's effectType.
                switch (effectType)
                {
                    case ProjectileEffectType.Freeze:
                        balloon.Freeze(effectDuration);
                        break;
                    case ProjectileEffectType.Slow:
                        balloon.ApplySlow(effectDuration, slowFactor);
                        break;
                    case ProjectileEffectType.Poison:
                        balloon.Poison(effectDuration);
                        break;
                    default:
                        break;
                }
            }
            // Destroy the projectile after impacting a balloon.
            Destroy(gameObject);
        }
    }
}
