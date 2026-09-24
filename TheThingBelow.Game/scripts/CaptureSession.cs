using System;
using System.Collections.Generic;
using System.IO;
using Godot;
using TheThingBelow.Core.Battles;
using TheThingBelow.Core.Content;
using TheThingBelow.Core.Effects;
using TheThingBelow.Core.Light;
using TheThingBelow.Core.Logging;
using TheThingBelow.Game.Ui;
using TheThingBelow.Storage;

namespace TheThingBelow.Game;

/// <summary>
/// The capture session of the screen-test job (D-172, D-732). It draws each fixture of
/// <see cref="ScreenCaptures"/>, sets the window size and the fit of each capture, and
/// writes one PNG for each one. Then it quits with the success code.
/// </summary>
/// <remarks>
/// The map fixture and the ui fixture run no tick. The frame time of the engine is a float
/// clock, and a tick from it would put the party in another place on each run (T-7, G-3).
/// Thus those captures show the run of <see cref="Boot.FixtureSeed"/> at tick 0. The walk
/// fixture gives the run the time of exactly one tick for each frame, and never the frame
/// time of the engine, so two runs give the same frames too (D-782). The battle fixture walks
/// into the fixture fight the same way, one tick for each call of the run (D-827).
/// <para>
/// A capture needs a drawn frame, so the session waits <see cref="FramesBeforeCapture"/>
/// frames after each change of the window size. The world draws into a viewport, the frame
/// draws into a second one, and the fit of D-573 can add a third, so one change takes more
/// than one frame to reach the screen. The session then reads the size of the image, and it
/// fails when the window did not reach the size of the capture (T-2).
/// </para>
/// </remarks>
public sealed partial class CaptureSession : Node
{
    /// <summary>The settings of every capture: the defaults, and never the file of the person (D-860).</summary>
    private static readonly GameSettings FixtureSettings = GameSettings.Defaults(GameInputMap.DefaultBindings());

    /// <summary>The line that a session writes when it wrote every capture (T-2).</summary>
    public const string SuccessLine = "capture: the session wrote every frame.";

    /// <summary>The count of frames that the session waits after each change of the window.</summary>
    public const int FramesBeforeCapture = 8;

    /// <summary>
    /// The time of one tick, which the walk fixture gives to the run for each frame that it
    /// writes. The fixed-step loop then runs exactly one tick (D-164, D-782).
    /// </summary>
    private const double OneTickSeconds = 1.0 / FixedStepLoop.TicksPerSecond;

    private ContentSet content = null!;
    private Action<Exception> reportFault = null!;
    private string folder = string.Empty;
    private IReadOnlyList<ScreenCapture> captures = [];
    private FrameRoot? frame;
    private GameRun? walkRun;
    private MapScreen? walkMap;
    private int stepTicks;
    private int next;
    private int waited;
    private bool stopped;

    /// <summary>Starts the capture session under one host node.</summary>
    /// <param name="host">The node that holds the session, which is the boot node.</param>
    /// <param name="content">The content set of this build.</param>
    /// <param name="folder">The folder that takes one PNG for each capture.</param>
    /// <param name="captures">The captures of this session, in order: every capture, or the captures of one fixture (D-782).</param>
    /// <param name="reportFault">The reporter of a fault, which writes the crash file (D-170).</param>
    /// <returns>The session, which draws from the next frame onward.</returns>
    /// <exception cref="ArgumentNullException">An argument is null (T-2).</exception>
    /// <exception cref="ArgumentException">The folder is empty, or the list holds no capture (T-2).</exception>
    public static CaptureSession Start(
        Node host,
        ContentSet content,
        string folder,
        IReadOnlyList<ScreenCapture> captures,
        Action<Exception> reportFault)
    {
        ArgumentNullException.ThrowIfNull(host);
        ArgumentNullException.ThrowIfNull(content);
        ArgumentException.ThrowIfNullOrEmpty(folder);
        ArgumentNullException.ThrowIfNull(captures);
        if (captures.Count == 0)
        {
            throw new ArgumentException("The capture session takes one capture or more (T-2).", nameof(captures));
        }

        ArgumentNullException.ThrowIfNull(reportFault);

        var session = new CaptureSession
        {
            content = content,
            folder = folder,
            captures = captures,
            reportFault = reportFault,
        };

        host.AddChild(session);
        return session;
    }

