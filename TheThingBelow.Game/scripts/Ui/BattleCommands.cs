using System;
using System.Collections.Generic;
using TheThingBelow.Core.Battles;
using TheThingBelow.Core.Content;
using TheThingBelow.Core.Runs;

namespace TheThingBelow.Game.Ui;

/// <summary>The part of the command menu that the cursor stands in (D-827).</summary>
public enum CommandStage
{
    /// <summary>The six actions of a turn.</summary>
    Action,

    /// <summary>The lessons of the character, after the Lessons command (D-1031).</summary>
    Lesson,

    /// <summary>The opened forms of the chosen lesson, with the cost of each (D-1027).</summary>
    Form,

    /// <summary>The items of the pack, after the item action.</summary>
    Item,

    /// <summary>The targets of an attack, a form, or an item.</summary>
    Target,
}

/// <summary>
/// The command menu of one turn of a character: the action, then the lesson and its form, or
/// the item, then the target (D-111, D-827, D-1027, D-1031). Each move of the cursor stays in
/// Game, and the menu makes one intent when
/// the player confirms a whole choice, so the record holds the choice alone (D-493).
/// </summary>
/// <remarks>
/// The menu offers each choice that the rules take now, and it reads that answer from the
/// rules themselves, so no rule lives here (D-100). An action with no legal choice, such as
/// the item action with an empty pack, stays on the menu and refuses the confirm.
/// <para>
/// This type holds no Godot value, so a test reads it from the built Game assembly with no
/// engine (D-614). PR-62 fits the cursor rules of D-872 to this menu. The remembered cursor
/// of D-226 comes from <see cref="CommandMemory"/>.
/// </para>
/// </remarks>
public sealed class BattleCommands
{
    /// <summary>The actions of the menu, in the order that the menu shows them (D-359).</summary>
    public static readonly IReadOnlyList<BattleAction> Actions =
    [
        BattleAction.Attack,
        BattleAction.Lesson,
        BattleAction.Defend,
        BattleAction.Step,
        BattleAction.Item,
        BattleAction.Flee,
    ];

    private readonly RunState state;
    private readonly List<PackValues> items = [];
    private readonly List<BattleTarget> targets = [];
    private readonly List<ContentId> lessons = [];
    private readonly List<LessonForm> forms = [];
    private readonly int slot;
    private BattleAction action = BattleAction.Attack;
    private ContentId? item;
    private ContentId? lesson;
    private int form;

    private BattleCommands(RunState state, Combatant actor)
    {
        this.state = state;
        this.Actor = actor.Id;
        this.ActorRow = actor.Row;
        this.slot = actor.Slot;
    }

    /// <summary>The content id of the character whose turn it is, which the remembered cursor keys on (D-226).</summary>
    public ContentId Actor { get; }

    /// <summary>The action of the choice that the menu builds now.</summary>
    public BattleAction Action => this.action;

    /// <summary>The row of the character whose turn it is, which names the step on the menu (D-836).</summary>
    public BattleRow ActorRow { get; }

    /// <summary>The part of the menu that the cursor stands in.</summary>
    public CommandStage Stage { get; private set; } = CommandStage.Action;

    /// <summary>The place of the cursor in the list of the stage.</summary>
    public int Cursor { get; private set; }

    /// <summary>The items that the item stage offers, with the count of each, in the order of the pack.</summary>
    public IReadOnlyList<PackValues> Items => this.items;

    /// <summary>The lessons that the lesson stage offers: the lesson of each filled slot of the character, in slot order (D-356).</summary>
    public IReadOnlyList<ContentId> Lessons => this.lessons;

    /// <summary>The forms that the form stage offers: each form of the chosen lesson that the character opened (D-1027).</summary>
    public IReadOnlyList<LessonForm> Forms => this.forms;

    /// <summary>The lesson of the form stage, or no value before the lesson stage confirms one.</summary>
    public ContentId? ChosenLesson => this.lesson;

    /// <summary>The targets that the target stage offers, in slot order.</summary>
    public IReadOnlyList<BattleTarget> Targets => this.targets;

