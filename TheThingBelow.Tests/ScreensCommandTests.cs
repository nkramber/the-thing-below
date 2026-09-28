using System;
using System.IO;
using TheThingBelow.Tools;
using TheThingBelow.Tools.Png;
using TheThingBelow.Tools.Screens;
using Xunit;

namespace TheThingBelow.Tests;

/// <summary>
/// The `screens` command compares the captures of one session with the committed baseline,
/// or it joins them into one contact sheet (D-172, D-735, D-736).
/// </summary>
/// <remarks>
/// Each test writes its own folders under the temporary folder of the system, so no test
/// reads the baseline of the checkout and no test writes into it.
/// </remarks>
public sealed class ScreensCommandTests : IDisposable
{
    private readonly string root = Path.Combine(Path.GetTempPath(), $"screens-{Guid.NewGuid():N}");

    public void Dispose()
    {
        if (Directory.Exists(this.root))
        {
            Directory.Delete(this.root, recursive: true);
        }
    }

    [Fact]
    public void EqualFoldersMatch()
    {
        string captures = this.Folder("captures", 10);
        string baseline = this.Folder("baseline", 10);
        using StringWriter output = new();
        using StringWriter errors = new();

        int exitCode = ScreensCommand.Run(
            ["--captures", captures, "--baseline", baseline], output, errors);

        Assert.Equal(string.Empty, errors.ToString());
        Assert.Equal(0, exitCode);
        Assert.Contains("1 capture(s) match the baseline", output.ToString(), StringComparison.Ordinal);
    }

    [Fact]
    public void OneChangedPixelFailsAndNamesThePlace()
    {
        // Exit test 3 of PR-41. One changed pixel fails the job, and the report names the
        // file, the count, and the first pixel (T-2).
        string captures = this.Folder("captures", 10);
        string baseline = this.Folder("baseline", 10);
        WriteOnePixel(Path.Combine(captures, "map-1x.png"), 10, 200);
        using StringWriter output = new();
        using StringWriter errors = new();

        int exitCode = ScreensCommand.Run(
            ["--captures", captures, "--baseline", baseline], output, errors);

        Assert.Equal(Program.FaultExitCode, exitCode);
        Assert.Contains("map-1x.png", errors.ToString(), StringComparison.Ordinal);
        Assert.Contains("1 pixel(s)", errors.ToString(), StringComparison.Ordinal);
        Assert.Contains("column 0, row 0", errors.ToString(), StringComparison.Ordinal);
        Assert.Contains("D-733", errors.ToString(), StringComparison.Ordinal);
    }

    [Fact]
    public void ACaptureWithNoBaselineFails()
    {
        string captures = this.Folder("captures", 10);
        string baseline = Path.Combine(this.root, "baseline");
        Directory.CreateDirectory(baseline);
        using StringWriter output = new();
        using StringWriter errors = new();

        int exitCode = ScreensCommand.Run(
            ["--captures", captures, "--baseline", baseline], output, errors);

        Assert.Equal(Program.FaultExitCode, exitCode);
        Assert.Contains("holds no such file", errors.ToString(), StringComparison.Ordinal);
    }

    [Fact]
    public void ABaselineWithNoCaptureFails()
    {
        // A leftover baseline of a list change never passes without a word (T-2).
        string captures = this.Folder("captures", 10);
        string baseline = this.Folder("baseline", 10);
        WriteOnePixel(Path.Combine(baseline, "ui-1x.png"), 10, 10);
        using StringWriter output = new();
        using StringWriter errors = new();

        int exitCode = ScreensCommand.Run(
            ["--captures", captures, "--baseline", baseline], output, errors);

        Assert.Equal(Program.FaultExitCode, exitCode);
        Assert.Contains("ui-1x.png", errors.ToString(), StringComparison.Ordinal);
    }

    [Fact]
    public void AnAbsentFolderFails()
    {
        // T-2. An absent folder is an error, and never an empty list that matches.
        string captures = this.Folder("captures", 10);
        using StringWriter output = new();
        using StringWriter errors = new();

        int exitCode = ScreensCommand.Run(
            ["--captures", captures, "--baseline", Path.Combine(this.root, "none")], output, errors);

        Assert.Equal(Program.FaultExitCode, exitCode);
        Assert.Contains("does not exist", errors.ToString(), StringComparison.Ordinal);
    }

    [Fact]
    public void TheSheetHoldsEveryCapture()
    {
        string captures = this.Folder("captures", 10);
        string sheet = Path.Combine(this.root, "sheets", "contact-sheet.png");
        using StringWriter output = new();
        using StringWriter errors = new();

        int exitCode = ScreensCommand.Run(["--captures", captures, "--sheet", sheet], output, errors);

        Assert.Equal(string.Empty, errors.ToString());
        Assert.Equal(0, exitCode);
        Assert.True(File.Exists(Path.Combine(this.root, "sheets", "contact-sheet-1.png")));
        Assert.False(File.Exists(Path.Combine(this.root, "sheets", "contact-sheet-2.png")));
        Assert.Contains("page 1 holds 'map-1x.png'", output.ToString(), StringComparison.Ordinal);
    }

