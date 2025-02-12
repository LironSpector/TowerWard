//------- After balloon code & behaviour changes: -----------
using UnityEngine;
using System.Collections;
using System.Collections.Generic;
using Newtonsoft.Json;
using Newtonsoft.Json.Linq;
using System.Linq;

public class BalloonSpawner : MonoBehaviour
{
    public Transform[] waypoints; // Assign in Inspector

    private int currentWaveIndex = 0;
    private int waveBalloonsRemaining = 0; // only for wave balloons

    // List of WaveData assets
    public List<WaveData> waves = new List<WaveData>();

    // New variables for balloon tracking
    private int balloonsRemaining = 0;

    public static BalloonSpawner Instance { get; private set; }

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

    public int GetCurrentWaveIndex()
    {
        return currentWaveIndex;
    }

    public void StartSpawningWaves()
    {
        // We do nothing automatically for Multi, but Single Player can do wave 0 if we want
        StartCoroutine(SpawnWave(currentWaveIndex));
    }

    private IEnumerator SpawnWave(int waveIndex)
    {
        if (waveIndex >= waves.Count) yield break;
        WaveData waveData = waves[waveIndex];

        waveBalloonsRemaining += waveData.spawnInstructions.Count;

        foreach (var instruction in waveData.spawnInstructions)
        {
            yield return new WaitForSeconds(instruction.spawnDelay);
            SpawnBalloon(instruction); // new approach
        }

        // Then wait for wave to complete, etc...
        yield return StartCoroutine(CheckWaveCompletion());
    }


    public void SetWaveIndex(int index)
    {
        currentWaveIndex = index;
    }

    private IEnumerator CheckWaveCompletion()
    {
        while (waveBalloonsRemaining > 0)
        {
            yield return null;
        }

        // We don't locally increment wave. We do:
        // Single Player => start next wave automatically
        if (GameManager.Instance.CurrentGameMode == GameManager.GameMode.SinglePlayer)
        {

            currentWaveIndex++;
            Debug.Log("currentWaveIndex is this one: " + currentWaveIndex);
            Debug.Log("waves.Count is this one: " + waves.Count);
            if (currentWaveIndex >= waves.Count)
            {
                yield return new WaitForSeconds(5f);
                GameManager.Instance.flowController.WinGame("You've defeated all the waves");
                //GameManager.Instance.WinGame("You've defeated all the waves");
            }
            else
            {
                StartCoroutine(SpawnWave(currentWaveIndex));
            }
        }
        else
        {
            // Multiplayer => notify server
            // We do NOT increment currentWaveIndex locally
            if (currentWaveIndex >= waves.Count)
            {
                yield return new WaitForSeconds(5f);
                GameManager.Instance.flowController.WinGame("You've defeated all the waves");
                //GameManager.Instance.WinGame("You've defeated all the waves");
            }
            else
            {


                //string waveDone = $"{{\"Type\":\"WaveDone\",\"WaveIndex\":{currentWaveIndex}}}";
                //NetworkManager.Instance.SendMessageWithLengthPrefix(waveDone);

                JObject dataObj = new JObject
                {
                    ["WaveIndex"] = currentWaveIndex,
                };
                NetworkManager.Instance.messageSender.SendAuthenticatedMessage("WaveDone", dataObj);
                //NetworkManager.Instance.SendAuthenticatedMessage("WaveDone", dataObj);
            }
        }
    }

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


    // Called by "StartNextWave" message from server
    public IEnumerator StartNextWave(int waveIndex)
    {
        Debug.Log($"StartNextWave with waveIndex= {waveIndex}");

        // forcibly set wave index
        currentWaveIndex = waveIndex;

        if (currentWaveIndex >= waves.Count)
        {
            yield break;
        }

        yield return StartCoroutine(SpawnWave(currentWaveIndex));
    }

    // ---------- Wave Balloon -----------
    public void SpawnWaveBalloon(GameObject prefab)
    {
        GameObject balloonGO = Instantiate(prefab, waypoints[0].position, Quaternion.identity); //GO = GameObject

        Balloon balloonScript = balloonGO.GetComponent<Balloon>();
        balloonScript.isWaveBalloon = true;

        balloonScript.OnDestroyed += OnBalloonDestroyed;
        balloonScript.OnEndReached += OnBalloonDestroyedByEnd;

        BalloonMovement movement = balloonGO.GetComponent<BalloonMovement>();
        movement.waypoints = waypoints;
    }


    public void OnBalloonDestroyed(Balloon b)
    {
        if (b.isWaveBalloon)
        {
            //Debug.Log("Wave balloon destroyed. Current num balloons: " + waveBalloonsRemaining);
            waveBalloonsRemaining--;
        }
        //else
        //{
        //    Debug.Log("Extra balloon destroyed");
        //}

        b.OnDestroyed -= OnBalloonDestroyed;
        b.OnEndReached -= OnBalloonDestroyedByEnd;
    }


    public void OnBalloonDestroyedByEnd(Balloon b)
    {
        if (b.isWaveBalloon)
        {
            waveBalloonsRemaining--;
            //Debug.Log("Wave balloon destroyed. Current num balloons: " + waveBalloonsRemaining);
        }
        //else
        //{
        //    Debug.Log("Extra balloon destroyed");
        //}


        b.OnDestroyed -= OnBalloonDestroyed;
        b.OnEndReached -= OnBalloonDestroyedByEnd;
    }

    // ---------- Extra Balloon -----------
    public void SpawnExtraBalloon(GameObject prefab, int balloonHealth)
    {
        GameObject balloonGO = Instantiate(prefab, waypoints[0].position, Quaternion.identity);

        Balloon b = balloonGO.GetComponent<Balloon>();

        b.health = balloonHealth;
        b.RecalculateAttributesBasedOnHealth();
        b.isWaveBalloon = false;

        // Possibly set balloon’s movement waypoints
        BalloonMovement movement = balloonGO.GetComponent<BalloonMovement>();
        movement.waypoints = waypoints; // or wherever you define them

        // Subscribe to OnDestroyed events
        b.OnDestroyed += OnBalloonDestroyed;
        b.OnEndReached += OnBalloonDestroyedByEnd;
    }


    public void ResetSpawnConfigurations()
    {
        currentWaveIndex = 0;
        Debug.Log("Balloon Spawner configurations have been reset.");
    }

}
