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

    /// <summary>The console command that switches the carried light, which the smoke session runs again (D-851).</summary>
    private const string TorchCommand = "torch";

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
    private BattleScreen? battle;
    private SettingsScreen? settingsScreen;
    private SettingsStore? settingsStore;
    private GameSettings? settings;
    private CommandMemory? memory;
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
            this.WriteLog(this.run.Advance(delta, this.HeldStepIntent));
            if (this.run.WipeReady)
            {
                this.ReloadAfterWipe();
            }

            this.FollowBattleScreen();
            if (this.battle is null)
            {
                this.map?.ShowParty(this.run.Party, this.run.TickPart);
                this.map?.ShowWeather(this.run.Tick, seek: false);
            }

            this.ShowHandOff(this.run);
        }
        catch (Exception fault)
        {
            this.ReportCrash(fault);
        }
    }

    /// <summary>
    /// Gives the step intent of the direction that the player holds now, for one tick (D-493,
    /// D-716, D-820). The party walks while a key or a button is down, so the run asks for one
    /// intent before each tick of a frame.
    /// </summary>
    /// <returns>The step intent, or null when no direction is down or the tick takes no step.</returns>
    /// <remarks>
    /// The held set comes from the press events and the release events, and never from a
    /// poll of the input singleton (F-50). A menu pauses the world and takes every input of
    /// the player, so no step intent goes out for a tick that a menu pauses (D-162, T-2).
    /// </remarks>
    private Intent? HeldStepIntent()
    {
        GameRun? open = this.run;

        // The queue can already hold the intent that opens the menu, because the host reads
        // input before it runs the ticks of a frame. A step intent for that tick would meet
        // the refusal of the rules (D-162, T-2). An encounter holds the map still, so a step
        // intent then moves nothing, and the record stays free of it (D-531).
        if (open is null || open.MenuOpenNextTick || open.InBattle)
        {
            return null;
        }

        string? action = this.held.Newest;
        return action is null ? null : open.IntentOf(action);
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
        SettingsStore store = SettingsStore.OfThisSystem();
        this.settingsStore = store;
        GameSettings chosen = this.LoadSettings(store);
        this.settings = chosen;
        this.memory = new CommandMemory(chosen.Battle.RememberCursor);
        this.run = GameRun.Start(loaded, FixtureSeed, DebugSeam.Handlers(), chosen.Battle.Messages);
        GameInputMap.Build(chosen.Controls);
        ApplyWindow(chosen.Display.Window);
        this.BuildScreen(loaded);
        this.ReportWindowMode(chosen.Display.Window);
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
    /// The fit and the body size come from the display settings. Auto takes the body size of
    /// the fit of the frame on this screen (D-707, D-874).
    /// </para>
    /// </remarks>
    private void BuildScreen(ContentSet loaded)
    {
        var built = new FrameRoot();
        this.AddChild(built);
        this.frame = built;

        GameSettings chosen = this.settings ?? throw new InvalidOperationException(
            "The screen built before the session read its settings (T-2).");
        built.SetMode(FitModeOf(chosen.Display.Fit));
        int body = BodyOf(chosen.Display.Body, built.Fit.Height, loaded.Style);
        UiBase built_ui = UiBase.Load(loaded, body);
        this.ui = built_ui;
        this.builtBody = body;

        GameRun open = this.run ?? throw new InvalidOperationException(
            $"The screen built before the run started (T-2).");

        // The capture session of the screen-test job builds the same map (D-172, D-734).
        this.map = MapFixture.Build(built, built_ui, open.Party, loaded);

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

        GameSettings chosen = this.settings ?? throw new InvalidOperationException(
            "A wipe reloads the run, and the session read no settings (T-2).");
        GameRun reloaded = ReloadRun(loaded, chosen.Battle.Messages);
        this.run = reloaded;
        this.RebuildScreen(loaded);
        this.WriteLog([new LogEntry(
            LogLevel.Info,
            "the party wiped and the run reloaded",
            reloaded.Tick,
            LogSubsystems.Game,
            [LogField.OfNumber("tick", reloaded.Tick)])]);
    }

    /// <summary>
    /// Builds the battle screen when a fight starts, draws it on each frame, and removes it
    /// when the fight ends, so the map shows again (D-111, D-532).
    /// </summary>
    /// <remarks>
    /// The screen follows the run: the fight shows from the end of the transition into it to the
    /// start of the fade back to the map. An encounter before the fight keeps the map on screen,
    /// and the transition plays over it (D-531, D-938, D-939).
    /// </remarks>
    /// <exception cref="InvalidOperationException">A fight runs, and the session built no frame or UI base (T-2).</exception>
    private void FollowBattleScreen()
    {
        GameRun? open = this.run;
        if (open is null)
        {
            return;
        }

        if (!open.ShowsBattle)
        {
            if (this.battle is not null)
            {
                this.battle.Free();
                this.battle = null;
                this.map?.Show();
            }

            return;
        }

        if (this.battle is null)
        {
            FrameRoot built = this.frame ?? throw new InvalidOperationException(
                $"A fight started at tick {open.Tick}, and the session built no frame (T-2).");
            UiBase shown = this.ui ?? throw new InvalidOperationException(
                $"A fight started at tick {open.Tick}, and the session built no UI base (T-2).");
            ContentSet loaded = this.content ?? throw new InvalidOperationException(
                $"A fight started at tick {open.Tick}, and the session loaded no content (T-2).");

            CommandMemory remembered = this.memory ?? throw new InvalidOperationException(
                $"A fight started at tick {open.Tick}, and the session made no command memory (T-2).");
            GameSettings chosen = this.settings ?? throw new InvalidOperationException(
                $"A fight started at tick {open.Tick}, and the session read no settings (T-2).");

            this.map?.Hide();
            this.battle = BattleScreen.Build(built, shown, loaded, open, remembered, chosen.Access.Effects);
            return;
        }

        this.battle.Show(open);
    }

    /// <summary>Draws the phase of the hand-off of the run over the whole frame (D-938, D-939).</summary>
    /// <param name="open">The run.</param>
    /// <exception cref="InvalidOperationException">The session built no frame, loaded no content, or read no settings (T-2).</exception>
    private void ShowHandOff(GameRun open)
    {
        FrameRoot built = this.frame ?? throw new InvalidOperationException(
            $"The hand-off draws at tick {open.Tick}, and the session built no frame (T-2).");
        ContentSet loaded = this.content ?? throw new InvalidOperationException(
            $"The hand-off draws at tick {open.Tick}, and the session loaded no content (T-2).");
        GameSettings chosen = this.settings ?? throw new InvalidOperationException(
            $"The hand-off draws at tick {open.Tick}, and the session read no settings (T-2).");

        built.HandOffPass.Show(open.HandOff, open.Transitions, open.Tick, chosen.Access.Effects, loaded.Palette);
    }

    /// <summary>Removes the frame and its nodes, and builds the screen again over the current run.</summary>
    /// <param name="loaded">The content set of this build.</param>
    private void RebuildScreen(ContentSet loaded)
    {
        this.frame?.QueueFree();
        this.frame = null;
        this.map = null;
        this.battle = null;
        this.console = null;
        this.BuildScreen(loaded);
    }

    /// <summary>
    /// Builds the screen again when a new window size gives another default body size, while the
    /// body size setting is auto (D-707, D-874).
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
            if (loaded is null || built is null || this.settings?.Display.Body != BodySetting.Auto)
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
    /// Puts the window of the play session in the mode of the display settings: borderless
    /// fullscreen, which is a window with no border that covers the screen, or a window. The
    /// game has no exclusive mode (D-865).
    /// </summary>
    /// <param name="window">The window mode of the settings.</param>
    /// <remarks>
    /// The play session sets the mode, and the project keeps the windowed default. The capture
    /// session sets the exact window size of each capture, and Godot ignores its `--windowed`
    /// option when the project asks for fullscreen. The smoke session has no window.
    /// <para>
    /// On macOS the switch ends a few frames later. The frame refits on each size change, and
    /// <see cref="OnWindowSizeChanged"/> builds the screen again when the body size moves.
    /// </para>
    /// </remarks>
    private static void ApplyWindow(WindowSetting window)
    {
        DisplayServer.WindowSetMode(WindowModeOf(window));
    }

    /// <summary>Gives the Godot window mode of one window setting (D-865).</summary>
    /// <param name="window">The window mode of the settings.</param>
    /// <returns>The borderless fullscreen mode of Godot, or the windowed mode.</returns>
    /// <exception cref="ArgumentOutOfRangeException">The setting has no mode (T-2).</exception>
    private static DisplayServer.WindowMode WindowModeOf(WindowSetting window) => window switch
    {
        WindowSetting.Borderless => DisplayServer.WindowMode.Fullscreen,
        WindowSetting.Window => DisplayServer.WindowMode.Windowed,
        _ => throw new ArgumentOutOfRangeException(nameof(window), window, "The window setting has no mode (D-865, T-2)."),
    };

    /// <summary>Gives the fit of the frame of one fit setting (D-232).</summary>
    /// <param name="fit">The fit of the settings.</param>
    /// <returns>The fit mode of the frame.</returns>
    /// <exception cref="ArgumentOutOfRangeException">The setting has no fit (T-2).</exception>
    private static FitMode FitModeOf(FitSetting fit) => fit switch
    {
        FitSetting.Fill => FitMode.Fill,
        FitSetting.WholePixels => FitMode.WholePixels,
        _ => throw new ArgumentOutOfRangeException(nameof(fit), fit, "The fit setting has no mode (D-232, T-2)."),
    };

    /// <summary>Gives the body size of one body setting on one fit (D-707, D-874).</summary>
    /// <param name="body">The body size setting.</param>
    /// <param name="drawnHeight">The height of the drawn frame, which auto reads.</param>
    /// <param name="style">The UI style file, which holds the two sizes.</param>
    /// <returns>The body size in frame pixels.</returns>
    /// <exception cref="ArgumentOutOfRangeException">The setting has no size (T-2).</exception>
    private static int BodyOf(BodySetting body, int drawnHeight, UiStyle style) => body switch
    {
        BodySetting.Auto => BodySize.DefaultFor(drawnHeight, style.SmallBody, style.LargeBody),
        BodySetting.Small => style.SmallBody,
        BodySetting.Large => style.LargeBody,
        _ => throw new ArgumentOutOfRangeException(nameof(body), body, "The body size setting has no size (D-874, T-2)."),
    };

    /// <summary>
    /// Writes the window mode and the window size of the start to the log. A mode other than
    /// the mode of the settings is an error line (T-2, D-865).
    /// </summary>
    /// <param name="window">The window mode of the settings.</param>
    private void ReportWindowMode(WindowSetting window)
    {
        DisplayServer.WindowMode mode = DisplayServer.WindowGetMode();
        Vector2I screen = this.GetWindow().Size;
        bool asked = mode == WindowModeOf(window);
        this.WriteLog([new LogEntry(
            asked ? LogLevel.Info : LogLevel.Error,
            asked
                ? "the window opened in the mode of the settings"
                : "the window opened in another mode than the mode of the settings",
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
    /// <param name="messageSpeed">The message speed of the settings (D-866).</param>
    /// <returns>The run.</returns>
    private static GameRun ReloadRun(ContentSet loaded, MessageSpeed messageSpeed)
    {
        SaveStore saves = SaveStore.OfThisSystem();
        SaveDocument? slot = saves.Exists(SaveKind.Slot) ? saves.Read(SaveKind.Slot) : null;
        SaveDocument? autosave = saves.Exists(SaveKind.Autosave) ? saves.Read(SaveKind.Autosave) : null;
        return GameRun.Reload(loaded, slot, autosave, FixtureSeed, DebugSeam.Handlers(), messageSpeed);
    }

    /// <summary>
    /// Turns the carried light of the map on or off, for the `torch` command of the console
    /// (D-847, D-851). No rule reads the light, so no intent and no record exist.
    /// </summary>
    /// <returns>True when the carried light is on now.</returns>
    /// <exception cref="InvalidOperationException">The session built no map (T-2).</exception>
    private bool SwitchCarriedLight()
    {
        MapScreen shown = this.map ?? throw new InvalidOperationException(
            "The console switched the carried light, and this session built no map (T-2, D-851).");
        shown.CarriedLightOn = !shown.CarriedLightOn;
        return shown.CarriedLightOn;
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
        if (!DebugSeam.TryBuildConsole(() => open.State, open.Queue, this.SwitchCarriedLight, out Control? made) || made is null)
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

        // The capture session holds no run and builds no input map. A read of an action that
        // the map lacks writes an error line, and the capture fails on it, so the check of
        // the run comes first (T-2).
        GameRun? run = this.run;
        if (run is null)
        {
            return;
        }

        // The settings screen takes every event while it is open, the mouse included, and the
        // world stays paused under it (D-162, D-871, D-872).
        if (this.settingsScreen is SettingsScreen open)
        {
            this.ReadSettings(run, open, signal);
            return;
        }

        // The party walks while a direction is down, so the map needs the press and the
        // release of each step action, and never a poll (D-716, F-50).
        this.held.Read(signal);

        // While a character has the turn, the command menu takes every action. A move of its
        // cursor makes no intent, and a whole choice makes one (D-493, D-827).
        if (this.battle?.Commands is not null)
        {
            this.ReadBattleCommand(run, this.battle, signal);
            return;
        }

        // A press of confirm shows the next battle message. The skip changes no state of the run,
        // so the record holds only the command that the player makes after it (D-866).
        if (this.battle is not null && signal.IsActionPressed(InputActions.Confirm) && run.SkipPlayingEvent())
        {
            return;
        }

        foreach (string action in InputActions.Names)
        {
            // A step action moves the party while the player holds it, so `HeldStepIntent`
            // makes its intent on each tick and this loop skips it (D-716, F-50).
            if (InputActions.IsStep(action) || !signal.IsActionPressed(action))
            {
                continue;
            }

            // The run holds the menu state. Until PR-62, the menu action on the map opens the
            // settings screen, and the screen closes the menu (D-162, D-650, D-871). In a fight
            // the intent goes to the rules as before.
            Intent made = run.IntentOf(action);
            if (string.CompareOrdinal(action, InputActions.Menu) == 0 && !run.InBattle)
            {
                this.OpenSettings(run, made);
            }
            else if (string.CompareOrdinal(action, InputActions.Menu) == 0)
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

    /// <summary>Opens the menu of the run and the settings screen over it (D-162, D-871).</summary>
    /// <param name="run">The run, whose world pauses.</param>
    /// <param name="open">The intent that opens the menu.</param>
    /// <exception cref="InvalidOperationException">The session built no frame, no UI base, or no settings (T-2).</exception>
    private void OpenSettings(GameRun run, Intent open)
    {
        FrameRoot built = this.frame ?? throw new InvalidOperationException(
            $"The menu opened at tick {run.Tick}, and the session built no frame (T-2).");
        UiBase shown = this.ui ?? throw new InvalidOperationException(
            $"The menu opened at tick {run.Tick}, and the session built no UI base (T-2).");
        ContentSet loaded = this.content ?? throw new InvalidOperationException(
            $"The menu opened at tick {run.Tick}, and the session loaded no content (T-2).");
        GameSettings chosen = this.settings ?? throw new InvalidOperationException(
            $"The menu opened at tick {run.Tick}, and the session read no settings (T-2).");

        run.Queue(open);

        // The screen takes every input while it is open, so no release of a held direction
        // reaches the map. A party that kept the direction would walk on after the close (T-2).
        this.held.Clear();
        int autoBody = BodySize.DefaultFor(built.Fit.Height, loaded.Style.SmallBody, loaded.Style.LargeBody);
        this.settingsScreen = SettingsScreen.Build(built, shown, loaded.Strings, chosen, autoBody);
        this.WriteLog([new LogEntry(LogLevel.Info, "the settings screen opened", run.Tick, LogSubsystems.Game, [])]);
    }

    /// <summary>Gives one event to the settings screen, and closes it when the player leaves (D-862).</summary>
    /// <param name="run">The run, whose menu the close ends.</param>
    /// <param name="open">The settings screen.</param>
    /// <param name="signal">The event of this frame.</param>
    /// <exception cref="InvalidOperationException">The session built no frame (T-2).</exception>
    private void ReadSettings(GameRun run, SettingsScreen open, InputEvent signal)
    {
        FrameRoot built = this.frame ?? throw new InvalidOperationException(
            $"The settings screen read an event at tick {run.Tick}, and the session built no frame (T-2).");
        if (open.Read(signal, built.Fit, InputActions.Menu) == SettingsOutcome.Close)
        {
            this.CloseSettings(run, open);
        }
    }

    /// <summary>
    /// Closes the settings screen, writes and applies each change, and closes the menu of the
    /// run (D-860, D-871).
    /// </summary>
    /// <param name="run">The run, whose menu closes.</param>
    /// <param name="open">The settings screen, which holds no conflict (D-862).</param>
    /// <exception cref="InvalidOperationException">The session read no settings (T-2).</exception>
    /// <exception cref="StorageException">The system refused the write (T-2).</exception>
    private void CloseSettings(GameRun run, SettingsScreen open)
    {
        GameSettings before = this.settings ?? throw new InvalidOperationException(
            $"The settings screen closed at tick {run.Tick}, and the session read no settings (T-2).");
        SettingsStore store = this.settingsStore ?? throw new InvalidOperationException(
            $"The settings screen closed at tick {run.Tick}, and the session made no settings store (T-2).");
        GameSettings after = open.Menu.Settings;

        open.Free();
        this.settingsScreen = null;
        if (!after.Equals(before))
        {
            store.Write(after);
            this.settings = after;
            this.ApplySettings(run, before, after);
        }

        // The close intent goes by its own id. The open intent can still wait in the queue when
        // one frame opens and closes the screen, and the menu action would then open it twice.
        run.Queue(Intent.OfPlayer(IntentIds.CloseMenu));
        this.WriteLog([new LogEntry(
            LogLevel.Info,
            after.Equals(before) ? "the settings screen closed with no change" : "the settings screen closed and wrote the settings file",
            run.Tick,
            LogSubsystems.Game,
            [new LogField("path", store.Path)])]);
    }

    /// <summary>Applies each setting that the game reads now (D-226).</summary>
    /// <param name="run">The run, which reads the message speed.</param>
    /// <param name="before">The settings before the screen opened.</param>
    /// <param name="after">The settings of the screen.</param>
    /// <remarks>
    /// The audio of PR-69 and PR-70, the effects of PR-58 to PR-60, the vibration of D-434, and
    /// the type-out of PR-36 read their settings from <see cref="settings"/> when they land. A
    /// change of the fit or the body size builds the screen again (D-707).
    /// </remarks>
    private void ApplySettings(GameRun run, GameSettings before, GameSettings after)
    {
        GameInputMap.Build(after.Controls);
        run.MessageSpeed = after.Battle.Messages;
        CommandMemory remembered = this.memory ?? throw new InvalidOperationException(
            $"The settings applied at tick {run.Tick}, and the session made no command memory (T-2).");
        remembered.Enabled = after.Battle.RememberCursor;
        if (this.battle is BattleScreen fight)
        {
            fight.Effects = after.Access.Effects;
        }

        if (after.Display.Window != before.Display.Window)
        {
            ApplyWindow(after.Display.Window);
        }

        if (after.Display.Fit != before.Display.Fit || after.Display.Body != before.Display.Body)
        {
            ContentSet loaded = this.content ?? throw new InvalidOperationException(
                $"The settings applied at tick {run.Tick}, and the session loaded no content (T-2).");
            this.RebuildScreen(loaded);
        }
    }

    /// <summary>Gives one pressed action to the command menu, and queues the intent of a whole choice (D-827).</summary>
    /// <param name="run">The run.</param>
    /// <param name="shown">The battle screen, whose menu is open.</param>
    /// <param name="signal">The event of this frame.</param>
    private void ReadBattleCommand(GameRun run, BattleScreen shown, InputEvent signal)
    {
        foreach (string action in InputActions.Names)
        {
            if (!signal.IsActionPressed(action))
            {
                continue;
            }

            if (shown.Read(action) is Intent made)
            {
                run.Queue(made);
                this.WriteLog([new LogEntry(
                    LogLevel.Debug,
                    "the player chose a battle command",
                    run.Tick,
                    LogSubsystems.Game,
                    [new LogField("intent", made.Action.Value)])]);
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

        GameRun session = GameRun.Start(content, FixtureSeed, DebugSeam.Handlers(), SmokeSettings().Battle.Messages);
        GD.Print($"smoke: the run is {this.DescribeRun(session)}.");
        GD.Print($"smoke: the log is {this.DescribeLog()}.");
        GD.Print($"smoke: the crash file is {DescribeCrashFile(session)}.");
        GD.Print($"smoke: the UI base is {DescribeUiBase(content)}.");
        GD.Print($"smoke: the map is {DescribeMap(content, session)}.");
        GD.Print($"smoke: the picture is {DescribePicture(content)}.");
        GD.Print($"smoke: the console is {this.DescribeConsole(session)}.");
        GD.Print($"smoke: the battle is {this.DescribeBattle(content, session)}.");
        GD.Print($"smoke: the settings screen is {this.DescribeSettings(content)}.");
        GD.Print("smoke: the session ends with no error.");
        GetTree().Quit(SuccessExitCode);
    }

    /// <summary>Gives the settings of the smoke session: the defaults, and never the file of the person (D-860).</summary>
    /// <returns>The default settings, with the default bindings of the game.</returns>
    /// <remarks>A CI machine holds no settings file, and a check never writes to the folder of the person.</remarks>
    private static GameSettings SmokeSettings() => GameSettings.Defaults(GameInputMap.DefaultBindings());

    /// <summary>
    /// Reads the settings file, or writes the defaults when no file exists, which is the first
    /// start (D-860, D-868).
    /// </summary>
    /// <param name="store">The store of the settings file of the person.</param>
    /// <returns>The settings of this session.</returns>
    /// <exception cref="StorageException">The file breaks a rule, or the system refused it (T-2).</exception>
    /// <remarks>
    /// A file that breaks a rule stops the start with the path and the field, and it never falls
    /// back to the defaults in silence, because the player would lose each choice (T-2, D-570).
    /// </remarks>
    private GameSettings LoadSettings(SettingsStore store)
    {
        if (store.Exists())
        {
            return store.Read();
        }

        GameSettings defaults = GameSettings.Defaults(GameInputMap.DefaultBindings());
        store.Write(defaults);
        this.WriteLog([new LogEntry(
            LogLevel.Info,
            "the session found no settings file and wrote the defaults",
            0,
            LogSubsystems.Game,
            [new LogField("path", store.Path)])]);
        return defaults;
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
    /// Builds the settings screen over a frame with the default settings, and walks the cursor
    /// over every row with the `ui_*` actions, as a player does (D-862, D-871). Each row reads
    /// its strings and its value, so a string id that the table lacks fails every CI leg (T-2).
    /// </summary>
    /// <param name="loaded">The content set of this build.</param>
    /// <returns>The count of rows, and the row of the cursor after one lap.</returns>
    /// <exception cref="InvalidOperationException">The lap ended on another row than the first (T-2).</exception>
    private string DescribeSettings(ContentSet loaded)
    {
        var built = new FrameRoot();
        this.AddChild(built);
        UiBase shownBase = UiBase.Load(loaded, loaded.Style.SmallBody);
        GameSettings defaults = SmokeSettings();
        SettingsScreen screen = SettingsScreen.Build(built, shownBase, loaded.Strings, defaults, loaded.Style.SmallBody);

        var down = new InputEventAction { Action = "ui_down", Pressed = true };
        int rows = SettingsMenu.Rows.Count;
        for (int step = 0; step < rows; step += 1)
        {
            if (screen.Read(down, built.Fit, InputActions.Menu) != SettingsOutcome.Stay)
            {
                throw new InvalidOperationException($"The settings screen closed on a move of its cursor at row {step} (T-2).");
            }
        }

        int cursor = screen.Menu.Cursor;
        screen.Free();
        this.RemoveChild(built);
        built.QueueFree();
        if (cursor != 0)
        {
            throw new InvalidOperationException(
                $"The cursor of the settings screen moved {rows} rows and stood on row {cursor}, and one lap ends on row 0 (T-2).");
        }

        return $"{rows} rows, and one lap of the cursor ends on the first row";
    }

    /// <summary>
    /// Builds the fixture large picture from the atlas, and checks that the view made one
    /// sprite for each copy (D-518, D-819). A headless session draws nothing, so the
    /// `picture` fixture of the screen-test job reads the pixels (F-23).
    /// </summary>
    /// <param name="loaded">The content set of this build.</param>
    /// <returns>The size of the picture, the count of copies, and the count of entries.</returns>
    /// <exception cref="InvalidOperationException">The view built another count of sprites (T-2).</exception>
    private static string DescribePicture(ContentSet loaded)
    {
        ContentId id = ContentId.Parse(ScreenCaptures.FixturePicture, LargePicture.Folder, "fixture");
        LargePicture picture = loaded.PictureOf(id);
        int expected = PictureCopies.Of(picture, loaded).Count;

        var view = new PictureView();
        view.Build(GameAtlas.Load(loaded.Atlas), picture, loaded);
        int built = view.CopyCount;
        view.QueueFree();

        if (built != expected)
        {
            throw new InvalidOperationException(
                $"The picture '{id.Value}' holds {expected} copies, and the view built {built} sprites (D-817, T-2).");
        }

        return $"'{id.Value}' at {picture.Width} by {picture.Height} art pixels, with {built} copies of "
            + $"{picture.Places.Count} entries";
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
        drawn.Build(GameAtlas.Load(loaded.Atlas), ui.Theme, session.Party, loaded, loaded.Effects.Ambient.WeatherOf(map.Id), loaded.Light.Passes);
        drawn.CarriedLightOn = true;
        drawn.ShowParty(session.Party, 0);
        drawn.ShowWeather(session.Tick, seek: false);

        CameraPlace view = MapCamera.Of(session.Party, FrameRoot.WorldWidth, FrameRoot.WorldHeight, 0);
        string ground = drawn.DescribeGround();
        string sprites = drawn.DescribeSprites(session.Party);
        string lights = drawn.DescribeLights();
        string weather = drawn.DescribeWeather();
        string lights2 = DescribeTorchLights(drawn);
        string room = DescribeAnotherRoom(loaded, drawn);
        drawn.QueueFree();
        return $"'{map.Id.Value}' at {map.Width} by {map.Height} tiles, "
            + $"the party at {session.Party.LeadAt}, the view at ({view.X}, {view.Y}), "
            + $"{sprites}, {lights}, {weather}, {lights2}, {room}, and {ground}";
    }

    /// <summary>
    /// Reads the light of each torch over 120 ticks, and fails on a light that moves (F-99). The
    /// flame of a torch jumps on each step of its fire, and the light of it never does (D-891).
    /// </summary>
    /// <param name="drawn">The map on screen, which this check steps and reads.</param>
    /// <returns>The ticks that it read, and the count of torches that held their place.</returns>
    /// <exception cref="InvalidOperationException">A light moved from its place (T-2, F-99).</exception>
    private static string DescribeTorchLights(MapScreen drawn)
    {
        const int Ticks = 120;
        for (int tick = 0; tick < Ticks; tick += 1)
        {
            drawn.ShowWeather(tick, seek: false);
            if (!drawn.TorchLightsHoldTheirPlaces)
            {
                throw new InvalidOperationException(
                    $"A torch of the map moved its light on tick {tick}, and a light that moves inside a doorway "
                    + "moves the shadow of the passage by a whole tile (T-2, D-891, F-99).");
            }
        }

        return $"each torch held its light over {Ticks} ticks";
    }

    /// <summary>
    /// Walks a run of its own into the room below, where the view leaves the first room, and
    /// reads the light and the streams of each torch back (F-97, F-98). A torch that stops
    /// there fails this session, because the owner saw each torch go out in a play session.
    /// </summary>
    /// <param name="loaded">The content set of this build.</param>
    /// <param name="drawn">The map on screen, which this check walks and reads.</param>
    /// <returns>The place of the party and the state of each torch in that room.</returns>
    /// <exception cref="InvalidOperationException">A step never ended, or a torch stopped (T-2).</exception>
    private static string DescribeAnotherRoom(ContentSet loaded, MapScreen drawn)
    {
        GameRun walked = GameRun.Start(loaded, FixtureSeed, DebugSeam.Handlers(), SmokeSettings().Battle.Messages);
        foreach (string action in ScreenCaptures.PitRoute)
        {
            walked.Queue(walked.IntentOf(action));
            for (int tick = 0; tick < ScreenCaptures.TicksOfOneStep; tick += 1)
            {
                walked.Advance(SmokeFrameSeconds);
                if (tick > 0 && walked.Party.Stepping is null)
                {
                    break;
                }
            }

            if (walked.Party.Stepping is not null)
            {
                throw new InvalidOperationException(
                    $"The step '{action}' of the walk to the room below never ended, and the lead stands at {walked.Party.LeadAt} (T-2).");
            }
        }

        // The checks of the weather and of each torch run inside this call (F-97, F-98).
        drawn.ShowParty(walked.Party, 0);
        drawn.ShowWeather(walked.Tick, seek: false);
        string weather = drawn.DescribeWeather();
        CameraPlace view = MapCamera.Of(walked.Party, FrameRoot.WorldWidth, FrameRoot.WorldHeight, 0);
        return $"in the room below the party stands at {walked.Party.LeadAt} with the view at ({view.X}, {view.Y}), and {weather}";
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

        // The `torch` command switches a light of the view alone, so this session holds the
        // switch in a local value and reads it back (D-851).
        bool carriedOn = false;
        bool SwitchLight()
        {
            carriedOn = !carriedOn;
            return carriedOn;
        }

        int answers = 0;
        IReadOnlyList<string> names = DebugSeam.CommandNames();
        foreach (string name in names)
        {
            answers += DebugSeam.Run(name, () => session.State, session.Queue, SwitchLight).Count;
        }

        string torch = CheckTorchCommand(session, SwitchLight, () => carriedOn);

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
            + $"they marked {marked} more tiles as walked, and {torch}";
    }

    /// <summary>
    /// Runs the `torch` command a second time, and fails when it sent an intent or left the
    /// carried light on (D-851). The loop of every command ran it once and turned the light on.
    /// </summary>
    /// <param name="session">The run of the smoke session.</param>
    /// <param name="switchLight">The switch that the commands of this session call.</param>
    /// <param name="isOn">Reads the switch back.</param>
    /// <returns>The line of the check, for the report of the smoke session.</returns>
    /// <exception cref="InvalidOperationException">The command sent an intent, or it did not switch the light (T-2).</exception>
    private static string CheckTorchCommand(GameRun session, Func<bool> switchLight, Func<bool> isOn)
    {
        if (!isOn())
        {
            throw new InvalidOperationException(
                $"The loop of every command ran '{TorchCommand}', and the carried light stayed off (D-851, T-2).");
        }

        int queued = 0;
        DebugSeam.Run(TorchCommand, () => session.State, _ => queued += 1, switchLight);
        if (isOn() || queued != 0)
        {
            throw new InvalidOperationException(
                $"The second '{TorchCommand}' left the carried light on {isOn()} and sent {queued} intents, "
                + "and it switches the light off with no intent (D-851, T-2).");
        }

        return $"'{TorchCommand}' switched the carried light on and off with no intent";
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
        GameInputMap.Build(SmokeSettings().Controls);
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
    /// Walks the party into the hall patrol of the first map and fights the battle to its end
    /// through the battle screen, inside the engine (D-767, D-827). A win or a flee then waits
    /// for the events to play and for the wait intent, and a wipe reloads the run (D-522,
    /// D-532, D-776).
    /// </summary>
    /// <param name="loaded">The content set of this build.</param>
    /// <param name="session">The run of the smoke session.</param>
    /// <returns>The frames, the outcome, the nodes of the screen, and the tick after the battle, as one line.</returns>
    /// <exception cref="InvalidOperationException">
    /// No battle starts, no battle ends inside the frame limit, or the menu sent no command (T-2).
    /// </exception>
    /// <remarks>
    /// The walk of <see cref="BattleWalk"/> meets the patrol. On each turn of a character the
    /// session presses confirm twice in the command menu: the attack, then the first target.
    /// Thus each leg of CI reads the build of the screen, the load of the flash shader, and
    /// the path of a real command (D-117). A headless session draws no pixel, so the `battle`
    /// fixture of the screen-test job reads the pixels (F-23).
    /// </remarks>
    private string DescribeBattle(ContentSet loaded, GameRun session)
    {
        GameRun open = session;
        string outcome = "none";
        int frame = 0;
        for (; frame < SmokeBattleFrames && !open.InBattle; frame += 1)
        {
            open.Queue(Intent.OfPlayer(BattleWalk.StepOf(open.Party)));
            this.WriteLog(open.Advance(SmokeFrameSeconds));
        }

        var built = new FrameRoot();
        this.AddChild(built);
        UiBase shownBase = UiBase.Load(loaded, loaded.Style.SmallBody);
        BattleScreen? screen = null;
        int commands = 0;
        string nodes = "no screen";
        for (; frame < SmokeBattleFrames && open.InBattle; frame += 1)
        {
            if (open.State.Battle is Battle battle && battle.Outcome != BattleOutcome.Running)
            {
                outcome = Battle.OutcomeName(battle.Outcome);
            }

            if (open.WipeReady)
            {
                open = ReloadRun(loaded, open.MessageSpeed);
                break;
            }

            if (open.BattleView is not null)
            {
                if (screen is null)
                {
                    screen = BattleScreen.Build(built, shownBase, loaded, open, new CommandMemory(SmokeSettings().Battle.RememberCursor), SmokeSettings().Access.Effects);
                    nodes = $"{screen.CombatantCount} combatants over {screen.BackdropCopies} backdrop copies in {screen.CheckLights()} key light, with {screen.CheckBursts()} particle nodes";
                }

                screen.Show(open);
            }

            if (screen?.Commands is not null)
            {
                commands += 1;
                open.Queue(PressConfirmTwice(screen, open.Tick));
            }

            this.WriteLog(open.Advance(SmokeFrameSeconds));
        }

        this.RemoveChild(built);
        built.QueueFree();

        // A wipe reloads a run that stands on the map, and a win or a flee ends on the map
        // after the wait intent. A run still in the battle ran out of frames (T-2).
        if (string.CompareOrdinal(outcome, "none") == 0 || open.InBattle || commands == 0)
        {
            throw new InvalidOperationException(
                $"The smoke battle reached no end in {SmokeBattleFrames} frames: the outcome is '{outcome}', the menu sent "
                + $"{commands} commands, and the party is at {open.Party.LeadAt} (D-767, D-827, T-2).");
        }

        return $"'{outcome}' after {frame} frames and {commands} commands of the menu, with {nodes}, "
            + $"and the run is at tick {open.Tick} with the party at {open.Party.LeadAt}";
    }

    /// <summary>Presses confirm twice in the command menu: the attack, then the first target (D-827).</summary>
    /// <param name="screen">The battle screen, whose menu is open.</param>
    /// <param name="tick">The tick of the run, for an error.</param>
    /// <returns>The intent of the attack.</returns>
    /// <exception cref="InvalidOperationException">The menu gave no intent (T-2).</exception>
    private static Intent PressConfirmTwice(BattleScreen screen, long tick)
    {
        Intent? first = screen.Read(InputActions.Confirm);
        Intent? second = first is null ? screen.Read(InputActions.Confirm) : null;
        return first ?? second ?? throw new InvalidOperationException(
            $"The smoke session pressed confirm twice at tick {tick}, and the command menu sent no intent (D-827, T-2).");
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
