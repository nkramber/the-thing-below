using System;
using System.Collections.Generic;
using TheThingBelow.Core.Battles;
using TheThingBelow.Core.Content;
using TheThingBelow.Core.Runs;

namespace TheThingBelow.Game.Ui;

/// <summary>The list that the cursor of the lesson window stands in (D-1027, D-1030).</summary>
public enum LessonStage
{
    /// <summary>The lesson slots of one character.</summary>
    Slot,

    /// <summary>The two uses of a slot, when both apply: a cast from the menu and a swap.</summary>
    Choice,

    /// <summary>An empty slot, then each lesson of the lesson pack, for a swap.</summary>
    Pack,

    /// <summary>The opened forms of the lesson of the slot, for a cast from the menu.</summary>
    Form,

    /// <summary>The characters of the party, for the target of a cast.</summary>
    Target,
}

/// <summary>What a confirm on a slot does (D-391, D-1030).</summary>
public enum SlotUse
{
    /// <summary>A cast of a heal or a cure from the menu.</summary>
    Cast,

    /// <summary>A swap of the lesson of the slot.</summary>
    Swap,
}

/// <summary>
/// The cursor of the lesson window: the character, the slot, and the lists of a swap and of a
/// cast from the menu (D-356, D-391, D-1027, D-1030). A move of the cursor makes no intent, and
/// a whole choice makes one, so the record holds the choice alone (D-493).
/// </summary>
/// <remarks>
/// The window reads each answer from the rules: the swap from the refusal of the party state,
/// and the cast from the refusal of the lesson rules. No rule lives here (D-100). A slot with a
/// heal or a cure at a swap place offers both uses. This type holds no Godot value, so a test
/// reads it with no engine (D-614).
/// </remarks>
public sealed class LessonCursor
{
    private readonly RunState state;
    private readonly List<ContentId?> pack = [];
    private readonly List<LessonForm> forms = [];
    private readonly List<SlotUse> uses = [];

    /// <summary>Opens the cursor on the first slot of the first character.</summary>
    /// <param name="state">The run, which the cursor reads and never changes.</param>
    /// <exception cref="ArgumentNullException">The state is null (T-2).</exception>
    public LessonCursor(RunState state)
    {
        ArgumentNullException.ThrowIfNull(state);

        this.state = state;
    }

    /// <summary>The party slot of the character that the window shows.</summary>
    public int Character { get; private set; }

    /// <summary>The list that the cursor stands in.</summary>
    public LessonStage Stage { get; private set; } = LessonStage.Slot;

    /// <summary>The place of the cursor in the list of the stage.</summary>
    public int Cursor { get; private set; }

    /// <summary>The lesson slot that a swap or a cast reads, which the slot stage chose.</summary>
    public int Slot { get; private set; }

    /// <summary>The form of a cast, which the form stage chose.</summary>
    public int Form { get; private set; }

    /// <summary>The character whose lessons the window shows.</summary>
    public PartyMember Member => this.state.Characters.Members[this.Character];

    /// <summary>The two uses of the choice stage, in the order that it shows them.</summary>
    public IReadOnlyList<SlotUse> Uses => this.uses;

    /// <summary>The entries of the pack stage: no value for an empty slot, then each lesson of the lesson pack (D-1024).</summary>
    public IReadOnlyList<ContentId?> PackEntries => this.pack;

    /// <summary>The opened forms of the lesson of the slot, for the form stage (D-1027).</summary>
    public IReadOnlyList<LessonForm> Forms => this.forms;

    /// <summary>The count of entries of the list that the cursor stands in.</summary>
    public int Count => this.Stage switch
    {
        LessonStage.Slot => this.Member.Slots.Count,
        LessonStage.Choice => this.uses.Count,
        LessonStage.Pack => this.pack.Count,
        LessonStage.Form => this.forms.Count,
        _ => this.state.Characters.Members.Count,
    };

    /// <summary>The lesson of the chosen slot, or no value for an empty slot.</summary>
    public ContentId? SlotLesson => this.Member.Slots[this.Slot];

