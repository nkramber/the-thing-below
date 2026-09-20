using System;
using System.Collections.Generic;
using System.Diagnostics.CodeAnalysis;
using TheThingBelow.Core.Content;

namespace TheThingBelow.Tools.Atlas;

/// <summary>
/// The place of every drawing on the pages of the atlas (D-666, D-667). The `atlas` command
/// builds the layout, renders each page from it, and writes the atlas index from it.
/// </summary>
/// <remarks>
/// The layout uses integer math alone, so every CI leg builds the same pages (D-502, T-7).
/// <para>
/// The order of the drawings is the ordinal order of their ids, so two runs over one set of
/// drawing files give one layout (G-4, F-39). A new drawing with an early id thus moves the
/// drawings after it on its page. The page of a kind holds that change, and the pages of the
/// other kinds do not change at all (D-666).
/// </para>
/// </remarks>
public sealed class AtlasLayout
{
    private AtlasLayout(IReadOnlyList<AtlasPage> pages, IReadOnlyList<AtlasEntry> entries)
    {
        this.Pages = pages;
        this.Entries = entries;
    }

    /// <summary>Every page, in the order of the kinds and then of the page numbers.</summary>
    public IReadOnlyList<AtlasPage> Pages { get; }

    /// <summary>Every drawing, in ordinal order of its id.</summary>
    public IReadOnlyList<AtlasEntry> Entries { get; }

    /// <summary>Builds the layout of a set of drawings.</summary>
    /// <param name="drawings">Every drawing of the content set, in any order.</param>
    /// <returns>The layout.</returns>
    /// <exception cref="InvalidOperationException">A drawing does not fit on one page (T-2).</exception>
    public static AtlasLayout Build(IReadOnlyList<Drawing> drawings)
    {
        ArgumentNullException.ThrowIfNull(drawings);

        var pages = new List<AtlasPage>();
        var entries = new List<AtlasEntry>();
        foreach (AtlasPageKind kind in KindsOf(drawings))
        {
            AddKind(kind, Ordered(drawings, kind), pages, entries);
        }

        return new AtlasLayout(pages, entries);
    }

    // The kinds come in the order of the enum, so the page list reads the same on every run.
    private static IReadOnlyList<AtlasPageKind> KindsOf(IReadOnlyList<Drawing> drawings)
    {
        var kinds = new List<AtlasPageKind>();
        foreach (AtlasPageKind kind in Enum.GetValues<AtlasPageKind>())
        {
            foreach (Drawing drawing in drawings)
            {
                if (drawing.Page == kind)
                {
                    kinds.Add(kind);
                    break;
                }
            }
        }

        return kinds;
    }

    private static List<Drawing> Ordered(IReadOnlyList<Drawing> drawings, AtlasPageKind kind)
    {
        var ofKind = new List<Drawing>();
        foreach (Drawing drawing in drawings)
        {
            if (drawing.Page == kind)
            {
                ofKind.Add(drawing);
            }
        }

        ofKind.Sort(static (first, second) => string.CompareOrdinal(first.Id.Value, second.Id.Value));
        return ofKind;
    }

    private static void AddKind(
        AtlasPageKind kind,
        List<Drawing> drawings,
        List<AtlasPage> pages,
        List<AtlasEntry> entries)
    {
        var shelf = new ShelfPacker(kind);
        foreach (Drawing drawing in drawings)
        {
            entries.Add(shelf.Place(drawing));
        }

        shelf.AddPages(pages);
    }

    /// <summary>
    /// The placement of one kind. A tile takes the next cell of the strict grid of D-667.
    /// Every other drawing takes the next free place of a shelf, and a shelf is as tall as
    /// its tallest drawing.
    /// </summary>
    private sealed class ShelfPacker(AtlasPageKind kind)
    {
        /// <summary>The count of cells of one tile page, which is the grid of D-667.</summary>
        private const int CellsPerPage = AtlasPages.TileColumns * AtlasPages.TileColumns;

        private readonly List<int> pageHeights = [];
        private readonly List<int> pageWidths = [];
        private int number = 1;
        private int usedX;
        private int shelfY;
        private int shelfX;
        private int shelfHeight;
        private int cells;

