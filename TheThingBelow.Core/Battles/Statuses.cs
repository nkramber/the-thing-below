using System;
using System.Collections.Generic;
using TheThingBelow.Core.Content;
using TheThingBelow.Core.Hashing;

namespace TheThingBelow.Core.Battles;

/// <summary>The ten statuses, in the order of D-75.</summary>
public enum StatusKind
{
    /// <summary>A share of full health at each turn of the holder, until a cure (D-390, D-803).</summary>
    Poison,

    /// <summary>A rate on the miss chance of each strike of the holder, until a cure (D-390, D-806).</summary>
    Blind,

    /// <summary>A mark that PR-12 reads to refuse a rite, until a cure (D-390, D-806).</summary>
    Silence,

    /// <summary>Each turn of the holder passes, and the damage of a strike wakes it (D-802).</summary>
    Sleep,

    /// <summary>The slow rate on each push (D-768, D-800).</summary>
    Slow,

    /// <summary>The haste rate on each push (D-768, D-800).</summary>
    Haste,

    /// <summary>A push of the next turn when it lands (D-777, D-802, D-810).</summary>
    Stun,

    /// <summary>A larger share of full health at each turn of the holder, for its ticks (D-803).</summary>
    Bleed,

    /// <summary>A heal of a share of full health at each turn of the holder (D-799).</summary>
    Regen,

    /// <summary>A cut of the damage of a move with an element (D-804).</summary>
    Shell,
}

/// <summary>A status that a move gives on a hit, with its chance in basis points (D-793, D-807).</summary>
/// <param name="Status">The status.</param>
/// <param name="Chance">The chance, from 1 to 10000.</param>
public sealed record StatusChance(StatusKind Status, int Chance);

/// <summary>One status that a combatant holds, for a snapshot (D-798).</summary>
/// <param name="Status">The status.</param>
/// <param name="EndsAt">The tick of the timeline of its end, and no value for poison, blind, and silence (D-390).</param>
public sealed record StatusValues(StatusKind Status, long? EndsAt);

/// <summary>The names of the statuses in content, and the rules of which ones last (D-75, D-390, D-797).</summary>
public static class Statuses
{
    /// <summary>Every status, in the order of D-75.</summary>
    public static readonly IReadOnlyList<StatusKind> All =
    [
        StatusKind.Poison,
        StatusKind.Blind,
        StatusKind.Silence,
        StatusKind.Sleep,
        StatusKind.Slow,
        StatusKind.Haste,
        StatusKind.Stun,
        StatusKind.Bleed,
        StatusKind.Regen,
        StatusKind.Shell,
    ];

    /// <summary>The names of every status, for an error (T-2).</summary>
    public const string EveryName = "poison, blind, silence, sleep, slow, haste, stun, bleed, regen, shell";

    /// <summary>Gives the name of a status in content.</summary>
    /// <param name="status">The status.</param>
    /// <returns>The name, such as `poison`.</returns>
    /// <exception cref="ArgumentOutOfRangeException">The value names no status (T-2).</exception>
    public static string NameOf(StatusKind status) => status switch
    {
        StatusKind.Poison => "poison",
        StatusKind.Blind => "blind",
        StatusKind.Silence => "silence",
        StatusKind.Sleep => "sleep",
        StatusKind.Slow => "slow",
        StatusKind.Haste => "haste",
        StatusKind.Stun => "stun",
        StatusKind.Bleed => "bleed",
        StatusKind.Regen => "regen",
        StatusKind.Shell => "shell",
        _ => throw new ArgumentOutOfRangeException(nameof(status), status, "the value names no status (D-75)"),
    };

    /// <summary>Finds the status of a name.</summary>
    /// <param name="name">The name, such as `poison`.</param>
    /// <param name="status">The status, when the name is one.</param>
    /// <returns>True when the name names a status.</returns>
    public static bool TryOf(string name, out StatusKind status)
    {
        foreach (StatusKind candidate in All)
        {
            if (string.CompareOrdinal(NameOf(candidate), name) == 0)
            {
                status = candidate;
                return true;
            }
        }

        status = StatusKind.Poison;
        return false;
    }

    /// <summary>True for poison, blind, and silence, which hold no end and last past the fight until a cure (D-390, D-798).</summary>
    /// <param name="status">The status.</param>
    /// <returns>True when the status lasts.</returns>
    public static bool Lasts(StatusKind status) =>
        status == StatusKind.Poison || status == StatusKind.Blind || status == StatusKind.Silence;

