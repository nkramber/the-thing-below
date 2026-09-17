using System;
using System.Collections.Generic;
using System.Globalization;
using System.IO;
using System.Text.RegularExpressions;

namespace TheThingBelow.Tools.ReviewGate;

/// <summary>
/// The three rules of the review record (D-15, D-17). The record exists, its verdict is
/// `Ready for owner merge`, and its head field names the effective head. The
/// `pr-review` reference file holds the form of each machine-read part.
/// </summary>
public static class ReviewRecordRules
{
    /// <summary>The verdict that lets the owner merge the pull request.</summary>
    public const string ApprovedVerdict = "Ready for owner merge";

    /// <summary>The three verdict names of the `pr-review` skill.</summary>
    public static readonly IReadOnlyList<string> VerdictNames =
        [ApprovedVerdict, "Changes required", "Blocked"];

    private const RegexOptions Options = RegexOptions.CultureInvariant | RegexOptions.Compiled;

    /// <summary>The head field of the Identity list, with the hash in backticks.</summary>
    private static readonly Regex HeadField = new Regex(@"^\s*-\s*Head:\s*`([0-9a-fA-F]+)`", Options);

    /// <summary>The shortest hash that names one commit without doubt.</summary>
    private const int ShortestHash = 7;

    /// <summary>Gives the path of the review record of one pull request.</summary>
    /// <param name="number">The GitHub number of the pull request.</param>
    /// <returns>The path of the record, from the root of the checkout.</returns>
    public static string RecordPath(int number) =>
        $"docs/reviews/pr-{number.ToString(CultureInfo.InvariantCulture)}.md";

    /// <summary>Runs RG 3, RG 4, and RG 5 against the files of the head.</summary>
    /// <param name="facts">The facts of the pull request.</param>
    /// <param name="headFilesRoot">The folder that holds the files of the head, as data alone.</param>
    /// <param name="effectiveHead">The effective head, or null when every commit is metadata.</param>
    /// <returns>The result of each of the three rules, in rule order.</returns>
    public static IReadOnlyList<GateCheck> Check(
        PullRequestFacts facts,
        string headFilesRoot,
        CommitFacts? effectiveHead)
    {
        ArgumentNullException.ThrowIfNull(facts);
        ArgumentException.ThrowIfNullOrEmpty(headFilesRoot);

        string path = RecordPath(facts.Number);
        string full = Path.Combine(headFilesRoot, path.Replace('/', Path.DirectorySeparatorChar));
        if (!File.Exists(full))
        {
            return
            [
                new GateCheck("RG 3", GateResult.Fault, $"the head holds no review record at `{path}` (T-4, D-17)."),
                new GateCheck("RG 4", GateResult.Skip, "RG 3 found no review record."),
                new GateCheck("RG 5", GateResult.Skip, "RG 3 found no review record."),
            ];
        }

        string text = File.ReadAllText(full);
        return
        [
            new GateCheck("RG 3", GateResult.Pass, $"the head holds the review record at `{path}`."),
            CheckVerdict(path, text),
            CheckHead(path, text, effectiveHead),
        ];
    }

    /// <summary>RG 4: the Verdict section names `Ready for owner merge` and nothing else.</summary>
    /// <param name="path">The path of the record, for the message.</param>
    /// <param name="text">The text of the record.</param>
    /// <returns>The result of the rule.</returns>
    public static GateCheck CheckVerdict(string path, string text)
    {
        ArgumentNullException.ThrowIfNull(text);
        IReadOnlyList<string>? section = MarkdownSection.ReadLines(text, "## Verdict");
        if (section is null)
        {
            return new GateCheck("RG 4", GateResult.Fault, $"`{path}` holds no `## Verdict` section (D-17).");
        }

        string body = string.Join('\n', section);
        List<string> found = [];
        foreach (string name in VerdictNames)
        {
            if (body.Contains(name, StringComparison.Ordinal))
            {
                found.Add(name);
            }
        }

        if (found.Count == 0)
        {
            return new GateCheck(
                "RG 4",
                GateResult.Fault,
                $"the `## Verdict` section of `{path}` names no verdict of the `pr-review` skill.");
        }

        if (found.Count > 1)
        {
            return new GateCheck(
                "RG 4",
                GateResult.Fault,
                $"the `## Verdict` section of `{path}` names {found.Count} verdicts: {string.Join(", ", found)}.");
        }

        if (!string.Equals(found[0], ApprovedVerdict, StringComparison.Ordinal))
        {
            return new GateCheck(
                "RG 4",
                GateResult.Fault,
                $"the verdict of `{path}` is `{found[0]}`, and the gate needs `{ApprovedVerdict}` (T-4).");
        }

        return new GateCheck("RG 4", GateResult.Pass, $"the verdict of `{path}` is `{ApprovedVerdict}`.");
    }

    /// <summary>RG 5: the head field of the record names the effective head (D-610).</summary>
    /// <param name="path">The path of the record, for the message.</param>
    /// <param name="text">The text of the record.</param>
    /// <param name="effectiveHead">The effective head, or null when every commit is metadata.</param>
    /// <returns>The result of the rule.</returns>
    public static GateCheck CheckHead(string path, string text, CommitFacts? effectiveHead)
    {
        ArgumentNullException.ThrowIfNull(text);
        if (effectiveHead is null)
        {
            return new GateCheck(
                "RG 5",
                GateResult.Fault,
                "every commit of the PR changes the metadata set alone, so the PR has no effective head (D-578, D-610).");
        }

        string? recorded = null;
        foreach (string line in text.Split('\n'))
        {
            Match match = HeadField.Match(line);
            if (match.Success)
            {
                recorded = match.Groups[1].Value;
                break;
            }
        }

        if (recorded is null)
        {
            return new GateCheck(
                "RG 5",
                GateResult.Fault,
                $"`{path}` holds no head field. The Identity list needs a line `- Head: ` with the hash in backticks.");
        }

        if (recorded.Length < ShortestHash)
        {
            return new GateCheck(
                "RG 5",
                GateResult.Fault,
                $"the head field of `{path}` is `{recorded}`, which is shorter than {ShortestHash} letters.");
        }

        if (!effectiveHead.Sha.StartsWith(recorded, StringComparison.OrdinalIgnoreCase))
        {
            return new GateCheck(
                "RG 5",
                GateResult.Fault,
                $"the head field of `{path}` is `{recorded}`, and the effective head is `{effectiveHead.Sha}` (D-610).");
        }

        return new GateCheck(
            "RG 5",
            GateResult.Pass,
            $"the head field of `{path}` names the effective head `{effectiveHead.Sha}`.");
    }
}
