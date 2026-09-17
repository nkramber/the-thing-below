using System;
using System.Collections.Generic;

namespace TheThingBelow.Tools.ReviewGate;

/// <summary>
/// The document rules of the review gate (D-577, D-579, D-581). The pull request changes the
/// handoff, its Documents section holds a line for each row of the `one-pr-one-session` skill,
/// and no line defers a document or a record of this pull request to a later pull request.
/// </summary>
public static class DocumentRules
{
    /// <summary>The handoff file, which every pull request changes (D-18).</summary>
    public const string HandoffPath = "docs/session-handoff.md";

    /// <summary>The heading of the Documents section of the description.</summary>
    public const string SectionHeading = "## Documents";

    /// <summary>The three forms of a Documents line (D-581).</summary>
    public const string ChangedForm = "Changed:";

    /// <summary>The form for a document that the pull request can affect and does not.</summary>
    public const string NoChangeForm = "No change needed because";

    /// <summary>The form for a category that the pull request cannot reach.</summary>
    public const string NotApplicableForm = "Not applicable because";

    /// <summary>
    /// Each row of the table of the `one-pr-one-session` skill, in the order of that table. The
    /// skill and this list change together (D-579).
    /// </summary>
    public static readonly IReadOnlyList<string> RequiredRows =
    [
        "docs/design.md",
        "docs/decisions.md",
        "docs/questions.md",
        "docs/roadmaps/",
        "docs/world/",
        "docs/runbooks/",
        "docs/reviews/",
        HandoffPath,
        "CLAUDE.md and AGENTS.md",
        ".claude/skills/ and .claude/agents/",
        ".github/pull_request_template.md",
        "README.md",
    ];

    /// <summary>
    /// The words of a deferral, in lower case (D-577, D-578). A line that holds one of them
    /// leaves a document or a record of this pull request to a later pull request.
    /// </summary>
    public static readonly IReadOnlyList<string> DeferralPhrases =
    [
        "later pr",
        "next pr",
        "follow-up",
        "follow up",
        "after the merge",
        "after this pr",
        "tbd",
        "to be determined",
        "defer",
        "a separate pr",
        "a future pr",
        "comes later",
        "lands later",
        "a later session",
        "a docs pr",
        "a documentation pr",
    ];

    /// <summary>RG 6: the diff of the pull request changes the handoff (D-18, D-579).</summary>
    /// <param name="facts">The facts of the pull request.</param>
    /// <returns>The result of the rule.</returns>
    public static GateCheck CheckHandoff(PullRequestFacts facts)
    {
        ArgumentNullException.ThrowIfNull(facts);
        foreach (string file in facts.Files)
        {
            if (string.Equals(file, HandoffPath, StringComparison.Ordinal))
            {
                return new GateCheck("RG 6", GateResult.Pass, $"the PR changes `{HandoffPath}`.");
            }
        }

        return new GateCheck(
            "RG 6",
            GateResult.Fault,
            $"the PR changes no `{HandoffPath}`, and each session adds an entry (D-18, D-579).");
    }

    /// <summary>RG 7 and RG 8: the Documents section of the description (D-579, D-581).</summary>
    /// <param name="facts">The facts of the pull request.</param>
    /// <returns>The result of the two rules, in rule order.</returns>
    public static IReadOnlyList<GateCheck> CheckSection(PullRequestFacts facts)
    {
        ArgumentNullException.ThrowIfNull(facts);
        IReadOnlyList<string>? lines = MarkdownSection.ReadLines(facts.Body, SectionHeading);
        if (lines is null)
        {
            return
            [
                new GateCheck(
                    "RG 7",
                    GateResult.Fault,
                    $"the description holds no `{SectionHeading}` section (D-577, D-581)."),
                new GateCheck("RG 8", GateResult.Skip, "RG 7 found no Documents section."),
            ];
        }

        List<string> faults = [];
        List<string> deferrals = [];
        foreach (string row in RequiredRows)
        {
            string? content = ReadRow(lines, row);
            if (content is null)
            {
                faults.Add($"the row `{row}` has no line");
                continue;
            }

            string? formFault = FormFault(row, content);
            if (formFault is not null)
            {
                faults.Add(formFault);
            }

            string? phrase = DeferralPhrase(content);
            if (phrase is not null)
            {
                deferrals.Add($"the row `{row}` holds the words `{phrase}`");
            }
        }

        GateCheck section = faults.Count == 0
            ? new GateCheck("RG 7", GateResult.Pass, $"each of the {RequiredRows.Count} rows has a line of D-581.")
            : new GateCheck("RG 7", GateResult.Fault, string.Join(". ", faults) + " (D-577, D-581).");
        GateCheck deferral = deferrals.Count == 0
            ? new GateCheck("RG 8", GateResult.Pass, "no line defers a document or a record of this PR.")
            : new GateCheck("RG 8", GateResult.Fault, string.Join(". ", deferrals) + " (D-577, D-578).");
        return [section, deferral];
    }

