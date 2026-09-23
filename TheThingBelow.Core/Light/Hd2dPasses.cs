using System;
using System.Collections.Generic;
using TheThingBelow.Core.Content;

namespace TheThingBelow.Core.Light;

/// <summary>The two modes of the passes of the HD-2D look, which the owner compares (D-917).</summary>
public enum PassMode
{
    /// <summary>Each pass reads each art pixel and fades with no steps, and its colors can leave the palette.</summary>
    Smooth,

    /// <summary>Each pass reads one value for each block of art pixels and fades in a few steps, as the fog does (D-907).</summary>
    Stepped,
}

/// <summary>
/// The passes of the HD-2D look: the values of the one effect file of the tilt-shift blur and the
/// vignette, and the mode of those two passes and of the light shafts (D-849, D-917, D-920).
/// Every value is an integer, and Game gives each one to its shader (D-517).
/// </summary>
/// <remarks>
/// The blur and the vignette draw on every map and every fight, above the fog and the light
/// shafts and under the marks and the UI (D-919, D-920). So the effect budget counts both on
/// every screen (D-523). Each light shaft takes its values from its kind (<see cref="ShaftKind"/>).
/// </remarks>
public sealed class Hd2dPasses
{
    /// <summary>The path of the file, under `content/`.</summary>
    public const string Path = "effects/hd2d.json";

    /// <summary>The full-screen passes of the blur and the vignette, which the effect budget counts on every map and every fight (D-523, D-920).</summary>
    public const int FullScreenPasses = 2;

    /// <summary>The fewest steps of a fade in the stepped mode, as the fog takes (D-907).</summary>
    public const int FewestSteps = 2;

    /// <summary>The most steps of a fade in the stepped mode, as the fog takes (D-907).</summary>
    public const int MostSteps = 8;

    /// <summary>The largest block of the stepped mode, in art pixels, as the fog takes (D-907).</summary>
    public const int MostCellSize = 8;

    /// <summary>The highest band of the blur, in art pixels: half the height of the view.</summary>
    public const int MostBlurBand = 180;

    /// <summary>The widest radius of the blur, in art pixels.</summary>
    public const int MostBlurRadius = 8;

    private Hd2dPasses(
        PassMode mode,
        int steps,
        int cellSize,
        int blurBand,
        int blurRadius,
        char vignetteKey,
        int vignetteStrength,
        int vignetteStart)
    {
        this.Mode = mode;
        this.Steps = steps;
        this.CellSize = cellSize;
        this.BlurBand = blurBand;
        this.BlurRadius = blurRadius;
        this.VignetteKey = vignetteKey;
        this.VignetteStrength = vignetteStrength;
        this.VignetteStart = vignetteStart;
    }

    /// <summary>The mode of the blur, the vignette, and the light shafts (D-917).</summary>
    public PassMode Mode { get; }

    /// <summary>The count of steps of a fade in the stepped mode.</summary>
    public int Steps { get; }

    /// <summary>The side of each block of the stepped mode, in art pixels.</summary>
    public int CellSize { get; }

    /// <summary>The height of the band of blur at the top and at the bottom of the view, in art pixels.</summary>
    public int BlurBand { get; }

    /// <summary>The radius of the blur at the edge of the view, in art pixels.</summary>
    public int BlurRadius { get; }

    /// <summary>The palette key of the dark of the vignette (D-181).</summary>
    public char VignetteKey { get; }

    /// <summary>The strength of the dark at the corners of the view, in basis points.</summary>
    public int VignetteStrength { get; }

    /// <summary>The part of the way from the middle of the view to a corner where the dark starts, in basis points.</summary>
    public int VignetteStart { get; }

    /// <summary>Reads the passes from the bytes of their file.</summary>
    /// <param name="bytes">The bytes of the file, as UTF-8.</param>
    /// <param name="file">The path of the file, under `content/`, for each error (T-2).</param>
    /// <returns>The passes.</returns>
    /// <exception cref="ContentException">The file breaks a rule of the reader (G-6, T-2).</exception>
    public static Hd2dPasses Read(ReadOnlySpan<byte> bytes, string file)
    {
        var reader = new ContentReader(bytes, file);
        Hd2dPasses passes = Read(ref reader);
        reader.ReadFileEnd();
        return passes;
    }

