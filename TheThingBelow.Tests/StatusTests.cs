using System;
using System.Collections.Generic;
using TheThingBelow.Core;
using TheThingBelow.Core.Battles;
using TheThingBelow.Core.Logging;
using TheThingBelow.Core.Maps;
using TheThingBelow.Core.Runs;
using Xunit;

namespace TheThingBelow.Tests;

/// <summary>
/// The ten statuses of a fight: the start, the end, the shares, sleep, stun, blind, the
/// immune list, a down, and the three that last past the fight (PR-66, D-75, D-390, D-798 to
/// D-810).
/// </summary>
/// <remarks>
/// Each exact test reads group.one with the rolls of `TestBattles.Exact`. Marrek starts
/// behind the grunt, so his turn is open at tick 0, and the grunt acts at tick 111.
/// </remarks>
public sealed class StatusTests
{
    private const ulong Seed = 20260921;

    private static readonly BattleTarget Marrek = new(BattleSide.Party, 0);

    private static readonly BattleTarget Grunt = new(BattleSide.Enemy, 0);

    private static readonly StatusKind[] Timed =
        [StatusKind.Sleep, StatusKind.Slow, StatusKind.Haste, StatusKind.Stun, StatusKind.Bleed, StatusKind.Regen, StatusKind.Shell];

    [Fact]
    public void EachTimedStatusHoldsUntilTheTimelineReachesItsEndOverOneThousandSeeds()
    {
        // Exit test 2: a status holds while the timeline stands before its end, and ends at
        // the first turn at or past it (D-798). Sleep can also end at a hit (D-802).
        for (ulong seed = 0; seed < 1000; seed += 1)
        {
            StatusKind status = Timed[(int)(seed % (ulong)Timed.Length)];
            Simulation run = BattleRuns.IntoBattle(seed, "group.one");
            Battle battle = BattleRuns.BattleOf(run);
            long ends = battle.Now + TicksOf(status);
            Give(run, Grunt, status);
            Assert.True(battle.Enemies[0].Statuses.Holds(status), $"Seed {seed}: '{Statuses.NameOf(status)}' did not land.");

            List<BattleEvent> events = [];
            while (battle.Outcome == BattleOutcome.Running && battle.Now < ends)
            {
                run.Step([Intent.OfPlayer(IntentIds.BattleDefend)]);
                events.AddRange(Events(run));
                bool holds = battle.Enemies[0].Statuses.Holds(status);
                bool woken = status == StatusKind.Sleep && HitOf(events, Grunt);
                Assert.True(
                    holds == (battle.Now < ends && !woken),
                    $"Seed {seed}: '{Statuses.NameOf(status)}' holds {holds} at tick {battle.Now}, and it ends at {ends}.");
            }

            Assert.True(battle.Outcome == BattleOutcome.Running, $"Seed {seed}: the fight ended before '{Statuses.NameOf(status)}' did.");
            Assert.True(
                events.Exists(e => e.Kind == BattleEventKind.StatusOff && e.Status == status && e.Actor == Grunt),
                $"Seed {seed}: no event names the end of '{Statuses.NameOf(status)}'.");
        }
    }

    [Fact]
    public void AStatusThatEndsWithItsFightIsAbsentAfterTheFight()
    {
        // Exit test 3, D-390: sleep, slow, haste, stun, bleed, regen, and shell end with the
        // fight, and the next fight starts with none of them.
        Simulation run = BattleRuns.IntoBattle(Seed, "group.one", TestBattles.Exact);
        Wound(run);
        foreach (StatusKind status in new[] { StatusKind.Haste, StatusKind.Regen, StatusKind.Shell })
        {
            Give(run, Marrek, status);
        }

        Assert.Equal(BattleOutcome.Won, BattleRuns.FightToEnd(run, Seed));
        run.Step([Intent.OfPlayer(IntentIds.WaitBattleEnd)]);

        Assert.Empty(run.State.Characters.Members[0].Statuses);
        Assert.True(NextFight(run).Party[0].Statuses.Empty);
    }

