using System;
using System.Collections.Generic;
using System.Globalization;
using System.IO;
using System.Text.Json;
using TheThingBelow.Tools.ReviewGate;

namespace TheThingBelow.Tools.CodexReview;

/// <summary>
/// The `codex-review` command. It starts the cross-provider review of one pull request through
/// the Codex CLI, in a separate worktree at the head of the pull request, and it reads the
/// outcome from the review record on origin (D-926 to D-929). The Makefile target `codex-review`
/// runs it.
/// </summary>
/// <remarks>
/// The command automates the start of the review alone. The reviewer loads the `pr-review`
/// skill, and the depth of the review and the form of the record stay the same (D-926).
/// </remarks>
public static class CodexReviewCommand
{
    /// <summary>The name of the command on the command line.</summary>
    public const string Name = "codex-review";

    /// <summary>The option that names the root of the author checkout.</summary>
    public const string RootOption = "--root";

    /// <summary>The option that gives the GitHub number of the pull request.</summary>
    public const string PullRequestOption = "--pull-request";

    /// <summary>
    /// The flag that skips the check of the Gitar pass before the review (D-946). The flag is
    /// permanent, and the rules of the review loop say when the author passes it.
    /// </summary>
    public const string SkipGitarReviewOption = "--skip-gitar-review";

    /// <summary>The folder under the root that takes each transcript. Git ignores `artifacts/`.</summary>
    public const string TranscriptFolder = "artifacts/codex-review";

    /// <summary>
    /// The time limit of the review itself (D-1087). The longest review of PR #63 to PR #77 took
    /// 17 minutes, so the limit is more than five times that. At the limit the command stops
    /// the review and gives a fault that names the command, and the worktree stays for a read.
    /// </summary>
    public static readonly TimeSpan ReviewLimit = TimeSpan.FromMinutes(90);

    /// <summary>The time limit of the npm install of the CLI, which downloads the package (D-927, D-1087).</summary>
    public static readonly TimeSpan InstallLimit = TimeSpan.FromMinutes(10);

    /// <summary>The time limit of the model probe, which sends one short prompt (D-926, D-1087).</summary>
    public static readonly TimeSpan ProbeLimit = TimeSpan.FromMinutes(10);

    /// <summary>Reads the options and runs one review.</summary>
    /// <param name="args">The arguments after the command name.</param>
    /// <param name="output">The writer that takes the report.</param>
    /// <param name="errors">The writer that takes each fault and each refusal.</param>
    /// <returns>The exit code of the outcome: 0, 2, or 3, or 1 for a fault or a refusal.</returns>
    public static int Run(IReadOnlyList<string> args, TextWriter output, TextWriter errors)
    {
        ArgumentNullException.ThrowIfNull(args);
        ArgumentNullException.ThrowIfNull(output);
        ArgumentNullException.ThrowIfNull(errors);

        OptionParser? options = OptionParser.Read(Name, args, [RootOption, PullRequestOption], [SkipGitarReviewOption], errors);
        if (options is null)
        {
            return Program.FaultExitCode;
        }

        string? numberText = options.Value(PullRequestOption);
        if (numberText is null
            || !int.TryParse(numberText, NumberStyles.None, CultureInfo.InvariantCulture, out int number)
            || number <= 0)
        {
            errors.WriteLine(
                $"Error: {Name} needs {PullRequestOption} <number>, the GitHub number of the PR, such as `make codex-review PR=63` (T-2).");
            return Program.FaultExitCode;
        }

        string root = Path.GetFullPath(options.ValueOr(RootOption, "."));
        try
        {
            return Review(root, number, options.Holds(SkipGitarReviewOption), output, errors);
        }
        catch (Exception fault) when (fault is InvalidOperationException or IOException or UnauthorizedAccessException or JsonException)
        {
            errors.WriteLine($"Error: {Name} stopped on PR #{numberText} in '{root}'. {fault.Message}");
            return Program.FaultExitCode;
        }
    }

