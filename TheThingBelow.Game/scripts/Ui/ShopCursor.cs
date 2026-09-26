using System;
using System.Collections.Generic;
using TheThingBelow.Core.Battles;
using TheThingBelow.Core.Content;
using TheThingBelow.Core.Runs;
using TheThingBelow.Core.Shops;

namespace TheThingBelow.Game.Ui;

/// <summary>The stage of the shop window (D-1158).</summary>
public enum ShopStage
{
    /// <summary>The shop menu: buy, sell, or leave.</summary>
    Mode,

    /// <summary>The list of the entries to buy, or of the things to sell.</summary>
    List,

    /// <summary>The count of a buy or a sale, which left and right set.</summary>
    Count,
}

/// <summary>One choice of the shop menu (D-1149, D-1150).</summary>
public enum ShopMode
{
    /// <summary>The list of the entries that the shop shows.</summary>
    Buy,

    /// <summary>The list of the things of the pack that the party can offer.</summary>
    Sell,

    /// <summary>The close of the window.</summary>
    Leave,
}

/// <summary>
/// The stages and the cursors of the shop window: the shop menu, the list, and the count (D-1149
/// to D-1159). The keyboard, the gamepad, and the mouse drive the one cursor (D-872).
/// </summary>
/// <remarks>
/// A confirm on a count gives the buy intent or the sale intent, and the window goes back to the
/// list, so the party can buy again. The window reads the most of each buy and each sale from the
/// rules of Core, so a count never passes what the rules take (D-1158, T-2). The rules act on the
/// next tick, and the window reads the new state on the frame after (D-493).
/// <para>
/// This type holds no Godot value, so a test reads it with no engine (D-614).
/// </para>
/// </remarks>
public sealed class ShopCursor
{
    private static readonly ShopMode[] AllModes = [ShopMode.Buy, ShopMode.Sell, ShopMode.Leave];

    private readonly RunState state;

    /// <summary>Opens the cursor on the shop menu, with the cursor on the buy.</summary>
    /// <param name="state">The state of the run, which the cursor reads and never changes.</param>
    /// <param name="shop">The shop of the open shop service.</param>
    /// <exception cref="ArgumentNullException">An argument is null (T-2).</exception>
    public ShopCursor(RunState state, ShopRecord shop)
    {
        ArgumentNullException.ThrowIfNull(state);
        ArgumentNullException.ThrowIfNull(shop);

        this.state = state;
        this.Shop = shop;
    }

    /// <summary>Every choice of the shop menu, in the order of the window.</summary>
    public static IReadOnlyList<ShopMode> Modes => AllModes;

    /// <summary>The shop.</summary>
    public ShopRecord Shop { get; }

    /// <summary>The stage of the window.</summary>
    public ShopStage Stage { get; private set; } = ShopStage.Mode;

    /// <summary>The place of the choice under the cursor of the shop menu.</summary>
    public int ModeCursor { get; private set; }

    /// <summary>The choice under the cursor of the shop menu, which the list and the count keep.</summary>
    public ShopMode Mode => AllModes[this.ModeCursor];

    /// <summary>The place of the entry under the cursor of the list.</summary>
    public int Cursor { get; private set; }

    /// <summary>The count of the buy or the sale, from 1 to <see cref="Most"/>, in the count stage.</summary>
    public int Count { get; private set; } = 1;

    /// <summary>The entries that the shop shows now, in the order of its stock (D-1152, D-1153).</summary>
    public IReadOnlyList<StockEntry> BuyEntries => ShopRules.Shown(this.state, this.Shop);

    /// <summary>The things of the pack that the party can offer now, in the order of the pack (D-1154).</summary>
    public IReadOnlyList<ContentId> SellEntries => ShopRules.Sellable(this.state);

    /// <summary>The count of the lines of the list of the mode.</summary>
    public int ListCount => this.Mode == ShopMode.Sell ? this.SellEntries.Count : this.BuyEntries.Count;

