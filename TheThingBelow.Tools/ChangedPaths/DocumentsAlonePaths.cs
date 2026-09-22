using System;
using System.Collections.Generic;

namespace TheThingBelow.Tools.ChangedPaths;

/// <summary>
/// The skip set: the paths that hold documents alone, and no code, content, or workflow
/// (D-600, D-857). A change inside this set runs ste-check, review-gate,
/// and Gitar alone (D-856). This set and the override set of D-16 are not the same set.
/// </summary>
public static class DocumentsAlonePaths
{
    /// <summary>Each folder of the set, with its closing slash. `.claude/settings.json` is inside it (D-857).</summary>
    public static readonly IReadOnlyList<string> Folders = ["docs/", ".claude/"];

    /// <summary>
    /// Each file of the set. The two instructions files are in it because the ste-check job
    /// reads the rule of D-20 on every PR (D-857).
    /// </summary>
    public static readonly IReadOnlyList<string> Files =
    [
        "AGENTS.md",
        "CLAUDE.md",
        "LICENSE",
        "README.md",
        ".github/pull_request_template.md",
    ];

    /// <summary>Reads whether one path is inside the set.</summary>
    /// <param name="path">A path from the root of the checkout, with forward slashes, as git gives it.</param>
    /// <returns>True when the path is inside a folder of the set or is a file of the set.</returns>
    public static bool Holds(string path)
    {
        ArgumentException.ThrowIfNullOrEmpty(path);

        foreach (string folder in Folders)
        {
            if (path.StartsWith(folder, StringComparison.Ordinal))
            {
                return true;
            }
        }

        foreach (string file in Files)
        {
            if (string.Equals(path, file, StringComparison.Ordinal))
            {
                return true;
            }
        }

        return false;
    }

    /// <summary>Gives the first path that is outside the set.</summary>
    /// <param name="paths">The paths that a change holds.</param>
    /// <returns>The first path outside the set, or null when every path is inside it.</returns>
    public static string? FirstPathOutside(IReadOnlyList<string> paths)
    {
        ArgumentNullException.ThrowIfNull(paths);

        foreach (string path in paths)
        {
            if (!Holds(path))
            {
                return path;
            }
        }

        return null;
    }
}
