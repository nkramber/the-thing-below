using System;
using System.Collections.Generic;
using System.IO;
using System.Text.Json;

namespace TheThingBelow.Tools.Night;

/// <summary>
/// The facts of one run of the night workflow, as the gate job reads them from the GitHub API
/// (D-509). The job writes them to the file `run.json` of the folder of that night.
/// </summary>
/// <param name="Id">The id of the workflow run.</param>
/// <param name="Commit">The full hash of the commit that the run played.</param>
/// <param name="Branch">The branch of the run: `main` for a scheduled night.</param>
/// <param name="Conclusion">The conclusion of the run, such as `success` or `failure`.</param>
/// <param name="Started">The time when the run started, in UTC.</param>
public sealed record NightRun(long Id, string Commit, string Branch, string Conclusion, DateTimeOffset Started);

/// <summary>
/// One night as the gate job downloads it: the facts of the run and the night record of each leg.
/// The job never reads a record from the checkout of the PR (D-509).
/// </summary>
/// <param name="Run">The facts of the run.</param>
/// <param name="Records">The night record of each leg that has one, in the order of <see cref="NightLegs.Labels"/>.</param>
/// <param name="AbsentLegs">Each leg with no night record.</param>
public sealed record NightEvidence(NightRun Run, IReadOnlyList<NightRecord> Records, IReadOnlyList<string> AbsentLegs)
{
    /// <summary>The name of the file of the facts of the run, inside the folder of a night.</summary>
    public const string RunFile = "run.json";

    private static readonly string[] RunFields = ["id", "commit", "branch", "conclusion", "started"];

    /// <summary>
    /// Reads the folder of one night. The gate job writes the file of the facts of the run, and
    /// `gh run download` writes each artifact into a folder of its own name.
    /// </summary>
    /// <param name="folder">The folder of the night.</param>
    /// <returns>The night, or null when the folder holds no file of the facts, because no such night exists.</returns>
    /// <exception cref="DirectoryNotFoundException">The folder does not exist: the job wrote no folder (T-2).</exception>
    /// <exception cref="InvalidOperationException">A file of the folder holds no valid facts or no valid record.</exception>
    public static NightEvidence? Read(string folder)
    {
        ArgumentException.ThrowIfNullOrEmpty(folder);
        if (!Directory.Exists(folder))
        {
            throw new DirectoryNotFoundException($"The folder of the night '{folder}' does not exist. The gate job writes it, and it can hold no night (T-2).");
        }

        string runPath = Path.Combine(folder, RunFile);
        if (!File.Exists(runPath))
        {
            return null;
        }

        List<NightRecord> records = [];
        List<string> absent = [];
        foreach (string leg in NightLegs.Labels)
        {
            string path = Path.Combine(folder, NightLegs.ArtifactOf(leg), NightLegs.FileOf(leg));
            if (File.Exists(path))
            {
                NightRecord record = NightRecord.Read(path);
                if (!string.Equals(record.Leg, leg, StringComparison.Ordinal))
                {
                    throw new InvalidOperationException($"The night record '{path}' names the leg '{record.Leg}', and its artifact belongs to the leg '{leg}'.");
                }

                records.Add(record);
            }
            else
            {
                absent.Add(leg);
            }
        }

        return new NightEvidence(RunOf(runPath), records, absent);
    }

    private static NightRun RunOf(string path)
    {
        string where = $"the facts of the night run '{path}'";
        using JsonDocument document = NightJson.Parse(path, where);
        JsonElement root = document.RootElement;
        NightJson.RequireFields(root, where, RunFields);
        string commit = NightJson.Text(root, "commit", where);
        if (!NightJson.IsCommit(commit))
        {
            throw new InvalidOperationException($"The field 'commit' of {where} holds '{commit}', and the night gate needs a full commit hash.");
        }

        return new NightRun(
            NightJson.Whole(root, "id", where, 1),
            commit,
            NightJson.Text(root, "branch", where),
            NightJson.Text(root, "conclusion", where),
            NightJson.Time(root, "started", where));
    }
}
