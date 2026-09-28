using System;
using System.Collections.Generic;
using TheThingBelow.Core.Content;

namespace TheThingBelow.Core.Maps;

/// <summary>The land of one basin of the overworld, which picks its cover and its zones (D-1283).</summary>
public enum OverworldLand
{
    /// <summary>The low land of frosted grass and forest, around the village.</summary>
    Low,

    /// <summary>The high valley of snow and forest, around the town.</summary>
    Valley,

    /// <summary>The high pass of snow, below the ice crossing.</summary>
    Pass,
}

/// <summary>The role of one thing that the generator places (D-1295).</summary>
public enum OverworldRole
{
    /// <summary>The spawn point, on the road of the low land.</summary>
    Spawn,

    /// <summary>The mark of the village, beside the road of the low land.</summary>
    Village,

    /// <summary>The mark of the town, on the road of the valley.</summary>
    Town,

    /// <summary>The mark of the border fort, on the road of the pass.</summary>
    Fort,

    /// <summary>The mark of the ice crossing, at the end of the road in the north wall.</summary>
    IceCrossing,

    /// <summary>The mark of the deep mine, in a notch of the west wall of the valley.</summary>
    Mine,

    /// <summary>The gate of the mine mouth, in the mouth of its notch.</summary>
    MineGate,

    /// <summary>The mark of the gallery, in a notch of the east wall of the valley.</summary>
    Gallery,

    /// <summary>The gate of the sealed door, in the mouth of the notch of the gallery.</summary>
    SealedDoor,

    /// <summary>The mark of the refuge, in the refuge basin past the gorge.</summary>
    Refuge,

    /// <summary>The gate of the road to the town, the first gate of the low pass from the south.</summary>
    TownRoad,

    /// <summary>The gate of the way down to the village, the first gate of the low pass from the north.</summary>
    VillageRoad,

    /// <summary>The gate of the road up, in the high pass.</summary>
    RoadUp,

    /// <summary>The mark of the broken waystone, off the road at an edge of the land (D-1299).</summary>
    BrokenWaystone,

    /// <summary>The mark of the dead mine head, off the road at an edge of the land (D-1299).</summary>
    DeadMineHead,

    /// <summary>The mark of the war graves, off the road at an edge of the land (D-1299).</summary>
    WarGraves,

    /// <summary>The mark of the bandit lookout, off the road above the way to the pass (D-1299).</summary>
    BanditLookout,
}

/// <summary>One basin of land: an ellipse whose edge the noise of the seed pushes in and out.</summary>
/// <param name="Name">The name of the basin, which a pass names.</param>
/// <param name="Land">The land of the basin.</param>
/// <param name="X">The column of the center.</param>
/// <param name="Y">The row of the center.</param>
/// <param name="RadiusX">The half width, in tiles.</param>
/// <param name="RadiusY">The half height, in tiles.</param>
public sealed record OverworldBasin(string Name, OverworldLand Land, int X, int Y, int RadiusX, int RadiusY);

/// <summary>One winding pass, one tile wide, from the top of one basin to the bottom of the next.</summary>
/// <param name="From">The basin to the south.</param>
/// <param name="To">The basin to the north.</param>
/// <param name="X">The column near which the pass leaves the basin to the south.</param>
/// <param name="Zig">The side step of each turn of the pass, in tiles. A negative value turns west first.</param>
public sealed record OverworldPass(string From, string To, int X, int Zig);

/// <summary>One thing of the map, with the tile near which the generator puts it.</summary>
/// <param name="Role">The role, which sets the rule of the place.</param>
/// <param name="Id">The id of the thing in the map file.</param>
/// <param name="Near">
/// The tile near which the generator puts the thing. The mine gate and the sealed door take the
/// mouth of the notch of their mark, so each of them holds no value, and every other role holds one.
/// </param>
public sealed record OverworldPlacement(OverworldRole Role, ContentId Id, TilePoint? Near);

/// <summary>
/// One treasure of the overworld: a chest at the end of a side route, which the lead takes one time
/// (D-1298, D-1304, D-1305). The generator puts it on the tile off the road nearest its hint.
/// </summary>
/// <param name="Id">The id of the chest in the map file.</param>
/// <param name="Near">The tile near which the generator puts the chest.</param>
public sealed record OverworldTreasure(ContentId Id, TilePoint Near);

/// <summary>One river: a winding line of water from one tile to another, inside the basins (D-1301).</summary>
/// <param name="From">The tile where the river starts.</param>
/// <param name="To">The tile where the river ends, such as the shore of a lake.</param>
public sealed record OverworldRiver(TilePoint From, TilePoint To);

