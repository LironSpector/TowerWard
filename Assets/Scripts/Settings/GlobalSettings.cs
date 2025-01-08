using UnityEngine;

public static class GlobalSettings
{
    public static int StatsDisplayMode = 0;       // 0=none,1=fps
    public static bool ExtremeSpeedEnabled = false;

    // Called whenever we want to confirm the timeScale
    public static void ApplyTimeScaleIfPossible()
    {
        // 1) If currently in a single-player game scene => we can do Time.timeScale=2 if desired
        // 2) If in multiplayer or main menu => force Time.timeScale=1
        // 3) If "ExtremeSpeedEnabled" is false => timeScale=1

        if (CheckIfInMultiplayerOrMenu())
        {
            Time.timeScale = 1f;
            return;
        }

        // else if single-player game scene:
        if (ExtremeSpeedEnabled) Time.timeScale = 2f;
        else Time.timeScale = 1f;
    }

    // A small check to see if we're in main menu or multiplayer
    static bool CheckIfInMultiplayerOrMenu()
    {
        // Option A: Check if "GameManager.Instance.CurrentGameMode == Multiplayer" 
        // or Scene name is "MainMenu" or "WaitingScene"
        // or check a custom bool.

        if (GameManager.Instance != null)
        {
            // if in multiplayer
            if (GameManager.Instance.CurrentGameMode == GameManager.GameMode.Multiplayer)
            {
                return true;
            }
        }

        // if you want to check scene name:
        string sceneName = UnityEngine.SceneManagement.SceneManager.GetActiveScene().name;
        if (sceneName == "MainMenu" || sceneName == "WaitingScene")
        {
            return true;
        }

        return false;
    }
}
