using System;
using System.Collections.Generic;
using TheThingBelow.Core.Content;

namespace TheThingBelow.Core.Maps;

/// <summary>
/// The rules that bind the NPCs of one map to its ground, its things, and each other: each
/// rectangle, each route, each start tile, and each target (D-739, D-1138, D-1139).
/// </summary>
/// <remarks>
/// The load of a map runs every check here, so no content author can place an NPC in a wall,
/// past an edge, on a thing, on the party, or on another body (T-2). Each check reads the
/// complete map, so the message of a fault names the map, the NPC, and the tile (T-2, G-18).
/// <para>
/// A rectangle can hold a thing that is not solid, such as a marker, and the walk of the NPC
/// treats that tile as a blocked step, which becomes a pause (D-1139). A route crosses no thing
/// at all, because a route NPC that meets a blocked tile waits for it to clear.
/// </para>
/// </remarks>
public static class NpcLayout
{
    /// <summary>Checks every NPC of one map, in the order of the file (G-4).</summary>
    /// <param name="reader">The reader of the map file, which names the file of a fault.</param>
    /// <param name="map">The map, with its terrain, its things, its enemies, and its NPCs.</param>
    /// <exception cref="ArgumentNullException">The map is null (T-2).</exception>
    /// <exception cref="ContentException">An NPC breaks a rule of a map (G-6, T-2).</exception>
    public static void Check(ref ContentReader reader, GameMap map)
    {
        ArgumentNullException.ThrowIfNull(map);

        RefuseRepeatedId(ref reader, map);
        foreach (Npc npc in map.Npcs)
        {
            if (npc.Move == NpcMove.Route)
            {
                CheckRoute(ref reader, map, npc);
            }
            else
            {
                CheckRange(ref reader, map, npc);
            }

            if (npc.Target is ContentId target && !map.PlacesNpc(target))
            {
                throw reader.Refuse(
                    $"the NPC '{npc.Id.Value}' chases '{target.Value}', and this map places no such NPC (D-1138)");
            }
        }

        RefuseCrowdedStart(ref reader, map);
    }

    /// <summary>
    /// Checks the range of a wander NPC or a chaser: each rectangle lies inside the map, each of
    /// its tiles takes a step, and the start tile lies inside a rectangle and holds no thing
    /// (D-1138, D-1139).
    /// </summary>
    private static void CheckRange(ref ContentReader reader, GameMap map, Npc npc)
    {
        foreach (TileArea area in npc.Areas)
        {
            // Each side reads against the room that the map leaves, and never as a sum. A sum of
            // a large coordinate and a side wraps below zero, and the area then passes (T-2).
            // The reader already proved each value at zero or more, so no difference wraps.
            if (area.Width > map.Width - area.X || area.Height > map.Height - area.Y)
            {
                throw reader.Refuse(
                    $"the NPC '{npc.Id.Value}' holds the rectangle {area}, and the map is {map.Width} by {map.Height} tiles (D-1138)");
            }

            for (int row = area.Y; row < checked(area.Y + area.Height); row += 1)
            {
                for (int column = area.X; column < checked(area.X + area.Width); column += 1)
                {
                    var at = new TilePoint(column, row);
                    if (!MapRules.CanEnter(map, at))
                    {
                        throw reader.Refuse(
                            $"the NPC '{npc.Id.Value}' holds the rectangle {area}, and its tile {at} takes no step. A range holds walkable tiles alone, and a list of rectangles takes any shape (D-1138)");
                    }
                }
            }
        }

        if (!npc.RangeHolds(npc.Start))
        {
            throw reader.Refuse(
                $"the NPC '{npc.Id.Value}' starts at {npc.Start}, and no rectangle of its range holds that tile (D-1138)");
        }

        RefuseThingAt(ref reader, map, npc, npc.Start, "starts on");
    }

