using System.Collections.Generic;
using System.Text;
using TheThingBelow.Core.Battles;
using TheThingBelow.Core.Content;
using TheThingBelow.Core.Maps;
using TheThingBelow.Core.Runs;
using TheThingBelow.Core.Story;

namespace TheThingBelow.Tests;

/// <summary>
/// The story fixture of the tests: a flag file, three story scenes, and a map that fires each
/// one (D-540, D-997, D-1004). Each file is read by the one reader of Core (T-1).
/// </summary>
/// <remarks>
/// The entry of the map plays the meeting, which uses every kind of step but the start battle
/// step. The tile at (3, 3) then plays the fight, and a win against the guard of the map plays
/// the victory. The ally of the meeting joins the party, so a test reads the join too (D-563).
/// </remarks>
public static class TestStory
{
    /// <summary>The tile of the tile trigger of the fight.</summary>
    public static readonly TilePoint FightTile = new(3, 3);

    /// <summary>The tile of the door marker, where the ally appears.</summary>
    public static readonly TilePoint DoorTile = new(7, 1);

    /// <summary>The flag file of the fixture.</summary>
    public const string FlagsFile = """
    {
     "comment": "The flags of the story fixture of the tests.",
     "flags": [
      { "id": "flag.test_met", "note": "The ally met Marrek." },
      { "id": "flag.test_yes", "note": "Marrek said yes." },
      { "id": "flag.test_no", "note": "Marrek said no." },
      { "id": "flag.test_done", "note": "The set fight ended." },
      { "id": "flag.test_victor", "note": "The party beat the guard." },
      { "id": "flag.test_marrek_side", "note": "The side aptitude of Marrek is open." },
      { "id": "flag.test_second_side", "note": "The side aptitude of the second character is open." },
      { "id": "flag.test_third_side", "note": "The side aptitude of the third character is open." },
      { "id": "flag.test_fourth_side", "note": "The side aptitude of the fourth character is open." }
     ]
    }
    """;

    /// <summary>The meeting: every kind of step but the start battle step.</summary>
    public const string MeetFile = """
    {
     "comment": "The ally walks in, speaks, waits, asks, joins, and leaves the map.",
     "id": "scene.test_meet",
     "steps": [
      { "id": "step.show_ally", "kind": "show", "actor": "character.test_second", "at": "marker.test_story_door", "facing": "west" },
      { "id": "step.look_door", "kind": "camera", "at": "marker.test_story_door" },
      { "id": "step.ally_walks", "kind": "move", "actor": "character.test_second", "path": ["west", "west"] },
      { "id": "step.lead_turns", "kind": "face", "actor": "lead", "facing": "east" },
      { "id": "step.greet", "kind": "say", "speaker": "character.test_second", "line": "line.test_greet" },
      { "id": "step.pause", "kind": "wait", "ticks": 5 },
      {
       "id": "step.ask", "kind": "choose",
       "options": [
        { "line": "line.test_yes", "flag": "flag.test_yes" },
        { "line": "line.test_no", "flag": "flag.test_no" }
       ]
      },
      { "id": "step.met", "kind": "set_flag", "flag": "flag.test_met" },
      { "id": "step.ally_joins", "kind": "join", "character": "character.test_second" },
      { "id": "step.ally_leaves", "kind": "hide", "actor": "character.test_second" }
     ]
    }
    """;

    /// <summary>The fight: a line, a set fight, and the steps after the win.</summary>
    public const string FightFile = """
    {
     "comment": "A line with no speaker, a set fight, a step, and a line of the lead.",
     "id": "scene.test_fight",
     "steps": [
      { "id": "step.ambush", "kind": "say", "speaker": "none", "line": "line.test_ambush" },
      { "id": "step.fight", "kind": "start_battle", "group": "group.one" },
      { "id": "step.lead_steps", "kind": "move", "actor": "lead", "path": ["east"] },
      { "id": "step.after", "kind": "say", "speaker": "lead", "line": "line.test_after" },
      { "id": "step.done", "kind": "set_flag", "flag": "flag.test_done" }
     ]
    }
    """;

    /// <summary>The victory over the guard of the map: one flag.</summary>
    public const string VictoryFile = """
    {
     "comment": "The party beat the guard.",
     "id": "scene.test_victory",
     "steps": [
      { "id": "step.victor", "kind": "set_flag", "flag": "flag.test_victor" }
     ]
    }
    """;

    /// <summary>The map of the fixture, with the three triggers.</summary>
    public const string MapFile = """
    {
     "comment": "A room with a door marker, a guard in the south-east corner, and three story scene triggers.",
     "id": "map.test_story",
     "region": "region.test",
     "label": "label.test_story",
     "time": "day",
     "dark": false,
     "kind": "dungeon", "npcs": [], "services": [],
     "terrain": [
      "##########",
      "#........#",
      "#........#",
      "#........#",
      "#........#",
      "##########"
     ],
     "things": [
      { "id": "spawn_point.test_story_start", "kind": "spawn_point", "x": 1, "y": 1 },
      { "id": "marker.test_story_door", "kind": "marker", "x": 7, "y": 1 }
     ],
     "enemies": [
      {
       "id": "patrol.test_story_guard",
       "group": "group.one",
       "size": "common",
       "facing": "west",
       "step_ticks": 32,
       "sight_range": 0,
       "routes": [{ "times": ["dawn", "day", "dusk", "night"], "tiles": [{ "x": 8, "y": 4 }] }]
      }
     ],
     "triggers": [
      {
       "id": "trigger.test_story_meet",
       "kind": "entry",
       "scene": "scene.test_meet",
       "condition": { "not": { "flag": "flag.test_met" } }
      },
      {
       "id": "trigger.test_story_fight",
       "kind": "tile",
       "x": 3,
       "y": 3,
       "scene": "scene.test_fight",
       "condition": { "all": [{ "flag": "flag.test_met" }, { "not": { "flag": "flag.test_done" } }] }
      },
      {
       "id": "trigger.test_story_victory",
       "kind": "battle_end",
       "patrol": "patrol.test_story_guard",
       "scene": "scene.test_victory",
       "condition": { "always": true }
      }
     ]
    }
    """;

