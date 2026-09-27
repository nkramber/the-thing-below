using System;
using System.Collections.Generic;
using System.Globalization;
using System.IO;
using TheThingBelow.Tools.CodexReview;
using TheThingBelow.Tools.Night;

namespace TheThingBelow.Tools.Watch;

/// <summary>
/// The `night-watch` command of PR-108 (D-1205 to D-1208). The launchd job on the Mac of the owner
/// runs it at minute 0 and minute 30 of each hour. For a failed night of `main`, it starts one
/// session of Claude Code in a new worktree, and that session follows the `night-fix` skill.
/// </summary>
/// <remarks>
/// launchd starts no second copy of a job that runs, so the command waits for its session, and
/// no second session starts in the meantime. The command never merges (D-933).
/// </remarks>
public static class NightWatchCommand
{
    /// <summary>The name of the command.</summary>
    public const string Name = "night-watch";

    /// <summary>The option of the checkout of the repository, whose git data holds the worktrees.</summary>
    public const string RepositoryOption = "--repository";

    /// <summary>The option of the folder of the watcher: the handled nights, the worktrees, and the logs.</summary>
    public const string StateOption = "--state";

    /// <summary>The option of the program of Claude Code. It is `claude` from the PATH when absent.</summary>
    public const string ClaudeOption = "--claude";

    /// <summary>The permission mode of a fix session (D-1208).</summary>
    public const string PermissionMode = "bypassPermissions";

    /// <summary>The time that one fix session can take: three nights of an hour each, the reviews, and the fixes.</summary>
    public static readonly TimeSpan SessionLimit = TimeSpan.FromHours(24);

    /// <summary>Runs the command.</summary>
    /// <param name="args">The arguments after the command name.</param>
    /// <param name="output">The writer of each line of the log.</param>
    /// <param name="errors">The writer of each error.</param>
    /// <returns>The exit code: 0 when the check ran and each session ended with no fault, else <see cref="Program.FaultExitCode"/>.</returns>
    public static int Run(IReadOnlyList<string> args, TextWriter output, TextWriter errors)
    {
        ArgumentNullException.ThrowIfNull(args);
        ArgumentNullException.ThrowIfNull(output);
        ArgumentNullException.ThrowIfNull(errors);

        OptionParser? options = OptionParser.Read(Name, args, [RepositoryOption, StateOption, ClaudeOption], [], errors);
        if (options is null)
        {
            return Program.FaultExitCode;
        }

        string? repository = options.Value(RepositoryOption);
        string? state = options.Value(StateOption);
        if (repository is null || state is null)
        {
            errors.WriteLine($"Error: {Name} needs {RepositoryOption} and {StateOption}. {ClaudeOption} is `claude` from the PATH when absent.");
            return Program.FaultExitCode;
        }

        string claude = options.ValueOr(ClaudeOption, "claude");
        try
        {
            Directory.CreateDirectory(state);
            return Check(Path.GetFullPath(repository), Path.GetFullPath(state), claude, output);
        }
        catch (Exception fault) when (fault is IOException or UnauthorizedAccessException or InvalidOperationException)
        {
            errors.WriteLine($"Error: {Name} stopped: {fault.Message}");
            if (Directory.Exists(state))
            {
                Log(state, $"Error: {fault.Message}", TextWriter.Null);
            }

            return Program.FaultExitCode;
        }
    }

    private static int Check(string repository, string state, string claude, TextWriter output)
    {
        string repo = ExternalProgram.RunChecked("gh", ["repo", "view", "--json", "nameWithOwner", "--jq", ".nameWithOwner"], repository);
        WatchedNight? night = NewestNight(repo, repository);
        SortedSet<long> handled = NightWatch.ReadHandled(state);
        NightOrder order = NightOrder.None;
        if (night is not null && !string.Equals(night.Conclusion, NightGate.SuccessConclusion, StringComparison.Ordinal) && !handled.Contains(night.Id))
        {
            order = PromotionOrder(repo, repository, state, night);
        }

        WatchStep step = NightWatch.Decide(night, handled, order);
        Log(state, step.Reason, output);
        return step.Night is null ? 0 : StartSession(repo, repository, state, claude, step.Night, output);
    }

