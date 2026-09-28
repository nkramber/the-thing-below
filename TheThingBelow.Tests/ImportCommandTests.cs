using System;
using System.Collections.Generic;
using System.IO;
using TheThingBelow.Core.Content;
using TheThingBelow.Tools;
using TheThingBelow.Tools.Atlas;
using TheThingBelow.Tools.Import;
using TheThingBelow.Tools.Png;
using Xunit;

namespace TheThingBelow.Tests;

/// <summary>
/// The `import` command: one frame of an existing drawing file from a PNG (D-688, D-1311,
/// D-1316).
/// </summary>
public sealed class ImportCommandTests : IDisposable
{
    private const string DrawingPath = "sprites/drawings/cast/one.json";

    private readonly ImportCheckout checkout = new();

    public void Dispose() => this.checkout.Dispose();

    /// <summary>Exit test 1 of PR-51: a round trip through the frame PNG (D-1313).</summary>
    [Fact]
    public void ARoundTripThroughTheFramePngGivesTheSameGrid()
    {
        string[] rows = ["kKdD", "wex.", "..kx"];
        string drawing = this.checkout.WriteContent(DrawingPath, DrawingFixtures.BodyOfRows("one", rows));
        string png = Path.Combine(this.checkout.Root, "frame.png");

        int written = FramePngCommand.Run(
            [FramePngCommand.RootOption, this.checkout.Root, FramePngCommand.DrawingOption, drawing, FramePngCommand.FrameOption, "0", FramePngCommand.OutOption, png],
            new StringWriter(),
            new StringWriter());
        this.checkout.WriteContent(DrawingPath, DrawingFixtures.BodyOfRows("one", ["kkkk", "kkkk", "kkkk"]));
        (int code, string output, _) = this.Import(ImportCommand.HandEditMode, png, drawing);

        Assert.Equal(0, written);
        Assert.Equal(0, code);
        Assert.Equal(rows, ImportCheckout.ReadDrawing(drawing).Frames[0].Rows);
        Assert.Contains("hand-edit mode, and 10 pixels changed", output);
    }

    /// <summary>Exit test 3 of PR-51: the PNG code refuses an indexed PNG (D-176).</summary>
    [Fact]
    public void AnIndexedPngFailsWithTheFile()
    {
        string drawing = this.WriteSolid();
        string png = Path.Combine(this.checkout.Root, "indexed.png");
        File.WriteAllBytes(png, PngFixtures.File(
            PngFixtures.Chunk("IHDR", PngFixtures.Header(32, 32, 8, 3)),
            PngFixtures.Chunk("IEND", ReadOnlySpan<byte>.Empty)));
        byte[] before = File.ReadAllBytes(drawing);

        (int code, _, string errors) = this.Import(ImportCommand.HandEditMode, png, drawing);

        Assert.Equal(Program.FaultExitCode, code);
        Assert.Contains("wrote nothing", errors);
        Assert.Contains(png, errors);
        Assert.Contains("the color type is 3", errors);
        Assert.Equal(before, File.ReadAllBytes(drawing));
    }

    /// <summary>Exit test 4 of PR-51: the atlas that the command builds again matches the new grid (F-19).</summary>
    [Fact]
    public void TheRebuiltAtlasMatchesThePixelsOfTheNewGrid()
    {
        string drawing = this.WriteSolid();
        Assert.Equal(0, this.Atlas());
        string png = this.checkout.WritePng("picture.png", new ImportPicture(40, 40).Fill(3, 5, 2, 2, "c03030").ToImage());

        (int code, string output, _) = this.Import(ImportCommand.GeneratorMode, png, drawing);
        int staleCheck = this.Atlas(AtlasCommand.CheckOption);
        int build = this.Atlas();
        int check = this.Atlas(AtlasCommand.CheckOption);

        Assert.Equal(0, code);
        Assert.Contains("-- atlas --root " + this.checkout.Root, output);
        Assert.Equal(Program.FaultExitCode, staleCheck);
        Assert.Equal(0, build);
        Assert.Equal(0, check);

        // The content of 2 by 2 sits at x 15 and y 15 of the frame, in the red x of c8353a.
        PngImage page = PngReader.ReadFile(this.checkout.PathOf("sprites/atlas-map_sprites.png"));
        Assert.Equal([0xc8, 0x35, 0x3a, 255], page.Row(15).Slice(15 * 4, 4).ToArray());
        Assert.Equal([0, 0, 0, 0], page.Row(14).Slice(15 * 4, 4).ToArray());
    }

