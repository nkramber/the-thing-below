using System;
using System.Collections.Generic;
using TheThingBelow.Core.Battles;
using TheThingBelow.Core.Content;
using TheThingBelow.Core.Runs;

namespace TheThingBelow.Game.Ui;

/// <summary>The list that the cursor of the item window stands in (D-1046, D-1049).</summary>
public enum ItemStage
{
    /// <summary>The items of the pack.</summary>
    Item,

    /// <summary>The characters of the party, for the target of a use.</summary>
    Target,
}

/// <summary>
/// The cursor of the item window: the items of the pack and the target of a use (D-382,
/// D-1046, D-1049). A move of the cursor makes no intent, and a whole choice makes one, so the
/// record holds the choice alone (D-493).
/// </summary>
/// <remarks>
/// The window reads each answer from the refusal of the item rules, and no rule lives here
/// (D-100). The list reads the pack on each call, so a use that spends the last copy takes the
/// item off the list. This type holds no Godot value, so a test reads it with no engine (D-614).
/// </remarks>
public sealed class ItemCursor
{
    private readonly RunState state;

    /// <summary>Opens the cursor on the first item of the pack.</summary>
    /// <param name="state">The run, which the cursor reads and never changes.</param>
    /// <exception cref="ArgumentNullException">The state is null (T-2).</exception>
    public ItemCursor(RunState state)
    {
        ArgumentNullException.ThrowIfNull(state);

        this.state = state;
    }

    /// <summary>The list that the cursor stands in.</summary>
    public ItemStage Stage { get; private set; } = ItemStage.Item;

    /// <summary>The place of the cursor in the list of the stage.</summary>
    public int Cursor { get; private set; }

    /// <summary>The item that the target stage uses, which the item stage chose.</summary>
    public ContentId? Chosen { get; private set; }

    /// <summary>The items of the pack, in the order of the pack. Spare gear stays in the gear window.</summary>
    public IReadOnlyList<PackValues> Items
    {
        get
        {
            var items = new List<PackValues>();
            foreach (PackValues entry in this.state.Characters.Pack)
            {
                if (string.CompareOrdinal(entry.Id.Kind, ItemList.Kind) == 0)
                {
                    items.Add(entry);
                }
            }

            return items;
        }
    }

    /// <summary>The count of entries of the list that the cursor stands in. The item list can be empty.</summary>
    public int Count => this.Stage == ItemStage.Item ? this.Items.Count : this.state.Characters.Members.Count;

    /// <summary>Moves the cursor by one entry, and wraps at each end. An empty list keeps the cursor.</summary>
    /// <param name="step">-1 for up, and 1 for down.</param>
    /// <exception cref="ArgumentOutOfRangeException">The step is not -1 or 1 (T-2).</exception>
    public void Move(int step)
    {
        if (step != -1 && step != 1)
        {
            throw new ArgumentOutOfRangeException(nameof(step), step, "The cursor moves by one entry, -1 or 1 (T-2).");
        }

        int count = this.Count;
        if (count > 0)
        {
            this.Cursor = (this.Cursor + step + count) % count;
        }
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

    /// <summary>Keeps the cursor inside the item list after a use spent the last copy of an item. The window calls it before each frame.</summary>
    public void Settle()
    {
        if (this.Stage == ItemStage.Item && this.Cursor >= this.Items.Count)
        {
            this.Cursor = Math.Max(0, this.Items.Count - 1);
        }
    }

    /// <summary>Tells whether the rules take a use of one item of the list on at least one character (D-1049).</summary>
    /// <param name="entry">The entry of the item list.</param>
    /// <returns>True when the use is legal on someone.</returns>
    public bool AllowsItem(int entry)
    {
        ContentId item = this.Items[entry].Id;
        for (int target = 0; target < this.state.Characters.Members.Count; target += 1)
        {
            if (ItemRules.RefusalOfMenuUse(this.state, item, target) is null)
            {
                return true;
            }
        }

        return false;
    }

    /// <summary>Tells whether the rules take the chosen item on one character (D-1049).</summary>
    /// <param name="target">The party slot of the target.</param>
    /// <returns>True when the use is legal.</returns>
    public bool AllowsTarget(int target)
    {
        ContentId item = this.Chosen ?? throw new InvalidOperationException("The target stage reads no chosen item (T-2).");
        return ItemRules.RefusalOfMenuUse(this.state, item, target) is null;
    }

    /// <summary>Confirms the entry under the cursor.</summary>
    /// <returns>The intent of a whole choice, or no value when the window moves to its next list or refuses the entry.</returns>
    public Intent? Confirm()
    {
        if (this.Stage == ItemStage.Item)
        {
            if (this.Cursor < this.Items.Count && this.AllowsItem(this.Cursor))
            {
                this.Chosen = this.Items[this.Cursor].Id;
                this.Stage = ItemStage.Target;
                this.Cursor = 0;
            }

            return null;
        }

        if (!this.AllowsTarget(this.Cursor))
        {
            return null;
        }

        Intent made = Intent.OfMenuItem(this.Chosen!, this.Cursor);
        this.Back();
        return made;
    }

    /// <summary>Goes back one list.</summary>
    /// <returns>True when the cursor stood in the item list, so the window closes.</returns>
    public bool Cancel()
    {
        if (this.Stage == ItemStage.Item)
        {
            return true;
        }

        this.Back();
        return false;
    }

    /// <summary>Puts the cursor back on the chosen item in the item list, or on the first item when the use spent the last copy.</summary>
    private void Back()
    {
        ContentId chosen = this.Chosen ?? throw new InvalidOperationException("The target stage reads no chosen item (T-2).");
        this.Stage = ItemStage.Item;
        this.Cursor = 0;
        IReadOnlyList<PackValues> items = this.Items;
        for (int entry = 0; entry < items.Count; entry += 1)
        {
            if (string.CompareOrdinal(items[entry].Id.Value, chosen.Value) == 0)
            {
                this.Cursor = entry;
            }
        }
    }
}
