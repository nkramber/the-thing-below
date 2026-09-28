using System;
using System.Collections.Generic;
using System.IO;
using TheThingBelow.Tools.Png;

namespace TheThingBelow.Tools.Screens;

/// <summary>
/// The `screens` command. It compares the captures of one session with the committed
/// baseline, or it joins them into the pages of a contact sheet (D-172, D-735, D-736, D-1309).
/// </summary>
/// <remarks>
/// The screen-test job of CI runs the compare, and the Mac of the owner runs the sheet. A
/// baseline comes from the renderer of CI, so no local run reproduces it, and the author
/// commits a new baseline from the artifact of the job (D-733).
/// <para>
/// The compare reads decoded pixels and never the bytes of a file, because the compressed
/// bytes of a PNG depend on the encoder (F-19).
/// </para>
/// </remarks>
public static class ScreensCommand
{
    /// <summary>The name of the command on the command line.</summary>
    public const string Name = "screens";

    /// <summary>The option that names the folder of the captures of one session.</summary>
    public const string CapturesOption = "--captures";

    /// <summary>The option that names the folder of the committed baseline (D-736).</summary>
    public const string BaselineOption = "--baseline";

    /// <summary>The option that names the file of the contact sheet. Each page takes the name with its number (D-735, D-1309).</summary>
    public const string SheetOption = "--sheet";

    /// <summary>The file type of every capture and every baseline.</summary>
    private const string PngType = ".png";

    /// <summary>Compares the captures with the baseline, or writes the contact sheet.</summary>
    /// <param name="args">The arguments after the command name.</param>
    /// <param name="output">The writer that takes each line of the report.</param>
    /// <param name="errors">The writer that takes each fault.</param>
    /// <returns>0 when the run holds, and 1 on any fault.</returns>
    public static int Run(IReadOnlyList<string> args, TextWriter output, TextWriter errors)
    {
        ArgumentNullException.ThrowIfNull(args);
        ArgumentNullException.ThrowIfNull(output);
        ArgumentNullException.ThrowIfNull(errors);

        OptionParser? options = OptionParser.Read(
            Name, args, [CapturesOption, BaselineOption, SheetOption], [], errors);
        if (options is null)
        {
            return Program.FaultExitCode;
        }

        string? captures = options.Value(CapturesOption);
        if (captures is null)
        {
            errors.WriteLine($"Error: {Name} needs the option '{CapturesOption}' (T-2).");
            return Program.FaultExitCode;
        }

        string? baseline = options.Value(BaselineOption);
        string? sheet = options.Value(SheetOption);
        if ((baseline is null) == (sheet is null))
        {
            errors.WriteLine(
                $"Error: {Name} takes one of '{BaselineOption}' and '{SheetOption}', and this run gave " +
                (baseline is null ? "neither" : "both") + " (T-2).");
            return Program.FaultExitCode;
        }

        if (OptionValue.ReportEmpty(CapturesOption, captures, errors)
            || (baseline is not null && OptionValue.ReportEmpty(BaselineOption, baseline, errors))
            || (sheet is not null && OptionValue.ReportEmpty(SheetOption, sheet, errors)))
        {
            return Program.FaultExitCode;
        }

        try
        {
            return baseline is null
                ? WriteSheet(captures, sheet!, output, errors)
                : Compare(captures, baseline, output, errors);
        }
        catch (Exception fault) when (
            fault is IOException or UnauthorizedAccessException or PngException
                or ArgumentException or InvalidOperationException)
        {
            errors.WriteLine($"Error: {Name} stopped on the captures of '{captures}': {fault.Message}");
            return Program.FaultExitCode;
        }
    }

