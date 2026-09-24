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
/// The seed loops of the battle core: the exit tests 1, 2, 5, and 8 of PR-9. Each failure
/// names its seed (T-3).
/// </summary>
public sealed class BattlePropertyTests
{
    /// <summary>The count of seeds of each loop (exit test 1 of PR-9).</summary>
    private const ulong SeedCount = 1000;

    /// <summary>The most turns of a character in one battle of a loop.</summary>
    private const int TurnLimit = 2000;

    private static readonly string[] Groups = ["group.one", "group.test_pair", "group.test_elite", "group.fixture_elite", "group.test_full", "group.test_boss"];

    private static readonly ContentId Draught = ContentId.Parse("item.fixture_draught", "BattlePropertyTests", "item");

    [Fact]
    public void TheTimelineNeverStallsOverOneThousandSeeds()
    {
        // Exit test 1 of PR-9 (D-376): every battle of every action mix ends, the timeline
        // never runs back, and no combatant on the field waits behind the time of the timeline.
        for (ulong seed = 0; seed < SeedCount; seed += 1)
        {
            BattleContent content = TestBattles.WithParty((int)(seed % 3) + 1);
            Simulation run = BattleRuns.IntoBattle(seed, Groups[seed % (ulong)Groups.Length], content);
            Battle battle = BattleRuns.BattleOf(run);
            long now = battle.Now;
            int turn = 0;

            while (battle.Outcome == BattleOutcome.Running)
            {
                Assert.True(turn < TurnLimit, $"Seed {seed}: the battle ran past {TurnLimit} turns (D-376).");
                Combatant? next = battle.Next();
                Assert.True(next is not null && next.Side == BattleSide.Party, $"Seed {seed}: a running battle gave no turn to a character at turn {turn} (D-532).");
                run.Step([ChoiceOf(run, seed, turn)]);

                Assert.True(battle.Now >= now, $"Seed {seed}: the timeline ran back from {now} to {battle.Now} (D-376).");
                now = battle.Now;
                foreach (Combatant combatant in battle.All())
                {
                    Assert.True(
                        combatant.Place != CombatantPlace.Field || combatant.ReadyAt >= battle.Now,
                        $"Seed {seed}: {combatant.Target.Describe()} waits at {combatant.ReadyAt}, behind the timeline at {battle.Now}.");
                }

                turn += 1;
            }
        }
    }

    [Fact]
    public void MeleeNeverReachesABackRowWhileItsFrontRowStandsOverOneThousandSeeds()
    {
        // Exit test 2 of PR-9 (D-377). The third character stands in the back row and never
        // steps, so an enemy strike at it is legal only when both front characters are down.
        for (ulong seed = 0; seed < SeedCount; seed += 1)
        {
            Simulation run = BattleRuns.IntoBattle(seed, Groups[seed % 5], TestBattles.WithParty(3));
            Battle battle = BattleRuns.BattleOf(run);
            SortedSet<int> down = [];

            for (int turn = 0; turn < TurnLimit && battle.Outcome == BattleOutcome.Running; turn += 1)
            {
                AssertNoBackRowTarget(battle, seed);
                run.Step([BattleRuns.AttackFirst(run)]);
                foreach (BattleEvent battleEvent in run.TakeBattleEvents())
                {
                    ReadStrike(battleEvent, down, seed);
                }
            }
        }
    }

