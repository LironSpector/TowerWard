using UnityEngine;
using Newtonsoft.Json.Linq;
using System.Diagnostics;
using System;
using System.Text;

/// <summary>
/// Description:
/// Responsible for sending network messages to the server. It handles both simple and authenticated messages,
/// where authenticated messages include token data (access and refresh tokens) retrieved from PlayerPrefs.
/// Messages are encrypted using AES (post-handshake) and sent over the network with a length prefix.
/// This class also provides helper methods for login, registration, updating last login time, game over details,
/// and auto login messages.
/// </summary>
public class NetworkMessageSender : MonoBehaviour
{
    private NetworkManager net;

    /// <summary>
    /// Awake is called when the script instance is being loaded.
    /// Initializes the reference to the NetworkManager component on the same GameObject.
    /// </summary>
    void Awake()
    {
        net = GetComponent<NetworkManager>();
    }

    /// <summary>
    /// Encrypts a plain JSON string using AES encryption, prepends the message with its byte length, and sends it over the network.
    /// This method is used for sending messages after the handshake has been completed.
    /// </summary>
    /// <param name="plainJson">A string containing the plain JSON message to send.</param>
    public void SendMessageWithLengthPrefix(string plainJson)
    {
        if (!net.isConnected || !net.isHandshakeCompleted) return;

        // 1) Encrypt the plain JSON message.
        string cipherBase64 = net.encryptionManager.Encrypt(plainJson);

        // 2) Convert the encrypted message to bytes and compute its length.
        byte[] messageBytes = Encoding.UTF8.GetBytes(cipherBase64);
        byte[] lengthBytes = BitConverter.GetBytes(messageBytes.Length);

        // 3) Write the length prefix followed by the encrypted message to the network stream.
        net.stream.Write(lengthBytes, 0, lengthBytes.Length);
        net.stream.Write(messageBytes, 0, messageBytes.Length);

        // Optionally, uncomment the following line to log the sent message:
        // Debug.Log("[CLIENT] Sent ENCRYPTED => " + plainJson);
    }

    /// <summary>
    /// Constructs an authenticated message by embedding access and refresh tokens into the JSON structure,
    /// then encrypts and sends it using AES.
    /// </summary>
    /// <param name="messageType">A string representing the type of message (e.g., "LoginUser", "RegisterUser").</param>
    /// <param name="data">A JObject containing the message-specific data. Can be null.</param>
    public void SendAuthenticatedMessage(string messageType, JObject data)
    {
        // 1) Build the complete JSON message with authentication tokens.
        JObject finalMsg = BuildMessageWithToken(messageType, data);

        // 2) Convert the JSON message to a string and send it.
        string plainJson = finalMsg.ToString();
        SendMessageWithLengthPrefix(plainJson);
    }

    /// <summary>
    /// Creates a JSON structure for a message that includes the message type, token information, and message data.
    /// The structure is as follows:
    /// <code>
    /// {
    ///   "Type": "<messageType>",
    ///   "TokenData": {
    ///       "AccessToken": "<token>",
    ///       "RefreshToken": "<rToken>"
    ///   },
    ///   "Data": { ...data... }
    /// }
    /// </code>
    /// </summary>
    /// <param name="messageType">The type of the message to be sent.</param>
    /// <param name="data">The message-specific data in a JObject. If null, an empty JObject is used.</param>
    /// <returns>A JObject representing the complete message with embedded token data.</returns>
    private JObject BuildMessageWithToken(string messageType, JObject data)
    {
        // Retrieve authentication tokens from PlayerPrefs.
        string accessToken = PlayerPrefs.GetString("AccessToken", "");
        string refreshToken = PlayerPrefs.GetString("RefreshToken", "");

        // Create the overall JSON message structure.
        JObject msg = new JObject
        {
            ["Type"] = messageType
        };

        // Create and attach the token part.
        JObject tokenObj = new JObject
        {
            ["AccessToken"] = accessToken,
            ["RefreshToken"] = refreshToken
        };
        msg["TokenData"] = tokenObj;

        // Attach the data part. If 'data' is null, attach an empty JObject.
        msg["Data"] = data ?? new JObject();
        return msg;
    }

    #region Helper Methods for Specific Messages

