using System;
using TheThingBelow.Core.Battles;
using TheThingBelow.Core.Content;

namespace TheThingBelow.Core.Maps;

/// <summary>What a trap does when it fires (D-1226).</summary>
public enum TrapHarmKind
{
    /// <summary>A share of full health from each character who fights (D-1230).</summary>
    Damage,

    /// <summary>Poison, blind, or silence on each character who fights (D-1230).</summary>
    Status,

    /// <summary>A fight with an enemy group, in which the enemies act first (D-1231).</summary>
    Encounter,
}

/// <summary>What one trap does when it fires: its kind, and the one value of that kind (D-1226).</summary>
/// <remarks>
/// A damage trap holds a share, a status trap holds a status, and an encounter trap holds a group.
/// Each other value holds nothing, so no reader meets a zero in place of an absent value (T-2).
/// Build each harm with <see cref="Damage"/>, <see cref="Put"/>, or <see cref="Ambush"/>.
/// </remarks>
public sealed record TrapHarm
{
    private TrapHarm(TrapHarmKind kind, int? share, StatusKind? status, ContentId? group)
    {
        this.Kind = kind;
        this.Share = share;
        this.Status = status;
        this.Group = group;
    }

    /// <summary>The field of a trap line that names the kind of its harm (D-1226).</summary>
    public const string HarmField = "harm";

    /// <summary>The field of a damage trap that holds its share, in basis points (D-1230).</summary>
    public const string ShareField = "share";

    /// <summary>The field of a status trap that names its status (D-1230).</summary>
    public const string StatusField = "status";

    /// <summary>The field of an encounter trap that names its enemy group (D-1231).</summary>
    public const string GroupField = "group";

    /// <summary>The names of every kind of harm, for an error (T-2).</summary>
    public const string EveryName = "damage, status, encounter";

    /// <summary>What the trap does.</summary>
    public TrapHarmKind Kind { get; }

    /// <summary>The share of full health that a damage trap takes, in basis points from 1 to 10000. Every other kind holds no value (D-1230).</summary>
    public int? Share { get; }

    /// <summary>The status that a status trap puts: poison, blind, or silence. Every other kind holds no value (D-390, D-1230).</summary>
    public StatusKind? Status { get; }

    /// <summary>The enemy group of an encounter trap. Every other kind holds no value (D-1231).</summary>
    public ContentId? Group { get; }

    /// <summary>Makes the harm of a damage trap (D-1230).</summary>
    /// <param name="share">The share of full health, in basis points from 1 to 10000.</param>
    /// <returns>The harm.</returns>
    /// <exception cref="ArgumentOutOfRangeException">The share lies outside 1 to 10000 (T-2).</exception>
    public static TrapHarm Damage(int share)
    {
        if (share < 1 || share > BasisPoints.One)
        {
            throw new ArgumentOutOfRangeException(nameof(share), share, $"a damage trap takes a share from 1 to {BasisPoints.One} basis points (D-1230)");
        }

        return new TrapHarm(TrapHarmKind.Damage, share, null, null);
    }

    /// <summary>Makes the harm of a status trap (D-1230).</summary>
    /// <param name="status">Poison, blind, or silence.</param>
    /// <returns>The harm.</returns>
    /// <exception cref="ArgumentOutOfRangeException">The status does not last past a fight (T-2, D-390).</exception>
    public static TrapHarm Put(StatusKind status)
    {
        if (!Statuses.Lasts(status))
        {
            throw new ArgumentOutOfRangeException(nameof(status), status, "a status trap puts poison, blind, or silence, the statuses that last on the map (D-390, D-1230)");
        }

        return new TrapHarm(TrapHarmKind.Status, null, status, null);
    }

    /// <summary>Makes the harm of an encounter trap (D-1231).</summary>
    /// <param name="group">The id of the enemy group.</param>
    /// <returns>The harm.</returns>
    /// <exception cref="ArgumentNullException">The group is null (T-2).</exception>
    public static TrapHarm Ambush(ContentId group)
    {
        ArgumentNullException.ThrowIfNull(group);

        return new TrapHarm(TrapHarmKind.Encounter, null, null, group);
    }

    /// <summary>Gives the kind of one name of a map file.</summary>
    /// <param name="name">The name, such as `damage`.</param>
    /// <param name="kind">The kind of that name, when the name names one.</param>
    /// <returns>True when the name names a kind.</returns>
    /// <exception cref="ArgumentNullException">The name is null (T-2).</exception>
    public static bool TryKindOf(string name, out TrapHarmKind kind)
    {
        ArgumentNullException.ThrowIfNull(name);

        foreach (TrapHarmKind candidate in new[] { TrapHarmKind.Damage, TrapHarmKind.Status, TrapHarmKind.Encounter })
        {
            if (string.CompareOrdinal(NameOf(candidate), name) == 0)
            {
                kind = candidate;
                return true;
            }
        }

        kind = TrapHarmKind.Damage;
        return false;
    }

    /// <summary>Gives the name of one kind, which a map file and a log field use (T-2).</summary>
    /// <param name="kind">The kind.</param>
    /// <returns>The name, such as `encounter`.</returns>
    /// <exception cref="ArgumentOutOfRangeException">The value names no kind (T-2).</exception>
    public static string NameOf(TrapHarmKind kind) => kind switch
    {
        TrapHarmKind.Damage => "damage",
        TrapHarmKind.Status => "status",
        TrapHarmKind.Encounter => "encounter",
        _ => throw new ArgumentOutOfRangeException(nameof(kind), kind, "the value names no kind of trap harm (D-1226)"),
    };
}
