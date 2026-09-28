using System;
using System.Collections.Generic;
using System.Linq;
using TheThingBelow.Core.Battles;
using TheThingBelow.Core.Content;
using TheThingBelow.Core.Maps;
using TheThingBelow.Core.Runs;
using TheThingBelow.Core.Story;
using TheThingBelow.Tools.Bots;
using TheThingBelow.Tools.Content;
using Xunit;

namespace TheThingBelow.Tests;

/// <summary>
/// The shipped content of the first playable of PR-17 on the checkout: the start in the village,
/// the five places with their companion files, the night after the end, the kits of the joins, and
/// the bribe of the turnkey (D-1328 to D-1355). Guided runs at natural levels play the story
/// from the village to the join of Dagvar, and the greedy bot fights each battle of them.
/// </summary>
public sealed class FirstPlayableContentTests
{
    private const ulong Seed = 20260928;



    /// <summary>The most ticks that one guided run takes before it counts as stuck (T-2).</summary>
    private const long RunBudget = 100_000;

    /// <summary>The count of seeds of the guided runs at natural levels.</summary>
    private const int RunSeeds = 40;

    /// <summary>The least count of guided runs that reach the end with no wipe: 85% of the seeds.</summary>
    private const int CleanRuns = 34;

    /// <summary>
    /// The route of the story through the first playable: on each map, the first goal that is not
    /// done is the next one. An exit or an entrance ends the goals of its map (D-1331, D-1337).
    /// </summary>
    private static readonly RouteGoal[] Route =
    [
        new("map.village", "chest.village_home", TargetReach.Confirm, state => state.Party.Place.LeftIn(Id("chest.village_home")) is not null),
        new("map.village", "door.village_marrek", TargetReach.Confirm, state => state.Party.Place.IsOpen(Id("door.village_marrek"))),
        new("map.village", "exit.village_road", TargetReach.StandOn, _ => false),
        new("map.overworld", "entrance.overworld_pasture", TargetReach.StandOn, state => state.Story.Flags.IsOn(Id("flag.region_one_bergit_joins"))),
        new("map.overworld", "entrance.overworld_town", TargetReach.StandOn, _ => false),
        new("map.village_pasture", "chest.pasture_hollow", TargetReach.Confirm, state => state.Party.Place.LeftIn(Id("chest.pasture_hollow")) is not null),
        new("map.village_pasture", "chest.pasture_stump", TargetReach.Confirm, state => state.Party.Place.LeftIn(Id("chest.pasture_stump")) is not null),
        new("map.village_pasture", "patrol.pasture_boar", TargetReach.Bump, state => state.Story.Flags.IsOn(Id("flag.region_one_bergit_joins"))),
        new("map.village_pasture", "exit.pasture_way_out", TargetReach.StandOn, _ => false),
        new("map.mining_town", "npc.town_turnkey", TargetReach.Confirm, state => state.Story.Flags.IsOn(Id("flag.town_turnkey_paid"))),
        new("map.mining_town", "npc.town_innkeeper", TargetReach.Confirm, state => AllAtFullHealth(state) || state.Characters.Gold < 10),
        new("map.mining_town", "exit.town_cells_stair", TargetReach.StandOn, _ => false),
        new("map.cells_upper", "chest.cells_guardroom", TargetReach.Confirm, state => state.Party.Place.LeftIn(Id("chest.cells_guardroom")) is not null),
        new("map.cells_upper", "door.cells_upper_stair", TargetReach.Confirm, state => state.Party.Place.IsOpen(Id("door.cells_upper_stair"))),
        new("map.cells_upper", "exit.cells_upper_stair_down", TargetReach.StandOn, _ => false),
        new("map.cells_lower", "chest.cells_yard", TargetReach.Confirm, state => state.Party.Place.LeftIn(Id("chest.cells_yard")) is not null),
        new("map.cells_lower", "door.cells_lower_yard", TargetReach.Confirm, state => state.Party.Place.IsOpen(Id("door.cells_lower_yard"))),
        new("map.cells_lower", "trigger.cells_dagvar", TargetReach.StandOn, state => state.Story.Flags.IsOn(Id("flag.region_one_dagvar_joins"))),
    ];

    private static readonly Lazy<ContentSet> Content = new(() => ContentSet.Load(ContentFolder.Read(RepositoryRoot.Find())));

    private static readonly string[] Places = ["map.village", "map.village_pasture", "map.mining_town", "map.cells_upper", "map.cells_lower"];

