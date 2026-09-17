using System;
using System.IO;
using TheThingBelow.Tools;
using TheThingBelow.Tools.SteCheck;
using Xunit;

namespace TheThingBelow.Tests;

/// <summary>
/// The tools project holds the `ste-check` command, and each other command names the PR that
/// adds it (G-16). Every run gives the fault code and writes the reason (T-2).
/// </summary>
public sealed class ToolsCommandLineTests
{
    [Fact]
    public void ARunWithNoCommandGivesTheFaultCodeAndNamesEveryCommand()
    {
        using StringWriter output = new StringWriter();
        using StringWriter errors = new StringWriter();

        int exitCode = Program.Run([], output, errors);

        Assert.Equal(Program.FaultExitCode, exitCode);
        Assert.Contains("no command", errors.ToString(), StringComparison.Ordinal);
        Assert.Contains("ste-check: ready", errors.ToString(), StringComparison.Ordinal);
        Assert.Contains("review-gate: PR-3", errors.ToString(), StringComparison.Ordinal);
    }

    [Fact]
    public void APlannedCommandNamesThePullRequestThatAddsIt()
    {
        using StringWriter output = new StringWriter();
        using StringWriter errors = new StringWriter();

        int exitCode = Program.Run(["det-lint"], output, errors);

        Assert.Equal(Program.FaultExitCode, exitCode);
        Assert.Contains("PR-46 adds it", errors.ToString(), StringComparison.Ordinal);
    }

    [Fact]
    public void AnUnknownCommandGivesTheFaultCodeAndNamesTheCommand()
    {
        using StringWriter output = new StringWriter();
        using StringWriter errors = new StringWriter();

        int exitCode = Program.Run(["walk-the-dog"], output, errors);

        Assert.Equal(Program.FaultExitCode, exitCode);
        Assert.Contains("unknown command 'walk-the-dog'", errors.ToString(), StringComparison.Ordinal);
    }

    [Fact]
    public void EveryPlannedCommandNamesAPullRequest()
    {
        Assert.NotEmpty(Program.PlannedCommands);
        Assert.All(
            Program.PlannedCommands,
            entry => Assert.StartsWith("PR-", entry.Value, StringComparison.Ordinal));
    }

    [Fact]
    public void ThePlannedCommandsNoLongerHoldTheSteCheckCommand()
    {
        Assert.DoesNotContain(SteCheckCommand.Name, Program.PlannedCommands.Keys);
    }

    [Fact]
    public void AnUnknownOptionOfTheSteCheckCommandNamesTheOption()
    {
        using StringWriter output = new StringWriter();
        using StringWriter errors = new StringWriter();

        int exitCode = Program.Run(["ste-check", "--every-file"], output, errors);

        Assert.Equal(Program.FaultExitCode, exitCode);
        Assert.Contains("'--every-file' is unknown", errors.ToString(), StringComparison.Ordinal);
    }

    [Fact]
    public void TheRootOptionWithNoPathNamesTheOption()
    {
        using StringWriter output = new StringWriter();
        using StringWriter errors = new StringWriter();

        int exitCode = Program.Run(["ste-check", "--root"], output, errors);

        Assert.Equal(Program.FaultExitCode, exitCode);
        Assert.Contains("needs a path after it", errors.ToString(), StringComparison.Ordinal);
    }

    [Fact]
    public void ARootThatDoesNotExistNamesTheRoot()
    {
        using StringWriter output = new StringWriter();
        using StringWriter errors = new StringWriter();

        int exitCode = Program.Run(
            ["ste-check", "--root", Path.Combine(Path.GetTempPath(), "no-such-checkout-9f2a")],
            output,
            errors);

        Assert.Equal(Program.FaultExitCode, exitCode);
        Assert.Contains("does not exist", errors.ToString(), StringComparison.Ordinal);
    }
}
