using System;
using System.Text;
using TheThingBelow.Core.Battles;
using TheThingBelow.Core.Content;
using TheThingBelow.Tools.Content;
using Xunit;

namespace TheThingBelow.Tests;

/// <summary>The reader of the item file: the kinds, the stack limits, and the effects (D-1038, D-1046).</summary>
public sealed class ItemListTests
{
    [Fact]
    public void TheCheckoutHoldsOneItemOfEachEffect()
    {
        // D-384, D-1046: the MP draughts, the healing, the cures, and the rare revive.
        ContentSet content = ContentSet.Load(ContentFolder.Read(RepositoryRoot.Find()));
        ItemList items = content.Battle.Items;

        // `ContentId` compares by reference, so each test compares the values (F-39).
        HealItem draught = Assert.IsType<HealItem>(items.Item(Id("item.fixture_draught")));
        Assert.Equal((5, 100, 30), (draught.Limit, draught.Delay, draught.Amount));
        RestoreItem tonic = Assert.IsType<RestoreItem>(items.Item(Id("item.fixture_tonic")));
        Assert.Equal((5, 100, 10), (tonic.Limit, tonic.Delay, tonic.Amount));
        CureItem salts = Assert.IsType<CureItem>(items.Item(Id("item.fixture_salts")));
        Assert.Equal([StatusKind.Poison, StatusKind.Blind, StatusKind.Silence], salts.Statuses);
        ReviveItem root = Assert.IsType<ReviveItem>(items.Item(Id("item.fixture_root")));
        Assert.Equal((3, 120, 25), (root.Limit, root.Delay, root.Amount));
    }

    [Fact]
    public void AKeyItemHoldsTheLimitOne()
    {
        ItemList items = Read(TestBattles.ItemsFile);

        ItemRecord token = items.Item(Id("item.test_token"));

        Assert.IsType<KeyItem>(token);
        Assert.Equal(1, token.Limit);
    }

    [Theory]
    [InlineData("\"limit\": 5, \"delay\": 100, \"amount\": 30", "\"limit\": 2, \"delay\": 100, \"amount\": 30", "limit", "outside 3 to 10")]
    [InlineData("\"limit\": 5, \"delay\": 100, \"amount\": 30", "\"limit\": 11, \"delay\": 100, \"amount\": 30", "limit", "outside 3 to 10")]
    [InlineData("\"kind\": \"key\", \"limit\": 1", "\"kind\": \"key\", \"limit\": 2", "limit", "has the limit 1")]
    [InlineData("\"kind\": \"key\", \"limit\": 1", "\"kind\": \"key\", \"limit\": 1, \"delay\": 100", "delay", "takes no field")]
    [InlineData("\"amount\": 30 }", "\"amount\": 30, \"statuses\": [\"poison\"] }", "statuses", "takes no field")]
    [InlineData("\"statuses\": [\"poison\", \"silence\"]", "\"statuses\": [\"poison\", \"silence\"], \"amount\": 5", "amount", "takes no field")]
    [InlineData("\"kind\": \"revive\"", "\"kind\": \"raise\"", "kind", "not one of key, heal, restore, cure, revive")]
    [InlineData("\"delay\": 100, \"amount\": 30", "\"amount\": 30", "delay", "absent")]
    [InlineData("\"amount\": 30 }", "\"amount\": 0 }", "amount", "outside 1 to")]
    [InlineData("\"id\": \"item.test_tonic\"", "\"id\": \"item.fixture_draught\"", "item.fixture_draught", "two times")]
    [InlineData("\"id\": \"item.test_tonic\"", "\"id\": \"gear.test_tonic\"", "id", "item")]
    public void AnItemThatBreaksARuleFailsWithTheField(string from, string to, string field, string reason)
    {
        int at = TestBattles.ItemsFile.IndexOf(from, StringComparison.Ordinal);
        Assert.True(at >= 0, $"The item file of the tests holds no '{from}'.");
        string text = string.Concat(TestBattles.ItemsFile.AsSpan(0, at), to, TestBattles.ItemsFile.AsSpan(at + from.Length));

        ContentException error = Assert.Throws<ContentException>(() => Read(text));

        Assert.Equal(ItemList.Path, error.File);
        Assert.Contains(field, error.Message, StringComparison.Ordinal);
        Assert.Contains(reason, error.Message, StringComparison.Ordinal);
    }

    private static ItemList Read(string text) => ItemList.Read(Encoding.UTF8.GetBytes(text), ItemList.Path);

    private static ContentId Id(string value) => ContentId.Parse(value, "test", value[..value.IndexOf('.', StringComparison.Ordinal)]);
}
