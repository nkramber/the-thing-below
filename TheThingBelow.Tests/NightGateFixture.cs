using System;
using System.Collections.Generic;
using System.IO;
using System.Text.Json;
using TheThingBelow.Tools;
using TheThingBelow.Tools.Bots;
using TheThingBelow.Tools.Night;

namespace TheThingBelow.Tests;

/// <summary>
/// A fixture of the night gate in a temporary folder: the facts of a PR and the folders of the
/// nights, in the form that the gate job writes. The gate cannot run live on the PR that creates
/// it, so these fixtures are the proof of the command (F-37, D-500).
/// </summary>
public sealed class NightGateFixture : IDisposable
{
    /// <summary>The head commit of the fixture PR.</summary>
    public const string Head = "a1b2c3d4e5f60718293a4b5c6d7e8f9012345678";

    /// <summary>The commit of `main` that the fixture nights play.</summary>
    public const string MainCommit = "d4e5f60718293a4b5c6d7e8f90123456789ab2c3";

    /// <summary>The first seed of the fixture nights: run 7 of D-1190.</summary>
    public const ulong FirstSeed = 7 * NightLegs.SeedStep;

    /// <summary>The time of the run of the fixture gate.</summary>
    public static readonly DateTimeOffset Now = new(2026, 9, 28, 9, 0, 0, TimeSpan.Zero);

    /// <summary>The start of the night of that day, at the time of D-1189.</summary>
    public static readonly DateTimeOffset LastNight = new(2026, 9, 28, 4, 17, 43, TimeSpan.Zero);

    private static readonly JsonWriterOptions WriteOptions = new() { Indented = true };

    private NightGateFixture(string root)
    {
        this.Root = root;
        this.PullRequestPath = Path.Combine(root, "pull-request.json");
        this.MainNight = Path.Combine(root, "main-night");
        this.HeadNight = Path.Combine(root, "head-night");
        Directory.CreateDirectory(this.MainNight);
        Directory.CreateDirectory(this.HeadNight);
    }

    /// <summary>Gets the root folder of the fixture.</summary>
    public string Root { get; }

    /// <summary>Gets the path of the facts of the PR.</summary>
    public string PullRequestPath { get; }

    /// <summary>Gets the folder of the newest night on `main`.</summary>
    public string MainNight { get; }

    /// <summary>Gets the folder of the newest night on the head commit.</summary>
    public string HeadNight { get; }

    /// <summary>Makes a fixture of a PR that changes the given paths, with no night.</summary>
    /// <param name="files">The changed paths of the PR.</param>
    /// <returns>The fixture.</returns>
    public static NightGateFixture Create(params string[] files)
    {
        NightGateFixture fixture = new(Path.Combine(Path.GetTempPath(), "night-gate-" + Guid.NewGuid().ToString("N")));
        using FileStream stream = File.Create(fixture.PullRequestPath);
        using Utf8JsonWriter writer = new(stream, WriteOptions);
        writer.WriteStartObject();
        writer.WriteString("head", Head);
        writer.WriteStartArray("files");
        foreach (string file in files)
        {
            writer.WriteStringValue(file);
        }

        writer.WriteEndArray();
        writer.WriteEndObject();
        return fixture;
    }

    /// <summary>Makes the record of one leg with no softlock and no crash.</summary>
    /// <param name="commit">The commit of the night.</param>
    /// <param name="leg">The runner label of the leg.</param>
    /// <returns>The record.</returns>
    public static NightRecord GreenRecord(string commit, string leg) => new(
        commit,
        leg,
        FirstSeed,
        [new NightPolicyCounts(BotPolicyKind.Greedy, 30, 28, 0, 0, 2, 11), new NightPolicyCounts(BotPolicyKind.Random, 20, 1, 0, 0, 19, 9)]);

    /// <summary>Makes the record of one leg with softlocks and a crash in the random runs.</summary>
    /// <param name="commit">The commit of the night.</param>
    /// <param name="leg">The runner label of the leg.</param>
    /// <returns>The record.</returns>
    public static NightRecord RedRecord(string commit, string leg) => new(
        commit,
        leg,
        FirstSeed,
        [new NightPolicyCounts(BotPolicyKind.Greedy, 30, 28, 0, 0, 2, 11), new NightPolicyCounts(BotPolicyKind.Random, 20, 1, 2, 1, 16, 9)]);

    /// <summary>Writes the facts of a night run into the folder of a night.</summary>
    /// <param name="folder">The folder of the night.</param>
    /// <param name="id">The id of the run.</param>
    /// <param name="commit">The commit of the run.</param>
    /// <param name="branch">The branch of the run.</param>
    /// <param name="conclusion">The conclusion of the run.</param>
    /// <param name="started">The start of the run.</param>
    public static void WriteRun(string folder, long id, string commit, string branch, string conclusion, DateTimeOffset started)
    {
        using FileStream stream = File.Create(Path.Combine(folder, NightEvidence.RunFile));
        using Utf8JsonWriter writer = new(stream, WriteOptions);
        writer.WriteStartObject();
        writer.WriteNumber("id", id);
        writer.WriteString("commit", commit);
        writer.WriteString("branch", branch);
        writer.WriteString("conclusion", conclusion);
        writer.WriteString("started", NightJson.TextOf(started));
        writer.WriteEndObject();
    }

    /// <summary>Writes a night record where `gh run download` puts its artifact.</summary>
    /// <param name="folder">The folder of the night.</param>
    /// <param name="record">The record.</param>
    public static void WriteRecord(string folder, NightRecord record)
    {
        string artifact = Path.Combine(folder, NightLegs.ArtifactOf(record.Leg));
        Directory.CreateDirectory(artifact);
        record.Write(Path.Combine(artifact, NightLegs.FileOf(record.Leg)));
    }

    /// <summary>Writes a night that succeeded on each leg.</summary>
    /// <param name="folder">The folder of the night.</param>
    /// <param name="commit">The commit of the night.</param>
    /// <param name="branch">The branch of the night.</param>
    /// <param name="started">The start of the night.</param>
    public static void WriteGreenNight(string folder, string commit, string branch, DateTimeOffset started)
    {
        WriteRun(folder, 101, commit, branch, NightGate.SuccessConclusion, started);
        foreach (string leg in NightLegs.Labels)
        {
            WriteRecord(folder, GreenRecord(commit, leg));
        }
    }

    /// <summary>Runs the `night-gate` command on the fixture at <see cref="Now"/>.</summary>
    /// <param name="output">The output of the command.</param>
    /// <param name="errors">The error lines of the command.</param>
    /// <returns>The exit code.</returns>
    public int Run(out string output, out string errors) => this.RunAt(NightJson.TextOf(Now), out output, out errors);

    /// <summary>Runs the `night-gate` command on the fixture at a given time.</summary>
    /// <param name="now">The value of the option of the time.</param>
    /// <param name="output">The output of the command.</param>
    /// <param name="errors">The error lines of the command.</param>
    /// <returns>The exit code.</returns>
    public int RunAt(string now, out string output, out string errors)
    {
        using StringWriter outputWriter = new();
        using StringWriter errorWriter = new();
        int exitCode = Program.Run(
            [NightGateCommand.Name, "--pull-request", this.PullRequestPath, "--main-night", this.MainNight, "--head-night", this.HeadNight, "--now", now],
            outputWriter,
            errorWriter);
        output = outputWriter.ToString();
        errors = errorWriter.ToString();
        return exitCode;
    }

    /// <inheritdoc/>
    public void Dispose()
    {
        if (Directory.Exists(this.Root))
        {
            Directory.Delete(this.Root, recursive: true);
        }
    }
}
