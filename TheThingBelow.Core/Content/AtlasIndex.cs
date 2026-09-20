using System;
using System.Collections.Generic;
using System.Diagnostics.CodeAnalysis;

namespace TheThingBelow.Core.Content;

/// <summary>One page of the atlas, as the index records it (D-666).</summary>
/// <param name="Kind">The kind of drawing that the page holds.</param>
/// <param name="Number">
/// The position of the page inside its kind, which starts at 1. A kind that fills its page
/// gains a second page (D-666).
/// </param>
/// <param name="Width">The width of the page in pixels.</param>
/// <param name="Height">The height of the page in pixels.</param>
public sealed record AtlasPage(AtlasPageKind Kind, int Number, int Width, int Height)
{
    /// <summary>The name of the page, such as `tiles` or `map_sprites-2`.</summary>
    public string Name => NameOf(this.Kind, this.Number);

    /// <summary>The path of the PNG of the page, under `content/`.</summary>
    public string File => $"sprites/atlas-{this.Name}.png";

    /// <summary>Gives the name of a page from its kind and its number.</summary>
    /// <param name="kind">The kind of the page.</param>
    /// <param name="number">The position inside the kind, which starts at 1.</param>
    /// <returns>The name, such as `tiles` or `map_sprites-2`.</returns>
    public static string NameOf(AtlasPageKind kind, int number)
    {
        ArgumentOutOfRangeException.ThrowIfLessThan(number, 1);

        string name = AtlasPages.NameOf(kind);
        return number == 1 ? name : $"{name}-{number}";
    }
}

/// <summary>The place of one frame on its page (D-666).</summary>
/// <param name="X">The pixel column of the left edge of the frame on the page.</param>
/// <param name="Y">The pixel row of the top edge of the frame on the page.</param>
/// <param name="Ticks">The time of the frame in ticks, as the drawing file holds it (D-669).</param>
public sealed record AtlasFrame(int X, int Y, int Ticks);

/// <summary>One drawing in the atlas, and the place of each of its frames (D-666).</summary>
/// <param name="Id">The id of the drawing, such as `drawing.marrek_map_front`.</param>
/// <param name="Page">The name of the page that holds every frame, such as `map_sprites`.</param>
/// <param name="Width">The width of one frame in pixels.</param>
/// <param name="Height">The height of one frame in pixels.</param>
/// <param name="Draws">Every thing that the drawing draws (D-519).</param>
/// <param name="Frames">Each frame, in the order that the animation plays them.</param>
public sealed record AtlasEntry(
    ContentId Id,
    string Page,
    int Width,
    int Height,
    IReadOnlyList<DrawingUse> Draws,
    IReadOnlyList<AtlasFrame> Frames);

/// <summary>
/// The place of every frame in the atlas (D-666). The `atlas` command writes this file
/// beside the pages, and no session edits it by hand (D-107).
/// </summary>
/// <remarks>
/// Core holds the record because Core holds the record of every content file, a file that no
/// rule reads included (D-517). Game reads the pages and this file from the resources of its
/// own assembly, and it finds each frame by a content id and a use (D-508, D-519).
/// <para>
/// A test decodes each committed page and compares its pixels with the drawing files, so a
/// stale atlas fails until the command runs again (F-19, G-24).
/// </para>
/// </remarks>
public sealed class AtlasIndex
{
    /// <summary>The path of the index file, under `content/`.</summary>
    public const string Path = "sprites/atlas-index.json";

    private readonly SortedDictionary<string, AtlasEntry> byId;
    private readonly SortedDictionary<string, AtlasEntry> byUse;

    private AtlasIndex(
        string comment,
        IReadOnlyList<AtlasPage> pages,
        IReadOnlyList<AtlasEntry> entries,
        SortedDictionary<string, AtlasEntry> byId,
        SortedDictionary<string, AtlasEntry> byUse)
    {
        this.Comment = comment;
        this.Pages = pages;
        this.Entries = entries;
        this.byId = byId;
        this.byUse = byUse;
    }

    /// <summary>The note at the top of the file, which names the command that wrote it.</summary>
    public string Comment { get; }

    /// <summary>Every page, in the order of the file.</summary>
    public IReadOnlyList<AtlasPage> Pages { get; }

    /// <summary>Every drawing, in the order of the file.</summary>
    public IReadOnlyList<AtlasEntry> Entries { get; }

