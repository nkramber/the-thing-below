using System;
using System.Collections.Generic;
using System.IO;
using Godot;
using TheThingBelow.Core;
using TheThingBelow.Core.Battles;
using TheThingBelow.Core.Content;
using TheThingBelow.Core.Crashes;
using TheThingBelow.Core.Logging;
using TheThingBelow.Core.Maps;
using TheThingBelow.Core.Runs;
using TheThingBelow.Core.Saves;
using TheThingBelow.Game.Ui;
using TheThingBelow.Storage;

namespace TheThingBelow.Game;

/// <summary>
/// The first node of the game. It opens the log file of the session and starts the run, or it
/// runs the smoke session and quits (D-117, D-179).
/// </summary>
/// <remarks>
/// Godot prints an error of a callback and runs on, so this node catches the error of each
/// callback itself. A crash writes the crash file through Storage, writes one log line, and
/// quits with <see cref="CrashExitCode"/> (D-170, D-559, T-2).
/// <para>
/// A crash shows <see cref="CrashScreen"/> with the name of the crash file and the address
/// of D-473, through the one text helper of D-499 (D-559, D-712). The session then quits on
/// the next input event. A session with no display quits at once, so the smoke job of CI
/// still ends with the crash code (D-117, T-2).
/// </para>
/// </remarks>
public partial class Boot : Node
{
    /// <summary>The argument that asks for the smoke session of the CI gate (D-117).</summary>
    public const string SmokeArgument = "--smoke";

    /// <summary>The argument that puts the debug lines of each subsystem in the log file (D-660).</summary>
    public const string DebugLogArgument = "--log-debug";

    /// <summary>
    /// The argument that asks for the capture session of the screen-test job (D-172, D-732).
    /// The argument after it names the folder that takes one PNG for each capture.
    /// </summary>
    public const string CaptureArgument = "--capture";

    /// <summary>
    /// The argument that limits the capture session to one fixture, such as `walk` (D-782).
    /// The argument after it names the fixture. A session with no such argument takes every
    /// capture.
    /// </summary>
    public const string FixtureArgument = "--fixture";

    /// <summary>The exit code of a session that ends with no error (T-2).</summary>
    public const int SuccessExitCode = 0;

    /// <summary>The exit code of a session that a crash ended (D-170, T-2).</summary>
    private const int CrashExitCode = 1;

    /// <summary>
    /// The seed of the run of this build. The title screen of PR-33 and the load of a save
    /// in PR-16 pick the seed of a real run, and this constant stands until then (D-258, G-3).
    /// </summary>
    public const ulong FixtureSeed = 20260918;

    /// <summary>The count of frames that the smoke session runs, at one frame of 1/60 second.</summary>
    private const int SmokeFrameCount = 120;

    /// <summary>The time of one frame of the smoke session, in seconds (D-164).</summary>
    private const double SmokeFrameSeconds = 1.0 / FixedStepLoop.TicksPerSecond;

    /// <summary>The frame of the smoke session that opens the menu, which logs one line (D-179).</summary>
    private const int SmokeMenuOpenFrame = 10;

    /// <summary>The frame of the smoke session that closes the menu, which logs one line (D-179).</summary>
    private const int SmokeMenuCloseFrame = 70;

    /// <summary>The folder inside the crash folder that the smoke session writes its check into (D-659).</summary>
    private const string SmokeFolderName = "smoke";

    /// <summary>The most frames that the smoke session walks or fights before it fails (T-2).</summary>
    private const int SmokeBattleFrames = 6000;

    /// <summary>The column of the east end of the hall of the first map, where the smoke walk turns south.</summary>
    private const int SmokeTurnColumn = 26;

    /// <summary>The row of the night route of the hall patrol, where the smoke walk turns west.</summary>
    private const int SmokePatrolRow = 7;

    /// <summary>The line that the smoke session types in the console of a development build (D-724).</summary>
    private const string SmokeConsoleLine = "help";

    /// <summary>The name that Godot gives the display server of a session with no window.</summary>
    private const string HeadlessDisplay = "headless";

    private LogStore? log;
    private GameRun? run;
    private ContentSet? content;
    private UiBase? ui;
    private int builtBody;
    private FrameRoot? frame;
    private MapScreen? map;
    private Control? console;
    private readonly HeldSteps held = new();
    private bool crashed;

    /// <summary>Opens the log file, reads the arguments, and picks the session.</summary>
    public override void _Ready()
    {
        try
        {
            this.StartSession();
        }
        catch (Exception fault)
        {
            this.ReportCrash(fault);
        }
    }

    /// <summary>
    /// Runs the ticks of one frame and writes their log entries (D-164, D-179). The frame time
    /// comes from the engine, and the fixed-step loop turns it into whole ticks, so no Godot
    /// timer or physics step reaches the simulation (D-100, G-23).
    /// </summary>
    /// <param name="delta">The time of the frame, in seconds.</param>
    public override void _Process(double delta)
    {
        if (this.run is null)
        {
            return;
        }

        try
        {
            this.QueueHeldStep();
            this.WriteLog(this.run.Advance(delta));
            if (this.run.WipeReady)
            {
                this.ReloadAfterWipe();
            }

            this.map?.ShowParty(this.run.Party);
        }
        catch (Exception fault)
        {
            this.ReportCrash(fault);
        }
    }

    /// <summary>
    /// Makes the step intent of the direction that the player holds now (D-493, D-716). The
    /// party walks while a key or a button is down, so one intent goes to each tick.
    /// </summary>
    /// <remarks>
    /// The held set comes from the press events and the release events, and never from a
    /// poll of the input singleton (F-50). A menu pauses the world and takes every input of
    /// the player, so no step intent goes out for a tick that a menu pauses (D-162, T-2).
    /// </remarks>
    private void QueueHeldStep()
    {
        GameRun? open = this.run;

        // The queue can already hold the intent that opens the menu, because the host reads
        // input before it runs the ticks of a frame. A step intent for that tick would meet
        // the refusal of the rules (D-162, T-2). An encounter holds the map still, so a step
        // intent then moves nothing, and the record stays free of it (D-531).
        if (open is null || open.MenuOpenNextTick || open.InBattle)
        {
            return;
        }

        string? action = this.held.Newest;
        if (action is not null)
        {
            open.Queue(open.IntentOf(action));
        }
    }

