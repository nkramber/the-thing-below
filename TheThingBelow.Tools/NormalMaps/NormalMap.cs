using System;
using System.Collections.Generic;
using TheThingBelow.Core.Content;
using TheThingBelow.Tools.Atlas;
using TheThingBelow.Tools.Png;

namespace TheThingBelow.Tools.NormalMaps;

/// <summary>
/// Builds the normal map of each drawing from the shape of its grid (D-184). The height of a
/// pixel is its distance to the edge, up to the rim of D-840, plus the height of its palette
/// color (D-838). An override grid sets the direction of a pixel by hand (D-839).
/// </summary>
/// <remarks>
/// Every value is a whole number, and the one root is <see cref="IntegerRoot"/>, so every CI
/// leg gives the same pixels (D-502, F-38, T-7).
/// <para>
/// Only a transparent pixel makes an edge. The border of a frame makes none, so a floor of
/// tiles and a repeated piece show no seam where two copies meet (D-840).
/// </para>
/// </remarks>
public static class NormalMap
{
    /// <summary>
    /// The depth of the rim in pixels. The height climbs one step for each pixel from the
    /// edge up to this depth, and it stays flat inside it (D-840).
    /// </summary>
    public const int RimDepth = 4;

    /// <summary>
    /// The run of one slope: the two neighbors of a pixel lie two pixels apart, so the part
    /// toward the viewer of a built normal is 2 (D-184).
    /// </summary>
    public const int SlopeRun = 2;

    // The scale of the length of a normal before the root, so the rounding of the root moves
    // a channel by less than one step.
    private const long LengthScale = 4096;

    // The largest distance of a channel from its middle.
    private const int ChannelReach = 127;

    private const int ChannelMiddle = 128;

    /// <summary>Builds the normal map of one frame of a drawing.</summary>
    /// <param name="drawing">The drawing.</param>
    /// <param name="frame">The position of the frame, which starts at 0.</param>
    /// <param name="palette">The palette that gives the height of each key (D-838).</param>
    /// <param name="grid">The override grid of the frame, or null when the drawing has none.</param>
    /// <returns>The normal map of the frame.</returns>
    /// <exception cref="InvalidOperationException">A key of the frame is not in the palette (T-2).</exception>
    public static NormalFrame Build(Drawing drawing, int frame, Palette palette, NormalOverrideFrame? grid)
    {
        ArgumentNullException.ThrowIfNull(drawing);
        ArgumentNullException.ThrowIfNull(palette);

        IReadOnlyList<string> rows = drawing.Frames[frame].Rows;
        int[,] heights = HeightsOf(drawing, frame, palette);
        var normals = new NormalFrame(drawing.Width, drawing.Height);
        for (int y = 0; y < drawing.Height; y += 1)
        {
            for (int x = 0; x < drawing.Width; x += 1)
            {
                if (rows[y][x] == Drawing.Transparent)
                {
                    continue;
                }

                char direction = grid is null ? NormalOverride.Keep : grid.Rows[y][x];
                if (direction == NormalOverride.Keep)
                {
                    SetSlope(normals, heights, x, y);
                }
                else
                {
                    SetDirection(normals, x, y, direction);
                }
            }
        }

        return normals;
    }

