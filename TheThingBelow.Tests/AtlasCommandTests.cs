using System;
using System.Collections.Generic;
using System.IO;
using TheThingBelow.Core.Content;
using TheThingBelow.Tools;
using TheThingBelow.Tools.Atlas;
using TheThingBelow.Tools.NormalMaps;
using TheThingBelow.Tools.Png;
using Xunit;

namespace TheThingBelow.Tests;

/// <summary>
/// The `atlas` command (D-107, D-666). It writes each page and the atlas index, and its
/// `--check` option compares the committed atlas by decoded pixels and never by bytes (F-19).
/// </summary>
public sealed class AtlasCommandTests : IDisposable
{
    private readonly string root = Path.Combine(Path.GetTempPath(), $"atlas-{Guid.NewGuid():N}");

    public void Dispose()
    {
        if (Directory.Exists(this.root))
        {
            Directory.Delete(this.root, recursive: true);
        }
    }

    [Fact]
    public void TheCommandWritesEachPageAndTheIndex()
    {
        this.Write("one");

        int code = this.Run();

        Assert.Equal(0, code);
        Assert.True(File.Exists(this.PathOf("sprites/atlas-map_sprites.png")));
        Assert.True(File.Exists(this.PathOf(AtlasIndex.Path)));
    }

    [Fact]
    public void TheIndexThatTheCommandWritesReadsBackThroughCore()
    {
        this.Write("one", "two");
        this.Run();

        AtlasIndex index = AtlasIndex.Read(File.ReadAllBytes(this.PathOf(AtlasIndex.Path)), AtlasIndex.Path);

        Assert.Equal(2, index.Entries.Count);
        Assert.True(index.Draws(ContentId.Parse("cast.one", "test", "id"), "map_front"));
    }

    [Fact]
    public void ThePageHoldsThePixelsOfEachDrawing()
    {
        this.Write("one", "two");
        this.Run();

        PngImage page = PngReader.ReadFile(this.PathOf("sprites/atlas-map_sprites.png"));

        Assert.Equal(64, page.Width);
        Assert.Equal(32, page.Height);
        Assert.Equal([11, 10, 15, 255], page.Row(0)[..4].ToArray());
    }

    [Fact]
    public void TheCheckPassesOnAnAtlasThatTheCommandWrote()
    {
        this.Write("one");
        this.Run();

        int code = this.Run(AtlasCommand.CheckOption);

        Assert.Equal(0, code);
    }

    /// <summary>A stale atlas fails until the command runs again (G-24).</summary>
    [Fact]
    public void TheCheckFailsOnAChangedDrawing()
    {
        this.Write("one");
        this.Run();
        this.Write("one", key: 'x');

        var errors = new StringWriter();
        int code = AtlasCommand.Run([AtlasCommand.RootOption, this.root, AtlasCommand.CheckOption], new StringWriter(), errors);

        Assert.Equal(Program.FaultExitCode, code);
        Assert.Contains("sprites/atlas-map_sprites.png", errors.ToString());

        // The message names the first pixel that differs, so a reader finds it (T-2).
        Assert.Contains("the first different pixel is at x ", errors.ToString());
    }

    [Fact]
    public void TheCheckFailsOnAnAbsentAtlas()
    {
        this.Write("one");

        var errors = new StringWriter();
        int code = AtlasCommand.Run([AtlasCommand.RootOption, this.root, AtlasCommand.CheckOption], new StringWriter(), errors);

        Assert.Equal(Program.FaultExitCode, code);
        Assert.Contains("absent", errors.ToString());
    }

    /// <summary>A page of a kind that lost its last drawing leaves no file behind (D-666).</summary>
    [Fact]
    public void TheCommandRemovesAPageThatNoDrawingNeeds()
    {
        this.Write("one");
        File.WriteAllBytes(this.PathOf("sprites/atlas-portraits.png"), PngWriter.Write(new PngImage(1, 1, PngColorKind.Rgba, new byte[4])));

        var output = new StringWriter();
        AtlasCommand.Run([AtlasCommand.RootOption, this.root], output, new StringWriter());

        Assert.False(File.Exists(this.PathOf("sprites/atlas-portraits.png")));
        Assert.Contains("removed sprites/atlas-portraits.png", output.ToString());
    }

