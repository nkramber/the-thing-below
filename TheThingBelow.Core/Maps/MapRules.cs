using System;
using System.Collections.Generic;

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
    /// The count of world ticks that one step of the party takes (D-164, D-203, D-821). The
    /// loop runs 60 ticks a second, so the party walks 3.75 tiles a second.
    /// </summary>
    /// <remarks>
    /// A tile is 32 art pixels, so each tick of a step moves the lead by 2 art pixels, and the
    /// slide shows no uneven tick (D-228, D-821).
    /// </remarks>
    public const int TicksPerStep = 16;

    /// <summary>
    /// The counts of world ticks that one step of an enemy can take (D-742, D-821). Each count
    /// divides the tile of 32 art pixels or is a multiple of it, so each tick moves the sprite
    /// by 2 pixels, by 1 pixel, or by 1 pixel every other tick. None is faster than the party.
    /// </summary>
    public static IReadOnlyList<int> EnemyStepTicks { get; } = [16, 32, 64];

    /// <summary>Tells whether an enemy can step in a count of ticks (D-821).</summary>
    /// <param name="ticks">The count of ticks of one step of the enemy.</param>
    /// <returns>True when the count is one of <see cref="EnemyStepTicks"/>.</returns>
    public static bool IsEnemyStep(int ticks)
    {
        foreach (int allowed in EnemyStepTicks)
        {
            if (ticks == allowed)
            {
                return true;
            }
        }

        return false;
    }

    /// <summary>
    /// The count of world ticks of the mark that a patrol shows before an encounter starts
    /// (D-208, D-745). The loop runs 60 ticks a second, so the mark lasts half a second.
    /// </summary>
    /// <remarks>
    /// Core counts the beat, and Game draws the mark over exactly these ticks. Thus the beat
    /// replays from the seed with no intent, and a bot meets the same map as a player
    /// (D-522, D-745, T-7). Nothing cancels the beat.
    /// </remarks>
    public const int BeatTicks = 30;

    /// <summary>
    /// The count of world ticks after a flee in which no battle with that enemy starts
    /// (D-381, D-748). The loop runs 60 ticks a second, so the grace time lasts 5 seconds.
    /// </summary>
    /// <remarks>
    /// The party walks 3.75 tiles a second, so 5 seconds carry it 18 tiles. That clears the
    /// longest sight of a patrol, because the 12 tiles of the party on a map set to day cap
    /// it (D-720).
    /// </remarks>
    public const int GraceTicks = 300;

    /// <summary>
    /// Gives the sight range of the party on a map of one time of day, in tiles (D-193,
    /// D-720).
    /// </summary>
    /// <param name="time">The time of day of the map (D-442).</param>
    /// <returns>The range, in tiles.</returns>
    /// <exception cref="ArgumentOutOfRangeException">The value names no time (T-2).</exception>
    /// <remarks>
    /// The range decides nothing that Game draws, because every live enemy draws at any
    /// distance (D-814). It sets the ceiling of the sight of each patrol on a map of that
    /// time, so a night map holds patrols that see less far (D-720).
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

    /// <summary>Tells whether the body of an enemy can stand on one map (D-206, D-209).</summary>
    /// <param name="map">The map.</param>
    /// <param name="body">The body, which can lie outside the map.</param>
    /// <returns>True when every tile of the body lies inside the map and takes a step.</returns>
    /// <exception cref="ArgumentNullException">The map is null (T-2).</exception>
    /// <exception cref="OverflowException">A count passes the range of an `int` (T-2).</exception>
    /// <remarks>
    /// The load of a map reads this rule for each tile of a route and for each anchor tile of
    /// an area, and the walk of an enemy reads it for each step (D-739, D-741).
    /// </remarks>
    public static bool CanPlace(GameMap map, EnemyBody body)
    {
        ArgumentNullException.ThrowIfNull(map);

        int side = body.Side;
        for (int row = 0; row < side; row += 1)
        {
            for (int column = 0; column < side; column += 1)
            {
                var at = new TilePoint(checked(body.Anchor.X + column), checked(body.Anchor.Y + row));
                if (!CanEnter(map, at))
                {
                    return false;
                }
            }
        }

        return true;
    }

    /// <summary>Tells whether two bodies hold one tile in common (D-206).</summary>
    /// <param name="one">The first body.</param>
    /// <param name="other">The second body.</param>
    /// <returns>True when the two bodies share a tile.</returns>
    /// <exception cref="OverflowException">A count passes the range of an `int` (T-2).</exception>
    public static bool BodiesOverlap(EnemyBody one, EnemyBody other)
    {
        int side = other.Side;
        for (int row = 0; row < side; row += 1)
        {
            for (int column = 0; column < side; column += 1)
            {
                if (one.Holds(new TilePoint(checked(other.Anchor.X + column), checked(other.Anchor.Y + row))))
                {
                    return true;
                }
            }
        }

        return false;
    }
}
