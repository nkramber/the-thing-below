using System;
using System.Collections.Generic;

namespace TheThingBelow.Core.Battles;

/// <summary>The eight kinds of ability, four for rites and four for drills (D-281).</summary>
public enum AptitudeKind
{
    /// <summary>A rite that heals or cures.</summary>
    Mend,

    /// <summary>A rite that deals damage.</summary>
    Harm,

    /// <summary>A rite that gives poison, sleep, and other statuses.</summary>
    Blight,

    /// <summary>A rite that gives shields and haste.</summary>
    Boon,

    /// <summary>A drill of melee.</summary>
    Blade,

    /// <summary>A drill that protects allies.</summary>
    Guard,

    /// <summary>A drill of the crossbow and the bow.</summary>
    Shot,

    /// <summary>A drill of the steal, the lock, and the trap.</summary>
    Theft,
}

/// <summary>The names of the kinds in content, and the rites among them (D-281, D-806).</summary>
public static class Aptitudes
{
    /// <summary>Every kind, in the order of D-281.</summary>
    public static readonly IReadOnlyList<AptitudeKind> All =
    [
        AptitudeKind.Mend,
        AptitudeKind.Harm,
        AptitudeKind.Blight,
        AptitudeKind.Boon,
        AptitudeKind.Blade,
        AptitudeKind.Guard,
        AptitudeKind.Shot,
        AptitudeKind.Theft,
    ];

    /// <summary>The names of every kind, for an error (T-2).</summary>
    public const string EveryName = "mend, harm, blight, boon, blade, guard, shot, theft";

    /// <summary>Gives the name of a kind in content.</summary>
    /// <param name="kind">The kind.</param>
    /// <returns>The name, such as `mend`.</returns>
    /// <exception cref="ArgumentOutOfRangeException">The value names no kind (T-2).</exception>
    public static string NameOf(AptitudeKind kind) => kind switch
    {
        AptitudeKind.Mend => "mend",
        AptitudeKind.Harm => "harm",
        AptitudeKind.Blight => "blight",
        AptitudeKind.Boon => "boon",
        AptitudeKind.Blade => "blade",
        AptitudeKind.Guard => "guard",
        AptitudeKind.Shot => "shot",
        AptitudeKind.Theft => "theft",
        _ => throw new ArgumentOutOfRangeException(nameof(kind), kind, "the value names no kind of ability (D-281)"),
    };

    /// <summary>Finds the kind of a name.</summary>
    /// <param name="name">The name, such as `mend`.</param>
    /// <param name="kind">The kind, when the name is one.</param>
    /// <returns>True when the name names a kind.</returns>
    public static bool TryOf(string name, out AptitudeKind kind)
    {
        foreach (AptitudeKind candidate in All)
        {
            if (string.CompareOrdinal(NameOf(candidate), name) == 0)
            {
                kind = candidate;
                return true;
            }
        }

        kind = AptitudeKind.Mend;
        return false;
    }

    /// <summary>True for the four kinds of rite, which silence stops (D-281, D-393, D-806).</summary>
    /// <param name="kind">The kind.</param>
    /// <returns>True when a lesson of the kind is a rite.</returns>
    public static bool IsRite(AptitudeKind kind) =>
        kind == AptitudeKind.Mend || kind == AptitudeKind.Harm || kind == AptitudeKind.Blight || kind == AptitudeKind.Boon;
}
