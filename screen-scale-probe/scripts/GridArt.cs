using System;
using System.Collections.Generic;
using Godot;

namespace ScreenScaleProbe;

/// <summary>Reads the palette and the text grids, and bakes the map into one image.</summary>
public static class GridArt
{
    /// <summary>One tile of the map is 32 art pixels on a side (D-568).</summary>
    public const int TilePixels = 32;

    /// <summary>Reads the palette file. A dot is transparent, and every other key needs a color.</summary>
    public static Dictionary<char, Color> LoadPalette(string path)
    {
        string text = ReadText(path);
        var json = new Json();
        Error parsed = json.Parse(text);
        if (parsed != Error.Ok)
        {
            throw new InvalidOperationException(
                $"the palette at {path} is not valid JSON: line {json.GetErrorLine()}, {json.GetErrorMessage()}");
        }

        var root = json.Data.AsGodotDictionary();
        if (!root.ContainsKey("colors"))
        {
            throw new InvalidOperationException($"the palette at {path} holds no colors array");
        }

        var table = new Dictionary<char, Color>();
        foreach (Variant entry in root["colors"].AsGodotArray())
        {
            var row = entry.AsGodotDictionary();
            string key = row["key"].AsString();
            string hex = row["hex"].AsString();
            if (key.Length != 1)
            {
                throw new InvalidOperationException($"the palette key \"{key}\" is not one character");
            }

            table[key[0]] = new Color(hex);
        }

        return table;
    }

    /// <summary>Reads one grid file as lines of palette keys. Every line needs the same length.</summary>
    public static string[] LoadGrid(string path, int size)
    {
        string[] lines = ReadText(path).Replace("\r", string.Empty).TrimEnd('\n').Split('\n');
        if (lines.Length != size)
        {
            throw new InvalidOperationException($"the grid at {path} holds {lines.Length} lines, and it needs {size}");
        }

        for (int y = 0; y < lines.Length; y++)
        {
            if (lines[y].Length != size)
            {
                throw new InvalidOperationException(
                    $"line {y + 1} of the grid at {path} holds {lines[y].Length} keys, and it needs {size}");
            }
        }

        return lines;
    }

    /// <summary>Draws one grid into an image at an art-pixel position. A dot key draws nothing.</summary>
    public static void StampGrid(Image target, string[] grid, Dictionary<char, Color> palette, int originX, int originY)
    {
        for (int y = 0; y < grid.Length; y++)
        {
            for (int x = 0; x < grid[y].Length; x++)
            {
                char key = grid[y][x];
                if (key == '.')
                {
                    continue;
                }

                if (!palette.TryGetValue(key, out Color color))
                {
                    throw new InvalidOperationException($"the palette holds no key \"{key}\"");
                }

                target.SetPixel(originX + x, originY + y, color);
            }
        }
    }

    private static string ReadText(string path)
    {
        using FileAccess? file = FileAccess.Open(path, FileAccess.ModeFlags.Read);
        if (file is null)
        {
            throw new InvalidOperationException($"cannot read {path}: {FileAccess.GetOpenError()}");
        }

        return file.GetAsText();
    }
}
