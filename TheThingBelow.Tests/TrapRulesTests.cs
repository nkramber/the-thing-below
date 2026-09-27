using System;
using System.Collections.Generic;
using TheThingBelow.Core;
using TheThingBelow.Core.Battles;
using TheThingBelow.Core.Content;
using TheThingBelow.Core.Maps;
using TheThingBelow.Core.Runs;
using Xunit;

namespace TheThingBelow.Tests;

/// <summary>
/// The tests of the traps of PR-64: what a trap does when it fires, the one fire, the show of a
/// Theft drill, and the disarm (D-386, D-1226 to D-1231).
/// </summary>
public sealed class TrapRulesTests
{
    private const ulong Seed = 0x64;

    private static readonly ContentId Pilfer = ContentId.Parse("lesson.test_pilfer", "test", "lesson");

    [Fact]
    public void ADamageTrapTakesItsShareFromEachCharacterWhoFightsAndThenIsSpent()
    {
        // Exit test 1 of PR-64 (D-1230): the blade takes 2500 basis points of full health from each of
        // the three who fight, and the reserve keeps its health.
        Simulation run = TestParty.StartFour(Seed, TrapMaps.Hall);
        List<int> before = HealthOf(run.State.Characters.Members);
        int reserve = run.State.Characters.Reserve[0].Health;

        TrapMaps.WalkTo(run, TrapMaps.Blade.X);

        for (int slot = 0; slot < before.Count; slot += 1)
        {
            PartyMember member = run.State.Characters.Members[slot];
            int loss = Math.Max(1, member.Stats.Health * 2500 / BasisPoints.One);
            Assert.Equal(Math.Max(0, before[slot] - loss), member.Health);
        }

        Assert.Equal(reserve, run.State.Characters.Reserve[0].Health);
        Assert.True(run.State.Party.Place.IsSpent(TrapMaps.TrapOf(TrapMaps.Blade)));
        Assert.Equal(TrapRules.DamageNotice.Value, Assert.Single(run.TakeNotices()).Id.Value);
    }

    [Fact]
    public void ASpentTrapNeverFiresAgain()
    {
        // D-1229: the trap fires one time, and a step back onto it does nothing.
        Simulation run = TestParty.StartFour(Seed, TrapMaps.Hall);
        TrapMaps.WalkTo(run, TrapMaps.Blade.X);
        List<int> hurt = HealthOf(run.State.Characters.Members);
        _ = run.TakeNotices();

        HubWalks.Walk(run, StepDirection.West, 1);
        HubWalks.Walk(run, StepDirection.East, 1);

        Assert.Equal(hurt, HealthOf(run.State.Characters.Members));
        Assert.Empty(run.TakeNotices());
    }

    [Fact]
    public void ADamageTrapCanDownACharacterAndTheDownEndsEachStatus()
    {
        // D-801, D-1230: a character at 1 health goes down, and the down ends its blind.
        Simulation run = TestParty.StartEach(Seed, (slot, stored) => stored with { Health = 1, Statuses = [StatusKind.Blind] }, TestParty.FourContent, TrapMaps.Hall);

        TrapMaps.WalkTo(run, TrapMaps.Blade.X);

        foreach (PartyMember member in run.State.Characters.Members)
        {
            Assert.True(member.Down);
            Assert.Empty(member.Statuses);
        }
    }

    [Fact]
    public void AStatusTrapPutsItsStatusOnEachStandingCharacterInTheOrderOfTheStatuses()
    {
        // D-75, D-1230: the needle puts poison before the silence that the first character holds, and
        // a down character takes nothing.
        Simulation run = TestParty.StartEach(
            Seed,
            (slot, stored) => slot switch
            {
                0 => stored with { Statuses = [StatusKind.Silence] },
                1 => stored with { Health = 0 },
                _ => stored,
            },
            TestParty.FourContent,
            TrapMaps.Hall);

        HubWalks.Walk(run, StepDirection.East, TrapMaps.Needle.X - 1);

        IReadOnlyList<PartyMember> members = run.State.Characters.Members;
        Assert.Equal([StatusKind.Poison, StatusKind.Silence], members[0].Statuses);
        Assert.Empty(members[1].Statuses);
        Assert.Equal([StatusKind.Poison], members[2].Statuses);
        Assert.Contains(TrapRules.PoisonNotice.Value, NoticeIds(run));
    }

