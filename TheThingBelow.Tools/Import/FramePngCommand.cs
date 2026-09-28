using System;
using System.Collections.Generic;
using System.IO;
using TheThingBelow.Core.Content;
using TheThingBelow.Tools.Atlas;
using TheThingBelow.Tools.Png;

namespace TheThingBelow.Tools.Import;

/// <summary>
/// The `frame-png` command. It writes one frame of a drawing file at 1x to an RGBA PNG, the
/// frame PNG, which the owner edits by hand and imports again (D-107, D-1313).
/// </summary>
/// <remarks>
/// A transparent key gives a pixel of alpha 0, so the hand-edit mode reads it back as the
/// transparent key (D-1315). The word export names a build of the Game (D-481), so the
/// command takes another name.
/// </remarks>
public static class FramePngCommand
{
    /// <summary>The name of the command on the command line.</summary>
    public const string Name = "frame-png";

    /// <summary>The option that names the root of the checkout, which holds the palette.</summary>
    public const string RootOption = "--root";

    /// <summary>The option that names the drawing file that holds the frame.</summary>
    public const string DrawingOption = "--drawing";

    /// <summary>The option that names the position of the frame, which starts at 0.</summary>
    public const string FrameOption = "--frame";

    /// <summary>The option that names the PNG to write.</summary>
    public const string OutOption = "--out";

    /// <summary>Writes one frame of a drawing file to a PNG.</summary>
    /// <param name="args">The arguments after the command name.</param>
    /// <param name="output">The writer that takes the line of the report.</param>
    /// <param name="errors">The writer that takes each fault.</param>
    /// <returns>0 when the command wrote the PNG, and 1 on any fault.</returns>
    public static int Run(IReadOnlyList<string> args, TextWriter output, TextWriter errors)
    {
        ArgumentNullException.ThrowIfNull(args);
        ArgumentNullException.ThrowIfNull(output);
        ArgumentNullException.ThrowIfNull(errors);

        OptionParser? options = OptionParser.Read(
            Name, args, [RootOption, DrawingOption, FrameOption, OutOption], [], errors);
        if (options is null)
        {
            return Program.FaultExitCode;
        }

        string root = options.ValueOr(RootOption, ".");
        string? drawing = options.Value(DrawingOption);
        string? frameText = options.Value(FrameOption);
        string? png = options.Value(OutOption);
        if (drawing is null || frameText is null || png is null)
        {
            errors.WriteLine($"Error: {Name} takes {DrawingOption} <file>, {FrameOption} <number>, and {OutOption} <file> (D-1313).");
            return Program.FaultExitCode;
        }

        if (!FrameTarget.TryParseFrame(frameText, out int frame))
        {
            errors.WriteLine($"Error: the frame '{frameText}' is not a whole number of 0 or more.");
            return Program.FaultExitCode;
        }

        try
        {
            FrameTarget target = FrameTarget.Read(drawing, frame);
            Palette palette = FrameTarget.ReadPalette(root);
            var canvas = new AtlasCanvas(target.Drawing.Width, target.Drawing.Height);
            canvas.Draw(target.Drawing, frame, palette, 0, 0, 1);
            WriteNewFile(png, PngWriter.Write(canvas.ToImage()));
            output.WriteLine(
                $"{Name}: wrote frame {frame} of {drawing} to {png}, {target.Drawing.Width} by {target.Drawing.Height} pixels. Save an edit as RGB or RGBA (D-176).");
            return 0;
        }
        catch (Exception fault) when (
            fault is IOException or UnauthorizedAccessException or ContentException
                or PngException or ImportException or InvalidOperationException)
        {
            errors.WriteLine($"Error: {Name} wrote nothing: {fault.Message}");
            return Program.FaultExitCode;
        }
    }

    // The command never writes over a file. A path, a symbolic link, or a hard link can each
    // name the drawing file, and no compare of paths finds every alias. The mode CreateNew
    // makes the system refuse any name that exists, so a write never reaches a drawing (T-2).
    private static void WriteNewFile(string png, byte[] bytes)
    {
        if (File.Exists(png) || new FileInfo(png).LinkTarget is not null)
        {
            throw ImportException.For(
                png,
                $"the output already exists, and {Name} never writes over a file. Remove it, or name another {OutOption} file");
        }

        using var stream = new FileStream(png, FileMode.CreateNew, FileAccess.Write);
        stream.Write(bytes);
    }
}
