using System;
using System.Collections.Generic;
using System.IO;
using TheThingBelow.Core.Content;
using TheThingBelow.Tools.Atlas;
using TheThingBelow.Tools.Content;
using TheThingBelow.Tools.Png;
using Xunit;

namespace TheThingBelow.Tests;

/// <summary>
/// The committed atlas of this checkout matches its drawing files (D-107, G-24). The test
/// compares decoded pixels and never the bytes of a PNG, because the compressed bytes depend
/// on the encoder (F-19).
/// </summary>
/// <remarks>
/// A stale atlas fails here until the `atlas` command runs again. Each CI leg runs the test,
/// so the three legs and the Mac read the same pixels (D-481, T-7).
/// </remarks>
public sealed class CheckoutAtlasTests
{
    /// <summary>The count of colors of the palette (D-181, D-238).</summary>
    private const int PaletteSize = 64;

    [Fact]
    public void ThePaletteHoldsSixtyFourColors()
    {
        Palette palette = ReadArt(out _);

        Assert.Equal(PaletteSize, palette.Colors.Count);
    }

    /// <summary>The first 48 colors keep their keys and their indices (D-121, D-181).</summary>
    [Fact]
    public void TheFirstFortyEightColorsKeepTheirIndices()
    {
        Palette palette = ReadArt(out _);

        Assert.Equal("ink", palette.Colors[0].Name);
        Assert.Equal("k", palette.Colors[0].Key);
        Assert.Equal("void", palette.Colors[47].Name);
        Assert.Equal("~", palette.Colors[47].Key);
    }

    [Fact]
    public void EachCommittedPageMatchesTheDrawingFilesByPixel()
    {
        Palette palette = ReadArt(out List<Drawing> drawings);
        AtlasLayout layout = AtlasLayout.Build(drawings);
        SortedDictionary<string, Drawing> byId = AtlasCommand.ById(drawings);

        Assert.NotEmpty(layout.Pages);
        foreach (AtlasPage page in layout.Pages)
        {
            PngImage wanted = AtlasCommand.RenderPage(page, layout, byId, palette);
            PngImage committed = PngReader.ReadFile(PathOf(page.File));

            Assert.Equal(wanted.Width, committed.Width);
            Assert.Equal(wanted.Height, committed.Height);
            Assert.Equal(wanted.Colors, committed.Colors);
            Assert.True(
                wanted.Pixels.SequenceEqual(committed.Pixels),
                $"The page '{page.File}' does not match the drawing files. Run the atlas command again (G-24).");
        }
    }

    /// <summary>The committed index names every frame of every drawing (D-666).</summary>
    [Fact]
    public void TheCommittedIndexHoldsEachFrameOfEachDrawing()
    {
        Palette palette = ReadArt(out List<Drawing> drawings);
        AtlasIndex index = ReadIndex();

        Assert.NotEmpty(drawings);
        Assert.Equal(drawings.Count, index.Entries.Count);
        foreach (Drawing drawing in drawings)
        {
            AtlasEntry entry = index.Entry(drawing.Id);
            Assert.Equal(drawing.Width, entry.Width);
            Assert.Equal(drawing.Height, entry.Height);
            Assert.Equal(drawing.Frames.Count, entry.Frames.Count);
            Assert.True(index.TryPage(entry.Page, out AtlasPage? page));
            Assert.Equal(drawing.Page, page.Kind);
        }

        Assert.NotEmpty(palette.Colors);
    }

    [Fact]
    public void TheCommittedIndexMatchesTheLayoutOfTheDrawingFiles()
    {
        ReadArt(out List<Drawing> drawings);

        string wanted = AtlasIndexText.Write(AtlasLayout.Build(drawings));
        string committed = File.ReadAllText(PathOf(AtlasIndex.Path)).Replace("\r\n", "\n");

        Assert.Equal(wanted, committed);
    }

    /// <summary>Each thing that a drawing names takes one drawing for one use (D-519).</summary>
    [Fact]
    public void NoTwoDrawingsDrawOneThingForOneUse()
    {
        ReadArt(out List<Drawing> drawings);

        var seen = new SortedDictionary<string, string>(StringComparer.Ordinal);
        foreach (Drawing drawing in drawings)
        {
            foreach (DrawingUse use in drawing.Draws)
            {
                string key = $"{use.Content.Value}:{use.Use}";
                Assert.True(
                    seen.TryAdd(key, drawing.Id.Value),
                    $"'{drawing.Id.Value}' and '{seen.GetValueOrDefault(key)}' both draw {key} (D-519).");
            }
        }
    }

    /// <summary>The whole content set loads, so every rule that spans files holds (D-517).</summary>
    [Fact]
    public void TheContentSetOfTheCheckoutHoldsTheAtlasAndEveryDrawing()
    {
        ContentSet set = ContentSet.Load(ContentFolder.Read(RepositoryRoot.Find()));

        Assert.NotEmpty(set.Atlas.Pages);
        Assert.NotEmpty(set.Drawings);
    }

    private static Palette ReadArt(out List<Drawing> drawings) =>
        AtlasCommand.ReadArt(RepositoryRoot.Find(), out drawings);

    private static AtlasIndex ReadIndex() =>
        AtlasIndex.Read(File.ReadAllBytes(PathOf(AtlasIndex.Path)), AtlasIndex.Path);

    private static string PathOf(string path) => Path.Combine(
        RepositoryRoot.Find(),
        ContentFolder.FolderName,
        path.Replace('/', Path.DirectorySeparatorChar));
}
