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
            byte[] bytes = PngWriter.Write(canvas.ToImage());
            WriteNewFile(png, stream => stream.Write(bytes));
            output.WriteLine(
                $"{Name}: wrote frame {frame} of {drawing} to {png}, {target.Drawing.Width} by {target.Drawing.Height} pixels. Save an edit as RGB or RGBA (D-176).");
            return 0;
        }
        catch (Exception fault) when (
            fault is IOException or UnauthorizedAccessException or ContentException
                or PngException or ImportException or InvalidOperationException)
        {
            errors.WriteLine($"Error: {Name} stopped: {fault.Message}");
            return Program.FaultExitCode;
        }
    }

    /// <summary>
    /// Writes a new file, and never writes over a file. A path, a symbolic link, or a hard
    /// link can each name a drawing file, and no compare of paths finds every alias. The mode
    /// CreateNew makes the system refuse any name that exists, so a write never reaches a
    /// drawing (T-2).
    /// </summary>
    /// <param name="path">The path of the new file.</param>
    /// <param name="write">The action that writes the bytes into the stream of the new file.</param>
    /// <exception cref="ImportException">
    /// The path names a file or a link, or the write failed. After a failed write the file
    /// that this call made is gone, or the message names it (T-2).
    /// </exception>
    public static void WriteNewFile(string path, Action<Stream> write)
    {
        ArgumentException.ThrowIfNullOrEmpty(path);
        ArgumentNullException.ThrowIfNull(write);

        if (File.Exists(path) || new FileInfo(path).LinkTarget is not null)
        {
            throw ImportException.For(
                path,
                $"the output already exists, and {Name} never writes over a file. Remove it, or name another {OutOption} file");
        }

        var stream = new FileStream(path, FileMode.CreateNew, FileAccess.Write);
        try
        {
            using (stream)
            {
                write(stream);
            }
        }
        catch (Exception fault) when (fault is IOException or UnauthorizedAccessException)
        {
            RemovePart(path, fault);
            throw ImportException.For(path, $"the write failed, and the command removed the part that it wrote. {fault.Message}");
        }
    }

    // This call made the file, so its removal takes nothing that was there before. A removal
    // that fails leaves a part, and the message names it so the next run does not refuse the
    // path with no reason (T-2).
    private static void RemovePart(string path, Exception writeFault)
    {
        try
        {
            File.Delete(path);
        }
        catch (Exception removeFault) when (removeFault is IOException or UnauthorizedAccessException)
        {
            throw ImportException.For(
                path,
                $"the write failed, and a part of the file stays because its removal failed too. Remove it by hand. The write: {writeFault.Message} The removal: {removeFault.Message}");
        }
    }
}
