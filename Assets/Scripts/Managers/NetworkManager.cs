// -------------------- New NetworkManager - after adding AES & RSA encryption --------------------
using UnityEngine;
using System;
using System.Net.Sockets;
using System.Text;
using System.Threading;
using System.Threading.Tasks;
using Newtonsoft.Json;
using Newtonsoft.Json.Linq;
using System.Collections;
using System.Collections.Generic;
using System.Security.Cryptography;


public class NetworkManager : MonoBehaviour
{
    public static NetworkManager Instance;

    private TcpClient clientSocket;
    private NetworkStream stream;
    private byte[] buffer = new byte[4096];

    //private string serverIP = "127.0.0.1";
    private string serverIP = "10.100.102.11";
    private int port = 5555; // Ensure it matches the server port

    private Thread clientThread;

    public bool isConnected = false;
    public bool IsMatchmakingRequested { get; private set; } = false;


    // Encryption
    private bool handshakeCompleted = false;
    private byte[] aesKey;
    private byte[] aesIV;

    // RSA server public key
    private byte[] serverModulus;
    private byte[] serverExponent;


    void Awake()
    {
        // Singleton pattern
        if (Instance == null)
        {
            Instance = this;
            DontDestroyOnLoad(gameObject);
        }
        else
            Destroy(gameObject);


        //Connect to the server only in the first time the MainMenu is shown (at entering the game in the first time and not also when returning to the MainMenu scene).
        if (NetworkManager.Instance.isConnected == false)
            ConnectToServer(); // Connect immediately


        UnityMainThreadDispatcher.Instance();

    }

    public async void ConnectToServer()
    {
        clientSocket = new TcpClient();

        try
        {
            await clientSocket.ConnectAsync(serverIP, port);
            stream = clientSocket.GetStream();

            // Start a thread to listen for incoming data
            clientThread = new Thread(ListenForData);
            clientThread.IsBackground = true;
            clientThread.Start();

            isConnected = true;
            Debug.Log("Connected to server.");
        }
        catch (Exception ex)
        {
            Debug.LogError("Failed to connect to server: " + ex.Message);
            isConnected = false;
            // Handle reconnection logic or notify the player
        }
    }


    //private void ListenForData()
    //{
    //    try
    //    {
    //        while (true)
    //        {
    //            // Read length
    //            byte[] lengthBuffer = new byte[4];
    //            int bytesRead = stream.Read(lengthBuffer, 0, 4);
    //            if (bytesRead == 0) break; // Connection closed
    //            int messageLength = BitConverter.ToInt32(lengthBuffer, 0);

    //            // Now read the message
    //            byte[] messageBuffer = new byte[messageLength];
    //            int totalBytesRead = 0;
    //            while (totalBytesRead < messageLength)
    //            {
    //                int read = stream.Read(messageBuffer, totalBytesRead, messageLength - totalBytesRead);
    //                if (read == 0) break;
    //                totalBytesRead += read;
    //            }

    //            if (totalBytesRead < messageLength)
    //                break; // Connection closed partway through message

    //            string data = Encoding.UTF8.GetString(messageBuffer, 0, totalBytesRead);
    //            Debug.Log("Data is: " + data);

    //            UnityMainThreadDispatcher.Instance().Enqueue(() => HandleMessage(data));
    //        }
    //    }
    //    catch (Exception ex)
    //    {
    //        UnityMainThreadDispatcher.Instance().Enqueue(() =>
    //        {
    //            Debug.Log("Disconnected from server: " + ex.Message);
    //            isConnected = false;
    //        });
    //    }
    //}


