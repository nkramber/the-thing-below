using System;
using System.Collections.Generic;
using System.Globalization;
using System.Text.RegularExpressions;
using TheThingBelow.Tools.ReviewGate;

namespace TheThingBelow.Tools.CodexReview;

/// <summary>One finding of a review record, with each round in which the review found it open.</summary>
/// <param name="Id">The stable id of the finding, such as `P1-1`.</param>
/// <param name="IsOpen">True when the status line of the finding says `open`.</param>
/// <param name="OpenHeads">
/// Each effective head, oldest first, at which a review round found the finding open. The
/// `Open at:` line of the finding holds them (D-929).
/// </param>
public sealed record ReviewFinding(string Id, bool IsOpen, IReadOnlyList<string> OpenHeads);

/// <summary>
/// The rounds of each finding of a review record, and the three-strike rule (D-929). A finding
/// counts one time for each review round in which it is open. The rounds need not follow each
/// other, so a finding that a fix closed and a later round opened again keeps its count.
/// </summary>
public static class FindingRounds
{
    /// <summary>The count of open rounds at which the fix loop stops, and the owner decides.</summary>
    public const int StrikeLimit = 3;

    /// <summary>The label of the line that lists the open rounds of a finding.</summary>
    public const string OpenAtLabel = "Open at:";

    private const RegexOptions Options = RegexOptions.CultureInvariant | RegexOptions.Compiled;

    /// <summary>The heading of one finding, such as `### P1-1: the title`.</summary>
    private static readonly Regex FindingHeading = new Regex(@"^###\s+(P[0-3]-[0-9]+):", Options);

    /// <summary>The status line of an open finding. `Status: open.` is the form of the `pr-review` skill.</summary>
    private static readonly Regex OpenStatus = new Regex(@"^Status:\s*open\b", Options);

    /// <summary>One commit hash in backticks.</summary>
    private static readonly Regex Hash = new Regex(@"`([0-9a-fA-F]+)`", Options);

    /// <summary>The shortest hash that names one commit without doubt, as RG 5 reads it.</summary>
    private const int ShortestHash = 7;

    /// <summary>Reads each finding of the `## Findings` section of a review record.</summary>
    /// <param name="path">The path of the record, for the message of a fault.</param>
    /// <param name="text">The text of the record.</param>
    /// <returns>Each finding in the order of the record. The text "No finding." gives none.</returns>
    /// <exception cref="InvalidOperationException">
    /// The record has no Findings section, a heading of that section is no finding id, or a finding
    /// has no status line or no `Open at:` line with a hash (T-2).
    /// </exception>
    public static IReadOnlyList<ReviewFinding> Read(string path, string text)
    {
        ArgumentException.ThrowIfNullOrEmpty(path);
        ArgumentNullException.ThrowIfNull(text);

        IReadOnlyList<string>? section = MarkdownSection.ReadLines(text, "## Findings");
        if (section is null)
        {
            throw new InvalidOperationException($"`{path}` holds no `## Findings` section (D-17).");
        }

        List<ReviewFinding> findings = [];
        string? id = null;
        string? status = null;
        string? openAt = null;
        foreach (string raw in section)
        {
            string line = raw.Trim();
            if (line.StartsWith("### ", StringComparison.Ordinal))
            {
                AddFinding(findings, path, id, status, openAt);
                id = ReadId(path, line);
                status = null;
                openAt = null;
            }
            else if (id is not null && status is null && line.StartsWith("Status:", StringComparison.Ordinal))
            {
                status = line;
            }
            else if (id is not null && openAt is null && line.StartsWith(OpenAtLabel, StringComparison.Ordinal))
            {
                openAt = line;
            }
        }

        AddFinding(findings, path, id, status, openAt);
        return findings;
    }