    /// <summary>A key that the palette lacks fails with the file, the row, and the column (F-20).</summary>
    [Fact]
    public void AKeyThatThePaletteLacksFailsWithTheFileAndTheColumn()
    {
        this.Write("one", key: 'Z');

        var errors = new StringWriter();
        int code = AtlasCommand.Run([AtlasCommand.RootOption, this.root], new StringWriter(), errors);

        Assert.Equal(Program.FaultExitCode, code);
        Assert.Contains(DrawingFixtures.PathOf("one"), errors.ToString());
        Assert.Contains("column 0", errors.ToString());
        Assert.Contains("F-20", errors.ToString());
    }

    [Fact]
    public void TheCommandWritesTheSwatchSheetAndOneReviewSheetOfEachKind()
    {
        this.Write("one");
        string sheets = Path.Combine(this.root, "sheets");

        this.Run(AtlasCommand.SheetsOption, sheets);

        Assert.True(File.Exists(Path.Combine(sheets, SwatchSheet.FileName)));
        Assert.True(File.Exists(Path.Combine(sheets, "review-map_sprites.png")));
    }

    /// <summary>A page of sprites takes scene light, so the command writes its normal map beside it (D-184).</summary>
    [Fact]
    public void TheCommandWritesTheNormalMapOfALitPageAlone()
    {
        this.Write("one");
        this.WriteFile(DrawingFixtures.PathOf("icon"), DrawingFixtures.Body("icon", page: "ui", width: 16, height: 16));

        int code = this.Run();

        Assert.Equal(0, code);
        PngImage normals = PngReader.ReadFile(this.PathOf("sprites/normal-map-map_sprites.png"));
        Assert.Equal([128, 128, 255, 255], normals.Row(0)[..4].ToArray());
        Assert.False(File.Exists(this.PathOf("sprites/normal-map-ui.png")));
    }

    /// <summary>The normal-map atlas takes the pixel test of the color atlas (D-184, F-19).</summary>
    [Fact]
    public void TheCheckFailsOnAChangedNormalMap()
    {
        this.Write("one");
        this.Run();
        PngImage flat = PngReader.ReadFile(this.PathOf("sprites/normal-map-map_sprites.png"));
        byte[] pixels = flat.Pixels.ToArray();
        pixels[0] = 200;
        PngWriter.WriteFile(this.PathOf("sprites/normal-map-map_sprites.png"), new PngImage(flat.Width, flat.Height, flat.Colors, pixels));

        var errors = new StringWriter();
        int code = AtlasCommand.Run([AtlasCommand.RootOption, this.root, AtlasCommand.CheckOption], new StringWriter(), errors);

        Assert.Equal(Program.FaultExitCode, code);
        Assert.Contains("sprites/normal-map-map_sprites.png", errors.ToString());
        Assert.Contains("the first different pixel is at x 0, y 0", errors.ToString());
    }

    [Fact]
    public void TheCheckFailsOnAnAbsentNormalMap()
    {
        this.Write("one");
        this.Run();
        File.Delete(this.PathOf("sprites/normal-map-map_sprites.png"));

        var errors = new StringWriter();
        int code = AtlasCommand.Run([AtlasCommand.RootOption, this.root, AtlasCommand.CheckOption], new StringWriter(), errors);

        Assert.Equal(Program.FaultExitCode, code);
        Assert.Contains("the page 'sprites/normal-map-map_sprites.png' is absent", errors.ToString());
    }

    /// <summary>The override grid reaches the committed normal map (D-839).</summary>
    [Fact]
    public void AnOverrideGridChangesTheNormalMapOfItsPixel()
    {
        this.Write("one");
        this.WriteFile(DrawingFixtures.OverridePathOf("one"), DrawingFixtures.OverrideBody("one", OverrideRows('6')));

        this.Run();

        PngImage normals = PngReader.ReadFile(this.PathOf("sprites/normal-map-map_sprites.png"));
        (int red, int green, int blue) = NormalMap.Encode(1, 0, 1);
        Assert.Equal([(byte)red, (byte)green, (byte)blue, 255], normals.Row(0)[..4].ToArray());
    }

