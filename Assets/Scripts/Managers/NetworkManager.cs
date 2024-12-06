using UnityEngine;
using System;
using System.Net.Sockets;
using System.Text;
using System.Threading;
using System.Threading.Tasks;
using Newtonsoft.Json; // Make sure to import Newtonsoft.Json
using Newtonsoft.Json.Linq;

public class NetworkManager : MonoBehaviour
{
    public static NetworkManager Instance;

    private TcpClient clientSocket;
    private NetworkStream stream;
    private byte[] buffer = new byte[4096];

    private string serverIP = "127.0.0.1"; // Replace with your server's IP address
    private int port = 5555; // Ensure it matches the server port

    private Thread clientThread;

    public bool isConnected = false;
    public bool IsMatchmakingRequested { get; private set; } = false;

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

        //DontDestroyOnLoad(gameObject);

        //Connect to the server only in the first time the MainMenu is shown (at entering the game in the first time and not also when returning to the MainMenu scene).
        if (NetworkManager.Instance.isConnected == false)
            ConnectToServer(); // Connect immediately

        //ConnectToServer(); // Connect immediately


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

    private void ListenForData()
    {
        try
        {
            while (true)
            {
                int bytesRead = stream.Read(buffer, 0, buffer.Length);
                //Debug.Log($"bytesRead: {bytesRead}");
                if (bytesRead == 0)
                    break; // Connection closed

                string data = Encoding.UTF8.GetString(buffer, 0, bytesRead);
                //Debug.Log($"data - bytesRead: {data}");

                //HandleMessage(data);

                UnityMainThreadDispatcher.Instance().Enqueue(() => HandleMessage(data));
            }
        }
        catch (Exception ex)
        {
            //Debug.Log("Disconnected from server: " + ex.Message);
            //isConnected = false;

            UnityMainThreadDispatcher.Instance().Enqueue(() =>
            {
                Debug.Log("Disconnected from server: " + ex.Message);
                isConnected = false;
            });

            // Handle reconnection logic or notify the player
        }
    }

    //private void HandleMessage(string data)
    //{
    //    // Decrypt data if encryption is implemented

    //    // Deserialize JSON message
    //    dynamic message = JsonConvert.DeserializeObject(data);
    //    string messageType = message.Type;


    //    switch (messageType)
    //    {
    //        case "MatchFound":
    //            // Notify the game manager that a match is found
    //            UnityMainThreadDispatcher.Instance().Enqueue(() =>
    //            {
    //                GameManager.Instance.OnMatchFound();
    //            });
    //            break;

    //        //case "SendBalloon":
    //        //    // Extract balloon data and spawn balloons
    //        //    string balloonType = message.Data.BalloonType;
    //        //    int quantity = message.Data.Quantity;
    //        //    GameManager.Instance.SpawnOpponentBalloons(balloonType, quantity);
    //        //    break;

    //        //case "GameSnapshot":
    //        //    // Receive and display opponent's snapshot
    //        //    string imageData = message.Data.ImageData;
    //        //    UIManager.Instance.UpdateOpponentSnapshot(imageData);
    //        //    break;

    //        case "GameOver":
    //            // Handle game over notification
    //            bool opponentWon = message.Data.Won;
    //            GameManager.Instance.OnOpponentGameOver(opponentWon);
    //            break;

    //        // Handle other message types
    //        default:
    //            Debug.LogWarning("Unknown message type received: " + messageType);
    //            break;
    //    }
    //}