        /// <summary>Places every frame of one drawing, and gives its index entry.</summary>
        /// <param name="drawing">The drawing to place.</param>
        /// <returns>The entry of the atlas index for this drawing.</returns>
        /// <exception cref="InvalidOperationException">The drawing fills more than one page (T-2).</exception>
        public AtlasEntry Place(Drawing drawing)
        {
            // Every frame of one drawing sits on one page, because the index gives one page
            // to each drawing (D-666). A drawing that does not fit in the rest of the page
            // starts the next page, and it leaves the rest of the old page empty.
            if (!this.TryPlace(drawing, out List<AtlasFrame>? frames))
            {
                this.StartPage();
                if (!this.TryPlace(drawing, out frames))
                {
                    throw new InvalidOperationException(
                        $"The drawing '{drawing.Id.Value}' holds {drawing.Frames.Count} frames of {drawing.Width} by {drawing.Height} pixels, and one page of {AtlasPages.Size} by {AtlasPages.Size} cannot hold them (D-666).");
                }
            }

            return new AtlasEntry(
                drawing.Id,
                AtlasPage.NameOf(kind, this.number),
                drawing.Width,
                drawing.Height,
                drawing.Draws,
                frames);
        }

        /// <summary>Adds each page of this kind, with the height that its drawings need.</summary>
        /// <param name="pages">The list that takes each page.</param>
        public void AddPages(List<AtlasPage> pages)
        {
            this.ClosePage();
            for (int index = 0; index < this.pageHeights.Count; index += 1)
            {
                pages.Add(new AtlasPage(kind, index + 1, this.pageWidths[index], this.pageHeights[index]));
            }
        }

        /// <summary>
        /// Tries to place every frame on the page that the packer holds now. A frame that
        /// leaves the page puts each field back, so a failed try changes nothing.
        /// </summary>
        private bool TryPlace(Drawing drawing, [NotNullWhen(true)] out List<AtlasFrame>? frames)
        {
            int savedY = this.shelfY;
            int savedX = this.shelfX;
            int savedHeight = this.shelfHeight;
            int savedCells = this.cells;
            int savedUsedX = this.usedX;

            var placed = new List<AtlasFrame>(drawing.Frames.Count);
            foreach (DrawingFrame frame in drawing.Frames)
            {
                if (!this.TryTake(drawing, frame.Ticks, out AtlasFrame? place))
                {
                    this.shelfY = savedY;
                    this.shelfX = savedX;
                    this.shelfHeight = savedHeight;
                    this.cells = savedCells;
                    this.usedX = savedUsedX;
                    frames = null;
                    return false;
                }

                placed.Add(place);
            }

            frames = placed;
            return true;
        }

        /// <summary>Takes the place of one frame, and gives false when the page is full.</summary>
        private bool TryTake(Drawing drawing, int ticks, [NotNullWhen(true)] out AtlasFrame? place)
        {
            if (kind == AtlasPageKind.Tiles)
            {
                return this.TryTakeCell(ticks, out place);
            }

            if (this.shelfX + drawing.Width > AtlasPages.Size)
            {
                this.shelfY += this.shelfHeight;
                this.shelfX = 0;
                this.shelfHeight = 0;
            }

            if (this.shelfY + drawing.Height > AtlasPages.Size)
            {
                place = null;
                return false;
            }

            place = new AtlasFrame(this.shelfX, this.shelfY, ticks);
            this.shelfX += drawing.Width;
            this.usedX = Math.Max(this.usedX, this.shelfX);
            this.shelfHeight = Math.Max(this.shelfHeight, drawing.Height);
            return true;
        }

        /// <summary>Takes the next cell of the strict tile grid of D-667.</summary>
        private bool TryTakeCell(int ticks, [NotNullWhen(true)] out AtlasFrame? place)
        {
            if (this.cells >= CellsPerPage)
            {
                place = null;
                return false;
            }

            int column = this.cells % AtlasPages.TileColumns;
            int row = this.cells / AtlasPages.TileColumns;
            this.cells += 1;

            // The height of a tile page is the count of its full rows, so a new tile appends
            // to the grid and moves no other tile (D-667).
            this.shelfY = (row + 1) * AtlasPages.TileSize;
            place = new AtlasFrame(column * AtlasPages.TileSize, row * AtlasPages.TileSize, ticks);
            return true;
        }

        private void StartPage()
        {
            this.ClosePage();
            this.number += 1;
            this.shelfY = 0;
            this.shelfX = 0;
            this.shelfHeight = 0;
            this.cells = 0;
            this.usedX = 0;
        }

        // A page is only as large as the drawings on it need, so a page of few drawings
        // makes a small PNG and a small diff in the git history (D-666). Each page closes
        // one time: the start of the next page closes it, and `AddPages` closes the last.
        // A tile page keeps the full width of the grid, so a new tile appends to the grid
        // and moves no other tile (D-667).
        private void ClosePage()
        {
            bool tiles = kind == AtlasPageKind.Tiles;
            this.pageHeights.Add(tiles ? this.shelfY : this.shelfY + this.shelfHeight);
            this.pageWidths.Add(tiles ? AtlasPages.Size : this.usedX);
        }
    }
}
