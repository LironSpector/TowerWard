using UnityEngine;
using Newtonsoft.Json.Linq;
using System.Collections;

/// <summary>
/// Processes decrypted JSON messages from the server (the 'HandleMessage' switch).
/// Also includes coroutines for special abilities, etc.
/// </summary>
public class NetworkMessageHandler : MonoBehaviour
{
    private NetworkManager net;

    void Awake()
    {
        net = GetComponent<NetworkManager>();
    }

    /// <summary>
    /// Main entry point for handling a decrypted JSON message from the server.
    /// </summary>
    public void HandleMessage(string data)
    {
        Debug.Log("[CLIENT] Decrypted message: " + data);

        JObject msgObj = JObject.Parse(data);
        string messageType = msgObj["Type"]?.ToString();

        switch (messageType)
        {
            case "MatchFound":
                HandleMatchFound(msgObj);
                break;

            case "SendBalloon":
                HandleSendBalloon(msgObj);
                break;

            case "GameSnapshot":
                HandleGameSnapshot(msgObj);
                break;

            case "ShowSnapshots":
                HandleShowSnapshots();
                break;

            case "HideSnapshots":
                HandleHideSnapshots();
                break;

            case "StartNextWave":
                HandleStartNextWave(msgObj);
                break;

            case "OpponentDisconnected":
                HandleOpponentDisconnected();
                break;

            case "UseMultiplayerAbility":
                HandleUseMultiplayerAbility(msgObj);
                break;

            case "GameOver":
                HandleGameOver(msgObj);
                break;

            case "LoginSuccess":
                HandleLoginSuccess(msgObj);
                break;

            case "LoginFail":
                HandleLoginFail(msgObj);
                break;

            case "RegisterSuccess":
                HandleRegisterSuccess(msgObj);
                break;

            case "RegisterFail":
                HandleRegisterFail(msgObj);
                break;

            case "AutoLoginSuccess":
                HandleAutoLoginSuccess(msgObj);
                break;

            case "AutoLoginFail":
                HandleAutoLoginFail(msgObj);
                break;

            default:
                Debug.LogWarning("[MessageHandler] Unknown message type: " + messageType);
                break;
        }
    }

    // -----------------------------
    //  Individual Handlers
    // -----------------------------
    private void HandleMatchFound(JObject msgObj)
    {
        JObject dataObj = (JObject)msgObj["Data"];
        int oppId = dataObj["OpponentId"]?.ToObject<int>() ?? -1;

        UnityMainThreadDispatcher.Instance().Enqueue(() =>
        {
            Debug.Log($"Match found! OpponentId = {oppId}");

            if (MatchmakingManager.Instance != null)
            {
                MatchmakingManager.Instance.OnMatchFound();
                PlayerPrefs.SetInt("OpponentUserId", oppId);
                PlayerPrefs.Save();
            }
            else if (GameManager.Instance != null)
            {
                GameManager.Instance.flowController.OnMatchFound();
                PlayerPrefs.SetInt("OpponentUserId", oppId);
                PlayerPrefs.Save();
            }
            else
            {
                Debug.LogError("[MessageHandler] No handler found for MatchFound.");
            }
        });
    }

    private void HandleSendBalloon(JObject msgObj)
    {
        JObject dataObj = (JObject)msgObj["Data"];
        int balloonHealth = (int)dataObj["BalloonHealth"];

        UnityMainThreadDispatcher.Instance().Enqueue(() =>
        {
            GameManager.Instance.SpawnOpponentBalloon(balloonHealth);
        });
    }

    private void HandleGameSnapshot(JObject msgObj)
    {
        // Deserialize into GameSnapshotMessage
        GameSnapshotMessage snapshotMsg = msgObj.ToObject<GameSnapshotMessage>();

        string imageData = snapshotMsg.Data.ImageData;
        //Debug.Log("imageData: " + imageData);

        UnityMainThreadDispatcher.Instance().Enqueue(() =>
        {
            UIManager.Instance.UpdateOpponentSnapshot(imageData);
        });
    }

    private void HandleShowSnapshots()
    {
        UnityMainThreadDispatcher.Instance().Enqueue(() =>
        {
            GameManager.Instance.snapshotManager.EnableSnapshotSending(true);
        });
    }

    private void HandleHideSnapshots()
    {
        UnityMainThreadDispatcher.Instance().Enqueue(() =>
        {
            GameManager.Instance.snapshotManager.EnableSnapshotSending(false);
        });
    }

