using System;
using System.Collections.Generic;
using System.IO;
using System.Text;
using TheThingBelow.Core.Content;
using TheThingBelow.Tools.Content;
using TheThingBelow.Tools.Png;

namespace TheThingBelow.Tools.Atlas;

/// <summary>
/// The `atlas` command. It reads the palette and every drawing file of `content/`, packs the
/// frames into the pages of the atlas, and writes each page and the atlas index (D-107,
/// D-666, D-667). It replaces the interim script of D-406.
/// </summary>
/// <remarks>
/// The command reads the drawing files itself, and not through <see cref="ContentSet"/>,
/// because a content set needs the atlas index that this command writes.
/// <para>
/// The `--check` option compares the committed pages by decoded pixels and never by bytes,
/// because the compressed bytes of a PNG depend on the encoder (F-19).
/// </para>
/// </remarks>
public static class AtlasCommand
{
    /// <summary>The name of the command on the command line.</summary>
    public const string Name = "atlas";

    /// <summary>The option that names the root of the checkout.</summary>
    public const string RootOption = "--root";

    /// <summary>The option that compares the committed atlas and writes no file.</summary>
    public const string CheckOption = "--check";

    /// <summary>The option that names the folder for the swatch sheet and the review sheets.</summary>
    public const string SheetsOption = "--sheets";

    /// <summary>Writes the atlas, or compares the committed atlas with the drawing files.</summary>
    /// <param name="args">The arguments after the command name.</param>
    /// <param name="output">The writer that takes each line of the report.</param>
    /// <param name="errors">The writer that takes each fault.</param>
    /// <returns>0 when the run holds, and 1 on any fault.</returns>
    public static int Run(IReadOnlyList<string> args, TextWriter output, TextWriter errors)
    {
        ArgumentNullException.ThrowIfNull(args);
        ArgumentNullException.ThrowIfNull(output);
        ArgumentNullException.ThrowIfNull(errors);

        string root = ".";
        string? sheets = null;
        bool check = false;
        for (int index = 0; index < args.Count; index += 1)
        {
            string option = args[index];
            if (option == CheckOption)
            {
                check = true;
                continue;
            }

            if (option != RootOption && option != SheetsOption)
            {
                errors.WriteLine(
                    $"Error: the option '{option}' is unknown. {Name} takes {RootOption} <path>, {CheckOption}, and {SheetsOption} <folder>.");
                return Program.FaultExitCode;
            }

            if (index + 1 >= args.Count)
            {
                errors.WriteLine($"Error: the option {option} needs a value after it.");
                return Program.FaultExitCode;
            }

            // An empty value is a fault of the command line, and it reads here. A path check
            // further down would throw `ArgumentException`, which no message of this command
            // catches, and the process would end with a stack trace (T-2).
            string value = args[index + 1];
            if (value.Length == 0)
            {
                errors.WriteLine($"Error: the value of the option {option} is empty.");
                return Program.FaultExitCode;
            }

            if (option == RootOption)
            {
                root = value;
            }
            else
            {
                sheets = value;
            }

            index += 1;
        }

        try
        {
            return Build(root, sheets, check, output, errors);
        }
        catch (Exception fault) when (
            fault is IOException or UnauthorizedAccessException or ContentException
                or PngException or InvalidOperationException)
        {
            errors.WriteLine($"Error: {Name} stopped on the root '{root}': {fault.Message}");
            return Program.FaultExitCode;
        }
    }

    /// <summary>Reads the palette and every drawing file of a checkout.</summary>
    /// <param name="root">The root of the checkout, which holds the `content` folder.</param>
    /// <param name="drawings">Every drawing, in ordinal order of its file path.</param>
    /// <returns>The palette of the checkout.</returns>
    /// <exception cref="ContentException">The palette or a drawing file breaks a rule (T-2).</exception>
    public static Palette ReadArt(string root, out List<Drawing> drawings)
    {
        ArgumentException.ThrowIfNullOrEmpty(root);

        Palette? palette = null;
        drawings = [];
        foreach (ContentFile file in ContentFolder.Read(root))
        {
            if (string.CompareOrdinal(file.Path, Palette.Path) == 0)
            {
                palette = Palette.Read(file.Bytes, file.Path);
            }
            else if (Drawing.IsDrawingFile(file.Path))
            {
                drawings.Add(Drawing.Read(file.Bytes, file.Path));
            }
        }

        return palette ?? throw ContentException.ForFile(Palette.Path, "the content folder holds no palette");
    }

