using UnityEngine;

/// <summary>
/// Description:
/// Handles a floating money effect by moving the GameObject and fading out its sprite over time.
/// The sprite will move either upward or downward based on the 'moveUp' flag, and gradually become transparent.
/// Once the sprite is fully faded, the GameObject is automatically destroyed.
/// </summary>
public class MoneyFloatFx : MonoBehaviour
{
    /// <summary>
    /// The speed at which the GameObject moves (float effect).
    /// </summary>
    public float floatSpeed = 1f;

    /// <summary>
    /// The speed at which the sprite fades out.
    /// </summary>
    public float fadeSpeed = 0.5f;

    /// <summary>
    /// Determines if the GameObject should move upward (true) or downward (false).
    /// </summary>
    public bool moveUp;

    /// <summary>
    /// Cached reference to the SpriteRenderer component.
    /// </summary>
    private SpriteRenderer sr;

    /// <summary>
    /// The current alpha value of the sprite (1 = fully opaque, 0 = fully transparent).
    /// </summary>
    private float alpha = 1f;

    /// <summary>
    /// Awake is called when the script instance is being loaded.
    /// Retrieves and caches the SpriteRenderer component, and logs the initialization.
    /// </summary>
    void Awake()
    {
        sr = GetComponent<SpriteRenderer>();
        Debug.Log("Started!");
    }

    /// <summary>
    /// Update is called once per frame.
    /// Moves the GameObject in the specified direction, decreases the sprite's alpha value over time,
    /// applies the updated alpha to the sprite, and destroys the GameObject when fully faded.
    /// </summary>
    void Update()
    {
        // Move the GameObject upward or downward based on the 'moveUp' flag.
        if (moveUp)
            transform.position += Vector3.up * floatSpeed * Time.deltaTime;
        else
            transform.position -= Vector3.up * floatSpeed * Time.deltaTime;

        // Gradually fade out the sprite by decreasing the alpha value.
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

        // Destroy the GameObject when the sprite is fully transparent.
        if (alpha <= 0f)
        {
            Debug.Log("Started!Finished!");
            Destroy(gameObject);
        }
    }
}
