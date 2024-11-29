using UnityEngine;
using UnityEngine.UI;
using TMPro;

public class BalloonButton : MonoBehaviour
{
    public string balloonType;
    public int cost;

    private Button button;
    private TextMeshProUGUI costText;

    void Start()
    {
        button = GetComponent<Button>();
        button.onClick.AddListener(OnButtonClicked);

        // Find the CostText child
        costText = GetComponentInChildren<TextMeshProUGUI>();
        if (costText != null)
        {
            costText.text = "$" + cost.ToString();
        }
    }

    void OnButtonClicked()
    {
        GameManager.Instance.SendBalloonToOpponent(balloonType, cost);
    }
}
