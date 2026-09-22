using System;
using System.Collections.Generic;
using TheThingBelow.Core.Content;

namespace TheThingBelow.Core.Effects;

/// <summary>
/// One stream of motes of a weather: snow, dust, embers, or mist (D-187, D-893). Game draws
/// each mote itself, from a pure function of the tick, so one tick gives one picture on every
/// run and in every capture (T-7, D-172, F-100).
/// </summary>
/// <remarks>
/// A mote lives <see cref="LifetimeTicks"/> ticks. It falls for <see cref="FallTicks"/> ticks,
/// and it holds its place for the rest of its life, as a mote that lies on the ground. The fall
/// takes it <see cref="FallPixels"/> pixels down the screen and <see cref="DriftPixels"/> pixels
/// to the east, and the sway pulls it from side to side on the way, as a sheet of paper falls.
/// <para>
/// Each value that a file gives is a whole number of art pixels or of ticks, so the motion holds
/// integer math (T-7). A negative fall lifts a mote, as an ember rises.
/// </para>
/// </remarks>
/// <param name="Amount">The count of motes of one cell of the world (<see cref="AmbientMotes.CellWidth"/>).</param>
/// <param name="LifetimeTicks">The ticks that each mote lives.</param>
/// <param name="Colors">The palette keys of the motes, one key for each mote in turn (D-181).</param>
/// <param name="Size">The side of each mote, in art pixels.</param>
/// <param name="FallPixels">The pixels that a mote falls down the screen over its fall. A negative value lifts it.</param>
/// <param name="FallTicks">The ticks of the fall. The mote holds its place from that tick to the end of its life.</param>
/// <param name="DriftPixels">The pixels that a mote moves to the east over its fall. A negative value moves it west.</param>
/// <param name="SwayPixels">The pixels of the sway to each side. Zero holds a straight fall.</param>
/// <param name="SwayTicks">The ticks of one full sway, from the middle to each side and back.</param>
public sealed record MoteStream(
    int Amount,
    int LifetimeTicks,
    IReadOnlyList<char> Colors,
    int Size,
    int FallPixels,
    int FallTicks,
    int DriftPixels,
    int SwayPixels,
    int SwayTicks)
{
    /// <summary>The most motes of one cell.</summary>
    public const int MostAmount = 512;

    /// <summary>The longest life of a mote, in ticks: 30 seconds.</summary>
    public const int MostLifetimeTicks = 1800;

    /// <summary>The most palette keys of one stream.</summary>
    public const int MostColors = 8;

    /// <summary>The largest side of a mote, in art pixels.</summary>
    public const int MostSize = 4;

    /// <summary>The farthest fall or lift of one mote, in art pixels.</summary>
    public const int MostFall = 1024;

    /// <summary>The farthest drift of one mote, in art pixels.</summary>
    public const int MostDrift = 1024;

    /// <summary>The widest sway to each side, in art pixels.</summary>
    public const int MostSway = 32;

    /// <summary>Reads a list of mote streams: the value of an `emitters` field of an ambient file.</summary>
    /// <param name="reader">The reader, at the start of the array.</param>
    /// <returns>The streams, in the order of the file. The list can be empty.</returns>
    /// <exception cref="ContentException">A field is absent, unknown, or outside its limits (G-6, T-2).</exception>
    public static List<MoteStream> ReadList(ref ContentReader reader)
    {
        var streams = new List<MoteStream>();
        int depth = reader.ReadArrayStart();
        while (reader.ReadNextElement(depth, streams.Count))
        {
            streams.Add(Read(ref reader));
        }

        return streams;
    }

    /// <summary>Gives the palette key of one mote, which its place in the stream picks (D-181).</summary>
    /// <param name="mote">The place of the mote in the stream, from 0.</param>
    /// <returns>The key.</returns>
    /// <exception cref="ArgumentOutOfRangeException">The place is outside the stream (T-2).</exception>
    public char ColorOf(int mote)
    {
        ArgumentOutOfRangeException.ThrowIfNegative(mote);
        ArgumentOutOfRangeException.ThrowIfGreaterThanOrEqual(mote, this.Amount);

        return this.Colors[mote % this.Colors.Count];
    }

    private static MoteStream Read(ref ContentReader reader)
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
            throw reader.RefuseField(depth, "colors", $"the stream names {keys.Count} palette keys, and it takes 1 to {MostColors}");
        }

        int lifetime = InRange(ref reader, depth, values, "lifetime_ticks", 1, MostLifetimeTicks);
        int fall = InRange(ref reader, depth, values, "fall_ticks", 1, lifetime);
        return new MoteStream(
            InRange(ref reader, depth, values, "amount", 1, MostAmount),
            lifetime,
            keys,
            InRange(ref reader, depth, values, "size", 1, MostSize),
            InRange(ref reader, depth, values, "fall_pixels", -MostFall, MostFall),
            fall,
            InRange(ref reader, depth, values, "drift_pixels", -MostDrift, MostDrift),
            InRange(ref reader, depth, values, "sway_pixels", 0, MostSway),
            InRange(ref reader, depth, values, "sway_ticks", 1, MostLifetimeTicks));
    }

    private static bool IsIntField(string field) => field switch
    {
        "amount" or "lifetime_ticks" or "size" or "fall_pixels" or "fall_ticks"
            or "drift_pixels" or "sway_pixels" or "sway_ticks" => true,
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
                throw reader.Refuse($"the color is '{key}', and a stream names one palette key of one character (D-181)");
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
