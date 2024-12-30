using UnityEngine;

public class BalloonSendingPanel : MonoBehaviour
{
    public BalloonButton[] balloonButtons; // Assign in inspector each balloon

    public void RefreshBalloonButtons()
    {
        foreach (var balloonButton in balloonButtons)
        {
            balloonButton.Refresh();
        }
    }
}