    [Fact]
    public void AnEncounterTrapStartsAFightInWhichTheEnemiesActFirst()
    {
        // D-265, D-1231: the lead stands on the alarm, the fight names the trap, and the grunt acts
        // before the first turn of a character.
        Simulation run = TestParty.Start(Seed, stored => stored, null, TrapMaps.Hall);

        HubWalks.Walk(run, StepDirection.East, TrapMaps.Alarm.X - 1);

        Battle battle = BattleRuns.BattleOf(run);
        Assert.True(battle.FromTrap);
        Assert.Equal(TrapMaps.TrapOf(TrapMaps.Alarm).Value, battle.Enemy.Value);
        Assert.Equal(TrapMaps.Alarm, run.State.Party.LeadAt);
        Assert.Null(run.State.Party.Stepping);
        Assert.True(run.State.Party.Place.IsSpent(TrapMaps.TrapOf(TrapMaps.Alarm)));
        List<BattleEventKind> kinds = BattleRuns.Kinds(run);
        Assert.Equal(BattleEventKind.Started, kinds[0]);
        Assert.Contains(kinds, kind => kind is BattleEventKind.Hit or BattleEventKind.Miss);
    }

    [Fact]
    public void AWonFightOfATrapEndsWithTheMapRunningAndTheTrapSpent()
    {
        // D-1229, D-1231: the win marks no enemy dead, and the trap never fires again.
        Simulation run = TestParty.Start(Seed, stored => stored, TestBattles.Exact, TrapMaps.Hall);
        HubWalks.Walk(run, StepDirection.East, TrapMaps.Alarm.X - 1);

        Assert.Equal(BattleOutcome.Won, BattleRuns.FightToEnd(run, Seed));
        run.Step([Intent.OfPlayer(IntentIds.WaitBattleEnd)]);

        Assert.Null(run.State.Battle);
        Assert.True(run.State.Party.Place.IsEmpty is false);
        Assert.Empty(run.State.Party.Place.Values().Dead);
        HubWalks.Walk(run, StepDirection.West, 1);
        HubWalks.Walk(run, StepDirection.East, 1);
        Assert.Null(run.State.Battle);
    }

    [Fact]
    public void AFleeEndsTheFightOfATrapAndLeavesTheTrapSpent()
    {
        // D-1231: the party can flee a trap as usual, and no grace time of a patrol follows.
        Simulation run = TestParty.Start(Seed, stored => stored, TestBattles.SureFlee, TrapMaps.Hall);
        HubWalks.Walk(run, StepDirection.East, TrapMaps.Alarm.X - 1);
        _ = BattleRuns.Kinds(run);

        run.Step([Intent.OfPlayer(IntentIds.BattleFlee)]);
        Assert.Equal(BattleOutcome.Fled, BattleRuns.BattleOf(run).Outcome);
        run.Step([Intent.OfPlayer(IntentIds.WaitBattleEnd)]);

        Assert.Null(run.State.Battle);
        Assert.True(run.State.Party.Place.IsSpent(TrapMaps.TrapOf(TrapMaps.Alarm)));
    }

    [Fact]
    public void TheFightOfATrapResumesFromItsSnapshot()
    {
        // D-1231, T-2: the snapshot of the fight names the trap, and the resume finds the spent trap.
        Simulation run = TestParty.Start(Seed, stored => stored, null, TrapMaps.Hall);
        HubWalks.Walk(run, StepDirection.East, TrapMaps.Alarm.X - 1);
        RunSnapshot snapshot = run.Snapshot();

        Simulation resumed = Simulation.Resume(Seed, snapshot, TrapMaps.Hall, TestBattles.Content, TestBattles.Notices, TestBattles.Story, DebugIntentHandlers.None);

        Assert.True(BattleRuns.BattleOf(resumed).FromTrap);
        Assert.Equal(run.StateHash(), resumed.StateHash());
    }

    [Fact]
    public void ASnapshotOfTheFightOfATrapThatIsNotSpentIsRefused()
    {
        // T-2: a trap that fired is spent, so a fight of an armed trap describes no state of a run.
        Simulation run = TestParty.Start(Seed, stored => stored, null, TrapMaps.Hall);
        HubWalks.Walk(run, StepDirection.East, TrapMaps.Alarm.X - 1);
        RunSnapshot snapshot = run.Snapshot();
        RunSnapshot armed = snapshot with { Places = [] };

        ArgumentException error = Assert.Throws<ArgumentException>(() => Simulation.Resume(Seed, armed, TrapMaps.Hall, TestBattles.Content, TestBattles.Notices, TestBattles.Story, DebugIntentHandlers.None));
        Assert.Contains("D-1231", error.Message, StringComparison.Ordinal);
    }

