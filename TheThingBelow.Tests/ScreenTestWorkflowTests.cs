using System;
using System.IO;
using Xunit;

namespace TheThingBelow.Tests;

/// <summary>
/// The `screen-test` job of CI pins Mesa, runs the Mobile renderer under Xvfb, compares two
/// runs, compares the captures with the committed baseline, and keeps the captures of every
/// run (D-172, D-729 to D-733).
/// </summary>
/// <remarks>
/// A trigger of this job cannot run on a branch, so these tests hold its contract in the
/// test job instead (D-500). The tests read the file as text, so each one names the line
/// that it expects (T-2).
/// </remarks>
public sealed class ScreenTestWorkflowTests
{
    private const string CiWorkflowPath = ".github/workflows/ci.yml";

    /// <summary>The Mesa version of D-730, which the job installs and reads back.</summary>
    private const string MesaVersion = "25.2.8-0ubuntu0.24.04.2";

    /// <summary>The snapshot timestamp of D-729 and D-730.</summary>
    private const string MesaSnapshot = "20260901T000000Z";

    [Fact]
    public void TheJobPinsTheMesaVersionOfTheDecision()
    {
        // D-730. The row of the register and the workflow name one version.
        Assert.Contains($"MESA_VERSION: \"{MesaVersion}\"", Workflow(), StringComparison.Ordinal);
    }

    [Fact]
    public void TheJobReadsMesaFromThePinnedSnapshot()
    {
        // D-729. The snapshot service keeps every timestamp, so the version never drops out
        // of the archive.
        string workflow = Workflow();

        Assert.Contains($"MESA_SNAPSHOT: \"{MesaSnapshot}\"", workflow, StringComparison.Ordinal);
        Assert.Contains("https://snapshot.ubuntu.com/ubuntu/$MESA_SNAPSHOT", workflow, StringComparison.Ordinal);
    }

    [Fact]
    public void TheJobReadsBackEachInstalledVersion()
    {
        // T-2. A dependency rule can pull another version, and the job never passes on it.
        string[] block = WorkflowText.RunBlockOf(CiWorkflowPath, "Install the pinned Mesa");
        string text = string.Join("\n", block);

        Assert.Contains("dpkg-query", text, StringComparison.Ordinal);
        Assert.Contains("mesa-vulkan-drivers", text, StringComparison.Ordinal);
        Assert.Contains("libgl1-mesa-dri", text, StringComparison.Ordinal);
        Assert.Contains("exit 1", text, StringComparison.Ordinal);
    }

    /// <summary>
    /// OQ-246, D-1127: the job installed the Vulkan loader and Xvfb at the newest version of
    /// any source, so a package of the picture could move between two jobs of one commit.
    /// </summary>
    [Fact]
    public void TheJobPinsAndReadsBackTheLoaderAndTheScreen()
    {
        string workflow = Workflow();
        string text = string.Join("\n", WorkflowText.RunBlockOf(CiWorkflowPath, "Install the pinned Mesa"));

        Assert.Contains("LIBVULKAN_VERSION: \"1.3.275.0-1build1\"", workflow, StringComparison.Ordinal);
        Assert.Contains("XVFB_VERSION: \"2:21.1.12-1ubuntu1.8\"", workflow, StringComparison.Ordinal);
        Assert.Contains("\"libvulkan1=$LIBVULKAN_VERSION\"", text, StringComparison.Ordinal);
        Assert.Contains("\"xvfb=$XVFB_VERSION\"", text, StringComparison.Ordinal);
        Assert.Contains("\"xserver-common=$XVFB_VERSION\"", text, StringComparison.Ordinal);
        Assert.Contains("the pin of D-1127", text, StringComparison.Ordinal);
    }

    /// <summary>OQ-246, D-1127: lavapipe compiles for the CPU of the runner, so the job logs it.</summary>
    [Fact]
    public void TheJobLogsTheCpuOfTheRunner()
    {
        string text = string.Join("\n", WorkflowText.RunBlockOf(CiWorkflowPath, "Log the CPU of the runner"));

        Assert.Contains("lscpu", text, StringComparison.Ordinal);
        Assert.Contains("avx512f", text, StringComparison.Ordinal);
        Assert.Contains("GITHUB_STEP_SUMMARY", text, StringComparison.Ordinal);
    }

    [Fact]
    public void TheJobFailsOnAFallbackToAnotherRenderer()
    {
        // D-731. Godot falls back to another driver when it cannot start the one of the
        // project, and another renderer draws another picture (T-2).
        string text = string.Join("\n", WorkflowText.RunBlockOf(CiWorkflowPath, "Take the captures two times"));

        Assert.Contains("the rendering method is mobile.", text, StringComparison.Ordinal);
        Assert.Contains("the rendering driver is vulkan.", text, StringComparison.Ordinal);
    }

