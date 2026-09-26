using System;
using System.Collections.Generic;
using System.Text;
using TheThingBelow.Core;
using TheThingBelow.Core.Battles;
using TheThingBelow.Core.Content;
using TheThingBelow.Core.Maps;
using TheThingBelow.Core.Runs;
using TheThingBelow.Core.Shops;
using TheThingBelow.Core.Story;
using Xunit;

namespace TheThingBelow.Tests;

/// <summary>
/// The rules of a shop and of the price of a rest (D-1149 to D-1158): the exit tests of PR-65 that
/// Core owns. The shop window of Game takes its own tests.
/// </summary>
/// <remarks>
/// The keeper of the inn of <see cref="HubMaps"/> holds the store of <see cref="TestBattles.ShopsFile"/>.
/// Marrek of the tests carries 3 draughts of a limit of 5 and owns the bolt, and the pilfer is
/// no lesson of his.
/// </remarks>
public sealed class ShopRulesTests
{
    private const ulong Seed = 20260926;

    private static readonly ContentId Store = Id("shop.test_store");
    private static readonly ContentId Draught = Id("item.fixture_draught");
    private static readonly ContentId Salts = Id("item.test_salts");
    private static readonly ContentId Torch = Id("item.torch");
    private static readonly ContentId Helm = Id("gear.test_helm");
    private static readonly ContentId Blade = Id("gear.test_blade");
    private static readonly ContentId Mail = Id("gear.test_mail");
    private static readonly ContentId Charm = Id("gear.test_weak_charm");
    private static readonly ContentId Bolt = Id("lesson.fixture_bolt");
    private static readonly ContentId Pilfer = Id("lesson.test_pilfer");
    private static readonly ContentId ClosingFlag = Id("flag.test_marrek_side");

    [Fact]
    public void APartyBuysAnItemAPieceAndALessonAndTheGoldFallsByEachPrice()
    {
        // Exit test 1 (D-1149): 2 draughts at 20, the helm at 50, and the pilfer at 120.
        Simulation run = OpenStore(1000);

        Buy(run, Draught, 2);
        Buy(run, Helm, 1);
        Buy(run, Pilfer, 1);

        PartyState party = run.State.Characters;
        Assert.Equal(1000 - 40 - 50 - 120, party.Gold);
        Assert.Equal(5, party.CountOf(Draught));
        Assert.Equal(1, party.CountOf(Helm));
        Assert.True(party.Owns(Pilfer));
    }

    [Fact]
    public void ASaleRoundsDownAndPaysAtLeastOneGold()
    {
        // Exit test 2 (D-1150, D-1155): the value times the rate of the category, rounded down.
        Simulation run = OpenStore(0);
        ShopRecord store = StoreOf(run);

        Assert.Equal(6, ShopRules.SaleOf(run.State, store, Draught));
        Assert.Equal(10, ShopRules.SaleOf(run.State, store, Helm));
        Assert.Equal(5, ShopRules.SaleOf(run.State, store, Charm));

        // A rate of 1 basis point on the value 12 rounds to 0, and the floor pays 1.
        BattleContent content = TestBattles.WithShops(TestBattles.ShopsFile.Replace("\"heal\": 5000", "\"heal\": 1", StringComparison.Ordinal));
        Simulation floor = OpenStore(0, content: content);
        Assert.Equal(1, ShopRules.SaleOf(floor.State, StoreOf(floor), Draught));
    }

    [Fact]
    public void ASaleTakesTheCountFromThePackAndPaysEachOne()
    {
        // Exit test 2 (D-1150): the sold draughts leave the game, and the store never lists them.
        Simulation run = OpenStore(0);

        run.Step([Intent.OfShopSell(Draught, 2)]);

        Assert.Equal(12, run.State.Characters.Gold);
        Assert.Equal(1, run.State.Characters.CountOf(Draught));
    }

