using System;

namespace TheThingBelow.Core.Maps;

/// <summary>The search of the walk home of an NPC after a story scene (D-1140).</summary>
/// <remarks>
/// The search is a breadth-first search over the open ground of the map, in the order of
/// <see cref="StepDirections.All"/>. Thus it finds a shortest path to the nearest tile of the
/// home, and a tie between two paths of one length always goes the same way on every machine
/// (T-7). The search reads the ground and the things alone. It ignores the lead, the other NPCs,
/// and the enemies, because they move: the walk waits for a body on the path, and then it
/// searches again (D-1140).
/// </remarks>
public static class NpcPaths
{
    /// <summary>Finds the first step of a shortest path from one tile to the home of an NPC (D-1140).</summary>
    /// <param name="map">The map, whose open ground the path walks (D-1139).</param>
    /// <param name="npc">The record of the NPC, which gives its home.</param>
    /// <param name="from">The tile of the NPC, which lies outside its home.</param>
    /// <param name="first">The direction of the first step, or north when no path exists.</param>
    /// <param name="distance">The count of steps of the path, or zero when no path exists.</param>
    /// <returns>True when a path of open ground leads home.</returns>
    /// <exception cref="ArgumentNullException">The map or the record is null (T-2).</exception>
    /// <exception cref="ArgumentException">The tile lies off the map, or the home already holds it (T-2).</exception>
    /// <exception cref="OverflowException">A count passes the range of an `int` (T-2).</exception>
    /// <remarks>
    /// A tile is open when <see cref="NpcState.IsOpen"/> gives true: the tile takes a step and
    /// holds no thing (D-1139). The search marks each tile when it first reaches it, so the tile
    /// of the home that it reaches first is a nearest one.
    /// </remarks>
    public static bool TryFirstStepHome(GameMap map, Npc npc, TilePoint from, out StepDirection first, out int distance)
    {
        ArgumentNullException.ThrowIfNull(map);
        ArgumentNullException.ThrowIfNull(npc);
        if (!map.Holds(from))
        {
            throw new ArgumentException($"The NPC '{npc.Id.Value}' walks home from {from}, which lies off the map '{map.Id.Value}' (T-2).", nameof(from));
        }

        if (npc.HomeHolds(from))
        {
            throw new ArgumentException($"The NPC '{npc.Id.Value}' walks home from {from}, which its home already holds (D-1140, T-2).", nameof(from));
        }

        int cells = checked(map.Width * map.Height);

        // The first step of the path to each tile, and the count of steps to it, with -1 for a
        // tile that the search did not reach. The queue holds the index of each reached tile in
        // the order of its reach.
        var firstOf = new StepDirection[cells];
        int[] steps = new int[cells];
        Array.Fill(steps, -1);
        int[] queue = new int[cells];
        int head = 0;
        int tail = 0;

        int start = IndexOf(map, from);
        steps[start] = 0;
        queue[tail] = start;
        tail += 1;

        while (head < tail)
        {
            int index = queue[head];
            head += 1;
            var at = new TilePoint(index % map.Width, index / map.Width);
            foreach (StepDirection direction in StepDirections.All)
            {
                TilePoint next = at.Step(direction);
                if (!map.Holds(next) || !NpcState.IsOpen(map, next))
                {
                    continue;
                }

                int nextIndex = IndexOf(map, next);
                if (steps[nextIndex] >= 0)
                {
                    continue;
                }

                steps[nextIndex] = checked(steps[index] + 1);
                firstOf[nextIndex] = index == start ? direction : firstOf[index];
                if (npc.HomeHolds(next))
                {
                    first = firstOf[nextIndex];
                    distance = steps[nextIndex];
                    return true;
                }

                queue[tail] = nextIndex;
                tail += 1;
            }
        }

        first = StepDirection.North;
        distance = 0;
        return false;
    }

    private static int IndexOf(GameMap map, TilePoint at) => (at.Y * map.Width) + at.X;
}
