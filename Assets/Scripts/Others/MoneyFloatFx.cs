using UnityEngine;

public class MoneyFloatFx : MonoBehaviour
{
    public float floatSpeed = 1f;
    public float fadeSpeed = 0.5f;
    public bool moveUp;
    private SpriteRenderer sr;
    private float alpha = 1f;

    void Awake()
    {
        sr = GetComponent<SpriteRenderer>();
        Debug.Log("Started!");
    }

    void Update()
    {
        // Move upward
        if (moveUp)
            transform.position += Vector3.up * floatSpeed * Time.deltaTime;
        else
            transform.position -= Vector3.up * floatSpeed * Time.deltaTime;

        // Fade out
        alpha -= fadeSpeed * Time.deltaTime;
        if (alpha < 0f) alpha = 0f;

        // Apply alpha
        if (sr != null)
        {
            Color c = sr.color;
            c.a = alpha;
            sr.color = c;
        }

        // Destroy when fully faded
        if (alpha <= 0f)
        {
            Debug.Log("Started!Fnished!");
            Destroy(gameObject);
        }
    }
}
