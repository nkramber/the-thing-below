using System;
using System.Collections.Generic;
using TheThingBelow.Core.Content;

namespace TheThingBelow.Tools.Atlas;

/// <summary>
/// The label font of the review sheets and the swatch sheet (D-668). Each glyph is 5 by 7
/// pixels, and the table below holds the seven rows of each one, separated by a slash.
/// </summary>
/// <remarks>
/// The font belongs to the tools alone. It labels a sheet that the owner reads in a pull
/// request, and no player ever sees it, so it is not a string of the string table (G-7).
/// The font of the game is a separate piece of work.
/// <para>
/// The table covers every key of the palette, every character of a content id, and the
/// punctuation of a label. A character that the table lacks fails with that character (T-2).
/// </para>
/// </remarks>
public static class SheetFont
{
    /// <summary>The width of one glyph in pixels.</summary>
    public const int GlyphWidth = 5;

    /// <summary>The height of one glyph in pixels.</summary>
    public const int GlyphHeight = 7;

    /// <summary>The pixels from the left edge of one glyph to the left edge of the next.</summary>
    public const int Advance = GlyphWidth + 1;

    /// <summary>The one character that a row of a glyph writes for a pixel of the label.</summary>
    public const char Ink = '#';

    private static readonly IReadOnlyDictionary<char, string> Glyphs =
        new SortedDictionary<char, string>
        {
            [' '] = "...../...../...../...../...../...../.....",
            ['!'] = "..#../..#../..#../..#../...../..#../.....",
            ['#'] = ".#.#./#####/.#.#./.#.#./#####/.#.#./.....",
            ['$'] = "..#../.####/#.#../.###./..#.#/####./..#..",
            ['%'] = "##..#/##.#./..#../.#.../#.##./#..##/.....",
            ['&'] = ".##../#..#./.##../#.#.#/#..#./.##.#/.....",
            ['*'] = "...../#.#.#/.###./#####/.###./#.#.#/.....",
            ['+'] = "...../..#../..#../#####/..#../..#../.....",
            [','] = "...../...../...../...../..##./..##./.##..",
            ['-'] = "...../...../...../#####/...../...../.....",
            ['.'] = "...../...../...../...../...../..##./..##.",
            ['/'] = "....#/...#./..#../..#../.#.../#..../.....",
            ['0'] = ".###./#..##/#.#.#/##..#/#...#/.###./.....",
            ['1'] = "..#../.##../..#../..#../..#../.###./.....",
            ['2'] = ".###./#...#/...#./..#../.#.../#####/.....",
            ['3'] = "####./....#/.###./....#/....#/####./.....",
            ['4'] = "#..#./#..#./#####/...#./...#./...#./.....",
            ['5'] = "#####/#..../####./....#/#...#/.###./.....",
            ['6'] = ".###./#..../####./#...#/#...#/.###./.....",
            ['7'] = "#####/....#/...#./..#../..#../..#../.....",
            ['8'] = ".###./#...#/.###./#...#/#...#/.###./.....",
            ['9'] = ".###./#...#/#...#/.####/....#/.###./.....",
            [':'] = "...../..##./..##./...../..##./..##./.....",
            ['='] = "...../...../#####/...../#####/...../.....",
            ['?'] = ".###./#...#/...#./..#../...../..#../.....",
            ['@'] = ".###./#...#/#.###/#.#.#/#.##./#..../.####",
            ['A'] = ".###./#...#/#...#/#####/#...#/#...#/.....",
            ['B'] = "####./#...#/####./#...#/#...#/####./.....",
            ['C'] = ".####/#..../#..../#..../#..../.####/.....",
            ['D'] = "####./#...#/#...#/#...#/#...#/####./.....",
            ['E'] = "#####/#..../####./#..../#..../#####/.....",
            ['F'] = "#####/#..../####./#..../#..../#..../.....",
            ['G'] = ".####/#..../#.###/#...#/#...#/.####/.....",
            ['H'] = "#...#/#...#/#####/#...#/#...#/#...#/.....",
            ['I'] = ".###./..#../..#../..#../..#../.###./.....",
            ['J'] = "..###/...#./...#./...#./#..#./.##../.....",
            ['K'] = "#...#/#..#./###../#..#./#...#/#...#/.....",
            ['L'] = "#..../#..../#..../#..../#..../#####/.....",
            ['M'] = "#...#/##.##/#.#.#/#...#/#...#/#...#/.....",
            ['N'] = "#...#/##..#/#.#.#/#..##/#...#/#...#/.....",
            ['O'] = ".###./#...#/#...#/#...#/#...#/.###./.....",
            ['P'] = "####./#...#/####./#..../#..../#..../.....",
            ['Q'] = ".###./#...#/#...#/#.#.#/#..#./.##.#/.....",
            ['R'] = "####./#...#/####./#.#../#..#./#...#/.....",
            ['S'] = ".####/#..../.###./....#/....#/####./.....",
            ['T'] = "#####/..#../..#../..#../..#../..#../.....",
            ['U'] = "#...#/#...#/#...#/#...#/#...#/.###./.....",
            ['V'] = "#...#/#...#/#...#/#...#/.#.#./..#../.....",
            ['W'] = "#...#/#...#/#...#/#.#.#/#.#.#/.#.#./.....",
            ['X'] = "#...#/.#.#./..#../..#../.#.#./#...#/.....",
            ['Y'] = "#...#/.#.#./..#../..#../..#../..#../.....",
            ['Z'] = "#####/...#./..#../.#.../#..../#####/.....",
            ['_'] = "...../...../...../...../...../...../#####",
            ['a'] = "...../...../.###./....#/.####/#...#/.####",
            ['b'] = "#..../#..../####./#...#/#...#/#...#/####.",
            ['c'] = "...../...../.####/#..../#..../#..../.####",
            ['d'] = "....#/....#/.####/#...#/#...#/#...#/.####",
            ['e'] = "...../...../.###./#...#/#####/#..../.###.",
            ['f'] = "..##./.#.../.#.../####./.#.../.#.../.#...",
            ['g'] = "...../...../.###./#...#/#...#/.####/###..",
            ['h'] = "#..../#..../####./#...#/#...#/#...#/#...#",
            ['i'] = "..#../...../.##../..#../..#../..#../.###.",
            ['j'] = "...#./...../..##./...#./...#./#..#./.##..",
            ['k'] = "#..../#..../#..#./#.#../##.../#.#../#..#.",
            ['l'] = ".##../..#../..#../..#../..#../..#../.###.",
            ['m'] = "...../...../##.#./#.#.#/#.#.#/#.#.#/#.#.#",
            ['n'] = "...../...../####./#...#/#...#/#...#/#...#",
            ['o'] = "...../...../.###./#...#/#...#/#...#/.###.",
            ['p'] = "...../...../####./#...#/####./#..../#....",
            ['q'] = "...../...../.####/#...#/.####/....#/....#",
            ['r'] = "...../...../#.##./##.../#..../#..../#....",
            ['s'] = "...../...../.####/#..../.###./....#/####.",
            ['t'] = ".#.../.#.../####./.#.../.#.../.#..#/..##.",
            ['u'] = "...../...../#...#/#...#/#...#/#..##/.##.#",
            ['v'] = "...../...../#...#/#...#/#...#/.#.#./..#..",
            ['w'] = "...../...../#.#.#/#.#.#/#.#.#/#.#.#/.#.#.",
            ['x'] = "...../...../#...#/.#.#./..#../.#.#./#...#",
            ['y'] = "...../...../#...#/#...#/.####/....#/.###.",
            ['z'] = "...../...../#####/...#./..#../.#.../#####",
            ['~'] = "...../...../.##.#/#..#./...../...../.....",
        };

