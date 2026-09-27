using System;
using System.Collections.Generic;
using TheThingBelow.Core;
using TheThingBelow.Core.Battles;
using TheThingBelow.Core.Content;
using TheThingBelow.Core.Logging;
using TheThingBelow.Core.Runs;
using Xunit;

namespace TheThingBelow.Tests;

/// <summary>
/// The rules of ability power: the cost of a drill, the regain of a fall, and the regain of a
/// basic attack that hits (D-1197, D-1198, D-1210, D-1213). Each exit test of PR-107 in
/// `phase-2-first-playable.md` names its test here.
/// </summary>
public sealed class AbilityPowerTests
{
    private const ulong Seed = 12;

    private static readonly ContentId Hew = ContentId.Parse("lesson.fixture_hew", "test", "id");
    private static readonly BattleTarget FirstEnemy = new(BattleSide.Enemy, 0);

    [Fact]
    public void ADrillWithTooLittleApRefusesTheChoice()
    {
        // Exit test 1 of PR-107: Hew costs 2 AP (D-1199), so a character with 1 AP holds too little.
        Simulation short1 = IntoFight(TestBattles.ExactWithParty(1), (_, stored) => WithAp(stored, 1));
        Simulation enough = IntoFight(TestBattles.ExactWithParty(1), (_, stored) => WithAp(stored, 2));

        string refusal = BattleTurns.RefusalOf(short1.State, LessonChoice(Hew))
            ?? throw new InvalidOperationException("The rules take Hew from a character with 1 AP.");
        Assert.Contains("costs 2 AP", refusal, StringComparison.Ordinal);
        Assert.Contains("holds 1", refusal, StringComparison.Ordinal);
        Assert.Null(BattleTurns.RefusalOf(enough.State, LessonChoice(Hew)));
    }

    [Fact]
    public void AFallGivesEachCharacterWhoIsNotDownItsShareAndADownCharacterNone()
    {
        // Exit test 2 of PR-107 (D-1198): a strike with no hit regain fells the first grunt. A rate of 60%
        // shows the share and the round down: 8 AP gives 4, 12 AP gives 7, and 16 AP gives 9.
        BattleContent content = TestBattles.ExactWithParty(3, ("fall_regain", 6000));
        Simulation run = IntoFight(content, (slot, stored) => slot == 1 ? WithAp(stored, 0) with { Health = 0 } : WithAp(stored, 0));
        _ = run.TakeBattleEvents();

        BattleTurns.StrikeWith(run.State, new BattleMove(100, 100000, StrikeStat.Attack, null, null), FirstEnemy, run.State.Context("test"), []);

        IReadOnlyList<PartyMember> members = run.State.Characters.Members;
        Assert.Equal(CombatantPlace.Down, BattleRuns.BattleOf(run).Enemies[0].Place);
        Assert.Equal([(0, 4), (2, 9)], Regains(run));
        Assert.Equal([4, 0, 9], new[] { members[0].Ap, members[1].Ap, members[2].Ap });
        Assert.Equal([8, 12, 16], new[] { members[0].Stats.Ap, members[1].Stats.Ap, members[2].Stats.Ap });
    }

