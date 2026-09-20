using System;

namespace TheThingBelow.Core.Streams;

/// <summary>
/// One random stream of a run. A subsystem draws every random value from its own stream
/// (G-4). <see cref="RandomStreams"/> opens a stream from the seed of the run.
/// </summary>
public sealed class RandomStream
{
    private readonly Pcg32 generator;

    /// <summary>Makes a stream from a generator.</summary>
    /// <param name="stream">The number of this stream.</param>
    /// <param name="generator">The generator that draws the values.</param>
    /// <exception cref="ArgumentNullException">The generator is null (T-2).</exception>
    public RandomStream(StreamId stream, Pcg32 generator)
    {
        ArgumentNullException.ThrowIfNull(generator);

        this.Stream = stream;
        this.generator = generator;
    }

    /// <summary>The number of this stream.</summary>
    public StreamId Stream { get; }

    /// <summary>The generator, whose state a snapshot holds (D-259).</summary>
    public Pcg32 Generator => this.generator;

    /// <summary>Draws the next 64-bit value.</summary>
    /// <returns>The value.</returns>
    /// <remarks>PCG32 gives 32 bits on each draw, so this method takes two draws (D-642).</remarks>
    public ulong NextUInt64()
    {
        ulong high = this.generator.Next();
        ulong low = this.generator.Next();
        return (high << 32) | low;
    }

    /// <summary>Draws a value from 0 up to the bound, and never the bound.</summary>
    /// <param name="boundExclusive">The bound. It must be above zero.</param>
    /// <param name="context">The seed, the tick, and the ids, for an error (T-2).</param>
    /// <returns>A value from 0 to the bound minus 1.</returns>
    /// <exception cref="SimulationException">The bound is zero or below zero.</exception>
    /// <remarks>
    /// The method refuses the values at the top of the 32-bit range that would make one
    /// result more common than another. The count of refused values comes from the bound
    /// alone, so every machine refuses the same values and the sequence stays the same (T-7).
    /// </remarks>
    public int NextInt(int boundExclusive, RunContext context)
    {
        ArgumentNullException.ThrowIfNull(context);

        if (boundExclusive <= 0)
        {
            throw new SimulationException(
                $"a draw with a bound of {boundExclusive}, which is not above zero",
                context);
        }

        uint bound = (uint)boundExclusive;
        uint threshold = unchecked((uint)-(int)bound) % bound;

        while (true)
        {
            uint drawn = this.generator.Next();
            if (drawn >= threshold)
            {
                return (int)(drawn % bound);
            }
        }
    }

    /// <summary>Draws a value from the first bound to the second, and both are legal results.</summary>
    /// <param name="minInclusive">The lowest result.</param>
    /// <param name="maxInclusive">The highest result. It must not be below the lowest.</param>
    /// <param name="context">The seed, the tick, and the ids, for an error (T-2).</param>
    /// <returns>A value from the lowest result to the highest.</returns>
    /// <exception cref="SimulationException">The highest result is below the lowest.</exception>
    public int NextInt(int minInclusive, int maxInclusive, RunContext context)
    {
        ArgumentNullException.ThrowIfNull(context);

        if (maxInclusive < minInclusive)
        {
            throw new SimulationException(
                $"a draw from {minInclusive} to {maxInclusive}, which is an empty range",
                context);
        }

        long count = (long)maxInclusive - minInclusive + 1;
        if (count > int.MaxValue)
        {
            throw new SimulationException(
                $"a draw from {minInclusive} to {maxInclusive} holds {count} values, " +
                "and a draw reads no more than the count of an `int`",
                context);
        }

        return minInclusive + this.NextInt((int)count, context);
    }

    /// <summary>Draws true with a rate in basis points (D-169).</summary>
    /// <param name="rate">The rate. 10000 always gives true, and 0 always gives false.</param>
    /// <param name="context">The seed, the tick, and the ids, for an error (T-2).</param>
    /// <returns>True when the draw falls below the rate.</returns>
    /// <exception cref="SimulationException">The rate is below zero or above 10000.</exception>
    public bool NextChance(int rate, RunContext context)
    {
        ArgumentNullException.ThrowIfNull(context);

        if (rate < 0 || rate > BasisPoints.One)
        {
            throw new SimulationException(
                $"a chance of {rate} basis points, which is outside 0 to {BasisPoints.One}",
                context);
        }

        return this.NextInt(BasisPoints.One, context) < rate;
    }
}
