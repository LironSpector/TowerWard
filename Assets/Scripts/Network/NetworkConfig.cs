[System.Serializable]
public class NetworkConfig
{
    public string serverIP = "127.0.0.1";  // Default fallback value
    public int port = 5555;              // Default fallback value
}

//Explanation:
// The default values here act as a fallback if the file is missing or cannot be read.
// The [System.Serializable] attribute is required for Unity’s JsonUtility to work.