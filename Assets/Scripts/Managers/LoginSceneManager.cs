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

    // Login security settings
    private int remainingAttempts = 5;
    private bool isLocked = false;
    private float lockoutEndTime = 0f; // Time.time when lockout expires

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

        Debug.Log("Connected? " + NetworkManager.Instance.isConnected);
        Debug.Log("isHandshakeCompleted? " + NetworkManager.Instance.isHandshakeCompleted);

        NetworkManager.Instance.messageSender.SendAutoLogin(token, refreshToken);
        Debug.Log("Passed initial check 3");
    }

    // Called by the "Login" button OnClick
    public void OnClickLogin()
    {
        // Check if login is currently locked
        if (isLocked)
        {
            // If lockout time still active, show message and do not proceed
            if (Time.time < lockoutEndTime)
            {
                statusText.text = "Too many incorrect attempts. Please try again after 5 minutes.";
                return; // Exit the function so that the login will not be possible
            }
            else
            {
                // Lockout has expired; reset lock status
                isLocked = false;
                remainingAttempts = 5;
            }
        }

        // Grab input
        string username = loginUsernameField.text;
        string password = loginPasswordField.text;

        // Basic client-side checks
        if (string.IsNullOrEmpty(username) || string.IsNullOrEmpty(password))
        {
            statusText.text = "Please enter username and password.";
            return;
        }

        // Basic malicious pattern checks (client-side)
        if (!InputValidator.IsSafeInput(username) || !InputValidator.IsSafeInput(password))
        {
            statusText.text = "Invalid or potentially dangerous input detected. Please try again.";
            return;
        }

        // Send login request to the server
        NetworkManager.Instance.messageSender.LoginUser(username, password);
    }

    // Called by the "Register" button OnClick
    public void OnClickRegister()
    {
        // Grab input
        string username = registerUsernameField.text;
        string password = registerPasswordField.text;
        string confirmPassword = registerConfirmPasswordField.text;

        if (string.IsNullOrEmpty(username) || string.IsNullOrEmpty(password) ||
            string.IsNullOrEmpty(confirmPassword))
        {
            statusText.text = "Please fill in all fields.";
            return;
        }

        // Basic malicious pattern checks
        if (!InputValidator.IsSafeInput(username) ||
            !InputValidator.IsSafeInput(password) ||
            !InputValidator.IsSafeInput(confirmPassword))
        {
            statusText.text = "Invalid or potentially dangerous input detected. Please try again.";
            return;
        }

        // Check if the password and confirmation match
        if (!password.Equals(confirmPassword))
        {
            statusText.text = "Passwords do not match! Please try again.";
            return;
        }

        // Validate password complexity using our new PasswordValidator utility
        string errorMessage;
        if (!PasswordValidator.IsValid(password, out errorMessage))
        {
            statusText.text = errorMessage;
            return;
        }

        // Send registration request to the server
        NetworkManager.Instance.messageSender.RegisterUser(username, password);
    }

    public void ShowLoginPanel()
    {
        loginPanel.SetActive(true);
        registerPanel.SetActive(false);
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
        // Reset the login attempt counter on success
        ResetLoginAttempts();

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
        // Reset the login attempt counter on a new registration
        ResetLoginAttempts();

        int userId = PlayerPrefs.GetInt("UserId", -1);
        if (userId != -1)
        {
            NetworkManager.Instance.messageSender.SendUpdateLastLogin(userId);
        }

        GoToMainMenu();
    }

    /// <summary>
    /// Handles login failure and manages attempt counts and lockout status.
    /// </summary>
    public void OnLoginFail(string reason)
    {
        remainingAttempts--;

        if (remainingAttempts <= 0)
        {
            isLocked = true;
            lockoutEndTime = Time.time + 20f; // Block further attempts for 5 minutes (300 seconds)
            statusText.text = "Too many incorrect attempts. Please try again after 5 minutes.";
        }
        else
        {
            statusText.text = "Login failed: " + reason + ". Attempts left: " + remainingAttempts;
        }
    }

    public void OnRegisterFail(string reason)
    {
        statusText.text = "Register failed: " + reason;
    }

    public void GoToMainMenu()
    {
        SceneManager.LoadScene("MainMenu");
    }

    /// <summary>
    /// Resets the login attempt counter and clears any lockout flags.
    /// </summary>
    private void ResetLoginAttempts()
    {
        remainingAttempts = 5;
        isLocked = false;
        lockoutEndTime = 0f;
    }
}








//using UnityEngine;
//using UnityEngine.UI;
//using TMPro; // For using TextMeshPro
//using UnityEngine.SceneManagement;
//using System;
//using System.Collections;

//public class LoginSceneManager : MonoBehaviour
//{
//    [Header("Panels")]
//    public GameObject loginPanel;
//    public GameObject registerPanel;