    /// <summary>Makes the folder of the captures and draws the first one.</summary>
    public override void _Ready()
    {
        try
        {
            Directory.CreateDirectory(this.folder);
            // The screen-test job reads these three lines. Godot falls back to another driver
            // when it cannot start the one of the project, and the job fails on a fallback,
            // because another renderer draws another picture (D-616, D-731, T-2).
            GD.Print($"capture: the rendering method is {RenderingServer.GetCurrentRenderingMethod()}.");
            GD.Print($"capture: the rendering driver is {RenderingServer.GetCurrentRenderingDriverName()}.");
            GD.Print($"capture: the video adapter is {RenderingServer.GetVideoAdapterName()}.");
            GD.Print($"capture: the folder is {this.folder}.");
            this.PushStrayKey();
            this.Begin(0);
        }
        catch (Exception fault)
        {
            this.stopped = true;
            this.reportFault(fault);
        }
    }

    /// <summary>
    /// Sends one press and one release of a step key through the root viewport, as a key of
    /// the person reaches the capture window.
    /// </summary>
    /// <remarks>
    /// The regression test of the fault that Session 182 found: a key in the capture window
    /// wrote "The InputMap action ... doesn't exist", because this session builds no input
    /// map and the boot node read the held steps before it checked the run. The screen-test
    /// job and `make sheet` fail on that error line, so each capture now proves the fix (T-3).
    /// </remarks>
    private void PushStrayKey()
    {
        GetViewport().PushInput(new InputEventKey { Keycode = Key.Up, PhysicalKeycode = Key.Up, Pressed = true });
        GetViewport().PushInput(new InputEventKey { Keycode = Key.Up, PhysicalKeycode = Key.Up, Pressed = false });
    }

    /// <summary>Waits for the draw of the current capture, writes it, and starts the next one.</summary>
    /// <param name="delta">The time of the frame, which this session never reads (T-7).</param>
    public override void _Process(double delta)
    {
        if (this.stopped)
        {
            return;
        }

        try
        {
            this.Step();
        }
        catch (Exception fault)
        {
            this.stopped = true;
            this.reportFault(fault);
        }
    }

    /// <summary>Writes the capture that the window now shows, and moves to the next one.</summary>
    /// <exception cref="InvalidOperationException">The window holds another size (T-2).</exception>
    /// <exception cref="IOException">The write of the file failed (T-2).</exception>
    private void Step()
    {
        if (this.waited < FramesBeforeCapture)
        {
            this.waited++;
            return;
        }

        ScreenCapture capture = this.captures[this.next];
        this.Write(capture);
        GD.Print($"capture: wrote {capture.FileName}.");

        this.next++;
        if (this.next >= this.captures.Count)
        {
            this.stopped = true;
            GD.Print(SuccessLine);
            this.GetTree().Quit(Boot.SuccessExitCode);
            return;
        }

        this.Begin(this.next);
    }

    /// <summary>Sets the window size of one capture, and builds its fixture.</summary>
    /// <param name="index">The place of the capture in the captures of this session.</param>
    /// <remarks>
    /// The window takes its new size first, so the frame reads that size when it builds its
    /// fit. Each capture builds its fixture again, because the default body size follows the
    /// fit of the screen, and the five captures of one fixture hold two body sizes (D-707).
    /// </remarks>
    private void Begin(int index)
    {
        ScreenCapture capture = this.captures[index];
        this.GetWindow().Size = new Vector2I(capture.Width, capture.Height);
        if (capture.Walk is null)
        {
            this.BuildFixture(capture);
        }
        else
        {
            this.WalkOneTick(capture, capture.Walk);
        }

        this.waited = 0;
    }

