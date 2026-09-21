using System;
using System.Collections.Generic;
using TheThingBelow.Core.Content;

namespace TheThingBelow.Core.Maps;

/// <summary>
/// One map of the game, as its rule file holds it: the terrain rows, every thing that a rule
/// reads, every enemy, and the time of day (D-528, D-738).
/// </summary>
/// <remarks>
/// One rule file holds each map, so one file holds each place for the author, for the review,
/// and for the content hash (D-495, D-528). The edge file of PR-53, the light setup of PR-56,
/// and the art of D-519 sit beside it and stay out of the content hash (D-501).
/// <para>
/// The author writes each map by hand, and no seed changes it (D-39, D-47).
/// </para>
/// </remarks>
public sealed class GameMap
{
    /// <summary>The kind of the id of a map (D-646).</summary>
    public const string IdKind = "map";

    /// <summary>The folder that holds every map file, under `content/` (D-495, D-528).</summary>
    public const string Folder = "rules/maps/";

    /// <summary>
    /// The largest count of tiles on one side of a map. A `TileMapLayer` of Godot holds
    /// coordinates from -32768 to 32767, and this limit stays far below that. It also bounds
    /// the walked-tile record that every snapshot carries (D-567).
    /// </summary>
    public const int MaxSide = 256;

    private readonly TileKind[] tiles;
    private readonly MapThing[] things;
    private readonly Patrol[] patrols;

    private GameMap(
        string file,
        ContentId id,
        ContentId label,
        TimeOfDay time,
        int width,
        int height,
        TileKind[] tiles,
        MapThing[] things,
        Patrol[] patrols,
        TilePoint spawn)
    {
        this.File = file;
        this.Id = id;
        this.Label = label;
        this.Time = time;
        this.Width = width;
        this.Height = height;
        this.tiles = tiles;
        this.things = things;
        this.patrols = patrols;
        this.Spawn = spawn;
    }

    /// <summary>The path of the file, under `content/`, for every error (T-2).</summary>
    public string File { get; }

    /// <summary>The permanent content id of the map (D-166, D-646).</summary>
    public ContentId Id { get; }

    /// <summary>The string id of the name that the player reads for this map (G-7).</summary>
    public ContentId Label { get; }

    /// <summary>The time of day that the file gives, which a story flag can change (D-442).</summary>
    public TimeOfDay Time { get; }

    /// <summary>The count of tiles from the west edge to the east edge.</summary>
    public int Width { get; }

    /// <summary>The count of tiles from the north edge to the south edge.</summary>
    public int Height { get; }

    /// <summary>The tile where the party starts on this map, from the one spawn point (D-528).</summary>
    public TilePoint Spawn { get; }

    /// <summary>Every thing of the map, in the order of the file (G-4).</summary>
    public IReadOnlyList<MapThing> Things => this.things;

    /// <summary>Every enemy that this map places, in the order of the file (D-738, G-4).</summary>
    public IReadOnlyList<Patrol> Patrols => this.patrols;

    /// <summary>Tells whether one tile lies inside the map.</summary>
    /// <param name="at">The tile.</param>
    /// <returns>True when the map holds that tile.</returns>
    public bool Holds(TilePoint at) =>
        at.X >= 0 && at.X < this.Width && at.Y >= 0 && at.Y < this.Height;

    /// <summary>Gives the kind of the ground of one tile.</summary>
    /// <param name="at">The tile, which lies inside the map.</param>
    /// <returns>The kind of that tile.</returns>
    /// <exception cref="ArgumentOutOfRangeException">The tile lies outside the map (T-2).</exception>
    public TileKind TileAt(TilePoint at)
    {
        if (!this.Holds(at))
        {
            throw new ArgumentOutOfRangeException(
                nameof(at),
                at,
                $"The map '{this.Id.Value}' is {this.Width} by {this.Height} tiles, and this tile lies outside it (T-2).");
        }

        return this.tiles[(at.Y * this.Width) + at.X];
    }

