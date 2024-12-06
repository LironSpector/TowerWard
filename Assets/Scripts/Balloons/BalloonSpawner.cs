using UnityEngine;
using System.Collections;
using System.Collections.Generic;

public class BalloonSpawner : MonoBehaviour
{
    public Transform[] waypoints; // Assign in Inspector

    private int currentWaveIndex = 0;

    // List of WaveData assets
    public List<WaveData> waves = new List<WaveData>();

    // New variables for balloon tracking
    private int balloonsToSpawn = 0;
    private int balloonsRemaining = 0;
    //private bool isSpawningWave = false;

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

    //removing this method - waves will not longer start automatically
    //void Start()
    //{
    //    StartCoroutine(StartNextWave());
    //}

    public void StartSpawningWaves()
    {
        StartCoroutine(StartNextWave());
    }


    IEnumerator StartNextWave()
    {
        if (currentWaveIndex >= waves.Count)
        {
            // No more waves to spawn
            yield break;
        }

        //isSpawningWave = true;

        WaveData currentWave = waves[currentWaveIndex];
        balloonsToSpawn = currentWave.spawnInstructions.Count;
        balloonsRemaining = balloonsToSpawn;

        foreach (var instruction in currentWave.spawnInstructions)
        {
            yield return new WaitForSeconds(instruction.spawnDelay);
            SpawnBalloon(instruction.balloonPrefab);
        }

        //isSpawningWave = false;

        // Wait until all balloons are destroyed before starting the next wave
        StartCoroutine(CheckForNextWave());
    }

    IEnumerator CheckForNextWave()
    {
        // Wait until all balloons are destroyed
        while (balloonsRemaining > 0)
        {
            yield return null; // Wait for next frame
        }

        // All balloons are destroyed
        currentWaveIndex++;

        if (currentWaveIndex >= waves.Count)
        {
            // No more waves and all balloons are destroyed, player wins
            yield return new WaitForSeconds(5); //wait for a few more seconds because sometimes at high levels the "balloonsRemaining" is sometimes 0 a bit early.
            GameManager.Instance.WinGame();
        }
        else
        {
            // Start the next wave
            StartCoroutine(StartNextWave());
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

    void OnBalloonDestroyed(Balloon balloon)
    {
        balloonsRemaining--;

        // Unsubscribe from the event to avoid memory leaks
        balloon.OnDestroyed -= OnBalloonDestroyed;
        balloon.OnEndReached -= OnBalloonDestroyed;
    }

    public void ResetSpawnConfigurations()
    {
        currentWaveIndex = 0;
        Debug.Log("Balloon Spawner configurations have been reset.");
    }

}











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

//    void Start()
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
//            GameManager.Instance.WinGame();
//        }
//        else
//        {
//            // Start the next wave
//            StartCoroutine(StartNextWave());
//        }
//    }

//    void SpawnBalloon(GameObject balloonPrefab)
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
//}
