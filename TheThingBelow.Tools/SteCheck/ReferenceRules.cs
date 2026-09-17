using System;
using System.Collections.Generic;
using System.Text.RegularExpressions;

namespace TheThingBelow.Tools.SteCheck;

/// <summary>
/// The reference check of D-605. It reads each citation of a live document: an id that a
/// register holds, a path of this repository that a file holds, and a superseded decision
/// that names the decision which superseded it.
/// </summary>
public static class ReferenceRules
{
    /// <summary>
    /// The top-level folders of this repository. A path in backticks that starts with another
    /// name points outside the repository, and the path rule reads none of them.
    /// </summary>
    public static readonly IReadOnlyList<string> RepositoryRoots =
    [
        ".claude", ".github", "content", "docs",
        "TheThingBelow.Core", "TheThingBelow.Game", "TheThingBelow.Tests", "TheThingBelow.Tools",
    ];

    /// <summary>The file types that make a bare name a path of this repository.</summary>
    public static readonly IReadOnlyList<string> PathExtensions =
    [
        ".md", ".cs", ".csproj", ".slnx", ".json", ".py", ".yml", ".yaml", ".props",
        ".godot", ".png", ".wav", ".ttf",
    ];

    /// <summary>
    /// The decision register. Its Effect column records each supersession, and a row of it is a
    /// record of one day, so the superseded-citation rule reads every other document (D-606).
    /// </summary>
    public const string DecisionRegisterPath = "docs/decisions.md";

    private const RegexOptions Options = RegexOptions.CultureInvariant | RegexOptions.Compiled;
    private static readonly Regex CodeSpan = new Regex("`([^`]+)`", Options);
    private static readonly Regex AnyId = new Regex(@"\b(D|OQ|F|G|T|L|M|PR)-(\d+)\b", Options);
    private static readonly Regex DecisionId = new Regex(@"\bD-(\d+)\b", Options);
    private static readonly Regex PullRequestMark = new Regex(@"\bPR-\d+\b", Options);
    private static readonly char[] ForbiddenInPath = ['<', '>', '*', '?', '|', '"', '\\', ' ', '\t'];

    /// <summary>Reads one live document and gives every reference finding of it.</summary>
    /// <param name="path">The path of the document, relative to the root of the checkout.</param>
    /// <param name="documents">The file set of the checkout, which resolves each path.</param>
    /// <param name="register">The ids of every register, and the supersession map.</param>
    /// <returns>Each finding, in the order of the lines.</returns>
    public static IReadOnlyList<Finding> Check(string path, DocumentSet documents, IdRegister register)
    {
        ArgumentException.ThrowIfNullOrEmpty(path);
        ArgumentNullException.ThrowIfNull(documents);
        ArgumentNullException.ThrowIfNull(register);

        List<Finding> findings = [];
        IReadOnlyList<string> lines = documents.ReadLines(path);
        bool inCodeBlock = false;
        bool inFrontMatter = false;
        for (int index = 0; index < lines.Count; index++)
        {
            int number = index + 1;
            string line = lines[index];
            string trimmed = line.Trim();
            if (number == 1 && trimmed == "---")
            {
                inFrontMatter = true;
                continue;
            }

            if (inFrontMatter)
            {
                inFrontMatter = trimmed != "---";
                continue;
            }

            if (trimmed.StartsWith("```", StringComparison.Ordinal))
            {
                inCodeBlock = !inCodeBlock;
                continue;
            }

            if (inCodeBlock)
            {
                continue;
            }

            ReadIds(path, number, line, register, findings);
            ReadPaths(path, number, line, documents, findings);
            if (!string.Equals(path, DecisionRegisterPath, StringComparison.Ordinal))
            {
                ReadSupersededDecisions(path, number, line, register, findings);
            }
        }

        return findings;
    }

    private static void ReadIds(
        string path, int number, string line, IdRegister register, List<Finding> findings)
    {
        foreach (Match match in AnyId.Matches(line))
        {
            if (!register.Holds(match.Value))
            {
                findings.Add(new Finding(
                    path, number, "REF 1", $"no register holds the id '{match.Value}'"));
            }
        }
    }