    [Fact]
    public void ASheetHigherThanOnePngTakesMorePages()
    {
        // D-1309, regression: 70 captures of 1000 rows need 70,552 rows, above the 65,535 of a
        // PNG. The old sheet failed on the height, and the new sheet writes two pages.
        string captures = Path.Combine(this.root, "tall");
        Directory.CreateDirectory(captures);
        for (int at = 0; at < 70; at++)
        {
            PngWriter.WriteFile(Path.Combine(captures, $"frame-{at:D2}.png"), new PngImage(1, 1000, PngColorKind.Rgba, new byte[1000 * 4]));
        }

        string sheet = Path.Combine(this.root, "sheets", "contact-sheet.png");
        using StringWriter output = new();
        using StringWriter errors = new();

        int exitCode = ScreensCommand.Run(["--captures", captures, "--sheet", sheet], output, errors);

        Assert.Equal(string.Empty, errors.ToString());
        Assert.Equal(0, exitCode);
        Assert.Contains("the sheet of 70 capture(s) takes 2 page(s)", output.ToString(), StringComparison.Ordinal);
        Assert.Contains("page 2 holds 'frame-69.png'", output.ToString(), StringComparison.Ordinal);
        PngImage first = PngReader.ReadFile(Path.Combine(this.root, "sheets", "contact-sheet-1.png"));
        PngImage second = PngReader.ReadFile(Path.Combine(this.root, "sheets", "contact-sheet-2.png"));
        Assert.True(first.Height <= PngImage.MaxSize);
        Assert.Equal((70 * 1000) + (68 * ContactSheet.GapPixels), first.Height + second.Height);
    }

    [Theory]
    [InlineData("artifacts/contact-sheet.png", 1, "artifacts/contact-sheet-1.png")]
    [InlineData("artifacts/contact-sheet.png", 12, "artifacts/contact-sheet-12.png")]
    [InlineData("sheet.png", 2, "sheet-2.png")]
    public void EachPageTakesTheNameOfTheSheetWithItsNumber(string sheet, int page, string file)
    {
        // D-1309: the page number goes before the type of the file.
        Assert.Equal(file.Replace('/', Path.DirectorySeparatorChar), ScreensCommand.PageFileOf(sheet, page).Replace('/', Path.DirectorySeparatorChar));
    }

    [Fact]
    public void BothOfTheTwoModesFail()
    {
        // T-2. The command compares or it makes a sheet, and a run that asks for both names
        // the fault instead of picking one.
        string captures = this.Folder("captures", 10);
        using StringWriter output = new();
        using StringWriter errors = new();

        int exitCode = ScreensCommand.Run(
            ["--captures", captures, "--baseline", captures, "--sheet", "sheet.png"], output, errors);

        Assert.Equal(Program.FaultExitCode, exitCode);
        Assert.Contains("gave both", errors.ToString(), StringComparison.Ordinal);
    }

    [Fact]
    public void NeitherOfTheTwoModesFails()
    {
        string captures = this.Folder("captures", 10);
        using StringWriter output = new();
        using StringWriter errors = new();

        int exitCode = ScreensCommand.Run(["--captures", captures], output, errors);

        Assert.Equal(Program.FaultExitCode, exitCode);
        Assert.Contains("gave neither", errors.ToString(), StringComparison.Ordinal);
    }

    [Fact]
    public void NoCapturesOptionFails()
    {
        using StringWriter output = new();
        using StringWriter errors = new();

        int exitCode = ScreensCommand.Run(["--baseline", "screens/baseline"], output, errors);

        Assert.Equal(Program.FaultExitCode, exitCode);
        Assert.Contains(ScreensCommand.CapturesOption, errors.ToString(), StringComparison.Ordinal);
    }

    [Fact]
    public void AStepOfOneLevelPassesAndTheReportNamesIt()
    {
        // OQ-246, D-1080: the compare passes a step of one level. The report names the capture
        // and the count, so a real change of one level never passes in silence.
        string captures = this.Folder("captures", 10);
        string baseline = this.Folder("baseline", 10);
        WriteOnePixel(Path.Combine(captures, "map-1x.png"), 10, 11);
        using StringWriter output = new();
        using StringWriter errors = new();

        int exitCode = ScreensCommand.Run(
            ["--captures", captures, "--baseline", baseline], output, errors);

        Assert.Equal(string.Empty, errors.ToString());
        Assert.Equal(0, exitCode);
        Assert.Contains("the capture 'map-1x.png' holds 1 pixel(s) one level from its baseline", output.ToString(), StringComparison.Ordinal);
    }

    [Fact]
    public void EqualFoldersNameNoCaptureAsNear()
    {
        string captures = this.Folder("captures", 10);
        string baseline = this.Folder("baseline", 10);
        using StringWriter output = new();
        using StringWriter errors = new();

        ScreensCommand.Run(["--captures", captures, "--baseline", baseline], output, errors);

        Assert.DoesNotContain("one level", output.ToString(), StringComparison.Ordinal);
    }

    /// <summary>Makes a folder with one picture named `map-1x.png`.</summary>
    /// <param name="name">The name of the folder under the root of this test.</param>
    /// <param name="level">The red, green, and blue of every pixel.</param>
    /// <returns>The path of the folder.</returns>
    private string Folder(string name, byte level)
    {
        string folder = Path.Combine(this.root, name);
        Directory.CreateDirectory(folder);
        WriteOnePixel(Path.Combine(folder, "map-1x.png"), level, level);
        return folder;
    }

    /// <summary>Writes a picture of two pixels, with its own level for the first one.</summary>
    /// <param name="path">The file to write.</param>
    /// <param name="level">The red, green, and blue of the second pixel.</param>
    /// <param name="firstRed">The red of the first pixel.</param>
    private static void WriteOnePixel(string path, byte level, byte firstRed)
    {
        byte[] pixels = [firstRed, level, level, 255, level, level, level, 255];
        PngWriter.WriteFile(path, new PngImage(2, 1, PngColorKind.Rgba, pixels));
    }
}
