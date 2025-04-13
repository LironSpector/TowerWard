using UnityEngine;
using UnityEngine.SceneManagement;
using System.Collections;

/// <summary>
/// Description:
/// Manages the matchmaking process for multiplayer games. Implements a singleton pattern
/// and handles the transition from the waiting scene to the game scene once a match is found.
/// </summary>
public class MatchmakingManager : MonoBehaviour
{
    /// <summary>
    /// The singleton instance of the MatchmakingManager.
    /// </summary>
    public static MatchmakingManager Instance;

    /// <summary>
    /// Awake is called when the script instance is being loaded.
    /// Implements the singleton pattern to ensure only one instance exists.
    /// </summary>
    void Awake()
    {
        if (Instance == null)
        {
            Instance = this;
            Debug.Log("MatchmakingManager has awaken!");
        }
        else
        {
            Destroy(gameObject);
        }
    }

    /// <summary>
    /// Called on the first frame. Use this method to initialize animations or display initial messages if needed.
    /// </summary>
    void Start()
    {
        // Optionally, display any initial messages or animations.
    }

    /// <summary>
    /// Called when a match has been found.
    /// Starts a coroutine to delay the scene transition for better user experience.
    /// </summary>
    public void OnMatchFound()
    {
        StartCoroutine(DelayedMatchStart());
    }

    /// <summary>
    /// Coroutine that delays the start of the match.
    /// Waits for 1 second before stopping any waiting scene activities and then loads the game scene.
    /// </summary>
    /// <returns>An IEnumerator for coroutine handling.</returns>
    private IEnumerator DelayedMatchStart()
    {
        yield return new WaitForSeconds(1f); // 1 second delay for user experience

        // Stop any ongoing activities in the WaitingScene, such as loading spinners or timers.
        var waitingSceneManager = FindObjectOfType<WaitingSceneManager>();
        if (waitingSceneManager != null)
        {
            waitingSceneManager.StopWaiting();
        }

        // Transition to the main game scene.
        SceneManager.LoadScene("SampleScene");
    }
}
