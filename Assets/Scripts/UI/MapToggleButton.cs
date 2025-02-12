using UnityEngine;
using TMPro;

public class MapToggleButton : MonoBehaviour
{
    private bool isShowingMap = false; // Our local knowledge: are we currently seeing the opponent’s map?

    public TextMeshProUGUI toggleMapButtonText;

    public void OnToggleMapClicked()
    {
        // If we are in Single Player or game is over, do nothing
        if (GameManager.Instance.CurrentGameMode != GameManager.GameMode.Multiplayer
            || GameManager.Instance.isGameOver)
        {
            Debug.Log("No snapshots in single player or if game is over");
            return;
        }

        // Toggle local state
        isShowingMap = !isShowingMap;

        // Update button text
        if (isShowingMap)
        {
            toggleMapButtonText.text = "Hide Map";
            // Send "ShowSnapshots" message to the server => Opponent will start capturing
            NetworkManager.Instance.messageSender.SendAuthenticatedMessage("ShowSnapshots", null);
            //NetworkManager.Instance.SendAuthenticatedMessage("ShowSnapshots", null);

            //string msg = "{\"Type\":\"ShowSnapshots\"}";
            //NetworkManager.Instance.SendMessageWithLengthPrefix(msg);

            // Also show the snapshot UI (top-left) for us to see the incoming snapshots
            UIManager.Instance.SetOpponentSnapshotPanel(true);
            UIManager.Instance.SetOpponentSnapshotAlpha(0.8f);
        }
        else
        {
            toggleMapButtonText.text = "Show Map";
            // Send "HideSnapshots" => Opponent stops capturing
            NetworkManager.Instance.messageSender.SendAuthenticatedMessage("HideSnapshots", null);
            //NetworkManager.Instance.SendAuthenticatedMessage("HideSnapshots", null);

            //string msg = "{\"Type\":\"HideSnapshots\"}";
            //NetworkManager.Instance.SendMessageWithLengthPrefix(msg);

            // Hide snapshot UI for us
            UIManager.Instance.SetOpponentSnapshotPanel(false);
        }
    }
}
