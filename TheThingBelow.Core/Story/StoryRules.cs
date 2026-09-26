using System;
using System.Collections.Generic;
using TheThingBelow.Core.Battles;
using TheThingBelow.Core.Content;
using TheThingBelow.Core.Logging;
using TheThingBelow.Core.Maps;
using TheThingBelow.Core.Runs;

namespace TheThingBelow.Core.Story;

/// <summary>
/// The story scene runner: the triggers of a map, each step of a story scene, and the four
/// intents of a story scene (D-540, D-1004). Core runs each step, and Game draws it.
/// </summary>
/// <remarks>
/// The world step reads the triggers in one fixed order: the entry of the map, then a win
/// against a patrol, then the tile that the party reached, and then the talk of a confirm. A
/// trigger fires when its event happens and its condition holds, and the first trigger of the map
/// file that fires wins (D-1004, D-1131, G-4). One story scene runs at a time.
/// <para>
/// While a story scene runs, the map holds still: no patrol walks or sees, the beat of a mark
/// stops, and each NPC stands still except for a step of the story scene (D-1009, D-1139). A move
/// step and a face step can name an NPC of the map, which acts where it stands, and a show step
/// can put a scene-only NPC on a marker (D-1006). An NPC of the map that a move step leaves outside
/// its home walks home after the story scene (D-1140). A step that changes the run at once runs in the same tick as the step
/// before it. A step that Game animates waits for the wait intent, a wait step counts its
/// ticks, and a choose step waits for the pick of the player (D-1000, D-1007, D-1013).
/// </para>
/// <para>
/// The pause of the player stops the world step, so a wait step stops its count (D-1010). The
/// menu stays closed while a story scene runs (D-1009).
/// </para>
/// </remarks>
public static class StoryRules
{
    /// <summary>Fires the entry trigger and the battle end trigger that wait for this world step (D-1004, D-1011).</summary>
    /// <param name="state">The run, with no story scene and no battle.</param>
    /// <param name="log">The log entries of this tick (D-179).</param>
    /// <returns>True when a story scene started.</returns>
    /// <exception cref="ArgumentNullException">An argument is null (T-2).</exception>
    /// <exception cref="SimulationException">A step of the story scene breaks a rule (T-2).</exception>
    /// <remarks>
    /// An entry that starts a story scene leaves a win in wait, and the world step after that
    /// story scene reads it. Thus no event passes in silence.
    /// </remarks>
    public static bool FireWaiting(RunState state, List<LogEntry> log)
    {
        ArgumentNullException.ThrowIfNull(state);
        ArgumentNullException.ThrowIfNull(log);

        if (state.Story.TakeEntry() && TryFire(state, TriggerKind.Entry, null, null, null, log))
        {
            return true;
        }

        return state.Story.TakeWin() is ContentId patrol && TryFire(state, TriggerKind.BattleEnd, null, patrol, null, log);
    }

    /// <summary>Fires the tile trigger of the tile that the party reached (D-1004).</summary>
    /// <param name="state">The run, with no story scene and no battle.</param>
    /// <param name="at">The tile.</param>
    /// <param name="log">The log entries of this tick (D-179).</param>
    /// <returns>True when a story scene started.</returns>
    /// <exception cref="ArgumentNullException">An argument is null (T-2).</exception>
    /// <exception cref="SimulationException">A step of the story scene breaks a rule (T-2).</exception>
    public static bool FireTile(RunState state, TilePoint at, List<LogEntry> log)
    {
        ArgumentNullException.ThrowIfNull(state);
        ArgumentNullException.ThrowIfNull(log);

        return TryFire(state, TriggerKind.Tile, at, null, null, log);
    }

    /// <summary>Fires the talk trigger of one NPC that the lead talks with (D-1005, D-1131).</summary>
    /// <param name="state">The run, with no story scene and no battle.</param>
    /// <param name="npc">The id of the NPC, which the map places.</param>
    /// <param name="log">The log entries of this tick (D-179).</param>
    /// <returns>True when a story scene started.</returns>
    /// <exception cref="ArgumentNullException">An argument is null (T-2).</exception>
    /// <exception cref="SimulationException">A story scene runs, or a step of the story scene breaks a rule (T-2).</exception>
    /// <remarks>The confirm rule of the map calls it after the NPC turned to the lead (D-1139).</remarks>
    public static bool FireTalk(RunState state, ContentId npc, List<LogEntry> log)
    {
        ArgumentNullException.ThrowIfNull(state);
        ArgumentNullException.ThrowIfNull(npc);
        ArgumentNullException.ThrowIfNull(log);

        if (state.Story.Running)
        {
            throw new SimulationException($"a talk with the NPC '{npc.Value}' while a story scene runs. {Describe(state.Story)} (D-1009)", state.Context("story"));
        }

        return TryFire(state, TriggerKind.Talk, null, null, npc, log);
    }

