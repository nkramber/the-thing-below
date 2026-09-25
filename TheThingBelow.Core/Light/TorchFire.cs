using System;
using System.Collections.Generic;
using System.Text;
using TheThingBelow.Core.Content;
using TheThingBelow.Core.Hashing;

namespace TheThingBelow.Core.Light;

/// <summary>One level of a torch light: the strength and the range, each in basis points of the light of the file (D-891).</summary>
/// <param name="Strength">The part of the strength, where 10000 is the strength of the file (D-169).</param>
/// <param name="Range">The part of the range, where 10000 is the range of the file.</param>
public sealed record FlickerLevel(int Strength, int Range);

/// <summary>The torch light on one tick: its level, and the jump of the flame and the light, in art pixels.</summary>
/// <param name="Level">The level of the light.</param>
/// <param name="JumpX">The jump to the east. A negative value jumps west.</param>
/// <param name="JumpY">The jump down the screen. A negative value jumps up.</param>
public sealed record FlickerStep(FlickerLevel Level, int JumpX, int JumpY);

/// <summary>
/// The glow of a fire: a soft round halo of one palette color behind the flame, which adds its
/// light to the world and pulses on a slow wave of the tick (D-913, D-1075). The halo stays below
/// the glow threshold, so it never draws as a box of full light (D-915, D-1075, F-47).
/// </summary>
/// <param name="Key">The palette key of the glow (D-181).</param>
/// <param name="Strength">The linear light at the middle of the halo, in basis points of its palette color, or 0 for a fire that never glows (D-912).</param>
/// <param name="Width">The width of the halo, in art pixels.</param>
/// <param name="Height">The height of the halo, in art pixels.</param>
/// <param name="X">The column of the middle of the halo, in art pixels from the place of the light.</param>
/// <param name="Y">The row of the middle of the halo, in art pixels from the place of the light.</param>
public sealed record GlowSeed(char Key, int Strength, int Width, int Height, int X, int Y)
{
    /// <summary>The highest strength, in basis points: 16 times the palette color. Godot caps the light that the glow reads at 12.</summary>
    public const int MostStrength = 16 * BasisPoints.One;

    /// <summary>The largest side of the halo, in art pixels: six tiles (D-1075, D-1092).</summary>
    public const int MostSide = 192;

    /// <summary>The farthest middle of the halo from the place of the light, in art pixels: two tiles.</summary>
    public const int MostOffset = 64;
}

/// <summary>
/// The fire of a torch: the flame, the embers, and the smoke as streams, and the light that
/// steps between a few levels (D-888, D-890, D-891). A wall torch and the carried light each
/// hold one.
/// </summary>
/// <remarks>
/// A hash of the id of the light and of the step number picks each step, so each torch
/// changes out of step with the next, and each tick shows one picture on every run (T-7,
/// D-172). No rule reads a torch light, so the simulation version stays (D-522).
/// </remarks>
/// <param name="StepTicks">The ticks of one step of the light.</param>
/// <param name="Levels">The levels that the light steps between.</param>
/// <param name="Jump">The farthest jump of the flame and the light from its place, in art pixels, to each side.</param>
/// <param name="Emitters">The streams of the fire, from the place of the light.</param>
/// <param name="Glow">The glow of the fire (D-912, D-913).</param>
public sealed record TorchFire(int StepTicks, IReadOnlyList<FlickerLevel> Levels, int Jump, IReadOnlyList<StreamEmitter> Emitters, GlowSeed Glow)
{
    /// <summary>The longest step, in ticks: one second.</summary>
    public const int MostStepTicks = 60;

    /// <summary>The most levels of one fire.</summary>
    public const int MostLevels = 8;

    /// <summary>The lowest part of a level, in basis points, so a torch never goes out.</summary>
    public const int LeastPart = 5000;

    /// <summary>The highest part of a level, in basis points.</summary>
    public const int MostPart = 12000;

    /// <summary>The farthest jump, in art pixels.</summary>
    public const int MostJump = 2;

    /// <summary>The seed of the hash of each step. A new value changes the pattern of every torch.</summary>
    private const ulong StepSeed = 0x746F726368UL;

    /// <summary>Gives the largest range part of the levels: the widest reach that the light steps to, which the light budget counts (D-842, D-891).</summary>
    /// <returns>The part, in basis points of the range of the file.</returns>
    /// <exception cref="InvalidOperationException">The fire holds no level (T-2).</exception>
    public int WidestRange()
    {
        if (this.Levels.Count == 0)
        {
            throw new InvalidOperationException("The fire holds no level, and a fire steps between 1 or more levels (D-891, T-2).");
        }

        int widest = 0;
        foreach (FlickerLevel level in this.Levels)
        {
            widest = Math.Max(widest, level.Range);
        }

        return widest;
    }

    /// <summary>Gives the torch light on one tick.</summary>
    /// <param name="light">The id of the light: the id of the decor piece, or the name of the carried light.</param>
    /// <param name="tick">The tick, from 0.</param>
    /// <returns>The level and the jump of that tick.</returns>
    /// <exception cref="ArgumentException">The id is empty (T-2).</exception>
    /// <exception cref="ArgumentOutOfRangeException">The tick is below zero (T-2).</exception>
    public FlickerStep StepAt(string light, long tick)
    {
        ArgumentException.ThrowIfNullOrEmpty(light);
        ArgumentOutOfRangeException.ThrowIfNegative(tick);

        long step = tick / this.StepTicks;
        byte[] name = Encoding.UTF8.GetBytes(light);
        var bytes = new byte[name.Length + sizeof(long)];
        name.CopyTo(bytes, 0);
        for (int index = 0; index < sizeof(long); index += 1)
        {
            bytes[name.Length + index] = (byte)(step >> (8 * index));
        }

        ulong hash = XxHash64.Compute(bytes, StepSeed);
        int span = (2 * this.Jump) + 1;
        FlickerLevel level = this.Levels[(int)(hash % (ulong)this.Levels.Count)];
        int jumpX = (int)((hash >> 16) % (ulong)span) - this.Jump;
        int jumpY = (int)((hash >> 32) % (ulong)span) - this.Jump;
        return new FlickerStep(level, jumpX, jumpY);
    }

