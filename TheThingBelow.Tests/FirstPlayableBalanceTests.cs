using System;
using System.Collections.Generic;
using System.Text;
using TheThingBelow.Core.Battles;
using TheThingBelow.Core.Content;
using TheThingBelow.Core.Maps;
using TheThingBelow.Core.Runs;
using TheThingBelow.Tools.Bots;
using TheThingBelow.Tools.Content;
using Xunit;

namespace TheThingBelow.Tests;

/// <summary>
/// The balance floor of the first playable (exit test 1 of PR-17, D-1337): the party at its
/// natural level wins each fight of the place where it meets it. The greedy bot fights each
/// battle, with its basic attack and its heal items alone (D-1183). A lone Marrek of level 1
/// with his start kit meets the beasts of the pasture, and Marrek at level 3 with Bergit at
/// level 2 meet the foes of the hanging cells. Each fight runs over a seed loop, and each rate
/// stays at 85% or more. PR-30 tunes the numbers of the whole game.
/// </summary>
public sealed class FirstPlayableBalanceTests
{
    /// <summary>The count of seeds of each fight.</summary>
    private const int Seeds = 100;

    /// <summary>The least share of the fights of one group that the party wins, in percent.</summary>
    private const int FloorPercent = 85;

    /// <summary>The most ticks of one fight before the test fails (T-2).</summary>
    private const int FightTicks = 20_000;

    private static readonly Lazy<ContentSet> Content = new(() => ContentSet.Load(ContentFolder.Read(RepositoryRoot.Find())));

    [Theory]
    [InlineData("group.one_wolves", "common")]
    [InlineData("group.one_crows", "common")]
    [InlineData("group.one_boar", "elite")]
    public void ALoneMarrekOfLevelOneWinsEachFightOfThePasture(string group, string size)
    {
        int wins = Wins(group, size, (snapshot, _) => snapshot);

        Assert.True(wins * 100 >= FloorPercent * Seeds, $"Marrek alone at level 1 won {wins} of {Seeds} fights with '{group}', and the floor is {FloorPercent}%.");
    }

    [Theory]
    [InlineData("group.one_rats", "common")]
    [InlineData("group.one_jailers", "common")]
    [InlineData("group.one_jailer_lamp", "common")]
    [InlineData("group.one_hounds", "common")]
    [InlineData("group.one_jailer_hound", "common")]
    [InlineData("group.one_cell_watch", "common")]
    public void MarrekAndBergitWinEachFightOfTheCells(string group, string size)
    {
        int wins = Wins(group, size, (snapshot, content) => WithBergit(snapshot, content, marrekLevel: 3));

        Assert.True(wins * 100 >= FloorPercent * Seeds, $"Marrek at level 3 and Bergit at level 2 won {wins} of {Seeds} fights with '{group}', and the floor is {FloorPercent}%.");
    }

    /// <summary>Gives the count of fights of one group that the greedy bot wins, over the seed loop.</summary>
    private static int Wins(string group, string size, Func<RunSnapshot, BattleContent, RunSnapshot> party)
    {
        ContentSet content = Content.Value;
        GameMap map = GuardMap(group, size);
        int wins = 0;
        for (ulong seed = 1; seed <= Seeds; seed += 1)
        {
            RunSnapshot start = party(Simulation.Start(seed, map, content.Battle, content.Notices, content.Story, DebugIntentHandlers.None).Snapshot(), content.Battle);
            Simulation run = Simulation.Resume(seed, start, MapSet.Of([map]), content.Battle, content.Notices, content.Story, DebugIntentHandlers.None);
            run.Step([Intent.OfPlayer(IntentIds.MoveEast)]);
            Assert.True(run.State.Battle is not null, $"Seed {seed}: the step into '{group}' started no fight.");
            wins += Fight(run, seed) == BattleOutcome.Won ? 1 : 0;
        }

        return wins;
    }

    /// <summary>Fights one battle with the greedy bot to its end, and gives the outcome.</summary>
    private static BattleOutcome Fight(Simulation run, ulong seed)
    {
        var policy = new GreedyPolicy();
        for (int tick = 0; tick < FightTicks; tick += 1)
        {
            Battle battle = run.State.Battle ?? throw new InvalidOperationException($"Seed {seed}: the fight ended with no outcome.");
            if (battle.Outcome != BattleOutcome.Running)
            {
                return battle.Outcome;
            }

            Intent? chosen = policy.Choose(run.State, run.Accepted());
            _ = run.Step(chosen is null ? [] : [chosen]);
            _ = run.TakeBattleEvents();
        }

        throw new InvalidOperationException($"Seed {seed}: the fight took {FightTicks} ticks.");
    }

