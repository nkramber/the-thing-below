using System;
using System.Collections.Generic;
using System.Globalization;
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
/// A crash shows <see cref="CrashScreen"/> with the name of the crash file, its folder with no
/// account name (D-1102), and the address of D-473, through the one text helper of D-499
/// (D-559, D-712). The session then quits on the next input event. A session with no display
/// quits at once, so the smoke job of CI still ends with the crash code (D-117, T-2).
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

    /// <summary>The count of frames that the smoke fight holds its pause (D-1083).</summary>
    private const int SmokePauseFrames = 120;

    /// <summary>The line that the smoke session types in the console of a development build (D-724).</summary>
    private const string SmokeConsoleLine = "help";

    /// <summary>The name that Godot gives the display server of a session with no window.</summary>
    private const string HeadlessDisplay = "headless";

    /// <summary>The message of the log line of a crash (D-179), which the crash fixture reads back (P3-26).</summary>
    private const string CrashLogMessage = "the game stopped with an error, and it wrote a crash file";

    private LogStore? log;
    private GameRun? run;
    private ContentSet? content;
    private UiBase? ui;
    private int builtBody;
    private FrameRoot? frame;
    private MapScreen? map;
    private BattleScreen? battle;
    private MenuHost? menus;
    private NoticeBox? noticeBox;
    private PauseView? pause;
    private SettingsStore? settingsStore;
    private GameSettings? settings;
    private SettingsRefusal? refusedSettings;
    private SettingsNotice? settingsNotice;
    private CommandMemory? memory;
    private Control? console;
    private readonly PressGate gate = new();
    private readonly HeldSteps held;
    private readonly MousePointer pointer = new();
    private bool crashed;
    private bool focused = true;

    /// <summary>The crash of the crash fixture, from its plant to its check, or null in every other session (P3-26).</summary>
    private PlantedCrash? planted;

    /// <summary>Makes the node, whose held steps read the sources of the gate (D-1084).</summary>
    public Boot()
    {
        this.held = new HeldSteps(this.gate);
    }

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
            // A window with no focus holds the world, and it runs no tick until the focus comes
            // back (D-1084). The message of a refused settings file holds it too (D-1099).
            if (this.focused && this.settingsNotice is null)
            {
                this.WriteLog(this.run.Advance(delta, this.HeldStepIntent));
            }

            if (this.run.WipeReady)
            {
                this.ReloadAfterWipe();
            }

            this.FollowBattleScreen();
            if (this.battle is null)
            {
                this.map?.ShowParty(this.run.Party, this.run.DrawnTickPart, this.run.Tick, this.run.TorchHeld);
                this.map?.ShowWeather(this.run.Tick, seek: false);
            }

            this.ShowHandOff(this.run);
            this.pause?.Show(this.run.FightPaused);
            this.ShowMenuAndNotice(this.run);
        }
        catch (Exception fault)
        {
            this.ReportCrash(fault);
        }
    }

    /// <summary>
    /// Draws each open window of the menu from the state of the run, and the notice box at the
    /// tick of the world. The notice box hides under a menu, and it waits with the world (D-221, D-995).
    /// </summary>
    /// <param name="open">The run.</param>
    /// <exception cref="InvalidOperationException">The session read no settings (T-2).</exception>
    private void ShowMenuAndNotice(GameRun open)
    {
        GameSettings chosen = this.settings ?? throw new InvalidOperationException(
            $"The notice box draws at tick {open.Tick}, and the session read no settings (T-2).");

        this.menus?.Show();
        this.noticeBox?.Show(open.NoticeAt(TextSpeeds.CharactersPerSecond(chosen.Access.Text)), open.MenuOpen);
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
        // intent then moves nothing, and the record stays free of it (D-531). A story scene
        // refuses a step intent, and it moves the lead itself (D-1009).
        if (open is null || open.MenuOpenNextTick || open.InBattle || open.StoryRunning)
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
        if (this.log.CleanupFault is StorageException logCleanup)
        {
            this.WriteLog([CleanupEntry(logCleanup, 0)]);
        }

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
                this, LoadContent(), captureFolder, CapturesOf(userArguments), this.ReportCrash, this.PlantCrash);
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
        this.ShowSettingsNotice();
        this.ReportWindowMode(chosen.Display.Window);
        this.GetWindow().SizeChanged += this.OnWindowSizeChanged;
        Input.Singleton.JoyConnectionChanged += this.OnPadConnectionChanged;
        foreach (int device in Input.GetConnectedJoypads())
        {
            this.OnPadConnectionChanged(device, true);
        }
    }

    /// <summary>
    /// Logs each pad that connects or disconnects, and forgets the held buttons of a pad that
    /// disconnects (D-1077).
    /// </summary>
    /// <param name="device">The device number of the pad.</param>
    /// <param name="connected">True when the pad connected.</param>
    /// <remarks>
    /// A system can show one pad as two devices, such as the Steam Deck with Steam Input. The
    /// log names each device, so a report of a pad fault shows the devices of that system (T-2).
    /// </remarks>
    private void OnPadConnectionChanged(long device, bool connected)
    {
        try
        {
            int pad = (int)device;
            int forgot = connected ? 0 : this.gate.ForgetPad(pad);
            string name = Input.GetJoyName(pad);
            this.WriteLog([new LogEntry(
                LogLevel.Info,
                connected ? "a pad connected" : "a pad disconnected",
                this.run?.Tick ?? 0,
                LogSubsystems.Game,
                [
                    LogField.OfNumber("device", pad),
                    new LogField("name", name.Length > 0 ? name : "none"),
                    new LogField("known", Input.IsJoyKnown(pad) ? "yes" : "no"),
                    LogField.OfNumber("forgot", forgot),
                ])]);
        }
        catch (Exception fault)
        {
            this.ReportCrash(fault);
        }
    }

    /// <summary>
    /// Holds the world and forgets every held input when the window loses the focus, and runs the
    /// world again when the focus comes back, with no press (D-1077, D-1084).
    /// </summary>
    /// <param name="what">The notification of the engine.</param>
    /// <remarks>
    /// The system sends no release to a window that lost the focus, so a held source would stop
    /// each later press of its action, and a held step walked the party on with nobody at the
    /// controls (T-2). The engine sends the notification of the application and of the window,
    /// so the second one of a pair changes nothing.
    /// </remarks>
    public override void _Notification(int what)
    {
        if (what == NotificationApplicationFocusOut || what == NotificationWMWindowFocusOut)
        {
            this.FollowFocus(focused: false);
        }
        else if (what == NotificationApplicationFocusIn || what == NotificationWMWindowFocusIn)
        {
            this.FollowFocus(focused: true);
        }
    }

    /// <summary>Records a change of the focus of the window, and logs it (D-1084).</summary>
    /// <param name="focused">True when the window has the focus now.</param>
    private void FollowFocus(bool focused)
    {
        if (focused == this.focused || this.crashed)
        {
            return;
        }

        this.focused = focused;
        this.gate.Clear();
        this.held.Clear();

        // The engine sends the focus of a new window before the node enters the tree, and the
        // log file opens in `_Ready`. That first change holds no run, so it takes no log line.
        if (this.log is null)
        {
            return;
        }

        try
        {
            this.WriteLog([new LogEntry(
                LogLevel.Info,
                focused ? "the window got the focus back, and the world runs again" : "the window lost the focus, and the world holds",
                this.run?.Tick ?? 0,
                LogSubsystems.Game,
                [])]);
        }
        catch (Exception fault)
        {
            this.ReportCrash(fault);
        }
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
        FrameRoot built = FrameRoot.AddTo(this);
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
        this.map = MapFixture.Build(built, built_ui, open, loaded);
        this.noticeBox = new NoticeBox(built, built_ui);
        this.pause = new PauseView(built, built_ui);

        // A new frame keeps the windows of the menu open, so a change of the fit from the
        // settings screen returns to the main list (D-707, D-871).
        if (this.menus is MenuHost host)
        {
            host.Rebuild(built, built_ui);
        }
        else
        {
            this.menus = new MenuHost(built, built_ui, open, loaded, this.SettingsInUse, this.CloseSettings, entries => this.WriteLog(entries));
        }

        this.BuildConsole(built, open);
    }

    /// <summary>Gives the settings in use, which the settings screen of the menu opens with (D-871).</summary>
    /// <returns>The settings.</returns>
    /// <exception cref="InvalidOperationException">The session read no settings (T-2).</exception>
    private GameSettings SettingsInUse() => this.settings ?? throw new InvalidOperationException(
        "The settings screen opened, and the session read no settings (T-2).");

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
        GameRun wiped = this.run ?? throw new InvalidOperationException(
            "A wipe reloads the run, and the session holds no run (T-2).");
        GameRun reloaded = this.ReloadRun(loaded, SaveStore.OfThisSystem(), wiped, chosen.Battle.Messages);
        this.run = reloaded;

        // The menu of the old run ends with it, and the new run builds its own (D-776).
        this.menus = null;
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

        built.HandOffPass.Show(open.HandOff, open.Transitions, open.FightTick, chosen.Access.Effects, loaded.Palette);
    }

    /// <summary>Removes the frame and its nodes, and builds the screen again over the current run.</summary>
    /// <param name="loaded">The content set of this build.</param>
    private void RebuildScreen(ContentSet loaded)
    {
        this.frame?.QueueFree();
        this.frame = null;
        this.map = null;
        this.battle = null;
        this.noticeBox = null;
        this.pause = null;
        this.console = null;
        this.settingsNotice = null;
        this.BuildScreen(loaded);

        // The message of a refused settings file stands until a press, so a new frame shows it again (D-1099).
        this.ShowSettingsNotice();
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
            if (loaded is null || built is null)
            {
                return;
            }

            Vector2I screen = this.GetWindow().Size;
            ScreenFit fit = ScreenFit.Of(built.Mode, Math.Max(1, screen.X), Math.Max(1, screen.Y));
            int body = BodySize.DefaultFor(fit.Height, loaded.Style.SmallBody, loaded.Style.LargeBody);
            bool running = this.run is not null && !this.crashed;
            if (!BodySize.RebuildsOnResize(running, this.settings?.Display.Body == BodySetting.Auto, this.builtBody, body))
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

    /// <summary>
    /// Gives the run after a wipe: the newer save of the same run, or the run again from its start
    /// when no save of it exists (D-231, D-776, D-1114). The log takes a line for each save of
    /// another run, and a line for each change of a save of another build (D-1113).
    /// </summary>
    /// <param name="loaded">The content set of this build.</param>
    /// <param name="saves">The folder of the saves: the folder of the person, or a new folder of the smoke session.</param>
    /// <param name="wiped">The run that wiped, whose seed names the run.</param>
    /// <param name="messageSpeed">The message speed of the settings (D-866).</param>
    /// <returns>The run.</returns>
    private GameRun ReloadRun(ContentSet loaded, SaveStore saves, GameRun wiped, MessageSpeed messageSpeed)
    {
        SaveDocument? slot = this.ReadSaveOfRun(saves, SaveKind.Slot, wiped);
        SaveDocument? autosave = this.ReadSaveOfRun(saves, SaveKind.Autosave, wiped);
        List<LogEntry> drift = [];
        GameRun reloaded = GameRun.Reload(loaded, slot, autosave, wiped.Seed, DebugSeam.Handlers(), messageSpeed, drift);
        this.WriteLog(drift);
        return reloaded;
    }

    /// <summary>Reads one save, and logs and drops a save of another run (D-1114).</summary>
    /// <param name="saves">The folder of the saves.</param>
    /// <param name="kind">The save to read.</param>
    /// <param name="wiped">The run that wiped.</param>
    /// <returns>The save of the run, or no value.</returns>
    private SaveDocument? ReadSaveOfRun(SaveStore saves, SaveKind kind, GameRun wiped)
    {
        if (!saves.Exists(kind))
        {
            return null;
        }

        SaveDocument save = saves.Read(kind);
        if (SavePick.OfRun(save, wiped.Seed))
        {
            return save;
        }

        this.WriteLog([new LogEntry(
            LogLevel.Warning,
            "a save of another run stays in its folder, and the wipe does not reload it",
            wiped.Tick,
            LogSubsystems.Game,
            [new LogField("save", SaveStore.FileNameOf(kind)), new LogField("save_seed", SeedText(save.Header.Seed)), new LogField("run_seed", SeedText(wiped.Seed))])]);
        return null;
    }

    /// <summary>Gives a seed as the hex text of a record header (G-5).</summary>
    private static string SeedText(ulong seed) => "0x" + seed.ToString("x16", CultureInfo.InvariantCulture);

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
    /// Reads each input event before any node of the frame reads it. The event shows or hides
    /// the mouse pointer, the gate stops a second press of a held action, and a key goes on its
    /// route (D-171, D-725, D-813, D-1077, D-1078).
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
        if (this.crashed || signal is null)
        {
            return;
        }

        try
        {
            this.pointer.Read(signal);

            // The gate stops a second press of a held action, from any device, before any node
            // reads it (D-1077, F-107). The console takes the repeat of a held key, so the gate
            // stops no key while the console is open (D-725).
            if (!this.gate.Read(signal) && this.console?.Visible != true)
            {
                GetViewport().SetInputAsHandled();
                return;
            }
        }
        catch (Exception fault)
        {
            this.ReportCrash(fault);
            return;
        }

        if (signal is not InputEventKey key)
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

        if (this.settingsNotice is SettingsNotice notice)
        {
            // The message of a refused settings file takes every event, and a press closes it
            // and lets the world go on (D-1099).
            if (signal.IsPressed() && !signal.IsEcho())
            {
                notice.QueueFree();
                this.settingsNotice = null;
                this.refusedSettings = null;
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

        // The menu takes every event while a window is open, the mouse included, and the world
        // stays paused under it (D-162, D-211, D-872).
        if (this.menus is MenuHost host && host.IsOpen)
        {
            host.Read(signal);
            return;
        }

        // The party walks while a direction is down, so the map needs the press and the
        // release of each step action, and never a poll (D-716, F-50).
        this.held.Read(signal);

        // In a fight, the pause takes the menu action, and while it holds it takes every action
        // (D-1083). The command menu then gets no event, so no battle intent meets the refusal of
        // the rules under the menu (D-162).
        if (run.InBattle && this.ReadFightPause(run, signal))
        {
            return;
        }

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

            // The menu action opens the main list on the walk, and the map action opens the
            // dungeon map screen. The host sends the intents of the menu (D-162, D-211, D-986).
            // The pause took the menu action of a fight, and a story scene takes no menu (D-1009).
            Intent made = run.IntentOf(action);
            bool menu = string.CompareOrdinal(action, InputActions.Menu) == 0;
            bool map = string.CompareOrdinal(action, InputActions.Map) == 0;
            bool torch = string.CompareOrdinal(action, InputActions.Torch) == 0;
            if (torch)
            {
                // The torch works on the walk alone, with the torch in the pack, and the rules
                // refuse the intent at any other time (D-1071, T-2).
                if (run.TorchWorks)
                {
                    run.Queue(made);
                }
                else
                {
                    this.WriteLog([new LogEntry(
                        LogLevel.Debug,
                        "the torch action works on the walk alone, with the torch in the pack",
                        run.Tick,
                        LogSubsystems.Game,
                        [new LogField("action", action)])]);
                }
            }
            else if ((menu || map) && run.MenuWorks)
            {
                this.OpenMenu(run, action);
            }
            else if (menu || map)
            {
                this.WriteLog([new LogEntry(
                    LogLevel.Debug,
                    "the menu action and the map action open a window on the walk alone",
                    run.Tick,
                    LogSubsystems.Game,
                    [new LogField("action", action)])]);
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

    /// <summary>
    /// Reads one event of a fight for its pause (D-1083). The menu action pauses the fight, and
    /// the menu action or the back action ends the pause. The pause takes every other action.
    /// </summary>
    /// <param name="run">The run, which holds a fight or its encounter.</param>
    /// <param name="signal">The event of this frame.</param>
    /// <returns>True when the pause took the event, and no other reader gets it.</returns>
    private bool ReadFightPause(GameRun run, InputEvent signal)
    {
        bool paused = run.FightPaused;
        string? action = null;
        if (signal.IsActionPressed(InputActions.Menu))
        {
            action = InputActions.Menu;
        }
        else if (paused && signal.IsActionPressed(InputActions.Cancel))
        {
            action = InputActions.Cancel;
        }

        if (action is null)
        {
            return paused;
        }

        if (run.PauseIntentOf(action) is Intent made)
        {
            run.Queue(made);
            this.WriteLog([new LogEntry(
                LogLevel.Info,
                paused ? "the player ended the pause of the fight" : "the player paused the fight",
                run.Tick,
                LogSubsystems.Game,
                [new LogField("action", action)])]);
            return true;
        }

        // The rules have yet to apply a menu intent, the fight ends, or a story scene runs, so
        // the action makes no intent now (D-1083, D-1009).
        this.WriteLog([new LogEntry(
            LogLevel.Debug,
            "the fight takes no pause intent now",
            run.Tick,
            LogSubsystems.Game,
            [new LogField("action", action)])]);
        return true;
    }

    /// <summary>Opens the main list or the dungeon map screen from the walk, and the world pauses (D-162, D-211, D-986).</summary>
    /// <param name="run">The run, whose world pauses.</param>
    /// <param name="action">The menu action or the map action.</param>
    /// <exception cref="InvalidOperationException">The session built no menu host (T-2).</exception>
    private void OpenMenu(GameRun run, string action)
    {
        MenuHost host = this.menus ?? throw new InvalidOperationException(
            $"The menu opened at tick {run.Tick}, and the session built no menu host (T-2).");

        // The menu takes every input while it is open, so no release of a held direction
        // reaches the map. A party that kept the direction would walk on after the close (T-2).
        this.held.Clear();
        if (string.CompareOrdinal(action, InputActions.Map) == 0)
        {
            host.OpenDungeonMap();
        }
        else
        {
            host.OpenMainList();
        }
    }

    /// <summary>
    /// Writes and applies the settings of the settings screen when it closes, with a change, and
    /// logs the close (D-860, D-871).
    /// </summary>
    /// <param name="after">The settings of the screen, which holds no conflict (D-862).</param>
    /// <exception cref="InvalidOperationException">The session read no settings or started no run (T-2).</exception>
    /// <exception cref="StorageException">The system refused the write (T-2).</exception>
    private void CloseSettings(GameSettings after)
    {
        GameRun run = this.run ?? throw new InvalidOperationException(
            "The settings screen closed, and the session started no run (T-2).");
        GameSettings before = this.settings ?? throw new InvalidOperationException(
            $"The settings screen closed at tick {run.Tick}, and the session read no settings (T-2).");
        SettingsStore store = this.settingsStore ?? throw new InvalidOperationException(
            $"The settings screen closed at tick {run.Tick}, and the session made no settings store (T-2).");

        if (!after.Equals(before))
        {
            store.Write(after);
            this.settings = after;
            this.ApplySettings(run, before, after);
        }

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
    /// <para>
    /// The error of the crash fixture takes the same path, with the store and the time of the
    /// fixture, and the session then goes on to check what this method left (P3-26).
    /// </para>
    /// </remarks>
    private void ReportCrash(Exception fault)
    {
        GameRun? stopped = this.run;
        this.run = null;

        // The first error after the plant is the planted one. Each later error takes the path of
        // a real crash, the error of a failed check included (T-2).
        PlantedCrash? plant = this.planted is { Reported: false } ? this.planted : null;
        DateTime time = plant?.Time ?? DateTime.UtcNow;
        string? path = null;
        StorageException? cleanup = null;
        try
        {
            CrashStore crashes = plant?.Store ?? CrashStore.OfThisSystem();
            path = crashes.Write(fault, stopped?.Record(), time);
            cleanup = crashes.CleanupFault;
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

                // The crash file exists, and an older file stayed. The limit of D-659 never hides
                // the file that this crash wrote (T-2).
                if (cleanup is not null)
                {
                    GD.PrintErr($"the game wrote the crash file '{path}', and it could not remove an older file: {cleanup.Message}");
                    this.log?.Write([CleanupEntry(cleanup, stopped?.Tick ?? 0)], time);
                }
            }
            catch (Exception second)
            {
                // The crash file exists, and the log line alone failed. The message says so.
                GD.PrintErr($"the game could not write the log line of the crash file '{path}': {second}");
            }
        }

        bool shown = this.ShowCrashMessage(path, this.content ?? plant?.Content);
        if (plant is not null)
        {
            // The crash fixture reads what this method left, and the session goes on (P3-26).
            plant.Reported = true;
            plant.Path = path;
            plant.Shown = shown;
            return;
        }

        if (shown)
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
    /// <param name="loaded">The content of the session, or of the crash fixture, or null before it loaded.</param>
    /// <returns>True when the message is on screen, and false when the session must quit now.</returns>
    /// <remarks>
    /// A session with no display, such as the smoke job of CI, shows nothing and quits with
    /// the crash code (D-117). A crash before the content loaded does the same, because the
    /// message needs the string table (T-2). A crash after the content loaded and before the
    /// screen, such as a settings file that the system refused, builds a frame of the default
    /// display for the message, so the start never ends with no text (P2-2, D-170).
    /// </remarks>
    private bool ShowCrashMessage(string? path, ContentSet? loaded)
    {
        if (path is null
            || loaded is null
            || string.CompareOrdinal(DisplayServer.GetName(), HeadlessDisplay) == 0)
        {
            return false;
        }

        try
        {
            (FrameRoot shownFrame, UiBase shownUi) = this.FrameForMessage(loaded);

            // The message draws above the pass of the hand-off, so a crash during a transition
            // still shows its text (D-559). It names the folder with no account name (D-1102).
            var message = new CrashScreen();
            shownFrame.Top.AddChild(message);
            message.Build(shownUi, loaded.Strings, Path.GetFileName(path), CrashStore.ShownFolderOfThisSystem());
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
    /// Gives the frame and the UI base that the message of a crash draws on: those of the screen,
    /// or a frame and a UI base of the default display when the crash came before the screen
    /// built them (P2-2, D-170). The default display needs no settings file.
    /// </summary>
    /// <param name="loaded">The content, which holds the style and the string table.</param>
    /// <returns>The frame and the UI base.</returns>
    private (FrameRoot Frame, UiBase Ui) FrameForMessage(ContentSet loaded)
    {
        DisplaySettings display = GameSettings.Defaults(GameInputMap.DefaultBindings()).Display;
        FrameRoot target = this.frame ?? FrameRoot.AddTo(this);
        if (this.frame is null)
        {
            target.SetMode(FitModeOf(display.Fit));
        }

        UiBase shown = this.ui ?? UiBase.Load(loaded, BodyOf(display.Body, target.Fit.Height, loaded.Style));
        return (target, shown);
    }

    /// <summary>
    /// The warning line of an older file that the limit of D-659 could not remove: the name of
    /// that file, with no folder, and the type of the error of the system (D-170, D-179).
    /// </summary>
    /// <param name="cleanup">The error of the removal.</param>
    /// <param name="tick">The tick of the run, or 0 before a run exists.</param>
    /// <returns>The log entry.</returns>
    private static LogEntry CleanupEntry(StorageException cleanup, long tick) =>
        new(
            LogLevel.Warning,
            "the game kept an older file, because the system refused its removal (D-659)",
            tick,
            LogSubsystems.Game,
            [
                new LogField("file", Path.GetFileName(cleanup.Path)),
                new LogField("error", cleanup.InnerException?.GetType().Name ?? nameof(StorageException)),
            ]);

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
            CrashLogMessage,
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

        // The wipe of the smoke fight reads a new folder with no save, so no save of the person
        // reaches the result of a check (D-1114).
        string smokeSaves = Directory.CreateTempSubdirectory("the-thing-below-smoke-").FullName;

        IReadOnlyList<ContentFile> files = EmbeddedContent.Read();
        ContentSet content = ContentSet.Load(files);
        GD.Print($"smoke: the content is {files.Count} files with the hash {content.Hash}.");

        GameRun session = GameRun.Start(content, FixtureSeed, DebugSeam.Handlers(), SmokeSettings().Battle.Messages);
        GD.Print($"smoke: the run is {this.DescribeRun(session)}.");
        GD.Print($"smoke: the log is {this.DescribeLog()}.");
        GD.Print($"smoke: the crash file is {DescribeCrashFile(session)}.");
        GD.Print($"smoke: the UI base is {DescribeUiBase(content)}.");
        GD.Print($"smoke: the map is {DescribeMap(content, session)}.");
        GD.Print($"smoke: the lit map is {DescribeLitMap(content, files, session)}.");
        GD.Print($"smoke: the picture is {DescribePicture(content)}.");
        GD.Print($"smoke: the console is {this.DescribeConsole(session)}.");
        GD.Print($"smoke: the battle is {this.DescribeBattle(content, session, new SaveStore(smokeSaves))}.");
        GD.Print($"smoke: the settings screen is {this.DescribeSettings(content)}.");
        GD.Print($"smoke: the menus are {this.DescribeMenus(content)}.");
        GD.Print($"smoke: the pads are {DescribePads()}.");
        GD.Print($"smoke: the light textures are {DescribeLightTextures()}.");
        Directory.Delete(smokeSaves, true);

        // The crash path runs last, inside a callback of the engine on the next frame, as the
        // error of a real crash does. The success line waits for its check (P3-26, T-2).
        this.PlantCrash(content, line =>
        {
            GD.Print($"smoke: the crash path is {line}.");
            GD.Print("smoke: the session ends with no error.");
            GetTree().Quit(SuccessExitCode);
        });
    }

    /// <summary>
    /// Reads the builds of the light texture and the halo texture in the session, and fails on a
    /// second build of either one (G-14). The session builds two maps and a fight with its spell
    /// flash, and each one takes the shared texture of <see cref="WorldLights"/>.
    /// </summary>
    /// <exception cref="InvalidOperationException">A texture was built more than once, or never (T-2).</exception>
    private static string DescribeLightTextures()
    {
        if (WorldLights.LightTextureBuilds != 1 || WorldLights.HaloTextureBuilds != 1)
        {
            throw new InvalidOperationException(
                $"The session built the light texture {WorldLights.LightTextureBuilds} times and the halo texture {WorldLights.HaloTextureBuilds} times, and each map, each fight, and each spell flash shares one of each (G-14, T-2).");
        }

        string time = WorldLights.TextureBuildTime.TotalMilliseconds.ToString("F1", CultureInfo.InvariantCulture);
        return $"one build of the light texture and one of the halo texture, in {time} ms, for two maps and a fight";
    }

    /// <summary>Gives the settings of the smoke session: the defaults, and never the file of the person (D-860).</summary>
    /// <returns>The default settings, with the default bindings of the game.</returns>
    /// <remarks>A CI machine holds no settings file, and a check never writes to the folder of the person.</remarks>
    private static GameSettings SmokeSettings() => GameSettings.Defaults(GameInputMap.DefaultBindings());

    /// <summary>
    /// Reads the settings file, or writes the defaults when no file exists, which is the first
    /// start (D-860, D-868). A file that fails to load goes aside, and the session runs on the
    /// defaults (D-1099, D-1100).
    /// </summary>
    /// <param name="store">The store of the settings file of the person.</param>
    /// <returns>The settings of this session.</returns>
    /// <exception cref="StorageException">The system refused the move of a refused file or the write of the defaults (T-2).</exception>
    /// <remarks>
    /// The fallback is loud, and never silent (T-2, D-570): the file stays as
    /// <see cref="SettingsStore.RefusedName"/>, a warning line holds the whole error, and the first
    /// screen shows the field and the kept file until the player presses a button. A file of a
    /// newer build takes the same path, so an update never breaks a start (D-1100).
    /// </remarks>
    private GameSettings LoadSettings(SettingsStore store)
    {
        if (store.Exists())
        {
            try
            {
                GameSettings read = store.Read();
                GameInputMap.CheckActions(read.Controls.Bindings);
                return read;
            }
            catch (Exception refused) when (refused is StorageException or InvalidOperationException)
            {
                return this.SetSettingsAside(store, refused);
            }
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
    /// Keeps a refused settings file aside, writes the defaults, and notes the refusal for the
    /// message of the first screen (D-1099, D-1100).
    /// </summary>
    /// <param name="store">The store of the settings file.</param>
    /// <param name="refused">The error of the read or of the check of the actions.</param>
    /// <returns>The defaults, which this session runs on.</returns>
    /// <exception cref="StorageException">The system refused the move or the write (T-2).</exception>
    private GameSettings SetSettingsAside(SettingsStore store, Exception refused)
    {
        string kept = store.SetAside();
        GameSettings defaults = GameSettings.Defaults(GameInputMap.DefaultBindings());
        store.Write(defaults);
        string field = SettingsFallback.FieldOf(refused);
        this.refusedSettings = new SettingsRefusal(field, Path.GetFileName(kept));
        this.WriteLog([new LogEntry(
            LogLevel.Warning,
            "the settings file did not load, and the session kept it and runs on the defaults",
            0,
            LogSubsystems.Game,
            [
                new LogField("path", store.Path),
                new LogField("kept", kept),
                new LogField("field", field),
                new LogField("error", refused.Message),
            ])]);
        return defaults;
    }

    /// <summary>
    /// Shows the message of a refused settings file on the first screen, above the hand-off, and
    /// holds the world until a press (D-1099). A start with no refusal, or a message that the player closed, shows nothing.
    /// </summary>
    /// <exception cref="InvalidOperationException">The screen or the UI base is not built (T-2).</exception>
    private void ShowSettingsNotice()
    {
        if (this.refusedSettings is not SettingsRefusal refusal)
        {
            return;
        }

        FrameRoot built = this.frame ?? throw new InvalidOperationException(
            "The message of a refused settings file shows, and the frame is not built (T-2).");
        UiBase shown = this.ui ?? throw new InvalidOperationException(
            "The message of a refused settings file shows, and the UI base is not built (T-2).");
        var notice = new SettingsNotice();
        built.Top.AddChild(notice);
        notice.Build(shown, refusal.Field, refusal.KeptFileName);
        this.settingsNotice = notice;
    }

    /// <summary>A settings file that a start refused: the field that failed, and the name of the kept file (D-1099).</summary>
    /// <param name="Field">The field of the file that failed.</param>
    /// <param name="KeptFileName">The name of the kept file, with no folder (D-170).</param>
    private sealed record SettingsRefusal(string Field, string KeptFileName);

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
    /// Plants the crash of the crash fixture: a <see cref="CrashProbe"/> throws inside its first
    /// callback, and <see cref="ReportCrash"/> takes the error (P3-26, T-3). The probe then runs
    /// <see cref="CheckPlantedCrash"/> and gives its line to the session.
    /// </summary>
    /// <param name="loaded">The content of the session, which the message reads (G-7).</param>
    /// <param name="passed">Takes the line of the check, and goes on with the session.</param>
    /// <exception cref="InvalidOperationException">The session planted a crash before (T-2).</exception>
    /// <remarks>
    /// The crash file goes into a new folder of its own, and never into the crash folder of the
    /// person, because that folder keeps the newest files alone (D-659). The new folder holds no
    /// file of an earlier run, so the file takes the same name on every run, and the message
    /// that names it gives one picture (D-172).
    /// </remarks>
    private void PlantCrash(ContentSet loaded, Action<string> passed)
    {
        ArgumentNullException.ThrowIfNull(loaded);
        ArgumentNullException.ThrowIfNull(passed);
        if (this.planted is not null)
        {
            throw new InvalidOperationException("The session plants one crash, and it planted one before (P3-26, T-2).");
        }

        string folder = Directory.CreateTempSubdirectory("the-thing-below-crash-").FullName;
        this.planted = new PlantedCrash(new CrashStore(folder), CrashProbe.PlantedTime, loaded);
        CrashProbe.Plant(this, this.ReportCrash, () => passed(this.CheckPlantedCrash()));
    }

    /// <summary>
    /// Checks what <see cref="ReportCrash"/> left for the planted crash: the crash file that names
    /// the error, the log line that names the file, and the message on screen with the folder of
    /// the crash files (P3-26, D-170, D-179, D-559, D-1102). Then it removes the folder of the file.
    /// </summary>
    /// <returns>The name of the file, the error, and the state of the message, as one line.</returns>
    /// <exception cref="InvalidOperationException">A part of the crash path is absent or wrong (T-2).</exception>
    private string CheckPlantedCrash()
    {
        PlantedCrash plant = this.planted ?? throw new InvalidOperationException(
            "The crash fixture checks its crash, and the session planted none (T-2).");
        if (!plant.Reported)
        {
            throw new InvalidOperationException("The crash fixture threw its error, and the reporter never took it (P3-26, T-2).");
        }

        string path = plant.Path ?? throw new InvalidOperationException(
            $"The reporter took the planted error, and it wrote no crash file into '{plant.Store.Folder}' (D-170, T-2).");
        string file = Path.GetFileName(path);

        CrashReport report = plant.Store.Read(path);
        if (string.CompareOrdinal(report.ErrorType, nameof(InvalidOperationException)) != 0
            || !report.Error.Contains(CrashProbe.PlantedMessage, StringComparison.Ordinal))
        {
            throw new InvalidOperationException(
                $"The crash file '{file}' names the error {report.ErrorType}: {report.Error}, and the probe threw "
                + $"{nameof(InvalidOperationException)}: {CrashProbe.PlantedMessage} (D-170, T-2).");
        }

        // The session holds no run, so the file holds no record (D-661, exit test 8 of PR-44).
        if (report.Record is not null)
        {
            throw new InvalidOperationException($"The crash file '{file}' holds a record, and the session held no run (D-661, T-2).");
        }

        this.CheckCrashLogLine(file);
        string message = this.CheckCrashMessage(plant, file);
        Directory.Delete(plant.Store.Folder, true);
        return $"{file} with the error {report.ErrorType}, the log line of the file, and {message}";
    }

    /// <summary>Finds the log line of the planted crash in the log file of the session (D-179).</summary>
    /// <param name="file">The name of the crash file, which the line names.</param>
    /// <exception cref="InvalidOperationException">No log file is open, or no line names the file (T-2).</exception>
    private void CheckCrashLogLine(string file)
    {
        LogStore store = this.log ?? throw new InvalidOperationException(
            "The crash fixture reads the log file, and the session opened none (T-2).");

        foreach (LogLine line in store.Read())
        {
            LogEntry entry = line.Entry;
            if (entry.Level != LogLevel.Error || string.CompareOrdinal(entry.Message, CrashLogMessage) != 0)
            {
                continue;
            }

            bool namesType = false;
            bool namesFile = false;
            foreach (LogField field in entry.Fields)
            {
                namesType |= field.Name == "type" && field.Value == nameof(InvalidOperationException);
                namesFile |= field.Name == "file" && field.Value == file;
            }

            if (namesType && namesFile)
            {
                return;
            }
        }

        throw new InvalidOperationException(
            $"The log file '{store.SessionFile}' holds no error line '{CrashLogMessage}' with the type "
            + $"{nameof(InvalidOperationException)} and the file '{file}' (D-179, T-2).");
    }

    /// <summary>
    /// Checks the message of the planted crash. A session with a display shows one message with
    /// the name of the file and the folder with no account name, and a session with no display
    /// shows none (D-117, D-559, D-1102).
    /// </summary>
    /// <param name="plant">The planted crash.</param>
    /// <param name="file">The name of the crash file.</param>
    /// <returns>The state of the message, as a part of a line.</returns>
    /// <exception cref="InvalidOperationException">The message is absent, hidden, or wrong (T-2).</exception>
    private string CheckCrashMessage(PlantedCrash plant, string file)
    {
        var screens = new List<CrashScreen>();
        CrashScreensUnder(this, screens);
        if (string.CompareOrdinal(DisplayServer.GetName(), HeadlessDisplay) == 0)
        {
            if (plant.Shown || screens.Count != 0)
            {
                throw new InvalidOperationException(
                    $"The session has no display, and the reporter showed the message {plant.Shown} with {screens.Count} nodes (D-117, T-2).");
            }

            return "no message, because the session has no display";
        }

        if (!plant.Shown || screens.Count != 1 || !screens[0].IsVisibleInTree())
        {
            throw new InvalidOperationException(
                $"The reporter showed the message {plant.Shown}, the tree holds {screens.Count} messages, "
                + "and the session has a display, so one message shows (D-559, T-2).");
        }

        string folder = CrashStore.ShownFolderOfThisSystem();
        string home = System.Environment.GetFolderPath(System.Environment.SpecialFolder.UserProfile);
        IReadOnlyList<string> lines = screens[0].ShownLines();
        bool namesFile = false;
        bool namesFolder = false;
        foreach (string line in lines)
        {
            namesFile |= line.Contains(file, StringComparison.Ordinal);
            namesFolder |= line.Contains(folder, StringComparison.Ordinal);
            if (home.Length > 0 && line.Contains(home, StringComparison.Ordinal))
            {
                throw new InvalidOperationException($"The line '{line}' of the message names the folder of the person (D-1102, T-2).");
            }
        }

        if (!namesFile || !namesFolder)
        {
            throw new InvalidOperationException(
                $"The message shows the lines '{string.Join(" | ", lines)}', and it names the file '{file}' {namesFile} "
                + $"and the folder '{folder}' {namesFolder} (D-559, D-1102, T-2).");
        }

        return $"the message on screen with the folder {folder}";
    }

    /// <summary>Adds each crash message under one node to a list, in the order of the tree.</summary>
    /// <param name="node">The node where the search starts.</param>
    /// <param name="found">The list that takes each message.</param>
    private static void CrashScreensUnder(Node node, List<CrashScreen> found)
    {
        foreach (Node child in node.GetChildren())
        {
            if (child is CrashScreen screen)
            {
                found.Add(screen);
            }

            CrashScreensUnder(child, found);
        }
    }

    /// <summary>The crash that the crash fixture plants, and what the reporter left for it (P3-26).</summary>
    /// <param name="store">The store of a new folder of the fixture, and never the crash folder of the person (D-659).</param>
    /// <param name="time">The fixed time of the crash, which the name of the file carries (D-172).</param>
    /// <param name="content">The content that the message reads (G-7).</param>
    private sealed class PlantedCrash(CrashStore store, DateTime time, ContentSet content)
    {
        /// <summary>The store that takes the crash file.</summary>
        public CrashStore Store { get; } = store;

        /// <summary>The time of the crash.</summary>
        public DateTime Time { get; } = time;

        /// <summary>The content that the message reads.</summary>
        public ContentSet Content { get; } = content;

        /// <summary>True after the reporter took the planted error.</summary>
        public bool Reported { get; set; }

        /// <summary>The path of the crash file that the reporter wrote, or null when the write failed.</summary>
        public string? Path { get; set; }

        /// <summary>True when the reporter showed the message.</summary>
        public bool Shown { get; set; }
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
    /// Builds the input map, and reads the pad rules of D-1077 and D-1078 with the events of a
    /// pad other than the first, as a Steam Deck with Steam Input sends them (F-107).
    /// </summary>
    /// <returns>The count of pad bindings that matched, and the results of the gate and the pointer.</returns>
    /// <exception cref="InvalidOperationException">A pad binding matched no event, or the gate or the pointer broke a rule (T-2).</exception>
    /// <remarks>
    /// Before D-1077 each pad binding matched the device 0 alone, and no menu action held a pad
    /// button but the D-pad, so this check failed on the old input map (T-3).
    /// </remarks>
    private static string DescribePads()
    {
        const int OtherPad = 3;
        ControlSettings controls = SmokeSettings().Controls;
        GameInputMap.Build(controls);

        int matched = 0;
        foreach (string action in InputActions.Names)
        {
            foreach (InputBinding binding in controls.Bindings.Of(action))
            {
                InputEvent press = binding.Kind switch
                {
                    BindingKind.Key => new InputEventKey { PhysicalKeycode = (Key)binding.Code, Pressed = true },
                    BindingKind.Button => PadButton((JoyButton)binding.Code, OtherPad, true),
                    _ => PadStick((JoyAxis)binding.Code, OtherPad, binding.Direction),
                };
                if (!press.IsActionPressed(action))
                {
                    throw new InvalidOperationException(
                        $"The binding {binding} of the action '{action}' matched no press of the device {OtherPad} (D-1077, T-2).");
                }

                matched += 1;
            }
        }

        foreach ((string action, JoyButton button) in GameInputMap.MenuPadButtons)
        {
            if (!PadButton(button, OtherPad, true).IsActionPressed(action))
            {
                throw new InvalidOperationException(
                    $"The pad button {button} of the device {OtherPad} did not press the menu action '{action}' (D-1077, T-2).");
            }

            matched += 1;
        }

        // One hold of the menu button, a mirror of it on a second device, and a new press after
        // the release of both. Then one push of the stick in three motion events (F-107).
        var gate = new PressGate();
        bool[] passed =
        [
            gate.Read(PadButton(JoyButton.Start, OtherPad, true)),
            gate.Read(PadButton(JoyButton.Start, OtherPad, true)),
            gate.Read(PadButton(JoyButton.Start, OtherPad + 1, true)),
            gate.Read(PadButton(JoyButton.Start, OtherPad, false)),
            gate.Read(PadButton(JoyButton.Start, OtherPad + 1, false)),
            gate.Read(PadButton(JoyButton.Start, OtherPad, true)),
            gate.Read(PadStick(JoyAxis.LeftY, OtherPad, 0.8f)),
            gate.Read(PadStick(JoyAxis.LeftY, OtherPad, 0.9f)),
            gate.Read(PadStick(JoyAxis.LeftY, OtherPad, 1f)),
            gate.Read(PadStick(JoyAxis.LeftY, OtherPad, 0f)),
            gate.Read(PadStick(JoyAxis.LeftY, OtherPad, 0.8f)),
        ];
        bool[] expected = [true, false, false, true, true, true, true, false, false, true, true];
        if (!passed.AsSpan().SequenceEqual(expected))
        {
            throw new InvalidOperationException(
                $"The press gate passed [{string.Join(", ", passed)}], and the rule gives [{string.Join(", ", expected)}] (D-1077, T-2).");
        }

        var pointer = new MousePointer();
        pointer.Read(PadButton(JoyButton.A, OtherPad, true));
        bool hidden = !pointer.Shown;
        pointer.Read(new InputEventMouseMotion { Relative = new Vector2(4, 0) });
        if (!hidden || !pointer.Shown)
        {
            throw new InvalidOperationException(
                $"A pad press hid the pointer {hidden}, and a mouse move showed it {pointer.Shown} (D-1078, T-2).");
        }

        return $"{matched} bindings that a pad of the device {OtherPad} presses, one press of each hold, " +
            "and a pointer that a pad hides and a mouse shows";
    }

    /// <summary>Makes the press or the release of one pad button, for the smoke session.</summary>
    private static InputEventJoypadButton PadButton(JoyButton button, int device, bool pressed) =>
        new() { ButtonIndex = button, Device = device, Pressed = pressed };

    /// <summary>Makes one motion event of one stick axis, for the smoke session.</summary>
    private static InputEventJoypadMotion PadStick(JoyAxis axis, int device, float value) =>
        new() { Axis = axis, Device = device, AxisValue = value };

    /// <summary>
    /// Opens each window of the menu stack with the `ui_*` actions, as a player does, and the
    /// dungeon map screen, and shows one notice in the notice box (D-211, D-221, D-986). Each window
    /// reads its strings, so a string id that the table lacks fails every CI leg (T-2).
    /// </summary>
    /// <param name="loaded">The content set of this build.</param>
    /// <returns>The count of windows that opened, the row of the lead after the party window, and the notice.</returns>
    /// <exception cref="InvalidOperationException">A window stayed open, or the row intent never reached the run (T-2).</exception>
    private string DescribeMenus(ContentSet loaded)
    {
        GameInputMap.Build(SmokeSettings().Controls);
        FrameRoot built = FrameRoot.AddTo(this);
        UiBase shownBase = UiBase.Load(loaded, loaded.Style.SmallBody);
        GameRun session = GameRun.Start(loaded, FixtureSeed, DebugSeam.Handlers(), SmokeSettings().Battle.Messages);
        int closes = 0;
        var host = new MenuHost(built, shownBase, session, loaded, SmokeSettings, _ => closes += 1, entries => this.WriteLog(entries));

        // The party window moves the lead to the other row, and each other entry opens and closes:
        // the lessons, the gear, the items, the status, the log, and the settings (D-988, D-992).
        host.OpenMainList();
        string[] presses =
        [
            "ui_accept", "ui_accept", "ui_cancel",
            "ui_down", "ui_accept", "ui_cancel",
            "ui_down", "ui_accept", "ui_cancel",
            "ui_down", "ui_accept", "ui_cancel",
            "ui_down", "ui_accept", "ui_cancel",
            "ui_down", "ui_accept", "ui_cancel",
            "ui_down", "ui_accept", "ui_cancel",
            "ui_cancel",
        ];
        int opened = 1;
        foreach (string press in presses)
        {
            int before = host.Path.Windows.Count;
            host.Read(new InputEventAction { Action = press, Pressed = true });
            opened += host.Path.Windows.Count > before ? 1 : 0;
        }

        host.OpenDungeonMap();
        host.Read(new InputEventAction { Action = "ui_cancel", Pressed = true });
        opened += 1;
        this.WriteLog(session.Advance(1.0 / FixedStepLoop.TicksPerSecond));

        Core.Notices.NoticeRecord notice = loaded.Notices.FirstThatLogs(true);
        var noticeBox = new NoticeBox(built, shownBase);
        noticeBox.Show(new NoticeFrame(notice.Id, NoticePhase.Hold, 0, loaded.Strings.Text(notice.Id).Length, 1000), underMenu: false);

        Core.Battles.BattleRow row = session.State.Characters.Members[0].Row;
        bool open = host.IsOpen || session.MenuOpen;
        this.RemoveChild(built);
        built.QueueFree();
        if (open || closes != 1 || row != Core.Battles.BattleRow.Back)
        {
            throw new InvalidOperationException(
                $"The smoke menus left the menu open ({open}), closed the settings screen {closes} times, or left the lead in the row '{row}' (T-2).");
        }

        return $"{opened} windows that opened and closed, the lead in the back row, and the notice '{notice.Id.Value}' in the notice box";
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
        FrameRoot built = FrameRoot.AddTo(this);
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
        string torch = CheckTorchDraw(drawn, session);
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
            + $"{sprites}, {torch}, {lights}, {weather}, {lights2}, {room}, and {ground}";
    }

    /// <summary>
    /// Draws a copy of the first map that is not dark, and fails when a live enemy draws no sprite
    /// (D-814, D-1118). The first map is dark since PR-91, and on a dark map the fade of the sight
    /// sets each sprite, so the check of that map alone could not see a lost enemy.
    /// </summary>
    /// <param name="loaded">The content set of this build.</param>
    /// <param name="files">The content files of this build, which hold the map file.</param>
    /// <param name="session">The run of the smoke session, whose map this check copies.</param>
    /// <returns>The count of enemies and the count that draws.</returns>
    /// <exception cref="InvalidOperationException">The map file holds no dark flag, or a live enemy draws no sprite (T-2).</exception>
    private static string DescribeLitMap(ContentSet loaded, IReadOnlyList<ContentFile> files, GameRun session)
    {
        GameMap dark = session.Party.Map;
        const string DarkFlag = "\"dark\": true";
        string text = System.Text.Encoding.UTF8.GetString(FileOf(files, dark.File).Bytes);
        if (!text.Contains(DarkFlag, StringComparison.Ordinal))
        {
            throw new InvalidOperationException($"The map file '{dark.File}' holds no '{DarkFlag}', so the lit copy would test nothing (D-1118, T-2).");
        }

        GameMap lit = GameMap.Read(System.Text.Encoding.UTF8.GetBytes(text.Replace(DarkFlag, "\"dark\": false", StringComparison.Ordinal)), dark.File);
        MapState party = MapState.Enter(lit);
        UiBase ui = UiBase.Load(loaded, loaded.Style.SmallBody);
        var drawn = new MapScreen();
        drawn.Build(GameAtlas.Load(loaded.Atlas), ui.Theme, party, loaded, loaded.Effects.Ambient.WeatherOf(lit.Id), loaded.Light.Passes);
        drawn.ShowParty(party, 0, session.Tick, torchHeld: false);
        string sprites = drawn.DescribeSprites(party);
        drawn.QueueFree();
        return sprites;
    }

    private static ContentFile FileOf(IReadOnlyList<ContentFile> files, string path)
    {
        foreach (ContentFile file in files)
        {
            if (string.Equals(file.Path, path, StringComparison.Ordinal))
            {
                return file;
            }
        }

        throw new InvalidOperationException($"The content of this build holds no file '{path}' (T-2).");
    }

    /// <summary>
    /// Draws the party with the torch put away and then held out, and fails when the carried
    /// light or the torch in the hand does not follow (exit test 7 of PR-91, D-1064, D-1066). The
    /// torch stays held out, so the check of the lights reads the carried light too.
    /// </summary>
    /// <exception cref="InvalidOperationException">The light or the torch in the hand draws in the wrong state (T-2).</exception>
    private static string CheckTorchDraw(MapScreen drawn, GameRun session)
    {
        drawn.ShowParty(session.Party, 0, session.Tick, torchHeld: false);
        if (drawn.CarriedLightOn || drawn.LeadHoldsTorch)
        {
            throw new InvalidOperationException(
                $"The torch is put away, and the carried light draws {drawn.CarriedLightOn} and the torch in the hand draws {drawn.LeadHoldsTorch} (D-1064, T-2).");
        }

        drawn.ShowParty(session.Party, 0, session.Tick, torchHeld: true);
        if (!drawn.CarriedLightOn || !drawn.LeadHoldsTorch)
        {
            throw new InvalidOperationException(
                $"The torch is held out, and the carried light draws {drawn.CarriedLightOn} and the torch in the hand draws {drawn.LeadHoldsTorch} (D-1066, T-2).");
        }

        return "the carried light and the torch in the hand follow the torch";
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
        drawn.ShowParty(walked.Party, 0, walked.Tick, walked.TorchHeld);
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
        GameInputMap.Build(SmokeSettings().Controls);
        FrameRoot built = FrameRoot.AddTo(this);
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
    /// <param name="saves">The new folder with no save that the wipe reads, never the folder of the person (D-1114).</param>
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
    private string DescribeBattle(ContentSet loaded, GameRun session, SaveStore saves)
    {
        GameRun open = session;
        string outcome = "none";
        int frame = 0;
        for (; frame < SmokeBattleFrames && !open.InBattle; frame += 1)
        {
            open.Queue(Intent.OfPlayer(BattleWalk.StepOf(open.Party)));
            this.WriteLog(open.Advance(SmokeFrameSeconds));
        }

        FrameRoot built = FrameRoot.AddTo(this);
        UiBase shownBase = UiBase.Load(loaded, loaded.Style.SmallBody);
        var pause = new PauseView(built, shownBase);
        string paused = "no pause";
        string itemLine = "no item line";
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
                open = this.ReloadRun(loaded, saves, open, open.MessageSpeed);
                break;
            }

            if (open.BattleView is not null)
            {
                if (screen is null)
                {
                    screen = BattleScreen.Build(built, shownBase, loaded, open, new CommandMemory(SmokeSettings().Battle.RememberCursor), SmokeSettings().Access.Effects);
                    nodes = $"{screen.CombatantCount} combatants over {screen.BackdropCopies} backdrop copies in {screen.CheckLights()} lights (the key light and the light of a spell), with {screen.CheckBursts()} particle nodes";
                }

                screen.Show(open);
            }

            // The pause of D-1083 runs once, in the middle of the playback of the first event.
            if (screen is not null && open.PlayingEvent is not null && string.CompareOrdinal(paused, "no pause") == 0)
            {
                paused = this.CheckFightPause(open, screen, pause, built);
            }

            // The item line of D-1093 runs once, at the first command gate that offers an item.
            if (screen?.Commands is BattleCommands offered && offered.Allows(BattleAction.Item) && string.CompareOrdinal(itemLine, "no item line") == 0)
            {
                itemLine = CheckItemDescription(screen, open, loaded);
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
        if (string.CompareOrdinal(outcome, "none") == 0
            || open.InBattle
            || commands == 0
            || string.CompareOrdinal(paused, "no pause") == 0
            || string.CompareOrdinal(itemLine, "no item line") == 0)
        {
            throw new InvalidOperationException(
                $"The smoke battle reached no end in {SmokeBattleFrames} frames: the outcome is '{outcome}', the menu sent "
                + $"{commands} commands, the party is at {open.Party.LeadAt}, the pause is '{paused}', and the item line is "
                + $"'{itemLine}' (D-767, D-827, D-1083, D-1093, T-2).");
        }

        return $"'{outcome}' after {frame} frames and {commands} commands of the menu, with {nodes}, the pause {paused}, "
            + $"the item line {itemLine}, and the run is at tick {open.Tick} with the party at {open.Party.LeadAt}";
    }

    /// <summary>
    /// Opens the item list of the command menu, and reads the line of the item under the cursor
    /// in the message box above it (D-1093). The check then closes the list and puts the cursor
    /// back on the attack, so the next press of the smoke fight attacks as before.
    /// </summary>
    /// <param name="screen">The battle screen, whose command menu offers an item.</param>
    /// <param name="open">The run of the fight.</param>
    /// <param name="loaded">The content set, whose string table holds the line of the item.</param>
    /// <returns>The string id of the line that the box showed.</returns>
    /// <exception cref="InvalidOperationException">The box showed another line, or kept the line after the list closed (T-2).</exception>
    private static string CheckItemDescription(BattleScreen screen, GameRun open, ContentSet loaded)
    {
        BattleCommands commands = screen.Commands ?? throw new InvalidOperationException(
            $"The item line check at tick {open.Tick} found no command menu (T-2).");
        MoveCursorTo(commands, BattleAction.Item);
        screen.Read(InputActions.Confirm);
        screen.Show(open);
        if (commands.Stage != CommandStage.Item)
        {
            throw new InvalidOperationException($"The item action opened the stage '{commands.Stage}' at tick {open.Tick} (D-1093, T-2).");
        }

        ContentId wanted = commands.Items[commands.Cursor].Id;
        if (screen.ShownDescription is not ContentId shown || string.CompareOrdinal(shown.Value, wanted.Value) != 0)
        {
            throw new InvalidOperationException(
                $"The item list showed the line '{screen.ShownDescription?.Value ?? "of the last event"}' above it, and the item under the cursor is '{wanted.Value}' (D-1093, T-2).");
        }

        // The string table holds the line, so a missing line fails here and not in play (G-7).
        _ = loaded.Strings.Text(wanted);
        screen.Read(InputActions.Cancel);
        screen.Show(open);
        if (screen.ShownDescription is not null)
        {
            throw new InvalidOperationException($"The line of the item stayed after the item list closed at tick {open.Tick} (D-1027, T-2).");
        }

        MoveCursorTo(commands, BattleAction.Attack);
        return $"'{wanted.Value}'";
    }

    /// <summary>Moves the cursor of the action stage to one action.</summary>
    private static void MoveCursorTo(BattleCommands commands, BattleAction action)
    {
        for (int step = 0; step < BattleCommands.Actions.Count && BattleCommands.Actions[commands.Cursor] != action; step += 1)
        {
            commands.Move(1);
        }
    }

    /// <summary>
    /// Pauses the smoke fight in the middle of the playback of an event, and reads the rules of
    /// D-1083: the pause shows above the hand-off, it holds the fight, it opens no command menu,
    /// and the back action ends it.
    /// </summary>
    /// <param name="open">The run, which plays an event of the fight.</param>
    /// <param name="screen">The battle screen of the fight.</param>
    /// <param name="pause">The pause view on the frame.</param>
    /// <param name="built">The frame of the fight.</param>
    /// <returns>What the pause did.</returns>
    /// <exception cref="InvalidOperationException">The pause broke a rule of D-1083 (T-2).</exception>
    /// <remarks>
    /// Before D-1083 the playback ran on under the menu of the rules, the command menu opened,
    /// and the next command crashed the session, so this check fails on that build (T-3).
    /// </remarks>
    private string CheckFightPause(GameRun open, BattleScreen screen, PauseView pause, FrameRoot built)
    {
        Intent start = open.PauseIntentOf(InputActions.Menu) ?? throw new InvalidOperationException(
            $"The menu action made no pause at tick {open.Tick} of the smoke fight (D-1083, T-2).");
        open.Queue(start);
        this.WriteLog(open.Advance(SmokeFrameSeconds));
        long held = open.FightTick;
        for (int frame = 0; frame < SmokePauseFrames; frame += 1)
        {
            this.WriteLog(open.Advance(SmokeFrameSeconds));
            screen.Show(open);
            pause.Show(open.FightPaused);
            if (!pause.Shown || screen.Commands is not null || open.TakesBattleCommand || open.FightTick != held)
            {
                throw new InvalidOperationException(
                    $"The pause of the smoke fight broke at frame {frame}: shown {pause.Shown}, command menu {screen.Commands is not null}, "
                    + $"gate {open.TakesBattleCommand}, and the fight tick {open.FightTick} after {held} (D-1083, T-2).");
            }
        }

        // The top layer is the last child of the frame, so it draws above the pass of the hand-off.
        Node top = built.Top;
        if (top.GetIndex() != top.GetParent().GetChildCount() - 1)
        {
            throw new InvalidOperationException(
                $"The top layer of the frame is child {top.GetIndex()} of {top.GetParent().GetChildCount()}, and it must draw last (D-1083, T-2).");
        }

        Intent end = open.PauseIntentOf(InputActions.Cancel) ?? throw new InvalidOperationException(
            $"The back action ended no pause at tick {open.Tick} of the smoke fight (D-1083, T-2).");
        open.Queue(end);
        this.WriteLog(open.Advance(SmokeFrameSeconds));
        pause.Show(open.FightPaused);
        if (pause.Shown)
        {
            throw new InvalidOperationException($"The back action left the pause on screen at tick {open.Tick} (D-1083, T-2).");
        }

        return $"held the fight tick at {held} for {SmokePauseFrames} frames above the hand-off, and the back action ended it";
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