/// <summary>One tile that the generator sets after each other step (D-1295).</summary>
/// <param name="At">The tile.</param>
/// <param name="Tile">The kind of the tile.</param>
public sealed record OverworldFix(TilePoint At, TileKind Tile);

/// <summary>
/// The settings of the generator of the overworld: the seed, the size, the basins, the passes,
/// the road, the ridges, the gorge, the lakes, the rivers, the things, the treasures, and the tile fixes (D-1294, D-1295, D-1301, D-1307). No rule of
/// the run reads it. The `overworld` command of Tools writes the map from it, and a test proves
/// that the committed map matches.
/// </summary>
public sealed class OverworldPlan
{
    /// <summary>The path of the file under `content/`.</summary>
    public const string Path = "worldgen/overworld.json";

    /// <summary>Every role, in one fixed order for a walk of them (G-4).</summary>
    public static readonly OverworldRole[] AllRoles =
    [
        OverworldRole.Spawn,
        OverworldRole.Village,
        OverworldRole.Town,
        OverworldRole.Fort,
        OverworldRole.IceCrossing,
        OverworldRole.Mine,
        OverworldRole.MineGate,
        OverworldRole.Gallery,
        OverworldRole.SealedDoor,
        OverworldRole.Refuge,
        OverworldRole.TownRoad,
        OverworldRole.VillageRoad,
        OverworldRole.RoadUp,
        OverworldRole.BrokenWaystone,
        OverworldRole.DeadMineHead,
        OverworldRole.WarGraves,
        OverworldRole.BanditLookout,
    ];

    private OverworldPlan(
        ContentId map,
        ulong seed,
        int width,
        int height,
        IReadOnlyList<OverworldBasin> basins,
        IReadOnlyList<OverworldPass> passes,
        int southX,
        TilePoint gorge,
        int gorgeBottom,
        int ridge,
        int rocks,
        IReadOnlyList<OverworldBasin> lakes,
        IReadOnlyList<OverworldRiver> rivers,
        IReadOnlyList<OverworldPlacement> things,
        IReadOnlyList<OverworldTreasure> treasures,
        IReadOnlyList<OverworldFix> fixes)
    {
        this.Map = map;
        this.Seed = seed;
        this.Width = width;
        this.Height = height;
        this.Basins = basins;
        this.Passes = passes;
        this.SouthX = southX;
        this.Gorge = gorge;
        this.GorgeBottom = gorgeBottom;
        this.Ridge = ridge;
        this.Rocks = rocks;
        this.Lakes = lakes;
        this.Rivers = rivers;
        this.Things = things;
        this.Treasures = treasures;
        this.Fixes = fixes;
    }

    /// <summary>The id of the map that the generator writes.</summary>
    public ContentId Map { get; }

    /// <summary>The seed of the noise and the turns.</summary>
    public ulong Seed { get; }

    /// <summary>The width of the map, in tiles.</summary>
    public int Width { get; }

    /// <summary>The height of the map, in tiles.</summary>
    public int Height { get; }

    /// <summary>The basins, in the order of the file. An earlier basin takes a tile that two basins hold.</summary>
    public IReadOnlyList<OverworldBasin> Basins { get; }

    /// <summary>The passes, from the south to the north: the low pass first, then the high pass.</summary>
    public IReadOnlyList<OverworldPass> Passes { get; }

    /// <summary>The column near which the road enters from the south edge.</summary>
    public int SouthX { get; }

    /// <summary>The column of the middle of the gorge, and the row of its top.</summary>
    public TilePoint Gorge { get; }

    /// <summary>The row of the bottom of the gorge.</summary>
    public int GorgeBottom { get; }

    /// <summary>
    /// The width of the band of the noise that turns into a rock ridge inside a basin, in 1024ths.
    /// A wider band splits the middle of the land more (D-1300).
    /// </summary>
    public int Ridge { get; }

    /// <summary>
    /// The value of the noise above which a tile inside a basin turns into a cluster of rock, in
    /// 1024ths, or 0 for no cluster. A lower value makes more rock in the middle of the land (D-1300).
    /// </summary>
    public int Rocks { get; }

    /// <summary>The lakes, each an ellipse whose shore the noise moves. The name of each is `lake`.</summary>
    public IReadOnlyList<OverworldBasin> Lakes { get; }

