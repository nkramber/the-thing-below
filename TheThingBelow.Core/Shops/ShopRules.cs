using System;
using System.Collections.Generic;
using TheThingBelow.Core.Battles;
using TheThingBelow.Core.Content;
using TheThingBelow.Core.Logging;
using TheThingBelow.Core.Maps;
using TheThingBelow.Core.Runs;

namespace TheThingBelow.Core.Shops;

/// <summary>Why a buy of an entry cannot take even one (D-385, D-1158).</summary>
public enum BuyRefusal
{
    /// <summary>The party can buy at least one.</summary>
    None,

    /// <summary>The party owns as many as the stack limit allows (D-385, D-1038).</summary>
    NoRoom,

    /// <summary>The party holds less gold than the price of one.</summary>
    NoGold,
}

/// <summary>The most of one entry that the party can buy now, and the reason when that is none (D-1158).</summary>
/// <param name="Most">The most that the gold, the stock, and the stack limit allow, from 0.</param>
/// <param name="Refusal">The reason when the most is 0, or <see cref="BuyRefusal.None"/>.</param>
public sealed record BuyLimit(int Most, BuyRefusal Refusal);

/// <summary>
/// The rules of a shop: the list that it shows, a buy, and a sale (D-1149 to D-1155, D-1158).
/// </summary>
/// <remarks>
/// A buy and a sale act at the shop service that the lead faces while the menu is open, as a rest
/// does (D-1131, D-1141). The shop window asks for the most of a buy or a sale first, so a count
/// past it points at a fault in the window, and the rule refuses it (T-2).
/// </remarks>
public static class ShopRules
{
    /// <summary>Gives the shop of the shop service that the lead faces while the menu is open.</summary>
    /// <param name="state">The run.</param>
    /// <param name="context">The seed, the tick, and the intent, for an error (T-2).</param>
    /// <returns>The shop.</returns>
    /// <exception cref="SimulationException">No shop service is open (D-1141, T-2).</exception>
    public static ShopRecord OpenShop(RunState state, RunContext context)
    {
        ArgumentNullException.ThrowIfNull(state);
        ArgumentNullException.ThrowIfNull(context);

        MapService service = ServiceRules.RequireOpen(state, ServiceKind.Shop, context);
        ContentId shop = service.Shop
            ?? throw new SimulationException($"the shop service '{service.Id.Value}' names no shop, and the reader refuses such a service (D-1149, T-2)", context);
        return state.BattleContent.Shops.Shop(shop);
    }

    /// <summary>
    /// Gives the entries that a shop shows, in the order of its stock. An entry at a count of 0,
    /// a lesson that the party owns, and an entry at its stack limit stay off the list (D-1024,
    /// D-1152, D-1153, D-1166).
    /// </summary>
    /// <param name="state">The run.</param>
    /// <param name="shop">The shop.</param>
    /// <returns>The entries.</returns>
    public static IReadOnlyList<StockEntry> Shown(RunState state, ShopRecord shop)
    {
        ArgumentNullException.ThrowIfNull(state);
        ArgumentNullException.ThrowIfNull(shop);

        List<StockEntry> shown = [];
        foreach (StockEntry entry in shop.Stock)
        {
            bool soldOut = state.Shops.LeftOf(shop, entry) == 0;
            bool owned = entry.Kind == StockKind.Lesson && state.Characters.Owns(entry.Thing);
            bool full = entry.Kind != StockKind.Lesson && LimitOf(state, shop, entry).Refusal == BuyRefusal.NoRoom;
            if (!soldOut && !owned && !full)
            {
                shown.Add(entry);
            }
        }

        return shown;
    }

