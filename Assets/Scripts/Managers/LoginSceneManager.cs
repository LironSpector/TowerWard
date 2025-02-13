using UnityEngine;
using UnityEngine.UI;
using TMPro; // For using TextMeshPro
using UnityEngine.SceneManagement;
using System;
using System.Collections;


public class LoginSceneManager : MonoBehaviour
{
    [Header("Panels")]
    public GameObject loginPanel;
    public GameObject registerPanel;

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
        ShowLoginPanel();

        Debug.Log("Passed initial check");
        string token = PlayerPrefs.GetString("AccessToken", "");
        string refreshToken = PlayerPrefs.GetString("RefreshToken", "");
        Debug.Log("Access token is this: " + token);
        Debug.Log("Refresh token is this: " + refreshToken);

        // Start a coroutine to delay the execution by 1 second
        StartCoroutine(DelayedAutoLogin(token, refreshToken));
    }

    private IEnumerator DelayedAutoLogin(string token, string refreshToken)
    {
        yield return new WaitForSeconds(1f); // Wait for 1 second
        NetworkManager.Instance.messageSender.SendAutoLogin(token, refreshToken);
        Debug.Log("Passed initial check 3");
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
        NetworkManager.Instance.messageSender.LoginUser(username, password);
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

        NetworkManager.Instance.messageSender.RegisterUser(username, password);
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
            NetworkManager.Instance.messageSender.SendUpdateLastLogin(userId);
        }

        GoToMainMenu();
    }

    /// <summary>
    /// Called by the NetworkManager once registration is successful.
    /// </summary>
    public void OnRegisterSuccess()
    {
        statusText.text = "Registration successful!";

        int userId = PlayerPrefs.GetInt("UserId", -1);
        if (userId != -1)
        {
            NetworkManager.Instance.messageSender.SendUpdateLastLogin(userId);
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

    public void GoToMainMenu()
    {
        SceneManager.LoadScene("MainMenu");
    }
}