    /// <summary>
    /// Checks each finding against the effective head of this round. An open finding lists that
    /// head, and a finding that is not open does not list it (T-2).
    /// </summary>
    /// <param name="path">The path of the record, for the message of a fault.</param>
    /// <param name="findings">The findings of the record.</param>
    /// <param name="effectiveHead">The full hash of the effective head (D-610).</param>
    /// <exception cref="InvalidOperationException">A finding and its `Open at:` line disagree.</exception>
    public static void CheckHeads(string path, IReadOnlyList<ReviewFinding> findings, string effectiveHead)
    {
        ArgumentException.ThrowIfNullOrEmpty(path);
        ArgumentNullException.ThrowIfNull(findings);
        ArgumentException.ThrowIfNullOrEmpty(effectiveHead);

        foreach (ReviewFinding finding in findings)
        {
            bool listed = Lists(finding, effectiveHead);
            if (finding.IsOpen && !listed)
            {
                throw new InvalidOperationException(
                    $"`{path}`: {finding.Id} is open, and its `{OpenAtLabel}` line does not list the effective head `{effectiveHead}` (D-929).");
            }

            if (!finding.IsOpen && listed)
            {
                throw new InvalidOperationException(
                    $"`{path}`: {finding.Id} is not open, and its `{OpenAtLabel}` line lists the effective head `{effectiveHead}` (D-929).");
            }
        }
    }

    /// <summary>Gives each finding that stops the fix loop: open now, and open in three rounds or more.</summary>
    /// <param name="findings">The findings of the record, after <see cref="CheckHeads"/>.</param>
    /// <returns>Each finding that needs the decision of the owner (D-929).</returns>
    public static IReadOnlyList<ReviewFinding> Strikes(IReadOnlyList<ReviewFinding> findings)
    {
        ArgumentNullException.ThrowIfNull(findings);
        List<ReviewFinding> strikes = [];
        foreach (ReviewFinding finding in findings)
        {
            if (finding.IsOpen && finding.OpenHeads.Count >= StrikeLimit)
            {
                strikes.Add(finding);
            }
        }

        return strikes;
    }

    private static void AddFinding(List<ReviewFinding> findings, string path, string? id, string? status, string? openAt)
    {
        if (id is null)
        {
            return;
        }

        if (status is null)
        {
            throw new InvalidOperationException($"`{path}`: {id} has no `Status:` line (D-17).");
        }

        if (openAt is null)
        {
            throw new InvalidOperationException(
                $"`{path}`: {id} has no `{OpenAtLabel}` line. The line lists each effective head at which the finding was open (D-929).");
        }

        findings.Add(new ReviewFinding(id, OpenStatus.IsMatch(status), ReadHeads(path, id, openAt)));
    }

    private static string ReadId(string path, string heading)
    {
        Match match = FindingHeading.Match(heading);
        if (!match.Success)
        {
            throw new InvalidOperationException(
                $"`{path}`: the heading '{heading}' of the Findings section names no finding id such as `P1-1` (D-17).");
        }

        return match.Groups[1].Value;
    }

    private static List<string> ReadHeads(string path, string id, string openAt)
    {
        List<string> heads = [];
        foreach (Match match in Hash.Matches(openAt))
        {
            string hash = match.Groups[1].Value;
            if (hash.Length < ShortestHash)
            {
                throw new InvalidOperationException(
                    $"`{path}`: the `{OpenAtLabel}` line of {id} holds `{hash}`, which is shorter than {ShortestHash.ToString(CultureInfo.InvariantCulture)} letters.");
            }

            foreach (string earlier in heads)
            {
                if (SameCommit(earlier, hash))
                {
                    throw new InvalidOperationException(
                        $"`{path}`: the `{OpenAtLabel}` line of {id} lists `{hash}` two times. One round is one effective head (D-929).");
                }
            }

            heads.Add(hash);
        }

        if (heads.Count == 0)
        {
            throw new InvalidOperationException(
                $"`{path}`: the `{OpenAtLabel}` line of {id} holds no hash in backticks (D-929).");
        }

        return heads;
    }

    private static bool Lists(ReviewFinding finding, string effectiveHead)
    {
        foreach (string head in finding.OpenHeads)
        {
            if (SameCommit(head, effectiveHead))
            {
                return true;
            }
        }

        return false;
    }

    private static bool SameCommit(string first, string second)
    {
        string shorter = first.Length <= second.Length ? first : second;
        string longer = first.Length <= second.Length ? second : first;
        return longer.StartsWith(shorter, StringComparison.OrdinalIgnoreCase);
    }
}
