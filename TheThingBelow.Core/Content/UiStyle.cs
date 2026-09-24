using System;
using System.Collections.Generic;

namespace TheThingBelow.Core.Content;

/// <summary>One color of the UI, named by its role and drawn from a palette key (D-527).</summary>
/// <param name="Role">What the color paints, such as `text` or `panel_border`.</param>
/// <param name="Key">The one-character palette key of the color, such as `w` (D-181).</param>
public sealed record UiColor(string Role, string Key)
{
    /// <summary>The one character of the key, which the palette reads.</summary>
    public char KeyCharacter => this.Key[0];
}

/// <summary>The time of a notice on screen, in ticks of the world (D-994, D-995).</summary>
/// <param name="SlideTicks">The ticks of the slide in at the top edge.</param>
/// <param name="HoldTicks">The ticks that the whole line holds after the type-out.</param>
/// <param name="FadeTicks">The ticks of the fade out.</param>
public sealed record NoticeTiming(int SlideTicks, int HoldTicks, int FadeTicks);

/// <summary>One window frame of the UI, named by its role and drawn from the atlas (D-220).</summary>
/// <param name="Role">Where the frame draws, such as `window`.</param>
/// <param name="Drawing">The id of the drawing that holds the nine parts of the frame.</param>
public sealed record UiFrame(string Role, ContentId Drawing);

/// <summary>
/// The look of every menu, panel, and label: the two body sizes, the colors as palette keys,
/// and the id of each window frame drawing (D-527). Game builds the Godot `Theme` from this
/// file at load, and no theme resource file exists (G-6).
/// </summary>
/// <remarks>
/// Every number in the file is an integer, because the frame counts whole pixels (D-517,
/// T-1). The player picks the body size, and the title is <see cref="TitleScale"/> times the
/// body (D-707). No rule of the game reads this file, so the content hash covers none of it
/// (D-495, D-648).
/// </remarks>
public sealed class UiStyle
{
    /// <summary>The path of the style file, under `content/`.</summary>
    public const string Path = "ui/style.json";

    /// <summary>The role of the frame that a window draws (D-220).</summary>
    public const string WindowFrameRole = "window";

    private readonly SortedDictionary<string, UiColor> colors;
    private readonly SortedDictionary<string, UiFrame> frames;

    private UiStyle(
        string comment,
        int smallBody,
        int largeBody,
        int titleScale,
        int borderPixels,
        NoticeTiming notice,
        SortedDictionary<string, UiColor> colors,
        SortedDictionary<string, UiFrame> frames)
    {
        this.Notice = notice;
        this.Comment = comment;
        this.SmallBody = smallBody;
        this.LargeBody = largeBody;
        this.TitleScale = titleScale;
        this.BorderPixels = borderPixels;
        this.colors = colors;
        this.frames = frames;
    }

    /// <summary>The note at the top of the file.</summary>
    public string Comment { get; }

    /// <summary>The smaller body size, in frame pixels. It is the default above a fit of 1x (D-707).</summary>
    public int SmallBody { get; }

    /// <summary>The larger body size, in frame pixels. It is the default at a fit of 1x (D-707).</summary>
    public int LargeBody { get; }

    /// <summary>The count that multiplies a body size to give a title size (D-707).</summary>
    public int TitleScale { get; }

    /// <summary>The width of the border of a panel, in frame pixels.</summary>
    public int BorderPixels { get; }

    /// <summary>The time of a notice on screen (D-994).</summary>
    public NoticeTiming Notice { get; }

    /// <summary>Every color, in ordinal order of its role (F-39).</summary>
    public IEnumerable<UiColor> Colors => this.colors.Values;

    /// <summary>Every window frame, in ordinal order of its role (F-39).</summary>
    public IEnumerable<UiFrame> Frames => this.frames.Values;

    /// <summary>The title size that belongs to one body size, in frame pixels (D-707).</summary>
    /// <param name="body">The body size, which is <see cref="SmallBody"/> or <see cref="LargeBody"/>.</param>
    /// <returns>The title size.</returns>
    /// <exception cref="ContentException">The size is neither of the two body sizes (T-2).</exception>
    public int TitleSizeOf(int body)
    {
        if (body != this.SmallBody && body != this.LargeBody)
        {
            throw ContentException.ForField(
                Path,
                "body",
                $"the size {body} is neither body size of the file, which are {this.SmallBody} and {this.LargeBody} (D-707)");
        }

        return checked(body * this.TitleScale);
    }

