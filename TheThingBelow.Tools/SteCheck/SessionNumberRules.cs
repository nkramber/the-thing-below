using System;
using System.Collections.Generic;
using System.Text.RegularExpressions;

namespace TheThingBelow.Tools.SteCheck;

/// <summary>
/// The session number check of D-18 and L-12. Two providers picked the same number on one day,
/// so a machine reads the handoff and its archive as one list, newest first.
/// </summary>
public static class SessionNumberRules
{
    /// <summary>The handoff, then the archive. The two files hold one descending list (D-18).</summary>
    public static readonly IReadOnlyList<string> HandoffPaths =
    [
        "docs/session-handoff.md",
        "docs/session-handoff-archive.md",
    ];

    /// <summary>The number of entries that the handoff keeps. Any older entry moves to the archive (D-18).</summary>
    public const int HandoffEntryLimit = 10;

    private static readonly Regex SessionHeading = new Regex(
        @"^## Session (\d+):", RegexOptions.CultureInvariant | RegexOptions.Compiled);

    /// <summary>Reads whether a line is the heading of a session entry (D-18).</summary>
    /// <param name="line">One line of the handoff or its archive.</param>
    /// <returns>True when the line has the form `## Session N:`, with a number for N.</returns>
    public static bool IsSessionHeading(string line)
    {
        ArgumentNullException.ThrowIfNull(line);
        return SessionHeading.IsMatch(line);
    }

    /// <summary>Reads the handoff and the archive, and gives every session number finding.</summary>
    /// <param name="documents">The file set of the checkout.</param>
    /// <returns>Each finding, in the order of the entries.</returns>
    /// <exception cref="InvalidOperationException">A handoff file is absent, or it holds no entry.</exception>
    public static IReadOnlyList<Finding> Check(DocumentSet documents)
    {
        ArgumentNullException.ThrowIfNull(documents);

        List<Finding> findings = [];
        Dictionary<int, string> firstUse = [];
        int previousNumber = int.MaxValue;
        int entries = 0;
        int handoffEntries = 0;
        int handoffLimitLine = 0;

        foreach (string path in HandoffPaths)
        {
            if (!documents.Holds(path))
            {
                throw new InvalidOperationException($"The handoff file '{path}' is absent (T-2).");
            }

            IReadOnlyList<string> lines = documents.ReadLines(path);
            for (int index = 0; index < lines.Count; index++)
            {
                Match heading = SessionHeading.Match(lines[index]);
                if (!heading.Success)
                {
                    continue;
                }

                int number = int.Parse(heading.Groups[1].Value, System.Globalization.CultureInfo.InvariantCulture);
                int line = index + 1;
                entries++;
                if (string.Equals(path, HandoffPaths[0], StringComparison.Ordinal))
                {
                    handoffEntries++;
                    if (handoffEntries == HandoffEntryLimit + 1)
                    {
                        handoffLimitLine = line;
                    }
                }

                if (firstUse.TryGetValue(number, out string? earlier))
                {
                    findings.Add(new Finding(
                        path, line, "HANDOFF 1", $"session {number} appears again. The first entry is at {earlier} (L-12)"));
                }
                else
                {
                    firstUse[number] = $"{path}:{line}";
                    if (number >= previousNumber)
                    {
                        findings.Add(new Finding(
                            path, line, "HANDOFF 2", $"session {number} comes after session {previousNumber}. The entries run newest first (D-18)"));
                    }
                }

                previousNumber = number;
            }
        }

        if (handoffEntries > HandoffEntryLimit)
        {
            findings.Add(new Finding(
                HandoffPaths[0],
                handoffLimitLine,
                "HANDOFF 3",
                $"the handoff holds {handoffEntries} entries. It keeps the {HandoffEntryLimit} newest, and every older entry moves to `{HandoffPaths[1]}` (D-18)"));
        }

        if (entries == 0)
        {
            throw new InvalidOperationException(
                "The handoff and its archive hold no entry that starts with '## Session ' (T-2).");
        }

        return findings;
    }
}