    [Fact]
    public void AShopTypeWithARateOfZeroRefusesTheSaleOfThatCategory()
    {
        // Exit test 3 (D-1151): the trader of the tests buys no body armor and no cure.
        Simulation run = OpenStore(0, party => party with { Pack = [.. party.Pack, new PackValues(Mail, 1), new PackValues(Salts, 1)] });
        ShopRecord store = StoreOf(run);

        Assert.Equal(0, ShopRules.SaleOf(run.State, store, Mail));
        Assert.Equal(0, ShopRules.SaleOf(run.State, store, Salts));
        SimulationException error = Assert.Throws<SimulationException>(() => run.Step([Intent.OfShopSell(Mail, 1)]));
        Assert.Contains("whose type buys nothing of its kind (D-1151)", error.Message, StringComparison.Ordinal);
        Assert.Equal(1, run.State.Characters.CountOf(Mail));
    }

    [Fact]
    public void WornGearAKeyItemAndALessonNeverSell()
    {
        // Exit test 4 (D-1154): the list of a sale holds the pack alone, less each key item.
        Simulation run = OpenStore(0, party => party with
        {
            Pack = [.. party.Pack, new PackValues(Torch, 1), new PackValues(Helm, 1)],
            Characters = [party.Characters[0] with { Gear = Worn(Blade) }],
        });

        Assert.Equal([Helm.Value, Draught.Value], HubWalks.Values(ShopRules.Sellable(run.State)));
        SimulationException key = Assert.Throws<SimulationException>(() => run.Step([Intent.OfShopSell(Torch, 1)]));
        Assert.Contains("a key item never sells (D-1154)", key.Message, StringComparison.Ordinal);
        SimulationException worn = Assert.Throws<SimulationException>(() => run.Step([Intent.OfShopSell(Blade, 1)]));
        Assert.Contains("and the pack holds 0", worn.Message, StringComparison.Ordinal);
        SimulationException lesson = Assert.Throws<SimulationException>(() => run.Step([new Intent(IntentIds.ShopSell, false, null, null, 1, Bolt)]));
        Assert.Contains("no rule of its action reads", lesson.Message, StringComparison.Ordinal);
    }

    [Fact]
    public void AnEntryAtItsStackLimitLeavesTheListAndTheRuleRefusesABuy()
    {
        // Exit test 5 (D-385, D-1166): 3 draughts of a limit of 5 leave room for 2, and a full
        // stack leaves the list.
        Simulation run = OpenStore(1000);
        ShopRecord store = StoreOf(run);
        StockEntry draught = Entry(store, Draught);
        Assert.Equal(new BuyLimit(2, BuyRefusal.None), ShopRules.LimitOf(run.State, store, draught));

        Buy(run, Draught, 2);

        Assert.Equal(new BuyLimit(0, BuyRefusal.NoRoom), ShopRules.LimitOf(run.State, store, draught));
        Assert.DoesNotContain(Draught.Value, Things(ShopRules.Shown(run.State, store)));
        SimulationException error = Assert.Throws<SimulationException>(() => run.Step([Intent.OfShopBuy(Draught, 1)]));
        Assert.Contains("shows no such entry", error.Message, StringComparison.Ordinal);
        Assert.Contains($"seed {Seed}", error.Message, StringComparison.Ordinal);
    }

    [Fact]
    public void TheGoldAndTheStockLimitABuy()
    {
        // D-1158: the blade holds room for 2 and a stock of 3, and 100 gold pays for one at 80. The
        // salts hold room for 5 and a stock of 2.
        Simulation rich = OpenStore(1000);
        Simulation poor = OpenStore(100);
        Simulation broke = OpenStore(10);

        Assert.Equal(new BuyLimit(2, BuyRefusal.None), ShopRules.LimitOf(rich.State, StoreOf(rich), Entry(StoreOf(rich), Blade)));
        Assert.Equal(new BuyLimit(1, BuyRefusal.None), ShopRules.LimitOf(poor.State, StoreOf(poor), Entry(StoreOf(poor), Blade)));
        Assert.Equal(new BuyLimit(0, BuyRefusal.NoGold), ShopRules.LimitOf(broke.State, StoreOf(broke), Entry(StoreOf(broke), Draught)));
        Assert.Equal(new BuyLimit(2, BuyRefusal.None), ShopRules.LimitOf(rich.State, StoreOf(rich), Entry(StoreOf(rich), Salts)));
    }

