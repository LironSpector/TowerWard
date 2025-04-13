using UnityEngine;

/// <summary>
/// Description:
/// Manages the panel used for sending balloons in the game UI. This component holds references to all
/// balloon buttons (each representing a different type of balloon) and provides a method to refresh their display,
/// updating their appearance based on the player's current resources.
/// </summary>
public class BalloonSendingPanel : MonoBehaviour
{
    /// <summary>
    /// Array of BalloonButton components assigned through the Unity Inspector.
    /// Each button represents a selectable balloon with its associated properties (health, cost, etc.).
    /// </summary>
    public BalloonButton[] balloonButtons;

    /// <summary>
    /// Refreshes the display of all balloon buttons by calling each button's Refresh method.
    /// This ensures the buttons accurately reflect the latest game state, such as changes in player currency.
    /// </summary>
    public void RefreshBalloonButtons()
    {
        foreach (var balloonButton in balloonButtons)
        {
            balloonButton.Refresh();
        }
    }
}
