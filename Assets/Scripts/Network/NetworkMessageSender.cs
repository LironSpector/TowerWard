using UnityEngine;
using Newtonsoft.Json.Linq;
using System.Diagnostics;
using UnityEngine;
using System;
using System.Text;

public class NetworkMessageSender : MonoBehaviour
{
    private NetworkManager net;

    void Awake()
    {
        net = GetComponent<NetworkManager>();
    }

    /// <summary>
    /// Send a plain JSON message post-handshake. We'll AES-encrypt it and then send with length prefix.
    /// </summary>
    public void SendMessageWithLengthPrefix(string plainJson)
    {
        if (!net.isConnected || !net.isHandshakeCompleted) return;

        // 1) Encrypt
        string cipherBase64 = net.encryptionManager.Encrypt(plainJson);

        // 2) Send
        byte[] messageBytes = Encoding.UTF8.GetBytes(cipherBase64);
        byte[] lengthBytes = BitConverter.GetBytes(messageBytes.Length);

        net.stream.Write(lengthBytes, 0, lengthBytes.Length);
        net.stream.Write(messageBytes, 0, messageBytes.Length);


        //Debug.Log("[CLIENT] Sent ENCRYPTED => " + plainJson);
    }

    /// <summary>
    /// Builds a message with tokens from PlayerPrefs, then sends via AES.
    /// </summary>
    public void SendAuthenticatedMessage(string messageType, JObject data)
    {
        // 1) Construct the JSON with tokens
        JObject finalMsg = BuildMessageWithToken(messageType, data);

        // 2) Convert to string & send via AES
        string plainJson = finalMsg.ToString();
        SendMessageWithLengthPrefix(plainJson);
    }

    /// <summary>
    /// Creates the JSON structure with "Type", "TokenData", and "Data".
    /// </summary>
    private JObject BuildMessageWithToken(string messageType, JObject data)
    {
        // Grab tokens from PlayerPrefs
        string accessToken = PlayerPrefs.GetString("AccessToken", "");
        string refreshToken = PlayerPrefs.GetString("RefreshToken", "");

        // The overall message format:
        // {
        //   "Type": "<messageType>",
        //   "TokenData": {
        //       "AccessToken": "<token>",
        //       "RefreshToken": "<rToken>"
        //   },
        //   "Data": { ...data... }
        // }

        JObject msg = new JObject
        {
            ["Type"] = messageType
        };

        // Token part
        JObject tokenObj = new JObject
        {
            ["AccessToken"] = accessToken,
            ["RefreshToken"] = refreshToken
        };
        msg["TokenData"] = tokenObj;

        // The actual message data
        msg["Data"] = data ?? new JObject();
        return msg;
    }


    // ------------------------------------------------------------------
    //  Helper methods for specific messages
    // ------------------------------------------------------------------
    public void LoginUser(string username, string password)
    {
        if (!net.isConnected)
        {
            //Debug.LogWarning("[CLIENT] Not connected. Can't login.");
            return;
        }
        JObject dataObj = new JObject
        {
            ["Username"] = username,
            ["Password"] = password
        };
        SendAuthenticatedMessage("LoginUser", dataObj);
    }

    public void RegisterUser(string username, string password)
    {
        if (!net.isConnected)
        {
            //Debug.LogWarning("[CLIENT] Not connected. Can't register.");
            return;
        }
        JObject dataObj = new JObject
        {
            ["Username"] = username,
            ["Password"] = password
        };
        SendAuthenticatedMessage("RegisterUser", dataObj);
    }

    public void SendUpdateLastLogin(int userId)
    {
        if (!net.isConnected || !net.isHandshakeCompleted)
        {
            //Debug.LogWarning("[CLIENT] Can't send UpdateLastLogin - not connected or handshake incomplete.");
            return;
        }
        JObject dataObj = new JObject
        {
            ["UserId"] = userId
        };
        SendAuthenticatedMessage("UpdateLastLogin", dataObj);
        //Debug.Log("[CLIENT] Sent UpdateLastLogin with userId=" + userId);
    }

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
        //Debug.Log($"[CLIENT] Sent GameOverDetailed => user1Id={user1Id}, user2Id={user2Id}, finalWave={finalWave}, timePlayed={timePlayed}");
    }

    public void SendAutoLogin(string accessToken, string refreshToken)
    {
        if (!net.isConnected || !net.isHandshakeCompleted)
        {
            //Debug.Log("[CLIENT] Not connected/handshake not done, can't AutoLogin");
            //Debug.LogWarning("[CLIENT] Not connected/handshake not done, can't AutoLogin");
            return;
        }
        JObject dataObj = new JObject
        {
            ["AccessToken"] = accessToken,
            ["RefreshToken"] = refreshToken,
        };
        SendAuthenticatedMessage("AutoLogin", dataObj);
        //Debug.Log("[CLIENT] Sent AutoLogin with token length=" + accessToken.Length);
    }
}
