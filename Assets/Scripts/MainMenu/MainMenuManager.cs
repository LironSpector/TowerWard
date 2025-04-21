using UnityEngine;
using UnityEngine.SceneManagement;
using TMPro;
using System.Collections.Generic;

/// <summary>
/// Description:
/// Manages the main menu UI functionality including navigation between scenes,
/// handling multiplayer matchmaking requests, exiting the game, logging out, and displaying tutorials.
/// </summary>
public class MainMenuManager : MonoBehaviour
{
    /// <summary>
    /// Reference to the Tutorial UI Manager for displaying tutorial content.
    /// </summary>
    public TutorialUIManager tutorialUIManager;

    /// <summary>
    /// Called on the frame when the script is enabled, before any Update methods.
    /// Initializes the main menu by playing music and optionally displaying the connection status.
    /// </summary>
    void Start()
    {
        Debug.Log("Started Main Menu");
        Debug.Log("A:" + NetworkManager.Instance + ", ");

        AudioManager.Instance.PlayMainMenuMusic();
    }

    /// <summary>
    /// Called when the Single Player button is clicked.
    /// Loads the main game scene.
    /// </summary>
    public void OnSinglePlayerButtonClicked()
    {
        SceneManager.LoadScene("SampleScene");
    }

    /// <summary>
    /// Called when the Multiplayer button is clicked.
    /// If the client is connected to the server, sends a matchmaking request and loads the WaitingScene.
    /// Otherwise, displays an error message indicating that multiplayer cannot start.
    /// </summary>
    public void OnMultiplayerButtonClicked()
    {
        if (NetworkManager.Instance.isConnected)
        {
            NetworkManager.Instance.RequestMatchmaking();
            SceneManager.LoadScene("WaitingScene");
        }
    }

    /// <summary>
    /// Called when the Exit button is clicked.
    /// Sends the last login update to the server (if connected), disconnects from the server, and quits the application.
    /// </summary>
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

    /// <summary>
    /// Called when the Logout button is clicked.
    /// Sends an update of the last login time to the server, clears stored authentication tokens and user ID,
    /// and navigates back to the LoginScene.
    /// </summary>
    public void OnLogoutButtonClicked()
    {
        int userId = PlayerPrefs.GetInt("UserId", -1);
        if (userId != -1)
        {
            NetworkManager.Instance.messageSender.SendUpdateLastLogin(userId);
        }

        // Clear saved tokens and user data so that the next session requires login.
        PlayerPrefs.DeleteKey("AccessToken");
        PlayerPrefs.DeleteKey("AccessTokenExpiry");
        PlayerPrefs.DeleteKey("RefreshToken");
        PlayerPrefs.DeleteKey("RefreshTokenExpiry");
        PlayerPrefs.DeleteKey("UserId");
        PlayerPrefs.Save();

        SceneManager.LoadScene("LoginScene");
    }

    /// <summary>
    /// Called when the Tutorial button is clicked.
    /// If a TutorialUIManager is assigned, it displays the tutorial.
    /// Otherwise, logs a warning message.
    /// </summary>
    public void OnTutorialButtonClicked()
    {
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