    /// <summary>Reads the index from the bytes of its file.</summary>
    /// <param name="bytes">The bytes of the file, as UTF-8.</param>
    /// <param name="file">The path of the file, under `content/`, for each error (T-2).</param>
    /// <returns>The index.</returns>
    /// <exception cref="ContentException">The file breaks a rule of the reader (G-6, T-2).</exception>
    public static AtlasIndex Read(ReadOnlySpan<byte> bytes, string file)
    {
        var reader = new ContentReader(bytes, file);
        AtlasIndex index = Read(ref reader);
        reader.ReadFileEnd();
        return index;
    }

    /// <summary>Gives the page of one name.</summary>
    /// <param name="name">The name of the page, such as `tiles` or `map_sprites-2`.</param>
    /// <param name="found">The page, when the index holds the name.</param>
    /// <returns>True when the index holds a page with that name.</returns>
    public bool TryPage(string name, [NotNullWhen(true)] out AtlasPage? found)
    {
        ArgumentException.ThrowIfNullOrEmpty(name);

        found = null;
        foreach (AtlasPage page in this.Pages)
        {
            if (string.CompareOrdinal(page.Name, name) == 0)
            {
                found = page;
                return true;
            }
        }

        return false;
    }

    /// <summary>Gives the entry of one drawing id.</summary>
    /// <param name="id">The id of the drawing.</param>
    /// <returns>The entry.</returns>
    /// <exception cref="ContentException">The index holds no such drawing (T-2).</exception>
    public AtlasEntry Entry(ContentId id)
    {
        ArgumentNullException.ThrowIfNull(id);

        if (!this.byId.TryGetValue(id.Value, out AtlasEntry? entry))
        {
            throw ContentException.ForField(Path, id.Value, "the atlas index holds no drawing with this id");
        }

        return entry;
    }

    /// <summary>Gives the drawing that serves one thing and one use (D-519).</summary>
    /// <param name="content">The id of the thing, such as `cast.marrek`.</param>
    /// <param name="use">The part that the drawing serves, such as `portrait`.</param>
    /// <returns>The entry.</returns>
    /// <exception cref="ContentException">No drawing serves that thing and that use (T-2).</exception>
    public AtlasEntry Entry(ContentId content, string use)
    {
        ArgumentNullException.ThrowIfNull(content);
        ArgumentException.ThrowIfNullOrEmpty(use);

        string key = UseKey(content, use);
        if (!this.byUse.TryGetValue(key, out AtlasEntry? entry))
        {
            throw ContentException.ForField(
                Path,
                key,
                $"the atlas index holds no drawing that draws '{content.Value}' as '{use}' (D-519)");
        }

        return entry;
    }

    /// <summary>Tells whether the index holds a drawing for one thing and one use.</summary>
    /// <param name="content">The id of the thing.</param>
    /// <param name="use">The part that the drawing serves.</param>
    /// <returns>True when a drawing serves them.</returns>
    public bool Draws(ContentId content, string use)
    {
        ArgumentNullException.ThrowIfNull(content);
        ArgumentException.ThrowIfNullOrEmpty(use);

        return this.byUse.ContainsKey(UseKey(content, use));
    }

    private static string UseKey(ContentId content, string use) => $"{content.Value}:{use}";

    private static AtlasIndex Read(ref ContentReader reader)
    {
        string? comment = null;
        List<AtlasPage>? pages = null;
        List<AtlasEntry>? entries = null;

        int depth = reader.ReadObjectStart();
        while (reader.ReadNextField(depth, out string field))
        {
            switch (field)
            {
                case "comment":
                    comment = reader.ReadString();
                    break;
                case "pages":
                    pages = ReadPages(ref reader);
                    break;
                case "drawings":
                    entries = ReadEntries(ref reader);
                    break;
                default:
                    throw reader.UnknownField(field);
            }
        }

        List<AtlasPage> readPages = reader.Require(pages, depth, "pages");
        List<AtlasEntry> readEntries = reader.Require(entries, depth, "drawings");
        return Build(ref reader, depth, reader.Require(comment, depth, "comment"), readPages, readEntries);
    }

    private static List<AtlasPage> ReadPages(ref ContentReader reader)
    {
        var pages = new List<AtlasPage>();
        int depth = reader.ReadArrayStart();
        while (reader.ReadNextElement(depth, pages.Count))
        {
            pages.Add(ReadPage(ref reader));
        }

        return pages;
    }

