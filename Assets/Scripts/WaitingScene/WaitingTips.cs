using System.Collections.Generic;

/// <summary>
/// Description:
/// Provides a collection of tip messages to be displayed in the waiting scene.
/// These tips offer gameplay strategies and advice, such as tower placement, resource management,
/// and using special abilities effectively.
/// </summary>
public static class WaitingTips
{
    /// <summary>
    /// Returns a list of tip messages for the waiting scene.
    /// The returned tips cover various aspects of gameplay, such as strategy, tower placement, and resource management.
    /// </summary>
    /// <returns>A List of strings, each containing a tip message.</returns>
    public static List<string> GetMessages()
    {
        return new List<string>
        {
            "Viewing the snapshots of the other player in Multiplayer mode can help develop a strategy against him.",
            "Place your towers strategically to maximize their range and efficiency.",
            "Different towers are effective against different types of balloons. Experiment to find the best combination!",
            "Upgrade your towers to handle stronger balloons as the game progresses.",
            "Use your resources wisely – balancing upgrades and new towers is the key to success.",
            "Pay attention to balloon types – some may require special strategies.",
            "Don't forget to check out new upgrades and abilities as they become available.",
            "Managing your economy is critical. Make sure you’re earning enough to support your defense.",
            "In Multiplayer mode, save enough money for sending balloons to your opponent when he is weak.",
            "You have special abilities. Remember to use them!",
            "Remember to place towers where they have a clear line of sight to maximize their effectiveness.",
            "Before placing a new tower, view its range to better understand where exactly to place it.",
            "Use abilities strategically to turn the tide in challenging rounds.",
            "Experiment with different tower placements and upgrades to find the ultimate strategy.",
            "Sometimes it's better to sell underperforming towers and invest in upgrades for others.",
            "Always have a backup plan – a few well-placed towers can save you in a pinch!",
            "Remember, you can sell towers and get half the money back.",
            "Early rounds are a great time to experiment with new tower placements and combinations.",
            "Save powerful abilities for tough rounds – they can make a big difference!",
            "Keep an eye on your income – higher income means more powerful defenses later.",
            "Remember! Some balloons move faster than others.",
            "Combine towers with different abilities to create an unstoppable defense!",
            "Remember to have a solid defense before the stronger balloons start to show up.",
            "Adapt your defense as the game progresses – what works early on might not work later!"
        };
    }
}
