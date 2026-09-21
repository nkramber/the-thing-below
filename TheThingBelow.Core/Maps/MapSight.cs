using System;

namespace TheThingBelow.Core.Maps;

/// <summary>
/// What the party sees, and what a patrol sees (D-37, D-718, D-719). A wall stops the sight
/// of each one, and the rule uses integer math alone (T-7).
/// </summary>
/// <remarks>
/// No fog of war covers a map, so the ground of a map is visible from the moment the party
/// enters (D-566). This rule decides which enemies and things Game draws, and whether a
/// patrol notices the party. The light of the screen never reaches it (G-1).
/// <para>
/// The party sees every direction out to its range, because a fixed camera already shows the
/// room and a player that turns the lead to look would find that tedious (D-719). A patrol
/// sees the quarter of the map that it faces, plus the eight tiles that touch it (D-718).
/// </para>
/// <para>
/// PR-8 walks each patrol and calls <see cref="PatrolSees"/>. A map gives no patrol a longer
/// range than the party has on that map, and the load of PR-8 refuses one (D-720).
/// </para>
/// </remarks>
public static class MapSight
{
    /// <summary>The range at which a patrol notices a tile in every direction (D-718).</summary>
    /// <remarks>
    /// A patrol notices anything that touches it, whichever way it faces. Without this rule
    /// the party walks along the flank of a patrol and stays unseen, which reads as a fault
    /// of the game.
    /// </remarks>
    public const int PatrolTouchRange = 1;

    /// <summary>Tells whether the party at one tile sees another tile (D-719).</summary>
    /// <param name="map">The map that both tiles lie on.</param>
    /// <param name="from">The tile of the lead.</param>
    /// <param name="at">The tile of the enemy or the thing.</param>
    /// <param name="range">The sight range of the party, which the time of day gives (D-193).</param>
    /// <returns>True when the party sees that tile.</returns>
    /// <exception cref="ArgumentNullException">The map is null (T-2).</exception>
    /// <exception cref="ArgumentOutOfRangeException">The range is below zero, or a tile lies outside the map (T-2).</exception>
    public static bool PartySees(GameMap map, TilePoint from, TilePoint at, int range)
    {
        ArgumentNullException.ThrowIfNull(map);
        ArgumentOutOfRangeException.ThrowIfNegative(range);
        RefuseOutside(map, from, nameof(from));
        RefuseOutside(map, at, nameof(at));

        return Reach(from, at) <= range && Clear(map, from, at);
    }

    /// <summary>Tells whether a patrol at one tile sees another tile (D-718).</summary>
    /// <param name="map">The map that both tiles lie on.</param>
    /// <param name="from">The tile of the patrol.</param>
    /// <param name="facing">The direction that the patrol faces, which carries its sight (D-208).</param>
    /// <param name="range">The sight range of the patrol, in tiles.</param>
    /// <param name="at">The tile of the party.</param>
    /// <returns>True when the patrol sees that tile.</returns>
    /// <exception cref="ArgumentNullException">The map is null (T-2).</exception>
    /// <exception cref="ArgumentOutOfRangeException">The range is below zero, or a tile lies outside the map (T-2).</exception>
    public static bool PatrolSees(GameMap map, TilePoint from, StepDirection facing, int range, TilePoint at)
    {
        ArgumentNullException.ThrowIfNull(map);
        ArgumentOutOfRangeException.ThrowIfNegative(range);
        RefuseOutside(map, from, nameof(from));
        RefuseOutside(map, at, nameof(at));

        if (!Clear(map, from, at))
        {
            return false;
        }

        if (Reach(from, at) <= PatrolTouchRange)
        {
            return true;
        }

        Forward(from, at, facing, out int ahead, out int beside);

        // The quarter of the map that the patrol faces: the tile lies ahead of it, and it
        // lies no further to the side than it lies ahead (D-718). The shape needs no
        // division, so Core keeps integer math (T-7).
        return ahead >= 1 && ahead <= range && beside <= ahead;
    }

    /// <summary>
    /// Tells whether one tile lies in the quarter behind a facing (D-265, D-746). The side
    /// that reached the other from behind acts first in the fight.
    /// </summary>
    /// <param name="from">The tile of the side that faces.</param>
    /// <param name="facing">The direction that this side faces (D-207, D-718).</param>
    /// <param name="at">The tile of the other side.</param>
    /// <returns>True when that tile lies behind the facing.</returns>
    /// <exception cref="ArgumentOutOfRangeException">The value names no direction (T-2).</exception>
    /// <exception cref="OverflowException">A distance passes the range of an `int` (T-2).</exception>
    /// <remarks>
    /// The quarter behind a facing is the mirror of the quarter that <see cref="PatrolSees"/>
    /// reads, so one shape serves the sight of a patrol and the approach of each side
    /// (D-718, D-746, T-1). A wall reaches no part of this rule, because the encounter
    /// already started.
    /// </remarks>
    public static bool BehindFacing(TilePoint from, StepDirection facing, TilePoint at)
    {
        Forward(from, at, facing, out int ahead, out int beside);
        return ahead <= -1 && beside <= -ahead;
    }

