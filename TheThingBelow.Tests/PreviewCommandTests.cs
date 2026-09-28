using System;
using System.Collections.Generic;
using System.IO;
using TheThingBelow.Core.Content;
using TheThingBelow.Core.Edges;
using TheThingBelow.Core.Maps;
using TheThingBelow.Tools;
using TheThingBelow.Tools.Content;
using TheThingBelow.Tools.Png;
using TheThingBelow.Tools.Preview;
using Xunit;

namespace TheThingBelow.Tests;

/// <summary>
/// The `preview` command on the content of this checkout (D-165, D-1319). The tests decode each
/// preview and compare its pixels with the committed atlas and the map, never the bytes (F-19).
/// </summary>
public sealed class PreviewCommandTests : IDisposable
{
    private const int Tile = MapPreview.TilePixels;

    private const string HubId = "map.fixture_hub";

    private static readonly Lazy<ContentSet> Content =
        new(() => ContentSet.Load(ContentFolder.Read(RepositoryRoot.Find())));

    private readonly string folder = Path.Combine(Path.GetTempPath(), $"map-preview-{Guid.NewGuid():N}");

    /// <summary>Removes the output folder of the test.</summary>
    public void Dispose()
    {
        if (Directory.Exists(this.folder))
        {
            Directory.Delete(this.folder, recursive: true);
        }
    }

    [Fact]
    public void TheCommandRendersTheFixtureHubAsAPng()
    {
        // Exit test 1 of PR-52.
        (int code, string output, string errors) = this.Run("--map", HubId);

        Assert.True(code == 0, $"The command failed: {errors}");
        Assert.Contains("preview: maps 1.", output, StringComparison.Ordinal);
        PngImage image = PngReader.ReadFile(Path.Combine(this.folder, "fixture_hub.png"));
        GameMap map = Content.Value.Map(ContentId.Parse(HubId, "test", "map"));
        Assert.Equal((map.Width * Tile, map.Height * Tile), (image.Width, image.Height));
    }

    [Fact]
    public void EachTileOfTheFixtureHubWithNoSpriteMatchesTheAtlas()
    {
        // Exit test 2 of PR-52: each tile that no sprite covers holds the pixels of the drawing
        // of its kind, from the committed page.
        (int code, _, string errors) = this.Run("--map", HubId);
        Assert.True(code == 0, $"The command failed: {errors}");
        PngImage image = PngReader.ReadFile(Path.Combine(this.folder, "fixture_hub.png"));
        ContentSet content = Content.Value;
        GameMap map = content.Map(ContentId.Parse(HubId, "test", "map"));
        SortedDictionary<string, PngImage> pages = MapPreview.ReadPages(RepositoryRoot.Find(), content.Atlas);
        Assert.Empty(content.Light.DecorOf(map.Id).Pieces);

        bool[] covered = SpriteCells(map, content.Atlas);
        int compared = 0;
        for (int row = 0; row < map.Height; row += 1)
        {
            for (int column = 0; column < map.Width; column += 1)
            {
                if (covered[(row * map.Width) + column])
                {
                    continue;
                }

                AtlasEntry entry = content.Atlas.Entry(TileIds.Of(map.TileAt(new TilePoint(column, row))), TileIds.MapUse);
                AssertBlock(image, column * Tile, row * Tile, pages[entry.Page], entry);
                compared += 1;
            }
        }

        // The hub holds a few sprites, so nearly each tile takes the compare.
        Assert.True(compared > (map.Width * map.Height) - 20, $"The test compared {compared} tiles alone.");
    }

