using TheThingBelow.Core.Content;

namespace TheThingBelow.Core.Maps;

/// <summary>The ids of the maps that a rule of this build names (D-528, D-646).</summary>
/// <remarks>
/// A map file holds each map, and no rule names a map by its path (D-495, D-528). This build
/// opens one map, because PR-7 ships the first fixture dungeon. PR-68 gives the story the
/// first map of a run, and PR-16 gives the party a way from one map to the next.
/// </remarks>
public static class MapIds
{
    /// <summary>The file that holds these ids, for the error of a malformed id (T-2).</summary>
    private const string Source = "TheThingBelow.Core/Maps/MapIds.cs";

    /// <summary>
    /// The map that a run opens, which is the fixture dungeon of PR-7. A save of format 1
    /// holds no map, so its migration puts the party on the spawn point of this map (D-166).
    /// </summary>
    public static readonly ContentId FirstMap = ContentId.Parse("map.fixture_dungeon", Source, nameof(FirstMap));
}
