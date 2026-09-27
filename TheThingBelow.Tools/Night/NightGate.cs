using System;
using System.Collections.Generic;
using System.Globalization;
using System.Text.Json;
using TheThingBelow.Tools.Bots;
using TheThingBelow.Tools.ReviewGate;

namespace TheThingBelow.Tools.Night;

/// <summary>
/// The facts of the PR that the night gate reads: the head commit, and each path that the PR
/// changes. The gate job reads both from the GitHub API, never from the checkout (D-513).
/// </summary>
/// <param name="Head">The full hash of the head commit of the PR.</param>
/// <param name="Files">Each path that the PR changes, with the old path of each rename.</param>
public sealed record NightPullRequest(string Head, IReadOnlyList<string> Files)
{
    private static readonly string[] Fields = ["head", "files"];

    /// <summary>Reads the facts from the JSON file that the gate job writes.</summary>
    /// <param name="path">The path of the file.</param>
    /// <returns>The facts.</returns>
    /// <exception cref="InvalidOperationException">The file holds no valid facts (T-2).</exception>
    public static NightPullRequest Read(string path)
    {
        ArgumentException.ThrowIfNullOrEmpty(path);

        string where = $"the facts of the PR '{path}'";
        using JsonDocument document = NightJson.Parse(path, where);
        JsonElement root = document.RootElement;
        NightJson.RequireFields(root, where, Fields);
        string head = NightJson.Text(root, "head", where);
        if (!NightJson.IsCommit(head))
        {
            throw new InvalidOperationException($"The field 'head' of {where} holds '{head}', and the night gate needs a full commit hash.");
        }

        JsonElement list = root.GetProperty("files");
        if (list.ValueKind != JsonValueKind.Array || list.GetArrayLength() == 0)
        {
            throw new InvalidOperationException($"The field 'files' of {where} holds '{list}', and a PR changes at least one path.");
        }

        List<string> files = [];
        foreach (JsonElement item in list.EnumerateArray())
        {
            string? file = item.ValueKind == JsonValueKind.String ? item.GetString() : null;
            files.Add(string.IsNullOrEmpty(file)
                ? throw new InvalidOperationException($"The field 'files' of {where} holds the item '{item}', and each item is a path.")
                : file);
        }

        return new NightPullRequest(head, files);
    }
}

/// <summary>The result of the night gate: whether the PR passes, and one line for each fact that the gate read.</summary>
/// <param name="Passes">True when the PR passes the gate.</param>
/// <param name="Lines">The lines of the result, in the order that the gate read the facts.</param>
public sealed record NightVerdict(bool Passes, IReadOnlyList<string> Lines);

/// <summary>
/// The rules of the night gate (G-22). A PR passes when it is a docs-only PR (D-513), when a
/// night on its head commit succeeded (D-510), or when the newest night on `main` succeeded on
/// each leg. A night counts inside 48 hours of the run of the gate on the last push (D-1188).
/// </summary>
public static class NightGate
{
    /// <summary>The age limit of a night at the run of the gate (G-22, D-1188).</summary>
    public static readonly TimeSpan Limit = TimeSpan.FromHours(48);

    /// <summary>The branch of the scheduled night (F-37).</summary>
    public const string MainBranch = "main";

    /// <summary>The conclusion of a workflow run that succeeded, as GitHub writes it.</summary>
    public const string SuccessConclusion = "success";

