/*
 * Description:
 * This file contains the GameManager class which acts as the main control hub for the game session.
 * It tracks game states such as player's lives, currency, game mode (single or multiplayer), 
 * and manages other sub-managers (e.g., game flow, cell occupation, snapshot management).
 * It also handles UI updates, resource management, and multiplayer-specific logic such as sending balloons.
 */

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

/// <summary>
/// Manages the overall game session including player state, UI updates, game mode management,
/// and integration with sub-managers like game flow and snapshot management. Also handles multiplayer operations.
/// </summary>
public class GameManager : MonoBehaviour
{
    /// <summary>
    /// Singleton instance of the GameManager.
    /// </summary>
    public static GameManager Instance;

    [Header("Player & Economy")]
    /// <summary>
    /// The player's number of lives.
    /// </summary>
    public int lives = 20;
    /// <summary>
    /// The player's currency.
    /// </summary>
    public int currency = 100;

    [Header("Global Buffs")]
    /// <summary>
    /// Multiplicative factor for money gains.
    /// </summary>
    public float moneyMultiplier = 1f;
    /// <summary>
    /// Global factor affecting the speed of all balloons.
    /// </summary>
    public float allBalloonsSpeedFactor = 1f;
    /// <summary>
    /// Multiplicative factor that increases the range of towers.
    /// </summary>
    public float rangeBuffFactor = 1f;
    /// <summary>
    /// Multiplicative factor that affects tower fire rate.
    /// </summary>
    public float fireRateBuffFactor = 1f;

    [Header("UI & Panels")]
    /// <summary>
    /// UI element displaying the current lives.
    /// </summary>
    public TextMeshProUGUI livesText;
    /// <summary>
    /// UI element displaying the current currency.
    /// </summary>
    public TextMeshProUGUI currencyText;
    /// <summary>
    /// The panel shown when the game is over.
    /// </summary>
    public GameObject gameOverPanel;
    /// <summary>
    /// The panel shown when the game is won.
    /// </summary>
    public GameObject winPanel;
    /// <summary>
    /// The panel for sending balloons in multiplayer.
    /// </summary>
    public GameObject balloonSendingPanel;
    /// <summary>
    /// A button to toggle the map in the UI.
    /// </summary>
    public GameObject toggleMapButton;

    [Header("Snapshot")]
    /// <summary>
    /// Camera used to capture the map snapshot.
    /// </summary>
    public Camera mapCamera;
    /// <summary>
    /// Render texture used to hold the snapshot image.
    /// </summary>
    public RenderTexture mapRenderTexture;

    /// <summary>
    /// Game modes available for the session.
    /// </summary>
    public enum GameMode { SinglePlayer, Multiplayer }
    /// <summary>
    /// Gets the current game mode. Defaults to SinglePlayer.
    /// </summary>
    public GameMode CurrentGameMode { get; private set; } = GameMode.SinglePlayer;
    /// <summary>
    /// A prefab for the balloon used in multiplayer.
    /// </summary>
    public GameObject universalBalloonPrefab;

    // Public booleans
    /// <summary>
    /// Flag indicating if the game session is over.
    /// </summary>
    public bool isGameOver = false;

    // Sub-Managers
    /// <summary>
    /// Manages the overall game flow.
    /// </summary>
    public GameFlowController flowController;
    /// <summary>
    /// Manages the cell occupation on the game grid.
    /// </summary>
    public CellOccupationManager cellManager;
    /// <summary>
    /// Manages snapshots for multiplayer view.
    /// </summary>
    public SnapshotManager snapshotManager;

    // Track the time the game starts.
    private float gameStartTime;

    /// <summary>
    /// Awake is called when the script instance is being loaded.
    /// Initializes the singleton instance of the GameManager.
    /// </summary>
    void Awake()
    {
        if (Instance == null)
        {
            Instance = this;
        }
        else
        {
            Destroy(gameObject);
        }
    }

    /// <summary>
    /// Start is called before the first frame update.
    /// Initializes sub-managers, UI elements, determines game mode,
    /// plays game music, and starts the game session.
    /// </summary>
    void Start()
    {
        // Ensure sub-managers are assigned
        if (flowController == null) flowController = GetComponent<GameFlowController>();
        if (cellManager == null) cellManager = GetComponent<CellOccupationManager>();
        if (snapshotManager == null) snapshotManager = GetComponent<SnapshotManager>();

        // Initialize UI panels and update display
        gameOverPanel.SetActive(false);
        winPanel.SetActive(false);
        UpdateUI();

        // Determine game mode based on networking state and matchmaking request status
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

        // Play game music via Audio Manager
        AudioManager.Instance.PlayGameMusic();

        // Record the game start time and begin the game session
        gameStartTime = Time.time;
        flowController.StartGame();

        GlobalSettings.ApplyTimeScaleIfPossible();

        Debug.Log("CurrentGameMode: " + CurrentGameMode);
    }

