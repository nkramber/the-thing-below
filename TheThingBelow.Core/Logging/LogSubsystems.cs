namespace TheThingBelow.Core.Logging;

/// <summary>
/// The subsystem names that a log entry carries (D-179). One name for each part that logs, so
/// a reader of a log file can filter by one text (T-1).
/// </summary>
/// <remarks>
/// PR-7 and each later PR that logs add the name of the subsystem that it writes, such as the
/// battle or the region map. Core holds every name, because Game and the tools read the same
/// set (D-179, D-494).
/// </remarks>
public static class LogSubsystems
{
    /// <summary>The run itself: the intents of a step, and the menu (D-162, D-650).</summary>
    public const string Run = "run";

    /// <summary>The world of a step, which a menu pauses (D-162).</summary>
    public const string World = "world";

    /// <summary>The host: the boot, the files of the person, and a crash (D-100, D-170).</summary>
    public const string Game = "game";

    /// <summary>The battle: each action, each down, and the end of an encounter (D-168, D-532).</summary>
    public const string Battle = "battle";
}
