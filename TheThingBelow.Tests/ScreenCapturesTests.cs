using System;
using System.Collections;
using System.Collections.Generic;
using System.IO;
using System.Reflection;
using TheThingBelow.Core;
using TheThingBelow.Core.Battles;
using TheThingBelow.Core.Content;
using TheThingBelow.Core.Effects;
using TheThingBelow.Core.Runs;
using TheThingBelow.Storage;
using TheThingBelow.Tools.Content;
using Xunit;

namespace TheThingBelow.Tests;

/// <summary>
/// The capture list of the screen-test job, and the committed baseline of `screens/baseline`
/// (D-172, D-734, D-736). The tests read the built Game assembly, because Tests takes no
/// reference to Game (D-614).
/// </summary>
/// <remarks>
/// The list covers the frame at 1x and both fit modes at 1080 and 1440 screen rows, for each
/// fixture (D-232, D-568). A capture with no baseline, and a baseline with no capture, each
/// fail the job, so these tests hold the same rule in the test job (T-2).
/// </remarks>
public sealed class ScreenCapturesTests
{
    private const string CapturesTypeName = "TheThingBelow.Game.ScreenCaptures";

    /// <summary>The folder of the committed baseline, under the root of the checkout (D-736).</summary>
    private const string BaselineFolder = "screens/baseline";

    /// <summary>The ten files of the map fixture and the ui fixture, in the order of the list.</summary>
    private static readonly string[] StillNames =
    [
        "map-1x.png",
        "map-fill-1080.png",
        "map-whole-1080.png",
        "map-fill-1440.png",
        "map-whole-1440.png",
        "ui-1x.png",
        "ui-fill-1080.png",
        "ui-whole-1080.png",
        "ui-fill-1440.png",
        "ui-whole-1440.png",
    ];

    /// <summary>The captures of the battle fixture (D-827, D-833), with the lists of the lessons of PR-12 (D-1027), each battle effect of PR-57 (D-863, D-879), and the summary of PR-67 (D-975).</summary>
    private static readonly string[] BattleNames =
    [
        "battle-menu-1x.png",
        "battle-menu-fill-1080.png",
        "battle-target-1x.png",
        "battle-lessons-1x.png",
        "battle-forms-1x.png",
        "battle-blow-1x.png",
        "battle-waiting-1x.png",
        "battle-blood-1x.png",
        "battle-sparks-1x.png",
        "battle-heavy-stop-1x.png",
        "battle-heavy-full-1x.png",
        "battle-heavy-reduced-1x.png",
        "battle-heavy-off-1x.png",
        "battle-spell-full-1x.png",
        "battle-spell-reduced-1x.png",
        "battle-spell-off-1x.png",
        "battle-experience-1x.png",
        "battle-level-up-rise-1x.png",
        "battle-level-up-1x.png",
    ];

    /// <summary>The captures of each ambient kind, and the pit room of the wall shape beside a doorway (D-852, D-889).</summary>
    private static readonly string[] WeatherNames =
    [
        "map-snow-1x.png",
        "battle-snow-1x.png",
        "map-fog-1x.png",
        "battle-fog-1x.png",
        "map-fire-1x.png",
        "battle-fire-1x.png",
        "pit-1x.png",
        "still-060.png",
        "still-120.png",
        "still-180.png",
        "still-240.png",
        "scroll-01.png",
        "scroll-09.png",
        "scroll-17.png",
    ];

    /// <summary>The captures of the stepped mode of the passes of the HD-2D look, on the map and in a fight (D-917).</summary>
    private static readonly string[] SteppedNames =
    [
        "map-stepped-1x.png",
        "battle-stepped-1x.png",
    ];

    /// <summary>The captures of the menu stack and of a notice (exit test 2 of PR-62, D-211, D-567, D-994).</summary>
    private static readonly string[] MenuNames =
    [
        "menu-list-1x.png",
        "menu-party-1x.png",
        "menu-status-1x.png",
        "menu-log-1x.png",
        "menu-map-1x.png",
        "menu-lessons-1x.png",
        "menu-lessons-swap-1x.png",
        "menu-gear-1x.png",
        "menu-gear-pack-1x.png",
        "menu-items-1x.png",
        "menu-list-fill-1080.png",
        "notice-type-1x.png",
        "notice-hold-1x.png",
    ];

