using System;
using System.Collections.Generic;
using System.IO;
using System.Xml.Linq;
using TheThingBelow.Tools;
using TheThingBelow.Tools.CodexReview;
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
    public void ARemovedMarkLetsTheNextCheckStartTheNightAgain()
    {
        // Finding of the Gitar pass on PR #89: a fault before the session kept the mark, and no
        // later check retried the night.
        NightWatch.MarkHandled(this.state, Failed, Guid.NewGuid(), NightGateFixture.Now);
        NightWatch.RemoveMark(this.state, Failed);

        Assert.Equal(Failed, NightWatch.Decide(Failed, NightWatch.ReadHandled(this.state), NightOrder.None).Night);
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
    [Fact]
    public void AFailedStartPushoverStillRunsTheSession()
    {
        // Finding of the second Gitar pass on PR #89: a failed start Pushover stopped the start
        // after the worktree existed, and each retry left one more worktree.
        FakePrograms fake = new() { NotifyFails = true };

        int exitCode = this.Start(fake);

        Assert.Equal(0, exitCode);
        Assert.Equal(1, fake.Sessions);
        Assert.Contains(Failed.Id, NightWatch.ReadHandled(this.state));
        Assert.Contains("failed", File.ReadAllText(Path.Combine(this.state, NightWatch.LogFile)), StringComparison.Ordinal);
    }

    [Fact]
    public void AFaultBeforeTheSessionRemovesItsWorktreeAndItsMark()
    {
        FakePrograms fake = new() { WorktreeAddFails = true };

        int exitCode = this.Start(fake);

        Assert.Equal(Program.FaultExitCode, exitCode);
        Assert.Equal(0, fake.Sessions);
        Assert.Equal(1, fake.Removed);
        Assert.Empty(Directory.GetDirectories(Path.Combine(this.state, NightWatch.WorktreeFolder)));
        Assert.DoesNotContain(Failed.Id, NightWatch.ReadHandled(this.state));
        Assert.Contains(fake.Notices, notice => notice.Contains("title=The Thing Below: fix session stopped", StringComparison.Ordinal));
    }

    [Fact]
    public void ASessionPastItsLimitKeepsTheMarkAndSendsTheStop()
    {
        FakePrograms fake = new() { SessionFails = true };

        int exitCode = this.Start(fake);

        Assert.Equal(Program.FaultExitCode, exitCode);
        Assert.Contains(Failed.Id, NightWatch.ReadHandled(this.state));
        Assert.Equal(0, fake.Removed);
        Assert.Contains(fake.Notices, notice => notice.Contains("title=The Thing Below: fix session stopped", StringComparison.Ordinal));
    }

    private int Start(FakePrograms fake)
    {
        Directory.CreateDirectory(this.state);
        return NightWatchCommand.StartSession(fake.Programs(), "o/r", this.state, this.state, "claude", Failed, TextWriter.Null);
    }

    /// <summary>Fakes of git, gh, and Claude Code for the start of a session.</summary>
    private sealed class FakePrograms
    {
        public bool NotifyFails { get; init; }

        public bool WorktreeAddFails { get; init; }

        public bool SessionFails { get; init; }

        public int Sessions { get; private set; }

        public int Removed { get; private set; }

        public List<string> Notices { get; } = [];

        public WatchPrograms Programs() => new(this.RunChecked, this.RunSession);

        private string RunChecked(string program, IReadOnlyList<string> arguments, string folder)
        {
            string line = string.Join(' ', arguments);
            if (program == "gh" && line.StartsWith("workflow run notify.yml", StringComparison.Ordinal))
            {
                this.Notices.Add(line);
                return this.NotifyFails ? throw new InvalidOperationException("gh: the workflow notify.yml is absent") : string.Empty;
            }

            int add = IndexOf(arguments, "add");
            if (add >= 0)
            {
                Directory.CreateDirectory(arguments[add + 2]);
                return this.WorktreeAddFails ? throw new InvalidOperationException("git: the checkout of the worktree failed") : string.Empty;
            }

            int remove = IndexOf(arguments, "remove");
            if (remove >= 0)
            {
                this.Removed += 1;
                Directory.Delete(arguments[^1], recursive: true);
            }

            return string.Empty;
        }

        private ProgramResult RunSession(string program, IReadOnlyList<string> arguments, string folder, string output, TimeSpan limit)
        {
            this.Sessions += 1;
            return this.SessionFails ? throw new InvalidOperationException("`claude` ran past its limit of 86400 seconds") : new ProgramResult(0, string.Empty, string.Empty);
        }

        private static int IndexOf(IReadOnlyList<string> arguments, string word)
        {
            for (int index = 0; index < arguments.Count; index += 1)
            {
                if (arguments[index] == word)
                {
                    return index;
                }
            }

            return -1;
        }
    }
}