    private void ListenForData()
    {
        try
        {
            while (true)
            {
                // 1) Read 4-byte length
                byte[] lengthBuffer = new byte[4];
                int bytesRead = stream.Read(lengthBuffer, 0, 4);
                if (bytesRead == 0) break;
                int messageLength = BitConverter.ToInt32(lengthBuffer, 0);

                // 2) Read the message
                byte[] messageBuffer = new byte[messageLength];
                int totalBytesRead = 0;
                while (totalBytesRead < messageLength)
                {
                    int read = stream.Read(messageBuffer, totalBytesRead, messageLength - totalBytesRead);
                    if (read == 0) break;
                    totalBytesRead += read;
                }
                if (totalBytesRead < messageLength) break;

                // 3) Convert to string
                string receivedData = Encoding.UTF8.GetString(messageBuffer, 0, totalBytesRead);
                if (!handshakeCompleted)
                {
                    // We are in handshake mode
                    Debug.Log("A");
                    HandleHandshakeMessage(receivedData);
                    Debug.Log("B");
                }
                else
                {
                    // AES-encrypted base64. Decrypt, then handle
                    string decryptedJson = AesEncryptionClient.DecryptAES(receivedData, aesKey, aesIV);
                    UnityMainThreadDispatcher.Instance().Enqueue(() => HandleMessage(decryptedJson));
                }
            }
        }
        catch (Exception ex)
        {
            UnityMainThreadDispatcher.Instance().Enqueue(() =>
            {
                Debug.Log("[CLIENT] Disconnected: " + ex.Message);
                isConnected = false;
            });
        }
    }


    /// <summary>
    /// During handshake, first we expect "ServerPublicKey", then we send "AESKeyExchange".
    /// </summary>
    private void HandleHandshakeMessage(string data)
    {
        Debug.Log("[CLIENT] Handshake msg: " + data);
        JObject jo = JObject.Parse(data);
        string msgType = jo["Type"].ToString();

        if (msgType == "ServerPublicKey")
        {
            // Retrieve modulus + exponent
            serverModulus = Convert.FromBase64String(jo["Modulus"].ToString());
            serverExponent = Convert.FromBase64String(jo["Exponent"].ToString());

            // Next step: generate AES and send "AESKeyExchange"
            Debug.Log("C");
            GenerateAESKeyAndSend();
            Debug.Log("D");
        }
        else
        {
            // Possibly it's a handshake confirmation or something else
            Debug.LogWarning("[CLIENT] Unexpected handshake message type: " + msgType);
        }
    }

    /// <summary>
    /// Generate random AES key+IV locally, encrypt them with the server's RSA public key,
    /// and send them in a "AESKeyExchange" message.
    /// </summary>
    private void GenerateAESKeyAndSend()
    {
        // 1) Generate random AES key/IV
        using (Aes aes = Aes.Create())
        {
            aes.KeySize = 256;
            aes.GenerateKey();
            aes.GenerateIV();
            aesKey = aes.Key;
            aesIV = aes.IV;
        }

        Debug.Log("X");

        // 2) RSA-encrypt them
        using (RSA rsa = RSA.Create())
        {
            rsa.ImportParameters(new RSAParameters
            {
                Modulus = serverModulus,
                Exponent = serverExponent
            });
            Debug.Log("Y");

            //byte[] encKey = rsa.Encrypt(aesKey, RSAEncryptionPadding.OaepSHA256);
            //byte[] encIV = rsa.Encrypt(aesIV, RSAEncryptionPadding.OaepSHA256);
            byte[] encKey = rsa.Encrypt(aesKey, RSAEncryptionPadding.OaepSHA1);
            byte[] encIV = rsa.Encrypt(aesIV, RSAEncryptionPadding.OaepSHA1);
            Debug.Log("Z");

            // 3) Build JSON
            JObject exchange = new JObject();
            exchange["Type"] = "AESKeyExchange";
            exchange["EncryptedKey"] = Convert.ToBase64String(encKey);
            exchange["EncryptedIV"] = Convert.ToBase64String(encIV);

            // 4) Send it unencrypted
            SendRaw(exchange.ToString());
        }

        // 5) The server should decrypt & store. 
        // We'll assume once we send that, the server is ready. 
        // We'll set handshakeCompleted = true, or we can wait for a handshake confirm message.
        handshakeCompleted = true;
        Debug.Log("[CLIENT] AESKeyExchange sent. Handshake completed on client side.");

        Debug.Log("client aesKey:");
        string key = "";
        for (int i = 0; i < aesKey.Length; i++)
        {
            key += aesKey[i];
        }
        Debug.Log(key);
        Debug.Log("client aesIV:");
        string iv = "";
        for (int i = 0; i < aesIV.Length; i++)
        {
            iv += aesIV[i];
        }
        Debug.Log(iv);
    }