    [Fact]
    public void TheRunStartsInTheHouseOfMarrekAndPlaysTheOpening()
    {
        // D-1344, D-1351: the run opens on the spawn point of the village, with the real kit of
        // Marrek, and the entry trigger plays the opening scene at the first world step.
        ContentSet content = Content.Value;
        GameMap village = content.Map(Id("map.village"));

        Simulation run = Start(content);
        PartyMember marrek = Assert.Single(run.State.Characters.Members);

        Assert.Equal("map.village", MapIds.FirstMap.Value);
        Assert.Equal("map.village", run.State.Party.Map.Id.Value);
        Assert.Equal(village.Spawn, run.State.Party.LeadAt);
        Assert.Equal(["lesson.hew", "lesson.undercut"], marrek.Slots.Select(slot => slot?.Value));
        Assert.Contains("gear.fathers_pick", Values(marrek.Gear));
        Assert.Contains("gear.work_coat", Values(marrek.Gear));

        run.Step([]);

        Assert.Equal("scene.village_opening", run.State.Story.Scene?.Id.Value);
    }

    [Fact]
    public void EachPlaceLoadsWithItsDecorItsLightAtEachTimeAndItsEdges()
    {
        // D-1328, D-1349: each map of the first playable lies in region one, and holds a decor
        // file, an edge file, and a light setup for each time that it can take.
        ContentSet content = Content.Value;
        foreach (string place in Places)
        {
            GameMap map = content.Map(Id(place));

            Assert.Equal("region.one", map.Region.Value);
            Assert.Equal(place, content.Light.DecorOf(map.Id).Map.Value);
            Assert.Equal(place, content.Edges.EdgesOf(map.Id).Map.Value);
            Assert.Equal("region.one", content.Effects.Transitions.RegionOf(map.Id).Value);
            foreach (TimeOfDay time in map.Times)
            {
                Assert.Equal(time, content.Light.SetupOf(map.Id, time).Time);
            }
        }
    }

    [Fact]
    public void EachMapOutsideTurnsToNightAtTheEndAndTheCellsStayNight()
    {
        // Exit test 9 of PR-17 (D-1338, D-1349): the village is day, the pasture and the town
        // dusk, and the overworld day, and each one takes the night after the flag of the end. The
        // hanging cells are night on each side of the end.
        ContentSet content = Content.Value;
        FlagSet before = FlagSet.Empty();
        FlagSet after = FlagSet.Empty();
        after.TurnOn(Id("flag.first_playable_end"));

        (string Map, TimeOfDay Time)[] outside =
        [
            ("map.village", TimeOfDay.Day), ("map.village_pasture", TimeOfDay.Dusk), ("map.mining_town", TimeOfDay.Dusk), ("map.overworld", TimeOfDay.Day),
        ];
        foreach ((string map, TimeOfDay time) in outside)
        {
            Assert.Equal(time, content.Map(Id(map)).TimeFor(before));
            Assert.Equal(TimeOfDay.Night, content.Map(Id(map)).TimeFor(after));
        }

        foreach (string cells in new[] { "map.cells_upper", "map.cells_lower" })
        {
            GameMap map = content.Map(Id(cells));
            Assert.True(map.Dark, $"The map '{cells}' is not dark (D-1063).");
            Assert.Equal(TimeOfDay.Night, map.TimeFor(before));
            Assert.Equal(TimeOfDay.Night, map.TimeFor(after));
        }
    }

    [Fact]
    public void BergitAndDagvarJoinWithTheirKitsAtTheirJoinLevels()
    {
        // D-1342, D-1350, D-1353: Bergit joins at level 2 with Cover and Quarrel, and Dagvar at
        // level 4 with Ember and Grave Rot, each with the gear of the join.
        ContentSet content = Content.Value;
        CharacterRecord bergit = content.Battle.Character(Id("character.bergit"));
        CharacterRecord dagvar = content.Battle.Character(Id("character.dagvar"));

        Assert.Equal(2, bergit.JoinLevel);
        Assert.Equal(["lesson.cover", "lesson.quarrel"], bergit.JoinLessons.Select(id => id.Value));
        Assert.Equal(["gear.short_sword", "gear.iron_buckler", "gear.padded_jerkin"], bergit.JoinGear.Select(id => id.Value));
        Assert.Equal(4, dagvar.JoinLevel);
        Assert.Equal(["lesson.ember", "lesson.grave_rot"], dagvar.JoinLessons.Select(id => id.Value));
        Assert.Equal(["gear.cell_bar", "gear.prison_smock"], dagvar.JoinGear.Select(id => id.Value));
        Assert.Equal(["character.marrek"], content.Battle.Fixture.StartParty.Select(id => id.Value));
    }

