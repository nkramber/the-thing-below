using System;
using System.Collections.Generic;
using TheThingBelow.Game.Ui;

namespace TheThingBelow.Game;

/// <summary>
/// One frame that the screen-test job takes: a fixture, a screen size, and a fit (D-172,
/// D-732). The file name of the capture names the fixture and the frame.
/// </summary>
/// <param name="Fixture">The fixture that draws, either `map` or `ui` (D-734).</param>
/// <param name="Frame">The name of this frame of the fixture, such as `fill-1080`.</param>
/// <param name="Width">The width of the screen of this capture, in device pixels.</param>
/// <param name="Height">The height of the screen of this capture, in device pixels.</param>
/// <param name="Fit">The fit of the frame to that screen (D-232).</param>
public sealed record ScreenCapture(string Fixture, string Frame, int Width, int Height, FitMode Fit)
{
    /// <summary>The file name of this capture, under the captures folder and the baseline folder.</summary>
    public string FileName => $"{this.Fixture}-{this.Frame}.png";
}

/// <summary>
/// The list of captures that the screen-test job takes on every run (D-172, D-734). The
/// committed baseline of `screens/baseline/` holds one PNG for each entry (D-736).
/// </summary>
/// <remarks>
/// The list covers the frame at 1x, and both fit modes of D-232 at 1080 and 1440 screen
/// rows (D-568). At 1440 rows the frame reaches the screen at a whole scale of 2, so both
/// fits give the same picture, and the pair proves that rule of D-568.
/// <para>
/// This type holds no Godot value, so a test reads the list from the built Game assembly
/// with no engine (D-614). Every later screen of PR-62 and PR-63 adds its fixture here.
/// </para>
/// </remarks>
public static class ScreenCaptures
{
    /// <summary>The running screen: the frame, the world, and the row of button prompts (D-734).</summary>
    public const string MapFixture = "map";

    /// <summary>The window frame and the longest string of the string table, with no world (D-734).</summary>
    public const string UiFixture = "ui";

    /// <summary>The width of a screen of 1080 rows, in device pixels.</summary>
    private const int DesktopWidth = 1920;

    /// <summary>The width of a screen of 1440 rows, in device pixels.</summary>
    private const int LargeWidth = 2560;

    /// <summary>Every capture that one session of the job takes, in a fixed order.</summary>
    public static IReadOnlyList<ScreenCapture> All { get; } = Build();

    /// <summary>The name of each fixture, in the order that the session draws it.</summary>
    public static IReadOnlyList<string> Fixtures { get; } = [MapFixture, UiFixture];

    /// <summary>Gives the file name of every capture, in the order of <see cref="All"/>.</summary>
    /// <returns>One file name for each capture.</returns>
    public static IReadOnlyList<string> FileNames()
    {
        var names = new List<string>(All.Count);
        foreach (ScreenCapture capture in All)
        {
            names.Add(capture.FileName);
        }

        return names;
    }

    private static IReadOnlyList<ScreenCapture> Build()
    {
        var captures = new List<ScreenCapture>();
        foreach (string fixture in new[] { MapFixture, UiFixture })
        {
            // The frame at 1x, which the Steam Deck also shows inside its bars (D-568).
            captures.Add(new ScreenCapture(
                fixture, "1x", ScreenFit.FrameWidth, ScreenFit.FrameHeight, FitMode.Fill));

            captures.Add(new ScreenCapture(fixture, "fill-1080", DesktopWidth, 1080, FitMode.Fill));
            captures.Add(new ScreenCapture(fixture, "whole-1080", DesktopWidth, 1080, FitMode.WholePixels));
            captures.Add(new ScreenCapture(fixture, "fill-1440", LargeWidth, 1440, FitMode.Fill));
            captures.Add(new ScreenCapture(fixture, "whole-1440", LargeWidth, 1440, FitMode.WholePixels));
        }

        return captures;
    }

    /// <summary>Gives the capture of one file name.</summary>
    /// <param name="fileName">The file name, such as `map-fill-1080.png`.</param>
    /// <returns>The capture that writes that file.</returns>
    /// <exception cref="ArgumentOutOfRangeException">No capture writes that file (T-2).</exception>
    public static ScreenCapture Of(string fileName)
    {
        ArgumentException.ThrowIfNullOrEmpty(fileName);

        foreach (ScreenCapture capture in All)
        {
            if (string.CompareOrdinal(capture.FileName, fileName) == 0)
            {
                return capture;
            }
        }

        throw new ArgumentOutOfRangeException(
            nameof(fileName), fileName, $"No capture of the screen-test job writes '{fileName}' (T-2).");
    }
}
