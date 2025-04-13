/// <summary>
/// Description:
/// Holds network configuration data such as the server IP address and port.
/// The default values act as fallbacks if the configuration file is missing or cannot be read.
/// The [System.Serializable] attribute is required for Unity’s JsonUtility to serialize/deserialize this object.
/// </summary>
[System.Serializable]
public class NetworkConfig
{
    /// <summary>
    /// The server IP address. Default fallback is "127.0.0.1".
    /// </summary>
    public string serverIP = "127.0.0.1";  // Default fallback value

    /// <summary>
    /// The server port. Default fallback is 5555.
    /// </summary>
    public int port = 5555;              // Default fallback value
}
