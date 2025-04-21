using UnityEngine;
using System;
using System.IO;
using System.Net.Sockets;
using System.Text;
using System.Threading;
using System.Threading.Tasks;

/// <summary>
/// Description:
/// Core network manager that connects to the server, spawns a thread to read data,
/// and delegates handshake plus message handling to sub-managers.
/// </summary>
public class NetworkManager : MonoBehaviour
{
    /// <summary>
    /// Singleton instance for global access.
    /// </summary>
    public static NetworkManager Instance;

    [Header("Server Connection")]
    // These values will be loaded from an external configuration file.
    /// <summary>
    /// The server IP address loaded from the configuration file.
    /// </summary>
    private string serverIP;
    /// <summary>
    /// The server port loaded from the configuration file (default is 5555).
    /// </summary>
    private int port = 5555; // Default port; may be overwritten

    /// <summary>
    /// Flag indicating if the client is connected to the server.
    /// </summary>
    [HideInInspector] public bool isConnected = false;
    /// <summary>
    /// Flag indicating if the handshake with the server has been completed.
    /// </summary>
    [HideInInspector] public bool isHandshakeCompleted = false;
    /// <summary>
    /// Flag indicating whether a matchmaking request has been made.
    /// </summary>
    [HideInInspector] public bool IsMatchmakingRequested { get; private set; } = false;

    // Main TCP connection
    /// <summary>
    /// The underlying TCP client socket.
    /// </summary>
    private TcpClient clientSocket;
    /// <summary>
    /// The network stream used to send and receive data.
    /// </summary>
    public NetworkStream stream;
    /// <summary>
    /// Thread dedicated to listening for incoming data.
    /// </summary>
    private Thread clientThread;

    // References to sub-managers
    /// <summary>
    /// Manages encryption for network messages and handshake procedures.
    /// </summary>
    public NetworkEncryptionManager encryptionManager;
    /// <summary>
    /// Handles incoming network messages after decryption.
    /// </summary>
    public NetworkMessageHandler messageHandler;
    /// <summary>
    /// Sends messages to the server.
    /// </summary>
    public NetworkMessageSender messageSender;

    /// <summary>
    /// Loads the external configuration from the StreamingAssets folder.
    /// </summary>
    private void LoadConfig()
    {
        // Construct the path to the configuration file.
        string configPath = Path.Combine(Application.streamingAssetsPath, "config.json");

        if (File.Exists(configPath))
        {
            Debug.Log("config.json file Exists");
            try
            {
                // Read and parse the JSON configuration.
                string jsonText = File.ReadAllText(configPath);
                NetworkConfig config = JsonUtility.FromJson<NetworkConfig>(jsonText);
                serverIP = config.serverIP;
                port = config.port;
                Debug.Log("[NetworkManager] Config loaded: Server IP = " + serverIP + ", Port = " + port);
            }
            catch (Exception ex)
            {
                Debug.LogError("[NetworkManager] Error loading config: " + ex.Message);
                // Use fallback defaults if there is an error.
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

    /// <summary>
    /// Awake is called when the script instance is being loaded.
    /// Sets up the singleton instance, loads configuration, and initiates a connection to the server.
    /// </summary>
    void Awake()
    {
        // Implement the singleton pattern.
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

        // Load external configuration before initiating connection.
        LoadConfig();

        // Automatically initiate a connection to the server at startup.
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
            // Asynchronously connect to the server using the loaded IP and port.
            await clientSocket.ConnectAsync(serverIP, port);
            stream = clientSocket.GetStream();

            // Spawn a background thread to listen for incoming data.
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
    /// Continuously listens for data from the server using a length-prefixed protocol.
    /// Processes handshake messages until the handshake is completed, after which incoming data is decrypted and handled.
    /// </summary>
    private void ListenForData()
    {
        try
        {
            while (true)
            {
                // 1) Read the 4-byte length prefix indicating the message size.
                byte[] lengthBuffer = new byte[4];
                int bytesRead = stream.Read(lengthBuffer, 0, 4);
                if (bytesRead == 0) break; // Connection closed by server.
                int messageLength = BitConverter.ToInt32(lengthBuffer, 0);

                // 2) Read the complete message.
                byte[] messageBuffer = new byte[messageLength];
                int totalBytesRead = 0;
                while (totalBytesRead < messageLength)
                {
                    int read = stream.Read(messageBuffer, totalBytesRead, messageLength - totalBytesRead);
                    if (read == 0) break; // Connection closed.
                    totalBytesRead += read;
                }
                if (totalBytesRead < messageLength) break;

                // 3) Convert the received message into a string.
                string receivedData = Encoding.UTF8.GetString(messageBuffer, 0, totalBytesRead);

                // Determine if this is a handshake message or a regular encrypted message.
                if (!isHandshakeCompleted)
                {
                    // Process handshake message.
                    encryptionManager.HandleHandshakeMessage(receivedData);
                }
                else
                {
                    // Decrypt the message and then delegate to the message handler on the main thread.
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
            // If an exception occurs during reading, assume disconnection.
            UnityMainThreadDispatcher.Instance().Enqueue(() =>
            {
                Debug.Log("[CLIENT] Disconnected: " + ex.Message);
                isConnected = false;
            });
        }
    }

    /// <summary>
    /// Sends a raw (unencrypted) message with a length prefix. This is used during the handshake phase.
    /// </summary>
    /// <param name="plainMsg">The plain text message to be sent.</param>
    public void SendRaw(string plainMsg)
    {
        if (!isConnected) return;
        if (stream == null) return;

        // Encode the message and prefix it with its length.
        byte[] bytes = Encoding.UTF8.GetBytes(plainMsg);
        byte[] len = BitConverter.GetBytes(bytes.Length);
        stream.Write(len, 0, len.Length);
        stream.Write(bytes, 0, bytes.Length);

        Debug.Log("[CLIENT] Sent RAW: " + plainMsg);
    }

    /// <summary>
    /// Requests matchmaking by setting the matchmaking flag and sending an authenticated message to the server.
    /// Typically called from UI or during game flow.
    /// </summary>
    public void RequestMatchmaking()
    {
        IsMatchmakingRequested = true;
        messageSender.SendAuthenticatedMessage("MatchmakingRequest", null);
    }

    /// <summary>
    /// Resets the matchmaking state flag.
    /// </summary>
    public void ResetMatchmaking()
    {
        IsMatchmakingRequested = false;
    }

    /// <summary>
    /// Called when the application quits.
    /// Attempts to send an update to the server and then cleans up all network connections.
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
    /// Forces disconnection from the server and then quits the application.
    /// </summary>
    public void DisconnectAndQuit()
    {
        Debug.Log("Trying to disconnect...");
        Cleanup();
        Application.Quit();
    }

    /// <summary>
    /// Cleans up network resources including the stream, client socket, and listener thread.
    /// </summary>
    private void Cleanup()
    {
        if (stream != null) stream.Close();
        if (clientSocket != null) clientSocket.Close();
        if (clientThread != null) clientThread.Abort();
        isConnected = false;
    }
}
