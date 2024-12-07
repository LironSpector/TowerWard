using System.Collections.Generic;
using System.Collections;
using UnityEngine;
using TMPro;
using UnityEngine.SceneManagement;
using Newtonsoft.Json;
using Newtonsoft.Json.Linq;
using System;
using System.IO;
using System.IO.Compression;


public class GameManager : MonoBehaviour
{
    public static GameManager Instance;

    public int lives = 20;
    public int currency = 100;

    // Store occupied cell positions and associated towers
    public Dictionary<Vector2, Tower> occupiedCells = new Dictionary<Vector2, Tower>();

    // UI Elements
    public TextMeshProUGUI livesText;
    public TextMeshProUGUI currencyText;
    public GameObject gameOverPanel;
    public GameObject winPanel;

    public bool isGameOver = false; // Track if the game is over

    // Multiplayer variables
    public enum GameMode { SinglePlayer, Multiplayer }
    public GameMode CurrentGameMode { get; private set; }

    public GameObject balloonSendingPanel; // Assign in Inspector

    private Coroutine snapshotCoroutine;

    void Awake()
    {
        // Singleton pattern
        if (Instance == null)
            Instance = this;
        else
            Destroy(gameObject);

        //DontDestroyOnLoad(gameObject);
    }

    void Start()
    {
        UpdateUI();
        gameOverPanel.SetActive(false);
        winPanel.SetActive(false);

        // Determine game mode
        if (NetworkManager.Instance.isConnected && NetworkManager.Instance.IsMatchmakingRequested)
        {
            Debug.Log("Yes!!!");
            CurrentGameMode = GameMode.Multiplayer;

            // Since we've already found a match, we can start the game
            if (balloonSendingPanel != null)
            {
                balloonSendingPanel.SetActive(true);
            }
            UIManager.Instance.SetOpponentSnapshotPanel(isMultiplayer: true);

            StartGame();
        }
        else
        {
            Debug.Log("No!!!");
            CurrentGameMode = GameMode.SinglePlayer;
            // Start single-player game immediately

            if (balloonSendingPanel != null)
            {
                //balloonSendingPanel.SetActive(CurrentGameMode == GameMode.Multiplayer);
                balloonSendingPanel.SetActive(false);
                UIManager.Instance.SetOpponentSnapshotPanel(isMultiplayer: false);
            }

            StartGame();
        }
        Debug.Log("CurrentGameMode: " + CurrentGameMode);
    }

    public void OnMatchFound()
    {
        // Called by NetworkManager when a match is found
        if (balloonSendingPanel != null)
        {
            balloonSendingPanel.SetActive(true);
        }

        StartGame();
    }

    private void StartGame()
    {
        // Initialize game components common to both modes
        BalloonSpawner.Instance.StartSpawningWaves();

        if (CurrentGameMode == GameMode.Multiplayer)
        {
            // Start sending snapshots every 5 seconds
            snapshotCoroutine = StartCoroutine(SendSnapshots());
        }
    }

    public void LoseLife()
    {
        if (isGameOver)
            return;

        lives--;
        UpdateUI();

        if (lives <= 0)
        {
            GameOver();
        }
    }

    public void AddCurrency(int amount)
    {
        currency += amount;
        UpdateUI();
    }

    public bool CanAfford(int amount)
    {
        return currency >= amount;
    }

    public void SpendCurrency(int amount)
    {
        currency -= amount;
        UpdateUI();
    }

    void UpdateUI()
    {
        livesText.text = "Lives: " + lives;
        currencyText.text = "Currency: " + currency;
    }

    public void WinGame(string reason)
    {
        if (isGameOver)
            return;

        isGameOver = true;
        winPanel.SetActive(true);

        // Find the reason text object:
        TextMeshProUGUI winReasonText = winPanel.transform.Find("WinMessageText").GetComponent<TextMeshProUGUI>();
        if (winReasonText != null)
        {
            winReasonText.text = reason;
        }


        NetworkManager.Instance.ResetMatchmaking();
        //BalloonSpawner.Instance.ResetSpawnConfigurations();

        Time.timeScale = 0; // Pause the game

        if (CurrentGameMode == GameMode.Multiplayer)
        {
            // Notify the server of the game over
            string message = "{\"Type\":\"GameOver\",\"Data\":{\"Won\":true}}";
            //NetworkManager.Instance.SendMessage(message);
            NetworkManager.Instance.SendMessageWithLengthPrefix(message);
        }
    }

    void GameOver()
    {
        if (isGameOver)
            return;

        isGameOver = true;
        gameOverPanel.SetActive(true);

        NetworkManager.Instance.ResetMatchmaking();
        //BalloonSpawner.Instance.ResetSpawnConfigurations();

        Time.timeScale = 0; // Pause the game

        if (CurrentGameMode == GameMode.Multiplayer)
        {
            if (snapshotCoroutine != null)
            {
                StopCoroutine(snapshotCoroutine);
                snapshotCoroutine = null;
            }

            // Notify the server of the game over
            string message = "{\"Type\":\"GameOver\",\"Data\":{\"Won\":false}}";
            //NetworkManager.Instance.SendMessage(message);
            NetworkManager.Instance.SendMessageWithLengthPrefix(message);
        }
    }