    [Fact]
    public void ATrapShowsAtTwoStepsOrLessWhileAStandingCharacterCarriesATheftDrill()
    {
        // D-1228: no trap shows with no drill. With the drill, the blade at 2 steps shows, and the
        // needle at 4 steps does not.
        Simulation run = TestParty.Start(Seed, stored => stored, null, TrapMaps.Hall);
        MapThing blade = TrapMaps.Hall.TrapAt(TrapMaps.Blade)!;
        MapThing needle = TrapMaps.Hall.TrapAt(TrapMaps.Needle)!;
        Assert.False(TrapRules.Shows(run.State, blade));

        CarryPilfer(run);

        Assert.True(TrapRules.Shows(run.State, blade));
        Assert.False(TrapRules.Shows(run.State, needle));
        Assert.Equal(2, MapRules.StepsApart(run.State.Party.LeadAt, blade.At));
    }

    [Fact]
    public void ADownedThiefShowsNoTrap()
    {
        // D-386, D-1228: the drill of a character who fights and stands shows a trap.
        Simulation run = TestParty.Start(Seed, stored => stored, null, TrapMaps.Hall);
        CarryPilfer(run);
        RunSnapshot snapshot = run.Snapshot();
        RunSnapshot downed = snapshot with { Characters = snapshot.Characters! with { Characters = [snapshot.Characters.Characters[0] with { Health = 0 }] } };

        Simulation resumed = Simulation.Resume(Seed, downed, TrapMaps.Hall, run.State.BattleContent, TestBattles.Notices, TestBattles.Story, DebugIntentHandlers.None);

        Assert.False(TrapRules.Shows(resumed.State, TrapMaps.Hall.TrapAt(TrapMaps.Blade)!));
    }

    [Fact]
    public void AConfirmDisarmsATrapThatShowsAndTheStepOntoItThenDoesNothing()
    {
        // Exit test 2 of PR-64 (D-1228): the lead faces the blade and confirms, and the blade is spent.
        Simulation run = TestParty.Start(Seed, stored => stored, null, TrapMaps.Hall);
        CarryPilfer(run);
        HubWalks.Walk(run, StepDirection.East, 1);
        List<int> before = HealthOf(run.State.Characters.Members);

        HubWalks.Confirm(run);

        Assert.True(run.State.Party.Place.IsSpent(TrapMaps.TrapOf(TrapMaps.Blade)));
        Assert.Equal(TrapRules.DisarmNotice.Value, Assert.Single(run.TakeNotices()).Id.Value);
        HubWalks.Walk(run, StepDirection.East, 1);
        Assert.Equal(before, HealthOf(run.State.Characters.Members));
        Assert.Empty(run.TakeNotices());
    }

    [Fact]
    public void AConfirmAtAHiddenTrapDisarmsNothing()
    {
        // D-1228: with no Theft drill, the blade stays hidden and armed.
        Simulation run = TestParty.Start(Seed, stored => stored, null, TrapMaps.Hall);
        HubWalks.Walk(run, StepDirection.East, 1);

        HubWalks.Confirm(run);

        Assert.False(run.State.Party.Place.IsSpent(TrapMaps.TrapOf(TrapMaps.Blade)));
        Assert.Empty(run.TakeNotices());
    }

    [Fact]
    public void ATheftDrillShowsAndDisarmsTheTrapOfTheFixtureDungeon()
    {
        // Exit test 2 of PR-64 (D-386, D-1228): the lead stands north of the pit of the fixture
        // dungeon with a Theft drill, and a confirm disarms it.
        GameMap dungeon = PartsMaps.FixtureDungeonToRoom;
        MapThing pit = dungeon.TrapAt(new TilePoint(8, 18))!;
        Simulation start = Simulation.Start(Seed, MapSet.Of([dungeon, TestMaps.Room]), dungeon.Id, TestBattles.Content, TestBattles.Notices, TestBattles.Story, DebugIntentHandlers.None);
        RunSnapshot snapshot = start.Snapshot();
        RunSnapshot north = snapshot with { Map = PlaceLead(snapshot.Map!, new TilePoint(8, 17), StepDirection.South) };
        Simulation run = Simulation.Resume(Seed, north, MapSet.Of([dungeon, TestMaps.Room]), TestBattles.Content, TestBattles.Notices, TestBattles.Story, DebugIntentHandlers.None);
        Assert.False(TrapRules.Shows(run.State, pit));
        CarryPilfer(run);

        Assert.True(TrapRules.Shows(run.State, pit));
        HubWalks.Confirm(run);

        Assert.True(run.State.Party.Place.IsSpent(pit.Id));
        Assert.False(TrapRules.Shows(run.State, pit));
    }

