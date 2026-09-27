using System;
using System.Collections.Generic;
using TheThingBelow.Core.Battles;
using TheThingBelow.Core.Content;

namespace TheThingBelow.Core.Shops;

/// <summary>A category that a shop type buys at a rate of its own (D-1151).</summary>
public enum ShopCategory
{
    /// <summary>A weapon.</summary>
    Weapon,

    /// <summary>A shield or another piece for the off-hand.</summary>
    OffHand,

    /// <summary>A piece for the head.</summary>
    Head,

    /// <summary>A piece for the body.</summary>
    Body,

    /// <summary>An accessory.</summary>
    Accessory,

    /// <summary>An item that restores health.</summary>
    Heal,

    /// <summary>An item that restores AP.</summary>
    Restore,

    /// <summary>An item that ends statuses.</summary>
    Cure,

    /// <summary>An item that stands a fallen ally up.</summary>
    Revive,
}

/// <summary>What one entry of a stock sells (D-1149).</summary>
public enum StockKind
{
    /// <summary>An item of the item file.</summary>
    Item,

    /// <summary>A piece of the gear file.</summary>
    Gear,

    /// <summary>A lesson of the lesson file (D-365).</summary>
    Lesson,
}

/// <summary>One shop type: a sale rate for each category (D-1151).</summary>
/// <param name="Id">The id, of the kind `shop_type`.</param>
/// <param name="Rates">The rate of each category in basis points, in the order of <see cref="ShopCategory"/>. A rate of 0 buys nothing of the category.</param>
public sealed record ShopType(ContentId Id, IReadOnlyList<int> Rates)
{
    /// <summary>Gives the rate of a category, in basis points.</summary>
    /// <param name="category">The category.</param>
    /// <returns>The rate, from 0 to 10000. A rate of 0 buys nothing of the category (D-1151).</returns>
    public int RateOf(ShopCategory category) => this.Rates[(int)category];
}

/// <summary>One entry of the stock of a shop (D-1149, D-1152, D-1153).</summary>
/// <param name="Kind">What the entry sells.</param>
/// <param name="Thing">The id of the item, the piece, or the lesson.</param>
/// <param name="Price">The price of one, in gold, from 1 (D-1149).</param>
/// <param name="Count">The count at the start of a run, from 1. No value for an entry that never runs out, and for a lesson, which the owned check limits (D-1152, D-1153).</param>
public sealed record StockEntry(StockKind Kind, ContentId Thing, int Price, int? Count);

/// <summary>One shop: its type and its stock (D-1149, D-1151).</summary>
/// <param name="Id">The id, of the kind `shop`.</param>
/// <param name="Type">The id of its shop type.</param>
/// <param name="Stock">The entries, in the order of the file, which is the order of the list on screen.</param>
public sealed record ShopRecord(ContentId Id, ContentId Type, IReadOnlyList<StockEntry> Stock)
{
    /// <summary>Finds the entry that sells a thing.</summary>
    /// <param name="thing">The id of the item, the piece, or the lesson.</param>
    /// <returns>The entry, or no value when the stock holds no such thing.</returns>
    public StockEntry? EntryOf(ContentId thing)
    {
        ArgumentNullException.ThrowIfNull(thing);

        foreach (StockEntry entry in this.Stock)
        {
            if (string.CompareOrdinal(entry.Thing.Value, thing.Value) == 0)
            {
                return entry;
            }
        }

        return null;
    }
}

/// <summary>
/// The shop file: each shop type and each shop (D-1149 to D-1155). The file is
/// `content/rules/shops.json`. A shop service of a hub names a shop of this file.
/// </summary>
/// <remarks>
/// The load of the content set checks each id of a stock against the item, gear, and lesson
/// files, and each shop service against this file (T-2).
/// </remarks>
public sealed class ShopList
{
    /// <summary>The path of the file under the content folder.</summary>
    public const string Path = "rules/shops.json";

    /// <summary>The kind of a shop id.</summary>
    public const string Kind = "shop";

    /// <summary>The kind of a shop type id.</summary>
    public const string TypeKind = "shop_type";

    /// <summary>The word of a stock count that never runs out (D-1152).</summary>
    public const string UnlimitedName = "unlimited";

    /// <summary>The highest rate of a category, in basis points: the full value.</summary>
    public const int HighestRate = BasisPoints.One;

    /// <summary>Every category, in the order of the enum.</summary>
    public static readonly IReadOnlyList<ShopCategory> AllCategories =
    [
        ShopCategory.Weapon,
        ShopCategory.OffHand,
        ShopCategory.Head,
        ShopCategory.Body,
        ShopCategory.Accessory,
        ShopCategory.Heal,
        ShopCategory.Restore,
        ShopCategory.Cure,
        ShopCategory.Revive,
    ];

