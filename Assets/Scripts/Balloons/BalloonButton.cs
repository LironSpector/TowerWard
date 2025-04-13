using UnityEngine;
using UnityEngine.UI;
using TMPro;

/// <summary>
/// Description:
/// Manages the behavior and display of a balloon selection button in the UI.
/// This script displays the cost of a balloon (with an optional discount factor), and handles
/// the button click to send a balloon (with a given health and cost) to the opponent.
/// </summary>
public class BalloonButton : MonoBehaviour
{
    #region Public Fields

    [Header("Balloon Data")]
    /// <summary>
    /// The health value of the balloon (e.g. 1 for red, 2 for blue, etc.).
    /// </summary>
    public int balloonHealth = 1;
    /// <summary>
    /// The base cost of the balloon.
    /// </summary>
    public int cost;
    /// <summary>
    /// Temporary discount factor applied to the cost.
    /// </summary>
    public float tempDiscountFactor = 1;

    [Header("UI References")]
    /// <summary>
    /// The Image component assigned via inspector representing the balloon icon.
    /// </summary>
    public Image image;

    #endregion

    #region Private Fields

    // Cached references.
    private Button button;
    private TextMeshProUGUI costText;
    private Image imageComponent;

    #endregion

    #region Unity Methods

    /// <summary>
    /// Start is called before the first frame update.
    /// Initializes the button, its image component, and displays the balloon cost.
    /// </summary>
    void Start()
    {
        // Get the Button component and register the click event.
        button = GetComponent<Button>();
        button.onClick.AddListener(OnButtonClicked);

        // If the image reference is not assigned in the Inspector, try to find it
        if (image == null)
        {
            image = transform.Find("Image").GetComponent<Image>();
        }

        // Cache the Image component for color manipulation.
        imageComponent = image.GetComponent<Image>();

        // Display the price on the button.
        DisplayBalloonPrice();
    }

    #endregion

    #region Public Methods

    /// <summary>
    /// Refreshes the button's appearance by updating its interactivity and visual colors based on the player's currency.
    /// Also updates the displayed balloon price.
    /// </summary>
    public void Refresh()
    {
        // Determine if the player can afford the balloon with the applied discount.
        bool canAfford = (GameManager.Instance.currency >= cost * tempDiscountFactor);

        if (canAfford)
        {
            // Enable button interactivity and set normal colors.
            button.interactable = true;
            if (imageComponent != null)
                imageComponent.color = Color.white;
            if (costText != null)
                costText.color = Color.white;
        }
        else
        {
            // Optionally disable interactivity or change colors to indicate insufficient funds.
            // (Disabling button.interactable may cause issues; instead, change colors.)
            if (imageComponent != null)
                imageComponent.color = new Color(0.4f, 0.4f, 0.4f); // Gray
            if (costText != null)
                costText.color = new Color(0.2f, 0.2f, 0.2f); // Darker gray text
        }

        // Update the balloon price display (in case discount has changed).
        DisplayBalloonPrice();
    }

    #endregion

    #region Private Methods

    /// <summary>
    /// Displays the price of the balloon on the button by updating the costText field.
    /// Formats the cost into thousands if the cost is greater than 999.
    /// </summary>
    private void DisplayBalloonPrice()
    {
        costText = GetComponentInChildren<TextMeshProUGUI>();
        if (costText != null)
        {
            if (cost > 999)
            {
                // Format cost in thousands with a "K" suffix.
                double costToDisplay = cost * tempDiscountFactor / 1000.0;
                costText.text = "$" + costToDisplay.ToString() + "K";
            }
            else
            {
                costText.text = "$" + (cost * tempDiscountFactor).ToString();
            }
        }
    }

    /// <summary>
    /// Callback for the button click event.
    /// Sends a balloon to the opponent by calling the GameManager with the specified balloon health and adjusted cost.
    /// </summary>
    private void OnButtonClicked()
    {
        GameManager.Instance.SendBalloonToOpponent(balloonHealth, (int)(cost * tempDiscountFactor));
    }

    #endregion
}
