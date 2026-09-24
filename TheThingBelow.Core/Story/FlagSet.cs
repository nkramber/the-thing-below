using System;
using System.Collections.Generic;
using TheThingBelow.Core.Content;
using TheThingBelow.Core.Hashing;

namespace TheThingBelow.Core.Story;

/// <summary>The story flags that are on in a run (D-542).</summary>
/// <remarks>
/// A flag is on or off, and a flag that the set does not hold is off. No rule turns a flag
/// off, because a flag id is permanent and the story remembers each event for good (D-163,
/// D-542). The set walks its ids in ordinal order, so a snapshot and the state hash read them
/// in one order on every machine (G-4, T-7).
/// </remarks>
public sealed class FlagSet
{
    private readonly SortedSet<string> on = new(StringComparer.Ordinal);

    private FlagSet()
    {
    }

    /// <summary>The count of the flags that are on.</summary>
    public int Count => this.on.Count;

    /// <summary>Gives an empty set, which a new run starts with.</summary>
    /// <returns>The set.</returns>
    public static FlagSet Empty() => new();

    /// <summary>Puts the set back from the values of a snapshot (D-166).</summary>
    /// <param name="ids">The stored ids, in ordinal order.</param>
    /// <param name="flags">The flag file of this build, which must declare each id (D-542).</param>
    /// <param name="source">What the values came from, such as `the save`, for an error (T-2).</param>
    /// <returns>The set.</returns>
    /// <exception cref="ArgumentNullException">An argument is null (T-2).</exception>
    /// <exception cref="ArgumentException">An id repeats, leaves the ordinal order, or has no line in the flag file (T-2).</exception>
    public static FlagSet Resume(IReadOnlyList<ContentId> ids, FlagList flags, string source)
    {
        ArgumentNullException.ThrowIfNull(ids);
        ArgumentNullException.ThrowIfNull(flags);
        ArgumentException.ThrowIfNullOrEmpty(source);

        FlagSet set = new();
        string? last = null;
        foreach (ContentId id in ids)
        {
            ArgumentNullException.ThrowIfNull(id);
            if (!flags.Declares(id))
            {
                throw new ArgumentException($"The flags of {source} hold '{id.Value}', which the flag file of this build does not declare (D-542, D-1003).", nameof(ids));
            }

            if (last is not null && string.CompareOrdinal(last, id.Value) >= 0)
            {
                throw new ArgumentException($"The flags of {source} repeat or leave the ordinal order at '{id.Value}' (G-4).", nameof(ids));
            }

            set.on.Add(id.Value);
            last = id.Value;
        }

        return set;
    }

    /// <summary>Tells whether one flag is on.</summary>
    /// <param name="id">The id of the flag.</param>
    /// <returns>True when the flag is on.</returns>
    /// <exception cref="ArgumentNullException">The id is null (T-2).</exception>
    public bool IsOn(ContentId id)
    {
        ArgumentNullException.ThrowIfNull(id);

        return this.on.Contains(id.Value);
    }

    /// <summary>Turns one flag on. A flag that is on stays on (D-542).</summary>
    /// <param name="id">The id of the flag.</param>
    /// <returns>True when the flag was off before this call.</returns>
    /// <exception cref="ArgumentNullException">The id is null (T-2).</exception>
    /// <remarks>
    /// Two story scenes can each set one flag, such as a flag for a first meeting, so a second
    /// set is legal. The runner logs whether the set changed the flag (D-179).
    /// </remarks>
    public bool TurnOn(ContentId id)
    {
        ArgumentNullException.ThrowIfNull(id);

        return this.on.Add(id.Value);
    }

    /// <summary>Gives the ids of the flags that are on, in ordinal order, for a snapshot (D-166).</summary>
    /// <param name="source">The file of the ids, for the parse of each one.</param>
    /// <returns>A new list, which a later set never changes.</returns>
    public IReadOnlyList<ContentId> Values(string source)
    {
        List<ContentId> ids = [];
        foreach (string id in this.on)
        {
            ids.Add(ContentId.Parse(id, source, "flags"));
        }

        return ids;
    }

    /// <summary>Adds every flag that is on to the state hash, in ordinal order (G-5).</summary>
    /// <param name="hasher">The hasher of the state.</param>
    /// <exception cref="ArgumentNullException">The hasher is null (T-2).</exception>
    public void Hash(StateHasher hasher)
    {
        ArgumentNullException.ThrowIfNull(hasher);

        hasher.AddInt32(this.on.Count);
        foreach (string id in this.on)
        {
            hasher.AddText(id);
        }
    }
}
