using System;
using System.Collections.Generic;
using TheThingBelow.Core.Content;
using TheThingBelow.Core.Hashing;
using TheThingBelow.Core.Logging;
using TheThingBelow.Core.Maps;
using TheThingBelow.Core.Runs;

namespace TheThingBelow.Core.Story;

/// <summary>Where the story scene that runs stands in its step (D-540).</summary>
public enum ScenePhase
{
    /// <summary>The step at the index runs at the next world step. An index past the last step ends the story scene there.</summary>
    Ready,

    /// <summary>The step waits for the wait intent of Game (D-1000).</summary>
    WaitIntent,

    /// <summary>Core counts the ticks of a wait step (D-1000).</summary>
    Ticks,

    /// <summary>The step waits for the pick intent of the player (D-1007).</summary>
    Pick,

    /// <summary>The battle of a start battle step runs (D-998, D-999).</summary>
    Battle,
}

/// <summary>The names of the phases, for a snapshot and an error (T-2).</summary>
public static class ScenePhases
{
    /// <summary>Every phase, in one fixed order for a walk of them (G-4).</summary>
    public static readonly ScenePhase[] All = [ScenePhase.Ready, ScenePhase.WaitIntent, ScenePhase.Ticks, ScenePhase.Pick, ScenePhase.Battle];

    /// <summary>Gives the name of one phase.</summary>
    /// <param name="phase">The phase.</param>
    /// <returns>The name, such as `wait_intent`.</returns>
    /// <exception cref="ArgumentOutOfRangeException">The value names no phase (T-2).</exception>
    public static string NameOf(ScenePhase phase) => phase switch
    {
        ScenePhase.Ready => "ready",
        ScenePhase.WaitIntent => "wait_intent",
        ScenePhase.Ticks => "ticks",
        ScenePhase.Pick => "pick",
        ScenePhase.Battle => "battle",
        _ => throw new ArgumentOutOfRangeException(nameof(phase), phase, "the value names no story scene phase (D-540)"),
    };

    /// <summary>Gives the phase of one name.</summary>
    /// <param name="name">The name.</param>
    /// <param name="phase">The phase of that name, when the name names one.</param>
    /// <returns>True when the name names a phase.</returns>
    /// <exception cref="ArgumentNullException">The name is null (T-2).</exception>
    public static bool TryOf(string name, out ScenePhase phase)
    {
        ArgumentNullException.ThrowIfNull(name);

        foreach (ScenePhase candidate in All)
        {
            if (string.CompareOrdinal(NameOf(candidate), name) == 0)
            {
                phase = candidate;
                return true;
            }
        }

        phase = ScenePhase.Ready;
        return false;
    }
}

/// <summary>One cast member that a show step put on the map (D-1006).</summary>
/// <param name="Character">The cast member.</param>
/// <param name="At">The tile.</param>
/// <param name="Facing">The direction that the cast member faces.</param>
public sealed record ActorValues(ContentId Character, TilePoint At, StepDirection Facing);

/// <summary>The stored values of the story scene that runs (D-540, D-166).</summary>
/// <param name="Scene">The id of the story scene.</param>
/// <param name="Step">The index of the step now.</param>
/// <param name="StepId">
/// The id of the step now, or <see cref="StoryScene.EndStepId"/> past the last step. The value
/// is absent in a snapshot before save format 14, whose resume reads the index alone (D-1112).
/// </param>
/// <param name="Phase">Where the step stands.</param>
/// <param name="TicksLeft">The ticks of a wait step that remain, and zero in every other phase.</param>
/// <param name="Actors">The shown cast members, in the order of their shows.</param>
public sealed record SceneValues(ContentId Scene, int Step, ContentId? StepId, ScenePhase Phase, int TicksLeft, IReadOnlyList<ActorValues> Actors);

/// <summary>The stored values of the story state (D-540, D-542, D-166).</summary>
/// <param name="Flags">The flags that are on, in ordinal order.</param>
/// <param name="Scene">The story scene that runs, or no value.</param>
/// <param name="Paused">True while the player paused the story scene (D-1010).</param>
/// <param name="EntryPending">True until the first world step on the map reads its entry triggers (D-1004).</param>
/// <param name="WonPatrol">The patrol of a win whose battle end triggers the next world step reads, or no value (D-1011).</param>
public sealed record StoryValues(IReadOnlyList<ContentId> Flags, SceneValues? Scene, bool Paused, bool EntryPending, ContentId? WonPatrol);

