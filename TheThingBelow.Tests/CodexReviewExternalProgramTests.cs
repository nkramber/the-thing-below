using System;
using System.Diagnostics;
using System.IO;
using TheThingBelow.Tools.CodexReview;
using Xunit;

namespace TheThingBelow.Tests;

/// <summary>The runner of the external programs of the `codex-review` command (T-2).</summary>
public sealed class CodexReviewExternalProgramTests
{
    [Fact]
    public void AProgramGivesItsOutputAndItsExitCode()
    {
        ProgramResult result = ExternalProgram.Run("git", ["--version"], Path.GetTempPath(), null, ExternalProgram.StepLimit);

        Assert.Equal(0, result.ExitCode);
        Assert.StartsWith("git version", result.Output, StringComparison.Ordinal);
    }

    [Fact]
    public void AnOutputFileTakesTheOutput()
    {
        string file = Path.Combine(Path.GetTempPath(), $"codex-review-output-{Guid.NewGuid():N}.txt");
        try
        {
            ProgramResult result = ExternalProgram.Run("git", ["--version"], Path.GetTempPath(), file, ExternalProgram.StepLimit);

            Assert.Equal(0, result.ExitCode);
            Assert.Equal(string.Empty, result.Output);
            Assert.StartsWith("git version", File.ReadAllText(file), StringComparison.Ordinal);
        }
        finally
        {
            File.Delete(file);
        }
    }

    /// <summary>
    /// A removed variable leaves the environment of the program alone. This process keeps it, so
    /// no other process, such as a run of another repository, loses a key (D-932).
    /// </summary>
    [Fact]
    public void ARemovedVariableLeavesTheProgramAlone()
    {
        string name = $"THE_THING_BELOW_TEST_KEY_{Guid.NewGuid():N}";
        Environment.SetEnvironmentVariable(name, "a-test-value");
        try
        {
            ProgramResult kept = ExternalProgram.Run("printenv", [name], Path.GetTempPath(), null, ExternalProgram.StepLimit);
            ProgramResult removed = ExternalProgram.Run("printenv", [name], Path.GetTempPath(), null, ExternalProgram.StepLimit, [name]);

            Assert.Equal("a-test-value", kept.Output.Trim());
            Assert.Equal(string.Empty, removed.Output.Trim());
            Assert.NotEqual(0, removed.ExitCode);
            Assert.Equal("a-test-value", Environment.GetEnvironmentVariable(name));
        }
        finally
        {
            Environment.SetEnvironmentVariable(name, null);
        }
    }

    [Fact]
    public void ACheckedRunThatFailsNamesTheCommandAndTheCode()
    {
        InvalidOperationException fault = Assert.Throws<InvalidOperationException>(
            () => ExternalProgram.RunChecked("git", ["no-such-command"], Path.GetTempPath()));

        Assert.Contains("`git no-such-command`", fault.Message, StringComparison.Ordinal);
        Assert.Contains("exit code 1", fault.Message, StringComparison.Ordinal);
    }

    [Fact]
    public void AProgramThatDoesNotExistIsAFault()
    {
        InvalidOperationException fault = Assert.Throws<InvalidOperationException>(
            () => ExternalProgram.Run("the-thing-below-no-such-program", [], Path.GetTempPath(), null, ExternalProgram.StepLimit));

        Assert.Contains("did not start", fault.Message, StringComparison.Ordinal);
    }