    private void HandleMessage(string data)
    {
        Debug.Log("[CLIENT] Decrypted message: " + data);

        JObject messageObject = JObject.Parse(data);
        string messageType = messageObject["Type"].ToString();

        switch (messageType)
        {
            // ------- Previous "MatchFound" case (before JWT server change and opponent user id getting) -------
            //case "MatchFound":
            //    // Handle MatchFound message
            //    UnityMainThreadDispatcher.Instance().Enqueue(() =>
            //    {
            //        Debug.Log("Match found!");
            //        // Check if MatchmakingManager exists
            //        if (MatchmakingManager.Instance != null)
            //        {
            //            MatchmakingManager.Instance.OnMatchFound();
            //        }
            //        else if (GameManager.Instance != null)
            //        {
            //            GameManager.Instance.OnMatchFound();
            //        }
            //        else
            //        {
            //            Debug.LogError("No handler found for MatchFound message.");
            //        }
            //    });
            //    break;

            case "MatchFound":
                {
                    // e.g. { "Type":"MatchFound", "Data":{"OpponentId":999} }
                    JObject dataObj = (JObject)messageObject["Data"];
                    int oppId = dataObj["OpponentId"]?.ToObject<int>() ?? -1;

                    UnityMainThreadDispatcher.Instance().Enqueue(() =>
                    {
                        Debug.Log($"Match found! OpponentId = {oppId}");

                        if (MatchmakingManager.Instance != null)
                        {
                            MatchmakingManager.Instance.OnMatchFound();
                            PlayerPrefs.SetInt("OpponentUserId", oppId);
                            PlayerPrefs.Save();
                        }
                        else if (GameManager.Instance != null)
                        {
                            GameManager.Instance.flowController.OnMatchFound();
                            //GameManager.Instance.OnMatchFound();
                            PlayerPrefs.SetInt("OpponentUserId", oppId);
                            PlayerPrefs.Save();
                        }
                        else
                        {
                            Debug.LogError("No handler found for MatchFound message.");
                        }
                    });
                    break;
                }


            case "SendBalloon":
                {
                    // e.g. { "Type":"SendBalloon", "Data": { "BalloonHealth": 2, "Cost":10 } }
                    // The server relays it to the opponent, or if you're the opponent receiving it:
                    // We parse "BalloonHealth"
                    JObject dataObj = (JObject)messageObject["Data"];
                    int balloonHealth = (int)dataObj["BalloonHealth"];
                    // int cost = (int)dataObj["Cost"]; // might not be needed on receiving side

                    // Now spawn that balloon locally
                    // If you're the local client receiving this, do:
                    UnityMainThreadDispatcher.Instance().Enqueue(() =>
                    {
                        GameManager.Instance.SpawnOpponentBalloon(balloonHealth);
                    });
                    break;
                }

            case "GameSnapshot":
                // Deserialize into GameSnapshotMessage
                GameSnapshotMessage snapshotMessage = messageObject.ToObject<GameSnapshotMessage>();

                // Extract data
                string imageData = snapshotMessage.Data.ImageData;
                //For testing:
                //string imageData = "H4sIAAAAAAAACusM8HPn5ZLiYmBg4PX0cAkC0l+AuJODCUhO+di0jYGBabani2NIBePbS4y8DAo8LIYHBJ0+NNc09Xo/f5A/9/edhyIaYhFHA800P3AfOOu4jeHOzetSbObMN5kK0uOzJe7wpHAfmFu7NrHAeIIuQ/m3rW0HzzqMKhpVRA1FHxjaDzJxbLHdHAtMrQyern4u65wSmgBodMdY1AIAAA==";
                Debug.Log("imageData: " + imageData);


                // Update opponent snapshot
                UnityMainThreadDispatcher.Instance().Enqueue(() =>
                {
                    UIManager.Instance.UpdateOpponentSnapshot(imageData);
                });
                break;

            case "ShowSnapshots":
                // Tells *THIS CLIENT* to start capturing snapshots
                UnityMainThreadDispatcher.Instance().Enqueue(() =>
                {
                    GameManager.Instance.snapshotManager.EnableSnapshotSending(true);
                    //GameManager.Instance.EnableSnapshotSending(true);
                });
                break;

            case "HideSnapshots":
                // Tells *THIS CLIENT* to stop capturing snapshots
                UnityMainThreadDispatcher.Instance().Enqueue(() =>
                {
                    GameManager.Instance.snapshotManager.EnableSnapshotSending(false);
                    //GameManager.Instance.EnableSnapshotSending(false);
                });
                break;

            case "StartNextWave":
                {
                    int waveIndex = (int)messageObject["WaveIndex"];
                    UnityMainThreadDispatcher.Instance().Enqueue(() =>
                    {
                        // forcibly set wave index and spawn wave
                        // note: StartNextWave method can accept the waveIndex
                        Debug.Log("From NetworkManager - StartNextWave!!!!!!!");
                        BalloonSpawner.Instance.StartCoroutine(BalloonSpawner.Instance.StartNextWave(waveIndex));
                    });
                    break;
                }

            case "OpponentDisconnected":
                UnityMainThreadDispatcher.Instance().Enqueue(() =>
                {
                    // The other player disconnected, we automatically win
                    GameManager.Instance.flowController.OnOpponentGameOver(opponentWon: false, reason: "The other player disconnected");
                    //GameManager.Instance.OnOpponentGameOver(opponentWon: false, reason: "The other player disconnected");
                });
                break;

            case "UseMultiplayerAbility":
                {
                    JObject dataObj = (JObject)messageObject["Data"];
                    string abilityName = dataObj["AbilityName"].ToString();

                    // Check if there's a "FromOpponent" or something
                    bool fromOpponent = false;
                    if (dataObj["FromOpponent"] != null)
                    {
                        fromOpponent = (bool)dataObj["FromOpponent"];
                    }

                    // If we have an opponent, or we are the opponent...
                    // Possibly check: if (opponent != null) forward... 
                    // But let's see:
                    if (GameManager.Instance.CurrentGameMode == GameManager.GameMode.Multiplayer)
                    {
                        // We'll do a local function to handle the effect
                        UnityMainThreadDispatcher.Instance().Enqueue(() =>
                        {
                            // If the ability is "NoMoneyForOpponent" or "CloudScreen" and we see "FromOpponent" is not set => we must set it and forward to the opponent
                            // But typically, the server will do that for us. 
                            // If we are the *receiving* side of "NoMoneyForOpponent", we do the effect. 
                            OnMultiplayerAbilityReceived(abilityName);
                        });
                    }

                    break;
                }

            case "GameOver":
                // Deserialize into GameOverMessage
                GameOverMessage gameOverMessage = messageObject.ToObject<GameOverMessage>();

                // Extract data
                bool opponentWon = gameOverMessage.Data.Won;

                // Handle opponent game over
                UnityMainThreadDispatcher.Instance().Enqueue(() =>
                {
                    GameManager.Instance.flowController.OnOpponentGameOver(opponentWon);
                    //GameManager.Instance.OnOpponentGameOver(opponentWon);
                });
                break;

            case "LoginSuccess":
                {
                    JObject d = (JObject)messageObject["Data"];

                    int userId = d["UserId"]?.ToObject<int>() ?? -1;
                    if (userId != -1)
                    {
                        // store in PlayerPrefs or a static field
                        PlayerPrefs.SetInt("UserId", userId);
                    }


                    string accessToken = d["AccessToken"].ToString();
                    string accessTokenExpiry = d["AccessTokenExpiry"].ToString();
                    string refreshToken = d["RefreshToken"].ToString();
                    string refreshTokenExpiry = d["RefreshTokenExpiry"].ToString();

                    // Save to PlayerPrefs
                    PlayerPrefs.SetString("AccessToken", accessToken);
                    PlayerPrefs.SetString("AccessTokenExpiry", accessTokenExpiry);
                    PlayerPrefs.SetString("RefreshToken", refreshToken);
                    PlayerPrefs.SetString("RefreshTokenExpiry", refreshTokenExpiry);
                    PlayerPrefs.Save();

                    // Notify the LoginSceneManager if it’s present
                    LoginSceneManager lsm = FindObjectOfType<LoginSceneManager>();
                    if (lsm != null)
                    {
                        // e.g. lsm.OnLoginSuccess();
                        UnityMainThreadDispatcher.Instance().Enqueue(() =>
                            lsm.OnLoginSuccess()
                        );
                    }
                    break;
                }
            case "LoginFail":
                {
                    // { "Type":"LoginFail","Data":{"Reason":"..."} }
                    string reason = messageObject["Data"]["Reason"].ToString();
                    LoginSceneManager lsm = FindObjectOfType<LoginSceneManager>();
                    if (lsm != null)
                    {
                        UnityMainThreadDispatcher.Instance().Enqueue(() =>
                            lsm.OnLoginFail(reason)
                        );
                    }
                    break;
                }
            case "RegisterSuccess":
                {
                    JObject d = (JObject)messageObject["Data"];

                    int userId = d["UserId"]?.ToObject<int>() ?? -1;
                    if (userId != -1)
                    {
                        PlayerPrefs.SetInt("UserId", userId);
                    }


                    string accessToken = d["AccessToken"].ToString();
                    string accessTokenExpiry = d["AccessTokenExpiry"].ToString();
                    string refreshToken = d["RefreshToken"].ToString();
                    string refreshTokenExpiry = d["RefreshTokenExpiry"].ToString();

                    // Store in PlayerPrefs
                    PlayerPrefs.SetString("AccessToken", accessToken);
                    PlayerPrefs.SetString("AccessTokenExpiry", accessTokenExpiry);
                    PlayerPrefs.SetString("RefreshToken", refreshToken);
                    PlayerPrefs.SetString("RefreshTokenExpiry", refreshTokenExpiry);
                    PlayerPrefs.Save();

                    // Notify 
                    LoginSceneManager lsm = FindObjectOfType<LoginSceneManager>();
                    if (lsm != null)
                    {
                        UnityMainThreadDispatcher.Instance().Enqueue(() =>
                            lsm.OnRegisterSuccess()
                        );
                    }
                    break;
                }
            case "RegisterFail":
                {
                    // { "Type":"RegisterFail","Data":{"Reason":"..."} }
                    string reason = messageObject["Data"]["Reason"].ToString();
                    LoginSceneManager lsm = FindObjectOfType<LoginSceneManager>();
                    if (lsm != null)
                    {
                        UnityMainThreadDispatcher.Instance().Enqueue(() =>
                            lsm.OnRegisterFail(reason)
                        );
                    }
                    break;
                }

            case "AutoLoginSuccess":
                {
                    JObject d = (JObject)messageObject["Data"];
                    int userId = d["UserId"].ToObject<int>();

                    string newAccessToken = d["AccessToken"]?.ToString();
                    string newAccessTokenExpiry = d["AccessTokenExpiry"]?.ToString();
                    string newRefreshToken = d["RefreshToken"]?.ToString();
                    string newRefreshTokenExpiry = d["RefreshTokenExpiry"]?.ToString();

                    Debug.Log("[CLIENT] AutoLoginSuccess => userId=" + userId);

                    // Optionally store in PlayerPrefs so we have it
                    PlayerPrefs.SetInt("UserId", userId);


                    // Store updated tokens
                    if (!string.IsNullOrEmpty(newAccessToken))
                        PlayerPrefs.SetString("AccessToken", newAccessToken);
                    if (!string.IsNullOrEmpty(newAccessTokenExpiry))
                        PlayerPrefs.SetString("AccessTokenExpiry", newAccessTokenExpiry);
                    if (!string.IsNullOrEmpty(newRefreshToken))
                        PlayerPrefs.SetString("RefreshToken", newRefreshToken);
                    if (!string.IsNullOrEmpty(newRefreshTokenExpiry))
                        PlayerPrefs.SetString("RefreshTokenExpiry", newRefreshTokenExpiry);


                    PlayerPrefs.Save();

                    // We must skip the LoginScene => call the method to load MainMenu
                    var loginSceneManager = FindObjectOfType<LoginSceneManager>();
                    if (loginSceneManager != null)
                    {
                        UnityMainThreadDispatcher.Instance().Enqueue(() =>
                        {
                            // "GoToMainMenu()" or "LoadMainMenu()" in the login manager
                            loginSceneManager.GoToMainMenu();
                        });
                    }
                    break;
                }

            case "AutoLoginFail":
                {
                    string reason = messageObject["Data"]["Reason"].ToString();
                    Debug.LogWarning("[CLIENT] AutoLoginFail => " + reason);

                    // Show login panel
                    var loginSceneManager = FindObjectOfType<LoginSceneManager>();
                    if (loginSceneManager != null)
                    {
                        UnityMainThreadDispatcher.Instance().Enqueue(() =>
                        {
                            loginSceneManager.ShowLoginPanel();
                        });
                    }
                    break;
                }


            // Handle other message types
            default:
                Debug.LogWarning("Unknown message type received: " + messageType);
                break;
        }
    }