    [Fact]
    public void TheTurnkeyAsksFortyGoldAndThePaidFlagOpensTheStairDoor()
    {
        // D-1334, D-1335, D-1347: the talk with the turnkey plays a pay step of 40 gold, whose
        // flag is the condition of the gate on the stair door, and the gate posts its notice.
        ContentSet content = Content.Value;
        PayStep pay = Assert.Single(content.Story.Scene(Id("scene.town_turnkey")).Steps.OfType<PayStep>());
        GameMap town = content.Map(Id("map.mining_town"));
        MapThing gate = Assert.Single(town.Things, thing => thing.Kind == MapThingKind.Gate);
        FlagSet flags = FlagSet.Empty();

        Assert.Equal(40, pay.Price);
        Assert.Equal("flag.town_turnkey_paid", pay.Flag.Value);
        Assert.Equal("gate.town_cells_door", gate.Id.Value);
        Assert.Equal("notice.town_cells_door", gate.Gate?.Notice.Value);
        Assert.False(gate.Gate!.Condition.Holds(flags));
        flags.TurnOn(Id("flag.town_turnkey_paid"));
        Assert.True(gate.Gate.Condition.Holds(flags));
    }

    [Fact]
    public void GuidedRunsAtNaturalLevelsPlayFromTheVillageToTheJoinOfDagvar()
    {
        // Exit test 1 of PR-17, played by the Core alone (D-1337): the opening, the pasture and
        // the boar, the join of Bergit, the road to the town, the bribe and a rest at the inn,
        // both floors of the cells with their keys, and the join of Dagvar. The walk follows the
        // route of the story, and the greedy bot answers each story step and fights each battle
        // with the start kit, the natural levels, and the fights of the zones of the overworld
        // (D-1183, D-1332). The bot attacks alone and never reloads, so a seed loop measures
        // the share of runs that reach the end with no wipe, and each clean run ends whole.
        ContentSet content = Content.Value;
        List<string> wipes = [];
        for (ulong seed = 1; seed <= RunSeeds; seed += 1)
        {
            GuidedRun played = PlayGuided(Start(content, seed));
            if (played.Wipe is string wipe)
            {
                wipes.Add($"seed {seed}: {wipe}");
                continue;
            }

            AssertTheEnd(played, seed);
        }

        Assert.True(RunSeeds - wipes.Count >= CleanRuns, $"{RunSeeds - wipes.Count} of {RunSeeds} runs reached the join of Dagvar, and the floor is {CleanRuns}. {string.Join(" / ", wipes)}");
    }

    /// <summary>Checks the end of one clean run: each flag of the story, the maps in the order of the walk, the kits of the joins, and the price of the bribe.</summary>
    private static void AssertTheEnd(GuidedRun played, ulong seed)
    {
        foreach (string flag in new[] { "flag.village_opening", "flag.region_one_bergit_joins", "flag.town_turnkey_paid", "flag.region_one_dagvar_joins" })
        {
            Assert.True(played.Run.State.Story.Flags.IsOn(Id(flag)), $"Seed {seed}: the flag '{flag}' is off at the end.");
        }

        Assert.Equal(
            ["map.village", "map.overworld", "map.village_pasture", "map.overworld", "map.mining_town", "map.cells_upper", "map.cells_lower"],
            played.Maps);
        IReadOnlyList<PartyMember> members = played.Run.State.Characters.Members;
        Assert.Equal(["character.marrek", "character.bergit", "character.dagvar"], members.Select(member => member.Record.Id.Value));
        AssertKit(members[1], ["lesson.cover", "lesson.quarrel"], ["gear.short_sword", "gear.iron_buckler", "gear.padded_jerkin"]);
        AssertKit(members[2], ["lesson.ember", "lesson.grave_rot"], ["gear.cell_bar", "gear.prison_smock"]);
        Assert.Equal(40, played.Paid);
    }

