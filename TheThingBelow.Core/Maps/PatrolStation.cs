using System;
using System.Collections.Generic;

namespace TheThingBelow.Core.Maps;

/// <summary>
/// One station of a patrol: the times of day that pick it, and the ground that it covers
/// (D-739, D-741, D-743).
/// </summary>
/// <remarks>
/// A station holds a route or an area, and never both. A route is a list of tiles, and the
/// patrol walks to the last tile and then back down the list (D-739). An area is a rectangle,
/// and the enemy steps at random inside it (D-741).
/// <para>
/// The time of day of the map picks the station, and a time that no station names keeps the
/// enemy off the map at that time (D-743). Thus one field carries both parts of D-193: the
/// route by the time, and which enemies appear.
/// </para>
/// </remarks>
public sealed class PatrolStation
{
    private readonly TimeOfDay[] times;
    private readonly TilePoint[] tiles;

    /// <summary>Makes one station of a patrol.</summary>
    /// <param name="times">The times of day that pick this station, which holds at least one.</param>
    /// <param name="tiles">The tiles of the route, in order, or none for an area (D-739).</param>
    /// <param name="area">The rectangle of an area, or no value for a route (D-741).</param>
    /// <exception cref="ArgumentNullException">The times or the tiles are null (T-2).</exception>
    /// <exception cref="ArgumentException">
    /// The station holds no time, it holds a route and an area, or it holds neither (T-2).
    /// </exception>
    public PatrolStation(IReadOnlyList<TimeOfDay> times, IReadOnlyList<TilePoint> tiles, TileArea? area)
    {
        ArgumentNullException.ThrowIfNull(times);
        ArgumentNullException.ThrowIfNull(tiles);

        if (times.Count == 0)
        {
            throw new ArgumentException(
                "A station of a patrol names at least one time of day (D-743, T-2).",
                nameof(times));
        }

        bool route = tiles.Count > 0;
        if (route == area.HasValue)
        {
            throw new ArgumentException(
                $"A station of a patrol holds a route or an area, and never both. It holds {tiles.Count} route tiles, and the area value is {(area.HasValue ? "present" : "absent")} (D-739, D-741, T-2).",
                nameof(tiles));
        }

        // The copy walks each list, because a spread of an `IReadOnlyList` calls
        // `System.Linq` and G-1 keeps that assembly out of Core.
        this.times = new TimeOfDay[times.Count];
        for (int index = 0; index < times.Count; index += 1)
        {
            this.times[index] = times[index];
        }

        this.tiles = new TilePoint[tiles.Count];
        for (int index = 0; index < tiles.Count; index += 1)
        {
            this.tiles[index] = tiles[index];
        }

        this.Area = area;
    }

    /// <summary>The times of day that pick this station, in the order of the file (G-4).</summary>
    public IReadOnlyList<TimeOfDay> Times => this.times;

    /// <summary>The tiles of the route, in order, or none when this station is an area (D-739).</summary>
    public IReadOnlyList<TilePoint> Tiles => this.tiles;

    /// <summary>The rectangle of the area, or no value when this station is a route (D-741).</summary>
    public TileArea? Area { get; }

    /// <summary>The tile where the enemy of this station starts (D-739, D-741).</summary>
    /// <remarks>
    /// A route starts at its first tile, and an area starts at its north-west tile. The load
    /// of the map proves that the body fits at both (D-741, T-2).
    /// </remarks>
    public TilePoint Start => this.Area is TileArea area ? area.Anchor : this.tiles[0];

    /// <summary>
    /// Gives the direction and the length of the leg between two route tiles (D-739). Each
    /// leg of a route is straight and on one axis, because a step goes in four directions
    /// alone (D-716).
    /// </summary>
    /// <param name="from">The tile that the leg starts at.</param>
    /// <param name="to">The tile that the leg ends at.</param>
    /// <param name="direction">The direction of the leg, when the leg is straight.</param>
    /// <param name="length">The count of steps of the leg, when the leg is straight.</param>
    /// <returns>True when the two tiles differ and lie on one axis.</returns>
    /// <exception cref="OverflowException">A distance passes the range of an `int` (T-2).</exception>
    public static bool TryLeg(TilePoint from, TilePoint to, out StepDirection direction, out int length)
    {
        int across = checked(to.X - from.X);
        int down = checked(to.Y - from.Y);

        if (across != 0 && down != 0)
        {
            direction = StepDirection.North;
            length = 0;
            return false;
        }

        if (across == 0 && down == 0)
        {
            direction = StepDirection.North;
            length = 0;
            return false;
        }

        if (across != 0)
        {
            direction = across > 0 ? StepDirection.East : StepDirection.West;
            length = across > 0 ? across : -across;
            return true;
        }

        direction = down > 0 ? StepDirection.South : StepDirection.North;
        length = down > 0 ? down : -down;
        return true;
    }

    /// <summary>Tells whether the time of day of a map picks this station (D-743).</summary>
    /// <param name="time">The time of day of the map (D-442).</param>
    /// <returns>True when this station names that time.</returns>
    public bool Picks(TimeOfDay time)
    {
        foreach (TimeOfDay named in this.times)
        {
            if (named == time)
            {
                return true;
            }
        }

        return false;
    }
}