    /// <summary>Gives every thing on one tile, in the order of the file (D-528).</summary>
    /// <param name="at">The tile.</param>
    /// <returns>The things there, which can hold none.</returns>
    /// <remarks>
    /// One tile holds one thing, and a lock shares its tile with the door that it locks
    /// (D-386). Thus this method gives at most two things.
    /// </remarks>
    public IReadOnlyList<MapThing> ThingsAt(TilePoint at)
    {
        var found = new List<MapThing>();
        foreach (MapThing thing in this.things)
        {
            if (thing.At == at)
            {
                found.Add(thing);
            }
        }

        return found;
    }

    /// <summary>Tells whether a path of this repository is a map file (D-528).</summary>
    /// <param name="path">The path under `content/`, with `/` separators.</param>
    /// <returns>True when the path lies in the map folder and is a JSON file.</returns>
    /// <exception cref="ArgumentNullException">The path is null (T-2).</exception>
    public static bool IsMapFile(string path)
    {
        ArgumentNullException.ThrowIfNull(path);

        return path.StartsWith(Folder, StringComparison.Ordinal) &&
            path.EndsWith(".json", StringComparison.Ordinal);
    }

    /// <summary>Reads one map file from its bytes, and checks every rule of a map (T-2).</summary>
    /// <param name="bytes">The bytes of the file, as UTF-8.</param>
    /// <param name="file">The path of the file, under `content/`, for each error (T-2).</param>
    /// <returns>The map.</returns>
    /// <exception cref="ContentException">The file breaks a rule of the reader (G-6, T-2).</exception>
    public static GameMap Read(ReadOnlySpan<byte> bytes, string file)
    {
        var reader = new ContentReader(bytes, file);
        GameMap map = Read(ref reader);
        reader.ReadFileEnd();
        return map;
    }

    private static GameMap Read(ref ContentReader reader)
    {
        string? comment = null;
        ContentId? id = null;
        ContentId? label = null;
        string? time = null;
        List<string>? terrain = null;
        List<ThingLine>? things = null;
        List<Patrol>? patrols = null;

        int depth = reader.ReadObjectStart();
        while (reader.ReadNextField(depth, out string field))
        {
            switch (field)
            {
                case "comment":
                    comment = reader.ReadString();
                    break;
                case "id":
                    id = reader.ReadContentId(IdKind);
                    break;
                case "label":
                    // A string id names where the player reads the text, so it takes the
                    // kind of that place and never the kind of this record (G-7, D-646).
                    label = reader.ReadContentId();
                    break;
                case "time":
                    time = reader.ReadString();
                    break;
                case "terrain":
                    terrain = ReadRows(ref reader);
                    break;
                case "things":
                    things = ReadThings(ref reader);
                    break;
                case "enemies":
                    patrols = Patrol.ReadAll(ref reader);
                    break;
                default:
                    throw reader.UnknownField(field);
            }
        }

        _ = reader.Require(comment, depth, "comment");
        return Build(
            ref reader,
            reader.Require(id, depth, "id"),
            reader.Require(label, depth, "label"),
            reader.Require(time, depth, "time"),
            reader.Require(terrain, depth, "terrain"),
            reader.Require(things, depth, "things"),
            reader.Require(patrols, depth, "enemies"));
    }

    private static List<string> ReadRows(ref ContentReader reader)
    {
        var rows = new List<string>();
        int depth = reader.ReadArrayStart();
        while (reader.ReadNextElement(depth, rows.Count))
        {
            rows.Add(reader.ReadString());
        }

        return rows;
    }

    private static List<ThingLine> ReadThings(ref ContentReader reader)
    {
        var things = new List<ThingLine>();
        int depth = reader.ReadArrayStart();
        while (reader.ReadNextElement(depth, things.Count))
        {
            things.Add(ReadThing(ref reader));
        }

        return things;
    }