    /// <summary>Draws the normal-map page of one page of the color atlas (D-184, D-517).</summary>
    /// <param name="page">The page of the color atlas, of a kind that takes scene light.</param>
    /// <param name="layout">The layout that names the place of each frame.</param>
    /// <param name="drawings">Every drawing of the layout, by its id.</param>
    /// <param name="palette">The palette that gives the height of each key.</param>
    /// <param name="overrides">Every override grid, by the id of its drawing.</param>
    /// <returns>The image of the page. Each frame sits at its place on the color page.</returns>
    /// <exception cref="ArgumentException">The page is of a kind that takes no scene light (D-210, T-2).</exception>
    public static PngImage RenderPage(
        AtlasPage page,
        AtlasLayout layout,
        IReadOnlyDictionary<string, Drawing> drawings,
        Palette palette,
        IReadOnlyDictionary<string, NormalOverride> overrides)
    {
        ArgumentNullException.ThrowIfNull(page);
        ArgumentNullException.ThrowIfNull(layout);
        ArgumentNullException.ThrowIfNull(drawings);
        ArgumentNullException.ThrowIfNull(palette);
        ArgumentNullException.ThrowIfNull(overrides);
        if (!AtlasPages.TakesLight(page.Kind))
        {
            throw new ArgumentException(
                $"The page '{page.Name}' takes no scene light, so it has no normal map (D-210).",
                nameof(page));
        }

        var canvas = new AtlasCanvas(page.Width, page.Height);
        foreach (AtlasEntry entry in layout.Entries)
        {
            if (string.CompareOrdinal(entry.Page, page.Name) != 0)
            {
                continue;
            }

            Drawing drawing = drawings[entry.Id.Value];
            overrides.TryGetValue(entry.Id.Value, out NormalOverride? grid);
            for (int frame = 0; frame < entry.Frames.Count; frame += 1)
            {
                NormalFrame normals = Build(drawing, frame, palette, grid?.Frames[frame]);
                Draw(canvas, normals, entry.Frames[frame].X, entry.Frames[frame].Y);
            }
        }

        return canvas.ToImage();
    }

    /// <summary>
    /// Gives the channels of a direction: the part to the right, the part up the screen, and
    /// the part toward the viewer, in any whole-number scale.
    /// </summary>
    /// <param name="right">The part to the right.</param>
    /// <param name="up">The part up the screen.</param>
    /// <param name="toward">The part toward the viewer, 1 or more.</param>
    /// <returns>The red, the green, and the blue channel, each from 1 to 255.</returns>
    public static (int Red, int Green, int Blue) Encode(int right, int up, int toward)
    {
        ArgumentOutOfRangeException.ThrowIfLessThan(toward, 1);

        long square = checked((((long)right * right) + ((long)up * up) + ((long)toward * toward)) * LengthScale * LengthScale);
        long length = IntegerRoot.Floor(square);
        return (ChannelOf(right, length), ChannelOf(up, length), ChannelOf(toward, length));
    }

    /// <summary>
    /// Gives the height of each pixel of a frame: the distance to the nearest transparent
    /// pixel, up to <see cref="RimDepth"/>, plus the height of the color (D-838, D-840).
    /// </summary>
    /// <param name="drawing">The drawing.</param>
    /// <param name="frame">The position of the frame.</param>
    /// <param name="palette">The palette that gives the height of each key.</param>
    /// <returns>The height of each pixel, by column and row. A transparent pixel holds 0.</returns>
    /// <exception cref="InvalidOperationException">A key of the frame is not in the palette (T-2).</exception>
    public static int[,] HeightsOf(Drawing drawing, int frame, Palette palette)
    {
        ArgumentNullException.ThrowIfNull(drawing);
        ArgumentNullException.ThrowIfNull(palette);

        IReadOnlyList<string> rows = drawing.Frames[frame].Rows;
        int[,] distances = EdgeDistances(rows, drawing.Width, drawing.Height);
        int[,] heights = new int[drawing.Width, drawing.Height];
        for (int y = 0; y < drawing.Height; y += 1)
        {
            for (int x = 0; x < drawing.Width; x += 1)
            {
                char key = rows[y][x];
                if (key == Drawing.Transparent)
                {
                    continue;
                }

                if (!palette.TryColorOf(key, out PaletteColor? color))
                {
                    throw new InvalidOperationException(
                        $"The file '{drawing.File}' holds the key '{key}' at frame {frame}, row {y}, column {x}, and the palette has no such color (F-20).");
                }

                heights[x, y] = distances[x, y] + color.Height;
            }
        }

        return heights;
    }

