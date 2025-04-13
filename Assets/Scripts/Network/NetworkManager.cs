using UnityEngine;
using System;
using System.IO;
using System.Net.Sockets;
using System.Text;
using System.Threading;
using System.Threading.Tasks;

/// <summary>
/// Core network manager that connects to the server, spawns a thread to read data,
/// and delegates handshake + message handling to sub-managers.
/// </summary>
public class NetworkManager : MonoBehaviour
{
    public static NetworkManager Instance;

    [Header("Server Connection")]
    // These will be loaded from the config file.
    private string serverIP;
    private int port = 5555; // Default port; may be overwritten

    [HideInInspector] public bool isConnected = false;
    [HideInInspector] public bool isHandshakeCompleted = false;
    [HideInInspector] public bool IsMatchmakingRequested { get; private set; } = false;

    // Main TCP connection
    private TcpClient clientSocket;
    public NetworkStream stream;
    private Thread clientThread;

    // References to sub-managers
    public NetworkEncryptionManager encryptionManager;
    public NetworkMessageHandler messageHandler;
    public NetworkMessageSender messageSender;

    /// <summary>
    /// Loads the external configuration from the StreamingAssets folder.
    /// </summary>
    private void LoadConfig()
    {
        // Construct the path to the config file in StreamingAssets
        string configPath = Path.Combine(Application.streamingAssetsPath, "config.json");

        if (File.Exists(configPath))
        {
            Debug.Log("config.json file Exists");
            try
            {
                string jsonText = File.ReadAllText(configPath);
                NetworkConfig config = JsonUtility.FromJson<NetworkConfig>(jsonText);
                serverIP = config.serverIP;
                port = config.port;
                Debug.Log("[NetworkManager] Config loaded: Server IP = " + serverIP + ", Port = " + port);
            }
            catch (Exception ex)
            {
                Debug.LogError("[NetworkManager] Error loading config: " + ex.Message);
                // Fallback to defaults if there is a problem
                serverIP = "127.0.0.1";
                port = 5555;
            }
        }
        else
        {
            Debug.LogWarning("[NetworkManager] Config file not found in StreamingAssets. Using default values.");
            serverIP = "127.0.0.1";
            port = 5555;
        }
    }

    void Awake()
    {
        // Singleton pattern
        if (Instance == null)
        {
            Instance = this;
            DontDestroyOnLoad(gameObject);
        }
        else
        {
            Destroy(gameObject);
            return;
        }

        // Load external configuration before connecting.
        LoadConfig();

        // Connect automatically right when the game starts. This is the only time a connection is initiated between the current client and the server.
        if (!isConnected)
        {
            ConnectToServer();
        }

        // Ensure the main thread dispatcher is loaded.
        UnityMainThreadDispatcher.Instance();
    }

    /// <summary>
    /// Initiates the TCP connection with the server asynchronously.
    /// </summary>
    public async void ConnectToServer()
    {
        clientSocket = new TcpClient();

        try
        {
            await clientSocket.ConnectAsync(serverIP, port);
            stream = clientSocket.GetStream();

            // Spawn a thread to listen for incoming data.
            clientThread = new Thread(ListenForData);
            clientThread.IsBackground = true;
            clientThread.Start();

            isConnected = true;
            Debug.Log("[NetworkManager] Connected to server at " + serverIP + ":" + port);
        }
        catch (Exception ex)
        {
            Debug.LogError("[NetworkManager] Failed to connect: " + ex.Message);
            isConnected = false;
        }
    }

    /// <summary>
    /// Reads data from the server in a loop, using length-prefixed messages.
    /// </summary>
    private void ListenForData()
    {
        try
        {
            while (true)
            {
                // 1) Read the 4-byte length prefix.
                byte[] lengthBuffer = new byte[4];
                int bytesRead = stream.Read(lengthBuffer, 0, 4);
                if (bytesRead == 0) break; // connection closed
                int messageLength = BitConverter.ToInt32(lengthBuffer, 0);

                // 2) Read the message.
                byte[] messageBuffer = new byte[messageLength];
                int totalBytesRead = 0;
                while (totalBytesRead < messageLength)
                {
                    int read = stream.Read(messageBuffer, totalBytesRead, messageLength - totalBytesRead);
                    if (read == 0) break; // closed
                    totalBytesRead += read;
                }
                if (totalBytesRead < messageLength) break;

                // 3) Convert to string.
                string receivedData = Encoding.UTF8.GetString(messageBuffer, 0, totalBytesRead);

                // Handshake or normal message?
                if (!isHandshakeCompleted)
                {
                    // Handshake flow
                    encryptionManager.HandleHandshakeMessage(receivedData);
                }
                else
                {
                    // Decrypt with AES, then handle
                    string decryptedJson = encryptionManager.Decrypt(receivedData);
                    UnityMainThreadDispatcher.Instance().Enqueue(() =>
                    {
                        messageHandler.HandleMessage(decryptedJson);
                    });
                }
            }
        }
        catch (Exception ex)
        {
            // If reading fails, assume disconnected.
            UnityMainThreadDispatcher.Instance().Enqueue(() =>
            {
                Debug.Log("[CLIENT] Disconnected: " + ex.Message);
                isConnected = false;
            });
        }
    }

