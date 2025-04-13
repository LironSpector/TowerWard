using UnityEngine;
using Newtonsoft.Json.Linq;
using System.Collections;

/// <summary>
/// Description:
/// Processes decrypted JSON messages received from the server by switching on the message type and delegating to the appropriate handler methods.
/// Also contains coroutines for handling special multiplayer abilities such as "NoMoneyForOpponent", "CloudScreen", and "FastBalloons".
/// </summary>
public class NetworkMessageHandler : MonoBehaviour
{
    private NetworkManager net;

    /// <summary>
    /// Awake is called when the script instance is being loaded.
    /// Retrieves the NetworkManager component attached to the same GameObject.
    /// </summary>
    void Awake()
    {
        net = GetComponent<NetworkManager>();
    }

    /// <summary>
    /// Main entry point for handling a decrypted JSON message from the server.
    /// Parses the message and invokes the specific handler based on the "Type" field.
    /// </summary>
    /// <param name="data">A decrypted JSON string received from the server.</param>
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

    #region Individual Handlers

    /// <summary>
    /// Handles the "MatchFound" message by extracting the opponent's ID, saving it in PlayerPrefs,
    /// and notifying the MatchmakingManager or GameFlowController.
    /// </summary>
    /// <param name="msgObj">The JSON object containing match found data.</param>
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

    /// <summary>
    /// Handles the "SendBalloon" message by extracting the balloon health data
    /// and instructs the GameManager to spawn an opponent balloon with that health.
    /// </summary>
    /// <param name="msgObj">The JSON object containing balloon sending data.</param>
    private void HandleSendBalloon(JObject msgObj)
    {
        JObject dataObj = (JObject)msgObj["Data"];
        int balloonHealth = (int)dataObj["BalloonHealth"];

        UnityMainThreadDispatcher.Instance().Enqueue(() =>
        {
            GameManager.Instance.SpawnOpponentBalloon(balloonHealth);
        });
    }

    /// <summary>
    /// Handles the "GameSnapshot" message by deserializing the snapshot data and instructing the UIManager
    /// to update the opponent snapshot display.
    /// </summary>
    /// <param name="msgObj">The JSON object containing the game snapshot data.</param>
    private void HandleGameSnapshot(JObject msgObj)
    {
        // Deserialize into GameSnapshotMessage.
        GameSnapshotMessage snapshotMsg = msgObj.ToObject<GameSnapshotMessage>();
        string imageData = snapshotMsg.Data.ImageData;

        UnityMainThreadDispatcher.Instance().Enqueue(() =>
        {
            UIManager.Instance.UpdateOpponentSnapshot(imageData);
        });
    }

    /// <summary>
    /// Handles the "ShowSnapshots" message by enabling snapshot sending.
    /// </summary>
    private void HandleShowSnapshots()
    {
        UnityMainThreadDispatcher.Instance().Enqueue(() =>
        {
            GameManager.Instance.snapshotManager.EnableSnapshotSending(true);
        });
    }

    /// <summary>
    /// Handles the "HideSnapshots" message by disabling snapshot sending.
    /// </summary>
    private void HandleHideSnapshots()
    {
        UnityMainThreadDispatcher.Instance().Enqueue(() =>
        {
            GameManager.Instance.snapshotManager.EnableSnapshotSending(false);
        });
    }

    /// <summary>
    /// Handles the "StartNextWave" message by retrieving the next wave index
    /// and starting the corresponding coroutine on the BalloonSpawner.
    /// </summary>
    /// <param name="msgObj">The JSON object containing the wave index.</param>
    private void HandleStartNextWave(JObject msgObj)
    {
        int waveIndex = (int)msgObj["WaveIndex"];
        UnityMainThreadDispatcher.Instance().Enqueue(() =>
        {
            Debug.Log("[MessageHandler] StartNextWave => waveIndex=" + waveIndex);
            BalloonSpawner.Instance.StartCoroutine(BalloonSpawner.Instance.StartNextWave(waveIndex));
        });
    }

    /// <summary>
    /// Handles the "OpponentDisconnected" message by triggering a win for the local player.
    /// </summary>
    private void HandleOpponentDisconnected()
    {
        UnityMainThreadDispatcher.Instance().Enqueue(() =>
        {
            // Auto-win if opponent is disconnected.
            GameManager.Instance.flowController.OnOpponentGameOver(opponentWon: false,
                reason: "The other player disconnected");
        });
    }

    /// <summary>
    /// Handles the "UseMultiplayerAbility" message by extracting the ability name
    /// and initiating the corresponding coroutine to apply its effect.
    /// </summary>
    /// <param name="msgObj">The JSON object containing the ability data.</param>
    private void HandleUseMultiplayerAbility(JObject msgObj)
    {
        JObject dataObj = (JObject)msgObj["Data"];
        string abilityName = dataObj["AbilityName"]?.ToString();

        bool fromOpponent = false;
        if (dataObj["FromOpponent"] != null)
        {
            fromOpponent = (bool)dataObj["FromOpponent"];
        }

        // In multiplayer mode, handle the received ability.
        if (GameManager.Instance.CurrentGameMode == GameManager.GameMode.Multiplayer)
        {
            UnityMainThreadDispatcher.Instance().Enqueue(() =>
            {
                OnMultiplayerAbilityReceived(abilityName);
            });
        }
    }