    /// <summary>The rivers, in the order of the file (D-1301).</summary>
    public IReadOnlyList<OverworldRiver> Rivers { get; }

    /// <summary>Each thing that the generator places, one for each role, in the order of the file.</summary>
    public IReadOnlyList<OverworldPlacement> Things { get; }

    /// <summary>Each treasure that the generator places, in the order of the file (D-1307).</summary>
    public IReadOnlyList<OverworldTreasure> Treasures { get; }

    /// <summary>The tile fixes, in the order of the file.</summary>
    public IReadOnlyList<OverworldFix> Fixes { get; }

    /// <summary>Gives the placement of one role.</summary>
    /// <param name="role">The role.</param>
    /// <returns>The placement.</returns>
    /// <exception cref="ContentException">No thing takes the role, which the reader refuses first (T-2).</exception>
    public OverworldPlacement Of(OverworldRole role)
    {
        foreach (OverworldPlacement thing in this.Things)
        {
            if (thing.Role == role)
            {
                return thing;
            }
        }

        throw ContentException.ForField(Path, "things", $"no thing takes the role '{NameOf(role)}' (D-1295)");
    }

    /// <summary>Gives the basin of one name.</summary>
    /// <param name="name">The name.</param>
    /// <returns>The basin.</returns>
    /// <exception cref="ContentException">No basin takes the name, which the reader refuses first (T-2).</exception>
    public OverworldBasin BasinOf(string name)
    {
        foreach (OverworldBasin basin in this.Basins)
        {
            if (string.CompareOrdinal(basin.Name, name) == 0)
            {
                return basin;
            }
        }

        throw ContentException.ForField(Path, "basins", $"no basin takes the name '{name}' (D-1295)");
    }

    /// <summary>Reads the settings file.</summary>
    /// <param name="bytes">The bytes of the file.</param>
    /// <param name="file">The path of the file, for each error.</param>
    /// <returns>The settings.</returns>
    /// <exception cref="ContentException">
    /// A field is absent, repeated, or unknown, a value lies outside the map, a pass names an
    /// absent basin, a role is absent or repeated, a treasure repeats an id or lies outside the map, or a fix
    /// names an unknown tile (G-6, T-2).
    /// </exception>
    public static OverworldPlan Read(ReadOnlySpan<byte> bytes, string file)
    {
        var reader = new ContentReader(bytes, file);
        string? comment = null;
        ContentId? map = null;
        ulong? seed = null;
        int? width = null;
        int? height = null;
        List<OverworldBasin>? basins = null;
        List<OverworldPass>? passes = null;
        int? southX = null;
        TilePoint? gorge = null;
        int? gorgeBottom = null;
        int? ridge = null;
        int? rocks = null;
        List<OverworldBasin>? lakes = null;
        List<OverworldRiver>? rivers = null;
        List<OverworldPlacement>? things = null;
        List<OverworldTreasure>? treasures = null;
        List<OverworldFix>? fixes = null;

        int depth = reader.ReadObjectStart();
        while (reader.ReadNextField(depth, out string field))
        {
            switch (field)
            {
                case "comment":
                    comment = reader.ReadString();
                    break;
                case "map":
                    map = reader.ReadContentId(GameMap.IdKind);
                    break;
                case "seed":
                    seed = (ulong)ReadPositive(ref reader, "seed");
                    break;
                case "width":
                    width = ReadPositive(ref reader, "width");
                    break;
                case "height":
                    height = ReadPositive(ref reader, "height");
                    break;
                case "basins":
                    basins = ReadList(ref reader, ReadBasin);
                    break;
                case "passes":
                    passes = ReadList(ref reader, ReadPass);
                    break;
                case "south_x":
                    southX = ReadPositive(ref reader, "south_x");
                    break;
                case "gorge":
                    (gorge, gorgeBottom) = ReadGorge(ref reader);
                    break;
                case "ridge":
                    ridge = reader.ReadInt();
                    break;
                case "rocks":
                    rocks = reader.ReadInt();
                    break;
                case "lakes":
                    lakes = ReadList(ref reader, ReadLake);
                    break;
                case "rivers":
                    rivers = ReadList(ref reader, ReadRiver);
                    break;
                case "things":
                    things = ReadList(ref reader, ReadPlacement);
                    break;
                case "treasures":
                    treasures = ReadList(ref reader, ReadTreasure);
                    break;
                case "fixes":
                    fixes = ReadList(ref reader, ReadFix);
                    break;
                default:
                    throw reader.UnknownField(field);
            }
        }

        _ = reader.Require(comment, depth, "comment");
        var plan = new OverworldPlan(
            reader.Require(map, depth, "map"),
            reader.RequireValue(seed, depth, "seed"),
            reader.RequireInt(width, depth, "width"),
            reader.RequireInt(height, depth, "height"),
            reader.Require(basins, depth, "basins"),
            reader.Require(passes, depth, "passes"),
            reader.RequireInt(southX, depth, "south_x"),
            reader.RequireValue(gorge, depth, "gorge"),
            reader.RequireInt(gorgeBottom, depth, "gorge"),
            reader.RequireInt(ridge, depth, "ridge"),
            reader.RequireInt(rocks, depth, "rocks"),
            reader.Require(lakes, depth, "lakes"),
            reader.Require(rivers, depth, "rivers"),
            reader.Require(things, depth, "things"),
            reader.Require(treasures, depth, "treasures"),
            reader.Require(fixes, depth, "fixes"));
        reader.ReadFileEnd();

        plan.RefuseCrossErrors(file);
        return plan;
    }

