using System;
using System.Collections.Generic;

namespace TheThingBelow.Core.Content;

/// <summary>One entry of a large picture: a piece, the place of its first copy, and its repeat (D-817).</summary>
/// <param name="Piece">The id of the drawing of the piece, on the `pieces` page (D-818).</param>
/// <param name="X">The art pixel column of the left edge of the first copy.</param>
/// <param name="Y">The art pixel row of the top edge of the first copy.</param>
/// <param name="Across">The count of copies in one row, edge to edge, 1 or more.</param>
/// <param name="Down">The count of rows of copies, edge to edge, 1 or more.</param>
public sealed record PicturePlace(ContentId Piece, int X, int Y, int Across, int Down);

/// <summary>
/// One large picture: a full-size image that places drawn pieces at art pixel positions,
/// with repeats (D-516, D-812, D-817). A backdrop layer is 640 by 360 art pixels, and Game
/// shows it at 2x like the world (D-816).
/// </summary>
/// <remarks>
/// The entries draw in the order of the file, so a later entry covers an earlier one, and a
/// dot of a piece leaves the pixel under it. The picture clips each copy at its edge (D-817).
/// A picture offers no mirror and no other operation on a piece (D-812).
/// <para>
/// The reader checks the shape of the file alone. The pieces need the drawings, and
/// <see cref="ContentSet"/> checks them over the whole set. No rule reads a picture, so a
/// picture never changes the content hash (D-495, D-517).
/// </para>
/// </remarks>
public sealed class LargePicture
{
    /// <summary>The folder of the large picture files, under `content/`.</summary>
    public const string Folder = "sprites/pictures/";

    /// <summary>The kind of the id of every large picture.</summary>
    public const string IdKind = "picture";

    /// <summary>
    /// The largest width and height of a picture, in art pixels. The Godot docs warn that a
    /// texture past 8192 by 8192 "may not be supported on older devices" (D-666).
    /// </summary>
    public const int MaxSize = 8192;

    private LargePicture(string file, ContentId id, int width, int height, IReadOnlyList<PicturePlace> places)
    {
        this.File = file;
        this.Id = id;
        this.Width = width;
        this.Height = height;
        this.Places = places;
    }

    /// <summary>The path of the file, under `content/`, which every error names (T-2).</summary>
    public string File { get; }

    /// <summary>The permanent id of the picture, such as `picture.fixture_backdrop`.</summary>
    public ContentId Id { get; }

    /// <summary>The count of art pixels in one row of the picture.</summary>
    public int Width { get; }

    /// <summary>The count of rows of art pixels of the picture.</summary>
    public int Height { get; }

    /// <summary>Every entry, in the order that they draw.</summary>
    public IReadOnlyList<PicturePlace> Places { get; }

    /// <summary>Tells whether a content path is a large picture file.</summary>
    /// <param name="path">The path under `content/`, with `/` separators.</param>
    /// <returns>True when the path lies in the picture folder.</returns>
    public static bool IsPictureFile(string path)
    {
        ArgumentNullException.ThrowIfNull(path);

        return path.StartsWith(Folder, StringComparison.Ordinal);
    }

    /// <summary>Reads one large picture from the bytes of its file.</summary>
    /// <param name="bytes">The bytes of the file, as UTF-8.</param>
    /// <param name="file">The path of the file, under `content/`, for each error (T-2).</param>
    /// <returns>The picture.</returns>
    /// <exception cref="ContentException">The file breaks a rule of the reader (G-6, T-2).</exception>
    public static LargePicture Read(ReadOnlySpan<byte> bytes, string file)
    {
        var reader = new ContentReader(bytes, file);
        LargePicture picture = Read(ref reader);
        reader.ReadFileEnd();
        return picture;
    }

    private static LargePicture Read(ref ContentReader reader)
    {
        ContentId? id = null;
        int? width = null;
        int? height = null;
        List<PicturePlace>? places = null;

        int depth = reader.ReadObjectStart();
        while (reader.ReadNextField(depth, out string field))
        {
            switch (field)
            {
                case "id":
                    id = reader.ReadContentId(IdKind);
                    break;
                case "width":
                    width = ReadSize(ref reader);
                    break;
                case "height":
                    height = ReadSize(ref reader);
                    break;
                case "places":
                    places = ReadPlaces(ref reader);
                    break;
                default:
                    throw reader.UnknownField(field);
            }
        }

        var picture = new LargePicture(
            reader.File,
            reader.Require(id, depth, "id"),
            reader.RequireInt(width, depth, "width"),
            reader.RequireInt(height, depth, "height"),
            reader.Require(places, depth, "places"));

        picture.RefuseWrongPlaces(ref reader, depth);
        return picture;
    }

    private static int ReadSize(ref ContentReader reader)
    {
        int value = reader.ReadInt();
        if (value < 1 || value > MaxSize)
        {
            throw reader.Refuse($"the size {value} is outside 1 to {MaxSize} art pixels (D-816)");
        }

        return value;
    }

    private static List<PicturePlace> ReadPlaces(ref ContentReader reader)
    {
        var places = new List<PicturePlace>();
        int depth = reader.ReadArrayStart();
        while (reader.ReadNextElement(depth, places.Count))
        {
            places.Add(ReadPlace(ref reader));
        }

        return places;
    }

    private static PicturePlace ReadPlace(ref ContentReader reader)
    {
        ContentId? piece = null;
        int? x = null;
        int? y = null;
        int? across = null;
        int? down = null;

        int depth = reader.ReadObjectStart();
        while (reader.ReadNextField(depth, out string field))
        {
            switch (field)
            {
                case "piece":
                    piece = reader.ReadContentId(Drawing.IdKind);
                    break;
                case "x":
                    x = reader.ReadInt();
                    break;
                case "y":
                    y = reader.ReadInt();
                    break;
                case "across":
                    across = ReadCount(ref reader);
                    break;
                case "down":
                    down = ReadCount(ref reader);
                    break;
                default:
                    throw reader.UnknownField(field);
            }
        }

        return new PicturePlace(
            reader.Require(piece, depth, "piece"),
            reader.RequireInt(x, depth, "x"),
            reader.RequireInt(y, depth, "y"),
            reader.RequireInt(across, depth, "across"),
            reader.RequireInt(down, depth, "down"));
    }

    private static int ReadCount(ref ContentReader reader)
    {
        int value = reader.ReadInt();
        if (value < 1 || value > MaxSize)
        {
            throw reader.Refuse($"the count {value} is outside 1 to {MaxSize}, and a place draws one copy at least (D-817)");
        }

        return value;
    }

    private void RefuseWrongPlaces(ref ContentReader reader, int depth)
    {
        if (this.Places.Count == 0)
        {
            throw reader.RefuseField(depth, "places", "a large picture places one piece at least, and this list is empty (D-516)");
        }

        for (int index = 0; index < this.Places.Count; index += 1)
        {
            PicturePlace place = this.Places[index];

            // The first copy starts inside the picture, because a copy that starts outside it
            // draws nothing (D-817, T-2).
            bool inside = place.X >= 0 && place.X < this.Width && place.Y >= 0 && place.Y < this.Height;
            if (!inside)
            {
                throw reader.RefuseField(
                    depth,
                    $"places[{index}]",
                    $"the first copy of '{place.Piece.Value}' starts at {place.X},{place.Y}, outside the picture of {this.Width} by {this.Height} (D-817)");
            }
        }
    }
}
