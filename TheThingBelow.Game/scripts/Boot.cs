using System;
using System.Collections.Generic;
using System.IO;
using Godot;
using TheThingBelow.Core;
using TheThingBelow.Core.Content;
using TheThingBelow.Core.Crashes;
using TheThingBelow.Core.Logging;
using TheThingBelow.Core.Maps;
using TheThingBelow.Core.Runs;
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

    /// <summary>The exit code of a session that ends with no error (T-2).</summary>
    private const int SuccessExitCode = 0;

    /// <summary>The exit code of a session that a crash ended (D-170, T-2).</summary>
    private const int CrashExitCode = 1;

    /// <summary>
    /// The seed of the run of this build. The title screen of PR-33 and the load of a save
    /// in PR-16 pick the seed of a real run, and this constant stands until then (D-258, G-3).
    /// </summary>
    private const ulong FixtureSeed = 20260918;

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

    /// <summary>The line that the smoke session types in the console of a development build (D-724).</summary>
    private const string SmokeConsoleLine = "help";

    /// <summary>The name that Godot gives the display server of a session with no window.</summary>
    private const string HeadlessDisplay = "headless";

    /// <summary>
    /// The key that opens the debug console and closes it, in a development build alone
    /// (D-171, D-725). The key sits outside the input map, so it makes no intent and the
    /// remap of PR-63 never reaches it (D-214, F-50).
    /// </summary>
    private const Key ConsoleKey = Key.Quoteleft;

    private LogStore? log;
    private GameRun? run;
    private ContentSet? content;
    private UiBase? ui;
    private FrameRoot? frame;
    private PromptBar? prompts;
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
        // the refusal of the rules (D-162, T-2).
        if (open is null || open.MenuOpenNextTick)
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

        ContentSet loaded = LoadContent();
        this.content = loaded;
        this.run = GameRun.Start(loaded, FixtureSeed, DebugSeam.Handlers());
        this.BuildScreen(loaded);
    }

    /// <summary>
    /// Builds the frame, the UI base, the map, and the row of button prompts (D-524, D-561,
    /// D-568, D-722).
    /// </summary>
    /// <param name="loaded">The content set of this build.</param>
    /// <remarks>
    /// The map draws in the world viewport at 1x, and the frame shows that viewport at 2x
    /// (D-633, D-634). The prompts draw on the frame layer, so the text of a prompt matches
    /// the art pixel of the frame (D-230).
    /// <para>
    /// The body size comes from the fit of the frame on this screen, and no setting exists
    /// yet. PR-63 adds the display setting that changes it (D-707).
    /// </para>
    /// </remarks>
    private void BuildScreen(ContentSet loaded)
    {
        GameInputMap.Build();

        var built = new FrameRoot();
        this.AddChild(built);
        this.frame = built;

        UiStyle style = loaded.Style;
        int body = BodySize.DefaultFor(built.Fit.Height, style.SmallBody, style.LargeBody);
        UiBase built_ui = UiBase.Load(loaded, body);
        this.ui = built_ui;

        GameRun open = this.run ?? throw new InvalidOperationException(
            $"The screen built before the run started (T-2).");

        var drawn = new MapScreen();
        built.World.AddChild(drawn);
        drawn.Build(built_ui.Atlas, open.Party.Map);
        drawn.ShowParty(open.Party);
        this.map = drawn;

        var row = new PromptBar
        {
            Position = new Vector2(UiMetrics.EdgePixels, ScreenFit.FrameHeight - UiMetrics.EdgePixels - body),
        };
        built.Layer.AddChild(row);
        row.Build(built_ui);
        this.prompts = row;

        this.BuildConsole(built, open);
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
    /// Reads the key that opens the debug console and closes it, before any node of the frame
    /// reads it (D-171, D-725).
    /// </summary>
    /// <param name="signal">Every input event of this frame.</param>
    /// <remarks>
    /// The entry of the console takes every key while the console is open, so a method that
    /// ran after the nodes of the frame would never see the key that closes it. Thus this
    /// method takes the key here and marks the event as handled, and no other node reads it.
    /// </remarks>
    public override void _Input(InputEvent signal)
    {
        if (this.crashed || this.console is null || signal is not InputEventKey key)
        {
            return;
        }

        if (!key.Pressed || key.Echo || key.Keycode != ConsoleKey)
        {
            return;
        }

        try
        {
            this.ToggleConsole(this.console);
        }
        catch (Exception fault)
        {
            this.ReportCrash(fault);
        }

        GetViewport().SetInputAsHandled();
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
    /// Reads one input event. The event sets the device of the prompts, and it makes at most
    /// one intent (D-222, D-493, F-50).
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

    /// <summary>Sets the glyph set of the prompts, and makes the intent of one action.</summary>
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

        if (this.ui is not null && this.ui.Device.Read(signal) && this.prompts is not null)
        {
            this.prompts.DrawPrompts();
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
        var drawn = new MapScreen();
        drawn.Build(GameAtlas.Load(loaded.Atlas), map);
        drawn.ShowParty(session.Party);

        CameraPlace view = MapCamera.Of(session.Party, FrameRoot.WorldWidth, FrameRoot.WorldHeight);
        string ground = drawn.DescribeGround();
        drawn.QueueFree();
        return $"'{map.Id.Value}' at {map.Width} by {map.Height} tiles, "
            + $"the lead at {session.Party.LeadAt}, the view at ({view.X}, {view.Y}), and {ground}";
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

        if (!DebugSeam.TryBuildConsole(() => session.State, session.Queue, out Control? made) || made is null)
        {
            throw new InvalidOperationException(
                "This build has the feature of a development build, and it built no debug console (D-723, T-2).");
        }

        // The console takes the focus when it opens, so every key of the person reaches its
        // entry and the game makes no intent (D-725). A node outside the tree can hold no
        // focus, so the check adds the console to the tree and then takes it away again.
        // The console takes the focus when it opens, and it reads a typed line from the signal
        // of its entry. The console owns those nodes, so the check of both lives behind the
        // seam and it fails with its own message (D-723, D-725, T-2). A node outside the tree
        // can hold no focus, so the check adds the console and then takes it away again.
        this.AddChild(made);
        made.Visible = true;
        int shown = DebugSeam.SubmitLine(made, SmokeConsoleLine).Count;
        made.Visible = false;
        this.RemoveChild(made);
        made.QueueFree();

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