    public void OnOpponentGameOver(bool opponentWon, string reason = null)
    {
        if (isGameOver)
            return;

        if (opponentWon)
        {
            // The opponent won, so we lost
            // show GameOver panel as usual
            GameOver();
        }
        else
        {
            // The opponent lost or disconnected, so we win
            if (string.IsNullOrEmpty(reason))
            {
                // Default reason if not provided, e.g., "You've defeated the other player"
                reason = "You've defeated the other player";
            }
            WinGame(reason);
        }
    }


    // Method to check if a position is occupied, with float tolerance
    public bool IsCellOccupied(Vector2 position)
    {
        return occupiedCells.ContainsKey(position);
    }

    // Method to mark a cell as occupied by a tower
    public void OccupyCell(Vector2 position, Tower tower)
    {
        occupiedCells[position] = tower;
    }

    // Optionally, method to free a cell when a tower is removed (if necessary)
    public void FreeCell(Vector2 position)
    {
        occupiedCells.Remove(position);
    }

    // Method to retrieve the tower at a specific position
    public Tower GetTowerAtPosition(Vector2 position)
    {
        if (occupiedCells.ContainsKey(position))
        {
            return occupiedCells[position];
        }
        return null;
    }

    public void LoadMainMenu()
    {
        Time.timeScale = 1; // Ensure the game is not paused
        SceneManager.LoadScene("MainMenu");
    }

    // Multiplayer Methods

    public void SendBalloonToOpponent(string balloonType, int cost)
    {
        if (isGameOver)
            return;

        if (CurrentGameMode != GameMode.Multiplayer)
        {
            Debug.Log("Cannot send balloons in Single Player mode.");
            return;
        }

        if (CanAfford(cost))
        {
            // Deduct currency
            SpendCurrency(cost);

            // Prepare message
            var message = new SendBalloonMessage
            {
                Type = "SendBalloon",
                Data = new SendBalloonData
                {
                    BalloonType = balloonType
                }
            };

            string jsonMessage = JsonConvert.SerializeObject(message);
            //NetworkManager.Instance.SendMessage(jsonMessage);
            NetworkManager.Instance.SendMessageWithLengthPrefix(jsonMessage);

            Debug.Log($"Sent balloon '{balloonType}' to opponent.");
        }
        else
        {
            Debug.Log("Not enough currency to send balloons.");
            // Optionally display a message to the player
        }
    }

    public void SpawnOpponentBalloon(string balloonType)
    {
        Debug.Log("balloonType: " + balloonType);
        if (isGameOver)
            return;

        // Find the balloon prefab based on the balloonType
        GameObject balloonPrefab = BalloonUtils.Instance.GetBalloonPrefabByName(balloonType);
        //GameObject balloonPrefab = BalloonSpawner.Instance.GetBalloonPrefabByName(balloonType);

        if (balloonPrefab == null)
        {
            Debug.LogError("Balloon prefab not found for type: " + balloonType);
            return;
        }

        // Spawn the balloon
        BalloonSpawner.Instance.SpawnBalloon(balloonPrefab);
    }


    private IEnumerator SendSnapshots()
    {
        while (!isGameOver)
        {
            yield return new WaitForSeconds(5f); // Every 5 seconds
            yield return new WaitForEndOfFrame(); //Check if this line is needed here.
            SendSnapshotToServer();
        }
    }

    private void SendSnapshotToServer()
    {
        Texture2D snapshot = CaptureSnapshot();

        byte[] pngBytes = snapshot.EncodeToPNG();
        Debug.Log("------ Image bytes - (beginning): ---------> " + BitConverter.ToString(pngBytes));


        // Compress the PNG data
        byte[] compressedBytes = CompressData(pngBytes);
        string imageData = Convert.ToBase64String(compressedBytes);
        //string imageData = Convert.ToBase64String(pngBytes);
        Debug.Log("Length comparison: ---------> " + pngBytes.Length + ", " + compressedBytes.Length);
        Debug.Log("Image Data initialy is: " + imageData);

        var snapshotMessage = new GameSnapshotMessage
        {
            Type = "GameSnapshot",
            Data = new GameSnapshotData { ImageData = imageData }
        };

        string jsonMessage = JsonConvert.SerializeObject(snapshotMessage);
        NetworkManager.Instance.SendMessageWithLengthPrefix(jsonMessage);
    }

    private byte[] CompressData(byte[] data)
    {
        using (var output = new MemoryStream())
        {
            using (var gzip = new GZipStream(output, System.IO.Compression.CompressionLevel.Optimal))
            {
                gzip.Write(data, 0, data.Length);
            }
            return output.ToArray();
        }
    }

    private Texture2D CaptureSnapshot()
    {
        // Capture a portion of the screen or the entire game view.
        // For simplicity, capture the entire screen:
        int width = Screen.width; // Adjust as necessary
        int height = Screen.height;
        Texture2D tex = new Texture2D(width, height, TextureFormat.RGB24, false);
        tex.ReadPixels(new Rect(0, 0, width, height), 0, 0);
        tex.Apply();
        return tex;
    }

}
