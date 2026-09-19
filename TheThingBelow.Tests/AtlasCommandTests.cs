using System;
using System.Collections.Generic;
using System.IO;
using TheThingBelow.Core.Content;
using TheThingBelow.Tools;
using TheThingBelow.Tools.Atlas;
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

        int code = AtlasCommand.Run(
            [AtlasCommand.RootOption, this.root, option, string.Empty],
            new StringWriter(),
            errors);

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
