using System;
using System.IO;
using TheThingBelow.Tools;
using TheThingBelow.Tools.Import;
using TheThingBelow.Tools.Png;
using Xunit;

namespace TheThingBelow.Tests;

/// <summary>The `frame-png` command: one frame at 1x, for a hand edit (D-1313).</summary>
public sealed class FramePngCommandTests : IDisposable
{
    private const string DrawingPath = "sprites/drawings/cast/walk.json";

    private readonly ImportCheckout checkout = new();

    public void Dispose() => this.checkout.Dispose();

    [Fact]
    public void TheCommandWritesTheFrameAtOneXWithAlphaZeroForTheDot()
    {
        string drawing = this.checkout.WriteContent(DrawingPath, DrawingFixtures.BodyOfRows("walk", ["kx", ".w"]));
        string png = Path.Combine(this.checkout.Root, "frame.png");

        (int code, string output, _) = this.Run(drawing, "0", png);

        PngImage image = PngReader.ReadFile(png);
        Assert.Equal(0, code);
        Assert.Contains("2 by 2 pixels", output);
        Assert.Equal(PngColorKind.Rgba, image.Colors);
        Assert.Equal([0x0b, 0x0a, 0x0f, 255, 0xc8, 0x35, 0x3a, 255], image.Row(0).ToArray());
        Assert.Equal([0, 0, 0, 0, 0xf2, 0xee, 0xea, 255], image.Row(1).ToArray());
    }

    [Fact]
    public void TheCommandWritesTheFrameThatTheOptionNames()
    {
        string drawing = this.checkout.WriteContent(DrawingPath, DrawingFixtures.Body("walk", width: 1, height: 1, frames: 3, key: 'e'));
        string png = Path.Combine(this.checkout.Root, "frame.png");

        (int code, _, _) = this.Run(drawing, "2", png);

        Assert.Equal(0, code);
        Assert.Equal([0xe8, 0xf2, 0xf7, 255], PngReader.ReadFile(png).Row(0).ToArray());
    }

    [Fact]
    public void AFrameThatTheDrawingLacksFailsAndWritesNoPng()
    {
        string drawing = this.checkout.WriteContent(DrawingPath, DrawingFixtures.Body("walk"));
        string png = Path.Combine(this.checkout.Root, "frame.png");

        (int code, _, string errors) = this.Run(drawing, "4", png);

        Assert.Equal(Program.FaultExitCode, code);
        Assert.Contains("has no frame 4", errors);
        Assert.False(File.Exists(png));
    }

    /// <summary>An output path of the drawing file fails and keeps each byte of the drawing (T-2).</summary>
    [Fact]
    public void AnOutputPathOfTheDrawingFileFailsAndKeepsTheDrawing()
    {
        string drawing = this.checkout.WriteContent(DrawingPath, DrawingFixtures.Body("walk"));
        byte[] before = File.ReadAllBytes(drawing);
        string samePath = Path.Combine(Path.GetDirectoryName(drawing)!, ".", "walk.json");

        (int code, _, string errors) = this.Run(drawing, "0", samePath);

        Assert.Equal(Program.FaultExitCode, code);
        Assert.Contains("the output already exists", errors);
        Assert.Contains(samePath, errors);
        Assert.Equal(before, File.ReadAllBytes(drawing));
    }

    /// <summary>
    /// A symbolic link to the drawing file is an alias that no compare of paths finds, so the
    /// command refuses every name that exists (T-2).
    /// </summary>
    [Fact]
    public void AnOutputLinkToTheDrawingFileFailsAndKeepsTheDrawing()
    {
        string drawing = this.checkout.WriteContent(DrawingPath, DrawingFixtures.Body("walk"));
        byte[] before = File.ReadAllBytes(drawing);
        string link = Path.Combine(this.checkout.Root, "alias.png");
        File.CreateSymbolicLink(link, drawing);

        (int code, _, string errors) = this.Run(drawing, "0", link);

        Assert.Equal(Program.FaultExitCode, code);
        Assert.Contains("never writes over a file", errors);
        Assert.Equal(before, File.ReadAllBytes(drawing));
    }

    /// <summary>
    /// Any file at the output path stays. A hard link to the drawing is such a file, so this
    /// case covers it on every system (T-2).
    /// </summary>
    [Fact]
    public void AnExistingOutputFileFailsAndKeepsItsBytes()
    {
        string drawing = this.checkout.WriteContent(DrawingPath, DrawingFixtures.Body("walk"));
        string png = Path.Combine(this.checkout.Root, "frame.png");
        File.WriteAllBytes(png, [1, 2, 3]);

        (int code, _, string errors) = this.Run(drawing, "0", png);

        Assert.Equal(Program.FaultExitCode, code);
        Assert.Contains("Remove it, or name another --out file", errors);
        Assert.Equal([1, 2, 3], File.ReadAllBytes(png));
    }

    [Fact]
    public void AnAbsentOptionFailsWithTheUsage()
    {
        var errors = new StringWriter();

        int code = Program.Run([FramePngCommand.Name, FramePngCommand.FrameOption, "0"], new StringWriter(), errors);

        Assert.Equal(Program.FaultExitCode, code);
        Assert.Contains("frame-png takes --drawing <file>, --frame <number>, and --out <file>", errors.ToString());
    }

    private (int Code, string Output, string Errors) Run(string drawing, string frame, string png)
    {
        var output = new StringWriter();
        var errors = new StringWriter();
        int code = FramePngCommand.Run(
            [FramePngCommand.RootOption, this.checkout.Root, FramePngCommand.DrawingOption, drawing, FramePngCommand.FrameOption, frame, FramePngCommand.OutOption, png],
            output,
            errors);
        return (code, output.ToString(), errors.ToString());
    }
}
