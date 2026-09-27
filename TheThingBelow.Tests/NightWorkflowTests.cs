using System;
using System.IO;
using System.Linq;
using System.Text.RegularExpressions;
using TheThingBelow.Tools.Night;
using TheThingBelow.Tools.Notify;
using Xunit;

namespace TheThingBelow.Tests;

/// <summary>
/// The workflows of the night and of the night gate. Neither can run on the PR that creates it,
/// so these tests read the committed workflow text (F-37, D-500).
/// </summary>
public sealed class NightWorkflowTests
{
    private const string NightPath = ".github/workflows/night.yml";

    private const string GatePath = ".github/workflows/night-gate.yml";

    private const string NotifyPath = ".github/workflows/notify.yml";

    private const string FactsPath = ".github/actions/night-facts/action.yml";

    private const string PromotePath = ".github/workflows/night-promote.yml";

    [Fact]
    public void TheNightStartsAt0417Utc()
    {
        // D-1189: a minute away from the start of the hour (F-41).
        Assert.Contains("    - cron: \"17 4 * * *\"", Lines(NightPath));
    }

    [Fact]
    public void TheNightStartsByHandToo()
    {
        // D-510: a session starts a night on the branch of a PR.
        Assert.Contains("  workflow_dispatch:", Lines(NightPath));
    }

    [Fact]
    public void TheMatrixOfTheNightNamesTheLegsOfTools()
    {
        string line = Lines(NightPath).Single(line => line.TrimStart().StartsWith("leg: [", StringComparison.Ordinal));
        string[] legs = line.Trim()["leg: [".Length..^1].Split(',').Select(leg => leg.Trim()).Order(StringComparer.Ordinal).ToArray();

        Assert.Equal(NightLegs.Labels, legs);
    }

    [Fact]
    public void TheFirstSeedOfANightIsItsRunNumberTimesTheSeedStepOfTools()
    {
        // D-1190.
        Assert.Contains($"first_seed=$(( RUN_NUMBER * {NightLegs.SeedStep} ))", Text(NightPath), StringComparison.Ordinal);
    }

    [Fact]
    public void EachLegPlaysTheCommitOfItsRunAndUploadsItsRecord()
    {
        string text = Text(NightPath);

        Assert.Contains("--commit \"$GITHUB_SHA\"", text, StringComparison.Ordinal);
        Assert.Contains($"name: {NightLegs.RecordPrefix}${{{{ matrix.leg }}}}", text, StringComparison.Ordinal);
        Assert.Contains($"path: artifacts/night/{NightLegs.RecordPrefix}${{{{ matrix.leg }}}}.json", text, StringComparison.Ordinal);
    }

    [Fact]
    public void TheGateRunsFromMainAndNeverReadsTheHeadOfThePr()
    {
        // D-509: the workflow and the command come from `main`, and the job reads the nights
        // through the GitHub API. No step checks out or fetches the head of the PR.
        string[] lines = Lines(GatePath);

        Assert.Contains("  pull_request_target:", lines);
        Assert.DoesNotContain(lines, line => Regex.IsMatch(line, @"^\s+ref:"));
        Assert.DoesNotContain(lines, line => line.Contains("git fetch", StringComparison.Ordinal) || line.Contains("refs/pull", StringComparison.Ordinal));
        Assert.Contains("uses: ./.github/actions/night-facts", Text(GatePath), StringComparison.Ordinal);
        Assert.Contains("actions/workflows/night.yml/runs?", Text(FactsPath), StringComparison.Ordinal);
        Assert.DoesNotContain(Lines(FactsPath), line => line.Contains("git fetch", StringComparison.Ordinal) || line.Contains("actions/checkout", StringComparison.Ordinal));
    }

    [Fact]
    public void TheTokenOfTheGateWritesNothing()
    {
        string[] lines = Lines(GatePath);
        int start = Array.IndexOf(lines, "permissions:");
        string[] block = lines[(start + 1)..].TakeWhile(line => line.StartsWith("  ", StringComparison.Ordinal)).ToArray();

        Assert.Equal(["  contents: read", "  actions: read", "  pull-requests: read"], block);
    }

    [Fact]
    public void TheGateReadsTheOldPathOfEachRename()
    {
        // F-109: a rename lists its old path too, so a move out of the code paths binds the PR.
        Assert.Contains("(.previous_filename // empty)", Text(GatePath), StringComparison.Ordinal);
    }

    [Fact]
    public void TheAlertRunsAfterAFailedLegAlone()
    {
        // D-1201: one message when a leg fails, and none for a green night or a cancel.
        string[] lines = Lines(NightPath);
        int start = Array.IndexOf(lines, "  alert:");

        Assert.True(start > 0, "The night workflow holds no alert job.");
        Assert.Contains("    needs: night", lines[start..]);
        Assert.Contains("    if: ${{ !cancelled() && needs.night.result == 'failure' }}", lines[start..]);
        Assert.Contains(lines[start..], line => line.Contains(NightAlertCommand.Name, StringComparison.Ordinal));
    }

