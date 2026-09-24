using System;
using System.Collections.Generic;
using TheThingBelow.Core.Battles;
using TheThingBelow.Core.Content;
using TheThingBelow.Core.Runs;

namespace TheThingBelow.Game.Ui;

/// <summary>The list that the cursor of the gear window stands in (D-44, D-1048).</summary>
public enum GearStage
{
    /// <summary>The six gear slots of one character.</summary>
    Slot,

    /// <summary>An entry that empties the slot, then each piece of the pack that fits the slot.</summary>
    Pack,
}

/// <summary>
/// The cursor of the gear window: the character, the gear slot, and the pieces of the pack that
/// fit it (D-44, D-1048). A move of the cursor makes no intent, and a whole choice makes one, so
/// the record holds the choice alone (D-493).
/// </summary>
/// <remarks>
/// The window reads each answer from the refusal of the party state, and no rule lives here
/// (D-100). This type holds no Godot value, so a test reads it with no engine (D-614).
/// </remarks>
public sealed class GearCursor
{
    private readonly RunState state;
    private readonly List<ContentId?> pack = [];

    /// <summary>Opens the cursor on the weapon slot of the first character.</summary>
    /// <param name="state">The run, which the cursor reads and never changes.</param>
    /// <exception cref="ArgumentNullException">The state is null (T-2).</exception>
    public GearCursor(RunState state)
    {
        ArgumentNullException.ThrowIfNull(state);

        this.state = state;
    }

    /// <summary>The party slot of the character that the window shows.</summary>
    public int Character { get; private set; }

    /// <summary>The list that the cursor stands in.</summary>
    public GearStage Stage { get; private set; } = GearStage.Slot;

    /// <summary>The place of the cursor in the list of the stage.</summary>
    public int Cursor { get; private set; }

    /// <summary>The gear slot that the pack stage fills, which the slot stage chose.</summary>
    public int Slot { get; private set; }

    /// <summary>The character whose gear the window shows.</summary>
    public PartyMember Member => this.state.Characters.Members[this.Character];

    /// <summary>The entries of the pack stage: no value to empty the slot, then each piece of the pack of the kind of the slot.</summary>
    public IReadOnlyList<ContentId?> PackEntries => this.pack;

    /// <summary>The count of entries of the list that the cursor stands in.</summary>
    public int Count => this.Stage == GearStage.Slot ? GearRules.SlotCount : this.pack.Count;

    /// <summary>Moves the cursor by one entry, and wraps at each end.</summary>
    /// <param name="step">-1 for up, and 1 for down.</param>
    /// <exception cref="ArgumentOutOfRangeException">The step is not -1 or 1 (T-2).</exception>
    public void Move(int step)
    {
        CheckStep(step);
        this.Cursor = (this.Cursor + step + this.Count) % this.Count;
    }

    /// <summary>Shows the next or the last character, in the slot stage alone. The cursor keeps its slot.</summary>
    /// <param name="step">-1 for the character before, and 1 for the character after.</param>
    /// <exception cref="ArgumentOutOfRangeException">The step is not -1 or 1 (T-2).</exception>
    public void Turn(int step)
    {
        CheckStep(step);
        if (this.Stage != GearStage.Slot)
        {
            return;
        }

        int count = this.state.Characters.Members.Count;
        this.Character = (this.Character + step + count) % count;
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

    /// <summary>Tells whether a gear slot takes a change now: a piece of the pack of its kind, or an empty of a filled slot (D-44).</summary>
    /// <param name="slot">The gear slot.</param>
    /// <returns>True when the pack stage of the slot holds a legal entry.</returns>
    public bool AllowsSlot(int slot)
    {
        foreach (ContentId? entry in this.EntriesOf(slot))
        {
            if (this.state.Characters.RefusalOfWear(this.Character, slot, entry, this.state.BattleContent) is null)
            {
                return true;
            }
        }

        return false;
    }

    /// <summary>
    /// Gives the stats of the character with the entry under the cursor in the chosen slot, for
    /// the line that compares it with the worn gear. In the slot stage, and on the piece that the
    /// slot holds, the stats equal the worn stats (D-1036, D-1052).
    /// </summary>
    /// <returns>The stats with the trial piece, or with the slot empty for the entry that empties it.</returns>
    public StatRow TrialStats()
    {
        PartyMember member = this.Member;
        GearList gear = this.state.BattleContent.Gear;
        if (this.Stage == GearStage.Slot)
        {
            return member.StatsWith(gear);
        }

        var trial = new List<ContentId?>(member.Gear);
        trial[this.Slot] = this.pack[this.Cursor];
        return GearRules.StatsOf(member.Stats, trial, gear);
    }

    /// <summary>Tells whether the rules take one entry of the pack stage in the chosen slot (D-1048).</summary>
    /// <param name="entry">The entry of the pack stage.</param>
    /// <returns>True when the change is legal.</returns>
    public bool AllowsPackEntry(int entry) =>
        this.state.Characters.RefusalOfWear(this.Character, this.Slot, this.pack[entry], this.state.BattleContent) is null;

    /// <summary>Confirms the entry under the cursor.</summary>
    /// <returns>The intent of a whole choice, or no value when the window moves to its next list or refuses the entry.</returns>
    public Intent? Confirm()
    {
        if (this.Stage == GearStage.Slot)
        {
            if (this.AllowsSlot(this.Cursor))
            {
                this.Slot = this.Cursor;
                this.pack.Clear();
                this.pack.AddRange(this.EntriesOf(this.Slot));

                // The cursor opens on the first piece, and on the empty entry when no piece fits.
                this.Open(GearStage.Pack, this.pack.Count > 1 ? 1 : 0);
            }

            return null;
        }

        if (!this.AllowsPackEntry(this.Cursor))
        {
            return null;
        }

        Intent made = Intent.OfGearWear(this.Character, this.Slot, this.pack[this.Cursor]);
        this.Open(GearStage.Slot, this.Slot);
        return made;
    }

    /// <summary>Goes back one list.</summary>
    /// <returns>True when the cursor stood in the slot stage, so the window closes.</returns>
    public bool Cancel()
    {
        if (this.Stage == GearStage.Slot)
        {
            return true;
        }

        this.Open(GearStage.Slot, this.Slot);
        return false;
    }

    private static void CheckStep(int step)
    {
        if (step != -1 && step != 1)
        {
            throw new ArgumentOutOfRangeException(nameof(step), step, "The cursor moves by one entry, -1 or 1 (T-2).");
        }
    }

    /// <summary>Gives the entries of the pack stage of one slot: the empty entry, then each piece of the pack of its kind, in the order of the pack.</summary>
    private List<ContentId?> EntriesOf(int slot)
    {
        var entries = new List<ContentId?> { null };
        GearSlotKind kind = GearRules.KindOf(slot);
        foreach (PackValues entry in this.state.Characters.Pack)
        {
            if (string.CompareOrdinal(entry.Id.Kind, GearList.Kind) == 0 && this.state.BattleContent.Piece(entry.Id).Slot == kind)
            {
                entries.Add(entry.Id);
            }
        }

        return entries;
    }

    private void Open(GearStage stage, int cursor)
    {
        this.Stage = stage;
        this.Cursor = cursor;
    }
}
