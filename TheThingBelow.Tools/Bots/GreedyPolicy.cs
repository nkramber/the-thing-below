using System;
using System.Collections.Generic;
using TheThingBelow.Core.Battles;
using TheThingBelow.Core.Content;
using TheThingBelow.Core.Maps;
using TheThingBelow.Core.Runs;

namespace TheThingBelow.Tools.Bots;

/// <summary>
/// The greedy policy (D-1183). On the map, it walks the shortest path to the nearest target that
/// it did not reach, then confirms. In a battle, it attacks the enemy with the least health, uses a
/// heal item on a character under a quarter of health, and never flees.
/// </summary>
/// <remarks>
/// The policy draws no random number, so two runs of one seed differ only by the rolls of the
/// rules. It ends each step of a story scene at once and picks the first option of each choice,
/// as a bot answers each wait intent (D-540). At an open service it saves or rests one time, then
/// it closes the menu.
/// </remarks>
public sealed class GreedyPolicy : IBotPolicy
{
    private readonly HashSet<string> reached = new(StringComparer.Ordinal);

    private bool served;

    /// <inheritdoc/>
    public BotPolicyKind Kind => BotPolicyKind.Greedy;

    /// <inheritdoc/>
    public Intent? Choose(RunState state, IReadOnlyList<Intent> accepted)
    {
        ArgumentNullException.ThrowIfNull(state);
        ArgumentNullException.ThrowIfNull(accepted);

        foreach (ContentId wait in new[] { IntentIds.StoryResume, IntentIds.StoryStepEnd, IntentIds.StoryPick, IntentIds.WaitBattleEnd })
        {
            if (FirstOf(accepted, wait) is Intent answer)
            {
                return answer;
            }
        }

        if (state.Battle is Battle battle && !state.MenuOpen)
        {
            return Command(state, battle, accepted);
        }

        if (state.MenuOpen)
        {
            return this.AtMenu(accepted);
        }

        this.served = false;
        return this.Walk(state, accepted);
    }

    /// <summary>Gives the heal of a character under a quarter of health, else the attack on the enemy with the least health, else a defend (D-1183).</summary>
    private static Intent? Command(RunState state, Battle battle, IReadOnlyList<Intent> accepted)
    {
        Intent? heal = null;
        int lowest = int.MaxValue;
        Intent? attack = null;
        int weakest = int.MaxValue;
        foreach (Intent intent in accepted)
        {
            if (intent.Target is not BattleTarget target)
            {
                continue;
            }

            Combatant aimed = target.Side == BattleSide.Party ? battle.Party[target.Slot] : battle.Enemies[target.Slot];
            bool low = checked(aimed.Health * 4) < aimed.FullHealth;
            if (Is(intent, IntentIds.BattleItem) && low && IsHealItem(state, intent.Item) && aimed.Health < lowest)
            {
                heal = intent;
                lowest = aimed.Health;
            }

            if (Is(intent, IntentIds.BattleAttack) && aimed.Health < weakest)
            {
                attack = intent;
                weakest = aimed.Health;
            }
        }

        return heal ?? attack ?? FirstOf(accepted, IntentIds.BattleDefend) ?? FirstOf(accepted, IntentIds.BattleStep);
    }

    /// <summary>Saves, or else rests, one time at an open service, then closes the menu.</summary>
    private Intent? AtMenu(IReadOnlyList<Intent> accepted)
    {
        if (!this.served)
        {
            this.served = true;
            if ((FirstOf(accepted, IntentIds.Save) ?? FirstOf(accepted, IntentIds.HubRest)) is Intent service)
            {
                return service;
            }
        }

        return FirstOf(accepted, IntentIds.CloseMenu);
    }

    /// <summary>Walks toward the nearest target that the policy did not reach, and marks each target that it reaches (D-1183).</summary>
    private Intent? Walk(RunState state, IReadOnlyList<Intent> accepted)
    {
        MapState party = state.Party;
        if (party.Stepping is not null || party.Patrols.Encounter is not null)
        {
            return null;
        }

        WalkTarget? nearest = this.Nearest(party, WalkTargets.Of(state), out StepDirection? step);
        if (nearest is null)
        {
            // Every target is reached or out of reach, so the walk starts over.
            this.reached.Clear();
            return null;
        }

        if (step is StepDirection toward)
        {
            return FirstOf(accepted, MoveOf(toward));
        }

        // The lead stands where it reaches the target.
        if (nearest.Reach == TargetReach.StandOn)
        {
            this.reached.Add(nearest.Key);
            return null;
        }

        StepDirection facing = WalkTargets.Toward(party.LeadAt, nearest.At)
            ?? throw new InvalidOperationException($"the greedy policy stands at {party.LeadAt} and reaches '{nearest.Key}' at {nearest.At}, and the two tiles do not touch (T-2)");

        // A step into an enemy bumps it, and a step onto a tile that takes the lead walks onto it
        // (D-747, D-1142). Either one reaches the target.
        if (nearest.Reach == TargetReach.Bump || party.CanStepOnto(nearest.At))
        {
            this.reached.Add(nearest.Key);
            return FirstOf(accepted, MoveOf(facing));
        }

        // A step into an NPC or a solid thing turns the lead alone, and the confirm follows (D-1139).
        if (party.Facing != facing)
        {
            return FirstOf(accepted, MoveOf(facing));
        }

        this.reached.Add(nearest.Key);
        return FirstOf(accepted, IntentIds.Confirm);
    }

    /// <summary>
    /// Gives the target with the shortest path that the policy did not reach, and marks each target
    /// with no path as reached. An exit counts once no other target is left (D-1216).
    /// </summary>
    private WalkTarget? Nearest(MapState party, IReadOnlyList<WalkTarget> targets, out StepDirection? step)
    {
        WalkTarget? found = this.Nearest(party, targets, last: false, out step);
        return found ?? this.Nearest(party, targets, last: true, out step);
    }

    /// <summary>Gives the target of one kind, an exit or any other, with the shortest path that the policy did not reach.</summary>
    private WalkTarget? Nearest(MapState party, IReadOnlyList<WalkTarget> targets, bool last, out StepDirection? step)
    {
        WalkTarget? nearest = null;
        int shortest = int.MaxValue;
        step = null;
        foreach (WalkTarget target in targets)
        {
            if (target.Last != last || this.reached.Contains(target.Key))
            {
                continue;
            }

            StepDirection? first = WalkTargets.FirstStep(party, target, out int length);
            if (length < 0)
            {
                this.reached.Add(target.Key);
                continue;
            }

            if (length < shortest)
            {
                nearest = target;
                shortest = length;
                step = first;
            }
        }

        return nearest;
    }

    private static bool IsHealItem(RunState state, ContentId? item) =>
        item is ContentId id && string.CompareOrdinal(id.Kind, ItemList.Kind) == 0 && state.BattleContent.Item(id) is HealItem;

    private static ContentId MoveOf(StepDirection direction) => direction switch
    {
        StepDirection.North => IntentIds.MoveNorth,
        StepDirection.South => IntentIds.MoveSouth,
        StepDirection.East => IntentIds.MoveEast,
        StepDirection.West => IntentIds.MoveWest,
        _ => throw new ArgumentOutOfRangeException(nameof(direction), direction, "the value names no step direction (D-716)"),
    };

    private static bool Is(Intent intent, ContentId action) => string.CompareOrdinal(intent.Action.Value, action.Value) == 0;

    private static Intent? FirstOf(IReadOnlyList<Intent> accepted, ContentId action)
    {
        foreach (Intent intent in accepted)
        {
            if (Is(intent, action))
            {
                return intent;
            }
        }

        return null;
    }
}