    private ShopList(string file, IReadOnlyList<ShopType> types, IReadOnlyList<ShopRecord> shops)
    {
        this.File = file;
        this.Types = types;
        this.Shops = shops;
    }

    /// <summary>The path of the file, for an error that names an absent id (T-2).</summary>
    public string File { get; }

    /// <summary>Every shop type, in the order of the file.</summary>
    public IReadOnlyList<ShopType> Types { get; }

    /// <summary>Every shop, in the order of the file.</summary>
    public IReadOnlyList<ShopRecord> Shops { get; }

    /// <summary>Gives the name of a category in the file, such as `off_hand`.</summary>
    /// <param name="category">The category.</param>
    /// <returns>The name.</returns>
    public static string NameOf(ShopCategory category) => category switch
    {
        ShopCategory.Weapon => "weapon",
        ShopCategory.OffHand => "off_hand",
        ShopCategory.Head => "head",
        ShopCategory.Body => "body",
        ShopCategory.Accessory => "accessory",
        ShopCategory.Heal => ItemList.HealName,
        ShopCategory.Restore => ItemList.RestoreName,
        ShopCategory.Cure => ItemList.CureName,
        ShopCategory.Revive => ItemList.ReviveName,
        _ => throw new ArgumentOutOfRangeException(nameof(category), category, "the value names no shop category (D-1151)"),
    };

    /// <summary>Gives the category of a piece of gear, which its slot kind sets (D-1151).</summary>
    /// <param name="piece">The piece.</param>
    /// <returns>The category.</returns>
    public static ShopCategory CategoryOf(GearRecord piece)
    {
        ArgumentNullException.ThrowIfNull(piece);

        return piece.Slot switch
        {
            GearSlotKind.Weapon => ShopCategory.Weapon,
            GearSlotKind.OffHand => ShopCategory.OffHand,
            GearSlotKind.Head => ShopCategory.Head,
            GearSlotKind.Body => ShopCategory.Body,
            GearSlotKind.Accessory => ShopCategory.Accessory,
            _ => throw new ArgumentOutOfRangeException(nameof(piece), piece.Slot, $"the slot kind of '{piece.Id.Value}' has no shop category (D-1151)"),
        };
    }

    /// <summary>Gives the category of a used-up item, which its kind sets (D-1151).</summary>
    /// <param name="item">The item.</param>
    /// <returns>The category.</returns>
    /// <remarks>A key item has no category, because it never sells (D-1154).</remarks>
    public static ShopCategory CategoryOf(UsedUpItem item)
    {
        ArgumentNullException.ThrowIfNull(item);

        return item switch
        {
            HealItem => ShopCategory.Heal,
            RestoreItem => ShopCategory.Restore,
            CureItem => ShopCategory.Cure,
            ReviveItem => ShopCategory.Revive,
            _ => throw new ArgumentOutOfRangeException(nameof(item), item.Id.Value, "the item has no shop category (D-1151)"),
        };
    }

    /// <summary>Reads the shop file, and refuses a repeated id (T-2, D-166).</summary>
    /// <param name="bytes">The bytes of the file, as UTF-8.</param>
    /// <param name="file">The path of the file, for an error.</param>
    /// <returns>The list.</returns>
    /// <exception cref="ContentException">
    /// A field is absent, unknown, repeated, or out of its range, an entry of a stock names no
    /// thing or two things, a lesson entry holds a count, a thing repeats in one stock, an id
    /// repeats, or a shop names an absent type (G-6, T-2).
    /// </exception>
    public static ShopList Read(ReadOnlySpan<byte> bytes, string file)
    {
        var reader = new ContentReader(bytes, file);
        string? comment = null;
        List<ShopType>? types = null;
        List<ShopRecord>? shops = null;

        int depth = reader.ReadObjectStart();
        while (reader.ReadNextField(depth, out string field))
        {
            switch (field)
            {
                case "comment":
                    comment = reader.ReadString();
                    break;
                case "types":
                    types = ReadTypes(ref reader);
                    break;
                case "shops":
                    shops = ReadShops(ref reader);
                    break;
                default:
                    throw reader.UnknownField(field);
            }
        }

        _ = reader.Require(comment, depth, "comment");
        var list = new ShopList(file, reader.Require(types, depth, "types"), reader.Require(shops, depth, "shops"));
        reader.ReadFileEnd();

        list.RefuseRepeatedIds();
        list.RefuseAbsentTypes();
        return list;
    }

