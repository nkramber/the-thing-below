using System.Collections.Generic;
using System.Text;
using TheThingBelow.Core.Battles;
using TheThingBelow.Core.Hashing;
using TheThingBelow.Core.Maps;
using TheThingBelow.Core.Runs;
using TheThingBelow.Core.Story;

namespace TheThingBelow.Core.Identity;

/// <summary>The story run of the identity set (D-504, D-540). PR-68 added it.</summary>
public static partial class IdentitySet
{
    /// <summary>The count of ticks that the story run steps.</summary>
    private const int StoryTickCount = 900;

    /// <summary>The flag file of the runs with no story scene. It never changes.</summary>
    private const string NoFlagFile = """
    {
     "comment": "The flag file of the identity runs with no story scene. It declares no flag.",
     "flags": []
    }
    """;

    /// <summary>The flag file of the story run. PR-68 added it, and it never changes.</summary>
    private const string StoryFlagFile = """
    {
     "comment": "The flags of the story run of the identity set.",
     "flags": [
      { "id": "flag.identity_met", "note": "The friend met the hero." },
      { "id": "flag.identity_trust", "note": "The hero trusts the friend." },
      { "id": "flag.identity_doubt", "note": "The hero doubts the friend." },
      { "id": "flag.identity_fought", "note": "The hero and the friend won the set fight." }
     ]
    }
    """;

    /// <summary>
    /// The first story scene of the story run: every kind of step except the start battle
    /// step. The map fires it on entry. It never changes.
    /// </summary>
    private const string MeetSceneFile = """
    {
     "comment": "The friend walks in, speaks, waits, asks, joins, and leaves the map.",
     "id": "scene.identity_meet",
     "steps": [
      { "kind": "show", "actor": "character.identity_friend", "at": "marker.identity_story_door", "facing": "west" },
      { "kind": "camera", "at": "marker.identity_story_door" },
      { "kind": "move", "actor": "character.identity_friend", "path": ["west", "west"] },
      { "kind": "face", "actor": "lead", "facing": "east" },
      { "kind": "say", "speaker": "character.identity_friend", "line": "line.identity_greet" },
      { "kind": "wait", "ticks": 20 },
      {
       "kind": "choose",
       "options": [
        { "line": "line.identity_yes", "flag": "flag.identity_trust" },
        { "line": "line.identity_no", "flag": "flag.identity_doubt" }
       ]
      },
      { "kind": "set_flag", "flag": "flag.identity_met" },
      { "kind": "join", "character": "character.identity_friend" },
      { "kind": "hide", "actor": "character.identity_friend" }
     ]
    }
    """;

    /// <summary>The second story scene of the story run: a set fight, then a walk and a line. It never changes.</summary>
    private const string FightSceneFile = """
    {
     "comment": "A line with no speaker, a set fight, and the step after the win.",
     "id": "scene.identity_fight",
     "steps": [
      { "kind": "say", "speaker": "none", "line": "line.identity_ambush" },
      { "kind": "start_battle", "group": "group.identity_run" },
      { "kind": "move", "actor": "lead", "path": ["east"] },
      { "kind": "say", "speaker": "lead", "line": "line.identity_after" },
      { "kind": "set_flag", "flag": "flag.identity_fought" }
     ]
    }
    """;

    /// <summary>
    /// The map of the story run. The entry fires the first story scene, and the tile at (3, 3)
    /// fires the second once the first set its flags. It never changes.
    /// </summary>
    private const string StoryMapFile = """
    {
     "comment": "The map of the story run of the identity set. PR-68 added it, and the map never changes again.",
     "id": "map.identity_story",
     "region": "region.identity",
     "label": "label.identity_story",
     "time": "day",
     "terrain": [
      "#########",
      "#.......#",
      "#.......#",
      "#.......#",
      "#########"
     ],
     "things": [
      { "id": "spawn_point.identity_story_start", "kind": "spawn_point", "x": 1, "y": 1 },
      { "id": "marker.identity_story_door", "kind": "marker", "x": 7, "y": 1 }
     ],
     "enemies": [],
     "triggers": [
      {
       "id": "trigger.identity_story_meet",
       "kind": "entry",
       "scene": "scene.identity_meet",
       "condition": { "not": { "flag": "flag.identity_met" } }
      },
      {
       "id": "trigger.identity_story_fight",
       "kind": "tile",
       "x": 3,
       "y": 3,
       "scene": "scene.identity_fight",
       "condition": {
        "all": [
         { "flag": "flag.identity_met" },
         { "any": [{ "flag": "flag.identity_trust" }, { "flag": "flag.identity_doubt" }] },
         { "not": { "flag": "flag.identity_fought" } }
        ]
       }
      }
     ]
    }
    """;

