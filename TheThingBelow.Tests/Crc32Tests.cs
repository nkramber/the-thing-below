using System.Text;
using TheThingBelow.Tools.Png;
using Xunit;

namespace TheThingBelow.Tests;

/// <summary>
/// The CRC-32 of the PNG chunks (D-663). Each value below is a published check value of the
/// CRC-32 of the PNG specification, and the last one is the CRC-32 that the IEND chunk of
/// every PNG file carries.
/// </summary>
public sealed class Crc32Tests
{
    [Theory]
    [InlineData("", 0x00000000u)]
    [InlineData("a", 0xe8b7be43u)]
    [InlineData("abc", 0x352441c2u)]
    [InlineData("123456789", 0xcbf43926u)]
    [InlineData("The quick brown fox jumps over the lazy dog", 0x414fa339u)]
    [InlineData("IEND", 0xae426082u)]
    public void TheValueMatchesThePublishedCheckValue(string message, uint expected)
    {
        uint value = Crc32.Compute(Encoding.ASCII.GetBytes(message));

        Assert.Equal(expected, value);
    }

    [Fact]
    public void OneChangedBitChangesTheValue()
    {
        uint first = Crc32.Compute(Encoding.ASCII.GetBytes("IDAT"));
        uint second = Crc32.Compute(Encoding.ASCII.GetBytes("IDAU"));

        Assert.NotEqual(first, second);
    }

    [Fact]
    public void TheValueOfTheSameBytesDoesNotChange()
    {
        byte[] data = PngFixtures.Pixels(8, 8, 4);

        Assert.Equal(Crc32.Compute(data), Crc32.Compute(data));
    }
}
