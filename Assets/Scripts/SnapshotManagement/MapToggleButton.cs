using UnityEngine;
using TMPro;

/// <summary>
/// Description:
/// Provides functionality for toggling the opponent's map (snapshot view) in multiplayer mode.
/// When the button is clicked, it switches between showing and hiding the snapshot UI.
/// It also sends messages to the server to start or stop capturing snapshots on the opponent's side
/// and adjusts the snapshot panel's opacity accordingly.
/// </summary>
public class MapToggleButton : MonoBehaviour
{
    /// <summary>
    /// Indicates whether the opponent's map is currently shown.
    /// </summary>
    private bool isShowingMap = false;

    /// <summary>
    /// The TextMeshProUGUI component on the toggle button used to display the current state.
    /// </summary>
    public TextMeshProUGUI toggleMapButtonText;

    /// <summary>
    /// Called when the map toggle button is clicked.
    /// Checks the current game mode and game over status before toggling the snapshot display.
    /// Sends the appropriate network messages and updates the snapshot UI accordingly.
    /// </summary>
    public void OnToggleMapClicked()
    {
        // Only allow toggling snapshots in multiplayer mode and if the game is still in progress.
        if (GameManager.Instance.CurrentGameMode != GameManager.GameMode.Multiplayer || GameManager.Instance.isGameOver)
        {
            Debug.Log("No snapshots in single player or if game is over");
            return;
        }

        // Toggle the state of showing the map.
        isShowingMap = !isShowingMap;

        if (isShowingMap)
        {
            toggleMapButtonText.text = "Hide Map";
            // Send "ShowSnapshots" message to the server => Opponent will start capturing
            NetworkManager.Instance.messageSender.SendAuthenticatedMessage("ShowSnapshots", null);
            // Show the snapshot panel in the UI and set its opacity to 80%.
            UIManager.Instance.SetOpponentSnapshotPanel(true);
            UIManager.Instance.SetOpponentSnapshotAlpha(0.8f);
        }
        else
        {
            toggleMapButtonText.text = "Show Map";
            // Send "HideSnapshots" => Opponent stops capturing (there is no need for snapshot capturing if they are not shown)
            NetworkManager.Instance.messageSender.SendAuthenticatedMessage("HideSnapshots", null);
            // Hide the snapshot panel from the UI.
            UIManager.Instance.SetOpponentSnapshotPanel(false);
        }
    }
}
