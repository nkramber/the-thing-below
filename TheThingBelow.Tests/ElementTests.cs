using System;
using System.Collections.Generic;
using System.Text;
using TheThingBelow.Core.Battles;
using TheThingBelow.Core.Content;
using TheThingBelow.Core.Logging;
using TheThingBelow.Core.Runs;
using Xunit;

namespace TheThingBelow.Tests;

/// <summary>
/// The eight elements, the four affinities, and the element table of the enemy record (PR-66,
/// D-74, D-794 to D-797, D-804, D-809).
/// </summary>
public sealed class ElementTests
{
    private const ulong Seed = 20260921;

    private static readonly Affinity[] Levels = [Affinity.Normal, Affinity.Weak, Affinity.Resist, Affinity.Absorb];

    [Fact]
    public void EachElementMeetsEachAffinityOverOneThousandSeeds()
    {
        // Exit test 1: the same seed gives the same miss roll and the same hit factor at each
        // level, so each level reads as a rate on one hit (D-794, D-795, D-807, D-809).
        var contents = new BattleContent[Elements.All.Count, Levels.Length];
        foreach (Element element in Elements.All)
        {
            for (int level = 0; level < Levels.Length; level += 1)
            {
                contents[(int)element, level] = TestBattles.WithGrunt(element, Levels[level], [], exact: false);
            }
        }

        for (ulong seed = 0; seed < 1000; seed += 1)
        {
            Element element = Elements.All[(int)(seed % 8)];
            int power = 5000 + (int)(seed * 7919 % 15000);
            var hits = new BattleEvent[Levels.Length];
            var before = new int[Levels.Length];
            var after = new int[Levels.Length];
            for (int level = 0; level < Levels.Length; level += 1)
            {
                (hits[level], before[level], after[level]) = StrikeAfterAWound(seed, contents[(int)element, level], element, power);
            }

            string where = $"Seed {seed}, element {Elements.NameOf(element)}, power {power}";
            if (hits[0].Kind == BattleEventKind.Miss)
            {
                Assert.All(hits, hit => Assert.True(hit.Kind == BattleEventKind.Miss, $"{where}: one level missed and another hit."));
                continue;
            }

            int normal = hits[0].Amount;
            Assert.True(normal > 1, $"{where}: the hit of D-771 is {normal}, and the property needs a hit above the floor.");
            Assert.Equal((BattleEventKind.Hit, Affinity.Normal), (hits[0].Kind, hits[0].Affinity));
            Assert.True(hits[1].Amount == normal * 15000 / 10000, $"{where}: weak dealt {hits[1].Amount}, and normal dealt {normal}.");
            Assert.True(hits[2].Amount == Math.Max(1, normal * 5000 / 10000), $"{where}: resist dealt {hits[2].Amount}, and normal dealt {normal}.");
            Assert.Equal((BattleEventKind.Hit, Affinity.Weak), (hits[1].Kind, hits[1].Affinity));
            Assert.Equal((BattleEventKind.Hit, Affinity.Resist), (hits[2].Kind, hits[2].Affinity));

            int restored = Math.Min(normal, 30 - before[3]);
            Assert.True(
                hits[3].Kind == BattleEventKind.Absorb && hits[3].Amount == restored && after[3] == before[3] + restored,
                $"{where}: absorb gave {hits[3].Kind} {hits[3].Amount} from {before[3]} to {after[3]}, and the heal is {restored}.");
        }
    }

    [Fact]
    public void AMoveWithNoElementTakesEveryAffinityAsNormal()
    {
        // D-796: the basic attack holds no element, so a grunt that absorbs fire takes 11.
        Simulation run = BattleRuns.IntoBattle(Seed, "group.one", TestBattles.WithGrunt(Element.Fire, Affinity.Absorb, [], exact: true));

        run.Step([BattleRuns.AttackFirst(run)]);

        Assert.Equal(30 - 11, BattleRuns.BattleOf(run).Enemies[0].Health);
    }

    [Fact]
    public void ShellHalvesAnElementalHitAndPassesTheBasicAttack()
    {
        // D-804: the hit of 11 on a grunt with shell is 5 for fire, and 11 for the basic attack.
        Simulation run = BattleRuns.IntoBattle(Seed, "group.test_pair", TestBattles.Exact);
        List<LogEntry> log = [];
        BattleTarget first = new(BattleSide.Enemy, 0);
        BattleTarget second = new(BattleSide.Enemy, 1);
        BattleTurns.GiveStatus(run.State, first, StatusKind.Shell, run.State.Context("test"));
        BattleTurns.GiveStatus(run.State, second, StatusKind.Shell, run.State.Context("test"));

        BattleTurns.StrikeWith(run.State, Fire(), first, run.State.Context("test"), log);
        run.Step([Intent.OfPlayer(IntentIds.BattleAttack, second, null)]);

        Battle battle = BattleRuns.BattleOf(run);
        Assert.Equal(30 - 5, battle.Enemies[0].Health);
        Assert.Equal(30 - 11, battle.Enemies[1].Health);
    }

    [Fact]
    public void AnAbsorbHealsTheWholeHitThroughShellAndDefend()
    {
        // D-809: no cut applies to the heal of an absorb. The grunt takes 11, then shell,
        // then absorbs a fire hit of 11 back to full.
        Simulation run = BattleRuns.IntoBattle(Seed, "group.one", TestBattles.WithGrunt(Element.Fire, Affinity.Absorb, [], exact: true));
        List<LogEntry> log = [];
        BattleTarget grunt = new(BattleSide.Enemy, 0);
        run.Step([BattleRuns.AttackFirst(run)]);
        Assert.Equal(19, BattleRuns.BattleOf(run).Enemies[0].Health);

        BattleTurns.GiveStatus(run.State, grunt, StatusKind.Shell, run.State.Context("test"));
        BattleTurns.StrikeWith(run.State, Fire(), grunt, run.State.Context("test"), log);

        Assert.Equal(30, BattleRuns.BattleOf(run).Enemies[0].Health);
    }