    /// <summary>The captures of the ten transitions, and the color split at the reduced level (D-195, D-863).</summary>
    private static readonly string[] TransitionNames =
    [
        "transition-shatter-1x.png",
        "transition-swirl-1x.png",
        "transition-pixel-dissolve-1x.png",
        "transition-mosaic-1x.png",
        "transition-crt-power-off-1x.png",
        "transition-snow-whiteout-1x.png",
        "transition-blinds-1x.png",
        "transition-ripple-1x.png",
        "transition-scanline-sweep-1x.png",
        "transition-color-split-1x.png",
        "transition-color-split-reduced-1x.png",
    ];

    /// <summary>
    /// The captures at the body of 24 of each menu window, each part of a fight with text, the notice,
    /// and the conflict line, at 1080 rows (G-28, D-707, P3-26).
    /// </summary>
    private static readonly string[] SmallBodyNames =
    [
        "menu-party-fill-1080.png",
        "menu-status-fill-1080.png",
        "menu-log-fill-1080.png",
        "menu-map-fill-1080.png",
        "menu-lessons-fill-1080.png",
        "menu-lessons-swap-fill-1080.png",
        "menu-gear-fill-1080.png",
        "menu-gear-pack-fill-1080.png",
        "menu-items-fill-1080.png",
        "battle-target-fill-1080.png",
        "battle-lessons-fill-1080.png",
        "battle-forms-fill-1080.png",
        "battle-experience-fill-1080.png",
        "battle-level-up-fill-1080.png",
        "notice-hold-fill-1080.png",
        "settings-conflict-fill-1080.png",
    ];

    /// <summary>The captures at the screen of the Steam Deck, and the message of a crash (G-19, D-559, P3-26).</summary>
    private static readonly string[] DeckAndCrashNames =
    [
        "map-fill-800.png",
        "battle-menu-fill-800.png",
        "crash-1x.png",
    ];

    // This property stays below `StillNames`, because its build reads that array, and a static
    // member takes its value in the order of the file (T-2).
    /// <summary>Every file that one run of the capture session writes, in the order of the list.</summary>
    public static TheoryData<string> ExpectedNames { get; } = BuildExpectedNames();

    [Fact]
    public void TheListHoldsFiveCapturesOfEachStillFixtureOneForEachTickOfTheWalkNineteenOfTheBattleAndThreeOfTheSettings()
    {
        // D-734. Two still fixtures, and five captures of each one: the frame at 1x, and both
        // fit modes at 1080 and 1440 screen rows (D-232, D-568). D-782 adds the walk: 17 ticks
        // of one step north and 17 of one step south (D-821). D-819 adds the picture at 1x.
        // D-827 adds the battle: the menu at both body sizes, the pointer, and a blow. PR-63
        // adds the settings screen at both body sizes, and its conflict line (D-862, D-871).
        // PR-57 adds the blood, the sparks, the stop of a heavy blow, and the heavy blow at each
        // level of the flash and shake reduction (D-863, exit test 1 of PR-57).
        // PR-58 adds one capture of each ambient kind on the map and over a fight, the pit room
        // of the wall shape beside a doorway, and three frames of a step that scrolls the view
        // (D-852, D-889, F-97, exit test 1 of PR-58). PR-92 adds the map and a fight in the
        // stepped mode of the passes (D-917). PR-60 adds each of the ten transitions, and the color
        // split at the reduced level (D-195, D-863, exit tests 1 and 2 of PR-60). PR-98 adds the
        // fight of the deep room with its waiting column (D-953, exit test 1 of PR-98). PR-67 adds the
        // experience after a win, and a staged level-up halfway and settled (D-975, exit test 8 of PR-67).
        // PR-62 adds the main list, each task window, and the dungeon map screen at 1x, the main list at
        // 1080 rows, and a notice inside its type-out and its hold (exit test 2 of PR-62).
        // PR-12 adds the list of the lessons and the list of the forms in a fight, and the lesson
        // window with its slots and its swap list, and the flash of a spell at each level (D-1027,
        // D-1030, D-1031, D-1032).
        // PR-13 adds the gear window, its pack list, and the item window to the menu names (D-44, D-1048).
        // PR-91 adds the pit room with an enemy inside the fade of the dark (D-1062, exit test 8 of PR-91).
        // PR-106 adds each window and each part of a fight at the body of 24, the screen of the
        // Steam Deck, and the message of a crash (G-19, G-28, P3-26).
        Assert.Equal(10 + 34 + 1 + 19 + 3 + 14 + 2 + 11 + 1 + MenuNames.Length + SmallBodyNames.Length + DeckAndCrashNames.Length, FileNames().Count);
    }

    [Fact]
    public void TheWalkTakesEveryTickOfEachStepInOrder()
    {
        // D-782. The frames inside a step are the regression test of F-95, so no tick of a
        // step north or south may drop out of the list.
        Assert.Equal(WalkNames(), NamesOf(OfFixture("walk")));
    }

