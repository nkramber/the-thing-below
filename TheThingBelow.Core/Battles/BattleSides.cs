using System;

namespace TheThingBelow.Core.Battles;

/// <summary>The two sides of an encounter (D-31).</summary>
public enum BattleSide
{
    /// <summary>The one to three characters of the player (D-31, D-336).</summary>
    Party,

    /// <summary>The enemies of the group (D-535).</summary>
    Enemy,
}

/// <summary>The two rows of each side (D-377).</summary>
public enum BattleRow
{
    /// <summary>The row that melee reaches while anyone stands in it (D-377).</summary>
    Front,

    /// <summary>The row behind it. A melee attack from it deals less damage (D-779).</summary>
    Back,
}

/// <summary>The names of the sides and the rows, for content, a snapshot, a log field, and an error (T-2).</summary>
public static class BattleSides
{
    /// <summary>The names of every side, for the error of an unknown name (T-2).</summary>
    public const string EverySideName = "party, enemy";

    /// <summary>The names of every row, for the error of an unknown name (T-2).</summary>
    public const string EveryRowName = "front, back";

    /// <summary>Gives the side of one name.</summary>
    /// <param name="name">The name, such as `enemy`.</param>
    /// <param name="side">The side of that name, when the name names one.</param>
    /// <returns>True when the name names a side.</returns>
    /// <exception cref="ArgumentNullException">The name is null (T-2).</exception>
    public static bool TrySideOf(string name, out BattleSide side)
    {
        ArgumentNullException.ThrowIfNull(name);

        if (string.CompareOrdinal(name, NameOf(BattleSide.Party)) == 0)
        {
            side = BattleSide.Party;
            return true;
        }

        if (string.CompareOrdinal(name, NameOf(BattleSide.Enemy)) == 0)
        {
            side = BattleSide.Enemy;
            return true;
        }

        side = BattleSide.Party;
        return false;
    }

    /// <summary>Gives the row of one name.</summary>
    /// <param name="name">The name, such as `back`.</param>
    /// <param name="row">The row of that name, when the name names one.</param>
    /// <returns>True when the name names a row.</returns>
    /// <exception cref="ArgumentNullException">The name is null (T-2).</exception>
    public static bool TryRowOf(string name, out BattleRow row)
    {
        ArgumentNullException.ThrowIfNull(name);

        if (string.CompareOrdinal(name, NameOf(BattleRow.Front)) == 0)
        {
            row = BattleRow.Front;
            return true;
        }

        if (string.CompareOrdinal(name, NameOf(BattleRow.Back)) == 0)
        {
            row = BattleRow.Back;
            return true;
        }

        row = BattleRow.Front;
        return false;
    }

    /// <summary>Gives the name of one side.</summary>
    /// <param name="side">The side.</param>
    /// <returns>The name, such as `party`.</returns>
    /// <exception cref="ArgumentOutOfRangeException">The value names no side (T-2).</exception>
    public static string NameOf(BattleSide side) => side switch
    {
        BattleSide.Party => "party",
        BattleSide.Enemy => "enemy",
        _ => throw new ArgumentOutOfRangeException(nameof(side), side, "the value names no battle side (D-31)"),
    };

    /// <summary>Gives the name of one row.</summary>
    /// <param name="row">The row.</param>
    /// <returns>The name, such as `front`.</returns>
    /// <exception cref="ArgumentOutOfRangeException">The value names no row (T-2).</exception>
    public static string NameOf(BattleRow row) => row switch
    {
        BattleRow.Front => "front",
        BattleRow.Back => "back",
        _ => throw new ArgumentOutOfRangeException(nameof(row), row, "the value names no battle row (D-377)"),
    };

    /// <summary>Gives the other row, which a row step reaches (D-380).</summary>
    /// <param name="row">The row that the combatant stands in.</param>
    /// <returns>The other row.</returns>
    /// <exception cref="ArgumentOutOfRangeException">The value names no row (T-2).</exception>
    public static BattleRow Other(BattleRow row) => row switch
    {
        BattleRow.Front => BattleRow.Back,
        BattleRow.Back => BattleRow.Front,
        _ => throw new ArgumentOutOfRangeException(nameof(row), row, "the value names no battle row (D-377)"),
    };
}
