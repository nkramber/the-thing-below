using System;
using System.IO;
using TheThingBelow.Tools;
using TheThingBelow.Tools.Night;
using Xunit;

namespace TheThingBelow.Tests;

/// <summary>
/// The `night-gate` command on fixture PRs and fixture nights (G-22, D-500). Each test names the
/// exit test of section 7.47 of the phase file that it proves, where one applies.
/// </summary>
public sealed class NightGateCommandTests
{
    private const string CodePath = "TheThingBelow.Core/Simulation.cs";

    /// <summary>Exit test 1: a night with every leg green inside 48 hours passes.</summary>
    [Fact]
    public void AGreenNightOnMainInside48HoursPasses()
    {
        using NightGateFixture fixture = NightGateFixture.Create(CodePath);
        NightGateFixture.WriteGreenNight(fixture.MainNight, NightGateFixture.MainCommit, NightGate.MainBranch, NightGateFixture.LastNight);

        int exitCode = fixture.Run(out string output, out string errors);

        Assert.True(exitCode == 0, errors);
        Assert.Contains($"The night run 101 on the commit {NightGateFixture.MainCommit} of `main` started at 2026-09-28T04:17:43Z and succeeded on each leg", output, StringComparison.Ordinal);
        Assert.Contains("The night gate passes.", output, StringComparison.Ordinal);
    }

    /// <summary>Exit test 2: no night fails the gate as absent.</summary>
    [Fact]
    public void NoNightFailsAsAbsent()
    {
        using NightGateFixture fixture = NightGateFixture.Create(CodePath);

        int exitCode = fixture.Run(out _, out string errors);

        Assert.Equal(Program.FaultExitCode, exitCode);
        Assert.Contains($"absent: no completed night exists on `main` or on the head commit {NightGateFixture.Head}", errors, StringComparison.Ordinal);
    }

    /// <summary>Exit test 2: a night older than 48 hours fails the gate as stale, with its commit and its time.</summary>
    [Fact]
    public void ANightOlderThan48HoursFailsAsStale()
    {
        using NightGateFixture fixture = NightGateFixture.Create(CodePath);
        DateTimeOffset started = NightGateFixture.Now - TimeSpan.FromHours(49);
        NightGateFixture.WriteGreenNight(fixture.MainNight, NightGateFixture.MainCommit, NightGate.MainBranch, started);

        int exitCode = fixture.Run(out _, out string errors);

        Assert.Equal(Program.FaultExitCode, exitCode);
        Assert.Contains($"stale: the night run 101 on the commit {NightGateFixture.MainCommit}, started at 2026-09-26T08:00:00Z, is 49 hours old", errors, StringComparison.Ordinal);
    }

    /// <summary>The limit holds at exactly 48 hours, and one second more fails (G-22).</summary>
    [Fact]
    public void ANightOfExactly48HoursPassesAndOneSecondMoreFails()
    {
        using NightGateFixture fixture = NightGateFixture.Create(CodePath);
        NightGateFixture.WriteGreenNight(fixture.MainNight, NightGateFixture.MainCommit, NightGate.MainBranch, NightGateFixture.LastNight);

        Assert.Equal(0, fixture.RunAt(NightJson.TextOf(NightGateFixture.LastNight + NightGate.Limit), out _, out _));
        Assert.Equal(Program.FaultExitCode, fixture.RunAt(NightJson.TextOf(NightGateFixture.LastNight + NightGate.Limit + TimeSpan.FromSeconds(1)), out _, out string errors));
        Assert.Contains("stale:", errors, StringComparison.Ordinal);
    }

    /// <summary>Exit test 2: a night run that failed fails the gate with its conclusion.</summary>
    [Fact]
    public void AFailedNightRunFailsAsFailed()
    {
        using NightGateFixture fixture = NightGateFixture.Create(CodePath);
        NightGateFixture.WriteRun(fixture.MainNight, 102, NightGateFixture.MainCommit, NightGate.MainBranch, "failure", NightGateFixture.LastNight);

        int exitCode = fixture.Run(out _, out string errors);

        Assert.Equal(Program.FaultExitCode, exitCode);
        Assert.Contains($"failed: the night run 102 on the commit {NightGateFixture.MainCommit}, started at 2026-09-28T04:17:43Z, ended as 'failure'.", errors, StringComparison.Ordinal);
        Assert.Contains("absent: the night run 102", errors, StringComparison.Ordinal);
    }