    [Fact]
    public void EachStepOfTheWalkTakesItsIntentAndItsTick()
    {
        // D-782. The session queues the intent of the step on tick 1 alone, and it writes the
        // frame after each tick up to the arrival of the lead.
        int index = 0;
        foreach (object capture in OfFixture("walk"))
        {
            object walk = Read<object>(capture, "Walk");
            string action = index < 17 ? "step_north" : "step_south";

            Assert.Equal(action, Read<string>(walk, "Action"));
            Assert.Equal((index % 17) + 1, Read<int>(walk, "Tick"));
            Assert.Equal(1280, Read<int>(capture, "Width"));
            Assert.Equal(720, Read<int>(capture, "Height"));
            index += 1;
        }

        Assert.Equal(34, index);
    }

    [Fact]
    public void AStillFixtureRunsNoTick()
    {
        // D-782, T-7. The map, ui, and picture captures show the run at tick 0, so no walk tick
        // reaches them.
        foreach (string fixture in new[] { "map", "ui", "picture", "pit", "transition" })
        {
            foreach (object capture in OfFixture(fixture))
            {
                Assert.Null(capture.GetType().GetProperty("Walk")!.GetValue(capture));
            }
        }
    }

    [Fact]
    public void AFixtureOutsideTheListFails()
    {
        // T-2. An unknown fixture is an error, and never every capture.
        MethodInfo method = GameAssemblyFile.Type(CapturesTypeName).GetMethod("OfFixture")!;
        TargetInvocationException thrown = Assert.Throws<TargetInvocationException>(
            () => method.Invoke(null, ["none"]));

        Assert.IsType<ArgumentOutOfRangeException>(thrown.InnerException);
        Assert.Contains("walk", thrown.InnerException!.Message, StringComparison.Ordinal);
    }

    [Theory]
    [MemberData(nameof(ExpectedNames))]
    public void TheListHoldsThisCapture(string fileName)
    {
        Assert.Contains(fileName, FileNames());
    }

    [Theory]
    [MemberData(nameof(ExpectedNames))]
    public void TheBaselineHoldsThisCapture(string fileName)
    {
        // D-733. The job compares each capture with the file of the same name, and a capture
        // with no baseline fails it.
        string path = Path.Combine(RepositoryRoot.Find(), BaselineFolder, fileName);

        Assert.True(File.Exists(path), $"The baseline folder holds no '{fileName}' (D-733, D-736, T-2).");
    }

    [Fact]
    public void TheBaselineHoldsNoOtherPicture()
    {
        // A baseline that no capture writes is a leftover of a list change, and the job fails
        // on it. This test names it in the test job instead (T-2).
        List<string> names = FileNames();
        foreach (string path in Directory.GetFiles(Path.Combine(RepositoryRoot.Find(), BaselineFolder), "*.png"))
        {
            string name = Path.GetFileName(path);
            Assert.True(
                names.Contains(name),
                $"The baseline holds '{name}', and the capture list of Game writes no such file (D-736, T-2).");
        }
    }

    [Fact]
    public void EveryCaptureTakesAScreenOfTheRoadmap()
    {
        // D-232, D-568. The captures cover the frame at 1x, and the two screens of the exit
        // test of PR-41, which hold 1080 rows and 1440 rows.
        var screens = new HashSet<string>();
        foreach (object capture in Captures())
        {
            screens.Add($"{Read<int>(capture, "Width")} by {Read<int>(capture, "Height")}");
        }

        Assert.Equal(
            new HashSet<string> { "1280 by 720", "1280 by 800", "1920 by 1080", "2560 by 1440" },
            screens);
    }

    [Fact]
    public void TheScreenOfTheSteamDeckShowsAMapAndAFight()
    {
        // G-19, D-568: the Steam Deck is the floor of readability, and its screen of 1280 by 800
        // shows the frame at 1x between two bars (P3-26).
        var deck = new List<string>();
        foreach (object capture in Captures())
        {
            if (Read<int>(capture, "Width") == 1280 && Read<int>(capture, "Height") == 800)
            {
                deck.Add(Read<string>(capture, "FileName"));
            }
        }

        Assert.Equal(["map-fill-800.png", "battle-menu-fill-800.png"], deck);
    }