    /// <summary>Runs the story scene for one world tick: the count of a wait step, then each step that is ready (D-540).</summary>
    /// <param name="state">The run, with a story scene and no battle.</param>
    /// <param name="log">The log entries of this tick (D-179).</param>
    /// <exception cref="ArgumentNullException">An argument is null (T-2).</exception>
    /// <exception cref="SimulationException">No story scene runs, or a step breaks a rule (T-2).</exception>
    public static void Advance(RunState state, List<LogEntry> log)
    {
        ArgumentNullException.ThrowIfNull(state);
        ArgumentNullException.ThrowIfNull(log);

        StoryState story = state.Story;
        StoryScene scene = story.Scene
            ?? throw new SimulationException("a step of a story scene, and no story scene runs (D-540)", state.Context("story"));

        if (story.Phase == ScenePhase.Ticks)
        {
            if (!story.CountTick())
            {
                return;
            }

            story.Next();
        }

        while (story.Phase == ScenePhase.Ready)
        {
            if (story.Step == scene.Steps.Count)
            {
                story.End();
                log.Add(Entry(state, LogLevel.Info, "a story scene ended", [new LogField("scene", scene.Id.Value)]));
                return;
            }

            RunStep(state, scene, story.Step, log);
        }
    }

    /// <summary>Ends the step that waits for Game, from the wait intent (D-540, D-1000).</summary>
    /// <param name="state">The run.</param>
    /// <param name="context">The seed, the tick, and the intent, for an error (T-2).</param>
    /// <param name="log">The log entries of this tick (D-179).</param>
    /// <exception cref="ArgumentNullException">An argument is null (T-2).</exception>
    /// <exception cref="SimulationException">No step waits for the wait intent (T-2).</exception>
    /// <remarks>The next step runs at the world step of the same tick.</remarks>
    public static void EndStep(RunState state, RunContext context, List<LogEntry> log)
    {
        ArgumentNullException.ThrowIfNull(state);
        ArgumentNullException.ThrowIfNull(context);
        ArgumentNullException.ThrowIfNull(log);

        StoryState story = state.Story;
        if (story.Scene is not StoryScene scene || story.Phase != ScenePhase.WaitIntent)
        {
            throw new SimulationException($"the wait intent of a story scene step, and no step waits for it. {Describe(story)} (D-1000)", context);
        }

        log.Add(Entry(state, LogLevel.Debug, "a step of a story scene ended", [new LogField("scene", scene.Id.Value), LogField.OfNumber("step", story.Step)]));
        story.Next();
    }

    /// <summary>Takes the pick of the player in a choose step, and turns on the flag of the option (D-1007).</summary>
    /// <param name="state">The run.</param>
    /// <param name="option">The index of the option, from zero.</param>
    /// <param name="context">The seed, the tick, and the intent, for an error (T-2).</param>
    /// <param name="log">The log entries of this tick (D-179).</param>
    /// <exception cref="ArgumentNullException">An argument is null (T-2).</exception>
    /// <exception cref="SimulationException">No choose step waits, or the choice holds no such option (T-2).</exception>
    public static void Pick(RunState state, int option, RunContext context, List<LogEntry> log)
    {
        ArgumentNullException.ThrowIfNull(state);
        ArgumentNullException.ThrowIfNull(context);
        ArgumentNullException.ThrowIfNull(log);

        StoryState story = state.Story;
        if (story.Scene is not StoryScene scene || story.Phase != ScenePhase.Pick || scene.Steps[story.Step] is not ChooseStep choose)
        {
            throw new SimulationException($"a pick of a story scene, and no choose step waits for it. {Describe(story)} (D-1007)", context);
        }

        if (option < 0 || option >= choose.Options.Count)
        {
            throw new SimulationException($"a pick of option {option}, and the choice holds the options 0 to {choose.Options.Count - 1} (D-1007)", context);
        }

        ContentId flag = choose.Options[option].Flag;
        bool changed = story.Flags.TurnOn(flag);
        log.Add(Entry(
            state,
            LogLevel.Info,
            "the player picked an option of a story scene",
            [new LogField("scene", scene.Id.Value), LogField.OfNumber("option", option), new LogField("flag", flag.Value), new LogField("changed", changed ? "yes" : "no")]));
        story.Next();
    }