    /// <summary>
    /// Updates the UI elements for lives and currency.
    /// </summary>
    public void UpdateUI()
    {
        if (livesText) livesText.text = lives.ToString();
        if (currencyText) currencyText.text = currency.ToString();
    }

    /// <summary>
    /// Decreases the player's lives by a specified amount.
    /// If lives drop to zero or below, triggers game over.
    /// </summary>
    /// <param name="livesToDecrease">The number of lives to subtract.</param>
    public void LoseLife(int livesToDecrease)
    {
        if (isGameOver)
            return;

        lives -= livesToDecrease;
        if (lives < 0)
            lives = 0;

        UpdateUI();

        // Trigger game over when lives are depleted.
        if (lives <= 0)
        {
            flowController.GameOver();
        }
    }

    /// <summary>
    /// Adds currency to the player's total after applying the money multiplier.
    /// </summary>
    /// <param name="amount">The base amount of currency to add.</param>
    public void AddCurrency(int amount)
    {
        int finalAmount = Mathf.RoundToInt(amount * moneyMultiplier);
        currency += finalAmount;
        UpdateUI();
        RefreshButtonsDisplay();
    }

    /// <summary>
    /// Checks if the player has sufficient currency for a cost.
    /// </summary>
    /// <param name="amount">The cost to check against.</param>
    /// <returns>True if the currency is equal to or exceeds the cost; otherwise, false.</returns>
    public bool CanAfford(int amount)
    {
        return (currency >= amount);
    }

    /// <summary>
    /// Deducts a specified amount of currency from the player's total.
    /// </summary>
    /// <param name="amount">The amount to deduct.</param>
    public void SpendCurrency(int amount)
    {
        currency -= amount;
        UpdateUI();
        RefreshButtonsDisplay();
    }

    /// <summary>
    /// Refreshes UI display on tower and balloon selection buttons.
    /// </summary>
    private void RefreshButtonsDisplay()
    {
        TowerSelectionUI towerSelectionUI = FindObjectOfType<TowerSelectionUI>();
        towerSelectionUI?.RefreshTowerButtons();

        BalloonSendingPanel bPanel = FindObjectOfType<BalloonSendingPanel>();
        bPanel?.RefreshBalloonButtons();
    }

    /// <summary>
    /// Gets the elapsed game time since the start of the session.
    /// </summary>
    /// <returns>The elapsed time in seconds.</returns>
    public float GetGameTimeElapsed()
    {
        return (Time.time - gameStartTime);
    }

    /// <summary>
    /// Loads the Main Menu scene and resets the time scale.
    /// </summary>
    public void LoadMainMenu()
    {
        Time.timeScale = 1f;
        SceneManager.LoadScene("MainMenu");
    }

    #region Multiplayer Methods

    /// <summary>
    /// Sends a balloon to the opponent in multiplayer mode if the player has enough currency.
    /// Deducts the corresponding cost and notifies the server with the balloon's health.
    /// </summary>
    /// <param name="balloonHealth">Health value associated with the balloon.</param>
    /// <param name="cost">The cost in currency to send the balloon.</param>
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

            // Create JSON data object containing the balloon health information.
            JObject dataObj = new JObject
            {
                ["BalloonHealth"] = balloonHealth,
            };
            NetworkManager.Instance.messageSender.SendAuthenticatedMessage("SendBalloon", dataObj);

            // Uncomment the following line for additional debug information:
            // Debug.Log($"Sent balloon with health {balloonHealth} to opponent.");
        }
        else
        {
            Debug.Log("Not enough currency to send balloons.");
        }
    }

    /// <summary>
    /// Spawns an opponent’s balloon in the game with a specified health value.
    /// </summary>
    /// <param name="balloonHealth">Health value of the opponent balloon to spawn.</param>
    public void SpawnOpponentBalloon(int balloonHealth)
    {
        if (isGameOver) return;

        // Spawn the universal balloon using the BalloonSpawner.
        BalloonSpawner.Instance.SpawnExtraBalloon(universalBalloonPrefab, balloonHealth);
    }

    #endregion Multiplayer Methods
}
