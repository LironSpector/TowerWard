// ------------------ BalloonSpawner before waves changes: ------------------
//using UnityEngine;
//using System.Collections;
//using System.Collections.Generic;

//public class BalloonSpawner : MonoBehaviour
//{
//    public Transform[] waypoints; // Assign in Inspector

//    private int currentWaveIndex = 0;

//    // List of WaveData assets
//    public List<WaveData> waves = new List<WaveData>();

//    // New variables for balloon tracking
//    private int balloonsToSpawn = 0;
//    private int balloonsRemaining = 0;
//    //private bool isSpawningWave = false;

//    public static BalloonSpawner Instance { get; private set; }

//    void Awake()
//    {
//        if (Instance == null)
//        {
//            Instance = this;
//        }
//        else
//        {
//            Destroy(gameObject);
//        }
//    }

//    //removing this method - waves will not longer start automatically
//    //void Start()
//    //{
//    //    StartCoroutine(StartNextWave());
//    //}

//    public void StartSpawningWaves()
//    {
//        StartCoroutine(StartNextWave());
//    }


//    IEnumerator StartNextWave()
//    {
//        if (currentWaveIndex >= waves.Count)
//        {
//            // No more waves to spawn
//            yield break;
//        }

//        //isSpawningWave = true;

//        WaveData currentWave = waves[currentWaveIndex];
//        balloonsToSpawn = currentWave.spawnInstructions.Count;
//        balloonsRemaining = balloonsToSpawn;

//        foreach (var instruction in currentWave.spawnInstructions)
//        {
//            yield return new WaitForSeconds(instruction.spawnDelay);
//            SpawnBalloon(instruction.balloonPrefab);
//        }

//        //isSpawningWave = false;

//        // Wait until all balloons are destroyed before starting the next wave
//        StartCoroutine(CheckForNextWave());
//    }

//    IEnumerator CheckForNextWave()
//    {
//        // Wait until all balloons are destroyed
//        while (balloonsRemaining > 0)
//        {
//            yield return null; // Wait for next frame
//        }

//        // All balloons are destroyed
//        currentWaveIndex++;

//        if (currentWaveIndex >= waves.Count)
//        {
//            // No more waves and all balloons are destroyed, player wins
//            yield return new WaitForSeconds(5); //wait for a few more seconds because sometimes at high levels the "balloonsRemaining" is sometimes 0 a bit early.
//            //GameManager.Instance.WinGame();
//            GameManager.Instance.WinGame(reason: "You've defeated all the waves");

//        }
//        else
//        {
//            // Start the next wave
//            StartCoroutine(StartNextWave());
//        }
//    }

//    public void SpawnBalloon(GameObject balloonPrefab)
//    {
//        GameObject balloon = Instantiate(balloonPrefab, waypoints[0].position, Quaternion.identity);
//        BalloonMovement movement = balloon.GetComponent<BalloonMovement>();
//        movement.waypoints = waypoints;

//        // Subscribe to balloon's OnDestroyed event
//        Balloon balloonScript = balloon.GetComponent<Balloon>();
//        balloonScript.OnDestroyed += OnBalloonDestroyed;
//        balloonScript.OnEndReached += OnBalloonDestroyed;
//    }

//    void OnBalloonDestroyed(Balloon balloon)
//    {
//        balloonsRemaining--;

//        // Unsubscribe from the event to avoid memory leaks
//        balloon.OnDestroyed -= OnBalloonDestroyed;
//        balloon.OnEndReached -= OnBalloonDestroyed;
//    }

//    public void ResetSpawnConfigurations()
//    {
//        currentWaveIndex = 0;
//        Debug.Log("Balloon Spawner configurations have been reset.");
//    }

//}