    /// <summary>Gives the same passes in another mode, which a capture of the screen test takes to show both modes (D-917).</summary>
    /// <param name="mode">The mode.</param>
    /// <returns>The passes in that mode.</returns>
    public Hd2dPasses WithMode(PassMode mode) =>
        new(mode, this.Steps, this.CellSize, this.BlurBand, this.BlurRadius, this.VignetteKey, this.VignetteStrength, this.VignetteStart);

    /// <summary>Gives the name of a mode in the file.</summary>
    /// <param name="mode">The mode.</param>
    /// <returns>The name, such as `smooth`.</returns>
    /// <exception cref="ArgumentOutOfRangeException">The mode has no name (T-2).</exception>
    public static string NameOf(PassMode mode) => mode switch
    {
        PassMode.Smooth => "smooth",
        PassMode.Stepped => "stepped",
        _ => throw new ArgumentOutOfRangeException(nameof(mode), mode, "The mode has no name in the file (T-2)."),
    };

    private static Hd2dPasses Read(ref ContentReader reader)
    {
        string? comment = null;
        string? mode = null;
        string? key = null;
        var values = new SortedDictionary<string, int>(StringComparer.Ordinal);

        int depth = reader.ReadObjectStart();
        while (reader.ReadNextField(depth, out string field))
        {
            switch (field)
            {
                case "comment":
                    comment = reader.ReadString();
                    break;
                case "mode":
                    mode = reader.ReadString();
                    break;
                case "vignette_color":
                    key = reader.ReadString();
                    break;
                case "steps" or "cell_size" or "blur_band" or "blur_radius" or "vignette_strength" or "vignette_start":
                    values[field] = reader.ReadInt();
                    break;
                default:
                    throw reader.UnknownField(field);
            }
        }

        _ = reader.Require(comment, depth, "comment");
        string text = reader.Require(key, depth, "vignette_color");
        if (text.Length != 1)
        {
            throw reader.RefuseField(depth, "vignette_color", $"the color is '{text}', and a vignette names one palette key of one character (D-181)");
        }

        return new Hd2dPasses(
            ModeOf(ref reader, depth, reader.Require(mode, depth, "mode")),
            InRange(ref reader, depth, values, "steps", FewestSteps, MostSteps, "the stepped mode fades in 2 to 8 steps, as the fog does (D-907)"),
            InRange(ref reader, depth, values, "cell_size", 1, MostCellSize, "a block holds 1 to 8 art pixels on a side, as the fog takes (D-907)"),
            InRange(ref reader, depth, values, "blur_band", 1, MostBlurBand, "a band of no rows draws no blur (T-2), and the two bands meet at the middle of the view"),
            InRange(ref reader, depth, values, "blur_radius", 1, MostBlurRadius, "a blur of no radius draws nothing (T-2)"),
            text[0],
            InRange(ref reader, depth, values, "vignette_strength", 1, BasisPoints.One, "a vignette of no strength draws nothing (T-2)"),
            InRange(ref reader, depth, values, "vignette_start", 0, BasisPoints.One - 1, "the dark starts before the corner"));
    }

    private static PassMode ModeOf(ref ContentReader reader, int depth, string name) => name switch
    {
        "smooth" => PassMode.Smooth,
        "stepped" => PassMode.Stepped,
        _ => throw reader.RefuseField(depth, "mode", $"the mode is '{name}', and a pass takes 'smooth' or 'stepped' (D-917)"),
    };

    private static int InRange(ref ContentReader reader, int depth, SortedDictionary<string, int> values, string field, int least, int most, string reason)
    {
        int? value = values.TryGetValue(field, out int found) ? found : null;
        int read = reader.RequireInt(value, depth, field);
        if (read < least || read > most)
        {
            throw reader.RefuseField(depth, field, $"the value is {read}, and it takes {least} to {most}: {reason}");
        }

        return read;
    }
}