    /// <summary>
    /// Gives the reach between two tiles: the larger of the two axis distances. A step goes
    /// in four directions alone, and this reach counts a diagonal as one (D-716, D-719).
    /// </summary>
    /// <param name="from">The first tile.</param>
    /// <param name="at">The second tile.</param>
    /// <returns>The reach, in tiles.</returns>
    public static int Reach(TilePoint from, TilePoint at)
    {
        int across = Math.Abs(checked(at.X - from.X));
        int down = Math.Abs(checked(at.Y - from.Y));
        return across > down ? across : down;
    }

    /// <summary>Tells whether no wall stands between two tiles (D-718, D-719).</summary>
    /// <param name="map">The map that both tiles lie on.</param>
    /// <param name="from">The first tile.</param>
    /// <param name="at">The second tile.</param>
    /// <returns>True when sight passes from one tile to the other.</returns>
    /// <exception cref="ArgumentNullException">The map is null (T-2).</exception>
    /// <exception cref="ArgumentOutOfRangeException">A tile lies outside the map (T-2).</exception>
    /// <remarks>
    /// The walk starts at the lower of the two tiles in one fixed order, so the answer is
    /// the same for both directions. An order by the caller would let a patrol see the party
    /// on a line where the party sees no patrol, which a player reads as a fault (T-2).
    /// <para>
    /// The two end tiles take no check. The viewer stands on the first, and the second holds
    /// the thing that the caller asks about.
    /// </para>
    /// </remarks>
    public static bool Clear(GameMap map, TilePoint from, TilePoint at)
    {
        ArgumentNullException.ThrowIfNull(map);
        RefuseOutside(map, from, nameof(from));
        RefuseOutside(map, at, nameof(at));

        TilePoint start = Lower(from, at);
        TilePoint end = start == from ? at : from;
        return NoWallBetween(map, start, end);
    }

    /// <summary>
    /// Walks the tiles of the straight line from one tile to another, and reads each tile
    /// between them. The walk uses whole numbers alone (T-7).
    /// </summary>
    /// <remarks>
    /// The walk can take one diagonal move, which passes the point where two walls meet.
    /// Sight through that point is the behavior of every line of this kind, and the party
    /// cannot walk through it, because a step goes in four directions alone (D-716).
    /// </remarks>
    private static bool NoWallBetween(GameMap map, TilePoint start, TilePoint end)
    {
        // One tile always sees itself, and the walk below needs two tiles to move between.
        if (start == end)
        {
            return true;
        }

        int across = Math.Abs(checked(end.X - start.X));
        int down = -Math.Abs(checked(end.Y - start.Y));
        int stepX = start.X < end.X ? 1 : -1;
        int stepY = start.Y < end.Y ? 1 : -1;
        int error = across + down;
        int x = start.X;
        int y = start.Y;

        while (true)
        {
            int twice = checked(2 * error);
            if (twice >= down)
            {
                error = checked(error + down);
                x = checked(x + stepX);
            }

            if (twice <= across)
            {
                error = checked(error + across);
                y = checked(y + stepY);
            }

            if (x == end.X && y == end.Y)
            {
                return true;
            }

            if (TileKinds.StopsSight(map.TileAt(new TilePoint(x, y))))
            {
                return false;
            }
        }
    }

    private static void Forward(TilePoint from, TilePoint at, StepDirection facing, out int ahead, out int beside)
    {
        int across = checked(at.X - from.X);
        int down = checked(at.Y - from.Y);
        switch (facing)
        {
            case StepDirection.North:
                ahead = -down;
                beside = Math.Abs(across);
                return;
            case StepDirection.South:
                ahead = down;
                beside = Math.Abs(across);
                return;
            case StepDirection.East:
                ahead = across;
                beside = Math.Abs(down);
                return;
            case StepDirection.West:
                ahead = -across;
                beside = Math.Abs(down);
                return;
            default:
                throw new ArgumentOutOfRangeException(
                    nameof(facing),
                    facing,
                    "the value names no step direction (D-716)");
        }
    }

    private static TilePoint Lower(TilePoint one, TilePoint other)
    {
        if (one.X != other.X)
        {
            return one.X < other.X ? one : other;
        }

        return one.Y <= other.Y ? one : other;
    }

    private static void RefuseOutside(GameMap map, TilePoint at, string name)
    {
        if (!map.Holds(at))
        {
            throw new ArgumentOutOfRangeException(
                name,
                at,
                $"The map '{map.Id.Value}' is {map.Width} by {map.Height} tiles, and this tile lies outside it (T-2).");
        }
    }
}
