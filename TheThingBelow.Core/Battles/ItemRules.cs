using System;
using TheThingBelow.Core.Content;
using TheThingBelow.Core.Runs;

namespace TheThingBelow.Core.Battles;

/// <summary>
/// The use of an item from the item window, outside a fight (D-382, D-1046, D-1049). Each
/// amount applies in full, because the item rate cuts an amount in a fight alone.
/// `BattleTurns` holds the use in a fight.
/// </summary>
public static class ItemRules
{
    /// <summary>
    /// Gives the reason that the item window refuses a use now, or no value when the use is
    /// legal. The window refuses a use that changes nothing, and it spends no item (D-1049).
    /// </summary>
    /// <param name="state">The run.</param>
    /// <param name="item">The id of the item.</param>
    /// <param name="target">The party slot of the target.</param>
    /// <returns>The reason, or no value.</returns>
    public static string? RefusalOfMenuUse(RunState state, ContentId item, int target)
    {
        ArgumentNullException.ThrowIfNull(state);
        ArgumentNullException.ThrowIfNull(item);

        BattleContent content = state.BattleContent;
        if (string.CompareOrdinal(item.Kind, ItemList.Kind) != 0 || !content.Items.Holds(item))
        {
            return $"the id '{item.Value}', which the item file does not hold (D-1038)";
        }

        if (state.Characters.CountOf(item) == 0)
        {
            return $"a use of '{item.Value}', and the pack holds none (D-775)";
        }

        if (content.Item(item) is not UsedUpItem used)
        {
            return $"a use of the key item '{item.Value}', which the item window does not use (D-1038)";
        }

        if (target < 0 || target >= state.Characters.Members.Count)
        {
            return $"the character slot {target}, and the party holds the slots 0 to {state.Characters.Members.Count - 1}";
        }

        PartyMember member = state.Characters.Members[target];
        string who = member.Record.Id.Value;
        if (used is ReviveItem)
        {
            return member.Down ? null : $"a revive of '{who}', who stands (D-36, D-1046)";
        }

        if (member.Down)
        {
            return $"a use of '{item.Value}' on '{who}', who is down, and the item reaches a character who stands (D-36, D-1046)";
        }

        return used switch
        {
            HealItem => member.Health < member.Stats.Health ? null : $"a heal of '{who}' at full health, which changes nothing (D-1049)",
            RestoreItem => member.Mp < member.Stats.Mp ? null : $"a restore of '{who}' at full MP, which changes nothing (D-1049)",
            CureItem cure => HoldsAny(member, cure) ? null : $"a cure of '{who}', who holds none of its statuses, which changes nothing (D-1049)",
            _ => $"the item '{item.Value}', whose effect names no rule (T-2)",
        };
    }

    /// <summary>Uses one item from the item window on one character, at the full amount (D-382, D-1046).</summary>
    /// <param name="state">The run.</param>
    /// <param name="item">The id of the item.</param>
    /// <param name="target">The party slot of the target.</param>
    /// <param name="context">The seed, the tick, and the ids, for an error (T-2).</param>
    /// <exception cref="SimulationException">The rules refuse the use, and the error names the reason (T-2).</exception>
    public static void UseFromMenu(RunState state, ContentId item, int target, RunContext context)
    {
        ArgumentNullException.ThrowIfNull(state);
        ArgumentNullException.ThrowIfNull(item);
        ArgumentNullException.ThrowIfNull(context);

        if (RefusalOfMenuUse(state, item, target) is string refusal)
        {
            throw new SimulationException($"a use of an item from the menu, and the rules refuse it: {refusal}", context);
        }

        PartyMember member = state.Characters.Members[target];
        UsedUpItem used = (UsedUpItem)state.BattleContent.Item(item);
        state.Characters.Take(item, context);
        switch (used)
        {
            case HealItem heal:
                member.Health = Math.Min(member.Stats.Health, checked(member.Health + heal.Amount));
                break;
            case RestoreItem restore:
                member.Mp = Math.Min(member.Stats.Mp, checked(member.Mp + restore.Amount));
                break;
            case CureItem cure:
                member.Statuses = LessonRules.Without(member.Statuses, cure.Statuses);
                break;
            case ReviveItem revive:
                member.Health = Math.Min(member.Stats.Health, revive.Amount);
                break;
            default:
                throw new SimulationException($"the item '{item.Value}', whose effect names no rule (T-2)", context);
        }
    }

    private static bool HoldsAny(PartyMember member, CureItem cure)
    {
        foreach (StatusKind status in cure.Statuses)
        {
            if (LessonRules.Holds(member.Statuses, status))
            {
                return true;
            }
        }

        return false;
    }
}
