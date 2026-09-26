using System;
using System.Collections.Generic;
using TheThingBelow.Core;
using TheThingBelow.Core.Battles;
using TheThingBelow.Core.Maps;
using TheThingBelow.Core.Notices;
using TheThingBelow.Core.Runs;
using Xunit;

namespace TheThingBelow.Tests;

/// <summary>
/// The rest service of a hub (D-390, D-1131): exit test 1 and exit test 5 of PR-14, and the
/// refusal of a rest request with no open rest service, which D-1141 keeps as a Core test.
/// </summary>
/// <remarks>
/// The inn of <see cref="HubMaps"/> puts the rest on the keeper at (2, 6). The lead walks from the
/// spawn point at (1, 1) to (2, 5) and faces south, so the keeper stands on the faced tile.
/// </remarks>
public sealed class HubRestTests
{
    private const ulong Seed = 20260925;

    private static readonly Intent Rest = Intent.OfPlayer(IntentIds.HubRest);

    [Fact]
    public void TheGroupOfFourWalksTheHubAndRests()
    {
        // Exit test 1 of PR-14 (D-362): the three in the party and the fourth in the reserve walk
        // to the keeper, the confirm opens the rest, and the rest fills each of them (D-1135).
        Simulation run = TestParty.StartFour(Seed, HubMaps.Inn, TestParty.FourContent, (_, stored) => Hurt(stored));
        WalkToKeeper(run);

        HubWalks.Confirm(run);
        Assert.True(run.State.MenuOpen);
        Assert.Equal(["service.hub_rest"], ServiceIds(run.TakeOpenedServices()));

        run.Step([Rest]);
        run.Step([Intent.OfPlayer(IntentIds.CloseMenu)]);

        Assert.False(run.State.MenuOpen);
        Assert.Equal(3, run.State.Characters.Members.Count);
        Assert.Single(run.State.Characters.Reserve);
        foreach (PartyMember member in Everyone(run.State.Characters))
        {
            Assert.Equal((member.Stats.Health, member.Stats.Mp), (member.Health, member.Mp));
        }

        Assert.Equal([ServiceRules.RestedNotice.Value], NoticeIds(run.TakeNotices()));
    }

    [Fact]
    public void ARestCuresPoisonBlindAndSilenceInThePartyAndTheReserve()
    {
        // Exit test 5 of PR-14 (D-390): each status that lasts past a fight ends, the reserve's
        // too, and a downed character stands again (D-1135). A downed character holds no status
        // (D-801), so the third character is down alone.
        Simulation run = TestParty.StartFour(Seed, HubMaps.Inn, TestParty.FourContent, (place, stored) => place == 2 ? stored with { Health = 0 } : Hurt(stored));
        Assert.True(run.State.Characters.Members[2].Down);
        Assert.Equal(3, run.State.Characters.Reserve[0].Statuses.Count);
        WalkToKeeper(run);
        HubWalks.Confirm(run);

        run.Step([Rest]);

        foreach (PartyMember member in Everyone(run.State.Characters))
        {
            Assert.Empty(member.Statuses);
            Assert.False(member.Down);
        }
    }

    [Fact]
    public void ARestRequestWithNoOpenRestServiceFailsWithContext()
    {
        // D-1141 and T-2: no player path asks for a rest at a hub with no rest service, and Core
        // still refuses the request with the reason, the seed, and the tick.
        Simulation run = TestParty.StartFour(Seed, HubMaps.Of());
        run.Step([Intent.OfPlayer(IntentIds.OpenMenu)]);

        SimulationException error = Assert.Throws<SimulationException>(() => run.Step([Rest]));

        Assert.Contains("a rest request, and the lead faces no host of a service", error.Message, StringComparison.Ordinal);
        Assert.Contains("D-1141", error.Message, StringComparison.Ordinal);
        Assert.Contains($"seed {Seed}", error.Message, StringComparison.Ordinal);
    }

    [Fact]
    public void ARestRequestWithTheMenuClosedFails()
    {
        // D-1141: the rest window of an open service makes the request, so a closed menu refuses it.
        Simulation run = TestParty.StartFour(Seed, HubMaps.Inn);
        WalkToKeeper(run);

        SimulationException error = Assert.Throws<SimulationException>(() => run.Step([Rest]));

        Assert.Contains("while no menu is open", error.Message, StringComparison.Ordinal);
    }

    [Fact]
    public void ARestRequestAtTheSaveServiceFails()
    {
        // D-1131: the bed holds the save service, so a rest request there names the wrong kind.
        Simulation run = TestParty.StartFour(Seed, HubMaps.Inn);
        HubWalks.Walk(run, StepDirection.East, 6);
        HubWalks.Face(run, StepDirection.East);
        HubWalks.Confirm(run);

        SimulationException error = Assert.Throws<SimulationException>(() => run.Step([Rest]));

        Assert.Contains("the faced service 'service.hub_save' is a save service", error.Message, StringComparison.Ordinal);
    }

    /// <summary>Walks the lead from the spawn point to (2, 5), and turns it to the keeper at (2, 6).</summary>
    internal static void WalkToKeeper(Simulation run)
    {
        HubWalks.Walk(run, StepDirection.South, 4);
        HubWalks.Walk(run, StepDirection.East, 1);
        HubWalks.Face(run, StepDirection.South);
    }

    /// <summary>Hurts one character: 1 health, no MP, and poison, blind, and silence (D-390).</summary>
    private static CharacterValues Hurt(CharacterValues stored) =>
        stored with
        {
            Health = 1,
            Statuses = [StatusKind.Poison, StatusKind.Blind, StatusKind.Silence],
            Growth = (stored.Growth ?? throw new InvalidOperationException("The stored character holds no growth.")) with { Mp = 0 },
        };

    private static List<PartyMember> Everyone(PartyState party) => [.. party.Members, .. party.Reserve];

    private static List<string> ServiceIds(IReadOnlyList<MapService> services)
    {
        List<string> ids = [];
        foreach (MapService service in services)
        {
            ids.Add(service.Id.Value);
        }

        return ids;
    }

    private static List<string> NoticeIds(IReadOnlyList<NoticeRecord> notices)
    {
        List<string> ids = [];
        foreach (NoticeRecord notice in notices)
        {
            ids.Add(notice.Id.Value);
        }

        return ids;
    }
}
