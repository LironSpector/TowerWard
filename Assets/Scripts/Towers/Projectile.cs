using UnityEngine;

public class Projectile : MonoBehaviour
{
    public float speed = 5f;
    public int damage = 1;

    private Transform target;

    public void Seek(Transform _target)
    {
        target = _target;
    }

    void Update()
    {
        if (target == null)
        {
            // Target is gone, destroy the projectile
            Destroy(gameObject);
            return;
        }

        // Move towards the target
        Vector2 direction = (Vector2)(target.position - transform.position).normalized;
        float distanceThisFrame = speed * Time.deltaTime;

        transform.Translate(direction * distanceThisFrame, Space.World);

        // Optionally rotate projectile to face the target
        float angle = Mathf.Atan2(direction.y, direction.x) * Mathf.Rad2Deg;
        transform.rotation = Quaternion.Euler(0, 0, angle);

        // Check if projectile reaches the target
        if (Vector2.Distance(transform.position, target.position) <= distanceThisFrame * 10) //Multiplying by 10 to be more accurate (it works good this way).
        {
            HitTarget();
        }
    }

    void HitTarget()
    {
        // Deal damage to the target
        Balloon balloon = target.GetComponent<Balloon>();
        if (balloon != null)
        {
            balloon.TakeDamage(damage);
        }

        Destroy(gameObject);
    }

    void OnTriggerEnter2D(Collider2D other)
    {
        Debug.Log("OnTriggerEnter2D called with: " + other);
        if (other.CompareTag("Balloon"))
        {
            Debug.Log("Z");
            Balloon balloon = other.GetComponent<Balloon>();
            if (balloon != null && other.transform == target)
            {
                HitTarget();
            }
        }
    }

}
