/*
 * Description:
 * This file contains the GameFlowController class, which is responsible for managing the overall 
 * flow of the game. It handles the start of the game, transitions when a match is found, and the game 
 * outcome, including both win and loss scenarios. It integrates with the GameManager to update UI and 
 * communicate with the server regarding game over details.
 */

using UnityEngine;
using TMPro;
using Newtonsoft.Json.Linq;

/// <summary>
/// Controls the game flow by managing game start, win, and game over events.
/// It communicates with the GameManager, AudioManager, NetworkManager, and various UI elements 
/// to coordinate the game state across single-player and multiplayer modes.
/// </summary>
public class GameFlowController : MonoBehaviour
{
    // Reference to the main GameManager.
    private GameManager gameManager;

    /// <summary>
    /// Awake is called when the script instance is being loaded.
    /// Retrieves and stores a reference to the GameManager component.
    /// </summary>
    void Awake()
    {
        gameManager = GetComponent<GameManager>();
    }

    /// <summary>
    /// Starts the game by beginning the balloon wave spawning process.
    /// This applies to both single-player and multiplayer game modes.
    /// </summary>
    public void StartGame()
    {
        // Start balloon waves (common to both single and multiplayer).
        BalloonSpawner.Instance.StartSpawningWaves();
    }

    /// <summary>
    /// Called when a match is found in multiplayer mode.
    /// Activates the balloon sending panel and starts the game.
    /// </summary>
    public void OnMatchFound()
    {
        if (gameManager.balloonSendingPanel != null)
        {
            gameManager.balloonSendingPanel.SetActive(true);
        }
        StartGame();
    }

    /// <summary>
    /// Processes a win event for the local player.
    /// Displays win UI, stops game music, plays win music, sends game over notifications to the server,
    /// and sends detailed game session information.
    /// </summary>
    /// <param name="reason">
    /// A string describing the reason for the win, which is displayed in the win panel.
    /// </param>
    public void WinGame(string reason)
    {
        // Do nothing if the game is already over.
        if (gameManager.isGameOver) return;

        gameManager.isGameOver = true;

        // Update UI for win state.
        gameManager.winPanel.SetActive(true);
        AudioManager.Instance.StopGameMusic();
        AudioManager.Instance.PlayWinMusic();

        // Display win reason using the "WinMessageText" child of the winPanel.
        TextMeshProUGUI winReasonText = gameManager.winPanel.transform
            .Find("WinMessageText")
            .GetComponent<TextMeshProUGUI>();
        if (winReasonText != null)
        {
            winReasonText.text = reason;
        }

        // Reset matchmaking state.
        NetworkManager.Instance.ResetMatchmaking();
        Time.timeScale = 0; // Pause the game.

        // 1) In multiplayer, notify the opponent that the local player won.
        if (gameManager.CurrentGameMode == GameManager.GameMode.Multiplayer)
        {
            JObject dataObj = new JObject
            {
                ["Won"] = true,
            };
            NetworkManager.Instance.messageSender.SendAuthenticatedMessage("GameOver", dataObj);
        }

        // 2) Prepare detailed game session data.
        int localUserId = PlayerPrefs.GetInt("UserId", -1);
        int? user2Id = null;

        if (gameManager.CurrentGameMode == GameManager.GameMode.Multiplayer)
        {
            int oppId = PlayerPrefs.GetInt("OpponentUserId", -1);
            user2Id = (oppId == -1) ? null : (int?)oppId;
            PlayerPrefs.DeleteKey("OpponentUserId");
            PlayerPrefs.Save();
        }

        // Set the local user as the winner.
        int? wonUserId = localUserId;
        int finalWave = BalloonSpawner.Instance.GetCurrentWaveIndex();
        int timePlayed = (int)gameManager.GetGameTimeElapsed();
        string mode = (gameManager.CurrentGameMode == GameManager.GameMode.SinglePlayer) ? "SinglePlayer" : "Multiplayer";

        // 3) Send detailed game session data to the server.
        NetworkManager.Instance.messageSender.SendGameOverDetailed(
            user1Id: localUserId,
            user2Id: user2Id,
            mode: mode,
            wonUserId: wonUserId,
            finalWave: finalWave,
            timePlayed: timePlayed
        );
    }

    /// <summary>
    /// Processes a game over event when the local player loses the game.
    /// For multiplayer, stops snapshots, notifies the server of the loss,
    /// and resets matchmaking. For single-player, sends detailed session information.
    /// </summary>
    public void GameOver()
    {
        // Do nothing if the game is already over.
        if (gameManager.isGameOver) return;

        gameManager.isGameOver = true;

        // Update UI for game over.
        UIManager.Instance.opponentSnapshotPanel.SetActive(false);
        gameManager.gameOverPanel.SetActive(true);

        AudioManager.Instance.StopGameMusic();
        AudioManager.Instance.PlayLoseMusic();
        NetworkManager.Instance.ResetMatchmaking();

        Time.timeScale = 0; // Pause the game.

        if (gameManager.CurrentGameMode == GameManager.GameMode.Multiplayer)
        {
            // Clear opponent user ID from player preferences.
            PlayerPrefs.DeleteKey("OpponentUserId");
            PlayerPrefs.Save();

            // Stop sending snapshots
            if (gameManager.snapshotManager != null)
            {
                gameManager.snapshotManager.StopSendingSnapshots();
            }

            // Notify the server that the local player lost.
            JObject dataObj = new JObject
            {
                ["Won"] = false,
            };
            NetworkManager.Instance.messageSender.SendAuthenticatedMessage("GameOver", dataObj);

            // In MULTIPLAYER, we do NOT send "GameOverDetailed" => that’s done only by the winner (in order to not create the same game session twice).
            // So do nothing more here
        }
        else
        {
            // For single-player, send detailed game session information even on loss.
            int localUserId = PlayerPrefs.GetInt("UserId", -1);
            int? user2Id = null;
            int? wonUserId = null;
            int finalWave = BalloonSpawner.Instance.GetCurrentWaveIndex();
            int timePlayed = (int)gameManager.GetGameTimeElapsed();
            string mode = "SinglePlayer";

            NetworkManager.Instance.messageSender.SendGameOverDetailed(
                user1Id: localUserId,
                user2Id: user2Id,
                mode: mode,
                wonUserId: wonUserId,
                finalWave: finalWave,
                timePlayed: timePlayed
            );
        }
    }

    /// <summary>
    /// Handles the event when the opponent's game over status is received.
    /// If the opponent won, the local player loses; otherwise, the local player wins.
    /// </summary>
    /// <param name="opponentWon">
    /// A boolean value indicating if the opponent won the game.
    /// </param>
    /// <param name="reason">
    /// An optional string explaining the reason for the result. If null or empty and the opponent lost,
    /// a default win message is used.
    /// </param>
    public void OnOpponentGameOver(bool opponentWon, string reason = null)
    {
        if (gameManager.isGameOver) return;

        if (opponentWon)
        {
            // The opponent won; therefore, the local player loses.
            GameOver();
        }
        else
        {
            // Opponent lost.
            if (string.IsNullOrEmpty(reason))
            {
                // Use a default win reason if none is provided.
                reason = "You've defeated the other player";
            }
            WinGame(reason);
        }
    }
}