    [Fact]
    public void AReopenArmsEachSpentTrapAgain()
    {
        // D-555, D-1229: a story event that reopens the place arms each spent trap again.
        var place = new PlaceState(TrapMaps.Hall.Id);
        place.Spend(TrapMaps.TrapOf(TrapMaps.Blade));

        _ = place.Reopen();

        Assert.False(place.IsSpent(TrapMaps.TrapOf(TrapMaps.Blade)));
        Assert.True(place.IsEmpty);
    }

    [Fact]
    public void ATrapSpentTwoTimesIsAFaultOfTheRules()
    {
        // T-2: a spent trap never fires, so a second spend names the trap.
        var place = new PlaceState(TrapMaps.Hall.Id);
        place.Spend(TrapMaps.TrapOf(TrapMaps.Blade));

        InvalidOperationException error = Assert.Throws<InvalidOperationException>(() => place.Spend(TrapMaps.TrapOf(TrapMaps.Blade)));
        Assert.Contains("trap.test_hall_blade", error.Message, StringComparison.Ordinal);
    }

    [Fact]
    public void ThePropertyOfTheTrapsHoldsOverOneThousandSeeds()
    {
        // Exit test 1 of PR-64 (D-1226 to D-1231): on each seed, a walk over the hall fires each trap
        // one time, in order, and each harm lands as its rule says. The seed moves the fight alone.
        for (ulong seed = 0; seed < 1000; seed += 1)
        {
            Simulation run = TestParty.StartFour(seed, TrapMaps.Hall);
            List<int> full = FullOf(run.State.Characters.Members);
            TrapMaps.WalkTo(run, TrapMaps.Dust.X);

            IReadOnlyList<PartyMember> members = run.State.Characters.Members;
            for (int slot = 0; slot < members.Count; slot += 1)
            {
                int expected = Math.Max(0, full[slot] - Math.Max(1, full[slot] * 2500 / BasisPoints.One));
                Assert.True(expected == members[slot].Health, $"Seed {seed}: slot {slot} holds {members[slot].Health} health, and the blade leaves {expected}.");
                Assert.True(members[slot].Statuses.Count == 2, $"Seed {seed}: slot {slot} holds {members[slot].Statuses.Count} statuses, and the needle and the dust give 2.");
            }

            HubWalks.Walk(run, StepDirection.East, 2);
            Battle battle = run.State.Battle ?? throw new InvalidOperationException($"Seed {seed}: the alarm started no fight.");
            Assert.True(battle.FromTrap, $"Seed {seed}: the fight of the alarm names '{battle.Enemy.Value}'.");
            Assert.True(run.State.Party.Place.Values().Spent.Count == 4, $"Seed {seed}: the memory holds {run.State.Party.Place.Values().Spent.Count} spent traps, and the walk fired 4.");
        }
    }

    /// <summary>Puts the Theft drill of the tests in the second slot of Marrek from the menu (D-1050).</summary>
    private static void CarryPilfer(Simulation run)
    {
        run.State.Characters.AddLesson(run.State.BattleContent.Lessons.Lesson(Pilfer), run.State.Context("test"));
        run.Step([Intent.OfPlayer(IntentIds.OpenMenu)]);
        run.Step([Intent.OfLessonSwap(0, 1, Pilfer)]);
        run.Step([Intent.OfPlayer(IntentIds.CloseMenu)]);
        Assert.True(DoorRules.CarriesTheft(run.State));
    }

    /// <summary>Moves the lead of a map snapshot to one tile, facing one way, and marks the tile walked.</summary>
    private static MapSnapshot PlaceLead(MapSnapshot map, TilePoint at, StepDirection facing)
    {
        List<string> walked = [.. map.Walked];
        char[] row = walked[at.Y].ToCharArray();
        row[at.X] = 'x';
        walked[at.Y] = new string(row);
        return map with { LeadX = at.X, LeadY = at.Y, Facing = facing, Walked = walked };
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

    private static List<int> FullOf(IReadOnlyList<PartyMember> members)
    {
        List<int> full = [];
        foreach (PartyMember member in members)
        {
            full.Add(member.Stats.Health);
        }

        return full;
    }

    private static List<string> NoticeIds(Simulation run)
    {
        List<string> ids = [];
        foreach (Core.Notices.PostedNotice notice in run.TakeNotices())
        {
            ids.Add(notice.Id.Value);
        }

        return ids;
    }
}