    [Fact]
    public void TheJobFailsWhenTheMesaPinBringsNoLavapipeDriver()
    {
        // D-731, T-2. A driver file that no file holds gives a loader with no driver, and
        // Godot then falls back to OpenGL with no word of this job.
        string text = string.Join("\n", WorkflowText.RunBlockOf(CiWorkflowPath, "Install the pinned Mesa"));

        Assert.Contains("lvp_icd", text, StringComparison.Ordinal);
        Assert.Contains("VK_DRIVER_FILES=$icd", text, StringComparison.Ordinal);
    }

    [Fact]
    public void TheCaptureSessionTakesTheDummyAudioDriver()
    {
        // The runner has no sound card, and a session with a window opens the audio driver
        // of the system. Its failure writes error lines that have nothing to do with a
        // screen (D-172, T-2).
        string text = string.Join("\n", WorkflowText.RunBlockOf(CiWorkflowPath, "Take the captures two times"));

        Assert.Contains("--audio-driver Dummy", text, StringComparison.Ordinal);
    }

    [Fact]
    public void TheJobFailsOnAnErrorLineOfTheLog()
    {
        // Exit test 5 of PR-41. A shader that fails to compile writes its failure to the log
        // alone (T-2).
        string text = string.Join("\n", WorkflowText.RunBlockOf(CiWorkflowPath, "Take the captures two times"));

        Assert.Contains("^(ERROR|SCRIPT ERROR|USER ERROR)", text, StringComparison.Ordinal);
    }

    [Fact]
    public void EachSessionFailsWithNoLineOfTheCrashFixture()
    {
        // P3-26. The crash fixture throws inside a callback of the engine, and each session checks
        // what the reporter left. The capture session under Xvfb checks the message too, and the
        // smoke session of each leg checks the file and the log line (D-117, D-559).
        string take = string.Join("\n", WorkflowText.RunBlockOf(CiWorkflowPath, "Take the captures two times"));
        string smoke = string.Join("\n", WorkflowText.RunBlockOf(CiWorkflowPath, "Run the headless smoke session"));

        Assert.Contains("capture: the crash path is .* the message on screen", take, StringComparison.Ordinal);
        Assert.Contains("smoke: the crash path is ", smoke, StringComparison.Ordinal);
    }

    [Fact]
    public void TheJobTakesTheCapturesTwoTimesAndComparesThem()
    {
        // Exit test 4 of PR-41. Two runs give the same frames.
        string take = string.Join("\n", WorkflowText.RunBlockOf(CiWorkflowPath, "Take the captures two times"));
        string compare = string.Join("\n", WorkflowText.RunBlockOf(CiWorkflowPath, "Compare the two runs"));

        Assert.Contains("for run in 1 2;", take, StringComparison.Ordinal);
        Assert.Contains("--captures captures-2 --baseline captures-1", compare, StringComparison.Ordinal);
    }

    [Fact]
    public void TheJobComparesTheCapturesWithTheCommittedBaseline()
    {
        string text = string.Join(
            "\n", WorkflowText.RunBlockOf(CiWorkflowPath, "Compare the captures with the committed baseline"));

        Assert.Contains("--captures captures-1 --baseline screens/baseline", text, StringComparison.Ordinal);
    }

    [Fact]
    public void TheJobKeepsTheCapturesOfEveryRun()
    {
        // D-733. The author reads the artifact and commits the new baseline by hand.
        string workflow = Workflow();

        Assert.Contains("name: Keep the captures of this run", workflow, StringComparison.Ordinal);
        Assert.Contains("name: screen-captures", workflow, StringComparison.Ordinal);
    }

    [Fact]
    public void TheJobRunsOnTheLinuxLegAlone()
    {
        // D-172. One renderer and one driver give one baseline, so the job has no matrix.
        string[] lines = File.ReadAllLines(RepositoryRoot.PathTo(CiWorkflowPath));
        int start = Array.FindIndex(lines, line => line == "  screen-test:");
        Assert.True(start >= 0, $"The workflow holds no `screen-test` job (T-2).");

        int end = Array.FindIndex(lines, start + 1, line => line.Length > 2 && line[2] != ' ' && line.EndsWith(':'));
        string job = string.Join("\n", lines[start..(end < 0 ? lines.Length : end)]);

        Assert.Contains("runs-on: ubuntu-24.04", job, StringComparison.Ordinal);
        Assert.DoesNotContain("matrix", job, StringComparison.Ordinal);
    }

    private static string Workflow() => File.ReadAllText(RepositoryRoot.PathTo(CiWorkflowPath));
}
