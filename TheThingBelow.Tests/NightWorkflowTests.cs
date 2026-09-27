using System;
using System.IO;
using System.Linq;
using System.Text.RegularExpressions;
using TheThingBelow.Tools.Night;
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
        Assert.Contains(lines, line => line.Contains("actions/workflows/night.yml/runs?", StringComparison.Ordinal));
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

    private static string Text(string path) => File.ReadAllText(RepositoryRoot.PathTo(path));

    private static string[] Lines(string path) => File.ReadAllLines(RepositoryRoot.PathTo(path));
}