    private static void ReadPaths(
        string path, int number, string line, DocumentSet documents, List<Finding> findings)
    {
        // A line that names the PR which creates a file marks that path (G-16).
        if (PullRequestMark.IsMatch(line))
        {
            return;
        }

        foreach (Match match in CodeSpan.Matches(line))
        {
            string candidate = match.Groups[1].Value.TrimEnd('/');
            if (!IsRepositoryPath(candidate) || Resolves(candidate, path, documents))
            {
                continue;
            }

            findings.Add(new Finding(
                path, number, "REF 2", $"the checkout holds no file at the path '{candidate}'"));
        }
    }

    private static void ReadSupersededDecisions(
        string path, int number, string line, IdRegister register, List<Finding> findings)
    {
        foreach (Match match in DecisionId.Matches(line))
        {
            IReadOnlyList<string> superseding = register.SupersedingDecisions(match.Value);
            if (superseding.Count == 0 || NamesOneOf(line, superseding))
            {
                continue;
            }

            findings.Add(new Finding(
                path,
                number,
                "REF 3",
                $"the citation of '{match.Value}' names no decision that superseded it: "
                + string.Join(", ", superseding)));
        }
    }

    /// <summary>Reads a code span as a path of this repository.</summary>
    /// <param name="candidate">The text between the backticks, with no trailing slash.</param>
    /// <returns>True when the path rule reads the candidate.</returns>
    public static bool IsRepositoryPath(string candidate)
    {
        ArgumentNullException.ThrowIfNull(candidate);
        if (candidate.Length == 0
            || candidate.IndexOfAny(ForbiddenInPath) >= 0
            || candidate.StartsWith('/')
            || candidate.StartsWith('~')
            || candidate.StartsWith('-')
            || candidate.StartsWith("http", StringComparison.Ordinal))
        {
            return false;
        }

        int firstSlash = candidate.IndexOf('/', StringComparison.Ordinal);
        if (firstSlash > 0)
        {
            string firstSegment = candidate[..firstSlash];
            foreach (string root in RepositoryRoots)
            {
                if (string.Equals(firstSegment, root, StringComparison.Ordinal))
                {
                    return true;
                }
            }

            return false;
        }

        // A bare name is a path when it names a file type of this repository.
        foreach (string extension in PathExtensions)
        {
            if (candidate.EndsWith(extension, StringComparison.Ordinal)
                && candidate.Length > extension.Length)
            {
                return true;
            }
        }

        return false;
    }

    private static bool Resolves(string candidate, string citingDocument, DocumentSet documents)
    {
        if (documents.Holds(candidate))
        {
            return true;
        }

        // A document cites a sibling file by its name alone, and a skill cites its own folder.
        string folder = FolderOf(citingDocument);
        if (folder.Length > 0 && documents.Holds(folder + "/" + candidate))
        {
            return true;
        }

        string parent = FolderOf(folder);
        if (parent.Length > 0 && documents.Holds(parent + "/" + candidate))
        {
            return true;
        }

        return documents.CountEndingWith(candidate) == 1;
    }

    private static string FolderOf(string path)
    {
        int lastSlash = path.LastIndexOf('/');
        return lastSlash < 0 ? string.Empty : path[..lastSlash];
    }

    /// <summary>
    /// Reads the line for a citation of one of the ids. A digit after the id makes another id,
    /// so `D-10` does not satisfy a rule that asks for `D-1`.
    /// </summary>
    private static bool NamesOneOf(string line, IReadOnlyList<string> ids)
    {
        foreach (string id in ids)
        {
            int index = line.IndexOf(id, StringComparison.Ordinal);
            while (index >= 0)
            {
                int after = index + id.Length;
                if (after >= line.Length || !char.IsDigit(line[after]))
                {
                    return true;
                }

                index = line.IndexOf(id, after, StringComparison.Ordinal);
            }
        }

        return false;
    }
}
