using System;
using System.Collections.Generic;
using TheThingBelow.Core.Content;
using TheThingBelow.Core.Maps;
using TheThingBelow.Core.Runs;
using TheThingBelow.Core.Story;

namespace TheThingBelow.Game.Ui;

/// <summary>
/// Follows the story scene that Core runs, and gives what the screen draws: the slide of each
/// move, the view, the line of the dialogue box, and the choices (D-540, D-1000, D-1013).
/// </summary>
/// <remarks>
/// <para>
/// Core runs each step at once and waits for the wait intent of Game (D-1000). This class counts
/// the ticks of the run, never a clock of the engine, and sends the wait intent when the step
/// that Game animates ends (G-23, F-52). A move ends after the ticks of its path, a face after
/// <see cref="FaceTicks"/>, and a camera step when the view reaches its marker. A line ends on a
/// press of confirm, and a choice on a pick (D-864, D-1007).
/// </para>
/// <para>
/// Core refuses an intent that no step waits for (T-2). Thus each intent goes to the queue once,
/// and never while the queue holds the pause of the player.
/// </para>
/// <para>
/// The class holds no engine type, so a test drives it with the run alone (D-614).
/// </para>
/// </remarks>
public sealed class ScenePlay
{
    /// <summary>The ticks that a face step holds before its end, so the turn shows (D-1000).</summary>
    public const int FaceTicks = 8;

    /// <summary>The art pixels that the view moves on each tick of a camera step, and on the way back to the lead (D-1013).</summary>
    public const int PanPixelsPerTick = 4;

    /// <summary>The character of the lead, who speaks the choices of the player (D-267, D-306).</summary>
    public const string LeadCharacter = "character.marrek";

    /// <summary>The kind of the art id of a cast member, such as `cast.marrek` (D-519).</summary>
    public const string CastKind = "cast";

    /// <summary>The use of the drawing of a portrait (D-234).</summary>
    public const string PortraitUse = "portrait";

    private readonly StringTable strings;
    private readonly int viewWidth;
    private readonly int viewHeight;
    private ContentId? scene;
    private int step = -1;
    private ScenePhase phase;
    private long lastTick = -1;
    private int elapsed;
    private bool filled;
    private bool cameraHeld;
    private CameraPlace? view;
    private SceneStep? stepOnScreen;
    private ContentId? lastLine;
    private SceneActor? lastSpeaker;

    /// <summary>Builds the follower of story scenes, with no story scene on screen.</summary>
    /// <param name="strings">The string table, which gives the length of each line for its type-out (D-709).</param>
    /// <param name="viewWidth">The width of the world viewport, in art pixels (D-634).</param>
    /// <param name="viewHeight">The height of the world viewport, in art pixels.</param>
    /// <exception cref="ArgumentNullException">The table is null (T-2).</exception>
    /// <exception cref="ArgumentOutOfRangeException">A side of the view is below one (T-2).</exception>
    public ScenePlay(StringTable strings, int viewWidth, int viewHeight)
    {
        ArgumentNullException.ThrowIfNull(strings);
        ArgumentOutOfRangeException.ThrowIfLessThan(viewWidth, 1);
        ArgumentOutOfRangeException.ThrowIfLessThan(viewHeight, 1);

        this.strings = strings;
        this.viewWidth = viewWidth;
        this.viewHeight = viewHeight;
    }

    /// <summary>The count of characters that the line types in one second, from the text speed of the settings (D-864).</summary>
    public int CharactersPerSecond { get; set; } = 60;

    /// <summary>The line of the dialogue box, or no value when the box hides (D-223, D-1175).</summary>
    /// <remarks>A choice keeps the last line of the story scene in the box (D-1175).</remarks>
    public ContentId? Line { get; private set; }

    /// <summary>The speaker of <see cref="Line"/>, or no value for a line with no speaker (D-997).</summary>
    public SceneActor? Speaker { get; private set; }

    /// <summary>The options of the choice that waits for a pick, or no value (D-1007, D-1175).</summary>
    public IReadOnlyList<ChooseOption>? Options { get; private set; }

    /// <summary>The option under the cursor of the choice, from zero.</summary>
    public int Cursor { get; private set; }

    /// <summary>True while the pause of the player holds the story scene (D-1009, D-1010).</summary>
    public bool Paused { get; private set; }

    /// <summary>The place of the view while a story scene holds it, or no value when the view follows the lead as on the walk (D-1013).</summary>
    public CameraPlace? View => this.view;