    /// <summary>The id of the thing under the cursor of the list, or no value when the list is empty.</summary>
    public ContentId? Thing
    {
        get
        {
            this.Settle();
            if (this.ListCount == 0)
            {
                return null;
            }

            return this.Mode == ShopMode.Sell ? this.SellEntries[this.Cursor] : this.BuyEntries[this.Cursor].Thing;
        }
    }

    /// <summary>The most of the thing under the cursor that the party can buy or sell now, from 0 (D-1158).</summary>
    public int Most
    {
        get
        {
            if (this.Thing is not ContentId thing)
            {
                return 0;
            }

            if (this.Mode == ShopMode.Sell)
            {
                return ShopRules.SaleOf(this.state, this.Shop, thing) == 0 ? 0 : this.state.Characters.CountOf(thing);
            }

            return ShopRules.LimitOf(this.state, this.Shop, this.BuyEntries[this.Cursor]).Most;
        }
    }

    /// <summary>Gives the string id of the reason that the thing under the cursor cannot sell or buy, or no value when it can (D-385, D-1151).</summary>
    /// <returns>The id of `menu.shop_no_room`, `menu.shop_no_gold`, or `menu.shop_not_bought`, or no value.</returns>
    public ContentId? Refusal()
    {
        if (this.Thing is not ContentId thing)
        {
            return null;
        }

        if (this.Mode == ShopMode.Sell)
        {
            return ShopRules.SaleOf(this.state, this.Shop, thing) == 0 ? Id("menu.shop_not_bought") : null;
        }

        return ShopRules.LimitOf(this.state, this.Shop, this.BuyEntries[this.Cursor]).Refusal switch
        {
            BuyRefusal.NoRoom => Id("menu.shop_no_room"),
            BuyRefusal.NoGold => Id("menu.shop_no_gold"),
            _ => null,
        };
    }

    /// <summary>Moves the cursor of the shop menu or of the list by one line, and wraps at each end. The count stage takes no move.</summary>
    /// <param name="step">-1 for up, and 1 for down.</param>
    /// <exception cref="ArgumentOutOfRangeException">The step is not -1 or 1 (T-2).</exception>
    public void Move(int step)
    {
        RequireStep(step);
        if (this.Stage == ShopStage.Mode)
        {
            this.ModeCursor = (this.ModeCursor + step + AllModes.Length) % AllModes.Length;
            return;
        }

        int count = this.ListCount;
        if (this.Stage == ShopStage.List && count > 0)
        {
            this.Cursor = (this.Settled(count) + step + count) % count;
        }
    }

    /// <summary>Changes the count by one in the count stage, from 1 to <see cref="Most"/>, with no wrap (D-1158). The other stages take no step.</summary>
    /// <param name="step">-1 for left, and 1 for right.</param>
    /// <exception cref="ArgumentOutOfRangeException">The step is not -1 or 1 (T-2).</exception>
    public void Step(int step)
    {
        RequireStep(step);
        if (this.Stage == ShopStage.Count)
        {
            this.Count = Math.Clamp(this.Count + step, 1, Math.Max(1, this.Most));
        }
    }

    /// <summary>Puts the cursor of the shop menu or of the list on the line under the mouse pointer (D-872).</summary>
    /// <param name="place">The place of the line.</param>
    /// <exception cref="ArgumentOutOfRangeException">The place is outside the lines of the stage (T-2).</exception>
    public void Point(int place)
    {
        int count = this.Stage == ShopStage.Mode ? AllModes.Length : this.ListCount;
        if (this.Stage == ShopStage.Count || place < 0 || place >= count)
        {
            throw new ArgumentOutOfRangeException(nameof(place), place, $"The stage '{this.Stage}' of the shop window holds {count} lines to point at (T-2).");
        }

        if (this.Stage == ShopStage.Mode)
        {
            this.ModeCursor = place;
            return;
        }

        this.Cursor = place;
    }