    /// <summary>Gives the name of one role, as the file writes it.</summary>
    /// <param name="role">The role.</param>
    /// <returns>The name, such as `town_road`.</returns>
    /// <exception cref="ArgumentOutOfRangeException">The value names no role (T-2).</exception>
    public static string NameOf(OverworldRole role) => role switch
    {
        OverworldRole.Spawn => "spawn",
        OverworldRole.Village => "village",
        OverworldRole.Town => "town",
        OverworldRole.Fort => "fort",
        OverworldRole.IceCrossing => "ice_crossing",
        OverworldRole.Mine => "mine",
        OverworldRole.MineGate => "mine_gate",
        OverworldRole.Gallery => "gallery",
        OverworldRole.SealedDoor => "sealed_door",
        OverworldRole.Refuge => "refuge",
        OverworldRole.TownRoad => "town_road",
        OverworldRole.VillageRoad => "village_road",
        OverworldRole.RoadUp => "road_up",
        OverworldRole.BrokenWaystone => "broken_waystone",
        OverworldRole.DeadMineHead => "dead_mine_head",
        OverworldRole.WarGraves => "war_graves",
        OverworldRole.BanditLookout => "bandit_lookout",
        _ => throw new ArgumentOutOfRangeException(nameof(role), role, "the value names no role of the overworld (D-1295)"),
    };

    private delegate T ElementReader<T>(ref ContentReader reader);

    private static List<T> ReadList<T>(ref ContentReader reader, ElementReader<T> read)
    {
        List<T> list = [];
        int depth = reader.ReadArrayStart();
        while (reader.ReadNextElement(depth, list.Count))
        {
            list.Add(read(ref reader));
        }

        return list;
    }

    private static int ReadPositive(ref ContentReader reader, string field)
    {
        int value = reader.ReadInt();
        if (value < 1)
        {
            throw reader.Refuse($"the field '{field}' takes the value {value}, and it takes 1 or more (D-1295)");
        }

        return value;
    }

    private static OverworldBasin ReadBasin(ref ContentReader reader)
    {
        string? name = null;
        OverworldLand? land = null;
        int? x = null, y = null, rx = null, ry = null;

        int depth = reader.ReadObjectStart();
        while (reader.ReadNextField(depth, out string field))
        {
            switch (field)
            {
                case "name":
                    name = reader.ReadString();
                    break;
                case "land":
                    land = ReadLand(ref reader);
                    break;
                case "x":
                    x = reader.ReadInt();
                    break;
                case "y":
                    y = reader.ReadInt();
                    break;
                case "rx":
                    rx = ReadPositive(ref reader, "rx");
                    break;
                case "ry":
                    ry = ReadPositive(ref reader, "ry");
                    break;
                default:
                    throw reader.UnknownField(field);
            }
        }

        return new OverworldBasin(
            reader.Require(name, depth, "name"),
            reader.RequireValue(land, depth, "land"),
            reader.RequireInt(x, depth, "x"),
            reader.RequireInt(y, depth, "y"),
            reader.RequireInt(rx, depth, "rx"),
            reader.RequireInt(ry, depth, "ry"));
    }

    private static OverworldLand ReadLand(ref ContentReader reader)
    {
        string name = reader.ReadString();
        return name switch
        {
            "low" => OverworldLand.Low,
            "valley" => OverworldLand.Valley,
            "pass" => OverworldLand.Pass,
            _ => throw reader.Refuse($"the land '{name}' is not low, valley, or pass (D-1283)"),
        };
    }

