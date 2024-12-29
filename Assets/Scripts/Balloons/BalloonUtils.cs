using UnityEngine;

public class BalloonUtils : MonoBehaviour
{
    // Singleton instance
    private static BalloonUtils _instance;
    public static BalloonUtils Instance
    {
        get
        {
            if (_instance == null)
            {
                Debug.LogError("BalloonUtils instance is not initialized. Ensure the script is added to a GameObject in the scene.");
            }
            return _instance;
        }
    }

    // Balloon prefabs
    public GameObject redBalloonPrefab;
    public GameObject blueBalloonPrefab;
    public GameObject greenBalloonPrefab;
    public GameObject yellowBalloonPrefab;
    public GameObject pinkBalloonPrefab;
    public GameObject blackBalloonPrefab;
    public GameObject whiteBalloonPrefab;
    public GameObject strongBalloonPrefab;
    public GameObject strongerBalloonPrefab;
    public GameObject veryStrongBalloonPrefab;
    public GameObject smallBossBalloonPrefab;
    public GameObject mediumBossBalloonPrefab;
    public GameObject bigBossBalloonPrefab;

    // Ensures the Singleton instance is set up.
    private void Awake()
    {
        if (_instance != null && _instance != this)
        {
            Debug.LogWarning("Multiple BalloonUtils instances found! Destroying duplicate.");
            Destroy(gameObject);
            return;
        }

        _instance = this;
    }

    public GameObject GetBalloonPrefabByName(string balloonType)
    {
        switch (balloonType)
        {
            case "RedBalloon":
                return redBalloonPrefab;
            case "BlueBalloon":
                return blueBalloonPrefab;
            case "GreenBalloon":
                return greenBalloonPrefab;
            case "YellowBalloon":
                return yellowBalloonPrefab;
            case "PinkBalloon":
                return pinkBalloonPrefab;
            case "BlackBalloon":
                return blackBalloonPrefab;
            case "WhiteBalloon":
                return whiteBalloonPrefab;
            case "StrongBalloon":
                return strongBalloonPrefab;
            case "StrongerBalloon":
                return strongerBalloonPrefab;
            case "VeryStrongBalloon":
                return veryStrongBalloonPrefab;
            case "SmallBossBalloon":
                return smallBossBalloonPrefab;
            case "MediumBossBalloon":
                return mediumBossBalloonPrefab;
            case "BigBossBalloon":
                return bigBossBalloonPrefab;
            default:
                Debug.LogWarning($"Balloon type '{balloonType}' not recognized.");
                return null;
        }
    }
}