    /// <summary>
    /// Sends a login request to the server with the provided username and password.
    /// </summary>
    /// <param name="username">The username for login.</param>
    /// <param name="password">The password for login.</param>
    public void LoginUser(string username, string password)
    {
        if (!net.isConnected)
        {
            // Optionally log a warning if the client is not connected.
            // Debug.LogWarning("[CLIENT] Not connected. Can't login.");
            return;
        }
        JObject dataObj = new JObject
        {
            ["Username"] = username,
            ["Password"] = password
        };
        SendAuthenticatedMessage("LoginUser", dataObj);
    }

    /// <summary>
    /// Sends a registration request to the server with the provided username and password.
    /// </summary>
    /// <param name="username">The username for registration.</param>
    /// <param name="password">The password for registration.</param>
    public void RegisterUser(string username, string password)
    {
        if (!net.isConnected)
        {
            // Optionally log a warning if the client is not connected.
            // Debug.LogWarning("[CLIENT] Not connected. Can't register.");
            return;
        }
        JObject dataObj = new JObject
        {
            ["Username"] = username,
            ["Password"] = password
        };
        SendAuthenticatedMessage("RegisterUser", dataObj);
    }

    /// <summary>
    /// Sends an update to the server with the user's last login information.
    /// </summary>
    /// <param name="userId">The unique identifier of the user.</param>
    public void SendUpdateLastLogin(int userId)
    {
        if (!net.isConnected || !net.isHandshakeCompleted)
        {
            // Optionally log a warning.
            // Debug.LogWarning("[CLIENT] Can't send UpdateLastLogin - not connected or handshake incomplete.");
            return;
        }
        JObject dataObj = new JObject
        {
            ["UserId"] = userId
        };
        SendAuthenticatedMessage("UpdateLastLogin", dataObj);
        // Optionally log for debugging:
        // Debug.Log("[CLIENT] Sent UpdateLastLogin with userId=" + userId);
    }

    /// <summary>
    /// Sends detailed game over information to the server.
    /// This includes user identifiers, game mode, the winning user ID, final wave reached, and total time played.
    /// </summary>
    /// <param name="user1Id">The user ID for the primary player.</param>
    /// <param name="user2Id">The user ID for the opponent, if applicable; otherwise, null.</param>
    /// <param name="mode">A string representing the game mode (e.g., "SinglePlayer", "Multiplayer").</param>
    /// <param name="wonUserId">The user ID of the winning player, or null if the local player lost.</param>
    /// <param name="finalWave">The final wave reached in the game.</param>
    /// <param name="timePlayed">The total time played (in seconds or minutes as per implementation).</param>
    public void SendGameOverDetailed(int user1Id, int? user2Id, string mode, int? wonUserId, int finalWave, int timePlayed)
    {
        if (!net.isConnected || !net.isHandshakeCompleted) return;

        JObject dataObj = new JObject
        {
            ["User1Id"] = user1Id,
            ["User2Id"] = user2Id,
            ["Mode"] = mode,
            ["WonUserId"] = wonUserId,
            ["FinalWave"] = finalWave,
            ["TimePlayed"] = timePlayed
        };
        SendAuthenticatedMessage("GameOverDetailed", dataObj);
        // Optionally log for debugging:
        // Debug.Log($"[CLIENT] Sent GameOverDetailed => user1Id={user1Id}, user2Id={user2Id}, finalWave={finalWave}, timePlayed={timePlayed}");
    }

    /// <summary>
    /// Sends an auto-login request to the server using the provided access and refresh tokens.
    /// </summary>
    /// <param name="accessToken">The access token to use for authentication.</param>
    /// <param name="refreshToken">The refresh token to use for authentication.</param>
    public void SendAutoLogin(string accessToken, string refreshToken)
    {
        if (!net.isConnected || !net.isHandshakeCompleted)
        {
            // Optionally log a warning if the client is not connected or handshake is not complete.
            // Debug.LogWarning("[CLIENT] Not connected/handshake not done, can't AutoLogin");
            return;
        }
        JObject dataObj = new JObject
        {
            ["AccessToken"] = accessToken,
            ["RefreshToken"] = refreshToken
        };
        SendAuthenticatedMessage("AutoLogin", dataObj);
        // Optionally log for debugging:
        // Debug.Log("[CLIENT] Sent AutoLogin with token length=" + accessToken.Length);
    }

    #endregion
}