    /// <summary>
    /// Runs one tick of the walk, and shows the party where that tick put it (D-782). The
    /// first frame of the walk builds the fixture, and every later frame keeps its run.
    /// </summary>
    /// <param name="capture">The capture of this tick.</param>
    /// <param name="walk">The step and the tick of the step.</param>
    /// <exception cref="InvalidOperationException">
    /// The loop ran another count of ticks, a tick wrote an error, or the step did not start (T-2).
    /// </exception>
    /// <remarks>
    /// The time of each frame is the time of one tick, and never the frame time of the
    /// engine. Thus every session walks the same ticks, and two sessions give the same
    /// frames (T-7, G-3).
    /// </remarks>
    private void WalkOneTick(ScreenCapture capture, WalkTick walk)
    {
        if (this.walkRun is null || this.walkMap is null)
        {
            this.BuildFixture(capture);
        }

        GameRun run = this.walkRun!;
        if (walk.Tick == 1 && string.CompareOrdinal(walk.Action, ScreenCaptures.StillAction) != 0)
        {
            run.Queue(run.IntentOf(walk.Action));
            this.stepTicks = 0;
        }

        // The walk fixture names every tick of its step, and the scroll fixture names three of
        // them (D-782, F-97). Thus this capture runs the ticks from the last frame to its own.
        long before = run.Tick;
        long asked = walk.Tick - this.stepTicks;
        while (this.stepTicks < walk.Tick)
        {
            foreach (LogEntry entry in run.Advance(OneTickSeconds))
            {
                if (entry.Level == LogLevel.Error)
                {
                    throw new InvalidOperationException(
                        $"The tick {run.Tick} of the capture '{capture.FileName}' wrote an error: {entry.Message} (T-2).");
                }
            }

            this.stepTicks += 1;
        }

        if (run.Tick != before + asked)
        {
            throw new InvalidOperationException(
                $"The capture '{capture.FileName}' gave the run the time of {asked} ticks, and the loop ran "
                + $"{run.Tick - before} ticks (T-2, D-782).");
        }

        if (walk.Tick == 1 && string.CompareOrdinal(walk.Action, ScreenCaptures.StillAction) != 0 && run.Party.Stepping is null)
        {
            throw new InvalidOperationException(
                $"The intent '{walk.Action}' of the capture '{capture.FileName}' started no step from "
                + $"{run.Party.LeadAt}. The walk fixture needs an open tile on each side of the start (T-2, D-782).");
        }

        // A capture runs whole ticks, so it reads no part of a tick (D-782, D-820).
        this.walkMap!.ShowParty(run.Party, 0);
        this.walkMap.ShowWeather(run.Tick, seek: true);
    }

    /// <summary>Gives the ambient file that one capture loads, or no value for the weather of the map (D-889).</summary>
    /// <exception cref="ContentException">No ambient file holds the id of the capture (T-2).</exception>
    private AmbientEffect? AmbientOf(ScreenCapture capture)
    {
        if (capture.Ambient is null)
        {
            return null;
        }

        return this.content.Effects.Ambient.Effect(
            ContentId.Parse(capture.Ambient, AmbientEffect.CaptureFolder, capture.FileName));
    }

    /// <summary>
    /// Builds the running screen with the party in the pit room: the same map fixture, walked
    /// from the spawn point down the corridor of column 6 (D-852).
    /// </summary>
    /// <exception cref="InvalidOperationException">A step of the route never ended, or a tick wrote an error (T-2).</exception>
    private void BuildPitRoom(FrameRoot built, UiBase @base)
    {
        GameRun open = GameRun.Start(this.content, Boot.FixtureSeed, DebugSeam.Handlers(), FixtureSettings.Battle.Messages);
        MapScreen drawn = MapFixture.Build(built, @base, open.Party, this.content, seekParticles: true);
        foreach (string action in ScreenCaptures.PitRoute)
        {
            this.StepOnce(open, action);
        }

        drawn.ShowParty(open.Party, 0);
        drawn.ShowWeather(open.Tick, seek: true);
    }

    /// <summary>Runs one whole step of the party, from its intent to the arrival of the lead (D-203).</summary>
    /// <exception cref="InvalidOperationException">The step never ended, or a tick wrote an error (T-2).</exception>
    private void StepOnce(GameRun run, string action)
    {
        run.Queue(run.IntentOf(action));
        for (int tick = 0; tick < ScreenCaptures.TicksOfOneStep; tick += 1)
        {
            foreach (LogEntry entry in run.Advance(OneTickSeconds))
            {
                if (entry.Level == LogLevel.Error)
                {
                    throw new InvalidOperationException(
                        $"The tick {run.Tick} of the route of a capture wrote an error: {entry.Message} (T-2).");
                }
            }

            if (run.Party.Stepping is null && tick > 0)
            {
                return;
            }
        }

        throw new InvalidOperationException(
            $"The step '{action}' of the route of a capture never ended, and the lead stands at {run.Party.LeadAt} (T-2, D-852).");
    }

    /// <summary>Gives the transition of one look from the content of the session (D-195).</summary>
    /// <param name="look">The look.</param>
    /// <returns>The transition.</returns>
    /// <exception cref="InvalidOperationException">No transition has the look (T-2).</exception>
    private Transition TransitionOf(TransitionLook look)
    {
        foreach (Transition transition in this.content.Effects.Transitions.Transitions)
        {
            if (transition.Look == look)
            {
                return transition;
            }
        }

        throw new InvalidOperationException($"The content holds no transition of the look '{Transition.NameOf(look)}' (D-195, T-2).");
    }