    private static AtlasPage ReadPage(ref ContentReader reader)
    {
        AtlasPageKind? kind = null;
        int? number = null;
        int? width = null;
        int? height = null;

        int depth = reader.ReadObjectStart();
        while (reader.ReadNextField(depth, out string field))
        {
            switch (field)
            {
                case "kind":
                    kind = ReadKind(ref reader);
                    break;
                case "number":
                    number = ReadCount(ref reader, "the number of a page");
                    break;
                case "width":
                    width = ReadSize(ref reader);
                    break;
                case "height":
                    height = ReadSize(ref reader);
                    break;
                default:
                    throw reader.UnknownField(field);
            }
        }

        return new AtlasPage(
            reader.RequireValue(kind, depth, "kind"),
            reader.RequireInt(number, depth, "number"),
            reader.RequireInt(width, depth, "width"),
            reader.RequireInt(height, depth, "height"));
    }

    private static AtlasPageKind ReadKind(ref ContentReader reader)
    {
        string name = reader.ReadString();
        if (!AtlasPages.TryParse(name, out AtlasPageKind kind))
        {
            throw reader.Refuse($"the kind '{name}' is not one of {AtlasPages.Names()} (D-666)");
        }

        return kind;
    }

    private static int ReadCount(ref ContentReader reader, string what)
    {
        int value = reader.ReadInt();
        if (value < 1)
        {
            throw reader.Refuse($"{what} starts at 1, and this one is {value}");
        }

        return value;
    }

    private static int ReadSize(ref ContentReader reader)
    {
        int value = reader.ReadInt();
        if (value < 1 || value > AtlasPages.Size)
        {
            throw reader.Refuse(
                $"the size {value} is outside 1 to {AtlasPages.Size}, the size of an atlas page (D-666)");
        }

        return value;
    }

    private static int ReadPlace(ref ContentReader reader)
    {
        int value = reader.ReadInt();
        if (value < 0 || value >= AtlasPages.Size)
        {
            throw reader.Refuse(
                $"the place {value} is outside 0 to {AtlasPages.Size - 1}, the pixels of an atlas page (D-666)");
        }

        return value;
    }

    private static List<AtlasEntry> ReadEntries(ref ContentReader reader)
    {
        var entries = new List<AtlasEntry>();
        int depth = reader.ReadArrayStart();
        while (reader.ReadNextElement(depth, entries.Count))
        {
            entries.Add(ReadEntry(ref reader));
        }

        return entries;
    }

    private static AtlasEntry ReadEntry(ref ContentReader reader)
    {
        ContentId? id = null;
        string? page = null;
        int? width = null;
        int? height = null;
        List<DrawingUse>? draws = null;
        List<AtlasFrame>? frames = null;

        int depth = reader.ReadObjectStart();
        while (reader.ReadNextField(depth, out string field))
        {
            switch (field)
            {
                case "id":
                    id = reader.ReadContentId(Drawing.IdKind);
                    break;
                case "page":
                    page = reader.ReadString();
                    break;
                case "width":
                    width = ReadSize(ref reader);
                    break;
                case "height":
                    height = ReadSize(ref reader);
                    break;
                case "draws":
                    draws = ReadDraws(ref reader);
                    break;
                case "frames":
                    frames = ReadFrames(ref reader);
                    break;
                default:
                    throw reader.UnknownField(field);
            }
        }

        return new AtlasEntry(
            reader.Require(id, depth, "id"),
            reader.Require(page, depth, "page"),
            reader.RequireInt(width, depth, "width"),
            reader.RequireInt(height, depth, "height"),
            reader.Require(draws, depth, "draws"),
            reader.Require(frames, depth, "frames"));
    }

    private static List<DrawingUse> ReadDraws(ref ContentReader reader)
    {
        var draws = new List<DrawingUse>();
        int depth = reader.ReadArrayStart();
        while (reader.ReadNextElement(depth, draws.Count))
        {
            draws.Add(ReadUse(ref reader));
        }

        return draws;
    }

    private static DrawingUse ReadUse(ref ContentReader reader)
    {
        ContentId? content = null;
        string? use = null;

        int depth = reader.ReadObjectStart();
        while (reader.ReadNextField(depth, out string field))
        {
            switch (field)
            {
                case "content":
                    content = reader.ReadContentId();
                    break;
                case "use":
                    // The same rule as the drawing file, so a stale index fails at load and
                    // not at the first lookup (D-519).
                    use = Drawing.ReadUseName(ref reader);
                    break;
                default:
                    throw reader.UnknownField(field);
            }
        }

        return new DrawingUse(
            reader.Require(content, depth, "content"),
            reader.Require(use, depth, "use"));
    }

