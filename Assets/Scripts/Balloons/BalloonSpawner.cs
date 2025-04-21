using UnityEngine;
using System.Collections;
using System.Collections.Generic;
using Newtonsoft.Json;
using Newtonsoft.Json.Linq;
using System.Linq;

/// <summary>
/// Description:
/// Manages the spawning of balloons based on wave data during gameplay.
/// This class handles spawning wave balloons according to predefined instructions, checking wave completion,
/// notifying the server (in multiplayer) or automatically starting the next wave (in single player), and spawning extra balloons.
/// It also provides methods for resetting spawn configurations and predicting wave progression.
/// </summary>
public class BalloonSpawner : MonoBehaviour
{
    #region Public Fields

    /// <summary>
    /// An array of waypoints that define the path for all balloons.
    /// These should be assigned in the Inspector.
    /// </summary>
    public Transform[] waypoints;

    /// <summary>
    /// A list of WaveData assets that contain spawn instructions for each wave.
    /// </summary>
    public List<WaveData> waves = new List<WaveData>();

    #endregion

    #region Private Fields

    /// <summary>
    /// Index of the current wave being spawned.
    /// </summary>
    private int currentWaveIndex = 0;

    /// <summary>
    /// The number of wave balloons remaining in the current wave.
    /// Used to determine wave completion.
    /// </summary>
    private int waveBalloonsRemaining = 0;

    /// <summary>
    /// Tracks the number of extra (non-wave) balloons remaining.
    /// </summary>
    private int balloonsRemaining = 0;

    #endregion

    #region Singleton

    /// <summary>
    /// Gets the singleton instance of the BalloonSpawner.
    /// </summary>
    public static BalloonSpawner Instance { get; private set; }

    /// <summary>
    /// Awake is called when the script instance is being loaded.
    /// Initializes the singleton instance of this class.
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

    #endregion

    #region Public Methods

    /// <summary>
    /// Returns the current wave index.
    /// </summary>
    /// <returns>The index of the current wave.</returns>
    public int GetCurrentWaveIndex()
    {
        return currentWaveIndex;
    }

    /// <summary>
    /// Starts spawning the waves of balloons.
    /// In Single Player mode, this begins with the first wave.
    /// In Multiplayer, wave spawning is usually managed by the server.
    /// </summary>
    public void StartSpawningWaves()
    {
        StartCoroutine(SpawnWave(currentWaveIndex));
    }

    /// <summary>
    /// Sets the current wave index to the specified value.
    /// </summary>
    /// <param name="index">The new current wave index.</param>
    public void SetWaveIndex(int index)
    {
        currentWaveIndex = index;
    }

    /// <summary>
    /// Spawns an extra (non-wave) balloon using the specified prefab and health.
    /// Extra balloons are tracked separately from wave balloons.
    /// </summary>
    /// <param name="prefab">The balloon prefab to instantiate.</param>
    /// <param name="balloonHealth">The health value to assign to the balloon.</param>
    public void SpawnExtraBalloon(GameObject prefab, int balloonHealth)
    {
        GameObject balloonGO = Instantiate(prefab, waypoints[0].position, Quaternion.identity);
        Balloon b = balloonGO.GetComponent<Balloon>();

        b.health = balloonHealth;
        b.RecalculateAttributesBasedOnHealth();
        b.isWaveBalloon = false;

        // Assign movement waypoints.
        BalloonMovement movement = balloonGO.GetComponent<BalloonMovement>();
        movement.waypoints = waypoints;

        // Subscribe to events for proper tracking.
        b.OnDestroyed += OnBalloonDestroyed;
        b.OnEndReached += OnBalloonDestroyedByEnd;
    }

    /// <summary>
    /// Starts the next wave given by the specified wave index.
    /// Typically called in response to a server message.
    /// </summary>
    /// <param name="waveIndex">The index of the wave to start.</param>
    /// <returns>An IEnumerator for coroutine handling.</returns>
    public IEnumerator StartNextWave(int waveIndex)
    {
        Debug.Log($"StartNextWave with waveIndex= {waveIndex}");

        // Force update the current wave index.
        currentWaveIndex = waveIndex;

        if (currentWaveIndex >= waves.Count)
        {
            yield break;
        }

        yield return StartCoroutine(SpawnWave(currentWaveIndex));
    }

    /// <summary>
    /// Resets the spawn configurations, setting the current wave index back to zero.
    /// </summary>
    public void ResetSpawnConfigurations()
    {
        currentWaveIndex = 0;
    }

    #endregion

    #region Private Methods: Wave Spawning

    /// <summary>
    /// Spawns all balloons for the specified wave based on the wave data.
    /// Waits for each balloon spawn delay and then checks for wave completion.
    /// </summary>
    /// <param name="waveIndex">The index of the wave to spawn.</param>
    /// <returns>An IEnumerator for coroutine handling.</returns>
    private IEnumerator SpawnWave(int waveIndex)
    {
        if (waveIndex >= waves.Count)
            yield break;

        WaveData waveData = waves[waveIndex];

        // Increase the count of wave balloons.
        waveBalloonsRemaining += waveData.spawnInstructions.Count;

        // Loop through each spawn instruction and wait for its delay before spawning.
        foreach (var instruction in waveData.spawnInstructions)
        {
            yield return new WaitForSeconds(instruction.spawnDelay);
            SpawnBalloon(instruction);
        }

        // Wait until the wave is completed.
        yield return StartCoroutine(CheckWaveCompletion());
    }