    /// <summary>
    /// Removes the frame that drew before, and builds the frame and the nodes of one fixture
    /// (D-734).
    /// </summary>
    /// <param name="capture">The capture that this fixture draws.</param>
    /// <exception cref="ArgumentOutOfRangeException">The capture names no fixture of this session (T-2).</exception>
    private void BuildFixture(ScreenCapture capture)
    {
        if (this.frame is not null)
        {
            // The node leaves the tree at once, so it draws no frame of the next capture.
            // `QueueFree` alone would keep it on screen until the end of this frame (T-2).
            this.RemoveChild(this.frame);
            this.frame.QueueFree();
            this.frame = null;
            this.walkRun = null;
            this.walkMap = null;
        }

        var built = new FrameRoot();
        this.AddChild(built);
        this.frame = built;
        built.SetMode(capture.Fit);

        // The body size comes from the capture and never from the window, so a resize that
        // the host reports late reaches no text of this capture (D-707, T-7).
        ScreenFit fit = ScreenFit.Of(capture.Fit, capture.Width, capture.Height);
        int body = BodySize.DefaultFor(fit.Height, this.content.Style.SmallBody, this.content.Style.LargeBody);
        UiBase @base = UiBase.Load(this.content, body);

        if (string.CompareOrdinal(capture.Fixture, ScreenCaptures.MapFixture) == 0)
        {
            GameRun open = GameRun.Start(this.content, Boot.FixtureSeed, DebugSeam.Handlers(), FixtureSettings.Battle.Messages);
            MapFixture.Build(built, @base, open.Party, this.content, this.AmbientOf(capture), seekParticles: true, mode: capture.Mode);
            return;
        }

        if (string.CompareOrdinal(capture.Fixture, ScreenCaptures.TransitionFixture) == 0)
        {
            // The map of the map fixture, with one transition over it at a fixed tick, so one frame
            // gives one picture (D-172, exit tests 1 and 2 of PR-60).
            GameRun open = GameRun.Start(this.content, Boot.FixtureSeed, DebugSeam.Handlers(), FixtureSettings.Battle.Messages);
            MapFixture.Build(built, @base, open.Party, this.content, seekParticles: true);
            TransitionFrame shown = ScreenCaptures.TransitionFrameOf(capture.Frame);
            Transition transition = this.TransitionOf(shown.Look);
            int progress = ScreenCaptures.TransitionTick * ScreenHandOff.ProgressScale / transition.Ticks;
            built.HandOffPass.ShowAt(transition, progress, shown.Level, this.content.Palette);
            return;
        }

        if (string.CompareOrdinal(capture.Fixture, ScreenCaptures.PitFixture) == 0)
        {
            this.BuildPitRoom(built, @base);
            return;
        }

        if (string.CompareOrdinal(capture.Fixture, ScreenCaptures.StillFixture) == 0)
        {
            GameRun still = GameRun.Start(this.content, Boot.FixtureSeed, DebugSeam.Handlers(), FixtureSettings.Battle.Messages);
            this.walkRun = still;
            this.walkMap = MapFixture.Build(built, @base, still.Party, this.content, seekParticles: true);
            this.stepTicks = 0;
            return;
        }

        if (string.CompareOrdinal(capture.Fixture, ScreenCaptures.ScrollFixture) == 0)
        {
            // The same map, walked to the pit room, where the view follows the lead. The frames
            // of this fixture then hold a step that scrolls the view, and each particle of the
            // weather and of a torch must stay on the world under it (F-97).
            GameRun scrolled = GameRun.Start(this.content, Boot.FixtureSeed, DebugSeam.Handlers(), FixtureSettings.Battle.Messages);
            this.walkRun = scrolled;
            this.walkMap = MapFixture.Build(built, @base, scrolled.Party, this.content, seekParticles: true);
            this.walkMap.CarriedLightOn = true;
            foreach (string action in ScreenCaptures.PitRoute)
            {
                this.StepOnce(scrolled, action);
            }

            this.stepTicks = 0;
            return;
        }

        if (string.CompareOrdinal(capture.Fixture, ScreenCaptures.WalkFixture) == 0)
        {
            // The same running screen as the map fixture. The session keeps the run and the
            // map, and each later frame of the walk runs one tick of them (D-782).
            GameRun walked = GameRun.Start(this.content, Boot.FixtureSeed, DebugSeam.Handlers(), FixtureSettings.Battle.Messages);
            this.walkRun = walked;
            this.walkMap = MapFixture.Build(built, @base, walked.Party, this.content, seekParticles: true);

            // The walk carries the light, so each frame of a step shows the light at the drawn
            // place of the lead, inside the step too (D-847).
            this.walkMap.CarriedLightOn = true;
            return;
        }

        if (string.CompareOrdinal(capture.Fixture, ScreenCaptures.PictureFixture) == 0)
        {
            // The fixture picture fills the world viewport, as a backdrop layer of PR-10 does
            // (D-816, D-819).
            var view = new PictureView();
            built.World.AddChild(view);
            ContentId id = ContentId.Parse(ScreenCaptures.FixturePicture, LargePicture.Folder, "fixture");
            view.Build(@base.Atlas, this.content.PictureOf(id), this.content);
            return;
        }

        if (string.CompareOrdinal(capture.Fixture, ScreenCaptures.BattleFixture) == 0)
        {
            this.BuildBattle(built, @base, capture);
            return;
        }

        if (string.CompareOrdinal(capture.Fixture, ScreenCaptures.SettingsFixture) == 0)
        {
            this.BuildSettings(built, @base, capture);
            return;
        }

        if (string.CompareOrdinal(capture.Fixture, ScreenCaptures.MenuFixture) == 0)
        {
            this.BuildMenu(built, @base, capture);
            return;
        }

        if (string.CompareOrdinal(capture.Fixture, ScreenCaptures.NoticeFixture) == 0)
        {
            this.BuildNotice(built, @base, capture);
            return;
        }

        if (string.CompareOrdinal(capture.Fixture, ScreenCaptures.UiFixture) == 0)
        {
            var panel = new UiFixture();
            built.Layer.AddChild(panel);
            panel.Build(@base, this.content.Strings);
            return;
        }

        throw new ArgumentOutOfRangeException(
            nameof(capture),
            capture.Fixture,
            $"The capture list names the fixture '{capture.Fixture}', and the session builds none (T-2).");
    }