    [Fact]
    public void ABasicAttackThatHitsGivesTheAttackerItsShareAndAMissGivesNothing()
    {
        // Exit test 3 of PR-107 (D-1198): a rate of 30% of 8 AP gives 2, rounded down. The hit of
        // level 1 leaves the grunt of 30 health standing, so no fall adds to it.
        Simulation hit = IntoFight(TestBattles.ExactWithParty(1, ("hit_regain", 3000)), (_, stored) => WithAp(stored, 0));
        _ = hit.TakeBattleEvents();
        hit.Step([BattleRuns.AttackFirst(hit)]);

        Assert.Equal(CombatantPlace.Field, BattleRuns.BattleOf(hit).Enemies[0].Place);
        Assert.Equal([(0, 2)], Regains(hit));
        Assert.Equal(2, hit.State.Characters.Members[0].Ap);

        // Blind adds a miss chance of 100% past the ceiling of 0 (D-806), so the attack misses.
        Simulation miss = IntoFight(TestBattles.ExactWithParty(1, ("hit_regain", 3000), ("blind_miss", 10000)), (_, stored) => WithAp(stored, 0));
        BattleTurns.GiveStatus(miss.State, new BattleTarget(BattleSide.Party, 0), StatusKind.Blind, miss.State.Context("test"));
        _ = miss.TakeBattleEvents();
        miss.Step([BattleRuns.AttackFirst(miss)]);

        List<BattleEvent> events = [.. miss.TakeBattleEvents()];
        Assert.Contains(events, played => played.Kind == BattleEventKind.Miss && played.Actor.Side == BattleSide.Party);
        Assert.DoesNotContain(events, played => played.Kind == BattleEventKind.Regain);
        Assert.Equal(0, miss.State.Characters.Members[0].Ap);
    }

    [Fact]
    public void AFormThatHitsGivesNoHitRegain()
    {
        // D-1198: the hit regain follows the basic attack alone. Hew spends its 2 AP and gives none back.
        Simulation run = IntoFight(TestBattles.ExactWithParty(1, ("hit_regain", 3000)), (_, stored) => WithAp(stored, 8));
        _ = run.TakeBattleEvents();

        run.Step([Intent.OfBattleLesson(Hew, 0, FirstEnemy)]);

        Assert.Equal(CombatantPlace.Field, BattleRuns.BattleOf(run).Enemies[0].Place);
        Assert.Empty(Regains(run));
        Assert.Equal(6, run.State.Characters.Members[0].Ap);
    }

    [Fact]
    public void EachRegainOfASmallPoolGivesAtLeastOne()
    {
        // Exit test 4 of PR-107 (D-1198): 5% and 10% of the 8 AP of Marrek round down to zero, and
        // each regain gives 1. The fixture rates stand, and the second blow fells the first grunt.
        Simulation run = IntoFight(TestBattles.ExactWithParty(1), (_, stored) => WithAp(stored, 0));
        _ = run.TakeBattleEvents();
        run.Step([BattleRuns.AttackFirst(run)]);
        Assert.Equal([(0, 1)], Regains(run));

        List<LogEntry> log = [];
        BattleTurns.StrikeWith(run.State, new BattleMove(100, 100000, StrikeStat.Attack, null, null), FirstEnemy, run.State.Context("test"), log);

        Assert.Equal([(0, 1)], Regains(run));
        Assert.Equal(2, run.State.Characters.Members[0].Ap);
    }

    [Fact]
    public void ABasicAttackThatFellsAnEnemyGivesTheAttackerBothRegains()
    {
        // D-1210, OQ-252: the blow gives the hit regain first, then the fall regain. At 30% and 60%
        // of 8 AP, the attacker gains 2 and then 4.
        BattleContent content = TestBattles.ExactWithParty(1, ("attack_power", 100000), ("hit_regain", 3000), ("fall_regain", 6000));
        Simulation run = IntoFight(content, (_, stored) => WithAp(stored, 0));
        _ = run.TakeBattleEvents();

        run.Step([BattleRuns.AttackFirst(run)]);

        Assert.Equal(CombatantPlace.Down, BattleRuns.BattleOf(run).Enemies[0].Place);
        Assert.Equal([(0, 2), (0, 4)], Regains(run));
        Assert.Equal(6, run.State.Characters.Members[0].Ap);
    }

    [Fact]
    public void ARegainStopsAtFullApAndAFullPoolTakesNoEvent()
    {
        // D-1198: the regain gives no more than the AP that the character lacks.
        BattleContent content = TestBattles.ExactWithParty(1, ("attack_power", 100000), ("hit_regain", 3000), ("fall_regain", 6000));
        Simulation run = IntoFight(content, (_, stored) => WithAp(stored, 7));
        _ = run.TakeBattleEvents();

        run.Step([BattleRuns.AttackFirst(run)]);

        Assert.Equal([(0, 1)], Regains(run));
        Assert.Equal(8, run.State.Characters.Members[0].Ap);
    }