    /// <summary>Exit test 2: a failed record fails the gate with its counts and its first seed.</summary>
    [Fact]
    public void AFailedRecordFailsWithItsCounts()
    {
        using NightGateFixture fixture = NightGateFixture.Create(CodePath);
        NightGateFixture.WriteGreenNight(fixture.MainNight, NightGateFixture.MainCommit, NightGate.MainBranch, NightGateFixture.LastNight);
        NightGateFixture.WriteRecord(fixture.MainNight, NightGateFixture.RedRecord(NightGateFixture.MainCommit, "macos-26"));

        int exitCode = fixture.Run(out _, out string errors);

        Assert.Equal(Program.FaultExitCode, exitCode);
        Assert.Contains("failed: the random runs of the leg macos-26", errors, StringComparison.Ordinal);
        Assert.Contains("ended with 2 softlocks and 1 crashes, from the first seed 7000000000.", errors, StringComparison.Ordinal);
    }

    /// <summary>Exit test 2: a leg with no record fails the gate as absent, and names the leg.</summary>
    [Fact]
    public void ALegWithNoRecordFailsAsAbsent()
    {
        using NightGateFixture fixture = NightGateFixture.Create(CodePath);
        NightGateFixture.WriteGreenNight(fixture.MainNight, NightGateFixture.MainCommit, NightGate.MainBranch, NightGateFixture.LastNight);
        Directory.Delete(Path.Combine(fixture.MainNight, NightLegs.ArtifactOf("windows-2025")), recursive: true);

        int exitCode = fixture.Run(out _, out string errors);

        Assert.Equal(Program.FaultExitCode, exitCode);
        Assert.Contains("holds no night record of the leg windows-2025.", errors, StringComparison.Ordinal);
    }

    /// <summary>Exit test 3: a docs-only PR passes with no night at all (D-513).</summary>
    [Fact]
    public void ADocsOnlyPrPassesWithNoNight()
    {
        using NightGateFixture fixture = NightGateFixture.Create("docs/design.md", "CLAUDE.md", "AGENTS.md", ".claude/skills/ste-writing/SKILL.md", ".github/pull_request_template.md");

        int exitCode = fixture.Run(out string output, out string errors);

        Assert.True(exitCode == 0, errors);
        Assert.Contains("a docs-only PR passes the night gate (D-513)", output, StringComparison.Ordinal);
    }

    /// <summary>A workflow and the settings file of the harness each bind the PR (D-513, D-700).</summary>
    [Theory]
    [InlineData(".github/workflows/ci.yml")]
    [InlineData(".claude/settings.json")]
    public void AWorkflowOrASettingsFileBindsThePr(string path)
    {
        using NightGateFixture fixture = NightGateFixture.Create("docs/design.md", path);

        int exitCode = fixture.Run(out _, out string errors);

        Assert.Equal(Program.FaultExitCode, exitCode);
        Assert.Contains($"The PR changes the path '{path}', so the night gate binds it", errors, StringComparison.Ordinal);
    }

    /// <summary>Exit test 4: a green night on the head commit passes the PR, with a stale night on `main` (D-510).</summary>
    [Fact]
    public void AGreenNightOnTheHeadCommitPassesThePr()
    {
        using NightGateFixture fixture = NightGateFixture.Create(CodePath);
        NightGateFixture.WriteGreenNight(fixture.MainNight, NightGateFixture.MainCommit, NightGate.MainBranch, NightGateFixture.Now - TimeSpan.FromDays(5));
        NightGateFixture.WriteGreenNight(fixture.HeadNight, NightGateFixture.Head, "fix/pr-99-night", NightGateFixture.Now - TimeSpan.FromHours(2));

        int exitCode = fixture.Run(out string output, out string errors);

        Assert.True(exitCode == 0, errors);
        Assert.Contains($"The night run 101 on the head commit {NightGateFixture.Head} succeeded on each leg, and it passes this PR alone (D-510).", output, StringComparison.Ordinal);
    }