    [Fact]
    public void EachNpcAndEachDrawnThingOfTheFixtureHubMatchesTheAtlas()
    {
        // Exit test 2 of PR-52: each opaque pixel of a sprite shows at its place. The sprites of
        // the hub stand apart, so no sprite covers another.
        (int code, _, string errors) = this.Run("--map", HubId);
        Assert.True(code == 0, $"The command failed: {errors}");
        PngImage image = PngReader.ReadFile(Path.Combine(this.folder, "fixture_hub.png"));
        ContentSet content = Content.Value;
        GameMap map = content.Map(ContentId.Parse(HubId, "test", "map"));
        SortedDictionary<string, PngImage> pages = MapPreview.ReadPages(RepositoryRoot.Find(), content.Atlas);

        int sprites = 0;
        foreach (NpcState npc in MapState.Enter(map).Npcs.All)
        {
            AtlasEntry entry = content.Atlas.Entry(npc.Npc.Id, MapDrawings.FigureUse);
            AssertSprite(image, npc.At, pages[entry.Page], entry, flip: npc.Facing == StepDirection.East);
            sprites += 1;
        }

        foreach (MapThing thing in map.Things)
        {
            if (MapDrawings.Draws(thing.Kind))
            {
                AtlasEntry entry = content.Atlas.Entry(thing.Id, MapDrawings.ThingUse);
                AssertSprite(image, thing.At, pages[entry.Page], entry, flip: false);
                sprites += 1;
            }
        }

        Assert.True(sprites >= 5, $"The hub gave {sprites} sprites alone.");
    }

    [Fact]
    public void EachEdgePieceOfTheFixtureOverworldShowsOnItsTile()
    {
        // PR-53: the preview draws the edge file of each map (D-501). The last piece of a tile
        // draws over the others, so each of its opaque pixels shows.
        const string OverworldId = "map.fixture_overworld";
        (int code, _, string errors) = this.Run("--map", OverworldId);
        Assert.True(code == 0, $"The command failed: {errors}");
        PngImage image = PngReader.ReadFile(Path.Combine(this.folder, "fixture_overworld.png"));
        ContentSet content = Content.Value;
        GameMap map = content.Map(ContentId.Parse(OverworldId, "test", "map"));
        SortedDictionary<string, PngImage> pages = MapPreview.ReadPages(RepositoryRoot.Find(), content.Atlas);

        bool[] covered = SpriteCells(map, content.Atlas);
        int compared = 0;
        foreach (EdgeTile tile in content.Edges.EdgesOf(map.Id).Tiles)
        {
            if (!covered[(tile.At.Y * map.Width) + tile.At.X])
            {
                AtlasEntry entry = content.Atlas.Entry(tile.Pieces[^1], TileIds.MapUse);
                AssertSprite(image, tile.At, pages[entry.Page], entry, flip: false);
                compared += 1;
            }
        }

        // The lake of the fixture overworld gives 9 tiles with edge pieces, and no sprite covers one.
        Assert.True(compared >= 9, $"The test compared {compared} edge tiles alone.");
    }

    [Fact]
    public void TheCommandRendersEachMapWithNoMapOption()
    {
        (int code, string output, string errors) = this.Run();

        Assert.True(code == 0, $"The command failed: {errors}");
        int count = 0;
        foreach (GameMap map in Content.Value.Maps)
        {
            Assert.True(File.Exists(Path.Combine(this.folder, $"{map.Id.Name}.png")), $"The command wrote no preview of '{map.Id.Value}'.");
            count += 1;
        }

        Assert.Contains($"preview: maps {count}.", output, StringComparison.Ordinal);
        Assert.Equal(count, Directory.GetFiles(this.folder).Length);
    }

    [Fact]
    public void AnAbsentMapFailsWithTheId()
    {
        (int code, _, string errors) = this.Run("--map", "map.absent");

        Assert.Equal(Program.FaultExitCode, code);
        Assert.Contains("map.absent", errors, StringComparison.Ordinal);
        Assert.False(Directory.Exists(this.folder), "The command made the output folder for a map that does not exist.");
    }

    [Fact]
    public void AMalformedMapIdFailsWithTheId()
    {
        (int code, _, string errors) = this.Run("--map", "Fixture Hub");

        Assert.Equal(Program.FaultExitCode, code);
        Assert.Contains("'Fixture Hub'", errors, StringComparison.Ordinal);
    }

    [Fact]
    public void TheCommandWithNoOutputFolderFails()
    {
        var output = new StringWriter();
        var errors = new StringWriter();

        int code = Program.Run([PreviewCommand.Name, PreviewCommand.RootOption, RepositoryRoot.Find()], output, errors);

        Assert.Equal(Program.FaultExitCode, code);
        Assert.Contains(PreviewCommand.OutOption, errors.ToString(), StringComparison.Ordinal);
    }

