using UnityEngine;
using UnityEngine.SceneManagement;
using TMPro;

public class MainMenuManager : MonoBehaviour
{
    public TextMeshProUGUI connectionStatusText;

    void Start()
    {
        Debug.Log("Started Main Menu");
        Debug.Log("A:" + NetworkManager.Instance + ", ");
        // Optional: Display connection status
        if (NetworkManager.Instance.isConnected)
        {
            connectionStatusText.text = "Connected to Server";
        }
        else
        {
            connectionStatusText.text = "Not Connected to Server";
        }
    }

    public void OnSinglePlayerButtonClicked()
    {
        // Load the game scene
        SceneManager.LoadScene("SampleScene");
    }

    public void OnMultiplayerButtonClicked()
    {
        if (NetworkManager.Instance.isConnected)
        {
            // Request matchmaking
            NetworkManager.Instance.RequestMatchmaking();

            // Load the game scene
            SceneManager.LoadScene("SampleScene");
        }
        else
        {
            // Notify the player that the server connection failed
            connectionStatusText.text = "Cannot start multiplayer: Not connected to server.";
        }
    }
}