    /// <summary>The story content of the runs with no story scene, from its own copy for the reason of <see cref="ReplayBattleContent"/>.</summary>
    private static StoryContent NoStory() =>
        StoryContent.Load(FlagList.Read(Encoding.UTF8.GetBytes(NoFlagFile), "identity-set-no-flags.json"), [], ReplayBattleContent());

    /// <summary>The story content of the story run.</summary>
    private static StoryContent StoryRunContent(BattleContent content) =>
        StoryContent.Load(
            FlagList.Read(Encoding.UTF8.GetBytes(StoryFlagFile), "identity-set-flags.json"),
            [
                StoryScene.Read(Encoding.UTF8.GetBytes(MeetSceneFile), $"{StoryScene.Folder}identity-meet.json"),
                StoryScene.Read(Encoding.UTF8.GetBytes(FightSceneFile), $"{StoryScene.Folder}identity-fight.json"),
            ],
            content);

    /// <summary>
    /// Plays the two story scenes of the story run, writes the record, reads the text of it
    /// again, and replays it (exit test 1 of PR-68, G-5). The run pauses a wait step, picks an
    /// option, fights the battle of a story scene, and saves once in the pause and once in the
    /// battle, so the snapshot holds the story state in both places (D-999, D-1010).
    /// </summary>
    private static ulong ComputeStory()
    {
        GameMap map = GameMap.Read(Encoding.UTF8.GetBytes(StoryMapFile), "identity-set-story-map.json");
        BattleContent content = ReplayBattleContent();
        StoryContent story = StoryRunContent(content);
        RunHeader header = RunHeader.ForThisBuild(ReplayContentHash, RunSeed);
        Simulation simulation = Simulation.Start(RunSeed, map, content, ReplayNotices(), story, DebugIntentHandlers.None);
        RunRecorder recorder = new(header, simulation.Snapshot());
        StateHasher hasher = new();
        int turns = 0;
        bool savedPause = false;
        bool savedBattle = false;

        for (int step = 0; step < StoryTickCount; step += 1)
        {
            IReadOnlyList<Intent> intents = IntentsOfStoryTick(simulation.State, turns);
            if (intents.Count > 0 && simulation.State.Battle is not null)
            {
                turns += 1;
            }

            simulation.Step(intents);
            recorder.Step(simulation.Tick, intents);
            foreach (BattleEvent battleEvent in simulation.TakeBattleEvents())
            {
                hasher.AddText(battleEvent.Describe());
            }

            if (!savedPause && simulation.State.Story.Paused)
            {
                recorder.Save(simulation.Snapshot());
                savedPause = true;
            }

            if (!savedBattle && turns == 2)
            {
                recorder.Save(simulation.Snapshot());
                savedBattle = true;
            }
        }

        string text = RunRecordText.Write(recorder.Build());
        RunState replayed = RunReplay.Play(
            RunRecordText.Read(text), ReplayContentHash, map, content, ReplayNotices(), story, DebugIntentHandlers.None);

        hasher.AddUInt64(simulation.StateHash());
        hasher.AddUInt64(replayed.StateHash());
        hasher.AddText(text);
        return hasher.Finish();
    }

    /// <summary>
    /// The script of the story run. It reads the state, and the record holds each intent, so
    /// the replay needs no script (D-493). It answers each wait intent at once, as a bot does
    /// (D-540), picks the first option, and pauses the wait step once.
    /// </summary>
    /// <remarks>
    /// The pause starts when the wait holds 10 ticks, and it ends on a tick that is a multiple
    /// of 8. The world step of that tick counts the wait on to 9, so the script pauses once.
    /// </remarks>
    private static IReadOnlyList<Intent> IntentsOfStoryTick(RunState state, int turns)
    {
        if (state.Battle is not null)
        {
            return IntentsOfBattleTick(state, turns);
        }

        StoryState story = state.Story;
        if (story.Paused)
        {
            return (state.Tick + 1) % 8 == 0 ? [Intent.OfPlayer(IntentIds.StoryResume)] : [];
        }

        if (story.Running)
        {
            return story.Phase switch
            {
                ScenePhase.WaitIntent => [Intent.OfPlayer(IntentIds.StoryStepEnd)],
                ScenePhase.Pick => [Intent.OfPick(0)],
                ScenePhase.Ticks when story.TicksLeft == 10 => [Intent.OfPlayer(IntentIds.StoryPause)],
                _ => [],
            };
        }

        // A move intent on the tick of an arrival chains the next step, so the script moves
        // only while the lead stands (D-716).
        TilePoint lead = state.Party.LeadAt;
        if (state.Party.Stepping is not null)
        {
            return [];
        }

        if (lead.Y < 3)
        {
            return [Intent.OfPlayer(IntentIds.MoveSouth)];
        }

        return lead.X < 3 ? [Intent.OfPlayer(IntentIds.MoveEast)] : [];
    }
}