    [Fact]
    public void AWipeEndsTheBattleForAPartyOfOneTwoOrThreeOverOneThousandSeeds()
    {
        // Exit test 5 of PR-9 (D-336, D-397): when every character who fights goes down, the
        // party wipes, whatever the size of the party. The fixture holds three characters, so
        // a party of one or two leaves a healthy character outside the battle, and no one steps
        // in. The wait intent refuses a wipe, because a wipe reloads (D-776).
        for (ulong seed = 0; seed < SeedCount; seed += 1)
        {
            int size = (int)(seed % 3) + 1;
            Simulation run = BattleRuns.IntoBattle(seed, "group.test_full", TestBattles.WithParty(size));
            Battle battle = BattleRuns.BattleOf(run);

            for (int turn = 0; turn < TurnLimit && battle.Outcome == BattleOutcome.Running; turn += 1)
            {
                run.Step([Intent.OfPlayer(IntentIds.BattleDefend)]);
            }

            Assert.True(battle.Outcome == BattleOutcome.Wiped, $"Seed {seed}: a party of {size} that only defends ended as '{Battle.OutcomeName(battle.Outcome)}'.");
            Assert.Equal(size, battle.Party.Count);
            foreach (PartyMember member in run.State.Characters.Members)
            {
                Assert.True(member.Down, $"Seed {seed}: '{member.Record.Id.Value}' stands after a wipe.");
            }

            Assert.Throws<SimulationException>(() => run.Step([Intent.OfPlayer(IntentIds.WaitBattleEnd)]));
        }
    }

    [Fact]
    public void NoMapSystemMovesDuringABattleOverOneThousandSeeds()
    {
        // Exit test 8 of PR-9 (D-531): the walker, the grace time, the mark, and the party all
        // stand still while the battle waits for a character, whatever the intents of the map.
        for (ulong seed = 0; seed < SeedCount; seed += 1)
        {
            Simulation run = Simulation.Start(seed, BattleRuns.MapWithWalker("group.test_pair"), TestBattles.Content, TestBattles.Notices, TestBattles.Story, DebugIntentHandlers.None);
            for (int tick = 0; tick < (int)(seed % 40); tick += 1)
            {
                run.Step([]);
            }

            run.Step([Intent.OfPlayer(IntentIds.MoveEast)]);
            Assert.NotNull(run.State.Battle);
            MapSnapshot before = run.Snapshot().Map!;

            for (int tick = 0; tick < 120; tick += 1)
            {
                run.Step([Intent.OfPlayer(tick % 2 == 0 ? IntentIds.MoveSouth : IntentIds.MoveWest)]);
            }

            MapSnapshot after = run.Snapshot().Map!;
            Assert.True(before.LeadX == after.LeadX && before.LeadY == after.LeadY, $"Seed {seed}: the party moved during a battle (D-531).");
            Assert.True(before.Walked.Count == after.Walked.Count, $"Seed {seed}: the walked tiles changed during a battle (D-531).");
            Assert.Equal(before.Enemies, after.Enemies);
            Assert.Equal(before.Mark, after.Mark);
        }
    }

    [Fact]
    public void AReplayOfAFightGivesTheSameStateHash()
    {
        // Exit test 7 of PR-9 on this leg. The identity set holds the same kind of run, and the
        // replay-identity job compares it on every leg (G-5, D-504).
        for (ulong seed = 0; seed < 50; seed += 1)
        {
            Simulation run = Simulation.Start(seed, BattleRuns.Map("group.test_pair"), TestBattles.Content, TestBattles.Notices, TestBattles.Story, DebugIntentHandlers.None);
            RunRecorder recorder = new(RunHeader.ForThisBuild("0123456789abcdef", seed), run.Snapshot());
            for (int turn = 0; turn < 60; turn += 1)
            {
                IReadOnlyList<Intent> intents = IntentsOf(run, seed, turn);
                run.Step(intents);
                recorder.Step(run.Tick, intents);
            }

            RunState replayed = RunReplay.Play(
                RunRecordText.Read(RunRecordText.Write(recorder.Build())),
                "0123456789abcdef",
                BattleRuns.Map("group.test_pair"),
                TestBattles.Content,
                TestBattles.Notices,
                TestBattles.Story,
                DebugIntentHandlers.None);

            Assert.True(run.StateHash() == replayed.StateHash(), $"Seed {seed}: the replay of a fight gave another state hash (G-5).");
        }
    }

