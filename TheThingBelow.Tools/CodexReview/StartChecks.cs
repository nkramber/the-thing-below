using System;
using System.Collections.Generic;
using System.Globalization;

namespace TheThingBelow.Tools.CodexReview;

/// <summary>The facts of the pull request and of the author checkout before a review run.</summary>
/// <param name="Number">The GitHub number of the pull request.</param>
/// <param name="State">The state of the pull request on GitHub, such as `OPEN`.</param>
/// <param name="Branch">The branch of the pull request on GitHub.</param>
/// <param name="GitHubHead">The head of the pull request on GitHub.</param>
/// <param name="LocalBranch">The branch of the checkout, or null when the checkout has no branch.</param>
/// <param name="LocalHead">The head of the checkout.</param>
/// <param name="RemoteHead">The head of the branch on origin after the fetch.</param>
/// <param name="Status">The output of `git status --porcelain`.</param>
public sealed record CheckoutFacts(
    int Number,
    string State,
    string Branch,
    string GitHubHead,
    string? LocalBranch,
    string LocalHead,
    string RemoteHead,
    string Status);

/// <summary>
/// The checks before a review run (D-926). Each one refuses the run, because the reviewer then
/// reads another head than the author pushed, or no head at all.
/// </summary>
public static class StartChecks
{
    /// <summary>The state of an open pull request in the output of `gh pr view`.</summary>
    public const string OpenState = "OPEN";

    /// <summary>Gives each reason that refuses the review run.</summary>
    /// <param name="facts">The facts of the pull request and the checkout.</param>
    /// <returns>Each reason. An empty list lets the run start.</returns>
    public static IReadOnlyList<string> Check(CheckoutFacts facts)
    {
        ArgumentNullException.ThrowIfNull(facts);
        List<string> reasons = [];
        string number = facts.Number.ToString(CultureInfo.InvariantCulture);

        if (!string.Equals(facts.State, OpenState, StringComparison.Ordinal))
        {
            reasons.Add($"PR #{number} has the state `{facts.State}`, and a review needs an open PR.");
        }

        if (!string.Equals(facts.LocalBranch, facts.Branch, StringComparison.Ordinal))
        {
            string local = facts.LocalBranch ?? "no branch";
            reasons.Add($"the checkout is on `{local}`, and PR #{number} has the branch `{facts.Branch}`.");
        }

        if (!string.Equals(facts.LocalHead, facts.RemoteHead, StringComparison.Ordinal))
        {
            reasons.Add(
                $"the checkout head `{facts.LocalHead}` differs from `origin/{facts.Branch}` at `{facts.RemoteHead}`. Push or pull first.");
        }

        if (!string.Equals(facts.RemoteHead, facts.GitHubHead, StringComparison.Ordinal))
        {
            reasons.Add(
                $"`origin/{facts.Branch}` is `{facts.RemoteHead}`, and GitHub gives the head `{facts.GitHubHead}` for PR #{number}.");
        }

        if (facts.Status.Trim().Length > 0)
        {
            reasons.Add($"the working tree has changes, and each one must be in a commit or out of the tree: {FirstLines(facts.Status)}");
        }

        return reasons;
    }

    private static string FirstLines(string status)
    {
        IReadOnlyList<string> lines = GitarPass.SplitLines(status);
        int count = Math.Min(lines.Count, 5);
        List<string> shown = [];
        for (int index = 0; index < count; index++)
        {
            shown.Add(lines[index]);
        }

        string more = lines.Count > count ? $", and {lines.Count - count} more" : string.Empty;
        return string.Join(", ", shown) + more + ".";
    }
}