    /// <summary>
    /// Builds one window of the menu stack over the map of the fixture run (D-211, exit test 2 of
    /// PR-62). The task windows open beside the main list, with its cursor on their entry. The map
    /// frame walks <see cref="ScreenCaptures.DungeonRoute"/> first, and the log frame posts three
    /// notices that log through the console (D-989).
    /// </summary>
    /// <exception cref="InvalidOperationException">A tick of the walk or of a notice wrote an error (T-2).</exception>
    /// <exception cref="ArgumentOutOfRangeException">The capture names no frame of the menu fixture (T-2).</exception>
    /// <remarks>
    /// The session builds each view itself and sends no input event, because it builds no input
    /// map, and a read of an absent action writes an error line (T-2).
    /// </remarks>
    private void BuildMenu(FrameRoot built, UiBase @base, ScreenCapture capture)
    {
        GameRun open = GameRun.Start(this.content, Boot.FixtureSeed, DebugSeam.Handlers(), FixtureSettings.Battle.Messages);
        string frame = capture.Frame;
        if (string.CompareOrdinal(frame, ScreenCaptures.MenuMapFrame) == 0)
        {
            foreach (string action in ScreenCaptures.DungeonRoute)
            {
                this.StepOnce(open, action);
            }
        }

        // The debug command marks a swap place, and the next tick applies it (D-1030).
        if (string.CompareOrdinal(frame, ScreenCaptures.MenuLessonsSwapFrame) == 0)
        {
            _ = DebugSeam.Run("swap", () => open.State, open.Queue, () => false);
            this.RunTicks(open, 1);
        }

        if (string.CompareOrdinal(frame, ScreenCaptures.MenuLogFrame) == 0)
        {
            for (int notice = 0; notice < ScreenCaptures.LogFrameNotices; notice += 1)
            {
                _ = DebugSeam.Run("notice", () => open.State, open.Queue, () => false);
                this.RunTicks(open, 1);
            }
        }

        // The map stays visible beside the main list, so each particle takes the tick of the run and
        // never the clock of the engine, and two sessions draw the same pixels (D-172, T-7).
        MapScreen drawn = MapFixture.Build(built, @base, open.Party, this.content, seekParticles: true);
        drawn.ShowParty(open.Party, 0);
        drawn.ShowWeather(open.Tick, seek: true);
        if (string.CompareOrdinal(frame, ScreenCaptures.MenuMapFrame) == 0)
        {
            _ = new DungeonMapView(built, @base, open.Party);
            return;
        }

        var list = new MainList();
        MenuEntry entry = frame switch
        {
            ScreenCaptures.MenuListFrame or ScreenCaptures.MenuListDesktopFrame or ScreenCaptures.MenuPartyFrame => MenuEntry.Party,
            ScreenCaptures.MenuStatusFrame => MenuEntry.Status,
            ScreenCaptures.MenuLogFrame => MenuEntry.Log,
            ScreenCaptures.MenuLessonsFrame or ScreenCaptures.MenuLessonsSwapFrame => MenuEntry.Lessons,
            _ => throw new ArgumentOutOfRangeException(nameof(capture), frame, $"The menu fixture draws no frame '{frame}' (T-2)."),
        };
        while (list.Current != entry)
        {
            list.Move(1);
        }

        _ = new MainListView(built, @base, list);
        if (string.CompareOrdinal(frame, ScreenCaptures.MenuPartyFrame) == 0)
        {
            _ = new PartyView(built, @base, open.State, new PartyList(open.State.Characters.Members.Count));
        }
        else if (string.CompareOrdinal(frame, ScreenCaptures.MenuStatusFrame) == 0)
        {
            _ = new StatusView(built, @base, this.content.Strings, open.State);
        }
        else if (string.CompareOrdinal(frame, ScreenCaptures.MenuLogFrame) == 0)
        {
            _ = new LogView(built, @base, open.State);
        }
        else if (string.CompareOrdinal(frame, ScreenCaptures.MenuLessonsFrame) == 0 || string.CompareOrdinal(frame, ScreenCaptures.MenuLessonsSwapFrame) == 0)
        {
            var cursor = new LessonCursor(open.State);
            if (string.CompareOrdinal(frame, ScreenCaptures.MenuLessonsSwapFrame) == 0 && cursor.Confirm() is not null)
            {
                throw new InvalidOperationException($"The capture '{capture.FileName}' confirmed a slot, and the window sent an intent before the pack list (D-1030, T-2).");
            }

            _ = new LessonsView(built, @base, this.content.Strings, open.State, cursor);
        }
    }

