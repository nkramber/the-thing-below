using System;
using System.Collections.Generic;
using TheThingBelow.Core.Battles;
using TheThingBelow.Core.Content;
using TheThingBelow.Core.Runs;

namespace TheThingBelow.Game.Ui;

/// <summary>The list that the cursor of the item window stands in (D-1046, D-1049, D-1219).</summary>
public enum ItemStage
{
    /// <summary>The items of the pack that a use spends, and the entry of the key items.</summary>
    Item,

    /// <summary>The key items, such as the torch, and the entry of the Keyring (D-1219).</summary>
    KeyItems,

    /// <summary>The keys on the Keyring (D-1219).</summary>
    Keyring,

    /// <summary>The characters of the party, for the target of a use.</summary>
    Target,
}

/// <summary>What one line of a list of the item window holds (D-1219).</summary>
public enum ItemLineKind
{
    /// <summary>An item of the pack, or a key item.</summary>
    Item,

    /// <summary>The entry that opens the list of the key items.</summary>
    KeyItems,

    /// <summary>The entry that opens the list of the keys.</summary>
    Keyring,
}

/// <summary>One line of a list of the item window: an item of the pack, or an entry that opens a list (D-1219).</summary>
/// <param name="Kind">What the line holds.</param>
/// <param name="Entry">The item and its count, for a line of the kind item, and no value for an entry that opens a list.</param>
public sealed record ItemLine(ItemLineKind Kind, PackValues? Entry);

