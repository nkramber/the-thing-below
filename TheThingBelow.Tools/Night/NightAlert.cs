using System;
using System.Collections.Generic;
using System.Globalization;
using System.Text.Json;
using TheThingBelow.Tools.Notify;

namespace TheThingBelow.Tools.Night;

/// <summary>One job of a night run, as the GitHub API lists it: its name and its conclusion.</summary>
/// <param name="Name">The name of the job, such as `night (windows-2025)`.</param>
/// <param name="Conclusion">The conclusion of the job, such as `failure`, or an empty text for a job that runs.</param>
public sealed record NightJob(string Name, string Conclusion);

/// <summary>The facts of a failed night that the alert names (D-1201).</summary>
/// <param name="Jobs">Each job of the night run.</param>
/// <param name="FirstSeed">The first seed of the night, or null when no leg reached the seed step.</param>
/// <param name="Branch">The branch of the night run.</param>
/// <param name="Commit">The full hash of the commit of the night.</param>
/// <param name="RunLink">The link of the page of the night run.</param>
public sealed record NightAlertFacts(IReadOnlyList<NightJob> Jobs, ulong? FirstSeed, string Branch, string Commit, string RunLink);

/// <summary>
/// The Pushover message of a failed night (D-1201). The alert job of the night workflow reads the
/// jobs of its run through the GitHub API and sends this message when a leg fails.
/// </summary>
public static class NightAlert
{
    /// <summary>The start of the name of each leg job of the night workflow: `night (` and the runner label.</summary>
    public const string LegJobPrefix = "night (";

    /// <summary>The conclusion of a job that succeeded, as GitHub writes it.</summary>
    public const string SuccessConclusion = "success";

    /// <summary>The title of each alert.</summary>
    public const string Title = "The Thing Below: the night failed";

    /// <summary>Reads the jobs of a night run from the JSON file that the alert job writes.</summary>
    /// <param name="path">The file: an array of objects with `name` and `conclusion`.</param>
    /// <returns>Each job, in the order of the file.</returns>
    /// <exception cref="InvalidOperationException">The file holds no valid list of jobs (T-2).</exception>
    public static IReadOnlyList<NightJob> ReadJobs(string path)
    {
        ArgumentException.ThrowIfNullOrEmpty(path);

        string where = $"the jobs of the night run '{path}'";
        using JsonDocument document = NightJson.Parse(path, where);
        JsonElement root = document.RootElement;
        if (root.ValueKind != JsonValueKind.Array)
        {
            throw new InvalidOperationException($"{where} holds '{root.ValueKind}', and the alert needs an array of jobs.");
        }

        List<NightJob> jobs = [];
        foreach (JsonElement item in root.EnumerateArray())
        {
            NightJson.RequireFields(item, where, ["name", "conclusion"]);
            string name = NightJson.Text(item, "name", where);
            JsonElement conclusion = item.GetProperty("conclusion");
            string conclusionText = conclusion.ValueKind switch
            {
                JsonValueKind.Null => string.Empty,
                JsonValueKind.String => conclusion.GetString()!,
                _ => throw new InvalidOperationException($"The job '{name}' of {where} holds the conclusion '{conclusion}', and a conclusion is a text or null."),
            };
            jobs.Add(new NightJob(name, conclusionText));
        }

        return jobs;
    }

    /// <summary>Gives the message of a failed night.</summary>
    /// <param name="facts">The facts of the night.</param>
    /// <returns>The message.</returns>
    /// <exception cref="InvalidOperationException">The jobs hold no leg job that failed, so no alert applies (T-2).</exception>
    public static PushoverMessage MessageOf(NightAlertFacts facts)
    {
        ArgumentNullException.ThrowIfNull(facts);

        List<string> failed = [];
        foreach (NightJob job in facts.Jobs)
        {
            if (job.Name.StartsWith(LegJobPrefix, StringComparison.Ordinal)
                && !string.Equals(job.Conclusion, SuccessConclusion, StringComparison.Ordinal))
            {
                string leg = job.Name[LegJobPrefix.Length..].TrimEnd(')');
                string conclusion = job.Conclusion.Length == 0 ? "no conclusion" : job.Conclusion;
                failed.Add($"{leg} ({conclusion})");
            }
        }

        if (failed.Count == 0)
        {
            throw new InvalidOperationException($"The {facts.Jobs.Count.ToString(CultureInfo.InvariantCulture)} jobs of the night run {facts.RunLink} hold no leg job that failed, so the alert has no leg to name (D-1201, T-2).");
        }

        string seed = facts.FirstSeed is ulong first
            ? $"First seed: {first.ToString(CultureInfo.InvariantCulture)}."
            : "First seed: absent, because no leg reached the seed step.";
        string commit = facts.Commit.Length > 12 ? facts.Commit[..12] : facts.Commit;
        string text = $"Failed legs: {string.Join(", ", failed)}.\n{seed}\nBranch: {facts.Branch}. Commit: {commit}.";
        return new PushoverMessage(Title, text, facts.RunLink);
    }
}