    [Fact]
    public void EachRegainOfAFightStaysInsideItsRulesOverOneThousandSeeds()
    {
        // D-1198, D-1210: a party of three with no AP fights the pair to its end with the basic
        // attack alone. Each regain gives at least 1 and at most the larger share, never reaches a
        // character who is down, and the AP at the end is the sum of the regains, at most full AP.
        BattleContent content = TestBattles.WithParty(3);
        BattleRules rules = content.Rules;
        int regains = 0;
        for (ulong seed = 1; seed <= 1000; seed += 1)
        {
            Simulation run = TestParty.StartEach(seed, (_, stored) => WithAp(stored, 0), content, BattleRuns.Map("group.test_pair"));
            run.Step([Intent.OfPlayer(IntentIds.MoveEast)]);
            _ = BattleRuns.FightToEnd(run, seed);

            IReadOnlyList<PartyMember> members = run.State.Characters.Members;
            var gained = new int[members.Count];
            var down = new bool[members.Count];
            foreach (BattleEvent played in run.TakeBattleEvents())
            {
                if (played.Kind == BattleEventKind.Down && played.Actor.Side == BattleSide.Party)
                {
                    down[played.Actor.Slot] = true;
                }

                if (played.Kind != BattleEventKind.Regain)
                {
                    continue;
                }

                int slot = played.Actor.Slot;
                int full = members[slot].Stats.Ap;
                int most = Math.Max(1, Math.Max(full * rules.HitRegain, full * rules.FallRegain) / 10000);
                Assert.False(down[slot], $"Seed {seed}: a regain reached the character of slot {slot}, who is down.");
                Assert.True(played.Amount >= 1 && played.Amount <= most, $"Seed {seed}: a regain of {played.Amount} AP for slot {slot}, outside 1 to {most}.");
                gained[slot] += played.Amount;
                regains += 1;
            }

            for (int slot = 0; slot < members.Count; slot += 1)
            {
                Assert.True(members[slot].Ap == gained[slot], $"Seed {seed}: slot {slot} holds {members[slot].Ap} AP, and its regains give {gained[slot]}.");
                Assert.True(members[slot].Ap <= members[slot].Stats.Ap, $"Seed {seed}: slot {slot} holds {members[slot].Ap} AP, above its full {members[slot].Stats.Ap}.");
            }
        }

        // The loop proves nothing when no regain happens, so each seed averages more than one.
        Assert.True(regains > 1000, $"The thousand fights gave {regains} regains.");
    }

    /// <summary>Starts a run on the map of the pair of grunts, changes the stored values of each character, and steps into the fight.</summary>
    private static Simulation IntoFight(BattleContent content, Func<int, CharacterValues, CharacterValues> change)
    {
        Simulation run = TestParty.StartEach(Seed, change, content, BattleRuns.Map("group.test_pair"));
        run.Step([Intent.OfPlayer(IntentIds.MoveEast)]);
        Assert.NotNull(run.State.Battle);
        return run;
    }

    private static CharacterValues WithAp(CharacterValues stored, int ap)
    {
        GrowthValues growth = stored.Growth ?? throw new InvalidOperationException($"The stored values of '{stored.Character.Value}' hold no growth.");
        return stored with { Growth = growth with { Ap = ap } };
    }

    private static BattleChoice LessonChoice(ContentId lesson) => new(BattleAction.Lesson, FirstEnemy, null, lesson, 0);

    /// <summary>Gives each regain event since the last take, as the slot of the character and the AP.</summary>
    private static List<(int Slot, int Ap)> Regains(Simulation run)
    {
        List<(int Slot, int Ap)> regains = [];
        foreach (BattleEvent played in run.TakeBattleEvents())
        {
            if (played.Kind == BattleEventKind.Regain)
            {
                Assert.Equal(BattleSide.Party, played.Actor.Side);
                regains.Add((played.Actor.Slot, played.Amount));
            }
        }

        return regains;
    }
}