    /// <summary>
    /// Builds the running screen with the notice of the console at one tick of its type-out or of
    /// its hold (D-221, D-994). The run posts the notice on its first tick, and each later tick
    /// takes the time of exactly one tick, so each session shows the same letters (T-7).
    /// </summary>
    /// <exception cref="InvalidOperationException">A tick wrote an error, or no notice shows at the frame (T-2).</exception>
    private void BuildNotice(FrameRoot built, UiBase @base, ScreenCapture capture)
    {
        GameRun open = GameRun.Start(this.content, Boot.FixtureSeed, DebugSeam.Handlers(), FixtureSettings.Battle.Messages);
        _ = DebugSeam.Run("notice", () => open.State, open.Queue, () => false);
        this.RunTicks(open, 1);
        bool typing = string.CompareOrdinal(capture.Frame, ScreenCaptures.NoticeTypeFrame) == 0;
        this.RunTicks(open, typing ? ScreenCaptures.NoticeTypeTicks : ScreenCaptures.NoticeHoldTicks);

        MapScreen drawn = MapFixture.Build(built, @base, open.Party, this.content, seekParticles: true);
        drawn.ShowParty(open.Party, 0);
        drawn.ShowWeather(open.Tick, seek: true);
        NoticeFrame shown = open.NoticeAt(TextSpeeds.CharactersPerSecond(FixtureSettings.Access.Text)) ?? throw new InvalidOperationException(
            $"The capture '{capture.FileName}' shows no notice at tick {open.Tick}, and the console posted one (D-994, T-2).");
        new NoticeBox(built, @base).Show(shown, underMenu: false);
    }

    /// <summary>Runs whole ticks of a run, one tick for each call of the loop, and fails on an error line (T-2).</summary>
    /// <exception cref="InvalidOperationException">A tick wrote an error (T-2).</exception>
    private void RunTicks(GameRun run, int ticks)
    {
        for (int tick = 0; tick < ticks; tick += 1)
        {
            foreach (LogEntry entry in run.Advance(OneTickSeconds))
            {
                if (entry.Level == LogLevel.Error)
                {
                    throw new InvalidOperationException($"The tick {run.Tick} of a capture wrote an error: {entry.Message} (T-2).");
                }
            }
        }
    }