    private (int Code, string Output, string Errors) Run(params string[] more)
    {
        var args = new List<string> { PreviewCommand.Name, PreviewCommand.RootOption, RepositoryRoot.Find(), PreviewCommand.OutOption, this.folder };
        args.AddRange(more);
        var output = new StringWriter();
        var errors = new StringWriter();

        int code = Program.Run([.. args], output, errors);
        return (code, output.ToString(), errors.ToString());
    }

    /// <summary>Marks each tile that a sprite of the map can cover: each tile from its feet up to the height of its drawing.</summary>
    private static bool[] SpriteCells(GameMap map, AtlasIndex atlas)
    {
        var covered = new bool[map.Width * map.Height];
        MapState start = MapState.Enter(map);
        foreach (PatrolState patrol in start.Patrols.All)
        {
            Cover(covered, map, patrol.At, patrol.Body.Side, atlas.Entry(patrol.Patrol.Id, MapDrawings.FigureUse));
        }

        foreach (NpcState npc in start.Npcs.All)
        {
            Cover(covered, map, npc.At, 1, atlas.Entry(npc.Npc.Id, MapDrawings.FigureUse));
        }

        foreach (MapThing thing in map.Things)
        {
            if (MapDrawings.Draws(thing.Kind))
            {
                Cover(covered, map, thing.At, 1, atlas.Entry(thing.Id, MapDrawings.ThingUse));
            }
        }

        return covered;
    }

    /// <summary>Marks the tiles of one sprite, from the south row of its body up to the height of its drawing.</summary>
    private static void Cover(bool[] covered, GameMap map, TilePoint at, int side, AtlasEntry entry)
    {
        int rows = Math.Max(side, (entry.Height + Tile - 1) / Tile);
        for (int row = at.Y + side - rows; row < at.Y + side; row += 1)
        {
            for (int column = at.X; column < at.X + ((entry.Width + Tile - 1) / Tile); column += 1)
            {
                if (row >= 0 && column < map.Width)
                {
                    covered[(row * map.Width) + column] = true;
                }
            }
        }
    }

    /// <summary>Asserts that each opaque pixel of the first frame of a drawing shows up from the feet of one tile.</summary>
    private static void AssertSprite(PngImage image, TilePoint at, PngImage page, AtlasEntry entry, bool flip)
    {
        int left = at.X * Tile;
        int top = ((at.Y + 1) * Tile) - entry.Height;
        for (int y = 0; y < entry.Height; y += 1)
        {
            for (int x = 0; x < entry.Width; x += 1)
            {
                int sourceX = entry.Frames[0].X + (flip ? entry.Width - 1 - x : x);
                (byte, byte, byte, byte) wanted = PixelOf(page, sourceX, entry.Frames[0].Y + y);
                if (wanted.Item4 != 0 && top + y >= 0)
                {
                    Assert.True(
                        PixelOf(image, left + x, top + y) == wanted,
                        $"The pixel {left + x},{top + y} of '{entry.Id.Value}' holds {PixelOf(image, left + x, top + y)}, and the atlas gives {wanted}.");
                }
            }
        }
    }

    /// <summary>Asserts that a block of the preview holds each pixel of the first frame of a drawing.</summary>
    private static void AssertBlock(PngImage image, int left, int top, PngImage page, AtlasEntry entry)
    {
        for (int y = 0; y < entry.Height; y += 1)
        {
            for (int x = 0; x < entry.Width; x += 1)
            {
                (byte, byte, byte, byte) wanted = PixelOf(page, entry.Frames[0].X + x, entry.Frames[0].Y + y);
                Assert.True(
                    PixelOf(image, left + x, top + y) == wanted,
                    $"The pixel {left + x},{top + y} holds {PixelOf(image, left + x, top + y)}, and the drawing '{entry.Id.Value}' gives {wanted}.");
            }
        }
    }

    private static (byte, byte, byte, byte) PixelOf(PngImage image, int x, int y)
    {
        ReadOnlySpan<byte> row = image.Row(y);
        return (row[x * 4], row[(x * 4) + 1], row[(x * 4) + 2], row[(x * 4) + 3]);
    }
}
