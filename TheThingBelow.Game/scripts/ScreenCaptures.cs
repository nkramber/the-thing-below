using System;
using System.Collections.Generic;
using TheThingBelow.Core.Effects;
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

/// <summary>One frame of the transition fixture: one look at one level of the flash and shake reduction (D-195, D-863).</summary>
/// <param name="Frame">The name of the frame, such as `color-split-reduced-1x`.</param>
/// <param name="Look">The look that the frame draws over the map.</param>
/// <param name="Level">The level of the reduction, which turns the color split into the fade (D-863).</param>
public sealed record TransitionFrame(string Frame, TransitionLook Look, EffectLevel Level);

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

    /// <summary>The running screen under a transition into a fight, halfway through it (D-195, exit tests 1 and 2 of PR-60).</summary>
    public const string TransitionFixture = "transition";

    /// <summary>The menu stack over the paused map: the main list, each task window beside it, and the dungeon map screen (D-211, D-567, D-982).</summary>
    public const string MenuFixture = "menu";

    /// <summary>The running screen with a notice at the top edge, inside its type-out and inside its hold (D-221, D-994).</summary>
    public const string NoticeFixture = "notice";

    /// <summary>The frame of the menu fixture with the main list alone.</summary>
    public const string MenuListFrame = "list-1x";

    /// <summary>The frame of the menu fixture with the main list at 1080 rows, which takes a body of 24 (D-707).</summary>
    public const string MenuListDesktopFrame = "list-fill-1080";

    /// <summary>The frame of the menu fixture with the party window over the main list (D-558).</summary>
    public const string MenuPartyFrame = "party-1x";

    /// <summary>The frame of the menu fixture with the status window over the main list (D-991).</summary>
    public const string MenuStatusFrame = "status-1x";

    /// <summary>The frame of the menu fixture with the notice log over the main list (D-987).</summary>
    public const string MenuLogFrame = "log-1x";

    /// <summary>The frame of the menu fixture with the lesson window over the main list: the aptitudes and the slots (D-1033).</summary>
    public const string MenuLessonsFrame = "lessons-1x";

    /// <summary>The frame of the menu fixture with the list of a swap, which works anywhere outside a fight (D-1050).</summary>
    public const string MenuLessonsSwapFrame = "lessons-swap-1x";

    /// <summary>The frame of the menu fixture with the gear window over the main list: the stats and the six slots, with each empty slot (D-44, exit test 2 of PR-13).</summary>
    public const string MenuGearFrame = "gear-1x";

    /// <summary>The frame of the menu fixture with the pieces of the pack that fit the first accessory slot (D-1048).</summary>
    public const string MenuGearPackFrame = "gear-pack-1x";

    /// <summary>The frame of the menu fixture with the item window over the main list: each item with its count and its limit (D-1039).</summary>
    public const string MenuItemsFrame = "items-1x";

    /// <summary>The gear slot that <see cref="MenuGearPackFrame"/> opens: the first accessory slot.</summary>
    public const int GearPackSlot = 4;

    /// <summary>The frame of the battle fixture with the lessons of the first character after the Lessons command (D-1031).</summary>
    public const string BattleLessonsFrame = "lessons-1x";

    /// <summary>The frame of the battle fixture with the forms of the cinder and the description of the first form (D-1027).</summary>
    public const string BattleFormsFrame = "forms-1x";

    /// <summary>The frame of the menu fixture with the dungeon map screen after the walk of <see cref="DungeonRoute"/> (D-982).</summary>
    public const string MenuMapFrame = "map-1x";

    /// <summary>The frame of the notice fixture inside the type-out of the line (D-709).</summary>
    public const string NoticeTypeFrame = "type-1x";

    /// <summary>The frame of the notice fixture inside the hold of the whole line (D-994).</summary>
    public const string NoticeHoldFrame = "hold-1x";

    /// <summary>The ticks after the post of the notice that the type-out frame shows: the slide of 12 ticks and 15 more (D-994).</summary>
    public const int NoticeTypeTicks = 27;

    /// <summary>The ticks after the post of the notice that the hold frame shows, past the slide and the type-out of the line (D-994).</summary>
    public const int NoticeHoldTicks = 90;

    /// <summary>The count of notices that the log frame posts, each one a notice that logs (D-983).</summary>
    public const int LogFrameNotices = 3;

    /// <summary>The tick of each transition that its frame shows, halfway through the 60 ticks of D-941.</summary>
    public const int TransitionTick = 30;

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

    /// <summary>The frame of the battle fixture that shows the first command of the fight of the deep room, whose grunt waits in the column (D-953, exit test 1 of PR-98).</summary>
    public const string BattleWaitingFrame = "waiting-1x";

    /// <summary>The frame of the battle fixture that stages a heavy blow inside its hit-stop, at the full level (D-877, D-880).</summary>
    public const string BattleStopFrame = "heavy-stop-1x";

    /// <summary>The frame of the battle fixture that shows the experience of the win above the head of the character (D-975).</summary>
    public const string BattleExperienceFrame = "experience-1x";

    /// <summary>The frame of the battle fixture that stages a level-up halfway: the lines rise, and the bars fill (D-975).</summary>
    public const string BattleLevelUpRiseFrame = "level-up-rise-1x";

    /// <summary>The frame of the battle fixture that stages a level-up after each line settled and the bars filled (D-975).</summary>
    public const string BattleLevelUpFrame = "level-up-1x";

    /// <summary>The ticks of the experience event that the experience frame shows: its line has settled (D-975).</summary>
    public const int ExperienceFrameTicks = 14;

    /// <summary>The ticks of the experience event at which the rise frame stages the level-up: three lines show, and the bars fill.</summary>
    public const int LevelUpRiseTicks = 20;

    /// <summary>The ticks of the experience event at which the settled frame stages the level-up: every line and each bar stand still.</summary>
    public const int LevelUpSettledTicks = 50;

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

    /// <summary>
    /// The steps of the map frame of the menu fixture: south into the save point at (9, 6), north,
    /// and east through the door at (11, 4), so the map shows a walked save point and a walked door
    /// (D-567, D-993).
    /// </summary>
    public static IReadOnlyList<string> DungeonRoute { get; } =
    [
        InputActions.StepSouth, InputActions.StepSouth,
        InputActions.StepEast, InputActions.StepEast, InputActions.StepEast, InputActions.StepEast, InputActions.StepEast,
        InputActions.StepNorth, InputActions.StepNorth,
        InputActions.StepEast, InputActions.StepEast, InputActions.StepEast,
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

    /// <summary>The frames of the flash of the cinder at each level of the flash and shake setting (D-863, D-1032).</summary>
    public static IReadOnlyList<LevelFrame> SpellFrames { get; } =
    [
        new("spell-full-1x", EffectLevel.Full),
        new("spell-reduced-1x", EffectLevel.Reduced),
        new("spell-off-1x", EffectLevel.Off),
    ];

    /// <summary>The ticks into the lesson event that each spell frame shows: the peak of the spike and the start of its fall (D-1032).</summary>
    public const int SpellFrameTicks = 3;

    /// <summary>The frames of the battle fixture that stage a heavy blow after its hit-stop, one for each level (D-863, D-877).</summary>
    public static IReadOnlyList<LevelFrame> HeavyFrames { get; } =
    [
        new("heavy-full-1x", EffectLevel.Full),
        new("heavy-reduced-1x", EffectLevel.Reduced),
        new("heavy-off-1x", EffectLevel.Off),
    ];

    /// <summary>
    /// The frames of the transition fixture: each of the ten looks at the full level, and the color
    /// split at the reduced level, where the fade takes its place (D-195, D-863, exit tests 1 and 2 of PR-60).
    /// </summary>
    public static IReadOnlyList<TransitionFrame> TransitionFrames { get; } = BuildTransitionFrames();

    // These lists stay above `All`, because the build of `All` reads them, and a static
    // property takes its value in the order of the file (T-2).
    /// <summary>Every capture that one session of the job takes, in a fixed order.</summary>
    public static IReadOnlyList<ScreenCapture> All { get; } = Build();

    /// <summary>The name of each fixture, in the order that the session draws it.</summary>
    public static IReadOnlyList<string> Fixtures { get; } =
        [MapFixture, UiFixture, WalkFixture, PictureFixture, BattleFixture, SettingsFixture, PitFixture, ScrollFixture, StillFixture, TransitionFixture, MenuFixture, NoticeFixture];

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

        // PR-12: the list of the lessons and the list of the forms with the description of the first form (D-1027, D-1031).
        foreach (string frame in new[] { BattleLessonsFrame, BattleFormsFrame })
        {
            captures.Add(new ScreenCapture(BattleFixture, frame, ScreenFit.FrameWidth, ScreenFit.FrameHeight, FitMode.Fill, null));
        }
        captures.Add(new ScreenCapture(
            BattleFixture, BattleBlowFrame, ScreenFit.FrameWidth, ScreenFit.FrameHeight, FitMode.Fill, null));
        captures.Add(new ScreenCapture(
            BattleFixture, BattleWaitingFrame, ScreenFit.FrameWidth, ScreenFit.FrameHeight, FitMode.Fill, null));

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

        // PR-12: the flash of the cinder at each level of the flash and shake setting (D-863, D-878, D-1032).
        foreach (LevelFrame spell in SpellFrames)
        {
            captures.Add(new ScreenCapture(BattleFixture, spell.Frame, ScreenFit.FrameWidth, ScreenFit.FrameHeight, FitMode.Fill, null));
        }

        // The summary after the win draws at 1x: the experience, and a staged level-up halfway
        // and settled (D-975, exit test 8 of PR-67).
        foreach (string frame in new[] { BattleExperienceFrame, BattleLevelUpRiseFrame, BattleLevelUpFrame })
        {
            captures.Add(new ScreenCapture(BattleFixture, frame, ScreenFit.FrameWidth, ScreenFit.FrameHeight, FitMode.Fill, null));
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

        // Each transition draws at 1x over the map, halfway through, and the color split draws at the
        // reduced level too, where the fade takes its place (D-195, D-863).
        foreach (TransitionFrame transition in TransitionFrames)
        {
            captures.Add(new ScreenCapture(
                TransitionFixture, transition.Frame, ScreenFit.FrameWidth, ScreenFit.FrameHeight, FitMode.Fill, null));
        }

        // The settings screen draws at both body sizes: 32 at 1x, and 24 at 1080 rows (D-707).
        // The conflict line draws at 1x, the floor of the Steam Deck (D-862).
        captures.Add(new ScreenCapture(
            SettingsFixture, "1x", ScreenFit.FrameWidth, ScreenFit.FrameHeight, FitMode.Fill, null));
        captures.Add(new ScreenCapture(SettingsFixture, "fill-1080", DesktopWidth, 1080, FitMode.Fill, null));
        captures.Add(new ScreenCapture(
            SettingsFixture, SettingsConflictFrame, ScreenFit.FrameWidth, ScreenFit.FrameHeight, FitMode.Fill, null));

        // PR-62: the menu stack at 1x, the floor of the Steam Deck, and the main list at 1080 rows,
        // which takes the smaller body (D-707). The notice draws inside its type-out and its hold.
        foreach (string frame in new[] { MenuListFrame, MenuPartyFrame, MenuStatusFrame, MenuLogFrame, MenuMapFrame, MenuLessonsFrame, MenuLessonsSwapFrame, MenuGearFrame, MenuGearPackFrame, MenuItemsFrame })
        {
            captures.Add(new ScreenCapture(MenuFixture, frame, ScreenFit.FrameWidth, ScreenFit.FrameHeight, FitMode.Fill, null));
        }

        captures.Add(new ScreenCapture(MenuFixture, MenuListDesktopFrame, DesktopWidth, 1080, FitMode.Fill, null));
        captures.Add(new ScreenCapture(NoticeFixture, NoticeTypeFrame, ScreenFit.FrameWidth, ScreenFit.FrameHeight, FitMode.Fill, null));
        captures.Add(new ScreenCapture(NoticeFixture, NoticeHoldFrame, ScreenFit.FrameWidth, ScreenFit.FrameHeight, FitMode.Fill, null));
        return captures;
    }

    /// <summary>Tells whether a frame of the battle fixture opens the lesson list of the command menu (D-1031).</summary>
    /// <param name="frame">The name of the frame.</param>
    /// <returns>True for the lessons frame and the forms frame.</returns>
    public static bool OpensLessons(string frame)
    {
        ArgumentException.ThrowIfNullOrEmpty(frame);

        return string.CompareOrdinal(frame, BattleLessonsFrame) == 0 || string.CompareOrdinal(frame, BattleFormsFrame) == 0;
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

    /// <summary>Gives the ticks of the experience event that a frame of the battle fixture shows (D-975).</summary>
    /// <param name="frame">The name of the frame, such as `experience-1x`.</param>
    /// <returns>The ticks, or no value for a frame that shows no summary.</returns>
    public static int? ExperienceTicksOf(string frame)
    {
        ArgumentException.ThrowIfNullOrEmpty(frame);

        if (string.CompareOrdinal(frame, BattleExperienceFrame) == 0)
        {
            return ExperienceFrameTicks;
        }

        if (string.CompareOrdinal(frame, BattleLevelUpRiseFrame) == 0)
        {
            return LevelUpRiseTicks;
        }

        return string.CompareOrdinal(frame, BattleLevelUpFrame) == 0 ? LevelUpSettledTicks : null;
    }

    /// <summary>
    /// Tells whether a frame of the battle fixture stages a level-up in place of the experience
    /// that plays. The fixture fight gives 12 experience, below the 20 of level 2 (D-977).
    /// </summary>
    /// <param name="frame">The name of the frame.</param>
    /// <returns>True for the two level-up frames.</returns>
    public static bool StagesLevelUp(string frame)
    {
        ArgumentException.ThrowIfNullOrEmpty(frame);

        return string.CompareOrdinal(frame, BattleLevelUpRiseFrame) == 0 || string.CompareOrdinal(frame, BattleLevelUpFrame) == 0;
    }

    /// <summary>Tells whether a frame of the battle fixture stages a heavy blow in place of the blow of the fight (D-877).</summary>
    /// <param name="frame">The name of the frame.</param>
    /// <returns>True for the stop frame and each heavy frame.</returns>
    public static bool StagesHeavyBlow(string frame)
    {
        ArgumentException.ThrowIfNullOrEmpty(frame);

        return string.CompareOrdinal(frame, BattleStopFrame) == 0 || HeavyLevelOf(frame) is not null;
    }

    /// <summary>Gives the frame of the transition fixture of one frame name.</summary>
    /// <param name="frame">The name of the frame, such as `shatter-1x`.</param>
    /// <returns>The frame.</returns>
    /// <exception cref="ArgumentOutOfRangeException">No frame of the transition fixture has the name (T-2).</exception>
    public static TransitionFrame TransitionFrameOf(string frame)
    {
        ArgumentException.ThrowIfNullOrEmpty(frame);

        foreach (TransitionFrame transition in TransitionFrames)
        {
            if (string.CompareOrdinal(transition.Frame, frame) == 0)
            {
                return transition;
            }
        }

        throw new ArgumentOutOfRangeException(nameof(frame), frame, $"No frame of the transition fixture has the name '{frame}' (T-2).");
    }

    private static IReadOnlyList<TransitionFrame> BuildTransitionFrames()
    {
        var frames = new List<TransitionFrame>(Transition.AllLooks.Length + 1);
        foreach (TransitionLook look in Transition.AllLooks)
        {
            frames.Add(new TransitionFrame($"{FrameNameOf(look)}-1x", look, EffectLevel.Full));
        }

        frames.Add(new TransitionFrame($"{FrameNameOf(TransitionLook.ColorSplit)}-reduced-1x", TransitionLook.ColorSplit, EffectLevel.Reduced));
        return frames;
    }

    private static string FrameNameOf(TransitionLook look) => Transition.NameOf(look).Replace('_', '-');

    /// <summary>Gives the level of the flash and shake reduction of a frame of the battle fixture (D-863).</summary>
    /// <param name="frame">The name of the frame.</param>
    /// <returns>The level of a heavy frame, and the default level, full, for every other frame (D-868).</returns>
    public static EffectLevel LevelOf(string frame) => HeavyLevelOf(frame) ?? SpellLevelOf(frame) ?? EffectLevel.Full;

    /// <summary>Tells whether a frame of the battle fixture shows the flash of a spell (D-1032).</summary>
    /// <param name="frame">The name of the frame.</param>
    /// <returns>True for each spell frame.</returns>
    public static bool StagesSpell(string frame) => SpellLevelOf(frame) is not null;

    private static EffectLevel? SpellLevelOf(string frame)
    {
        foreach (LevelFrame spell in SpellFrames)
        {
            if (string.CompareOrdinal(spell.Frame, frame) == 0)
            {
                return spell.Level;
            }
        }

        return null;
    }

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
