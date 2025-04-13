using UnityEngine;

/// <summary>
/// A simple script that fades out a sprite over time.
/// This component gradually decreases the sprite's alpha transparency at a specified speed.
/// Once the sprite is fully transparent, the GameObject is automatically destroyed.
/// </summary>
public class ElectricityFx : MonoBehaviour
{
    #region Public Fields

    /// <summary>
    /// The speed at which the sprite fades out.
    /// </summary>
    public float fadeSpeed = 2f;  // How fast to fade out

    #endregion

    #region Private Fields

    /// <summary>
    /// Current alpha (transparency) value of the sprite.
    /// </summary>
    private float alpha = 1f;     // Current alpha value

    /// <summary>
    /// Cached reference to the SpriteRenderer component attached to this GameObject.
    /// </summary>
    private SpriteRenderer sr;

    #endregion

    #region Unity Methods

    /// <summary>
    /// Awake is called when the script instance is being loaded.
    /// Caches the SpriteRenderer component.
    /// </summary>
    void Awake()
    {
        sr = GetComponent<SpriteRenderer>();
    }

    /// <summary>
    /// Update is called once per frame.
    /// Decreases the sprite's alpha value based on fadeSpeed and applies the updated transparency.
    /// Destroys the GameObject when the sprite is fully transparent.
    /// </summary>
    void Update()
    {
        // Decrease alpha based on fadeSpeed and elapsed time.
        alpha -= fadeSpeed * Time.deltaTime;
        if (alpha < 0f)
            alpha = 0f;

        // Apply the updated alpha value to the sprite's color.
        if (sr != null)
        {
            Color c = sr.color;
            c.a = alpha;
            sr.color = c;
        }

        // Destroy the GameObject once the sprite is fully transparent.
        if (alpha <= 0f)
        {
            Destroy(gameObject);
        }
    }

    #endregion
}
