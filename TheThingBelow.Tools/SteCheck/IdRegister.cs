using System;
using System.Collections.Generic;
using System.Text.RegularExpressions;

namespace TheThingBelow.Tools.SteCheck;

/// <summary>
/// The ids that the registers of the repository define, and the decisions that a later
/// decision superseded. The reference check reads both (D-605).
/// </summary>
public sealed class IdRegister
{
    /// <summary>The register file of each id prefix, and the pattern that defines an id there.</summary>
    private static readonly (string Prefix, string File, string Pattern)[] Definitions =
    [
        ("D", "docs/decisions.md", @"^\| D-(\d+) \|"),
        ("OQ", "docs/questions.md", @"\*\*OQ-(\d+)\."),
        ("F", "docs/design.md", @"^\| F-(\d+) \|"),
        ("G", "docs/design.md", @"\*\*G-(\d+)\."),
        ("T", "docs/design.md", @"\*\*T-(\d+)\."),
        ("L", "docs/design.md", @"\*\*L-(\d+)\."),
        ("M", "docs/design.md", @"^- M-(\d+):"),
    ];

    /// <summary>The heading of the section of `docs/design.md` that lists every PR (D-605).</summary>
    private const string SequenceHeading = "## 8. Sequence";

    /// <summary>The start of the name of each phase file, which holds one entry per PR (D-144).</summary>
    private const string PhaseFilePrefix = "docs/roadmaps/phase-";

    private const RegexOptions Options = RegexOptions.CultureInvariant | RegexOptions.Compiled;
    private static readonly Regex PullRequestId = new Regex(@"\bPR-(\d+)\b", Options);
    private static readonly Regex PullRequestHeading = new Regex(@"^#{2,4} [\d.]+ PR-(\d+):", Options);
    private static readonly Regex SupersededBy = new Regex(@"Superseded by ((?:D-\d+(?: and )?)+)", Options);
    private static readonly Regex DecisionId = new Regex(@"D-(\d+)", Options);
    private static readonly Regex DecisionRow = new Regex(@"^\| (D-\d+) \|", Options);

    private readonly HashSet<string> defined = new HashSet<string>(StringComparer.Ordinal);
    private readonly Dictionary<string, List<string>> supersededBy =
        new Dictionary<string, List<string>>(StringComparer.Ordinal);

    private IdRegister()
    {
    }

    /// <summary>Every prefix that the registers define. A citation of another prefix passes.</summary>
    public static IReadOnlyList<string> KnownPrefixes { get; } = ["D", "OQ", "F", "G", "T", "L", "M", "PR"];

    /// <summary>Builds the register from the document set of the checkout.</summary>
    /// <param name="documents">The file set of the checkout.</param>
    /// <returns>The ids of every register, and the supersession map.</returns>
    public static IdRegister Read(DocumentSet documents)
    {
        ArgumentNullException.ThrowIfNull(documents);
        IdRegister register = new IdRegister();
        foreach ((string prefix, string file, string pattern) in Definitions)
        {
            register.ReadIds(documents, prefix, file, new Regex(pattern, Options));
        }

        register.ReadPullRequestIds(documents);
        register.ReadSupersessions(documents);
        return register;
    }

    /// <summary>Reads whether a register defines an id.</summary>
    /// <param name="id">The id, such as `D-604`.</param>
    /// <returns>True when a register holds the id.</returns>
    public bool Holds(string id)
    {
        ArgumentException.ThrowIfNullOrEmpty(id);
        return defined.Contains(id);
    }

    /// <summary>Gives the decisions that superseded this one, or an empty list.</summary>
    /// <param name="id">A decision id, such as `D-80`.</param>
    /// <returns>Each decision that supersedes the id.</returns>
    public IReadOnlyList<string> SupersedingDecisions(string id)
    {
        ArgumentException.ThrowIfNullOrEmpty(id);
        return supersededBy.TryGetValue(id, out List<string>? ids) ? ids : [];
    }

    private void ReadIds(DocumentSet documents, string prefix, string file, Regex pattern)
    {
        if (!documents.Holds(file))
        {
            throw new InvalidOperationException(
                $"The register '{file}' of the prefix '{prefix}-' is absent from the checkout (T-2).");
        }

        foreach (string line in documents.ReadLines(file))
        {
            foreach (Match match in pattern.Matches(line))
            {
                defined.Add($"{prefix}-{match.Groups[1].Value}");
            }
        }
    }

    /// <summary>
    /// Section 8 of `docs/design.md` gives the strict order of every live PR, and each phase file
    /// gives one entry per PR. A retired PR keeps its entry and never its place in the order
    /// (G-10), so the two sources together are the register of the `PR-` prefix (D-605).
    /// </summary>
    private void ReadPullRequestIds(DocumentSet documents)
    {
        foreach (string document in documents.LiveDocuments)
        {
            if (!document.StartsWith(PhaseFilePrefix, StringComparison.Ordinal))
            {
                continue;
            }

            foreach (string line in documents.ReadLines(document))
            {
                Match heading = PullRequestHeading.Match(line);
                if (heading.Success)
                {
                    defined.Add($"PR-{heading.Groups[1].Value}");
                }
            }
        }

        bool inSequence = false;
        bool found = false;
        foreach (string line in documents.ReadLines("docs/design.md"))
        {
            if (line.StartsWith("## ", StringComparison.Ordinal))
            {
                inSequence = line.StartsWith(SequenceHeading, StringComparison.Ordinal);
                found |= inSequence;
                continue;
            }

            if (!inSequence)
            {
                continue;
            }

            foreach (Match match in PullRequestId.Matches(line))
            {
                defined.Add($"PR-{match.Groups[1].Value}");
            }
        }

        if (!found)
        {
            throw new InvalidOperationException(
                $"`docs/design.md` holds no section that starts with '{SequenceHeading}' (T-2).");
        }
    }

    private void ReadSupersessions(DocumentSet documents)
    {
        foreach (string line in documents.ReadLines("docs/decisions.md"))
        {
            Match row = DecisionRow.Match(line);
            if (!row.Success)
            {
                continue;
            }

            List<string> superseding = [];
            foreach (Match match in SupersededBy.Matches(line))
            {
                foreach (Match id in DecisionId.Matches(match.Groups[1].Value))
                {
                    superseding.Add(id.Value);
                }
            }

            if (superseding.Count > 0)
            {
                supersededBy[row.Groups[1].Value] = superseding;
            }
        }
    }
}
