using System;
using System.IO;
using TheThingBelow.Tools;
using TheThingBelow.Tools.Identity;
using TheThingBelow.Tools.ReviewGate;
using TheThingBelow.Tools.SteCheck;
using Xunit;

namespace TheThingBelow.Tests;

/// <summary>
/// The tools project holds the `ste-check` command, and each other command names the PR that
/// adds it (G-16). Every run gives the fault code and writes the reason (T-2).
/// </summary>
public sealed class ToolsCommandLineTests
{
    /// <summary>
    /// Every command reads an empty option value at its parse, and never as a stack trace
    /// (T-2, F-83). The old code reached a path check that threw `ArgumentException`, which
    /// no catch filter names, so the process ended with a crash and no fault code.
    /// </summary>
    [Theory]
    [InlineData("ste-check", "--root")]
    [InlineData("det-lint", "--root")]
    [InlineData("det-lint", "--configuration")]
    [InlineData("replay-identity", "--root")]
    [InlineData("content-hash", "--root")]
    [InlineData("atlas", "--root")]
    [InlineData("atlas", "--sheets")]
    [InlineData("review-gate", "--pull-request")]
    [InlineData("review-gate", "--head-files")]
    public void AnEmptyOptionValueGivesTheFaultCodeAndNamesTheOption(string command, string option)
    {
        using StringWriter output = new StringWriter();
        using StringWriter errors = new StringWriter();

        int exitCode = Program.Run([command, option, string.Empty], output, errors);

        Assert.Equal(Program.FaultExitCode, exitCode);
        Assert.Contains(
            $"the value of the option {option} is empty",
            errors.ToString(),
            StringComparison.Ordinal);
    }

    /// <summary>A repeated option would take the last value in silence, so each command refuses one (T-2).</summary>
    [Theory]
    [InlineData("ste-check", "--root")]
    [InlineData("det-lint", "--root")]
    [InlineData("replay-identity", "--root")]
    [InlineData("content-hash", "--root")]
    [InlineData("atlas", "--root")]
    [InlineData("review-gate", "--pull-request")]
    public void ARepeatedOptionGivesTheFaultCodeAndNamesTheOption(string command, string option)
    {
        using StringWriter output = new StringWriter();
        using StringWriter errors = new StringWriter();

        int exitCode = Program.Run([command, option, ".", option, "."], output, errors);

        Assert.Equal(Program.FaultExitCode, exitCode);
        Assert.Contains($"the option {option} is on the command line two times", errors.ToString(), StringComparison.Ordinal);
    }

    [Fact]
    public void ARunWithNoCommandGivesTheFaultCodeAndNamesEveryCommand()
    {
        using StringWriter output = new StringWriter();
        using StringWriter errors = new StringWriter();

        int exitCode = Program.Run([], output, errors);

        Assert.Equal(Program.FaultExitCode, exitCode);
        Assert.Contains("no command", errors.ToString(), StringComparison.Ordinal);
        Assert.Contains("ste-check: ready", errors.ToString(), StringComparison.Ordinal);
        Assert.Contains("review-gate: ready", errors.ToString(), StringComparison.Ordinal);
        Assert.Contains("det-lint: ready", errors.ToString(), StringComparison.Ordinal);
        Assert.Contains("replay-identity: ready", errors.ToString(), StringComparison.Ordinal);
        Assert.Contains("content-hash: ready", errors.ToString(), StringComparison.Ordinal);
    }

    [Fact]
    public void APlannedCommandNamesThePullRequestThatAddsIt()
    {
        using StringWriter output = new StringWriter();
        using StringWriter errors = new StringWriter();

        int exitCode = Program.Run(["night-gate"], output, errors);

        Assert.Equal(Program.FaultExitCode, exitCode);
        Assert.Contains("PR-49 adds it", errors.ToString(), StringComparison.Ordinal);
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
    public void ThePlannedCommandsNoLongerHoldTheCommandsThatExist()
    {
        Assert.DoesNotContain(SteCheckCommand.Name, Program.PlannedCommands.Keys);
        Assert.DoesNotContain(ReviewGateCommand.Name, Program.PlannedCommands.Keys);
        Assert.DoesNotContain(ReplayIdentityCommand.Name, Program.PlannedCommands.Keys);
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
        Assert.Contains("needs a value after it", errors.ToString(), StringComparison.Ordinal);
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