    /// <summary>
    /// Opens the log file of the session, checks the save folder, and starts the run or the
    /// smoke session.
    /// </summary>
    /// <remarks>
    /// The log file opens before the check of the save folder, so the log of a session holds
    /// the reason when the check of D-657 fails (T-2).
    /// </remarks>
    private void StartSession()
    {
        string[] userArguments = OS.GetCmdlineUserArgs();

        this.log = LogStore.OfThisSystem(MinimumLevelOf(userArguments));
        this.log.Open(DateTime.UtcNow);
        this.WriteLog([StartEntry()]);

        CheckSaveFolder();

        if (Array.IndexOf(userArguments, SmokeArgument) >= 0)
        {
            this.RunSmokeSession();
            return;
        }

        string? captureFolder = CaptureFolderOf(userArguments);
        if (captureFolder is not null)
        {
            CaptureSession.Start(
                this, LoadContent(), captureFolder, CapturesOf(userArguments), this.ReportCrash);
            return;
        }

        ContentSet loaded = LoadContent();
        this.content = loaded;
        this.run = GameRun.Start(loaded, FixtureSeed, DebugSeam.Handlers());
        GameInputMap.Build();
        EnterBorderlessFullscreen();
        this.BuildScreen(loaded);
        this.ReportWindowMode();
        this.GetWindow().SizeChanged += this.OnWindowSizeChanged;
    }

    /// <summary>
    /// Reads the folder of the capture session from the arguments of the session (D-732).
    /// </summary>
    /// <param name="userArguments">The arguments after the two dashes of the session.</param>
    /// <returns>The folder, or null when the session holds no capture argument.</returns>
    /// <exception cref="InvalidOperationException">The argument names no folder (T-2).</exception>
    private static string? CaptureFolderOf(string[] userArguments)
    {
        int mark = Array.IndexOf(userArguments, CaptureArgument);
        if (mark < 0)
        {
            return null;
        }

        if (mark + 1 >= userArguments.Length || userArguments[mark + 1].Length == 0)
        {
            throw new InvalidOperationException(
                $"The argument '{CaptureArgument}' takes the folder of the captures after it, " +
                $"and this session gave none (T-2).");
        }

        return userArguments[mark + 1];
    }

    /// <summary>Reads the captures of the capture session from the arguments of the session (D-782).</summary>
    /// <param name="userArguments">The arguments after the two dashes of the session.</param>
    /// <returns>Every capture, or the captures of the fixture that the arguments name.</returns>
    /// <exception cref="InvalidOperationException">The argument names no fixture (T-2).</exception>
    /// <exception cref="ArgumentOutOfRangeException">No capture has that fixture (T-2).</exception>
    private static IReadOnlyList<ScreenCapture> CapturesOf(string[] userArguments)
    {
        int mark = Array.IndexOf(userArguments, FixtureArgument);
        if (mark < 0)
        {
            return ScreenCaptures.All;
        }

        if (mark + 1 >= userArguments.Length || userArguments[mark + 1].Length == 0)
        {
            throw new InvalidOperationException(
                $"The argument '{FixtureArgument}' takes the name of a fixture after it, " +
                $"and this session gave none (T-2).");
        }

        return ScreenCaptures.OfFixture(userArguments[mark + 1]);
    }

    /// <summary>
    /// Builds the frame, the UI base, and the map (D-524, D-561, D-568, D-815).
    /// </summary>
    /// <param name="loaded">The content set of this build.</param>
    /// <remarks>
    /// The map draws in the world viewport at 1x, and the frame shows that viewport at 2x
    /// (D-633, D-634). The game shows no button prompt, so no row draws on the frame (D-815).
    /// <para>
    /// The body size comes from the fit of the frame on this screen, and no setting exists
    /// yet. PR-63 adds the display setting that changes it (D-707).
    /// </para>
    /// </remarks>
    private void BuildScreen(ContentSet loaded)
    {
        var built = new FrameRoot();
        this.AddChild(built);
        this.frame = built;

        UiStyle style = loaded.Style;
        int body = BodySize.DefaultFor(built.Fit.Height, style.SmallBody, style.LargeBody);
        UiBase built_ui = UiBase.Load(loaded, body);
        this.ui = built_ui;
        this.builtBody = body;

        GameRun open = this.run ?? throw new InvalidOperationException(
            $"The screen built before the run started (T-2).");

        // The capture session of the screen-test job builds the same map (D-172, D-734).
        this.map = MapFixture.Build(built, built_ui, open.Party);

        this.BuildConsole(built, open);
    }

    /// <summary>
    /// Starts the run again after a wipe, and builds the screen again over the new run, so the
    /// map and the console read it (D-231, D-397, D-776).
    /// </summary>
    /// <exception cref="InvalidOperationException">The session loaded no content (T-2).</exception>
    private void ReloadAfterWipe()
    {
        ContentSet loaded = this.content ?? throw new InvalidOperationException(
            "A wipe reloads the run, and the session loaded no content (T-2).");

        GameRun reloaded = ReloadRun(loaded);
        this.run = reloaded;
        this.RebuildScreen(loaded);
        this.WriteLog([new LogEntry(
            LogLevel.Info,
            "the party wiped and the run reloaded",
            reloaded.Tick,
            LogSubsystems.Game,
            [LogField.OfNumber("tick", reloaded.Tick)])]);
    }

    /// <summary>Removes the frame and its nodes, and builds the screen again over the current run.</summary>
    /// <param name="loaded">The content set of this build.</param>
    private void RebuildScreen(ContentSet loaded)
    {
        this.frame?.QueueFree();
        this.frame = null;
        this.map = null;
        this.console = null;
        this.BuildScreen(loaded);
    }