    [Fact]
    public void TheFirstSeedOfTheNightReachesTheAlert()
    {
        string text = Text(NightPath);

        Assert.Contains("first-seed: ${{ steps.seed.outputs.first-seed }}", text, StringComparison.Ordinal);
        Assert.Contains("echo \"first-seed=$first_seed\" >> \"$GITHUB_OUTPUT\"", text, StringComparison.Ordinal);
        Assert.Contains("FIRST_SEED: ${{ needs.night.outputs.first-seed }}", text, StringComparison.Ordinal);
    }

    [Theory]
    [InlineData(NightPath)]
    [InlineData(NotifyPath)]
    public void EachSecretReachesTheCommandThroughItsEnvironmentAlone(string path)
    {
        // D-1201: no step writes a secret, and no script text holds one.
        string[] lines = Lines(path).Where(line => line.Contains("secrets.", StringComparison.Ordinal)).ToArray();

        Assert.Equal(2, lines.Length);
        Assert.All(lines, line => Assert.Matches(@"^          (PUSHOVER_USER_KEY|PUSHOVER_API_TOKEN): \$\{\{ secrets\.\1 \}\}$", line));
    }

    [Fact]
    public void EachInputOfTheNotifyWorkflowGoesThroughTheEnvironment()
    {
        // D-1207: an input never reaches the text of a script.
        string[] lines = Lines(NotifyPath).Where(line => line.Contains("${{ inputs.", StringComparison.Ordinal)).ToArray();

        Assert.Equal(3, lines.Length);
        Assert.All(lines, line => Assert.Matches(@"^          [A-Z]+: \$\{\{ inputs\.[a-z]+ \}\}$", line));
        Assert.Contains(Lines(NotifyPath), line => line.Contains(PushoverCommand.Name + " --title", StringComparison.Ordinal));
    }

    [Fact]
    public void ThePromotionRunsOnEachPushToMainWithAReadToken()
    {
        // D-1202: the job writes nothing through the API, and the upload takes the runtime token.
        string[] lines = Lines(PromotePath);
        int start = Array.IndexOf(lines, "permissions:");
        string[] block = lines[(start + 1)..].TakeWhile(line => line.StartsWith("  ", StringComparison.Ordinal)).ToArray();

        Assert.Contains("  push:", lines);
        Assert.Contains("    branches: [main]", lines);
        Assert.Equal(["  contents: read", "  actions: read", "  pull-requests: read"], block);
    }

    [Fact]
    public void ThePromotionComparesTreesAndKeepsItsArtifactOnAPromotionAlone()
    {
        // D-1202: a squash merge makes a new commit, so the job compares trees. A rename lists
        // both paths. The job fetches the commits of the PR as data alone.
        string text = Text(PromotePath);

        Assert.Contains("git fetch --no-tags origin \"refs/pull/$NUMBER/head\"", text, StringComparison.Ordinal);
        Assert.Contains("git diff --name-only --no-renames \"$night_commit\" \"$GITHUB_SHA\"", text, StringComparison.Ordinal);
        Assert.Contains("if: steps.promote.outputs.promoted == 'true'", text, StringComparison.Ordinal);
        Assert.Contains($"name: {NightPromotion.ArtifactName}", text, StringComparison.Ordinal);
        Assert.Contains(NightPromoteCommand.Name + " --merge-commit", text, StringComparison.Ordinal);
        Assert.DoesNotContain("dotnet run --project TheThingBelow.Tools/TheThingBelow.Tools.csproj -- bots", text, StringComparison.Ordinal);
    }

    [Fact]
    public void EachPushKeepsItsPromotionRunInTheOrderOfThePushes()
    {
        // P1-1 of the review of PR #89: a concurrency group keeps one pending run, and a new run
        // cancels it, so a push lost its promotion. Each run now waits for each earlier run.
        string[] lines = Lines(PromotePath);
        int wait = Array.FindIndex(lines, line => line.Contains("- name: Wait for each earlier promotion run", StringComparison.Ordinal));
        int facts = Array.FindIndex(lines, line => line.Contains("uses: ./.github/actions/night-facts", StringComparison.Ordinal));

        Assert.DoesNotContain("concurrency:", lines);
        Assert.True(wait > 0 && wait < facts, "The wait for the earlier runs comes before the read of the facts.");
        Assert.Contains("select(.status != \\\"completed\\\" and .id < $RUN_ID)", Text(PromotePath), StringComparison.Ordinal);
    }

    [Fact]
    public void TheFactsReadAPromotionOfAPushRunOfThePromoteWorkflowAlone()
    {
        // D-1202: an artifact of the same name from another workflow or branch takes no part.
        string text = Text(FactsPath);

        Assert.Contains($"artifacts?name={NightPromotion.ArtifactName}", text, StringComparison.Ordinal);
        Assert.Contains(".workflow_run.head_branch == \"main\"", text, StringComparison.Ordinal);
        Assert.Contains("\".github/workflows/night-promote.yml push\"", text, StringComparison.Ordinal);
        Assert.Contains("compare/$night_commit...$merge_commit", text, StringComparison.Ordinal);
        Assert.Contains(NightWalkCommand.Name + " --commits", text, StringComparison.Ordinal);
    }

    private static string Text(string path) => File.ReadAllText(RepositoryRoot.PathTo(path));

    private static string[] Lines(string path) => File.ReadAllLines(RepositoryRoot.PathTo(path));
}