    /// <summary>Compares every capture with the baseline of the same file name.</summary>
    /// <param name="captures">The folder that the capture session wrote.</param>
    /// <param name="baseline">The folder of the committed baseline.</param>
    /// <param name="output">The writer that takes each line of the report.</param>
    /// <param name="errors">The writer that takes each fault.</param>
    /// <returns>0 when every capture matches, and 1 on any difference.</returns>
    /// <exception cref="PngException">A file is not a PNG that this reader reads (T-2).</exception>
    private static int Compare(string captures, string baseline, TextWriter output, TextWriter errors)
    {
        IReadOnlyList<string> capturedNames = NamesOf(captures, errors, out bool capturesRead);
        IReadOnlyList<string> baselineNames = NamesOf(baseline, errors, out bool baselineRead);
        if (!capturesRead || !baselineRead)
        {
            return Program.FaultExitCode;
        }

        int faults = ReportMissing(capturedNames, baselineNames, captures, baseline, errors);
        foreach (string name in capturedNames)
        {
            if (!Holds(baselineNames, name))
            {
                continue;
            }

            PngImage committed = PngReader.ReadFile(Path.Combine(baseline, name));
            PngImage taken = PngReader.ReadFile(Path.Combine(captures, name));
            int count = ScreenCompare.Differences(committed, taken, out PixelDifference? difference);
            if (count == 0)
            {
                // The compare passes a step of one level, so the report names each capture that
                // holds one, and the log of CI keeps the evidence of OQ-246 (D-1080).
                int near = ScreenCompare.NearDifferences(committed, taken);
                if (near > 0)
                {
                    output.WriteLine(
                        $"{Name}: the capture '{name}' holds {near} pixel(s) one level from its baseline, " +
                        $"which the compare passes (D-1080, OQ-246).");
                }

                continue;
            }

            PixelDifference found = difference ?? throw new InvalidOperationException(
                $"The compare of '{name}' counted {count} differences and named none (T-2).");

            faults++;
            errors.WriteLine(
                $"Error: {Name}: the capture '{name}' differs from its baseline in {count} pixel(s). " +
                $"The first is at column {found.X}, row {found.Y}: the baseline holds {found.Baseline} " +
                $"and the capture holds {found.Capture} (D-172, T-2).");
        }

        if (faults > 0)
        {
            errors.WriteLine(
                $"Error: {Name}: {faults} capture(s) do not match the baseline. A change of a screen " +
                $"takes a new baseline from the artifact of the job (D-733).");
            return Program.FaultExitCode;
        }

        output.WriteLine($"{Name}: {capturedNames.Count} capture(s) match the baseline of '{baseline}'.");
        return 0;
    }

    /// <summary>Joins every capture into the pages of the contact sheet (D-735, D-1309).</summary>
    /// <param name="captures">The folder that the local capture session wrote.</param>
    /// <param name="sheet">The file of the sheet, which each page numbers and which enters no commit.</param>
    /// <param name="output">The writer that takes each line of the report.</param>
    /// <param name="errors">The writer that takes each fault.</param>
    /// <returns>0 when the sheet is written, and 1 on any fault.</returns>
    /// <exception cref="PngException">A file is not a PNG that this reader reads (T-2).</exception>
    private static int WriteSheet(string captures, string sheet, TextWriter output, TextWriter errors)
    {
        IReadOnlyList<string> names = NamesOf(captures, errors, out bool read);
        if (!read)
        {
            return Program.FaultExitCode;
        }

        if (names.Count == 0)
        {
            errors.WriteLine($"Error: {Name}: the folder '{captures}' holds no {PngType} file (T-2).");
            return Program.FaultExitCode;
        }

        var images = new List<PngImage>(names.Count);
        foreach (string name in names)
        {
            images.Add(PngReader.ReadFile(Path.Combine(captures, name)));
        }

        string? folder = Path.GetDirectoryName(sheet);
        if (!string.IsNullOrEmpty(folder))
        {
            Directory.CreateDirectory(folder);
        }

        IReadOnlyList<(int First, int Count)> pages = ContactSheet.PagesOf(images, ContactSheet.MaxPageHeight);
        output.WriteLine($"{Name}: the sheet of {names.Count} capture(s) takes {pages.Count} page(s).");
        for (int page = 0; page < pages.Count; page++)
        {
            (int first, int count) = pages[page];
            string file = PageFileOf(sheet, page + 1);
            PngImage built = ContactSheet.Build(images.GetRange(first, count));
            PngWriter.WriteFile(file, built);
            output.WriteLine($"{Name}: page {page + 1} is '{file}', {built.Width} by {built.Height} pixels.");
            for (int at = first; at < first + count; at++)
            {
                output.WriteLine($"{Name}: page {page + 1} holds '{names[at]}'.");
            }
        }

        return 0;
    }

