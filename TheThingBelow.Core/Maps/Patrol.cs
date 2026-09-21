using System;
using System.Collections.Generic;
using TheThingBelow.Core.Content;

namespace TheThingBelow.Core.Maps;

/// <summary>
/// One enemy that a map places, with its stations, its size, and its group (D-738, D-752).
/// </summary>
/// <remarks>
/// The map file holds each patrol in its `enemies` array, beside the things of the map, so
/// one rule file still holds each place for the author, the review, and the content hash
/// (D-495, D-528, D-738).
/// <para>
/// One record covers a fixed enemy and a walking one. A route of one tile stands still, and
/// the facing of the record gives its sight (D-740). Thus one walk rule and one sight rule
/// serve both cases (T-1).
/// </para>
/// <para>
/// The record names its group, and the load checks the form of that id alone. The test that
/// proves each named group exists lands with the group file in PR-11 (D-535, D-753, G-16).
/// </para>
/// </remarks>
public sealed class Patrol
{
    /// <summary>The kind of the id of a patrol (D-646, D-752).</summary>
    public const string IdKind = "patrol";

    /// <summary>The kind of the id of an enemy group, which PR-11 writes (D-535, D-753).</summary>
    public const string GroupKind = "group";

    private readonly PatrolStation[] stations;

    private Patrol(
        ContentId id,
        ContentId group,
        EnemySize size,
        StepDirection facing,
        int stepTicks,
        int sightRange,
        PatrolStation[] stations)
    {
        this.Id = id;
        this.Group = group;
        this.Size = size;
        this.Facing = facing;
        this.StepTicks = stepTicks;
        this.SightRange = sightRange;
        this.stations = stations;
    }

    /// <summary>The permanent content id of this patrol (D-166, D-752).</summary>
    public ContentId Id { get; }

    /// <summary>The id of the enemy group that a fight with this patrol uses (D-535, D-753).</summary>
    public ContentId Group { get; }

    /// <summary>The count of tiles of the body of this enemy (D-206, D-754).</summary>
    public EnemySize Size { get; }

    /// <summary>The direction that this patrol faces at the start of a run (D-208, D-740).</summary>
    public StepDirection Facing { get; }

    /// <summary>The count of world ticks that one step of this enemy takes (D-742).</summary>
    public int StepTicks { get; }

    /// <summary>The sight range of this enemy, in tiles (D-718, D-720).</summary>
    public int SightRange { get; }

    /// <summary>Every station of this patrol, in the order of the file (G-4).</summary>
    public IReadOnlyList<PatrolStation> Stations => this.stations;

    /// <summary>
    /// Gives the station that the time of day of a map picks, or no value when this patrol
    /// is not on the map at that time (D-743).
    /// </summary>
    /// <param name="time">The time of day of the map (D-442).</param>
    /// <returns>The station, or null.</returns>
    public PatrolStation? StationOf(TimeOfDay time)
    {
        foreach (PatrolStation station in this.stations)
        {
            if (station.Picks(time))
            {
                return station;
            }
        }

        return null;
    }

    /// <summary>Reads the `enemies` array of a map file (D-738).</summary>
    /// <param name="reader">The reader of the map file, at the start of the array.</param>
    /// <returns>Each patrol, in the order of the file.</returns>
    /// <exception cref="ContentException">An entry breaks a rule of the reader (G-6, T-2).</exception>
    public static List<Patrol> ReadAll(ref ContentReader reader)
    {
        List<Patrol> patrols = [];
        int depth = reader.ReadArrayStart();
        while (reader.ReadNextElement(depth, patrols.Count))
        {
            patrols.Add(Read(ref reader));
        }

        return patrols;
    }

    private static Patrol Read(ref ContentReader reader)
    {
        ContentId? id = null;
        ContentId? group = null;
        string? size = null;
        string? facing = null;
        int? stepTicks = null;
        int? sightRange = null;
        List<PatrolStation>? routes = null;
        List<PatrolStation>? areas = null;

        int depth = reader.ReadObjectStart();
        while (reader.ReadNextField(depth, out string field))
        {
            switch (field)
            {
                case "id":
                    id = reader.ReadContentId(IdKind);
                    break;
                case "group":
                    group = reader.ReadContentId(GroupKind);
                    break;
                case "size":
                    size = reader.ReadString();
                    break;
                case "facing":
                    facing = reader.ReadString();
                    break;
                case "step_ticks":
                    stepTicks = reader.ReadInt();
                    break;
                case "sight_range":
                    sightRange = reader.ReadInt();
                    break;
                case "routes":
                    routes = ReadRoutes(ref reader);
                    break;
                case "areas":
                    areas = ReadAreas(ref reader);
                    break;
                default:
                    throw reader.UnknownField(field);
            }
        }

        return Build(
            ref reader,
            reader.Require(id, depth, "id"),
            reader.Require(group, depth, "group"),
            reader.Require(size, depth, "size"),
            reader.Require(facing, depth, "facing"),
            reader.RequireInt(stepTicks, depth, "step_ticks"),
            reader.RequireInt(sightRange, depth, "sight_range"),
            routes,
            areas);
    }

