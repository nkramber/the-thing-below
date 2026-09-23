using System;
using TheThingBelow.Core.Maps;

namespace TheThingBelow.Core.Effects;

/// <summary>The kind of an encounter, which picks the transition into its fight (D-196, D-937).</summary>
/// <remarks>
/// No rule reads the kind. Game reads it from the state when a fight starts, and the transition
/// table gives the transition of it (D-495, D-522).
/// </remarks>
public enum EncounterKind
{
    /// <summary>A fight that fits no other kind. It takes a transition from the pool of its region (D-934).</summary>
    Common,

    /// <summary>The enemy reached the party from behind (D-746).</summary>
    Ambush,

    /// <summary>A patrol of elite size or larger, in a group with no boss flag (D-206).</summary>
    Elite,

    /// <summary>A group with the boss flag (D-378).</summary>
    Boss,

    /// <summary>A thing that should not be there (D-155). No mark gives this kind yet (D-937).</summary>
    WrongThing,
}

/// <summary>The names of the encounter kinds, and the rule that gives the kind of one encounter (D-937).</summary>
public static class EncounterKinds
{
    /// <summary>The kinds that take one transition for the whole game, in the order of the table file (D-934).</summary>
    public static readonly EncounterKind[] Fixed = [EncounterKind.Ambush, EncounterKind.Elite, EncounterKind.Boss, EncounterKind.WrongThing];

    /// <summary>The names of the fixed kinds, for the error of an unknown name (T-2).</summary>
    public const string FixedNames = "ambush, elite, boss, wrong_thing";

    /// <summary>Gives the kind of one encounter, in the order boss, wrong thing, ambush, elite, common (D-937).</summary>
    /// <param name="boss">True when the group of the fight has the boss flag (D-378).</param>
    /// <param name="behind">The side that reached the other from behind (D-746).</param>
    /// <param name="size">The size of the patrol of the encounter, which is the largest body of its group (D-788).</param>
    /// <returns>The kind.</returns>
    /// <remarks>
    /// No mark of a wrong thing exists yet, so this method never gives <see cref="EncounterKind.WrongThing"/>.
    /// The PR that adds the wrong things adds the mark here, after the boss and before the ambush (D-937).
    /// </remarks>
    public static EncounterKind Of(bool boss, EncounterSide behind, EnemySize size)
    {
        if (boss)
        {
            return EncounterKind.Boss;
        }

        if (behind == EncounterSide.Enemy)
        {
            return EncounterKind.Ambush;
        }

        return size >= EnemySize.Elite ? EncounterKind.Elite : EncounterKind.Common;
    }

    /// <summary>Gives the kind of one name of the table file.</summary>
    /// <param name="name">The name, such as `wrong_thing`.</param>
    /// <param name="kind">The kind of that name, when the name names a fixed kind.</param>
    /// <returns>True when the name names a fixed kind. The name `common` names none, because the pool serves it (D-934).</returns>
    /// <exception cref="ArgumentNullException">The name is null (T-2).</exception>
    public static bool TryFixedOf(string name, out EncounterKind kind)
    {
        ArgumentNullException.ThrowIfNull(name);

        foreach (EncounterKind candidate in Fixed)
        {
            if (string.CompareOrdinal(NameOf(candidate), name) == 0)
            {
                kind = candidate;
                return true;
            }
        }

        kind = EncounterKind.Common;
        return false;
    }

    /// <summary>Gives the name of one kind, which the table file and a log field use (T-2).</summary>
    /// <param name="kind">The kind.</param>
    /// <returns>The name, such as `ambush`.</returns>
    /// <exception cref="ArgumentOutOfRangeException">The value names no kind (T-2).</exception>
    public static string NameOf(EncounterKind kind) => kind switch
    {
        EncounterKind.Common => "common",
        EncounterKind.Ambush => "ambush",
        EncounterKind.Elite => "elite",
        EncounterKind.Boss => "boss",
        EncounterKind.WrongThing => "wrong_thing",
        _ => throw new ArgumentOutOfRangeException(nameof(kind), kind, "the value names no encounter kind (D-196)"),
    };
}