    [Fact]
    public void EachFrameOfTheSmallBodyShowsTheMomentOfItsFrameAt1x()
    {
        // G-28, D-707: a frame at 1080 rows takes the body of 24, and it shows the same moment as
        // its frame at 1x, which takes the body of 32 (P3-26).
        MethodInfo momentOf = GameAssemblyFile.Type(CapturesTypeName).GetMethod("MomentOf")!;
        List<string> names = FileNames();
        foreach (string name in SmallBodyNames)
        {
            string frame = name[(name.IndexOf('-', StringComparison.Ordinal) + 1)..^".png".Length];
            string moment = (string)momentOf.Invoke(null, [frame])!;
            string fixture = name[..name.IndexOf('-', StringComparison.Ordinal)];

            Assert.Equal(frame.Replace("-fill-1080", "-1x", StringComparison.Ordinal), moment);
            Assert.Contains($"{fixture}-{moment}.png", names);
        }

        Assert.Equal("fill-1080", (string)momentOf.Invoke(null, ["fill-1080"])!);
        Assert.Equal("blow-1x", (string)momentOf.Invoke(null, ["blow-1x"])!);
    }

    [Fact]
    public void TheCaptureOfTheCrashMessageComesLast()
    {
        // D-559, P3-26: the message of a crash stands until the session ends, so no capture may
        // follow it, and it draws at 1x, the floor of the Steam Deck.
        List<object> all = [.. Captures()];
        object last = all[^1];

        Assert.Equal("crash-1x.png", Read<string>(last, "FileName"));
        Assert.Equal(1280, Read<int>(last, "Width"));
        Assert.Equal(720, Read<int>(last, "Height"));
        Assert.Single(OfFixture("crash"));
    }

    [Theory]
    [InlineData("heavy-full-1x", EffectLevel.Full)]
    [InlineData("heavy-reduced-1x", EffectLevel.Reduced)]
    [InlineData("heavy-off-1x", EffectLevel.Off)]
    [InlineData("heavy-stop-1x", EffectLevel.Full)]
    [InlineData("blood-1x", EffectLevel.Full)]
    public void EachHeavyFrameTakesItsLevelAndStagesAHeavyBlow(string frame, EffectLevel level)
    {
        // Exit test 1 of PR-57: the heavy blow draws at each level of the flash and shake
        // reduction (D-214, D-863). A frame outside the heavy frames takes the default, full (D-868).
        Type captures = GameAssemblyFile.Type(CapturesTypeName);
        Assert.Equal(level, (EffectLevel)captures.GetMethod("LevelOf")!.Invoke(null, [frame])!);

        bool heavy = frame.StartsWith("heavy-", StringComparison.Ordinal);
        Assert.Equal(heavy, (bool)captures.GetMethod("StagesHeavyBlow")!.Invoke(null, [frame])!);
    }

    [Theory]
    [InlineData("menu-1x", null)]
    [InlineData("target-1x", null)]
    [InlineData("waiting-1x", null)]
    [InlineData("blow-1x", 2)]
    [InlineData("heavy-stop-1x", 3)]
    [InlineData("blood-1x", 8)]
    [InlineData("sparks-1x", 8)]
    [InlineData("heavy-off-1x", 8)]
    public void EachBattleFrameShowsItsTicksAfterTheBlow(string frame, int? ticks)
    {
        // D-829, D-880: the stop frame falls inside the hit-stop of 6 ticks, and the burst
        // frames show a spread burst. A frame of the menu plays no blow.
        object? found = GameAssemblyFile.Type(CapturesTypeName).GetMethod("TicksAfterBlowOf")!.Invoke(null, [frame]);

        Assert.Equal(ticks, (int?)found);
    }

    [Fact]
    public void EachTransitionFrameDrawsItsLookAndTheColorSplitDrawsAtBothLevels()
    {
        // Exit tests 1 and 2 of PR-60: the screen test captures each of the ten looks, and the color
        // split at the full level and at the reduced level, where the fade takes its place (D-863).
        MethodInfo frameOf = GameAssemblyFile.Type(CapturesTypeName).GetMethod("TransitionFrameOf")!;
        var looks = new SortedSet<string>(StringComparer.Ordinal);
        var splitLevels = new List<EffectLevel>();
        foreach (object capture in OfFixture("transition"))
        {
            object frame = frameOf.Invoke(null, [Read<string>(capture, "Frame")])!;
            var look = Read<TransitionLook>(frame, "Look");
            var level = Read<EffectLevel>(frame, "Level");
            looks.Add(Transition.NameOf(look));
            if (look == TransitionLook.ColorSplit)
            {
                splitLevels.Add(level);
            }
            else
            {
                Assert.Equal(EffectLevel.Full, level);
            }
        }

        Assert.Equal(Transition.AllLooks.Length, looks.Count);
        Assert.Equal([EffectLevel.Full, EffectLevel.Reduced], splitLevels);
        Assert.Equal(30, (int)GameAssemblyFile.Type(CapturesTypeName).GetField("TransitionTick")!.GetValue(null)!);
    }

