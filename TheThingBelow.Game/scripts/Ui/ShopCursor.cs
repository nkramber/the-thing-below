using System;
using System.Collections.Generic;
using TheThingBelow.Core.Battles;
using TheThingBelow.Core.Content;
using TheThingBelow.Core.Runs;
using TheThingBelow.Core.Shops;

namespace TheThingBelow.Game.Ui;

/// <summary>The stage of the shop window (D-1158, D-1167).</summary>
public enum ShopStage
{
    /// <summary>The shop menu: buy, sell, or leave.</summary>
    Mode,

    /// <summary>The list of the entries to buy, or of the things to sell.</summary>
    List,

    /// <summary>The count of a buy or a sale, which left and right set.</summary>
    Count,

    /// <summary>The popup after a buy of gear: equip it now, yes or no.</summary>
    EquipAsk,

    /// <summary>The list of the characters of the party, one of whom wears the piece.</summary>
    EquipWho,

    /// <summary>The two accessory slots of the chosen character, when both hold a piece.</summary>
    EquipSlot,
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
/// The stages and the cursors of the shop window: the shop menu, the list, the count, and the
/// equip step after a buy of gear (D-1149 to D-1158, D-1165 to D-1167). The keyboard, the gamepad,
/// and the mouse drive the one cursor (D-872).
/// </summary>
/// <remarks>
/// A confirm on a count gives the buy intent or the sale intent. After a buy of gear, a popup asks
/// to equip each copy: no leaves the copy in the pack, and yes asks which character wears it, and
/// for two full accessory slots which slot (D-1167). The window reads the most of each buy and each
/// sale from the rules of Core, so a count never passes what the rules take (T-2). The rules act on
/// the next tick, and the window reads the new state on the frame after (D-493).
/// <para>
/// This type holds no Godot value, so a test reads it with no engine (D-614).
/// </para>
/// </remarks>
public sealed class ShopCursor
{
    /// <summary>The place of yes in the popup of the equip step.</summary>
    public const int Yes = 0;

    /// <summary>The place of no in the popup of the equip step.</summary>
    public const int No = 1;

    private static readonly ShopMode[] AllModes = [ShopMode.Buy, ShopMode.Sell, ShopMode.Leave];

    private readonly RunState state;
    private int copiesLeft;

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

    /// <summary>The place of yes or no under the cursor of the popup of the equip step.</summary>
    public int AskCursor { get; private set; }

    /// <summary>The party slot of the character under the cursor of the equip step.</summary>
    public int WhoCursor { get; private set; }

    /// <summary>The place of the slot under the cursor, when both accessory slots hold a piece.</summary>
    public int SlotCursor { get; private set; }

    /// <summary>The piece that the equip step offers, or no value outside it.</summary>
    public ContentId? EquipPiece { get; private set; }

    /// <summary>True when the cursor of the shop menu stands on leave, so a confirm closes the window.</summary>
    public bool LeaveChosen => this.Stage == ShopStage.Mode && this.Mode == ShopMode.Leave;

    /// <summary>The entries that the shop shows now, in the order of its stock (D-1152, D-1153, D-1166).</summary>
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

    /// <summary>
    /// Gives the string id of the reason that the thing under the cursor of the sale list cannot
    /// sell, or no value. A buy that the gold cannot pay gives no reason (D-1151, D-1166).
    /// </summary>
    /// <returns>The id of `menu.shop_not_bought`, or no value.</returns>
    public ContentId? Refusal()
    {
        if (this.Mode != ShopMode.Sell || this.Thing is not ContentId thing)
        {
            return null;
        }

        return ShopRules.SaleOf(this.state, this.Shop, thing) == 0 ? Id("menu.shop_not_bought") : null;
    }

    /// <summary>
    /// Gives the gear slots of a character that the piece of the equip step can take: the one slot
    /// of its kind, the first empty accessory slot, or both accessory slots when both hold a piece
    /// (D-44, D-1167).
    /// </summary>
    /// <param name="member">The character.</param>
    /// <returns>The slots, in the order of the slots.</returns>
    /// <exception cref="InvalidOperationException">The cursor stands outside the equip step (T-2).</exception>
    public IReadOnlyList<int> SlotsFor(PartyMember member)
    {
        ArgumentNullException.ThrowIfNull(member);

        GearSlotKind kind = this.PieceRecord().Slot;
        List<int> slots = [];
        for (int slot = 0; slot < GearRules.SlotCount; slot += 1)
        {
            if (GearRules.KindOf(slot) != kind)
            {
                continue;
            }

            if (member.Gear[slot] is null)
            {
                return [slot];
            }

            slots.Add(slot);
        }

        return slots;
    }

