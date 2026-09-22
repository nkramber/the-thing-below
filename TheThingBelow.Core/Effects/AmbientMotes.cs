using System;
using System.Collections.Generic;
using TheThingBelow.Core.Hashing;

namespace TheThingBelow.Core.Effects;

/// <summary>One mote of a weather on screen: its place in the world, its size, and its color.</summary>
/// <param name="X">The column of the north-west corner of the mote, in art pixels of the world.</param>
/// <param name="Y">The row of the north-west corner of the mote, in art pixels of the world.</param>
/// <param name="Size">The side of the mote, in art pixels.</param>
/// <param name="Color">The palette key of the mote (D-181).</param>
public sealed record Mote(int X, int Y, int Size, char Color);

/// <summary>
/// The motes of a weather at one tick, as a pure function of the tick (D-893). Game draws each
/// one, and no rule reads them (D-522, G-1).
/// </summary>
/// <remarks>
/// The world holds a grid of cells of <see cref="CellWidth"/> by <see cref="CellHeight"/> art
/// pixels, which is one view (D-842). Each cell holds the motes of its stream, and a hash of the
/// cell, the mote, and the round gives the start of each one. Thus a mote keeps its place in the
/// world while the view moves past it (F-97), and one tick gives one picture (T-7, F-100).
/// <para>
/// A mote falls for the ticks of its fall and holds its place for the rest of its life. The sway
/// reads a table of a sine of 64 steps, so the motion holds integer math (T-7).
/// </para>
/// </remarks>
public static class AmbientMotes
{
    /// <summary>The width of one cell of the grid of the world, in art pixels: one view (D-842).</summary>
    public const int CellWidth = 640;

    /// <summary>The height of one cell of the grid of the world, in art pixels: one view (D-842).</summary>
    public const int CellHeight = 360;

    /// <summary>The steps of the table of the sway: one full sway.</summary>
    public const int SwaySteps = 64;

    /// <summary>The scale of each value of the table of the sway.</summary>
    public const int SwayScale = 1000;

    /// <summary>The seed of the hash of each mote. A new value moves every mote of every map.</summary>
    private const ulong MoteSeed = 0x6D6F746573UL;

    /// <summary>A sine of one full round, in 64 steps, at the scale of <see cref="SwayScale"/>. Core holds no float, so the table holds whole numbers (G-2).</summary>
    private static readonly int[] Sine =
    [
        0, 98, 195, 290, 383, 471, 556, 634,
        707, 773, 831, 882, 924, 957, 981, 995,
        1000, 995, 981, 957, 924, 882, 831, 773,
        707, 634, 556, 471, 383, 290, 195, 98,
        0, -98, -195, -290, -383, -471, -556, -634,
        -707, -773, -831, -882, -924, -957, -981, -995,
        -1000, -995, -981, -957, -924, -882, -831, -773,
        -707, -634, -556, -471, -383, -290, -195, -98,
    ];

    /// <summary>Gives each mote of one stream that lies inside a view, at one tick.</summary>
    /// <param name="stream">The stream of the ambient file.</param>
    /// <param name="viewX">The column of the north-west corner of the view, in art pixels of the world.</param>
    /// <param name="viewY">The row of the north-west corner of the view, in art pixels of the world.</param>
    /// <param name="tick">The tick of the run, from 0.</param>
    /// <returns>The motes inside the view, in the order of the cell and of the mote.</returns>
    /// <exception cref="ArgumentNullException">The stream is null (T-2).</exception>
    /// <exception cref="ArgumentOutOfRangeException">The tick is below zero (T-2).</exception>
    public static IReadOnlyList<Mote> InView(MoteStream stream, int viewX, int viewY, long tick)
    {
        ArgumentNullException.ThrowIfNull(stream);
        ArgumentOutOfRangeException.ThrowIfNegative(tick);

        var motes = new List<Mote>();
        int firstColumn = FloorDiv(viewX - stream.SwayPixels - Math.Max(0, -stream.DriftPixels), CellWidth);
        int lastColumn = FloorDiv(viewX + CellWidth + stream.SwayPixels + Math.Max(0, stream.DriftPixels), CellWidth);
        int firstRow = FloorDiv(viewY - Math.Max(0, -stream.FallPixels), CellHeight);
        int lastRow = FloorDiv(viewY + CellHeight + Math.Max(0, stream.FallPixels), CellHeight);
        for (int row = firstRow; row <= lastRow; row += 1)
        {
            for (int column = firstColumn; column <= lastColumn; column += 1)
            {
                AddCell(motes, stream, column, row, viewX, viewY, tick);
            }
        }

        return motes;
    }

