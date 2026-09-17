using System;
using Godot;

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

    /// <summary>Reads the arguments and picks the session.</summary>
    public override void _Ready()
    {
        string[] userArguments = OS.GetCmdlineUserArgs();
        if (Array.IndexOf(userArguments, SmokeArgument) >= 0)
        {
            RunSmokeSession();
            return;
        }

        GD.Print("The Thing Below: the scaffold booted. No screen exists yet (PR-61).");
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
        GD.Print("smoke: the session ends with no error.");
        GetTree().Quit(SuccessExitCode);
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
