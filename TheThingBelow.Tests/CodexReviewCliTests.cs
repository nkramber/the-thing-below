using System;
using System.Collections.Generic;
using TheThingBelow.Tools.CodexReview;
using Xunit;

namespace TheThingBelow.Tests;

/// <summary>
/// The configuration of the Codex CLI (D-926, D-927). The command passes each value on the
/// command line, so the effort of `~/.codex/config.toml` never reaches a review.
/// </summary>
public sealed class CodexReviewCliTests
{
    [Fact]
    public void TheReviewUsesTheModelAndTheEffortOfTheOwner()
    {
        Assert.Equal("gpt-6-luna", CodexCli.Model);
        Assert.Equal("medium", CodexCli.ReasoningEffort);
        Assert.Equal("danger-full-access", CodexCli.ReviewSandbox);
        Assert.Equal("@openai/codex", CodexCli.Package);
    }

    [Fact]
    public void EachApiKeyVariableLeavesTheCodexProcesses()
    {
        Assert.Equal(["OPENAI_API_KEY", "CODEX_API_KEY"], CodexCli.ApiKeyVariables);
        Assert.Equal(["login", "status"], CodexCli.LoginStatusArguments);
    }

    /// <summary>CLI 0.156.1 writes the login to the error stream, so the check reads both streams (D-932).</summary>
    [Theory]
    [InlineData(0, "", "Logged in using ChatGPT", true)]
    [InlineData(0, "Logged in using ChatGPT\n", "", true)]
    [InlineData(0, "", "Logged in using an API key - sk-proj-***", false)]
    [InlineData(1, "", "Not logged in", false)]
    [InlineData(1, "", "Logged in using ChatGPT", false)]
    public void OnlyAChatGptLoginPasses(int exitCode, string output, string error, bool accepted)
    {
        Assert.Equal(accepted, CodexCli.IsChatGptLogin(new ProgramResult(exitCode, output, error)));
    }

    [Fact]
    public void NoArgumentsChangeTheLogin()
    {
        IReadOnlyList<string> review = CodexCli.ReviewArguments("/tmp/review", "/tmp/last.md", "Review PR #63.");
        IReadOnlyList<string> probe = CodexCli.ProbeArguments("/tmp/probe", "/tmp/probe/answer.txt");

        // A forced login method that differs from the login makes the CLI log out (D-932).
        Assert.DoesNotContain(review, argument => argument.Contains("forced_login_method", StringComparison.Ordinal));
        Assert.DoesNotContain(probe, argument => argument.Contains("forced_login_method", StringComparison.Ordinal));
    }

    [Fact]
    public void TheReviewArgumentsPassEachValueOnTheCommandLine()
    {
        IReadOnlyList<string> arguments = CodexCli.ReviewArguments("/tmp/review", "/tmp/last.md", "Review PR #63.");

        Assert.Equal("exec", arguments[0]);
        AssertPair(arguments, "-m", "gpt-6-luna");
        AssertPair(arguments, "-s", "danger-full-access");
        AssertPair(arguments, "-C", "/tmp/review");
        AssertPair(arguments, "-o", "/tmp/last.md");
        Assert.Contains("model_reasoning_effort=\"medium\"", arguments);
        Assert.Contains("approval_policy=\"never\"", arguments);
        Assert.Contains("--json", arguments);
        Assert.Equal("Review PR #63.", arguments[^1]);
    }

    [Fact]
    public void TheProbeReadsNothingAndKeepsNoSession()
    {
        IReadOnlyList<string> arguments = CodexCli.ProbeArguments("/tmp/probe", "/tmp/probe/answer.txt");

        AssertPair(arguments, "-m", "gpt-6-luna");
        AssertPair(arguments, "-s", "read-only");
        Assert.Contains("model_reasoning_effort=\"medium\"", arguments);
        Assert.Contains("--skip-git-repo-check", arguments);
        Assert.Contains("--ephemeral", arguments);
        Assert.Equal(CodexCli.ProbePrompt, arguments[^1]);
    }

    [Fact]
    public void ThePromptIsTheRequestOfTheOwnerWithTheSkillAndThePush()
    {
        string prompt = CodexCli.ReviewPrompt(63, "feat/pr-95-codex-review", "review/pr-63", false);

        Assert.StartsWith("Review PR #63.\n", prompt, StringComparison.Ordinal);
        Assert.Contains("`.claude/skills/pr-review/SKILL.md`", prompt, StringComparison.Ordinal);
        Assert.Contains("git push origin HEAD:feat/pr-95-codex-review", prompt, StringComparison.Ordinal);
        Assert.Contains("one metadata commit", prompt, StringComparison.Ordinal);
        Assert.DoesNotContain("--skip-gitar-review", prompt, StringComparison.Ordinal);
        Assert.Contains(CodexCli.ItemlessGitarLine, prompt, StringComparison.Ordinal);
    }

