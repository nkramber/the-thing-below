using System;
using TheThingBelow.Core.Streams;
using TheThingBelow.Tools.Bots;
using Xunit;

namespace TheThingBelow.Tests;

public sealed class BotDiceTests
{
    [Fact]
    public void NoPolicySharesTheIncrementOfARuleStream()
    {
        // PR-15 exit test 6 (G-4): the numbers of a policy come from a sequence of their own.
        foreach (BotPolicyKind policy in new[] { BotPolicyKind.Random, BotPolicyKind.Greedy })
        {
            ulong increment = Pcg32.IncrementOf(BotDice.SequenceOf(policy));
            foreach (StreamId stream in RandomStreams.All)
            {
                Assert.True(increment != RandomStreams.IncrementOf(stream), $"The {BotPolicyKinds.NameOf(policy)} policy shares the increment of the stream {stream}.");
            }
        }

        Assert.NotEqual(BotDice.SequenceOf(BotPolicyKind.Random), BotDice.SequenceOf(BotPolicyKind.Greedy));
    }

    [Fact]
    public void EachPickStaysInsideTheCountAndRepeatsForTheSameSeed()
    {
        for (ulong seed = 0; seed < 50; seed += 1)
        {
            BotDice first = new(seed, BotPolicyKind.Random);
            BotDice second = new(seed, BotPolicyKind.Random);
            for (int count = 1; count <= 40; count += 1)
            {
                int pick = first.Pick(count);
                Assert.True(pick >= 0 && pick < count, $"Seed {seed}: the pick {pick} lies outside 0 to {count - 1}.");
                Assert.True(pick == second.Pick(count), $"Seed {seed}: two dice of one seed gave two picks at the count {count}.");
            }
        }
    }

    [Fact]
    public void APickOfNoChoiceIsAnError()
    {
        BotDice dice = new(1, BotPolicyKind.Random);

        Assert.Throws<ArgumentOutOfRangeException>(() => dice.Pick(0));
    }
}
