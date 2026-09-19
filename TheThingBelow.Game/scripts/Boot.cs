using System;
using System.Collections.Generic;
using Godot;
using TheThingBelow.Core.Content;
using TheThingBelow.Core.Runs;
using TheThingBelow.Storage;

namespace TheThingBelow.Game;

/// <summary>
/// The first node of the game. It reads the arguments of the session and starts the run,
/// or it runs the smoke session and quits (D-117).
/// </summary>
public partial class Boot : Node
{
    /// <summary>The argument that asks for the smoke session of the CI gate (D-117).</summary>
    public const string SmokeArgument = "--smoke";

    /// <summary>The exit code of a session that ends with no error (T-2).</summary>
    private const int SuccessExitCode = 0;

    /// <summary>
    /// The seed of the run of this build. The title screen of PR-33 and the load of a save
    /// in PR-16 pick the seed of a real run, and this constant stands until then (D-258, G-3).
    /// </summary>
    private const ulong FixtureSeed = 20260918;

    /// <summary>The count of frames that the smoke session runs, at one frame of 1/60 second.</summary>
    private const int SmokeFrameCount = 120;

    /// <summary>The time of one frame of the smoke session, in seconds (D-164).</summary>
    private const double SmokeFrameSeconds = 1.0 / FixedStepLoop.TicksPerSecond;

    private GameRun? run;

    /// <summary>Reads the arguments and picks the session.</summary>
    public override void _Ready()
    {
        CheckSaveFolder();

        string[] userArguments = OS.GetCmdlineUserArgs();
        if (Array.IndexOf(userArguments, SmokeArgument) >= 0)
        {
            RunSmokeSession();
            return;
        }

        this.run = GameRun.Start(LoadContent(), FixtureSeed);
        GD.Print("The Thing Below: the scaffold booted. No screen exists yet (PR-61).");
    }

    /// <summary>
    /// Runs the ticks of one frame (D-164). The frame time comes from the engine, and the
    /// fixed-step loop turns it into whole ticks, so no Godot timer or physics step reaches
    /// the simulation (D-100, G-23).
    /// </summary>
    /// <param name="delta">The time of the frame, in seconds.</param>
    public override void _Process(double delta)
    {
        this.run?.Advance(delta);
    }

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
        GD.Print($"smoke: the run is {DescribeRun()}.");
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
    /// Steps a run through the fixed-step loop, and gives the tick, the count of lines of
    /// the record, and the state hash. The session thus reads the loop of D-164 and the
    /// record of G-5 inside the engine.
    /// </summary>
    /// <returns>The tick, the count of lines, and the state hash, as one line.</returns>
    private static string DescribeRun()
    {
        GameRun run = GameRun.Start(LoadContent(), FixtureSeed);
        for (int frame = 0; frame < SmokeFrameCount; frame += 1)
        {
            run.Advance(SmokeFrameSeconds);
        }

        return $"tick {run.Tick} with {run.RecordedLines} recorded lines and the state hash 0x{run.StateHash():x16}";
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
