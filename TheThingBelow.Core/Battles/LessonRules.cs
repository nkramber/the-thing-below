using System;
using System.Collections.Generic;
using TheThingBelow.Core.Content;
using TheThingBelow.Core.Runs;
using TheThingBelow.Core.Story;

namespace TheThingBelow.Core.Battles;

/// <summary>
/// The rules of the lessons outside the turn order: the aptitude bonus, the refusal of a
/// lesson use, the cast from the menu, and the points of a battle won (D-357, D-358, D-360,
/// D-391, D-393, D-1019 to D-1022, D-1028).
/// </summary>
public static class LessonRules
{
    /// <summary>
    /// Gives the aptitude bonus of a character for one kind, in basis points: the bonus of the
    /// rules for the main aptitude, half of it for a side aptitude whose flag is on, and zero for
    /// any other kind (D-358, D-360, D-538, D-1028).
    /// </summary>
    /// <param name="character">The character.</param>
    /// <param name="kind">The kind of the lesson.</param>
    /// <param name="rules">The rules, which hold the bonus.</param>
    /// <param name="flags">The story flags, which unlock the side aptitude.</param>
    /// <returns>The bonus, from zero.</returns>
    /// <exception cref="ArgumentNullException">An argument is null (T-2).</exception>
    public static int BonusOf(CharacterRecord character, AptitudeKind kind, BattleRules rules, FlagSet flags)
    {
        ArgumentNullException.ThrowIfNull(character);
        ArgumentNullException.ThrowIfNull(rules);
        ArgumentNullException.ThrowIfNull(flags);

        if (character.MainAptitude == kind)
        {
            return rules.AptitudeBonus;
        }

        return character.SideAptitude == kind && flags.IsOn(character.SideFlag) ? rules.AptitudeBonus / 2 : 0;
    }

    /// <summary>Gives the chance of a status after the aptitude bonus: the chance times the rate, to a sure hit at most (D-1028).</summary>
    /// <param name="chance">The chance of the move, in basis points.</param>
    /// <param name="rate">One plus the bonus, in basis points.</param>
    /// <param name="context">The seed, the tick, and the ids, for an error (T-2).</param>
    /// <returns>The raised chance, from the chance to 10000.</returns>
    public static int RaisedChance(int chance, int rate, RunContext context) =>
        Math.Min(BasisPoints.One, BasisPoints.Apply(chance, rate, context));

    /// <summary>
    /// Gives the reason that the rules refuse a use of a form now, or no value when the use is
    /// legal. A slot of the character must hold the lesson, the character must have opened the
    /// form, the MP must cover the cost, and silence refuses a rite (D-42, D-806, D-1027). The
    /// lists of forms of the battle screen and the lesson window read it (T-2).
    /// </summary>
    /// <param name="state">The run.</param>
    /// <param name="character">The party slot of the character.</param>
    /// <param name="lesson">The lesson.</param>
    /// <param name="form">The index of the form, from zero.</param>
    /// <param name="silenced">True while the character holds silence: in a fight on its combatant, and outside a fight on the party (D-393).</param>
    /// <returns>The reason, or no value.</returns>
    /// <exception cref="ArgumentNullException">An argument is null (T-2).</exception>
    public static string? RefusalOfForm(RunState state, int character, ContentId lesson, int form, bool silenced)
    {
        ArgumentNullException.ThrowIfNull(state);
        ArgumentNullException.ThrowIfNull(lesson);

        IReadOnlyList<PartyMember> members = state.Characters.Members;
        if (character < 0 || character >= members.Count)
        {
            return $"the character slot {character}, and the party holds the slots 0 to {members.Count - 1}";
        }

        PartyMember member = members[character];
        if (member.SlotOf(lesson) is null)
        {
            return $"the lesson '{lesson.Value}', which no slot of '{member.Record.Id.Value}' holds (D-356)";
        }

        LessonRecord record = state.BattleContent.Lessons.Lesson(lesson);
        int opened = record.OpenedAt(member.PointsOf(lesson));
        if (form < 0 || form >= opened)
        {
            return $"form {form} of '{lesson.Value}', and '{member.Record.Id.Value}' opened the forms 0 to {opened - 1} (D-539)";
        }

        int cost = record.Forms[form].Mp;
        if (member.Mp < cost)
        {
            return $"form {form} of '{lesson.Value}', which costs {cost} MP, and '{member.Record.Id.Value}' holds {member.Mp} (D-42)";
        }

        return record.IsRite && silenced
            ? $"the rite '{lesson.Value}' of '{member.Record.Id.Value}', who holds silence (D-393, D-806)"
            : null;
    }