/// <summary>
/// The story state of a run: the flags that are on, the story scene that runs, and the two
/// events that fire a trigger at the next world step (D-540, D-542, D-1004).
/// </summary>
/// <remarks>
/// Core holds the step index and every flag, and Game draws each step (D-540). The snapshot
/// holds every value, so a replay reproduces every flag and every step (G-5). Each shown cast
/// member leaves at the end of its story scene, so the actors live inside the story scene
/// that runs (D-1006).
/// </remarks>
public sealed class StoryState
{
    private readonly List<ShownActor> actors = [];

    private StoryState(StoryContent content, FlagSet flags, bool entryPending)
    {
        this.Content = content;
        this.Flags = flags;
        this.EntryPending = entryPending;
    }

    /// <summary>The story content of this build.</summary>
    public StoryContent Content { get; }

    /// <summary>The flags that are on (D-542).</summary>
    public FlagSet Flags { get; }

    /// <summary>The story scene that runs, or no value.</summary>
    public StoryScene? Scene { get; private set; }

    /// <summary>The index of the step now. Zero while no story scene runs.</summary>
    public int Step { get; private set; }

    /// <summary>Where the step stands. Ready while no story scene runs.</summary>
    public ScenePhase Phase { get; private set; }

    /// <summary>The ticks of a wait step that remain (D-1000).</summary>
    public int TicksLeft { get; private set; }

    /// <summary>True while the player paused the story scene (D-1009, D-1010).</summary>
    public bool Paused { get; private set; }

    /// <summary>True until the first world step on the map reads its entry triggers (D-1004).</summary>
    public bool EntryPending { get; private set; }

    /// <summary>The patrol of a win whose battle end triggers the next world step reads (D-1011).</summary>
    public ContentId? WonPatrol { get; private set; }

    /// <summary>True while a story scene runs, a battle of the story scene included (D-1009).</summary>
    public bool Running => this.Scene is not null;

    /// <summary>The shown cast members, in the order of their shows (D-1006). Game draws each one.</summary>
    public IReadOnlyList<ActorValues> Actors
    {
        get
        {
            List<ActorValues> values = [];
            foreach (ShownActor actor in this.actors)
            {
                values.Add(new ActorValues(actor.Character, actor.At, actor.Facing));
            }

            return values;
        }
    }

    /// <summary>Starts the story state of a new run: no flag on, no story scene, and the entry of the first map to read (D-1004).</summary>
    /// <param name="content">The story content of this build.</param>
    /// <returns>The state.</returns>
    /// <exception cref="ArgumentNullException">The content is null (T-2).</exception>
    public static StoryState Start(StoryContent content)
    {
        ArgumentNullException.ThrowIfNull(content);

        return new StoryState(content, FlagSet.Empty(), true);
    }

    /// <summary>Puts the story state back from the values of a snapshot (D-166).</summary>
    /// <param name="content">The story content of this build.</param>
    /// <param name="values">The stored values.</param>
    /// <param name="map">The map of the run, which holds each tile and each patrol.</param>
    /// <param name="source">What the values came from, such as `the save`, for an error (T-2).</param>
    /// <returns>The state.</returns>
    /// <exception cref="ArgumentNullException">An argument is null (T-2).</exception>
    /// <exception cref="ArgumentException">The values describe no story state of this content and this map (T-2).</exception>
    /// <remarks>
    /// The caller checks that a story scene in its battle phase holds a battle, because the
    /// battle lives outside this state (D-999).
    /// </remarks>
    public static StoryState Resume(StoryContent content, StoryValues values, GameMap map, string source) =>
        Resume(content, values, map, source, ResumeDrift.Of(SnapshotOrigin.ThisBuild, 0));

