using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using TheThingBelow.Core.Battles;
using TheThingBelow.Core.Content;
using TheThingBelow.Core.Shops;
using TheThingBelow.Tools.Content;
using Xunit;

namespace TheThingBelow.Tests;

/// <summary>The reader of the shop file: the types, the stocks, and each refusal of the load (D-1149 to D-1155).</summary>
public sealed class ShopListTests
{
    [Fact]
    public void TheCheckoutHoldsTheFixtureTraderAndItsType()
    {
        ContentSet content = ContentSet.Load(ContentFolder.Read(RepositoryRoot.Find()));
        ShopList shops = content.Battle.Shops;

        ShopRecord trader = shops.Shop(Id("shop.fixture_hub_trader"));
        ShopType type = shops.Type(trader.Type);

        Assert.Equal("shop_type.fixture_trader", type.Id.Value);
        Assert.Equal(2500, type.RateOf(ShopCategory.Accessory));
        Assert.Equal(5000, type.RateOf(ShopCategory.Heal));
        StockEntry draught = trader.Stock[0];
        Assert.Equal((StockKind.Item, "item.fixture_draught", 25, (int?)null), (draught.Kind, draught.Thing.Value, draught.Price, draught.Count));
        StockEntry bolt = Assert.Single(trader.Stock, entry => entry.Kind == StockKind.Lesson);
        Assert.Null(bolt.Count);
    }

    [Fact]
    public void TheShopOfTheTestsReadsEachKindOfEntry()
    {
        ShopList shops = Read(TestBattles.ShopsFile);

        ShopRecord store = Assert.Single(shops.Shops);
        Assert.Equal(
            [StockKind.Item, StockKind.Item, StockKind.Gear, StockKind.Gear, StockKind.Lesson, StockKind.Lesson],
            store.Stock.Select(entry => entry.Kind));
        Assert.Equal([null, 2, 1, 3, null, null], store.Stock.Select(entry => entry.Count));
        Assert.Equal(0, shops.Type(store.Type).RateOf(ShopCategory.Body));
        Assert.Equal("gear.test_helm", store.EntryOf(Id("gear.test_helm"))?.Thing.Value);
        Assert.Null(store.EntryOf(Id("gear.test_mail")));
    }

    [Theory]
    [InlineData(ShopCategory.Weapon, "weapon")]
    [InlineData(ShopCategory.OffHand, "off_hand")]
    [InlineData(ShopCategory.Accessory, "accessory")]
    [InlineData(ShopCategory.Revive, "revive")]
    public void EachCategoryTakesTheNameOfItsSlotOrItsKind(ShopCategory category, string name)
    {
        Assert.Equal(name, ShopList.NameOf(category));
    }

    [Fact]
    public void APieceTakesTheCategoryOfItsSlotAndAnItemTheCategoryOfItsKind()
    {
        BattleContent content = TestBattles.Content;

        Assert.Equal(ShopCategory.Head, ShopList.CategoryOf(content.Gear.Piece(Id("gear.test_helm"))));
        Assert.Equal(ShopCategory.Accessory, ShopList.CategoryOf(content.Gear.Piece(Id("gear.test_weak_charm"))));
        Assert.Equal(ShopCategory.Cure, ShopList.CategoryOf((UsedUpItem)content.Items.Item(Id("item.test_salts"))));
        Assert.Equal(ShopCategory.Revive, ShopList.CategoryOf((UsedUpItem)content.Items.Item(Id("item.test_root"))));
    }