    [Fact]
    public void TheGeneratorModeReportsThePlaceAndTheCountOfTheMap()
    {
        string drawing = this.WriteSolid();
        string png = this.checkout.WritePng(
            "picture.png",
            new ImportPicture(42, 42).Fill(5, 7, 30, 30, "0b0a0f").Set(6, 8, "c03030").ToImage());

        (int code, string output, _) = this.Import(ImportCommand.GeneratorMode, png, drawing);

        Assert.Equal(0, code);
        Assert.Contains("30 by 30 pixels at x 5, y 7", output);
        Assert.Contains("x 1, y 1 of the frame of 32 by 32", output);
        Assert.Contains("mapped 1 of 900 opaque pixels", output);
        Assert.Equal("." + new string('k', 30) + ".", ImportCheckout.ReadDrawing(drawing).Frames[0].Rows[1]);
    }

    /// <summary>A failure writes nothing, so the drawing file keeps each byte (T-2).</summary>
    [Fact]
    public void AFailedImportLeavesTheDrawingFile()
    {
        string drawing = this.WriteSolid();
        string png = this.checkout.WritePng("big.png", new ImportPicture(40, 40).Fill(0, 0, 33, 10, "0b0a0f").ToImage());
        byte[] before = File.ReadAllBytes(drawing);

        (int code, _, string errors) = this.Import(ImportCommand.GeneratorMode, png, drawing);

        Assert.Equal(Program.FaultExitCode, code);
        Assert.Contains("the content is 33 by 10 pixels", errors);
        Assert.Contains(png, errors);
        Assert.Equal(before, File.ReadAllBytes(drawing));
    }

    [Fact]
    public void AFrameThatTheDrawingLacksFails()
    {
        string drawing = this.WriteSolid();
        string png = this.checkout.WritePng("edit.png", new ImportPicture(32, 32).ToImage());

        (int code, _, string errors) = this.Import(ImportCommand.HandEditMode, png, drawing, frame: "1");

        Assert.Equal(Program.FaultExitCode, code);
        Assert.Contains("holds 1 frames, from 0 to 0, and it has no frame 1", errors);
        Assert.Contains(drawing, errors);
    }

    [Theory]
    [InlineData("paint", "0", "the mode 'paint' is unknown")]
    [InlineData(ImportCommand.HandEditMode, "-1", "the frame '-1' is not a whole number")]
    [InlineData(ImportCommand.HandEditMode, "one", "the frame 'one' is not a whole number")]
    public void AWrongModeOrFrameFails(string mode, string frame, string message)
    {
        string drawing = this.WriteSolid();

        (int code, _, string errors) = this.Import(mode, "edit.png", drawing, frame);

        Assert.Equal(Program.FaultExitCode, code);
        Assert.Contains(message, errors);
    }

    [Fact]
    public void AnAbsentOptionFailsWithTheUsage()
    {
        var errors = new StringWriter();

        int code = ImportCommand.Run([ImportCommand.ModeOption, ImportCommand.HandEditMode], new StringWriter(), errors);

        Assert.Equal(Program.FaultExitCode, code);
        Assert.Contains("--png <file>, --drawing <file>, and --frame <number>", errors.ToString());
    }

    [Fact]
    public void TheProgramRunsTheCommandByItsName()
    {
        var errors = new StringWriter();

        int code = Program.Run([ImportCommand.Name], new StringWriter(), errors);

        Assert.Equal(Program.FaultExitCode, code);
        Assert.Contains("Error: import takes --mode", errors.ToString());
    }

    private string WriteSolid() =>
        this.checkout.WriteContent(DrawingPath, DrawingFixtures.Body("one"));

    private int Atlas(params string[] options)
    {
        List<string> args = [AtlasCommand.RootOption, this.checkout.Root, .. options];
        return AtlasCommand.Run(args, new StringWriter(), new StringWriter());
    }

    private (int Code, string Output, string Errors) Import(string mode, string png, string drawing, string frame = "0")
    {
        var output = new StringWriter();
        var errors = new StringWriter();
        int code = ImportCommand.Run(
            [ImportCommand.RootOption, this.checkout.Root, ImportCommand.ModeOption, mode, ImportCommand.PngOption, png, ImportCommand.DrawingOption, drawing, ImportCommand.FrameOption, frame],
            output,
            errors);
        return (code, output.ToString(), errors.ToString());
    }
}