    /// <summary>
    /// Gives the choice of a character on one turn: each action of D-755 and D-359 to D-382 in
    /// a mix that the seed and the turn set. A choice that the rules refuse becomes an attack.
    /// </summary>
    private static Intent ChoiceOf(Simulation run, ulong seed, int turn)
    {
        Battle battle = BattleRuns.BattleOf(run);
        ulong mix = (seed * 31) + (ulong)turn;
        IReadOnlyList<Combatant> reach = battle.MeleeTargets(BattleSide.Enemy);
        Intent attack = Intent.OfPlayer(IntentIds.BattleAttack, reach[(int)(mix % (ulong)reach.Count)].Target, null);
        Intent chosen = (mix % 7) switch
        {
            0 => Intent.OfPlayer(IntentIds.BattleDefend),
            1 => Intent.OfPlayer(IntentIds.BattleStep),
            2 => Intent.OfPlayer(IntentIds.BattleItem, battle.Next()!.Target, Draught),
            3 => Intent.OfPlayer(IntentIds.BattleFlee),
            _ => attack,
        };

        BattleChoice choice = chosen.Action.Value switch
        {
            "intent.battle_defend" => new BattleChoice(BattleAction.Defend, null, null),
            "intent.battle_step" => new BattleChoice(BattleAction.Step, null, null),
            "intent.battle_item" => new BattleChoice(BattleAction.Item, chosen.Target, chosen.Item),
            "intent.battle_flee" => new BattleChoice(BattleAction.Flee, null, null),
            _ => new BattleChoice(BattleAction.Attack, chosen.Target, null),
        };

        return BattleTurns.RefusalOf(run.State, choice) is null ? chosen : attack;
    }

    /// <summary>Gives the intents of one tick of a recorded fight: a step, a choice, or the wait intent.</summary>
    internal static IReadOnlyList<Intent> IntentsOf(Simulation run, ulong seed, int turn)
    {
        if (run.State.Battle is not Battle battle)
        {
            return run.State.Party.Patrols.Encounter is null && turn == 0 ? [Intent.OfPlayer(IntentIds.MoveEast)] : [];
        }

        if (battle.Outcome == BattleOutcome.Won || battle.Outcome == BattleOutcome.Fled)
        {
            return [Intent.OfPlayer(IntentIds.WaitBattleEnd)];
        }

        return battle.Outcome == BattleOutcome.Running ? [ChoiceOf(run, seed, turn)] : [];
    }

    private static void AssertNoBackRowTarget(Battle battle, ulong seed)
    {
        foreach (Combatant enemy in battle.Enemies)
        {
            bool frontStands = false;
            foreach (Combatant other in battle.Enemies)
            {
                frontStands |= other.Place == CombatantPlace.Field && other.Row == BattleRow.Front;
            }

            if (frontStands && enemy.Place == CombatantPlace.Field && enemy.Row == BattleRow.Back)
            {
                Assert.DoesNotContain(enemy, battle.MeleeTargets(BattleSide.Enemy));
            }
        }

        Assert.True(battle.MeleeTargets(BattleSide.Enemy).Count > 0 || battle.Outcome != BattleOutcome.Running, $"Seed {seed}: melee reaches no enemy of a running battle.");
    }

    private static void ReadStrike(BattleEvent battleEvent, SortedSet<int> down, ulong seed)
    {
        if (battleEvent.Kind == BattleEventKind.Down && battleEvent.Actor.Side == BattleSide.Party)
        {
            down.Add(battleEvent.Actor.Slot);
            return;
        }

        bool strike = battleEvent.Kind == BattleEventKind.Hit || battleEvent.Kind == BattleEventKind.Miss;
        if (strike && battleEvent.Actor.Side == BattleSide.Enemy && battleEvent.Target?.Slot == 2)
        {
            Assert.True(
                down.Contains(0) && down.Contains(1),
                $"Seed {seed}: an enemy struck the back row while the front row stood (D-377).");
        }
    }
}