// ------------------ BalloonSpawner with waves changes: ------------------
using UnityEngine;
using System.Collections;
using System.Collections.Generic;

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

    public void StartSpawningWaves()
    {
        // We do nothing automatically for Multi, but Single Player can do wave 0 if we want
        StartCoroutine(SpawnWave(currentWaveIndex));
    }

    private IEnumerator SpawnWave(int waveIndex)
    {
        Debug.Log($"---- Spawning wave {waveIndex}");

        if (waveIndex >= waves.Count)
        {
            // No more waves
            yield break;
        }

        WaveData waveData = waves[waveIndex];
        //waveBalloonsRemaining = waveData.spawnInstructions.Count;
        waveBalloonsRemaining += waveData.spawnInstructions.Count; //Add the new wave's balloon amount + amount of WaveBalloons from prev waves which is still on map.

        foreach (var instruction in waveData.spawnInstructions)
        {
            yield return new WaitForSeconds(instruction.spawnDelay);
            SpawnWaveBalloon(instruction.balloonPrefab);
        }

        // Wait for wave balloons to be destroyed
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
            if (currentWaveIndex >= waves.Count)
            {
                yield return new WaitForSeconds(5f);
                GameManager.Instance.WinGame("You've defeated all the waves");
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
                GameManager.Instance.WinGame("You've defeated all the waves");
            }
            else
            {
                string waveDone = $"{{\"Type\":\"WaveDone\",\"WaveIndex\":{currentWaveIndex}}}";
                NetworkManager.Instance.SendMessageWithLengthPrefix(waveDone);
            }

            //string waveDone = $"{{\"Type\":\"WaveDone\",\"WaveIndex\":{currentWaveIndex}}}";
            //NetworkManager.Instance.SendMessageWithLengthPrefix(waveDone);
        }
    }


    public void SpawnBalloon(GameObject balloonPrefab)
    {
        GameObject balloon = Instantiate(balloonPrefab, waypoints[0].position, Quaternion.identity);
        BalloonMovement movement = balloon.GetComponent<BalloonMovement>();
        movement.waypoints = waypoints;

        // Subscribe to balloon's OnDestroyed event
        Balloon balloonScript = balloon.GetComponent<Balloon>();
        balloonScript.OnDestroyed += OnBalloonDestroyed;
        balloonScript.OnEndReached += OnBalloonDestroyed;
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

        WaveData waveData = waves[currentWaveIndex];
        // Wait waveData.delayBeforeWaveBegins
        if (waveData.delayBeforeWaveBegins > 0f)
        {
            yield return new WaitForSeconds(waveData.delayBeforeWaveBegins);
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
        // If it's wave balloon, check if it's final form
        // Actually, we only decrement waveBalloonsRemaining 
        // if b.nextBalloonPrefab == null 
        // meaning it doesn't spawn a successor balloon

        //Debug.Log("b.isWaveBalloon: " + b.isWaveBalloon);
        //Debug.Log("b.nextBalloonPrefab == null: " + (b.nextBalloonPrefab == null));
        if (b.isWaveBalloon && b.nextBalloonPrefab == null)
        {
            waveBalloonsRemaining--;
            Debug.Log("Wave balloon destroyed. Current num balloons: " + waveBalloonsRemaining);
        }


        b.OnDestroyed -= OnBalloonDestroyed;
        b.OnEndReached -= OnBalloonDestroyedByEnd;
    }


    public void OnBalloonDestroyedByEnd(Balloon b)
    {
        // If it's wave balloon, check if it's final form
        // Actually, we only decrement waveBalloonsRemaining 
        // if b.nextBalloonPrefab == null 
        // meaning it doesn't spawn a successor balloon

        if (b.isWaveBalloon)
        {
            waveBalloonsRemaining--;
            Debug.Log("Wave balloon destroyed. Current num balloons: " + waveBalloonsRemaining);
        }


        b.OnDestroyed -= OnBalloonDestroyed;
        b.OnEndReached -= OnBalloonDestroyedByEnd;
    }

    // ---------- Extra Balloon -----------
    public void SpawnExtraBalloon(GameObject prefab)
    {
        GameObject balloonGO = Instantiate(prefab, waypoints[0].position, Quaternion.identity);

        Balloon balloonScript = balloonGO.GetComponent<Balloon>();
        balloonScript.isWaveBalloon = false; // Extra balloon does not belong to wave

        // Optionally handle collisions, but do not track wave
        balloonScript.OnDestroyed += OnBalloonDestroyed;
        balloonScript.OnEndReached += OnBalloonDestroyedByEnd;

        BalloonMovement movement = balloonGO.GetComponent<BalloonMovement>();
        movement.waypoints = waypoints;
    }


    public void ResetSpawnConfigurations()
    {
        currentWaveIndex = 0;
        Debug.Log("Balloon Spawner configurations have been reset.");
    }

}