    /// <summary>
    /// Builds the settings screen over the paused map, with the default settings and never the
    /// file of the person (D-860, D-871). The conflict frame remaps the gamepad slot of the
    /// back action to the button of confirm, so the screen shows the conflict line (D-862).
    /// </summary>
    private void BuildSettings(FrameRoot built, UiBase @base, ScreenCapture capture)
    {
        GameRun open = GameRun.Start(this.content, Boot.FixtureSeed, DebugSeam.Handlers(), FixtureSettings.Battle.Messages);
        MapFixture.Build(built, @base, open.Party, this.content);

        int autoBody = BodySize.DefaultFor(built.Fit.Height, this.content.Style.SmallBody, this.content.Style.LargeBody);
        SettingsScreen screen = SettingsScreen.Build(built, @base, this.content.Strings, FixtureSettings, autoBody);
        if (string.CompareOrdinal(capture.Frame, ScreenCaptures.SettingsConflictFrame) != 0)
        {
            return;
        }

        int row = SettingsMenu.RowOf(InputActions.Cancel);
        screen.Menu.Point(row, BindingSlot.Gamepad);
        screen.Menu.Choose();
        screen.Menu.Capture(InputBinding.OfButton((int)JoyButton.A));
        if (screen.Menu.CanClose)
        {
            throw new InvalidOperationException(
                $"The capture '{capture.FileName}' put the button of confirm on the back action, and the menu found no conflict (D-862, T-2).");
        }

        screen.Show();
    }

    /// <summary>
    /// Walks the run of the fixture seed into the fixture fight, and builds the battle screen
    /// at the moment of one frame (D-172, D-827).
    /// </summary>
    /// <param name="built">The frame of this capture.</param>
    /// <param name="base">The atlas, the theme, and the text helper.</param>
    /// <param name="capture">The capture, whose frame names the moment.</param>
    /// <exception cref="InvalidOperationException">The walk or the menu failed (T-2).</exception>
    /// <remarks>
    /// The run takes the time of exactly one tick on each call, so every session reaches the
    /// same tick, the same sway of the backdrop, and the same pixels (T-7, D-782). The menu
    /// frames show the first command of the fight. The target frame presses confirm on the
    /// attack. Each frame of a blow plays a hit of a character to the ticks after the blow that
    /// <see cref="ScreenCaptures.TicksAfterBlowOf"/> gives.
    /// <para>
    /// The sparks frame walks to the deep room, where the character hits the brute (D-882). The
    /// waiting frame walks there too, because the grunt of that group waits in the column (D-953).
    /// The experience frame fights to the win, and each level-up frame stages a level-up at the
    /// ticks of that experience, because the fixture fight gives no level-up (D-975, D-977).
    /// The stop frame and each heavy frame stage the same hit on a weakness, because no move of
    /// the fixture fight carries an element before PR-12 (D-877).
    /// </para>
    /// </remarks>
    private void BuildBattle(FrameRoot built, UiBase @base, ScreenCapture capture)
    {
        GameRun fight = GameRun.Start(this.content, Boot.FixtureSeed, DebugSeam.Handlers(), FixtureSettings.Battle.Messages);
        if (string.CompareOrdinal(capture.Frame, ScreenCaptures.BattleSparksFrame) == 0
            || string.CompareOrdinal(capture.Frame, ScreenCaptures.BattleWaitingFrame) == 0)
        {
            BattleWalk.ToFirstCommandOfElite(fight);
        }
        else
        {
            BattleWalk.ToFirstCommand(fight);
        }

        if (ScreenCaptures.TicksAfterBlowOf(capture.Frame) is int afterBlow)
        {
            BattleWalk.ToBlowOfCharacter(fight, fight.Pace.BlowTick + afterBlow);
        }

        if (ScreenCaptures.StagesSpell(capture.Frame))
        {
            BattleWalk.ToSpellOfCharacter(fight, ScreenCaptures.SpellFrameTicks);
        }

        if (ScreenCaptures.ExperienceTicksOf(capture.Frame) is int intoExperience)
        {
            BattleWalk.ToExperienceOfCharacter(fight, intoExperience);
        }

        BattleScreen screen = BattleScreen.Build(
            built,
            @base,
            this.content,
            fight,
            new CommandMemory(FixtureSettings.Battle.RememberCursor),
            ScreenCaptures.LevelOf(capture.Frame),
            this.AmbientOf(capture));
        screen.SeekParticles = true;

        // A fight draws no light shaft, so the mode of the capture reaches the blur and the vignette alone (D-917, D-920).
        if (capture.Mode is PassMode mode)
        {
            built.ShowPasses(this.content.Light.Passes.WithMode(mode), this.content.Palette);
        }
        if (ScreenCaptures.OpensLessons(capture.Frame))
        {
            this.OpenLessons(screen, capture);
        }

        if (string.CompareOrdinal(capture.Frame, ScreenCaptures.BattleTargetFrame) == 0
            && screen.Read(InputActions.Confirm) is not null)
        {
            throw new InvalidOperationException(
                $"The capture '{capture.FileName}' pressed confirm on the attack, and the menu sent an intent before a target (D-827, T-2).");
        }

        if (ScreenCaptures.StagesHeavyBlow(capture.Frame))
        {
            BattleEvent blow = fight.PlayingEvent ?? throw new InvalidOperationException(
                $"The capture '{capture.FileName}' stages a heavy blow, and the fight plays no event (D-877, T-2).");
            screen.ShowStaged(fight, blow with { Affinity = Affinity.Weak });
            return;
        }

        if (ScreenCaptures.StagesLevelUp(capture.Frame))
        {
            // The staged level-up takes the first character from level 1 to 2 on the view alone,
            // at the ticks of the experience that plays (D-975, D-977).
            BattleView view = fight.BattleView ?? throw new InvalidOperationException(
                $"The capture '{capture.FileName}' stages a level-up, and the run holds no view of a fight (D-975, T-2).");
            var levelUp = new BattleEvent(BattleEventKind.LevelUp, new BattleTarget(BattleSide.Party, 0), null, view.Party[0].Level + 1);
            view.Apply(levelUp);
            screen.ShowStaged(fight, levelUp);
            return;
        }

        screen.Show(fight);
    }