    /// <summary>Gives the palette key of one color role.</summary>
    /// <param name="role">The role, such as `text`.</param>
    /// <returns>The color.</returns>
    /// <exception cref="ContentException">The file holds no color of that role (T-2).</exception>
    public UiColor ColorOf(string role)
    {
        ArgumentException.ThrowIfNullOrEmpty(role);

        if (!this.colors.TryGetValue(role, out UiColor? color))
        {
            throw ContentException.ForField(Path, role, "the style file holds no color with this role");
        }

        return color;
    }

    /// <summary>Gives the drawing id of one window frame role.</summary>
    /// <param name="role">The role, such as `window`.</param>
    /// <returns>The frame.</returns>
    /// <exception cref="ContentException">The file holds no frame of that role (T-2).</exception>
    public UiFrame FrameOf(string role)
    {
        ArgumentException.ThrowIfNullOrEmpty(role);

        if (!this.frames.TryGetValue(role, out UiFrame? frame))
        {
            throw ContentException.ForField(Path, role, "the style file holds no window frame with this role");
        }

        return frame;
    }

    /// <summary>Reads the style from the bytes of its file.</summary>
    /// <param name="bytes">The bytes of the file, as UTF-8.</param>
    /// <param name="file">The path of the file, under `content/`, for each error (T-2).</param>
    /// <returns>The style.</returns>
    /// <exception cref="ContentException">The file breaks a rule of the reader (G-6, T-2).</exception>
    public static UiStyle Read(ReadOnlySpan<byte> bytes, string file)
    {
        var reader = new ContentReader(bytes, file);
        UiStyle style = Read(ref reader);
        reader.ReadFileEnd();
        return style;
    }

    private static UiStyle Read(ref ContentReader reader)
    {
        string? comment = null;
        int? smallBody = null;
        int? largeBody = null;
        int? titleScale = null;
        int? borderPixels = null;
        NoticeTiming? notice = null;
        SortedDictionary<string, UiColor>? colors = null;
        SortedDictionary<string, UiFrame>? frames = null;

        int depth = reader.ReadObjectStart();
        while (reader.ReadNextField(depth, out string field))
        {
            switch (field)
            {
                case "comment":
                    comment = reader.ReadString();
                    break;
                case "small_body":
                    smallBody = reader.ReadInt();
                    break;
                case "large_body":
                    largeBody = reader.ReadInt();
                    break;
                case "title_scale":
                    titleScale = reader.ReadInt();
                    break;
                case "border_pixels":
                    borderPixels = reader.ReadInt();
                    break;
                case "notice":
                    notice = ReadNotice(ref reader);
                    break;
                case "colors":
                    colors = ReadColors(ref reader);
                    break;
                case "frames":
                    frames = ReadFrames(ref reader);
                    break;
                default:
                    throw reader.UnknownField(field);
            }
        }

        return Build(
            ref reader,
            depth,
            reader.Require(comment, depth, "comment"),
            reader.RequireInt(smallBody, depth, "small_body"),
            reader.RequireInt(largeBody, depth, "large_body"),
            reader.RequireInt(titleScale, depth, "title_scale"),
            reader.RequireInt(borderPixels, depth, "border_pixels"),
            reader.Require(notice, depth, "notice"),
            reader.Require(colors, depth, "colors"),
            reader.Require(frames, depth, "frames"));
    }

    /// <summary>
    /// Checks the numbers of the file. The two body sizes differ, the smaller comes first,
    /// and each count is above zero, so no layout reads a size that draws nothing (T-2).
    /// </summary>
    private static UiStyle Build(
        ref ContentReader reader,
        int depth,
        string comment,
        int smallBody,
        int largeBody,
        int titleScale,
        int borderPixels,
        NoticeTiming notice,
        SortedDictionary<string, UiColor> colors,
        SortedDictionary<string, UiFrame> frames)
    {
        if (smallBody <= 0)
        {
            throw reader.RefuseField(depth, "small_body", $"the size is {smallBody}, and a size is above zero");
        }

        if (largeBody <= smallBody)
        {
            throw reader.RefuseField(
                depth,
                "large_body",
                $"the size is {largeBody}, and the small body is {smallBody}. The large body is the larger of the two (D-707)");
        }

        if (titleScale <= 1)
        {
            throw reader.RefuseField(
                depth,
                "title_scale",
                $"the count is {titleScale}, and a title is larger than its body (D-707)");
        }

        if (borderPixels <= 0)
        {
            throw reader.RefuseField(
                depth,
                "border_pixels",
                $"the width is {borderPixels}, and a panel draws a border of one pixel or more (D-220)");
        }

        if (colors.Count == 0)
        {
            throw reader.RefuseField(depth, "colors", "the style file holds no color, and the theme needs one for each role");
        }

        if (frames.Count == 0)
        {
            throw reader.RefuseField(depth, "frames", $"the style file holds no window frame, and a panel draws the '{WindowFrameRole}' frame (D-220)");
        }

        return new UiStyle(comment, smallBody, largeBody, titleScale, borderPixels, notice, colors, frames);
    }