    /// <summary>Checks one PR against the nights that the gate job downloaded.</summary>
    /// <param name="pullRequest">The facts of the PR.</param>
    /// <param name="mainNight">The newest completed night on `main`, or null when no night exists there.</param>
    /// <param name="headNight">The newest completed night on the head commit of the PR, or null when no night exists there.</param>
    /// <param name="now">The time of the run of the gate, in UTC.</param>
    /// <returns>The result.</returns>
    public static NightVerdict Check(NightPullRequest pullRequest, NightEvidence? mainNight, NightEvidence? headNight, DateTimeOffset now)
    {
        ArgumentNullException.ThrowIfNull(pullRequest);

        string? code = FirstPathOutsideDocs(pullRequest.Files);
        if (code is null)
        {
            return new NightVerdict(true, ["The PR changes documents alone, and a docs-only PR passes the night gate (D-513)."]);
        }

        List<string> lines = [$"The PR changes the path '{code}', so the night gate binds it (G-22, D-513)."];

        // A failed night on the head commit fails the PR, even when `main` has a green night,
        // because that night played the code of this PR (D-510, T-2). A green head night that is
        // only older than 48 hours proves nothing now, so the gate reads the night of `main` (G-22).
        if (headNight is not null)
        {
            List<string> headFaults = FaultsOf(headNight, pullRequest.Head, null, now);
            string? headStale = StaleOf(headNight.Run, now);
            if (headFaults.Count == 0 && headStale is null)
            {
                lines.Add($"The night run {headNight.Run.Id} on the head commit {pullRequest.Head} succeeded on each leg, and it passes this PR alone (D-510).");
                return new NightVerdict(true, lines);
            }

            if (headFaults.Count > 0)
            {
                lines.Add($"The night run {headNight.Run.Id} on the head commit {pullRequest.Head} does not pass:");
                lines.AddRange(headFaults);
                if (headStale is not null)
                {
                    lines.Add(headStale);
                }

                return new NightVerdict(false, lines);
            }

            lines.Add($"The night run {headNight.Run.Id} on the head commit {pullRequest.Head} succeeded, and it is older than 48 hours, so the gate reads the night of `{MainBranch}` (G-22).");
        }

        if (mainNight is null)
        {
            lines.Add($"absent: no completed night exists on `{MainBranch}` or on the head commit {pullRequest.Head} (G-22).");
            return new NightVerdict(false, lines);
        }

        List<string> problems = ProblemsOf(mainNight, null, MainBranch, now);
        if (problems.Count == 0)
        {
            lines.Add($"The night run {mainNight.Run.Id} on the commit {mainNight.Run.Commit} of `{MainBranch}` started at {NightJson.TextOf(mainNight.Run.Started)} and succeeded on each leg (G-22).");
            return new NightVerdict(true, lines);
        }

        lines.Add($"The newest night on `{MainBranch}` does not pass:");
        lines.AddRange(problems);
        return new NightVerdict(false, lines);
    }

    /// <summary>Gives each reason why a night does not pass, or an empty list for a night that passes.</summary>
    /// <param name="night">The night.</param>
    /// <param name="commit">The commit that the night must play, or null for any commit.</param>
    /// <param name="branch">The branch that the night must run on, or null for any branch.</param>
    /// <param name="now">The time of the run of the gate, in UTC.</param>
    /// <returns>One line for each reason, with the case, the commit, and the time (T-2).</returns>
    public static List<string> ProblemsOf(NightEvidence night, string? commit, string? branch, DateTimeOffset now)
    {
        ArgumentNullException.ThrowIfNull(night);

        List<string> problems = FaultsOf(night, commit, branch, now);
        if (StaleOf(night.Run, now) is string stale)
        {
            problems.Add(stale);
        }

        return problems;
    }

    /// <summary>Gives each reason why a night does not pass, except its age: a wrong branch, commit, or time, a failed run, and an absent or a failed record.</summary>
    /// <param name="night">The night.</param>
    /// <param name="commit">The commit that the night must play, or null for any commit.</param>
    /// <param name="branch">The branch that the night must run on, or null for any branch.</param>
    /// <param name="now">The time of the run of the gate, in UTC.</param>
    /// <returns>One line for each reason, with the case, the commit, and the time (T-2).</returns>
    public static List<string> FaultsOf(NightEvidence night, string? commit, string? branch, DateTimeOffset now)
    {
        ArgumentNullException.ThrowIfNull(night);

        NightRun run = night.Run;
        string name = $"the night run {run.Id} on the commit {run.Commit}, started at {NightJson.TextOf(run.Started)},";
        List<string> problems = [];
        if (branch is not null && !string.Equals(run.Branch, branch, StringComparison.Ordinal))
        {
            problems.Add($"wrong branch: {name} ran on the branch '{run.Branch}', and the gate reads the nights of `{branch}` alone.");
        }

        if (commit is not null && !string.Equals(run.Commit, commit, StringComparison.Ordinal))
        {
            problems.Add($"wrong commit: {name} did not play the head commit {commit}.");
        }

        if (!string.Equals(run.Conclusion, SuccessConclusion, StringComparison.Ordinal))
        {
            problems.Add($"failed: {name} ended as '{run.Conclusion}'.");
        }

        if (now < run.Started)
        {
            problems.Add($"wrong time: {name} starts after the run of the gate at {NightJson.TextOf(now)}.");
        }

        foreach (string leg in night.AbsentLegs)
        {
            problems.Add($"absent: {name} holds no night record of the leg {leg}.");
        }

        problems.AddRange(RecordProblemsOf(night, name));
        return problems;
    }