    /// <summary>
    /// Checks the route of a route NPC: each leg is straight and on one axis, and each tile of
    /// each leg takes a step and holds no thing (D-716, D-739, D-1139).
    /// </summary>
    private static void CheckRoute(ref ContentReader reader, GameMap map, Npc npc)
    {
        IReadOnlyList<RouteStop> route = npc.Route;
        for (int index = 0; index < route.Count; index += 1)
        {
            RequireOpen(ref reader, map, npc, route[index].At);
        }

        for (int index = 0; index + 1 < route.Count; index += 1)
        {
            TilePoint from = route[index].At;
            TilePoint to = route[index + 1].At;
            if (!PatrolStation.TryLeg(from, to, out StepDirection direction, out int length))
            {
                throw reader.Refuse(
                    $"the NPC '{npc.Id.Value}' walks from {from} to {to}, and each leg of a route is straight and on one axis (D-716, D-739)");
            }

            TilePoint step = from;
            for (int count = 0; count < length; count += 1)
            {
                step = step.Step(direction);
                RequireOpen(ref reader, map, npc, step);
            }
        }
    }

    private static void RequireOpen(ref ContentReader reader, GameMap map, Npc npc, TilePoint at)
    {
        if (!MapRules.CanEnter(map, at))
        {
            throw reader.Refuse(
                $"the NPC '{npc.Id.Value}' holds the route tile {at}, which takes no step (D-739)");
        }

        RefuseThingAt(ref reader, map, npc, at, "walks across");
    }

    /// <summary>Refuses a tile of an NPC that a thing of the map holds, because an NPC never steps onto a thing (D-1139).</summary>
    private static void RefuseThingAt(ref ContentReader reader, GameMap map, Npc npc, TilePoint at, string verb)
    {
        IReadOnlyList<MapThing> things = map.ThingsAt(at);
        if (things.Count > 0)
        {
            throw reader.Refuse(
                $"the NPC '{npc.Id.Value}' {verb} {at}, which the thing '{things[0].Id.Value}' holds, and an NPC never steps onto a thing (D-1139)");
        }
    }

    private static void RefuseRepeatedId(ref ContentReader reader, GameMap map)
    {
        IReadOnlyList<Npc> npcs = map.Npcs;
        for (int index = 0; index < npcs.Count; index += 1)
        {
            for (int other = index + 1; other < npcs.Count; other += 1)
            {
                if (string.CompareOrdinal(npcs[index].Id.Value, npcs[other].Id.Value) == 0)
                {
                    throw reader.Refuse(
                        $"two NPCs of this map take the id '{npcs[index].Id.Value}', and an id is permanent (D-166)");
                }
            }
        }
    }

    /// <summary>
    /// Refuses a start tile that another NPC or the start body of an enemy already holds, because
    /// an NPC is solid (D-1139, T-2). The time of day of the map picks the station of each enemy,
    /// so this check reads that time alone (D-743). The check of the things covers the spawn point.
    /// </summary>
    private static void RefuseCrowdedStart(ref ContentReader reader, GameMap map)
    {
        IReadOnlyList<Npc> npcs = map.Npcs;
        for (int index = 0; index < npcs.Count; index += 1)
        {
            Npc npc = npcs[index];
            for (int earlier = 0; earlier < index; earlier += 1)
            {
                if (npcs[earlier].Start == npc.Start)
                {
                    throw reader.Refuse(
                        $"the NPC '{npc.Id.Value}' starts at {npc.Start}, and the NPC '{npcs[earlier].Id.Value}' starts there too (D-1139)");
                }
            }

            foreach (Patrol patrol in map.Patrols)
            {
                PatrolStation? station = patrol.StationOf(map.Time);
                if (station is not null && new EnemyBody(station.Start, patrol.Size).Holds(npc.Start))
                {
                    throw reader.Refuse(
                        $"the NPC '{npc.Id.Value}' starts at {npc.Start}, and the enemy '{patrol.Id.Value}' starts on that tile (D-1139)");
                }
            }
        }
    }
}