    /// <summary>
    /// Builds the screen again when a new window size gives another default body size (D-707).
    /// </summary>
    /// <remarks>
    /// The window opens in borderless fullscreen, and on some systems it reaches the size of
    /// the screen only after the first frame. The frame refits by itself, but the body size
    /// comes from the fit when the screen builds. Thus a screen that built at the first size
    /// would keep the text size of the wrong fit.
    /// <para>
    /// The fit comes from the window size here, and never from the frame, because the frame
    /// reads the same signal and the order of two handlers is not a contract.
    /// </para>
    /// </remarks>
    private void OnWindowSizeChanged()
    {
        try
        {
            ContentSet? loaded = this.content;
            FrameRoot? built = this.frame;
            if (loaded is null || built is null)
            {
                return;
            }

            Vector2I screen = this.GetWindow().Size;
            ScreenFit fit = ScreenFit.Of(built.Mode, Math.Max(1, screen.X), Math.Max(1, screen.Y));
            int body = BodySize.DefaultFor(fit.Height, loaded.Style.SmallBody, loaded.Style.LargeBody);
            if (body == this.builtBody)
            {
                return;
            }

            int before = this.builtBody;
            this.RebuildScreen(loaded);
            this.WriteLog([new LogEntry(
                LogLevel.Info,
                "the window size changed the default body size, and the screen built again",
                this.run?.Tick ?? 0,
                LogSubsystems.Game,
                [
                    LogField.OfNumber("width", screen.X),
                    LogField.OfNumber("height", screen.Y),
                    LogField.OfNumber("body_before", before),
                    LogField.OfNumber("body", this.builtBody),
                ])]);
        }
        catch (Exception fault)
        {
            this.ReportCrash(fault);
        }
    }

    /// <summary>
    /// Puts the window of the play session in borderless fullscreen: a window with no border
    /// that covers the screen, and not the exclusive mode. Every build takes it, the
    /// development build included.
    /// </summary>
    /// <remarks>
    /// The play session sets the mode, and the project keeps the windowed default. The capture
    /// session sets the exact window size of each capture, and Godot ignores its `--windowed`
    /// option when the project asks for fullscreen. The smoke session has no window.
    /// <para>
    /// On macOS the switch ends a few frames later. The frame refits on each size change, and
    /// <see cref="OnWindowSizeChanged"/> builds the screen again when the body size moves.
    /// </para>
    /// </remarks>
    private static void EnterBorderlessFullscreen()
    {
        DisplayServer.WindowSetMode(DisplayServer.WindowMode.Fullscreen);
    }

    /// <summary>
    /// Writes the window mode and the window size of the start to the log. A mode other than
    /// borderless fullscreen is an error line, because the project asks for that mode (T-2).
    /// </summary>
    private void ReportWindowMode()
    {
        DisplayServer.WindowMode mode = DisplayServer.WindowGetMode();
        Vector2I screen = this.GetWindow().Size;
        bool fullscreen = mode == DisplayServer.WindowMode.Fullscreen;
        this.WriteLog([new LogEntry(
            fullscreen ? LogLevel.Info : LogLevel.Error,
            fullscreen
                ? "the window opened in borderless fullscreen"
                : "the window opened in another mode than borderless fullscreen, which the project asks for",
            0,
            LogSubsystems.Game,
            [
                new LogField("mode", mode.ToString()),
                LogField.OfNumber("width", screen.X),
                LogField.OfNumber("height", screen.Y),
            ])]);
    }

    /// <summary>Gives the run after a wipe: the newer save, or a new run when no save exists (D-231, D-776).</summary>
    /// <param name="loaded">The content set of this build.</param>
    /// <returns>The run.</returns>
    private static GameRun ReloadRun(ContentSet loaded)
    {
        SaveStore saves = SaveStore.OfThisSystem();
        SaveDocument? slot = saves.Exists(SaveKind.Slot) ? saves.Read(SaveKind.Slot) : null;
        SaveDocument? autosave = saves.Exists(SaveKind.Autosave) ? saves.Read(SaveKind.Autosave) : null;
        return GameRun.Reload(loaded, slot, autosave, FixtureSeed, DebugSeam.Handlers());
    }

    /// <summary>
    /// Builds the debug console of a development build, and adds it to the frame layer above
    /// every other node (D-171, D-725). A release build builds none, because it holds no debug
    /// assembly (D-260, D-492).
    /// </summary>
    /// <param name="built">The frame, which holds the layer of every UI node (D-568).</param>
    /// <param name="open">The run that the console reads and sends its intents to.</param>
    private void BuildConsole(FrameRoot built, GameRun open)
    {
        if (!DebugSeam.TryBuildConsole(() => open.State, open.Queue, out Control? made) || made is null)
        {
            return;
        }

        built.Layer.AddChild(made);
        made.Visible = false;
        this.console = made;
    }

    /// <summary>
    /// Reads each key event before any node of the frame reads it, and sends it on its route
    /// (D-171, D-725, D-813).
    /// </summary>
    /// <param name="signal">Every input event of this frame.</param>
    /// <remarks>
    /// The console draws in the frame viewport, which gets no event from the engine, so this
    /// method pushes each key of an open console there itself. The method then marks the event
    /// as handled, and no other node reads it. Before this rule the entry held the focus and
    /// read no key at all, so no battle could run from the console.
    /// </remarks>
    public override void _Input(InputEvent signal)
    {
        if (this.crashed || signal is not InputEventKey key)
        {
            return;
        }

        KeyRoute route = KeyRoutes.Of(
            (long)key.Keycode,
            key.Pressed,
            key.Echo,
            this.console?.Visible == true,
            this.QuitAllowed());

        if (route == KeyRoute.Game || (route == KeyRoute.ToggleConsole && this.console is null))
        {
            return;
        }

        try
        {
            this.Follow(route, key);
        }
        catch (Exception fault)
        {
            this.ReportCrash(fault);
        }

        GetViewport().SetInputAsHandled();
    }

    /// <summary>
    /// Tells whether the quit key ends this session: a development build with a run on the
    /// map and no menu (D-813). A release build reads the key as cancel alone.
    /// </summary>
    /// <returns>True when the quit key ends the session.</returns>
    private bool QuitAllowed() =>
        DebugSeam.IsDevelopmentBuild && this.run is not null && !this.run.MenuOpenNextTick;

