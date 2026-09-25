using System;
using System.Collections.Generic;
using TheThingBelow.Core.Content;
using TheThingBelow.Core.Runs;
using TheThingBelow.Core.Streams;

namespace TheThingBelow.Core.Battles;

/// <summary>
/// The rules of the loot of a fight: the steal of a Theft drill, and the drops of a win
/// (D-383, D-949, D-950, D-1042 to D-1045, D-1051). Each roll draws from the battle stream.
/// </summary>
public static class LootRules
{
    /// <summary>The steal tries of the whole party in one fight. A failed try counts (D-1045).</summary>
    public const int MostStealTries = 3;

    /// <summary>Gives the reason that the rules refuse a steal try now, or no value (D-1045).</summary>
    /// <param name="battle">The fight.</param>
    /// <returns>The reason, or no value while a try is left.</returns>
    public static string? RefusalOfSteal(Battle battle)
    {
        ArgumentNullException.ThrowIfNull(battle);

        return battle.StealTries >= MostStealTries
            ? $"a steal try after {battle.StealTries} tries, and a fight allows {MostStealTries} (D-1045)"
            : null;
    }

    /// <summary>
    /// Gives the chance of a steal: the base chance of the profile plus the Theft term of the
    /// thief, halved by the steal rate once for each earlier success of the fight, then held
    /// in the clamp of the rules (D-949, D-1045).
    /// </summary>
    /// <param name="baseChance">The base chance of the profile, in basis points.</param>
    /// <param name="theftTerm">The Theft term of the thief, in basis points: the aptitude bonus of D-1028 for a main or an opened side aptitude of Theft.</param>
    /// <param name="successes">The earlier successes of the fight, from zero.</param>
    /// <param name="rules">The rules, which hold the rate and the clamp.</param>
    /// <returns>The chance, in basis points.</returns>
    public static int StealChance(int baseChance, int theftTerm, int successes, BattleRules rules)
    {
        ArgumentNullException.ThrowIfNull(rules);
        ArgumentOutOfRangeException.ThrowIfNegative(successes);

        long chance = (long)baseChance + theftTerm;
        for (int success = 0; success < successes; success += 1)
        {
            chance = chance * rules.StealRate / BasisPoints.One;
        }

        return (int)Math.Clamp(chance, rules.StealFloor, rules.StealCeiling);
    }

    /// <summary>
    /// Gives the gear chance of a successful steal: the gear chance of the profile, held under
    /// the cap of the success. The count reads the earlier successes of the fight and never the
    /// tries, so a first success after two failures takes the cap of a first success (D-1051).
    /// </summary>
    /// <param name="profileChance">The gear chance of the profile, in basis points.</param>
    /// <param name="successes">The earlier successes of the fight, from 0 to 2.</param>
    /// <param name="rules">The rules, which hold the cap of each success.</param>
    /// <returns>The gear chance, in basis points.</returns>
    /// <exception cref="ArgumentOutOfRangeException">The count of successes is outside 0 to 2 (T-2).</exception>
    public static int GearChance(int profileChance, int successes, BattleRules rules)
    {
        ArgumentNullException.ThrowIfNull(rules);
        if (successes < 0 || successes >= rules.StealGearCaps.Count)
        {
            throw new ArgumentOutOfRangeException(nameof(successes), successes, $"A fight holds {rules.StealGearCaps.Count} successful steals at most (D-1045, D-1051).");
        }

        return Math.Min(profileChance, rules.StealGearCaps[successes]);
    }

