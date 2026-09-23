using System;
using System.Collections.Generic;
using TheThingBelow.Core.Hashing;

namespace TheThingBelow.Core.Streams;

/// <summary>
/// Opens the random stream of a subsystem from the seed of the run (G-4, D-643).
/// </summary>
/// <remarks>
/// Each stream takes its seed from the seed of the run and its own stream number, and no
/// order between the streams reaches the value. A new subsystem therefore never moves the
/// numbers of an existing stream, and an old run record still replays (D-643).
/// <para>
/// The split calls <see cref="XxHash64"/> and never `GetHashCode`. A string hash code of
/// .NET can differ between two runs of one program (F-35).
/// </para>
/// </remarks>
public static class RandomStreams
{
    /// <summary>Every stream number that Core holds now.</summary>
    /// <remarks>A test reads this list and fails on two streams with one number.</remarks>
    public static readonly IReadOnlyList<StreamId> All =
    [
        StreamId.Exploration,
        StreamId.Battle,
        StreamId.Progression,
        StreamId.Story,
        StreamId.Evaluator,
    ];

    /// <summary>The seed of the hash that makes each stream seed. It never changes.</summary>
    private const ulong SplitSeed = 0x5354524541_4D5331UL;

    /// <summary>Opens the stream of one subsystem.</summary>
    /// <param name="runSeed">The seed of the run, which the host picks (G-3).</param>
    /// <param name="stream">The subsystem that draws from this stream.</param>
    /// <returns>The stream, at its first value.</returns>
    /// <exception cref="ArgumentOutOfRangeException">Core holds no such stream number (T-2).</exception>
    public static RandomStream Open(ulong runSeed, StreamId stream)
    {
        RequireKnown(stream);

        return new RandomStream(stream, Pcg32.FromSeed(SeedOf(runSeed, stream), (ulong)stream));
    }

    /// <summary>Gives the increment that the generator of one stream takes (D-643).</summary>
    /// <param name="stream">The subsystem that draws from this stream.</param>
    /// <returns>The increment, which <see cref="Pcg32.FromSeed"/> makes from the stream number.</returns>
    /// <exception cref="ArgumentOutOfRangeException">Core holds no such stream number (T-2).</exception>
    /// <remarks>A snapshot check reads it, so no stream of another sequence reaches a replay (T-7).</remarks>
    public static ulong IncrementOf(StreamId stream)
    {
        RequireKnown(stream);

        return Pcg32.IncrementOf((ulong)stream);
    }

    /// <summary>Gives the seed of one stream.</summary>
    /// <param name="runSeed">The seed of the run.</param>
    /// <param name="stream">The subsystem that draws from this stream.</param>
    /// <returns>The seed of that stream.</returns>
    /// <exception cref="ArgumentOutOfRangeException">Core holds no such stream number (T-2).</exception>
    public static ulong SeedOf(ulong runSeed, StreamId stream)
    {
        RequireKnown(stream);

        // The seed of a stream reads the run seed and the stream number alone. No other
        // stream and no order between the streams reaches this value (D-643).
        Span<byte> bytes = stackalloc byte[12];
        for (int offset = 0; offset < 8; offset += 1)
        {
            bytes[offset] = (byte)(runSeed >> (offset * 8));
        }

        uint number = (uint)stream;
        for (int offset = 0; offset < 4; offset += 1)
        {
            bytes[8 + offset] = (byte)(number >> (offset * 8));
        }

        return XxHash64.Compute(bytes, SplitSeed);
    }

    private static void RequireKnown(StreamId stream)
    {
        // A `StreamId` value comes from a cast in a test or a save file, so the value can
        // name no subsystem. An absent value is an error, never a stream of its own (T-2).
        bool known = stream switch
        {
            StreamId.Exploration or StreamId.Battle or
            StreamId.Progression or StreamId.Story or
            StreamId.Evaluator => true,
            _ => false,
        };

        if (!known)
        {
            throw new ArgumentOutOfRangeException(
                nameof(stream),
                stream,
                $"Core holds no stream with the number {(int)stream}.");
        }
    }
}