    /// <summary>Finds a shop by id.</summary>
    /// <param name="id">The id.</param>
    /// <returns>The shop.</returns>
    /// <exception cref="ContentException">The file holds no such shop (T-2).</exception>
    public ShopRecord Shop(ContentId id)
    {
        ArgumentNullException.ThrowIfNull(id);

        foreach (ShopRecord shop in this.Shops)
        {
            if (string.CompareOrdinal(shop.Id.Value, id.Value) == 0)
            {
                return shop;
            }
        }

        throw ContentException.ForField(this.File, id.Value, "the shop file holds no shop with this id (T-2, D-1149)");
    }

    /// <summary>Tells whether the file holds a shop id.</summary>
    /// <param name="id">The id.</param>
    /// <returns>True when a shop of the file has this id.</returns>
    public bool Holds(ContentId id)
    {
        ArgumentNullException.ThrowIfNull(id);

        foreach (ShopRecord shop in this.Shops)
        {
            if (string.CompareOrdinal(shop.Id.Value, id.Value) == 0)
            {
                return true;
            }
        }

        return false;
    }

    /// <summary>Finds a shop type by id.</summary>
    /// <param name="id">The id.</param>
    /// <returns>The type.</returns>
    /// <exception cref="ContentException">The file holds no such type (T-2).</exception>
    public ShopType Type(ContentId id)
    {
        ArgumentNullException.ThrowIfNull(id);

        return this.FindType(id)
            ?? throw ContentException.ForField(this.File, id.Value, "the shop file holds no shop type with this id (T-2, D-1151)");
    }

    private ShopType? FindType(ContentId id)
    {
        foreach (ShopType type in this.Types)
        {
            if (string.CompareOrdinal(type.Id.Value, id.Value) == 0)
            {
                return type;
            }
        }

        return null;
    }

    private static List<ShopType> ReadTypes(ref ContentReader reader)
    {
        List<ShopType> types = [];
        int depth = reader.ReadArrayStart();
        while (reader.ReadNextElement(depth, types.Count))
        {
            types.Add(ReadType(ref reader));
        }

        return types;
    }

    private static ShopType ReadType(ref ContentReader reader)
    {
        ContentId? id = null;
        int[]? rates = null;

        int depth = reader.ReadObjectStart();
        while (reader.ReadNextField(depth, out string field))
        {
            switch (field)
            {
                case "id":
                    id = reader.ReadContentId(TypeKind);
                    break;
                case "rates":
                    rates = ReadRates(ref reader);
                    break;
                default:
                    throw reader.UnknownField(field);
            }
        }

        return new ShopType(reader.Require(id, depth, "id"), reader.Require(rates, depth, "rates"));
    }

    /// <summary>Reads the rate of each category. Each category takes a rate, and an absent one is an error (D-1151, T-2).</summary>
    private static int[] ReadRates(ref ContentReader reader)
    {
        var rates = new int?[AllCategories.Count];
        int depth = reader.ReadObjectStart();
        while (reader.ReadNextField(depth, out string field))
        {
            ShopCategory category = CategoryNamed(ref reader, field);
            int rate = reader.ReadInt();
            if (rate < 0 || rate > HighestRate)
            {
                throw reader.Refuse($"the rate {rate} is outside 0 to {HighestRate} basis points (D-1151)");
            }

            rates[(int)category] = rate;
        }

        var read = new int[rates.Length];
        foreach (ShopCategory category in AllCategories)
        {
            read[(int)category] = reader.RequireInt(rates[(int)category], depth, NameOf(category));
        }

        return read;
    }

    private static ShopCategory CategoryNamed(ref ContentReader reader, string field)
    {
        foreach (ShopCategory category in AllCategories)
        {
            if (string.CompareOrdinal(NameOf(category), field) == 0)
            {
                return category;
            }
        }

        throw reader.UnknownField(field);
    }

    private static List<ShopRecord> ReadShops(ref ContentReader reader)
    {
        List<ShopRecord> shops = [];
        int depth = reader.ReadArrayStart();
        while (reader.ReadNextElement(depth, shops.Count))
        {
            shops.Add(ReadShop(ref reader));
        }

        return shops;
    }

