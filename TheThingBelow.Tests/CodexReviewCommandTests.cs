using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using TheThingBelow.Tools;
using TheThingBelow.Tools.CodexReview;
using Xunit;

namespace TheThingBelow.Tests;

/// <summary>
/// The flag `--skip-gitar-review` of the `codex-review` command and of its Makefile target
/// (D-946). The flag skips the check of the Gitar pass, and it is permanent.
/// </summary>
public sealed class CodexReviewCommandTests
{
    [Fact]
    public void TheCommandTakesTheFlag()
    {
        var errors = new StringWriter();

        int code = CodexReviewCommand.Run([CodexReviewCommand.SkipGitarReviewOption], new StringWriter(), errors);

        // The command stops on the absent PR number, after the parser took the flag.
        Assert.Equal(Program.FaultExitCode, code);
        Assert.Contains($"needs {CodexReviewCommand.PullRequestOption}", errors.ToString(), StringComparison.Ordinal);
        Assert.DoesNotContain("is unknown", errors.ToString(), StringComparison.Ordinal);
    }

    [Fact]
    public void TheSkipReadsNoGitarFactAndGivesNoReason()
    {
        var output = new StringWriter();
        bool read = false;

        IReadOnlyList<string> reasons = CodexReviewCommand.GitarReasons(
            true,
            () =>
            {
                read = true;
                throw new InvalidOperationException("The skip must read no Gitar fact (D-946).");
            },
            output);

        Assert.Empty(reasons);
        Assert.False(read);
        Assert.Contains("--skip-gitar-review skips the check of the Gitar pass (D-946).", output.ToString(), StringComparison.Ordinal);
    }

    [Fact]
    public void WithoutTheFlagAnAbsentPassRefusesTheReview()
    {
        var output = new StringWriter();
        GitarFacts noPass = new GitarFacts([], [], [], "2026-09-23T10:00:00Z");

        IReadOnlyList<string> reasons = CodexReviewCommand.GitarReasons(false, () => noPass, output);

        Assert.Contains(reasons, reason => reason.Contains("no Gitar dashboard", StringComparison.Ordinal));
        Assert.Empty(output.ToString());
    }

    /// <summary>
    /// Make reads a word after `--` as a goal. The target passes the goal of the flag to the
    /// command, and the goal of the flag holds the same text as the option of the command.
    /// </summary>
    [Fact]
    public void TheMakefileTargetPassesTheFlagGoalToTheCommand()
    {
        string[] lines = File.ReadAllLines(RepositoryRoot.PathTo("Makefile"));

        Assert.Contains($"SKIP_GITAR_REVIEW := {CodexReviewCommand.SkipGitarReviewOption}", lines);
        Assert.Contains("CODEX_REVIEW_FLAGS := $(filter $(SKIP_GITAR_REVIEW),$(MAKECMDGOALS))", lines);
        Assert.Contains("$(SKIP_GITAR_REVIEW):", lines);

        string command = Assert.Single(lines, line => line.Contains("-- codex-review --root .", StringComparison.Ordinal));
        Assert.EndsWith("--pull-request $(PR) $(CODEX_REVIEW_FLAGS)", command, StringComparison.Ordinal);
        Assert.Contains(lines, line => line.Contains("$(filter-out codex-review $(SKIP_GITAR_REVIEW),$(MAKECMDGOALS))", StringComparison.Ordinal));
        Assert.True(
            lines.Any(line => line.StartsWith(".PHONY:", StringComparison.Ordinal) && line.EndsWith("$(SKIP_GITAR_REVIEW)", StringComparison.Ordinal)),
            "The goal of the flag is a phony goal, so no file of that name stops it (D-946).");
    }
}