    /// <summary>Reads the time of a notice. Each count is above zero, so each phase of a notice shows (D-994, T-2).</summary>
    private static NoticeTiming ReadNotice(ref ContentReader reader)
    {
        int? slide = null;
        int? hold = null;
        int? fade = null;

        int depth = reader.ReadObjectStart();
        while (reader.ReadNextField(depth, out string field))
        {
            switch (field)
            {
                case "slide_ticks":
                    slide = reader.ReadInt();
                    break;
                case "hold_ticks":
                    hold = reader.ReadInt();
                    break;
                case "fade_ticks":
                    fade = reader.ReadInt();
                    break;
                default:
                    throw reader.UnknownField(field);
            }
        }

        var timing = new NoticeTiming(
            reader.RequireInt(slide, depth, "slide_ticks"),
            reader.RequireInt(hold, depth, "hold_ticks"),
            reader.RequireInt(fade, depth, "fade_ticks"));
        if (timing.SlideTicks <= 0 || timing.HoldTicks <= 0 || timing.FadeTicks <= 0)
        {
            throw reader.Refuse(
                $"the notice takes {timing.SlideTicks}, {timing.HoldTicks}, and {timing.FadeTicks} ticks, and each phase of a notice takes one tick or more (D-994)");
        }

        return timing;
    }

    private static SortedDictionary<string, UiColor> ReadColors(ref ContentReader reader)
    {
        // An ordinal order, because the default order of .NET follows the culture of the
        // machine (F-39, G-4).
        var colors = new SortedDictionary<string, UiColor>(StringComparer.Ordinal);

        int depth = reader.ReadArrayStart();
        int index = 0;
        while (reader.ReadNextElement(depth, index))
        {
            UiColor color = ReadColor(ref reader, depth, index);
            if (!colors.TryAdd(color.Role, color))
            {
                throw reader.RefuseField(
                    depth,
                    $"colors[{index}].role",
                    $"the style file holds the role '{color.Role}' two times, and a role names one color");
            }

            index += 1;
        }

        return colors;
    }

    private static UiColor ReadColor(ref ContentReader reader, int listDepth, int index)
    {
        string? role = null;
        string? key = null;

        int depth = reader.ReadObjectStart();
        while (reader.ReadNextField(depth, out string field))
        {
            switch (field)
            {
                case "role":
                    role = reader.ReadString();
                    break;
                case "key":
                    key = reader.ReadString();
                    break;
                default:
                    throw reader.UnknownField(field);
            }
        }

        var color = new UiColor(
            reader.Require(role, depth, "role"),
            reader.Require(key, depth, "key"));

        if (color.Key.Length != 1)
        {
            throw reader.RefuseField(
                listDepth,
                $"colors[{index}].key",
                $"the key '{color.Key}' holds {color.Key.Length} characters, and a palette key is one character (D-515)");
        }

        return color;
    }

    private static SortedDictionary<string, UiFrame> ReadFrames(ref ContentReader reader)
    {
        var frames = new SortedDictionary<string, UiFrame>(StringComparer.Ordinal);

        int depth = reader.ReadArrayStart();
        int index = 0;
        while (reader.ReadNextElement(depth, index))
        {
            UiFrame frame = ReadFrame(ref reader);
            if (!frames.TryAdd(frame.Role, frame))
            {
                throw reader.RefuseField(
                    depth,
                    $"frames[{index}].role",
                    $"the style file holds the role '{frame.Role}' two times, and a role names one frame");
            }

            index += 1;
        }

        return frames;
    }

    private static UiFrame ReadFrame(ref ContentReader reader)
    {
        string? role = null;
        ContentId? drawing = null;

        int depth = reader.ReadObjectStart();
        while (reader.ReadNextField(depth, out string field))
        {
            switch (field)
            {
                case "role":
                    role = reader.ReadString();
                    break;
                case "drawing":
                    drawing = reader.ReadContentId(Drawing.IdKind);
                    break;
                default:
                    throw reader.UnknownField(field);
            }
        }

        return new UiFrame(
            reader.Require(role, depth, "role"),
            reader.Require(drawing, depth, "drawing"));
    }
}