    [Fact]
    public void ABuyOfACountedEntryLowersItsCountAndAnEntryAtZeroLeavesTheList()
    {
        // Exit test 6 (D-1152): the store holds 2 salts.
        Simulation run = OpenStore(1000);
        ShopRecord store = StoreOf(run);

        Buy(run, Salts, 1);
        Assert.Equal(1, run.State.Shops.LeftOf(store, Entry(store, Salts)));
        Buy(run, Salts, 1);

        Assert.Equal(0, run.State.Shops.LeftOf(store, Entry(store, Salts)));
        Assert.DoesNotContain(Salts.Value, Things(ShopRules.Shown(run.State, store)));
        Assert.Null(run.State.Shops.LeftOf(store, Entry(store, Draught)));
        SimulationException error = Assert.Throws<SimulationException>(() => run.Step([Intent.OfShopBuy(Salts, 1)]));
        Assert.Contains("shows no such entry", error.Message, StringComparison.Ordinal);
    }

    [Fact]
    public void AStoryFlagClosesAShopAndTheShopRefusesTheParty()
    {
        // Exit test 7 (D-319, D-543): the closed notice posts, and no menu opens.
        GameMap map = HubMaps.Of(npcs: HubMaps.Keeper, services: $$"""{ "id": "service.hub_shop", "kind": "shop", "shop": "shop.test_store", "npc": "npc.hub_keeper", "condition": { "not": { "flag": "{{ClosingFlag.Value}}" } } }""");
        RunSnapshot start = Simulation.Start(Seed, map, TestBattles.Content, TestBattles.Notices, TestBattles.Story, DebugIntentHandlers.None).Snapshot();
        StoryValues story = start.Story ?? throw new InvalidOperationException("The start snapshot holds no story state.");
        Simulation run = Simulation.Resume(Seed, start with { Story = story with { Flags = [ClosingFlag] } }, map, TestBattles.Content, TestBattles.Notices, TestBattles.Story, DebugIntentHandlers.None);
        HubRestTests.WalkToKeeper(run);

        HubWalks.Confirm(run);

        Assert.False(run.State.MenuOpen);
        Assert.Empty(run.TakeOpenedServices());
        Assert.Equal([ServiceRules.ClosedNotice.Value], HubWalks.Values(NoticeIds(run)));
    }

    [Fact]
    public void TheSnapshotHoldsTheGoldAndEachCountThatRemains()
    {
        // Exit test 8 (D-1152): a written snapshot resumes with the same gold and the same stock.
        Simulation run = OpenStore(1000);
        Buy(run, Salts, 1);
        Buy(run, Blade, 2);
        RunSnapshot snapshot = run.State.Snapshot();

        RunSnapshot read = ReadLine(RunSnapshotText.Write(snapshot));
        Simulation resumed = Simulation.Resume(Seed, read, HubMaps.Store, TestBattles.Content, TestBattles.Notices, TestBattles.Story, DebugIntentHandlers.None);

        ShopRecord store = StoreOf(resumed);
        Assert.Equal(1000 - 30 - 160, resumed.State.Characters.Gold);
        Assert.Equal(1, resumed.State.Shops.LeftOf(store, Entry(store, Salts)));
        Assert.Equal(1, resumed.State.Shops.LeftOf(store, Entry(store, Blade)));
        Assert.Equal(run.StateHash(), resumed.StateHash());
        Assert.Contains("\"stock\":[{\"shop\":\"shop.test_store\",\"thing\":\"gear.test_blade\",\"left\":1},{\"shop\":\"shop.test_store\",\"thing\":\"item.test_salts\",\"left\":1}]", RunSnapshotText.Write(snapshot), StringComparison.Ordinal);
    }

    [Theory]
    [InlineData("shop.test_smithy", "item.test_salts", 1, "which the shop file lacks")]
    [InlineData("shop.test_store", "item.fixture_tonic", 1, "which the shop does not sell")]
    [InlineData("shop.test_store", "item.fixture_draught", 1, "which never runs out")]
    [InlineData("shop.test_store", "item.test_salts", 3, "above the count 2")]
    public void AStoredStockOutsideTheShopFileFailsTheResumeOfASaveOfThisBuild(string shop, string thing, int left, string reason)
    {
        // T-2, D-1163: a save of this build holds only a count that its shop file can hold.
        ArgumentException error = Assert.Throws<ArgumentException>(() => ShopState.Resume(TestBattles.Content.Shops, [new StockValues(Id(shop), Id(thing), left)], "the test", ResumeDrift.Of(SnapshotOrigin.ThisBuild, 0)));

        Assert.Contains(reason, error.Message, StringComparison.Ordinal);
        Assert.Contains("D-1152", error.Message, StringComparison.Ordinal);
    }