    private static int StartSession(string repo, string repository, string state, string claude, WatchedNight night, TextWriter output)
    {
        Guid sessionId = Guid.NewGuid();
        string id = night.Id.ToString(CultureInfo.InvariantCulture);
        string worktree = Path.Combine(state, NightWatch.WorktreeFolder, $"night-{id}");
        string log = Path.Combine(state, NightWatch.LogFolder, $"night-{id}.log");
        Directory.CreateDirectory(Path.GetDirectoryName(log)!);

        // The mark comes first, so a session that stops or fails never gets a second start.
        NightWatch.MarkHandled(state, night, sessionId, DateTimeOffset.UtcNow);
        ExternalProgram.RunChecked("git", ["-C", repository, "fetch", "--no-tags", "origin", NightGate.MainBranch], repository);
        ExternalProgram.RunChecked("git", ["-C", repository, "worktree", "add", "--detach", worktree, $"origin/{NightGate.MainBranch}"], repository);
        Notify(repo, repository, "The Thing Below: fix session started", $"Night run {id} failed. Session {sessionId:D} runs in {worktree}.", night.Link);
        Log(state, $"The session {sessionId:D} starts in '{worktree}', and its log is '{log}'.", output);

        ProgramResult result = ExternalProgram.Run(
            claude,
            ["-p", NightWatch.PromptOf(night, sessionId, worktree), "--permission-mode", PermissionMode, "--session-id", sessionId.ToString("D")],
            worktree,
            log,
            SessionLimit);
        if (result.ExitCode == 0)
        {
            Log(state, $"The session {sessionId:D} ended with the exit code 0.", output);
            return 0;
        }

        string message = $"The session {sessionId:D} of night run {id} ended with the exit code {result.ExitCode.ToString(CultureInfo.InvariantCulture)}. Log: {log}. {result.Error.Trim()}";
        Log(state, message, output);
        Notify(repo, repository, "The Thing Below: fix session stopped", Clip(message), night.Link);
        return Program.FaultExitCode;
    }

    private static WatchedNight? NewestNight(string repo, string repository)
    {
        string line = ExternalProgram.RunChecked(
            "gh",
            ["api", $"repos/{repo}/actions/workflows/night.yml/runs?branch={NightGate.MainBranch}&status=completed&per_page=1", "--jq", ".workflow_runs[0] // empty | [.id, .head_sha, .conclusion, .html_url] | @tsv"],
            repository);
        if (line.Length == 0)
        {
            return null;
        }

        string[] parts = line.Split('\t');
        if (parts.Length != 4 || !long.TryParse(parts[0], NumberStyles.None, CultureInfo.InvariantCulture, out long id) || !NightJson.IsCommit(parts[1]))
        {
            throw new InvalidOperationException($"The newest night of {repo} reads '{line}', and the watcher needs the id, the commit, the conclusion, and the link (T-2).");
        }

        return new WatchedNight(id, parts[1], parts[2], parts[3]);
    }

    private static NightOrder PromotionOrder(string repo, string repository, string state, WatchedNight night)
    {
        string runs = ExternalProgram.RunChecked(
            "gh",
            ["api", $"repos/{repo}/actions/artifacts?name={NightPromotion.ArtifactName}&per_page=30", "--jq", $".artifacts[] | select(.expired == false and .workflow_run.head_branch == \"{NightGate.MainBranch}\") | .workflow_run.id"],
            repository);
        foreach (string run in runs.Split('\n', StringSplitOptions.RemoveEmptyEntries | StringSplitOptions.TrimEntries))
        {
            string source = ExternalProgram.RunChecked("gh", ["api", $"repos/{repo}/actions/runs/{run}", "--jq", ".path + \" \" + .event"], repository);
            if (!string.Equals(source, ".github/workflows/night-promote.yml push", StringComparison.Ordinal))
            {
                continue;
            }

            string folder = Path.Combine(state, "promotion");
            if (Directory.Exists(folder))
            {
                Directory.Delete(folder, recursive: true);
            }

            ExternalProgram.RunChecked("gh", ["run", "download", run, "--repo", repo, "--name", NightPromotion.ArtifactName, "--dir", folder], repository);
            NightPromotion promotion = NightPromotion.Read(folder)
                ?? throw new InvalidOperationException($"The artifact {NightPromotion.ArtifactName} of the run {run} holds no {NightPromotion.PromotionFile} (T-2).");
            string status = ExternalProgram.RunChecked("gh", ["api", $"repos/{repo}/compare/{night.Commit}...{promotion.MergeCommit}", "--jq", ".status"], repository);
            return NightPromotion.OrderOf(status);
        }

        return NightOrder.None;
    }

    private static void Notify(string repo, string repository, string title, string message, string link)
    {
        // D-1207: the notify workflow reads the secrets, and the Mac holds no copy.
        ExternalProgram.RunChecked(
            "gh",
            ["workflow", "run", "notify.yml", "--repo", repo, "--ref", NightGate.MainBranch, "-f", $"title={title}", "-f", $"message={message}", "-f", $"link={link}"],
            repository);
    }

    private static string Clip(string text) => text.Length <= 1000 ? text : text[..1000];

    private static void Log(string state, string line, TextWriter output)
    {
        string stamped = $"{NightJson.TextOf(DateTimeOffset.UtcNow)} {line}";
        output.WriteLine(stamped);
        File.AppendAllText(Path.Combine(state, NightWatch.LogFile), stamped + "\n");
    }
}