    /// <summary>The target under the cursor, or no value outside the target stage (D-833).</summary>
    public BattleTarget? PointedTarget => this.Stage == CommandStage.Target ? this.targets[this.Cursor] : null;

    /// <summary>The count of entries of the stage that the cursor stands in.</summary>
    public int Count => this.Stage switch
    {
        CommandStage.Action => Actions.Count,
        CommandStage.Item => this.items.Count,
        CommandStage.Lesson => this.lessons.Count,
        CommandStage.Form => this.forms.Count,
        _ => this.targets.Count,
    };

    /// <summary>Opens the menu for the character whose turn it is.</summary>
    /// <param name="state">The run, whose battle waits for the command of a character (D-532).</param>
    /// <param name="memory">The remembered cursor, which can give the first action (D-226).</param>
    /// <returns>The menu, with the cursor on the remembered action or on the attack.</returns>
    /// <exception cref="ArgumentNullException">The state or the memory is null (T-2).</exception>
    /// <exception cref="InvalidOperationException">No fight runs, or no character has the turn (T-2).</exception>
    public static BattleCommands Open(RunState state, CommandMemory memory)
    {
        ArgumentNullException.ThrowIfNull(state);
        ArgumentNullException.ThrowIfNull(memory);

        if (state.Battle is not Battle battle
            || battle.Outcome != BattleOutcome.Running
            || battle.Next() is not Combatant next
            || next.Side != BattleSide.Party)
        {
            throw new InvalidOperationException(
                $"The command menu opened at tick {state.Tick}, and no fight runs with a character to command (D-532, T-2).");
        }

        BattleCommands opened = new(state, next);
        if (memory.StartOf(next.Id) is BattleAction start)
        {
            opened.action = start;
            opened.Cursor = IndexOf(start);
        }

        return opened;
    }

    /// <summary>Tells whether the action takes at least one choice that the rules allow now.</summary>
    /// <param name="offered">The action.</param>
    /// <returns>True when the menu can confirm the action.</returns>
    public bool Allows(BattleAction offered) => offered switch
    {
        BattleAction.Attack => this.TargetsOf(BattleAction.Attack, null).Count > 0,
        BattleAction.Item => this.UsableItems().Count > 0,
        BattleAction.Lesson => this.LessonsOfSlots().Exists(this.AllowsLesson),
        _ => BattleTurns.RefusalOf(this.state, new BattleChoice(offered, null, null)) is null,
    };

    /// <summary>Tells whether a lesson of the character has a form that the rules take now, on at least one target (D-1027).</summary>
    /// <param name="offered">The lesson.</param>
    /// <returns>True when the lesson stage can confirm the lesson.</returns>
    public bool AllowsLesson(ContentId offered)
    {
        ArgumentNullException.ThrowIfNull(offered);

        int opened = this.OpenedFormsOf(offered).Count;
        for (int index = 0; index < opened; index += 1)
        {
            if (this.AllowsForm(offered, index))
            {
                return true;
            }
        }

        return false;
    }

    /// <summary>Tells whether the rules take a form of a lesson now, on at least one target: the MP, silence, and the reach (D-42, D-806).</summary>
    /// <param name="offered">The lesson.</param>
    /// <param name="index">The index of the form, from zero.</param>
    /// <returns>True when the form stage can confirm the form.</returns>
    public bool AllowsForm(ContentId offered, int index)
    {
        ArgumentNullException.ThrowIfNull(offered);

        return this.LessonTargets(offered, index).Count > 0;
    }

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

