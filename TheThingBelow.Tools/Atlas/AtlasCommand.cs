using System;
using System.Collections.Generic;
using System.IO;
using System.Text;
using TheThingBelow.Core.Content;
using TheThingBelow.Tools.Content;
using TheThingBelow.Tools.NormalMaps;
using TheThingBelow.Tools.Png;

namespace TheThingBelow.Tools.Atlas;

/// <summary>
/// The `atlas` command. It reads the palette and every drawing file of `content/`, packs the
/// frames into the pages of the atlas, and writes each page and the atlas index (D-107,
/// D-666, D-667). It also writes the normal map of each page that takes scene light, with
/// each frame at its place on the color page (D-184, D-517). It replaces the interim script
/// of D-406.
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

    /// <summary>Reads every override grid of a checkout, and checks each one against its drawing.</summary>
    /// <param name="root">The root of the checkout, which holds the `content` folder.</param>
    /// <param name="drawings">Every drawing of the checkout, by its id.</param>
    /// <returns>Every override grid, by the id of its drawing (D-839).</returns>
    /// <exception cref="ContentException">
    /// A grid breaks a rule of the reader, names no drawing, repeats a drawing, or does not fit
    /// its drawing (T-2).
    /// </exception>
    public static SortedDictionary<string, NormalOverride> ReadOverrides(
        string root,
        IReadOnlyDictionary<string, Drawing> drawings)
    {
        ArgumentException.ThrowIfNullOrEmpty(root);
        ArgumentNullException.ThrowIfNull(drawings);

        var overrides = new SortedDictionary<string, NormalOverride>(StringComparer.Ordinal);
        foreach (ContentFile file in ContentFolder.Read(root))
        {
            if (!NormalOverride.IsOverrideFile(file.Path))
            {
                continue;
            }

            NormalOverride grid = NormalOverride.Read(file.Bytes, file.Path);
            if (!drawings.TryGetValue(grid.Drawing.Value, out Drawing? drawing))
            {
                throw ContentException.ForField(
                    file.Path,
                    "drawing",
                    $"the grid names the drawing '{grid.Drawing.Value}', and no drawing file holds it");
            }

            if (!overrides.TryAdd(grid.Drawing.Value, grid))
            {
                throw ContentException.ForField(
                    file.Path,
                    "drawing",
                    $"the file '{overrides[grid.Drawing.Value].File}' already holds the grid of '{grid.Drawing.Value}', and a drawing takes one grid (D-839)");
            }

            grid.RefuseWrongShape(drawing);
        }

        return overrides;
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
        SortedDictionary<string, NormalOverride> overrides = ReadOverrides(root, byId);
        AtlasLayout layout = AtlasLayout.Build(drawings);

        int faults = check
            ? CheckFiles(root, layout, byId, palette, overrides, output, errors)
            : WriteFiles(root, layout, byId, palette, overrides, output);

        if (sheets is not null)
        {
            WriteSheets(sheets, layout, byId, palette, overrides, output);
        }

        output.WriteLine($"{Name}: drawings {drawings.Count}, pages {layout.Pages.Count}.");
        return faults == 0 ? 0 : Program.FaultExitCode;
    }

    private static int WriteFiles(
        string root,
        AtlasLayout layout,
        SortedDictionary<string, Drawing> byId,
        Palette palette,
        SortedDictionary<string, NormalOverride> overrides,
        TextWriter output)
    {
        foreach (AtlasPage page in layout.Pages)
        {
            string path = ContentPath(root, page.File);
            PngWriter.WriteFile(path, RenderPage(page, layout, byId, palette));
            output.WriteLine($"{Name}: wrote {page.File}, {page.Width} by {page.Height} pixels.");

            // A page of a kind that takes scene light gets its normal map, with each frame at
            // the same place (D-184, D-517). A portrait and the UI take none (D-210).
            if (AtlasPages.TakesLight(page.Kind))
            {
                PngImage normals = NormalMap.RenderPage(page, layout, byId, palette, overrides);
                PngWriter.WriteFile(ContentPath(root, page.NormalFile), normals);
                output.WriteLine($"{Name}: wrote {page.NormalFile}, {page.Width} by {page.Height} pixels.");
            }
        }

        foreach (string stale in StalePages(root, layout))
        {
            // The command owns every page file and every normal-map page, so a kind that lost
            // its last drawing leaves no page behind (D-666, T-2).
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
        SortedDictionary<string, NormalOverride> overrides,
        TextWriter output,
        TextWriter errors)
    {
        int faults = 0;
        foreach (AtlasPage page in layout.Pages)
        {
            faults += ComparePage(root, page.File, RenderPage(page, layout, byId, palette), errors);

            // The normal-map atlas takes the same pixel test as the color atlas (D-184, F-19).
            if (AtlasPages.TakesLight(page.Kind))
            {
                PngImage normals = NormalMap.RenderPage(page, layout, byId, palette, overrides);
                faults += ComparePage(root, page.NormalFile, normals, errors);
            }
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
            output.WriteLine($"{Name}: the committed atlas and its normal maps match every drawing file.");
        }
        else
        {
            errors.WriteLine($"Run `{Name} {RootOption} {root}` to write the atlas again.");
        }

        return faults;
    }

    private static int ComparePage(string root, string file, PngImage wanted, TextWriter errors)
    {
        string path = ContentPath(root, file);
        if (!File.Exists(path))
        {
            errors.WriteLine($"Error: the page '{file}' is absent.");
            return 1;
        }

        PngImage committed = PngReader.ReadFile(path);
        string? difference = Difference(wanted, committed);
        if (difference is not null)
        {
            errors.WriteLine($"Error: the page '{file}' does not match the drawing files: {difference}.");
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
            if (AtlasPages.TakesLight(page.Kind))
            {
                wanted.Add(page.NormalFile);
            }
        }

        var stale = new List<string>();
        foreach (ContentFile file in ContentFolder.Read(root))
        {
            bool page = ContentPaths.IsAtlasPage(file.Path) || ContentPaths.IsNormalPage(file.Path);
            if (page && !wanted.Contains(file.Path))
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
        SortedDictionary<string, NormalOverride> overrides,
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
            WriteSheetSet(folder, $"review-{AtlasPages.NameOf(kind)}", ReviewSheet.Render(kind, layout, byId, palette), output);

            // A kind that takes scene light gets the sheet of its normal maps too (D-521, D-841).
            if (AtlasPages.TakesLight(kind))
            {
                IReadOnlyList<PngImage> lit = NormalSheet.Render(kind, layout, byId, palette, overrides);
                WriteSheetSet(folder, $"review-normals-{AtlasPages.NameOf(kind)}", lit, output);
            }
        }
    }

    private static void WriteSheetSet(string folder, string stem, IReadOnlyList<PngImage> sheets, TextWriter output)
    {
        for (int index = 0; index < sheets.Count; index += 1)
        {
            string number = sheets.Count == 1 ? string.Empty : $"-{index + 1}";
            string path = Path.Combine(folder, $"{stem}{number}.png");
            PngWriter.WriteFile(path, sheets[index]);
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
