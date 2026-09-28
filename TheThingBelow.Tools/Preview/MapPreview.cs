using System;
using System.Collections.Generic;
using System.IO;
using TheThingBelow.Core.Content;
using TheThingBelow.Core.Light;
using TheThingBelow.Core.Maps;
using TheThingBelow.Tools.Content;
using TheThingBelow.Tools.Png;

namespace TheThingBelow.Tools.Preview;

/// <summary>
/// The render of one map as a PNG from the committed atlas, for the approval of the owner
/// (D-165, D-1317). The preview draws the tiles, then each sprite that the map screen draws at
/// the start of the map, at full light.
/// </summary>
/// <remarks>
/// The order follows the map screen of Game. The ground draws first, and each trap lies flat on
/// it (D-1238). Each other sprite draws up from the south edge of its body, and a sprite with a
/// south edge further south draws in front (F-94, D-737). Two sprites with one south edge keep
/// the order of the screen: the enemies, the NPCs, the drawn things, the decor pieces, and the
/// openings of the light shafts (D-844, D-924).
/// <para>
/// The preview draws no light, no shadow, no effect, no party, and no hidden part of the map
/// (D-1317, D-1318).
/// </para>
/// </remarks>
public static class MapPreview
{
    /// <summary>The width and the height of one tile, in pixels (D-228, D-667).</summary>
    public const int TilePixels = AtlasPages.TileSize;

    private const int BytesPerPixel = 4;

    /// <summary>Reads each page of the atlas that the index names, from the content folder of a checkout (D-666).</summary>
    /// <param name="root">The root of the checkout.</param>
    /// <param name="index">The atlas index, which names each page.</param>
    /// <returns>Each page, by its name, as an RGBA image.</returns>
    /// <exception cref="PngException">A page is absent, broken, or not RGBA (T-2).</exception>
    public static SortedDictionary<string, PngImage> ReadPages(string root, AtlasIndex index)
    {
        ArgumentException.ThrowIfNullOrEmpty(root);
        ArgumentNullException.ThrowIfNull(index);

        var pages = new SortedDictionary<string, PngImage>(StringComparer.Ordinal);
        foreach (AtlasPage page in index.Pages)
        {
            string path = Path.Combine(root, ContentFolder.FolderName, page.File.Replace('/', Path.DirectorySeparatorChar));
            PngImage image = PngReader.ReadFile(path);
            if (image.Colors != PngColorKind.Rgba)
            {
                throw PngException.For(path, $"the page '{page.Name}' holds {image.Colors} pixels, and the preview reads RGBA pages alone (D-666)");
            }

            pages.Add(page.Name, image);
        }

        return pages;
    }

    /// <summary>Renders one map with its decor, at 1x (D-165, D-1317).</summary>
    /// <param name="map">The map.</param>
    /// <param name="decor">The decor file of the map (D-844).</param>
    /// <param name="atlas">The atlas index.</param>
    /// <param name="pages">Each page of the atlas, by its name, from <see cref="ReadPages"/>.</param>
    /// <returns>An RGBA image of <see cref="TilePixels"/> pixels for each tile of the map.</returns>
    /// <exception cref="ArgumentNullException">An argument is null (T-2).</exception>
    /// <exception cref="ContentException">
    /// The atlas holds no drawing of a tile or a sprite of the map, a page is absent, or a frame
    /// holds a pixel of partial alpha. The message names the map and the id (T-2).
    /// </exception>
    public static PngImage Render(GameMap map, DecorFile decor, AtlasIndex atlas, IReadOnlyDictionary<string, PngImage> pages)
    {
        ArgumentNullException.ThrowIfNull(map);
        ArgumentNullException.ThrowIfNull(decor);
        ArgumentNullException.ThrowIfNull(atlas);
        ArgumentNullException.ThrowIfNull(pages);

        if (string.CompareOrdinal(decor.Map.Value, map.Id.Value) != 0)
        {
            throw ContentException.ForFile(decor.File, $"the decor file serves '{decor.Map.Value}', and the preview draws '{map.Id.Value}' (T-2)");
        }

        var canvas = new Canvas(map, checked(map.Width * TilePixels), checked(map.Height * TilePixels), atlas, pages);
        for (int row = 0; row < map.Height; row += 1)
        {
            for (int column = 0; column < map.Width; column += 1)
            {
                TileKind kind = map.TileAt(new TilePoint(column, row));
                canvas.Draw(TileIds.Of(kind), TileIds.MapUse, column * TilePixels, row * TilePixels, flip: false);
            }
        }

        // A trap lies flat in the floor, so it draws over the ground and under each figure (D-1238).
        foreach (MapThing thing in map.Things)
        {
            if (thing.Kind == MapThingKind.Trap)
            {
                canvas.DrawStanding(thing.Id, MapDrawings.ThingUse, thing.At.X, FeetOf(thing.At.Y, 1), flip: false);
            }
        }

        foreach (Standing sprite in StandingSprites(map, decor))
        {
            canvas.DrawStanding(sprite.Content, sprite.Use, sprite.Column, sprite.Feet, sprite.Flip);
        }

        return canvas.ToImage();
    }