    /// <summary>
    /// The count of characters of <see cref="Line"/> on screen, or no value when the whole line
    /// shows: after a press of confirm, at the end of the type-out, and under a choice (D-709, D-864).
    /// </summary>
    public int? Characters
    {
        get
        {
            if (this.Line is not ContentId line || this.filled || this.Options is not null)
            {
                return null;
            }

            long typed = (long)this.elapsed * this.CharactersPerSecond / FixedStepLoop.TicksPerSecond;
            return typed >= this.strings.Text(line).Length ? null : (int)typed;
        }
    }

    /// <summary>Gives the art id of one actor or speaker: `cast.` and the name for a character, and the id itself for an NPC (D-519, D-1006).</summary>
    /// <param name="actor">The actor, or the lead.</param>
    /// <returns>The art id, which a drawing names in its draws.</returns>
    /// <exception cref="ArgumentNullException">The actor is null (T-2).</exception>
    public static ContentId ArtIdOf(SceneActor actor)
    {
        ArgumentNullException.ThrowIfNull(actor);

        return ArtIdOfActor(actor.Id ?? ContentId.Parse(LeadCharacter, nameof(ScenePlay), nameof(LeadCharacter)));
    }

    /// <summary>Gives the art id of one actor id: `cast.` and the name for a character, and the id itself for an NPC (D-519, D-1006).</summary>
    /// <param name="actor">The id of a character or of an NPC.</param>
    /// <returns>The art id.</returns>
    /// <exception cref="ArgumentNullException">The id is null (T-2).</exception>
    public static ContentId ArtIdOfActor(ContentId actor)
    {
        ArgumentNullException.ThrowIfNull(actor);

        return SceneActor.IsNpcId(actor) ? actor : ContentId.Parse($"{CastKind}.{actor.Name}", nameof(ScenePlay), actor.Value);
    }

    /// <summary>Gives the string id of the name plate of one speaker, such as `name.marrek` (D-223, G-7).</summary>
    /// <param name="speaker">The speaker, or the lead.</param>
    /// <returns>The string id.</returns>
    /// <exception cref="ArgumentNullException">The speaker is null (T-2).</exception>
    public static ContentId NameIdOf(SceneActor speaker)
    {
        ArgumentNullException.ThrowIfNull(speaker);

        return BattleMessages.NameIdOf(speaker.Id ?? ContentId.Parse(LeadCharacter, nameof(ScenePlay), nameof(LeadCharacter)));
    }

    /// <summary>Follows the story scene after the ticks of one frame, and queues the wait intent of each step that Game animated to its end (D-1000).</summary>
    /// <param name="run">The run, after the ticks of this frame.</param>
    /// <exception cref="ArgumentNullException">The run is null (T-2).</exception>
    /// <exception cref="InvalidOperationException">A camera step names a marker that the map lacks (T-2).</exception>
    public void Follow(GameRun run)
    {
        ArgumentNullException.ThrowIfNull(run);

        StoryState story = run.State.Story;
        int ticks = this.lastTick < 0 ? 0 : (int)Math.Min(run.Tick - this.lastTick, int.MaxValue);
        this.lastTick = run.Tick;
        this.Paused = story.Paused;
        if (story.Scene is not StoryScene running)
        {
            this.EndScene(run.Party, ticks);
            return;
        }

        if (!this.Shows(running, story))
        {
            this.Begin(running, story, run.Party);
        }
        else if (!story.Paused)
        {
            this.elapsed = (int)Math.Min((long)this.elapsed + ticks, int.MaxValue);
        }

        if (story.Paused)
        {
            return;
        }

        this.MoveView(run.Party, ticks);
        if (this.phase == ScenePhase.WaitIntent
            && this.StepEnds(run.Party)
            && !run.Queued(IntentIds.StoryStepEnd)
            && !run.Queued(IntentIds.StoryPause))
        {
            run.Queue(Intent.OfPlayer(IntentIds.StoryStepEnd));
        }
    }

    /// <summary>Takes a press of confirm: it shows the whole line, then ends the line, and it picks the option under the cursor of a choice (D-864, D-1007, D-1174).</summary>
    /// <param name="run">The run.</param>
    /// <returns>The intent to queue, or no value when the press made none.</returns>
    /// <exception cref="ArgumentNullException">The run is null (T-2).</exception>
    public Intent? Confirm(GameRun run)
    {
        ArgumentNullException.ThrowIfNull(run);

        if (this.scene is null || this.Paused || run.Queued(IntentIds.StoryPause))
        {
            return null;
        }

        if (this.phase == ScenePhase.Pick)
        {
            return run.Queued(IntentIds.StoryPick) ? null : Intent.OfPick(this.Cursor);
        }

        if (this.phase != ScenePhase.WaitIntent || this.Line is null || this.Options is not null)
        {
            return null;
        }

        if (this.Characters is not null)
        {
            // The first press shows the whole line at once, and the next press ends it (D-864).
            this.filled = true;
            return null;
        }

        return run.Queued(IntentIds.StoryStepEnd) ? null : Intent.OfPlayer(IntentIds.StoryStepEnd);
    }