    /// <summary>
    /// Gives each reason of the Gitar pass that refuses the review. With the flag of D-946, the
    /// command reads no Gitar fact, writes that it skips the check, and gives no reason.
    /// </summary>
    /// <param name="skipGitarReview">True when the command line holds `--skip-gitar-review`.</param>
    /// <param name="readFacts">Reads the Gitar facts of the PR. The skip never calls it.</param>
    /// <param name="output">The writer that takes the line of the skip.</param>
    /// <returns>Each reason of <see cref="GitarPass.Check"/>, or none for the skip.</returns>
    public static IReadOnlyList<string> GitarReasons(bool skipGitarReview, Func<GitarFacts> readFacts, TextWriter output)
    {
        ArgumentNullException.ThrowIfNull(readFacts);
        ArgumentNullException.ThrowIfNull(output);

        if (skipGitarReview)
        {
            output.WriteLine($"{Name}: {SkipGitarReviewOption} skips the check of the Gitar pass (D-946).");
            return [];
        }

        return GitarPass.Check(readFacts());
    }

    private static int Review(string root, int number, bool skipGitarReview, TextWriter output, TextWriter errors)
    {
        output.WriteLine($"{Name}: install the newest `{CodexCli.Package}` with npm (D-927).");
        string codex = InstallCli(root);
        output.WriteLine($"{Name}: check the ChatGPT login of the CLI, with no API key in its environment (D-932).");
        CheckLogin(codex, root);
        output.WriteLine($"{Name}: probe the model `{CodexCli.Model}` at the effort `{CodexCli.ReasoningEffort}` (D-926).");
        ProbeModel(codex);

        output.WriteLine($"{Name}: read PR #{number.ToString(CultureInfo.InvariantCulture)}, the checkout, and the Gitar pass.");
        (CheckoutFacts checkout, string baseBranch) = ReadCheckout(root, number);
        List<string> reasons = [.. StartChecks.Check(checkout)];
        CommitFacts? head = ReadEffectiveHead(root, number, checkout.Branch, baseBranch);
        if (head is null)
        {
            reasons.Add("every commit of the PR changes the metadata set alone, so the PR has no effective head (D-610).");
        }
        else
        {
            reasons.AddRange(GitarReasons(
                skipGitarReview,
                () => ReadGitarFacts(root, number, checkout.Branch, baseBranch, head),
                output));
        }

        if (reasons.Count > 0 || head is null)
        {
            foreach (string reason in reasons)
            {
                errors.WriteLine($"Refused: {reason}");
            }

            errors.WriteLine($"{Name}: outcome refused (exit {Program.FaultExitCode}). No review ran (T-2).");
            return Program.FaultExitCode;
        }

        string localBranch = $"review/pr-{number.ToString(CultureInfo.InvariantCulture)}";
        string worktree = PrepareWorktree(root, number, checkout.Branch, localBranch);
        (string transcript, string errorLog, string lastMessage) = TranscriptPaths(root, number, head.Sha);
        output.WriteLine($"{Name}: the review runs in '{worktree}'. The transcript is '{transcript}'.");

        string prompt = CodexCli.ReviewPrompt(number, checkout.Branch, localBranch, skipGitarReview);
        ProgramResult run = RunCodex(codex, CodexCli.ReviewArguments(worktree, lastMessage, prompt), worktree, transcript, ReviewLimit);
        File.WriteAllText(errorLog, run.Error);

        ExternalProgram.RunChecked("git", ["fetch", "--quiet", "origin"], root);
        string path = ReviewRecordRules.RecordPath(number);
        string? record = ReadRemoteFile(root, checkout.Branch, path);
        CommitFacts? reviewedHead = ReadEffectiveHead(root, number, checkout.Branch, baseBranch);
        ReviewOutcome outcome = ReviewOutcome.Decide(run.ExitCode, path, record, reviewedHead);

        Report(outcome, transcript, errorLog, output);
        if (outcome.Kind == ReviewOutcomeKind.Fault)
        {
            output.WriteLine($"{Name}: the worktree stays at '{worktree}' for the fault. The next run removes it.");
        }
        else
        {
            RemoveWorktree(root, worktree, localBranch);
        }

        return outcome.ExitCode;
    }

