using UnityEngine;

public class GreenFlashFx : MonoBehaviour
{
    private SpriteRenderer sr;
    public float fadeSpeed = 2f;
    private float alpha = 1f;

    void Awake()
    {
        sr = GetComponent<SpriteRenderer>();
        // Alternatively, this could be a UI Image in Canvas
    }

    void Update()
    {
        alpha -= fadeSpeed * Time.deltaTime;
        if (alpha < 0f) alpha = 0f;

        if (sr != null)
        {
            Color c = sr.color;
            c.a = alpha;
            sr.color = c;
        }

        if (alpha <= 0f)
        {
            Destroy(gameObject);
        }
    }
}
