using System;
using System.IO;
using System.Xml.Linq;
using TheThingBelow.Tools;
using TheThingBelow.Tools.Night;
using TheThingBelow.Tools.Watch;
using Xunit;

namespace TheThingBelow.Tests;

/// <summary>The rules of the night watcher and its launchd file (D-1205 to D-1208). No test starts a session.</summary>
public sealed class NightWatchTests : IDisposable
{
    private static readonly WatchedNight Failed = new(36290389944, "a8ba7104b96dadd9b30816ae85b4efe81c621524", "failure", "https://github.com/nkramber/the-thing-below/actions/runs/36290389944");

    private readonly string state = Path.Combine(Path.GetTempPath(), "night-watch-" + Guid.NewGuid().ToString("N"));

    public void Dispose()
    {
        if (Directory.Exists(this.state))
        {
            Directory.Delete(this.state, recursive: true);
        }
    }

    [Fact]
    public void AFailedNightStartsASession()
    {
        WatchStep step = NightWatch.Decide(Failed, [], NightOrder.None);

        Assert.Equal(Failed, step.Night);
        Assert.Contains("starts a fix session (D-1205)", step.Reason, StringComparison.Ordinal);
    }

    [Fact]
    public void AGreenNightStartsNoSession()
    {
        Assert.Null(NightWatch.Decide(Failed with { Conclusion = "success" }, [], NightOrder.None).Night);
    }

    [Fact]
    public void NoNightStartsNoSession()
    {
        Assert.Equal("No completed night exists on main.", NightWatch.Decide(null, [], NightOrder.None).Reason);
    }

    [Fact]
    public void AHandledNightNeverStartsASecondSession()
    {
        NightWatch.MarkHandled(this.state, Failed, Guid.NewGuid(), NightGateFixture.Now);

        WatchStep step = NightWatch.Decide(Failed, NightWatch.ReadHandled(this.state), NightOrder.None);

        Assert.Null(step.Night);
        Assert.Contains("got a fix session before", step.Reason, StringComparison.Ordinal);
    }

    [Fact]
    public void ANightThatANewerPromotionCoversStartsNoSession()
    {
        WatchStep step = NightWatch.Decide(Failed, [], NightOrder.Ahead);

        Assert.Null(step.Night);
        Assert.Contains("a newer promotion covers it (D-1202)", step.Reason, StringComparison.Ordinal);
    }

    [Theory]
    [InlineData(NightOrder.Behind)]
    [InlineData(NightOrder.Identical)]
    public void AnOlderPromotionStillStartsASession(NightOrder order)
    {
        Assert.Equal(Failed, NightWatch.Decide(Failed, [], order).Night);
    }

    [Fact]
    public void AFileOfNoIdInTheHandledFolderStopsTheWatcher()
    {
        Directory.CreateDirectory(Path.Combine(this.state, NightWatch.HandledFolder));
        File.WriteAllText(Path.Combine(this.state, NightWatch.HandledFolder, "notes.txt"), "x");

        InvalidOperationException fault = Assert.Throws<InvalidOperationException>(() => NightWatch.ReadHandled(this.state));

        Assert.Contains("no run id", fault.Message, StringComparison.Ordinal);
    }

    [Fact]
    public void ThePromptNamesTheSkillTheNightAndTheSession()
    {
        Guid session = Guid.Parse("5042853c-402d-4a18-abcb-168734a801de");

        string prompt = NightWatch.PromptOf(Failed, session, "/tmp/worktree");

        Assert.Contains("`.claude/skills/night-fix/SKILL.md`", prompt, StringComparison.Ordinal);
        Assert.Contains("run 36290389944, commit a8ba7104b96dadd9b30816ae85b4efe81c621524", prompt, StringComparison.Ordinal);
        Assert.Contains("Session id: 5042853c-402d-4a18-abcb-168734a801de", prompt, StringComparison.Ordinal);
        Assert.Contains(Failed.Link, prompt, StringComparison.Ordinal);
    }

    [Fact]
    public void TheSkillOfThePromptExists()
    {
        Assert.True(File.Exists(RepositoryRoot.PathTo(".claude/skills/night-fix/SKILL.md")));
    }

    [Fact]
    public void TheLaunchdFileStartsTheCommandAtMinutesZeroAndThirty()
    {
        NightWatchJob job = new("/Users/o/.dotnet/dotnet", "/s/bin/TheThingBelow.Tools.dll", "/Volumes/SSD/the-thing-below", "/s", "/opt/homebrew/bin:/usr/bin&x", "/Users/o");

        XDocument plist = XDocument.Parse(NightWatchPlist.TextOf(job));
        string text = plist.ToString();

        Assert.Contains(NightWatchPlist.Label, text, StringComparison.Ordinal);
        Assert.Contains("<string>night-watch</string>", text, StringComparison.Ordinal);
        Assert.Contains("<string>/Volumes/SSD/the-thing-below</string>", text, StringComparison.Ordinal);
        Assert.Contains("<integer>0</integer>", text, StringComparison.Ordinal);
        Assert.Contains("<integer>30</integer>", text, StringComparison.Ordinal);
        Assert.Contains("/opt/homebrew/bin:/usr/bin&amp;x", text, StringComparison.Ordinal);
        Assert.Contains("/s/launchd-error.log", text, StringComparison.Ordinal);
    }

    [Fact]
    public void AProgramThatNoFolderOfThePathHoldsStopsTheInstall()
    {
        InvalidOperationException fault = Assert.Throws<InvalidOperationException>(() => NightWatchInstallCommand.Find("no-such-program-of-the-tests", this.state));

        Assert.Contains("'no-such-program-of-the-tests'", fault.Message, StringComparison.Ordinal);
    }

    [Fact]
    public void TheSessionBypassesThePromptsOfPermission()
    {
        // D-1208: a headless session has no person at a prompt.
        Assert.Equal("bypassPermissions", NightWatchCommand.PermissionMode);
    }

    [Theory]
    [InlineData("night-watch")]
    [InlineData("night-watch-install")]
    public void EachCommandWithNoOptionNamesEachOption(string command)
    {
        using StringWriter output = new();
        using StringWriter errors = new();

        Assert.Equal(Program.FaultExitCode, Program.Run([command], output, errors));
        Assert.Contains("needs --repository and --state", errors.ToString(), StringComparison.Ordinal);
    }
}
