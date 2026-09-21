using System;
using TheThingBelow.Core.Content;

namespace TheThingBelow.Core.Light;

/// <summary>
/// One kind of decor piece, such as a wall torch, with its default light (D-843, D-844). No
/// rule reads a decor kind, so the file lies outside the rule folder (D-495).
/// </summary>
/// <remarks>
/// A drawing file names the id of the kind that it draws, and this file names no art (D-519).
/// <para>
/// Each kind of PR-56 hangs on a wall and faces the tile to its south, as a wall torch does
/// (D-844). A kind that stands on the floor is a second case, and it adds a field when it
/// comes (T-1).
/// </para>
/// <para>
/// A light setup can change the light of one piece of this kind, and the change wins over
/// this default (D-843).
/// </para>
/// </remarks>
public sealed class DecorKind
{
    /// <summary>The folder of the kind files, under `content/`.</summary>
    public const string Folder = "decor/kinds/";

    /// <summary>The kind of the id of each decor kind (D-646).</summary>
    public const string IdKind = "decor";

    private DecorKind(string file, ContentId id, PointLightValues light, int lightX, int lightY)
    {
        this.File = file;
        this.Id = id;
        this.Light = light;
        this.LightX = lightX;
        this.LightY = lightY;
    }

    /// <summary>The path of the file, under `content/`, which every error names (T-2).</summary>
    public string File { get; }

    /// <summary>The id of the kind, such as `decor.wall_torch`.</summary>
    public ContentId Id { get; }

    /// <summary>The default light of each piece of this kind (D-843).</summary>
    public PointLightValues Light { get; }

    /// <summary>The column of the center of the light, in art pixels from the west edge of the tile.</summary>
    public int LightX { get; }

    /// <summary>
    /// The row of the center of the light, in art pixels from the north edge of the tile. A
    /// value of the tile size or more puts the light over the tile to the south.
    /// </summary>
    /// <remarks>
    /// Each wall casts a shadow of its full tile (D-845). A light inside the wall that holds it
    /// would lie inside that shadow, so a wall torch puts its light over the tile to the south.
    /// </remarks>
    public int LightY { get; }

    /// <summary>Tells whether a content path is a kind file.</summary>
    /// <param name="path">The path under `content/`, with `/` separators.</param>
    /// <returns>True when the path lies in the folder of the kind files.</returns>
    public static bool IsKindFile(string path)
    {
        ArgumentNullException.ThrowIfNull(path);

        return path.StartsWith(Folder, StringComparison.Ordinal);
    }

    /// <summary>Reads one decor kind from the bytes of its file.</summary>
    /// <param name="bytes">The bytes of the file, as UTF-8.</param>
    /// <param name="file">The path of the file, under `content/`, for each error (T-2).</param>
    /// <returns>The kind.</returns>
    /// <exception cref="ContentException">The file breaks a rule of the reader (G-6, T-2).</exception>
    public static DecorKind Read(ReadOnlySpan<byte> bytes, string file)
    {
        var reader = new ContentReader(bytes, file);
        DecorKind kind = Read(ref reader);
        reader.ReadFileEnd();
        return kind;
    }

    private static DecorKind Read(ref ContentReader reader)
    {
        string? comment = null;
        ContentId? id = null;
        PlacedLight? light = null;

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
                case "light":
                    light = ReadLight(ref reader);
                    break;
                default:
                    throw reader.UnknownField(field);
            }
        }

        _ = reader.Require(comment, depth, "comment");
        PlacedLight placed = reader.Require(light, depth, "light");
        return new DecorKind(
            reader.File,
            reader.Require(id, depth, "id"),
            placed.Values,
            placed.X,
            placed.Y);
    }

    private static PlacedLight ReadLight(ref ContentReader reader)
    {
        var light = new PointLightFields();
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
                    if (!light.TryRead(ref reader, field))
                    {
                        throw reader.UnknownField(field);
                    }

                    break;
            }
        }

        PointLightValues values = light.Build(ref reader, depth);

        int column = reader.RequireInt(x, depth, "x");
        if (column < 0 || column >= AtlasPages.TileSize)
        {
            throw reader.RefuseField(depth, "x", $"the column is {column}, and it takes 0 to {AtlasPages.TileSize - 1} inside the tile");
        }

        int row = reader.RequireInt(y, depth, "y");
        if (row < 0 || row >= 2 * AtlasPages.TileSize)
        {
            throw reader.RefuseField(
                depth,
                "y",
                $"the row is {row}, and it takes 0 to {(2 * AtlasPages.TileSize) - 1}: the tile, or the tile to its south");
        }

        return new PlacedLight(values, column, row);
    }

    private sealed record PlacedLight(PointLightValues Values, int X, int Y);
}