    /// <summary>
    /// Plays a run along the route of the story until the flag of the end, or until a wipe. The
    /// greedy bot takes each tick of a story scene, a battle, and a menu (D-1183).
    /// </summary>
    private static GuidedRun PlayGuided(Simulation run)
    {
        var fighter = new GreedyPolicy();
        List<string> maps = [run.State.Party.Map.Id.Value];
        List<string> fights = [];
        int paid = 0;
        bool fighting = false;
        for (long played = 0; !run.State.Story.Flags.IsOn(Id("flag.first_playable_end")); played += 1)
        {
            MapState party = run.State.Party;
            if (played >= RunBudget)
            {
                return new GuidedRun(run, maps, paid, $"The run took {RunBudget} ticks and stands on '{party.Map.Id.Value}' at {party.LeadAt} with {run.State.Characters.Gold} gold, with the goal '{GoalOf(run.State)?.Thing}'.");
            }

            if (run.State.MapWiped || run.State.Battle is { Outcome: BattleOutcome.Wiped })
            {
                return new GuidedRun(run, maps, paid, $"The party wiped on '{party.Map.Id.Value}' at tick {run.Tick}. Fights: {string.Join(" | ", fights)}.");
            }

            if (!fighting && run.State.Battle is Battle battle)
            {
                fights.Add($"{battle.Group.Id.Value} on '{party.Map.Id.Value}' at {string.Join("/", run.State.Characters.Members.Select(member => $"L{member.Level} {member.Health}/{member.Stats.Health}"))}");
            }

            fighting = run.State.Battle is not null;

            IReadOnlyList<Intent> accepted = run.Accepted();
            Intent? chosen = HoldsTheWalk(run.State, accepted) ? fighter.Choose(run.State, accepted) : WalkTheRoute(run.State, accepted);
            bool paidBefore = run.State.Story.Flags.IsOn(Id("flag.town_turnkey_paid"));
            int goldBefore = run.State.Characters.Gold;
            _ = run.Step(chosen is null ? [] : [chosen]);
            if (!paidBefore && run.State.Story.Flags.IsOn(Id("flag.town_turnkey_paid")))
            {
                paid = goldBefore - run.State.Characters.Gold;
            }

            _ = run.TakeNotices();
            _ = run.TakeOpenedServices();
            _ = run.TakeSaveRequests();
            _ = run.TakeBattleEvents();

            if (string.CompareOrdinal(maps[^1], run.State.Party.Map.Id.Value) != 0)
            {
                maps.Add(run.State.Party.Map.Id.Value);
            }
        }

        return new GuidedRun(run, maps, paid, null);
    }

    private static void AssertKit(PartyMember member, string[] lessons, string[] gear)
    {
        foreach (string lesson in lessons)
        {
            Assert.Contains(lesson, Values(member.Slots));
        }

        foreach (string piece in gear)
        {
            Assert.Contains(piece, Values(member.Gear));
        }
    }

    private static Simulation Start(ContentSet content) => Start(content, Seed);

    private static Simulation Start(ContentSet content, ulong seed) =>
        Simulation.Start(seed, MapSet.Of(content.Maps), MapIds.FirstMap, content.Battle, content.Notices, content.Story, DebugIntentHandlers.None);

    /// <summary>True when each character of the party stands at full health, so the rest of the inn gives nothing more.</summary>
    private static bool AllAtFullHealth(RunState state) => state.Characters.Members.All(member => member.Health == member.Stats.Health);

    /// <summary>True when the greedy bot takes the tick: a wait intent of a story scene or a battle, a battle, or an open menu.</summary>
    private static bool HoldsTheWalk(RunState state, IReadOnlyList<Intent> accepted)
    {
        if (state.Battle is not null || state.MenuOpen)
        {
            return true;
        }

        foreach (ContentId wait in new[] { IntentIds.StoryResume, IntentIds.StoryStepEnd, IntentIds.StoryPick, IntentIds.WaitBattleEnd })
        {
            if (FirstOf(accepted, wait) is not null)
            {
                return true;
            }
        }

        return false;
    }

    /// <summary>Gives the next goal of the route on the map of the party, or no goal once each goal of the map is done.</summary>
    private static RouteGoal? GoalOf(RunState state)
    {
        foreach (RouteGoal goal in Route)
        {
            if (string.CompareOrdinal(goal.Map, state.Party.Map.Id.Value) == 0 && !goal.Done(state))
            {
                return goal;
            }
        }

        return null;
    }

