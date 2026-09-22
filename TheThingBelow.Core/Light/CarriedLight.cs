using System;
using TheThingBelow.Core.Content;

namespace TheThingBelow.Core.Light;

/// <summary>
/// The carried light: the light on the lead that follows its drawn place on each frame
/// (D-847). No rule reads it, so the file lies outside the rule folder (D-495).
/// </summary>
/// <remarks>
/// A switch of Game turns the light on or off, and it is off in play. The lit screen-test
/// fixture and the `torch` command of the console turn it on, and PR-91 connects the switch to
/// the torch item (D-847, D-848, D-851).
/// </remarks>
public sealed class CarriedLight
{
    /// <summary>The path of the file, under `content/`.</summary>
    public const string Path = "light/carried.json";

    /// <summary>The highest row of the light over the feet of the lead, in art pixels: two tiles.</summary>
    public const int MostLift = 2 * AtlasPages.TileSize;

    private CarriedLight(PointLightValues light, int x, int y)
    {
        this.Light = light;
        this.X = x;
        this.Y = y;
    }

    /// <summary>The values of the light (D-846, F-46).</summary>
    public PointLightValues Light { get; }

    /// <summary>The column of the center of the light, in art pixels east of the west edge of the lead.</summary>
    public int X { get; }

    /// <summary>
    /// The row of the center of the light, in art pixels from the feet of the lead. A negative
    /// value puts the light up the screen, in the hand of the lead.
    /// </summary>
    public int Y { get; }

    /// <summary>Reads the carried light from the bytes of its file.</summary>
    /// <param name="bytes">The bytes of the file, as UTF-8.</param>
    /// <param name="file">The path of the file, under `content/`, for each error (T-2).</param>
    /// <returns>The carried light.</returns>
    /// <exception cref="ContentException">The file breaks a rule of the reader (G-6, T-2).</exception>
    public static CarriedLight Read(ReadOnlySpan<byte> bytes, string file)
    {
        var reader = new ContentReader(bytes, file);
        CarriedLight carried = Read(ref reader);
        reader.ReadFileEnd();
        return carried;
    }

    private static CarriedLight Read(ref ContentReader reader)
    {
        var light = new PointLightFields();
        string? comment = null;
        int? x = null;
        int? y = null;

        int depth = reader.ReadObjectStart();
        while (reader.ReadNextField(depth, out string field))
        {
            switch (field)
            {
                case "comment":
                    comment = reader.ReadString();
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

        _ = reader.Require(comment, depth, "comment");
        PointLightValues values = light.Build(ref reader, depth);

        int column = reader.RequireInt(x, depth, "x");
        if (column < 0 || column >= AtlasPages.TileSize)
        {
            throw reader.RefuseField(depth, "x", $"the column is {column}, and it takes 0 to {AtlasPages.TileSize - 1} inside the tile of the lead");
        }

        int row = reader.RequireInt(y, depth, "y");
        if (row < -MostLift || row > 0)
        {
            throw reader.RefuseField(depth, "y", $"the row is {row}, and it takes {-MostLift} to 0 from the feet of the lead");
        }

        return new CarriedLight(values, column, row);
    }
}
