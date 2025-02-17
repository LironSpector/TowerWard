using UnityEngine;
using UnityEngine.SceneManagement;
using TMPro;
using System.Collections.Generic;

public class MainMenuManager : MonoBehaviour
{
    public TextMeshProUGUI connectionStatusText;

    // Add a reference to the tutorial manager.
    // If you prefer, you can use a singleton pattern or `FindObjectOfType<TutorialUIManager>()`,
    // but let's keep it a public reference for clarity.
    public TutorialUIManager tutorialUIManager;

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

            // Load the WaitingScene
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
            int userId = PlayerPrefs.GetInt("UserId", -1);
            if (userId != -1)
            {
                NetworkManager.Instance.messageSender.SendUpdateLastLogin(userId);
            }

            NetworkManager.Instance.DisconnectAndQuit();
        }
        Application.Quit();
    }

    public void OnLogoutButtonClicked()
    {
        // Send the "UpdateLastLogin" message
        int userId = PlayerPrefs.GetInt("UserId", -1);
        if (userId != -1)
        {
            NetworkManager.Instance.messageSender.SendUpdateLastLogin(userId);
        }

        // Clear token data (and userId) so the next time we start the game or go back to login scene, we won't skip login
        PlayerPrefs.DeleteKey("AccessToken");
        PlayerPrefs.DeleteKey("AccessTokenExpiry");
        PlayerPrefs.DeleteKey("RefreshToken");
        PlayerPrefs.DeleteKey("RefreshTokenExpiry");
        PlayerPrefs.DeleteKey("UserId");
        PlayerPrefs.Save();

        SceneManager.LoadScene("LoginScene"); // Go back to the LoginScene immediately
    }

    public void OnTutorialButtonClicked()
    {
        // If we have a reference to the tutorial manager, show the tutorial.
        if (tutorialUIManager != null)
        {
            tutorialUIManager.ShowTutorial();
        }
        else
        {
            Debug.LogWarning("TutorialUIManager reference is not assigned in MainMenuManager!");
        }
    }
}