    /// <summary>Moves the cursor by one entry, and wraps at each end.</summary>
    /// <param name="step">-1 for up, and 1 for down.</param>
    /// <exception cref="ArgumentOutOfRangeException">The step is not -1 or 1 (T-2).</exception>
    public void Move(int step)
    {
        CheckStep(step);
        this.Cursor = (this.Cursor + step + this.Count) % this.Count;
    }

    /// <summary>Shows the next or the last character, in the slot stage alone. The cursor keeps its slot when the character has it.</summary>
    /// <param name="step">-1 for the character before, and 1 for the character after.</param>
    /// <exception cref="ArgumentOutOfRangeException">The step is not -1 or 1 (T-2).</exception>
    public void Turn(int step)
    {
        CheckStep(step);
        if (this.Stage != LessonStage.Slot)
        {
            return;
        }

        int count = this.state.Characters.Members.Count;
        this.Character = (this.Character + step + count) % count;
        this.Cursor = Math.Min(this.Cursor, this.Member.Slots.Count - 1);
    }

    /// <summary>Puts the cursor on the entry under the mouse pointer (D-872).</summary>
    /// <param name="entry">The entry of the list of the stage.</param>
    /// <exception cref="ArgumentOutOfRangeException">The list holds no such entry (T-2).</exception>
    public void Point(int entry)
    {
        if (entry < 0 || entry >= this.Count)
        {
            throw new ArgumentOutOfRangeException(nameof(entry), entry, $"The list of the stage {this.Stage} holds {this.Count} entries (T-2).");
        }

        this.Cursor = entry;
    }

    /// <summary>Gives the uses of a slot that the rules take now: a cast of an opened heal or cure, and a swap at a swap place (D-391, D-1030).</summary>
    /// <param name="slot">The lesson slot.</param>
    /// <returns>The uses, a cast first.</returns>
    public IReadOnlyList<SlotUse> UsesOf(int slot)
    {
        var found = new List<SlotUse>();
        if (this.Member.Slots[slot] is ContentId held && this.CastableForms(held) > 0)
        {
            found.Add(SlotUse.Cast);
        }

        if (this.state.Characters.RefusalOfSwap(this.Character, slot, null) is null || this.SwapInAllowed(slot))
        {
            found.Add(SlotUse.Swap);
        }

        return found;
    }

    /// <summary>Tells whether the rules take a swap of the chosen slot to one entry of the pack stage (D-1030).</summary>
    /// <param name="entry">The entry of the pack stage.</param>
    /// <returns>True when the swap is legal.</returns>
    public bool AllowsPackEntry(int entry) => this.state.Characters.RefusalOfSwap(this.Character, this.Slot, this.pack[entry]) is null;

    /// <summary>Tells whether the rules take a cast of one form of the chosen slot on at least one character (D-391, D-393).</summary>
    /// <param name="form">The index of the form.</param>
    /// <returns>True when the cast is legal on someone.</returns>
    public bool AllowsForm(int form)
    {
        ContentId held = this.SlotLesson ?? throw new InvalidOperationException("The form stage reads an empty slot (T-2).");
        for (int target = 0; target < this.state.Characters.Members.Count; target += 1)
        {
            if (LessonRules.RefusalOfMenuCast(this.state, this.Character, held, form, target) is null)
            {
                return true;
            }
        }

        return false;
    }

    /// <summary>Tells whether the rules take the chosen cast on one character (D-391).</summary>
    /// <param name="target">The party slot of the target.</param>
    /// <returns>True when the cast is legal.</returns>
    public bool AllowsTarget(int target)
    {
        ContentId held = this.SlotLesson ?? throw new InvalidOperationException("The target stage reads an empty slot (T-2).");
        return LessonRules.RefusalOfMenuCast(this.state, this.Character, held, this.Form, target) is null;
    }

