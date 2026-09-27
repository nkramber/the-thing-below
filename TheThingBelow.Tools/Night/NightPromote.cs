using System;
using System.Collections.Generic;
using System.Globalization;

namespace TheThingBelow.Tools.Night;

/// <summary>The facts that the `night-promote` workflow reads on a push to `main` (D-1202).</summary>
/// <param name="MergeCommit">The full hash of the pushed commit of `main`.</param>
/// <param name="PullRequest">The number of the merged PR of the push, or null when the push holds no merged PR.</param>
/// <param name="Main">The evidence of `main` before the push: the newest night, the newest promotion, and their order.</param>
/// <param name="BranchNight">The newest completed night on a commit of the walk of the PR, or null when none exists (D-1204).</param>
/// <param name="Candidates">Each commit of the PR whose night passes its head, newest first, or an empty list with no PR.</param>
/// <param name="TreeDifference">Each path in which the tree of the merge commit differs from the tree of the commit of the branch night.</param>
public sealed record NightPromoteFacts(
    string MergeCommit,
    long? PullRequest,
    NightMainFacts Main,
    NightEvidence? BranchNight,
    IReadOnlyList<string> Candidates,
    IReadOnlyList<string> TreeDifference);

/// <summary>The result of a promotion check: the promotion, or no value, and one line for each fact that the check read.</summary>
/// <param name="Promotion">The promotion, or null when the push promotes nothing.</param>
/// <param name="Lines">The lines of the result, in the order that the check read the facts.</param>
public sealed record NightPromoteVerdict(NightPromotion? Promotion, IReadOnlyList<string> Lines);

/// <summary>
/// The rules of a promotion (D-1202, D-1203). When the newest evidence of `main` is a failed night,
/// a merged PR with a green night on the failed range turns `main` green at its merge commit.
/// </summary>
public static class NightPromote
{
    /// <summary>Checks whether a push to `main` promotes the night of its PR.</summary>
    /// <param name="facts">The facts of the push.</param>
    /// <param name="now">The time of the check, in UTC.</param>
    /// <returns>The result. A push that promotes nothing is no fault, and each line names the reason.</returns>
    public static NightPromoteVerdict Check(NightPromoteFacts facts, DateTimeOffset now)
    {
        ArgumentNullException.ThrowIfNull(facts);

        List<string> lines = [];
        if (facts.Main.PromotionWins)
        {
            NightPromotion newest = facts.Main.Promotion!;
            lines.Add($"The newest evidence of `{NightGate.MainBranch}` is the promotion of the PR #{Text(newest.PullRequest)} at {newest.MergeCommit}, so no failed night needs a promotion (D-1202).");
            return new NightPromoteVerdict(null, lines);
        }

        NightEvidence? failed = facts.Main.Night;
        if (failed is null)
        {
            lines.Add($"No completed night exists on `{NightGate.MainBranch}`, so no failed night needs a promotion (D-1202).");
            return new NightPromoteVerdict(null, lines);
        }

        string failedName = $"the night run {Text(failed.Run.Id)} on the commit {failed.Run.Commit} of `{NightGate.MainBranch}`";
        if (NightGate.FaultsOf(failed, null, NightGate.MainBranch, now).Count == 0)
        {
            lines.Add($"The newest night is {failedName}, and it succeeded, so nothing needs a promotion (D-1202).");
            return new NightPromoteVerdict(null, lines);
        }

        lines.Add($"The newest night is {failedName}, and it failed.");
        if (facts.PullRequest is not long pullRequest)
        {
            lines.Add($"The push of {facts.MergeCommit} holds no merged PR, so it promotes nothing (D-1202).");
            return new NightPromoteVerdict(null, lines);
        }

        NightEvidence? night = facts.BranchNight;
        if (night is null)
        {
            lines.Add($"The PR #{Text(pullRequest)} has no completed night on its head, or on a commit with documents alone after it, so it promotes nothing (D-510, D-1204).");
            return new NightPromoteVerdict(null, lines);
        }

        List<string> reasons = ReasonsOf(facts, failed, night, now);
        string nightName = $"night run {Text(night.Run.Id)} on the commit {night.Run.Commit} of the PR #{Text(pullRequest)}";
        if (reasons.Count > 0)
        {
            lines.Add($"The {nightName} does not promote:");
            lines.AddRange(reasons);
            return new NightPromoteVerdict(null, lines);
        }

        lines.Add($"The {nightName} succeeded on the failed range, so it promotes at the merge commit {facts.MergeCommit} over the failed night run {Text(failed.Run.Id)} (D-1202, D-1203).");
        return new NightPromoteVerdict(new NightPromotion(facts.MergeCommit, pullRequest, failed.Run.Id, night), lines);
    }

    private static List<string> ReasonsOf(NightPromoteFacts facts, NightEvidence failed, NightEvidence night, DateTimeOffset now)
    {
        List<string> reasons = NightGate.ProblemsOf(night, null, null, now);
        bool onWalk = false;
        foreach (string candidate in facts.Candidates)
        {
            onWalk = onWalk || string.Equals(candidate, night.Run.Commit, StringComparison.Ordinal);
        }

        if (!onWalk)
        {
            reasons.Add($"wrong commit: a later commit of the PR changes a path outside the paths of a docs-only PR, so the night of {night.Run.Commit} does not play the merged code (D-1204).");
        }

        // A night on the merge commit itself played the merged code, and it failed (T-2).
        if (string.Equals(failed.Run.Commit, facts.MergeCommit, StringComparison.Ordinal))
        {
            reasons.Add($"wrong order: the failed night run {Text(failed.Run.Id)} played the merge commit {facts.MergeCommit} itself (D-1202).");
        }

        if (failed.Records.Count == 0)
        {
            reasons.Add($"absent: the failed night run {Text(failed.Run.Id)} holds no night record, so its range is unknown (D-1203).");
        }
        else if (night.Records.Count > 0 && !NightGate.SameRange(night.Records[0], failed.Records[0]))
        {
            reasons.Add($"wrong range: the branch night played {NightGate.RangeOf(night.Records[0])}, and the failed night played {NightGate.RangeOf(failed.Records[0])} (D-1203).");
        }

        string? code = NightWalk.FirstCodePath(facts.TreeDifference);
        if (code is not null)
        {
            reasons.Add($"wrong tree: the tree of the merge commit {facts.MergeCommit} differs from the tree of {night.Run.Commit} in the path '{code}', outside the paths of a docs-only PR (D-1202).");
        }

        return reasons;
    }

    private static string Text(long value) => value.ToString(CultureInfo.InvariantCulture);
}
