using System;
using System.Collections.Generic;
using System.Globalization;
using System.Text.RegularExpressions;
using TheThingBelow.Core.Battles;
using TheThingBelow.Core.Content;
using TheThingBelow.Core.Maps;
using TheThingBelow.Core.Runs;
using Xunit;

namespace TheThingBelow.Tests;

/// <summary>
/// The stat set of PR-99: the magic and the resistance, the stat of a strike, and the heal that
/// reads the magic of the caster with its roll (D-1052, D-1053, D-1057 to D-1059).
/// </summary>
/// <remarks>
/// The fixture of the tests sets the magic to the attack and the resistance to the defense, so
/// each other test keeps its numbers. These tests set the stats apart.
/// </remarks>
public sealed partial class StatSetTests
{
    private const ulong Seed = 99;

    /// <summary>The magic that each tests raises the magic of the fixture by, so the magic of Marrek at level 1 is 30.</summary>
    private const int MagicRaise = 18;

    private static readonly ContentId Salve = ContentId.Parse("lesson.fixture_salve", "test", "lesson");
    private static readonly BattleTarget Grunt = new(BattleSide.Enemy, 0);
    private static readonly BattleTarget Marrek = new(BattleSide.Party, 0);

    [Fact]
    public void AStrikeOfAttackAndAStrikeOfMagicEachReadTheirOwnStats()
    {
        // Exit test 1 of PR-99 (D-771, D-1053): Marrek holds 12 attack and 30 magic, and the grunt
        // holds 2 defense and 40 resistance.
        string grunt = TestBattles.GruntFile.Replace("\"resistance\": 2,", "\"resistance\": 40,", StringComparison.Ordinal);
        BattleContent content = TestBattles.WithLessonFiles(fixture: RaisedMagic(TestBattles.FixtureFile), grunt: grunt, exact: true);
        Simulation run = BattleRuns.IntoBattle(Seed, "group.one", content);
        _ = run.TakeBattleEvents();

        int physical = HitOf(run, StrikeStat.Attack);
        int magic = HitOf(run, StrikeStat.Magic);

        Assert.Equal(12 * 100 / (100 + 2), physical);
        Assert.Equal(30 * 100 / (100 + 40), magic);
    }

    [Fact]
    public void AHealFromTheMenuAddsTheShareOfTheMagicOfTheCasterToItsBase()
    {
        // D-1057: the salve heals 10 plus half of the 30 magic of Marrek. The exact rules hold the
        // hit factor at 10000 (D-1059).
        BattleContent content = TestBattles.WithLessonFiles(fixture: RaisedMagic(WithSalve()), abilities: SalveOf(10, 5000), exact: true);
        Simulation run = Start(content, TestMaps.Room, 20);
        run.Step([Intent.OfPlayer(IntentIds.OpenMenu)]);

        run.Step([Intent.OfMenuCast(0, Salve, 0, 0)]);

        Assert.Equal(20 + 10 + (30 * 5000 / 10000), run.State.Characters.Members[0].Health);
    }

    [Fact]
    public void AHealFromTheMenuRollsTheHitFactorOnTheProgressionStream()
    {
        // D-1059: the heal of 25 takes a factor from 9000 to 11000, so it heals 22 to 27.
        BattleContent content = TestBattles.WithLessonFiles(fixture: RaisedMagic(WithSalve()), abilities: SalveOf(10, 5000));
        var seen = new SortedSet<int>();
        for (ulong seed = 0; seed < 40; seed += 1)
        {
            Simulation run = Start(content, TestMaps.Room, 20, seed);
            run.Step([Intent.OfPlayer(IntentIds.OpenMenu)]);
            run.Step([Intent.OfMenuCast(0, Salve, 0, 0)]);

            int healed = run.State.Characters.Members[0].Health - 20;
            Assert.True(healed >= 25 * 9000 / 10000 && healed <= 25 * 11000 / 10000, $"Seed {seed}: the salve healed {healed}.");
            seen.Add(healed);
        }

        Assert.True(seen.Count > 1, "Forty seeds gave one heal, and a heal rolls its factor (D-1059).");
    }