    [Theory]
    [InlineData(SnapshotOrigin.ThisBuild)]
    [InlineData(SnapshotOrigin.OtherBuild)]
    public void AStoredStockBelowZeroOrTwiceFailsTheResumeOfEachBuild(SnapshotOrigin origin)
    {
        // T-2: no edit of a shop file makes a count below 0 or a second value of one entry.
        StockValues salts = new(Store, Salts, 1);

        ArgumentException below = Assert.Throws<ArgumentException>(() => ShopState.Resume(TestBattles.Content.Shops, [salts with { Left = -1 }], "the test", ResumeDrift.Of(origin, 0)));
        ArgumentException twice = Assert.Throws<ArgumentException>(() => ShopState.Resume(TestBattles.Content.Shops, [salts, salts], "the test", ResumeDrift.Of(origin, 0)));

        Assert.Contains("which is below 0", below.Message, StringComparison.Ordinal);
        Assert.Contains("two times", twice.Message, StringComparison.Ordinal);
    }

    [Fact]
    public void ASaveOfAnotherBuildDropsAStockThatTheShopFileNoLongerCountsAndLowersACountAboveIt()
    {
        // D-1163: the drift rule of D-1111 serves the stocks too, and a log line names each change.
        ResumeDrift drift = ResumeDrift.Of(SnapshotOrigin.OtherBuild, 40);
        StockValues[] stored =
        [
            new(Id("shop.test_smithy"), Salts, 1),
            new(Store, Id("item.fixture_tonic"), 1),
            new(Store, Draught, 1),
            new(Store, Salts, 3),
            new(Store, Blade, 1),
        ];

        ShopState state = ShopState.Resume(TestBattles.Content.Shops, stored, "the test", drift);

        Assert.Equal([new StockValues(Store, Blade, 1), new StockValues(Store, Salts, 2)], state.Values());
        Assert.Equal(4, drift.Entries.Count);
    }

    [Fact]
    public void AShopHidesAnOwnedLessonAndShowsItAgainAfterItsLoss()
    {
        // Exit test 9 (D-1024, D-1025, D-1153): Marrek owns the bolt, so the store hides it. A
        // party whose lesson pack lost the bolt sees it again, and a bought pilfer leaves the list.
        Simulation run = OpenStore(1000);
        ShopRecord store = StoreOf(run);
        Assert.Equal([Draught.Value, Salts.Value, Helm.Value, Blade.Value, Pilfer.Value], Things(ShopRules.Shown(run.State, store)));

        Buy(run, Pilfer, 1);
        Assert.DoesNotContain(Pilfer.Value, Things(ShopRules.Shown(run.State, store)));

        Simulation lost = OpenStore(1000, party => party with { LessonPack = WithoutBolt(party.LessonPack) });
        Assert.Contains(Bolt.Value, Things(ShopRules.Shown(lost.State, StoreOf(lost))));
        Assert.Equal(new BuyLimit(1, BuyRefusal.None), ShopRules.LimitOf(lost.State, StoreOf(lost), Entry(StoreOf(lost), Bolt)));
    }

    [Fact]
    public void ABuyOfALessonTakesACountOfOneAlone()
    {
        // D-1023: the party never owns two copies of one lesson.
        Simulation run = OpenStore(1000);

        SimulationException error = Assert.Throws<SimulationException>(() => run.Step([Intent.OfShopBuy(Pilfer, 2)]));

        Assert.Contains("and the most is 1", error.Message, StringComparison.Ordinal);
    }

    [Fact]
    public void ARestTakesItsPriceAndRefusesARestPastTheGold()
    {
        // Exit test 12 (D-1156): the keeper asks 10 gold. The rest window refuses a rest past the
        // gold first, and the rule refuses the request that a fault would send.
        Simulation paid = OpenRest(15);
        paid.Step([Intent.OfPlayer(IntentIds.HubRest)]);
        Assert.Equal(5, paid.State.Characters.Gold);

        Simulation unpaid = OpenRest(9);
        SimulationException error = Assert.Throws<SimulationException>(() => unpaid.Step([Intent.OfPlayer(IntentIds.HubRest)]));
        Assert.Contains("for 10 gold, and the party holds 9 (D-1156)", error.Message, StringComparison.Ordinal);
        Assert.Equal(9, unpaid.State.Characters.Gold);
    }

