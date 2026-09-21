using System;
using System.Collections.Generic;
using System.IO;
using TheThingBelow.Core.Content;
using TheThingBelow.Tools.Content;
using TheThingBelow.Tools.Png;

namespace TheThingBelow.Tools.Pictures;

/// <summary>
/// The `picture` command: it renders each large picture of a checkout as a PNG, for a review
/// sheet or a store image (D-514, D-518). No render enters git.
/// </summary>
public static class PictureCommand
{
    /// <summary>The name of the command.</summary>
    public const string Name = "picture";

    /// <summary>The option that names the root of the checkout.</summary>
    public const string RootOption = "--root";

    /// <summary>The option that names the folder that takes one PNG for each picture.</summary>
    public const string OutOption = "--out";

    /// <summary>Renders each large picture of the content set into the output folder.</summary>
    /// <param name="args">The arguments after the command name.</param>
    /// <param name="output">The writer that takes one line for each file.</param>
    /// <param name="errors">The writer that takes each fault.</param>
    /// <returns>0 when each picture renders, and 1 on any fault.</returns>
    public static int Run(IReadOnlyList<string> args, TextWriter output, TextWriter errors)
    {
        ArgumentNullException.ThrowIfNull(args);
        ArgumentNullException.ThrowIfNull(output);
        ArgumentNullException.ThrowIfNull(errors);

        OptionParser? options = OptionParser.Read(Name, args, [RootOption, OutOption], [], errors);
        if (options is null)
        {
            return Program.FaultExitCode;
        }

        string root = options.ValueOr(RootOption, ".");
        string? folder = options.Value(OutOption);
        if (folder is null)
        {
            errors.WriteLine($"Error: {Name} takes {OutOption} <folder>, the folder of the renders (T-2).");
            return Program.FaultExitCode;
        }

        try
        {
            return Write(root, folder, output, errors);
        }
        catch (Exception fault) when (
            fault is IOException or UnauthorizedAccessException or ContentException
                or PngException or InvalidOperationException)
        {
            errors.WriteLine($"Error: {Name} stopped on the root '{root}': {fault.Message}");
            return Program.FaultExitCode;
        }
    }

    private static int Write(string root, string folder, TextWriter output, TextWriter errors)
    {
        ContentSet content = ContentSet.Load(ContentFolder.Read(root));
        Directory.CreateDirectory(folder);

        int count = 0;
        foreach (LargePicture picture in content.Pictures)
        {
            string path = Path.Combine(folder, $"{picture.Id.Name}.png");
            PngWriter.WriteFile(path, PictureRender.Render(picture, content));
            output.WriteLine($"{Name}: wrote {path}, {picture.Width} by {picture.Height} pixels.");
            count += 1;
        }

        if (count == 0)
        {
            // A run that writes nothing reads as a pass, so the command fails (T-2).
            errors.WriteLine($"Error: {Name} found no large picture under '{LargePicture.Folder}' of the root '{root}' (T-2).");
            return Program.FaultExitCode;
        }

        output.WriteLine($"{Name}: pictures {count}.");
        return 0;
    }
}