    /// <summary>
    /// Sends a JSON string *un*encrypted with length prefix (used in handshake).
    /// </summary>
    private void SendRaw(string plainMsg)
    {
        if (!isConnected) return;

        byte[] bytes = Encoding.UTF8.GetBytes(plainMsg);
        byte[] len = BitConverter.GetBytes(bytes.Length);
        stream.Write(len, 0, len.Length);
        stream.Write(bytes, 0, bytes.Length);

        Debug.Log("[CLIENT] Sent RAW: " + plainMsg);
    }

    /// <summary>
    /// For normal post-handshake messages, we AES-encrypt them.
    /// </summary>
    public void SendMessageWithLengthPrefix(string plainJson)
    {
        if (!isConnected || !handshakeCompleted) return;

        string cipherBase64 = AesEncryptionClient.EncryptAES(plainJson, aesKey, aesIV);
        byte[] messageBytes = Encoding.UTF8.GetBytes(cipherBase64);

        byte[] lengthBytes = BitConverter.GetBytes(messageBytes.Length);
        stream.Write(lengthBytes, 0, lengthBytes.Length);
        stream.Write(messageBytes, 0, messageBytes.Length);

        Debug.Log("[CLIENT] Sent ENCRYPTED: " + plainJson);
    }



    private void OnMultiplayerAbilityReceived(string abilityName)
    {
        switch (abilityName)
        {
            case "NoMoneyForOpponent":
                // That means *we* are the ones who got locked from money => For 10s set moneyMultiplier=0 or disallowMoney
                StartCoroutine(NoMoneyRoutine());
                break;

            case "CloudScreen":
                // Show the cloud panel for 10s
                StartCoroutine(CloudScreenRoutine());
                break;

            case "FastBalloons":
                StartCoroutine(FastBalloonsRoutineOpponent());
                break;

            default:
                Debug.Log("Unknown or no effect needed for: " + abilityName);
                break;
        }
    }

