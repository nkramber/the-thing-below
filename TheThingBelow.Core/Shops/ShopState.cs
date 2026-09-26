using System;
using System.Collections.Generic;
using TheThingBelow.Core.Content;
using TheThingBelow.Core.Hashing;
using TheThingBelow.Core.Logging;
using TheThingBelow.Core.Runs;

namespace TheThingBelow.Core.Shops;

/// <summary>The count that remains of one counted entry of a stock, which a save holds (D-1152).</summary>
/// <param name="Shop">The id of the shop.</param>
/// <param name="Thing">The id of the item or the piece of the entry.</param>
/// <param name="Left">The count that remains, from 0 to the count of the entry.</param>
public sealed record StockValues(ContentId Shop, ContentId Thing, int Left);

/// <summary>
/// The stock that remains in each shop of a run (D-1152). Only an entry that a buy changed has a
/// value here. Every other counted entry holds the count of its shop file.
/// </summary>
/// <remarks>
/// An entry that never runs out and a lesson take no value, because a buy never changes them
/// (D-1152, D-1153). The values sit in the order of the shop id and then the thing id, so the
/// snapshot and the hash never depend on the order of the buys (G-4).
/// </remarks>
public sealed class ShopState
{
    private readonly SortedDictionary<string, StockValues> left;

    private ShopState(SortedDictionary<string, StockValues> left)
    {
        this.left = left;
    }

    /// <summary>Gives the state of a new run, with each stock at the count of its shop file.</summary>
    /// <returns>The state.</returns>
    public static ShopState Start() => new(new SortedDictionary<string, StockValues>(StringComparer.Ordinal));

    /// <summary>Gives the state of a stored run, and checks each value against the shop file (T-2).</summary>
    /// <param name="shops">The shop file of this build.</param>
    /// <param name="values">The stored values, or no value on a snapshot before save format 16, which starts each stock full.</param>
    /// <param name="source">What the values came from, such as `this run`, for the error.</param>
    /// <param name="drift">The drift rule of the resume (D-1111, D-1163).</param>
    /// <returns>The state.</returns>
    /// <exception cref="ArgumentException">
    /// A value is below 0, two values name one entry, or a snapshot of this build names an absent
    /// shop, an entry that the stock lacks, an entry with no count, or a count above the count of
    /// the entry (T-2).
    /// </exception>
    /// <remarks>
    /// A save of another build takes the drift rule of D-1163: the value of an entry that the shop
    /// file no longer lists or counts leaves, and a count above the count of its entry takes that
    /// count. A log line names each change (D-1113).
    /// </remarks>
    public static ShopState Resume(ShopList shops, IReadOnlyList<StockValues>? values, string source, ResumeDrift drift)
    {
        ArgumentNullException.ThrowIfNull(shops);
        ArgumentException.ThrowIfNullOrEmpty(source);
        ArgumentNullException.ThrowIfNull(drift);

        var left = new SortedDictionary<string, StockValues>(StringComparer.Ordinal);
        foreach (StockValues value in values ?? [])
        {
            Refuse(value.Left < 0, source, $"the stock of '{value.Shop.Value}' holds {value.Left} of '{value.Thing.Value}', which is below 0");
            Refuse(left.ContainsKey(KeyOf(value.Shop, value.Thing)), source, $"the stock of '{value.Shop.Value}' holds '{value.Thing.Value}' two times");
            string? misfit = MisfitOf(shops, value, out int count);
            if (misfit is not null)
            {
                Refuse(!drift.Adjusts, source, misfit);
                drift.Note(LogSubsystems.Run, "the shop file of this build takes no stored count of an entry, and the count leaves", [new LogField("shop", value.Shop.Value), new LogField("thing", value.Thing.Value), new LogField("reason", misfit)]);
                continue;
            }

            if (value.Left > count)
            {
                Refuse(!drift.Adjusts, source, $"the stock of '{value.Shop.Value}' holds {value.Left} of '{value.Thing.Value}', above the count {count}");
                drift.Note(LogSubsystems.Run, "the shop file of this build lowered the count of an entry, and the stored count takes the new count", [new LogField("shop", value.Shop.Value), new LogField("thing", value.Thing.Value), LogField.OfNumber("stored_left", value.Left), LogField.OfNumber("count", count)]);
                left.Add(KeyOf(value.Shop, value.Thing), value with { Left = count });
                continue;
            }

            left.Add(KeyOf(value.Shop, value.Thing), value);
        }

        return new ShopState(left);
    }