    private void HandleStartNextWave(JObject msgObj)
    {
        int waveIndex = (int)msgObj["WaveIndex"];
        UnityMainThreadDispatcher.Instance().Enqueue(() =>
        {
            Debug.Log("[MessageHandler] StartNextWave => waveIndex=" + waveIndex);
            BalloonSpawner.Instance.StartCoroutine(BalloonSpawner.Instance.StartNextWave(waveIndex));
        });
    }

    private void HandleOpponentDisconnected()
    {
        UnityMainThreadDispatcher.Instance().Enqueue(() =>
        {
            // We automatically win
            GameManager.Instance.flowController.OnOpponentGameOver(opponentWon: false,
                reason: "The other player disconnected");
        });
    }

    private void HandleUseMultiplayerAbility(JObject msgObj)
    {
        JObject dataObj = (JObject)msgObj["Data"];
        string abilityName = dataObj["AbilityName"]?.ToString();

        // Check if there's a "FromOpponent" or something
        bool fromOpponent = false;
        if (dataObj["FromOpponent"] != null)
        {
            fromOpponent = (bool)dataObj["FromOpponent"];
        }

        // If we have an opponent, or we are the opponent...
        // Possibly check: if (opponent != null) forward... 
        // But let's see:
        if (GameManager.Instance.CurrentGameMode == GameManager.GameMode.Multiplayer)
        {
            // We'll do a local function to handle the effect
            UnityMainThreadDispatcher.Instance().Enqueue(() =>
            {
                // If the ability is "NoMoneyForOpponent" or "CloudScreen" and we see "FromOpponent" is not set => we must set it and forward to the opponent
                // But typically, the server will do that for us. 
                // If we are the *receiving* side of "NoMoneyForOpponent", we do the effect. 
                OnMultiplayerAbilityReceived(abilityName);
            });
        }
    }

    private void HandleGameOver(JObject msgObj)
    {
        GameOverMessage gameOverMsg = msgObj.ToObject<GameOverMessage>();
        bool opponentWon = gameOverMsg.Data.Won;

        UnityMainThreadDispatcher.Instance().Enqueue(() =>
        {
            GameManager.Instance.flowController.OnOpponentGameOver(opponentWon);
        });
    }

    private void HandleLoginSuccess(JObject msgObj)
    {
        JObject dataObj = (JObject)msgObj["Data"];

        int userId = dataObj["UserId"]?.ToObject<int>() ?? -1;
        if (userId != -1)
        {
            PlayerPrefs.SetInt("UserId", userId);
        }

        string accessToken = dataObj["AccessToken"]?.ToString();
        string accessTokenExpiry = dataObj["AccessTokenExpiry"]?.ToString();
        string refreshToken = dataObj["RefreshToken"]?.ToString();
        string refreshTokenExpiry = dataObj["RefreshTokenExpiry"]?.ToString();

        PlayerPrefs.SetString("AccessToken", accessToken);
        PlayerPrefs.SetString("AccessTokenExpiry", accessTokenExpiry);
        PlayerPrefs.SetString("RefreshToken", refreshToken);
        PlayerPrefs.SetString("RefreshTokenExpiry", refreshTokenExpiry);
        PlayerPrefs.Save();

        LoginSceneManager lsm = FindObjectOfType<LoginSceneManager>();
        if (lsm != null)
        {
            UnityMainThreadDispatcher.Instance().Enqueue(() => lsm.OnLoginSuccess());
        }
    }

    private void HandleLoginFail(JObject msgObj)
    {
        string reason = msgObj["Data"]["Reason"]?.ToString();
        LoginSceneManager lsm = FindObjectOfType<LoginSceneManager>();
        if (lsm != null)
        {
            UnityMainThreadDispatcher.Instance().Enqueue(() =>
                lsm.OnLoginFail(reason)
            );
        }
    }

    private void HandleRegisterSuccess(JObject msgObj)
    {
        JObject d = (JObject)msgObj["Data"];

        int userId = d["UserId"]?.ToObject<int>() ?? -1;
        if (userId != -1)
        {
            PlayerPrefs.SetInt("UserId", userId);
        }

        string accessToken = d["AccessToken"]?.ToString();
        string accessTokenExpiry = d["AccessTokenExpiry"]?.ToString();
        string refreshToken = d["RefreshToken"]?.ToString();
        string refreshTokenExpiry = d["RefreshTokenExpiry"]?.ToString();

        PlayerPrefs.SetString("AccessToken", accessToken);
        PlayerPrefs.SetString("AccessTokenExpiry", accessTokenExpiry);
        PlayerPrefs.SetString("RefreshToken", refreshToken);
        PlayerPrefs.SetString("RefreshTokenExpiry", refreshTokenExpiry);
        PlayerPrefs.Save();

        LoginSceneManager lsm = FindObjectOfType<LoginSceneManager>();
        if (lsm != null)
        {
            UnityMainThreadDispatcher.Instance().Enqueue(() =>
                lsm.OnRegisterSuccess()
            );
        }
    }

