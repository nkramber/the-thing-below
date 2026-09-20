using System;

namespace TheThingBelow.Core.Streams;

/// <summary>
/// The random generator of Core: PCG32, the pcg-xsh-rr 64/32 variant (D-642). It holds a
/// 64-bit state and gives a 32-bit value on each draw.
/// </summary>
/// <remarks>
/// The algorithm and the constants come from the reference C code at
/// `https://www.pcg-random.org/`, read 2026-09-18. The test class holds the published
/// output of the reference demo.
/// <para>
/// The increment carries the stream number, so two streams of one seed walk two different
/// sequences (D-643). Every operation wraps on purpose, which is the definition of the
/// algorithm, and each `unchecked` block says so (T-2).
/// </para>
/// </remarks>
public sealed class Pcg32
{
    private const ulong Multiplier = 6364136223846793005UL;

    private ulong state;
    private readonly ulong increment;

    private Pcg32(ulong state, ulong increment)
    {
        this.state = state;
        this.increment = increment;
    }

    /// <summary>Makes a generator from a seed and a stream number.</summary>
    /// <param name="seed">The seed of this stream.</param>
    /// <param name="sequence">The stream number. Two numbers give two sequences.</param>
    /// <returns>A generator at the first value of that sequence.</returns>
    public static Pcg32 FromSeed(ulong seed, ulong sequence)
    {
        // The reference seeding routine: the increment takes an odd value from the stream
        // number, then two steps mix the seed into the state.
        Pcg32 generator = new(0, IncrementOf(sequence));
        generator.Next();
        generator.state = unchecked(generator.state + seed);
        generator.Next();
        return generator;
    }

    /// <summary>Gives the increment of one stream number, which is always odd.</summary>
    /// <param name="sequence">The stream number.</param>
    /// <returns>The increment that <see cref="FromSeed"/> gives a generator of that number.</returns>
    public static ulong IncrementOf(ulong sequence) => unchecked((sequence << 1) | 1UL);

    /// <summary>Makes a generator that continues from a saved position.</summary>
    /// <param name="state">The state, from <see cref="State"/>.</param>
    /// <param name="increment">The increment, from <see cref="Increment"/>.</param>
    /// <returns>A generator that gives the next value of the saved sequence.</returns>
    /// <exception cref="ArgumentException">The increment is even (T-2).</exception>
    /// <remarks>A snapshot holds both values, so a load continues the same sequence (D-259).</remarks>
    public static Pcg32 FromSnapshot(ulong state, ulong increment)
    {
        if ((increment & 1UL) == 0)
        {
            throw new ArgumentException(
                $"The increment {increment} is even, and every PCG32 increment is odd.",
                nameof(increment));
        }

        return new Pcg32(state, increment);
    }

    /// <summary>The state of the generator. A snapshot holds it (D-259).</summary>
    public ulong State => this.state;

    /// <summary>The increment of the generator. A snapshot holds it (D-259).</summary>
    public ulong Increment => this.increment;

    /// <summary>Draws the next 32 bits and moves the state one step.</summary>
    /// <returns>The next value of the sequence.</returns>
    public uint Next()
    {
        ulong previous = this.state;
        this.state = unchecked((previous * Multiplier) + this.increment);

        uint shifted = (uint)(((previous >> 18) ^ previous) >> 27);
        int rotation = (int)(previous >> 59);
        return (shifted >> rotation) | (shifted << ((-rotation) & 31));
    }
}
