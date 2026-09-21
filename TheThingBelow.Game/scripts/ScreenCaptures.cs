using System;
using System.Collections.Generic;
using TheThingBelow.Core.Maps;
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
/// <param name="Walk">The tick of the walk that this frame shows, or null for a fixture that runs no tick (D-782).</param>
public sealed record ScreenCapture(string Fixture, string Frame, int Width, int Height, FitMode Fit, WalkTick? Walk)
{
    /// <summary>The file name of this capture, under the captures folder and the baseline folder.</summary>
    public string FileName => $"{this.Fixture}-{this.Frame}.png";
}

/// <summary>One tick of the walk of the `walk` fixture (D-782).</summary>
/// <param name="Action">The step action of the step that this tick belongs to, such as `step_north`.</param>
/// <param name="Tick">The count of ticks of that step that ran, from 1 to <see cref="ScreenCaptures.TicksOfOneStep"/>.</param>
/// <remarks>
/// The step intent goes to tick 1 alone, and the step then runs by itself until the lead
/// arrives (D-203). The session writes the frame after the tick.
/// </remarks>
public sealed record WalkTick(string Action, int Tick);

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
    /// <summary>The running screen: the frame and the world, with no button prompt (D-734, D-815).</summary>
    public const string MapFixture = "map";

    /// <summary>The window frame and the longest string of the string table, with no world (D-734).</summary>
    public const string UiFixture = "ui";

    /// <summary>
    /// The running screen after each tick of one step north and one step south, which shows
    /// the slide inside a step (D-782). The other fixtures run no tick.
    /// </summary>
    public const string WalkFixture = "walk";

    /// <summary>The fixture large picture in the world viewport, with no map (D-819).</summary>
    public const string PictureFixture = "picture";

    /// <summary>The id of the large picture that the picture fixture draws (D-819).</summary>
    public const string FixturePicture = "picture.fixture_backdrop";

    /// <summary>
    /// The count of ticks from the intent of a step to the arrival of the lead. The intent
    /// starts the step on its tick, and the step then runs <see cref="MapRules.TicksPerStep"/>
    /// more ticks (D-203).
    /// </summary>
    public const int TicksOfOneStep = MapRules.TicksPerStep + 1;

    /// <summary>The width of a screen of 1080 rows, in device pixels.</summary>
    private const int DesktopWidth = 1920;

    /// <summary>The width of a screen of 1440 rows, in device pixels.</summary>
    private const int LargeWidth = 2560;

    /// <summary>The steps of the walk fixture, in the order that the session walks them (D-782).</summary>
    /// <remarks>
    /// A slide north and a slide south move the feet of the lead inside a row of tiles, and a
    /// slide east or west never does. The fault of PR-89 showed on those two alone.
    /// </remarks>
    public static IReadOnlyList<string> WalkSteps { get; } = [InputActions.StepNorth, InputActions.StepSouth];

    // This list stays above `All`, because the build of `All` reads it, and a static
    // property takes its value in the order of the file (T-2).
    /// <summary>Every capture that one session of the job takes, in a fixed order.</summary>
    public static IReadOnlyList<ScreenCapture> All { get; } = Build();

    /// <summary>The name of each fixture, in the order that the session draws it.</summary>
    public static IReadOnlyList<string> Fixtures { get; } = [MapFixture, UiFixture, WalkFixture, PictureFixture];

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
                fixture, "1x", ScreenFit.FrameWidth, ScreenFit.FrameHeight, FitMode.Fill, null));

            captures.Add(new ScreenCapture(fixture, "fill-1080", DesktopWidth, 1080, FitMode.Fill, null));
            captures.Add(new ScreenCapture(fixture, "whole-1080", DesktopWidth, 1080, FitMode.WholePixels, null));
            captures.Add(new ScreenCapture(fixture, "fill-1440", LargeWidth, 1440, FitMode.Fill, null));
            captures.Add(new ScreenCapture(fixture, "whole-1440", LargeWidth, 1440, FitMode.WholePixels, null));
        }

        // The walk draws at 1x alone. The world viewport holds every art pixel of the slide,
        // and a larger screen only scales that viewport (D-634, D-782).
        foreach (string step in WalkSteps)
        {
            for (int tick = 1; tick <= TicksOfOneStep; tick += 1)
            {
                captures.Add(new ScreenCapture(
                    WalkFixture,
                    $"{DirectionOf(step)}-{tick:D2}",
                    ScreenFit.FrameWidth,
                    ScreenFit.FrameHeight,
                    FitMode.Fill,
                    new WalkTick(step, tick)));
            }
        }


        // The picture draws at 1x alone. The map captures prove each fit of the world
        // viewport, and the picture draws inside that viewport (D-634, D-819).
        captures.Add(new ScreenCapture(
            PictureFixture, "1x", ScreenFit.FrameWidth, ScreenFit.FrameHeight, FitMode.Fill, null));
        return captures;
    }

    /// <summary>Gives every capture of one fixture, in the order of <see cref="All"/> (D-782).</summary>
    /// <param name="fixture">The name of the fixture, such as `walk`.</param>
    /// <returns>The captures of that fixture.</returns>
    /// <exception cref="ArgumentOutOfRangeException">No fixture has that name (T-2).</exception>
    /// <remarks>
    /// A session that looks for a visual fault on a small screen takes one fixture alone. A
    /// screen below 1080 rows cannot hold the larger captures of the map fixture (D-782).
    /// </remarks>
    public static IReadOnlyList<ScreenCapture> OfFixture(string fixture)
    {
        ArgumentException.ThrowIfNullOrEmpty(fixture);

        var found = new List<ScreenCapture>();
        foreach (ScreenCapture capture in All)
        {
            if (string.CompareOrdinal(capture.Fixture, fixture) == 0)
            {
                found.Add(capture);
            }
        }

        if (found.Count == 0)
        {
            throw new ArgumentOutOfRangeException(
                nameof(fixture),
                fixture,
                $"No capture has the fixture '{fixture}'. The fixtures are {string.Join(", ", Fixtures)} (T-2).");
        }

        return found;
    }

    /// <summary>Gives the direction word of a step action for a file name, such as `north`.</summary>
    /// <exception cref="ArgumentOutOfRangeException">The action is no step of the walk (T-2).</exception>
    private static string DirectionOf(string step)
    {
        const string prefix = "step_";
        if (!step.StartsWith(prefix, StringComparison.Ordinal))
        {
            throw new ArgumentOutOfRangeException(
                nameof(step), step, $"The walk fixture takes a step action, and '{step}' is none (T-2, D-782).");
        }

        return step[prefix.Length..];
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
