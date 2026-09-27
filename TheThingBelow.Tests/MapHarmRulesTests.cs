using System;
using System.Collections.Generic;
using TheThingBelow.Core;
using TheThingBelow.Core.Battles;
using TheThingBelow.Core.Maps;
using TheThingBelow.Core.Runs;
using Xunit;

namespace TheThingBelow.Tests;

/// <summary>
/// The tests of the harm of the map of PR-64: poison and bad air once each second of the world, and
/// the wipe when each character who fights goes down (D-397, D-1234 to D-1236).
/// </summary>
public sealed class MapHarmRulesTests
{
    private const ulong Seed = 0x64;

    /// <summary>The tile of bad air of the yard that the tests stand on.</summary>
    private static readonly TilePoint Air = new(1, 5);

    [Fact]
    public void PoisonTakesOnePercentOfFullHealthOnEachSixtiethWorldTick()
    {
        // D-1234: 100 basis points of full health, and the lead stands still the whole time.
        Simulation run = TestParty.Start(Seed, stored => stored with { Statuses = [StatusKind.Poison] }, null, TrapMaps.Yard);
        PartyMember marrek = run.State.Characters.Members[0];
        int full = marrek.Health;

        StepToHarm(run);
        int once = full - Math.Max(1, full * 100 / BasisPoints.One);
        Assert.Equal(once, marrek.Health);

        run.Step([]);
        Assert.Equal(once, marrek.Health);
        StepToHarm(run);
        Assert.Equal(once - Math.Max(1, full * 100 / BasisPoints.One), marrek.Health);
    }

    [Fact]
    public void AnOpenMenuStopsTheHarmBecauseTheWorldDoesNotTick()
    {
        // D-650, D-1234: the world tick stands still under a menu, so poison never lands there.
        Simulation run = TestParty.Start(Seed, stored => stored with { Statuses = [StatusKind.Poison] }, null, TrapMaps.Yard);
        int health = run.State.Characters.Members[0].Health;
        run.Step([Intent.OfPlayer(IntentIds.OpenMenu)]);
        long world = run.State.WorldTick;

        for (int tick = 0; tick < 3 * MapRules.HarmTicks; tick += 1)
        {
            run.Step([]);
        }

        Assert.Equal(world, run.State.WorldTick);
        Assert.Equal(health, run.State.Characters.Members[0].Health);
    }

    [Fact]
    public void PoisonHurtsTheReserveAndADownInTheReserveIsNoWipe()
    {
        // D-1236: the poisoned fourth in the reserve goes down from 1 health, and the three who fight
        // stand, so the run holds no wipe. The notice of the down still tells the player.
        Simulation run = TestParty.StartFour(Seed, TrapMaps.Yard, TestParty.FourContent, (place, stored) => place == 3 ? stored with { Health = 1, Statuses = [StatusKind.Poison] } : stored);

        StepToHarm(run);

        PartyMember fourth = run.State.Characters.Reserve[0];
        Assert.True(fourth.Down);
        Assert.Empty(fourth.Statuses);
        Assert.False(run.State.MapWiped);
        Assert.Equal(MapHarmRules.FellNotice.Value, Assert.Single(run.TakeNotices()).Id.Value);
    }

    [Fact]
    public void BadAirHurtsEachCharacterWhoFightsWhileTheLeadStandsInIt()
    {
        // D-1235, D-1236: 200 basis points from each of the three who fight, and none from the reserve.
        Simulation run = TestParty.StartFour(Seed, TrapMaps.Yard);
        HubWalks.Walk(run, StepDirection.South, 4);
        Assert.Equal(Air, run.State.Party.LeadAt);
        Assert.Equal(TileKind.BadAir, TrapMaps.Yard.TileAt(Air));
        List<int> before = HealthOf(run.State.Characters.Members);
        int reserve = run.State.Characters.Reserve[0].Health;

        StepToHarm(run);

        IReadOnlyList<PartyMember> members = run.State.Characters.Members;
        for (int slot = 0; slot < members.Count; slot += 1)
        {
            Assert.Equal(before[slot] - Math.Max(1, members[slot].Stats.Health * 200 / BasisPoints.One), members[slot].Health);
        }

        Assert.Equal(reserve, run.State.Characters.Reserve[0].Health);

        HubWalks.Walk(run, StepDirection.East, 2);
        List<int> clear = HealthOf(members);
        StepToHarm(run);
        Assert.Equal(clear, HealthOf(members));
    }

    [Fact]
    public void ADownOnTheMapEndsEachStatusAndPostsTheNoticeWhileAnotherStands()
    {
        // D-392, D-801: Marrek goes down from 1 health, the down ends his blind, and the second stands.
        Simulation run = TestParty.StartEach(
            Seed,
            (slot, stored) => slot == 0 ? stored with { Health = 1, Statuses = [StatusKind.Poison, StatusKind.Blind] } : stored,
            TestBattles.WithParty(2),
            TrapMaps.Yard);

        StepToHarm(run);

        Assert.True(run.State.Characters.Members[0].Down);
        Assert.Empty(run.State.Characters.Members[0].Statuses);
        Assert.False(run.State.MapWiped);
        Assert.Equal(MapHarmRules.FellNotice.Value, Assert.Single(run.TakeNotices()).Id.Value);
    }