    [Theory]
    [InlineData("\"cure\": 0, ", "", "cure", "absent")]
    [InlineData("\"cure\": 0, ", "\"cure\": -1, ", "rates", "outside 0 to 10000")]
    [InlineData("\"cure\": 0, ", "\"cure\": 10001, ", "rates", "outside 0 to 10000")]
    [InlineData("\"cure\": 0, ", "\"cure\": 0, \"key\": 0, ", "key", "an unknown field")]
    [InlineData("\"price\": 30, \"count\": 2", "\"price\": 30, \"count\": 0", "count", "below 1")]
    [InlineData("\"price\": 30, \"count\": 2", "\"price\": 30, \"count\": \"many\"", "count", "is not a whole number and is not 'unlimited'")]
    [InlineData("\"price\": 30, \"count\": 2", "\"price\": 30", "count", "takes a count or 'unlimited'")]
    [InlineData("\"price\": 30, \"count\": 2", "\"price\": 0, \"count\": 2", "price", "outside 1 to")]
    [InlineData("\"lesson\": \"lesson.fixture_bolt\", \"price\": 100", "\"lesson\": \"lesson.fixture_bolt\", \"price\": 100, \"count\": 1", "count", "the lesson 'lesson.fixture_bolt' takes no count")]
    [InlineData("{ \"gear\": \"gear.test_helm\", \"price\": 50", "{ \"gear\": \"gear.test_helm\", \"item\": \"item.test_salts\", \"price\": 50", "item", "names 2 things")]
    [InlineData("{ \"gear\": \"gear.test_helm\", \"price\": 50", "{ \"price\": 50", "item", "names 0 things")]
    [InlineData("\"gear\": \"gear.test_blade\"", "\"gear\": \"gear.test_helm\"", "stock", "lists 'gear.test_helm' two times")]
    [InlineData("\"type\": \"shop_type.test_trader\"", "\"type\": \"shop_type.test_fence\"", "shop.test_store", "names the type 'shop_type.test_fence', which the file does not define")]
    [InlineData("\"id\": \"shop.test_store\"", "\"id\": \"shop_type.test_trader\"", "id", "shop")]
    public void AFileThatBreaksARuleFailsWithTheFieldAndTheReason(string from, string to, string field, string reason)
    {
        int at = TestBattles.ShopsFile.IndexOf(from, StringComparison.Ordinal);
        Assert.True(at >= 0, $"The shop file of the tests holds no '{from}'.");
        string text = string.Concat(TestBattles.ShopsFile.AsSpan(0, at), to, TestBattles.ShopsFile.AsSpan(at + from.Length));

        ContentException error = Assert.Throws<ContentException>(() => Read(text));

        Assert.Equal(ShopList.Path, error.File);
        Assert.Contains(field, error.Message, StringComparison.Ordinal);
        Assert.Contains(reason, error.Message, StringComparison.Ordinal);
    }

    [Fact]
    public void AStockThatNamesAnAbsentThingFailsTheContent()
    {
        // T-2: each entry of a stock names an item, a piece, or a lesson of its file.
        string shops = TestBattles.ShopsFile.Replace("\"gear.test_helm\"", "\"gear.test_crown\"", StringComparison.Ordinal);

        ContentException error = Assert.Throws<ContentException>(() => ContentWithShops(shops));

        Assert.Equal(ShopList.Path, error.File);
        Assert.Contains("gear.test_crown", error.Message, StringComparison.Ordinal);
        Assert.Contains(GearList.Path, error.Message, StringComparison.Ordinal);
    }

    [Fact]
    public void AShopServiceThatNamesAnAbsentShopFailsWithTheMapAndTheService()
    {
        List<ContentFile> files = [.. ContentFolder.Read(RepositoryRoot.Find())];
        const string Hub = "rules/maps/fixture-hub.json";
        int at = files.FindIndex(file => string.Equals(file.Path, Hub, StringComparison.Ordinal));
        string text = Encoding.UTF8.GetString(files[at].Bytes).Replace("\"shop\": \"shop.fixture_hub_trader\"", "\"shop\": \"shop.fixture_hub_smith\"", StringComparison.Ordinal);
        files[at] = new ContentFile(Hub, Encoding.UTF8.GetBytes(text));

        ContentException error = Assert.Throws<ContentException>(() => ContentSet.Load(files));

        Assert.Equal(Hub, error.File);
        Assert.Contains("service.fixture_hub_shop", error.Message, StringComparison.Ordinal);
        Assert.Contains("the shop 'shop.fixture_hub_smith'", error.Message, StringComparison.Ordinal);
    }

    private static ShopList Read(string text) => ShopList.Read(Encoding.UTF8.GetBytes(text), ShopList.Path);

    private static BattleContent ContentWithShops(string shops)
    {
        BattleContent tests = TestBattles.Content;
        return new BattleContent(tests.Rules, tests.Fixture, tests.Enemies, tests.Abilities, tests.Lessons, tests.Items, tests.Gear, tests.GroupFiles, tests.Profiles, Read(shops));
    }

    private static ContentId Id(string value) => ContentId.Parse(value, "test", value[..value.IndexOf('.', StringComparison.Ordinal)]);
}
