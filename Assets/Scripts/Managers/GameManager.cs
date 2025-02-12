//Game Manager - after organizing and dividing it to more classes
using UnityEngine;
using TMPro;
using UnityEngine.SceneManagement;
using System.Collections.Generic;
using System.Collections;
using Newtonsoft.Json;
using Newtonsoft.Json.Linq;
using System;
using System.IO;
using System.IO.Compression;

public class GameManager : MonoBehaviour
{
    public static GameManager Instance;

    [Header("Player & Economy")]
    public int lives = 20;
    public int currency = 100;

    [Header("Global Buffs")]
    public float moneyMultiplier = 1f;
    public float allBalloonsSpeedFactor = 1f;
    public float rangeBuffFactor = 1f;
    public float fireRateBuffFactor = 1f;

    [Header("UI & Panels")]
    public TextMeshProUGUI livesText;
    public TextMeshProUGUI currencyText;
    public GameObject gameOverPanel;
    public GameObject winPanel;
    public GameObject balloonSendingPanel;
    public GameObject toggleMapButton;

    [Header("Snapshot")]
    public Camera mapCamera;
    public RenderTexture mapRenderTexture;

    public enum GameMode { SinglePlayer, Multiplayer }
    public GameMode CurrentGameMode { get; private set; } = GameMode.SinglePlayer;
    public GameObject universalBalloonPrefab;

    // Public booleans
    public bool isGameOver = false;

    // Sub-Managers
    public GameFlowController flowController;
    public CellOccupationManager cellManager;
    public SnapshotManager snapshotManager;
    // If you have a ResourceManager, declare that here as well.

    // Track the time the game starts
    private float gameStartTime;

    void Awake()
    {
        if (Instance == null) Instance = this;
        else Destroy(gameObject);
    }

    void Start()
    {
        if (flowController == null) flowController = GetComponent<GameFlowController>();
        if (cellManager == null) cellManager = GetComponent<CellOccupationManager>();
        if (snapshotManager == null) snapshotManager = GetComponent<SnapshotManager>();

        // Initialize UI
        gameOverPanel.SetActive(false);
        winPanel.SetActive(false);
        UpdateUI();

        // Determine if we’re in single or multi
        if (NetworkManager.Instance.isConnected && NetworkManager.Instance.IsMatchmakingRequested)
        {
            CurrentGameMode = GameMode.Multiplayer;
            balloonSendingPanel?.SetActive(true);
            UIManager.Instance?.SetOpponentSnapshotPanel(false);
        }
        else
        {
            CurrentGameMode = GameMode.SinglePlayer;
            balloonSendingPanel?.SetActive(false);
            UIManager.Instance?.SetOpponentSnapshotPanel(false);
            toggleMapButton.SetActive(false);
        }

        AudioManager.Instance.PlayGameMusic();

        // Start the game
        gameStartTime = Time.time;
        flowController.StartGame();

        GlobalSettings.ApplyTimeScaleIfPossible();

        Debug.Log("CurrentGameMode: " + CurrentGameMode);
    }

    // Basic UI updates
    public void UpdateUI()
    {
        if (livesText) livesText.text = lives.ToString();
        if (currencyText) currencyText.text = currency.ToString();
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
            flowController.GameOver();
            //GameOver();
        }
    }

    // Basic Resource Management
    public void AddCurrency(int amount)
    {
        int finalAmount = Mathf.RoundToInt(amount * moneyMultiplier);
        currency += finalAmount;
        UpdateUI();
        RefreshButtonsDisplay();
    }

    public bool CanAfford(int amount)
    {
        return (currency >= amount);
    }

    public void SpendCurrency(int amount)
    {
        currency -= amount;
        UpdateUI();
        RefreshButtonsDisplay();
    }

    private void RefreshButtonsDisplay()
    {
        TowerSelectionUI towerSelectionUI = FindObjectOfType<TowerSelectionUI>();
        towerSelectionUI?.RefreshTowerButtons();

        BalloonSendingPanel bPanel = FindObjectOfType<BalloonSendingPanel>();
        bPanel?.RefreshBalloonButtons();
    }

    public float GetGameTimeElapsed()
    {
        return (Time.time - gameStartTime);
    }

    // Quick method to load main menu
    public void LoadMainMenu()
    {
        Time.timeScale = 1f;
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

            //var message = new
            //{
            //    Type = "SendBalloon",
            //    Data = new
            //    {
            //        BalloonHealth = balloonHealth,
            //        Cost = cost
            //    }
            //};

            //string jsonMessage = JsonConvert.SerializeObject(message);
            //NetworkManager.Instance.SendMessageWithLengthPrefix(jsonMessage);

            JObject dataObj = new JObject
            {
                ["BalloonHealth"] = balloonHealth,
                ["Cost"] = cost,
            };
            NetworkManager.Instance.SendAuthenticatedMessage("SendBalloon", dataObj);

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
}
