using System;
using System.Collections.Generic;
using TheThingBelow.Core.Battles;
using TheThingBelow.Core.Content;
using TheThingBelow.Core.Runs;

namespace TheThingBelow.Game.Ui;

/// <summary>The part of the command menu that the cursor stands in (D-827).</summary>
public enum CommandStage
{
    /// <summary>The five actions of a turn.</summary>
    Action,

    /// <summary>The items of the pack, after the item action.</summary>
    Item,

    /// <summary>The targets of an attack or of an item.</summary>
    Target,
}

/// <summary>
/// The command menu of one turn of a character: the action, then the item or the target
/// (D-111, D-827). Each move of the cursor stays in Game, and the menu makes one intent when
/// the player confirms a whole choice, so the record holds the choice alone (D-493).
/// </summary>
/// <remarks>
/// The menu offers each choice that the rules take now, and it reads that answer from the
/// rules themselves, so no rule lives here (D-100). An action with no legal choice, such as
/// the item action with an empty pack, stays on the menu and refuses the confirm.
/// <para>
/// This type holds no Godot value, so a test reads it from the built Game assembly with no
/// engine (D-614). PR-62 fits the cursor rules of OQ-110 to this menu, and PR-63 adds the
/// remembered cursor of D-226.
/// </para>
/// </remarks>
public sealed class BattleCommands
{
    /// <summary>The actions of the menu, in the order that the menu shows them (D-359).</summary>
    public static readonly IReadOnlyList<BattleAction> Actions =
    [
        BattleAction.Attack,
        BattleAction.Defend,
        BattleAction.Step,
        BattleAction.Item,
        BattleAction.Flee,
    ];

    private readonly RunState state;
    private readonly List<PackValues> items = [];
    private readonly List<BattleTarget> targets = [];
    private BattleAction action = BattleAction.Attack;
    private ContentId? item;

    private BattleCommands(RunState state)
    {
        this.state = state;
    }

    /// <summary>The part of the menu that the cursor stands in.</summary>
    public CommandStage Stage { get; private set; } = CommandStage.Action;

    /// <summary>The place of the cursor in the list of the stage.</summary>
    public int Cursor { get; private set; }

    /// <summary>The items that the item stage offers, with the count of each, in the order of the pack.</summary>
    public IReadOnlyList<PackValues> Items => this.items;

    /// <summary>The targets that the target stage offers, in slot order.</summary>
    public IReadOnlyList<BattleTarget> Targets => this.targets;

    /// <summary>The target under the cursor, or no value outside the target stage (D-833).</summary>
    public BattleTarget? PointedTarget => this.Stage == CommandStage.Target ? this.targets[this.Cursor] : null;

    /// <summary>The count of entries of the stage that the cursor stands in.</summary>
    public int Count => this.Stage switch
    {
        CommandStage.Action => Actions.Count,
        CommandStage.Item => this.items.Count,
        _ => this.targets.Count,
    };

    /// <summary>Opens the menu for the character whose turn it is.</summary>
    /// <param name="state">The run, whose battle waits for the command of a character (D-532).</param>
    /// <returns>The menu, with the cursor on the attack.</returns>
    /// <exception cref="ArgumentNullException">The state is null (T-2).</exception>
    /// <exception cref="InvalidOperationException">No fight runs, or no character has the turn (T-2).</exception>
    public static BattleCommands Open(RunState state)
    {
        ArgumentNullException.ThrowIfNull(state);

        if (state.Battle is not Battle battle
            || battle.Outcome != BattleOutcome.Running
            || battle.Next() is not Combatant next
            || next.Side != BattleSide.Party)
        {
            throw new InvalidOperationException(
                $"The command menu opened at tick {state.Tick}, and no fight runs with a character to command (D-532, T-2).");
        }

        return new BattleCommands(state);
    }

    /// <summary>Tells whether the action takes at least one choice that the rules allow now.</summary>
    /// <param name="offered">The action.</param>
    /// <returns>True when the menu can confirm the action.</returns>
    public bool Allows(BattleAction offered) => offered switch
    {
        BattleAction.Attack => this.TargetsOf(BattleAction.Attack, null).Count > 0,
        BattleAction.Item => this.UsableItems().Count > 0,
        _ => BattleTurns.RefusalOf(this.state, new BattleChoice(offered, null, null)) is null,
    };

