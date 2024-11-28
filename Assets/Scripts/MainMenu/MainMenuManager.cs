//using UnityEngine;
//using UnityEngine.SceneManagement;

//public class MainMenuManager : MonoBehaviour
//{
//    public void StartGame()
//    {
//        // Load the main game scene
//        SceneManager.LoadScene("SampleScene");
//    }

//    public void StartMultiplayer()
//    {
//        //// Ensure NetworkManager is connected
//        //NetworkManager.Instance.ConnectToServer();

//        // Request matchmaking
//        NetworkManager.Instance.RequestMatchmaking();

//        // Load the game scene
//        //SceneManager.LoadScene("GameScene");
//        SceneManager.LoadScene("SampleScene");
//    }

//}









//using UnityEngine;
//using UnityEngine.SceneManagement;

//public class MainMenuManager : MonoBehaviour
//{
//    public void StartSinglePlayer()
//    {
//        // Since we're already connected, simply load the game scene
//        SceneManager.LoadScene("SampleScene");
//    }

//    public void StartMultiplayer()
//    {
//        if (NetworkManager.Instance.isConnected)
//        {
//            // Request matchmaking
//            NetworkManager.Instance.RequestMatchmaking();

//            // Load the game scene if not already loaded
//            SceneManager.LoadScene("SampleScene");
//        }
//        else
//        {
//            // Handle the case where the server is not connected
//            Debug.LogError("Cannot start multiplayer: Not connected to server.");
//            // Notify the player or attempt reconnection
//        }
//    }
//}









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