    /// <summary>
    /// Gives the count of steps from each pixel to the nearest transparent pixel, through the
    /// four sides, up to <see cref="RimDepth"/>. A transparent pixel holds 0.
    /// </summary>
    private static int[,] EdgeDistances(IReadOnlyList<string> rows, int width, int height)
    {
        int[,] distances = new int[width, height];
        for (int y = 0; y < height; y += 1)
        {
            for (int x = 0; x < width; x += 1)
            {
                distances[x, y] = rows[y][x] == Drawing.Transparent ? 0 : RimDepth;
            }
        }

        // Each pass moves the distance one pixel further in, so the rim needs one pass for
        // each step of its depth. The border of the frame is no edge (D-840).
        for (int pass = 0; pass < RimDepth; pass += 1)
        {
            int[,] before = (int[,])distances.Clone();
            for (int y = 0; y < height; y += 1)
            {
                for (int x = 0; x < width; x += 1)
                {
                    int nearest = Math.Min(
                        Math.Min(DistanceAt(before, x - 1, y), DistanceAt(before, x + 1, y)),
                        Math.Min(DistanceAt(before, x, y - 1), DistanceAt(before, x, y + 1)));
                    distances[x, y] = Math.Min(distances[x, y], nearest + 1);
                }
            }
        }

        return distances;
    }

    private static int DistanceAt(int[,] distances, int x, int y)
    {
        bool outside = x < 0 || y < 0 || x >= distances.GetLength(0) || y >= distances.GetLength(1);
        return outside ? RimDepth : distances[x, y];
    }

    private static void SetSlope(NormalFrame normals, int[,] heights, int x, int y)
    {
        // A neighbor outside the frame takes the height of the pixel itself, so the border
        // of a frame makes no slope (D-840).
        int here = heights[x, y];
        int left = HeightAt(heights, x - 1, y, here);
        int right = HeightAt(heights, x + 1, y, here);
        int above = HeightAt(heights, x, y - 1, here);
        int below = HeightAt(heights, x, y + 1, here);

        // A surface that rises to the right faces left, and a surface that rises down the
        // screen faces up.
        (int red, int green, int blue) = Encode(left - right, below - above, SlopeRun);
        normals.Set(x, y, red, green, blue);
    }

    private static int HeightAt(int[,] heights, int x, int y, int fallback)
    {
        bool outside = x < 0 || y < 0 || x >= heights.GetLength(0) || y >= heights.GetLength(1);
        return outside ? fallback : heights[x, y];
    }

    private static void SetDirection(NormalFrame normals, int x, int y, char direction)
    {
        // The digits sit as on a numpad: 7 8 9 on top, 4 5 6 in the middle, 1 2 3 at the
        // bottom, so each digit points the way of its key from 5 (D-839).
        int digit = direction - '0';
        int right = ((digit - 1) % 3) - 1;
        int up = ((digit - 1) / 3) - 1;
        (int red, int green, int blue) = Encode(right, up, 1);
        normals.Set(x, y, red, green, blue);
    }

    private static void Draw(AtlasCanvas canvas, NormalFrame normals, int left, int top)
    {
        for (int y = 0; y < normals.Height; y += 1)
        {
            for (int x = 0; x < normals.Width; x += 1)
            {
                if (normals.IsOpaque(x, y))
                {
                    canvas.Set(left + x, top + y, normals.Channel(x, y, 0), normals.Channel(x, y, 1), normals.Channel(x, y, 2));
                }
            }
        }
    }

    private static int ChannelOf(int part, long length)
    {
        // The division rounds half away from zero, so a part and its opposite give channels
        // at the same distance from the middle.
        long scaled = (long)part * ChannelReach * LengthScale;
        long half = length / 2;
        long step = scaled >= 0 ? (scaled + half) / length : -((-scaled + half) / length);
        return ChannelMiddle + (int)step;
    }
}