    private static ThingLine ReadThing(ref ContentReader reader)
    {
        ContentId? id = null;
        string? kind = null;
        int? x = null;
        int? y = null;
        bool? pickable = null;

        int depth = reader.ReadObjectStart();
        while (reader.ReadNextField(depth, out string field))
        {
            switch (field)
            {
                case "id":
                    id = reader.ReadContentId();
                    break;
                case "kind":
                    kind = reader.ReadString();
                    break;
                case "x":
                    x = reader.ReadInt();
                    break;
                case "y":
                    y = reader.ReadInt();
                    break;
                case "pickable":
                    pickable = reader.ReadBoolean();
                    break;
                default:
                    throw reader.UnknownField(field);
            }
        }

        return new ThingLine(
            reader.Require(id, depth, "id"),
            reader.Require(kind, depth, "kind"),
            reader.RequireInt(x, depth, "x"),
            reader.RequireInt(y, depth, "y"),
            pickable);
    }

    private static GameMap Build(
        ref ContentReader reader,
        ContentId id,
        ContentId label,
        string time,
        List<string> rows,
        List<ThingLine> lines,
        List<Patrol> patrols)
    {
        if (!TimesOfDay.TryOf(time, out TimeOfDay parsed))
        {
            throw reader.Refuse(
                $"the time of day is '{time}', and a map takes one of {TimesOfDay.EveryName} (D-442)");
        }

        TileKind[] tiles = ReadTerrain(ref reader, rows, out int width, out int height);
        MapThing[] things = BuildThings(ref reader, lines, tiles, width, height);
        TilePoint spawn = OneSpawn(ref reader, things);
        var map = new GameMap(reader.File, id, label, parsed, width, height, tiles, things, [.. patrols], spawn);

        // The map is complete here, so each check of a patrol reads the terrain and the
        // spawn point through the map itself and never through a second copy of them (T-1).
        PatrolLayout.Check(ref reader, map);
        return map;
    }

    private static TileKind[] ReadTerrain(ref ContentReader reader, List<string> rows, out int width, out int height)
    {
        height = rows.Count;
        if (height == 0)
        {
            throw reader.Refuse("the terrain holds no row, and a map holds at least one (D-528)");
        }

        width = rows[0].Length;
        if (width == 0)
        {
            throw reader.Refuse("the first terrain row holds no character, and a map holds at least one column (D-528)");
        }

        if (width > MaxSide || height > MaxSide)
        {
            throw reader.Refuse(
                $"the terrain is {width} by {height} tiles, and a map holds at most {MaxSide} on each side (D-528)");
        }

        var tiles = new TileKind[width * height];
        for (int row = 0; row < height; row += 1)
        {
            string line = rows[row];
            if (line.Length != width)
            {
                throw reader.Refuse(
                    $"the terrain row {row} holds {line.Length} characters, and row 0 holds {width}. Every row of a map holds the same count (D-528)");
            }

            for (int column = 0; column < width; column += 1)
            {
                if (!TileKinds.TryOf(line[column], out TileKind kind))
                {
                    throw reader.Refuse(
                        $"the terrain holds the character '{line[column]}' at the tile ({column}, {row}), and a terrain row takes one of '{TileKinds.EveryCharacter}' (D-528)");
                }

                tiles[(row * width) + column] = kind;
            }
        }

        return tiles;
    }