    /// <summary>True when an immune list holds the status (D-805). A loop, because G-1 keeps `System.Linq` out of Core.</summary>
    /// <param name="immune">The immune list.</param>
    /// <param name="status">The status.</param>
    /// <returns>True when the list holds it.</returns>
    public static bool Refuses(IReadOnlyList<StatusKind> immune, StatusKind status)
    {
        ArgumentNullException.ThrowIfNull(immune);

        foreach (StatusKind refused in immune)
        {
            if (refused == status)
            {
                return true;
            }
        }

        return false;
    }

    /// <summary>Reads a list of status names, and refuses a name that the list holds two times (T-2).</summary>
    /// <param name="reader">The reader of the file.</param>
    /// <returns>The statuses, in the order of the file.</returns>
    /// <exception cref="ContentException">A name is unknown or repeated (T-2).</exception>
    public static IReadOnlyList<StatusKind> ReadList(ref ContentReader reader)
    {
        List<StatusKind> statuses = [];
        int depth = reader.ReadArrayStart();
        while (reader.ReadNextElement(depth, statuses.Count))
        {
            string name = reader.ReadString();
            if (!TryOf(name, out StatusKind status))
            {
                throw reader.Refuse($"the status '{name}' is not one of {EveryName} (D-75)");
            }

            if (statuses.Contains(status))
            {
                throw reader.Refuse($"the list names '{name}' two times");
            }

            statuses.Add(status);
        }

        return statuses;
    }
}

/// <summary>
/// The statuses that one combatant holds, each with the tick of its end (D-798). Poison,
/// blind, and silence hold no end (D-390). A second copy of a status resets the end, and
/// never stacks (D-800).
/// </summary>
public sealed class StatusSet
{
    private readonly bool[] held = new bool[Statuses.All.Count];
    private readonly long[] endsAt = new long[Statuses.All.Count];

    /// <summary>True when the set holds no status.</summary>
    public bool Empty
    {
        get
        {
            foreach (bool holds in this.held)
            {
                if (holds)
                {
                    return false;
                }
            }

            return true;
        }
    }

    /// <summary>True when the set holds the status.</summary>
    /// <param name="status">The status.</param>
    /// <returns>True when the set holds it.</returns>
    public bool Holds(StatusKind status) => this.held[(int)status];

    /// <summary>Gives every status of the set, in the order of D-75.</summary>
    /// <returns>The values, each with its end or no end.</returns>
    public IReadOnlyList<StatusValues> Values()
    {
        List<StatusValues> values = [];
        foreach (StatusKind status in Statuses.All)
        {
            if (this.held[(int)status])
            {
                values.Add(new StatusValues(status, Statuses.Lasts(status) ? null : this.endsAt[(int)status]));
            }
        }

        return values;
    }

    /// <summary>Adds every status of the set to the state hash, in the order of D-75 (G-5).</summary>
    /// <param name="hasher">The hasher of the state.</param>
    public void Hash(StateHasher hasher)
    {
        ArgumentNullException.ThrowIfNull(hasher);

        foreach (StatusKind status in Statuses.All)
        {
            hasher.AddBoolean(this.held[(int)status]);
            hasher.AddInt64(this.held[(int)status] && !Statuses.Lasts(status) ? this.endsAt[(int)status] : 0);
        }
    }

    /// <summary>Puts a status in the set, or resets its end (D-800).</summary>
    /// <param name="status">The status.</param>
    /// <param name="ends">The tick of its end, and no value for a status that lasts (D-390).</param>
    /// <exception cref="ArgumentException">A status that lasts takes an end, or a timed status takes none (T-2).</exception>
    internal void Put(StatusKind status, long? ends)
    {
        if (Statuses.Lasts(status) != (ends is null))
        {
            throw new ArgumentException(
                $"The status '{Statuses.NameOf(status)}' takes {(Statuses.Lasts(status) ? "no end" : "an end")}, and the call gave {(ends is null ? "none" : ends.Value)} (D-390, D-798).",
                nameof(ends));
        }

        this.held[(int)status] = true;
        this.endsAt[(int)status] = ends ?? 0;
    }

    /// <summary>Gives the tick of the end of a timed status that the set holds.</summary>
    /// <param name="status">The status, which must not last.</param>
    /// <returns>The tick, or no value when the set does not hold the status or the status lasts.</returns>
    internal long? EndOf(StatusKind status) =>
        this.held[(int)status] && !Statuses.Lasts(status) ? this.endsAt[(int)status] : null;

    /// <summary>Takes one status out of the set.</summary>
    /// <param name="status">The status.</param>
    internal void Remove(StatusKind status)
    {
        this.held[(int)status] = false;
        this.endsAt[(int)status] = 0;
    }
}
