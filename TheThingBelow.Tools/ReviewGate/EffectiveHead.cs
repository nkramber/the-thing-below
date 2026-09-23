using System;
using System.Collections.Generic;
using System.Globalization;
using TheThingBelow.Tools.ChangedPaths;

namespace TheThingBelow.Tools.ReviewGate;

/// <summary>
/// The effective head of a pull request. It is the newest commit that changes a path outside
/// the metadata set (D-603, D-610). A review record names that commit, and a commit that changes
/// the metadata set alone never makes the record stale.
/// </summary>
public static class EffectiveHead
{
    /// <summary>
    /// The metadata set of one pull request (D-610). It holds the two review files of this pull
    /// request and the two handoff files. The record of another pull request is not in the set,
    /// so a change to that record moves the effective head.
    /// </summary>
    /// <param name="number">The GitHub number of the pull request.</param>
    /// <returns>The four paths of the metadata set, from the root of the checkout.</returns>
    public static IReadOnlyList<string> MetadataPaths(int number)
    {
        string text = number.ToString(CultureInfo.InvariantCulture);
        return
        [
            $"docs/reviews/pr-{text}.md",
            $"docs/reviews/pr-{text}-response.md",
            "docs/session-handoff.md",
            "docs/session-handoff-archive.md",
        ];
    }

    /// <summary>Finds the newest commit that changes a path outside the metadata set.</summary>
    /// <param name="commits">Each commit from the merge base to the head, oldest first.</param>
    /// <param name="number">The GitHub number of the pull request.</param>
    /// <returns>
    /// The commit that a review record must name, or null when every commit changes the metadata
    /// set alone. Such a pull request records the work of an earlier pull request, which D-578
    /// refuses.
    /// </returns>
    public static CommitFacts? Find(IReadOnlyList<CommitFacts> commits, int number)
    {
        ArgumentNullException.ThrowIfNull(commits);
        HashSet<string> metadata = new HashSet<string>(MetadataPaths(number), StringComparer.Ordinal);
        for (int index = commits.Count - 1; index >= 0; index--)
        {
            if (ChangesOutside(commits[index], metadata))
            {
                return commits[index];
            }
        }

        return null;
    }

    /// <summary>
    /// Finds each commit that an approved review record can name for the `review-gate` check
    /// (D-943). The effective head is always one of them. An earlier commit outside the metadata
    /// set is one of them too when each commit after it changes paths of the skip set alone
    /// (D-857). Thus a commit of documents alone after an approval keeps the approval.
    /// </summary>
    /// <param name="commits">Each commit from the merge base to the head, oldest first.</param>
    /// <param name="number">The GitHub number of the pull request.</param>
    /// <returns>
    /// The commits newest first, with the effective head first. The list is empty when every
    /// commit changes the metadata set alone.
    /// </returns>
    public static IReadOnlyList<CommitFacts> ReviewableHeads(IReadOnlyList<CommitFacts> commits, int number)
    {
        ArgumentNullException.ThrowIfNull(commits);
        HashSet<string> metadata = new HashSet<string>(MetadataPaths(number), StringComparer.Ordinal);
        List<CommitFacts> heads = [];
        for (int index = commits.Count - 1; index >= 0; index--)
        {
            CommitFacts commit = commits[index];
            if (ChangesOutside(commit, metadata))
            {
                heads.Add(commit);
            }

            // A commit outside the skip set ends the walk. A review of an earlier commit never
            // read it, so no earlier commit can keep an approval.
            if (DocumentsAlonePaths.FirstPathOutside(commit.Files) is not null)
            {
                break;
            }
        }

        return heads;
    }

    private static bool ChangesOutside(CommitFacts commit, HashSet<string> metadata)
    {
        foreach (string file in commit.Files)
        {
            if (!metadata.Contains(file))
            {
                return true;
            }
        }

        return false;
    }
}