    private static Patrol Build(
        ref ContentReader reader,
        ContentId id,
        ContentId group,
        string size,
        string facing,
        int stepTicks,
        int sightRange,
        List<PatrolStation>? routes,
        List<PatrolStation>? areas)
    {
        if (!EnemySizes.TryOf(size, out EnemySize parsedSize))
        {
            throw reader.Refuse(
                $"the enemy '{id.Value}' takes the size '{size}', and an enemy takes one of {EnemySizes.EveryName} (D-206)");
        }

        if (!TryDirection(facing, out StepDirection parsedFacing))
        {
            throw reader.Refuse(
                $"the enemy '{id.Value}' faces '{facing}', and a facing is north, south, east, or west (D-716)");
        }

        // A patrol that steps faster than the party can never be walked away from, and no
        // check would then hold the fairness line that D-720 drew for the sight (D-742).
        if (stepTicks < MapRules.TicksPerStep)
        {
            throw reader.Refuse(
                $"the enemy '{id.Value}' steps in {stepTicks} ticks, and the party steps in {MapRules.TicksPerStep}. No enemy outwalks the party (D-742)");
        }

        if (sightRange < 0)
        {
            throw reader.Refuse(
                $"the enemy '{id.Value}' takes the sight range {sightRange}, which is below zero (D-718)");
        }

        PatrolStation[] stations = OneForm(ref reader, id, parsedSize, routes, areas);
        RefuseRepeatedTime(ref reader, id, stations);
        return new Patrol(id, group, parsedSize, parsedFacing, stepTicks, sightRange, stations);
    }

    /// <summary>
    /// Takes the one station form of the record. A record holds routes or areas, and never
    /// both (D-739, D-741). A large enemy takes an area alone, because D-209 gives it
    /// presence in its own area and refused the wide-route form.
    /// </summary>
    private static PatrolStation[] OneForm(
        ref ContentReader reader,
        ContentId id,
        EnemySize size,
        List<PatrolStation>? routes,
        List<PatrolStation>? areas)
    {
        if (routes is not null && areas is not null)
        {
            throw reader.Refuse(
                $"the enemy '{id.Value}' holds routes and areas, and a record holds one form (D-739, D-741)");
        }

        if (routes is null && areas is null)
        {
            throw reader.Refuse(
                $"the enemy '{id.Value}' holds no route and no area, and a record holds one form (D-739, D-741)");
        }

        if (routes is not null && size != EnemySize.Common)
        {
            throw reader.Refuse(
                $"the enemy '{id.Value}' is an {EnemySizes.NameOf(size)} with a route, and a large enemy keeps its place inside an area (D-209, D-741)");
        }

        List<PatrolStation> stations = routes ?? areas!;
        if (stations.Count == 0)
        {
            throw reader.Refuse(
                $"the enemy '{id.Value}' holds an empty list of stations, and a record holds at least one (D-743)");
        }

        return [.. stations];
    }

    /// <summary>
    /// Refuses a record that names one time of day two times (D-743). The time picks one
    /// station, so two stations of one time would leave the pick to the order of the file.
    /// </summary>
    private static void RefuseRepeatedTime(ref ContentReader reader, ContentId id, PatrolStation[] stations)
    {
        foreach (TimeOfDay time in TimesOfDay.All)
        {
            int count = 0;
            foreach (PatrolStation station in stations)
            {
                if (station.Picks(time))
                {
                    count += 1;
                }
            }

            if (count > 1)
            {
                throw reader.Refuse(
                    $"the enemy '{id.Value}' names the time '{TimesOfDay.NameOf(time)}' on {count} stations, and one time picks one station (D-743)");
            }
        }
    }

    private static List<PatrolStation> ReadRoutes(ref ContentReader reader)
    {
        List<PatrolStation> stations = [];
        int depth = reader.ReadArrayStart();
        while (reader.ReadNextElement(depth, stations.Count))
        {
            stations.Add(ReadRoute(ref reader));
        }

        return stations;
    }

