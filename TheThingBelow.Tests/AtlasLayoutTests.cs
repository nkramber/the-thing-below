using System.Collections.Generic;
using TheThingBelow.Core.Content;
using TheThingBelow.Tools.Atlas;
using Xunit;

namespace TheThingBelow.Tests;

/// <summary>
/// The packer of the atlas (D-666, D-667). Two runs over one set of drawing files give one
/// layout, so every CI leg builds the same pages (T-7, G-4).
/// </summary>
public sealed class AtlasLayoutTests
{
    [Fact]
    public void EachKindTakesItsOwnPage()
    {
        AtlasLayout layout = AtlasLayout.Build(
        [
            DrawingFixtures.Solid("one", "map_sprites"),
            DrawingFixtures.Solid("two", "portraits", 64, 64),
            DrawingFixtures.Solid("three", "tiles"),
        ]);

        Assert.Equal(3, layout.Pages.Count);
        Assert.Equal(AtlasPageKind.Tiles, layout.Pages[0].Kind);
        Assert.Equal(AtlasPageKind.MapSprites, layout.Pages[1].Kind);
        Assert.Equal(AtlasPageKind.Portraits, layout.Pages[2].Kind);
    }

    [Fact]
    public void TheOrderOfTheDrawingsIsTheOrderOfTheirIds()
    {
        AtlasLayout first = AtlasLayout.Build(
            [DrawingFixtures.Solid("b"), DrawingFixtures.Solid("a")]);
        AtlasLayout second = AtlasLayout.Build(
            [DrawingFixtures.Solid("a"), DrawingFixtures.Solid("b")]);

        Assert.Equal("drawing.a", first.Entries[0].Id.Value);
        Assert.Equal(first.Entries[1].Frames[0].X, second.Entries[1].Frames[0].X);
    }

    /// <summary>A tile page is a grid of 32 by 32 cells, with no gap (D-667).</summary>
    [Fact]
    public void ATilePageHoldsTheStrictGrid()
    {
        var tiles = new List<Drawing>();
        for (int index = 0; index < AtlasPages.TileColumns + 2; index += 1)
        {
            tiles.Add(DrawingFixtures.Solid($"tile_{index:00}", "tiles"));
        }

        AtlasLayout layout = AtlasLayout.Build(tiles);

        Assert.Equal(0, layout.Entries[0].Frames[0].X);
        Assert.Equal(32, layout.Entries[1].Frames[0].X);
        Assert.Equal(0, layout.Entries[AtlasPages.TileColumns].Frames[0].X);
        Assert.Equal(32, layout.Entries[AtlasPages.TileColumns].Frames[0].Y);
    }

    /// <summary>A tile page keeps the grid width, so a new tile moves no other tile (D-667).</summary>
    [Fact]
    public void ATilePageKeepsTheFullWidthOfTheGrid()
    {
        AtlasLayout layout = AtlasLayout.Build([DrawingFixtures.Solid("tile_00", "tiles")]);

        AtlasPage page = Assert.Single(layout.Pages);
        Assert.Equal(AtlasPages.Size, page.Width);
        Assert.Equal(AtlasPages.TileSize, page.Height);
    }

    /// <summary>A page of few drawings makes a small PNG and a small diff (D-666).</summary>
    [Fact]
    public void APageOfSpritesIsOnlyAsLargeAsItsDrawingsNeed()
    {
        AtlasLayout layout = AtlasLayout.Build(
            [DrawingFixtures.Solid("a"), DrawingFixtures.Solid("b")]);

        AtlasPage page = Assert.Single(layout.Pages);
        Assert.Equal(64, page.Width);
        Assert.Equal(32, page.Height);
    }

    [Fact]
    public void EachFrameOfADrawingTakesItsOwnPlace()
    {
        AtlasLayout layout = AtlasLayout.Build([DrawingFixtures.Solid("walk", frames: 3)]);

        AtlasEntry entry = Assert.Single(layout.Entries);
        Assert.Equal(3, entry.Frames.Count);
        Assert.Equal(0, entry.Frames[0].X);
        Assert.Equal(32, entry.Frames[1].X);
        Assert.Equal(64, entry.Frames[2].X);
        Assert.Equal([1, 2, 3], new[] { entry.Frames[0].Ticks, entry.Frames[1].Ticks, entry.Frames[2].Ticks });
    }

    [Fact]
    public void ARowOfDrawingsWiderThanAPageStartsANewShelf()
    {
        var wide = new List<Drawing>();
        for (int index = 0; index < (AtlasPages.Size / 512) + 1; index += 1)
        {
            wide.Add(DrawingFixtures.Solid($"wide_{index:00}", "portraits", 512, 64));
        }

        AtlasLayout layout = AtlasLayout.Build(wide);

        AtlasPage page = Assert.Single(layout.Pages);
        Assert.Equal(128, page.Height);
        Assert.Equal(0, layout.Entries[^1].Frames[0].X);
        Assert.Equal(64, layout.Entries[^1].Frames[0].Y);
    }

    /// <summary>Every frame of one drawing sits on one page, because the index names one (D-666).</summary>
    [Fact]
    public void ADrawingThatDoesNotFitStartsANewPage()
    {
        var tall = new List<Drawing>();
        for (int index = 0; index < 3; index += 1)
        {
            tall.Add(DrawingFixtures.Solid($"tall_{index:00}", "portraits", 2048, 1024));
        }

        AtlasLayout layout = AtlasLayout.Build(tall);

        Assert.Equal(2, layout.Pages.Count);
        Assert.Equal("portraits", layout.Entries[0].Page);
        Assert.Equal("portraits-2", layout.Entries[2].Page);
    }
}