    /// <summary>Does what the route of one key event asks (D-725, D-813).</summary>
    /// <param name="route">The route, other than <see cref="KeyRoute.Game"/>.</param>
    /// <param name="key">The key event.</param>
    /// <exception cref="InvalidOperationException">The route needs a node that this session has not built (T-2).</exception>
    private void Follow(KeyRoute route, InputEventKey key)
    {
        switch (route)
        {
            case KeyRoute.ToggleConsole:
            case KeyRoute.CloseConsole:
                this.ToggleConsole(this.console ?? throw new InvalidOperationException(
                    $"The key route '{route}' needs the debug console, and this session built none (T-2)."));
                return;
            case KeyRoute.Console:
                FrameRoot frame = this.frame ?? throw new InvalidOperationException(
                    "The debug console is open, and this session built no frame to take its keys (D-725, T-2).");
                frame.PushToLayer(key);
                return;
            case KeyRoute.Quit:
                this.WriteLog([new LogEntry(
                    LogLevel.Info,
                    "the quit key ended the session of a development build",
                    this.run?.Tick ?? 0,
                    LogSubsystems.Game,
                    [])]);
                GetTree().Quit(SuccessExitCode);
                return;
            default:
                throw new InvalidOperationException(
                    $"The key route '{route}' has no action here (T-2).");
        }
    }

    /// <summary>
    /// Opens the debug console or closes it, and forgets every held direction (D-725).
    /// </summary>
    /// <param name="open">The node of the console.</param>
    /// <remarks>
    /// The console takes every key of the person while it is open, so the release of a held
    /// direction never reaches <see cref="HeldSteps"/>. A party that kept the direction would
    /// walk on while the person types, so the set empties on each change (T-2).
    /// </remarks>
    private void ToggleConsole(Control open)
    {
        open.Visible = !open.Visible;
        this.held.Clear();
        this.WriteLog([new LogEntry(
            LogLevel.Debug,
            open.Visible ? "the debug console opened" : "the debug console closed",
            this.run?.Tick ?? 0,
            LogSubsystems.Game,
            [])]);
    }

    /// <summary>
    /// Reads one input event, and makes at most one intent (D-493, F-50).
    /// </summary>
    /// <param name="signal">The event that no node of the frame took.</param>
    /// <remarks>
    /// Game makes each intent from an event and never from a poll of the input singleton,
    /// because a poll ignores what a menu already took and would make a second intent for
    /// one press (F-50).
    /// </remarks>
    public override void _UnhandledInput(InputEvent signal)
    {
        if (signal is null)
        {
            return;
        }

        if (this.crashed)
        {
            // The message of the crash stands until the player presses a button (D-559).
            if (signal.IsPressed())
            {
                GetTree().Quit(CrashExitCode);
            }

            return;
        }

        try
        {
            this.ReadInput(signal);
        }
        catch (Exception fault)
        {
            this.ReportCrash(fault);
        }
    }

    /// <summary>Makes the intent of one action.</summary>
    /// <param name="signal">The event of this frame.</param>
    private void ReadInput(InputEvent signal)
    {
        if (this.console is not null && this.console.Visible)
        {
            // The person types in the console, and the game makes no intent at all until it
            // closes. The entry takes every key, and a gamepad event still reaches this
            // method, so the refusal stands here for every device (D-725, T-2).
            return;
        }

        // The party walks while a direction is down, so the map needs the press and the
        // release of each step action, and never a poll (D-716, F-50).
        this.held.Read(signal);

        GameRun? run = this.run;
        if (run is null)
        {
            return;
        }

        foreach (string action in InputActions.Names)
        {
            // A step action moves the party while the player holds it, so `QueueHeldStep`
            // makes its intent on each tick and this loop skips it (D-716, F-50).
            if (InputActions.IsStep(action) || !signal.IsActionPressed(action))
            {
                continue;
            }

            // The run holds the menu state, so the menu action opens the menu and closes it
            // (D-162, D-650).
            Intent made = run.IntentOf(action);
            if (string.CompareOrdinal(action, InputActions.Menu) == 0)
            {
                run.Queue(made);
            }
            else
            {
                // No rule of this build reads the choice intents. PR-16 gives them the
                // door, the chest, and the save point of a map, and the session logs each
                // one until then (D-493, G-16).
                this.WriteLog([new LogEntry(
                    LogLevel.Debug,
                    "the player made an intent that no rule of this build reads",
                    run.Tick,
                    LogSubsystems.Game,
                    [new LogField("action", action), new LogField("intent", made.Action.Value)])]);
            }

            return;
        }
    }

    /// <summary>Gives the lowest level that the log file of this session holds (D-660).</summary>
    /// <param name="userArguments">The arguments after the two dashes of the session.</param>
    /// <returns>The debug level when the session asks for it, and the info level otherwise.</returns>
    private static LogLevel MinimumLevelOf(string[] userArguments) =>
        Array.IndexOf(userArguments, DebugLogArgument) >= 0 ? LogLevel.Debug : LogLevel.Info;

    /// <summary>The first entry of the log file: the versions of this build (D-179, D-448).</summary>
    private static LogEntry StartEntry() =>
        new(
            LogLevel.Info,
            "the session started",
            0,
            LogSubsystems.Game,
            [
                new LogField("game", GameVersion.Current),
                LogField.OfNumber("simulation", SimulationVersion.Current),
            ]);

    /// <summary>Writes the entries of one step or of one frame to the log file (D-179).</summary>
    /// <param name="entries">The entries that Core returned.</param>
    /// <returns>The count of lines that the file took, which drops the levels below the minimum.</returns>
    /// <exception cref="InvalidOperationException">No log file is open (T-2).</exception>
    private int WriteLog(IReadOnlyList<LogEntry> entries)
    {
        LogStore store = this.log ?? throw new InvalidOperationException(
            "The session writes a log entry, and it opened no log file (T-2).");

        return store.Write(entries, DateTime.UtcNow);
    }

