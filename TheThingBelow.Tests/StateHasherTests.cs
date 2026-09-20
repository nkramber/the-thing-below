using System;
using TheThingBelow.Core.Hashing;
using Xunit;

namespace TheThingBelow.Tests;

/// <summary>
/// The state hash over the whole state in a fixed order (G-4, G-5, D-644).
/// </summary>
public sealed class StateHasherTests
{
    [Fact]
    public void TwoHashersWithTheSameValuesGiveTheSameHash()
    {
        Assert.Equal(Fill().Finish(), Fill().Finish());
    }

    [Fact]
    public void TheOrderOfTheValuesChangesTheHash()
    {
        StateHasher forward = new();
        forward.AddInt32(1);
        forward.AddInt32(2);

        StateHasher backward = new();
        backward.AddInt32(2);
        backward.AddInt32(1);

        Assert.NotEqual(forward.Finish(), backward.Finish());
    }

    [Fact]
    public void TwoTextsThatSplitTheSameCharactersGiveTwoHashes()
    {
        // The byte count comes before the bytes, so no two splits collide.
        StateHasher first = new();
        first.AddText("ab");
        first.AddText("c");

        StateHasher second = new();
        second.AddText("a");
        second.AddText("bc");

        Assert.NotEqual(first.Finish(), second.Finish());
    }

    [Fact]
    public void AnEmptyHasherGivesTheHashOfNoBytes()
    {
        StateHasher hasher = new();

        Assert.Equal(0, hasher.ByteCount);
        Assert.Equal(XxHash64.Compute(ReadOnlySpan<byte>.Empty, 0), hasher.Finish());
    }

    [Fact]
    public void TheHashDoesNotChangeWhenACallerReadsItTwoTimes()
    {
        StateHasher hasher = Fill();

        Assert.Equal(hasher.Finish(), hasher.Finish());
    }

    [Fact]
    public void ANullTextIsAnError()
    {
        StateHasher hasher = new();

        Assert.Throws<ArgumentNullException>(() => hasher.AddText(null!));
    }

    [Fact]
    public void ALongStateGrowsTheBufferAndKeepsEveryValue()
    {
        // The buffer starts at 64 bytes, so this state grows it many times.
        StateHasher grown = new();
        StateHasher direct = new();
        for (int index = 0; index < 1000; index += 1)
        {
            grown.AddInt64(index);
            direct.AddInt64(index);
        }

        Assert.Equal(8000, grown.ByteCount);
        Assert.Equal(direct.Finish(), grown.Finish());
    }

    [Fact]
    public void ATrueValueAndAFalseValueGiveTwoHashes()
    {
        StateHasher yes = new();
        yes.AddBoolean(true);

        StateHasher no = new();
        no.AddBoolean(false);

        Assert.NotEqual(yes.Finish(), no.Finish());
    }

    [Fact]
    public void EveryValueOfARangeChangesTheHash()
    {
        // A seed loop (T-3). Each failure names the value that found it.
        ulong previous = 0;
        for (int value = -500; value <= 500; value += 1)
        {
            StateHasher hasher = new();
            hasher.AddInt32(value);
            ulong current = hasher.Finish();

            Assert.True(current != previous, $"The value {value} gave the hash of the value before it.");
            previous = current;
        }
    }

    private static StateHasher Fill()
    {
        StateHasher hasher = new();
        hasher.AddInt32(-7);
        hasher.AddInt64(long.MinValue);
        hasher.AddUInt64(ulong.MaxValue);
        hasher.AddBoolean(true);
        hasher.AddText("the thing below");
        hasher.AddText(string.Empty);
        return hasher;
    }
}