    [Fact]
    public void AnOverrideGridOfNoDrawingFails()
    {
        this.Write("one");
        this.WriteFile(DrawingFixtures.OverridePathOf("absent"), DrawingFixtures.OverrideBody("absent", OverrideRows('5')));

        var errors = new StringWriter();
        int code = AtlasCommand.Run([AtlasCommand.RootOption, this.root], new StringWriter(), errors);

        Assert.Equal(Program.FaultExitCode, code);
        Assert.Contains("drawing.absent", errors.ToString());
    }

    [Fact]
    public void TheCommandRemovesANormalMapThatNoPageNeeds()
    {
        this.Write("one");
        File.WriteAllBytes(this.PathOf("sprites/normal-map-tiles.png"), PngWriter.Write(new PngImage(1, 1, PngColorKind.Rgba, new byte[4])));

        var output = new StringWriter();
        AtlasCommand.Run([AtlasCommand.RootOption, this.root], output, new StringWriter());

        Assert.False(File.Exists(this.PathOf("sprites/normal-map-tiles.png")));
        Assert.Contains("removed sprites/normal-map-tiles.png", output.ToString());
    }

    [Fact]
    public void TheCommandWritesTheNormalSheetOfALitKind()
    {
        this.Write("one");
        string sheets = Path.Combine(this.root, "sheets");

        this.Run(AtlasCommand.SheetsOption, sheets);

        Assert.True(File.Exists(Path.Combine(sheets, "review-normals-map_sprites.png")));
    }

    [Fact]
    public void AnUnknownOptionFails()
    {
        var errors = new StringWriter();

        int code = AtlasCommand.Run(["--pages"], new StringWriter(), errors);

        Assert.Equal(Program.FaultExitCode, code);
        Assert.Contains("--pages", errors.ToString());
    }

    /// <summary>An empty option value reads at the parse, and never as a stack trace (T-2).</summary>
    [Theory]
    [InlineData(AtlasCommand.RootOption)]
    [InlineData(AtlasCommand.SheetsOption)]
    public void AnEmptyOptionValueFails(string option)
    {
        this.Write("one");
        var errors = new StringWriter();
        string[] args = option == AtlasCommand.RootOption
            ? [option, string.Empty]
            : [AtlasCommand.RootOption, this.root, option, string.Empty];

        int code = AtlasCommand.Run(args, new StringWriter(), errors);

        Assert.Equal(Program.FaultExitCode, code);
        Assert.Contains($"the value of the option {option} is empty", errors.ToString());
    }

    [Fact]
    public void AnAbsentContentFolderFails()
    {
        var errors = new StringWriter();

        int code = AtlasCommand.Run([AtlasCommand.RootOption, this.root], new StringWriter(), errors);

        Assert.Equal(Program.FaultExitCode, code);
        Assert.Contains("content", errors.ToString());
    }

    /// <summary>The rows of a grid of 32 by 32 pixels with one digit at the top left pixel.</summary>
    private static List<string> OverrideRows(char digit)
    {
        var rows = new List<string> { digit + new string('.', 31) };
        for (int row = 1; row < 32; row += 1)
        {
            rows.Add(new string('.', 32));
        }

        return rows;
    }

    private int Run(params string[] options)
    {
        List<string> args = [AtlasCommand.RootOption, this.root, .. options];
        return AtlasCommand.Run(args, new StringWriter(), new StringWriter());
    }

    private void Write(params string[] names) => this.Write(names, 'k');

    private void Write(string name, char key) => this.Write([name], key);

    private void Write(IReadOnlyList<string> names, char key)
    {
        this.WriteFile(Palette.Path, DrawingFixtures.PaletteBody);
        foreach (string name in names)
        {
            this.WriteFile(DrawingFixtures.PathOf(name), DrawingFixtures.Body(name, key: key));
        }
    }

    private void WriteFile(string path, string body)
    {
        string full = this.PathOf(path);
        Directory.CreateDirectory(Path.GetDirectoryName(full) ?? this.root);
        File.WriteAllText(full, body);
    }

    private string PathOf(string path) =>
        Path.Combine(this.root, "content", path.Replace('/', Path.DirectorySeparatorChar));
}
