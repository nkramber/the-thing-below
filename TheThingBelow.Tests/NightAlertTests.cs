using System;
using System.IO;
using TheThingBelow.Tools;
using TheThingBelow.Tools.Night;
using TheThingBelow.Tools.Notify;
using Xunit;

namespace TheThingBelow.Tests;

/// <summary>The alert of a failed night and the `night-alert` command (D-1201). No test reaches the network.</summary>
public sealed class NightAlertTests : IDisposable
{
    private const string Commit = "a8ba7104b96dadd9b30816ae85b4efe81c621524";

    private const string RunLink = "https://github.com/nkramber/the-thing-below/actions/runs/36290389944";

    private const string FailedJobs = """
        [
          {"name": "night (ubuntu-24.04)", "conclusion": "success"},
          {"name": "night (windows-2025)", "conclusion": "failure"},
          {"name": "night (macos-26)", "conclusion": "cancelled"},
          {"name": "alert", "conclusion": null}
        ]
        """;

    private readonly string folder = Path.Combine(Path.GetTempPath(), "night-alert-" + Guid.NewGuid().ToString("N"));

    public NightAlertTests()
    {
        Directory.CreateDirectory(this.folder);
    }

    public void Dispose()
    {
        Directory.Delete(this.folder, recursive: true);
    }

    [Fact]
    public void TheMessageNamesEachFailedLegTheSeedTheBranchAndTheLink()
    {
        PushoverMessage message = NightAlert.MessageOf(new NightAlertFacts(NightAlert.ReadJobs(this.Jobs(FailedJobs)), 36 * NightLegs.SeedStep, "main", Commit, RunLink));

        Assert.Equal(NightAlert.Title, message.Title);
        Assert.Equal(RunLink, message.Link);
        Assert.Contains("Failed legs: windows-2025 (failure), macos-26 (cancelled).", message.Text, StringComparison.Ordinal);
        Assert.Contains("First seed: 36000000000.", message.Text, StringComparison.Ordinal);
        Assert.Contains("Branch: main. Commit: a8ba7104b96d.", message.Text, StringComparison.Ordinal);
        Assert.DoesNotContain("ubuntu", message.Text, StringComparison.Ordinal);
        Pushover.Check(message);
    }

    [Fact]
    public void ANightWithNoSeedSaysSo()
    {
        PushoverMessage message = NightAlert.MessageOf(new NightAlertFacts(NightAlert.ReadJobs(this.Jobs(FailedJobs)), null, "main", Commit, RunLink));

        Assert.Contains("First seed: absent, because no leg reached the seed step.", message.Text, StringComparison.Ordinal);
    }

    [Fact]
    public void JobsWithNoFailedLegGiveNoMessage()
    {
        string jobs = this.Jobs("""[{"name": "night (ubuntu-24.04)", "conclusion": "success"}, {"name": "alert", "conclusion": "failure"}]""");

        InvalidOperationException fault = Assert.Throws<InvalidOperationException>(
            () => NightAlert.MessageOf(new NightAlertFacts(NightAlert.ReadJobs(jobs), 1, "main", Commit, RunLink)));

        Assert.Contains("hold no leg job that failed", fault.Message, StringComparison.Ordinal);
    }

    [Fact]
    public void AJobWithNoConclusionFieldFails()
    {
        string jobs = this.Jobs("""[{"name": "night (ubuntu-24.04)"}]""");

        InvalidOperationException fault = Assert.Throws<InvalidOperationException>(() => NightAlert.ReadJobs(jobs));

        Assert.Contains("'conclusion'", fault.Message, StringComparison.Ordinal);
    }

    [Fact]
    public void TheCommandSendsTheAlert()
    {
        PushoverFakeHandler handler = PushoverFakeHandler.Taking();
        using StringWriter output = new();
        using StringWriter errors = new();

        int exitCode = NightAlertCommand.Run(
            ["--jobs", this.Jobs(FailedJobs), "--first-seed", "36000000000", "--branch", "main", "--commit", Commit, "--run-link", RunLink],
            output,
            errors,
            Keys,
            handler);

        Assert.Equal(0, exitCode);
        Assert.Contains("windows-2025", Uri.UnescapeDataString(Assert.Single(handler.Bodies)), StringComparison.Ordinal);
    }

    [Fact]
    public void TheCommandWithNoSeedStillSendsTheAlert()
    {
        PushoverFakeHandler handler = PushoverFakeHandler.Taking();
        using StringWriter output = new();
        using StringWriter errors = new();

        int exitCode = NightAlertCommand.Run(
            ["--jobs", this.Jobs(FailedJobs), "--branch", "main", "--commit", Commit, "--run-link", RunLink],
            output,
            errors,
            Keys,
            handler);

        Assert.Equal(0, exitCode);
        Assert.Contains("absent", Uri.UnescapeDataString(Assert.Single(handler.Bodies).Replace('+', ' ')), StringComparison.Ordinal);
    }

    [Fact]
    public void TheCommandRefusesASeedThatIsNoNumber()
    {
        using StringWriter output = new();
        using StringWriter errors = new();

        int exitCode = NightAlertCommand.Run(
            ["--jobs", this.Jobs(FailedJobs), "--first-seed", "-5", "--branch", "main", "--commit", Commit, "--run-link", RunLink],
            output,
            errors,
            Keys,
            PushoverFakeHandler.Taking());

        Assert.Equal(Program.FaultExitCode, exitCode);
        Assert.Contains("whole number", errors.ToString(), StringComparison.Ordinal);
    }

    [Fact]
    public void TheCommandWithAnAbsentFileFails()
    {
        using StringWriter output = new();
        using StringWriter errors = new();

        int exitCode = NightAlertCommand.Run(
            ["--jobs", Path.Combine(this.folder, "none.json"), "--branch", "main", "--commit", Commit, "--run-link", RunLink],
            output,
            errors,
            Keys,
            PushoverFakeHandler.Taking());

        Assert.Equal(Program.FaultExitCode, exitCode);
        Assert.Contains("Error: night-alert stopped:", errors.ToString(), StringComparison.Ordinal);
    }

    [Fact]
    public void TheProgramKnowsTheCommand()
    {
        using StringWriter output = new();
        using StringWriter errors = new();

        int exitCode = Program.Run([NightAlertCommand.Name], output, errors);

        Assert.Equal(Program.FaultExitCode, exitCode);
        Assert.Contains("needs --jobs", errors.ToString(), StringComparison.Ordinal);
    }

    private static string? Keys(string name) => name switch
    {
        Pushover.UserKeyVariable => "fake-user",
        Pushover.ApiTokenVariable => "fake-token",
        _ => null,
    };

    private string Jobs(string text)
    {
        string path = Path.Combine(this.folder, Guid.NewGuid().ToString("N") + ".json");
        File.WriteAllText(path, text);
        return path;
    }
}
