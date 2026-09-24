using System;
using System.Collections.Generic;
using TheThingBelow.Core.Runs;

namespace TheThingBelow.Core.Battles;

/// <summary>Where a character stood in a battle won, which sets the part of the experience it earns (D-73, D-974).</summary>
public enum ExperienceStanding
{
    /// <summary>The character fought and stands at the end. It earns all of it.</summary>
    Fought,

    /// <summary>The character waited in the reserve. It earns half (D-73).</summary>
    Reserve,

    /// <summary>The character is down at the end. It earns none (D-974).</summary>
    Down,
}

/// <summary>
/// The experience of a battle won: the shrink of the experience of each enemy, the part of each
/// standing, the level of a total, and the award at the end of a fight (D-34, D-968 to D-974).
/// </summary>
public static class Experience
{
    /// <summary>
    /// Gives the experience of one enemy for one character, after the shrink (D-968, D-969). A character at the level
    /// of the enemy or below it earns the base in full. Each level above cuts the base by the
    /// cut of the rules, and a character past the gap earns none. The result rounds down (D-977).
    /// </summary>
    /// <param name="baseExperience">The experience of the enemy record.</param>
    /// <param name="characterLevel">The level of the character.</param>
    /// <param name="enemyLevel">The level of the enemy record.</param>
    /// <param name="rules">The rules, which hold the cut and the gap.</param>
    /// <returns>The shrunk experience, from zero to the base.</returns>
    /// <exception cref="ArgumentNullException">The rules are null (T-2).</exception>
    public static int Shrunk(int baseExperience, int characterLevel, int enemyLevel, BattleRules rules)
    {
        ArgumentNullException.ThrowIfNull(rules);

        int gap = characterLevel - enemyLevel;
        if (gap <= 0)
        {
            return baseExperience;
        }

        if (gap > rules.ExperienceGap)
        {
            return 0;
        }

        long rate = BasisPoints.One - ((long)rules.ExperienceCut * gap);
        if (rate <= 0)
        {
            return 0;
        }

        return (int)(baseExperience * rate / BasisPoints.One);
    }

    /// <summary>Gives the part of the shrunk experience that a standing earns: all, half, or none (D-73, D-969, D-974). A half rounds down.</summary>
    /// <param name="shrunk">The sum of the shrunk experience of each enemy.</param>
    /// <param name="standing">Where the character stood.</param>
    /// <returns>The experience that the character earns.</returns>
    public static int EarnedBy(int shrunk, ExperienceStanding standing) => standing switch
    {
        ExperienceStanding.Fought => shrunk,
        ExperienceStanding.Reserve => shrunk / 2,
        ExperienceStanding.Down => 0,
        _ => throw new ArgumentOutOfRangeException(nameof(standing), standing, "the value names no standing of a battle (D-73, D-974)"),
    };

    /// <summary>Gives the level of a total of experience: the highest level whose total the experience reaches (D-971).</summary>
    /// <param name="experience">The total experience of the character, from zero.</param>
    /// <param name="rules">The rules, which hold the table.</param>
    /// <returns>The level, from 1 to <see cref="StatCurve.HighestLevel"/>.</returns>
    /// <exception cref="ArgumentNullException">The rules are null (T-2).</exception>
    /// <exception cref="ArgumentOutOfRangeException">The experience is below zero (T-2).</exception>
    public static int LevelOf(int experience, BattleRules rules)
    {
        ArgumentNullException.ThrowIfNull(rules);
        ArgumentOutOfRangeException.ThrowIfNegative(experience);

        IReadOnlyList<int> table = rules.LevelExperience;
        for (int level = table.Count; level > 1; level -= 1)
        {
            if (experience >= table[level - 1])
            {
                return level;
            }
        }

        return 1;
    }

    /// <summary>
    /// Gives the experience of each character of the party at the end of a battle won, and
    /// raises the level of each one that reaches a new level (D-34, D-973). The events follow
    /// the win: the experience of each character, then each level-up (D-422, D-975).
    /// </summary>
    /// <param name="state">The run, whose party took the health of the fight back.</param>
    /// <param name="battle">The battle won.</param>
    /// <param name="context">The seed, the tick, and the ids, for an error (T-2).</param>
    internal static void Award(RunState state, Battle battle, RunContext context)
    {
        BattleContent content = state.BattleContent;
        List<(int Slot, int Level)> levelUps = [];
        for (int slot = 0; slot < battle.Party.Count; slot += 1)
        {
            PartyMember member = state.Characters.Members[slot];
            ExperienceStanding standing = member.Down ? ExperienceStanding.Down : ExperienceStanding.Fought;
            int shrunk = 0;
            foreach (Combatant enemy in battle.Enemies)
            {
                EnemyRecord record = content.Enemy(enemy.Id);
                shrunk = checked(shrunk + Shrunk(record.Experience, member.Level, record.Level, content.Rules));
            }

            int earned = EarnedBy(shrunk, standing);
            if (earned == 0)
            {
                continue;
            }

            int before = member.Level;
            int added = member.Gain(earned, content.Rules, context);
            if (added == 0)
            {
                // A character at the top of the table gains nothing, and shows no line (D-972, D-975).
                continue;
            }

            state.AddEvent(new BattleEvent(BattleEventKind.Experience, new BattleTarget(BattleSide.Party, slot), null, added));
            if (member.Level > before)
            {
                battle.Party[slot].Raise(member.StatsWith(content.Gear));
                levelUps.Add((slot, member.Level));
            }
        }

        foreach ((int slot, int level) in levelUps)
        {
            state.AddEvent(new BattleEvent(BattleEventKind.LevelUp, new BattleTarget(BattleSide.Party, slot), null, level));
        }
    }
}
