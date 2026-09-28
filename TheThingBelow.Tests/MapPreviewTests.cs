using System;
using System.Collections.Generic;
using TheThingBelow.Core.Content;
using TheThingBelow.Core.Maps;
using TheThingBelow.Tools.Png;
using TheThingBelow.Tools.Preview;
using Xunit;

namespace TheThingBelow.Tests;

/// <summary>
/// The render of a map preview (D-165, D-1317). Each test decodes the PNG and compares its
/// pixels with the colors of the atlas of <see cref="PreviewFixtures"/>, never the bytes (F-19).
/// </summary>
public sealed class MapPreviewTests
{
    private const int Tile = MapPreview.TilePixels;

    [Fact]
    public void ThePreviewHasOneTileOfPixelsForEachTileOfTheMap()
    {
        PngImage image = Render();

        Assert.Equal((10 * Tile, 8 * Tile, PngColorKind.Rgba), (image.Width, image.Height, image.Colors));
    }

    [Fact]
    public void EachTileWithNoSpriteShowsTheDrawingOfItsKind()
    {
        PngImage image = Render();

        AssertCell(image, 7, 5, PreviewFixtures.Floor);
        AssertCell(image, 9, 7, PreviewFixtures.Wall);
        AssertCell(image, 5, 3, PreviewFixtures.Wall);
    }

    [Fact]
    public void AnEnemyATrapAndAThingDrawOnTheirTiles()
    {
        PngImage image = Render();

        AssertCell(image, 7, 6, PreviewFixtures.Enemy);
        AssertCell(image, 6, 2, PreviewFixtures.Trap);

        // The waystone draws its square over the floor, and the transparent pixels leave the floor.
        AssertPixel(image, (8 * Tile) + 12, (1 * Tile) + 24, PreviewFixtures.Save);
        AssertPixel(image, (8 * Tile) + 19, (1 * Tile) + 31, PreviewFixtures.Save);
        AssertPixel(image, (8 * Tile) + 11, (1 * Tile) + 24, PreviewFixtures.Floor);
        AssertPixel(image, 8 * Tile, 1 * Tile, PreviewFixtures.Floor);
    }

    [Fact]
    public void AnNpcThatFacesEastDrawsItsDrawingFlipped()
    {
        // The drawing holds its color in the west half. The map screen flips an NPC that faces
        // east, so the color shows in the east half (D-1138).
        PngImage image = Render();

        AssertPixel(image, (2 * Tile) + 15, (2 * Tile) + 5, PreviewFixtures.Floor);
        AssertPixel(image, (2 * Tile) + 16, (2 * Tile) + 5, PreviewFixtures.Npc);
        AssertPixel(image, (2 * Tile) + 31, (2 * Tile) + 31, PreviewFixtures.Npc);
    }

    [Fact]
    public void ATallSpriteDrawsUpFromItsFeetAndInFrontOfASpriteFurtherNorth()
    {
        // The torch hangs on the wall at (4, 4), so it covers the tiles (4, 3) and (4, 4). The tall
        // NPC stands at (4, 5), so it covers the tiles (4, 4) and (4, 5). Its feet lie further
        // south, so it draws in front of the torch on the tile that both cover (F-94, D-737).
        PngImage image = Render();

        AssertCell(image, 4, 3, PreviewFixtures.Torch);
        AssertCell(image, 4, 4, PreviewFixtures.Tall);
        AssertCell(image, 4, 5, PreviewFixtures.Tall);
    }

    [Fact]
    public void TheTopEdgeClipsASpriteThatReachesAboveTheMap()
    {
        // The torch on the north wall at (1, 0) is two tiles in height, so its top half lies
        // above the map.
        PngImage image = Render();

        AssertCell(image, 1, 0, PreviewFixtures.Torch);
        AssertCell(image, 0, 0, PreviewFixtures.Wall);
    }

    [Fact]
    public void TheSpawnPointAndThePartyDrawNothing()
    {
        // The spawn point of the hub map lies at (1, 1), and the preview marks no hidden part of
        // the map (D-1318).
        PngImage image = Render();

        AssertCell(image, 1, 1, PreviewFixtures.Floor);
    }

