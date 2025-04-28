using UnityEngine;
using UnityEngine.SceneManagement;
using TMPro;
using System;
using System.Collections;

/// <summary>
/// Manages the login and registration processes in the login scene.
/// Handles user input, displays status messages, enforces security measures like login lockouts and password complexity,
/// and sends appropriate network requests via the NetworkManager.
/// </summary>
public class LoginSceneManager : MonoBehaviour
{
    [Header("Panels")]
    /// <summary>
    /// The panel used to display login UI.
    /// </summary>
    public GameObject loginPanel;
    /// <summary>
    /// The panel used to display registration UI.
    /// </summary>
    public GameObject registerPanel;

    [Header("Login Fields")]
    /// <summary>
    /// Input field for entering the username during login.
    /// </summary>
    public TMP_InputField loginUsernameField;
    /// <summary>
    /// Input field for entering the password during login.
    /// </summary>
    public TMP_InputField loginPasswordField;

    [Header("Register Fields")]
    /// <summary>
    /// Input field for entering the username during registration.
    /// </summary>
    public TMP_InputField registerUsernameField;
    /// <summary>
    /// Input field for entering the password during registration.
    /// </summary>
    public TMP_InputField registerPasswordField;
    /// <summary>
    /// Input field for confirming the password during registration.
    /// </summary>
    public TMP_InputField registerConfirmPasswordField;

    [Header("UI Status Text")]
    /// <summary>
    /// Text field for displaying status messages (such as errors or notifications).
    /// </summary>
    public TextMeshProUGUI statusText;

    // Login security settings
    /// <summary>
    /// Remaining number of allowed login attempts.
    /// </summary>
    private int remainingAttempts = 5;
    /// <summary>
    /// Flag indicating if login is currently locked due to too many failed attempts.
    /// </summary>
    private bool isLocked = false;
    /// <summary>
    /// The time (using Time.time) when the login lockout period will expire.
    /// </summary>
    private float lockoutEndTime = 0f; // Time.time when lockout expires

    /// <summary>
    /// Initializes the login scene. Displays the login panel, logs token information, and starts a delayed auto-login.
    /// </summary>
    private void Start()
    {
        ShowLoginPanel();

        Debug.Log("Passed initial check");
        string token = PlayerPrefs.GetString("AccessToken", "");
        string refreshToken = PlayerPrefs.GetString("RefreshToken", "");
        Debug.Log("Access token is this: " + token);
        Debug.Log("Refresh token is this: " + refreshToken);

        // Delay auto-login to allow initial network setup.
        StartCoroutine(DelayedAutoLogin(token, refreshToken));
    }

    /// <summary>
    /// Coroutine to delay the auto-login request by 0.5 seconds.
    /// </summary>
    /// <param name="token">The access token retrieved from PlayerPrefs.</param>
    /// <param name="refreshToken">The refresh token retrieved from PlayerPrefs.</param>
    /// <returns>An IEnumerator for the coroutine.</returns>
    private IEnumerator DelayedAutoLogin(string token, string refreshToken)
    {
        yield return new WaitForSeconds(0.5f);

        //Debug.Log("Connected? " + NetworkManager.Instance.isConnected);
        //Debug.Log("isHandshakeCompleted? " + NetworkManager.Instance.isHandshakeCompleted);

        NetworkManager.Instance.messageSender.SendAutoLogin(token, refreshToken);
        Debug.Log("Passed initial check 3");
    }

    /// <summary>
    /// Called when the "Login" button is clicked.
    /// Validates input, checks for lockout, and sends a login request if safe.
    /// </summary>
    public void OnClickLogin()
    {
        // Check if login is locked.
        if (isLocked)
        {
            if (Time.time < lockoutEndTime)
            {
                statusText.text = "Too many incorrect attempts. Please try again after 5 minutes.";
                return; // Exit the function so that the login will not be possible
            }
            else
            {
                // Lockout period expired; reset lockout status.
                isLocked = false;
                remainingAttempts = 5;
            }
        }

        // Retrieve input from login fields.
        string username = loginUsernameField.text;
        string password = loginPasswordField.text;

        // Ensure both username and password are provided.
        if (string.IsNullOrEmpty(username) || string.IsNullOrEmpty(password))
        {
            statusText.text = "Please enter username and password.";
            return;
        }

        // Perform basic client-side security checks.
        if (!InputValidator.IsSafeInput(username) || !InputValidator.IsSafeInput(password))
        {
            statusText.text = "Invalid or potentially dangerous input detected. Please try again.";
            return;
        }

        // Send the login request to the server.
        NetworkManager.Instance.messageSender.LoginUser(username, password);
    }

