//------- After balloon code & behaviour changes: -----------
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
    public float moneyMultiplier = 1f;
    public float allBalloonsSpeedFactor = 1f;
    public float rangeBuffFactor = 1f;
    public float fireRateBuffFactor = 1f;

    // Multiplayer variables
    public enum GameMode { SinglePlayer, Multiplayer }
    public GameMode CurrentGameMode { get; private set; }

    public GameObject balloonSendingPanel; // Assign in Inspector

    public Camera mapCamera; // Assign in Inspector
    public RenderTexture mapRenderTexture; // Assign in Inspector

    private Coroutine snapshotCoroutine = null;
    private bool isSendingSnapshots = false;

    public GameObject toggleMapButton;

    public GameObject universalBalloonPrefab;

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

        AudioManager.Instance.PlayGameMusic();

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
            UIManager.Instance.SetOpponentSnapshotPanel(false);

            StartGame();
        }
        else
        {
            Debug.Log("No!!!");
            CurrentGameMode = GameMode.SinglePlayer;
            // Start single-player game immediately

            if (balloonSendingPanel != null)
            {
                balloonSendingPanel.SetActive(false);
                UIManager.Instance.SetOpponentSnapshotPanel(false);
            }

            // Hide toggle snapshot map button
            toggleMapButton.SetActive(false);

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

    }

    public void LoseLife(int livesToDecrease)
    {
        if (isGameOver)
            return;

        lives -= livesToDecrease;
        if (lives < 0)
            lives = 0;

        UpdateUI();

        if (lives <= 0)
        {
            GameOver();
        }
    }

    public void AddCurrency(int amount)
    {
        //currency += amount;
        // apply multiplier
        int finalAmount = Mathf.RoundToInt(amount * moneyMultiplier);
        currency += finalAmount;
        UpdateUI();

        RefreshButtonsDisplay();
    }

    public bool CanAfford(int amount)
    {
        return currency >= amount;
    }

    public void SpendCurrency(int amount)
    {
        currency -= amount;
        UpdateUI();

        RefreshButtonsDisplay();

    }

    public void RefreshButtonsDisplay()
    {
        // After spending money, refresh tower UI
        TowerSelectionUI towerSelectionUI = FindObjectOfType<TowerSelectionUI>();
        if (towerSelectionUI != null)
        {
            towerSelectionUI.RefreshTowerButtons();
        }

        //// Also refresh balloon panel if desired
        BalloonSendingPanel balloonSendingPanel = FindObjectOfType<BalloonSendingPanel>();
        if (balloonSendingPanel != null)
        {
            balloonSendingPanel.RefreshBalloonButtons();
        }
    }

    // If we want to re-apply stats to all towers (so they pick up the new buff factors)
    public void RefreshAllTowersStats()
    {
        foreach (var kvp in occupiedCells)
        {
            Tower t = kvp.Value;
            t.ApplyLevelStats();
            // For ProjectileTowers, that’ll recalc final range/fireRate using the buff factors
        }
    }

    void UpdateUI()
    {
        livesText.text = lives.ToString();
        currencyText.text = currency.ToString();
    }

    public void WinGame(string reason)
    {
        if (isGameOver)
            return;

        isGameOver = true;
        winPanel.SetActive(true);

        AudioManager.Instance.StopGameMusic();
        AudioManager.Instance.PlayWinMusic();

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
        UIManager.Instance.opponentSnapshotPanel.SetActive(false);
        gameOverPanel.SetActive(true);

        AudioManager.Instance.StopGameMusic();
        AudioManager.Instance.PlayLoseMusic();

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
        Debug.Log("Position to occupy: " + position);
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

    public void SendBalloonToOpponent(int balloonHealth, int cost)
    {
        if (isGameOver) return;
        if (CurrentGameMode != GameMode.Multiplayer)
        {
            Debug.Log("Cannot send balloons in Single Player mode.");
            return;
        }

        if (CanAfford(cost))
        {
            SpendCurrency(cost);

            // Prepare message
            // Instead of "BalloonType", we do "BalloonHealth"
            // We also keep "Cost" if needed, or you might omit it.
            var message = new
            {
                Type = "SendBalloon",
                Data = new
                {
                    BalloonHealth = balloonHealth,
                    Cost = cost
                }
            };

            string jsonMessage = JsonConvert.SerializeObject(message);
            NetworkManager.Instance.SendMessageWithLengthPrefix(jsonMessage);
            //Debug.Log($"Sent balloon with health {balloonHealth} to opponent.");
        }
        else
        {
            Debug.Log("Not enough currency to send balloons.");
        }
    }

    public void SpawnOpponentBalloon(int balloonHealth)
    {
        Debug.Log($"Spawning opponent balloon with health={balloonHealth}");
        if (isGameOver) return;

        // Spawn the universal balloon
        BalloonSpawner.Instance.SpawnExtraBalloon(universalBalloonPrefab, balloonHealth);
    }

    private IEnumerator SendSnapshots()
    {
        // Immediately send one snapshot for better user experience
        yield return new WaitForEndOfFrame();
        SendSnapshotToServer();

        while (isSendingSnapshots && !isGameOver)
        {
            yield return new WaitForSeconds(1f); // Every 5 seconds
            yield return new WaitForEndOfFrame(); //Check if this line is needed here.
            SendSnapshotToServer();
        }
    }

    private void SendSnapshotToServer()
    {
        Texture2D snapshot = CaptureSnapshot();

        byte[] jpgBytes = snapshot.EncodeToJPG(50);


        // Compress the PNG data
        byte[] compressedBytes = CompressData(jpgBytes);
        string imageData = Convert.ToBase64String(compressedBytes);
        //string imageData = Convert.ToBase64String(pngBytes);

        //Debug.Log("Length comparison: ---------> " + jpgBytes.Length + ", " + compressedBytes.Length);
        //Debug.Log("Image Data initialy is: " + imageData);

        var snapshotMessage = new GameSnapshotMessage
        {
            Type = "GameSnapshot",
            Data = new GameSnapshotData { ImageData = imageData }
        };

        string jsonMessage = JsonConvert.SerializeObject(snapshotMessage);
        NetworkManager.Instance.SendMessageWithLengthPrefix(jsonMessage);

        //Works without it, but maybe add this:
        //Destroy(snapshot);
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
        // Set the RenderTexture as active
        RenderTexture currentRT = RenderTexture.active;
        RenderTexture.active = mapRenderTexture;

        // Create a Texture2D with the same size as mapRenderTexture
        Texture2D tex = new Texture2D(mapRenderTexture.width, mapRenderTexture.height, TextureFormat.RGB24, false);
        tex.ReadPixels(new Rect(0, 0, mapRenderTexture.width, mapRenderTexture.height), 0, 0);
        tex.Apply();


        // Restore the active RenderTexture
        RenderTexture.active = currentRT;

        return tex;
    }

    // Called by NetworkManager when we receive "ShowSnapshots" or "HideSnapshots"
    public void EnableSnapshotSending(bool enable)
    {
        if (isGameOver)
            return; // No snapshots if the game is over

        if (enable && !isSendingSnapshots)
        {
            isSendingSnapshots = true;
            snapshotCoroutine = StartCoroutine(SendSnapshots());
        }
        else if (!enable && isSendingSnapshots)
        {
            isSendingSnapshots = false;
            if (snapshotCoroutine != null)
            {
                StopCoroutine(snapshotCoroutine);
                snapshotCoroutine = null;
            }
        }
    }

}