    private IEnumerator NoMoneyRoutine()
    {
        Debug.Log("We are receiving no money for 10s!");
        // store old multiplier
        float oldMultiplier = GameManager.Instance.moneyMultiplier;
        // or define a separate bool "disallowMoney"
        GameManager.Instance.moneyMultiplier = 0f;

        yield return new WaitForSeconds(10f);

        // revert
        GameManager.Instance.moneyMultiplier = oldMultiplier;
    }

    private IEnumerator CloudScreenRoutine()
    {
        Debug.Log("CloudScreen effect for 10s!");
        // Suppose you have a "CloudPanel" in the scene

        SpecialAbilitiesManager.Instance.cloudPanel.SetActive(true);

        yield return new WaitForSeconds(10f);

        SpecialAbilitiesManager.Instance.cloudPanel.SetActive(false);
    }

    private IEnumerator FastBalloonsRoutineOpponent()
    {
        Debug.Log("Opponent's FastBalloons effect - wave speed +50% for 10s");

        GameManager.Instance.allBalloonsSpeedFactor = 1.5f;

        yield return new WaitForSeconds(10f);

        // revert
        GameManager.Instance.allBalloonsSpeedFactor = 1f;
    }


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

    public void SendAuthenticatedMessage(string messageType, JObject data)
    {
        // 1) Construct the JSON with tokens
        JObject finalMsg = BuildMessageWithToken(messageType, data);

        // 2) Convert to string & send via AES
        string plainJson = finalMsg.ToString();
        SendMessageWithLengthPrefix(plainJson);
    }


