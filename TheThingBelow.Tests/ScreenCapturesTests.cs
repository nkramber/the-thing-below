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

    /// <summary>Every file that one run of the capture session writes, in the order of the list.</summary>
    public static TheoryData<string> ExpectedNames { get; } = new TheoryData<string>
    {
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
    };

    [Fact]
    public void TheListHoldsFiveCapturesOfEachFixture()
    {
        // D-734. Two fixtures, and five captures of each one: the frame at 1x, and both fit
        // modes at 1080 and 1440 screen rows (D-232, D-568).
        Assert.Equal(10, FileNames().Count);
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