    private static OverworldPass ReadPass(ref ContentReader reader)
    {
        string? from = null, to = null;
        int? x = null, zig = null;

        int depth = reader.ReadObjectStart();
        while (reader.ReadNextField(depth, out string field))
        {
            switch (field)
            {
                case "from":
                    from = reader.ReadString();
                    break;
                case "to":
                    to = reader.ReadString();
                    break;
                case "x":
                    x = reader.ReadInt();
                    break;
                case "zig":
                    zig = reader.ReadInt();
                    break;
                default:
                    throw reader.UnknownField(field);
            }
        }

        return new OverworldPass(
            reader.Require(from, depth, "from"),
            reader.Require(to, depth, "to"),
            reader.RequireInt(x, depth, "x"),
            reader.RequireInt(zig, depth, "zig"));
    }

    private static (TilePoint Gorge, int Bottom) ReadGorge(ref ContentReader reader)
    {
        int? x = null, top = null, bottom = null;

        int depth = reader.ReadObjectStart();
        while (reader.ReadNextField(depth, out string field))
        {
            switch (field)
            {
                case "x":
                    x = reader.ReadInt();
                    break;
                case "top":
                    top = reader.ReadInt();
                    break;
                case "bottom":
                    bottom = reader.ReadInt();
                    break;
                default:
                    throw reader.UnknownField(field);
            }
        }

        return (new TilePoint(reader.RequireInt(x, depth, "x"), reader.RequireInt(top, depth, "top")), reader.RequireInt(bottom, depth, "bottom"));
    }