    private static PatrolStation ReadRoute(ref ContentReader reader)
    {
        List<TimeOfDay>? times = null;
        List<TilePoint>? tiles = null;

        int depth = reader.ReadObjectStart();
        while (reader.ReadNextField(depth, out string field))
        {
            switch (field)
            {
                case "times":
                    times = ReadTimes(ref reader);
                    break;
                case "tiles":
                    tiles = ReadTiles(ref reader);
                    break;
                default:
                    throw reader.UnknownField(field);
            }
        }

        List<TimeOfDay> named = reader.Require(times, depth, "times");
        List<TilePoint> route = reader.Require(tiles, depth, "tiles");
        if (named.Count == 0)
        {
            throw reader.Refuse("a route names no time of day, and a station names at least one (D-743)");
        }

        if (route.Count == 0)
        {
            throw reader.Refuse("a route holds no tile, and a route holds at least one (D-739)");
        }

        return new PatrolStation(named, route, null);
    }

    private static List<PatrolStation> ReadAreas(ref ContentReader reader)
    {
        List<PatrolStation> stations = [];
        int depth = reader.ReadArrayStart();
        while (reader.ReadNextElement(depth, stations.Count))
        {
            stations.Add(ReadArea(ref reader));
        }

        return stations;
    }

    private static PatrolStation ReadArea(ref ContentReader reader)
    {
        List<TimeOfDay>? times = null;
        int? x = null;
        int? y = null;
        int? width = null;
        int? height = null;

        int depth = reader.ReadObjectStart();
        while (reader.ReadNextField(depth, out string field))
        {
            switch (field)
            {
                case "times":
                    times = ReadTimes(ref reader);
                    break;
                case "x":
                    x = reader.ReadInt();
                    break;
                case "y":
                    y = reader.ReadInt();
                    break;
                case "width":
                    width = reader.ReadInt();
                    break;
                case "height":
                    height = reader.ReadInt();
                    break;
                default:
                    throw reader.UnknownField(field);
            }
        }

        List<TimeOfDay> named = reader.Require(times, depth, "times");
        if (named.Count == 0)
        {
            throw reader.Refuse("an area names no time of day, and a station names at least one (D-743)");
        }

        TileArea area = new(
            reader.RequireInt(x, depth, "x"),
            reader.RequireInt(y, depth, "y"),
            reader.RequireInt(width, depth, "width"),
            reader.RequireInt(height, depth, "height"));
        if (!area.IsRectangle())
        {
            throw reader.Refuse(
                $"the area {area} holds no tile, and an area starts inside the map and holds at least one tile (D-741)");
        }

        return new PatrolStation(named, [], area);
    }

    private static List<TimeOfDay> ReadTimes(ref ContentReader reader)
    {
        List<TimeOfDay> times = [];
        int depth = reader.ReadArrayStart();
        while (reader.ReadNextElement(depth, times.Count))
        {
            string name = reader.ReadString();
            if (!TimesOfDay.TryOf(name, out TimeOfDay time))
            {
                throw reader.Refuse(
                    $"a station names the time '{name}', and a time is one of {TimesOfDay.EveryName} (D-442)");
            }

            times.Add(time);
        }

        return times;
    }

    private static List<TilePoint> ReadTiles(ref ContentReader reader)
    {
        List<TilePoint> tiles = [];
        int depth = reader.ReadArrayStart();
        while (reader.ReadNextElement(depth, tiles.Count))
        {
            tiles.Add(ReadTile(ref reader));
        }

        return tiles;
    }

    private static TilePoint ReadTile(ref ContentReader reader)
    {
        int? x = null;
        int? y = null;

        int depth = reader.ReadObjectStart();
        while (reader.ReadNextField(depth, out string field))
        {
            switch (field)
            {
                case "x":
                    x = reader.ReadInt();
                    break;
                case "y":
                    y = reader.ReadInt();
                    break;
                default:
                    throw reader.UnknownField(field);
            }
        }

        return new TilePoint(reader.RequireInt(x, depth, "x"), reader.RequireInt(y, depth, "y"));
    }

    private static bool TryDirection(string name, out StepDirection direction)
    {
        foreach (StepDirection candidate in StepDirections.All)
        {
            if (string.CompareOrdinal(StepDirections.NameOf(candidate), name) == 0)
            {
                direction = candidate;
                return true;
            }
        }

        direction = StepDirection.North;
        return false;
    }
}