    [Fact]
    public void AMapThatNamesATileWithNoDrawingFailsWithTheMapAndTheId()
    {
        AtlasIndex atlas = PreviewFixtures.Index(PreviewFixtures.IndexText(withWall: false));

        ContentException fault = Assert.Throws<ContentException>(
            () => MapPreview.Render(PreviewFixtures.Map(), PreviewFixtures.Decor(), atlas, PreviewFixtures.Pages()));

        Assert.Contains("'map.hub_test'", fault.Message, StringComparison.Ordinal);
        Assert.Contains("'tile.wall'", fault.Message, StringComparison.Ordinal);
    }

    [Fact]
    public void AMapThatNamesAnNpcWithNoDrawingFailsWithTheMapAndTheId()
    {
        GameMap map = HubMaps.Of(npcs: HubMaps.Keeper);

        ContentException fault = Assert.Throws<ContentException>(
            () => MapPreview.Render(map, PreviewFixtures.Decor(), PreviewFixtures.Index(), PreviewFixtures.Pages()));

        Assert.Contains("'map.hub_test'", fault.Message, StringComparison.Ordinal);
        Assert.Contains("'npc.hub_keeper'", fault.Message, StringComparison.Ordinal);
    }

    [Fact]
    public void ADrawingOnAnAbsentPageFailsWithThePage()
    {
        SortedDictionary<string, PngImage> pages = PreviewFixtures.Pages();
        pages.Remove("pieces");

        ContentException fault = Assert.Throws<ContentException>(
            () => MapPreview.Render(PreviewFixtures.Map(), PreviewFixtures.Decor(), PreviewFixtures.Index(), pages));

        Assert.Contains("'pieces'", fault.Message, StringComparison.Ordinal);
        Assert.Contains("'drawing.preview_torch'", fault.Message, StringComparison.Ordinal);
    }

    [Fact]
    public void APixelOfPartialAlphaFailsWithThePixel()
    {
        // A drawing holds a transparent pixel or a color of the palette, so a partial alpha
        // means a broken page (D-1315).
        var tiles = new PreviewFixtures.Page(64, 32);
        tiles.Fill(0, 0, 64, 32, PreviewFixtures.Floor);
        tiles.Set(3, 4, 1, 2, 3, 128);
        SortedDictionary<string, PngImage> pages = PreviewFixtures.Pages();
        pages["tiles"] = tiles.ToImage();

        ContentException fault = Assert.Throws<ContentException>(
            () => MapPreview.Render(PreviewFixtures.Map(), PreviewFixtures.Decor(), PreviewFixtures.Index(), pages));

        Assert.Contains("the pixel 3,4", fault.Message, StringComparison.Ordinal);
        Assert.Contains("the alpha 128", fault.Message, StringComparison.Ordinal);
    }

    [Fact]
    public void TheDecorOfAnotherMapFails()
    {
        ContentException fault = Assert.Throws<ContentException>(
            () => MapPreview.Render(PreviewFixtures.Map(), PreviewFixtures.Decor("map.other"), PreviewFixtures.Index(), PreviewFixtures.Pages()));

        Assert.Contains("'map.other'", fault.Message, StringComparison.Ordinal);
        Assert.Contains("'map.hub_test'", fault.Message, StringComparison.Ordinal);
    }

    private static PngImage Render()
    {
        PngImage image = MapPreview.Render(PreviewFixtures.Map(), PreviewFixtures.Decor(), PreviewFixtures.Index(), PreviewFixtures.Pages());

        // The test reads the pixels of the decoded file, as the owner sees it (F-19).
        return PngReader.Read(PngWriter.Write(image), "the test preview");
    }

    private static void AssertCell(PngImage image, int column, int row, (byte R, byte G, byte B) color)
    {
        for (int y = row * Tile; y < (row + 1) * Tile; y += 1)
        {
            for (int x = column * Tile; x < (column + 1) * Tile; x += 1)
            {
                AssertPixel(image, x, y, color);
            }
        }
    }

    private static void AssertPixel(PngImage image, int x, int y, (byte R, byte G, byte B) color)
    {
        ReadOnlySpan<byte> row = image.Row(y);
        var found = (row[x * 4], row[(x * 4) + 1], row[(x * 4) + 2], row[(x * 4) + 3]);
        Assert.True(
            found == (color.R, color.G, color.B, byte.MaxValue),
            $"The pixel {x},{y} holds {found}, and the preview must give {color} at full alpha.");
    }
}
