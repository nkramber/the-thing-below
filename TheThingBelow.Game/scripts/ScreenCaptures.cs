using System;
using System.Collections.Generic;
using TheThingBelow.Core.Light;
using TheThingBelow.Core.Maps;
using TheThingBelow.Game.Ui;
using TheThingBelow.Storage;

namespace TheThingBelow.Game;

/// <summary>
/// One frame that the screen-test job takes: a fixture, a screen size, and a fit (D-172,
/// D-732). The file name of the capture names the fixture and the frame.
/// </summary>
/// <param name="Fixture">The fixture that draws, such as `map`, `ui`, or `battle` (D-734).</param>
/// <param name="Frame">The name of this frame of the fixture, such as `fill-1080`.</param>
/// <param name="Width">The width of the screen of this capture, in device pixels.</param>
/// <param name="Height">The height of the screen of this capture, in device pixels.</param>
/// <param name="Fit">The fit of the frame to that screen (D-232).</param>
/// <param name="Walk">The tick of the walk that this frame shows, or null for a fixture that runs no tick (D-782).</param>
/// <param name="Ambient">The id of the ambient file that this frame loads, or null for the weather of the map (D-889).</param>
/// <param name="Mode">The mode of the passes of the HD-2D look that this frame shows, or null for the mode of the file (D-917).</param>
public sealed record ScreenCapture(string Fixture, string Frame, int Width, int Height, FitMode Fit, WalkTick? Walk, string? Ambient = null, PassMode? Mode = null)
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

/// <summary>One frame of a fixture that loads an ambient file of the screen test (D-889).</summary>
/// <param name="Frame">The name of the frame, such as `snow-1x`.</param>
/// <param name="Ambient">The id of the ambient file that the frame loads.</param>
public sealed record WeatherFrame(string Frame, string Ambient);

