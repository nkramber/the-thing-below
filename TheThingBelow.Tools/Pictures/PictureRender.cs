using System;
using System.Collections.Generic;
using TheThingBelow.Core.Content;
using TheThingBelow.Tools.Atlas;
using TheThingBelow.Tools.Png;

namespace TheThingBelow.Tools.Pictures;

/// <summary>
/// Renders one large picture into an image of its art pixels (D-516, D-518, D-816). The
/// review sheets and the store images of PR-76 read this render.
/// </summary>
/// <remarks>
/// Each entry draws in the order of the file, so a later entry covers an earlier one, and a
/// dot of a piece leaves the pixel under it. Each copy is clipped at the edge of the picture
/// (D-817). Every value is a whole number, and each color comes from the palette, so every CI
/// leg makes the same pixels (D-502, F-19).
/// </remarks>
public static class PictureRender
{
    /// <summary>Renders one picture of a content set.</summary>
    /// <param name="picture">The picture.</param>
    /// <param name="content">The content set, which holds the pieces and the palette.</param>
    /// <returns>The image, of the width and the height of the picture, with four channels.</returns>
    /// <exception cref="ArgumentNullException">An argument is null (T-2).</exception>
    /// <exception cref="ContentException">The set holds no drawing of a piece (T-2).</exception>
    /// <exception cref="InvalidOperationException">A key of a piece is not in the palette (T-2).</exception>
    public static PngImage Render(LargePicture picture, ContentSet content)
    {
        ArgumentNullException.ThrowIfNull(picture);
        ArgumentNullException.ThrowIfNull(content);

        var canvas = new AtlasCanvas(picture.Width, picture.Height);
        foreach (PictureCopy copy in PictureCopies.Of(picture, content))
        {
            DrawCopy(canvas, content.DrawingOf(copy.Piece), content.Palette, copy);
        }

        return canvas.ToImage();
    }

    /// <summary>Draws the part of the one frame of a piece that lies inside the picture (D-817).</summary>
    private static void DrawCopy(AtlasCanvas canvas, Drawing piece, Palette palette, PictureCopy copy)
    {
        IReadOnlyList<string> rows = piece.Frames[0].Rows;
        for (int row = 0; row < copy.Height; row += 1)
        {
            string keys = rows[row];
            for (int column = 0; column < copy.Width; column += 1)
            {
                char key = keys[column];
                if (key == Drawing.Transparent)
                {
                    continue;
                }

                if (!palette.TryColorOf(key, out PaletteColor? color))
                {
                    throw new InvalidOperationException(
                        $"The file '{piece.File}' holds the key '{key}' at row {row}, column {column}, and the palette has no such color (F-20).");
                }

                canvas.Set(copy.X + column, copy.Y + row, color.Red, color.Green, color.Blue);
            }
        }
    }
}