    /// <summary>The string entries of every line of the fixture, for the string table of a test content set (G-7).</summary>
    public const string Strings = """
    { "id": "line.test_greet", "text": "You came." },
    { "id": "line.test_yes", "text": "Yes." },
    { "id": "line.test_no", "text": "No." },
    { "id": "line.test_ambush", "text": "Steel, in the dark." },
    { "id": "line.test_after", "text": "Move." },
    { "id": "label.test_story", "text": "The story room" }
    """;

    /// <summary>The flag file, as a run reads it.</summary>
    public static readonly FlagList Flags = FlagList.Read(Encoding.UTF8.GetBytes(FlagsFile), FlagList.Path);

    /// <summary>The map, as a run reads it.</summary>
    public static readonly GameMap Map = GameMap.Read(Encoding.UTF8.GetBytes(MapFile), "rules/maps/test-story.json");

    /// <summary>The story content of the fixture, with the battle content of the tests.</summary>
    public static readonly StoryContent Content = ContentOf(TestBattles.Content);

    /// <summary>The id of the meeting.</summary>
    public static readonly ContentId Meet = Id("scene.test_meet");

    /// <summary>The id of the fight.</summary>
    public static readonly ContentId Fight = Id("scene.test_fight");

    /// <summary>The id of the ally who joins.</summary>
    public static readonly ContentId Ally = Id("character.test_second");

    /// <summary>Gives the story content of the fixture against one battle content.</summary>
    /// <param name="battle">The battle content, which holds the ally and the group.</param>
    /// <returns>The story content.</returns>
    public static StoryContent ContentOf(BattleContent battle) =>
        StoryContent.Load(Flags, [Scene(MeetFile, "meet"), Scene(FightFile, "fight"), Scene(VictoryFile, "victory")], battle);

    /// <summary>Reads one story scene file of the tests.</summary>
    /// <param name="text">The text of the file.</param>
    /// <param name="name">The name of the file, without the folder and the type.</param>
    /// <returns>The story scene.</returns>
    public static StoryScene Scene(string text, string name) =>
        StoryScene.Read(Encoding.UTF8.GetBytes(text), $"{StoryScene.Folder}{name}.json");

    /// <summary>Starts a run on the story map with the battle content of the tests.</summary>
    /// <param name="seed">The seed.</param>
    /// <returns>The run at tick zero.</returns>
    public static Simulation Start(ulong seed) =>
        Simulation.Start(seed, Map, TestBattles.Content, TestBattles.Notices, Content, DebugIntentHandlers.None);

    /// <summary>Starts the run again from a snapshot of the story map.</summary>
    /// <param name="seed">The seed.</param>
    /// <param name="snapshot">The snapshot.</param>
    /// <returns>The run.</returns>
    public static Simulation Resume(ulong seed, RunSnapshot snapshot) =>
        Simulation.Resume(seed, snapshot, Map, TestBattles.Content, TestBattles.Notices, Content, DebugIntentHandlers.None);

    /// <summary>
    /// Gives the intents of a bot for the next tick (D-540): each wait intent at once, the first
    /// option of each choice, an attack on each turn, and a walk to the tile of the fight.
    /// </summary>
    /// <param name="state">The run, before the step.</param>
    /// <returns>The intents.</returns>
    public static IReadOnlyList<Intent> BotIntents(RunState state)
    {
        if (state.Battle is Battle battle)
        {
            if (battle.Outcome == BattleOutcome.Won)
            {
                return [Intent.OfPlayer(IntentIds.WaitBattleEnd)];
            }

            if (battle.Outcome != BattleOutcome.Running || battle.Next() is not Combatant next || next.Side != BattleSide.Party)
            {
                return [];
            }

            return [Intent.OfPlayer(IntentIds.BattleAttack, battle.MeleeTargets(BattleSide.Enemy)[0].Target, null)];
        }

        StoryState story = state.Story;
        if (story.Running)
        {
            return story.Phase switch
            {
                ScenePhase.WaitIntent => [Intent.OfPlayer(IntentIds.StoryStepEnd)],
                ScenePhase.Pick => [Intent.OfPick(0)],
                _ => [],
            };
        }

        // A move intent on the tick of an arrival chains the next step, so the bot moves only
        // while the lead stands (D-716).
        TilePoint lead = state.Party.LeadAt;
        if (state.Party.Stepping is not null)
        {
            return [];
        }

        if (lead.Y < FightTile.Y)
        {
            return [Intent.OfPlayer(IntentIds.MoveSouth)];
        }

        return lead.X < FightTile.X ? [Intent.OfPlayer(IntentIds.MoveEast)] : [];
    }

    private static ContentId Id(string value) => ContentId.Parse(value, "test", "id");
}
