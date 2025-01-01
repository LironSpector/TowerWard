using UnityEngine;

public class Projectile : MonoBehaviour
{
    public float speed = 5f;
    public int damage = 1;

    private Vector2 moveDir;
    private bool isInitialized = false;

    // Example: The size of the map grid. Could also place these in a config or read them from another script.
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
        // 1) Check if the projectile is still inside the map - needs to be checked before this line: "if (!isInitialized) return;" since it returns false many times.
        if (!GridManager.Instance.IsWithinGrid(transform.position, mapGridWidth, mapGridHeight))
        {
            Destroy(gameObject); // It's out of the bounding rectangle => destroy
            return;
        }

        if (!isInitialized) return;

        // 2) Move the projectile
        float distFrame = speed * Time.deltaTime;
        transform.Translate(moveDir * distFrame, Space.World);

        // 3) Rotate to face direction
        float angle = Mathf.Atan2(moveDir.y, moveDir.x) * Mathf.Rad2Deg;
        transform.rotation = Quaternion.Euler(0, 0, angle);
    }

    void OnTriggerEnter2D(Collider2D other)
    {
        Debug.Log("----- Meets balloon -----");
        if (other.CompareTag("Balloon"))
        {
            Balloon balloon = other.GetComponent<Balloon>();
            if (balloon != null)
            {
                Debug.Log("Damage is: " + damage + ", balloon health: " + balloon.health);
                balloon.TakeDamage(damage);
                Debug.Log("balloon health after: " + balloon.health);
            }
            Destroy(gameObject);
        }
    }
}







//------- Before projectile code & behaviour changes: -----------
//using UnityEngine;

//public class Projectile : MonoBehaviour
//{
//    public float speed = 5f;
//    public int damage = 1;

//    private Transform target;

//    public void Seek(Transform _target)
//    {
//        target = _target;
//    }

//    void Update()
//    {
//        if (target == null)
//        {
//            // Target is gone, destroy the projectile
//            Debug.Log("Target is gone, destroy the projectile.");
//            Destroy(gameObject);
//            return;
//        }

//        // Move towards the target
//        Vector2 direction = (Vector2)(target.position - transform.position).normalized;
//        float distanceThisFrame = speed * Time.deltaTime;

//        transform.Translate(direction * distanceThisFrame, Space.World);

//        // Optionally rotate projectile to face the target
//        float angle = Mathf.Atan2(direction.y, direction.x) * Mathf.Rad2Deg;
//        transform.rotation = Quaternion.Euler(0, 0, angle);

//        // Check if projectile reaches the target
//        if (Vector2.Distance(transform.position, target.position) <= distanceThisFrame * 10) //Multiplying by 10 to be more accurate (it works good this way).
//        {
//            HitTarget();
//        }
//    }

//    void HitTarget()
//    {
//        // Deal damage to the target
//        Balloon balloon = target.GetComponent<Balloon>();
//        if (balloon != null)
//        {
//            balloon.TakeDamage(damage);
//        }

//        Destroy(gameObject);
//    }

//    void OnTriggerEnter2D(Collider2D other)
//    {
//        Debug.Log("OnTriggerEnter2D called with: " + other);
//        if (other.CompareTag("Balloon"))
//        {
//            Debug.Log("Z");
//            Balloon balloon = other.GetComponent<Balloon>();
//            if (balloon != null && other.transform == target)
//            {
//                HitTarget();
//            }
//        }
//    }

//    void OnCollisionEnter2D(Collision2D collision)
//    {

//        Debug.Log("OnCollisionEnter2D called with: " + collision);
//        //if (collision.CompareTag("Balloon"))
//        //{
//        //    Balloon balloon = collision.GetComponent<Balloon>();
//        //    if (balloon != null && collision.transform == target)
//        //    {
//        //        HitTarget();
//        //    }
//        //}
//    }

//}