    /// <summary>Gives why the shop file takes no count of a stored value, or no value when it takes one, with the count of the entry.</summary>
    private static string? MisfitOf(ShopList shops, StockValues value, out int count)
    {
        count = 0;
        if (!shops.Holds(value.Shop))
        {
            return $"the stock names the shop '{value.Shop.Value}', which the shop file lacks";
        }

        StockEntry? entry = shops.Shop(value.Shop).EntryOf(value.Thing);
        if (entry is null)
        {
            return $"the stock of '{value.Shop.Value}' names '{value.Thing.Value}', which the shop does not sell";
        }

        if (entry.Count is not int counted)
        {
            return $"the stock of '{value.Shop.Value}' holds a count of '{value.Thing.Value}', which never runs out";
        }

        count = counted;
        return null;
    }

    /// <summary>Gives the count that remains of an entry.</summary>
    /// <param name="shop">The shop.</param>
    /// <param name="entry">An entry of its stock.</param>
    /// <returns>The count, or no value for an entry that never runs out and for a lesson (D-1152, D-1153).</returns>
    public int? LeftOf(ShopRecord shop, StockEntry entry)
    {
        ArgumentNullException.ThrowIfNull(shop);
        ArgumentNullException.ThrowIfNull(entry);

        if (entry.Count is not int count)
        {
            return null;
        }

        return this.left.TryGetValue(KeyOf(shop.Id, entry.Thing), out StockValues? stored) ? stored.Left : count;
    }

    /// <summary>Lowers the count of a counted entry after a buy (D-1152).</summary>
    /// <param name="shop">The shop.</param>
    /// <param name="entry">A counted entry of its stock.</param>
    /// <param name="bought">The count of the buy, from 1 to the count that remains.</param>
    /// <param name="context">The seed, the tick, and the ids, for an error (T-2).</param>
    /// <exception cref="SimulationException">The entry has no count, or the buy passes the count that remains (T-2).</exception>
    internal void Take(ShopRecord shop, StockEntry entry, int bought, RunContext context)
    {
        int left = this.LeftOf(shop, entry)
            ?? throw new SimulationException($"a take from '{entry.Thing.Value}' of '{shop.Id.Value}', which never runs out (D-1152)", context);
        if (bought < 1 || bought > left)
        {
            throw new SimulationException($"a take of {bought} of '{entry.Thing.Value}' from '{shop.Id.Value}', which holds {left} (D-1152, T-2)", context);
        }

        this.left[KeyOf(shop.Id, entry.Thing)] = new StockValues(shop.Id, entry.Thing, left - bought);
    }

    /// <summary>Gives the values for a snapshot, in the order of the shop id and then the thing id.</summary>
    /// <returns>The values.</returns>
    public IReadOnlyList<StockValues> Values()
    {
        List<StockValues> values = [];
        foreach (StockValues value in this.left.Values)
        {
            values.Add(value);
        }

        return values;
    }

    /// <summary>Adds each value to the state hash, in the order of <see cref="Values"/> (G-5).</summary>
    /// <param name="hasher">The hasher.</param>
    public void Hash(StateHasher hasher)
    {
        ArgumentNullException.ThrowIfNull(hasher);

        hasher.AddInt32(this.left.Count);
        foreach (StockValues value in this.left.Values)
        {
            hasher.AddText(value.Shop.Value);
            hasher.AddText(value.Thing.Value);
            hasher.AddInt32(value.Left);
        }
    }

    // A tab never appears in an id, so the key of one entry never matches the key of another (D-646).
    private static string KeyOf(ContentId shop, ContentId thing) => $"{shop.Value}\t{thing.Value}";

    private static void Refuse(bool refused, string source, string reason)
    {
        if (refused)
        {
            throw Fault(source, reason);
        }
    }

    private static ArgumentException Fault(string source, string reason) =>
        new($"The snapshot of {source} is not a state of a run: {reason} (T-2, D-1152).");
}
