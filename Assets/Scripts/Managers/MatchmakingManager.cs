using UnityEngine;
using UnityEngine.SceneManagement;
using System.Collections;

public class MatchmakingManager : MonoBehaviour
{
    public static MatchmakingManager Instance;

    void Awake()
    {
        // Singleton pattern
        if (Instance == null)
        {
            Instance = this;
            Debug.Log("MatchmakingManager has awaken!");
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
        // Start a coroutine to delay the scene load
        StartCoroutine(DelayedMatchStart());
    }

    private IEnumerator DelayedMatchStart()
    {
        yield return new WaitForSeconds(1f); // 1 second delay between the moment a match is found and the moment the game begins, for better user experience.

        //Stop the actions on the WaitingScene (such as loading spinner and timer counting)
        var waitingSceneManager = FindObjectOfType<WaitingSceneManager>();
        if (waitingSceneManager != null) waitingSceneManager.StopWaiting();

        SceneManager.LoadScene("SampleScene");
    }

}
