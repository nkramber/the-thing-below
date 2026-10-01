using TheThingBelow.Core.Content;

namespace TheThingBelow.Core.Maps;

/// <summary>The ids of the maps that a rule of this build names (D-528, D-646).</summary>
/// <remarks>
/// A map file holds each map, and no rule names a map by its path (D-495, D-528). PR-7 opened
/// the first fixture dungeon, PR-16 gave the party a way from one map to the next, and PR-17
/// opens the run in the village of the first playable (D-1344, D-1351).
/// </remarks>
public static class MapIds
{
    /// <summary>The file that holds these ids, for the error of a malformed id (T-2).</summary>
    private const string Source = "TheThingBelow.Core/Maps/MapIds.cs";

    /// <summary>
    /// The map that a run opens: the village of the first playable, where the opening scene plays
    /// in the house of Marrek (D-1344).
    /// </summary>
    public static readonly ContentId FirstMap = ContentId.Parse("map.village", Source, nameof(FirstMap));

    /// <summary>
    /// The fixture dungeon of PR-7, which the smoke session and the screen fixtures of Game walk
    /// from its spawn point (D-117, D-172, D-767). The fixtures stay in the content for them (D-1351).
    /// A save of format 1 holds no map, so its migration puts the party on the spawn point of this
    /// map, the first map of the build that wrote it (D-166, D-654).
    /// </summary>
    public static readonly ContentId FixtureDungeon = ContentId.Parse("map.fixture_dungeon", Source, nameof(FixtureDungeon));
}
