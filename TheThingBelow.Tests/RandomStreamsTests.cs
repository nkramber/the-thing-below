using System;
using System.Collections.Generic;
using TheThingBelow.Core;
using TheThingBelow.Core.Streams;
using Xunit;

namespace TheThingBelow.Tests;

/// <summary>
/// The split of the run seed into one stream for each subsystem (G-4, D-643), and the draws
/// that a stream gives (D-642).
/// </summary>
public sealed class RandomStreamsTests
{
    private static readonly RunContext Context = new(seed: 11, tick: 0, subject: "battle/roll");

    [Fact]
    public void EveryStreamHasItsOwnNumber()
    {
        SortedSet<int> numbers = [];

        foreach (StreamId stream in RandomStreams.All)
        {
            Assert.True(
                numbers.Add((int)stream),
                $"The stream {stream} takes a number that another stream already holds.");
        }

        Assert.Equal(RandomStreams.All.Count, numbers.Count);
    }

    [Fact]
    public void NoStreamTakesTheNumberZero()
    {
        // A default `StreamId` value must name no stream, because an absent value is an
        // error and never a stream of its own (T-2).
        foreach (StreamId stream in RandomStreams.All)
        {
            Assert.NotEqual(0, (int)stream);
        }
    }

    [Fact]
    public void TwoStreamsOfOneSeedNeverGiveTheSameSequence()
    {
        // Exit test 4 of section 7.11 of `phase-1-foundations.md`.
        for (int first = 0; first < RandomStreams.All.Count; first += 1)
        {
            for (int second = first + 1; second < RandomStreams.All.Count; second += 1)
            {
                AssertDifferentSequences(RandomStreams.All[first], RandomStreams.All[second]);
            }
        }
    }

    [Fact]
    public void TheSeedOfAStreamReadsTheRunSeedAndTheStreamNumberAlone()
    {
        // The rule of D-643: no order between the streams reaches the value. A caller that
        // opens the streams in another order gets the same seeds.
        ulong[] forward = new ulong[RandomStreams.All.Count];
        for (int index = 0; index < RandomStreams.All.Count; index += 1)
        {
            forward[index] = RandomStreams.SeedOf(777, RandomStreams.All[index]);
        }

        for (int index = RandomStreams.All.Count - 1; index >= 0; index -= 1)
        {
            Assert.Equal(forward[index], RandomStreams.SeedOf(777, RandomStreams.All[index]));
        }
    }

    [Fact]
    public void AStreamNumberThatCoreDoesNotHoldIsAnError()
    {
        StreamId absent = (StreamId)999;

        Assert.Throws<ArgumentOutOfRangeException>(() => RandomStreams.SeedOf(1, absent));
        Assert.Throws<ArgumentOutOfRangeException>(() => RandomStreams.Open(1, absent));
        Assert.Throws<ArgumentOutOfRangeException>(() => RandomStreams.Open(1, default));
    }

    [Fact]
    public void ADrawStaysInsideItsBound()
    {
        RandomStream stream = RandomStreams.Open(12345, StreamId.Battle);

        for (int draw = 0; draw < 5000; draw += 1)
        {
            int value = stream.NextInt(6, Context);

            Assert.InRange(value, 0, 5);
        }
    }

    [Fact]
    public void ADrawOfARangeStaysInsideThatRange()
    {
        RandomStream stream = RandomStreams.Open(12345, StreamId.Exploration);

        for (int draw = 0; draw < 5000; draw += 1)
        {
            Assert.InRange(stream.NextInt(-10, 10, Context), -10, 10);
        }
    }

    [Fact]
    public void ADrawOfOneValueAlwaysGivesZero()
    {
        RandomStream stream = RandomStreams.Open(1, StreamId.Story);

        for (int draw = 0; draw < 64; draw += 1)
        {
            Assert.Equal(0, stream.NextInt(1, Context));
        }
    }

    [Fact]
    public void ADrawReachesEveryValueOfASmallBound()
    {
        RandomStream stream = RandomStreams.Open(999, StreamId.Battle);
        bool[] seen = new bool[6];

        for (int draw = 0; draw < 2000; draw += 1)
        {
            seen[stream.NextInt(6, Context)] = true;
        }

        for (int value = 0; value < seen.Length; value += 1)
        {
            Assert.True(seen[value], $"2000 draws of a bound of 6 never gave {value}.");
        }
    }

    [Theory]
    [InlineData(0)]
    [InlineData(-1)]
    [InlineData(int.MinValue)]
    public void ABoundThatIsNotAboveZeroIsAnError(int bound)
    {
        RandomStream stream = RandomStreams.Open(1, StreamId.Battle);

        SimulationException error = Assert.Throws<SimulationException>(
            () => stream.NextInt(bound, Context));

        Assert.Contains("seed 11", error.Message, StringComparison.Ordinal);
    }

    [Fact]
    public void AnEmptyRangeIsAnError()
    {
        RandomStream stream = RandomStreams.Open(1, StreamId.Battle);

        Assert.Throws<SimulationException>(() => stream.NextInt(5, 4, Context));
    }

    [Fact]
    public void ARangeWiderThanAnIntegerIsAnError()
    {
        RandomStream stream = RandomStreams.Open(1, StreamId.Battle);

        Assert.Throws<SimulationException>(
            () => stream.NextInt(int.MinValue, int.MaxValue, Context));
    }

    [Theory]
    [InlineData(-1)]
    [InlineData(10001)]
    public void AChanceOutsideTheBasisPointRangeIsAnError(int rate)
    {
        RandomStream stream = RandomStreams.Open(1, StreamId.Battle);

        Assert.Throws<SimulationException>(() => stream.NextChance(rate, Context));
    }

    [Fact]
    public void AChanceOfZeroNeverHoldsAndAChanceOfOneAlwaysHolds()
    {
        RandomStream stream = RandomStreams.Open(64, StreamId.Battle);

        for (int draw = 0; draw < 500; draw += 1)
        {
            Assert.False(stream.NextChance(0, Context));
            Assert.True(stream.NextChance(BasisPoints.One, Context));
        }
    }

    [Fact]
    public void TheSameSeedGivesTheSameDrawsForEverySeedOfARange()
    {
        // A seed loop (T-3). Each failure names the seed that found it.
        for (ulong seed = 0; seed < 100; seed += 1)
        {
            RandomStream first = RandomStreams.Open(seed, StreamId.Progression);
            RandomStream second = RandomStreams.Open(seed, StreamId.Progression);

            for (int draw = 0; draw < 16; draw += 1)
            {
                Assert.True(
                    first.NextInt(100, Context) == second.NextInt(100, Context),
                    $"Two streams of the seed {seed} gave two different draws.");
            }
        }
    }

    [Fact]
    public void AStreamCarriesItsOwnNumber()
    {
        foreach (StreamId stream in RandomStreams.All)
        {
            Assert.Equal(stream, RandomStreams.Open(5, stream).Stream);
        }
    }

    private static void AssertDifferentSequences(StreamId first, StreamId second)
    {
        RandomStream left = RandomStreams.Open(2026, first);
        RandomStream right = RandomStreams.Open(2026, second);

        bool same = true;
        for (int draw = 0; draw < 16 && same; draw += 1)
        {
            same = left.NextUInt64() == right.NextUInt64();
        }

        Assert.False(same, $"The streams {first} and {second} gave the same 16 draws.");
    }
}