    /// <summary>
    /// Opens the lesson list of the command menu, and for the forms frame the forms of the cinder,
    /// the second lesson of the first character (D-1027, D-1031). No press sends an intent.
    /// </summary>
    private void OpenLessons(BattleScreen screen, ScreenCapture capture)
    {
        List<string> presses = [InputActions.StepEast, InputActions.Confirm];
        if (string.CompareOrdinal(capture.Frame, ScreenCaptures.BattleFormsFrame) == 0)
        {
            presses.AddRange([InputActions.StepEast, InputActions.Confirm]);
        }

        foreach (string press in presses)
        {
            if (screen.Read(press) is not null)
            {
                throw new InvalidOperationException($"The capture '{capture.FileName}' pressed '{press}', and the menu sent an intent before a target (D-1031, T-2).");
            }
        }
    }

    /// <summary>
    /// Gives the image in the sRGB colors that the screen shows. The window renders in linear
    /// HDR 2D, and its image holds linear half floats (D-188, <see cref="CaptureColors"/>).
    /// </summary>
    /// <param name="window">The image of the root viewport.</param>
    /// <param name="capture">The capture, for the error message.</param>
    /// <returns>An image of 8-bit RGBA in sRGB.</returns>
    /// <exception cref="InvalidOperationException">The image holds another format, or Godot made no image (T-2).</exception>
    private static Image ToScreenColors(Image window, ScreenCapture capture)
    {
        if (window.GetFormat() != Image.Format.Rgbh)
        {
            throw new InvalidOperationException(
                $"The capture '{capture.FileName}' holds the format {window.GetFormat()}, and the "
                + $"window of HDR 2D gives {Image.Format.Rgbh}. Read the setting `viewport/hdr_2d` (D-188, T-2).");
        }

        int width = window.GetWidth();
        int height = window.GetHeight();
        byte[] srgb = CaptureColors.ToSrgb(window.GetData(), width * height);

        // The call reports a failure in the log alone, so the result takes a check (F-45, T-2).
        Image? image = Image.CreateFromData(width, height, false, Image.Format.Rgba8, srgb);
        return image ?? throw new InvalidOperationException(
            $"Godot made no image from the sRGB bytes of the capture '{capture.FileName}' (T-2).");
    }

    /// <summary>Reads the window of this frame and writes one PNG.</summary>
    /// <param name="capture">The capture that this file holds.</param>
    /// <exception cref="InvalidOperationException">The window or the image holds another size (T-2).</exception>
    private void Write(ScreenCapture capture)
    {
        Image window = this.GetViewport().GetTexture().GetImage();
        if (window.GetWidth() != capture.Width || window.GetHeight() != capture.Height)
        {
            throw new InvalidOperationException(
                $"The capture '{capture.FileName}' asks for {capture.Width} by {capture.Height} pixels, " +
                $"and the window gave {window.GetWidth()} by {window.GetHeight()}. The screen of the " +
                $"session is too small, or the window did not resize in {FramesBeforeCapture} frames (T-2).");
        }

        Image image = ToScreenColors(window, capture);

        byte[] bytes = image.SavePngToBuffer();
        if (bytes.Length == 0)
        {
            throw new InvalidOperationException(
                $"Godot wrote no PNG bytes for the capture '{capture.FileName}' (T-2).");
        }

        File.WriteAllBytes(Path.Combine(this.folder, capture.FileName), bytes);
    }
}