    /// <summary>
    /// Gives each sprite that stands on the map, in the order of the draw: by the south edge of
    /// its body, then in the order of the map screen (F-94, D-737).
    /// </summary>
    private static List<Standing> StandingSprites(GameMap map, DecorFile decor)
    {
        // The state at the entry of the map places each enemy on its station and each NPC on its
        // start tile, with each door and each chest shut (D-528, D-743).
        MapState start = MapState.Enter(map);
        var sprites = new List<Standing>();
        foreach (PatrolState patrol in start.Patrols.All)
        {
            sprites.Add(new Standing(patrol.Patrol.Id, MapDrawings.FigureUse, patrol.At.X, FeetOf(patrol.At.Y, patrol.Body.Side), Flip: false, sprites.Count));
        }

        foreach (NpcState npc in start.Npcs.All)
        {
            // An NPC that faces east draws its drawing flipped, as the map screen does.
            sprites.Add(new Standing(npc.Npc.Id, MapDrawings.FigureUse, npc.At.X, FeetOf(npc.At.Y, 1), npc.Facing == StepDirection.East, sprites.Count));
        }

        foreach (MapThing thing in map.Things)
        {
            if (MapDrawings.Draws(thing.Kind) && thing.Kind != MapThingKind.Trap)
            {
                sprites.Add(new Standing(thing.Id, MapDrawings.ThingUse, thing.At.X, FeetOf(thing.At.Y, 1), Flip: false, sprites.Count));
            }
        }

        foreach (DecorPiece piece in decor.Pieces)
        {
            sprites.Add(new Standing(piece.Kind, LightContent.MapUse, piece.Tile.X, FeetOf(piece.Tile.Y, 1), Flip: false, sprites.Count));
        }

        foreach (DecorPiece shaft in decor.Shafts)
        {
            sprites.Add(new Standing(shaft.Kind, LightContent.MapUse, shaft.Tile.X, FeetOf(shaft.Tile.Y, 1), Flip: false, sprites.Count));
        }

        // The sort of .NET is not stable, so the place in the list above breaks a tie of the south edge.
        sprites.Sort(static (first, second) => first.Feet != second.Feet
            ? first.Feet.CompareTo(second.Feet)
            : first.Order.CompareTo(second.Order));
        return sprites;
    }

    /// <summary>Gives the pixel of the south edge of a body that starts on one row (F-94, D-737).</summary>
    /// <param name="row">The north row of the body.</param>
    /// <param name="sideTiles">The count of tiles on one side of the body (D-206).</param>
    private static int FeetOf(int row, int sideTiles) => checked((row + sideTiles) * TilePixels);

    /// <summary>One sprite that stands on the map: its drawing, its west column, the pixel of its south edge, and its place in the order of the screen.</summary>
    private readonly record struct Standing(ContentId Content, string Use, int Column, int Feet, bool Flip, int Order);

    /// <summary>The pixels of one preview, and the atlas that fills them.</summary>
    private sealed class Canvas(GameMap map, int width, int height, AtlasIndex atlas, IReadOnlyDictionary<string, PngImage> pages)
    {
        private readonly byte[] pixels = new byte[checked(width * height * BytesPerPixel)];

        /// <summary>Draws a sprite up from the south edge of its body (F-94, D-737).</summary>
        public void DrawStanding(ContentId content, string use, int column, int feet, bool flip)
        {
            AtlasEntry entry = this.EntryOf(content, use);
            this.Draw(entry, column * TilePixels, feet - entry.Height, flip);
        }

        /// <summary>Draws the first frame of a drawing with its north-west corner on one pixel.</summary>
        public void Draw(ContentId content, string use, int left, int top, bool flip) =>
            this.Draw(this.EntryOf(content, use), left, top, flip);

        public PngImage ToImage() => new(width, height, PngColorKind.Rgba, this.pixels);

        private void Draw(AtlasEntry entry, int left, int top, bool flip)
        {
            if (!pages.TryGetValue(entry.Page, out PngImage? page))
            {
                throw ContentException.ForField(map.File, entry.Id.Value, $"the map '{map.Id.Value}' draws '{entry.Id.Value}' from the page '{entry.Page}', and the atlas has no such page (T-2)");
            }

            // The first frame is the look at rest, as the map screen draws it (D-666).
            AtlasFrame frame = entry.Frames[0];
            ReadOnlySpan<byte> source = page.Pixels;
            for (int y = 0; y < entry.Height; y += 1)
            {
                int targetY = top + y;
                if (targetY < 0 || targetY >= height)
                {
                    continue;
                }

                for (int x = 0; x < entry.Width; x += 1)
                {
                    int targetX = left + x;
                    if (targetX < 0 || targetX >= width)
                    {
                        continue;
                    }

                    int sourceX = frame.X + (flip ? entry.Width - 1 - x : x);
                    int from = (((frame.Y + y) * page.Width) + sourceX) * BytesPerPixel;
                    byte alpha = source[from + 3];
                    if (alpha == 0)
                    {
                        continue;
                    }

                    // A drawing holds a transparent pixel or a color of the palette, so a partial
                    // alpha means a broken page (D-181, D-1315).
                    if (alpha != byte.MaxValue)
                    {
                        throw ContentException.ForField(map.File, entry.Id.Value, $"the map '{map.Id.Value}' draws '{entry.Id.Value}', and the pixel {sourceX},{frame.Y + y} of the page '{entry.Page}' has the alpha {alpha} (T-2)");
                    }

                    int to = ((targetY * width) + targetX) * BytesPerPixel;
                    source.Slice(from, BytesPerPixel).CopyTo(this.pixels.AsSpan(to, BytesPerPixel));
                }
            }
        }

        private AtlasEntry EntryOf(ContentId content, string use)
        {
            if (!atlas.Draws(content, use))
            {
                throw ContentException.ForField(map.File, content.Value, $"the map '{map.Id.Value}' names '{content.Value}', and the atlas holds no drawing that draws it as '{use}' (D-519)");
            }

            return atlas.Entry(content, use);
        }
    }
}