    /// <summary>A failed night on the head commit fails the PR, even with a green night on `main` (D-510, T-2).</summary>
    [Fact]
    public void AFailedNightOnTheHeadCommitFailsThePr()
    {
        using NightGateFixture fixture = NightGateFixture.Create(CodePath);
        NightGateFixture.WriteGreenNight(fixture.MainNight, NightGateFixture.MainCommit, NightGate.MainBranch, NightGateFixture.LastNight);
        NightGateFixture.WriteGreenNight(fixture.HeadNight, NightGateFixture.Head, "fix/pr-99-night", NightGateFixture.Now - TimeSpan.FromHours(2));
        NightGateFixture.WriteRecord(fixture.HeadNight, NightGateFixture.RedRecord(NightGateFixture.Head, "ubuntu-24.04"));

        int exitCode = fixture.Run(out _, out string errors);

        Assert.Equal(Program.FaultExitCode, exitCode);
        Assert.Contains($"The night run 101 on the head commit {NightGateFixture.Head} does not pass:", errors, StringComparison.Ordinal);
        Assert.Contains("failed: the random runs of the leg ubuntu-24.04", errors, StringComparison.Ordinal);
    }

    /// <summary>A night that played another commit does not pass as the night of the head (D-510).</summary>
    [Fact]
    public void AHeadNightOfAnotherCommitFails()
    {
        using NightGateFixture fixture = NightGateFixture.Create(CodePath);
        NightGateFixture.WriteGreenNight(fixture.HeadNight, NightGateFixture.MainCommit, "fix/pr-99-night", NightGateFixture.Now - TimeSpan.FromHours(2));

        int exitCode = fixture.Run(out _, out string errors);

        Assert.Equal(Program.FaultExitCode, exitCode);
        Assert.Contains($"wrong commit: the night run 101 on the commit {NightGateFixture.MainCommit}, started at 2026-09-28T07:00:00Z, did not play the head commit {NightGateFixture.Head}.", errors, StringComparison.Ordinal);
    }

    /// <summary>
    /// Exit test 5: a PR that carries a night record in its own checkout fails, because the gate
    /// reads the nights of the GitHub API alone (D-509).
    /// </summary>
    [Fact]
    public void APrThatCarriesANightRecordInItsCheckoutFails()
    {
        string carried = "artifacts/night/" + NightLegs.FileOf("ubuntu-24.04");
        using NightGateFixture fixture = NightGateFixture.Create(carried);
        string checkout = Path.Combine(fixture.Root, "checkout", "artifacts", "night");
        Directory.CreateDirectory(checkout);
        NightGateFixture.GreenRecord(NightGateFixture.Head, "ubuntu-24.04").Write(Path.Combine(checkout, NightLegs.FileOf("ubuntu-24.04")));

        int exitCode = fixture.Run(out _, out string errors);

        Assert.Equal(Program.FaultExitCode, exitCode);
        Assert.Contains($"The PR changes the path '{carried}', so the night gate binds it", errors, StringComparison.Ordinal);
        Assert.Contains("absent: no completed night exists on `main`", errors, StringComparison.Ordinal);
    }

    /// <summary>A night of another branch in the folder of `main` fails, because the gate reads the nights of `main` alone.</summary>
    [Fact]
    public void ANightOfAnotherBranchDoesNotPassAsTheNightOfMain()
    {
        using NightGateFixture fixture = NightGateFixture.Create(CodePath);
        NightGateFixture.WriteGreenNight(fixture.MainNight, NightGateFixture.MainCommit, "feat/pr-98-other", NightGateFixture.LastNight);

        int exitCode = fixture.Run(out _, out string errors);

        Assert.Equal(Program.FaultExitCode, exitCode);
        Assert.Contains("wrong branch:", errors, StringComparison.Ordinal);
    }