    /// <summary>
    /// Writes the crash file, writes one log line, and quits with the crash code (D-170,
    /// D-559, T-2).
    /// </summary>
    /// <param name="fault">The error that stopped the game.</param>
    /// <remarks>
    /// The run stops before the write, so no later frame steps a state that an error left. A
    /// crash before the log file exists writes the crash file alone, because the folder of the
    /// person is the reason of such a crash (T-2).
    /// </remarks>
    private void ReportCrash(Exception fault)
    {
        GameRun? stopped = this.run;
        this.run = null;

        DateTime time = DateTime.UtcNow;
        string? path = null;
        try
        {
            path = CrashStore.OfThisSystem().Write(fault, stopped?.Record(), time);
            GD.PrintErr($"the game stopped with an error, and it wrote the crash file '{path}'.");
        }
        catch (Exception second)
        {
            // The report of the second error never hides the first one (T-2, G-18).
            GD.PrintErr($"the game stopped with this error: {fault}");
            GD.PrintErr($"the game could not write the crash file of that error: {second}");
        }

        if (path is not null)
        {
            try
            {
                this.log?.Write([CrashEntry(fault, Path.GetFileName(path), stopped)], time);
            }
            catch (Exception second)
            {
                // The crash file exists, and the log line alone failed. The message says so.
                GD.PrintErr($"the game could not write the log line of the crash file '{path}': {second}");
            }
        }

        if (this.ShowCrashMessage(path))
        {
            this.crashed = true;
            return;
        }

        GetTree().Quit(CrashExitCode);
    }

    /// <summary>
    /// Shows the message of a crash on the frame, through the one text helper (D-170, D-559,
    /// D-712). The player then reads the name of the crash file and the address that takes
    /// it, and the session quits on the next press.
    /// </summary>
    /// <param name="path">The path of the crash file, or null when the write failed.</param>
    /// <returns>True when the message is on screen, and false when the session must quit now.</returns>
    /// <remarks>
    /// A session with no display, such as the smoke job of CI, shows nothing and quits with
    /// the crash code (D-117). A crash before the UI base loaded does the same, because the
    /// message needs the string table and the theme (T-2).
    /// </remarks>
    private bool ShowCrashMessage(string? path)
    {
        if (path is null
            || this.ui is null
            || this.frame is null
            || this.content is null
            || string.CompareOrdinal(DisplayServer.GetName(), HeadlessDisplay) == 0)
        {
            return false;
        }

        try
        {
            var message = new CrashScreen();
            this.frame.Layer.AddChild(message);
            message.Build(this.ui, this.content.Strings, Path.GetFileName(path));
            return true;
        }
        catch (Exception second)
        {
            // The crash file is written, and the message alone failed. The session says so
            // and quits, and no second error hides the first one (T-2, G-18).
            GD.PrintErr($"the game could not show the message of the crash file '{path}': {second}");
            return false;
        }
    }

    /// <summary>
    /// The log line of a crash: the type of the error and the name of the crash file (D-179).
    /// </summary>
    /// <remarks>
    /// The line carries no message of the error, because a message of a file error holds a
    /// path. The crash file holds the message, with the folders of the person hidden (D-170).
    /// </remarks>
    private static LogEntry CrashEntry(Exception fault, string file, GameRun? stopped) =>
        new(
            LogLevel.Error,
            "the game stopped with an error, and it wrote a crash file",
            stopped?.Tick ?? 0,
            LogSubsystems.Game,
            [
                new LogField("type", fault.GetType().Name),
                new LogField("file", file),
            ]);

    /// <summary>
    /// Boots the engine, prints the state of the build, and quits with the success code.
    /// The smoke job of CI runs this session on each leg (D-117).
    /// </summary>
    private void RunSmokeSession()
    {
        GD.Print("smoke: the engine started.");
        GD.Print($"smoke: the renderer is {GetRendererName()}.");
        GD.Print($"smoke: the frame is {GetFrameSize()}.");
        GD.Print($"smoke: the save folder is {SaveFolder.OfThisSystem()}.");

        IReadOnlyList<ContentFile> files = EmbeddedContent.Read();
        ContentSet content = ContentSet.Load(files);
        GD.Print($"smoke: the content is {files.Count} files with the hash {content.Hash}.");

        GameRun session = GameRun.Start(content, FixtureSeed, DebugSeam.Handlers());
        GD.Print($"smoke: the run is {this.DescribeRun(session)}.");
        GD.Print($"smoke: the log is {this.DescribeLog()}.");
        GD.Print($"smoke: the crash file is {DescribeCrashFile(session)}.");
        GD.Print($"smoke: the UI base is {DescribeUiBase(content)}.");
        GD.Print($"smoke: the map is {DescribeMap(content, session)}.");
        GD.Print($"smoke: the console is {this.DescribeConsole(session)}.");
        GD.Print($"smoke: the battle is {this.DescribeBattle(content, session)}.");
        GD.Print("smoke: the session ends with no error.");
        GetTree().Quit(SuccessExitCode);
    }

    /// <summary>
    /// Fails when the user folder of Godot and the save folder of Storage differ (D-465, F-33).
    /// </summary>
    /// <remarks>
    /// Godot resolves `user://` from the custom user folder of the project, and Storage reads
    /// the environment of the system. Two rules give one folder, and a session that finds two
    /// folders would write a save that no later session reads. The smoke session runs this
    /// check on Windows, on Linux, and on macOS (D-117, D-481, T-2).
    /// </remarks>
    /// <exception cref="InvalidOperationException">The two folders differ (T-2).</exception>
    private static void CheckSaveFolder()
    {
        string engineFolder = NormalizeFolder(ProjectSettings.GlobalizePath("user://"));
        string storageFolder = NormalizeFolder(SaveFolder.OfThisSystem());

        // Windows reads a path without case, and the two rules read two sources of the same
        // folder there. macOS and Linux compare by character.
        StringComparison comparison = OperatingSystem.IsWindows()
            ? StringComparison.OrdinalIgnoreCase
            : StringComparison.Ordinal;

        if (!string.Equals(engineFolder, storageFolder, comparison))
        {
            throw new InvalidOperationException(
                $"Godot writes to '{engineFolder}', and the folder rule of Storage gives " +
                $"'{storageFolder}'. The custom user folder of the project and " +
                $"'{nameof(SaveFolder)}.{nameof(SaveFolder.Name)}' must name one folder (D-465, F-33, T-2).");
        }
    }

