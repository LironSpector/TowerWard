//------- After balloon code & behaviour changes: -----------
using UnityEngine;
using UnityEngine.UI;
using TMPro;

public class BalloonButton : MonoBehaviour
{
    [Header("Balloon Data")]
    public int balloonHealth = 1;  // e.g. 1 for red, 2 for blue, etc.
    public int cost;
    public float tempDiscountFactor = 1;

    [Header("UI References")]
    public Image image;  // Assign in inspector, the child "Image"
    private Button button;
    private TextMeshProUGUI costText;
    private Image imageComponent;

    void Start()
    {
        button = GetComponent<Button>();
        button.onClick.AddListener(OnButtonClicked);

        if (!image) // fallback if not assigned
            image = transform.Find("Image").GetComponent<Image>();

        imageComponent = image.GetComponent<Image>();

        DisplayBalloonPrice();
    }

    private void DisplayBalloonPrice()
    {
        costText = GetComponentInChildren<TextMeshProUGUI>();
        if (costText != null)
        {
            if (cost > 999) //There is no space for more than $ + 3 numbers
            {
                double costToDisplay = cost * tempDiscountFactor / 1000.0;
                costText.text = "$" + costToDisplay.ToString() + "K";
            }
            else
            {
                costText.text = "$" + (cost * tempDiscountFactor).ToString();
            }
        }
    }

    void OnButtonClicked()
    {
        // Instead of balloonType, we pass balloonHealth to GameManager
        GameManager.Instance.SendBalloonToOpponent(balloonHealth, (int)(cost * tempDiscountFactor));
    }

    // Called by BalloonSendingPanel manager or something
    public void Refresh()
    {
        bool canAfford = (GameManager.Instance.currency >= cost * tempDiscountFactor);

        if (canAfford)
        {
            button.interactable = true;
            if (imageComponent != null)
                imageComponent.color = Color.white; // normal icon color
            if (costText != null)
                costText.color = Color.white; // dark text
        }
        else
        {
            //button.interactable = false; //Causes problems (didn't work: I changed in UnityEditor the "Disabled Color" to fully transparent!!!)
            if (imageComponent != null)
                imageComponent.color = new Color(0.4f, 0.4f, 0.4f); // gray
            if (costText != null)
                costText.color = new Color(0.2f, 0.2f, 0.2f); // darker gray text
        }

        DisplayBalloonPrice();
    }
}
