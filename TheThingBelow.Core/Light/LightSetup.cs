using System;
using System.Collections.Generic;
using TheThingBelow.Core.Content;
using TheThingBelow.Core.Maps;

namespace TheThingBelow.Core.Light;

/// <summary>A change of the light of one decor piece, which wins over the default of its kind (D-843).</summary>
/// <param name="Piece">The id of the piece, such as `piece.fixture_dungeon_hall_torch`.</param>
/// <param name="Light">The light that the piece gives in place of the default of its kind.</param>
/// <remarks>
/// The change names the piece and never a tile, so a moved piece keeps its change (D-843).
/// </remarks>
public sealed record LightChange(ContentId Piece, PointLightValues Light);

/// <summary>A light that no decor piece gives, at the center of one tile (D-843).</summary>
/// <param name="Id">The id of the light, such as `light.fixture_dungeon_crack`.</param>
/// <param name="Tile">The tile at the center of the light.</param>
/// <param name="Light">The values of the light.</param>
public sealed record AddedLight(ContentId Id, TilePoint Tile, PointLightValues Light);

/// <summary>
/// The light setup of one map at one time of day: the ambient light, the changes of the lights
/// of decor pieces, the added lights, and the key light of a battle (D-442, D-843, D-850).
/// </summary>
/// <remarks>
/// No rule reads light, so the file lies outside the rule folder, and new light never breaks a
/// record (D-495, D-519). The load checks the map, the pieces, and the palette keys after every
/// file is read (<see cref="LightContent"/>).
/// </remarks>
public sealed class LightSetup
{
    /// <summary>The folder of the light setups, under `content/`.</summary>
    public const string Folder = "light/setups/";

    /// <summary>The kind of the id of each added light (D-646).</summary>
    public const string LightKind = "light";

    private LightSetup(
        string file,
        ContentId map,
        TimeOfDay time,
        LightColor ambient,
        PointLightValues battle,
        IReadOnlyList<LightChange> changes,
        IReadOnlyList<AddedLight> added)
    {
        this.File = file;
        this.Map = map;
        this.Time = time;
        this.Ambient = ambient;
        this.Battle = battle;
        this.Changes = changes;
        this.Added = added;
    }

    /// <summary>The path of the file, under `content/`, which every error names (T-2).</summary>
    public string File { get; }

    /// <summary>The id of the map that the setup lights, such as `map.fixture_dungeon`.</summary>
    public ContentId Map { get; }

    /// <summary>The time of day of the setup (D-442).</summary>
    public TimeOfDay Time { get; }

    /// <summary>The ambient light of the world: the one canvas modulate (D-846).</summary>
    public LightColor Ambient { get; }

    /// <summary>The key light of a battle that begins on this map at this time (D-850).</summary>
    public PointLightValues Battle { get; }

    /// <summary>Each change of the light of one decor piece, in the order of the file (D-843).</summary>
    public IReadOnlyList<LightChange> Changes { get; }

    /// <summary>Each light that no decor piece gives, in the order of the file (D-843).</summary>
    public IReadOnlyList<AddedLight> Added { get; }

    /// <summary>Tells whether a content path is a light setup.</summary>
    /// <param name="path">The path under `content/`, with `/` separators.</param>
    /// <returns>True when the path lies in the folder of the light setups.</returns>
    public static bool IsSetupFile(string path)
    {
        ArgumentNullException.ThrowIfNull(path);

        return path.StartsWith(Folder, StringComparison.Ordinal);
    }

    /// <summary>Reads one light setup from its bytes.</summary>
    /// <param name="bytes">The bytes of the file, as UTF-8.</param>
    /// <param name="file">The path of the file, under `content/`, for each error (T-2).</param>
    /// <returns>The light setup.</returns>
    /// <exception cref="ContentException">
    /// The file breaks a rule of the reader, two changes name one piece, or two added lights take
    /// one id (G-6, T-2).
    /// </exception>
    public static LightSetup Read(ReadOnlySpan<byte> bytes, string file)
    {
        var reader = new ContentReader(bytes, file);
        LightSetup setup = Read(ref reader);
        reader.ReadFileEnd();
        return setup;
    }

