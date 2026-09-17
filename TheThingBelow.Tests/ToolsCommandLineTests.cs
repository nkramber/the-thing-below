using System;
using System.IO;
using TheThingBelow.Tools;
using Xunit;

namespace TheThingBelow.Tests;

/// <summary>
/// The tools project holds no command yet, and each planned command names its PR (G-16).
/// Every run gives the fault code and writes the reason, and none of them is silent (T-2).
/// </summary>
public sealed class ToolsCommandLineTests
{
    [Fact]
    public void ARunWithNoCommandGivesTheFaultCodeAndNamesEveryPlannedCommand()
    {
        using StringWriter errors = new StringWriter();

        int exitCode = Program.Run([], errors);

        Assert.Equal(Program.FaultExitCode, exitCode);
        Assert.Contains("no command", errors.ToString(), StringComparison.Ordinal);
        Assert.Contains("ste-check: PR-2", errors.ToString(), StringComparison.Ordinal);
    }

    [Fact]
    public void APlannedCommandNamesThePullRequestThatAddsIt()
    {
        using StringWriter errors = new StringWriter();

        int exitCode = Program.Run(["det-lint"], errors);

        Assert.Equal(Program.FaultExitCode, exitCode);
        Assert.Contains("PR-46 adds it", errors.ToString(), StringComparison.Ordinal);
    }

    [Fact]
    public void AnUnknownCommandGivesTheFaultCodeAndNamesTheCommand()
    {
        using StringWriter errors = new StringWriter();

        int exitCode = Program.Run(["walk-the-dog"], errors);

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
}
