using System;

namespace TheThingBelow.Core.Maps;

/// <summary>The count of tiles on one side of the body of an enemy (D-206, D-236).</summary>
/// <remarks>
/// A common enemy holds one tile, an elite two by two, and a boss three by three, on the map
/// as in battle (D-206). The map file of each place gives the size of each enemy that it
/// places, and the enemy record of PR-80 gives the same size. A load then fails a map whose
/// size disagrees with that record (D-754, T-2).
/// </remarks>
public enum EnemySize
{
    /// <summary>One tile, 32 by 32 pixels (D-236).</summary>
    Common,

    /// <summary>Two by two tiles, 64 by 64 pixels (D-236).</summary>
    Elite,

    /// <summary>Three by three tiles, 96 by 96 pixels (D-236).</summary>
    Boss,
}

/// <summary>The names of the enemy sizes, and the tile count of each one (D-206, D-236).</summary>
public static class EnemySizes
{
    /// <summary>Every size, in one fixed order for a walk of them (G-4).</summary>
    public static readonly EnemySize[] All = [EnemySize.Common, EnemySize.Elite, EnemySize.Boss];

    /// <summary>The names of every size, for the error of an unknown name (T-2).</summary>
    public const string EveryName = "common, elite, boss";

    /// <summary>Gives the size of one name.</summary>
    /// <param name="name">The name, such as `elite`.</param>
    /// <param name="size">The size of that name, when the name names one.</param>
    /// <returns>True when the name names a size.</returns>
    /// <exception cref="ArgumentNullException">The name is null (T-2).</exception>
    public static bool TryOf(string name, out EnemySize size)
    {
        ArgumentNullException.ThrowIfNull(name);

        foreach (EnemySize candidate in All)
        {
            if (string.CompareOrdinal(NameOf(candidate), name) == 0)
            {
                size = candidate;
                return true;
            }
        }

        size = EnemySize.Common;
        return false;
    }

    /// <summary>Gives the name of one size, which a map file and an error use (T-2).</summary>
    /// <param name="size">The size.</param>
    /// <returns>The name, such as `boss`.</returns>
    /// <exception cref="ArgumentOutOfRangeException">The value names no size (T-2).</exception>
    public static string NameOf(EnemySize size) => size switch
    {
        EnemySize.Common => "common",
        EnemySize.Elite => "elite",
        EnemySize.Boss => "boss",
        _ => throw new ArgumentOutOfRangeException(nameof(size), size, "the value names no enemy size (D-206)"),
    };

    /// <summary>Gives the count of tiles on one side of a body of this size (D-206).</summary>
    /// <param name="size">The size.</param>
    /// <returns>1, 2, or 3.</returns>
    /// <exception cref="ArgumentOutOfRangeException">The value names no size (T-2).</exception>
    public static int SideOf(EnemySize size) => size switch
    {
        EnemySize.Common => 1,
        EnemySize.Elite => 2,
        EnemySize.Boss => 3,
        _ => throw new ArgumentOutOfRangeException(nameof(size), size, "the value names no enemy size (D-206)"),
    };
}
