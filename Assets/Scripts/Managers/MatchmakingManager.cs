using UnityEngine;
using UnityEngine.SceneManagement;

public class MatchmakingManager : MonoBehaviour
{
    public static MatchmakingManager Instance;

    void Awake()
    {
        // Singleton pattern
        if (Instance == null)
        {
            Instance = this;
            DontDestroyOnLoad(gameObject);
        }
        else
            Destroy(gameObject);
    }

    void Start()
    {
        // Optionally, display any initial messages or animations
    }

    public void OnMatchFound()
    {
        // Transition to the Game scene
        SceneManager.LoadScene("SampleScene");
    }
}
