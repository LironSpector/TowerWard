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
            Instance = this;
        else
            Destroy(gameObject);

        DontDestroyOnLoad(gameObject);

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
                // Handle MatchFound message
                UnityMainThreadDispatcher.Instance().Enqueue(() =>
                {
                    Debug.Log("Match found!");
                    GameManager.Instance.OnMatchFound();
                });
                break;

            //case "SendBalloon":
            //    // Deserialize into SendBalloonMessage
            //    SendBalloonMessage sendBalloonMessage = messageObject.ToObject<SendBalloonMessage>();

            //    // Extract data
            //    string balloonType = sendBalloonMessage.Data.BalloonType;
            //    int quantity = sendBalloonMessage.Data.Quantity;

            //    // Spawn opponent balloons
            //    UnityMainThreadDispatcher.Instance().Enqueue(() =>
            //    {
            //        GameManager.Instance.SpawnOpponentBalloons(balloonType, quantity);
            //    });
            //    break;

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









//using UnityEngine;
//using System;
//using System.Net.Sockets;
//using System.Text;
//using System.Threading;
//using System.Threading.Tasks;
////using Microsoft.CSharp.RuntimeBinder;
//using System.Collections.Generic;
//using Newtonsoft.Json;
//using Newtonsoft;



//public class NetworkManager : MonoBehaviour
//{
//    public static NetworkManager Instance;

//    private TcpClient clientSocket;
//    private NetworkStream stream;
//    private byte[] buffer = new byte[4096];

//    private string serverIP = "127.0.0.1"; // Replace with your server's IP address
//    private int port = 5555; // Ensure it matches the server port

//    private Thread clientThread;

//    public bool isConnected = false;

//    public bool IsMatchmakingRequested { get; private set; } = false;

//    void Awake()
//    {
//        // Singleton pattern
//        if (Instance == null)
//            Instance = this;
//        else
//            Destroy(gameObject);

//        DontDestroyOnLoad(gameObject);

//        ConnectToServer(); // Connect immediately
//    }

//    public async void ConnectToServer()
//    {
//        clientSocket = new TcpClient();

//        try
//        {
//            await clientSocket.ConnectAsync(serverIP, port);
//            stream = clientSocket.GetStream();

//            // Start a thread to listen for incoming data
//            clientThread = new Thread(ListenForData);
//            clientThread.IsBackground = true;
//            clientThread.Start();

//            isConnected = true;
//            Debug.Log("Connected to server.");
//        }
//        catch (Exception ex)
//        {
//            Debug.LogError("Failed to connect to server: " + ex.Message);
//            isConnected = false;
//            // Handle reconnection logic or notify the player
//        }
//    }

//    private void ListenForData()
//    {
//        try
//        {
//            while (true)
//            {
//                int bytesRead = stream.Read(buffer, 0, buffer.Length);
//                if (bytesRead == 0)
//                    break; // Connection closed

//                string data = Encoding.UTF8.GetString(buffer, 0, bytesRead);
//                HandleMessage(data);
//            }
//        }
//        catch (Exception ex)
//        {
//            Debug.Log("Disconnected from server: " + ex.Message);
//            isConnected = false;
//            // Handle reconnection logic or notify the player
//        }
//    }

//    private void HandleMessage(string data)
//    {
//        // Decrypt data if encryption is implemented

//        // Deserialize JSON message
//        //dynamic message = Newtonsoft.Json.JsonConvert.DeserializeObject(data);
//        var message = Newtonsoft.Json.JsonConvert.DeserializeObject(data);
//        string messageType = message.Type;

//        //switch (messageType)
//        //{
//        //    //case "MatchFound":
//        //    //    // Notify the game manager that a match is found
//        //    //    GameManager.Instance.OnMatchFound();
//        //    //    break;

//        //    //case "SendBalloon":
//        //    //    // Extract balloon data and spawn balloons
//        //    //    string balloonType = message.Data.BalloonType;
//        //    //    int quantity = message.Data.Quantity;
//        //    //    GameManager.Instance.SpawnOpponentBalloons(balloonType, quantity);
//        //    //    break;

//        //    //case "GameSnapshot":
//        //    //    // Receive and display opponent's snapshot
//        //    //    string imageData = message.Data.ImageData;
//        //    //    UIManager.Instance.UpdateOpponentSnapshot(imageData);
//        //    //    break;

//        //    //case "GameOver":
//        //    //    // Handle game over notification
//        //    //    bool opponentWon = message.Data.Won;
//        //    //    GameManager.Instance.OnOpponentGameOver(opponentWon);
//        //    //    break;

//        //        // Handle other message types

//        //}
//    }