    /// <summary>Reads the text after the colon of one row of the Documents section.</summary>
    /// <param name="lines">The lines of the Documents section.</param>
    /// <param name="row">The name of the row, as `RequiredRows` holds it.</param>
    /// <returns>The text after the colon, or null when the section holds no line for the row.</returns>
    public static string? ReadRow(IReadOnlyList<string> lines, string row)
    {
        ArgumentNullException.ThrowIfNull(lines);
        ArgumentException.ThrowIfNullOrEmpty(row);

        string key = Normalize(row) + ":";
        foreach (string line in lines)
        {
            string text = Normalize(line);
            if (text.StartsWith(key, StringComparison.Ordinal))
            {
                return text[key.Length..].Trim();
            }
        }

        return null;
    }

    /// <summary>Reads one Documents line and gives the fault of its form (D-581).</summary>
    /// <param name="row">The name of the row, for the message.</param>
    /// <param name="content">The text after the colon of that row.</param>
    /// <returns>The fault, or null when the line holds one of the three forms.</returns>
    public static string? FormFault(string row, string content)
    {
        ArgumentNullException.ThrowIfNull(content);
        if (content.Length == 0)
        {
            return $"the row `{row}` has an empty line";
        }

        bool handoff = string.Equals(row, HandoffPath, StringComparison.Ordinal);
        if (content.StartsWith(ChangedForm, StringComparison.Ordinal))
        {
            string rest = content[ChangedForm.Length..].Trim();
            return HoldsPath(rest) && WordCount(rest) >= 4
                ? null
                : $"the row `{row}` gives no path and no reason after `{ChangedForm}`";
        }

        if (handoff)
        {
            return $"the row `{row}` is not `{ChangedForm}`, and every session adds an entry (D-18)";
        }

        if (content.StartsWith(NoChangeForm, StringComparison.Ordinal))
        {
            string rest = content[NoChangeForm.Length..].Trim();
            return HoldsPath(rest) ? null : $"the row `{row}` names no path after `{NoChangeForm}`";
        }

        if (content.StartsWith(NotApplicableForm, StringComparison.Ordinal))
        {
            string rest = content[NotApplicableForm.Length..].Trim();
            return WordCount(rest) >= 3 ? null : $"the row `{row}` gives no specific reason after `{NotApplicableForm}`";
        }

        return $"the row `{row}` holds no form of D-581";
    }

    /// <summary>Reads one Documents line and gives the words of a deferral (D-577, D-578).</summary>
    /// <param name="content">The text after the colon of one row.</param>
    /// <returns>The first deferral phrase of the line, or null when the line holds none.</returns>
    public static string? DeferralPhrase(string content)
    {
        ArgumentNullException.ThrowIfNull(content);
        string text = content.ToLowerInvariant();
        foreach (string phrase in DeferralPhrases)
        {
            if (text.Contains(phrase, StringComparison.Ordinal))
            {
                return phrase;
            }
        }

        return null;
    }

    private static string Normalize(string line)
    {
        string text = line.Trim();
        if (text.StartsWith("- ", StringComparison.Ordinal) || text.StartsWith("* ", StringComparison.Ordinal))
        {
            text = text[2..];
        }

        return text.Replace("`", string.Empty, StringComparison.Ordinal).Trim();
    }

    private static bool HoldsPath(string text) =>
        text.Contains('/', StringComparison.Ordinal) || text.Contains(".md", StringComparison.Ordinal);

    private static int WordCount(string text) =>
        text.Split(' ', StringSplitOptions.RemoveEmptyEntries | StringSplitOptions.TrimEntries).Length;
}