    /// <summary>
    /// Puts the story state back from the values of a snapshot that this build or another build
    /// wrote (D-166, D-1112).
    /// </summary>
    /// <param name="content">The story content of this build.</param>
    /// <param name="values">The stored values.</param>
    /// <param name="map">The map of the run, which holds each tile and each patrol.</param>
    /// <param name="source">What the values came from, such as `the save`, for an error (T-2).</param>
    /// <param name="drift">
    /// The build of the snapshot. A snapshot of another build finds a moved step by its id,
    /// and it drops a win against a patrol that the map no longer places. The drift logs each
    /// change (D-1111, D-1112).
    /// </param>
    /// <returns>The state.</returns>
    /// <exception cref="ArgumentNullException">An argument is null (T-2).</exception>
    /// <exception cref="ArgumentException">The values describe no story state of this content and this map (T-2).</exception>
    public static StoryState Resume(StoryContent content, StoryValues values, GameMap map, string source, ResumeDrift drift)
    {
        ArgumentNullException.ThrowIfNull(content);
        ArgumentNullException.ThrowIfNull(values);
        ArgumentNullException.ThrowIfNull(map);
        ArgumentException.ThrowIfNullOrEmpty(source);
        ArgumentNullException.ThrowIfNull(drift);

        var state = new StoryState(content, FlagSet.Resume(values.Flags, content.Flags, source), values.EntryPending);
        Refuse(values.Paused && values.Scene is null, source, "it is paused with no story scene, and the pause holds a story scene alone (D-1010)");
        Refuse(values.EntryPending && values.Scene is not null, source, "it holds an entry to read and a story scene that runs, and an entry trigger reads the map before any story scene");
        if (values.WonPatrol is ContentId won && !PlacesPatrol(map, won) && drift.Adjusts)
        {
            drift.Note(
                LogSubsystems.Story,
                "the save holds a win against a patrol that the map of this build no longer places, and its battle end triggers do not fire",
                [new LogField("patrol", won.Value), new LogField("map", map.Id.Value)]);
        }
        else if (values.WonPatrol is ContentId kept)
        {
            Refuse(!PlacesPatrol(map, kept), source, $"it holds a win against '{kept.Value}', and the map places no such patrol (D-1011)");
            state.WonPatrol = kept;
        }

        state.Paused = values.Paused;
        if (values.Scene is SceneValues scene)
        {
            state.ResumeScene(scene, map, source, drift);
        }

        return state;
    }

    /// <summary>Gives the stored values of the state (D-166).</summary>
    /// <returns>The values.</returns>
    public StoryValues Values()
    {
        SceneValues? scene = this.Scene is StoryScene running
            ? new SceneValues(running.Id, this.Step, this.StepIdNow(running), this.Phase, this.TicksLeft, this.Actors)
            : null;
        return new StoryValues(this.Flags.Values(FlagList.Path), scene, this.Paused, this.EntryPending, this.WonPatrol);
    }

    /// <summary>Adds every value of the state to the state hash (G-5).</summary>
    /// <param name="hasher">The hasher of the state.</param>
    /// <exception cref="ArgumentNullException">The hasher is null (T-2).</exception>
    public void Hash(StateHasher hasher)
    {
        ArgumentNullException.ThrowIfNull(hasher);

        this.Flags.Hash(hasher);
        hasher.AddBoolean(this.Scene is not null);
        hasher.AddText(this.Scene?.Id.Value ?? string.Empty);
        hasher.AddInt32(this.Step);
        hasher.AddInt32((int)this.Phase);
        hasher.AddInt32(this.TicksLeft);
        hasher.AddInt32(this.actors.Count);
        foreach (ShownActor actor in this.actors)
        {
            hasher.AddText(actor.Character.Value);
            hasher.AddInt32(actor.At.X);
            hasher.AddInt32(actor.At.Y);
            hasher.AddInt32((int)actor.Facing);
        }

        hasher.AddBoolean(this.Paused);
        hasher.AddBoolean(this.EntryPending);
        hasher.AddText(this.WonPatrol?.Value ?? string.Empty);
    }

    /// <summary>Finds a shown cast member (D-1006).</summary>
    /// <param name="character">The cast member.</param>
    /// <param name="actor">The values, when the cast member is on the map.</param>
    /// <returns>True when the cast member is on the map.</returns>
    public bool TryActor(ContentId character, out ActorValues? actor)
    {
        ArgumentNullException.ThrowIfNull(character);

        ShownActor? found = this.Find(character);
        actor = found is null ? null : new ActorValues(found.Character, found.At, found.Facing);
        return found is not null;
    }

    /// <summary>Tells whether a shown cast member stands on one tile (D-1012).</summary>
    /// <param name="at">The tile.</param>
    /// <returns>True when a shown cast member stands there.</returns>
    public bool ActorStandsAt(TilePoint at)
    {
        foreach (ShownActor actor in this.actors)
        {
            if (actor.At == at)
            {
                return true;
            }
        }

        return false;
    }

    /// <summary>Starts a story scene at its first step (D-540).</summary>
    internal void Begin(StoryScene scene)
    {
        this.Scene = scene;
        this.Step = 0;
        this.Phase = ScenePhase.Ready;
        this.TicksLeft = 0;
    }

    /// <summary>Ends the story scene. Each shown cast member leaves the map (D-1006).</summary>
    internal void End()
    {
        this.Scene = null;
        this.Step = 0;
        this.Phase = ScenePhase.Ready;
        this.TicksLeft = 0;
        this.actors.Clear();
    }