    private static LightSetup Read(ref ContentReader reader)
    {
        string? comment = null;
        ContentId? map = null;
        string? time = null;
        LightColor? ambient = null;
        PointLightValues? battle = null;
        List<LightChange>? changes = null;
        List<AddedLight>? added = null;

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
                case "time":
                    time = reader.ReadString();
                    break;
                case "ambient":
                    ambient = ReadAmbient(ref reader);
                    break;
                case "battle":
                    battle = ReadBattle(ref reader);
                    break;
                case "changes":
                    changes = ReadChanges(ref reader);
                    break;
                case "added":
                    added = ReadAdded(ref reader);
                    break;
                default:
                    throw reader.UnknownField(field);
            }
        }

        _ = reader.Require(comment, depth, "comment");
        string timeName = reader.Require(time, depth, "time");
        if (!TimesOfDay.TryOf(timeName, out TimeOfDay parsed))
        {
            throw reader.RefuseField(
                depth,
                "time",
                $"the time of day is '{timeName}', and a light setup takes one of {TimesOfDay.EveryName} (D-442)");
        }

        List<LightChange> readChanges = reader.Require(changes, depth, "changes");
        List<AddedLight> readAdded = reader.Require(added, depth, "added");
        RefuseRepeatedPiece(ref reader, depth, readChanges);
        RefuseRepeatedLight(ref reader, depth, readAdded);

        return new LightSetup(
            reader.File,
            reader.Require(map, depth, "map"),
            parsed,
            reader.Require(ambient, depth, "ambient"),
            reader.Require(battle, depth, "battle"),
            readChanges,
            readAdded);
    }

    private static LightColor ReadAmbient(ref ContentReader reader)
    {
        string? color = null;
        int? strength = null;

        int depth = reader.ReadObjectStart();
        while (reader.ReadNextField(depth, out string field))
        {
            switch (field)
            {
                case "color":
                    color = reader.ReadString();
                    break;
                case "strength":
                    strength = reader.ReadInt();
                    break;
                default:
                    throw reader.UnknownField(field);
            }
        }

        return LightValues.BuildColor(ref reader, depth, color, strength, LightValues.MostAmbientStrength);
    }

    private static PointLightValues ReadBattle(ref ContentReader reader)
    {
        var light = new PointLightFields();

        int depth = reader.ReadObjectStart();
        while (reader.ReadNextField(depth, out string field))
        {
            if (!light.TryRead(ref reader, field))
            {
                throw reader.UnknownField(field);
            }
        }

        return light.Build(ref reader, depth);
    }

    private static List<LightChange> ReadChanges(ref ContentReader reader)
    {
        var changes = new List<LightChange>();
        int depth = reader.ReadArrayStart();
        while (reader.ReadNextElement(depth, changes.Count))
        {
            changes.Add(ReadChange(ref reader));
        }

        return changes;
    }

    private static LightChange ReadChange(ref ContentReader reader)
    {
        var light = new PointLightFields();
        ContentId? piece = null;

        int depth = reader.ReadObjectStart();
        while (reader.ReadNextField(depth, out string field))
        {
            if (string.CompareOrdinal(field, "piece") == 0)
            {
                piece = reader.ReadContentId(DecorFile.PieceKind);
            }
            else if (!light.TryRead(ref reader, field))
            {
                throw reader.UnknownField(field);
            }
        }

        return new LightChange(reader.Require(piece, depth, "piece"), light.Build(ref reader, depth));
    }

    private static List<AddedLight> ReadAdded(ref ContentReader reader)
    {
        var added = new List<AddedLight>();
        int depth = reader.ReadArrayStart();
        while (reader.ReadNextElement(depth, added.Count))
        {
            added.Add(ReadOneAdded(ref reader));
        }

        return added;
    }

    private static AddedLight ReadOneAdded(ref ContentReader reader)
    {
        var light = new PointLightFields();
        ContentId? id = null;
        int? x = null;
        int? y = null;

        int depth = reader.ReadObjectStart();
        while (reader.ReadNextField(depth, out string field))
        {
            switch (field)
            {
                case "id":
                    id = reader.ReadContentId(LightKind);
                    break;
                case "x":
                    x = reader.ReadInt();
                    break;
                case "y":
                    y = reader.ReadInt();
                    break;
                default:
                    if (!light.TryRead(ref reader, field))
                    {
                        throw reader.UnknownField(field);
                    }

                    break;
            }
        }

        return new AddedLight(
            reader.Require(id, depth, "id"),
            new TilePoint(reader.RequireInt(x, depth, "x"), reader.RequireInt(y, depth, "y")),
            light.Build(ref reader, depth));
    }

    private static void RefuseRepeatedPiece(ref ContentReader reader, int depth, List<LightChange> changes)
    {
        var seen = new SortedSet<string>(StringComparer.Ordinal);
        for (int index = 0; index < changes.Count; index += 1)
        {
            if (!seen.Add(changes[index].Piece.Value))
            {
                throw reader.RefuseField(
                    depth,
                    $"changes[{index}].piece",
                    $"two changes name the piece '{changes[index].Piece.Value}', and the second would win in silence (T-2, D-843)");
            }
        }
    }

    private static void RefuseRepeatedLight(ref ContentReader reader, int depth, List<AddedLight> added)
    {
        var seen = new SortedSet<string>(StringComparer.Ordinal);
        for (int index = 0; index < added.Count; index += 1)
        {
            if (!seen.Add(added[index].Id.Value))
            {
                throw reader.RefuseField(
                    depth,
                    $"added[{index}].id",
                    $"the id '{added[index].Id.Value}' names two added lights, and one id names one light (D-166)");
            }
        }
    }
}