    /// <summary>Pauses the story scene that runs, from the start button (D-1009, D-1010).</summary>
    /// <param name="state">The run.</param>
    /// <param name="context">The seed, the tick, and the intent, for an error (T-2).</param>
    /// <exception cref="ArgumentNullException">An argument is null (T-2).</exception>
    /// <exception cref="SimulationException">No story scene runs, a pause holds, or a battle of the story scene runs (T-2).</exception>
    /// <remarks>A battle of a story scene is a battle, and the pause holds the story scene alone (D-1010).</remarks>
    public static void Pause(RunState state, RunContext context)
    {
        ArgumentNullException.ThrowIfNull(state);
        ArgumentNullException.ThrowIfNull(context);

        StoryState story = state.Story;
        if (!story.Running || story.Paused || story.Phase == ScenePhase.Battle)
        {
            throw new SimulationException($"a pause, and the pause holds a story scene out of battle that is not paused. {Describe(story)} (D-1010)", context);
        }

        story.SetPaused(true);
    }

    /// <summary>Ends the pause of the player (D-1010).</summary>
    /// <param name="state">The run.</param>
    /// <param name="context">The seed, the tick, and the intent, for an error (T-2).</param>
    /// <exception cref="ArgumentNullException">An argument is null (T-2).</exception>
    /// <exception cref="SimulationException">No pause holds (T-2).</exception>
    public static void Resume(RunState state, RunContext context)
    {
        ArgumentNullException.ThrowIfNull(state);
        ArgumentNullException.ThrowIfNull(context);

        if (!state.Story.Paused)
        {
            throw new SimulationException("the end of a pause, and no pause holds (D-1010)", context);
        }

        state.Story.SetPaused(false);
    }

    /// <summary>Lets the story scene go on after the win of its battle (D-999).</summary>
    /// <param name="state">The run.</param>
    /// <param name="context">The seed, the tick, and the intent, for an error (T-2).</param>
    /// <exception cref="SimulationException">No story scene waits for a battle (T-2).</exception>
    internal static void FinishBattle(RunState state, RunContext context)
    {
        if (state.Story.Phase != ScenePhase.Battle)
        {
            throw new SimulationException($"the end of a battle of a story scene, and no story scene waits for a battle. {Describe(state.Story)} (D-999)", context);
        }

        state.Story.Next();
    }

    private static bool TryFire(RunState state, TriggerKind kind, TilePoint? at, ContentId? patrol, ContentId? npc, List<LogEntry> log)
    {
        foreach (SceneTrigger trigger in state.Party.Map.Triggers)
        {
            if (trigger.Kind != kind
                || (at is TilePoint tile && trigger.At != tile)
                || (patrol is ContentId won && string.CompareOrdinal(trigger.Patrol?.Value, won.Value) != 0)
                || (npc is ContentId talker && string.CompareOrdinal(trigger.Npc?.Value, talker.Value) != 0)
                || !trigger.Condition.Holds(state.Story.Flags))
            {
                continue;
            }

            StoryScene scene = state.Story.Content.Scene(trigger.Scene);
            state.Party.HoldStill();
            state.Story.Begin(scene);
            log.Add(Entry(
                state,
                LogLevel.Info,
                "a story scene started",
                [new LogField("scene", scene.Id.Value), new LogField("trigger", trigger.Id.Value), new LogField("kind", TriggerKinds.NameOf(kind))]));
            Advance(state, log);
            return true;
        }

        return false;
    }

    private static void RunStep(RunState state, StoryScene scene, int index, List<LogEntry> log)
    {
        StoryState story = state.Story;
        RunContext context = state.Context($"story/{scene.Id.Value}/steps[{index}]");
        switch (scene.Steps[index])
        {
            case MoveStep move:
                Move(state, move, context);
                story.Hold(ScenePhase.WaitIntent, 0);
                break;
            case FaceStep face:
                Face(state, face, context);
                story.Hold(ScenePhase.WaitIntent, 0);
                break;
            case WaitStep wait:
                story.Hold(ScenePhase.Ticks, wait.Ticks);
                break;
            case SayStep:
            case CameraStep:
                story.Hold(ScenePhase.WaitIntent, 0);
                break;
            case ChooseStep:
                story.Hold(ScenePhase.Pick, 0);
                break;
            case SetFlagStep set:
                bool changed = story.Flags.TurnOn(set.Flag);
                log.Add(Entry(state, LogLevel.Info, "a story scene set a flag", [new LogField("flag", set.Flag.Value), new LogField("changed", changed ? "yes" : "no")]));
                story.Next();
                break;
            case JoinStep join:
                state.Characters.Join(state.BattleContent.Character(join.Character), state.BattleContent.Rules, context);
                log.Add(Entry(state, LogLevel.Info, "a cast member joined the party", [new LogField("character", join.Character.Value)]));
                story.Next();
                break;
            case ShowStep show:
                if (SceneActor.IsNpcId(show.Actor) && state.Party.Map.PlacesNpc(show.Actor))
                {
                    throw new SimulationException($"a show of the NPC '{show.Actor.Value}', which the map '{state.Party.Map.Id.Value}' places, and a show puts a scene-only NPC alone on the map (D-1006)", context);
                }

                TilePoint at = MarkerTile(state, show.Marker, context);
                RequireFree(state, at, context);
                story.Show(show.Actor, at, show.Facing);
                story.Next();
                break;
            case HideStep hide:
                if (!story.TryActor(hide.Actor, out _))
                {
                    throw new SimulationException($"a hide of '{hide.Actor.Value}', who is not on the map (D-1006)", context);
                }

                story.Hide(hide.Actor);
                story.Next();
                break;
            case StartBattleStep start:
                story.Hold(ScenePhase.Battle, 0);
                BattleTurns.BeginStory(state, scene.Id, start.Group, log);
                break;
            default:
                throw new SimulationException($"the step '{SceneStepKinds.NameOf(scene.Steps[index].Kind)}' has no rule (T-2)", context);
        }
    }