    /// <summary>Every run tells the reviewer to ignore a Gitar comment with no item, with the flag or without it (D-964).</summary>
    [Theory]
    [InlineData(false)]
    [InlineData(true)]
    public void EveryPromptIgnoresAGitarCommentWithNoItem(bool skipGitarReview)
    {
        string prompt = CodexCli.ReviewPrompt(68, "feat/pr-98-waiting-enemies", "review/pr-68", skipGitarReview);

        Assert.Contains("Ignore a Gitar comment with no item", prompt, StringComparison.Ordinal);
        Assert.Contains("never blocks the verdict (D-964)", prompt, StringComparison.Ordinal);
        Assert.DoesNotContain("Each Gitar comment that exists still needs its answer", prompt, StringComparison.Ordinal);
    }

    /// <summary>A run with the flag tells the reviewer that no complete Gitar pass is a condition (D-946).</summary>
    [Fact]
    public void ThePromptOfASkippedGitarPassSaysSo()
    {
        string prompt = CodexCli.ReviewPrompt(66, "feat/pr-97-gitar-pause", "review/pr-66", true);

        Assert.Contains(CodexCli.SkippedGitarLine, prompt, StringComparison.Ordinal);
        Assert.Contains("`--skip-gitar-review` (D-946)", prompt, StringComparison.Ordinal);
        Assert.Contains("each Gitar item needs its answer", prompt, StringComparison.Ordinal);
        Assert.EndsWith("git push origin HEAD:feat/pr-97-gitar-pause`.", prompt, StringComparison.Ordinal);
    }

    [Theory]
    [InlineData("codex-cli 0.156.1", 0, 156, 1, false)]
    [InlineData("codex-cli 0.155.0-alpha.9.2", 0, 155, 0, true)]
    [InlineData("1.2.3\n", 1, 2, 3, false)]
    public void TheVersionLineParses(string text, int major, int minor, int patch, bool prerelease)
    {
        Assert.Equal(new CodexVersion(major, minor, patch, prerelease), CodexCli.ParseVersion(text));
    }

    [Theory]
    [InlineData("")]
    [InlineData("codex-cli")]
    [InlineData("codex-cli 0.156")]
    [InlineData("codex-cli 0.x.1")]
    public void AVersionLineWithNoVersionIsAFault(string text)
    {
        Assert.Throws<InvalidOperationException>(() => CodexCli.ParseVersion(text));
    }

    [Theory]
    [InlineData("codex-cli 0.39.0", false)]
    [InlineData("codex-cli 0.155.0-alpha.9.2", false)]
    [InlineData("codex-cli 0.156.1-alpha.1", false)]
    [InlineData("codex-cli 0.156.1", true)]
    [InlineData("codex-cli 0.157.0-alpha.1", true)]
    [InlineData("codex-cli 1.0.0", true)]
    public void TheMinimumVersionRefusesAnOlderCli(string text, bool accepted)
    {
        Assert.Equal(accepted, CodexCli.ParseVersion(text).IsAtLeast(CodexCli.MinimumVersion));
    }

    [Theory]
    [InlineData("abc")]
    [InlineData("0")]
    [InlineData("-3")]
    public void APullRequestNumberThatIsNotPositiveIsRefused(string number)
    {
        using System.IO.StringWriter output = new System.IO.StringWriter();
        using System.IO.StringWriter errors = new System.IO.StringWriter();

        int exitCode = TheThingBelow.Tools.Program.Run([CodexReviewCommand.Name, "--pull-request", number], output, errors);

        Assert.Equal(TheThingBelow.Tools.Program.FaultExitCode, exitCode);
        Assert.Contains("the GitHub number of the PR", errors.ToString(), StringComparison.Ordinal);
    }

    [Fact]
    public void ARunWithNoPullRequestIsRefused()
    {
        using System.IO.StringWriter output = new System.IO.StringWriter();
        using System.IO.StringWriter errors = new System.IO.StringWriter();

        int exitCode = TheThingBelow.Tools.Program.Run([CodexReviewCommand.Name], output, errors);

        Assert.Equal(TheThingBelow.Tools.Program.FaultExitCode, exitCode);
        Assert.Contains("--pull-request <number>", errors.ToString(), StringComparison.Ordinal);
    }

    private static void AssertPair(IReadOnlyList<string> arguments, string option, string value)
    {
        int index = -1;
        for (int position = 0; position < arguments.Count; position++)
        {
            if (arguments[position] == option)
            {
                Assert.Equal(-1, index);
                index = position;
            }
        }

        Assert.True(index >= 0 && index + 1 < arguments.Count, $"The arguments hold no option {option}.");
        Assert.Equal(value, arguments[index + 1]);
    }
}