    /// <summary>Gives the snapshot with Marrek at a level and Bergit at her join level with her kit, each at full health and AP (D-1342, D-1350).</summary>
    private static RunSnapshot WithBergit(RunSnapshot snapshot, BattleContent content, int marrekLevel)
    {
        PartySnapshot party = snapshot.Characters ?? throw new InvalidOperationException("The start snapshot holds no party.");
        CharacterValues marrek = party.Characters[0];
        CharacterRecord marrekRecord = content.Character(marrek.Character);
        LessonValues lessons = marrek.Lessons ?? throw new InvalidOperationException("The start snapshot holds no lessons of Marrek.");
        CharacterValues grown = marrek with
        {
            Health = marrekRecord.At(marrekLevel).Health,
            Growth = new GrowthValues(marrekLevel, content.Rules.LevelExperience[marrekLevel - 1], marrekRecord.At(marrekLevel).Ap),
            Lessons = lessons with { Slots = SlotsOf(lessons.Slots, content.Rules.SlotsAt(marrekLevel)) },
        };

        CharacterRecord bergit = content.Character(ContentId.Parse("character.bergit", "test", "character"));
        List<LessonPoints> points = [];
        foreach (ContentId lesson in bergit.JoinLessons)
        {
            points.Add(new LessonPoints(lesson, 0));
        }

        points.Sort((one, other) => string.CompareOrdinal(one.Lesson.Value, other.Lesson.Value));
        CharacterValues joined = new(
            bergit.Id,
            bergit.At(bergit.JoinLevel).Health,
            bergit.Row,
            [],
            new GrowthValues(bergit.JoinLevel, content.Rules.LevelExperience[bergit.JoinLevel - 1], bergit.At(bergit.JoinLevel).Ap),
            new LessonValues(SlotsOf([.. bergit.JoinLessons], content.Rules.SlotsAt(bergit.JoinLevel)), points),
            GearRules.SlotsOf(bergit.JoinGear, content.Gear, "test", bergit.Id.Value));
        return snapshot with { Characters = party with { Characters = [grown, joined] } };
    }

    private static ContentId?[] SlotsOf(IReadOnlyList<ContentId?> lessons, int count)
    {
        var slots = new ContentId?[count];
        for (int index = 0; index < lessons.Count && index < count; index += 1)
        {
            slots[index] = lessons[index];
        }

        return slots;
    }

    /// <summary>Gives a room of region one with one guard of a group east of the spawn point, as <see cref="BattleRuns.Map"/> builds for the fixture.</summary>
    private static GameMap GuardMap(string group, string size)
    {
        string station = string.CompareOrdinal(size, "common") == 0
            ? "\"routes\": [{ \"times\": [\"dawn\", \"day\", \"dusk\", \"night\"], \"tiles\": [{ \"x\": 2, \"y\": 1 }] }]"
            : "\"areas\": [{ \"times\": [\"dawn\", \"day\", \"dusk\", \"night\"], \"x\": 2, \"y\": 1, \"width\": 3, \"height\": 2 }]";
        string text = $$"""
        {
         "comment": "A room of region one with one guard beside the spawn point.",
         "id": "map.test_balance",
         "region": "region.one",
         "label": "label.village",
         "time": "day",
         "dark": false,
         "kind": "dungeon", "npcs": [], "services": [], "zones": [], "zone_grid": [], "time_changes": [], "reopen": [],
         "terrain": ["######", "#....#", "#....#", "######"],
         "things": [{ "id": "spawn_point.test_balance_start", "kind": "spawn_point", "x": 1, "y": 1 }],
         "enemies": [{ "id": "patrol.test_balance", "group": "{{group}}", "size": "{{size}}", "facing": "west", "step_ticks": 32, "sight_range": 0, {{station}} }],
         "triggers": []
        }
        """;
        return GameMap.Read(Encoding.UTF8.GetBytes(text), "tests-balance.json");
    }
}