    /// <summary>Sets the phase of the step now, with the ticks of a wait step.</summary>
    internal void Hold(ScenePhase phase, int ticksLeft)
    {
        this.Phase = phase;
        this.TicksLeft = ticksLeft;
    }

    /// <summary>Moves to the next step, which runs at the next world step or at once.</summary>
    internal void Next()
    {
        this.Step = checked(this.Step + 1);
        this.Phase = ScenePhase.Ready;
        this.TicksLeft = 0;
    }

    /// <summary>Counts one tick of a wait step.</summary>
    /// <returns>True when the wait ended on this tick.</returns>
    internal bool CountTick()
    {
        this.TicksLeft -= 1;
        return this.TicksLeft == 0;
    }

    /// <summary>Starts or ends the pause of the player (D-1010).</summary>
    internal void SetPaused(bool paused) => this.Paused = paused;

    /// <summary>Takes the entry of the map, which the world step reads once (D-1004).</summary>
    /// <returns>True when the entry waited.</returns>
    internal bool TakeEntry()
    {
        bool pending = this.EntryPending;
        this.EntryPending = false;
        return pending;
    }

    /// <summary>Holds a win against a patrol for the battle end triggers of the next world step (D-1011).</summary>
    internal void NoteWin(ContentId patrol) => this.WonPatrol = patrol;

    /// <summary>Takes the win that waits, which the world step reads once (D-1011).</summary>
    /// <returns>The patrol, or no value.</returns>
    internal ContentId? TakeWin()
    {
        ContentId? won = this.WonPatrol;
        this.WonPatrol = null;
        return won;
    }

    /// <summary>Puts a cast member on the map (D-1006).</summary>
    internal void Show(ContentId character, TilePoint at, StepDirection facing) => this.actors.Add(new ShownActor(character, at, facing));

    /// <summary>Removes a shown cast member from the map (D-1006).</summary>
    internal void Hide(ContentId character) => this.actors.Remove(this.Find(character)!);

    /// <summary>Moves a shown cast member to a tile, with the facing of its last step (D-1012).</summary>
    internal void Place(ContentId character, TilePoint at, StepDirection facing)
    {
        ShownActor actor = this.Find(character)!;
        actor.At = at;
        actor.Facing = facing;
    }

    private static bool PlacesPatrol(GameMap map, ContentId id)
    {
        foreach (Patrol patrol in map.Patrols)
        {
            if (string.CompareOrdinal(patrol.Id.Value, id.Value) == 0)
            {
                return true;
            }
        }

        return false;
    }

    private static void Refuse(bool broken, string source, string reason)
    {
        if (broken)
        {
            throw new ArgumentException($"The story state of {source} is not a state of a run: {reason}.");
        }
    }

    /// <summary>Gives the id of the step now, or the end id past the last step (D-1112).</summary>
    private ContentId StepIdNow(StoryScene running) =>
        this.Step < running.Steps.Count ? running.StepIds[this.Step] : StoryScene.EndStep;

    /// <summary>
    /// Finds the index of the stored step in the story scene of this build (D-1112). A snapshot
    /// before save format 14 holds the index alone. A snapshot of another build finds a moved
    /// step by its id, and a step id that the story scene no longer holds refuses the save, so
    /// no resume continues at another step in silence (T-2).
    /// </summary>
    private static int StepOf(SceneValues values, StoryScene scene, string source, ResumeDrift drift)
    {
        int count = scene.Steps.Count;
        string name = scene.Id.Value;
        if (values.StepId is not ContentId stepId)
        {
            Refuse(values.Step < 0 || values.Step > count, source, $"the step is {values.Step}, and '{name}' holds {count} steps");
            return values.Step;
        }

        Refuse(values.Step < 0, source, $"the step is {values.Step}, and a step index is zero or more");
        int found;
        if (string.CompareOrdinal(stepId.Value, StoryScene.EndStepId) == 0)
        {
            found = count;
        }
        else
        {
            Refuse(
                !scene.TryIndexOfStep(stepId, out found),
                source,
                $"it runs the step '{stepId.Value}' of '{name}', and no step of that story scene in this build takes that id (D-1112)");
        }

        if (found == values.Step)
        {
            return found;
        }

        Refuse(
            !drift.Adjusts,
            source,
            $"it runs the step '{stepId.Value}' at index {values.Step}, and '{name}' holds that step at index {found}");
        drift.Note(
            LogSubsystems.Story,
            "the story scene of the save moved its step in this build, and the resume found the step by its id",
            [new LogField("scene", name), new LogField("step", stepId.Value), LogField.OfNumber("stored_index", values.Step), LogField.OfNumber("index", found)]);
        return found;
    }

