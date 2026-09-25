using System;
using TheThingBelow.Tools.Png;
using TheThingBelow.Tools.Screens;
using Xunit;

namespace TheThingBelow.Tests;

/// <summary>
/// The frame compare of the screen tests (D-172). It reads decoded pixels and never the
/// bytes of a file, because the compressed bytes of a PNG depend on the encoder (F-19).
/// </summary>
public sealed class ScreenCompareTests
{
    [Fact]
    public void TwoEqualImagesGiveNoDifference()
    {
        PngImage baseline = Image(4, 3, 10);
        PngImage capture = Image(4, 3, 10);

        Assert.Equal(0, ScreenCompare.Differences(baseline, capture, out PixelDifference? first));
        Assert.Null(first);
    }

    [Fact]
    public void OneChangedPixelGivesOneDifference()
    {
        // Exit test 3 of PR-41. One changed pixel fails the job.
        PngImage baseline = Image(4, 3, 10);
        byte[] pixels = Pixels(4, 3, 10);
        pixels[((1 * 4) + 2) * 4] = 200;
        var capture = new PngImage(4, 3, PngColorKind.Rgba, pixels);

        int count = ScreenCompare.Differences(baseline, capture, out PixelDifference? first);

        Assert.Equal(1, count);
        Assert.NotNull(first);
        Assert.Equal(2, first.X);
        Assert.Equal(1, first.Y);
        Assert.Equal("10,10,10,255", first.Baseline);
        Assert.Equal("200,10,10,255", first.Capture);
    }

    [Fact]
    public void EveryChangedPixelCounts()
    {
        PngImage baseline = Image(2, 2, 10);
        PngImage capture = Image(2, 2, 12);

        Assert.Equal(4, ScreenCompare.Differences(baseline, capture, out PixelDifference? first));
        Assert.NotNull(first);
        Assert.Equal(0, first.X);
        Assert.Equal(0, first.Y);
    }

    [Fact]
    public void AStepOfOneLevelOnEachChannelMatches()
    {
        // D-1080, F-108: the edge of a wall shadow drew one level apart on two runners of CI, with
        // no change of the screen. A step of one level on each channel still matches.
        PngImage baseline = Image(2, 2, 10);
        byte[] pixels = Pixels(2, 2, 11);
        pixels[3] = 254;
        var capture = new PngImage(2, 2, PngColorKind.Rgba, pixels);

        Assert.Equal(0, ScreenCompare.Differences(baseline, capture, out PixelDifference? first));
        Assert.Null(first);
    }

    [Fact]
    public void AStepOfTwoLevelsOnOneChannelFails()
    {
        // D-1080: the compare keeps each change larger than one level.
        PngImage baseline = Image(4, 3, 10);
        byte[] pixels = Pixels(4, 3, 10);
        pixels[(((2 * 4) + 1) * 4) + 1] = 12;
        var capture = new PngImage(4, 3, PngColorKind.Rgba, pixels);

        int count = ScreenCompare.Differences(baseline, capture, out PixelDifference? first);

        Assert.Equal(1, count);
        Assert.NotNull(first);
        Assert.Equal("10,10,10,255", first.Baseline);
        Assert.Equal("10,12,10,255", first.Capture);
    }

    [Fact]
    public void AnotherSizeFails()
    {
        // T-2. A capture of another size names both sizes, and never compares the rows that
        // the two images share.
        PngImage baseline = Image(4, 3, 10);
        PngImage capture = Image(4, 2, 10);

        ArgumentException thrown = Assert.Throws<ArgumentException>(
            () => ScreenCompare.Differences(baseline, capture, out _));

        Assert.Contains("4 by 3", thrown.Message, StringComparison.Ordinal);
        Assert.Contains("4 by 2", thrown.Message, StringComparison.Ordinal);
    }

    [Fact]
    public void AnotherColorKindFails()
    {
        // T-2. A picture with three channels and one with four hold different bytes for the
        // same color, so the compare refuses the pair and names both kinds.
        PngImage baseline = Image(2, 2, 10);
        var capture = new PngImage(2, 2, PngColorKind.Rgb, new byte[2 * 2 * 3]);

        ArgumentException thrown = Assert.Throws<ArgumentException>(
            () => ScreenCompare.Differences(baseline, capture, out _));

        Assert.Contains("Rgba", thrown.Message, StringComparison.Ordinal);
        Assert.Contains("Rgb", thrown.Message, StringComparison.Ordinal);
    }

    [Theory]
    [InlineData(10, 0)]
    [InlineData(11, 4)]
    [InlineData(12, 0)]
    public void OnlyAStepOfOneLevelCountsAsNear(byte captured, int near)
    {
        // OQ-246, D-1080: the compare passes a step of one level, and the report names it. An
        // equal pixel is not near, and a step of two levels is a difference and not near.
        PngImage baseline = Image(2, 2, 10);
        PngImage capture = Image(2, 2, captured);

        Assert.Equal(near, ScreenCompare.NearDifferences(baseline, capture));
    }

    private static PngImage Image(int width, int height, byte level) =>
        new(width, height, PngColorKind.Rgba, Pixels(width, height, level));

    private static byte[] Pixels(int width, int height, byte level)
    {
        var pixels = new byte[width * height * 4];
        for (int at = 0; at < pixels.Length; at += 4)
        {
            pixels[at] = level;
            pixels[at + 1] = level;
            pixels[at + 2] = level;
            pixels[at + 3] = 255;
        }

        return pixels;
    }
}
