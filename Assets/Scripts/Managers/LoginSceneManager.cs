using UnityEngine;
using UnityEngine.UI;
using TMPro; // If using TextMeshPro
using UnityEngine.SceneManagement;
using System;

public class LoginSceneManager : MonoBehaviour
{
    [Header("Panels")]
    public GameObject loginPanel;     // Assign in Inspector
    public GameObject registerPanel;  // Assign in Inspector

    [Header("Login Fields")]
    public TMP_InputField loginUsernameField;
    public TMP_InputField loginPasswordField;

    [Header("Register Fields")]
    public TMP_InputField registerUsernameField;
    public TMP_InputField registerPasswordField;
    public TMP_InputField registerConfirmPasswordField;

    [Header("UI Status Text")]
    public TextMeshProUGUI statusText;   // For displaying errors, status, etc.

    private void Start()
    {
        // Step 1: The game just launched on the LoginScene.
        // We'll see if we have a locally stored token in PlayerPrefs 
        // and if it's still valid. If it is, skip to MainMenu.

        if (CheckExistingTokenValid())
        {
            Debug.Log("[LoginSceneManager] Token still valid. Skipping login UI.");
            GoToMainMenu();
        }
        else
        {
            // Show the login panel by default
            ShowLoginPanel();
        }
    }

    /// <summary>
    /// Checks if there's an AccessToken + Expiry in PlayerPrefs. 
    /// If it's still in the future, we consider it valid. 
    /// (This is a simple approach; in real code, you might parse the JWT or do a server check.)
    /// </summary>
    private bool CheckExistingTokenValid()
    {
        string accessToken = PlayerPrefs.GetString("AccessToken", "");
        string expiryString = PlayerPrefs.GetString("AccessTokenExpiry", "");
        // if we never stored the expiry, this might be blank.

        if (string.IsNullOrEmpty(accessToken) || string.IsNullOrEmpty(expiryString))
        {
            return false; // no token
        }

        if (DateTime.TryParse(expiryString, out DateTime expiry))
        {
            // If it's still valid
            if (DateTime.UtcNow < expiry)
            {
                Debug.Log("[LoginSceneManager] Found unexpired token in PlayerPrefs.");
                return true;
            }
        }
        return false;
    }

    // Called by the "Login" button OnClick
    public void OnClickLogin()
    {
        string username = loginUsernameField.text;
        string password = loginPasswordField.text;

        // Basic client-side checks
        if (string.IsNullOrEmpty(username) || string.IsNullOrEmpty(password))
        {
            statusText.text = "Please enter username and password.";
            return;
        }

        // Send login request to the server
        NetworkManager.Instance.LoginUser(username, password);
    }

    // Called by the "Register" button OnClick
    public void OnClickRegister()
    {
        string username = registerUsernameField.text;
        string password = registerPasswordField.text;
        string confirmPassword = registerConfirmPasswordField.text;

        if (string.IsNullOrEmpty(username) || string.IsNullOrEmpty(password) ||
            string.IsNullOrEmpty(confirmPassword))
        {
            statusText.text = "Please fill in all fields.";
            return;
        }
        if (!password.Equals(confirmPassword))
        {
            // Password mismatch
            statusText.text = "Passwords do not match! Please try again.";
            return;
        }

        NetworkManager.Instance.RegisterUser(username, password);
    }

    public void ShowLoginPanel()
    {
        loginPanel.SetActive(true);
        //registerPanel.SetActive(false);
        registerPanel.SetActive(true); //For testing
        statusText.text = "";
    }

    public void ShowRegisterPanel()
    {
        loginPanel.SetActive(false);
        registerPanel.SetActive(true);
        statusText.text = "";
    }

    /// <summary>
    /// Called by the NetworkManager once login is successful.
    /// </summary>
    public void OnLoginSuccess()
    {
        statusText.text = "Login successful!";

        int userId = PlayerPrefs.GetInt("UserId", -1);
        if (userId != -1)
        {
            NetworkManager.Instance.SendUpdateLastLogin(userId);
        }

        // Optionally wait a bit, or directly go to MainMenu
        GoToMainMenu();
    }

    /// <summary>
    /// Called by the NetworkManager once registration is successful.
    /// </summary>
    public void OnRegisterSuccess()
    {
        statusText.text = "Registration successful!";
        // Possibly auto login or just say success. 

        int userId = PlayerPrefs.GetInt("UserId", -1);
        if (userId != -1)
        {
            NetworkManager.Instance.SendUpdateLastLogin(userId);
        }

        GoToMainMenu();
    }

    public void OnLoginFail(string reason)
    {
        statusText.text = "Login failed: " + reason;
    }

    public void OnRegisterFail(string reason)
    {
        statusText.text = "Register failed: " + reason;
    }

    private void GoToMainMenu()
    {
        SceneManager.LoadScene("MainMenu");
    }
}