    private void ResumeScene(SceneValues values, GameMap map, string source, ResumeDrift drift)
    {
        ArgumentNullException.ThrowIfNull(values.Scene);
        ArgumentNullException.ThrowIfNull(values.Actors);

        if (!this.Content.TryScene(values.Scene, out StoryScene? found))
        {
            throw new ArgumentException($"The story state of {source} is not a state of a run: it runs '{values.Scene.Value}', which no story scene of this build holds (D-166).");
        }

        StoryScene scene = found!;

        int step = StepOf(values, scene, source, drift);
        this.Begin(scene);
        this.Step = step;
        this.Phase = values.Phase;
        this.TicksLeft = this.CheckPhase(values, step, scene, source, drift);
        foreach (ActorValues actor in values.Actors)
        {
            ArgumentNullException.ThrowIfNull(actor);
            Refuse(!this.Content.HoldsCast(actor.Character), source, $"it shows '{actor.Character.Value}', which no character of this build holds (D-1006)");
            Refuse(this.Find(actor.Character) is not null, source, $"it shows '{actor.Character.Value}' two times");
            Refuse(!MapRules.CanEnter(map, actor.At), source, $"it shows '{actor.Character.Value}' at {actor.At}, which no actor can stand on");
            Refuse(this.ActorStandsAt(actor.At), source, $"two actors stand at {actor.At}");
            this.Show(actor.Character, actor.At, actor.Facing);
        }
    }

    /// <summary>
    /// Refuses a phase that the kind of the step never takes (T-2), and gives the ticks left of
    /// the step. A wait of another build that got shorter ends at its new length (D-1112).
    /// </summary>
    private int CheckPhase(SceneValues values, int stepIndex, StoryScene scene, string source, ResumeDrift drift)
    {
        bool pastEnd = stepIndex == scene.Steps.Count;
        Refuse(pastEnd && values.Phase != ScenePhase.Ready, source, $"the step is past the last step, and its phase is '{ScenePhases.NameOf(values.Phase)}'");
        Refuse(values.Phase != ScenePhase.Ticks && values.TicksLeft != 0, source, $"the phase '{ScenePhases.NameOf(values.Phase)}' holds {values.TicksLeft} ticks left, and a wait step alone counts ticks");
        if (pastEnd || values.Phase == ScenePhase.Ready)
        {
            return values.TicksLeft;
        }

        SceneStep step = scene.Steps[stepIndex];
        ScenePhase wanted = SceneStepKinds.EndOf(step.Kind) switch
        {
            SceneStepEnd.WaitIntent => ScenePhase.WaitIntent,
            SceneStepEnd.Ticks => ScenePhase.Ticks,
            SceneStepEnd.Pick => ScenePhase.Pick,
            SceneStepEnd.Battle => ScenePhase.Battle,
            _ => ScenePhase.Ready,
        };
        Refuse(
            values.Phase != wanted,
            source,
            $"step {stepIndex} of '{scene.Id.Value}' is a '{SceneStepKinds.NameOf(step.Kind)}' step, and its phase is '{ScenePhases.NameOf(values.Phase)}'");
        if (step is not WaitStep wait)
        {
            return values.TicksLeft;
        }

        if (values.TicksLeft > wait.Ticks && drift.Adjusts)
        {
            drift.Note(
                LogSubsystems.Story,
                "the wait of the save is longer than the wait of this build, and it ends at the new length",
                [new LogField("scene", scene.Id.Value), new LogField("step", scene.StepIds[stepIndex].Value), LogField.OfNumber("stored_ticks", values.TicksLeft), LogField.OfNumber("ticks", wait.Ticks)]);
            return wait.Ticks;
        }

        Refuse(values.TicksLeft < 1 || values.TicksLeft > wait.Ticks, source, $"the wait holds {values.TicksLeft} ticks left, and the range is 1 to {wait.Ticks}");
        return values.TicksLeft;
    }

    private ShownActor? Find(ContentId character)
    {
        foreach (ShownActor actor in this.actors)
        {
            if (string.CompareOrdinal(actor.Character.Value, character.Value) == 0)
            {
                return actor;
            }
        }

        return null;
    }

    private sealed class ShownActor(ContentId character, TilePoint at, StepDirection facing)
    {
        public ContentId Character { get; } = character;

        public TilePoint At { get; set; } = at;

        public StepDirection Facing { get; set; } = facing;
    }
}