//    [Header("Login Fields")]
//    public TMP_InputField loginUsernameField;
//    public TMP_InputField loginPasswordField;

//    [Header("Register Fields")]
//    public TMP_InputField registerUsernameField;
//    public TMP_InputField registerPasswordField;
//    public TMP_InputField registerConfirmPasswordField;

//    [Header("UI Status Text")]
//    public TextMeshProUGUI statusText;   // For displaying errors, status, etc.

//    private void Start()
//    {
//        ShowLoginPanel();

//        Debug.Log("Passed initial check");
//        string token = PlayerPrefs.GetString("AccessToken", "");
//        string refreshToken = PlayerPrefs.GetString("RefreshToken", "");
//        Debug.Log("Access token is this: " + token);
//        Debug.Log("Refresh token is this: " + refreshToken);

//        // Start a coroutine to delay the execution by 1 second
//        StartCoroutine(DelayedAutoLogin(token, refreshToken));
//    }

//    private IEnumerator DelayedAutoLogin(string token, string refreshToken)
//    {
//        yield return new WaitForSeconds(1f); // Wait for 1 second

//        Debug.Log("Connected? " + NetworkManager.Instance.isConnected);
//        Debug.Log("isHandshakeCompleted? " + NetworkManager.Instance.isHandshakeCompleted);

//        NetworkManager.Instance.messageSender.SendAutoLogin(token, refreshToken);
//        Debug.Log("Passed initial check 3");
//    }

//    // Called by the "Login" button OnClick
//    public void OnClickLogin()
//    {
//        // Grab input
//        string username = loginUsernameField.text;
//        string password = loginPasswordField.text;

//        // Basic client-side checks
//        if (string.IsNullOrEmpty(username) || string.IsNullOrEmpty(password))
//        {
//            statusText.text = "Please enter username and password.";
//            return;
//        }

//        // Basic malicious pattern checks (client-side)
//        if (!InputValidator.IsSafeInput(username) || !InputValidator.IsSafeInput(password))
//        {
//            statusText.text = "Invalid or potentially dangerous input detected. Please try again.";
//            return;
//        }

//        // Send login request to the server
//        NetworkManager.Instance.messageSender.LoginUser(username, password);
//    }

//    // Called by the "Register" button OnClick
//    public void OnClickRegister()
//    {
//        // Grab input
//        string username = registerUsernameField.text;
//        string password = registerPasswordField.text;
//        string confirmPassword = registerConfirmPasswordField.text;

//        if (string.IsNullOrEmpty(username) || string.IsNullOrEmpty(password) ||
//            string.IsNullOrEmpty(confirmPassword))
//        {
//            statusText.text = "Please fill in all fields.";
//            return;
//        }

//        // Basic malicious pattern checks
//        if (!InputValidator.IsSafeInput(username) ||
//            !InputValidator.IsSafeInput(password) ||
//            !InputValidator.IsSafeInput(confirmPassword))
//        {
//            statusText.text = "Invalid or potentially dangerous input detected. Please try again.";
//            return;
//        }

//        if (!password.Equals(confirmPassword))
//        {
//            // Password mismatch
//            statusText.text = "Passwords do not match! Please try again.";
//            return;
//        }

//        NetworkManager.Instance.messageSender.RegisterUser(username, password);
//    }

//    public void ShowLoginPanel()
//    {
//        loginPanel.SetActive(true);
//        registerPanel.SetActive(false);
//        //registerPanel.SetActive(true); // For testing
//        statusText.text = "";
//    }

//    public void ShowRegisterPanel()
//    {
//        loginPanel.SetActive(false);
//        registerPanel.SetActive(true);
//        statusText.text = "";
//    }

//    /// <summary>
//    /// Called by the NetworkManager once login is successful.
//    /// </summary>
//    public void OnLoginSuccess()
//    {
//        statusText.text = "Login successful!";

//        int userId = PlayerPrefs.GetInt("UserId", -1);
//        if (userId != -1)
//        {
//            NetworkManager.Instance.messageSender.SendUpdateLastLogin(userId);
//        }

//        GoToMainMenu();
//    }

//    /// <summary>
//    /// Called by the NetworkManager once registration is successful.
//    /// </summary>
//    public void OnRegisterSuccess()
//    {
//        statusText.text = "Registration successful!";

//        int userId = PlayerPrefs.GetInt("UserId", -1);
//        if (userId != -1)
//        {
//            NetworkManager.Instance.messageSender.SendUpdateLastLogin(userId);
//        }

//        GoToMainMenu();
//    }

//    public void OnLoginFail(string reason)
//    {
//        statusText.text = "Login failed: " + reason;
//    }

//    public void OnRegisterFail(string reason)
//    {
//        statusText.text = "Register failed: " + reason;
//    }

//    public void GoToMainMenu()
//    {
//        SceneManager.LoadScene("MainMenu");
//    }
//}