    /// <summary>
    /// Send a raw (unencrypted) message with length prefix. Used during handshake.
    /// </summary>
    public void SendRaw(string plainMsg)
    {
        if (!isConnected) return;
        if (stream == null) return;

        byte[] bytes = Encoding.UTF8.GetBytes(plainMsg);
        byte[] len = BitConverter.GetBytes(bytes.Length);
        stream.Write(len, 0, len.Length);
        stream.Write(bytes, 0, bytes.Length);

        Debug.Log("[CLIENT] Sent RAW: " + plainMsg);
    }

    /// <summary>
    /// Request matchmaking. We set the property so we know in other parts of the game.
    /// Typically called from UI or game flow.
    /// </summary>
    public void RequestMatchmaking()
    {
        IsMatchmakingRequested = true;
        messageSender.SendAuthenticatedMessage("MatchmakingRequest", null);
    }

    /// <summary>
    /// Reset matchmaking state if needed.
    /// </summary>
    public void ResetMatchmaking()
    {
        IsMatchmakingRequested = false;
        Debug.Log("[NetworkManager] Reset matchmaking flag.");
    }

    /// <summary>
    /// Called by Unity when the application quits.
    /// Closes the connection gracefully if possible.
    /// </summary>
    void OnApplicationQuit()
    {
        int userId = PlayerPrefs.GetInt("UserId", -1);
        if (userId != -1 && isConnected && isHandshakeCompleted)
        {
            messageSender.SendUpdateLastLogin(userId);
        }
        Cleanup();
    }

    /// <summary>
    /// Public method to force disconnection and quit the app.
    /// </summary>
    public void DisconnectAndQuit()
    {
        Debug.Log("Trying to disconnect...");
        Cleanup();
        Application.Quit();
    }

    private void Cleanup()
    {
        if (stream != null) stream.Close();
        if (clientSocket != null) clientSocket.Close();
        if (clientThread != null) clientThread.Abort();
        isConnected = false;
    }
}












////// -------------------- New NetworkManager - before adding config.json --------------------
//using UnityEngine;
//using System;
//using System.Net.Sockets;
//using System.Text;
//using System.Threading;
//using System.Threading.Tasks;

///// <summary>
///// Core network manager that connects to the server, spawns a thread to read data,
///// and delegates handshake + message handling to sub-managers.
///// </summary>
//public class NetworkManager : MonoBehaviour
//{
//    public static NetworkManager Instance;

//    [Header("Server Connection")]
//    //[SerializeField] private string serverIP = "10.100.102.11";
//    //[SerializeField] private string serverIP = "10.100.102.7"; //Make sure to also change it from the Inspector in Unity if it changes
//    private string serverIP = "10.100.102.6"; //Make sure to also change it from the Inspector in Unity if it changes
//    //private string serverIP = "127.0.0.1";
//    [SerializeField] private int port = 5555;

//    [HideInInspector] public bool isConnected = false;
//    [HideInInspector] public bool isHandshakeCompleted = false;
//    [HideInInspector] public bool IsMatchmakingRequested { get; private set; } = false;

//    // Main TCP connection
//    private TcpClient clientSocket;
//    public NetworkStream stream;
//    private Thread clientThread;

//    // References to sub-managers
//    public NetworkEncryptionManager encryptionManager;
//    public NetworkMessageHandler messageHandler;
//    public NetworkMessageSender messageSender;

//    void Awake()
//    {
//        // Singleton pattern
//        if (Instance == null)
//        {
//            Instance = this;
//            DontDestroyOnLoad(gameObject);
//        }
//        else
//        {
//            Destroy(gameObject);
//            return;
//        }

//        // Connect automatically right when the game starts. This is the only time a connection is initiated between the current client and the server.
//        if (!isConnected)
//        {
//            ConnectToServer();
//        }

//        // Ensure main thread dispatcher is loaded
//        UnityMainThreadDispatcher.Instance();
//    }

//    /// <summary>
//    /// Initiates the TCP connection with the server asynchronously.
//    /// </summary>
//    public async void ConnectToServer()
//    {
//        clientSocket = new TcpClient();