    /// <summary>
    /// Makes one spelling of a folder. Godot writes a slash on every system, also on Windows,
    /// and the path of `user://` can end with one.
    /// </summary>
    /// <param name="path">The path of a folder.</param>
    /// <returns>The path with slashes, and with no separator at the end.</returns>
    private static string NormalizeFolder(string path) => path.Replace('\\', '/').TrimEnd('/');

    /// <summary>
    /// Loads the content of this build from the resources of this assembly (D-508). The
    /// smoke session loads it the same way, and it prints the count of files and the hash,
    /// so the session proves the embed inside the engine, where the match test of Tests
    /// reads the assembly file alone (F-42).
    /// </summary>
    /// <returns>The content set, with its hash (D-648).</returns>
    private static ContentSet LoadContent() => ContentSet.Load(EmbeddedContent.Read());

    /// <summary>
    /// Steps a run through the fixed-step loop and writes its log entries, and gives the tick,
    /// the count of lines of the record, the count of log lines, and the state hash. The
    /// session thus reads the loop of D-164, the record of G-5, and the log file of D-179
    /// inside the engine.
    /// </summary>
    /// <param name="session">The run of the smoke session.</param>
    /// <returns>The tick, the counts, and the state hash, as one line.</returns>
    /// <remarks>
    /// The session opens and closes the menu, so the log file holds the info lines of a menu
    /// and each CI leg reads a written line (D-117, D-179).
    /// </remarks>
    private string DescribeRun(GameRun session)
    {
        int lines = 0;
        for (int frame = 0; frame < SmokeFrameCount; frame += 1)
        {
            if (frame == SmokeMenuOpenFrame)
            {
                session.Queue(Intent.OfPlayer(IntentIds.OpenMenu));
            }

            if (frame == SmokeMenuCloseFrame)
            {
                session.Queue(Intent.OfPlayer(IntentIds.CloseMenu));
            }

            lines += this.WriteLog(session.Advance(SmokeFrameSeconds));
        }

        return $"tick {session.Tick} with {session.RecordedLines} recorded lines, "
            + $"{lines} log lines, and the state hash 0x{session.StateHash():x16}";
    }

    /// <summary>Reads the log file of the session again, and gives its path and its count of lines.</summary>
    /// <returns>The path and the count of lines, as one line.</returns>
    /// <exception cref="InvalidOperationException">No log file is open (T-2).</exception>
    private string DescribeLog()
    {
        LogStore store = this.log ?? throw new InvalidOperationException(
            "The smoke session reads the log file, and it opened none (T-2).");

        return $"{store.SessionFile} with {store.Read().Count} lines, "
            + $"and the count of files in the folder is {store.Names().Count}";
    }

    /// <summary>
    /// Writes one crash file and reads it again, so each CI leg proves the folder, the write,
    /// and the reader of a crash file (D-117, D-170).
    /// </summary>
    /// <param name="session">The run of the smoke session, whose record the file carries.</param>
    /// <returns>The name of the file and the end tick of its record, as one line.</returns>
    /// <exception cref="InvalidOperationException">The file that the check wrote holds no record (T-2).</exception>
    /// <remarks>
    /// The check writes into a folder of its own inside the crash folder. A write into the
    /// crash folder itself would remove a crash file of a real session, because the folder
    /// keeps the newest files alone (D-659).
    /// </remarks>
    private static string DescribeCrashFile(GameRun session)
    {
        CrashStore crashes = new(Path.Combine(CrashStore.OfThisSystem().Folder, SmokeFolderName));
        Exception fault = new InvalidOperationException(
            "the smoke session made this error, and the file is a check of the crash path (D-117)");

        string path = crashes.Write(fault, session.Record(), DateTime.UtcNow);
        CrashReport report = crashes.Read(path);
        if (report.Record is null)
        {
            throw new InvalidOperationException(
                $"The crash file '{path}' holds no record, and the check wrote one (T-2).");
        }

        return $"{Path.GetFileName(path)} with a record that ends at tick {report.Record.EndTick}, "
            + $"and the count of files in the folder is {crashes.Names().Count}";
    }

    /// <summary>
    /// Builds the UI base at each body size and reads every font setting back, so each CI
    /// leg proves the settings of D-710 inside the engine (D-117, F-49).
    /// </summary>
    /// <param name="loaded">The content set of this build.</param>
    /// <returns>The body sizes, the title sizes, and the strike of each font, as one line.</returns>
    /// <exception cref="InvalidOperationException">A font setting is not the one of D-710 (T-2).</exception>
    /// <remarks>
    /// `--headless` draws nothing, so the session reads the settings and never a pixel. The
    /// screen-test job of PR-41 captures each screen (F-23, G-16).
    /// </remarks>
    private static string DescribeUiBase(ContentSet loaded)
    {
        UiStyle style = loaded.Style;
        var built = new List<string>();

        foreach (int body in new[] { style.SmallBody, style.LargeBody })
        {
            UiBase ui = UiBase.Load(loaded, body);
            int title = style.TitleSizeOf(body);

            CheckFontSettings(ui.Theme.Theme.DefaultFont, $"the body of {body}");
            CheckFontSettings(ui.Theme.Theme.GetFont("font", UiTheme.TitleVariation), $"the title of {title}");
            built.Add($"body {body} with title {title}");
        }

        return $"{string.Join(", ", built)}, and the six font settings of D-710 read back";
    }

