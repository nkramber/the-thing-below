using System;
using System.Collections.Generic;

namespace TheThingBelow.Core.Content;

/// <summary>One entry of a fixture rule file (D-649).</summary>
/// <param name="Id">The permanent id of the entry, in the form of D-646.</param>
/// <param name="Label">The string id of the text that the player reads for this entry (G-7).</param>
/// <param name="Weight">A whole number, which the reader checks against the integer rule (G-2).</param>
public sealed record RuleFixtureEntry(ContentId Id, ContentId Label, int Weight);

/// <summary>
/// A fixture rule file. PR-5 ships it so that the content hash, the repeated-id rule, and
/// the string-id rule each read the real tree of `content/rules/` (D-649).
/// </summary>
/// <remarks>
/// The game reads no fixture. A later PR writes the first real rule record, such as the
/// enemy of PR-8 or the item of PR-13, and it removes this record and its files. The id of
/// each fixture entry is permanent, so no later entry takes one (D-166).
/// <para>
/// A fixture file lives under `content/rules/`, so the content hash covers it (D-495, D-648).
/// </para>
/// </remarks>
public sealed class RuleFixture
{
    private RuleFixture(string comment, IReadOnlyList<RuleFixtureEntry> entries)
    {
        this.Comment = comment;
        this.Entries = entries;
    }

    /// <summary>The note at the top of the file.</summary>
    public string Comment { get; }

    /// <summary>Every entry of the file, in the order of the file.</summary>
    public IReadOnlyList<RuleFixtureEntry> Entries { get; }

    /// <summary>Reads a fixture rule file from its bytes.</summary>
    /// <param name="bytes">The bytes of the file, as UTF-8.</param>
    /// <param name="file">The path of the file, under `content/`, for each error (T-2).</param>
    /// <returns>The file.</returns>
    /// <exception cref="ContentException">The file breaks a rule of the reader (G-6, T-2).</exception>
    public static RuleFixture Read(ReadOnlySpan<byte> bytes, string file)
    {
        var reader = new ContentReader(bytes, file);
        RuleFixture fixture = Read(ref reader);
        reader.ReadFileEnd();
        return fixture;
    }

    private static RuleFixture Read(ref ContentReader reader)
    {
        string? comment = null;
        List<RuleFixtureEntry>? entries = null;

        int depth = reader.ReadObjectStart();
        while (reader.ReadNextField(depth, out string field))
        {
            switch (field)
            {
                case "comment":
                    comment = reader.ReadString();
                    break;
                case "fixtures":
                    entries = ReadEntries(ref reader);
                    break;
                default:
                    throw reader.UnknownField(field);
            }
        }

        return new RuleFixture(
            reader.Require(comment, depth, "comment"),
            reader.Require(entries, depth, "fixtures"));
    }

    private static List<RuleFixtureEntry> ReadEntries(ref ContentReader reader)
    {
        var entries = new List<RuleFixtureEntry>();
        int depth = reader.ReadArrayStart();
        while (reader.ReadNextElement(depth, entries.Count))
        {
            entries.Add(ReadEntry(ref reader));
        }

        return entries;
    }

    private static RuleFixtureEntry ReadEntry(ref ContentReader reader)
    {
        ContentId? id = null;
        ContentId? label = null;
        int? weight = null;

        int depth = reader.ReadObjectStart();
        while (reader.ReadNextField(depth, out string field))
        {
            switch (field)
            {
                case "id":
                    id = reader.ReadContentId();
                    break;
                case "label":
                    label = reader.ReadContentId();
                    break;
                case "weight":
                    weight = reader.ReadInt();
                    break;
                default:
                    throw reader.UnknownField(field);
            }
        }

        return new RuleFixtureEntry(
            reader.Require(id, depth, "id"),
            reader.Require(label, depth, "label"),
            reader.RequireInt(weight, depth, "weight"));
    }
}
