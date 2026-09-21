using System;
using TheThingBelow.Core.Content;

namespace TheThingBelow.Core.Maps;

/// <summary>Which side reached the other from behind at the start of an encounter (D-265, D-746).</summary>
public enum EncounterSide
{
    /// <summary>Neither side, or both. No side acts first (D-746).</summary>
    None,

    /// <summary>The party reached the enemy from behind, which is a sneak (D-265).</summary>
    Party,

    /// <summary>The enemy reached the party from behind, which is an ambush (D-265).</summary>
    Enemy,
}

/// <summary>The names of the sides, for a log field and for an error (T-2).</summary>
public static class EncounterSides
{
    /// <summary>Every side, in one fixed order for a walk of them (G-4).</summary>
    public static readonly EncounterSide[] All = [EncounterSide.None, EncounterSide.Party, EncounterSide.Enemy];

    /// <summary>The names of every side, for the error of an unknown name (T-2).</summary>
    public const string EveryName = "none, party, enemy";

    /// <summary>Gives the side of one name.</summary>
    /// <param name="name">The name, such as `party`.</param>
    /// <param name="side">The side of that name, when the name names one.</param>
    /// <returns>True when the name names a side.</returns>
    /// <exception cref="ArgumentNullException">The name is null (T-2).</exception>
    public static bool TryOf(string name, out EncounterSide side)
    {
        ArgumentNullException.ThrowIfNull(name);

        foreach (EncounterSide candidate in All)
        {
            if (string.CompareOrdinal(NameOf(candidate), name) == 0)
            {
                side = candidate;
                return true;
            }
        }

        side = EncounterSide.None;
        return false;
    }

    /// <summary>Gives the name of one side, which a snapshot and a log field use (T-2).</summary>
    /// <param name="side">The side.</param>
    /// <returns>The name, such as `enemy`.</returns>
    /// <exception cref="ArgumentOutOfRangeException">The value names no side (T-2).</exception>
    public static string NameOf(EncounterSide side) => side switch
    {
        EncounterSide.None => "none",
        EncounterSide.Party => "party",
        EncounterSide.Enemy => "enemy",
        _ => throw new ArgumentOutOfRangeException(nameof(side), side, "the value names no encounter side (D-746)"),
    };
}

/// <summary>
/// The mark that a patrol shows for a beat before an encounter starts (D-208, D-745).
/// </summary>
/// <remarks>
/// Core counts the ticks of the beat, and Game draws the mark over the same ticks. Nothing
/// cancels the beat, so the mark tells the player that the fight comes (D-745).
/// <para>
/// The party still walks and turns during the beat, and the side that reached the other from
/// behind comes from the facings at the moment that the encounter starts (D-746).
/// </para>
/// </remarks>
/// <param name="Enemy">The id of the patrol that saw the party (D-752).</param>
/// <param name="TicksLeft">The count of world ticks of the beat that remain.</param>
public sealed record SightMark(ContentId Enemy, int TicksLeft);

/// <summary>
/// The encounter that runs now. While it holds, no map system ticks, so the patrols and the
/// grace time all stand still (D-531).
/// </summary>
/// <remarks>
/// PR-8 reaches the start of an encounter, and PR-9 builds the fight. One command of the
/// debug console ends the encounter as a flee, so the grace time of D-381 has a live path and
/// the smoke session never stops (D-749).
/// </remarks>
/// <param name="Enemy">The id of the patrol of the encounter (D-752).</param>
/// <param name="Group">The id of the enemy group that the fight uses (D-535, D-753).</param>
/// <param name="Behind">The side that reached the other from behind (D-265, D-746).</param>
public sealed record MapEncounter(ContentId Enemy, ContentId Group, EncounterSide Behind);
