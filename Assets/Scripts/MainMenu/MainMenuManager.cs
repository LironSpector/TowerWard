using UnityEngine;
using UnityEngine.SceneManagement;
using TMPro;
using System.Collections.Generic;

public class MainMenuManager : MonoBehaviour
{
    public TextMeshProUGUI connectionStatusText;

    void Start()
    {
        Debug.Log("Started Main Menu");
        Debug.Log("A:" + NetworkManager.Instance + ", ");

        AudioManager.Instance.PlayMainMenuMusic();

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
            SceneManager.LoadScene("WaitingScene");
        }
        else
        {
            // Notify the player that the server connection failed
            connectionStatusText.text = "Cannot start multiplayer: Not connected to server.";
        }
    }

    public void OnExitButtonClicked()
    {
        if (NetworkManager.Instance != null && NetworkManager.Instance.isConnected)
        {
            NetworkManager.Instance.DisconnectAndQuit();
            //NetworkManager.Instance.Disconnect(); // you can write a custom Disconnect method
        }
        Application.Quit();
    }

    public void OnLogoutButtonClicked()
    {
        // Clear token data so the next time we start the game or go back to login scene, 
        // we won't skip login
        PlayerPrefs.DeleteKey("AccessToken");
        PlayerPrefs.DeleteKey("AccessTokenExpiry");
        PlayerPrefs.DeleteKey("RefreshToken");
        PlayerPrefs.DeleteKey("RefreshTokenExpiry");

        // Optional: If you want to load the LoginScene immediately:
        SceneManager.LoadScene("LoginScene");
    }

}