    /// <summary>Gives the most of one shown entry that the party can buy now, and the reason when that is none (D-385, D-1158).</summary>
    /// <param name="state">The run.</param>
    /// <param name="shop">The shop.</param>
    /// <param name="entry">An entry that the shop shows.</param>
    /// <returns>The limit. A lesson allows one at most (D-1023).</returns>
    public static BuyLimit LimitOf(RunState state, ShopRecord shop, StockEntry entry)
    {
        ArgumentNullException.ThrowIfNull(state);
        ArgumentNullException.ThrowIfNull(shop);
        ArgumentNullException.ThrowIfNull(entry);

        int room = entry.Kind == StockKind.Lesson
            ? 1
            : Math.Max(0, state.BattleContent.LimitOf(entry.Thing) - state.Characters.OwnedCount(entry.Thing));
        if (room == 0)
        {
            return new BuyLimit(0, BuyRefusal.NoRoom);
        }

        int most = Math.Min(room, state.Characters.Gold / entry.Price);
        if (state.Shops.LeftOf(shop, entry) is int left)
        {
            most = Math.Min(most, left);
        }

        return new BuyLimit(most, most == 0 ? BuyRefusal.NoGold : BuyRefusal.None);
    }

    /// <summary>
    /// Gives each id in the pack that the party can offer for a sale, in the order of the pack:
    /// each used-up item and each spare piece of gear. A key item never sells, and worn gear and a
    /// lesson never sit in the pack (D-1154).
    /// </summary>
    /// <param name="state">The run.</param>
    /// <returns>The ids.</returns>
    public static IReadOnlyList<ContentId> Sellable(RunState state)
    {
        ArgumentNullException.ThrowIfNull(state);

        List<ContentId> sellable = [];
        foreach (PackValues entry in state.Characters.Pack)
        {
            bool key = state.BattleContent.Items.Holds(entry.Id) && state.BattleContent.Items.Item(entry.Id) is KeyItem;
            if (!key)
            {
                sellable.Add(entry.Id);
            }
        }

        return sellable;
    }

    /// <summary>
    /// Gives the gold that a shop pays for one of a thing: the value of the record times the rate
    /// of the shop type for its category, rounded down, and at least 1 (D-1150, D-1151, D-1155).
    /// </summary>
    /// <param name="state">The run.</param>
    /// <param name="shop">The shop.</param>
    /// <param name="thing">A used-up item or a piece of gear.</param>
    /// <returns>The gold, or 0 when the type buys nothing of the category.</returns>
    /// <exception cref="SimulationException">The thing is a key item, which never sells (D-1154).</exception>
    public static int SaleOf(RunState state, ShopRecord shop, ContentId thing)
    {
        ArgumentNullException.ThrowIfNull(state);
        ArgumentNullException.ThrowIfNull(shop);
        ArgumentNullException.ThrowIfNull(thing);

        RunContext context = state.Context($"shop/{shop.Id.Value}/{thing.Value}");
        BattleContent content = state.BattleContent;
        (ShopCategory category, int value) = content.Gear.Holds(thing)
            ? (ShopList.CategoryOf(content.Gear.Piece(thing)), content.Gear.Piece(thing).Value)
            : content.Items.Item(thing) is UsedUpItem item
                ? (ShopList.CategoryOf(item), item.Value)
                : throw new SimulationException($"a sale of the key item '{thing.Value}', and a key item never sells (D-1154)", context);
        int rate = content.Shops.Type(shop.Type).RateOf(category);
        return rate == 0 ? 0 : Math.Max(1, BasisPoints.Apply(value, rate, context));
    }

