using System;

namespace TheThingBelow.Core.Maps;

/// <summary>The numbers of the map: the length of one step, and the sight range of the party.</summary>
/// <remarks>
/// A step takes a fixed count of ticks, and Game slides the sprite across them (D-164,
/// D-203). Thus every step of every direction costs the same, and a replay lands on the same
/// tile at the same tick (T-7, D-716).
/// </remarks>
public static class MapRules
{
    /// <summary>
    /// The count of world ticks that one step takes (D-164, D-203). The loop runs 60 ticks a
    /// second, so the party walks four tiles a second.
    /// </summary>
    public const int TicksPerStep = 15;

    /// <summary>
    /// Gives the sight range of the party on a map of one time of day, in tiles (D-193,
    /// D-719).
    /// </summary>
    /// <param name="time">The time of day of the map (D-442).</param>
    /// <returns>The range, in tiles.</returns>
    /// <exception cref="ArgumentOutOfRangeException">The value names no time (T-2).</exception>
    /// <remarks>
    /// The frame holds 20 by 11.25 tiles (D-633). A range of 12 reaches past every edge of
    /// that frame, so a day map hides no enemy that the player can see. A range of 5 holds
    /// the party inside its own part of the frame, so a night map hides a patrol until it
    /// comes close. This is what gives a night map its threat, because no fog of war covers
    /// the ground (D-566).
    /// </remarks>
    public static int PartySightRange(TimeOfDay time) => time switch
    {
        TimeOfDay.Dawn => 8,
        TimeOfDay.Day => 12,
        TimeOfDay.Dusk => 8,
        TimeOfDay.Night => 5,
        _ => throw new ArgumentOutOfRangeException(nameof(time), time, "the value names no time of day (D-442)"),
    };

    /// <summary>Tells whether the party can step onto one tile of a map.</summary>
    /// <param name="map">The map.</param>
    /// <param name="at">The tile that the step reaches, which can lie outside the map.</param>
    /// <returns>True when the tile lies inside the map and its kind takes a step.</returns>
    /// <exception cref="ArgumentNullException">The map is null (T-2).</exception>
    /// <remarks>
    /// A door, a lock, a chest, and a save point answer the step into them in PR-16 and
    /// PR-64. This build reads the terrain alone, so a thing never blocks a step yet.
    /// </remarks>
    public static bool CanEnter(GameMap map, TilePoint at)
    {
        ArgumentNullException.ThrowIfNull(map);

        return map.Holds(at) && TileKinds.CanWalk(map.TileAt(at));
    }
}