    private static OverworldBasin ReadLake(ref ContentReader reader)
    {
        int? x = null, y = null, rx = null, ry = null;

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
                case "rx":
                    rx = ReadPositive(ref reader, "rx");
                    break;
                case "ry":
                    ry = ReadPositive(ref reader, "ry");
                    break;
                default:
                    throw reader.UnknownField(field);
            }
        }

        return new OverworldBasin(
            "lake",
            OverworldLand.Low,
            reader.RequireInt(x, depth, "x"),
            reader.RequireInt(y, depth, "y"),
            reader.RequireInt(rx, depth, "rx"),
            reader.RequireInt(ry, depth, "ry"));
    }

    private static OverworldRiver ReadRiver(ref ContentReader reader)
    {
        int? fromX = null, fromY = null, toX = null, toY = null;

        int depth = reader.ReadObjectStart();
        while (reader.ReadNextField(depth, out string field))
        {
            switch (field)
            {
                case "from_x":
                    fromX = reader.ReadInt();
                    break;
                case "from_y":
                    fromY = reader.ReadInt();
                    break;
                case "to_x":
                    toX = reader.ReadInt();
                    break;
                case "to_y":
                    toY = reader.ReadInt();
                    break;
                default:
                    throw reader.UnknownField(field);
            }
        }

        return new OverworldRiver(
            new TilePoint(reader.RequireInt(fromX, depth, "from_x"), reader.RequireInt(fromY, depth, "from_y")),
            new TilePoint(reader.RequireInt(toX, depth, "to_x"), reader.RequireInt(toY, depth, "to_y")));
    }

    private static OverworldPlacement ReadPlacement(ref ContentReader reader)
    {
        OverworldRole? role = null;
        ContentId? id = null;
        int? x = null, y = null;

        int depth = reader.ReadObjectStart();
        while (reader.ReadNextField(depth, out string field))
        {
            switch (field)
            {
                case "role":
                    role = ReadRole(ref reader);
                    break;
                case "id":
                    id = reader.ReadContentId();
                    break;
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

        OverworldRole readRole = reader.RequireValue(role, depth, "role");
        ContentId readId = reader.Require(id, depth, "id");
        if (readRole is OverworldRole.MineGate or OverworldRole.SealedDoor)
        {
            if (x is not null || y is not null)
            {
                throw reader.Refuse($"the role '{NameOf(readRole)}' takes the mouth of the notch of its mark, so it holds no field 'x' or 'y' (D-1295)");
            }

            return new OverworldPlacement(readRole, readId, null);
        }

        return new OverworldPlacement(readRole, readId, new TilePoint(reader.RequireInt(x, depth, "x"), reader.RequireInt(y, depth, "y")));
    }

    private static OverworldTreasure ReadTreasure(ref ContentReader reader)
    {
        ContentId? id = null;
        int? x = null, y = null;

        int depth = reader.ReadObjectStart();
        while (reader.ReadNextField(depth, out string field))
        {
            switch (field)
            {
                case "id":
                    id = reader.ReadContentId(MapThingKinds.NameOf(MapThingKind.Chest));
                    break;
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

        return new OverworldTreasure(reader.Require(id, depth, "id"), new TilePoint(reader.RequireInt(x, depth, "x"), reader.RequireInt(y, depth, "y")));
    }

    private static OverworldRole ReadRole(ref ContentReader reader)
    {
        string name = reader.ReadString();
        foreach (OverworldRole role in AllRoles)
        {
            if (string.CompareOrdinal(NameOf(role), name) == 0)
            {
                return role;
            }
        }

        throw reader.Refuse($"the role '{name}' is not a role of the overworld (D-1295)");
    }

    private static OverworldFix ReadFix(ref ContentReader reader)
    {
        int? x = null, y = null;
        string? tile = null;

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
                case "tile":
                    tile = reader.ReadString();
                    break;
                default:
                    throw reader.UnknownField(field);
            }
        }

        string readTile = reader.Require(tile, depth, "tile");
        if (readTile.Length != 1 || !TileKinds.TryOf(readTile[0], out TileKind kind))
        {
            throw reader.RefuseField(depth, "tile", $"the fix takes the tile '{readTile}', and a tile is one character of '{TileKinds.EveryCharacter}' (D-1295)");
        }

        return new OverworldFix(new TilePoint(reader.RequireInt(x, depth, "x"), reader.RequireInt(y, depth, "y")), kind);
    }

    /// <summary>Refuses a pass of an absent basin, a role that no thing or two things take, a repeated id, and a tile outside the map (T-2).</summary>
    private void RefuseCrossErrors(string file)
    {
        var names = new SortedSet<string>(StringComparer.Ordinal);
        foreach (OverworldBasin basin in this.Basins)
        {
            if (!names.Add(basin.Name))
            {
                throw ContentException.ForField(file, "basins", $"two basins take the name '{basin.Name}' (D-1295)");
            }
        }

        foreach (OverworldPass pass in this.Passes)
        {
            if (!names.Contains(pass.From) || !names.Contains(pass.To))
            {
                throw ContentException.ForField(file, "passes", $"the pass from '{pass.From}' to '{pass.To}' names a basin that the file lacks (D-1295)");
            }
        }

        if (this.Passes.Count != 2)
        {
            throw ContentException.ForField(file, "passes", $"the file holds {this.Passes.Count} passes, and the overworld of region one holds the low pass and the high pass (D-1276)");
        }

        foreach (OverworldRole role in AllRoles)
        {
            int count = 0;
            foreach (OverworldPlacement thing in this.Things)
            {
                count += thing.Role == role ? 1 : 0;
            }

            if (count != 1)
            {
                throw ContentException.ForField(file, "things", $"{count} things take the role '{NameOf(role)}', and each role takes one (D-1295)");
            }
        }

        var ids = new SortedSet<string>(StringComparer.Ordinal);
        foreach (OverworldPlacement thing in this.Things)
        {
            _ = ids.Add(thing.Id.Value);
        }

        foreach (OverworldTreasure treasure in this.Treasures)
        {
            if (!ids.Add(treasure.Id.Value))
            {
                throw ContentException.ForField(file, "treasures", $"the treasure '{treasure.Id.Value}' takes an id that another thing of the file takes, and an id is permanent (D-166)");
            }

            if (treasure.Near.X < 0 || treasure.Near.Y < 0 || treasure.Near.X >= this.Width || treasure.Near.Y >= this.Height)
            {
                throw ContentException.ForField(file, "treasures", $"the treasure '{treasure.Id.Value}' names the tile {treasure.Near}, which lies outside the map of {this.Width} by {this.Height} (D-1295)");
            }
        }

        if (this.Ridge < 0 || this.Ridge > 256 || this.Rocks < 0 || this.Rocks > 1024)
        {
            throw ContentException.ForField(file, "ridge", $"the ridge takes {this.Ridge} and the rocks take {this.Rocks}, and the ridge runs from 0 to 256 and the rocks from 0 to 1024 (D-1300)");
        }

        foreach (OverworldFix fix in this.Fixes)
        {
            if (fix.At.X < 0 || fix.At.Y < 0 || fix.At.X >= this.Width || fix.At.Y >= this.Height)
            {
                throw ContentException.ForField(file, "fixes", $"the fix at {fix.At} lies outside the map of {this.Width} by {this.Height} (D-1295)");
            }
        }
    }
}
