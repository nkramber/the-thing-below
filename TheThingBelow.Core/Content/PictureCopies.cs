using System;
using System.Collections.Generic;

namespace TheThingBelow.Core.Content;

/// <summary>One copy of a piece inside a large picture, clipped at the edge of the picture (D-817).</summary>
/// <param name="Piece">The id of the drawing of the piece.</param>
/// <param name="X">The art pixel column of the left edge of the copy.</param>
/// <param name="Y">The art pixel row of the top edge of the copy.</param>
/// <param name="Width">The count of columns of the piece that lie inside the picture, 1 or more.</param>
/// <param name="Height">The count of rows of the piece that lie inside the picture, 1 or more.</param>
public sealed record PictureCopy(ContentId Piece, int X, int Y, int Width, int Height);

/// <summary>
/// Gives every copy of every entry of a large picture, in the order that they draw (D-817).
/// The render of Tools and the draw of Game both read this list, so both clip at one edge.
/// </summary>
public static class PictureCopies
{
    /// <summary>Gives the copies of one picture of a content set.</summary>
    /// <param name="picture">The picture.</param>
    /// <param name="content">The content set, which holds the drawing of each piece.</param>
    /// <returns>Each copy, by the order of the entries, then row by row, then left to right.</returns>
    /// <exception cref="ArgumentNullException">An argument is null (T-2).</exception>
    /// <exception cref="ContentException">The set holds no drawing of a piece (T-2).</exception>
    /// <remarks>
    /// The load of the content set refuses a copy that starts outside the picture, so each
    /// copy here holds one pixel at least (D-817).
    /// </remarks>
    public static IReadOnlyList<PictureCopy> Of(LargePicture picture, ContentSet content)
    {
        ArgumentNullException.ThrowIfNull(picture);
        ArgumentNullException.ThrowIfNull(content);

        var copies = new List<PictureCopy>();
        foreach (PicturePlace place in picture.Places)
        {
            Drawing piece = content.DrawingOf(place.Piece);
            for (int down = 0; down < place.Down; down += 1)
            {
                for (int across = 0; across < place.Across; across += 1)
                {
                    int x = checked(place.X + (across * piece.Width));
                    int y = checked(place.Y + (down * piece.Height));
                    int width = Math.Min(piece.Width, picture.Width - x);
                    int height = Math.Min(piece.Height, picture.Height - y);
                    copies.Add(new PictureCopy(place.Piece, x, y, width, height));
                }
            }
        }

        return copies;
    }
}
