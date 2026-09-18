using System;
using System.Collections.Generic;

namespace TheThingBelow.Core.Content;

/// <summary>
/// Every content file of one build, read and checked. The load fails on the first error, and
/// the error names the file and the field (G-6, T-2).
/// </summary>
/// <remarks>
/// Core reads no folder and no resource (G-1). The host gives the bytes: Game from the
/// resources of its own assembly, and Tools from the `content/` folder (D-508).
/// <para>
/// The load runs four rules over the whole set. Every file has a record (D-517). No two rule
/// entries take one id (D-166). Every string id that a rule names is in the table (G-7). The
/// content hash covers the rule files alone (D-495, D-648).
/// </para>
/// </remarks>
public sealed class ContentSet
{
    private readonly SortedDictionary<string, RuleFixtureEntry> ruleEntries;

    private ContentSet(
        Palette palette,
        StringTable strings,
        SortedDictionary<string, RuleFixtureEntry> ruleEntries,
        string hash)
    {
        this.Palette = palette;
        this.Strings = strings;
        this.ruleEntries = ruleEntries;
        this.Hash = hash;
    }

    /// <summary>The palette of the game (D-89, D-121).</summary>
    public Palette Palette { get; }

    /// <summary>The table of every string that the player reads (G-7).</summary>
    public StringTable Strings { get; }

    /// <summary>The hash of the rule files, as 64 lowercase hexadecimal characters (G-5).</summary>
    public string Hash { get; }

    /// <summary>Every rule entry, in ordinal order of its id (F-39).</summary>
    public IEnumerable<RuleFixtureEntry> RuleEntries => this.ruleEntries.Values;

    /// <summary>Reads and checks every content file of one build.</summary>
    /// <param name="files">Every file of `content/`, in any order.</param>
    /// <returns>The content set.</returns>
    /// <exception cref="ContentException">
    /// A file breaks a rule of the reader, a file has no record, two rule entries take one
    /// id, or a rule names a string id that the table lacks (T-2).
    /// </exception>
    public static ContentSet Load(IReadOnlyList<ContentFile> files)
    {
        ArgumentNullException.ThrowIfNull(files);

        Palette? palette = null;
        StringTable? strings = null;
        var ruleEntries = new SortedDictionary<string, RuleFixtureEntry>(StringComparer.Ordinal);
        var sources = new SortedDictionary<string, string>(StringComparer.Ordinal);

        foreach (ContentFile file in Ordered(files))
        {
            if (string.CompareOrdinal(file.Path, Palette.Path) == 0)
            {
                palette = Palette.Read(file.Bytes, file.Path);
            }
            else if (string.CompareOrdinal(file.Path, StringTable.Path) == 0)
            {
                strings = StringTable.Read(file.Bytes, file.Path);
            }
            else if (ContentPaths.IsRuleFile(file.Path))
            {
                AddRuleFile(file, ruleEntries, sources);
            }
            else
            {
                throw ContentException.ForFile(
                    file.Path,
                    "no record of Core reads this file, and every content file needs one (D-517)");
            }
        }

        var set = new ContentSet(
            palette ?? throw AbsentFile(Palette.Path),
            strings ?? throw AbsentFile(StringTable.Path),
            ruleEntries,
            ContentHash.Compute(files));

        set.RefuseAbsentString(sources);
        return set;
    }

    /// <summary>Gives a rule entry by its id.</summary>
    /// <param name="id">The content id of the entry.</param>
    /// <returns>The entry.</returns>
    /// <exception cref="ContentException">The set holds no entry with that id (T-2).</exception>
    public RuleFixtureEntry RuleEntry(ContentId id)
    {
        ArgumentNullException.ThrowIfNull(id);

        if (!this.ruleEntries.TryGetValue(id.Value, out RuleFixtureEntry? entry))
        {
            throw ContentException.ForField(
                ContentPaths.RuleFolder,
                id.Value,
                "the content set holds no rule entry with this id");
        }

        return entry;
    }

    private static IEnumerable<ContentFile> Ordered(IReadOnlyList<ContentFile> files)
    {
        // An ordinal order, so the first error of a broken set is the same on every platform
        // and every host (G-4, F-39, T-2).
        // The list takes the files one at a time. A spread element makes the compiler emit a
        // call of `System.Linq`, and G-1 keeps that assembly out of the reference list of Core.
        List<ContentFile> ordered = new(files.Count);
        foreach (ContentFile file in files)
        {
            ordered.Add(file);
        }

        ordered.Sort(static (first, second) => string.CompareOrdinal(first.Path, second.Path));
        return ordered;
    }

    private static void AddRuleFile(
        ContentFile file,
        SortedDictionary<string, RuleFixtureEntry> ruleEntries,
        SortedDictionary<string, string> sources)
    {
        RuleFixture fixture = RuleFixture.Read(file.Bytes, file.Path);
        foreach (RuleFixtureEntry entry in fixture.Entries)
        {
            if (sources.TryGetValue(entry.Id.Value, out string? first))
            {
                throw ContentException.ForField(
                    file.Path,
                    entry.Id.Value,
                    $"the content id '{entry.Id.Value}' is already the id of an entry of '{first}', and an id is permanent (D-166)");
            }

            ruleEntries.Add(entry.Id.Value, entry);
            sources.Add(entry.Id.Value, file.Path);
        }
    }

    private static ContentException AbsentFile(string path) =>
        ContentException.ForFile(path, "the content set holds no such file");

    private void RefuseAbsentString(SortedDictionary<string, string> sources)
    {
        foreach (RuleFixtureEntry entry in this.ruleEntries.Values)
        {
            if (!this.Strings.Contains(entry.Label))
            {
                throw ContentException.ForField(
                    sources[entry.Id.Value],
                    $"{entry.Id.Value}.label",
                    $"the string table holds no id '{entry.Label.Value}' (G-7)");
            }
        }
    }
}