    private static string InstallCli(string root)
    {
        ExternalProgram.RunChecked("npm", ["install", "--global", $"{CodexCli.Package}@latest"], root, InstallLimit);
        string prefix = ExternalProgram.RunChecked("npm", ["prefix", "--global"], root);
        string codex = Path.Combine(prefix, "bin", "codex");
        if (!File.Exists(codex))
        {
            throw new InvalidOperationException(
                $"npm installed `{CodexCli.Package}`, and '{codex}' does not exist. Read `npm prefix --global` (D-927).");
        }

        ProgramResult versionRun = RunCodex(codex, ["--version"], root, null, ExternalProgram.StepLimit);
        if (versionRun.ExitCode != 0)
        {
            throw new InvalidOperationException(
                $"`{codex} --version` gave the exit code {versionRun.ExitCode} (D-927). {Tail(versionRun.Error)}");
        }

        CodexVersion version = CodexCli.ParseVersion(versionRun.Output);
        if (!version.IsAtLeast(CodexCli.MinimumVersion))
        {
            throw new InvalidOperationException(
                $"'{codex}' is the version {version}, and the review needs {CodexCli.MinimumVersion} or later (D-927).");
        }

        return codex;
    }

    private static void CheckLogin(string codex, string root)
    {
        ProgramResult status = RunCodex(codex, CodexCli.LoginStatusArguments, root, null, ExternalProgram.StepLimit);
        if (!CodexCli.IsChatGptLogin(status))
        {
            throw new InvalidOperationException(
                $"the CLI has no ChatGPT login, so a review can run at API prices. Run `codex login` in a terminal (D-932). " +
                $"`codex login status` gave the exit code {status.ExitCode}: {Tail(status.Output + '\n' + status.Error)}");
        }
    }

    /// <summary>Runs the Codex CLI with no API key in its environment. Each Codex call of the command goes through here (D-932).</summary>
    private static ProgramResult RunCodex(string codex, IReadOnlyList<string> arguments, string folder, string? outputFile, TimeSpan limit)
    {
        return ExternalProgram.Run(codex, arguments, folder, outputFile, limit, CodexCli.ApiKeyVariables);
    }

    private static void ProbeModel(string codex)
    {
        string folder = Path.Combine(Path.GetTempPath(), "the-thing-below-codex-probe");
        if (Directory.Exists(folder))
        {
            Directory.Delete(folder, true);
        }

        Directory.CreateDirectory(folder);
        string answerFile = Path.Combine(folder, "answer.txt");
        ProgramResult probe = RunCodex(codex, CodexCli.ProbeArguments(folder, answerFile), folder, null, ProbeLimit);
        if (probe.ExitCode != 0)
        {
            throw new InvalidOperationException(
                $"the model probe gave the exit code {probe.ExitCode}, so the CLI refuses `{CodexCli.Model}` or the effort (D-926). {Tail(probe.Error)}");
        }

        string answer = File.Exists(answerFile) ? File.ReadAllText(answerFile).Trim() : string.Empty;
        if (!string.Equals(answer, CodexCli.ProbeAnswer, StringComparison.OrdinalIgnoreCase))
        {
            throw new InvalidOperationException(
                $"the model probe answered '{answer}', and the probe needs '{CodexCli.ProbeAnswer}' (D-926).");
        }
    }

    private static (CheckoutFacts Facts, string BaseBranch) ReadCheckout(string root, int number)
    {
        string numberText = number.ToString(CultureInfo.InvariantCulture);
        string json = ExternalProgram.RunChecked(
            "gh",
            ["pr", "view", numberText, "--json", "state,headRefName,headRefOid,baseRefName"],
            root);
        using JsonDocument document = JsonDocument.Parse(json);
        JsonElement pr = document.RootElement;
        string state = ReadField(pr, "state", numberText);
        string branch = ReadField(pr, "headRefName", numberText);
        string gitHubHead = ReadField(pr, "headRefOid", numberText);
        string baseBranch = ReadField(pr, "baseRefName", numberText);

        ExternalProgram.RunChecked("git", ["fetch", "--quiet", "origin"], root);
        ProgramResult symbolic = ExternalProgram.Run("git", ["symbolic-ref", "--quiet", "--short", "HEAD"], root, null, ExternalProgram.StepLimit);
        string? localBranch = symbolic.ExitCode == 0 ? symbolic.Output.Trim() : null;
        string localHead = ExternalProgram.RunChecked("git", ["rev-parse", "HEAD"], root);
        string remoteHead = ExternalProgram.RunChecked("git", ["rev-parse", $"origin/{branch}"], root);
        string status = ExternalProgram.RunChecked("git", ["status", "--porcelain"], root);

        CheckoutFacts facts = new CheckoutFacts(number, state, branch, gitHubHead, localBranch, localHead, remoteHead, status);
        return (facts, baseBranch);
    }