    /// <summary>Buys a count of one shown entry at the open shop (D-1149, D-1152, D-1158).</summary>
    /// <param name="state">The run.</param>
    /// <param name="thing">The id of the item, the piece, or the lesson of the entry.</param>
    /// <param name="count">The count, from 1 to the most of <see cref="LimitOf"/>.</param>
    /// <param name="context">The seed, the tick, and the intent, for an error (T-2).</param>
    /// <param name="log">The log entries of this tick (D-179).</param>
    /// <exception cref="SimulationException">
    /// No shop is open, the shop shows no such entry, or the count is outside 1 to the most (T-2).
    /// </exception>
    public static void Buy(RunState state, ContentId thing, int count, RunContext context, List<LogEntry> log)
    {
        ArgumentNullException.ThrowIfNull(state);
        ArgumentNullException.ThrowIfNull(thing);
        ArgumentNullException.ThrowIfNull(context);
        ArgumentNullException.ThrowIfNull(log);

        ShopRecord shop = OpenShop(state, context);
        StockEntry entry = ShownEntry(state, shop, thing)
            ?? throw new SimulationException($"a buy of '{thing.Value}', and the shop '{shop.Id.Value}' shows no such entry (D-1152, D-1153)", context);
        BuyLimit limit = LimitOf(state, shop, entry);
        if (count < 1 || count > limit.Most)
        {
            throw new SimulationException($"a buy of {count} of '{thing.Value}' at '{shop.Id.Value}', and the most is {limit.Most} (D-1158, T-2)", context);
        }

        state.Characters.SpendGold(checked(entry.Price * count), context);
        if (entry.Kind == StockKind.Lesson)
        {
            state.Characters.AddLesson(state.BattleContent.Lessons.Lesson(thing), context);
        }
        else
        {
            int left = state.Characters.Pick(thing, count, state.BattleContent);
            CoreAssert.That(left == 0, $"the buy of {count} of '{thing.Value}' left {left}, past the room that the limit read (T-2)", context);
        }

        if (entry.Count is not null)
        {
            state.Shops.Take(shop, entry, count, context);
        }

        log.Add(Entry(state, "the party bought from a shop", shop, thing, count));
    }

    /// <summary>Sells a count of one thing from the pack at the open shop (D-1150, D-1154, D-1155).</summary>
    /// <param name="state">The run.</param>
    /// <param name="thing">The id of a used-up item or a piece of gear in the pack.</param>
    /// <param name="count">The count, from 1 to the count in the pack.</param>
    /// <param name="context">The seed, the tick, and the intent, for an error (T-2).</param>
    /// <param name="log">The log entries of this tick (D-179).</param>
    /// <exception cref="SimulationException">
    /// No shop is open, the thing is a key item, the shop type buys nothing of its category, or
    /// the count is outside 1 to the count in the pack (T-2).
    /// </exception>
    /// <remarks>The sold thing leaves the game, and the shop never sells it back (D-1150).</remarks>
    public static void Sell(RunState state, ContentId thing, int count, RunContext context, List<LogEntry> log)
    {
        ArgumentNullException.ThrowIfNull(state);
        ArgumentNullException.ThrowIfNull(thing);
        ArgumentNullException.ThrowIfNull(context);
        ArgumentNullException.ThrowIfNull(log);

        ShopRecord shop = OpenShop(state, context);
        int each = SaleOf(state, shop, thing);
        if (each == 0)
        {
            throw new SimulationException($"a sale of '{thing.Value}' at '{shop.Id.Value}', whose type buys nothing of its kind (D-1151)", context);
        }

        int held = state.Characters.CountOf(thing);
        if (count < 1 || count > held)
        {
            throw new SimulationException($"a sale of {count} of '{thing.Value}', and the pack holds {held} (D-1158, T-2)", context);
        }

        for (int sold = 0; sold < count; sold += 1)
        {
            state.Characters.Take(thing, context);
        }

        state.Characters.AddGold(checked(each * count), context);
        log.Add(Entry(state, "the party sold to a shop", shop, thing, count));
    }

    private static StockEntry? ShownEntry(RunState state, ShopRecord shop, ContentId thing)
    {
        foreach (StockEntry entry in Shown(state, shop))
        {
            if (string.CompareOrdinal(entry.Thing.Value, thing.Value) == 0)
            {
                return entry;
            }
        }

        return null;
    }

    private static LogEntry Entry(RunState state, string message, ShopRecord shop, ContentId thing, int count) =>
        new(
            LogLevel.Info,
            message,
            state.Tick,
            LogSubsystems.Run,
            [new LogField("shop", shop.Id.Value), new LogField("thing", thing.Value), LogField.OfNumber("count", count), LogField.OfNumber("gold", state.Characters.Gold)]);
}