    /// <summary>Gives one mote of one cell at one tick, wherever it lies.</summary>
    /// <param name="stream">The stream of the ambient file.</param>
    /// <param name="cellX">The column of the cell of the world.</param>
    /// <param name="cellY">The row of the cell of the world.</param>
    /// <param name="mote">The place of the mote in the stream, from 0.</param>
    /// <param name="tick">The tick of the run, from 0.</param>
    /// <returns>The mote.</returns>
    /// <exception cref="ArgumentNullException">The stream is null (T-2).</exception>
    /// <exception cref="ArgumentOutOfRangeException">The mote is outside the stream, or the tick is below zero (T-2).</exception>
    public static Mote MoteAt(MoteStream stream, int cellX, int cellY, int mote, long tick)
    {
        ArgumentNullException.ThrowIfNull(stream);
        ArgumentOutOfRangeException.ThrowIfNegative(mote);
        ArgumentOutOfRangeException.ThrowIfGreaterThanOrEqual(mote, stream.Amount);
        ArgumentOutOfRangeException.ThrowIfNegative(tick);

        // Each mote of a cell starts at its own tick, so the motes of one stream never fall together.
        long start = checked(tick + ((long)mote * stream.LifetimeTicks / stream.Amount));
        long round = start / stream.LifetimeTicks;
        int age = (int)(start % stream.LifetimeTicks);

        ulong hash = HashOf(cellX, cellY, mote, round);
        int startX = checked((cellX * CellWidth) + (int)(hash % CellWidth));
        int startY = checked((cellY * CellHeight) + (int)((hash >> 20) % CellHeight));
        int phase = (int)((hash >> 40) % SwaySteps);

        int fallen = Math.Min(age, stream.FallTicks);
        int x = checked(startX + (stream.DriftPixels * fallen / stream.FallTicks) + SwayAt(stream, phase, fallen));
        int y = checked(startY + (stream.FallPixels * fallen / stream.FallTicks));
        return new Mote(x, y, stream.Size, stream.Color);
    }

    /// <summary>Gives the sway of one mote at one tick of its fall, in art pixels.</summary>
    /// <param name="stream">The stream of the ambient file.</param>
    /// <param name="phase">The step of the sway where the mote starts, from 0 to <see cref="SwaySteps"/> minus 1.</param>
    /// <param name="ticks">The ticks of the fall that ran.</param>
    /// <returns>The sway, from minus the sway of the stream to that sway.</returns>
    /// <exception cref="ArgumentNullException">The stream is null (T-2).</exception>
    public static int SwayAt(MoteStream stream, int phase, int ticks)
    {
        ArgumentNullException.ThrowIfNull(stream);

        if (stream.SwayPixels == 0)
        {
            return 0;
        }

        int step = (int)((((long)ticks * SwaySteps / stream.SwayTicks) + phase) % SwaySteps);
        return Sine[step] * stream.SwayPixels / SwayScale;
    }

    /// <summary>Gives the count of motes that one view holds, which the effect budget counts (D-523).</summary>
    /// <param name="stream">The stream of the ambient file.</param>
    /// <returns>The count of the cells of one view, which is four cells at the worst place.</returns>
    /// <exception cref="ArgumentNullException">The stream is null (T-2).</exception>
    public static int ParticlesOf(MoteStream stream)
    {
        ArgumentNullException.ThrowIfNull(stream);

        return checked(4 * stream.Amount);
    }

    private static void AddCell(List<Mote> motes, MoteStream stream, int cellX, int cellY, int viewX, int viewY, long tick)
    {
        for (int mote = 0; mote < stream.Amount; mote += 1)
        {
            Mote found = MoteAt(stream, cellX, cellY, mote, tick);
            bool inside = found.X + found.Size > viewX
                && found.X < viewX + CellWidth
                && found.Y + found.Size > viewY
                && found.Y < viewY + CellHeight;
            if (inside)
            {
                motes.Add(found);
            }
        }
    }

    private static ulong HashOf(int cellX, int cellY, int mote, long round)
    {
        var bytes = new byte[8 + 8 + 8 + 8];
        WriteLong(bytes, 0, cellX);
        WriteLong(bytes, 8, cellY);
        WriteLong(bytes, 16, mote);
        WriteLong(bytes, 24, round);
        return XxHash64.Compute(bytes, MoteSeed);
    }

    private static void WriteLong(byte[] bytes, int at, long value)
    {
        for (int index = 0; index < 8; index += 1)
        {
            bytes[at + index] = (byte)(value >> (8 * index));
        }
    }

    /// <summary>Gives the whole part of a division that rounds down, also for a negative value.</summary>
    private static int FloorDiv(int value, int step) => value >= 0 ? value / step : ((value - step + 1) / step);

}