    [Fact]
    public void TheWalkToTheDeepRoomMeetsTheBrute()
    {
        // D-882: the sparks frame needs a real fight with the brute, so the walk must meet the
        // elite patrol of the deep room at the fixture seed.
        object run = GameAssemblyFile.Type("TheThingBelow.Game.GameRun")
            .GetMethod("Start", [typeof(ContentSet), typeof(ulong), typeof(DebugIntentHandlers), typeof(MessageSpeed)])!
            .Invoke(null, [ContentSet.Load(ContentFolder.Read(RepositoryRoot.Find())), 20260918UL, DebugIntentHandlers.None, MessageSpeed.Normal])!;

        GameAssemblyFile.Type("TheThingBelow.Game.BattleWalk").GetMethod("ToFirstCommandOfElite")!.Invoke(null, [run]);

        var state = (RunState)run.GetType().GetProperty("State")!.GetValue(run)!;
        Battle battle = state.Battle ?? throw new InvalidOperationException("The walk ended with no battle (T-2).");
        Assert.Equal("group.fixture_elite", battle.Group.Id.Value);
        Assert.Contains(battle.Enemies, enemy => string.Equals(enemy.Id.Value, "enemy.fixture_brute", StringComparison.Ordinal));
    }

    [Fact]
    public void AFileNameOutsideTheListFails()
    {
        // T-2. An unknown name is an error, and never the first capture of the list.
        MethodInfo method = GameAssemblyFile.Type(CapturesTypeName).GetMethod("Of")!;
        TargetInvocationException thrown = Assert.Throws<TargetInvocationException>(
            () => method.Invoke(null, ["map-none.png"]));

        Assert.IsType<ArgumentOutOfRangeException>(thrown.InnerException);
    }

    private static List<string> FileNames()
    {
        object names = GameAssemblyFile.Type(CapturesTypeName).GetMethod("FileNames")!.Invoke(null, null)!;
        var read = new List<string>();
        foreach (object? name in (IEnumerable)names)
        {
            read.Add((string)name!);
        }

        return read;
    }

    private static TheoryData<string> BuildExpectedNames()
    {
        // The walk names come from a loop, so each of the 34 walk baselines has its own case,
        // and a baseline that drops out names its file (D-782, T-2).
        var names = new TheoryData<string>();
        foreach (string name in StillNames)
        {
            names.Add(name);
        }

        foreach (string name in WalkNames())
        {
            names.Add(name);
        }

        names.Add("picture-1x.png");
        foreach (string name in BattleNames)
        {
            names.Add(name);
        }

        foreach (string name in WeatherNames)
        {
            names.Add(name);
        }

        foreach (string name in SteppedNames)
        {
            names.Add(name);
        }

        foreach (string name in TransitionNames)
        {
            names.Add(name);
        }

        foreach (string name in MenuNames)
        {
            names.Add(name);
        }

        foreach (string name in SmallBodyNames)
        {
            names.Add(name);
        }

        foreach (string name in DeckAndCrashNames)
        {
            names.Add(name);
        }

        return names;
    }

    /// <summary>The 34 files of the walk fixture: 17 ticks of one step north, then 17 of one step south.</summary>
    private static List<string> WalkNames()
    {
        var names = new List<string>();
        foreach (string direction in new[] { "north", "south" })
        {
            for (int tick = 1; tick <= 17; tick += 1)
            {
                names.Add($"walk-{direction}-{tick:D2}.png");
            }
        }

        return names;
    }

    private static IEnumerable<object> OfFixture(string fixture)
    {
        object found = GameAssemblyFile.Type(CapturesTypeName).GetMethod("OfFixture")!.Invoke(null, [fixture])!;
        foreach (object? capture in (IEnumerable)found)
        {
            yield return capture!;
        }
    }

    private static List<string> NamesOf(IEnumerable<object> captures)
    {
        var read = new List<string>();
        foreach (object capture in captures)
        {
            read.Add(Read<string>(capture, "FileName"));
        }

        return read;
    }

    private static IEnumerable<object> Captures()
    {
        object all = GameAssemblyFile.Type(CapturesTypeName).GetProperty("All")!.GetValue(null)!;
        foreach (object? capture in (IEnumerable)all)
        {
            yield return capture!;
        }
    }

    private static T Read<T>(object capture, string name) =>
        (T)capture.GetType().GetProperty(name)!.GetValue(capture)!;
}