    /// <summary>
    /// Called when the "Register" button is clicked.
    /// Validates registration input, checks for password confirmation and complexity,
    /// and sends a registration request to the server if all validations pass.
    /// </summary>
    public void OnClickRegister()
    {
        // Retrieve input from registration fields.
        string username = registerUsernameField.text;
        string password = registerPasswordField.text;
        string confirmPassword = registerConfirmPasswordField.text;

        // Ensure all registration fields are populated.
        if (string.IsNullOrEmpty(username) || string.IsNullOrEmpty(password) || string.IsNullOrEmpty(confirmPassword))
        {
            statusText.text = "Please fill in all fields.";
            return;
        }

        // Validate input for potential malicious content.
        if (!InputValidator.IsSafeInput(username) ||
            !InputValidator.IsSafeInput(password) ||
            !InputValidator.IsSafeInput(confirmPassword))
        {
            statusText.text = "Invalid or potentially dangerous input detected. Please try again.";
            return;
        }

        // Ensure the password and confirmation match.
        if (!password.Equals(confirmPassword))
        {
            statusText.text = "Passwords do not match! Please try again.";
            return;
        }

        // Check password complexity; if not valid, display the error message.
        string errorMessage;
        if (!PasswordValidator.IsValid(password, out errorMessage))
        {
            statusText.text = errorMessage;
            return;
        }

        // Send the registration request to the server.
        NetworkManager.Instance.messageSender.RegisterUser(username, password);
    }

    /// <summary>
    /// Displays the login panel while hiding the registration panel and clearing any status text.
    /// </summary>
    public void ShowLoginPanel()
    {
        loginPanel.SetActive(true);
        registerPanel.SetActive(false);
        statusText.text = "";
    }

    /// <summary>
    /// Displays the registration panel while hiding the login panel and clearing any status text.
    /// </summary>
    public void ShowRegisterPanel()
    {
        loginPanel.SetActive(false);
        registerPanel.SetActive(true);
        statusText.text = "";
    }

    /// <summary>
    /// Called by the NetworkManager when a login attempt is successful.
    /// Resets login attempt counters, updates last login information, and transitions to the main menu.
    /// </summary>
    public void OnLoginSuccess()
    {
        ResetLoginAttempts();

        int userId = PlayerPrefs.GetInt("UserId", -1);
        if (userId != -1)
        {
            NetworkManager.Instance.messageSender.SendUpdateLastLogin(userId);
        }

        GoToMainMenu();
    }

    /// <summary>
    /// Called by the NetworkManager when registration is successful.
    /// Resets login attempt counters, updates last login information, and transitions to the main menu.
    /// </summary>
    public void OnRegisterSuccess()
    {
        statusText.text = "Registration successful!";
        ResetLoginAttempts();

        int userId = PlayerPrefs.GetInt("UserId", -1);
        if (userId != -1)
        {
            NetworkManager.Instance.messageSender.SendUpdateLastLogin(userId);
        }

        GoToMainMenu();
    }

    /// <summary>
    /// Handles a failed login attempt by reducing the number of remaining attempts.
    /// If attempts reach zero, locks login for 5 minutes.
    /// Displays an error message with the remaining attempts or lockout message.
    /// </summary>
    /// <param name="reason">The reason for the login failure as provided by the server.</param>
    public void OnLoginFail(string reason)
    {
        remainingAttempts--;

        if (remainingAttempts <= 0)
        {
            isLocked = true;
            lockoutEndTime = Time.time + 300f; // Block further attempts for 5 minutes (300 seconds)
            statusText.text = "Too many incorrect attempts. Please try again after 5 minutes.";
        }
        else
        {
            statusText.text = "Login failed: " + reason + ". Attempts left: " + remainingAttempts;
        }
    }

    /// <summary>
    /// Handles a failed registration attempt by displaying the failure reason.
    /// </summary>
    /// <param name="reason">The reason for the registration failure as provided by the server.</param>
    public void OnRegisterFail(string reason)
    {
        statusText.text = "Register failed: " + reason;
    }

    /// <summary>
    /// Transitions the scene to the main menu.
    /// </summary>
    public void GoToMainMenu()
    {
        SceneManager.LoadScene("MainMenu");
    }

    /// <summary>
    /// Resets the login attempt counter and clears any active lockout.
    /// </summary>
    private void ResetLoginAttempts()
    {
        remainingAttempts = 5;
        isLocked = false;
        lockoutEndTime = 0f;
    }
}