    /// <summary>Draws one page of a layout.</summary>
    /// <param name="page">The page to draw.</param>
    /// <param name="layout">The layout that names the place of each frame.</param>
    /// <param name="drawings">Every drawing of the layout, by its id.</param>
    /// <param name="palette">The palette that gives the color of each key.</param>
    /// <returns>The image of the page, with four channels for each pixel.</returns>
    public static PngImage RenderPage(
        AtlasPage page,
        AtlasLayout layout,
        IReadOnlyDictionary<string, Drawing> drawings,
        Palette palette)
    {
        ArgumentNullException.ThrowIfNull(page);
        ArgumentNullException.ThrowIfNull(layout);
        ArgumentNullException.ThrowIfNull(drawings);
        ArgumentNullException.ThrowIfNull(palette);

        var canvas = new AtlasCanvas(page.Width, page.Height);
        foreach (AtlasEntry entry in layout.Entries)
        {
            if (string.CompareOrdinal(entry.Page, page.Name) != 0)
            {
                continue;
            }

            Drawing drawing = drawings[entry.Id.Value];
            for (int frame = 0; frame < entry.Frames.Count; frame += 1)
            {
                AtlasFrame place = entry.Frames[frame];
                canvas.Draw(drawing, frame, palette, place.X, place.Y, 1);
            }
        }

        return canvas.ToImage();
    }

    /// <summary>Gives every drawing of a list by its id.</summary>
    /// <param name="drawings">The drawings to index.</param>
    /// <returns>The map from an id to its drawing.</returns>
    public static SortedDictionary<string, Drawing> ById(IReadOnlyList<Drawing> drawings)
    {
        ArgumentNullException.ThrowIfNull(drawings);

        var byId = new SortedDictionary<string, Drawing>(StringComparer.Ordinal);
        foreach (Drawing drawing in drawings)
        {
            if (!byId.TryAdd(drawing.Id.Value, drawing))
            {
                throw new InvalidOperationException(
                    $"The file '{drawing.File}' takes the id '{drawing.Id.Value}', which '{byId[drawing.Id.Value].File}' already takes. An id is permanent (D-166).");
            }
        }

        return byId;
    }

    private static int Build(string root, string? sheets, bool check, TextWriter output, TextWriter errors)
    {
        Palette palette = ReadArt(root, out List<Drawing> drawings);
        SortedDictionary<string, Drawing> byId = ById(drawings);
        AtlasLayout layout = AtlasLayout.Build(drawings);

        int faults = check
            ? CheckFiles(root, layout, byId, palette, output, errors)
            : WriteFiles(root, layout, byId, palette, output);

        if (sheets is not null)
        {
            WriteSheets(sheets, layout, byId, palette, output);
        }

        output.WriteLine($"{Name}: drawings {drawings.Count}, pages {layout.Pages.Count}.");
        return faults == 0 ? 0 : Program.FaultExitCode;
    }

    private static int WriteFiles(
        string root,
        AtlasLayout layout,
        SortedDictionary<string, Drawing> byId,
        Palette palette,
        TextWriter output)
    {
        foreach (AtlasPage page in layout.Pages)
        {
            string path = ContentPath(root, page.File);
            PngWriter.WriteFile(path, RenderPage(page, layout, byId, palette));
            output.WriteLine($"{Name}: wrote {page.File}, {page.Width} by {page.Height} pixels.");
        }

        foreach (string stale in StalePages(root, layout))
        {
            // The command owns every page file, so a kind that lost its last drawing leaves
            // no page behind (D-666, T-2).
            File.Delete(ContentPath(root, stale));
            output.WriteLine($"{Name}: removed {stale}, which no drawing needs now.");
        }

        File.WriteAllText(ContentPath(root, AtlasIndex.Path), AtlasIndexText.Write(layout));
        output.WriteLine($"{Name}: wrote {AtlasIndex.Path}.");
        return 0;
    }

