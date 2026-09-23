using System;
using System.Collections.Generic;
using System.Globalization;

namespace TheThingBelow.Tools.CodexReview;

/// <summary>One version of the Codex CLI, as `codex --version` prints it.</summary>
/// <param name="Major">The first number.</param>
/// <param name="Minor">The second number.</param>
/// <param name="Patch">The third number.</param>
/// <param name="Prerelease">True when a part such as `-alpha.9.2` follows the three numbers.</param>
public sealed record CodexVersion(int Major, int Minor, int Patch, bool Prerelease)
{
    /// <summary>Tells whether this version is the same as the minimum, or later.</summary>
    /// <param name="minimum">The pinned minimum version.</param>
    /// <returns>True when this version can run the review. A prerelease comes before its release.</returns>
    public bool IsAtLeast(CodexVersion minimum)
    {
        ArgumentNullException.ThrowIfNull(minimum);
        if (this.Major != minimum.Major)
        {
            return this.Major > minimum.Major;
        }

        if (this.Minor != minimum.Minor)
        {
            return this.Minor > minimum.Minor;
        }

        if (this.Patch != minimum.Patch)
        {
            return this.Patch > minimum.Patch;
        }

        return !this.Prerelease || minimum.Prerelease;
    }

    /// <inheritdoc/>
    public override string ToString()
    {
        string text = string.Create(CultureInfo.InvariantCulture, $"{this.Major}.{this.Minor}.{this.Patch}");
        return this.Prerelease ? text + " (prerelease)" : text;
    }
}

/// <summary>
/// The configuration of the Codex CLI for the `codex-review` command (D-926, D-927). The command
/// passes the model, the reasoning effort, the sandbox, and the approval policy on the command
/// line, so no value comes from `~/.codex/config.toml`.
/// </summary>
public static class CodexCli
{
    /// <summary>The npm package of the CLI. The command installs its newest version before each run (D-927).</summary>
    public const string Package = "@openai/codex";

    /// <summary>The model of the review (D-926).</summary>
    public const string Model = "gpt-6-luna";

    /// <summary>The reasoning effort of the review (D-926).</summary>
    public const string ReasoningEffort = "medium";

    /// <summary>
    /// The sandbox of the review. The reviewer needs the network for `gh` and `git push`, the
    /// dotnet caches for the build, and the git folder of the main checkout for a commit (D-926).
    /// </summary>
    public const string ReviewSandbox = "danger-full-access";

    /// <summary>The sandbox of the model probe, which runs no command.</summary>
    public const string ProbeSandbox = "read-only";

    /// <summary>The prompt of the model probe.</summary>
    public const string ProbePrompt = "Reply with the single word OK.";

    /// <summary>The answer that the model probe expects.</summary>
    public const string ProbeAnswer = "OK";

    /// <summary>
    /// The variables that give the CLI an API key. The command removes each one from every
    /// Codex process that it starts, so no review runs at API prices (D-932).
    /// </summary>
    public static readonly IReadOnlyList<string> ApiKeyVariables = ["OPENAI_API_KEY", "CODEX_API_KEY"];

    /// <summary>The arguments that read the login of the CLI. The command changes no login (D-932).</summary>
    public static readonly IReadOnlyList<string> LoginStatusArguments = ["login", "status"];

    /// <summary>The words of `codex login status` for a login through ChatGPT. CLI 0.156.1 writes them to the error stream.</summary>
    public const string ChatGptLoginText = "Logged in using ChatGPT";

    /// <summary>
    /// The oldest version that ran the model probe on 2026-09-23. The Homebrew formula stays at
    /// 0.39.0, which knows no current model (D-927).
    /// </summary>
    public static readonly CodexVersion MinimumVersion = new CodexVersion(0, 156, 1, false);

    /// <summary>Reads the version from the output of `codex --version`, such as `codex-cli 0.156.1`.</summary>
    /// <param name="text">The output of the command.</param>
    /// <returns>The version.</returns>
    /// <exception cref="InvalidOperationException">The text holds no version of three numbers (T-2).</exception>
    public static CodexVersion ParseVersion(string text)
    {
        ArgumentNullException.ThrowIfNull(text);
        string[] words = text.Trim().Split((char[]?)null, StringSplitOptions.RemoveEmptyEntries);
        if (words.Length == 0)
        {
            throw new InvalidOperationException("`codex --version` printed no text, so the version is unknown (T-2).");
        }

        string word = words[^1];
        int dash = word.IndexOf('-', StringComparison.Ordinal);
        string numbers = dash < 0 ? word : word[..dash];
        string[] parts = numbers.Split('.');
        if (parts.Length != 3
            || !TryReadNumber(parts[0], out int major)
            || !TryReadNumber(parts[1], out int minor)
            || !TryReadNumber(parts[2], out int patch))
        {
            throw new InvalidOperationException(
                $"`codex --version` printed '{text.Trim()}', which holds no version of three numbers (T-2).");
        }

        return new CodexVersion(major, minor, patch, dash >= 0);
    }