    /// <summary>Moves the cursor of the choice, and wraps at each end (D-1175).</summary>
    /// <param name="by">-1 for up, 1 for down.</param>
    public void MoveCursor(int by)
    {
        if (this.Options is not IReadOnlyList<ChooseOption> options || this.Paused)
        {
            return;
        }

        this.Cursor = ((this.Cursor + by) % options.Count + options.Count) % options.Count;
    }

    /// <summary>Puts the cursor of the choice on one option, as the mouse does (D-219).</summary>
    /// <param name="option">The option, from zero.</param>
    public void PointAt(int option)
    {
        if (this.Options is IReadOnlyList<ChooseOption> options && !this.Paused && option >= 0 && option < options.Count)
        {
            this.Cursor = option;
        }
    }

    /// <summary>Gives the intent of the pause of a story scene: the menu action pauses, and the menu action or the back action ends the pause (D-1009, D-1010).</summary>
    /// <param name="run">The run.</param>
    /// <param name="menu">True for the menu action, false for the back action.</param>
    /// <returns>The intent to queue, or no value when the action makes none now.</returns>
    /// <exception cref="ArgumentNullException">The run is null (T-2).</exception>
    /// <remarks>A battle of a story scene is a battle, and the pause holds the story scene alone (D-1010).</remarks>
    public Intent? PauseOf(GameRun run, bool menu)
    {
        ArgumentNullException.ThrowIfNull(run);

        if (this.scene is null || run.Queued(IntentIds.StoryPause) || run.Queued(IntentIds.StoryResume))
        {
            return null;
        }

        if (this.Paused)
        {
            return Intent.OfPlayer(IntentIds.StoryResume);
        }

        return menu && this.phase != ScenePhase.Battle ? Intent.OfPlayer(IntentIds.StoryPause) : null;
    }

    /// <summary>Gives the pixel of the north-west corner of one actor while a move step walks it, with the slide of each tile (D-203, D-1012).</summary>
    /// <param name="actor">The id of the actor, or no value for the lead.</param>
    /// <param name="end">The tile of the actor in Core, at the end of the path.</param>
    /// <param name="x">The pixel on the west to east axis.</param>
    /// <param name="y">The pixel on the north to south axis.</param>
    /// <returns>True when a move step walks this actor now, and false when the actor stands on its tile.</returns>
    public bool TryWalk(ContentId? actor, TilePoint end, out int x, out int y)
    {
        x = 0;
        y = 0;
        if (this.scene is null || this.phase != ScenePhase.WaitIntent || this.stepOnScreen is not MoveStep move || !Same(move.Actor.Id, actor))
        {
            return false;
        }

        // Core moved the actor along the whole path at once, so the walk starts at the end and
        // steps back along the path (D-1012).
        TilePoint at = end;
        for (int index = move.Path.Count - 1; index >= 0; index -= 1)
        {
            at = at.Step(StepDirections.Opposite(move.Path[index]));
        }

        int tiles = this.elapsed / MapRules.TicksPerStep;
        if (tiles >= move.Path.Count)
        {
            x = end.X * MapCamera.TilePixels;
            y = end.Y * MapCamera.TilePixels;
            return true;
        }

        for (int index = 0; index < tiles; index += 1)
        {
            at = at.Step(move.Path[index]);
        }

        StepDirection next = move.Path[tiles];
        int part = this.elapsed % MapRules.TicksPerStep;
        x = MapCamera.SlideOf(at.X, MapCamera.AcrossOf(next), part, MapRules.TicksPerStep, 0);
        y = MapCamera.SlideOf(at.Y, MapCamera.DownOf(next), part, MapRules.TicksPerStep, 0);
        return true;
    }

    private static bool Same(ContentId? left, ContentId? right) =>
        (left is null && right is null) || (left is not null && right is not null && string.CompareOrdinal(left.Value, right.Value) == 0);

    /// <summary>Tells whether the step on screen is the step of Core, in the same phase.</summary>
    private bool Shows(StoryScene running, StoryState story) =>
        this.scene is not null
        && string.CompareOrdinal(this.scene.Value, running.Id.Value) == 0
        && this.step == story.Step
        && this.phase == story.Phase;