    /// <summary>
    /// Moves the cursor to the row above or below in the grid of the menu, in the same column,
    /// and wraps at the top and the bottom (D-1034). A short last row takes its last entry.
    /// </summary>
    /// <param name="step">-1 for the row above, and 1 for the row below.</param>
    /// <exception cref="ArgumentOutOfRangeException">The step is not -1 or 1 (T-2).</exception>
    public void MoveRow(int step)
    {
        if (step != -1 && step != 1)
        {
            throw new ArgumentOutOfRangeException(nameof(step), step, "The cursor moves by one row, -1 or 1 (T-2).");
        }

        int rows = (this.Count + BattleLayout.CommandColumns - 1) / BattleLayout.CommandColumns;
        int row = ((this.Cursor / BattleLayout.CommandColumns) + step + rows) % rows;
        this.Cursor = Math.Min((row * BattleLayout.CommandColumns) + (this.Cursor % BattleLayout.CommandColumns), this.Count - 1);
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
            case CommandStage.Lesson:
                this.ConfirmLesson(this.lessons[this.Cursor]);
                return null;
            case CommandStage.Form:
                this.ConfirmForm(this.Cursor);
                return null;
            case CommandStage.Target when this.action == BattleAction.Lesson:
                return Intent.OfBattleLesson(this.lesson!, this.form, this.targets[this.Cursor]);
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
            case CommandStage.Target when this.action == BattleAction.Lesson:
                this.Stage = CommandStage.Form;
                this.Cursor = this.form;
                return;
            case CommandStage.Form:
                this.Stage = CommandStage.Lesson;
                this.Cursor = Math.Max(0, this.lessons.FindIndex(entry => string.CompareOrdinal(entry.Value, this.lesson!.Value) == 0));
                return;
            case CommandStage.Target:
            case CommandStage.Item:
            case CommandStage.Lesson:
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
            case BattleAction.Lesson:
                this.lessons.Clear();
                this.lessons.AddRange(this.LessonsOfSlots());
                this.Stage = CommandStage.Lesson;
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

    private void ConfirmLesson(ContentId chosen)
    {
        if (!this.AllowsLesson(chosen))
        {
            return;
        }

        this.lesson = chosen;
        this.forms.Clear();
        this.forms.AddRange(this.OpenedFormsOf(chosen));
        this.Stage = CommandStage.Form;
        this.Cursor = 0;
    }

    private void ConfirmForm(int index)
    {
        ContentId chosen = this.lesson ?? throw new InvalidOperationException("The form stage holds no lesson (T-2).");
        if (!this.AllowsForm(chosen, index))
        {
            return;
        }

        this.form = index;
        this.targets.Clear();
        this.targets.AddRange(this.LessonTargets(chosen, index));
        this.Stage = CommandStage.Target;
        this.Cursor = 0;
    }

    /// <summary>Gives the lesson of each filled slot of the character, in slot order (D-356).</summary>
    private List<ContentId> LessonsOfSlots()
    {
        var held = new List<ContentId>();
        foreach (ContentId? carried in this.state.Characters.Members[this.slot].Slots)
        {
            if (carried is ContentId one)
            {
                held.Add(one);
            }
        }

        return held;
    }

    /// <summary>Gives the forms of a lesson that the character opened (D-361, D-539).</summary>
    private List<LessonForm> OpenedFormsOf(ContentId chosen)
    {
        LessonRecord record = this.state.BattleContent.Lessons.Lesson(chosen);
        int opened = record.OpenedAt(this.state.Characters.Members[this.slot].PointsOf(chosen));
        var found = new List<LessonForm>();
        for (int index = 0; index < opened; index += 1)
        {
            found.Add(record.Forms[index]);
        }

        return found;
    }

    /// <summary>Gives each combatant of either side that the rules let the form reach now, in slot order (D-1029).</summary>
    private List<BattleTarget> LessonTargets(ContentId chosen, int index)
    {
        Battle battle = this.state.Battle ?? throw new InvalidOperationException(
            $"The command menu reads the targets of a lesson at tick {this.state.Tick}, and no battle runs (T-2).");
        var found = new List<BattleTarget>();
        foreach (IReadOnlyList<Combatant> side in new[] { battle.Enemies, battle.Party })
        {
            foreach (Combatant combatant in side)
            {
                if (BattleTurns.RefusalOf(this.state, new BattleChoice(BattleAction.Lesson, combatant.Target, null, chosen, index)) is null)
                {
                    found.Add(combatant.Target);
                }
            }
        }

        return found;
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