    private void HandleMessage(string data)
    {
        // Decrypt data if encryption is implemented
        string decryptedData = data; // Assuming data is already decrypted

        // Parse the JSON into a JObject
        JObject messageObject = JObject.Parse(decryptedData);

        // Extract the message type
        string messageType = messageObject["Type"].ToString();

        switch (messageType)
        {
            case "MatchFound":
                //// Handle MatchFound message
                //UnityMainThreadDispatcher.Instance().Enqueue(() =>
                //{
                //    Debug.Log("Match found!");
                //    Debug.Log("Game Manager: " + GameManager.Instance);

                //    if (GameManager.Instance == null)
                //    {
                //        Debug.LogWarning("GameManager.Instance is null. Attempting to find it...");
                //        GameManager.Instance = FindObjectOfType<GameManager>();

                //        if (GameManager.Instance == null)
                //        {
                //            Debug.LogError("GameManager could not be found in the scene!");
                //            return; // Avoid calling OnMatchFound if GameManager still doesn't exist
                //        }
                //    }

                //    GameManager.Instance.OnMatchFound();
                //});
                //break;



                // Handle MatchFound message
                UnityMainThreadDispatcher.Instance().Enqueue(() =>
                {
                    Debug.Log("Match found!");
                    // Check if MatchmakingManager exists
                    if (MatchmakingManager.Instance != null)
                    {
                        MatchmakingManager.Instance.OnMatchFound();
                    }
                    else if (GameManager.Instance != null)
                    {
                        GameManager.Instance.OnMatchFound();
                    }
                    else
                    {
                        Debug.LogError("No handler found for MatchFound message.");
                    }
                });
                break;

            case "SendBalloon":
                // Deserialize into SendBalloonMessage
                SendBalloonMessage sendBalloonMessage = messageObject.ToObject<SendBalloonMessage>();

                // Extract data
                string balloonType = sendBalloonMessage.Data.BalloonType;

                // Spawn opponent balloons
                UnityMainThreadDispatcher.Instance().Enqueue(() =>
                {
                    GameManager.Instance.SpawnOpponentBalloon(balloonType);
                });
                break;


            //case "GameSnapshot":
            //    // Deserialize into GameSnapshotMessage
            //    GameSnapshotMessage snapshotMessage = messageObject.ToObject<GameSnapshotMessage>();

            //    // Extract data
            //    string imageData = snapshotMessage.Data.ImageData;

            //    // Update opponent snapshot
            //    UnityMainThreadDispatcher.Instance().Enqueue(() =>
            //    {
            //        UIManager.Instance.UpdateOpponentSnapshot(imageData);
            //    });
            //    break;


            case "OpponentDisconnected":
                UnityMainThreadDispatcher.Instance().Enqueue(() =>
                {
                    // The other player disconnected, we automatically win
                    GameManager.Instance.OnOpponentGameOver(opponentWon: false, reason: "The other player disconnected");
                });
                break;

            case "GameOver":
                // Deserialize into GameOverMessage
                GameOverMessage gameOverMessage = messageObject.ToObject<GameOverMessage>();

                // Extract data
                bool opponentWon = gameOverMessage.Data.Won;

                // Handle opponent game over
                UnityMainThreadDispatcher.Instance().Enqueue(() =>
                {
                    GameManager.Instance.OnOpponentGameOver(opponentWon);
                });
                break;

            // Handle other message types
            default:
                Debug.LogWarning("Unknown message type received: " + messageType);
                break;
        }
    }



    public new void SendMessage(string message)
    {
        if (!isConnected)
            return;

        // Encrypt message if encryption is implemented
        byte[] bytes = Encoding.UTF8.GetBytes(message);
        stream.Write(bytes, 0, bytes.Length);
        Debug.Log("Message: " + message);
    }

    public void RequestMatchmaking()
    {
        IsMatchmakingRequested = true;

        string message = "{\"Type\":\"MatchmakingRequest\"}";
        SendMessage(message);
    }

    void OnApplicationQuit()
    {
        // Clean up
        if (stream != null)
            stream.Close();
        if (clientSocket != null)
            clientSocket.Close();
        if (clientThread != null)
            clientThread.Abort();
    }

    public void ResetMatchmaking()
    {
        IsMatchmakingRequested = false;
        Debug.Log("NetworkManager configurations have been reset.");
    }
}