    /// <summary>Gives the stats of a character with the piece of the equip step in one slot (D-1167).</summary>
    /// <param name="member">The character.</param>
    /// <param name="slot">The gear slot that takes the piece.</param>
    /// <returns>The stats with the piece.</returns>
    /// <exception cref="InvalidOperationException">The cursor stands outside the equip step (T-2).</exception>
    public StatRow TrialOf(PartyMember member, int slot)
    {
        ArgumentNullException.ThrowIfNull(member);

        var trial = new List<ContentId?>(member.Gear);
        trial[slot] = this.PieceRecord().Id;
        return GearRules.StatsOf(member.Stats, trial, this.state.BattleContent.Gear);
    }

    /// <summary>Moves the cursor of the stage by one line, and wraps at each end. The count stage takes no move.</summary>
    /// <param name="step">-1 for up, and 1 for down.</param>
    /// <exception cref="ArgumentOutOfRangeException">The step is not -1 or 1 (T-2).</exception>
    public void Move(int step)
    {
        RequireStep(step);
        switch (this.Stage)
        {
            case ShopStage.Mode:
                this.ModeCursor = Wrap(this.ModeCursor, step, AllModes.Length);
                break;
            case ShopStage.List when this.ListCount > 0:
                this.Cursor = Wrap(this.Settled(this.ListCount), step, this.ListCount);
                break;
            case ShopStage.EquipAsk:
                this.AskCursor = Wrap(this.AskCursor, step, 2);
                break;
            case ShopStage.EquipWho:
                this.WhoCursor = Wrap(this.WhoCursor, step, this.state.Characters.Members.Count);
                break;
            case ShopStage.EquipSlot:
                this.SlotCursor = Wrap(this.SlotCursor, step, this.SlotsFor(this.Who()).Count);
                break;
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

    /// <summary>Puts the cursor of the stage on the line under the mouse pointer (D-872). The count stage takes no point.</summary>
    /// <param name="place">The place of the line.</param>
    /// <exception cref="ArgumentOutOfRangeException">The place is outside the lines of the stage (T-2).</exception>
    public void Point(int place)
    {
        int count = this.Stage switch
        {
            ShopStage.Mode => AllModes.Length,
            ShopStage.List => this.ListCount,
            ShopStage.EquipAsk => 2,
            ShopStage.EquipWho => this.state.Characters.Members.Count,
            ShopStage.EquipSlot => this.SlotsFor(this.Who()).Count,
            _ => 0,
        };
        if (place < 0 || place >= count)
        {
            throw new ArgumentOutOfRangeException(nameof(place), place, $"The stage '{this.Stage}' of the shop window holds {count} lines to point at (T-2).");
        }

        switch (this.Stage)
        {
            case ShopStage.Mode:
                this.ModeCursor = place;
                break;
            case ShopStage.List:
                this.Cursor = place;
                break;
            case ShopStage.EquipAsk:
                this.AskCursor = place;
                break;
            case ShopStage.EquipWho:
                this.WhoCursor = place;
                break;
            default:
                this.SlotCursor = place;
                break;
        }
    }

    /// <summary>
    /// Confirms the line under the cursor. A choice of the shop menu opens its list, an entry that
    /// the party can take opens the count, and a count gives its intent. After a buy of gear, the
    /// equip step asks for each copy, and a choice of a character and a slot gives the wear intent.
    /// </summary>
    /// <returns>The buy intent, the sale intent, or the wear intent, or no value.</returns>
    /// <exception cref="InvalidOperationException">The cursor stands on leave, which <see cref="LeaveChosen"/> reads first (T-2).</exception>
    public Intent? Confirm()
    {
        switch (this.Stage)
        {
            case ShopStage.Mode:
                if (this.Mode == ShopMode.Leave)
                {
                    throw new InvalidOperationException("A confirm on leave closes the shop window, and the window reads it first (T-2).");
                }

                this.Stage = ShopStage.List;
                this.Cursor = 0;
                return null;
            case ShopStage.List:
                if (this.Thing is not null && this.Most > 0)
                {
                    this.Stage = ShopStage.Count;
                    this.Count = 1;
                }

                return null;
            case ShopStage.Count:
                return this.Trade();
            case ShopStage.EquipAsk:
                if (this.AskCursor == No)
                {
                    this.NextCopy();
                    return null;
                }

                this.Stage = ShopStage.EquipWho;
                this.WhoCursor = 0;
                return null;
            case ShopStage.EquipWho:
                IReadOnlyList<int> slots = this.SlotsFor(this.Who());
                if (slots.Count > 1)
                {
                    this.Stage = ShopStage.EquipSlot;
                    this.SlotCursor = 0;
                    return null;
                }

                return this.Wear(slots[0]);
            default:
                return this.Wear(this.SlotsFor(this.Who())[this.SlotCursor]);
        }
    }

    /// <summary>Goes back one stage. Back on the popup of the equip step leaves the copy in the pack, as no does (D-1167).</summary>
    /// <returns>True when the cursor stood on the shop menu, so the window closes.</returns>
    public bool Cancel()
    {
        switch (this.Stage)
        {
            case ShopStage.Mode:
                return true;
            case ShopStage.Count:
                this.Stage = ShopStage.List;
                break;
            case ShopStage.List:
                this.Stage = ShopStage.Mode;
                break;
            case ShopStage.EquipAsk:
                this.NextCopy();
                break;
            case ShopStage.EquipWho:
                this.Stage = ShopStage.EquipAsk;
                break;
            default:
                this.Stage = ShopStage.EquipWho;
                break;
        }

        return false;
    }

    private static void RequireStep(int step)
    {
        if (step != -1 && step != 1)
        {
            throw new ArgumentOutOfRangeException(nameof(step), step, "The cursor moves one line or one count at a time (T-2).");
        }
    }

    private static int Wrap(int place, int step, int count) => count == 0 ? 0 : (place + step + count) % count;

    private static ContentId Id(string value) => ContentId.Parse(value, StringTable.Path, nameof(ShopCursor));

    /// <summary>Gives the buy intent or the sale intent of the count, and opens the equip step after a buy of gear (D-1167).</summary>
    private Intent? Trade()
    {
        if (this.Thing is not ContentId thing || this.Most == 0)
        {
            return null;
        }

        int count = Math.Min(this.Count, this.Most);
        if (this.Mode == ShopMode.Sell)
        {
            this.Stage = ShopStage.List;
            return Intent.OfShopSell(thing, count);
        }

        if (this.state.BattleContent.Gear.Holds(thing))
        {
            this.EquipPiece = thing;
            this.copiesLeft = count;
            this.Stage = ShopStage.EquipAsk;
            this.AskCursor = Yes;
        }
        else
        {
            this.Stage = ShopStage.List;
        }

        return Intent.OfShopBuy(thing, count);
    }

    /// <summary>
    /// Gives the wear intent of the copy on the chosen character, when the pack holds the copy and
    /// the rules take the change, and then asks for the next copy (D-1048, D-1167).
    /// </summary>
    private Intent? Wear(int slot)
    {
        ContentId piece = this.PieceRecord().Id;
        bool held = this.state.Characters.CountOf(piece) > 0;
        if (!held || this.state.Characters.RefusalOfWear(this.WhoCursor, slot, piece, this.state.BattleContent) is not null)
        {
            return null;
        }

        this.NextCopy();
        return Intent.OfGearWear(this.WhoCursor, slot, piece);
    }

    /// <summary>Asks for the next copy of the buy, or goes back to the list after the last one (D-1167).</summary>
    private void NextCopy()
    {
        this.copiesLeft -= 1;
        this.AskCursor = Yes;
        if (this.copiesLeft > 0)
        {
            this.Stage = ShopStage.EquipAsk;
            return;
        }

        this.EquipPiece = null;
        this.Stage = ShopStage.List;
    }

    private PartyMember Who() => this.state.Characters.Members[this.WhoCursor];

    private GearRecord PieceRecord()
    {
        ContentId piece = this.EquipPiece ?? throw new InvalidOperationException($"The shop window reads the piece of the equip step in the stage '{this.Stage}', which offers none (T-2).");
        return this.state.BattleContent.Gear.Piece(piece);
    }

    /// <summary>Keeps the cursor of the list on a line after a buy or a sale shortens the list.</summary>
    private void Settle() => this.Cursor = this.Settled(this.ListCount);

    private int Settled(int count) => count == 0 ? 0 : Math.Min(this.Cursor, count - 1);
}