    public void LoginUser(string username, string password)
    {
        if (!isConnected)
        {
            Debug.LogWarning("[CLIENT] Not connected to server. Can't login.");
            return;
        }

        // Build JSON
        // e.g. { "Type":"LoginUser", "Data": { "Username":"...", "Password":"..." } }

        //JObject msg = new JObject
        //{
        //    ["Type"] = "LoginUser"
        //};
        JObject dataObj = new JObject
        {
            ["Username"] = username,
            ["Password"] = password
        };
        //msg["Data"] = dataObj;


        SendAuthenticatedMessage("LoginUser", dataObj);

        //SendMessageWithLengthPrefix(msg.ToString());
    }

    public void RegisterUser(string username, string password)
    {
        if (!isConnected)
        {
            Debug.LogWarning("[CLIENT] Not connected to server. Can't register.");
            return;
        }

        //JObject msg = new JObject
        //{
        //    ["Type"] = "RegisterUser"
        //};
        JObject dataObj = new JObject
        {
            ["Username"] = username,
            ["Password"] = password
        };
        //msg["Data"] = dataObj;

        SendAuthenticatedMessage("RegisterUser", dataObj);

        //SendMessageWithLengthPrefix(msg.ToString());
    }

    public void SendUpdateLastLogin(int userId)
    {
        if (!isConnected || !handshakeCompleted)
        {
            Debug.LogWarning("[CLIENT] Can't send UpdateLastLogin - not connected or handshake incomplete.");
            return;
        }

        // Build JSON
        //JObject msg = new JObject
        //{
        //    ["Type"] = "UpdateLastLogin"
        //};
        JObject dataObj = new JObject
        {
            ["UserId"] = userId
        };
        //msg["Data"] = dataObj;

        SendAuthenticatedMessage("UpdateLastLogin", dataObj);

        //SendMessageWithLengthPrefix(msg.ToString());
        Debug.Log("[CLIENT] Sent UpdateLastLogin with userId=" + userId);
    }