    [Fact]
    public void PoisonBlindAndSilenceRemainAfterTheFight()
    {
        // Exit test 4, D-390, D-792: the three follow the character out of the fight, into
        // the snapshot, and into the next fight, with no end.
        Simulation run = BattleRuns.IntoBattle(Seed, "group.one", TestBattles.Exact);
        Wound(run);
        foreach (StatusKind status in new[] { StatusKind.Silence, StatusKind.Poison, StatusKind.Blind, StatusKind.Haste })
        {
            Give(run, Marrek, status);
        }

        Assert.Equal(BattleOutcome.Won, BattleRuns.FightToEnd(run, Seed));
        run.Step([Intent.OfPlayer(IntentIds.WaitBattleEnd)]);

        StatusKind[] lasting = [StatusKind.Poison, StatusKind.Blind, StatusKind.Silence];
        Assert.Equal(lasting, run.State.Characters.Members[0].Statuses);
        Assert.Equal(lasting, run.Snapshot().Characters!.Characters[0].Statuses);
        Battle next = NextFight(run);
        Assert.Equal(
            [new StatusValues(StatusKind.Poison, null), new StatusValues(StatusKind.Blind, null), new StatusValues(StatusKind.Silence, null)],
            next.Party[0].Statuses.Values());
    }

    [Fact]
    public void PoisonAndBleedTakeTheirShareAndRegenHealsAtTheStartOfATurn()
    {
        // D-799, D-803, D-808: 5% and 10% of 60 is 3 and 6. Regen heals 10%, up to full.
        Simulation run = BattleRuns.IntoBattle(Seed, "group.one", TestBattles.Exact);
        Give(run, Marrek, StatusKind.Poison);
        Give(run, Marrek, StatusKind.Bleed);
        _ = run.TakeBattleEvents();

        run.Step([Intent.OfPlayer(IntentIds.BattleDefend)]);

        Battle battle = BattleRuns.BattleOf(run);
        Assert.Equal(60, battle.Now);
        Assert.Equal(60 - 3 - 6, battle.Party[0].Health);
        Give(run, Marrek, StatusKind.Regen);
        run.Step([Intent.OfPlayer(IntentIds.BattleDefend)]);
        // The grunt hits into the defend for 3 at tick 111 (D-755), then the turn of Marrek
        // begins at 120.
        Assert.Equal(51 - 3 - 3 - 6 + 6, battle.Party[0].Health);
        Assert.Contains(Events(run), e => e.Kind == BattleEventKind.StatusHeal && e.Amount == 6 && e.Status == StatusKind.Regen);
    }

    [Fact]
    public void PoisonDownsItsHolderAndTakesTheTurn()
    {
        // D-799: a share that takes the last health downs the holder before it acts, and a
        // down takes every status off it (D-801).
        Simulation run = BattleRuns.IntoBattle(Seed, "group.one", TestBattles.Exact);
        Battle battle = BattleRuns.BattleOf(run);
        Wound(run);
        Give(run, Grunt, StatusKind.Poison);
        Give(run, Grunt, StatusKind.Bleed);
        _ = run.TakeBattleEvents();

        for (int turn = 0; turn < 20 && battle.Outcome == BattleOutcome.Running; turn += 1)
        {
            run.Step([Intent.OfPlayer(IntentIds.BattleDefend)]);
        }

        Assert.Equal(BattleOutcome.Won, battle.Outcome);
        Assert.True(battle.Enemies[0].Statuses.Empty);
        List<BattleEvent> events = Events(run);
        int down = events.FindIndex(e => e.Kind == BattleEventKind.Down && e.Actor == Grunt);
        Assert.True(down > 0, "The grunt never went down.");
        Assert.Equal((BattleEventKind.StatusHurt, Grunt), (events[down - 1].Kind, events[down - 1].Actor));
        Assert.DoesNotContain(events.GetRange(down, events.Count - down), e => e.Kind == BattleEventKind.Hit && e.Actor == Grunt);
    }