    /// <summary>
    /// Checks if the current wave is complete by waiting until all wave balloons are destroyed.
    /// After completion, starts the next wave automatically in Single Player mode,
    /// or notifies the server in Multiplayer mode.
    /// </summary>
    /// <returns>An IEnumerator for coroutine handling.</returns>
    private IEnumerator CheckWaveCompletion()
    {
        while (waveBalloonsRemaining > 0)
        {
            yield return null;
        }

        // Single Player: automatically start the next wave.
        if (GameManager.Instance.CurrentGameMode == GameManager.GameMode.SinglePlayer)
        {
            currentWaveIndex++;
            Debug.Log("currentWaveIndex is this one: " + currentWaveIndex);
            Debug.Log("waves.Count is this one: " + waves.Count);
            if (currentWaveIndex >= waves.Count)
            {
                yield return new WaitForSeconds(5f);
                GameManager.Instance.flowController.WinGame("You've defeated all the waves");
            }
            else
            {
                StartCoroutine(SpawnWave(currentWaveIndex));
            }
        }
        else
        {
            // Multiplayer: notify the server about wave completion; do not increment locally.
            if (currentWaveIndex >= waves.Count)
            {
                yield return new WaitForSeconds(5f);
                GameManager.Instance.flowController.WinGame("You've defeated all the waves");
            }
            else
            {
                JObject dataObj = new JObject
                {
                    ["WaveIndex"] = currentWaveIndex,
                };
                NetworkManager.Instance.messageSender.SendAuthenticatedMessage("WaveDone", dataObj);
            }
        }
    }

    /// <summary>
    /// Spawns a wave balloon using the provided spawn instruction.
    /// Sets the balloon's health, recalculates its attributes, marks it as a wave balloon,
    /// and subscribes to its destruction and end reached events.
    /// </summary>
    /// <param name="instruction">The spawn instruction containing details such as the prefab, spawn delay, and initial health.</param>
    public void SpawnBalloon(WaveData.SpawnInstruction instruction)
    {
        GameObject balloonGO = Instantiate(instruction.balloonPrefab, waypoints[0].position, Quaternion.identity);
        Balloon b = balloonGO.GetComponent<Balloon>();
        b.health = instruction.initialHealth;
        b.RecalculateAttributesBasedOnHealth();

        b.isWaveBalloon = true;
        b.OnDestroyed += OnBalloonDestroyed;
        b.OnEndReached += OnBalloonDestroyedByEnd;

        BalloonMovement m = balloonGO.GetComponent<BalloonMovement>();
        m.waypoints = waypoints;
    }

    #endregion

    #region Balloon Destruction Handlers

    /// <summary>
    /// Handles a balloon destruction event by decrementing the wave balloon counter if applicable,
    /// and unsubscribing the balloon from events.
    /// </summary>
    /// <param name="b">The balloon that was destroyed.</param>
    public void OnBalloonDestroyed(Balloon b)
    {
        if (b.isWaveBalloon)
        {
            waveBalloonsRemaining--;
        }

        b.OnDestroyed -= OnBalloonDestroyed;
        b.OnEndReached -= OnBalloonDestroyedByEnd;
    }

    /// <summary>
    /// Handles a balloon reaching the end of its path by decrementing the wave balloon counter if applicable,
    /// and unsubscribing the balloon from its events.
    /// </summary>
    /// <param name="b">The balloon that reached the end.</param>
    public void OnBalloonDestroyedByEnd(Balloon b)
    {
        if (b.isWaveBalloon)
        {
            waveBalloonsRemaining--;
        }

        b.OnDestroyed -= OnBalloonDestroyed;
        b.OnEndReached -= OnBalloonDestroyedByEnd;
    }

    #endregion

    #region Extra Balloon Spawning

    /// <summary>
    /// Spawns a wave balloon from an extra source using the specified prefab.
    /// This method is used when the balloon belongs to a wave.
    /// </summary>
    /// <param name="prefab">The balloon prefab to instantiate.</param>
    public void SpawnWaveBalloon(GameObject prefab)
    {
        GameObject balloonGO = Instantiate(prefab, waypoints[0].position, Quaternion.identity);
        Balloon balloonScript = balloonGO.GetComponent<Balloon>();
        balloonScript.isWaveBalloon = true;

        balloonScript.OnDestroyed += OnBalloonDestroyed;
        balloonScript.OnEndReached += OnBalloonDestroyedByEnd;

        BalloonMovement movement = balloonGO.GetComponent<BalloonMovement>();
        movement.waypoints = waypoints;
    }

    #endregion
}
