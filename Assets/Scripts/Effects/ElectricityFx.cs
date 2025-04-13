using UnityEngine;

/// <summary>
/// A simple script that fades out a sprite over time.
/// </summary>
public class ElectricityFx : MonoBehaviour
{
    public float fadeSpeed = 2f;  // How fast to fade out
    private float alpha = 1f;     // Current alpha value

    private SpriteRenderer sr;

    void Awake()
    {
        sr = GetComponent<SpriteRenderer>();
    }

    void Update()
    {
        // Decrease alpha over time
        alpha -= fadeSpeed * Time.deltaTime;
        if (alpha < 0f) alpha = 0f;

        // Apply alpha to the sprite
        if (sr != null)
        {
            Color c = sr.color;
            c.a = alpha;
            sr.color = c;
        }

        // If fully transparent, destroy
        if (alpha <= 0f)
        {
            Destroy(gameObject);
        }
    }
}