    /// <summary>Moves the cursor by one entry, and wraps at each end.</summary>
    /// <param name="step">-1 for the entry before, and 1 for the entry after.</param>
    /// <exception cref="ArgumentOutOfRangeException">The step is not -1 or 1 (T-2).</exception>
    public void Move(int step)
    {
        if (step != -1 && step != 1)
        {
            throw new ArgumentOutOfRangeException(nameof(step), step, "The cursor moves by one entry, -1 or 1 (T-2).");
        }

        this.Cursor = (this.Cursor + step + this.Count) % this.Count;
    }

    /// <summary>Confirms the entry under the cursor.</summary>
    /// <returns>The intent of a whole choice, or no value when the menu moves to its next stage or refuses the entry.</returns>
    public Intent? Confirm()
    {
        switch (this.Stage)
        {
            case CommandStage.Action:
                return this.ConfirmAction(Actions[this.Cursor]);
            case CommandStage.Item:
                this.item = this.items[this.Cursor].Item;
                this.OpenTargets(BattleAction.Item, this.item);
                return null;
            default:
                BattleTarget target = this.targets[this.Cursor];
                ContentId actionId = this.action == BattleAction.Item ? IntentIds.BattleItem : IntentIds.BattleAttack;
                return Intent.OfPlayer(actionId, target, this.action == BattleAction.Item ? this.item : null);
        }
    }

    /// <summary>Goes back one stage. The action stage stays where it is.</summary>
    public void Cancel()
    {
        switch (this.Stage)
        {
            case CommandStage.Target when this.action == BattleAction.Item:
                this.Stage = CommandStage.Item;
                this.Cursor = Math.Max(0, this.items.FindIndex(entry => string.CompareOrdinal(entry.Item.Value, this.item!.Value) == 0));
                return;
            case CommandStage.Target:
            case CommandStage.Item:
                this.Stage = CommandStage.Action;
                this.Cursor = IndexOf(this.action);
                return;
            default:
                return;
        }
    }

    private Intent? ConfirmAction(BattleAction chosen)
    {
        if (!this.Allows(chosen))
        {
            return null;
        }

        this.action = chosen;
        switch (chosen)
        {
            case BattleAction.Attack:
                this.OpenTargets(BattleAction.Attack, null);
                return null;
            case BattleAction.Item:
                this.items.Clear();
                this.items.AddRange(this.UsableItems());
                this.Stage = CommandStage.Item;
                this.Cursor = 0;
                return null;
            case BattleAction.Defend:
                return Intent.OfPlayer(IntentIds.BattleDefend);
            case BattleAction.Step:
                return Intent.OfPlayer(IntentIds.BattleStep);
            case BattleAction.Flee:
                return Intent.OfPlayer(IntentIds.BattleFlee);
            default:
                throw new InvalidOperationException($"The command menu offers the action '{chosen}', which it cannot send (T-2).");
        }
    }

    private void OpenTargets(BattleAction chosen, ContentId? used)
    {
        this.targets.Clear();
        this.targets.AddRange(this.TargetsOf(chosen, used));
        this.Stage = CommandStage.Target;
        this.Cursor = 0;
    }

    private List<BattleTarget> TargetsOf(BattleAction chosen, ContentId? used)
    {
        Battle battle = this.state.Battle ?? throw new InvalidOperationException(
            $"The command menu reads its targets at tick {this.state.Tick}, and no battle runs (T-2).");

        // An attack reaches the enemies, and an item serves the party (D-377, D-382).
        IReadOnlyList<Combatant> side = chosen == BattleAction.Attack ? battle.Enemies : battle.Party;
        var found = new List<BattleTarget>();
        foreach (Combatant combatant in side)
        {
            if (BattleTurns.RefusalOf(this.state, new BattleChoice(chosen, combatant.Target, used)) is null)
            {
                found.Add(combatant.Target);
            }
        }

        return found;
    }

    private List<PackValues> UsableItems()
    {
        var usable = new List<PackValues>();
        foreach (PackValues entry in this.state.Characters.Pack)
        {
            if (entry.Count > 0 && this.TargetsOf(BattleAction.Item, entry.Item).Count > 0)
            {
                usable.Add(entry);
            }
        }

        return usable;
    }

    private static int IndexOf(BattleAction chosen)
    {
        for (int index = 0; index < Actions.Count; index += 1)
        {
            if (Actions[index] == chosen)
            {
                return index;
            }
        }

        throw new InvalidOperationException($"The command menu holds no action '{chosen}' (T-2).");
    }
}