    /// <summary>
    /// Walks an actor along the path of a move step, one tile at a time (D-1012): the lead, a shown
    /// actor, or an NPC of the map (D-1006). An NPC of the map then settles: outside its home, it
    /// walks home after the story scene (D-1140).
    /// </summary>
    private static void Move(RunState state, MoveStep move, RunContext context)
    {
        if (move.Actor.Id is not ContentId id)
        {
            foreach (StepDirection direction in move.Path)
            {
                RequireFree(state, state.Party.LeadAt.Step(direction), context);
                state.Party.WalkInScene(direction);
            }

            return;
        }

        if (state.Story.TryActor(id, out ActorValues? shown))
        {
            TilePoint at = shown!.At;
            foreach (StepDirection direction in move.Path)
            {
                at = at.Step(direction);
                RequireFree(state, at, context);

                // The tile of each step holds the actor at once, so no second actor can stand on a
                // tile that this path crossed.
                state.Story.Place(id, at, direction);
            }

            return;
        }

        NpcState npc = NpcOf(state, id, context);
        foreach (StepDirection direction in move.Path)
        {
            TilePoint next = npc.At.Step(direction);
            RequireFree(state, next, context);
            npc.MoveInScene(next, direction);
        }

        npc.SettleAfterScene();
    }

    private static void Face(RunState state, FaceStep face, RunContext context)
    {
        if (face.Actor.Id is not ContentId id)
        {
            state.Party.FaceInScene(face.Facing);
            return;
        }

        if (state.Story.TryActor(id, out ActorValues? shown))
        {
            state.Story.Place(id, shown!.At, face.Facing);
            return;
        }

        NpcOf(state, id, context).Turn(face.Facing);
    }

    /// <summary>Finds the NPC of the map that a step names, when no show step put it on the map (D-1006).</summary>
    private static NpcState NpcOf(RunState state, ContentId id, RunContext context)
    {
        return SceneActor.IsNpcId(id) && state.Party.Npcs.TryFind(id, out NpcState? npc)
            ? npc!
            : throw new SimulationException($"a step of '{id.Value}', who is not on the map (D-1006)", context);
    }

    private static TilePoint MarkerTile(RunState state, ContentId marker, RunContext context)
    {
        return state.Party.Map.TryMarker(marker, out TilePoint at)
            ? at
            : throw new SimulationException($"the marker '{marker.Value}', which the map '{state.Party.Map.Id.Value}' does not hold (D-1006)", context);
    }

    /// <summary>
    /// Refuses a tile that an actor cannot stand on: a wall, a solid thing, an enemy, the lead, an
    /// NPC of the map, or a shown actor (D-1012, D-1139, T-2).
    /// </summary>
    private static void RequireFree(RunState state, TilePoint at, RunContext context)
    {
        string? blocker = null;
        if (!MapRules.CanEnter(state.Party.Map, at))
        {
            blocker = "ground that no actor walks on";
        }
        else if (state.Party.Patrols.TryEnemyAt(at, out PatrolState? enemy))
        {
            blocker = $"the enemy '{enemy!.Patrol.Id.Value}'";
        }
        else if (state.Party.LeadAt == at)
        {
            blocker = "the lead";
        }
        else if (state.Party.Npcs.TryNpcAt(at, out NpcState? npc))
        {
            blocker = $"the NPC '{npc!.Npc.Id.Value}'";
        }
        else if (state.Story.ActorStandsAt(at))
        {
            blocker = "a shown actor";
        }

        if (blocker is not null)
        {
            throw new SimulationException($"an actor on the tile {at}, which holds {blocker} (D-1012)", context);
        }
    }

    private static string Describe(StoryState story)
    {
        return story.Scene is StoryScene scene
            ? $"The story scene '{scene.Id.Value}' stands at step {story.Step} in the phase '{ScenePhases.NameOf(story.Phase)}'."
            : "No story scene runs.";
    }

    private static LogEntry Entry(RunState state, LogLevel level, string message, IReadOnlyList<LogField> fields) =>
        new(level, message, state.Tick, LogSubsystems.Story, fields);
}