    /// <summary>
    /// Gives the reason that the rules refuse a cast from the menu now, or no value when the
    /// cast is legal (D-391, D-393). Outside a fight, a heal or a cure works alone. The caster
    /// and the target stand, and silence on the party stops a rite.
    /// </summary>
    /// <param name="state">The run.</param>
    /// <param name="caster">The party slot of the caster.</param>
    /// <param name="lesson">The lesson.</param>
    /// <param name="form">The index of the form, from zero.</param>
    /// <param name="target">The party slot of the target.</param>
    /// <returns>The reason, or no value.</returns>
    /// <exception cref="ArgumentNullException">An argument is null (T-2).</exception>
    public static string? RefusalOfMenuCast(RunState state, int caster, ContentId lesson, int form, int target)
    {
        ArgumentNullException.ThrowIfNull(state);
        ArgumentNullException.ThrowIfNull(lesson);

        if (!state.MenuOpen || state.Battle is not null)
        {
            return "a cast from the menu while no menu is open or a battle holds the run, and a fight uses the Lessons command (D-391, D-1031)";
        }

        IReadOnlyList<PartyMember> members = state.Characters.Members;
        bool silenced = caster >= 0 && caster < members.Count && Holds(members[caster].Statuses, StatusKind.Silence);
        if (RefusalOfForm(state, caster, lesson, form, silenced) is string refusal)
        {
            return refusal;
        }

        PartyMember member = members[caster];
        AbilityRecord ability = FormAbility(state, lesson, form);
        if (ability is not (HealAbility or CureAbility))
        {
            return $"the form '{ability.Id.Value}' from the menu, and a heal or a cure alone works outside a fight (D-391)";
        }

        if (member.Down)
        {
            return $"a cast of '{member.Record.Id.Value}', who is down (D-36)";
        }

        if (target < 0 || target >= members.Count)
        {
            return $"the target slot {target}, and the party holds the slots 0 to {members.Count - 1}";
        }

        return members[target].Down
            ? $"a cast on '{members[target].Record.Id.Value}', who is down, and a rare item or a hub stands a character up (D-36)"
            : null;
    }

    /// <summary>
    /// Casts a Mend rite or a cure rite from the menu (D-391). The caster spends the MP, and the
    /// heal or the cure takes the aptitude bonus of the caster (D-1028).
    /// </summary>
    /// <param name="state">The run.</param>
    /// <param name="caster">The party slot of the caster.</param>
    /// <param name="lesson">The lesson.</param>
    /// <param name="form">The index of the form, from zero.</param>
    /// <param name="target">The party slot of the target.</param>
    /// <param name="context">The seed, the tick, and the intent, for an error (T-2).</param>
    /// <exception cref="ArgumentNullException">An argument is null (T-2).</exception>
    /// <exception cref="SimulationException">The rules refuse the cast, and the error names the reason (T-2).</exception>
    public static void CastFromMenu(RunState state, int caster, ContentId lesson, int form, int target, RunContext context)
    {
        ArgumentNullException.ThrowIfNull(state);
        ArgumentNullException.ThrowIfNull(lesson);
        ArgumentNullException.ThrowIfNull(context);

        if (RefusalOfMenuCast(state, caster, lesson, form, target) is string refusal)
        {
            throw new SimulationException($"a cast from the menu, and the rules refuse it: {refusal}", context);
        }

        PartyMember member = state.Characters.Members[caster];
        PartyMember aimed = state.Characters.Members[target];
        LessonRecord record = state.BattleContent.Lessons.Lesson(lesson);
        member.Mp -= record.Forms[form].Mp;
        int bonus = BonusOf(member.Record, record.Kind, state.BattleContent.Rules, state.Story.Flags);
        switch (FormAbility(state, lesson, form))
        {
            case HealAbility heal:
                int healed = BasisPoints.Apply(heal.Heal, BasisPoints.One + bonus, context);
                aimed.Health = Math.Min(aimed.Stats.Health, aimed.Health + healed);
                break;
            case CureAbility cure:
                aimed.Statuses = Without(aimed.Statuses, cure.Statuses);
                break;
            default:
                throw new SimulationException($"a cast from the menu of '{lesson.Value}', whose form is neither a heal nor a cure (D-391)", context);
        }
    }

