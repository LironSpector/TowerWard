using UnityEngine;
using UnityEngine.SceneManagement;

/// <summary>
/// Global settings that manage various game-wide options, including the display mode for statistics
/// and the time scale. The time scale can be adjusted for single-player mode based on whether extreme speed
/// is enabled, but is forced to 1 in multiplayer and menu scenes.
/// </summary>
public static class GlobalSettings
{
    /// <summary>
    /// Determines the mode for displaying statistics. 0 = no stats displayed, 1 = display FPS.
    /// </summary>
    public static int StatsDisplayMode = 0;

    /// <summary>
    /// Indicates whether extreme speed is enabled in single-player games.
    /// When true, the time scale may be increased (e.g., to 2x speed).
    /// </summary>
    public static bool ExtremeSpeedEnabled = false;

    /// <summary>
    /// Applies the appropriate time scale based on the current scene and game mode.
    /// In multiplayer or menu scenes, the time scale is forced to 1.
    /// In single-player scenes, if ExtremeSpeedEnabled is true, the time scale is set to 2;
    /// otherwise, it is set to 1.
    /// </summary>
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

        if (ExtremeSpeedEnabled)
            Time.timeScale = 2f;
        else
            Time.timeScale = 1f;
    }

    /// <summary>
    /// Checks whether the current game context is either multiplayer or a non-game scene (MainMenu or WaitingScene).
    /// This helps determine whether time scaling for extreme speed should be applied.
    /// </summary>
    /// <returns>
    /// True if the game is in multiplayer mode (via GameManager) or if the active scene is "MainMenu" or "WaitingScene";
    /// otherwise, false.
    /// </returns>
    static bool CheckIfInMultiplayerOrMenu()
    {
        if (GameManager.Instance != null)
        {
            if (GameManager.Instance.CurrentGameMode == GameManager.GameMode.Multiplayer)
            {
                return true;
            }
        }

        string sceneName = SceneManager.GetActiveScene().name;
        if (sceneName == "MainMenu" || sceneName == "WaitingScene")
        {
            return true;
        }

        return false;
    }
}