    [Fact]
    public void ABuyAtTheRestServiceFails()
    {
        // D-1141: a buy needs the shop service that the lead faces.
        Simulation run = OpenRest(100);

        SimulationException error = Assert.Throws<SimulationException>(() => run.Step([Intent.OfShopBuy(Draught, 1)]));

        Assert.Contains("the faced service 'service.hub_rest' is a rest service", error.Message, StringComparison.Ordinal);
    }

    /// <summary>Starts a run on the inn with a gold count, walks to the keeper, and opens the store.</summary>
    internal static Simulation OpenStore(int gold, Func<PartySnapshot, PartySnapshot>? change = null, BattleContent? content = null) =>
        Open(HubMaps.Store, gold, change, content ?? TestBattles.Content);

    /// <summary>Starts a run on the inn whose keeper rests the party for 10 gold, and opens the rest.</summary>
    private static Simulation OpenRest(int gold) =>
        Open(HubMaps.Of(npcs: HubMaps.Keeper, services: """{ "id": "service.hub_rest", "kind": "rest", "price": 10, "npc": "npc.hub_keeper", "condition": { "always": true } }"""), gold, null, TestBattles.Content);

    private static Simulation Open(GameMap map, int gold, Func<PartySnapshot, PartySnapshot>? change, BattleContent content)
    {
        RunSnapshot start = Simulation.Start(Seed, map, content, TestBattles.Notices, TestBattles.Story, DebugIntentHandlers.None).Snapshot();
        PartySnapshot party = (start.Characters ?? throw new InvalidOperationException("The start snapshot holds no party.")) with { Gold = gold };
        Simulation run = Simulation.Resume(Seed, start with { Characters = change is null ? party : change(party) }, map, content, TestBattles.Notices, TestBattles.Story, DebugIntentHandlers.None);
        HubRestTests.WalkToKeeper(run);
        HubWalks.Confirm(run);
        Assert.True(run.State.MenuOpen, "The confirm opened no service.");
        return run;
    }

    private static void Buy(Simulation run, ContentId thing, int count) => run.Step([Intent.OfShopBuy(thing, count)]);

    private static ShopRecord StoreOf(Simulation run) => run.State.BattleContent.Shops.Shop(Store);

    private static StockEntry Entry(ShopRecord shop, ContentId thing) =>
        shop.EntryOf(thing) ?? throw new InvalidOperationException($"The store sells no '{thing.Value}'.");

    private static List<string> Things(IReadOnlyList<StockEntry> entries)
    {
        List<string> things = [];
        foreach (StockEntry entry in entries)
        {
            things.Add(entry.Thing.Value);
        }

        return things;
    }

    private static List<ContentId> NoticeIds(Simulation run)
    {
        List<ContentId> ids = [];
        foreach (Core.Notices.NoticeRecord notice in run.TakeNotices())
        {
            ids.Add(notice.Id);
        }

        return ids;
    }

    private static List<ContentId> WithoutBolt(IReadOnlyList<ContentId>? pack)
    {
        List<ContentId> kept = [];
        foreach (ContentId lesson in pack ?? throw new InvalidOperationException("The start snapshot holds no lesson pack."))
        {
            if (!string.Equals(lesson.Value, Bolt.Value, StringComparison.Ordinal))
            {
                kept.Add(lesson);
            }
        }

        return kept;
    }

    private static ContentId?[] Worn(ContentId weapon)
    {
        var gear = new ContentId?[GearRules.SlotCount];
        gear[0] = weapon;
        return gear;
    }

    private static RunSnapshot ReadLine(string line)
    {
        var reader = new ContentReader(Encoding.UTF8.GetBytes(line), "the test");
        return RunSnapshotText.Read(ref reader);
    }

    private static ContentId Id(string value) => ContentId.Parse(value, "test", value[..value.IndexOf('.', StringComparison.Ordinal)]);
}