    public void SendGameOverDetailed(
        int user1Id,
        int? user2Id,
        string mode,
        int? wonUserId,
        int finalWave,
        int timePlayed
    )
    {
        if (!isConnected || !handshakeCompleted) return;

        //JObject msg = new JObject
        //{
        //    ["Type"] = "GameOverDetailed"
        //};

        JObject dataObj = new JObject
        {
            ["User1Id"] = user1Id,
            ["User2Id"] = user2Id,  // or null
            ["Mode"] = mode,
            ["WonUserId"] = wonUserId, // or null
            ["FinalWave"] = finalWave,
            ["TimePlayed"] = timePlayed
        };
        //msg["Data"] = dataObj;

        SendAuthenticatedMessage("GameOverDetailed", dataObj);

        //SendMessageWithLengthPrefix(msg.ToString());
        Debug.Log($"[CLIENT] Sent GameOverDetailed with user1Id={user1Id}, user2Id={user2Id}, finalWave={finalWave}, timePlayed={timePlayed}");
    }


    public void SendAutoLogin(string accessToken, string refreshToken)
    {
        Debug.Log("[CLIENT] First and foremost!!!");
        if (!isConnected || !handshakeCompleted)
        {
            Debug.Log("[CLIENT] Not connected/handshake not done, can't AutoLogin");
            Debug.LogWarning("[CLIENT] Not connected/handshake not done, can't AutoLogin");
            return;
        }
        Debug.LogWarning("[CLIENT] handshake is done, can try to AutoLogin");

        //JObject msg = new JObject
        //{
        //    ["Type"] = "AutoLogin"
        //};
        JObject dataObj = new JObject
        {
            ["AccessToken"] = accessToken,
            ["RefreshToken"] = refreshToken,
        };
        //msg["Data"] = dataObj;

        SendAuthenticatedMessage("AutoLogin", dataObj);

        //SendMessageWithLengthPrefix(msg.ToString());
        Debug.Log("[CLIENT] Sent AutoLogin with token length=" + accessToken.Length);
    }



    public void RequestMatchmaking()
    {
        IsMatchmakingRequested = true;

        //string message = "{\"Type\":\"MatchmakingRequest\"}";

        SendAuthenticatedMessage("MatchmakingRequest", null);

        //SendMessageWithLengthPrefix(message);
    }

    void OnApplicationQuit()
    {
        int userId = PlayerPrefs.GetInt("UserId", -1);
        if (userId != -1 && isConnected)
        {
            SendUpdateLastLogin(userId);
            // Possibly wait briefly, but there's no guarantee it finishes sending
        }

        // Clean up
        if (stream != null)
            stream.Close();
        if (clientSocket != null)
            clientSocket.Close();
        if (clientThread != null)
            clientThread.Abort();
    }

    public void DisconnectAndQuit()
    {
        Debug.Log("Trying to disconnect...");
        // Disconnect from server
        if (isConnected)
        {
            // Close streams and sockets if needed
            if (stream != null)
                stream.Close();
            if (clientSocket != null)
                clientSocket.Close();
            if (clientThread != null)
                clientThread.Abort();
            isConnected = false;
        }

        // Quit the application
        Application.Quit();
    }


    public void ResetMatchmaking()
    {
        IsMatchmakingRequested = false;
        Debug.Log("NetworkManager configurations have been reset.");
    }
}
