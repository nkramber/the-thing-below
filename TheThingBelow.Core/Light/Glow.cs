using System;
using TheThingBelow.Core.Content;

namespace TheThingBelow.Core.Light;

/// <summary>
/// The glow pass of the world: the values of the one glow file (D-188, D-913, D-914). Every value
/// is an integer, and Game turns each one into the value of its shader, at load (D-517).
/// </summary>
/// <remarks>
/// Each fire that glows draws a small rectangle into a mask view alone (<see cref="GlowSeed"/>).
/// The glow pass spreads the mask and adds it over the world, so light sources alone glow, and a
/// sprite or a tile never does (D-188, F-47).
/// <para>
/// The glow is smooth, or it fades in steps over blocks of art pixels as the fog does (D-907,
/// D-914). Each glow pulses on a slow wave of the tick, and never flickers (D-913). The glow is one
/// full-screen pass on every map and every fight, so the effect budget counts it (D-523).
/// </para>
/// </remarks>
public sealed class Glow
{
    /// <summary>The path of the file, under `content/`.</summary>
    public const string Path = "effects/glow.json";

    /// <summary>The full-screen passes of the glow, which the effect budget counts on every map and every fight (D-523).</summary>
    public const int FullScreenPasses = 1;

    /// <summary>The highest intensity of the glow, in basis points: eight times the color of its source, because the blur spreads a small source over many pixels.</summary>
    public const int MostIntensity = 8 * BasisPoints.One;

    /// <summary>The most steps of a stepped glow, as the fog takes (D-907).</summary>
    public const int MostSteps = 8;

    /// <summary>The largest block of a stepped glow, in art pixels, as the fog takes (D-907).</summary>
    public const int MostCellSize = 8;

    /// <summary>The longest pulse, in ticks: ten seconds.</summary>
    public const int MostPulseTicks = 600;

    /// <summary>The deepest pulse, in basis points: the glow falls to half its intensity at the low of the wave.</summary>
    public const int MostPulseDepth = BasisPoints.One / 2;

    private Glow(int intensity, int steps, int cellSize, int pulseTicks, int pulseDepth)
    {
        this.Intensity = intensity;
        this.Steps = steps;
        this.CellSize = cellSize;
        this.PulseTicks = pulseTicks;
        this.PulseDepth = pulseDepth;
    }

    /// <summary>The intensity of the glow, in basis points of the color of its source.</summary>
    public int Intensity { get; }

    /// <summary>The count of steps of the fade, or 0 for a smooth glow (D-914).</summary>
    public int Steps { get; }

    /// <summary>The side of a block of a stepped glow, in art pixels. A smooth glow reads it as 1.</summary>
    public int CellSize { get; }

    /// <summary>The ticks of one wave of the pulse (D-913).</summary>
    public int PulseTicks { get; }

    /// <summary>The part of the glow that the pulse takes away at the low of the wave, in basis points (D-913).</summary>
    public int PulseDepth { get; }

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
        int? intensity = null;
        int? steps = null;
        int? cellSize = null;
        int? pulseTicks = null;
        int? pulseDepth = null;

        int depth = reader.ReadObjectStart();
        while (reader.ReadNextField(depth, out string field))
        {
            switch (field)
            {
                case "comment":
                    comment = reader.ReadString();
                    break;
                case "intensity":
                    intensity = reader.ReadInt();
                    break;
                case "steps":
                    steps = reader.ReadInt();
                    break;
                case "cell_size":
                    cellSize = reader.ReadInt();
                    break;
                case "pulse_ticks":
                    pulseTicks = reader.ReadInt();
                    break;
                case "pulse_depth":
                    pulseDepth = reader.ReadInt();
                    break;
                default:
                    throw reader.UnknownField(field);
            }
        }

        _ = reader.Require(comment, depth, "comment");
        int stepCount = InRange(ref reader, depth, "steps", steps, 0, MostSteps);
        if (stepCount == 1)
        {
            throw reader.RefuseField(depth, "steps", "the glow takes 1 step, which draws a hard block with no fade: 0 is smooth, and 2 or more fade in steps (D-914)");
        }

        return new Glow(
            InRange(ref reader, depth, "intensity", intensity, 1, MostIntensity),
            stepCount,
            InRange(ref reader, depth, "cell_size", cellSize, 1, MostCellSize),
            InRange(ref reader, depth, "pulse_ticks", pulseTicks, 2, MostPulseTicks),
            InRange(ref reader, depth, "pulse_depth", pulseDepth, 0, MostPulseDepth));
    }

    private static int InRange(ref ContentReader reader, int depth, string field, int? value, int least, int most)
    {
        int read = reader.RequireInt(value, depth, field);
        if (read < least || read > most)
        {
            throw reader.RefuseField(depth, field, $"the value is {read}, and it takes {least} to {most}");
        }

        return read;
    }
}
