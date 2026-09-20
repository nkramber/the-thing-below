using System;
using TheThingBelow.Tools.Png;
using Xunit;

namespace TheThingBelow.Tests;

/// <summary>
/// The five row filters of the PNG reader (D-664). Each test puts pixels through the forward
/// filter of <see cref="PngFixtures"/>, and it asserts that the reader gives the pixels again.
/// </summary>
public sealed class PngRowFilterTests
{
    /// <summary>The five filters, which the reader must restore (D-664).</summary>
    [Theory]
    [InlineData(PngRowFilter.None)]
    [InlineData(PngRowFilter.Sub)]
    [InlineData(PngRowFilter.Up)]
    [InlineData(PngRowFilter.Average)]
    [InlineData(PngRowFilter.Paeth)]
    public void EachFilterGivesThePixelsOfTheRowAgain(byte filterType)
    {
        const int width = 5;
        const int bytesPerPixel = 4;
        byte[] pixels = PngFixtures.Pixels(width, 2, bytesPerPixel);
        byte[] filtered = PngFixtures.Filter(width, 2, bytesPerPixel, filterType, pixels);
        int stride = width * bytesPerPixel;

        byte[] first = filtered[1..(1 + stride)];
        PngRowFilter.Restore(filterType, first, new byte[stride], bytesPerPixel, "row.png");
        byte[] second = filtered[(stride + 2)..];
        PngRowFilter.Restore(filterType, second, first, bytesPerPixel, "row.png");

        Assert.Equal(pixels[..stride], first);
        Assert.Equal(pixels[stride..], second);
    }

    [Fact]
    public void AFilterByteAboveFourFailsWithTheFileAndTheReason()
    {
        byte[] row = new byte[12];

        PngException fault = Assert.Throws<PngException>(
            () => PngRowFilter.Restore(5, row, new byte[12], 3, "bad-filter.png"));

        Assert.Equal("bad-filter.png", fault.File);
        Assert.Contains("the filter byte of a row is 5", fault.Message, StringComparison.Ordinal);
    }

    [Fact]
    public void ARowAboveOfAnotherLengthFails()
    {
        byte[] row = new byte[12];

        Assert.Throws<ArgumentException>(
            () => PngRowFilter.Restore(PngRowFilter.Up, row, new byte[9], 3, "short-row.png"));
    }

    /// <summary>
    /// The Paeth prediction picks the neighbor that the estimate comes closest to, and the
    /// left byte wins a tie. The values come from the definition of the specification.
    /// </summary>
    [Theory]
    [InlineData(10, 20, 30, 10)]
    [InlineData(200, 10, 5, 200)]
    [InlineData(10, 200, 15, 200)]
    [InlineData(0, 255, 128, 128)]
    [InlineData(10, 10, 10, 10)]
    [InlineData(0, 0, 255, 0)]
    public void ThePaethPredictionPicksTheNearestNeighbor(int left, int up, int upLeft, int expected)
    {
        Assert.Equal(expected, PngRowFilter.PaethPrediction(left, up, upLeft));
    }
}