    /// <summary>
    /// Builds the tiles of the map and reads the place of the view back (D-667, D-717). A
    /// headless session draws nothing, so this check reads the nodes and never the pixels
    /// (F-23). PR-41 builds the screen test that reads the pixels.
    /// </summary>
    /// <param name="loaded">The content set of this build.</param>
    /// <param name="session">The run of the smoke session, which stands on the first map.</param>
    /// <returns>The size of the map, the count of tiles, and the place of the view.</returns>
    private static string DescribeMap(ContentSet loaded, GameRun session)
    {
        GameMap map = session.Party.Map;
        UiBase ui = UiBase.Load(loaded, loaded.Style.SmallBody);
        var drawn = new MapScreen();
        drawn.Build(GameAtlas.Load(loaded.Atlas), ui.Theme, session.Party);
        drawn.ShowParty(session.Party);

        CameraPlace view = MapCamera.Of(session.Party, FrameRoot.WorldWidth, FrameRoot.WorldHeight);
        string ground = drawn.DescribeGround();
        string sprites = drawn.DescribeSprites(session.Party);
        drawn.QueueFree();
        return $"'{map.Id.Value}' at {map.Width} by {map.Height} tiles, "
            + $"the party at {session.Party.LeadAt}, the view at ({view.X}, {view.Y}), "
            + $"{sprites}, and {ground}";
    }

    /// <summary>
    /// Builds the debug console and runs every command of it, inside the engine (D-171,
    /// D-724). A release export holds no debug assembly, so the line of this session says so,
    /// and the export leg of CI reads that line (D-260, D-492, D-726).
    /// </summary>
    /// <param name="session">The run of the smoke session.</param>
    /// <returns>The count of commands, the count of answer lines, and the effect of `reveal`.</returns>
    /// <exception cref="InvalidOperationException">
    /// A development build built no console, or a command changed no state (T-2).
    /// </exception>
    /// <remarks>
    /// The console of this session goes free at once, because a headless session draws no
    /// pixel (F-23). The commands run through the same seam that the console uses, so each
    /// leg of CI reads the load of the assembly, the build of the nodes, and every command
    /// (D-117).
    /// </remarks>
    private string DescribeConsole(GameRun session)
    {
        if (!DebugSeam.IsDevelopmentBuild)
        {
            return $"absent, because this build has no feature '{DebugSeam.DevelopmentFeature}' (D-260, D-492)";
        }

        int shown = this.TypeInConsole(session);

        int answers = 0;
        IReadOnlyList<string> names = DebugSeam.CommandNames();
        foreach (string name in names)
        {
            answers += DebugSeam.Run(name, () => session.State, session.Queue).Count;
        }

        // The console sends an intent, and the rules apply it on the next tick. Thus the
        // count below reads the work of the handler of the seam, and never a change that the
        // console made itself (D-171, T-7).
        int walked = session.Party.Walked.Count;
        this.WriteLog(session.Advance(SmokeFrameSeconds));
        int marked = session.Party.Walked.Count - walked;
        if (marked <= 0)
        {
            throw new InvalidOperationException(
                $"The commands of the console marked {marked} more tiles, and the map "
                + $"holds {session.Party.Map.Width * session.Party.Map.Height} tiles (D-724, T-2).");
        }

        return $"{names.Count} commands with {answers} answer lines, {shown} lines on the screen "
            + $"after a typed line, "
            + $"and they marked {marked} more tiles as walked";
    }

    /// <summary>
    /// Builds the frame and the console as a play session does, and types a line in the
    /// console through key events of the root viewport, as the person does (D-725, D-813).
    /// </summary>
    /// <param name="session">The run of the smoke session.</param>
    /// <returns>The count of lines that the console shows after the typed line.</returns>
    /// <exception cref="InvalidOperationException">
    /// The build built no console, the console key opened nothing, the typed line reached no
    /// entry, or the quit key left the console open (T-2).
    /// </exception>
    /// <remarks>
    /// The console draws in the frame viewport, which gets no event from the engine. A check
    /// that set the text of the entry, or that put the console outside the frame, passed while
    /// no key of the person reached the entry. Thus this check sends every key through
    /// <see cref="_Input"/>, on the path of a real key (T-3).
    /// </remarks>
    private int TypeInConsole(GameRun session)
    {
        // A key that the console does not take goes on to the input map, as in a play session.
        GameInputMap.Build();
        var built = new FrameRoot();
        this.AddChild(built);
        this.frame = built;
        this.BuildConsole(built, session);
        Control made = this.console ?? throw new InvalidOperationException(
            "This build has the feature of a development build, and it built no debug console (D-723, T-2).");

        this.PushKey((Key)KeyRoutes.ConsoleKey, 0);
        if (!made.Visible)
        {
            throw new InvalidOperationException(
                "The smoke session pressed the console key, and the console stayed closed (D-725, T-2).");
        }

        int before = DebugSeam.ShownLines(made).Count;
        foreach (char typed in SmokeConsoleLine)
        {
            this.PushKey((Key)char.ToUpperInvariant(typed), typed);
        }

        this.PushKey(Key.Enter, 0);
        int shown = DebugSeam.ShownLines(made).Count;
        if (shown <= before)
        {
            throw new InvalidOperationException(
                $"The smoke session typed '{SmokeConsoleLine}' and Enter, and the console still "
                + $"shows {shown} lines. No key reached the entry of the console (D-725, T-2).");
        }

        this.PushKey((Key)KeyRoutes.QuitKey, 0);
        if (made.Visible)
        {
            throw new InvalidOperationException(
                "The smoke session pressed the quit key, and the console stayed open (D-813, T-2).");
        }

        this.console = null;
        this.frame = null;
        this.RemoveChild(built);
        built.QueueFree();
        return shown;
    }

    /// <summary>Pushes the press and the release of one key into the root viewport.</summary>
    /// <param name="key">The key.</param>
    /// <param name="unicode">The character that the key types, or 0 for a key that types none.</param>
    private void PushKey(Key key, long unicode)
    {
        GetViewport().PushInput(new InputEventKey { Keycode = key, Unicode = unicode, Pressed = true });
        GetViewport().PushInput(new InputEventKey { Keycode = key, Unicode = unicode, Pressed = false });
    }

