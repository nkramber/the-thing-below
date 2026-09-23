using System;
using System.Collections.Generic;
using TheThingBelow.Core.Content;

namespace TheThingBelow.Core.Light;

/// <summary>
/// The glow of the world view: the values of the one glow file, which Game gives to the glow of
/// Godot (D-188, D-910, D-911). Every value is an integer in basis points, and Game turns each
/// one into the value that Godot takes, at load (D-517).
/// </summary>
/// <remarks>
/// The world draws in HDR 2D, so a pixel keeps linear light above full white. A pixel glows when
/// its brightest channel passes <see cref="Threshold"/>. The load keeps the brightest lit art of
/// every light setup below the threshold, so light alone glows, and a sprite or a tile never
/// does (F-47, <see cref="BrightestLight"/>).
/// <para>
/// The glow is one full-screen pass on every map and every fight, so the effect budget counts it
/// (D-523).
/// </para>
/// </remarks>
public sealed class Glow
{
    /// <summary>The path of the file, under `content/`.</summary>
    public const string Path = "effects/glow.json";

    /// <summary>The full-screen passes of the glow, which the effect budget counts on every map and every fight (D-523).</summary>
    public const int FullScreenPasses = 1;

    /// <summary>The count of blur levels of the glow of Godot, from the sharpest to the widest.</summary>
    public const int LevelCount = 7;

    /// <summary>
    /// The highest threshold, in basis points of linear light. Godot caps the light that the glow
    /// reads at 12, so a source needs room above the threshold.
    /// </summary>
    public const int MostThreshold = 8 * BasisPoints.One;

    /// <summary>The widest soft start of the glow above the threshold, in basis points: the limit of Godot.</summary>
    public const int MostKnee = 4 * BasisPoints.One;

    /// <summary>The highest intensity of the glow, in basis points: the limit of Godot.</summary>
    public const int MostIntensity = 8 * BasisPoints.One;

    /// <summary>The highest strength of the glow, in basis points: the limit of Godot.</summary>
    public const int MostStrength = 2 * BasisPoints.One;

    private Glow(int threshold, int knee, int intensity, int strength, IReadOnlyList<int> levels)
    {
        this.Threshold = threshold;
        this.Knee = knee;
        this.Intensity = intensity;
        this.Strength = strength;
        this.Levels = levels;
    }

    /// <summary>
    /// The linear light where the glow starts, in basis points, where 10000 is full white. It is
    /// above full white, so no art that draws with no light can glow (D-910).
    /// </summary>
    public int Threshold { get; }

    /// <summary>The span of linear light above the threshold where the glow grows to its full strength, in basis points.</summary>
    public int Knee { get; }

    /// <summary>The intensity of the glow, in basis points.</summary>
    public int Intensity { get; }

    /// <summary>The strength of the glow, in basis points.</summary>
    public int Strength { get; }

    /// <summary>The weight of each blur level, in basis points, from the sharpest to the widest.</summary>
    public IReadOnlyList<int> Levels { get; }

    /// <summary>Reads the glow from the bytes of its file.</summary>
    /// <param name="bytes">The bytes of the file, as UTF-8.</param>
    /// <param name="file">The path of the file, under `content/`, for each error (T-2).</param>
    /// <returns>The glow.</returns>
    /// <exception cref="ContentException">The file breaks a rule of the reader (G-6, T-2).</exception>
    public static Glow Read(ReadOnlySpan<byte> bytes, string file)
    {
        var reader = new ContentReader(bytes, file);
        Glow glow = Read(ref reader);
        reader.ReadFileEnd();
        return glow;
    }

    private static Glow Read(ref ContentReader reader)
    {
        string? comment = null;
        int? threshold = null;
        int? knee = null;
        int? intensity = null;
        int? strength = null;
        List<int>? levels = null;

        int depth = reader.ReadObjectStart();
        while (reader.ReadNextField(depth, out string field))
        {
            switch (field)
            {
                case "comment":
                    comment = reader.ReadString();
                    break;
                case "threshold":
                    threshold = reader.ReadInt();
                    break;
                case "knee":
                    knee = reader.ReadInt();
                    break;
                case "intensity":
                    intensity = reader.ReadInt();
                    break;
                case "strength":
                    strength = reader.ReadInt();
                    break;
                case "levels":
                    levels = ReadLevels(ref reader);
                    break;
                default:
                    throw reader.UnknownField(field);
            }
        }

        _ = reader.Require(comment, depth, "comment");
        return new Glow(
            InRange(ref reader, depth, "threshold", threshold, BasisPoints.One + 1, MostThreshold, "the threshold stays above full white, so no art with no light glows (D-910)"),
            InRange(ref reader, depth, "knee", knee, 1, MostKnee, "Godot takes this range"),
            InRange(ref reader, depth, "intensity", intensity, 1, MostIntensity, "Godot takes this range"),
            InRange(ref reader, depth, "strength", strength, 1, MostStrength, "Godot takes this range"),
            CheckLevels(ref reader, depth, reader.Require(levels, depth, "levels")));
    }

    private static List<int> ReadLevels(ref ContentReader reader)
    {
        var levels = new List<int>();
        int depth = reader.ReadArrayStart();
        while (reader.ReadNextElement(depth, levels.Count))
        {
            levels.Add(reader.ReadInt());
        }

        return levels;
    }

    private static List<int> CheckLevels(ref ContentReader reader, int depth, List<int> levels)
    {
        if (levels.Count != LevelCount)
        {
            throw reader.RefuseField(depth, "levels", $"the file holds {levels.Count} levels, and the glow of Godot takes {LevelCount}");
        }

        int total = 0;
        for (int index = 0; index < levels.Count; index += 1)
        {
            if (levels[index] < 0 || levels[index] > BasisPoints.One)
            {
                throw reader.RefuseField(depth, "levels", $"the level {index + 1} is {levels[index]}, and a level takes 0 to {BasisPoints.One} basis points");
            }

            total += levels[index];
        }

        // A glow with no level draws nothing, in silence (T-2).
        if (total == 0)
        {
            throw reader.RefuseField(depth, "levels", "every level is 0, and a glow with no level draws nothing (T-2)");
        }

        return levels;
    }

    private static int InRange(ref ContentReader reader, int depth, string field, int? value, int least, int most, string reason)
    {
        int read = reader.RequireInt(value, depth, field);
        if (read < least || read > most)
        {
            throw reader.RefuseField(depth, field, $"the value is {read}, and it takes {least} to {most} basis points: {reason}");
        }

        return read;
    }
}
