using UnityEngine;
using System;
using System.Net.Sockets;
using System.Text;
using System.Threading;
using System.Threading.Tasks;
using Newtonsoft.Json;
using Newtonsoft.Json.Linq;

public class NetworkManager : MonoBehaviour
{
    public static NetworkManager Instance;

    private TcpClient clientSocket;
    private NetworkStream stream;
    private byte[] buffer = new byte[4096];

    private string serverIP = "127.0.0.1";
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
    //            int bytesRead = stream.Read(buffer, 0, buffer.Length);
    //            //Debug.Log($"bytesRead: {bytesRead}");
    //            if (bytesRead == 0)
    //                break; // Connection closed

    //            string data = Encoding.UTF8.GetString(buffer, 0, bytesRead);
    //            //Debug.Log($"data - bytesRead: {data}");

    //            //HandleMessage(data);

    //            UnityMainThreadDispatcher.Instance().Enqueue(() => HandleMessage(data));
    //        }
    //    }
    //    catch (Exception ex)
    //    {
    //        //Debug.Log("Disconnected from server: " + ex.Message);
    //        //isConnected = false;

    //        UnityMainThreadDispatcher.Instance().Enqueue(() =>
    //        {
    //            Debug.Log("Disconnected from server: " + ex.Message);
    //            isConnected = false;
    //        });

    //        // Handle reconnection logic or notify the player
    //    }
    //}


    private void ListenForData()
    {
        try
        {
            while (true)
            {
                // Read length
                byte[] lengthBuffer = new byte[4];
                int bytesRead = stream.Read(lengthBuffer, 0, 4);
                if (bytesRead == 0) break; // Connection closed
                int messageLength = BitConverter.ToInt32(lengthBuffer, 0);

                // Now read the message
                byte[] messageBuffer = new byte[messageLength];
                int totalBytesRead = 0;
                while (totalBytesRead < messageLength)
                {
                    int read = stream.Read(messageBuffer, totalBytesRead, messageLength - totalBytesRead);
                    if (read == 0) break;
                    totalBytesRead += read;
                }

                if (totalBytesRead < messageLength)
                    break; // Connection closed partway through message

                string data = Encoding.UTF8.GetString(messageBuffer, 0, totalBytesRead);
                Debug.Log("Data is: " + data);

                UnityMainThreadDispatcher.Instance().Enqueue(() => HandleMessage(data));
            }
        }
        catch (Exception ex)
        {
            UnityMainThreadDispatcher.Instance().Enqueue(() =>
            {
                Debug.Log("Disconnected from server: " + ex.Message);
                isConnected = false;
            });
        }
    }


    private void HandleMessage(string data)
    {
        // Decrypt data if encryption is implemented
        string decryptedData = data; // Assuming data is already decrypted

        // Parse the JSON into a JObject
        JObject messageObject = JObject.Parse(decryptedData);
        Debug.Log("messageObject: " + messageObject);

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
                Debug.Log("Sending balloon---: " + sendBalloonMessage);

                // Extract data
                string balloonType = sendBalloonMessage.Data.BalloonType;

                // Spawn opponent balloons
                UnityMainThreadDispatcher.Instance().Enqueue(() =>
                {
                    GameManager.Instance.SpawnOpponentBalloon(balloonType);
                });
                break;


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


    //public new void SendMessage(string message)
    //{
    //    if (!isConnected)
    //        return;

    //    // Encrypt message if encryption is implemented
    //    byte[] bytes = Encoding.UTF8.GetBytes(message);
    //    stream.Write(bytes, 0, bytes.Length);
    //    Debug.Log("Message: " + message);
    //}



    //public new void SendMessage(string message)
    //{
    //    byte[] messageBytes = Encoding.UTF8.GetBytes(message);

    //    // Prepend the length of the message as a 4-byte integer
    //    byte[] lengthPrefix = BitConverter.GetBytes(messageBytes.Length);
    //    if (BitConverter.IsLittleEndian)
    //    {
    //        Array.Reverse(lengthPrefix); // Ensure big-endian for consistency
    //    }

    //    // Send the length prefix followed by the actual message
    //    stream.Write(lengthPrefix, 0, lengthPrefix.Length);
    //    stream.Write(messageBytes, 0, messageBytes.Length);
    //}


    public void SendMessageWithLengthPrefix(string message)
    {
        if (!isConnected) return;

        byte[] messageBytes = Encoding.UTF8.GetBytes(message);
        byte[] lengthBytes = BitConverter.GetBytes(messageBytes.Length);

        // Send length first
        stream.Write(lengthBytes, 0, lengthBytes.Length);
        // Then send message
        stream.Write(messageBytes, 0, messageBytes.Length);

        Debug.Log("Sent (with length): " + message);
    }




    public void RequestMatchmaking()
    {
        IsMatchmakingRequested = true;

        string message = "{\"Type\":\"MatchmakingRequest\"}";
        //SendMessage(message);
        SendMessageWithLengthPrefix(message);
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
