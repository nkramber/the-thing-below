using System;
using System.Collections.Generic;
using System.IO;
using TheThingBelow.Core.Content;
using TheThingBelow.Tools.Png;

namespace TheThingBelow.Tools.Import;

/// <summary>
/// The `import` command. It reads a PNG and replaces one frame of an existing drawing file
/// with its pixels (D-107, D-688, D-1311). The hand-edit mode reads a frame PNG that the
/// owner edited, and the generator mode reads a picture of the Sprite Fusion generator
/// (D-686).
/// </summary>
/// <remarks>
/// The command writes the drawing file alone. The `atlas` command stays the one writer of the
/// atlas, and the command names it after each write (D-1316).
/// </remarks>
public static class ImportCommand
{
    /// <summary>The name of the command on the command line.</summary>
    public const string Name = "import";

    /// <summary>The option that names the root of the checkout, which holds the palette.</summary>
    public const string RootOption = "--root";

    /// <summary>The option that names the mode: <see cref="HandEditMode"/> or <see cref="GeneratorMode"/>.</summary>
    public const string ModeOption = "--mode";

    /// <summary>The option that names the PNG to read.</summary>
    public const string PngOption = "--png";

    /// <summary>The option that names the drawing file that takes the frame.</summary>
    public const string DrawingOption = "--drawing";

    /// <summary>The option that names the position of the frame, which starts at 0.</summary>
    public const string FrameOption = "--frame";

    /// <summary>The mode that reads a frame PNG that the owner edited by hand (D-688).</summary>
    public const string HandEditMode = "hand-edit";

    /// <summary>The mode that reads a picture of the Sprite Fusion generator (D-688).</summary>
    public const string GeneratorMode = "generator";

    /// <summary>Replaces one frame of a drawing file with the pixels of a PNG.</summary>
    /// <param name="args">The arguments after the command name.</param>
    /// <param name="output">The writer that takes each line of the report.</param>
    /// <param name="errors">The writer that takes each fault.</param>
    /// <returns>0 when the command wrote the frame, and 1 on any fault.</returns>
    public static int Run(IReadOnlyList<string> args, TextWriter output, TextWriter errors)
    {
        ArgumentNullException.ThrowIfNull(args);
        ArgumentNullException.ThrowIfNull(output);
        ArgumentNullException.ThrowIfNull(errors);

        OptionParser? options = OptionParser.Read(
            Name, args, [RootOption, ModeOption, PngOption, DrawingOption, FrameOption], [], errors);
        if (options is null)
        {
            return Program.FaultExitCode;
        }

        string root = options.ValueOr(RootOption, ".");
        string? mode = options.Value(ModeOption);
        string? png = options.Value(PngOption);
        string? drawing = options.Value(DrawingOption);
        string? frameText = options.Value(FrameOption);
        if (mode is null || png is null || drawing is null || frameText is null)
        {
            errors.WriteLine(
                $"Error: {Name} takes {ModeOption} {HandEditMode}|{GeneratorMode}, {PngOption} <file>, {DrawingOption} <file>, and {FrameOption} <number> (D-1311).");
            return Program.FaultExitCode;
        }

        if (mode != HandEditMode && mode != GeneratorMode)
        {
            errors.WriteLine($"Error: the mode '{mode}' is unknown. {Name} takes {HandEditMode} or {GeneratorMode} (D-688).");
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
            var palette = new PaletteMatch(FrameTarget.ReadPalette(root));
            PngImage image = PngReader.ReadFile(png);
            IReadOnlyList<string> rows = mode == HandEditMode
                ? FrameImport.FromHandEdit(image, png, target.Drawing, palette)
                : Generate(image, png, target.Drawing, palette, output);

            int changed = CountChanged(target.Drawing.Frames[frame].Rows, rows);
            File.WriteAllBytes(drawing, DrawingFrameText.ReplaceRows(target.Bytes, drawing, frame, rows));
            output.WriteLine(
                $"{Name}: wrote frame {frame} of {drawing} from {png} in the {mode} mode, and {changed} pixels changed.");
            output.WriteLine(
                $"{Name}: build the atlas again with `dotnet run --project TheThingBelow.Tools/TheThingBelow.Tools.csproj -- atlas --root {root}` (D-1316).");
            return 0;
        }
        catch (Exception fault) when (
            fault is IOException or UnauthorizedAccessException or ContentException
                or PngException or ImportException)
        {
            errors.WriteLine($"Error: {Name} wrote nothing: {fault.Message}");
            return Program.FaultExitCode;
        }
    }

    private static IReadOnlyList<string> Generate(
        PngImage image,
        string png,
        Drawing drawing,
        PaletteMatch palette,
        TextWriter output)
    {
        GeneratorFrame result = FrameImport.FromGenerator(image, png, drawing, palette);
        ContentBox content = result.Content;
        output.WriteLine(
            $"{Name}: the content of {png} is {content.Width} by {content.Height} pixels at x {content.Left}, y {content.Top}, and it sits at x {result.FrameLeft}, y {result.FrameTop} of the frame of {drawing.Width} by {drawing.Height} (D-1312).");

        // The count makes each map visible, so no map is silent (D-688, T-2).
        output.WriteLine(
            $"{Name}: mapped {result.Mapped} of {result.Opaque} opaque pixels to the nearest color of the palette (D-688, D-1314).");
        return result.Rows;
    }

    private static int CountChanged(IReadOnlyList<string> before, IReadOnlyList<string> after)
    {
        int changed = 0;
        for (int row = 0; row < before.Count; row += 1)
        {
            for (int column = 0; column < before[row].Length; column += 1)
            {
                if (before[row][column] != after[row][column])
                {
                    changed += 1;
                }
            }
        }

        return changed;
    }
}
