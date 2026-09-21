using System;
using System.Collections.Generic;
using System.IO;
using System.Reflection;
using System.Text;
using TheThingBelow.Core.Content;
using TheThingBelow.Tools.Png;
using Xunit;

namespace TheThingBelow.Tests;

/// <summary>
/// The colors of a capture of the screen-test job (D-172, D-188). The window renders in linear
/// HDR 2D, and a capture must hold the sRGB colors that the screen shows. The tests read the
/// built Game assembly, because Tests takes no reference to Game (D-614).
/// </summary>
/// <remarks>
/// Before the fix of PR-55, each capture saved the linear values as bytes, so the shadow
/// `2b2836` of the palette read `06 05 09`, and every baseline and contact sheet showed the
/// game far darker than the screen.
/// </remarks>
public sealed class CaptureColorsTests
{
    private const string TypeName = "TheThingBelow.Game.CaptureColors";

    /// <summary>The 1x captures of the world, whose every pixel is an art pixel of the palette.</summary>
    public static TheoryData<string> WorldCaptures { get; } = BuildWorldCaptures();

    [Fact]
    public void EachPaletteColorComesBackFromItsLinearHalfFloat()
    {
        // The window holds each channel as a half float of linear light. The capture turns it
        // back into the byte of the palette, with no shift of one step.
        foreach (PaletteColor color in Palette().Colors)
        {
            foreach (int channel in new[] { color.Red, color.Green, color.Blue })
            {
                double light = channel / 255.0;
                double linear = light <= 0.04045 ? light / 12.92 : Math.Pow((light + 0.055) / 1.055, 2.4);

                byte back = ToSrgbByte((float)(Half)linear);

                Assert.True(back == channel, $"The channel {channel} of '{color.Key}' came back as {back}.");
            }
        }
    }

    [Fact]
    public void BlackAndFullLightKeepTheirBytes()
    {
        Assert.Equal(0, ToSrgbByte(0f));
        Assert.Equal(255, ToSrgbByte(1f));
        Assert.Equal(255, ToSrgbByte(2f));
    }

    [Fact]
    public void AnImageOfTheWrongLengthFails()
    {
        MethodInfo method = GameAssemblyFile.Type(TypeName).GetMethod("ToSrgb")!;

        TargetInvocationException thrown = Assert.Throws<TargetInvocationException>(
            () => method.Invoke(null, [new byte[5], 1]));

        Assert.IsType<ArgumentException>(thrown.InnerException);
    }

    [Theory]
    [MemberData(nameof(WorldCaptures))]
    public void EachPixelOfAWorldBaselineIsAColorOfThePalette(string fileName)
    {
        // D-107: every art pixel is a palette key. The world draws with the Nearest filter at
        // a whole number of 2x, so the 1x capture holds no blended pixel.
        var colors = new HashSet<(int, int, int)>();
        foreach (PaletteColor color in Palette().Colors)
        {
            colors.Add((color.Red, color.Green, color.Blue));
        }

        PngImage image = PngReader.ReadFile(Path.Combine(RepositoryRoot.Find(), "screens", "baseline", fileName));
        int step = image.BytesPerPixel;
        for (int start = 0; start < image.Pixels.Length; start += step)
        {
            (int, int, int) pixel = (image.Pixels[start], image.Pixels[start + 1], image.Pixels[start + 2]);
            if (!colors.Contains(pixel))
            {
                int index = start / step;
                Assert.Fail($"The pixel {index % image.Width},{index / image.Width} of '{fileName}' holds {pixel}, and no palette color is {pixel} (D-107).");
            }
        }
    }

    private static byte ToSrgbByte(float linear) =>
        (byte)GameAssemblyFile.Type(TypeName).GetMethod("ToSrgbByte")!.Invoke(null, [linear])!;

    private static Palette Palette() =>
        Core.Content.Palette.Read(
            File.ReadAllBytes(Path.Combine(RepositoryRoot.Find(), "content", "sprites", "palette.json")),
            Core.Content.Palette.Path);

    private static TheoryData<string> BuildWorldCaptures()
    {
        var names = new TheoryData<string> { "map-1x.png", "picture-1x.png" };
        foreach (string direction in new[] { "north", "south" })
        {
            for (int tick = 1; tick <= 17; tick += 1)
            {
                names.Add($"walk-{direction}-{tick:D2}.png");
            }
        }

        return names;
    }
}
