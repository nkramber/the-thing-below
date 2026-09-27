using System;
using System.Collections.Generic;
using System.Globalization;
using System.Text.Json;
using TheThingBelow.Tools.ReviewGate;

namespace TheThingBelow.Tools.Night;

/// <summary>One commit of a PR, as the GitHub API gives it: the hash, the count of parents, and each changed path.</summary>
/// <param name="Sha">The full hash of the commit.</param>
/// <param name="Parents">The count of parents of the commit.</param>
/// <param name="Files">Each path that the commit changes, with the old path of each rename.</param>
public sealed record NightCommit(string Sha, int Parents, IReadOnlyList<string> Files);

/// <summary>
/// The walk of D-1204: a green night on a commit of a PR passes each later head when each later
/// commit changes paths of a docs-only PR alone (D-513). The night gate and the promotion read
/// the same walk, so both accept the same nights.
/// </summary>
public static class NightWalk
{
    private static readonly string[] Fields = ["sha", "parents", "files"];

    /// <summary>Reads the commits of a PR from the JSON file that the workflow writes, oldest first.</summary>
    /// <param name="path">The file: an array of objects with `sha`, `parents`, and `files`.</param>
    /// <returns>Each commit, in the order of the file.</returns>
    /// <exception cref="InvalidOperationException">The file holds no valid list of commits (T-2).</exception>
    public static IReadOnlyList<NightCommit> ReadCommits(string path)
    {
        ArgumentException.ThrowIfNullOrEmpty(path);

        string where = $"the commits of the PR '{path}'";
        using JsonDocument document = NightJson.Parse(path, where);
        JsonElement root = document.RootElement;
        if (root.ValueKind != JsonValueKind.Array || root.GetArrayLength() == 0)
        {
            throw new InvalidOperationException($"{where} holds '{root.ValueKind}' with no commit, and a PR holds at least one commit.");
        }

        List<NightCommit> commits = [];
        foreach (JsonElement item in root.EnumerateArray())
        {
            NightJson.RequireFields(item, where, Fields);
            string sha = NightJson.Text(item, "sha", where);
            if (!NightJson.IsCommit(sha))
            {
                throw new InvalidOperationException($"The field 'sha' of {where} holds '{sha}', and the walk needs a full commit hash.");
            }

            int parents = (int)NightJson.Whole(item, "parents", where, 0);
            JsonElement list = item.GetProperty("files");
            if (list.ValueKind != JsonValueKind.Array)
            {
                throw new InvalidOperationException($"The field 'files' of the commit {sha} in {where} holds '{list.ValueKind}', and the walk needs an array.");
            }

            List<string> files = [];
            foreach (JsonElement file in list.EnumerateArray())
            {
                string? name = file.ValueKind == JsonValueKind.String ? file.GetString() : null;
                files.Add(string.IsNullOrEmpty(name)
                    ? throw new InvalidOperationException($"The field 'files' of the commit {sha} in {where} holds the item '{file}', and each item is a path.")
                    : name);
            }

            commits.Add(new NightCommit(sha, parents, files));
        }

        return commits;
    }

    /// <summary>
    /// Gives each commit whose night can pass the head: the head, then each earlier commit while
    /// each later commit changes paths of a docs-only PR alone. A commit with more or less than
    /// one parent, or with no changed path, ends the walk (D-1204).
    /// </summary>
    /// <param name="commits">The commits of the PR, oldest first.</param>
    /// <param name="head">The full hash of the head commit of the PR.</param>
    /// <returns>The hash of each such commit, newest first. The first item is the head.</returns>
    /// <exception cref="InvalidOperationException">The last commit of the list is not the head (T-2).</exception>
    public static IReadOnlyList<string> CandidatesOf(IReadOnlyList<NightCommit> commits, string head)
    {
        ArgumentNullException.ThrowIfNull(commits);
        ArgumentException.ThrowIfNullOrEmpty(head);
        if (commits.Count == 0 || !string.Equals(commits[^1].Sha, head, StringComparison.Ordinal))
        {
            string last = commits.Count == 0 ? "no commit" : commits[^1].Sha;
            throw new InvalidOperationException($"The last of the {commits.Count.ToString(CultureInfo.InvariantCulture)} commits of the PR is {last}, and the head of the PR is {head}. The list is out of date or out of order (D-1204, T-2).");
        }

        List<string> candidates = [head];
        for (int index = commits.Count - 1; index > 0; index -= 1)
        {
            if (!ChangesDocumentsAlone(commits[index]))
            {
                break;
            }

            candidates.Add(commits[index - 1].Sha);
        }

        return candidates;
    }

    /// <summary>Tells whether a commit changes paths of a docs-only PR alone, with one parent and at least one path (D-513, D-1204).</summary>
    /// <param name="commit">The commit.</param>
    /// <returns>True when a night before the commit still plays the code of the commit.</returns>
    public static bool ChangesDocumentsAlone(NightCommit commit)
    {
        ArgumentNullException.ThrowIfNull(commit);

        // A merge commit brings the code of another branch, and a commit with no path can be a
        // commit that the API did not list in full. Each one ends the walk (T-2).
        if (commit.Parents != 1 || commit.Files.Count == 0)
        {
            return false;
        }

        return FirstCodePath(commit.Files) is null;
    }

    /// <summary>Gives the first path outside the paths of a docs-only PR, or no value (D-513).</summary>
    /// <param name="files">The paths.</param>
    /// <returns>The first such path, or null when each path is a path of a docs-only PR.</returns>
    public static string? FirstCodePath(IReadOnlyList<string> files)
    {
        ArgumentNullException.ThrowIfNull(files);
        foreach (string file in files)
        {
            if (!OverrideRules.IsEligible(file))
            {
                return file;
            }
        }

        return null;
    }
}