    private static MapThing[] BuildThings(
        ref ContentReader reader,
        List<ThingLine> lines,
        TileKind[] tiles,
        int width,
        int height)
    {
        var things = new MapThing[lines.Count];
        for (int index = 0; index < lines.Count; index += 1)
        {
            ThingLine line = lines[index];
            if (!MapThingKinds.TryOf(line.Kind, out MapThingKind kind))
            {
                throw reader.Refuse(
                    $"the thing '{line.Id.Value}' takes the kind '{line.Kind}', and a map takes one of {MapThingKinds.EveryName} (D-528)");
            }

            // The kind of the id names the kind of the thing, so no reader needs a second
            // field and no id of another record reaches a map (D-646, T-2).
            if (string.CompareOrdinal(line.Id.Kind, MapThingKinds.NameOf(kind)) != 0)
            {
                throw reader.Refuse(
                    $"the thing '{line.Id.Value}' takes the kind '{MapThingKinds.NameOf(kind)}', and the kind of its id is '{line.Id.Kind}'. The two agree (D-646)");
            }

            var at = new TilePoint(line.X, line.Y);
            if (at.X < 0 || at.X >= width || at.Y < 0 || at.Y >= height)
            {
                throw reader.Refuse(
                    $"the thing '{line.Id.Value}' sits at {at}, and the map is {width} by {height} tiles (D-528)");
            }

            TileKind under = tiles[(at.Y * width) + at.X];
            TileKind wanted = MapThingKinds.TileOf(kind);
            if (under != wanted)
            {
                throw reader.Refuse(
                    $"the thing '{line.Id.Value}' sits at {at} on a {TileKinds.NameOf(under)} tile, and a {MapThingKinds.NameOf(kind)} sits on a {TileKinds.NameOf(wanted)} tile (D-528)");
            }

            if (line.Pickable.HasValue && kind != MapThingKind.Lock)
            {
                throw reader.Refuse(
                    $"the thing '{line.Id.Value}' is a {MapThingKinds.NameOf(kind)} and it holds the field 'pickable', which a lock alone holds (D-386)");
            }

            if (!line.Pickable.HasValue && kind == MapThingKind.Lock)
            {
                throw reader.Refuse(
                    $"the lock '{line.Id.Value}' holds no field 'pickable', and the layout marks every lock (D-386)");
            }

            things[index] = new MapThing(line.Id, kind, at, line.Pickable ?? false);
        }

        RefuseRepeatedId(ref reader, things);
        RefuseCrowdedTile(ref reader, things);
        return things;
    }

    private static void RefuseRepeatedId(ref ContentReader reader, MapThing[] things)
    {
        for (int index = 0; index < things.Length; index += 1)
        {
            for (int other = index + 1; other < things.Length; other += 1)
            {
                if (string.CompareOrdinal(things[index].Id.Value, things[other].Id.Value) == 0)
                {
                    throw reader.Refuse(
                        $"two things of this map take the id '{things[index].Id.Value}', and an id is permanent (D-166)");
                }
            }
        }
    }

    /// <summary>
    /// Refuses two things on one tile. A lock is the one exception, because a lock sits on
    /// the door that it holds shut (D-386). Thus one tile holds one thing, or it holds one
    /// door and one lock.
    /// </summary>
    private static void RefuseCrowdedTile(ref ContentReader reader, MapThing[] things)
    {
        for (int index = 0; index < things.Length; index += 1)
        {
            MapThing thing = things[index];
            int doors = 0;
            int locks = 0;
            int count = 0;
            foreach (MapThing other in things)
            {
                if (other.At != thing.At)
                {
                    continue;
                }

                count += 1;
                if (other.Kind == MapThingKind.Door)
                {
                    doors += 1;
                }
                else if (other.Kind == MapThingKind.Lock)
                {
                    locks += 1;
                }
            }

            bool lone = count == 1 && locks == 0;
            bool lockedDoor = count == 2 && doors == 1 && locks == 1;
            if (lone || lockedDoor)
            {
                continue;
            }

            if (locks == 1 && count == 1)
            {
                throw reader.Refuse(
                    $"the lock '{thing.Id.Value}' at {thing.At} sits on no door, and a lock holds one door shut (D-386)");
            }

            throw reader.Refuse(
                $"the tile {thing.At} holds {count} things, and one tile holds one thing or one door with one lock (D-386, D-528)");
        }
    }

    private static TilePoint OneSpawn(ref ContentReader reader, MapThing[] things)
    {
        int count = 0;
        TilePoint spawn = default;
        foreach (MapThing thing in things)
        {
            if (thing.Kind == MapThingKind.SpawnPoint)
            {
                count += 1;
                spawn = thing.At;
            }
        }

        if (count != 1)
        {
            throw reader.Refuse(
                $"the map holds {count} spawn points, and the party starts on exactly one (D-528)");
        }

        return spawn;
    }

    private sealed record ThingLine(ContentId Id, string Kind, int X, int Y, bool? Pickable);
}
