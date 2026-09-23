using System;
using System.Collections.Generic;
using TheThingBelow.Core.Content;

namespace TheThingBelow.Core.Light;

/// <summary>
/// One emitter of a steady stream of particles, such as snow, dust, or the flame of a torch
/// (D-187, D-890). Every value is an integer, and Game turns each one into the value that
/// Godot takes, at load (D-517).
/// </summary>
/// <remarks>
/// A hit plays a burst, and a stream does not. A stream starts its particles evenly
/// over one lifetime, and it starts them again for as long as its place shows. A particle has no
/// texture, so it draws as a square of <see cref="Size"/> art pixels with hard edges, in one
/// palette color (G-27, D-181).
/// </remarks>
/// <param name="Amount">The count of live particles of the stream.</param>
/// <param name="LifetimeTicks">The ticks that each particle lives (D-266).</param>
/// <param name="Colors">The palette keys that the particles take, one key for each particle (D-181).</param>
/// <param name="Size">The side of each particle, in art pixels.</param>
/// <param name="X">The column of the middle of the start box, in art pixels from the anchor of the stream.</param>
/// <param name="Y">The row of the middle of the start box, in art pixels from the anchor of the stream.</param>
/// <param name="HalfWidth">The half width of the start box, in art pixels. Zero is one column.</param>
/// <param name="HalfHeight">The half height of the start box, in art pixels. Zero is one row.</param>
/// <param name="Direction">The direction of the stream, in degrees: 0 points east, 90 points down the screen, and -90 points up.</param>
/// <param name="Spread">The angle to each side of the direction, in degrees.</param>
/// <param name="SlowestSpeed">The lowest start speed, in art pixels in each second of 60 ticks.</param>
/// <param name="FastestSpeed">The highest start speed, in art pixels in each second of 60 ticks.</param>
/// <param name="Gravity">The pull down the screen, in art pixels in each second for each second. A negative value pulls up.</param>
/// <param name="Glow">
/// The linear light of each particle, in basis points of its palette color, or 0 for a particle
/// that draws its palette color and never glows. A value above the glow threshold makes the
/// particle glow (D-910, D-912).
/// </param>
public sealed record StreamEmitter(
    int Amount,
    int LifetimeTicks,
    IReadOnlyList<char> Colors,
    int Size,
    int X,
    int Y,
    int HalfWidth,
    int HalfHeight,
    int Direction,
    int Spread,
    int SlowestSpeed,
    int FastestSpeed,
    int Gravity,
    int Glow)
{
    /// <summary>The most palette keys of one stream.</summary>
    public const int MostColors = 8;

    /// <summary>The largest side of a particle, in art pixels.</summary>
    public const int MostSize = 4;

    /// <summary>The largest spread, in degrees: every direction.</summary>
    public const int MostSpread = 180;

    /// <summary>The highest start speed, in art pixels in each second.</summary>
    public const int MostSpeed = 2000;

    /// <summary>The strongest pull, up or down, in art pixels in each second for each second.</summary>
    public const int MostGravity = 2000;

    /// <summary>The highest glow of a particle, in basis points: 16 times its palette color (D-912).</summary>
    public const int MostGlow = 16 * BasisPoints.One;

    /// <summary>The most live particles of one stream.</summary>
    public const int MostAmount = 2048;

    /// <summary>The longest life of a particle, in ticks: ten seconds, so snow can cross the view.</summary>
    public const int MostLifetimeTicks = 600;

    /// <summary>The largest half width of the start box, in art pixels: half the width of the view (D-842).</summary>
    public const int MostHalfWidth = 320;

    /// <summary>The largest half height of the start box, in art pixels: half the height of the view (D-842).</summary>
    public const int MostHalfHeight = 180;

    /// <summary>The farthest middle of the start box from its anchor, in art pixels: the width of the view.</summary>
    public const int MostOffset = 640;

    /// <summary>
    /// Gives the particles of one palette key: the amount split as evenly as whole numbers
    /// allow, with the rest on the first keys. Game draws each key as a node of its own, so each
    /// particle keeps one palette color (D-181).
    /// </summary>
    /// <param name="key">The place of the key in <see cref="Colors"/>.</param>
    /// <returns>The count of particles of that key. The counts of every key add up to <see cref="Amount"/>.</returns>
    /// <exception cref="ArgumentOutOfRangeException">The emitter holds no key at that place (T-2).</exception>
    public int AmountOf(int key)
    {
        ArgumentOutOfRangeException.ThrowIfNegative(key);
        ArgumentOutOfRangeException.ThrowIfGreaterThanOrEqual(key, this.Colors.Count);

        int keys = this.Colors.Count;
        return (this.Amount / keys) + (key < this.Amount % keys ? 1 : 0);
    }

    /// <summary>Gives the count of live particles of a list of streams, which the effect budget counts (D-523).</summary>
    /// <param name="emitters">The streams.</param>
    /// <returns>The sum of each amount.</returns>
    /// <exception cref="ArgumentNullException">The list is null (T-2).</exception>
    public static int ParticlesOf(IReadOnlyList<StreamEmitter> emitters)
    {
        ArgumentNullException.ThrowIfNull(emitters);

        int total = 0;
        foreach (StreamEmitter emitter in emitters)
        {
            total = checked(total + emitter.Amount);
        }

        return total;
    }

    /// <summary>Reads a list of streams: the value of an `emitters` field.</summary>
    /// <param name="reader">The reader, at the start of the array.</param>
    /// <returns>The streams, in the order of the file. The list can be empty.</returns>
    /// <exception cref="ContentException">A field is absent, unknown, or outside its limits (G-6, T-2).</exception>
    public static List<StreamEmitter> ReadList(ref ContentReader reader)
    {
        var emitters = new List<StreamEmitter>();
        int depth = reader.ReadArrayStart();
        while (reader.ReadNextElement(depth, emitters.Count))
        {
            emitters.Add(Read(ref reader));
        }

        return emitters;
    }

    /// <summary>Reads one stream, an object inside an `emitters` array.</summary>
    /// <param name="reader">The reader, at the start of the object.</param>
    /// <returns>The stream.</returns>
    /// <exception cref="ContentException">A field is absent, unknown, or outside its limits (G-6, T-2).</exception>
    public static StreamEmitter Read(ref ContentReader reader)
    {
        var values = new SortedDictionary<string, int>(StringComparer.Ordinal);
        List<char>? colors = null;

        int depth = reader.ReadObjectStart();
        while (reader.ReadNextField(depth, out string field))
        {
            if (string.CompareOrdinal(field, "colors") == 0)
            {
                colors = ReadColors(ref reader);
            }
            else if (IsIntField(field))
            {
                values[field] = reader.ReadInt();
            }
            else
            {
                throw reader.UnknownField(field);
            }
        }

        List<char> keys = reader.Require(colors, depth, "colors");
        if (keys.Count < 1 || keys.Count > MostColors)
        {
            throw reader.RefuseField(depth, "colors", $"the emitter names {keys.Count} palette keys, and it takes 1 to {MostColors}");
        }

        int slow = InRange(ref reader, depth, values, "slowest_speed", 0, MostSpeed);
        int fast = InRange(ref reader, depth, values, "fastest_speed", 0, MostSpeed);
        if (fast < slow)
        {
            throw reader.RefuseField(depth, "fastest_speed", $"the fastest speed is {fast}, below the slowest speed {slow}");
        }

        return new StreamEmitter(
            InRange(ref reader, depth, values, "amount", 1, MostAmount),
            InRange(ref reader, depth, values, "lifetime_ticks", 1, MostLifetimeTicks),
            keys,
            InRange(ref reader, depth, values, "size", 1, MostSize),
            InRange(ref reader, depth, values, "x", -MostOffset, MostOffset),
            InRange(ref reader, depth, values, "y", -MostOffset, MostOffset),
            InRange(ref reader, depth, values, "half_width", 0, MostHalfWidth),
            InRange(ref reader, depth, values, "half_height", 0, MostHalfHeight),
            InRange(ref reader, depth, values, "direction", -180, 180),
            InRange(ref reader, depth, values, "spread", 0, MostSpread),
            slow,
            fast,
            InRange(ref reader, depth, values, "gravity", -MostGravity, MostGravity),
            InRange(ref reader, depth, values, "glow", 0, MostGlow));
    }

    private static bool IsIntField(string field) => field switch
    {
        "amount" or "lifetime_ticks" or "size" or "x" or "y" or "half_width" or "half_height"
            or "direction" or "spread" or "slowest_speed" or "fastest_speed" or "gravity" or "glow" => true,
        _ => false,
    };

    private static List<char> ReadColors(ref ContentReader reader)
    {
        var keys = new List<char>();
        int depth = reader.ReadArrayStart();
        while (reader.ReadNextElement(depth, keys.Count))
        {
            string key = reader.ReadString();
            if (key.Length != 1)
            {
                throw reader.Refuse($"the color is '{key}', and an emitter names one palette key of one character (D-181)");
            }

            keys.Add(key[0]);
        }

        return keys;
    }

    private static int InRange(ref ContentReader reader, int depth, SortedDictionary<string, int> values, string field, int least, int most)
    {
        int? value = values.TryGetValue(field, out int found) ? found : null;
        int read = reader.RequireInt(value, depth, field);
        if (read < least || read > most)
        {
            throw reader.RefuseField(depth, field, $"the value is {read}, and it takes {least} to {most}");
        }

        return read;
    }
}
