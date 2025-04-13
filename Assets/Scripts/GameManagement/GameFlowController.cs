using UnityEngine;
using TMPro;
using Newtonsoft.Json.Linq;

public class GameFlowController : MonoBehaviour
{
    private GameManager gameManager; // We'll reference the main GameManager

    void Awake()
    {
        gameManager = GetComponent<GameManager>();
    }

    public void StartGame()
    {
        // Start balloon waves (common to both single & multi).
        BalloonSpawner.Instance.StartSpawningWaves();
    }

    public void OnMatchFound()
    {
        if (gameManager.balloonSendingPanel != null)
        {
            gameManager.balloonSendingPanel.SetActive(true);
        }
        StartGame();
    }

    public void WinGame(string reason)
    {
        if (gameManager.isGameOver) return;

        gameManager.isGameOver = true;

        // Show UI
        gameManager.winPanel.SetActive(true);
        AudioManager.Instance.StopGameMusic();
        AudioManager.Instance.PlayWinMusic();

        // Display reason
        TextMeshProUGUI winReasonText = gameManager.winPanel.transform
            .Find("WinMessageText")
            .GetComponent<TextMeshProUGUI>();
        if (winReasonText != null)
        {
            winReasonText.text = reason;
        }

        NetworkManager.Instance.ResetMatchmaking();
        Time.timeScale = 0;

        // 1) Notify the other client that I (local) won => "GameOver" with {Won=true}
        if (gameManager.CurrentGameMode == GameManager.GameMode.Multiplayer)
        {
            JObject dataObj = new JObject
            {
                ["Won"] = true,
            };

            NetworkManager.Instance.messageSender.SendAuthenticatedMessage("GameOver", dataObj);
        }

        // 2) Send "GameOverDetailed" => only do it in Single Player or if I'm the winner in Multiplayer
        // Since I'm the winner here, I'll do it in every scenario: single or multi.
        int localUserId = PlayerPrefs.GetInt("UserId", -1);
        int? user2Id = null;

        if (gameManager.CurrentGameMode == GameManager.GameMode.Multiplayer)
        {
            int oppId = PlayerPrefs.GetInt("OpponentUserId", -1);
            user2Id = (oppId == -1) ? null : (int?)oppId;
            PlayerPrefs.DeleteKey("OpponentUserId");
            PlayerPrefs.Save();
        }

        // localUserId is the winner
        int? wonUserId = localUserId;

        int finalWave = BalloonSpawner.Instance.GetCurrentWaveIndex();
        int timePlayed = (int)gameManager.GetGameTimeElapsed();

        string mode = (gameManager.CurrentGameMode == GameManager.GameMode.SinglePlayer) ? "SinglePlayer" : "Multiplayer";

        // 3) Send the detailed message
        NetworkManager.Instance.messageSender.SendGameOverDetailed(
            user1Id: localUserId,
            user2Id: user2Id,
            mode: mode,
            wonUserId: wonUserId,
            finalWave: finalWave,
            timePlayed: timePlayed
        );
    }

    public void GameOver() //The player has lost (game over in terms of losing)
    {
        if (gameManager.isGameOver) return;

        gameManager.isGameOver = true;

        UIManager.Instance.opponentSnapshotPanel.SetActive(false);
        gameManager.gameOverPanel.SetActive(true);

        AudioManager.Instance.StopGameMusic();
        AudioManager.Instance.PlayLoseMusic();
        NetworkManager.Instance.ResetMatchmaking();

        Time.timeScale = 0; // Pause the game

        if (gameManager.CurrentGameMode == GameManager.GameMode.Multiplayer)
        {
            PlayerPrefs.DeleteKey("OpponentUserId");
            PlayerPrefs.Save();

            // Stop snapshot if running
            if (gameManager.snapshotManager != null)
            {
                gameManager.snapshotManager.StopSendingSnapshots();
            }

            // Notify the server of the game over (and I lost)
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
            // Single player losing => still record session
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

    public void OnOpponentGameOver(bool opponentWon, string reason = null)
    {
        if (gameManager.isGameOver) return;

        if (opponentWon)
        {
            // The opponent won, so we lost
            GameOver();
        }
        else
        {
            // Opponent lost
            if (string.IsNullOrEmpty(reason))
            {
                // Default reason if not provided
                reason = "You've defeated the other player";
            }
            WinGame(reason);
        }
    }
}
