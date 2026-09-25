using System;
using TheThingBelow.Core;
using TheThingBelow.Core.Content;
using TheThingBelow.Core.Maps;
using TheThingBelow.Core.Notices;
using TheThingBelow.Core.Runs;
using Xunit;

namespace TheThingBelow.Tests;

/// <summary>
/// The save service of a hub (D-1131, D-1132): Core emits a save request of the kind slot and
/// posts the saved notice, and Game writes the save through `GameRun.Save`. Core does no file work
/// (G-1).
/// </summary>
/// <remarks>The inn of <see cref="HubMaps"/> puts the save on the bed, a service point at (8, 1).</remarks>
public sealed class HubSaveTests
{
    private const ulong Seed = 20260925;

    private static readonly Intent Save = Intent.OfPlayer(IntentIds.HubSave);

    [Fact]
    public void TheSaveServiceAsksForTheSlotSaveAndPostsTheNotice()
    {
        Simulation run = TestParty.StartFour(Seed, HubMaps.Inn);
        WalkToBed(run);
        HubWalks.Confirm(run);
        MapService opened = Assert.Single(run.TakeOpenedServices());
        Assert.Equal(("service.hub_save", ServiceKind.Save), (opened.Id.Value, opened.Kind));

        run.Step([Save]);

        Assert.Equal([SaveRequestKind.Slot], run.TakeSaveRequests());
        Assert.Empty(run.TakeSaveRequests());
        Assert.Equal(ServiceRules.SavedNotice.Value, Assert.Single(run.TakeNotices()).Id.Value);
    }

    [Fact]
    public void ASaveRequestIsOutputAndNeverState()
    {
        // D-168: a request waits for Game alone, so the state hash and the snapshot never read it.
        Simulation run = TestParty.StartFour(Seed, HubMaps.Inn);
        WalkToBed(run);
        HubWalks.Confirm(run);
        Simulation twin = TestParty.StartFour(Seed, HubMaps.Inn);
        WalkToBed(twin);
        HubWalks.Confirm(twin);

        run.Step([Save]);
        twin.Step([Save]);
        _ = twin.TakeSaveRequests();

        Assert.Equal(twin.StateHash(), run.StateHash());
        Assert.Equal(RunSnapshotText.Write(twin.Snapshot()), RunSnapshotText.Write(run.Snapshot()));
    }

    [Fact]
    public void ASaveRequestAtTheRestServiceFails()
    {
        Simulation run = TestParty.StartFour(Seed, HubMaps.Inn);
        HubRestTests.WalkToKeeper(run);
        HubWalks.Confirm(run);

        SimulationException error = Assert.Throws<SimulationException>(() => run.Step([Save]));

        Assert.Contains("a save request, and the faced service 'service.hub_rest' is a rest service", error.Message, StringComparison.Ordinal);
        Assert.Empty(run.TakeSaveRequests());
    }

    [Fact]
    public void ASaveRequestWithNoOpenSaveServiceFails()
    {
        Simulation run = TestParty.StartFour(Seed, HubMaps.Of());
        run.Step([Intent.OfPlayer(IntentIds.OpenMenu)]);

        SimulationException error = Assert.Throws<SimulationException>(() => run.Step([Save]));

        Assert.Contains("a save request, and the lead faces no host of a service", error.Message, StringComparison.Ordinal);
    }

    [Theory]
    [InlineData(SaveRequestKind.Slot, "slot")]
    [InlineData(SaveRequestKind.Autosave, "autosave")]
    public void EachSaveRequestKindHasItsName(SaveRequestKind kind, string name)
    {
        Assert.Equal(name, SaveRequestKinds.NameOf(kind));
    }

    [Fact]
    public void TheNoticeFileOfTheCheckoutHoldsEachNoticeOfTheServices()
    {
        // D-989 and T-2: a post of a notice that the file lacks fails in play, so the load of the
        // shipped content proves each notice that the rules of a hub post.
        NoticeList notices = NoticeList.Read(System.IO.File.ReadAllBytes(RepositoryRoot.PathTo("content/rules/notices.json")), NoticeList.Path);

        foreach (ContentId notice in ServiceRules.Notices)
        {
            Assert.True(notices.Holds(notice), $"The notice file lacks '{notice.Value}'.");
        }
    }

    /// <summary>Walks the lead from the spawn point to (7, 1), and turns it to the bed at (8, 1).</summary>
    private static void WalkToBed(Simulation run)
    {
        HubWalks.Walk(run, StepDirection.East, 6);
        HubWalks.Face(run, StepDirection.East);
    }
}
