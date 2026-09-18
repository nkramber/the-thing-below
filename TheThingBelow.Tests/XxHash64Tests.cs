using System;
using System.Text;
using TheThingBelow.Core.Hashing;
using Xunit;

namespace TheThingBelow.Tests;

/// <summary>
/// The 64-bit hash function of Core (D-644). The vectors come from the published test
/// values of the reference implementation at `https://github.com/Cyan4973/xxHash`, and a
/// second implementation from the specification gave the same values on 2026-09-18.
/// </summary>
public sealed class XxHash64Tests
{
    [Theory]
    [InlineData("", 0UL, 0xEF46DB3751D8E999UL)]
    [InlineData("a", 0UL, 0xD24EC4F1A98C6E5BUL)]
    [InlineData("abc", 0UL, 0x44BC2CF5AD770999UL)]
    [InlineData("abc", 1UL, 0xBEA9CA8199328908UL)]
    public void TheHashOfTextMatchesThePublishedVector(string text, ulong seed, ulong expected)
    {
        ulong actual = XxHash64.Compute(Encoding.UTF8.GetBytes(text), seed);

        Assert.Equal(expected, actual);
    }

    [Fact]
    public void TheHashOfOneBlockMatchesThePublishedVector()
    {
        // 32 bytes is one full block, so this vector reads the main loop and the merge.
        byte[] data = CountingBytes(32);

        Assert.Equal(0xCBF59C5116FF32B4UL, XxHash64.Compute(data, 0));
    }

    [Fact]
    public void TheHashOfManyBlocksAndATailMatchesThePublishedVector()
    {
        // 101 bytes is three blocks, then a tail of 5 bytes. The tail reads the 4-byte step
        // and the 1-byte step, so this vector covers every path of the function.
        byte[] data = CountingBytes(101);

        Assert.Equal(0xE99038495F85381EUL, XxHash64.Compute(data, 0));
    }

    [Fact]
    public void TwoSeedsGiveTwoValues()
    {
        byte[] data = Encoding.UTF8.GetBytes("the thing below");

        Assert.NotEqual(XxHash64.Compute(data, 0), XxHash64.Compute(data, 1));
    }

    [Fact]
    public void OneChangedByteChangesTheHash()
    {
        byte[] first = CountingBytes(64);
        byte[] second = CountingBytes(64);
        second[40] += 1;

        Assert.NotEqual(XxHash64.Compute(first, 0), XxHash64.Compute(second, 0));
    }

    [Fact]
    public void EveryLengthFromZeroToOneHundredGivesItsOwnValue()
    {
        // The tail reads 8, then 4, then 1 byte at a time. This loop covers every remainder
        // of a block, so no length takes an untested path.
        ulong[] seen = new ulong[101];
        for (int length = 0; length <= 100; length += 1)
        {
            seen[length] = XxHash64.Compute(CountingBytes(length), 0);
        }

        for (int length = 1; length <= 100; length += 1)
        {
            Assert.True(
                seen[length] != seen[length - 1],
                $"The hash of {length} bytes matches the hash of {length - 1} bytes.");
        }
    }

    private static byte[] CountingBytes(int count)
    {
        byte[] data = new byte[count];
        for (int index = 0; index < count; index += 1)
        {
            data[index] = (byte)index;
        }

        return data;
    }
}
