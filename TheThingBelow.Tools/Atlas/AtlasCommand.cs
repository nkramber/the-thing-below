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

    /// <summary>
    /// The option that compares the committed atlas and writes no page and no index. With
    /// <see cref="SheetsOption"/> the run still writes the sheets, which enter no commit.
    /// </summary>
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

        OptionParser? options = OptionParser.Read(Name, args, [RootOption, SheetsOption], [CheckOption], errors);
        if (options is null)
        {
            return Program.FaultExitCode;
        }

        string root = options.ValueOr(RootOption, ".");
        string? sheets = options.Value(SheetsOption);
        bool check = options.Holds(CheckOption);

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
        string? difference = Difference(wanted, committed);
        if (difference is not null)
        {
            errors.WriteLine($"Error: the page '{page.File}' does not match the drawing files: {difference}.");
            return 1;
        }

        return 0;
    }

    /// <summary>
    /// Compares two images by their decoded pixels, and never by the bytes of a PNG. The
    /// compressed bytes follow the encoder and its version (F-19). The result names the first
    /// difference, so a reader finds the pixel (T-2).
    /// </summary>
    /// <returns>The first difference, or null when the two images hold the same pixels.</returns>
    public static string? Difference(PngImage wanted, PngImage committed)
    {
        ArgumentNullException.ThrowIfNull(wanted);
        ArgumentNullException.ThrowIfNull(committed);

        if (wanted.Width != committed.Width || wanted.Height != committed.Height)
        {
            return $"the drawing files give {wanted.Width} by {wanted.Height} pixels, and the committed page holds {committed.Width} by {committed.Height}";
        }

        if (wanted.Colors != committed.Colors)
        {
            return $"the drawing files give the color kind {wanted.Colors}, and the committed page holds {committed.Colors}";
        }

        int bytesPerPixel = PngImage.BytesPerPixelOf(wanted.Colors);
        ReadOnlySpan<byte> first = wanted.Pixels;
        ReadOnlySpan<byte> second = committed.Pixels;
        for (int index = 0; index < first.Length; index += 1)
        {
            if (first[index] != second[index])
            {
                int pixel = index / bytesPerPixel;
                int x = pixel % wanted.Width;
                int y = pixel / wanted.Width;
                int channel = index % bytesPerPixel;
                return $"the first different pixel is at x {x}, y {y}, channel {channel}: the drawing files give {first[index]}, and the committed page holds {second[index]}";
            }
        }

        return null;
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
            // A batch of one sheet takes the name of its kind, and a larger batch numbers
            // each sheet, so the PR description names them in order (D-668).
            IReadOnlyList<PngImage> sheets = ReviewSheet.Render(kind, layout, byId, palette);
            for (int index = 0; index < sheets.Count; index += 1)
            {
                string number = sheets.Count == 1 ? string.Empty : $"-{index + 1}";
                string path = Path.Combine(folder, $"review-{AtlasPages.NameOf(kind)}{number}.png");
                PngWriter.WriteFile(path, sheets[index]);
                output.WriteLine($"{Name}: wrote {path}.");
            }
        }
    }

    private static string ContentPath(string root, string path) =>
        Path.Combine(root, ContentFolder.FolderName, path.Replace('/', Path.DirectorySeparatorChar));

    // The file holds `\n` line endings on every platform, and a checkout on Windows can turn
    // them into `\r\n`, so the comparison reads the text with one line ending (T-7).
    private static string ReadText(string path) =>
        File.ReadAllText(path, Encoding.UTF8).Replace("\r\n", "\n");
}