    private static int CheckFiles(
        string root,
        AtlasLayout layout,
        SortedDictionary<string, Drawing> byId,
        Palette palette,
        TextWriter output,
        TextWriter errors)
    {
        int faults = 0;
        foreach (AtlasPage page in layout.Pages)
        {
            faults += ComparePage(root, page, layout, byId, palette, errors);
        }

        foreach (string stale in StalePages(root, layout))
        {
            errors.WriteLine($"Error: the file '{stale}' is a page that no drawing needs now.");
            faults += 1;
        }

        string expected = AtlasIndexText.Write(layout);
        string path = ContentPath(root, AtlasIndex.Path);
        string committed = File.Exists(path) ? ReadText(path) : string.Empty;
        if (string.CompareOrdinal(expected, committed) != 0)
        {
            errors.WriteLine($"Error: the file '{AtlasIndex.Path}' does not match the drawing files.");
            faults += 1;
        }

        if (faults == 0)
        {
            output.WriteLine($"{Name}: the committed atlas matches every drawing file.");
        }
        else
        {
            errors.WriteLine($"Run `{Name} {RootOption} {root}` to write the atlas again.");
        }

        return faults;
    }

    private static int ComparePage(
        string root,
        AtlasPage page,
        AtlasLayout layout,
        SortedDictionary<string, Drawing> byId,
        Palette palette,
        TextWriter errors)
    {
        string path = ContentPath(root, page.File);
        if (!File.Exists(path))
        {
            errors.WriteLine($"Error: the page '{page.File}' is absent.");
            return 1;
        }

        PngImage wanted = RenderPage(page, layout, byId, palette);
        PngImage committed = PngReader.ReadFile(path);
        if (!SamePixels(wanted, committed))
        {
            errors.WriteLine($"Error: the page '{page.File}' does not match the drawing files.");
            return 1;
        }

        return 0;
    }

    /// <summary>
    /// Compares two images by their decoded pixels, and never by the bytes of a PNG. The
    /// compressed bytes follow the encoder and its version (F-19).
    /// </summary>
    private static bool SamePixels(PngImage first, PngImage second)
    {
        if (first.Width != second.Width || first.Height != second.Height || first.Colors != second.Colors)
        {
            return false;
        }

        return first.Pixels.SequenceEqual(second.Pixels);
    }

    private static List<string> StalePages(string root, AtlasLayout layout)
    {
        var wanted = new SortedSet<string>(StringComparer.Ordinal);
        foreach (AtlasPage page in layout.Pages)
        {
            wanted.Add(page.File);
        }

        var stale = new List<string>();
        foreach (ContentFile file in ContentFolder.Read(root))
        {
            if (ContentPaths.IsAtlasPage(file.Path) && !wanted.Contains(file.Path))
            {
                stale.Add(file.Path);
            }
        }

        return stale;
    }

    private static void WriteSheets(
        string folder,
        AtlasLayout layout,
        SortedDictionary<string, Drawing> byId,
        Palette palette,
        TextWriter output)
    {
        Directory.CreateDirectory(folder);

        string swatch = Path.Combine(folder, SwatchSheet.FileName);
        PngWriter.WriteFile(swatch, SwatchSheet.Render(palette));
        output.WriteLine($"{Name}: wrote {swatch}.");

        foreach (AtlasPageKind kind in ReviewSheet.KindsOf(layout))
        {
            string name = $"review-{AtlasPages.NameOf(kind)}.png";
            string path = Path.Combine(folder, name);
            PngWriter.WriteFile(path, ReviewSheet.Render(kind, layout, byId, palette));
            output.WriteLine($"{Name}: wrote {path}.");
        }
    }

    private static string ContentPath(string root, string path) =>
        Path.Combine(root, ContentFolder.FolderName, path.Replace('/', Path.DirectorySeparatorChar));

    // The file holds `\n` line endings on every platform, and a checkout on Windows can turn
    // them into `\r\n`, so the comparison reads the text with one line ending (T-7).
    private static string ReadText(string path) =>
        File.ReadAllText(path, Encoding.UTF8).Replace("\r\n", "\n");
}