    private static CommitFacts? ReadEffectiveHead(string root, int number, string branch, string baseBranch)
    {
        string mergeBase = ExternalProgram.RunChecked("git", ["merge-base", $"origin/{baseBranch}", $"origin/{branch}"], root);
        string log = ExternalProgram.RunChecked("git", BranchCommits.LogArguments($"{mergeBase}..origin/{branch}"), root);
        return EffectiveHead.Find(BranchCommits.Parse(log), number);
    }

    private static GitarFacts ReadGitarFacts(string root, int number, string branch, string baseBranch, CommitFacts head)
    {
        string numberText = number.ToString(CultureInfo.InvariantCulture);
        string repo = ExternalProgram.RunChecked("gh", ["repo", "view", "--json", "nameWithOwner", "--jq", ".nameWithOwner"], root);
        string[] parts = repo.Split('/');
        if (parts.Length != 2)
        {
            throw new InvalidOperationException($"`gh repo view` gave '{repo}', which is no owner and name (T-2).");
        }

        string comments = ExternalProgram.RunChecked("gh", GitarPass.CommentArguments(repo, numberText), root);

        string tip = ExternalProgram.RunChecked("git", ["rev-parse", $"origin/{branch}"], root);
        string statuses = ExternalProgram.RunChecked("gh", GitarPass.CheckRunArguments(repo, tip), root);
        string threads = ExternalProgram.RunChecked("gh", GitarPass.ThreadArguments(parts[0], parts[1], numberText), root);

        // The push that brought the effective head made the first check suite of a commit from
        // that head to the tip. A later push of metadata alone makes later suites (D-603).
        string mergeBase = ExternalProgram.RunChecked("git", ["merge-base", $"origin/{baseBranch}", $"origin/{branch}"], root);
        IReadOnlyList<CommitFacts> commits = BranchCommits.Parse(
            ExternalProgram.RunChecked("git", BranchCommits.LogArguments($"{mergeBase}..origin/{branch}"), root));
        List<string> suiteTimes = [];
        bool reached = false;
        foreach (CommitFacts commit in commits)
        {
            reached = reached || string.Equals(commit.Sha, head.Sha, StringComparison.Ordinal);
            if (reached)
            {
                suiteTimes.Add(ExternalProgram.RunChecked(
                    "gh",
                    GitarPass.CheckSuiteArguments(repo, commit.Sha),
                    root));
            }
        }

        return new GitarFacts(
            GitarPass.ParseComments(comments),
            GitarPass.SplitLines(statuses),
            GitarPass.ParseThreads(threads),
            GitarPass.Earliest(string.Join('\n', suiteTimes)));
    }

    private static string PrepareWorktree(string root, int number, string branch, string localBranch)
    {
        string worktree = Path.Combine(Path.GetTempPath(), $"the-thing-below-review-pr-{number.ToString(CultureInfo.InvariantCulture)}");
        if (Directory.Exists(worktree))
        {
            string list = ExternalProgram.RunChecked("git", ["worktree", "list", "--porcelain"], root);
            if (!ListsWorktree(list, worktree))
            {
                throw new InvalidOperationException(
                    $"the folder '{worktree}' exists, and git lists no worktree there. Move the folder out of the way (T-2).");
            }

            ExternalProgram.RunChecked("git", ["worktree", "remove", "--force", worktree], root);
        }

        ExternalProgram.RunChecked("git", ["worktree", "prune"], root);

        // The pre-commit hook refuses a checkout with no branch, so the worktree takes a local
        // branch that tracks the PR branch. The reviewer pushes with `HEAD:<branch>` (D-926).
        ExternalProgram.RunChecked("git", ["worktree", "add", "--quiet", "-B", localBranch, worktree, $"origin/{branch}"], root);
        return worktree;
    }

