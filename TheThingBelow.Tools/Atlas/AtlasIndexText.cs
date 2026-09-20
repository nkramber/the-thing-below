using System;
using System.Collections.Generic;
using System.Text;
using System.Text.Json;
using TheThingBelow.Core.Content;

namespace TheThingBelow.Tools.Atlas;

/// <summary>
/// Writes the text of the atlas index from a layout (D-666). The `atlas` command writes the
/// file, and the reader of Core reads it again (D-517).
/// </summary>
/// <remarks>
/// The writer holds no date and no path of the machine, so two runs over one set of drawing
/// files give the same bytes on every platform (T-7). The one line ending is `\n`, which the
/// Windows leg writes too.
/// </remarks>
public static class AtlasIndexText
{
    /// <summary>The note at the top of the file, which no session edits by hand (D-107).</summary>
    public const string Comment =
        "The place of every frame in the atlas (D-666). The atlas command of Tools writes " +
        "this file and each page beside it, and no session edits either one by hand (D-107). " +
        "A page holds one kind of drawing, and a tile page is a strict grid of 32 by 32 " +
        "cells (D-667). Run the command again after a change to a drawing file.";

    /// <summary>Gives the text of the atlas index file.</summary>
    /// <param name="layout">The layout that the command built.</param>
    /// <returns>The text of the file, with a line ending after the last line.</returns>
    public static string Write(AtlasLayout layout)
    {
        ArgumentNullException.ThrowIfNull(layout);

        var text = new StringBuilder();
        text.Append("{\n");
        text.Append($" \"comment\": {Quote(Comment)},\n");
        text.Append(" \"pages\": [\n");
        AppendList(text, PageLines(layout.Pages));
        text.Append(" ],\n");
        text.Append(" \"drawings\": [\n");
        AppendList(text, EntryLines(layout.Entries));
        text.Append(" ]\n");
        text.Append("}\n");
        return text.ToString();
    }

    private static List<string> PageLines(IReadOnlyList<AtlasPage> pages)
    {
        var lines = new List<string>(pages.Count);
        foreach (AtlasPage page in pages)
        {
            string kind = Quote(AtlasPages.NameOf(page.Kind));
            lines.Add($"  {{ \"kind\": {kind}, \"number\": {page.Number}, \"width\": {page.Width}, \"height\": {page.Height} }}");
        }

        return lines;
    }

    private static List<string> EntryLines(IReadOnlyList<AtlasEntry> entries)
    {
        var lines = new List<string>(entries.Count);
        foreach (AtlasEntry entry in entries)
        {
            var text = new StringBuilder();
            text.Append("  {\n");
            text.Append($"   \"id\": {Quote(entry.Id.Value)},\n");
            text.Append($"   \"page\": {Quote(entry.Page)},\n");
            text.Append($"   \"width\": {entry.Width},\n");
            text.Append($"   \"height\": {entry.Height},\n");
            text.Append("   \"draws\": [\n");
            AppendList(text, DrawLines(entry.Draws));
            text.Append("   ],\n");
            text.Append("   \"frames\": [\n");
            AppendList(text, FrameLines(entry.Frames));
            text.Append("   ]\n");
            text.Append("  }");
            lines.Add(text.ToString());
        }

        return lines;
    }

    private static List<string> DrawLines(IReadOnlyList<DrawingUse> draws)
    {
        var lines = new List<string>(draws.Count);
        foreach (DrawingUse draw in draws)
        {
            lines.Add($"    {{ \"content\": {Quote(draw.Content.Value)}, \"use\": {Quote(draw.Use)} }}");
        }

        return lines;
    }

    private static List<string> FrameLines(IReadOnlyList<AtlasFrame> frames)
    {
        var lines = new List<string>(frames.Count);
        foreach (AtlasFrame frame in frames)
        {
            lines.Add($"    {{ \"x\": {frame.X}, \"y\": {frame.Y}, \"ticks\": {frame.Ticks} }}");
        }

        return lines;
    }

    // An empty list appends nothing, and the caller then writes `[` and `]` on two lines.
    private static void AppendList(StringBuilder text, List<string> lines)
    {
        for (int index = 0; index < lines.Count; index += 1)
        {
            text.Append(lines[index]);
            text.Append(index + 1 < lines.Count ? ",\n" : "\n");
        }
    }

    // Every value of the index passes a form check, so no value needs an escape today. The
    // encoder runs anyway, because a later field could hold a name with a quote mark (T-2).
    private static string Quote(string value) => $"\"{JsonEncodedText.Encode(value)}\"";
}
