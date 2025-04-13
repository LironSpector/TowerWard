using UnityEngine;
using System.Collections.Generic;

/// <summary>
/// Description:
/// Defines the data for a wave of balloons in the game. This ScriptableObject contains the delay before the wave begins
/// as well as a list of spawn instructions specifying which balloons to spawn, their initial health, and the delay between spawns.
/// </summary>
[CreateAssetMenu(menuName = "Wave Data")]
public class WaveData : ScriptableObject
{
    /// <summary>
    /// Describes a single spawn instruction for a balloon.
    /// Each instruction includes the balloon prefab to spawn, the delay after the previous balloon spawn,
    /// and the initial health to assign to the spawned balloon.
    /// </summary>
    [System.Serializable]
    public class SpawnInstruction
    {
        /// <summary>
        /// The balloon prefab to be spawned.
        /// </summary>
        public GameObject balloonPrefab;

        /// <summary>
        /// The delay (in seconds) after the previous balloon spawn before this balloon is spawned.
        /// </summary>
        public float spawnDelay;

        /// <summary>
        /// The initial health to assign to the spawned balloon.
        /// </summary>
        public int initialHealth;
    }

    /// <summary>
    /// The artificial delay (in seconds) before the wave starts.
    /// </summary>
    public float delayBeforeWaveBegins = 0f;

    /// <summary>
    /// The list of spawn instructions defining which balloons to spawn for this wave.
    /// </summary>
    public List<SpawnInstruction> spawnInstructions = new List<SpawnInstruction>();
}
