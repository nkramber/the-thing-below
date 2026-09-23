using System;
using System.Collections.Generic;
using TheThingBelow.Tools.ReviewGate;

namespace TheThingBelow.Tools.CodexReview;

/// <summary>The four outcomes of one review run, each with its own exit code (D-926).</summary>
public enum ReviewOutcomeKind
{
    /// <summary>The record gives `Ready for owner merge` for the effective head.</summary>
    Approve,

    /// <summary>The record gives `Changes required` or `Blocked` for the effective head.</summary>
    ChangesRequired,

    /// <summary>A finding is open for the third time, and the owner decides (D-929).</summary>
    ThreeStrikes,

    /// <summary>The Codex CLI failed, the record is absent, or the record is stale or malformed.</summary>
    Fault,
}

/// <summary>The outcome of one review run, read from the record on origin after the reviewer ends.</summary>
/// <param name="Kind">The outcome.</param>
/// <param name="Verdict">The verdict name of the record, or null when no verdict reads.</param>
/// <param name="OpenFindings">The id of each open finding.</param>
/// <param name="Strikes">Each finding that stops the fix loop (D-929).</param>
/// <param name="Reason">One line that gives the reason of the outcome.</param>
public sealed record ReviewOutcome(
    ReviewOutcomeKind Kind,
    string? Verdict,
    IReadOnlyList<string> OpenFindings,
    IReadOnlyList<ReviewFinding> Strikes,
    string Reason)
{
    /// <summary>The exit code of an approval.</summary>
    public const int ApproveExitCode = 0;

    /// <summary>The exit code of `Changes required` and of `Blocked`.</summary>
    public const int ChangesRequiredExitCode = 2;

    /// <summary>The exit code of the three-strike stop (D-929).</summary>
    public const int ThreeStrikesExitCode = 3;

    /// <summary>Gives the exit code of this outcome. A fault gives the fault code of every Tools command.</summary>
    public int ExitCode => this.Kind switch
    {
        ReviewOutcomeKind.Approve => ApproveExitCode,
        ReviewOutcomeKind.ChangesRequired => ChangesRequiredExitCode,
        ReviewOutcomeKind.ThreeStrikes => ThreeStrikesExitCode,
        _ => Program.FaultExitCode,
    };

    /// <summary>Decides the outcome of one review run.</summary>
    /// <param name="codexExitCode">The exit code of the Codex CLI.</param>
    /// <param name="path">The path of the review record.</param>
    /// <param name="recordText">The text of the record on origin, or null when origin holds none.</param>
    /// <param name="effectiveHead">The effective head of the branch on origin, or null when it has none (D-610).</param>
    /// <returns>The outcome. A fault never gives a verdict, so a stale approval never merges (T-2).</returns>
    public static ReviewOutcome Decide(int codexExitCode, string path, string? recordText, CommitFacts? effectiveHead)
    {
        ArgumentException.ThrowIfNullOrEmpty(path);

        if (codexExitCode != 0)
        {
            return Fault($"the Codex CLI gave the exit code {codexExitCode}. Read the transcript.");
        }

        if (recordText is null)
        {
            return Fault($"origin holds no review record at `{path}` after the review (D-17).");
        }

        GateCheck head = ReviewRecordRules.CheckHead(
            path,
            recordText,
            effectiveHead is null ? [] : [effectiveHead]);
        if (head.Result != GateResult.Pass || effectiveHead is null)
        {
            return Fault(head.Detail);
        }

        IReadOnlyList<string>? bold = ReviewRecordRules.ReadBoldNames(recordText);
        if (bold is null || bold.Count != 1 || !ReviewRecordRules.IsVerdictName(bold[0]))
        {
            GateCheck verdictCheck = ReviewRecordRules.CheckVerdict(path, recordText);
            return Fault(verdictCheck.Detail);
        }

        string verdict = bold[0];
        IReadOnlyList<ReviewFinding> findings;
        try
        {
            findings = FindingRounds.Read(path, recordText);
            FindingRounds.CheckHeads(path, findings, effectiveHead.Sha);
        }
        catch (InvalidOperationException fault)
        {
            return Fault(fault.Message);
        }

        List<string> open = [];
        foreach (ReviewFinding finding in findings)
        {
            if (finding.IsOpen)
            {
                open.Add(finding.Id);
            }
        }

        // The stop wins over each verdict. A finding that three rounds found open needs the
        // owner, also when the verdict approves (D-929).
        IReadOnlyList<ReviewFinding> strikes = FindingRounds.Strikes(findings);
        if (strikes.Count > 0)
        {
            return new ReviewOutcome(
                ReviewOutcomeKind.ThreeStrikes,
                verdict,
                open,
                strikes,
                $"{strikes.Count} finding(s) open in {FindingRounds.StrikeLimit} rounds or more. Stop the fix loop, and ask the owner (D-929).");
        }

        if (string.Equals(verdict, ReviewRecordRules.ApprovedVerdict, StringComparison.Ordinal))
        {
            return new ReviewOutcome(
                ReviewOutcomeKind.Approve,
                verdict,
                open,
                [],
                $"the record approves the effective head `{effectiveHead.Sha}`.");
        }

        return new ReviewOutcome(
            ReviewOutcomeKind.ChangesRequired,
            verdict,
            open,
            [],
            $"the record gives `{verdict}` for the effective head `{effectiveHead.Sha}`.");
    }

    private static ReviewOutcome Fault(string reason)
    {
        return new ReviewOutcome(ReviewOutcomeKind.Fault, null, [], [], reason);
    }
}
