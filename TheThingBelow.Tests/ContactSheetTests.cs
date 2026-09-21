using System;
using System.Collections.Generic;
using TheThingBelow.Tools.Png;
using TheThingBelow.Tools.Screens;
using Xunit;

namespace TheThingBelow.Tests;

/// <summary>
/// The contact sheet of the screen tests (D-172, D-735). It stacks each capture of one local
/// run, and it scales a wide capture down by a whole number alone.
/// </summary>
public sealed class ContactSheetTests
{
    [Theory]
    [InlineData(1280, 1)]
    [InlineData(1920, 2)]
    [InlineData(2560, 2)]
    [InlineData(5120, 4)]
    public void TheStepScalesACaptureToTheLargestWidth(int width, int step)
    {
        // The step is a whole number, so the sheet takes every n-th pixel and no new color
        // enters the picture (D-502, T-1).
        Assert.Equal(step, ContactSheet.StepOf(width));
        Assert.True(width / step <= ContactSheet.MaxCaptureWidth);
    }

    [Fact]
    public void AWidthOfZeroFails()
    {
        // T-2. An absent width is an error, and never a step of one.
        Assert.Throws<ArgumentOutOfRangeException>(() => ContactSheet.StepOf(0));
    }

    [Fact]
    public void TheSheetStacksEveryCapture()
    {
        // Two captures of 1280 by 720 give a sheet of 1280 pixels across, and two pictures
        // with one gap between them.
        PngImage first = Image(1280, 720, 10);
        PngImage second = Image(1280, 720, 20);

        PngImage sheet = ContactSheet.Build([first, second]);

        Assert.Equal(1280, sheet.Width);
        Assert.Equal((720 * 2) + ContactSheet.GapPixels, sheet.Height);
        Assert.Equal(10, sheet.Row(0)[0]);
        Assert.Equal(20, sheet.Row(720 + ContactSheet.GapPixels)[0]);
    }

    [Fact]
    public void TheGapShowsTheGroundOfTheSheet()
    {
        // The ground is lighter than black, so the black bars of a fit show on the sheet.
        PngImage sheet = ContactSheet.Build([Image(1280, 720, 0), Image(1280, 720, 0)]);

        Assert.Equal(ContactSheet.GroundLevel, sheet.Row(720)[0]);
    }

    [Fact]
    public void AWideCaptureScalesDownAndCenters()
    {
        // A capture of 2560 pixels scales to 1280, and a sheet of one capture is that wide.
        PngImage sheet = ContactSheet.Build([Image(2560, 1440, 30)]);

        Assert.Equal(1280, sheet.Width);
        Assert.Equal(720, sheet.Height);
        Assert.Equal(30, sheet.Row(0)[0]);
    }

    [Fact]
    public void AnEmptyListFails()
    {
        // T-2. A sheet of no capture is an error, and never a picture of one pixel.
        Assert.Throws<ArgumentException>(() => ContactSheet.Build(new List<PngImage>()));
    }

    private static PngImage Image(int width, int height, byte level)
    {
        var pixels = new byte[width * height * 4];
        for (int at = 0; at < pixels.Length; at += 4)
        {
            pixels[at] = level;
            pixels[at + 1] = level;
            pixels[at + 2] = level;
            pixels[at + 3] = 255;
        }

        return new PngImage(width, height, PngColorKind.Rgba, pixels);
    }
}
