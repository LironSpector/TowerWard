//using UnityEngine;
//using UnityEngine.UI;
//using TMPro;

//public class BalloonButton : MonoBehaviour
//{
//    public string balloonType;
//    public int cost;

//    private Button button;
//    private TextMeshProUGUI costText;

//    void Start()
//    {
//        button = GetComponent<Button>();
//        button.onClick.AddListener(OnButtonClicked);

//        // Find the CostText child
//        costText = GetComponentInChildren<TextMeshProUGUI>();
//        if (costText != null)
//        {
//            costText.text = "$" + cost.ToString();
//        }
//    }

//    void OnButtonClicked()
//    {
//        GameManager.Instance.SendBalloonToOpponent(balloonType, cost);
//    }
//}



using UnityEngine;
using UnityEngine.UI;
using TMPro;

public class BalloonButton : MonoBehaviour
{
    public string balloonType;
    public int cost;

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

        costText = GetComponentInChildren<TextMeshProUGUI>();
        if (costText != null)
        {
            if (cost > 999) //There is no space for more than $ + 3 numbers
            {
                double costToDisplay = cost / 1000.0;
                costText.text = "$" + costToDisplay.ToString() + "K";
            }
            else
            {
                costText.text = "$" + cost.ToString();
            }
        }

        imageComponent = image.GetComponent<Image>();
    }

    void OnButtonClicked()
    {
        GameManager.Instance.SendBalloonToOpponent(balloonType, cost);
    }

    // Called by BalloonSendingPanel manager or something
    public void Refresh()
    {
        bool canAfford = (GameManager.Instance.currency >= cost);

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
    }
}