    [Fact]
    public void TheRatesApplyInTheOrderOfTheOwner()
    {
        // D-809: 11 times weak is 16, the back row halves it to 8, and shell halves it to 4.
        // A rate before the floor of each step gives another number, such as 5 for the
        // back row before weak.
        Simulation run = BattleRuns.IntoBattle(Seed, "group.one", TestBattles.WithGrunt(Element.Fire, Affinity.Weak, [], exact: true));
        List<LogEntry> log = [];
        BattleTarget grunt = new(BattleSide.Enemy, 0);
        run.Step([Intent.OfPlayer(IntentIds.BattleStep)]);
        BattleTurns.GiveStatus(run.State, grunt, StatusKind.Shell, run.State.Context("test"));
        int before = BattleRuns.BattleOf(run).Enemies[0].Health;

        BattleTurns.StrikeWith(run.State, Fire(), grunt, run.State.Context("test"), log);

        Assert.Equal(before - 4, BattleRuns.BattleOf(run).Enemies[0].Health);
    }

    [Fact]
    public void TheCheckoutEnemiesTakeEveryElementAsNormalAndRefuseNoStatus()
    {
        // D-794, D-805: the fixture enemies hold the full table, and PR-17 writes real ones.
        BattleContent content = TestBattles.Content;
        foreach (string id in new[] { "enemy.fixture_grunt", "enemy.fixture_brute" })
        {
            EnemyRecord record = content.Enemy(ContentId.Parse(id, "test", EnemyRecord.Kind));
            foreach (Element element in Elements.All)
            {
                Assert.Equal(Affinity.Normal, record.Elements.Of(element));
            }

            Assert.Empty(record.Immune);
        }
    }

    [Fact]
    public void ARecordReadsEachAffinityOfItsTable()
    {
        EnemyRecord record = ReadGrunt("\"fire\": \"normal\", \"ice\": \"normal\"", "\"fire\": \"weak\", \"ice\": \"absorb\"");

        Assert.Equal(Affinity.Weak, record.Elements.Of(Element.Fire));
        Assert.Equal(Affinity.Absorb, record.Elements.Of(Element.Ice));
        Assert.Equal(Affinity.Normal, record.Elements.Of(Element.Dark));
    }

    [Theory]
    [InlineData("\"elements\": { \"fire\": \"normal\", \"ice\": \"normal\", \"lightning\": \"normal\", \"earth\": \"normal\", \"wind\": \"normal\", \"water\": \"normal\", \"holy\": \"normal\", \"dark\": \"normal\" },\n", "", "elements", "absent")]
    [InlineData(", \"dark\": \"normal\" }", " }", "elements.dark", "absent")]
    [InlineData("\"dark\": \"normal\"", "\"dark\": \"vulnerable\"", "elements.dark", "vulnerable")]
    [InlineData("\"dark\": \"normal\"", "\"dark\": \"normal\", \"metal\": \"weak\"", "elements.metal", "metal")]
    [InlineData("\"dark\": \"normal\"", "\"dark\": \"normal\", \"dark\": \"weak\"", "elements.dark", "two times")]
    public void AFaultOfTheElementTableFailsWithTheFileAndTheField(string find, string replace, string field, string reason)
    {
        // T-2, D-794, D-797: an absent, unknown, or repeated element fails the load.
        ContentException error = Assert.Throws<ContentException>(() => ReadGrunt(find, replace));

        Assert.Contains(TestBattles.GruntPath, error.Message, StringComparison.Ordinal);
        Assert.Contains(field, error.Message, StringComparison.Ordinal);
        Assert.Contains(reason, error.Message, StringComparison.Ordinal);
    }

    private static (BattleEvent Hit, int Before, int After) StrikeAfterAWound(ulong seed, BattleContent content, Element element, int power)
    {
        Simulation run = BattleRuns.IntoBattle(seed, "group.one", content);
        run.Step([BattleRuns.AttackFirst(run)]);
        Battle battle = BattleRuns.BattleOf(run);
        Assert.True(battle.Outcome == BattleOutcome.Running, $"Seed {seed}: the first attack ended the fight.");
        int before = battle.Enemies[0].Health;
        _ = run.TakeBattleEvents();

        BattleTurns.StrikeWith(run.State, new BattleMove(100, power, element, null), new BattleTarget(BattleSide.Enemy, 0), run.State.Context("test"), []);

        foreach (BattleEvent battleEvent in run.TakeBattleEvents())
        {
            if (battleEvent.Actor.Side == BattleSide.Party
                && (battleEvent.Kind == BattleEventKind.Hit || battleEvent.Kind == BattleEventKind.Miss || battleEvent.Kind == BattleEventKind.Absorb))
            {
                int after = Math.Max(0, battle.Enemies[0].Health);
                return (battleEvent, before, battleEvent.Kind == BattleEventKind.Absorb ? after : before - battleEvent.Amount);
            }
        }

        throw new InvalidOperationException($"Seed {seed}: the strike emitted no hit, miss, or absorb.");
    }

    private static BattleMove Fire() => new(100, 10000, Element.Fire, null);

    private static EnemyRecord ReadGrunt(string find, string replace)
    {
        string text = TestBattles.GruntFile.Replace(find, replace, StringComparison.Ordinal);
        Assert.NotEqual(TestBattles.GruntFile, text);
        return EnemyRecord.Read(Encoding.UTF8.GetBytes(text), TestBattles.GruntPath);
    }
}
