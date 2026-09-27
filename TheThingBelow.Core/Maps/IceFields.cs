using System;
using System.Collections.Generic;

namespace TheThingBelow.Core.Maps;

/// <summary>The load check that each field of ice of a map has a way out (D-1232).</summary>
/// <remarks>
/// The lead slides over ice until the next tile is not ice or takes no step, so a field of ice can
/// hold the lead where each slide ends on the ice again. The check finds each tile of ice where a
/// slide can stop, and it proves that some chain of slides from that tile reaches ground that is
/// not ice.
/// <para>
/// The check reads each door as shut. An open door only adds a way out, so a map that passes with
/// each door shut passes with any door open. An enemy or an NPC can also stop a slide, and the
/// check leaves them out: each one walks on, and the lead can then slide again.
/// </para>
/// </remarks>
public static class IceFields
{
    /// <summary>Gives the fault of the ice of one map, or no value when each field of ice has a way out (D-1232).</summary>
    /// <param name="map">The map.</param>
    /// <returns>A phrase that names the first tile of ice with no way out, in row order, or null.</returns>
    /// <exception cref="ArgumentNullException">The map is null (T-2).</exception>
    public static string? FaultOf(GameMap map)
    {
        ArgumentNullException.ThrowIfNull(map);

        List<TilePoint> stops = StopsOf(map);
        var free = new bool[checked(map.Width * map.Height)];

        // Each pass marks each stop with a slide to ground or to a stop that an earlier pass
        // marked. The passes end when one pass marks nothing new.
        bool changed = true;
        while (changed)
        {
            changed = false;
            foreach (TilePoint stop in stops)
            {
                if (free[IndexOf(map, stop)])
                {
                    continue;
                }

                foreach (StepDirection direction in StepDirections.All)
                {
                    if (SlideEnd(map, stop, direction) is TilePoint end && (map.TileAt(end) != TileKind.Ice || free[IndexOf(map, end)]))
                    {
                        free[IndexOf(map, stop)] = true;
                        changed = true;
                        break;
                    }
                }
            }
        }

        foreach (TilePoint stop in stops)
        {
            if (!free[IndexOf(map, stop)])
            {
                return $"the lead can stop on the ice at {stop}, and no chain of slides from that tile reaches ground that is not ice (D-1232)";
            }
        }

        return null;
    }

    /// <summary>
    /// Gives each tile of ice where a slide can stop: a step can reach the tile in one direction,
    /// and the next tile in that direction takes no step. The list is in row order.
    /// </summary>
    private static List<TilePoint> StopsOf(GameMap map)
    {
        List<TilePoint> stops = [];
        for (int row = 0; row < map.Height; row += 1)
        {
            for (int column = 0; column < map.Width; column += 1)
            {
                var at = new TilePoint(column, row);
                if (map.TileAt(at) != TileKind.Ice)
                {
                    continue;
                }

                foreach (StepDirection direction in StepDirections.All)
                {
                    TilePoint behind = at.Step(StepDirections.Opposite(direction));
                    if (MapRules.CanEnter(map, behind) && !MapRules.CanEnter(map, at.Step(direction)))
                    {
                        stops.Add(at);
                        break;
                    }
                }
            }
        }

        return stops;
    }

    /// <summary>Gives the tile where a slide from one tile in one direction ends, or no value when the first step takes no step.</summary>
    private static TilePoint? SlideEnd(GameMap map, TilePoint from, StepDirection direction)
    {
        TilePoint at = from.Step(direction);
        if (!MapRules.CanEnter(map, at))
        {
            return null;
        }

        while (map.TileAt(at) == TileKind.Ice && MapRules.CanEnter(map, at.Step(direction)))
        {
            at = at.Step(direction);
        }

        return at;
    }

    private static int IndexOf(GameMap map, TilePoint at) => checked((at.Y * map.Width) + at.X);
}