/// <summary>
/// The cursor of the item window: the items of the pack, the key items, the keys of the Keyring,
/// and the target of a use (D-382, D-1046, D-1049, D-1219). A move of the cursor makes no intent,
/// and a whole choice makes one, so the record holds the choice alone (D-493).
/// </summary>
/// <remarks>
/// The first list holds each item that a use spends, then one entry of the key items when the
/// pack holds one. That entry opens the key items, and the Keyring entry there opens the keys
/// (D-1219). The window reads each answer from the refusal of the item rules, and no rule lives
/// here (D-100). The list reads the pack on each call, so a use that spends the last copy takes
/// the item off the list. This type holds no Godot value, so a test reads it with no engine (D-614).
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

    /// <summary>The items of the pack that a use spends, in the order of the pack. Spare gear stays in the gear window, and a key item sits in its own list.</summary>
    public IReadOnlyList<PackValues> Items => this.PackItems(key: false, ring: false);

    /// <summary>The lines of the list of the stage, or no line on the target stage (D-1219).</summary>
    public IReadOnlyList<ItemLine> Lines => this.Stage switch
    {
        ItemStage.Item => LinesOf(this.Items, this.PackItems(key: true, ring: false).Count + this.PackItems(key: true, ring: true).Count > 0 ? ItemLineKind.KeyItems : null),
        ItemStage.KeyItems => LinesOf(this.PackItems(key: true, ring: false), this.PackItems(key: true, ring: true).Count > 0 ? ItemLineKind.Keyring : null),
        ItemStage.Keyring => LinesOf(this.PackItems(key: true, ring: true), null),
        ItemStage.Target => [],
        _ => throw new InvalidOperationException($"The item window holds no stage {this.Stage} (T-2)."),
    };

    /// <summary>The count of entries of the list that the cursor stands in. The item list can be empty.</summary>
    public int Count => this.Stage == ItemStage.Target ? this.state.Characters.Members.Count : this.Lines.Count;

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

    /// <summary>Keeps the cursor inside the list after a use spent the last copy of an item. The window calls it before each frame.</summary>
    public void Settle()
    {
        if (this.Stage != ItemStage.Target && this.Cursor >= this.Lines.Count)
        {
            this.Cursor = Math.Max(0, this.Lines.Count - 1);
        }
    }

    /// <summary>
    /// Tells whether a confirm on one line of the list does something: a use of an item that the
    /// rules take on at least one character, or an entry that opens a list (D-1049, D-1219).
    /// </summary>
    /// <param name="entry">The entry of the list of the stage.</param>
    /// <returns>True when the use is legal on someone, or the line opens a list. A key item takes no use.</returns>
    public bool AllowsItem(int entry)
    {
        ItemLine line = this.Lines[entry];
        if (line.Kind != ItemLineKind.Item)
        {
            return true;
        }

        ContentId item = line.Entry!.Id;
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
    /// <returns>The intent of a whole choice, or no value when the window moves to another list or refuses the entry.</returns>
    public Intent? Confirm()
    {
        if (this.Stage == ItemStage.Target)
        {
            if (!this.AllowsTarget(this.Cursor))
            {
                return null;
            }

            Intent made = Intent.OfMenuItem(this.Chosen!, this.Cursor);
            this.Back();
            return made;
        }

        if (this.Cursor >= this.Lines.Count || !this.AllowsItem(this.Cursor))
        {
            return null;
        }

        ItemLine line = this.Lines[this.Cursor];
        switch (line.Kind)
        {
            case ItemLineKind.KeyItems:
                this.Open(ItemStage.KeyItems);
                break;
            case ItemLineKind.Keyring:
                this.Open(ItemStage.Keyring);
                break;
            case ItemLineKind.Item:
                this.Chosen = line.Entry!.Id;
                this.Open(ItemStage.Target);
                break;
            default:
                throw new InvalidOperationException($"The item window holds no line of the kind {line.Kind} (T-2).");
        }

        return null;
    }

    /// <summary>Goes back one list.</summary>
    /// <returns>True when the cursor stood in the item list, so the window closes.</returns>
    public bool Cancel()
    {
        switch (this.Stage)
        {
            case ItemStage.Item:
                return true;
            case ItemStage.KeyItems:
                this.Open(ItemStage.Item);
                this.Cursor = this.Lines.Count - 1;
                return false;
            case ItemStage.Keyring:
                this.Open(ItemStage.KeyItems);
                this.Cursor = this.Lines.Count - 1;
                return false;
            case ItemStage.Target:
                this.Back();
                return false;
            default:
                throw new InvalidOperationException($"The item window holds no stage {this.Stage} (T-2).");
        }
    }

    private void Open(ItemStage stage)
    {
        this.Stage = stage;
        this.Cursor = 0;
    }

    /// <summary>Gives the lines of one list of items, with the entry that opens another list at the end, or no such entry.</summary>
    private static List<ItemLine> LinesOf(IReadOnlyList<PackValues> items, ItemLineKind? entry)
    {
        var lines = new List<ItemLine>();
        foreach (PackValues item in items)
        {
            lines.Add(new ItemLine(ItemLineKind.Item, item));
        }

        if (entry is ItemLineKind opens)
        {
            lines.Add(new ItemLine(opens, null));
        }

        return lines;
    }

    /// <summary>Gives the items of the pack of one sort: the used-up items, the key items off the Keyring, or the keys on it (D-1219).</summary>
    private List<PackValues> PackItems(bool key, bool ring)
    {
        var items = new List<PackValues>();
        foreach (PackValues entry in this.state.Characters.Pack)
        {
            if (string.CompareOrdinal(entry.Id.Kind, ItemList.Kind) != 0)
            {
                continue;
            }

            ItemRecord record = this.state.BattleContent.Item(entry.Id);
            bool isKey = record is KeyItem;
            if (isKey == key && (!isKey || ((KeyItem)record).OnRing == ring))
            {
                items.Add(entry);
            }
        }

        return items;
    }

    /// <summary>Puts the cursor back on the chosen item in the item list, or on the first item when the use spent the last copy.</summary>
    private void Back()
    {
        ContentId chosen = this.Chosen ?? throw new InvalidOperationException("The target stage reads no chosen item (T-2).");
        this.Open(ItemStage.Item);
        IReadOnlyList<ItemLine> lines = this.Lines;
        for (int entry = 0; entry < lines.Count; entry += 1)
        {
            if (lines[entry].Entry is PackValues item && string.CompareOrdinal(item.Id.Value, chosen.Value) == 0)
            {
                this.Cursor = entry;
            }
        }
    }
}
