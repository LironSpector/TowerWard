using UnityEngine;

/// <summary>
/// Description:
/// Implements a simple green flash effect by fading out a sprite over time.
/// The script decreases the sprite's alpha value at a specified speed, and once fully transparent, 
/// destroys the GameObject. This effect can be applied to a SpriteRenderer or a UI Image.
/// </summary>
public class GreenFlashFx : MonoBehaviour
{
    private SpriteRenderer sr; // Cached reference to the SpriteRenderer component.

    /// <summary>
    /// The speed at which the sprite fades out.
    /// </summary>
    public float fadeSpeed = 2f;

    // Current alpha value of the sprite (1 = fully opaque, 0 = fully transparent).
    private float alpha = 1f;

    /// <summary>
    /// Awake is called when the script instance is being loaded.
    /// Retrieves and caches the SpriteRenderer component attached to the GameObject.
    /// </summary>
    void Awake()
    {
        sr = GetComponent<SpriteRenderer>();
        // Alternatively, this could be a UI Image in a Canvas if needed.
    }

    /// <summary>
    /// Update is called once per frame.
    /// Gradually decreases the sprite's alpha value, updates the sprite color,
    /// and destroys the GameObject when the sprite is fully faded out.
    /// </summary>
    void Update()
    {
        // Decrease the alpha value based on the fade speed and elapsed time.
        alpha -= fadeSpeed * Time.deltaTime;
        if (alpha < 0f)
            alpha = 0f;

        // Apply the updated alpha to the sprite's color.
        if (sr != null)
        {
            Color c = sr.color;
            c.a = alpha;
            sr.color = c;
        }

        // Destroy the GameObject when it becomes fully transparent.
        if (alpha <= 0f)
        {
            Destroy(gameObject);
        }
    }
}