    /// <summary>A record that names another commit than its run fails the gate (T-2).</summary>
    [Fact]
    public void ARecordOfAnotherCommitFails()
    {
        using NightGateFixture fixture = NightGateFixture.Create(CodePath);
        NightGateFixture.WriteGreenNight(fixture.MainNight, NightGateFixture.MainCommit, NightGate.MainBranch, NightGateFixture.LastNight);
        NightGateFixture.WriteRecord(fixture.MainNight, NightGateFixture.GreenRecord(NightGateFixture.Head, "macos-26"));

        int exitCode = fixture.Run(out _, out string errors);

        Assert.Equal(Program.FaultExitCode, exitCode);
        Assert.Contains($"wrong commit: the night record of the leg macos-26 names the commit {NightGateFixture.Head}", errors, StringComparison.Ordinal);
    }

    /// <summary>The legs of one night play the same range, so a leg of another range fails (D-1190, D-1191).</summary>
    [Fact]
    public void ALegOfAnotherRangeFails()
    {
        using NightGateFixture fixture = NightGateFixture.Create(CodePath);
        NightGateFixture.WriteGreenNight(fixture.MainNight, NightGateFixture.MainCommit, NightGate.MainBranch, NightGateFixture.LastNight);
        NightRecord shifted = NightGateFixture.GreenRecord(NightGateFixture.MainCommit, "windows-2025") with { FirstSeed = NightGateFixture.FirstSeed + 1 };
        NightGateFixture.WriteRecord(fixture.MainNight, shifted);

        int exitCode = fixture.Run(out _, out string errors);

        Assert.Equal(Program.FaultExitCode, exitCode);
        Assert.Contains("wrong range: the leg windows-2025 played 30 greedy runs and 20 random runs from the seed 7000000001", errors, StringComparison.Ordinal);
    }

    /// <summary>A record in the artifact of another leg is an error that names both legs (T-2).</summary>
    [Fact]
    public void ARecordInTheArtifactOfAnotherLegIsAnError()
    {
        using NightGateFixture fixture = NightGateFixture.Create(CodePath);
        NightGateFixture.WriteGreenNight(fixture.MainNight, NightGateFixture.MainCommit, NightGate.MainBranch, NightGateFixture.LastNight);
        string path = Path.Combine(fixture.MainNight, NightLegs.ArtifactOf("macos-26"), NightLegs.FileOf("macos-26"));
        NightGateFixture.GreenRecord(NightGateFixture.MainCommit, "ubuntu-24.04").Write(path);

        int exitCode = fixture.Run(out _, out string errors);

        Assert.Equal(Program.FaultExitCode, exitCode);
        Assert.Contains("names the leg 'ubuntu-24.04', and its artifact belongs to the leg 'macos-26'.", errors, StringComparison.Ordinal);
    }

    /// <summary>A folder of a night that the job never wrote is an error, never an absent night (T-2).</summary>
    [Fact]
    public void AnAbsentFolderOfANightIsAnError()
    {
        using NightGateFixture fixture = NightGateFixture.Create(CodePath);
        Directory.Delete(fixture.HeadNight);

        int exitCode = fixture.Run(out _, out string errors);

        Assert.Equal(Program.FaultExitCode, exitCode);
        Assert.Contains("The folder of the night", errors, StringComparison.Ordinal);
        Assert.Contains("does not exist", errors, StringComparison.Ordinal);
    }

    /// <summary>A time of the gate in another form is an error that names the value (T-2).</summary>
    [Fact]
    public void ATimeInAnotherFormIsAnError()
    {
        using NightGateFixture fixture = NightGateFixture.Create(CodePath);

        int exitCode = fixture.RunAt("2026-09-28 09:00", out _, out string errors);

        Assert.Equal(Program.FaultExitCode, exitCode);
        Assert.Contains("it read '2026-09-28 09:00'", errors, StringComparison.Ordinal);
    }

    /// <summary>The command with no option names each option that it needs.</summary>
    [Fact]
    public void TheCommandWithNoOptionNamesEachOption()
    {
        using StringWriter output = new();
        using StringWriter errors = new();

        int exitCode = Program.Run([NightGateCommand.Name], output, errors);

        Assert.Equal(Program.FaultExitCode, exitCode);
        Assert.Contains("night-gate needs --pull-request, --main-night, --head-night, and --now.", errors.ToString(), StringComparison.Ordinal);
    }
}
