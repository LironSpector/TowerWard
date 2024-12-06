using System.Collections.Generic;
using UnityEngine;
using TMPro;
using UnityEngine.SceneManagement;
using Newtonsoft.Json; // Make sure to import Newtonsoft.Json
using Newtonsoft.Json.Linq;


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

        //// Additional multiplayer initialization if needed
        //if (CurrentGameMode == GameMode.Multiplayer)
        //{
        //    // Additional initialization for multiplayer
        //    // Start sending snapshots
        //    StartCoroutine(SendSnapshots());
        //}
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

    public void WinGame()
    {
        if (isGameOver)
            return;

        isGameOver = true;
        winPanel.SetActive(true);

        NetworkManager.Instance.ResetMatchmaking();
        //BalloonSpawner.Instance.ResetSpawnConfigurations();

        Time.timeScale = 0; // Pause the game

        if (CurrentGameMode == GameMode.Multiplayer)
        {
            // Notify the server of the game over
            string message = "{\"Type\":\"GameOver\",\"Data\":{\"Won\":true}}";
            NetworkManager.Instance.SendMessage(message);
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
            // Notify the server of the game over
            string message = "{\"Type\":\"GameOver\",\"Data\":{\"Won\":false}}";
            NetworkManager.Instance.SendMessage(message);
        }
    }

    public void OnOpponentGameOver(bool opponentWon)
    {
        if (isGameOver)
            return;

        if (opponentWon)
        {
            // The opponent won, so we lost
            GameOver();
        }
        else
        {
            // The opponent lost, so we won
            WinGame();
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
            NetworkManager.Instance.SendMessage(jsonMessage);

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




    //// Coroutine to send game snapshots periodically
    //public IEnumerator SendSnapshots()
    //{
    //    while (!isGameOver)
    //    {
    //        yield return new WaitForSeconds(5f); // Adjust interval as needed

    //        // Capture snapshot
    //        Texture2D snapshot = CaptureSnapshot();

    //        // Convert to PNG and Base64
    //        byte[] imageBytes = snapshot.EncodeToPNG();
    //        string imageData = Convert.ToBase64String(imageBytes);

    //        // Prepare message
    //        string message = $"{{\"Type\":\"GameSnapshot\",\"Data\":{{\"ImageData\":\"{imageData}\"}}}}";
    //        NetworkManager.Instance.SendMessage(message);
    //    }
    //}

    //private Texture2D CaptureSnapshot()
    //{
    //    // Implement snapshot capture
    //    // Return a Texture2D

    //    // For example:
    //    int width = Screen.width / 4;
    //    int height = Screen.height / 4;
    //    Texture2D tex = new Texture2D(width, height, TextureFormat.RGB24, false);
    //    tex.ReadPixels(new Rect(0, 0, width, height), 0, 0);
    //    tex.Apply();
    //    return tex;
    //}



    //public void SendBalloonToOpponent(string balloonType, int quantity, int cost)
    //{
    //    if (CanAfford(cost))
    //    {
    //        // Deduct currency
    //        SpendCurrency(cost);

    //        // Prepare message
    //        string message = $"{{\"Type\":\"SendBalloon\",\"Data\":{{\"BalloonType\":\"{balloonType}\",\"Quantity\":{quantity}}}}}";
    //        NetworkManager.Instance.SendMessage(message);
    //    }
    //    else
    //    {
    //        Debug.Log("Not enough currency to send balloons.");
    //        // Optionally display a message to the player
    //    }
    //}

}










//using System.Collections.Generic;
//using UnityEngine;
//using TMPro;
//using UnityEngine.UI;
//using UnityEngine.SceneManagement;


//public class GameManager : MonoBehaviour
//{
//    public static GameManager Instance;

//    public int lives = 20;
//    public int currency = 100;

//    // Store occupied cell positions and associated towers
//    public Dictionary<Vector2, Tower> occupiedCells = new Dictionary<Vector2, Tower>();

//    // UI Elements
//    public TextMeshProUGUI livesText;
//    public TextMeshProUGUI currencyText;
//    public GameObject gameOverPanel;
//    public GameObject winPanel;

//    public bool isGameOver = false; // Track if the game is over

//    void Awake()
//    {
//        // Singleton pattern
//        if (Instance == null)
//            Instance = this;
//        else
//            Destroy(gameObject);
//    }

//    void Start()
//    {
//        UpdateUI();
//        gameOverPanel.SetActive(false);
//        winPanel.SetActive(false);
//    }

//    public void LoseLife()
//    {
//        if (isGameOver)
//            return;

//        lives--;
//        UpdateUI();

//        if (lives <= 0)
//        {
//            GameOver();
//        }
//    }

//    public void AddCurrency(int amount)
//    {
//        currency += amount;
//        UpdateUI();
//    }

//    public bool CanAfford(int amount)
//    {
//        return currency >= amount;
//    }

//    public void SpendCurrency(int amount)
//    {
//        currency -= amount;
//        UpdateUI();
//    }

//    void UpdateUI()
//    {
//        livesText.text = "Lives: " + lives;
//        currencyText.text = "Currency: " + currency;
//    }

//    public void WinGame()
//    {
//        if (isGameOver)
//            return;

//        isGameOver = true;
//        winPanel.SetActive(true);
//        Time.timeScale = 0; // Pause the game (the tower's shooting, the projectiles and more)
//    }

//    void GameOver()
//    {
//        if (isGameOver)
//            return;

//        isGameOver = true;
//        gameOverPanel.SetActive(true);
//        Time.timeScale = 0; // Pause the game (the tower's shooting, the projectiles and more)
//    }


//    // Method to check if a position is occupied, with float tolerance
//    public bool IsCellOccupied(Vector2 position)
//    {
//        return occupiedCells.ContainsKey(position);
//    }

//    // Method to mark a cell as occupied by a tower
//    public void OccupyCell(Vector2 position, Tower tower)
//    {
//        occupiedCells[position] = tower;
//    }

//    // Optionally, method to free a cell when a tower is removed (if necessary)
//    public void FreeCell(Vector2 position)
//    {
//        occupiedCells.Remove(position);
//    }

//    // Method to retrieve the tower at a specific position
//    public Tower GetTowerAtPosition(Vector2 position)
//    {
//        if (occupiedCells.ContainsKey(position))
//        {
//            return occupiedCells[position];
//        }
//        return null;
//    }

//    public void LoadMainMenu()
//    {
//        Time.timeScale = 1; // Ensure the game is not paused
//        SceneManager.LoadScene("MainMenu");
//    }
//}
