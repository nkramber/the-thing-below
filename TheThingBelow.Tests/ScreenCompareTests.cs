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
        PngImage capture = Image(2, 2, 11);

        Assert.Equal(4, ScreenCompare.Differences(baseline, capture, out PixelDifference? first));
        Assert.NotNull(first);
        Assert.Equal(0, first.X);
        Assert.Equal(0, first.Y);
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
