using System;
using TheThingBelow.Core.Content;

namespace TheThingBelow.Core.Effects;

/// <summary>The look of a transition, which names its shader in Game (D-195, D-825).</summary>
public enum TransitionLook
{
    /// <summary>The map breaks into shards that fall away.</summary>
    Shatter,

    /// <summary>The map turns around the middle of the view.</summary>
    Swirl,

    /// <summary>The map goes out one block of art pixels at a time.</summary>
    PixelDissolve,

    /// <summary>The blocks of the map grow until the map goes out.</summary>
    Mosaic,

    /// <summary>The map shrinks to a line, and the line shrinks to a dot, as an old monitor goes off (D-619).</summary>
    CrtPowerOff,

    /// <summary>Snow covers the map until the view is white (D-194).</summary>
    SnowWhiteout,

    /// <summary>Bands close over the map, as blinds do.</summary>
    Blinds,

    /// <summary>Rings of waves bend the map.</summary>
    Ripple,

    /// <summary>A line sweeps down the view, and it closes every other row, then the rest (D-619).</summary>
    ScanlineSweep,

    /// <summary>The red and the blue of the map pull apart. A fade takes its place at the reduced level and at off (D-863).</summary>
    ColorSplit,
}

/// <summary>
/// One transition of the library: the look, the length, and the color that covers the view at
/// its end (D-182, D-195, D-939). No rule reads a transition file (D-495, D-522).
/// </summary>
public sealed class Transition
{
    /// <summary>The folder of the transition files, under `content/`.</summary>
    public const string Folder = "effects/transitions/";

    /// <summary>The kind of the id of each transition (D-646).</summary>
    public const string IdKind = "transition";

    /// <summary>The most ticks of one transition, ten seconds, so a typing fault never holds a fight for minutes (T-2).</summary>
    public const int MostTicks = 600;

    /// <summary>The names of every look, for the error of an unknown name (T-2).</summary>
    public const string EveryLookName = "shatter, swirl, pixel_dissolve, mosaic, crt_power_off, snow_whiteout, blinds, ripple, scanline_sweep, color_split";

    /// <summary>Every look, in one fixed order for a walk of them (G-4).</summary>
    public static readonly TransitionLook[] AllLooks =
    [
        TransitionLook.Shatter, TransitionLook.Swirl, TransitionLook.PixelDissolve, TransitionLook.Mosaic, TransitionLook.CrtPowerOff,
        TransitionLook.SnowWhiteout, TransitionLook.Blinds, TransitionLook.Ripple, TransitionLook.ScanlineSweep, TransitionLook.ColorSplit,
    ];

    private Transition(string file, ContentId id, TransitionLook look, int ticks, char cover)
    {
        this.File = file;
        this.Id = id;
        this.Look = look;
        this.Ticks = ticks;
        this.Cover = cover;
    }

    /// <summary>The path of the file, under `content/`, which every error names (T-2).</summary>
    public string File { get; }

    /// <summary>The id of the transition, such as `transition.shatter`.</summary>
    public ContentId Id { get; }

    /// <summary>The look, which names the shader of Game.</summary>
    public TransitionLook Look { get; }

    /// <summary>The length, in ticks (D-266, D-941).</summary>
    public int Ticks { get; }

    /// <summary>The palette key of the one color that covers the view at the end, and that the fight fades in from (D-939).</summary>
    public char Cover { get; }

    /// <summary>Tells whether a content path is a transition file.</summary>
    /// <param name="path">The path under `content/`, with `/` separators.</param>
    /// <returns>True when the path lies in the folder of the transition files.</returns>
    public static bool IsTransitionFile(string path)
    {
        ArgumentNullException.ThrowIfNull(path);

        return path.StartsWith(Folder, StringComparison.Ordinal);
    }

    /// <summary>Gives the name of one look, which the file, the shader file, and a log field use.</summary>
    /// <param name="look">The look.</param>
    /// <returns>The name, such as `snow_whiteout`.</returns>
    /// <exception cref="ArgumentOutOfRangeException">The value names no look (T-2).</exception>
    public static string NameOf(TransitionLook look) => look switch
    {
        TransitionLook.Shatter => "shatter",
        TransitionLook.Swirl => "swirl",
        TransitionLook.PixelDissolve => "pixel_dissolve",
        TransitionLook.Mosaic => "mosaic",
        TransitionLook.CrtPowerOff => "crt_power_off",
        TransitionLook.SnowWhiteout => "snow_whiteout",
        TransitionLook.Blinds => "blinds",
        TransitionLook.Ripple => "ripple",
        TransitionLook.ScanlineSweep => "scanline_sweep",
        TransitionLook.ColorSplit => "color_split",
        _ => throw new ArgumentOutOfRangeException(nameof(look), look, "the value names no transition look (D-195)"),
    };

    /// <summary>Reads one transition from the bytes of its file.</summary>
    /// <param name="bytes">The bytes of the file, as UTF-8.</param>
    /// <param name="file">The path of the file, under `content/`, for each error (T-2).</param>
    /// <returns>The transition.</returns>
    /// <exception cref="ContentException">The file breaks a rule of the reader (G-6, T-2).</exception>
    public static Transition Read(ReadOnlySpan<byte> bytes, string file)
    {
        var reader = new ContentReader(bytes, file);
        Transition transition = Read(ref reader);
        reader.ReadFileEnd();
        return transition;
    }

    private static Transition Read(ref ContentReader reader)
    {
        string? comment = null;
        ContentId? id = null;
        string? look = null;
        int? ticks = null;
        string? cover = null;

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
                case "look":
                    look = reader.ReadString();
                    break;
                case "ticks":
                    ticks = reader.ReadInt();
                    break;
                case "cover":
                    cover = reader.ReadString();
                    break;
                default:
                    throw reader.UnknownField(field);
            }
        }

        _ = reader.Require(comment, depth, "comment");
        int length = reader.RequireInt(ticks, depth, "ticks");
        if (length < 1 || length > MostTicks)
        {
            throw reader.RefuseField(depth, "ticks", $"the length is {length} ticks, and a transition takes 1 to {MostTicks} (D-266, D-941)");
        }

        string key = reader.Require(cover, depth, "cover");
        if (key.Length != 1)
        {
            throw reader.RefuseField(depth, "cover", $"the cover is '{key}', and a cover names one palette key of one character (D-181)");
        }

        return new Transition(
            reader.File,
            reader.Require(id, depth, "id"),
            LookOf(ref reader, depth, reader.Require(look, depth, "look")),
            length,
            key[0]);
    }

    private static TransitionLook LookOf(ref ContentReader reader, int depth, string name)
    {
        foreach (TransitionLook look in AllLooks)
        {
            if (string.CompareOrdinal(NameOf(look), name) == 0)
            {
                return look;
            }
        }

        throw reader.RefuseField(depth, "look", $"the look is '{name}', and a transition takes one of {EveryLookName} (D-195)");
    }
}
