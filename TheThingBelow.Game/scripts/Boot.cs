using System;
using System.Collections.Generic;
using System.IO;
using Godot;
using TheThingBelow.Core;
using TheThingBelow.Core.Content;
using TheThingBelow.Core.Crashes;
using TheThingBelow.Core.Logging;
using TheThingBelow.Core.Runs;
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
/// From PR-44 to PR-61 a crash shows no message on screen. PR-61 adds the message and the
/// address of D-473 through the one text helper of D-499 (D-559).
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

    private LogStore? log;
    private GameRun? run;

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
            this.WriteLog(this.run.Advance(delta));
        }
        catch (Exception fault)
        {
            this.ReportCrash(fault);
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

        this.run = GameRun.Start(LoadContent(), FixtureSeed);
        GD.Print("The Thing Below: the scaffold booted. No screen exists yet (PR-61).");
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

        try
        {
            DateTime time = DateTime.UtcNow;
            string path = CrashStore.OfThisSystem().Write(fault, stopped?.Record(), time);
            GD.PrintErr($"the game stopped with an error, and it wrote the crash file '{path}'.");
            this.log?.Write([CrashEntry(fault, Path.GetFileName(path), stopped)], time);
        }
        catch (Exception second)
        {
            // The report of the second error never hides the first one (T-2, G-18).
            GD.PrintErr($"the game stopped with this error: {fault}");
            GD.PrintErr($"the game could not write the crash file of that error: {second}");
        }

        GetTree().Quit(CrashExitCode);
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
        GD.Print($"smoke: the content is {DescribeContent()}.");

        GameRun session = GameRun.Start(LoadContent(), FixtureSeed);
        GD.Print($"smoke: the run is {this.DescribeRun(session)}.");
        GD.Print($"smoke: the log is {this.DescribeLog()}.");
        GD.Print($"smoke: the crash file is {DescribeCrashFile(session)}.");
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
    /// Loads every content file from the resources of this assembly, and gives the count and
    /// the content hash. The session thus proves the embed of D-508 inside the engine, where
    /// the match test of Tests reads the assembly file alone (F-42).
    /// </summary>
    /// <returns>The number of files and the content hash, as one line.</returns>
    private static string DescribeContent()
    {
        IReadOnlyList<ContentFile> files = EmbeddedContent.Read();
        ContentSet set = ContentSet.Load(files);
        return $"{files.Count} files with the hash {set.Hash}";
    }

    /// <summary>Loads the content of this build from the resources of this assembly (D-508).</summary>
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
    private static string DescribeCrashFile(GameRun session)
    {
        CrashStore crashes = CrashStore.OfThisSystem();
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