    private void HandleRegisterFail(JObject msgObj)
    {
        string reason = msgObj["Data"]["Reason"]?.ToString();
        LoginSceneManager lsm = FindObjectOfType<LoginSceneManager>();
        if (lsm != null)
        {
            UnityMainThreadDispatcher.Instance().Enqueue(() =>
                lsm.OnRegisterFail(reason)
            );
        }
    }

    private void HandleAutoLoginSuccess(JObject msgObj)
    {
        JObject d = (JObject)msgObj["Data"];
        int userId = d["UserId"].ToObject<int>();

        string newAccessToken = d["AccessToken"]?.ToString();
        string newAccessTokenExpiry = d["AccessTokenExpiry"]?.ToString();
        string newRefreshToken = d["RefreshToken"]?.ToString();
        string newRefreshTokenExpiry = d["RefreshTokenExpiry"]?.ToString();

        Debug.Log("[CLIENT] AutoLoginSuccess => userId=" + userId);

        PlayerPrefs.SetInt("UserId", userId);

        // Store updated tokens
        if (!string.IsNullOrEmpty(newAccessToken))
            PlayerPrefs.SetString("AccessToken", newAccessToken);
        if (!string.IsNullOrEmpty(newAccessTokenExpiry))
            PlayerPrefs.SetString("AccessTokenExpiry", newAccessTokenExpiry);
        if (!string.IsNullOrEmpty(newRefreshToken))
            PlayerPrefs.SetString("RefreshToken", newRefreshToken);
        if (!string.IsNullOrEmpty(newRefreshTokenExpiry))
            PlayerPrefs.SetString("RefreshTokenExpiry", newRefreshTokenExpiry);

        PlayerPrefs.Save();

        // Skip the LoginScene => call the method to load MainMenu
        var loginSceneManager = FindObjectOfType<LoginSceneManager>();
        if (loginSceneManager != null)
        {
            UnityMainThreadDispatcher.Instance().Enqueue(() =>
            {
                loginSceneManager.GoToMainMenu();
            });
        }
    }

    private void HandleAutoLoginFail(JObject msgObj)
    {
        string reason = msgObj["Data"]["Reason"]?.ToString();
        Debug.LogWarning("[CLIENT] AutoLoginFail => " + reason);

        var loginSceneManager = FindObjectOfType<LoginSceneManager>();
        if (loginSceneManager != null)
        {
            UnityMainThreadDispatcher.Instance().Enqueue(() =>
            {
                loginSceneManager.ShowLoginPanel();
            });
        }
    }


    // ---------------------------------------------------------
    //  Multiplayer Ability Coroutines
    // ---------------------------------------------------------
    private void OnMultiplayerAbilityReceived(string abilityName)
    {
        switch (abilityName)
        {
            case "NoMoneyForOpponent":
                // That means *we* are the ones who got locked from money => For 10s set moneyMultiplier=0 or disallowMoney
                StartCoroutine(NoMoneyRoutine());
                break;

            case "CloudScreen":
                // Show the cloud panel for 10s
                StartCoroutine(CloudScreenRoutine());
                break;

            case "FastBalloons":
                StartCoroutine(FastBalloonsRoutineOpponent());
                break;

            default:
                Debug.Log("Unknown or no effect needed for: " + abilityName);
                break;
        }
    }

    private IEnumerator NoMoneyRoutine()
    {
        Debug.Log("We are receiving no money for 10s!");
        float oldMultiplier = GameManager.Instance.moneyMultiplier;
        GameManager.Instance.moneyMultiplier = 0f;

        yield return new WaitForSeconds(10f);

        // revert
        GameManager.Instance.moneyMultiplier = oldMultiplier;
    }

    private IEnumerator CloudScreenRoutine()
    {
        Debug.Log("CloudScreen effect for 10s!");
        if (SpecialAbilitiesManager.Instance != null)
        {
            SpecialAbilitiesManager.Instance.cloudPanel.SetActive(true);
            yield return new WaitForSeconds(10f);
            SpecialAbilitiesManager.Instance.cloudPanel.SetActive(false);
        }
    }

    private IEnumerator FastBalloonsRoutineOpponent()
    {
        Debug.Log("Opponent's FastBalloons effect - wave speed +50% for 10s");
        GameManager.Instance.allBalloonsSpeedFactor = 1.5f;

        yield return new WaitForSeconds(10f);

        // revert
        GameManager.Instance.allBalloonsSpeedFactor = 1f;
    }
}