//    public new void SendMessage(string message)
//    {
//        if (!isConnected)
//            return;

//        // Encrypt message if encryption is implemented
//        byte[] bytes = Encoding.UTF8.GetBytes(message);
//        stream.Write(bytes, 0, bytes.Length);
//    }

//    void OnApplicationQuit()
//    {
//        // Clean up
//        if (stream != null)
//            stream.Close();
//        if (clientSocket != null)
//            clientSocket.Close();
//        if (clientThread != null)
//            clientThread.Abort();
//    }

//    public void RequestMatchmaking()
//    {
//        IsMatchmakingRequested = true;

//        string message = "{\"Type\":\"MatchmakingRequest\"}";
//        SendMessage(message);
//    }
//}
















//using UnityEngine;
//using System;
//using System.Net.Sockets;
//using System.Text;
//using System.Threading;
//using System.Threading.Tasks;


//public class NetworkManager : MonoBehaviour
//{
//    public static NetworkManager Instance;

//    private TcpClient clientSocket;
//    private NetworkStream stream;
//    private byte[] buffer = new byte[4096];

//    private string serverIP = "127.0.0.1"; // Replace with your server's IP address
//    private int port = 5555; // Ensure it matches the server port

//    private Thread clientThread;

//    void Awake()
//    {
//        // Singleton pattern
//        if (Instance == null)
//            Instance = this;
//        else
//            Destroy(gameObject);

//        DontDestroyOnLoad(gameObject);

//        ConnectToServer();
//    }

//    void Start()
//    {
//        //ConnectToServer();
//    }

//    //public void ConnectToServer()
//    //{
//    //    clientSocket = new TcpClient();
//    //    clientSocket.Connect(serverIP, port);
//    //    stream = clientSocket.GetStream();

//    //    // Start a thread to listen for incoming data
//    //    clientThread = new Thread(ListenForData);
//    //    clientThread.IsBackground = true;
//    //    clientThread.Start();
//    //}

//    public async void ConnectToServer()
//    {
//        clientSocket = new TcpClient();

//        try
//        {
//            await clientSocket.ConnectAsync(serverIP, port);
//            stream = clientSocket.GetStream();

//            // Start a thread to listen for incoming data
//            clientThread = new Thread(ListenForData);
//            clientThread.IsBackground = true;
//            clientThread.Start();

//            Debug.Log("Connected to server.");
//        }
//        catch (Exception ex)
//        {
//            Debug.LogError("Failed to connect to server: " + ex.Message);
//            // Handle reconnection logic or notify the player
//        }
//    }


//    private void ListenForData()
//    {
//        try
//        {
//            while (true)
//            {
//                int bytesRead = stream.Read(buffer, 0, buffer.Length);
//                if (bytesRead == 0)
//                    break; // Connection closed

//                string data = Encoding.UTF8.GetString(buffer, 0, bytesRead);
//                HandleMessage(data);
//            }
//        }
//        catch (Exception ex)
//        {
//            Debug.Log("Disconnected from server: " + ex.Message);
//        }
//    }

//    private void HandleMessage(string data)
//    {
//        // Decrypt data if encryption is implemented

//        // Deserialize JSON message
//        dynamic message = Newtonsoft.Json.JsonConvert.DeserializeObject(data);
//        string messageType = message.Type;

//        switch (messageType)
//        {
//            case "MatchFound":
//                // Notify the game manager that a match is found
//                GameManager.Instance.OnMatchFound();
//                break;

//            case "SendBalloon":
//                // Extract balloon data and spawn balloons
//                string balloonType = message.Data.BalloonType;
//                int quantity = message.Data.Quantity;
//                GameManager.Instance.SpawnOpponentBalloons(balloonType, quantity);
//                break;

//            case "GameSnapshot":
//                // Receive and display opponent's snapshot
//                string imageData = message.Data.ImageData;
//                UIManager.Instance.UpdateOpponentSnapshot(imageData);
//                break;

//            case "GameOver":
//                // Handle game over notification
//                bool opponentWon = message.Data.Won;
//                GameManager.Instance.OnOpponentGameOver(opponentWon);
//                break;

//                // Handle other message types
//        }
//    }


//    public void SendMessage(string message)
//    {
//        // Encrypt message if encryption is implemented
//        byte[] bytes = Encoding.UTF8.GetBytes(message);
//        stream.Write(bytes, 0, bytes.Length);
//    }

//    void OnApplicationQuit()
//    {
//        // Clean up
//        stream.Close();
//        clientSocket.Close();
//        clientThread.Abort();
//    }

//    public void RequestMatchmaking()
//    {
//        string message = "{\"Type\":\"MatchmakingRequest\"}";
//        SendMessage(message);
//    }

//}
