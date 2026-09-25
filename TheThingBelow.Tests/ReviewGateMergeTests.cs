using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.IO;
using System.Linq;
using TheThingBelow.Tools.CodexReview;
using TheThingBelow.Tools.ReviewGate;
using Xunit;

namespace TheThingBelow.Tests;

/// <summary>
/// A merge into the branch of a pull request, read by git in a temporary repository (F-147). By
/// default git lists no path for a clean merge, and it sorts the commits of a merged branch by
/// date. A merge of a code commit that is older than the approved head then kept the approval.
/// </summary>
public sealed class ReviewGateMergeTests : IDisposable
{
    private const int Number = 21;

    private readonly string root = Path.Combine(Path.GetTempPath(), "review-gate-merge-" + Guid.NewGuid().ToString("N"));

    public ReviewGateMergeTests()
    {
        Directory.CreateDirectory(this.root);
    }

    public void Dispose()
    {
        Directory.Delete(this.root, recursive: true);
    }

    /// <summary>
    /// The regression test of F-147. The branch holds an approved commit of documents, and then
    /// the merge of a side branch whose code commit is older than the approval. The merge is the
    /// effective head, and the approved commit is no longer reviewable.
    /// </summary>
    [Fact]
    public void AMergeOfAnOlderCodeCommitIsTheEffectiveHead()
    {
        this.Git("2026-09-19T10:00:00Z", "init", "--quiet", "--initial-branch=main");
        this.Commit("2026-09-19T10:00:00Z", "README.md", "base");
        this.Git("2026-09-19T10:00:00Z", "checkout", "--quiet", "-b", "pr");
        string approved = this.Commit("2026-09-20T10:00:00Z", "docs/design.md", "approved");
        this.Git("2026-09-20T10:00:00Z", "checkout", "--quiet", "-b", "side", "main");
        string side = this.Commit("2026-09-01T10:00:00Z", "TheThingBelow.Core/Rules.cs", "side");
        this.Git("2026-09-21T10:00:00Z", "checkout", "--quiet", "pr");
        this.Git("2026-09-21T10:00:00Z", "merge", "--quiet", "--no-ff", "--no-verify", "-m", "merge", "side");
        string merge = this.Git("2026-09-21T10:00:00Z", "rev-parse", "HEAD");

        IReadOnlyList<CommitFacts> commits = BranchCommits.Parse(this.Git("2026-09-21T10:00:00Z", [.. BranchCommits.LogArguments("main..pr")]));
        IReadOnlyList<CommitFacts> heads = EffectiveHead.ReviewableHeads(commits, Number);

        Assert.DoesNotContain(commits, commit => commit.Sha == side);
        Assert.Equal([approved, merge], commits.Select(commit => commit.Sha));
        Assert.Equal(["TheThingBelow.Core/Rules.cs"], commits[1].Files);
        Assert.Equal([merge], heads.Select(head => head.Sha));
        Assert.Equal(merge, EffectiveHead.Find(commits, Number)?.Sha);
    }

    /// <summary>A merge that brings documents alone keeps the approval of the code commit before it (D-943).</summary>
    [Fact]
    public void AMergeOfDocumentsAloneKeepsTheApproval()
    {
        this.Git("2026-09-19T10:00:00Z", "init", "--quiet", "--initial-branch=main");
        this.Commit("2026-09-19T10:00:00Z", "README.md", "base");
        this.Git("2026-09-19T10:00:00Z", "checkout", "--quiet", "-b", "pr");
        string approved = this.Commit("2026-09-20T10:00:00Z", "TheThingBelow.Core/Rules.cs", "approved");
        this.Git("2026-09-20T10:00:00Z", "checkout", "--quiet", "-b", "side", "main");
        this.Commit("2026-09-20T11:00:00Z", "docs/design.md", "side");
        this.Git("2026-09-21T10:00:00Z", "checkout", "--quiet", "pr");
        this.Git("2026-09-21T10:00:00Z", "merge", "--quiet", "--no-ff", "--no-verify", "-m", "merge", "side");
        string merge = this.Git("2026-09-21T10:00:00Z", "rev-parse", "HEAD");

        IReadOnlyList<CommitFacts> commits = BranchCommits.Parse(this.Git("2026-09-21T10:00:00Z", [.. BranchCommits.LogArguments("main..pr")]));

        Assert.Equal([merge, approved], EffectiveHead.ReviewableHeads(commits, Number).Select(head => head.Sha));
    }

    private string Commit(string date, string path, string message)
    {
        string full = Path.Combine(this.root, path.Replace('/', Path.DirectorySeparatorChar));
        Directory.CreateDirectory(Path.GetDirectoryName(full)!);
        File.WriteAllText(full, message + "\n");
        this.Git(date, "add", "--", path);
        this.Git(date, "commit", "--quiet", "--no-verify", "-m", message);
        return this.Git(date, "rev-parse", "HEAD");
    }

    /// <summary>Runs git in the temporary repository with a fixed name and a fixed date for each commit.</summary>
    private string Git(string date, params string[] arguments)
    {
        ProcessStartInfo start = new ProcessStartInfo("git")
        {
            WorkingDirectory = this.root,
            RedirectStandardOutput = true,
            RedirectStandardError = true,
            UseShellExecute = false,
        };
        start.ArgumentList.Add("-c");
        start.ArgumentList.Add("commit.gpgsign=false");
        foreach (string argument in arguments)
        {
            start.ArgumentList.Add(argument);
        }

        start.Environment["GIT_AUTHOR_NAME"] = "fixture";
        start.Environment["GIT_AUTHOR_EMAIL"] = "fixture@example.invalid";
        start.Environment["GIT_COMMITTER_NAME"] = "fixture";
        start.Environment["GIT_COMMITTER_EMAIL"] = "fixture@example.invalid";
        start.Environment["GIT_AUTHOR_DATE"] = date;
        start.Environment["GIT_COMMITTER_DATE"] = date;

        using Process process = Process.Start(start)
            ?? throw new InvalidOperationException($"`git {string.Join(' ', arguments)}` gave no process in '{this.root}'.");
        // The error stream reads beside the output, so a full pipe of one stream never holds git.
        System.Threading.Tasks.Task<string> error = process.StandardError.ReadToEndAsync();
        string output = process.StandardOutput.ReadToEnd();
        process.WaitForExit();
        string errorText = error.GetAwaiter().GetResult();
        if (process.ExitCode != 0)
        {
            throw new InvalidOperationException(
                $"`git {string.Join(' ', arguments)}` gave the exit code {process.ExitCode} in '{this.root}'. {errorText.Trim()}");
        }

        return output.Trim();
    }
}