    /// <summary>Reads one fire, the value of a `fire` field.</summary>
    /// <param name="reader">The reader, at the start of the object.</param>
    /// <returns>The fire.</returns>
    /// <exception cref="ContentException">A field is absent, unknown, or outside its limits (G-6, T-2).</exception>
    public static TorchFire Read(ref ContentReader reader)
    {
        int? stepTicks = null;
        List<FlickerLevel>? levels = null;
        int? jump = null;
        List<StreamEmitter>? emitters = null;
        GlowSeed? glow = null;

        int depth = reader.ReadObjectStart();
        while (reader.ReadNextField(depth, out string field))
        {
            switch (field)
            {
                case "step_ticks":
                    stepTicks = reader.ReadInt();
                    break;
                case "levels":
                    levels = ReadLevels(ref reader);
                    break;
                case "jump":
                    jump = reader.ReadInt();
                    break;
                case "emitters":
                    emitters = StreamEmitter.ReadList(ref reader);
                    break;
                case "glow":
                    glow = ReadGlow(ref reader);
                    break;
                default:
                    throw reader.UnknownField(field);
            }
        }

        int ticks = reader.RequireInt(stepTicks, depth, "step_ticks");
        if (ticks < 1 || ticks > MostStepTicks)
        {
            throw reader.RefuseField(depth, "step_ticks", $"the step is {ticks} ticks, and it takes 1 to {MostStepTicks}");
        }

        List<FlickerLevel> read = reader.Require(levels, depth, "levels");
        if (read.Count < 1 || read.Count > MostLevels)
        {
            throw reader.RefuseField(depth, "levels", $"the fire holds {read.Count} levels, and it takes 1 to {MostLevels}");
        }

        int most = reader.RequireInt(jump, depth, "jump");
        if (most < 0 || most > MostJump)
        {
            throw reader.RefuseField(depth, "jump", $"the jump is {most} pixels, and it takes 0 to {MostJump}");
        }

        return new TorchFire(ticks, read, most, reader.Require(emitters, depth, "emitters"), reader.Require(glow, depth, "glow"));
    }

    private static List<FlickerLevel> ReadLevels(ref ContentReader reader)
    {
        var levels = new List<FlickerLevel>();
        int depth = reader.ReadArrayStart();
        while (reader.ReadNextElement(depth, levels.Count))
        {
            int? strength = null;
            int? range = null;
            int level = reader.ReadObjectStart();
            while (reader.ReadNextField(level, out string field))
            {
                switch (field)
                {
                    case "strength":
                        strength = reader.ReadInt();
                        break;
                    case "range":
                        range = reader.ReadInt();
                        break;
                    default:
                        throw reader.UnknownField(field);
                }
            }

            levels.Add(new FlickerLevel(Part(ref reader, level, "strength", strength), Part(ref reader, level, "range", range)));
        }

        return levels;
    }

    private static GlowSeed ReadGlow(ref ContentReader reader)
    {
        string? key = null;
        var values = new SortedDictionary<string, int>(StringComparer.Ordinal);
        int depth = reader.ReadObjectStart();
        while (reader.ReadNextField(depth, out string field))
        {
            switch (field)
            {
                case "color":
                    key = reader.ReadString();
                    break;
                case "strength" or "width" or "height" or "x" or "y":
                    values[field] = reader.ReadInt();
                    break;
                default:
                    throw reader.UnknownField(field);
            }
        }

        string text = reader.Require(key, depth, "color");
        if (text.Length != 1)
        {
            throw reader.RefuseField(depth, "color", $"the color is '{text}', and a glow names one palette key of one character (D-181)");
        }

        return new GlowSeed(
            text[0],
            GlowValue(ref reader, depth, values, "strength", 0, GlowSeed.MostStrength),
            GlowValue(ref reader, depth, values, "width", 1, GlowSeed.MostSide),
            GlowValue(ref reader, depth, values, "height", 1, GlowSeed.MostSide),
            GlowValue(ref reader, depth, values, "x", -GlowSeed.MostOffset, GlowSeed.MostOffset),
            GlowValue(ref reader, depth, values, "y", -GlowSeed.MostOffset, GlowSeed.MostOffset));
    }

    private static int GlowValue(ref ContentReader reader, int depth, SortedDictionary<string, int> values, string field, int least, int most)
    {
        int? value = values.TryGetValue(field, out int found) ? found : null;
        int read = reader.RequireInt(value, depth, field);
        if (read < least || read > most)
        {
            throw reader.RefuseField(depth, field, $"the glow value is {read}, and it takes {least} to {most}");
        }

        return read;
    }

    private static int Part(ref ContentReader reader, int depth, string field, int? value)
    {
        int read = reader.RequireInt(value, depth, field);
        if (read < LeastPart || read > MostPart)
        {
            throw reader.RefuseField(depth, field, $"the part is {read} basis points, and a level takes {LeastPart} to {MostPart}");
        }

        return read;
    }
}