    /// <summary>
    /// Walks the party into the hall patrol of the first map and fights the battle to its end,
    /// inside the engine (D-767). A win or a flee then waits for the event queue to drain and
    /// for the wait intent, and a wipe reloads the run (D-522, D-532, D-776).
    /// </summary>
    /// <param name="loaded">The content set of this build.</param>
    /// <param name="session">The run of the smoke session.</param>
    /// <returns>The frames, the outcome, and the tick after the battle, as one line.</returns>
    /// <exception cref="InvalidOperationException">No battle starts, or no battle ends, inside the frame limit (T-2).</exception>
    /// <remarks>
    /// The walk goes east along the hall, south at its east end, and west along the night route
    /// of the patrol, so the party meets it by sight or by a step into it. The fight sends the
    /// attack of a player at the first enemy that melee reaches, on each turn that the input
    /// gate of D-532 opens.
    /// </remarks>
    private string DescribeBattle(ContentSet loaded, GameRun session)
    {
        GameRun open = session;
        string outcome = "none";
        int frame = 0;
        for (; frame < SmokeBattleFrames && !open.InBattle; frame += 1)
        {
            open.Queue(Intent.OfPlayer(SmokeStepOf(open.Party)));
            this.WriteLog(open.Advance(SmokeFrameSeconds));
        }

        for (; frame < SmokeBattleFrames && open.InBattle; frame += 1)
        {
            if (open.State.Battle is Battle battle && battle.Outcome != BattleOutcome.Running)
            {
                outcome = Battle.OutcomeName(battle.Outcome);
            }

            if (open.WipeReady)
            {
                open = ReloadRun(loaded);
                break;
            }

            if (open.TakesBattleCommand && open.State.Battle is Battle running)
            {
                BattleTarget target = running.MeleeTargets(BattleSide.Enemy)[0].Target;
                open.Queue(Intent.OfPlayer(IntentIds.BattleAttack, target, null));
            }

            this.WriteLog(open.Advance(SmokeFrameSeconds));
        }

        // A wipe reloads a run that stands on the map, and a win or a flee ends on the map
        // after the wait intent. A run still in the battle ran out of frames (T-2).
        if (string.CompareOrdinal(outcome, "none") == 0 || open.InBattle)
        {
            throw new InvalidOperationException(
                $"The smoke battle reached no end in {SmokeBattleFrames} frames: the outcome is '{outcome}', and the party is at {open.Party.LeadAt} (D-767, T-2).");
        }

        return $"'{outcome}' after {frame} frames, and the run is at tick {open.Tick} with the party at {open.Party.LeadAt}";
    }

    /// <summary>Gives the step of the smoke walk: east along the hall, then south, then west (D-767).</summary>
    private static ContentId SmokeStepOf(MapState party)
    {
        if (party.LeadAt.X < SmokeTurnColumn && party.LeadAt.Y < SmokePatrolRow)
        {
            return IntentIds.MoveEast;
        }

        return party.LeadAt.Y < SmokePatrolRow ? IntentIds.MoveSouth : IntentIds.MoveWest;
    }

    /// <summary>
    /// Reads every setting of one font back, and fails when one of them is not the setting
    /// of a pixel font (D-710, F-49, T-2).
    /// </summary>
    /// <param name="font">The font that the theme holds.</param>
    /// <param name="what">The place of the font, for the message of a failure.</param>
    /// <exception cref="InvalidOperationException">The font is absent, or a setting is wrong (T-2).</exception>
    private static void CheckFontSettings(Font? font, string what)
    {
        if (font is not FontFile file)
        {
            throw new InvalidOperationException(
                $"The theme holds no font file for {what}, and it holds '{font?.GetType().Name ?? "nothing"}' (T-2).");
        }

        Refuse(what, "antialiasing", file.Antialiasing, TextServer.FontAntialiasing.None);
        Refuse(what, "hinting", file.Hinting, TextServer.Hinting.None);
        Refuse(what, "subpixel positioning", file.SubpixelPositioning, TextServer.SubpixelPositioning.Disabled);
        Refuse(what, "the distance field", file.MultichannelSignedDistanceField, false);
        Refuse(what, "the mipmaps", file.GenerateMipmaps, false);
        Refuse(what, "the system fallback", file.AllowSystemFallback, false);

        if (file.FixedSize <= 0 || file.FixedSizeScaleMode != TextServer.FixedSizeScaleMode.IntegerOnly)
        {
            throw new InvalidOperationException(
                $"The font of {what} pins the size {file.FixedSize} with the scale mode "
                + $"{file.FixedSizeScaleMode}. A pixel font pins one bitmap strike and scales it by a "
                + "whole number, or a glyph draws from the traced outline (T-2, D-710, F-49).");
        }
    }

    /// <summary>Fails when one setting of a font is not the value that D-710 gives.</summary>
    /// <typeparam name="T">The type of the setting.</typeparam>
    /// <param name="what">The place of the font, for the message.</param>
    /// <param name="name">The name of the setting.</param>
    /// <param name="read">The value that the font holds.</param>
    /// <param name="wanted">The value of D-710.</param>
    /// <exception cref="InvalidOperationException">The two values differ (T-2).</exception>
    private static void Refuse<T>(string what, string name, T read, T wanted)
    {
        if (!EqualityComparer<T>.Default.Equals(read, wanted))
        {
            throw new InvalidOperationException(
                $"The font of {what} holds {name} as '{read}', and D-710 gives '{wanted}' (T-2, F-49).");
        }
    }

    /// <summary>Reads the renderer of this session from the project settings (D-599).</summary>
    /// <returns>The name of the renderer, for example "forward_plus".</returns>
    private static string GetRendererName()
    {
        Variant setting = ProjectSettings.GetSetting("rendering/renderer/rendering_method");
        string name = setting.AsString();
        if (string.IsNullOrEmpty(name))
        {
            throw new InvalidOperationException(
                "The project setting 'rendering/renderer/rendering_method' is absent (T-2).");
        }

        return name;
    }

    /// <summary>Reads the frame of the project settings, which is 1280 by 720 (D-568).</summary>
    /// <returns>The width and the height of the frame, as "1280 by 720".</returns>
    private static string GetFrameSize()
    {
        int width = ProjectSettings.GetSetting("display/window/size/viewport_width").AsInt32();
        int height = ProjectSettings.GetSetting("display/window/size/viewport_height").AsInt32();
        if (width <= 0 || height <= 0)
        {
            throw new InvalidOperationException(
                $"The frame of the project settings is {width} by {height} (T-2).");
        }

        return $"{width} by {height}";
    }
}