    /// <summary>Gives the file of one page of the sheet: the file that the option names, with the page number before its type (D-1309).</summary>
    /// <param name="sheet">The file that the sheet option names, such as `artifacts/contact-sheet.png`.</param>
    /// <param name="page">The number of the page, from 1.</param>
    /// <returns>The file of the page, such as `artifacts/contact-sheet-1.png`.</returns>
    public static string PageFileOf(string sheet, int page)
    {
        ArgumentException.ThrowIfNullOrEmpty(sheet);
        ArgumentOutOfRangeException.ThrowIfLessThan(page, 1);

        string stem = Path.Combine(Path.GetDirectoryName(sheet) ?? string.Empty, Path.GetFileNameWithoutExtension(sheet));
        return $"{stem}-{page}{Path.GetExtension(sheet)}";
    }

    /// <summary>Reads the PNG file names of one folder, in ordinal order.</summary>
    /// <param name="folder">The folder to read.</param>
    /// <param name="errors">The writer that takes the fault of an absent folder.</param>
    /// <param name="read">True when the folder exists, and false when it does not.</param>
    /// <returns>The file names, with no folder part.</returns>
    private static IReadOnlyList<string> NamesOf(string folder, TextWriter errors, out bool read)
    {
        if (!Directory.Exists(folder))
        {
            errors.WriteLine($"Error: {Name}: the folder '{folder}' does not exist (T-2).");
            read = false;
            return [];
        }

        var names = new List<string>();
        foreach (string path in Directory.GetFiles(folder, $"*{PngType}"))
        {
            names.Add(Path.GetFileName(path));
        }

        // The order of the file system changes with the machine, so the report and the sheet
        // take one order on every leg (D-502, T-7).
        names.Sort(StringComparer.Ordinal);
        read = true;
        return names;
    }

    /// <summary>Reports each file that one folder holds and the other does not.</summary>
    /// <param name="capturedNames">The file names of the captures folder.</param>
    /// <param name="baselineNames">The file names of the baseline folder.</param>
    /// <param name="captures">The path of the captures folder, for each fault.</param>
    /// <param name="baseline">The path of the baseline folder, for each fault.</param>
    /// <param name="errors">The writer that takes each fault.</param>
    /// <returns>The count of file names that one folder holds alone.</returns>
    private static int ReportMissing(
        IReadOnlyList<string> capturedNames,
        IReadOnlyList<string> baselineNames,
        string captures,
        string baseline,
        TextWriter errors)
    {
        int faults = 0;
        foreach (string name in capturedNames)
        {
            if (!Holds(baselineNames, name))
            {
                faults++;
                errors.WriteLine(
                    $"Error: {Name}: the session captured '{name}', and the baseline folder " +
                    $"'{baseline}' holds no such file (D-733, T-2).");
            }
        }

        foreach (string name in baselineNames)
        {
            if (!Holds(capturedNames, name))
            {
                faults++;
                errors.WriteLine(
                    $"Error: {Name}: the baseline holds '{name}', and the captures folder " +
                    $"'{captures}' holds no such file (T-2).");
            }
        }

        return faults;
    }

    /// <summary>Tells whether a list of file names holds one name.</summary>
    /// <param name="names">The file names, in ordinal order.</param>
    /// <param name="name">The name to find.</param>
    /// <returns>True when the list holds the name.</returns>
    private static bool Holds(IReadOnlyList<string> names, string name)
    {
        foreach (string held in names)
        {
            if (string.CompareOrdinal(held, name) == 0)
            {
                return true;
            }
        }

        return false;
    }
}
