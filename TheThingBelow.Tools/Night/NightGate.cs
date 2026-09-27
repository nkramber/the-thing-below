using System;
using System.Collections.Generic;
using System.Globalization;
using System.Text.Json;
using TheThingBelow.Tools.Bots;

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
/// night on its head commit succeeded (D-510), or a night on an earlier commit with documents alone
/// after it (D-1204). Else the newest evidence of `main` must pass: its newest night, or a newer
/// promotion of the night of a merged PR (D-1202). A night counts inside 48 hours of the run of
/// the gate on the last push (D-1188).
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
    /// <param name="candidates">Each commit of the PR whose night passes the head, newest first, as <see cref="NightWalk.CandidatesOf"/> gives them (D-1204).</param>
    /// <param name="main">The newest night of `main`, the newest promotion, and their order (D-1202).</param>
    /// <param name="headNight">The newest completed night on a commit of <paramref name="candidates"/>, or null when no night exists there.</param>
    /// <param name="now">The time of the run of the gate, in UTC.</param>
    /// <returns>The result.</returns>
    public static NightVerdict Check(NightPullRequest pullRequest, IReadOnlyList<string> candidates, NightMainFacts main, NightEvidence? headNight, DateTimeOffset now)
    {
        ArgumentNullException.ThrowIfNull(pullRequest);
        ArgumentNullException.ThrowIfNull(candidates);
        ArgumentNullException.ThrowIfNull(main);

        string? code = NightWalk.FirstCodePath(pullRequest.Files);
        if (code is null)
        {
            return new NightVerdict(true, ["The PR changes documents alone, and a docs-only PR passes the night gate (D-513)."]);
        }

        List<string> lines = [$"The PR changes the path '{code}', so the night gate binds it (G-22, D-513)."];

        // A failed night on the head commit fails the PR, even when `main` has a green night,
        // because that night played the code of this PR (D-510, T-2). A night on an earlier commit
        // counts when each later commit changes documents alone (D-1204). A green head night that
        // is only older than 48 hours proves nothing now, so the gate reads the night of `main` (G-22).
        if (headNight is not null)
        {
            string name = HeadNightName(headNight, pullRequest.Head);
            List<string> headFaults = FaultsOf(headNight, null, null, now);
            if (!Contains(candidates, headNight.Run.Commit))
            {
                headFaults.Insert(0, $"wrong commit: {name} is not the head {pullRequest.Head}, and a later commit of the PR changes a path outside the paths of a docs-only PR (D-1204).");
            }

            string? headStale = StaleOf(headNight.Run, now);
            if (headFaults.Count == 0 && headStale is null)
            {
                lines.Add(string.Equals(headNight.Run.Commit, pullRequest.Head, StringComparison.Ordinal)
                    ? $"{Capital(name)} succeeded on each leg, and it passes this PR alone (D-510)."
                    : $"{Capital(name)} succeeded on each leg. Each later commit changes documents alone, so it passes this PR alone (D-510, D-1204).");
                return new NightVerdict(true, lines);
            }

            if (headFaults.Count > 0)
            {
                lines.Add($"{Capital(name)} does not pass:");
                lines.AddRange(headFaults);
                if (headStale is not null)
                {
                    lines.Add(headStale);
                }

                return new NightVerdict(false, lines);
            }

            lines.Add($"{Capital(name)} succeeded, and it is older than 48 hours, so the gate reads the night of `{MainBranch}` (G-22).");
        }

        if (main.PromotionWins)
        {
            return CheckPromotion(main.Promotion!, lines, now);
        }

        if (main.Night is null)
        {
            lines.Add($"absent: no completed night exists on `{MainBranch}` or on the head commit {pullRequest.Head} (G-22).");
            return new NightVerdict(false, lines);
        }

        NightEvidence mainNight = main.Night;
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

    /// <summary>Tells whether two night records played the same range: the same first seed and the same count of runs of each policy (D-1190, D-1203).</summary>
    /// <param name="record">One record.</param>
    /// <param name="other">The other record.</param>
    /// <returns>True when the ranges match.</returns>
    public static bool SameRange(NightRecord record, NightRecord other)
    {
        ArgumentNullException.ThrowIfNull(record);
        ArgumentNullException.ThrowIfNull(other);
        if (record.FirstSeed != other.FirstSeed || record.Policies.Count != other.Policies.Count)
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

    /// <summary>Gives the text of the range of a night record, for a message.</summary>
    /// <param name="record">The record.</param>
    /// <returns>The count of runs of each policy and the first seed.</returns>
    public static string RangeOf(NightRecord record)
    {
        ArgumentNullException.ThrowIfNull(record);
        List<string> parts = [];
        foreach (NightPolicyCounts counts in record.Policies)
        {
            parts.Add($"{counts.Runs.ToString(CultureInfo.InvariantCulture)} {BotPolicyKinds.NameOf(counts.Policy)} runs");
        }

        return $"{string.Join(" and ", parts)} from the seed {record.FirstSeed.ToString(CultureInfo.InvariantCulture)}";
    }

    private static NightVerdict CheckPromotion(NightPromotion promotion, List<string> lines, DateTimeOffset now)
    {
        NightRun run = promotion.Night.Run;
        string name = $"the night run {run.Id} of the PR #{promotion.PullRequest.ToString(CultureInfo.InvariantCulture)}, promoted at the merge commit {promotion.MergeCommit} of `{MainBranch}` over the failed night run {promotion.FailedRun.ToString(CultureInfo.InvariantCulture)}";
        List<string> problems = ProblemsOf(promotion.Night, null, null, now);
        if (problems.Count == 0)
        {
            lines.Add($"{Capital(name)}, started at {NightJson.TextOf(run.Started)} and succeeded on each leg (G-22, D-1202).");
            return new NightVerdict(true, lines);
        }

        lines.Add($"The newest evidence of `{MainBranch}` is {name}, and it does not pass:");
        lines.AddRange(problems);
        return new NightVerdict(false, lines);
    }

    private static string HeadNightName(NightEvidence night, string head)
    {
        return string.Equals(night.Run.Commit, head, StringComparison.Ordinal)
            ? $"the night run {night.Run.Id} on the head commit {head}"
            : $"the night run {night.Run.Id} on the commit {night.Run.Commit} of this PR";
    }

    private static string Capital(string text) => char.ToUpperInvariant(text[0]) + text[1..];

    private static bool Contains(IReadOnlyList<string> items, string item)
    {
        foreach (string each in items)
        {
            if (string.Equals(each, item, StringComparison.Ordinal))
            {
                return true;
            }
        }

        return false;
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
}