    private static void RemoveWorktree(string root, string worktree, string localBranch)
    {
        ExternalProgram.RunChecked("git", ["worktree", "remove", "--force", worktree], root);
        ExternalProgram.RunChecked("git", ["branch", "--quiet", "-D", localBranch], root);
    }

    private static bool ListsWorktree(string list, string worktree)
    {
        string full = Path.GetFullPath(worktree).TrimEnd(Path.DirectorySeparatorChar);
        foreach (string line in GitarPass.SplitLines(list))
        {
            if (!line.StartsWith("worktree ", StringComparison.Ordinal))
            {
                continue;
            }

            // macOS gives the temporary folder under `/var`, and git gives it under `/private/var`.
            string listed = line["worktree ".Length..].TrimEnd(Path.DirectorySeparatorChar);
            if (string.Equals(listed, full, StringComparison.Ordinal)
                || string.Equals(listed, "/private" + full, StringComparison.Ordinal))
            {
                return true;
            }
        }

        return false;
    }

    private static (string Transcript, string ErrorLog, string LastMessage) TranscriptPaths(string root, int number, string sha)
    {
        string folder = Path.Combine(root, TranscriptFolder);
        Directory.CreateDirectory(folder);
        string stem = $"pr-{number.ToString(CultureInfo.InvariantCulture)}-{sha[..7]}";
        string name = stem;
        for (int run = 2; File.Exists(Path.Combine(folder, name + ".jsonl")); run++)
        {
            name = $"{stem}-{run.ToString(CultureInfo.InvariantCulture)}";
        }

        return (
            Path.Combine(folder, name + ".jsonl"),
            Path.Combine(folder, name + ".err.log"),
            Path.Combine(folder, name + ".last.md"));
    }

    private static string? ReadRemoteFile(string root, string branch, string path)
    {
        string spec = $"origin/{branch}:{path}";
        ProgramResult exists = ExternalProgram.Run("git", ["cat-file", "-e", spec], root, null, ExternalProgram.StepLimit);
        if (exists.ExitCode != 0)
        {
            return null;
        }

        return ExternalProgram.RunChecked("git", ["show", spec], root);
    }

    private static void Report(ReviewOutcome outcome, string transcript, string errorLog, TextWriter output)
    {
        output.WriteLine($"{Name}: verdict: {outcome.Verdict ?? "none"}");
        output.WriteLine($"{Name}: open findings: {(outcome.OpenFindings.Count == 0 ? "none" : string.Join(", ", outcome.OpenFindings))}");
        foreach (ReviewFinding strike in outcome.Strikes)
        {
            output.WriteLine(
                $"{Name}: three-strike stop: {strike.Id} is open at {string.Join(", ", strike.OpenHeads)} (D-929).");
        }

        output.WriteLine($"{Name}: transcript: {transcript}");
        output.WriteLine($"{Name}: error log: {errorLog}");
        output.WriteLine($"{Name}: {outcome.Reason}");
        string kind = outcome.Kind switch
        {
            ReviewOutcomeKind.Approve => "approve",
            ReviewOutcomeKind.ChangesRequired => "changes-required",
            ReviewOutcomeKind.ThreeStrikes => "three-strike-stop",
            _ => "fault",
        };
        output.WriteLine($"{Name}: outcome {kind} (exit {outcome.ExitCode.ToString(CultureInfo.InvariantCulture)})");
    }

    private static string ReadField(JsonElement pr, string field, string numberText)
    {
        if (!pr.TryGetProperty(field, out JsonElement value) || value.ValueKind != JsonValueKind.String)
        {
            throw new InvalidOperationException($"`gh pr view {numberText}` gave no text field `{field}` (T-2).");
        }

        string? text = value.GetString();
        if (string.IsNullOrEmpty(text))
        {
            throw new InvalidOperationException($"`gh pr view {numberText}` gave an empty field `{field}` (T-2).");
        }

        return text;
    }

    private static string Tail(string text)
    {
        IReadOnlyList<string> lines = GitarPass.SplitLines(text);
        int start = Math.Max(0, lines.Count - 5);
        List<string> tail = [];
        for (int index = start; index < lines.Count; index++)
        {
            tail.Add(lines[index]);
        }

        return string.Join(" | ", tail);
    }
}