//        try
//        {
//            await clientSocket.ConnectAsync(serverIP, port);
//            stream = clientSocket.GetStream();

//            // Spawn a thread to listen for incoming data
//            clientThread = new Thread(ListenForData);
//            clientThread.IsBackground = true;
//            clientThread.Start();

//            isConnected = true;
//            Debug.Log("[NetworkManager] Connected to server.");
//        }
//        catch (Exception ex)
//        {
//            Debug.LogError("[NetworkManager] Failed to connect: " + ex.Message);
//            isConnected = false;
//        }
//    }

//    /// <summary>
//    /// Reads data from the server in a loop, using length-prefixed messages.
//    /// </summary>
//    private void ListenForData()
//    {
//        try
//        {
//            while (true)
//            {
//                // 1) Read 4-byte length
//                byte[] lengthBuffer = new byte[4];
//                int bytesRead = stream.Read(lengthBuffer, 0, 4);
//                if (bytesRead == 0) break; // connection closed
//                int messageLength = BitConverter.ToInt32(lengthBuffer, 0);

//                // 2) Read the message
//                byte[] messageBuffer = new byte[messageLength];
//                int totalBytesRead = 0;
//                while (totalBytesRead < messageLength)
//                {
//                    int read = stream.Read(messageBuffer, totalBytesRead, messageLength - totalBytesRead);
//                    if (read == 0) break; // closed
//                    totalBytesRead += read;
//                }
//                if (totalBytesRead < messageLength) break;

//                // 3) Convert to string
//                string receivedData = Encoding.UTF8.GetString(messageBuffer, 0, totalBytesRead);
//                // Handshake or normal message?
//                if (!isHandshakeCompleted)
//                {
//                    // Handshake flow
//                    encryptionManager.HandleHandshakeMessage(receivedData);
//                }
//                else
//                {
//                    // Decrypt with AES, then handle
//                    string decryptedJson = encryptionManager.Decrypt(receivedData);
//                    UnityMainThreadDispatcher.Instance().Enqueue(() =>
//                    {
//                        messageHandler.HandleMessage(decryptedJson);
//                    });
//                }
//            }
//        }
//        catch (Exception ex)
//        {
//            // If reading fails, assume disconnected
//            UnityMainThreadDispatcher.Instance().Enqueue(() =>
//            {
//                Debug.Log("[CLIENT] Disconnected: " + ex.Message);
//                isConnected = false;
//            });
//        }
//    }

//    /// <summary>
//    /// Send a raw (unencrypted) message with length prefix. Used during handshake.
//    /// </summary>
//    public void SendRaw(string plainMsg)
//    {
//        if (!isConnected) return;
//        if (stream == null) return;

//        byte[] bytes = Encoding.UTF8.GetBytes(plainMsg);
//        byte[] len = BitConverter.GetBytes(bytes.Length);
//        stream.Write(len, 0, len.Length);
//        stream.Write(bytes, 0, bytes.Length);

//        Debug.Log("[CLIENT] Sent RAW: " + plainMsg);
//    }

//    /// <summary>
//    /// Request matchmaking. We set the property so we know in other parts of the game.
//    /// Typically called from UI or game flow.
//    /// </summary>
//    public void RequestMatchmaking()
//    {
//        IsMatchmakingRequested = true;
//        messageSender.SendAuthenticatedMessage("MatchmakingRequest", null);
//    }

//    /// <summary>
//    /// Reset matchmaking state if needed.
//    /// </summary>
//    public void ResetMatchmaking()
//    {
//        IsMatchmakingRequested = false;
//        Debug.Log("[NetworkManager] Reset matchmaking flag.");
//    }

//    /// <summary>
//    /// Called by Unity when the application quits.
//    /// Closes the connection gracefully if possible.
//    /// </summary>
//    void OnApplicationQuit()
//    {
//        int userId = PlayerPrefs.GetInt("UserId", -1);
//        if (userId != -1 && isConnected && isHandshakeCompleted)
//        {
//            messageSender.SendUpdateLastLogin(userId);
//        }
//        Cleanup();
//    }

//    /// <summary>
//    /// Public method to force disconnection and quit the app.
//    /// </summary>
//    public void DisconnectAndQuit()
//    {
//        Debug.Log("Trying to disconnect...");
//        Cleanup();
//        Application.Quit();
//    }

//    private void Cleanup()
//    {
//        if (stream != null) stream.Close();
//        if (clientSocket != null) clientSocket.Close();
//        if (clientThread != null) clientThread.Abort();
//        isConnected = false;
//    }
//}