    /// <summary>Tells whether the output of `codex login status` names a login through ChatGPT (D-932).</summary>
    /// <param name="result">The result of the command. The check reads both streams.</param>
    /// <returns>True for an exit code of 0 and the words of a ChatGPT login. An API key login gives false.</returns>
    public static bool IsChatGptLogin(ProgramResult result)
    {
        ArgumentNullException.ThrowIfNull(result);
        string text = result.Output + "\n" + result.Error;
        return result.ExitCode == 0 && text.Contains(ChatGptLoginText, StringComparison.Ordinal);
    }

    /// <summary>Gives the arguments of the model probe.</summary>
    /// <param name="folder">The empty folder in which the probe runs.</param>
    /// <param name="lastMessageFile">The file that takes the answer of the model.</param>
    /// <returns>The arguments after the program name.</returns>
    public static IReadOnlyList<string> ProbeArguments(string folder, string lastMessageFile)
    {
        ArgumentException.ThrowIfNullOrEmpty(folder);
        ArgumentException.ThrowIfNullOrEmpty(lastMessageFile);

        List<string> arguments = ExecArguments(ProbeSandbox, folder, lastMessageFile);
        arguments.Add("--skip-git-repo-check");
        arguments.Add("--ephemeral");
        arguments.Add(ProbePrompt);
        return arguments;
    }

    /// <summary>Gives the arguments of the review run.</summary>
    /// <param name="worktree">The worktree at the head of the pull request.</param>
    /// <param name="lastMessageFile">The file that takes the last message of the reviewer.</param>
    /// <param name="prompt">The prompt of the review.</param>
    /// <returns>The arguments after the program name. The events go to the standard output as JSON lines.</returns>
    public static IReadOnlyList<string> ReviewArguments(string worktree, string lastMessageFile, string prompt)
    {
        ArgumentException.ThrowIfNullOrEmpty(worktree);
        ArgumentException.ThrowIfNullOrEmpty(lastMessageFile);
        ArgumentException.ThrowIfNullOrEmpty(prompt);

        List<string> arguments = ExecArguments(ReviewSandbox, worktree, lastMessageFile);
        arguments.Add("--json");
        arguments.Add(prompt);
        return arguments;
    }

    /// <summary>
    /// Gives the prompt of the review. It is the request that the owner typed in the desktop
    /// app, and two lines that tell the reviewer the skill and the push (D-926). A run with the
    /// flag of D-946 adds a line that tells the reviewer that no complete Gitar pass is a condition.
    /// </summary>
    /// <param name="number">The GitHub number of the pull request.</param>
    /// <param name="branch">The branch of the pull request on GitHub.</param>
    /// <param name="localBranch">The local branch of the worktree, which tracks the branch on GitHub.</param>
    /// <param name="skipGitarReview">True when the command line holds `--skip-gitar-review`.</param>
    /// <returns>The prompt text.</returns>
    public static string ReviewPrompt(int number, string branch, string localBranch, bool skipGitarReview)
    {
        ArgumentException.ThrowIfNullOrEmpty(branch);
        ArgumentException.ThrowIfNullOrEmpty(localBranch);

        string text = number.ToString(CultureInfo.InvariantCulture);
        string gitar = skipGitarReview ? SkippedGitarLine : string.Empty;
        return
            $"Review PR #{text}.\n\n" +
            "Load and follow `.claude/skills/pr-review/SKILL.md`. " +
            $"The `codex-review` command started this review in a separate worktree, on the local branch `{localBranch}`, which tracks `origin/{branch}`.\n\n" +
            gitar +
            "Commit the review record and your handoff entry as one metadata commit. " +
            $"Push it with `git push origin HEAD:{branch}`.";
    }

    /// <summary>
    /// The prompt line of a run with `--skip-gitar-review` (D-946). A Gitar comment that exists
    /// still needs its answer, so the line lifts the condition of a complete pass alone.
    /// </summary>
    public const string SkippedGitarLine =
        "The author started this review with `--skip-gitar-review` (D-946). A complete or current Gitar pass is not a condition of this review. " +
        "Each Gitar comment that exists still needs its answer.\n\n";

    private static List<string> ExecArguments(string sandbox, string folder, string lastMessageFile)
    {
        return
        [
            "exec",
            "-m",
            Model,
            "-c",
            $"model_reasoning_effort=\"{ReasoningEffort}\"",
            "-c",
            "approval_policy=\"never\"",
            "-s",
            sandbox,
            "-C",
            folder,
            "-o",
            lastMessageFile,
        ];
    }

    private static bool TryReadNumber(string text, out int value)
    {
        return int.TryParse(text, NumberStyles.None, CultureInfo.InvariantCulture, out value);
    }
}