    /// <summary>Confirms the entry under the cursor.</summary>
    /// <returns>The intent of a whole choice, or no value when the window moves to its next list or refuses the entry.</returns>
    public Intent? Confirm()
    {
        switch (this.Stage)
        {
            case LessonStage.Slot:
                this.ConfirmSlot(this.Cursor);
                return null;
            case LessonStage.Choice:
                this.OpenUse(this.uses[this.Cursor]);
                return null;
            case LessonStage.Pack:
                return this.AllowsPackEntry(this.Cursor) ? this.Done(Intent.OfLessonSwap(this.Character, this.Slot, this.pack[this.Cursor])) : null;
            case LessonStage.Form:
                if (this.AllowsForm(this.Cursor))
                {
                    this.Form = this.Cursor;
                    this.Open(LessonStage.Target, this.Character);
                }

                return null;
            default:
                return this.AllowsTarget(this.Cursor)
                    ? this.Done(Intent.OfMenuCast(this.Character, this.SlotLesson!, this.Form, this.Cursor))
                    : null;
        }
    }

    /// <summary>Goes back one list.</summary>
    /// <returns>True when the cursor stood in the slot stage, so the window closes.</returns>
    public bool Cancel()
    {
        switch (this.Stage)
        {
            case LessonStage.Slot:
                return true;
            case LessonStage.Target:
                this.Open(LessonStage.Form, this.Form);
                return false;
            case LessonStage.Form or LessonStage.Pack when this.uses.Count > 1:
                this.Open(LessonStage.Choice, this.Stage == LessonStage.Form ? 0 : 1);
                return false;
            default:
                this.Open(LessonStage.Slot, this.Slot);
                return false;
        }
    }

    private static void CheckStep(int step)
    {
        if (step != -1 && step != 1)
        {
            throw new ArgumentOutOfRangeException(nameof(step), step, "The cursor moves by one entry, -1 or 1 (T-2).");
        }
    }

    private void ConfirmSlot(int slot)
    {
        IReadOnlyList<SlotUse> found = this.UsesOf(slot);
        if (found.Count == 0)
        {
            return;
        }

        this.Slot = slot;
        this.uses.Clear();
        this.uses.AddRange(found);
        if (found.Count > 1)
        {
            this.Open(LessonStage.Choice, 0);
            return;
        }

        this.OpenUse(found[0]);
    }

    private void OpenUse(SlotUse use)
    {
        if (use == SlotUse.Swap)
        {
            this.pack.Clear();
            this.pack.Add(null);
            this.pack.AddRange(this.state.Characters.LessonPack);

            // The cursor opens on the first lesson of the pack, and on the empty entry when the pack holds none.
            this.Open(LessonStage.Pack, this.pack.Count > 1 ? 1 : 0);
            return;
        }

        ContentId held = this.SlotLesson ?? throw new InvalidOperationException("A cast reads an empty slot (T-2).");
        LessonRecord record = this.state.BattleContent.Lessons.Lesson(held);
        this.forms.Clear();
        for (int index = 0; index < record.OpenedAt(this.Member.PointsOf(held)); index += 1)
        {
            this.forms.Add(record.Forms[index]);
        }

        this.Open(LessonStage.Form, 0);
    }

    /// <summary>Gives the count of the opened forms of a lesson that act from the menu: a heal or a cure (D-391).</summary>
    private int CastableForms(ContentId held)
    {
        LessonRecord record = this.state.BattleContent.Lessons.Lesson(held);
        int castable = 0;
        for (int index = 0; index < record.OpenedAt(this.Member.PointsOf(held)); index += 1)
        {
            if (this.state.BattleContent.Abilities.Ability(record.Forms[index].Ability) is HealAbility or CureAbility)
            {
                castable += 1;
            }
        }

        return castable;
    }

    /// <summary>True when a lesson of the pack can go in the slot, which an empty slot needs for a swap (D-1030).</summary>
    private bool SwapInAllowed(int slot)
    {
        foreach (ContentId lesson in this.state.Characters.LessonPack)
        {
            if (this.state.Characters.RefusalOfSwap(this.Character, slot, lesson) is null)
            {
                return true;
            }
        }

        return false;
    }

    private void Open(LessonStage stage, int cursor)
    {
        this.Stage = stage;
        this.Cursor = cursor;
    }

    /// <summary>Puts the cursor back on the slot after a whole choice, so the window shows the change on the next frame.</summary>
    private Intent Done(Intent made)
    {
        this.Open(LessonStage.Slot, this.Slot);
        return made;
    }
}
