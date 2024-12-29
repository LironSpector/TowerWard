using UnityEngine;
using System.Collections.Generic;

[CreateAssetMenu(menuName = "Wave Data")]
public class WaveData : ScriptableObject
{
    [System.Serializable]
    public class SpawnInstruction
    {
        public GameObject balloonPrefab;
        public float spawnDelay; // Delay after the previous balloon
    }

    public float delayBeforeWaveBegins = 0f; // The artificial delay before this wave starts

    public List<SpawnInstruction> spawnInstructions = new List<SpawnInstruction>();
}