/// <summary>One frame of the battle fixture at one level of the flash and shake reduction (D-863).</summary>
/// <param name="Frame">The name of the frame, such as `heavy-reduced-1x`.</param>
/// <param name="Level">The level that the battle screen of the frame takes.</param>
public sealed record LevelFrame(string Frame, EffectLevel Level);

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
/// with no engine (D-614). Every later screen of PR-62 adds its fixture here, as the settings screen did (D-871).
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

    /// <summary>
    /// The battle screen of the fixture fight: the command menu, the pointer of a target, and
    /// a blow of a character with its flash and its number (D-172, D-827).
    /// </summary>
    public const string BattleFixture = "battle";

    /// <summary>The settings screen over the paused map (D-226, D-871).</summary>
    public const string SettingsFixture = "settings";

    /// <summary>The running screen with the party in the pit room, which shows the walls beside the doorway at (6, 12) (D-852).</summary>
    public const string PitFixture = "pit";

    /// <summary>The running screen with the party still, which shows the motion of the weather over 4 seconds (D-894).</summary>
    public const string StillFixture = "still";

    /// <summary>The running screen through a step that scrolls the view, which shows that each particle stays on the world (F-97).</summary>
    public const string ScrollFixture = "scroll";

    /// <summary>The frame of the map fixture and of the battle fixture in the stepped mode of the passes (D-917).</summary>
    public const string SteppedFrame = "stepped-1x";

    /// <summary>The frame of the settings screen with a binding conflict and its line (D-862).</summary>
    public const string SettingsConflictFrame = "conflict-1x";

    /// <summary>The frame of the battle fixture that shows the pointer on the first target (D-833).</summary>
    public const string BattleTargetFrame = "target-1x";

    /// <summary>The frame of the battle fixture that shows a blow of a character (D-96, D-213).</summary>
    public const string BattleBlowFrame = "blow-1x";

    /// <summary>
    /// The ticks after the blow that the blow frame shows: the flash is on, and the number
    /// rises (D-829).
    /// </summary>
    public const int TicksAfterBlow = 2;

    /// <summary>The frame of the battle fixture that shows the blood of a hit on the grunt (D-879, D-882).</summary>
    public const string BattleBloodFrame = "blood-1x";

    /// <summary>The frame of the battle fixture that shows the sparks of a hit on the brute of the deep room (D-879, D-882).</summary>
    public const string BattleSparksFrame = "sparks-1x";

    /// <summary>The frame of the battle fixture that stages a heavy blow inside its hit-stop, at the full level (D-877, D-880).</summary>
    public const string BattleStopFrame = "heavy-stop-1x";

    /// <summary>The ticks after the blow that the blood, the sparks, and each heavy frame show: the burst has spread (D-879).</summary>
    public const int BurstTicksAfterBlow = 8;

    /// <summary>The ticks after the blow that the stop frame shows: inside the hit-stop of the battle file (D-880).</summary>
    public const int StopTicksAfterBlow = 3;

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

    /// <summary>The steps from the spawn point to the pit room, in the order that the session walks them (D-852).</summary>
    /// <remarks>The party goes east to the corridor of column 6, then south through both doorways into the room below.</remarks>
    public static IReadOnlyList<string> PitRoute { get; } =
    [
        InputActions.StepEast, InputActions.StepEast,
        InputActions.StepSouth, InputActions.StepSouth, InputActions.StepSouth,
        InputActions.StepSouth, InputActions.StepSouth, InputActions.StepSouth,
        InputActions.StepSouth, InputActions.StepSouth, InputActions.StepSouth,
    ];

    /// <summary>The ticks of the still fixture that take a frame: one second apart (D-894).</summary>
    public static IReadOnlyList<int> StillTicks { get; } = [60, 120, 180, 240];

    /// <summary>The action of a frame that runs ticks and sends no intent, which the still fixture takes.</summary>
    public const string StillAction = "none";

    /// <summary>The ticks of the step of the scroll fixture that take a frame: the start, the middle, and the arrival (F-97).</summary>
    /// <remarks>
    /// The view follows the lead in the pit room, so the three frames hold three places of the
    /// view. A particle that rides the view stands at the same place of each frame, and a
    /// particle of the world moves with the tiles under it.
    /// </remarks>
    public static IReadOnlyList<int> ScrollTicks { get; } = [1, 9, TicksOfOneStep];

    /// <summary>The ambient file of each capture of a weather: one for each kind of D-187, which the screen test alone loads (D-889).</summary>
    /// <remarks>The dust of the map shows in every other capture of the map and of a fight, because the fixture dungeon ships with it (D-202).</remarks>
    public static IReadOnlyList<WeatherFrame> WeatherFrames { get; } =
    [
        new("snow-1x", "effect.snow_capture"),
        new("fog-1x", "effect.fog_capture"),
        new("fire-1x", "effect.fire_capture"),
    ];

    /// <summary>The frames of the battle fixture that stage a heavy blow after its hit-stop, one for each level (D-863, D-877).</summary>
    public static IReadOnlyList<LevelFrame> HeavyFrames { get; } =
    [
        new("heavy-full-1x", EffectLevel.Full),
        new("heavy-reduced-1x", EffectLevel.Reduced),
        new("heavy-off-1x", EffectLevel.Off),
    ];

    // These lists stay above `All`, because the build of `All` reads them, and a static
    // property takes its value in the order of the file (T-2).
    /// <summary>Every capture that one session of the job takes, in a fixed order.</summary>
    public static IReadOnlyList<ScreenCapture> All { get; } = Build();

    /// <summary>The name of each fixture, in the order that the session draws it.</summary>
    public static IReadOnlyList<string> Fixtures { get; } =
        [MapFixture, UiFixture, WalkFixture, PictureFixture, BattleFixture, SettingsFixture, PitFixture, ScrollFixture, StillFixture];

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

        // The menu draws at both body sizes: 32 at 1x, and 24 at 1080 rows (D-707). The
        // pointer and the blow draw at 1x, where every part of the screen shows its pixels.
        captures.Add(new ScreenCapture(
            BattleFixture, "menu-1x", ScreenFit.FrameWidth, ScreenFit.FrameHeight, FitMode.Fill, null));
        captures.Add(new ScreenCapture(BattleFixture, "menu-fill-1080", DesktopWidth, 1080, FitMode.Fill, null));
        captures.Add(new ScreenCapture(
            BattleFixture, BattleTargetFrame, ScreenFit.FrameWidth, ScreenFit.FrameHeight, FitMode.Fill, null));
        captures.Add(new ScreenCapture(
            BattleFixture, BattleBlowFrame, ScreenFit.FrameWidth, ScreenFit.FrameHeight, FitMode.Fill, null));

        // Each battle effect draws at 1x, and the heavy blow draws at each level of the flash
        // and shake reduction (D-214, D-863, exit test 1 of PR-57).
        foreach (string frame in new[] { BattleBloodFrame, BattleSparksFrame, BattleStopFrame })
        {
            captures.Add(new ScreenCapture(BattleFixture, frame, ScreenFit.FrameWidth, ScreenFit.FrameHeight, FitMode.Fill, null));
        }

        foreach (LevelFrame heavy in HeavyFrames)
        {
            captures.Add(new ScreenCapture(BattleFixture, heavy.Frame, ScreenFit.FrameWidth, ScreenFit.FrameHeight, FitMode.Fill, null));
        }

        // Each ambient kind draws on the map and over the backdrop of a fight, at 1x (D-187,
        // D-205, exit test 1 of PR-58). The dust of the fixture dungeon draws in every other
        // capture of the map and of a fight (D-889).
        foreach (WeatherFrame weather in WeatherFrames)
        {
            foreach (string fixture in new[] { MapFixture, BattleFixture })
            {
                captures.Add(new ScreenCapture(
                    fixture,
                    weather.Frame,
                    ScreenFit.FrameWidth,
                    ScreenFit.FrameHeight,
                    FitMode.Fill,
                    null,
                    weather.Ambient));
            }
        }

        // The map and a fight in the stepped mode of the tilt-shift blur, the vignette, and the
        // light shafts, which the owner compares with the smooth mode of every other frame (D-917).
        foreach (string fixture in new[] { MapFixture, BattleFixture })
        {
            captures.Add(new ScreenCapture(
                fixture, SteppedFrame, ScreenFit.FrameWidth, ScreenFit.FrameHeight, FitMode.Fill, null, Mode: PassMode.Stepped));
        }

        // The party stands still, and each frame reads the weather one second later (D-894).
        // A fault in the motion of a mote or of a flame then changes a baseline.
        foreach (int tick in StillTicks)
        {
            captures.Add(new ScreenCapture(
                StillFixture,
                $"{tick:D3}",
                ScreenFit.FrameWidth,
                ScreenFit.FrameHeight,
                FitMode.Fill,
                new WalkTick(StillAction, tick)));
        }

        // The pit room draws at 1x. The frame holds the doorway at (6, 12) and the walls beside
        // it, whose shape stops the light of a torch on the same wall (D-852).
        captures.Add(new ScreenCapture(
            PitFixture, "1x", ScreenFit.FrameWidth, ScreenFit.FrameHeight, FitMode.Fill, null));

        // The scroll fixture draws at 1x. Each frame holds one tick of a step south in the pit
        // room, where the view follows the lead (F-97).
        foreach (int tick in ScrollTicks)
        {
            captures.Add(new ScreenCapture(
                ScrollFixture,
                $"{tick:D2}",
                ScreenFit.FrameWidth,
                ScreenFit.FrameHeight,
                FitMode.Fill,
                new WalkTick(InputActions.StepSouth, tick)));
        }

        // The settings screen draws at both body sizes: 32 at 1x, and 24 at 1080 rows (D-707).
        // The conflict line draws at 1x, the floor of the Steam Deck (D-862).
        captures.Add(new ScreenCapture(
            SettingsFixture, "1x", ScreenFit.FrameWidth, ScreenFit.FrameHeight, FitMode.Fill, null));
        captures.Add(new ScreenCapture(SettingsFixture, "fill-1080", DesktopWidth, 1080, FitMode.Fill, null));
        captures.Add(new ScreenCapture(
            SettingsFixture, SettingsConflictFrame, ScreenFit.FrameWidth, ScreenFit.FrameHeight, FitMode.Fill, null));
        return captures;
    }

    /// <summary>Gives the ticks after the blow that a frame of the battle fixture shows (D-829).</summary>
    /// <param name="frame">The name of the frame, such as `blood-1x`.</param>
    /// <returns>The ticks, or no value for a frame that shows the command menu.</returns>
    public static int? TicksAfterBlowOf(string frame)
    {
        ArgumentException.ThrowIfNullOrEmpty(frame);

        if (string.CompareOrdinal(frame, BattleBlowFrame) == 0)
        {
            return TicksAfterBlow;
        }

        if (string.CompareOrdinal(frame, BattleStopFrame) == 0)
        {
            return StopTicksAfterBlow;
        }

        bool burst = string.CompareOrdinal(frame, BattleBloodFrame) == 0 || string.CompareOrdinal(frame, BattleSparksFrame) == 0;
        return burst || HeavyLevelOf(frame) is not null ? BurstTicksAfterBlow : null;
    }

    /// <summary>Tells whether a frame of the battle fixture stages a heavy blow in place of the blow of the fight (D-877).</summary>
    /// <param name="frame">The name of the frame.</param>
    /// <returns>True for the stop frame and each heavy frame.</returns>
    public static bool StagesHeavyBlow(string frame)
    {
        ArgumentException.ThrowIfNullOrEmpty(frame);

        return string.CompareOrdinal(frame, BattleStopFrame) == 0 || HeavyLevelOf(frame) is not null;
    }

    /// <summary>Gives the level of the flash and shake reduction of a frame of the battle fixture (D-863).</summary>
    /// <param name="frame">The name of the frame.</param>
    /// <returns>The level of a heavy frame, and the default level, full, for every other frame (D-868).</returns>
    public static EffectLevel LevelOf(string frame) => HeavyLevelOf(frame) ?? EffectLevel.Full;

    private static EffectLevel? HeavyLevelOf(string frame)
    {
        foreach (LevelFrame heavy in HeavyFrames)
        {
            if (string.CompareOrdinal(heavy.Frame, frame) == 0)
            {
                return heavy.Level;
            }
        }

        return null;
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
