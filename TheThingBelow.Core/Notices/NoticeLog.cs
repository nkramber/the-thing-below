using System;
using System.Collections.Generic;
using TheThingBelow.Core.Content;
using TheThingBelow.Core.Hashing;

namespace TheThingBelow.Core.Notices;

/// <summary>
/// The notice log of a run: the newest notices that content marks to log, oldest first
/// (D-221, D-983, D-984). The snapshot holds it from save format 8 (D-985).
/// </summary>
/// <remarks>
/// The log keeps <see cref="MostEntries"/> entries. A new entry past that count removes the
/// oldest one (D-984). The log window of Game lists the entries newest first (D-987).
/// </remarks>
public sealed class NoticeLog
{
    /// <summary>The count of entries that the log keeps (D-984).</summary>
    public const int MostEntries = 30;

    private readonly List<ContentId> entries;

    private NoticeLog(List<ContentId> entries)
    {
        this.entries = entries;
    }

    /// <summary>Every entry, oldest first.</summary>
    public IReadOnlyList<ContentId> Entries => this.entries;

    /// <summary>Starts the empty log of a new run, or of a save before format 8 (D-985).</summary>
    /// <returns>The log.</returns>
    public static NoticeLog Empty() => new([]);

    /// <summary>Puts the log back from the ids of a snapshot (D-166, D-985).</summary>
    /// <param name="stored">The stored ids, oldest first.</param>
    /// <param name="notices">The notice file of this build.</param>
    /// <param name="source">What the values came from, such as `the save`, for an error (T-2).</param>
    /// <returns>The log.</returns>
    /// <exception cref="ArgumentNullException">An argument is null (T-2).</exception>
    /// <exception cref="ArgumentException">
    /// The ids pass the count of the log, or an id names no notice of this build that logs (T-2).
    /// </exception>
    public static NoticeLog Resume(IReadOnlyList<ContentId> stored, NoticeList notices, string source)
    {
        ArgumentNullException.ThrowIfNull(stored);
        ArgumentNullException.ThrowIfNull(notices);
        ArgumentException.ThrowIfNullOrEmpty(source);

        if (stored.Count > MostEntries)
        {
            throw new ArgumentException(
                $"The notice log of {source} is not a state of a run: it holds {stored.Count} entries, and the log keeps {MostEntries} (D-984).");
        }

        List<ContentId> entries = [];
        foreach (ContentId id in stored)
        {
            ArgumentNullException.ThrowIfNull(id);
            if (!notices.Holds(id) || !notices.Notice(id).Logs)
            {
                throw new ArgumentException(
                    $"The notice log of {source} is not a state of a run: it holds '{id.Value}', which is no notice of this build that logs (D-983).");
            }

            entries.Add(id);
        }

        return new NoticeLog(entries);
    }

    /// <summary>Adds every entry to the state hash, oldest first (G-5).</summary>
    /// <param name="hasher">The hasher of the state.</param>
    public void Hash(StateHasher hasher)
    {
        ArgumentNullException.ThrowIfNull(hasher);

        hasher.AddInt32(this.entries.Count);
        foreach (ContentId id in this.entries)
        {
            hasher.AddText(id.Value);
        }
    }

    /// <summary>Gives a copy of the entries for a snapshot (D-985).</summary>
    /// <returns>A new list, oldest first, which a later notice never changes.</returns>
    public IReadOnlyList<ContentId> Values() => [.. this.entries];

    /// <summary>Adds one entry, and removes the oldest past <see cref="MostEntries"/> (D-984).</summary>
    internal void Add(ContentId notice)
    {
        this.entries.Add(notice);
        if (this.entries.Count > MostEntries)
        {
            this.entries.RemoveAt(0);
        }
    }
}