    /// <summary>
    /// The regression test of F-115. The old runner waited with no limit, so a program that
    /// hung stopped the review with no report. Each case stops within seconds of its limit.
    /// </summary>
    [Theory]
    [InlineData(false)]
    [InlineData(true)]
    public void AProgramPastItsLimitStopsWithAFaultThatNamesTheCommand(bool toFile)
    {
        string? file = toFile ? Path.Combine(Path.GetTempPath(), $"codex-review-limit-{Guid.NewGuid():N}.txt") : null;
        Stopwatch clock = Stopwatch.StartNew();
        try
        {
            InvalidOperationException fault = Assert.Throws<InvalidOperationException>(
                () => ExternalProgram.Run("sleep", ["30"], Path.GetTempPath(), file, TimeSpan.FromSeconds(1)));

            Assert.Contains("`sleep 30`", fault.Message, StringComparison.Ordinal);
            Assert.Contains("limit of 1 seconds", fault.Message, StringComparison.Ordinal);
            Assert.Contains("D-1087", fault.Message, StringComparison.Ordinal);
            Assert.True(clock.Elapsed < TimeSpan.FromSeconds(20), $"The stop took {clock.Elapsed}, and the limit is 1 second.");
        }
        finally
        {
            if (file is not null)
            {
                File.Delete(file);
            }
        }
    }

    /// <summary>
    /// A child of the program holds the output pipe open after the program itself hangs. The stop
    /// takes the whole tree. Windows loses the parent link of a child of Git Bash, so the child
    /// can run on there, and the bound on the read ends the wait. CI on Windows waited the whole
    /// 30 seconds of the child before that bound.
    /// </summary>
    [Fact]
    public void AStopAtTheLimitEndsEachChildOfTheProgram()
    {
        Stopwatch clock = Stopwatch.StartNew();

        InvalidOperationException fault = Assert.Throws<InvalidOperationException>(
            () => ExternalProgram.Run("sh", ["-c", "sleep 30 & wait"], Path.GetTempPath(), null, TimeSpan.FromSeconds(1)));

        Assert.Contains("each process that it started", fault.Message, StringComparison.Ordinal);
        Assert.True(clock.Elapsed < TimeSpan.FromSeconds(20), $"The stop took {clock.Elapsed}, and the limit is 1 second.");
    }

    /// <summary>
    /// The program ends at once, and a child that it left holds the output pipe. The old read
    /// waited as long as the child ran, and no limit applied after the end of the program.
    /// </summary>
    [Fact]
    public void AnOutputThatAChildHoldsOpenAfterTheEndIsAFault()
    {
        Stopwatch clock = Stopwatch.StartNew();

        InvalidOperationException fault = Assert.Throws<InvalidOperationException>(
            () => ExternalProgram.Run("sh", ["-c", "sleep 30 & exit 0"], Path.GetTempPath(), null, TimeSpan.FromSeconds(20)));

        Assert.Contains("stayed open", fault.Message, StringComparison.Ordinal);
        Assert.Contains("D-1087", fault.Message, StringComparison.Ordinal);
        Assert.True(clock.Elapsed < TimeSpan.FromSeconds(20), $"The read took {clock.Elapsed}, and the bound is {ExternalProgram.DrainLimit}.");
    }

    [Fact]
    public void AProgramInsideItsLimitGivesItsResult()
    {
        ProgramResult result = ExternalProgram.Run("sh", ["-c", "sleep 1; echo done"], Path.GetTempPath(), null, TimeSpan.FromSeconds(30));

        Assert.Equal(0, result.ExitCode);
        Assert.Equal("done", result.Output.Trim());
    }

    [Fact]
    public void ALimitOfZeroIsAnError()
    {
        Assert.Throws<ArgumentOutOfRangeException>(
            () => ExternalProgram.Run("git", ["--version"], Path.GetTempPath(), null, TimeSpan.Zero));
    }

    [Fact]
    public void EachLimitOfTheCommandIsTheValueOfD1087()
    {
        Assert.Equal(TimeSpan.FromMinutes(5), ExternalProgram.StepLimit);
        Assert.Equal(TimeSpan.FromSeconds(5), ExternalProgram.DrainLimit);
        Assert.Equal(TimeSpan.FromMinutes(10), CodexReviewCommand.InstallLimit);
        Assert.Equal(TimeSpan.FromMinutes(10), CodexReviewCommand.ProbeLimit);
        Assert.Equal(TimeSpan.FromMinutes(90), CodexReviewCommand.ReviewLimit);
    }
}