    [Fact]
    public void ASleeperPassesItsTurnAndAStrikeWakesIt()
    {
        // D-802: the grunt sleeps at its turn of 111, one attack push goes by, and the hit of
        // Marrek wakes it. The share of poison does not wake it.
        Simulation run = BattleRuns.IntoBattle(Seed, "group.one", TestBattles.Exact);
        Battle battle = BattleRuns.BattleOf(run);
        Give(run, Grunt, StatusKind.Sleep);
        Give(run, Grunt, StatusKind.Poison);
        _ = run.TakeBattleEvents();

        run.Step([Intent.OfPlayer(IntentIds.BattleDefend)]);
        run.Step([Intent.OfPlayer(IntentIds.BattleDefend)]);

        Assert.Equal(111 + 111, battle.Enemies[0].ReadyAt);
        Assert.True(battle.Enemies[0].Statuses.Holds(StatusKind.Sleep));
        List<BattleEvent> events = Events(run);
        Assert.Contains(events, e => e.Kind == BattleEventKind.StatusHurt && e.Actor == Grunt);
        Assert.Contains(events, e => e.Kind == BattleEventKind.Asleep && e.Actor == Grunt);
        Assert.Equal(60, battle.Party[0].Health);

        run.Step([BattleRuns.AttackFirst(run)]);

        Assert.False(battle.Enemies[0].Statuses.Holds(StatusKind.Sleep));
    }

    [Fact]
    public void AStunPushesTheNextTurnOnceAndASecondStunResetsTheEndAlone()
    {
        // D-802, D-810: the first stun moves the turn of the grunt from 111 to 161, and a
        // second stun inside its ticks moves the end and not the turn.
        Simulation run = BattleRuns.IntoBattle(Seed, "group.one", TestBattles.Exact);
        Battle battle = BattleRuns.BattleOf(run);

        Give(run, Grunt, StatusKind.Stun);
        Assert.Equal(161, battle.Enemies[0].ReadyAt);
        Give(run, Grunt, StatusKind.Stun);

        Assert.Equal(161, battle.Enemies[0].ReadyAt);
        Assert.Equal(2, Events(run).FindAll(e => e.Kind == BattleEventKind.StatusOn && e.Status == StatusKind.Stun).Count);
    }

    [Fact]
    public void AStunOnTheCharacterWhoseTurnIsOpenLetsTheNextTurnRun()
    {
        // D-802: a stun moves Marrek from 0 to 50, and no other turn comes before 50, so his
        // turn opens again at 50 with the stun still on him.
        Simulation run = BattleRuns.IntoBattle(Seed, "group.one", TestBattles.Exact);
        Battle battle = BattleRuns.BattleOf(run);

        Give(run, Marrek, StatusKind.Stun);

        Assert.Equal(50, battle.Now);
        Assert.Same(battle.Party[0], battle.Next());
        Assert.False(battle.Party[0].Statuses.Holds(StatusKind.Stun));
    }

    [Fact]
    public void BlindAddsItsRatePastTheCeilingOfTheMissChance()
    {
        // D-806: the exact rules clamp every miss chance to 0, and blind adds 3000 after the
        // clamp. Over 1000 seeds, about 300 attacks of a blind Marrek miss, and none of his
        // attacks miss with no blind.
        int blindMisses = 0;
        for (ulong seed = 0; seed < 1000; seed += 1)
        {
            blindMisses += FirstAttackMisses(seed, blind: true) ? 1 : 0;
            Assert.False(FirstAttackMisses(seed, blind: false), $"Seed {seed}: an attack with no blind missed, and the chance is 0.");
        }

        Assert.InRange(blindMisses, 230, 370);
    }

    [Fact]
    public void AnImmuneEnemyTakesNoStatusAndTheMoveDrawsNoRoll()
    {
        // D-805, D-807: a grunt immune to sleep takes none, and a move of sleep draws the
        // same rolls as a move with no status.
        BattleContent content = TestBattles.WithGrunt(Element.Fire, Affinity.Normal, [StatusKind.Sleep], exact: false);
        Simulation immune = BattleRuns.IntoBattle(Seed, "group.one", content);
        Simulation plain = BattleRuns.IntoBattle(Seed, "group.one", content);
        Give(immune, Grunt, StatusKind.Sleep);
        Assert.False(BattleRuns.BattleOf(immune).Enemies[0].Statuses.Holds(StatusKind.Sleep));
        Assert.Contains(Events(immune), e => e.Kind == BattleEventKind.Immune && e.Status == StatusKind.Sleep);

        BattleTurns.StrikeWith(immune.State, new BattleMove(100, 10000, null, new StatusChance(StatusKind.Sleep, 5000)), Grunt, immune.State.Context("test"), []);
        BattleTurns.StrikeWith(plain.State, new BattleMove(100, 10000, null, null), Grunt, plain.State.Context("test"), []);

        Assert.Equal(plain.StateHash(), immune.StateHash());
    }