    /// <summary>Gives the line of a night older than 48 hours at the run of the gate, or no value (G-22, D-1188).</summary>
    /// <param name="run">The facts of the night run.</param>
    /// <param name="now">The time of the run of the gate, in UTC.</param>
    /// <returns>The line, or no value for a night inside the limit.</returns>
    public static string? StaleOf(NightRun run, DateTimeOffset now)
    {
        ArgumentNullException.ThrowIfNull(run);

        TimeSpan age = now - run.Started;
        if (age <= Limit)
        {
            return null;
        }

        long hours = age.Ticks / TimeSpan.TicksPerHour;
        return $"stale: the night run {run.Id} on the commit {run.Commit}, started at {NightJson.TextOf(run.Started)}, is {hours.ToString(CultureInfo.InvariantCulture)} hours old at the run of the gate at {NightJson.TextOf(now)}, and the limit is 48 hours (G-22, D-1188).";
    }

    private static List<string> RecordProblemsOf(NightEvidence night, string name)
    {
        List<string> problems = [];
        NightRecord? first = null;
        foreach (NightRecord record in night.Records)
        {
            if (!string.Equals(record.Commit, night.Run.Commit, StringComparison.Ordinal))
            {
                problems.Add($"wrong commit: the night record of the leg {record.Leg} names the commit {record.Commit}, and {name} played {night.Run.Commit}.");
            }

            foreach (NightPolicyCounts counts in record.Policies)
            {
                if (counts.Fails)
                {
                    problems.Add($"failed: the {BotPolicyKinds.NameOf(counts.Policy)} runs of the leg {record.Leg} in {name} ended with {counts.Softlock.ToString(CultureInfo.InvariantCulture)} softlocks and {counts.Crash.ToString(CultureInfo.InvariantCulture)} crashes, from the first seed {record.FirstSeed.ToString(CultureInfo.InvariantCulture)}.");
                }
            }

            // The legs of one night play the same seed range (D-1190, D-1191).
            if (first is null)
            {
                first = record;
            }
            else if (!SameRange(record, first))
            {
                problems.Add($"wrong range: the leg {record.Leg} played {RangeOf(record)}, and the leg {first.Leg} played {RangeOf(first)} (D-1190, D-1191).");
            }
        }

        return problems;
    }


    private static bool SameRange(NightRecord record, NightRecord other)
    {
        if (record.FirstSeed != other.FirstSeed)
        {
            return false;
        }

        for (int index = 0; index < record.Policies.Count; index += 1)
        {
            if (record.Policies[index].Runs != other.Policies[index].Runs)
            {
                return false;
            }
        }

        return true;
    }

    private static string RangeOf(NightRecord record)
    {
        List<string> parts = [];
        foreach (NightPolicyCounts counts in record.Policies)
        {
            parts.Add($"{counts.Runs.ToString(CultureInfo.InvariantCulture)} {BotPolicyKinds.NameOf(counts.Policy)} runs");
        }

        return $"{string.Join(" and ", parts)} from the seed {record.FirstSeed.ToString(CultureInfo.InvariantCulture)}";
    }

    private static string? FirstPathOutsideDocs(IReadOnlyList<string> files)
    {
        foreach (string file in files)
        {
            if (!OverrideRules.IsEligible(file))
            {
                return file;
            }
        }

        return null;
    }
}
