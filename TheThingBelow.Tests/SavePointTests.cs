using System;
using TheThingBelow.Core;
using TheThingBelow.Core.Battles;
using TheThingBelow.Core.Content;
using TheThingBelow.Core.Maps;
using TheThingBelow.Core.Notices;
using TheThingBelow.Core.Runs;
using TheThingBelow.Tools.Content;
using Xunit;

namespace TheThingBelow.Tests;

/// <summary>
/// The save point (D-1132, D-1221): a confirm at it opens the save window, Core emits a save
/// request of the kind slot and posts the saved notice, and Game writes the save through
/// `GameRun.Save`. Core does no file work (G-1). The save restores nothing.
/// </summary>
/// <remarks>The inn of <see cref="HubMaps"/> puts the waystone, a save point, at (8, 1).</remarks>
public sealed class SavePointTests
{
    private const ulong Seed = 20260925;

    private static readonly Intent Save = Intent.OfPlayer(IntentIds.Save);

    private static readonly Lazy<ContentSet> Content =
        new(() => ContentSet.Load(ContentFolder.Read(RepositoryRoot.Find())));

    [Fact]
    public void AConfirmAtTheSavePointOpensTheSaveWindowAndTheSaveAsksForTheSlotSave()
    {
        // Exit test 3 of PR-16 (D-1221): the confirm opens the menu and the save window.
        Simulation run = TestParty.StartFour(Seed, HubMaps.Inn);
        WalkToWaystone(run);
        HubWalks.Confirm(run);
        Assert.Equal("save_point.hub_waystone", Assert.Single(run.TakeOpenedSavePoints()).Value);
        Assert.Empty(run.TakeOpenedServices());
        Assert.True(run.State.MenuOpen);

        run.Step([Save]);

        Assert.Equal([SaveRequestKind.Slot], run.TakeSaveRequests());
        Assert.Empty(run.TakeSaveRequests());
        Assert.Equal(SavePointRules.SavedNotice.Value, Assert.Single(run.TakeNotices()).Id.Value);
    }

    [Fact]
    public void ASavePointRestoresNoApAndNoHealth()
    {
        // Exit test 3 of PR-16 (D-1221): AP comes back in a fight, from items, and at a rest.
        Simulation run = TestParty.StartFour(Seed, HubMaps.Inn, TestParty.FourContent, (_, stored) => stored with { Health = 5, Growth = stored.Growth! with { Ap = 1 } });
        WalkToWaystone(run);
        int[] before = [.. System.Linq.Enumerable.Select(run.State.Characters.Members, member => member.Ap)];

        HubWalks.Confirm(run);
        run.Step([Save]);

        for (int slot = 0; slot < run.State.Characters.Members.Count; slot += 1)
        {
            PartyMember member = run.State.Characters.Members[slot];
            Assert.Equal((5, before[slot]), (member.Health, member.Ap));
        }
    }

    [Fact]
    public void ASavePointIsSolid()
    {
        // D-1222: the lead faces a save point and never walks onto it.
        Assert.True(MapThingKinds.IsSolid(MapThingKind.SavePoint));
        Assert.False(MapRules.CanEnter(HubMaps.Inn, new TilePoint(8, 1)));
    }

    [Fact]
    public void ASaveRequestIsOutputAndNeverState()
    {
        // D-168: a request waits for Game alone, so the state hash and the snapshot never read it.
        Simulation run = TestParty.StartFour(Seed, HubMaps.Inn);
        WalkToWaystone(run);
        HubWalks.Confirm(run);
        Simulation twin = TestParty.StartFour(Seed, HubMaps.Inn);
        WalkToWaystone(twin);
        HubWalks.Confirm(twin);

        run.Step([Save]);
        twin.Step([Save]);
        _ = twin.TakeSaveRequests();

        Assert.Equal(twin.StateHash(), run.StateHash());
        Assert.Equal(RunSnapshotText.Write(twin.Snapshot()), RunSnapshotText.Write(run.Snapshot()));
    }

    [Fact]
    public void ASaveRequestAtTheKeeperFails()
    {
        Simulation run = TestParty.StartFour(Seed, HubMaps.Inn);
        HubRestTests.WalkToKeeper(run);
        HubWalks.Confirm(run);

        SimulationException error = Assert.Throws<SimulationException>(() => run.Step([Save]));

        Assert.Contains("a save request, and the lead faces no save point", error.Message, StringComparison.Ordinal);
        Assert.Empty(run.TakeSaveRequests());
    }

    [Fact]
    public void ASaveRequestWithNoOpenMenuFails()
    {
        Simulation run = TestParty.StartFour(Seed, HubMaps.Inn);
        WalkToWaystone(run);

        SimulationException error = Assert.Throws<SimulationException>(() => run.Step([Save]));

        Assert.Contains("a save request while no menu is open", error.Message, StringComparison.Ordinal);
    }

    [Fact]
    public void TheFixtureDungeonAndTheFixtureHubEachHoldASavePoint()
    {
        // D-1221: one kind serves each map, and the hub holds no save service.
        foreach (string id in new[] { "map.fixture_dungeon", "map.fixture_hub" })
        {
            GameMap map = Content.Value.Map(ContentId.Parse(id, "test", "map"));
            Assert.Contains(map.Things, thing => thing.Kind == MapThingKind.SavePoint);
            Assert.DoesNotContain(map.Services, service => string.CompareOrdinal(ServiceKinds.NameOf(service.Kind), "save") == 0);
        }
    }

    [Theory]
    [InlineData(SaveRequestKind.Slot, "slot")]
    [InlineData(SaveRequestKind.Autosave, "autosave")]
    public void EachSaveRequestKindHasItsName(SaveRequestKind kind, string name)
    {
        Assert.Equal(name, SaveRequestKinds.NameOf(kind));
    }

    [Fact]
    public void TheNoticeFileOfTheCheckoutHoldsEachNoticeOfTheSaveTheDoorsAndTheChests()
    {
        // D-989 and T-2: a post of a notice that the file lacks fails in play, so the load of the
        // shipped content proves each notice that these rules post.
        NoticeList notices = NoticeList.Read(System.IO.File.ReadAllBytes(RepositoryRoot.PathTo("content/rules/notices.json")), NoticeList.Path);

        ContentId[] posted = [.. ServiceRules.Notices, .. SavePointRules.Notices, .. DoorRules.Notices, .. ChestRules.Notices];
        foreach (ContentId notice in posted)
        {
            Assert.True(notices.Holds(notice), $"The notice file lacks '{notice.Value}'.");
        }
    }

    /// <summary>Walks the lead from the spawn point to (7, 1), and turns it to the waystone at (8, 1).</summary>
    private static void WalkToWaystone(Simulation run)
    {
        HubWalks.Walk(run, StepDirection.East, 6);
        HubWalks.Face(run, StepDirection.East);
    }
}
