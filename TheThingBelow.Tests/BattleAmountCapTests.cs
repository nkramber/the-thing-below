using System;
using System.Collections.Generic;
using System.Text.RegularExpressions;
using TheThingBelow.Core.Battles;
using TheThingBelow.Core.Content;
using TheThingBelow.Core.Runs;
using Xunit;

namespace TheThingBelow.Tests;

/// <summary>
/// The cap of a battle amount (D-1357): each hit, heal, absorb, status share, and item of a battle
/// holds <see cref="BattleRules.MostAmount"/> at most, so each battle line holds its number (D-1356).
/// </summary>
public sealed class BattleAmountCapTests
{
    private const ulong Seed = 31;

    private static readonly BattleTarget FirstEnemy = new(BattleSide.Enemy, 0);

    [Fact]
    public void TheCapIsNineThousandNineHundredNinetyNine()
    {
        Assert.Equal(9_999, BattleRules.MostAmount);
    }

    [Fact]
    public void AHitAboveTheCapDealsTheCap()
    {
        // A strike of attack 50,000 at ten times power hits the grunt for about 490,000, and the
        // blow deals the cap alone (D-1357). The grunt holds more health than the cap, so the hit
        // leaves it standing.
        Simulation run = IntoFight(StrongContent(gruntHealth: 100_000));
        _ = run.TakeBattleEvents();

        BattleTurns.StrikeWith(run.State, new BattleMove(100, BattleRules.MostRate, StrikeStat.Attack, null, null), FirstEnemy, run.State.Context("test"), []);

        BattleEvent hit = Assert.Single(run.TakeBattleEvents(), played => played.Kind == BattleEventKind.Hit && played.Actor.Side == BattleSide.Party);
        Assert.Equal(BattleRules.MostAmount, hit.Amount);
        Assert.Equal(100_000 - BattleRules.MostAmount, BattleRuns.BattleOf(run).Enemies[0].Health);
    }

    [Fact]
    public void AStatusShareAboveTheCapTakesTheCap()
    {
        // A poison share of the whole health of a grunt of 100,000 takes the cap alone at the start
        // of its turn (D-799, D-1357).
        Simulation run = IntoFight(StrongContent(gruntHealth: 100_000, ("poison_share", 10000)));
        BattleTurns.GiveStatus(run.State, FirstEnemy, StatusKind.Poison, run.State.Context("test"));
        _ = run.TakeBattleEvents();

        // Marrek is fast, so a few weak strikes pass before the turn of the grunt comes.
        List<BattleEvent> events = [];
        for (int turn = 0; turn < 10 && !events.Exists(played => played.Kind == BattleEventKind.StatusHurt); turn += 1)
        {
            BattleTurns.StrikeWith(run.State, new BattleMove(100, 100, StrikeStat.Attack, null, null), FirstEnemy, run.State.Context("test"), []);
            events.AddRange(run.TakeBattleEvents());
        }

        BattleEvent share = Assert.Single(events, played => played.Kind == BattleEventKind.StatusHurt && played.Actor.Side == BattleSide.Enemy);
        Assert.Equal(BattleRules.MostAmount, share.Amount);
    }

    /// <summary>The exact content of the tests with Marrek at attack 50,000 and a grunt of the given health.</summary>
    private static BattleContent StrongContent(int gruntHealth, params (string Field, int Value)[] rules)
    {
        string fixture = Regex.Replace(
            TestBattles.FixtureFile,
            "(\"id\": \"character\\.marrek\"[^\\n]*?\"curve\": )\\[[^\\n]*?\\]( \\})",
            "${1}" + StatCurve.FlatText(new StatRow(900, 20, 50_000, 10, 4, 3, 200)) + "${2}");
        Assert.Contains("50000", fixture, StringComparison.Ordinal);
        string grunt = TestBattles.GruntFile.Replace("\"health\": 30,", $"\"health\": {gruntHealth},", StringComparison.Ordinal);
        return TestBattles.WithLessonFiles(fixture: fixture, grunt: grunt, exact: true, rules: rules);
    }

    private static Simulation IntoFight(BattleContent content)
    {
        Simulation run = TestParty.StartEach(Seed, (_, stored) => stored, content, BattleRuns.Map("group.test_pair"));
        run.Step([Intent.OfPlayer(IntentIds.MoveEast)]);
        Assert.NotNull(run.State.Battle);
        return run;
    }
}
