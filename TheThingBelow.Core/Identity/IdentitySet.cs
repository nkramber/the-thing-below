using System;
using System.Collections.Generic;
using TheThingBelow.Core.Hashing;
using TheThingBelow.Core.Streams;

namespace TheThingBelow.Core.Identity;

/// <summary>
/// The replay-identity set: a fixed list of runs, each of which gives one state hash. Every
/// CI leg computes each hash and compares it with the identity file in Tests (G-5, D-504).
/// </summary>
/// <remarks>
/// PR-4 fills the set with the vectors of the fixed-point math, the streams, and the state
/// hash. PR-6 adds replays of run records, and each later Core PR adds a run. A run that
/// changes its hash also bumps <see cref="SimulationVersion"/>, and the review of that PR
/// reads each changed hash (G-17, D-504).
/// </remarks>
public static class IdentitySet
{
    /// <summary>The name of the run that reads the fixed-point math (D-641).</summary>
    public const string BasisPointsRun = "basis-points";

    /// <summary>The name of the run that reads the stream split (D-643).</summary>
    public const string StreamSplitRun = "stream-split";

    /// <summary>The name of the run that reads the bounded draws of a stream (D-642).</summary>
    public const string RandomDrawsRun = "random-draws";

    /// <summary>The name of the run that reads the state hash (D-644).</summary>
    public const string StateHashRun = "state-hash";

    /// <summary>The seed of every run of this set. It never changes.</summary>
    private const ulong RunSeed = 20260918;

    /// <summary>Every run of the set, in the order of the identity file.</summary>
    /// <remarks>The order is the ordinal order of the names, which every machine reads the same (F-39).</remarks>
    public static readonly IReadOnlyList<string> RunNames =
    [
        BasisPointsRun,
        RandomDrawsRun,
        StateHashRun,
        StreamSplitRun,
    ];

    /// <summary>Computes the state hash of one run of the set.</summary>
    /// <param name="runName">The name of the run, from <see cref="RunNames"/>.</param>
    /// <returns>The state hash of that run.</returns>
    /// <exception cref="ArgumentOutOfRangeException">The set holds no run with that name (T-2).</exception>
    public static ulong Compute(string runName)
    {
        ArgumentException.ThrowIfNullOrEmpty(runName);

        return runName switch
        {
            BasisPointsRun => ComputeBasisPoints(),
            RandomDrawsRun => ComputeRandomDraws(),
            StateHashRun => ComputeStateHash(),
            StreamSplitRun => ComputeStreamSplit(),
            _ => throw new ArgumentOutOfRangeException(
                nameof(runName),
                runName,
                $"The identity set holds no run with that name. It holds {string.Join(", ", RunNames)}."),
        };
    }

    private static ulong ComputeBasisPoints()
    {
        RunContext context = new(RunSeed, 0, $"identity/{BasisPointsRun}");
        StateHasher hasher = new();

        // The tables cover a positive value, a negative value, a rate above 100%, a rate of
        // zero, and a product that loses a fraction. Each result rounds toward zero (D-641).
        // No pair below overflows, because an overflow is its own test (T-3).
        int[] values = [0, 1, 7, 100, 9999, -1, -7, -9999, 1_000_000, -1_000_000];
        int[] rates = [0, 1, 3333, 5000, BasisPoints.One, 15000, 99999];
        int[] divisors = [1, 2, 3, 7, 10000, -3, -10000];
        int[] parts = [0, 1, 7, 100, 9999, -1, -9999, 200_000];
        int[] wholes = [1, 2, 3, 100, 9999, 214_748, -7];

        foreach (int value in values)
        {
            foreach (int rate in rates)
            {
                hasher.AddInt32(BasisPoints.Apply(value, rate, context));
            }

            foreach (int divisor in divisors)
            {
                hasher.AddInt32(BasisPoints.Divide(value, divisor, context));
            }
        }

        foreach (int part in parts)
        {
            foreach (int whole in wholes)
            {
                hasher.AddInt32(BasisPoints.RateOf(part, whole, context));
            }
        }

        return hasher.Finish();
    }

    private static ulong ComputeStreamSplit()
    {
        StateHasher hasher = new();

        foreach (StreamId stream in RandomStreams.All)
        {
            hasher.AddInt32((int)stream);
            hasher.AddUInt64(RandomStreams.SeedOf(RunSeed, stream));

            RandomStream opened = RandomStreams.Open(RunSeed, stream);
            for (int draw = 0; draw < 16; draw += 1)
            {
                hasher.AddUInt64(opened.NextUInt64());
            }
        }

        return hasher.Finish();
    }

    private static ulong ComputeRandomDraws()
    {
        RunContext context = new(RunSeed, 0, $"identity/{RandomDrawsRun}");
        StateHasher hasher = new();
        RandomStream stream = RandomStreams.Open(RunSeed, StreamId.Battle);

        // The bounds cover a bound of one, a small bound, a bound that no power of two
        // divides, and the widest bound. The last one exercises the refusal step.
        int[] bounds = [1, 2, 6, 20, 100, 3, int.MaxValue];
        foreach (int bound in bounds)
        {
            for (int draw = 0; draw < 32; draw += 1)
            {
                hasher.AddInt32(stream.NextInt(bound, context));
            }
        }

        for (int draw = 0; draw < 32; draw += 1)
        {
            hasher.AddInt32(stream.NextInt(-50, 50, context));
            hasher.AddBoolean(stream.NextChance(2500, context));
        }

        hasher.AddUInt64(stream.Generator.State);
        hasher.AddUInt64(stream.Generator.Increment);
        return hasher.Finish();
    }

    private static ulong ComputeStateHash()
    {
        StateHasher hasher = new();

        hasher.AddInt32(SimulationVersion.Current);
        hasher.AddInt32(int.MinValue);
        hasher.AddInt32(int.MaxValue);
        hasher.AddInt64(long.MinValue);
        hasher.AddInt64(long.MaxValue);
        hasher.AddUInt64(ulong.MaxValue);
        hasher.AddBoolean(true);
        hasher.AddBoolean(false);
        hasher.AddText(string.Empty);
        hasher.AddText("the thing below");

        // The text below holds characters outside the ASCII range, so the run proves that
        // the UTF-8 bytes are the same on every leg (T-7, F-39).
        hasher.AddText("a lantern, a knife, and a door é中🔑");
        return hasher.Finish();
    }
}
