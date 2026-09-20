using System;
using TheThingBelow.Tools.Png;
using Xunit;

namespace TheThingBelow.Tests;

/// <summary>The decoded image of the PNG code: its sizes, its rows, and its checks (D-176).</summary>
public sealed class PngImageTests
{
    [Fact]
    public void AnRgbImageHasThreeBytesForEachPixel()
    {
        PngImage image = new(2, 3, PngColorKind.Rgb, PngFixtures.Pixels(2, 3, 3));

        Assert.Equal(3, image.BytesPerPixel);
        Assert.Equal(6, image.Stride);
        Assert.Equal(18, image.Pixels.Length);
    }

    [Fact]
    public void AnRgbaImageHasFourBytesForEachPixel()
    {
        PngImage image = new(2, 3, PngColorKind.Rgba, PngFixtures.Pixels(2, 3, 4));

        Assert.Equal(4, image.BytesPerPixel);
        Assert.Equal(8, image.Stride);
        Assert.Equal(24, image.Pixels.Length);
    }

    [Fact]
    public void EachRowHoldsTheBytesOfThatRow()
    {
        byte[] pixels = PngFixtures.Pixels(2, 3, 3);
        PngImage image = new(2, 3, PngColorKind.Rgb, pixels);

        Assert.Equal(pixels[0..6], image.Row(0).ToArray());
        Assert.Equal(pixels[6..12], image.Row(1).ToArray());
        Assert.Equal(pixels[12..18], image.Row(2).ToArray());
    }

    [Fact]
    public void ARowOutsideTheImageFails()
    {
        PngImage image = new(2, 3, PngColorKind.Rgb, PngFixtures.Pixels(2, 3, 3));

        Assert.Throws<ArgumentOutOfRangeException>(() => image.Row(3));
        Assert.Throws<ArgumentOutOfRangeException>(() => image.Row(-1));
    }

    [Fact]
    public void ACountOfBytesThatDoesNotMatchTheSizeFails()
    {
        ArgumentException fault = Assert.Throws<ArgumentException>(
            () => new PngImage(2, 3, PngColorKind.Rgba, new byte[18]));

        Assert.Contains("needs 24 bytes, and the call gave 18", fault.Message, StringComparison.Ordinal);
    }

    [Theory]
    [InlineData(0, 1)]
    [InlineData(1, 0)]
    [InlineData(PngImage.MaxSize + 1, 1)]
    [InlineData(1, PngImage.MaxSize + 1)]
    public void ASizeOutsideTheLimitsFails(int width, int height)
    {
        Assert.Throws<ArgumentOutOfRangeException>(
            () => new PngImage(width, height, PngColorKind.Rgb, new byte[3]));
    }

    [Fact]
    public void AColorKindThatIsNeitherOfTheTwoFails()
    {
        Assert.Throws<ArgumentOutOfRangeException>(() => PngImage.BytesPerPixelOf((PngColorKind)3));
    }

    /// <summary>The image copies the bytes, so a later change of the array reaches no image.</summary>
    [Fact]
    public void AChangeOfTheArrayAfterTheCallReachesNoImage()
    {
        byte[] pixels = PngFixtures.Pixels(1, 1, 3);
        PngImage image = new(1, 1, PngColorKind.Rgb, pixels);

        pixels[0] = (byte)(pixels[0] + 1);

        Assert.NotEqual(pixels[0], image.Pixels[0]);
    }
}
