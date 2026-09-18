using System;
using System.Collections.Generic;
using Godot;

namespace ScreenScaleProbe;

/// <summary>Bakes the world of the mock frame into one image, from the map and the grids.</summary>
public sealed class MockFrame
{
    private static readonly Dictionary<char, string> MapKeys = new()
    {
        ['.'] = "snow",
        ['r'] = "snow-rock",
        ['f'] = "floor",
        ['s'] = "step",
        ['#'] = "wall",
        ['i'] = "ice",
    };

    private static readonly string[] Cast = ["marrek", "bergit", "dagvar", "ottild", "elio"];

    /// <summary>The tile column of the first member of the party.</summary>
    private const int PartyFirstColumn = 26;

    /// <summary>The tile row of the party.</summary>
    private const int PartyRow = 16;

    /// <summary>The tile gap between two members of the party.</summary>
    private const int PartyStep = 2;

    private MockFrame(ImageTexture texture, Vector2I center)
    {
        World = texture;
        PartyCenter = center;
    }

    /// <summary>The world of the frame at one art pixel for each texture pixel.</summary>
    public ImageTexture World { get; }

    /// <summary>The art pixel that the frame keeps at its center, at every world scale.</summary>
    public Vector2I PartyCenter { get; }

    /// <summary>Reads the palette, the map, the tiles, and the cast, and bakes the world.</summary>
    /// <param name="root">The folder of the project, with the slash at its end, such as res://.</param>
    public static MockFrame Bake(string root)
    {
        Dictionary<char, Color> palette = GridArt.LoadPalette($"{root}palette.json");
        string[] map = ReadMap($"{root}probe.map");
        var tiles = new Dictionary<char, string[]>();
        foreach (KeyValuePair<char, string> pair in MapKeys)
        {
            tiles[pair.Key] = GridArt.LoadGrid($"{root}tiles/{pair.Value}.grid", GridArt.TilePixels);
        }

        int tile = GridArt.TilePixels;
        Image image = Image.CreateEmpty(map[0].Length * tile, map.Length * tile, false, Image.Format.Rgba8);
        for (int row = 0; row < map.Length; row++)
        {
            for (int column = 0; column < map[row].Length; column++)
            {
                char key = map[row][column];
                if (!tiles.TryGetValue(key, out string[]? grid))
                {
                    throw new InvalidOperationException($"the map key \"{key}\" at row {row + 1} names no tile");
                }

                GridArt.StampGrid(image, grid, palette, column * tile, row * tile);
            }
        }

        for (int member = 0; member < Cast.Length; member++)
        {
            string[] sprite = GridArt.LoadGrid($"{root}sprites/{Cast[member]}.grid", GridArt.TilePixels);
            int column = PartyFirstColumn + (member * PartyStep);
            GridArt.StampGrid(image, sprite, palette, column * tile, PartyRow * tile);
        }

        int lastColumn = PartyFirstColumn + ((Cast.Length - 1) * PartyStep);
        var center = new Vector2I((PartyFirstColumn + lastColumn + 1) * tile / 2, (PartyRow * tile) + (tile / 2));
        return new MockFrame(ImageTexture.CreateFromImage(image), center);
    }

    /// <summary>The part of the world that the frame shows at one world scale.</summary>
    public Rect2I VisibleRegion(ScaleState state)
    {
        var size = new Vector2I(ScreenFacts.FrameWidth / state.World, ScreenFacts.FrameHeight / state.World);
        var origin = new Vector2I(PartyCenter.X - (size.X / 2), PartyCenter.Y - (size.Y / 2));
        var world = new Vector2I(World.GetWidth(), World.GetHeight());
        origin.X = Math.Clamp(origin.X, 0, Math.Max(0, world.X - size.X));
        origin.Y = Math.Clamp(origin.Y, 0, Math.Max(0, world.Y - size.Y));
        return new Rect2I(origin, size);
    }

    private static string[] ReadMap(string path)
    {
        using FileAccess? file = FileAccess.Open(path, FileAccess.ModeFlags.Read);
        if (file is null)
        {
            throw new InvalidOperationException($"cannot read the map at {path}: {FileAccess.GetOpenError()}");
        }

        string[] lines = file.GetAsText().Replace("\r", string.Empty).TrimEnd('\n').Split('\n');
        if (lines.Length == 0)
        {
            throw new InvalidOperationException($"the map at {path} is empty");
        }

        foreach (string line in lines)
        {
            if (line.Length != lines[0].Length)
            {
                throw new InvalidOperationException($"the map at {path} holds lines of two lengths");
            }
        }

        return lines;
    }
}
