using System;
using System.Collections.Generic;
using TheThingBelow.Core.Content;

namespace TheThingBelow.Core.Effects;

/// <summary>
/// One emitter of a particle effect: how many particles, which colors, how long, and how they
/// move (D-182, D-266). Every value is an integer, and Game turns each one into the value that
/// Godot takes, at load (D-517).
/// </summary>
/// <remarks>
/// A particle has no texture, so it draws as a square of <see cref="Size"/> art pixels with
/// hard edges (G-27). Each particle takes one color of <see cref="Colors"/>, and never a blend
/// of two, so the screen keeps one palette (D-181).
/// <para>
/// Every particle of the emitter starts at the same tick, as a burst.
/// </para>
/// </remarks>
/// <param name="Amount">The count of particles of one burst.</param>
/// <param name="LifetimeTicks">The ticks that each particle lives (D-266).</param>
/// <param name="Colors">The palette keys that the particles take, one key for each particle (D-181).</param>
/// <param name="Size">The side of each particle, in art pixels.</param>
/// <param name="Area">The half side of the square that the particles start inside, in art pixels. Zero is one point.</param>
/// <param name="Direction">The direction of the burst, in degrees: 0 points away from the attacker, 90 points down the screen, and -90 points up.</param>
/// <param name="Spread">The angle to each side of the direction, in degrees.</param>
/// <param name="SlowestSpeed">The lowest start speed, in art pixels in each second of 60 ticks.</param>
/// <param name="FastestSpeed">The highest start speed, in art pixels in each second of 60 ticks.</param>
/// <param name="Gravity">The pull down the screen, in art pixels in each second for each second. A negative value pulls up.</param>
public sealed record ParticleEmitter(
    int Amount,
    int LifetimeTicks,
    IReadOnlyList<char> Colors,
    int Size,
    int Area,
    int Direction,
    int Spread,
    int SlowestSpeed,
    int FastestSpeed,
    int Gravity)
{
    /// <summary>The most particles of one emitter.</summary>
    public const int MostAmount = 256;

    /// <summary>The longest life of a particle, in ticks: two seconds.</summary>
    public const int MostLifetimeTicks = 120;

    /// <summary>The most palette keys of one emitter.</summary>
    public const int MostColors = 8;

    /// <summary>The largest side of a particle, in art pixels.</summary>
    public const int MostSize = 4;

    /// <summary>The largest half side of the start square, in art pixels: one tile.</summary>
    public const int MostArea = AtlasPages.TileSize;

    /// <summary>The largest spread, in degrees: every direction.</summary>
    public const int MostSpread = 180;

    /// <summary>The highest start speed, in art pixels in each second.</summary>
    public const int MostSpeed = 2000;

    /// <summary>The strongest pull, up or down, in art pixels in each second for each second.</summary>
    public const int MostGravity = 2000;

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

    /// <summary>Reads one emitter, an object inside the `emitters` array of an effect file.</summary>
    /// <param name="reader">The reader, at the start of the object.</param>
    /// <returns>The emitter.</returns>
    /// <exception cref="ContentException">A field is absent, unknown, or outside its limits (G-6, T-2).</exception>
    public static ParticleEmitter Read(ref ContentReader reader)
    {
        int? amount = null;
        int? lifetime = null;
        List<char>? colors = null;
        int? size = null;
        int? area = null;
        int? direction = null;
        int? spread = null;
        int? slowest = null;
        int? fastest = null;
        int? gravity = null;

        int depth = reader.ReadObjectStart();
        while (reader.ReadNextField(depth, out string field))
        {
            switch (field)
            {
                case "amount":
                    amount = reader.ReadInt();
                    break;
                case "lifetime_ticks":
                    lifetime = reader.ReadInt();
                    break;
                case "colors":
                    colors = ReadColors(ref reader);
                    break;
                case "size":
                    size = reader.ReadInt();
                    break;
                case "area":
                    area = reader.ReadInt();
                    break;
                case "direction":
                    direction = reader.ReadInt();
                    break;
                case "spread":
                    spread = reader.ReadInt();
                    break;
                case "slowest_speed":
                    slowest = reader.ReadInt();
                    break;
                case "fastest_speed":
                    fastest = reader.ReadInt();
                    break;
                case "gravity":
                    gravity = reader.ReadInt();
                    break;
                default:
                    throw reader.UnknownField(field);
            }
        }

        List<char> keys = reader.Require(colors, depth, "colors");
        if (keys.Count < 1 || keys.Count > MostColors)
        {
            throw reader.RefuseField(depth, "colors", $"the emitter names {keys.Count} palette keys, and it takes 1 to {MostColors}");
        }

        int slow = InRange(ref reader, depth, "slowest_speed", slowest, 0, MostSpeed);
        int fast = InRange(ref reader, depth, "fastest_speed", fastest, 0, MostSpeed);
        if (fast < slow)
        {
            throw reader.RefuseField(depth, "fastest_speed", $"the fastest speed is {fast}, below the slowest speed {slow}");
        }

        return new ParticleEmitter(
            InRange(ref reader, depth, "amount", amount, 1, MostAmount),
            InRange(ref reader, depth, "lifetime_ticks", lifetime, 1, MostLifetimeTicks),
            keys,
            InRange(ref reader, depth, "size", size, 1, MostSize),
            InRange(ref reader, depth, "area", area, 0, MostArea),
            InRange(ref reader, depth, "direction", direction, -180, 180),
            InRange(ref reader, depth, "spread", spread, 0, MostSpread),
            slow,
            fast,
            InRange(ref reader, depth, "gravity", gravity, -MostGravity, MostGravity));
    }

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