    /// <summary>Gives the width of a label in pixels.</summary>
    /// <param name="text">The text of the label.</param>
    /// <returns>The count of pixels from the left edge to the right edge of the last glyph.</returns>
    public static int WidthOf(string text)
    {
        ArgumentNullException.ThrowIfNull(text);

        return text.Length == 0 ? 0 : ((text.Length - 1) * Advance) + GlyphWidth;
    }

    /// <summary>Tells whether the font holds a glyph for a character.</summary>
    /// <param name="character">The character to look for.</param>
    /// <returns>True when the font can draw it.</returns>
    public static bool Holds(char character) => Glyphs.ContainsKey(character);

    /// <summary>Draws a label on a canvas.</summary>
    /// <param name="canvas">The canvas that takes the label.</param>
    /// <param name="text">The text of the label.</param>
    /// <param name="x">The pixel column of the left edge of the first glyph.</param>
    /// <param name="y">The pixel row of the top edge of the label.</param>
    /// <param name="color">The color of every pixel of the label.</param>
    /// <exception cref="InvalidOperationException">The font holds no glyph for a character (T-2).</exception>
    public static void Draw(AtlasCanvas canvas, string text, int x, int y, PaletteColor color)
    {
        ArgumentNullException.ThrowIfNull(canvas);
        ArgumentNullException.ThrowIfNull(text);
        ArgumentNullException.ThrowIfNull(color);

        for (int index = 0; index < text.Length; index += 1)
        {
            char character = text[index];
            if (!Glyphs.TryGetValue(character, out string? glyph))
            {
                throw new InvalidOperationException(
                    $"The label '{text}' holds the character '{character}', and the sheet font has no glyph for it (T-2).");
            }

            DrawGlyph(canvas, glyph, x + (index * Advance), y, color);
        }
    }

    private static void DrawGlyph(AtlasCanvas canvas, string glyph, int x, int y, PaletteColor color)
    {
        int row = 0;
        int column = 0;
        foreach (char pixel in glyph)
        {
            if (pixel == '/')
            {
                row += 1;
                column = 0;
                continue;
            }

            if (pixel == Ink)
            {
                canvas.Set(x + column, y + row, color.Red, color.Green, color.Blue);
            }

            column += 1;
        }
    }
}