    /// <summary>Starts to draw the step of Core, or the next phase of the same step.</summary>
    private void Begin(StoryScene running, StoryState story, MapState party)
    {
        bool newScene = this.scene is null || string.CompareOrdinal(this.scene.Value, running.Id.Value) != 0;
        bool newStep = newScene || this.step != story.Step;
        if (newScene)
        {
            this.lastLine = null;
            this.lastSpeaker = null;
            this.cameraHeld = false;
            this.view ??= this.LeadView(party);
        }

        this.scene = running.Id;
        this.step = story.Step;
        this.phase = story.Phase;
        this.stepOnScreen = story.Step < running.Steps.Count ? running.Steps[story.Step] : null;
        this.elapsed = 0;
        if (newStep)
        {
            this.filled = false;
            this.Cursor = 0;
        }

        this.Options = null;
        switch (this.stepOnScreen)
        {
            case SayStep say when this.phase == ScenePhase.WaitIntent:
                this.Line = say.Line;
                this.Speaker = say.Speaker;
                this.lastLine = say.Line;
                this.lastSpeaker = say.Speaker;
                break;
            case ChooseStep choose when this.phase == ScenePhase.Pick:
                // The last line of the story scene stays in the box over the choice (D-1175).
                this.Line = this.lastLine;
                this.Speaker = this.lastSpeaker;
                this.Options = choose.Options;
                break;
            default:
                this.Line = null;
                this.Speaker = null;
                break;
        }
    }

    /// <summary>Tells whether the step that Game animates reached its end.</summary>
    private bool StepEnds(MapState party) => this.stepOnScreen switch
    {
        MoveStep move => this.elapsed >= move.Path.Count * MapRules.TicksPerStep,
        FaceStep => this.elapsed >= FaceTicks,
        CameraStep camera => this.view == this.MarkerView(party, camera),
        _ => false,
    };

    /// <summary>Moves the view: to the marker of a camera step, then back to the lead at the end of the story scene (D-1013).</summary>
    private void MoveView(MapState party, int ticks)
    {
        if (this.phase == ScenePhase.WaitIntent && this.stepOnScreen is CameraStep camera)
        {
            this.cameraHeld = true;
            this.view = Toward(this.view ?? this.LeadView(party), this.MarkerView(party, camera), ticks);
            return;
        }

        if (!this.cameraHeld)
        {
            // No camera step moved the view yet, so it follows the lead, and the walk of a move
            // step too (D-292).
            this.view = this.LeadView(party);
        }
    }

    /// <summary>Ends the story scene on screen, and moves the view back to the lead (D-1013).</summary>
    private void EndScene(MapState party, int ticks)
    {
        this.scene = null;
        this.step = -1;
        this.lastLine = null;
        this.lastSpeaker = null;
        this.stepOnScreen = null;
        this.Line = null;
        this.Speaker = null;
        this.Options = null;
        this.Paused = false;
        if (this.view is not CameraPlace shown)
        {
            return;
        }

        CameraPlace lead = this.LeadView(party);
        CameraPlace moved = this.cameraHeld ? Toward(shown, lead, ticks) : lead;
        this.view = moved == lead ? null : moved;
        this.cameraHeld = this.view is not null;
    }

    private CameraPlace LeadView(MapState party)
    {
        int x = party.LeadAt.X * MapCamera.TilePixels;
        int y = party.LeadAt.Y * MapCamera.TilePixels;
        if (this.TryWalk(null, party.LeadAt, out int walkX, out int walkY))
        {
            x = walkX;
            y = walkY;
        }

        return MapCamera.OfPixels(party.Map, this.viewWidth, this.viewHeight, x, y);
    }

    private CameraPlace MarkerView(MapState party, CameraStep camera)
    {
        if (!party.Map.TryMarker(camera.Marker, out TilePoint at))
        {
            throw new InvalidOperationException($"The camera step of the story scene '{this.scene?.Value}' names the marker '{camera.Marker.Value}', and the map '{party.Map.Id.Value}' lacks it (D-1013, T-2).");
        }

        return MapCamera.OfPixels(party.Map, this.viewWidth, this.viewHeight, at.X * MapCamera.TilePixels, at.Y * MapCamera.TilePixels);
    }

    /// <summary>Moves one place toward another by the pixels of the ticks on each axis, and never past it.</summary>
    private static CameraPlace Toward(CameraPlace from, CameraPlace to, int ticks)
    {
        int reach = (int)Math.Min((long)ticks * PanPixelsPerTick, int.MaxValue);
        return new CameraPlace(from.X + Math.Clamp(to.X - from.X, -reach, reach), from.Y + Math.Clamp(to.Y - from.Y, -reach, reach));
    }
}
