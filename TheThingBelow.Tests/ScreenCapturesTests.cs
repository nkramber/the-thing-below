using System;
using System.Collections;
using System.Collections.Generic;
using System.IO;
using System.Reflection;
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

    /// <summary>The captures of the battle fixture (D-827, D-833).</summary>
    private static readonly string[] BattleNames =
    [
        "battle-menu-1x.png",
        "battle-menu-fill-1080.png",
        "battle-target-1x.png",
        "battle-blow-1x.png",
    ];

    // This property stays below `StillNames`, because its build reads that array, and a static
    // member takes its value in the order of the file (T-2).
    /// <summary>Every file that one run of the capture session writes, in the order of the list.</summary>
    public static TheoryData<string> ExpectedNames { get; } = BuildExpectedNames();

    [Fact]
    public void TheListHoldsFiveCapturesOfEachStillFixtureOneForEachTickOfTheWalkFourOfTheBattleAndThreeOfTheSettings()
    {
        // D-734. Two still fixtures, and five captures of each one: the frame at 1x, and both
        // fit modes at 1080 and 1440 screen rows (D-232, D-568). D-782 adds the walk: 17 ticks
        // of one step north and 17 of one step south (D-821). D-819 adds the picture at 1x.
        // D-827 adds the battle: the menu at both body sizes, the pointer, and a blow. PR-63
        // adds the settings screen at both body sizes, and its conflict line (D-862, D-871).
        Assert.Equal(10 + 34 + 1 + 4 + 3, FileNames().Count);
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
        foreach (string fixture in new[] { "map", "ui", "picture" })
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
            new HashSet<string> { "1280 by 720", "1920 by 1080", "2560 by 1440" },
            screens);
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
