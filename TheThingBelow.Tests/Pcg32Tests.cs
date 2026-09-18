using System;
using TheThingBelow.Core.Streams;
using Xunit;

namespace TheThingBelow.Tests;

/// <summary>
/// The random generator of Core, PCG32 (D-642). The first vector is the published output of
/// the reference demo at `https://www.pcg-random.org/`, for an initial state of 42 and a
/// sequence of 54. A second implementation from the reference algorithm gave the same six
/// values on 2026-09-18.
/// </summary>
public sealed class Pcg32Tests
{
    [Fact]
    public void TheFirstSixDrawsMatchThePublishedVector()
    {
        Pcg32 generator = Pcg32.FromSeed(42, 54);

        uint[] drawn = [
            generator.Next(), generator.Next(), generator.Next(),
            generator.Next(), generator.Next(), generator.Next(),
        ];

        Assert.Equal<uint[]>(
            [0xA15C02B7, 0x7B47F409, 0xBA1D3330, 0x83D2F293, 0xBFA4784B, 0xCBED606E],
            drawn);
    }

    [Fact]
    public void TheFirstFourDrawsOfAnotherSequenceMatchTheReference()
    {
        Pcg32 generator = Pcg32.FromSeed(42, 1);

        uint[] drawn = [generator.Next(), generator.Next(), generator.Next(), generator.Next()];

        Assert.Equal<uint[]>([0x4DF1CCF9, 0xE5838752, 0x58ED9E10, 0xF3E37B51], drawn);
    }

    [Fact]
    public void TwoSequencesOfOneSeedGiveTwoDifferentDraws()
    {
        Pcg32 first = Pcg32.FromSeed(42, 1);
        Pcg32 second = Pcg32.FromSeed(42, 2);

        Assert.NotEqual(first.Next(), second.Next());
    }

    [Fact]
    public void TheIncrementOfASequenceIsAlwaysOdd()
    {
        for (ulong sequence = 0; sequence < 64; sequence += 1)
        {
            Pcg32 generator = Pcg32.FromSeed(7, sequence);

            Assert.True(
                (generator.Increment & 1UL) == 1UL,
                $"The increment of the sequence {sequence} is even.");
        }
    }

    [Fact]
    public void ASnapshotContinuesTheSameSequence()
    {
        Pcg32 original = Pcg32.FromSeed(2026, 3);
        for (int draw = 0; draw < 17; draw += 1)
        {
            original.Next();
        }

        Pcg32 loaded = Pcg32.FromSnapshot(original.State, original.Increment);

        for (int draw = 0; draw < 32; draw += 1)
        {
            Assert.Equal(original.Next(), loaded.Next());
        }
    }

    [Fact]
    public void ASnapshotWithAnEvenIncrementIsAnError()
    {
        ArgumentException error = Assert.Throws<ArgumentException>(
            () => Pcg32.FromSnapshot(state: 1, increment: 2));

        Assert.Contains("odd", error.Message, StringComparison.Ordinal);
    }

    [Fact]
    public void TheSameSeedGivesTheSameSequenceEverySeedOfARange()
    {
        // A seed loop (T-3). Each failure names the seed that found it.
        for (ulong seed = 0; seed < 200; seed += 1)
        {
            Pcg32 first = Pcg32.FromSeed(seed, 1);
            Pcg32 second = Pcg32.FromSeed(seed, 1);

            for (int draw = 0; draw < 8; draw += 1)
            {
                Assert.True(
                    first.Next() == second.Next(),
                    $"Two generators of the seed {seed} gave two different draws.");
            }
        }
    }
}