    /// <summary>
    /// Walks one tick toward the next goal of the route: a step on the shortest path, a turn to a
    /// thing to confirm, or the confirm. A patrol that blocks each path takes a bump, and its fight.
    /// </summary>
    private static Intent? WalkTheRoute(RunState state, IReadOnlyList<Intent> accepted)
    {
        MapState party = state.Party;
        if (party.Stepping is not null || party.Patrols.Encounter is not null)
        {
            return null;
        }

        RouteGoal goal = GoalOf(state) ?? throw new InvalidOperationException($"The route holds no goal on '{party.Map.Id.Value}' (T-2).");
        var target = new WalkTarget(goal.Thing, TileOf(party, goal.Thing), goal.Reach);
        StepDirection? first = WalkTargets.FirstStep(party, state.Story.Flags, target, out int length);
        if (length < 0)
        {
            target = NearestPatrol(state) ?? throw new InvalidOperationException($"No path reaches '{goal.Thing}' on '{party.Map.Id.Value}' from {party.LeadAt}, and no patrol blocks it (T-2).");
            first = WalkTargets.FirstStep(party, state.Story.Flags, target, out _);
        }

        if (first is StepDirection toward)
        {
            return FirstOf(accepted, MoveOf(toward));
        }

        StepDirection facing = WalkTargets.Toward(party.LeadAt, target.At)
            ?? throw new InvalidOperationException($"The lead at {party.LeadAt} stands off '{target.Key}' at {target.At} (T-2).");
        return target.Reach == TargetReach.Confirm && party.Facing == facing ? FirstOf(accepted, IntentIds.Confirm) : FirstOf(accepted, MoveOf(facing));
    }

    /// <summary>Gives the living patrol of the map with the shortest path, as a target to bump, or no target.</summary>
    private static WalkTarget? NearestPatrol(RunState state)
    {
        WalkTarget? nearest = null;
        int shortest = int.MaxValue;
        foreach (PatrolState patrol in state.Party.Patrols.All)
        {
            var target = new WalkTarget(patrol.Patrol.Id.Value, patrol.At, TargetReach.Bump);
            _ = WalkTargets.FirstStep(state.Party, state.Story.Flags, target, out int length);
            if (!patrol.Dead && length >= 0 && length < shortest)
            {
                nearest = target;
                shortest = length;
            }
        }

        return nearest;
    }

    /// <summary>Gives the tile of a goal: a patrol, an NPC, a tile trigger, or a thing of the map.</summary>
    private static TilePoint TileOf(MapState party, string id)
    {
        if (id.StartsWith("patrol.", StringComparison.Ordinal))
        {
            return party.Patrols.All.Single(patrol => string.CompareOrdinal(patrol.Patrol.Id.Value, id) == 0).At;
        }

        if (id.StartsWith("npc.", StringComparison.Ordinal))
        {
            return party.Npcs.All.Single(npc => string.CompareOrdinal(npc.Npc.Id.Value, id) == 0).At;
        }

        if (id.StartsWith("trigger.", StringComparison.Ordinal))
        {
            return party.Map.Triggers.Single(trigger => string.CompareOrdinal(trigger.Id.Value, id) == 0).At
                ?? throw new InvalidOperationException($"The trigger '{id}' holds no tile (T-2).");
        }

        return party.Map.Things.Single(thing => string.CompareOrdinal(thing.Id.Value, id) == 0).At;
    }

    private static ContentId MoveOf(StepDirection direction) => direction switch
    {
        StepDirection.North => IntentIds.MoveNorth,
        StepDirection.South => IntentIds.MoveSouth,
        StepDirection.East => IntentIds.MoveEast,
        StepDirection.West => IntentIds.MoveWest,
        _ => throw new ArgumentOutOfRangeException(nameof(direction), direction, "The value names no step direction (D-716)."),
    };

    private static Intent? FirstOf(IReadOnlyList<Intent> accepted, ContentId action) =>
        accepted.FirstOrDefault(intent => string.CompareOrdinal(intent.Action.Value, action.Value) == 0);

    private static ContentId Id(string value) => ContentId.Parse(value, "test", "id");

    private static List<string?> Values(IReadOnlyList<ContentId?> ids) => [.. ids.Select(id => id?.Value)];

    /// <summary>The end of a guided run: the run, each map in the order of the walk, the gold of the bribe, and the text of a wipe or no value.</summary>
    private sealed record GuidedRun(Simulation Run, List<string> Maps, int Paid, string? Wipe);

    /// <summary>One goal of the route: the thing, the patrol, the NPC, or the trigger to reach on one map, and when it is done.</summary>
    private sealed record RouteGoal(string Map, string Thing, TargetReach Reach, Func<RunState, bool> Done);
}