    /// <summary>
    /// Handles the "GameOver" message by deserializing the game over data
    /// and calling the appropriate method on the GameFlowController.
    /// </summary>
    /// <param name="msgObj">The JSON object containing the game over data.</param>
    private void HandleGameOver(JObject msgObj)
    {
        GameOverMessage gameOverMsg = msgObj.ToObject<GameOverMessage>();
        bool opponentWon = gameOverMsg.Data.Won;

        UnityMainThreadDispatcher.Instance().Enqueue(() =>
        {
            GameManager.Instance.flowController.OnOpponentGameOver(opponentWon);
        });
    }

    /// <summary>
    /// Handles the "LoginSuccess" message by storing authentication tokens and user ID,
    /// and notifying the LoginSceneManager of successful login.
    /// </summary>
    /// <param name="msgObj">The JSON object containing login success data.</param>
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

    /// <summary>
    /// Handles the "LoginFail" message by retrieving the failure reason and notifying the LoginSceneManager.
    /// </summary>
    /// <param name="msgObj">The JSON object containing login failure data.</param>
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

    /// <summary>
    /// Handles the "RegisterSuccess" message by storing authentication data and notifying the LoginSceneManager.
    /// </summary>
    /// <param name="msgObj">The JSON object containing registration success data.</param>
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

    /// <summary>
    /// Handles the "RegisterFail" message by retrieving the failure reason and notifying the LoginSceneManager.
    /// </summary>
    /// <param name="msgObj">The JSON object containing registration failure data.</param>
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

    /// <summary>
    /// Handles the "AutoLoginSuccess" message by updating user authentication tokens and transitioning to the Main Menu.
    /// </summary>
    /// <param name="msgObj">The JSON object containing auto-login success data.</param>
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

        // Store updated tokens.
        if (!string.IsNullOrEmpty(newAccessToken))
            PlayerPrefs.SetString("AccessToken", newAccessToken);
        if (!string.IsNullOrEmpty(newAccessTokenExpiry))
            PlayerPrefs.SetString("AccessTokenExpiry", newAccessTokenExpiry);
        if (!string.IsNullOrEmpty(newRefreshToken))
            PlayerPrefs.SetString("RefreshToken", newRefreshToken);
        if (!string.IsNullOrEmpty(newRefreshTokenExpiry))
            PlayerPrefs.SetString("RefreshTokenExpiry", newRefreshTokenExpiry);

        PlayerPrefs.Save();

        // Skip the LoginScene and load the MainMenu.
        var loginSceneManager = FindObjectOfType<LoginSceneManager>();
        if (loginSceneManager != null)
        {
            UnityMainThreadDispatcher.Instance().Enqueue(() =>
            {
                loginSceneManager.GoToMainMenu();
            });
        }
    }

    /// <summary>
    /// Handles the "AutoLoginFail" message by showing the Login panel.
    /// </summary>
    /// <param name="msgObj">The JSON object containing auto-login failure data.</param>
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

    #endregion

    #region Multiplayer Ability Coroutines

    /// <summary>
    /// Processes a received multiplayer ability by starting the corresponding coroutine to apply its effect.
    /// </summary>
    /// <param name="abilityName">The name of the ability received from the server.</param>
    private void OnMultiplayerAbilityReceived(string abilityName)
    {
        switch (abilityName)
        {
            case "NoMoneyForOpponent":
                // Apply an effect that sets the money multiplier to 0 for 10 seconds.
                StartCoroutine(NoMoneyRoutine());
                break;

            case "CloudScreen":
                // Display a cloud effect for 10 seconds.
                StartCoroutine(CloudScreenRoutine());
                break;

            case "FastBalloons":
                // Temporarily increase the opponent's balloon speed by 50% for 10 seconds.
                StartCoroutine(FastBalloonsRoutineOpponent());
                break;

            default:
                Debug.Log("Unknown or no effect needed for: " + abilityName);
                break;
        }
    }

    /// <summary>
    /// Coroutine that temporarily sets the money multiplier to 0 for 10 seconds, simulating a "No Money" effect.
    /// </summary>
    /// <returns>An IEnumerator for the coroutine.</returns>
    private IEnumerator NoMoneyRoutine()
    {
        Debug.Log("We are receiving no money for 10s!");
        float oldMultiplier = GameManager.Instance.moneyMultiplier;
        GameManager.Instance.moneyMultiplier = 0f;

        yield return new WaitForSeconds(10f);

        // Revert the money multiplier to its original value.
        GameManager.Instance.moneyMultiplier = oldMultiplier;
    }

    /// <summary>
    /// Coroutine that activates a cloud panel effect for 10 seconds to simulate a "CloudScreen" effect.
    /// </summary>
    /// <returns>An IEnumerator for the coroutine.</returns>
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

    /// <summary>
    /// Coroutine that temporarily increases the speed factor for opponent balloons by 50% for 10 seconds,
    /// simulating a "FastBalloons" effect.
    /// </summary>
    /// <returns>An IEnumerator for the coroutine.</returns>
    private IEnumerator FastBalloonsRoutineOpponent()
    {
        Debug.Log("Opponent's FastBalloons effect - wave speed +50% for 10s");
        GameManager.Instance.allBalloonsSpeedFactor = 1.5f;

        yield return new WaitForSeconds(10f);

        // Revert the speed factor to normal.
        GameManager.Instance.allBalloonsSpeedFactor = 1f;
    }

    #endregion
}