    [Fact]
    public void HasteOnASlowedHolderCancelsBoth()
    {
        // D-800: the opposite goes, and the new status does not land.
        Simulation run = BattleRuns.IntoBattle(Seed, "group.one", TestBattles.Exact);
        Combatant grunt = BattleRuns.BattleOf(run).Enemies[0];

        Give(run, Grunt, StatusKind.Slow);
        Assert.Equal(15000, grunt.PushRate);
        Give(run, Grunt, StatusKind.Haste);

        Assert.False(grunt.Statuses.Holds(StatusKind.Slow));
        Assert.False(grunt.Statuses.Holds(StatusKind.Haste));
        Assert.Equal(10000, grunt.PushRate);
    }

    [Fact]
    public void AStatusForACombatantOffTheFieldIsAnError()
    {
        // T-2, D-801: a status holds on the field alone.
        Simulation run = BattleRuns.IntoBattle(Seed, "group.test_full", TestBattles.Exact);

        SimulationException error = Assert.Throws<SimulationException>(
            () => Give(run, new BattleTarget(BattleSide.Enemy, 6), StatusKind.Poison));

        Assert.Contains("not on the field", error.Message, StringComparison.Ordinal);
    }

    [Fact]
    public void AStatusEventNamesItsStatusAndAHitNamesItsAffinity()
    {
        Assert.Equal("status on: enemy 0 (poison)", new BattleEvent(BattleEventKind.StatusOn, Grunt, null, 0, StatusKind.Poison).Describe());
        Assert.Equal("hit: party 0 at enemy 0 for 16 (weak)", new BattleEvent(BattleEventKind.Hit, Marrek, Grunt, 16, null, Affinity.Weak).Describe());
    }

    private static void Give(Simulation run, BattleTarget target, StatusKind status) =>
        BattleTurns.GiveStatus(run.State, target, status, run.State.Context("test"), []);

    private static List<BattleEvent> Events(Simulation run) => [.. run.TakeBattleEvents()];

    private static bool HitOf(List<BattleEvent> events, BattleTarget target) =>
        events.Exists(e => e.Kind == BattleEventKind.Hit && e.Target == target);

    private static bool FirstAttackMisses(ulong seed, bool blind)
    {
        Simulation run = BattleRuns.IntoBattle(seed, "group.one", TestBattles.Exact);
        if (blind)
        {
            Give(run, Marrek, StatusKind.Blind);
        }

        _ = run.TakeBattleEvents();
        run.Step([BattleRuns.AttackFirst(run)]);
        return Events(run).Exists(e => e.Kind == BattleEventKind.Miss && e.Actor == Marrek);
    }

    /// <summary>Starts a second fight of the same party against the same group, as the next encounter of the map does (D-792).</summary>
    private static Battle NextFight(Simulation run)
    {
        Patrol guard = BattleRuns.Map("group.one").Patrols[0];
        MapEncounter encounter = new(guard.Id, guard.Group, EncounterSide.None);
        return Battle.Start(TestBattles.Exact, encounter, run.State.Characters, run.State.Context("test"));
    }

    /// <summary>Attacks two times, so the grunt holds 8 of its 30, and the next hit ends the fight.</summary>
    private static void Wound(Simulation run)
    {
        run.Step([BattleRuns.AttackFirst(run)]);
        run.Step([BattleRuns.AttackFirst(run)]);
        Assert.Equal(8, BattleRuns.BattleOf(run).Enemies[0].Health);
    }

    private static int TicksOf(StatusKind status) => status switch
    {
        StatusKind.Sleep => 300,
        StatusKind.Bleed => 300,
        StatusKind.Stun => 50,
        _ => 400,
    };
}
