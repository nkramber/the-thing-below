using TheThingBelow.Core.Content;

namespace TheThingBelow.Core.Runs;

/// <summary>
/// The ids of every intent that the rules of this build read (D-493, D-646). Each later
/// Core PR adds the ids of its own screen, such as the steps of the map in PR-7.
/// </summary>
/// <remarks>
/// The kind of each id is `intent`. An id is permanent, so no later entry takes one (D-166).
/// The ids below open and close the menu, which pauses the world (D-162, D-650).
/// </remarks>
public static class IntentIds
{
    /// <summary>The kind of every intent id (D-646).</summary>
    public const string Kind = "intent";

    /// <summary>The file that holds these ids, for the error of a malformed id (T-2).</summary>
    private const string Source = "TheThingBelow.Core/Runs/IntentIds.cs";

    /// <summary>The player opened a menu, and the world pauses from this tick (D-162).</summary>
    public static readonly ContentId OpenMenu = ContentId.Parse("intent.open_menu", Source, nameof(OpenMenu));

    /// <summary>The player closed the menu, and the world runs again from this tick (D-162).</summary>
    public static readonly ContentId CloseMenu = ContentId.Parse("intent.close_menu", Source, nameof(CloseMenu));
}
