using UnityEngine;

public enum ProjectileEffectType
{
    None,
    Freeze,
    Slow,
    Poison
}

public class Projectile : MonoBehaviour
{
    public float speed = 5f;
    public int damage = 1;

    // For status effects
    public ProjectileEffectType effectType = ProjectileEffectType.None;
    public float effectDuration = 0f;
    public float slowFactor = 1f;        // For slow only
    public float poisonTickInterval = 1f; // For poison

    private Vector2 moveDir;
    private bool isInitialized = false;

    private int mapGridWidth = 26;
    private int mapGridHeight = 14;

    public void InitDirection(Vector2 direction, float _speed)
    {
        moveDir = direction.normalized;
        speed = _speed;
        isInitialized = true;
    }

    void Update()
    {
        if (!GridManager.Instance.IsWithinGrid(transform.position, mapGridWidth, mapGridHeight))
        {
            Destroy(gameObject);
            return;
        }

        if (!isInitialized) return;

        float distFrame = speed * Time.deltaTime;
        transform.Translate(moveDir * distFrame, Space.World);

        float angle = Mathf.Atan2(moveDir.y, moveDir.x) * Mathf.Rad2Deg;
        transform.rotation = Quaternion.Euler(0, 0, angle);
    }

    void OnTriggerEnter2D(Collider2D other)
    {
        if (other.CompareTag("Balloon"))
        {
            Balloon balloon = other.GetComponent<Balloon>();
            if (balloon != null)
            {
                // Deal direct damage
                balloon.TakeDamage(damage);

                // Apply effect if any
                switch (effectType)
                {
                    case ProjectileEffectType.Freeze:
                        Debug.Log("Freeze Time");
                        balloon.Freeze(effectDuration);
                        break;
                    case ProjectileEffectType.Slow:
                        balloon.ApplySlow(effectDuration, slowFactor);
                        break;
                    case ProjectileEffectType.Poison:
                        // Possibly set poisonTickInterval in the balloon
                        balloon.Poison(effectDuration);
                        break;
                    default:
                        Debug.Log("Regular Time");
                        break;
                }
            }
            Destroy(gameObject);
        }
    }
}
