using System.Collections.Generic;

namespace TheThingBelow.Tests;

/// <summary>
/// The fixture documents of the `ste-check` command. One document fails each writing rule
/// one time, and one document passes every rule (PR-2 exit tests 2 and 3).
/// </summary>
public static class SteCheckFixtures
{
    /// <summary>One case of the failing document: the rule, its lines, and the line of the finding.</summary>
    /// <param name="Rule">The rule id that this case triggers.</param>
    /// <param name="Lines">The lines of the case, with no empty line between them.</param>
    /// <param name="FindingOffset">The 0-based line of the case that the finding names.</param>
    public sealed record Case(string Rule, string[] Lines, int FindingOffset);

    /// <summary>Each writing rule, with the text that fails it one time.</summary>
    public static IReadOnlyList<Case> FailingCases { get; } =
    [
        new Case(
            "STE 5.1",
            ["1. This numbered item holds one sentence of more than twenty words, and the rule of a numbered item allows twenty words at most."],
            0),
        new Case(
            "STE 6.3",
            ["- This bullet item holds one sentence of more than twenty five words, and the rule of every sentence outside a numbered item allows twenty five words at most in the text."],
            0),
        new Case("STE 8.1", ["The tool reads the file; the tool prints the finding."], 0),
        new Case("STE 4.2", ["The tool reads the file, and it isn't a parser of Markdown."], 0),
        new Case("STE 3.6", ["The finding is printed to the output of the tool."], 0),
        new Case("STE 3.2/3.4", ["The session should read the top handoff entry first."], 0),
        new Case("STE 3.5", ["Reading the top handoff entry comes first."], 0),
        new Case(
            "STE 6.6",
            [
                "The tool reads a file. The tool reads a line. The tool reads a word.",
                "The tool counts the words. The tool prints a finding. The tool adds the finding.",
                "The tool gives the count.",
            ],
            3),
        new Case("MD 1", ["<!-- This comment has no end on this line", "and it ends here. -->"], 0),
    ];

    /// <summary>A document that passes every writing rule.</summary>
    public static IReadOnlyList<string> PassingDocument { get; } =
    [
        "# The fixture that passes",
        string.Empty,
        "This document passes each rule of the checker. Each sentence stays short.",
        "The tool reads it and finds nothing.",
        string.Empty,
        "1. Read the file.",
        "2. Count the words of each sentence.",
        "3. Print one line for each finding.",
        string.Empty,
        "- A bullet item is one unit, and the rule of a descriptive sentence applies to it.",
        "- The tool can read this line, and it must print no finding.",
        string.Empty,
        "<!-- This comment stays on one line, so the tool removes it. -->",
        string.Empty,
        "| A table | is exempt |",
        "|---|---|",
        "| The cell text of a table should be long enough that no length rule reads it | yes |",
        string.Empty,
        "```",
        "This fenced block should hold text that no rule reads; it has a semicolon.",
        "```",
        string.Empty,
        "The tool reads the last paragraph of a file with no empty line after it.",
    ];

    /// <summary>Builds the document that fails each writing rule one time.</summary>
    /// <returns>The lines of the document, with one empty line after each case.</returns>
    public static IReadOnlyList<string> BuildFailingDocument()
    {
        List<string> lines = [];
        foreach (Case failing in FailingCases)
        {
            lines.AddRange(failing.Lines);
            lines.Add(string.Empty);
        }

        return lines;
    }

    /// <summary>Gives the 1-based line that the finding of a case names.</summary>
    /// <param name="rule">The rule id of the case.</param>
    /// <returns>The line number in the document that <see cref="BuildFailingDocument"/> builds.</returns>
    public static int LineOf(string rule)
    {
        int line = 1;
        foreach (Case failing in FailingCases)
        {
            if (failing.Rule == rule)
            {
                return line + failing.FindingOffset;
            }

            line += failing.Lines.Length + 1;
        }

        return 0;
    }
}
