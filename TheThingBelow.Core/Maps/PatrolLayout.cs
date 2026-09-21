using System;
using System.Collections.Generic;
using TheThingBelow.Core.Content;

namespace TheThingBelow.Core.Maps;

/// <summary>
/// The rules that bind the enemies of one map to its ground: the sight range floor, each
/// route, each area, and each start tile (D-720, D-739, D-741).
/// </summary>
/// <remarks>
/// The load of a map runs every check here, so no content author can place an enemy in a
/// wall, past an edge, on the party, or on another enemy (T-2). The check of an area also
/// proves that the body fits everywhere in it, which D-209 asks for.
/// <para>
/// Each check reads the complete map, so the message of a fault names the map, the enemy,
/// and the tile (T-2, G-18).
/// </para>
/// </remarks>
public static class PatrolLayout
{
    /// <summary>Checks every enemy of one map, in the order of the file (G-4).</summary>
    /// <param name="reader">The reader of the map file, which names the file of a fault.</param>
    /// <param name="map">The map, with its terrain, its things, and its spawn point.</param>
    /// <exception cref="ContentException">An enemy breaks a rule of a map (G-6, T-2).</exception>
    public static void Check(ref ContentReader reader, GameMap map)
    {
        ArgumentNullException.ThrowIfNull(map);

        int party = MapRules.PartySightRange(map.Time);
        foreach (Patrol patrol in map.Patrols)
        {
            // The player never loses to a thing that it could not see, so no map gives a
            // patrol a longer sight range than the party has on that map (D-720).
            if (patrol.SightRange > party)
            {
                throw reader.Refuse(
                    $"the enemy '{patrol.Id.Value}' sees {patrol.SightRange} tiles, and the party sees {party} on a map set to {TimesOfDay.NameOf(map.Time)} (D-720)");
            }

            foreach (PatrolStation station in patrol.Stations)
            {
                if (station.Area is TileArea area)
                {
                    CheckArea(ref reader, map, patrol, area);
                }
                else
                {
                    CheckRoute(ref reader, map, patrol, station);
                }
            }
        }

        RefuseRepeatedId(ref reader, map);
        RefuseCrowdedStart(ref reader, map);
    }

    /// <summary>
    /// Checks one route: each leg is straight and on one axis, and the body of the enemy
    /// fits on every tile of every leg (D-716, D-739). Thus a patrol never leaves its route
    /// onto a wall or past an edge.
    /// </summary>
    private static void CheckRoute(ref ContentReader reader, GameMap map, Patrol patrol, PatrolStation station)
    {
        IReadOnlyList<TilePoint> route = station.Tiles;
        for (int index = 0; index < route.Count; index += 1)
        {
            EnemyBody body = new(route[index], patrol.Size);
            if (!MapRules.CanPlace(map, body))
            {
                throw reader.Refuse(
                    $"the enemy '{patrol.Id.Value}' holds the route tile {route[index]}, and the body {body} takes no step there (D-739)");
            }
        }

        for (int index = 0; index + 1 < route.Count; index += 1)
        {
            TilePoint from = route[index];
            TilePoint to = route[index + 1];
            if (!PatrolStation.TryLeg(from, to, out StepDirection direction, out int length))
            {
                throw reader.Refuse(
                    $"the enemy '{patrol.Id.Value}' walks from {from} to {to}, and each leg of a route is straight and on one axis (D-716, D-739)");
            }

            TilePoint step = from;
            for (int count = 0; count < length; count += 1)
            {
                step = step.Step(direction);
                EnemyBody body = new(step, patrol.Size);
                if (!MapRules.CanPlace(map, body))
                {
                    throw reader.Refuse(
                        $"the enemy '{patrol.Id.Value}' walks from {from} to {to}, and the body {body} takes no step at {step} (D-739)");
                }
            }
        }
    }

    /// <summary>
    /// Checks one area: it lies inside the map, it holds the whole body, and the body fits at
    /// every anchor tile of it (D-209, D-741).
    /// </summary>
    private static void CheckArea(ref ContentReader reader, GameMap map, Patrol patrol, TileArea area)
    {
        // Each side reads against the room that the map leaves, and never as a sum. A sum of
        // a large coordinate and a side wraps below zero, and the area then passes (T-2).
        // The reader already proved each value at zero or more, so no difference wraps.
        if (area.Width > map.Width - area.X || area.Height > map.Height - area.Y)
        {
            throw reader.Refuse(
                $"the enemy '{patrol.Id.Value}' holds the area {area}, and the map is {map.Width} by {map.Height} tiles (D-741)");
        }

        int side = EnemySizes.SideOf(patrol.Size);
        if (area.Width < side || area.Height < side)
        {
            throw reader.Refuse(
                $"the enemy '{patrol.Id.Value}' holds the area {area}, and its body is {side} by {side} tiles. An area holds the whole body (D-209)");
        }

        // An area of exactly the body size holds one anchor tile, so the enemy of it would
        // stand perfectly still and draw from the exploration stream on every tick. D-209
        // refuses a large enemy that stands perfectly still (D-741, T-7).
        if (area.Width == side && area.Height == side)
        {
            throw reader.Refuse(
                $"the enemy '{patrol.Id.Value}' holds the area {area}, and its body of {side} by {side} tiles fills it. An area leaves room to move (D-209)");
        }

        for (int row = area.Y; row + side <= area.Y + area.Height; row += 1)
        {
            for (int column = area.X; column + side <= area.X + area.Width; column += 1)
            {
                EnemyBody body = new(new TilePoint(column, row), patrol.Size);
                if (!MapRules.CanPlace(map, body))
                {
                    throw reader.Refuse(
                        $"the enemy '{patrol.Id.Value}' holds the area {area}, and the body {body} takes no step there. A body fits everywhere in its area (D-209)");
                }
            }
        }
    }

    private static void RefuseRepeatedId(ref ContentReader reader, GameMap map)
    {
        IReadOnlyList<Patrol> patrols = map.Patrols;
        for (int index = 0; index < patrols.Count; index += 1)
        {
            for (int other = index + 1; other < patrols.Count; other += 1)
            {
                if (string.CompareOrdinal(patrols[index].Id.Value, patrols[other].Id.Value) == 0)
                {
                    throw reader.Refuse(
                        $"two enemies of this map take the id '{patrols[index].Id.Value}', and an id is permanent (D-166)");
                }
            }
        }
    }

    /// <summary>
    /// Refuses a start tile that the party or another enemy already holds (T-2). The time of
    /// day of the map picks the station of each enemy, so this check reads that time alone
    /// (D-743).
    /// </summary>
    private static void RefuseCrowdedStart(ref ContentReader reader, GameMap map)
    {
        List<EnemyBody> bodies = [];
        List<ContentId> owners = [];
        foreach (Patrol patrol in map.Patrols)
        {
            PatrolStation? station = patrol.StationOf(map.Time);
            if (station is null)
            {
                continue;
            }

            EnemyBody body = new(station.Start, patrol.Size);
            if (body.Holds(map.Spawn))
            {
                throw reader.Refuse(
                    $"the enemy '{patrol.Id.Value}' starts on the body {body}, which holds the spawn point {map.Spawn} (D-528)");
            }

            for (int index = 0; index < bodies.Count; index += 1)
            {
                if (MapRules.BodiesOverlap(bodies[index], body))
                {
                    throw reader.Refuse(
                        $"the enemy '{patrol.Id.Value}' starts on the body {body}, and the enemy '{owners[index].Value}' holds the body {bodies[index]} there (D-206)");
                }
            }

            bodies.Add(body);
            owners.Add(patrol.Id);
        }
    }
}