    [Theory]
    [InlineData(1)]
    [InlineData(2)]
    [InlineData(3)]
    public void APoisonDownOfEachCharacterWhoFightsWipesTheParty(int size)
    {
        // Exit tests 3 and 4 of PR-64 (D-336, D-397): for a party of one, two, or three, the last
        // down holds a wipe. With three, the healthy fourth in the reserve never steps in.
        Simulation run = size == 3
            ? TestParty.StartFour(Seed, TrapMaps.Yard, TestParty.FourContent, (place, stored) => place < 3 ? Dying(stored) : stored)
            : TestParty.StartEach(Seed, (slot, stored) => Dying(stored), TestBattles.WithParty(size), TrapMaps.Yard);
        Assert.Equal(size, run.State.Characters.Members.Count);

        StepToHarm(run);

        Assert.True(run.State.MapWiped);
        Assert.Empty(run.Accepted());
        long world = run.State.WorldTick;
        SimulationException error = Assert.Throws<SimulationException>(() => run.Step([HubWalks.Move(StepDirection.East)]));
        Assert.Contains("D-397", error.Message, StringComparison.Ordinal);
        run.Step([]);
        Assert.Equal(world, run.State.WorldTick);
        if (size == 3)
        {
            Assert.False(run.State.Characters.Reserve[0].Down);
        }
    }

    [Fact]
    public void AWipeOnTheMapHoldsNoFieldOfItsOwnInTheSnapshot()
    {
        // D-397: the wipe comes from the party alone, so a resume of the snapshot holds it too.
        Simulation run = TestParty.Start(Seed, Dying, null, TrapMaps.Yard);
        StepToHarm(run);

        Simulation resumed = Simulation.Resume(Seed, run.Snapshot(), TrapMaps.Yard, TestBattles.Content, TestBattles.Notices, TestBattles.Story, DebugIntentHandlers.None);

        Assert.True(resumed.State.MapWiped);
        Assert.Equal(run.StateHash(), resumed.StateHash());
    }

    [Fact]
    public void ThePropertyOfTheHarmOfTheMapHoldsOverOneThousandSeeds()
    {
        // Exit test 1 of PR-64 (D-397, D-1234 to D-1236): on each seed, a party of one to three with
        // a health and a poison from the seed stands on bad air or on ground. Each harm takes its
        // share from each character that it reads, no health ever rises, and the run wipes exactly
        // when each character who fights is down.
        for (ulong seed = 0; seed < 1000; seed += 1)
        {
            int size = (int)(seed % 3) + 1;
            bool onAir = seed % 2 == 0;
            Simulation run = TestParty.StartEach(
                seed,
                (slot, stored) => stored with
                {
                    Health = 1 + (int)((seed * 7 + (ulong)slot * 13) % (ulong)stored.Health),
                    Statuses = (seed >> slot) % 2 == 0 ? [StatusKind.Poison] : [],
                },
                TestBattles.WithParty(size),
                TrapMaps.Yard);
            if (onAir)
            {
                run = OnAir(run, seed);
            }

            for (int harm = 0; harm < 5 && !run.State.MapWiped; harm += 1)
            {
                List<int> before = HealthOf(run.State.Characters.Members);
                StepToHarm(run);
                IReadOnlyList<PartyMember> members = run.State.Characters.Members;
                for (int slot = 0; slot < members.Count; slot += 1)
                {
                    PartyMember member = members[slot];
                    int expected = before[slot];
                    int full = member.Stats.Health;
                    bool poisoned = before[slot] > 0 && (seed >> slot) % 2 == 0;
                    if (poisoned)
                    {
                        expected -= Math.Min(expected, Math.Max(1, full * 100 / BasisPoints.One));
                    }

                    if (onAir && expected > 0)
                    {
                        expected -= Math.Min(expected, Math.Max(1, full * 200 / BasisPoints.One));
                    }

                    Assert.True(expected == member.Health, $"Seed {seed}: slot {slot} holds {member.Health} health after harm {harm}, and the rules give {expected}.");
                }

                bool allDown = !run.State.Characters.AnyStands();
                Assert.True(allDown == run.State.MapWiped, $"Seed {seed}: each character who fights is down is {allDown}, and the wipe is {run.State.MapWiped}.");
            }
        }
    }

    /// <summary>A character at 1 health with poison, whom the next harm downs.</summary>
    private static CharacterValues Dying(CharacterValues stored) => stored with { Health = 1, Statuses = [StatusKind.Poison] };

    /// <summary>Resumes the run with the lead on the bad air of the yard, with no tick between.</summary>
    private static Simulation OnAir(Simulation run, ulong seed)
    {
        RunSnapshot snapshot = run.Snapshot();
        MapSnapshot map = snapshot.Map!;
        List<string> walked = [.. map.Walked];
        char[] row = walked[Air.Y].ToCharArray();
        row[Air.X] = 'x';
        walked[Air.Y] = new string(row);
        RunSnapshot moved = snapshot with { Map = map with { LeadX = Air.X, LeadY = Air.Y, Walked = walked } };
        return Simulation.Resume(seed, moved, TrapMaps.Yard, run.State.BattleContent, TestBattles.Notices, TestBattles.Story, DebugIntentHandlers.None);
    }

    /// <summary>Steps the run with no intent up to the next world tick that divides by the harm interval.</summary>
    private static void StepToHarm(Simulation run)
    {
        do
        {
            run.Step([]);
        }
        while (run.State.WorldTick % MapRules.HarmTicks != 0 && !run.State.MapWiped);
    }

    private static List<int> HealthOf(IReadOnlyList<PartyMember> members)
    {
        List<int> health = [];
        foreach (PartyMember member in members)
        {
            health.Add(member.Health);
        }

        return health;
    }
}