    /// <summary>
    /// Tries a steal from one enemy (D-1044, D-1045, D-1051). The try counts first. An enemy with
    /// no entry left gives a line. Otherwise the battle stream rolls the chance. A success then
    /// rolls the gear chance, and on a hit takes a remaining piece that the party has room for.
    /// On a miss it takes a remaining gold or item entry, and the first gold entry again when only
    /// gear is left. Gold goes to the party. An item over its stack limit stays with the enemy,
    /// and a line names it. The caller pushes the thief back.
    /// </summary>
    /// <param name="state">The run.</param>
    /// <param name="battle">The fight.</param>
    /// <param name="thief">The character who steals.</param>
    /// <param name="aimed">The enemy.</param>
    /// <param name="context">The seed, the tick, and the ids, for an error (T-2).</param>
    internal static void Steal(RunState state, Battle battle, Combatant thief, BattleTarget aimed, RunContext context)
    {
        battle.StealTries += 1;
        BattleContent content = state.BattleContent;
        ProfileRecord profile = content.Profile(battle.Group.Entries[aimed.Slot].Profile);
        List<int> left = [];
        for (int entry = 0; entry < profile.Steal.Count; entry += 1)
        {
            if (!battle.WasStolen(aimed.Slot, entry))
            {
                left.Add(entry);
            }
        }

        if (left.Count == 0)
        {
            state.AddEvent(new BattleEvent(BattleEventKind.StealEmpty, thief.Target, aimed, 0));
            return;
        }

        PartyMember member = state.Characters.Members[thief.Slot];
        int term = LessonRules.BonusOf(member.Record, AptitudeKind.Theft, content.Rules, state.Story.Flags);
        int chance = StealChance(profile.StealChance, term, battle.Stolen.Count, content.Rules);
        RandomStream stream = state.Stream(StreamId.Battle);
        if (!stream.NextChance(chance, context))
        {
            state.AddEvent(new BattleEvent(BattleEventKind.StealFailed, thief.Target, aimed, 0));
            return;
        }

        int pick = PickOf(state, profile, left, battle.Stolen.Count, stream, context);
        switch (profile.Steal[pick])
        {
            case StealGold gold:
                state.Characters.AddGold(gold.Gold, context);
                battle.NoteStolen(new StolenEntry(aimed.Slot, pick));
                state.AddEvent(new BattleEvent(BattleEventKind.StealGold, thief.Target, aimed, gold.Gold));
                break;
            case StealGear gear:
                // The pick checked the room, so the piece fits (D-1051). A remainder would drop
                // the piece and still count the steal, so it fails with its context (T-2).
                if (state.Characters.Pick(gear.Gear, 1, content) != 0)
                {
                    throw new SimulationException($"the steal of '{gear.Gear.Value}' from '{profile.Id.Value}' found no room, and the pick of the entry checked the room (D-1051, T-2)", context);
                }

                battle.NoteStolen(new StolenEntry(aimed.Slot, pick));
                state.AddEvent(new BattleEvent(BattleEventKind.StealGear, thief.Target, aimed, 0, null, Affinity.Normal, gear.Gear));
                break;
            case StealItem item:
                // A full stack keeps the item with the enemy, and the steal counts no success (D-1044).
                if (state.Characters.Pick(item.Item, 1, content) != 0)
                {
                    state.AddEvent(new BattleEvent(BattleEventKind.StealFull, thief.Target, aimed, 0, null, Affinity.Normal, item.Item));
                    break;
                }

                battle.NoteStolen(new StolenEntry(aimed.Slot, pick));
                state.AddEvent(new BattleEvent(BattleEventKind.StealItem, thief.Target, aimed, 0, null, Affinity.Normal, item.Item));
                break;
            default:
                throw new SimulationException($"the steal entry {pick} of '{profile.Id.Value}', whose kind names no rule (T-2)", context);
        }
    }

    /// <summary>
    /// Picks the entry of a successful steal (D-1051): a piece of gear at the gear chance, from
    /// the remaining gear that the party has room for, or else a remaining gold or item entry.
    /// When only gear remains, the first gold entry of the list comes back.
    /// </summary>
    private static int PickOf(RunState state, ProfileRecord profile, List<int> left, int successes, RandomStream stream, RunContext context)
    {
        List<int> gear = [];
        List<int> others = [];
        foreach (int entry in left)
        {
            if (profile.Steal[entry] is StealGear piece)
            {
                if (state.Characters.OwnedCount(piece.Gear) < state.BattleContent.Piece(piece.Gear).Limit)
                {
                    gear.Add(entry);
                }
            }
            else
            {
                others.Add(entry);
            }
        }

        if (gear.Count > 0 && stream.NextChance(GearChance(profile.StealGearChance, successes, state.BattleContent.Rules), context))
        {
            return gear[stream.NextInt(gear.Count, context)];
        }

        if (others.Count > 0)
        {
            return others[stream.NextInt(others.Count, context)];
        }

        for (int entry = 0; entry < profile.Steal.Count; entry += 1)
        {
            if (profile.Steal[entry] is StealGold)
            {
                return entry;
            }
        }

        throw new SimulationException($"a steal from '{profile.Id.Value}', whose list holds gear and no gold, which the load refuses (T-2, D-1051)", context);
    }

    /// <summary>
    /// Rolls the drops of a win (D-1042). For each fallen enemy in slot order, the battle stream
    /// rolls each entry of the drop list of its profile one time. A drop over the stack limit
    /// leaves the game, and a line names it.
    /// </summary>
    /// <param name="state">The run.</param>
    /// <param name="battle">The fight, which the party won.</param>
    /// <param name="context">The seed, the tick, and the ids, for an error (T-2).</param>
    internal static void Drop(RunState state, Battle battle, RunContext context)
    {
        BattleContent content = state.BattleContent;
        RandomStream stream = state.Stream(StreamId.Battle);
        foreach (Combatant enemy in battle.Enemies)
        {
            if (enemy.Place != CombatantPlace.Down)
            {
                continue;
            }

            ProfileRecord profile = content.Profile(battle.Group.Entries[enemy.Slot].Profile);
            foreach (DropEntry entry in profile.Drops)
            {
                if (!stream.NextChance(entry.Chance, context))
                {
                    continue;
                }

                BattleEventKind kind = state.Characters.Pick(entry.Item, 1, content) > 0 ? BattleEventKind.DropLost : BattleEventKind.Drop;
                state.AddEvent(new BattleEvent(kind, enemy.Target, null, 0, null, Affinity.Normal, entry.Item));
            }
        }
    }
}