    /// <summary>Gives the ability of one form of a lesson (D-1026).</summary>
    /// <param name="state">The run.</param>
    /// <param name="lesson">The lesson.</param>
    /// <param name="form">The index of the form, which the caller checked.</param>
    /// <returns>The ability of the ability file.</returns>
    public static AbilityRecord FormAbility(RunState state, ContentId lesson, int form)
    {
        ArgumentNullException.ThrowIfNull(state);
        ArgumentNullException.ThrowIfNull(lesson);

        LessonRecord record = state.BattleContent.Lessons.Lesson(lesson);
        return state.BattleContent.Abilities.Ability(record.Forms[form].Ability);
    }

    /// <summary>
    /// Gives the points of each equipped lesson of each character after a battle won, and one
    /// event for each form that opens (D-357, D-1019 to D-1022). Each lesson gains the
    /// experience of each enemy, shrunk by the gap between the level of the lesson and the level
    /// of the enemy. A downed character gains none, and a lesson stops at its last form.
    /// </summary>
    /// <param name="state">The run, whose party took the health of the fight back.</param>
    /// <param name="battle">The battle won.</param>
    internal static void Award(RunState state, Battle battle)
    {
        BattleContent content = state.BattleContent;
        for (int slot = 0; slot < battle.Party.Count; slot += 1)
        {
            PartyMember member = state.Characters.Members[slot];
            ExperienceStanding standing = member.Down ? ExperienceStanding.Down : ExperienceStanding.Fought;
            foreach (ContentId? held in member.Slots)
            {
                if (held is not ContentId lessonId)
                {
                    continue;
                }

                LessonRecord lesson = content.Lessons.Lesson(lessonId);
                int before = member.PointsOf(lessonId);
                int earned = Experience.EarnedBy(ShrunkForLesson(before, battle, content), standing);
                if (earned == 0 || member.GainPoints(lesson, earned) == 0)
                {
                    continue;
                }

                AddOpenedForms(state, lesson, slot, before, member.PointsOf(lessonId));
            }
        }
    }

    /// <summary>
    /// Gives the level of a total of lesson points: the level that the points reach on the
    /// experience table (D-971, D-1020).
    /// </summary>
    /// <param name="points">The points of one character for one lesson.</param>
    /// <param name="rules">The rules, which hold the table.</param>
    /// <returns>The lesson level, from 1 to <see cref="StatCurve.HighestLevel"/>.</returns>
    public static int LessonLevelOf(int points, BattleRules rules) => Experience.LevelOf(points, rules);

    /// <summary>The sum of the experience of each enemy, each shrunk by the gap to the lesson level (D-1020).</summary>
    private static int ShrunkForLesson(int points, Battle battle, BattleContent content)
    {
        int level = LessonLevelOf(points, content.Rules);
        int shrunk = 0;
        foreach (Combatant enemy in battle.Enemies)
        {
            EnemyRecord record = content.Enemy(enemy.Id);
            shrunk = checked(shrunk + Experience.Shrunk(record.Experience, level, record.Level, content.Rules));
        }

        return shrunk;
    }

    private static void AddOpenedForms(RunState state, LessonRecord lesson, int slot, int before, int after)
    {
        for (int index = lesson.OpenedAt(before); index < lesson.OpenedAt(after); index += 1)
        {
            state.AddEvent(new BattleEvent(
                BattleEventKind.FormOpened,
                new BattleTarget(BattleSide.Party, slot),
                null,
                after,
                null,
                Affinity.Normal,
                lesson.Forms[index].Ability));
        }
    }

    internal static bool Holds(IReadOnlyList<StatusKind> statuses, StatusKind status)
    {
        foreach (StatusKind held in statuses)
        {
            if (held == status)
            {
                return true;
            }
        }

        return false;
    }

    internal static List<StatusKind> Without(IReadOnlyList<StatusKind> statuses, IReadOnlyList<StatusKind> cured)
    {
        List<StatusKind> kept = [];
        foreach (StatusKind status in statuses)
        {
            if (!Holds(cured, status))
            {
                kept.Add(status);
            }
        }

        return kept;
    }
}