    [Fact]
    public void AHealInAFightRollsTheHitFactorOnTheBattleStream()
    {
        // D-1058: the heal of 25 takes a factor from 9000 to 11000 in a fight too.
        BattleContent content = TestBattles.WithLessonFiles(fixture: RaisedMagic(WithSalve()), abilities: SalveOf(10, 5000));
        var seen = new SortedSet<int>();
        for (ulong seed = 0; seed < 40; seed += 1)
        {
            Simulation run = Start(content, BattleRuns.Map("group.one"), 20, seed);
            run.Step([Intent.OfPlayer(IntentIds.MoveEast)]);
            _ = run.TakeBattleEvents();

            run.Step([Intent.OfBattleLesson(Salve, 0, Marrek)]);

            BattleEvent heal = Assert.Single(run.TakeBattleEvents(), each => each.Kind == BattleEventKind.Heal);
            Assert.True(heal.Amount >= 25 * 9000 / 10000 && heal.Amount <= 25 * 11000 / 10000, $"Seed {seed}: the salve healed {heal.Amount}.");
            seen.Add(heal.Amount);
        }

        Assert.True(seen.Count > 1, "Forty seeds gave one heal, and a heal rolls its factor (D-1058).");
    }

    /// <summary>Strikes the grunt with a move of power 10000 and one stat, and gives the damage.</summary>
    private static int HitOf(Simulation run, StrikeStat stat)
    {
        BattleTurns.StrikeWith(run.State, new BattleMove(100, 10000, stat, null, null), Grunt, run.State.Context("test"), []);
        return Assert.Single(run.TakeBattleEvents(), each => each.Kind == BattleEventKind.Hit).Amount;
    }

    /// <summary>Raises each magic of the fixture by <see cref="MagicRaise"/>, so no curve falls.</summary>
    private static string RaisedMagic(string fixture) =>
        MagicField().Replace(fixture, match => $"\"magic\": {(int.Parse(match.Groups[1].Value, CultureInfo.InvariantCulture) + MagicRaise).ToString(CultureInfo.InvariantCulture)}");

    /// <summary>Gives the fixture with the salve in the first slot of Marrek, in place of the hew.</summary>
    private static string WithSalve() => TestBattles.FixtureFile
        .Replace("[\"lesson.fixture_hew\", \"lesson.fixture_cinder\"]", "[\"lesson.fixture_salve\", \"lesson.fixture_cinder\"]", StringComparison.Ordinal)
        .Replace("\"lesson_pack\": [\"lesson.fixture_salve\", ", "\"lesson_pack\": [\"lesson.fixture_hew\", ", StringComparison.Ordinal);

    /// <summary>Gives the ability file of the tests with a salve of one base and one power (D-1057).</summary>
    private static string SalveOf(int healBase, int power)
    {
        string text = TestBattles.AbilitiesFile.Replace(
            "\"kind\": \"heal\", \"delay\": 120, \"base\": 30, \"power\": 1 }",
            $"\"kind\": \"heal\", \"delay\": 120, \"base\": {healBase}, \"power\": {power} }}",
            StringComparison.Ordinal);
        Assert.NotEqual(TestBattles.AbilitiesFile, text);
        return text;
    }

    /// <summary>Starts a run on a map with Marrek at one health.</summary>
    private static Simulation Start(BattleContent content, GameMap map, int health, ulong seed = Seed)
    {
        var story = TheThingBelow.Core.Story.StoryContent.Load(
            TheThingBelow.Core.Story.FlagList.Read(System.Text.Encoding.UTF8.GetBytes(TestBattles.NoFlagsFile), TheThingBelow.Core.Story.FlagList.Path),
            [],
            content);
        RunSnapshot start = Simulation.Start(seed, map, content, TestBattles.Notices, story, DebugIntentHandlers.None).Snapshot();
        PartySnapshot party = start.Characters ?? throw new InvalidOperationException("The snapshot holds no party.");
        List<CharacterValues> characters = [party.Characters[0] with { Health = health }];
        return Simulation.Resume(seed, start with { Characters = party with { Characters = characters } }, map, content, TestBattles.Notices, story, DebugIntentHandlers.None);
    }

    [GeneratedRegex("\"magic\": (\\d+)")]
    private static partial Regex MagicField();
}