    private static ShopRecord ReadShop(ref ContentReader reader)
    {
        ContentId? id = null;
        ContentId? type = null;
        List<StockEntry>? stock = null;

        int depth = reader.ReadObjectStart();
        while (reader.ReadNextField(depth, out string field))
        {
            switch (field)
            {
                case "id":
                    id = reader.ReadContentId(Kind);
                    break;
                case "type":
                    type = reader.ReadContentId(TypeKind);
                    break;
                case "stock":
                    stock = ReadStock(ref reader);
                    break;
                default:
                    throw reader.UnknownField(field);
            }
        }

        ContentId readId = reader.Require(id, depth, "id");
        List<StockEntry> readStock = reader.Require(stock, depth, "stock");
        var seen = new SortedSet<string>(StringComparer.Ordinal);
        foreach (StockEntry entry in readStock)
        {
            if (!seen.Add(entry.Thing.Value))
            {
                throw reader.RefuseField(depth, "stock", $"the shop '{readId.Value}' lists '{entry.Thing.Value}' two times, and the count of a stock names its thing (D-1152)");
            }
        }

        return new ShopRecord(readId, reader.Require(type, depth, "type"), readStock);
    }

    private static List<StockEntry> ReadStock(ref ContentReader reader)
    {
        List<StockEntry> stock = [];
        int depth = reader.ReadArrayStart();
        while (reader.ReadNextElement(depth, stock.Count))
        {
            stock.Add(ReadEntry(ref reader));
        }

        return stock;
    }

    /// <summary>
    /// Reads one entry: exactly one of `item`, `gear`, and `lesson`, a price, and a count. An
    /// item or a piece takes a count or `unlimited`, and a lesson takes no count (D-1152, D-1153).
    /// </summary>
    private static StockEntry ReadEntry(ref ContentReader reader)
    {
        List<(StockKind Kind, ContentId Thing)> things = [];
        int? price = null;
        int? count = null;
        bool countRead = false;

        int depth = reader.ReadObjectStart();
        while (reader.ReadNextField(depth, out string field))
        {
            switch (field)
            {
                case "item":
                    things.Add((StockKind.Item, reader.ReadContentId(ItemList.Kind)));
                    break;
                case "gear":
                    things.Add((StockKind.Gear, reader.ReadContentId(GearList.Kind)));
                    break;
                case "lesson":
                    things.Add((StockKind.Lesson, reader.ReadContentId(LessonList.Kind)));
                    break;
                case "price":
                    price = BattleFixture.ReadStat(ref reader, 1);
                    break;
                case "count":
                    countRead = true;
                    count = ReadCount(ref reader);
                    break;
                default:
                    throw reader.UnknownField(field);
            }
        }

        if (things.Count != 1)
        {
            throw reader.RefuseField(depth, "item", $"the entry names {things.Count} things, and an entry names exactly one item, one piece of gear, or one lesson (D-1149)");
        }

        (StockKind kind, ContentId thing) = things[0];
        int readPrice = reader.RequireInt(price, depth, "price");
        if (kind == StockKind.Lesson)
        {
            if (countRead)
            {
                throw reader.RefuseField(depth, "count", $"the lesson '{thing.Value}' takes no count, because the owned check alone limits a lesson (D-1153)");
            }

            return new StockEntry(kind, thing, readPrice, null);
        }

        if (!countRead)
        {
            throw reader.RefuseField(depth, "count", $"the field is absent, and an item or a piece takes a count or '{UnlimitedName}' (D-1152)");
        }

        return new StockEntry(kind, thing, readPrice, count);
    }

    /// <summary>Reads a count from 1, or `unlimited`, which gives no value (D-1152).</summary>
    private static int? ReadCount(ref ContentReader reader)
    {
        if (reader.ReadIntOrWord(UnlimitedName, out int count))
        {
            return null;
        }

        if (count < 1)
        {
            throw reader.Refuse($"the count {count} is below 1. An entry that never runs out takes '{UnlimitedName}' (D-1152)");
        }

        return count;
    }

    private void RefuseRepeatedIds()
    {
        var seen = new SortedSet<string>(StringComparer.Ordinal);
        foreach (ShopType type in this.Types)
        {
            if (!seen.Add(type.Id.Value))
            {
                throw ContentException.ForField(this.File, type.Id.Value, "the file defines this id two times, and an id is permanent (D-166)");
            }
        }

        foreach (ShopRecord shop in this.Shops)
        {
            if (!seen.Add(shop.Id.Value))
            {
                throw ContentException.ForField(this.File, shop.Id.Value, "the file defines this id two times, and an id is permanent (D-166)");
            }
        }
    }

    private void RefuseAbsentTypes()
    {
        foreach (ShopRecord shop in this.Shops)
        {
            if (this.FindType(shop.Type) is null)
            {
                throw ContentException.ForField(this.File, shop.Id.Value, $"the shop names the type '{shop.Type.Value}', which the file does not define (D-1151, T-2)");
            }
        }
    }
}