    private static List<AtlasFrame> ReadFrames(ref ContentReader reader)
    {
        var frames = new List<AtlasFrame>();
        int depth = reader.ReadArrayStart();
        while (reader.ReadNextElement(depth, frames.Count))
        {
            frames.Add(ReadFrame(ref reader));
        }

        return frames;
    }

    private static AtlasFrame ReadFrame(ref ContentReader reader)
    {
        int? x = null;
        int? y = null;
        int? ticks = null;

        int depth = reader.ReadObjectStart();
        while (reader.ReadNextField(depth, out string field))
        {
            switch (field)
            {
                case "x":
                    x = ReadPlace(ref reader);
                    break;
                case "y":
                    y = ReadPlace(ref reader);
                    break;
                case "ticks":
                    ticks = Drawing.ReadTicks(ref reader);
                    break;
                default:
                    throw reader.UnknownField(field);
            }
        }

        return new AtlasFrame(
            reader.RequireInt(x, depth, "x"),
            reader.RequireInt(y, depth, "y"),
            reader.RequireInt(ticks, depth, "ticks"));
    }

    private static AtlasIndex Build(
        ref ContentReader reader,
        int depth,
        string comment,
        List<AtlasPage> pages,
        List<AtlasEntry> entries)
    {
        var sizes = new SortedDictionary<string, AtlasPage>(StringComparer.Ordinal);
        foreach (AtlasPage page in pages)
        {
            if (!sizes.TryAdd(page.Name, page))
            {
                throw reader.RefuseField(depth, "pages", $"the page '{page.Name}' is in the file two times");
            }
        }

        var byId = new SortedDictionary<string, AtlasEntry>(StringComparer.Ordinal);
        var byUse = new SortedDictionary<string, AtlasEntry>(StringComparer.Ordinal);
        for (int index = 0; index < entries.Count; index += 1)
        {
            AtlasEntry entry = entries[index];
            if (!byId.TryAdd(entry.Id.Value, entry))
            {
                throw reader.RefuseField(depth, $"drawings[{index}].id", $"the id '{entry.Id.Value}' is in the file two times");
            }

            if (!sizes.TryGetValue(entry.Page, out AtlasPage? page))
            {
                throw reader.RefuseField(depth, $"drawings[{index}].page", $"the file holds no page '{entry.Page}'");
            }

            RefuseFramesOutsidePage(ref reader, depth, index, entry, page);
            AddUses(ref reader, depth, index, entry, byUse);
        }

        return new AtlasIndex(comment, pages, entries, byId, byUse);
    }

    private static void RefuseFramesOutsidePage(
        ref ContentReader reader,
        int depth,
        int index,
        AtlasEntry entry,
        AtlasPage page)
    {
        if (entry.Frames.Count == 0)
        {
            throw reader.RefuseField(depth, $"drawings[{index}].frames", "a drawing holds one frame at least (D-515)");
        }

        for (int frame = 0; frame < entry.Frames.Count; frame += 1)
        {
            AtlasFrame place = entry.Frames[frame];
            bool inside = place.X + entry.Width <= page.Width && place.Y + entry.Height <= page.Height;
            if (!inside)
            {
                throw reader.RefuseField(
                    depth,
                    $"drawings[{index}].frames[{frame}]",
                    $"the frame at {place.X},{place.Y} of {entry.Width} by {entry.Height} pixels leaves the page '{page.Name}' of {page.Width} by {page.Height}");
            }
        }
    }

    private static void AddUses(
        ref ContentReader reader,
        int depth,
        int index,
        AtlasEntry entry,
        SortedDictionary<string, AtlasEntry> byUse)
    {
        for (int use = 0; use < entry.Draws.Count; use += 1)
        {
            DrawingUse draw = entry.Draws[use];
            string key = UseKey(draw.Content, draw.Use);
            if (!byUse.TryAdd(key, entry))
            {
                throw reader.RefuseField(
                    depth,
                    $"drawings[{index}].draws[{use}]",
                    $"the drawing '{byUse[key].Id.Value}' already draws '{draw.Content.Value}' as '{draw.Use}' (D-519)");
            }
        }
    }
}