    /// <summary>True when the cursor of the shop menu stands on leave, so a confirm closes the window.</summary>
    public bool LeaveChosen => this.Stage == ShopStage.Mode && this.Mode == ShopMode.Leave;

    /// <summary>
    /// Confirms the line under the cursor. A choice of the shop menu opens its list, an entry that
    /// the party can take opens the count, and a count gives its intent and goes back to the list.
    /// </summary>
    /// <returns>The buy intent or the sale intent of a count, or no value.</returns>
    /// <exception cref="InvalidOperationException">The cursor stands on leave, which <see cref="LeaveChosen"/> reads first (T-2).</exception>
    public Intent? Confirm()
    {
        if (this.Stage == ShopStage.Mode)
        {
            if (this.Mode == ShopMode.Leave)
            {
                throw new InvalidOperationException("A confirm on leave closes the shop window, and the window reads it first (T-2).");
            }

            this.Stage = ShopStage.List;
            this.Cursor = 0;
            return null;
        }

        if (this.Thing is not ContentId thing || this.Most == 0)
        {
            return null;
        }

        if (this.Stage == ShopStage.List)
        {
            this.Stage = ShopStage.Count;
            this.Count = 1;
            return null;
        }

        int count = Math.Min(this.Count, this.Most);
        this.Stage = ShopStage.List;
        return this.Mode == ShopMode.Sell ? Intent.OfShopSell(thing, count) : Intent.OfShopBuy(thing, count);
    }

    /// <summary>Goes back one stage: from the count to the list, and from the list to the shop menu.</summary>
    /// <returns>True when the cursor stood on the shop menu, so the window closes.</returns>
    public bool Cancel()
    {
        if (this.Stage == ShopStage.Mode)
        {
            return true;
        }

        this.Stage = this.Stage == ShopStage.Count ? ShopStage.List : ShopStage.Mode;
        return false;
    }

    /// <summary>
    /// Gives the stats of a fighter with a piece in place of the piece that the fighter wears in its
    /// slot: the first empty slot of its kind, or else the first slot of its kind (D-1159).
    /// </summary>
    /// <param name="member">The fighter.</param>
    /// <param name="piece">The piece.</param>
    /// <returns>The stats with the piece.</returns>
    /// <exception cref="ArgumentNullException">An argument is null (T-2).</exception>
    public StatRow TrialOf(PartyMember member, GearRecord piece)
    {
        ArgumentNullException.ThrowIfNull(member);
        ArgumentNullException.ThrowIfNull(piece);

        int slot = SlotFor(member, piece.Slot);
        var trial = new List<ContentId?>(member.Gear);
        trial[slot] = piece.Id;
        return GearRules.StatsOf(member.Stats, trial, this.state.BattleContent.Gear);
    }

    /// <summary>Gives the first empty slot of a kind, or else the first slot of the kind.</summary>
    private static int SlotFor(PartyMember member, GearSlotKind kind)
    {
        int first = -1;
        for (int slot = 0; slot < GearRules.SlotCount; slot += 1)
        {
            if (GearRules.KindOf(slot) != kind)
            {
                continue;
            }

            if (member.Gear[slot] is null)
            {
                return slot;
            }

            first = first < 0 ? slot : first;
        }

        return first >= 0 ? first : throw new InvalidOperationException($"No gear slot takes the kind '{GearList.NameOf(kind)}' (D-44, T-2).");
    }

    private static void RequireStep(int step)
    {
        if (step != -1 && step != 1)
        {
            throw new ArgumentOutOfRangeException(nameof(step), step, "The cursor moves one line or one count at a time (T-2).");
        }
    }

    private static ContentId Id(string value) => ContentId.Parse(value, StringTable.Path, nameof(ShopCursor));

    /// <summary>Keeps the cursor of the list on a line after a buy or a sale shortens the list.</summary>
    private void Settle() => this.Cursor = this.Settled(this.ListCount);

    private int Settled(int count) => count == 0 ? 0 : Math.Min(this.Cursor, count - 1);
}
