using System;
using System.Collections.Generic;

namespace TheThingBelow.Core.Content;

/// <summary>One rule of the device table: a part of a pad name, and the glyph set it picks.</summary>
/// <param name="Match">
/// The part of the name of the pad, in lowercase, such as `dualsense`. The rule holds when
/// the name of the pad carries this text.
/// </param>
/// <param name="Set">The name of the glyph set, such as `playstation`.</param>
public sealed record DeviceMatch(string Match, string Set);

/// <summary>
/// The table that picks a glyph set from the name of a gamepad (D-711). Godot reports no
/// controller type, so Game reads the name of the pad and this table names the glyphs (F-50).
/// </summary>
/// <remarks>
/// The table is content, so a new pad name needs no build (D-116, G-6). The rules run in the
/// order of the file, and the first rule that holds wins. A name that no rule matches takes
/// <see cref="DefaultSet"/>.
/// <para>
/// Under Steam, PR-78 asks Steamworks for the controller type and overrides this pick
/// (D-460, D-553).
/// </para>
/// </remarks>
public sealed class DeviceNames
{
    /// <summary>The path of the device table, under `content/`.</summary>
    public const string Path = "ui/devices.json";

    private readonly List<DeviceMatch> matches;
    private readonly SortedSet<string> sets;
    private readonly List<string> prompts;

    private DeviceNames(
        string comment,
        string keyboardSet,
        string defaultSet,
        SortedSet<string> sets,
        List<string> prompts,
        List<DeviceMatch> matches)
    {
        this.Comment = comment;
        this.KeyboardSet = keyboardSet;
        this.DefaultSet = defaultSet;
        this.sets = sets;
        this.prompts = prompts;
        this.matches = matches;
    }

    /// <summary>The note at the top of the file.</summary>
    public string Comment { get; }

    /// <summary>The glyph set of the keyboard and the mouse (D-222).</summary>
    public string KeyboardSet { get; }

    /// <summary>The glyph set of a gamepad whose name no rule matches (D-711).</summary>
    public string DefaultSet { get; }

    /// <summary>Every glyph set of the file, in ordinal order (F-39).</summary>
    public IEnumerable<string> Sets => this.sets;

    /// <summary>Every rule, in the order of the file. The first rule that holds wins.</summary>
    public IReadOnlyList<DeviceMatch> Matches => this.matches;

    /// <summary>Every button that a prompt can draw, such as `confirm`, in the order of the file.</summary>
    public IReadOnlyList<string> Prompts => this.prompts;

    /// <summary>The id of the drawing of one prompt in one glyph set (D-222, D-519).</summary>
    /// <param name="set">The name of the glyph set, such as `xbox`.</param>
    /// <param name="prompt">The name of the button, such as `confirm`.</param>
    /// <returns>The content id, such as `drawing.ui_glyph_xbox_confirm`.</returns>
    public static string GlyphDrawingId(string set, string prompt)
    {
        ArgumentException.ThrowIfNullOrEmpty(set);
        ArgumentException.ThrowIfNullOrEmpty(prompt);

        return $"{Drawing.IdKind}.ui_glyph_{set}_{prompt}";
    }

    /// <summary>Tells whether the file names a glyph set.</summary>
    /// <param name="set">The name of a glyph set, such as `deck`.</param>
    /// <returns>True when the file names it.</returns>
    public bool HasSet(string set)
    {
        ArgumentException.ThrowIfNullOrEmpty(set);

        return this.sets.Contains(set);
    }

    /// <summary>Gives the glyph set of one gamepad (D-711).</summary>
    /// <param name="padName">The name that the engine reports for the pad, in any case.</param>
    /// <returns>The name of the glyph set, which is always one of <see cref="Sets"/>.</returns>
    /// <exception cref="ArgumentNullException">The name is null (T-2).</exception>
    /// <remarks>
    /// The comparison drops the case of both texts, because one pad reports `DualSense` and
    /// another reports `Sony DUALSHOCK 4`. An empty name takes the default set, which is the
    /// name that a pad with no report gives.
    /// </remarks>
    public string SetOfPad(string padName)
    {
        ArgumentNullException.ThrowIfNull(padName);

        foreach (DeviceMatch match in this.matches)
        {
            if (padName.Contains(match.Match, StringComparison.OrdinalIgnoreCase))
            {
                return match.Set;
            }
        }

        return this.DefaultSet;
    }

    /// <summary>Reads the device table from the bytes of its file.</summary>
    /// <param name="bytes">The bytes of the file, as UTF-8.</param>
    /// <param name="file">The path of the file, under `content/`, for each error (T-2).</param>
    /// <returns>The table.</returns>
    /// <exception cref="ContentException">The file breaks a rule of the reader (G-6, T-2).</exception>
    public static DeviceNames Read(ReadOnlySpan<byte> bytes, string file)
    {
        var reader = new ContentReader(bytes, file);
        DeviceNames names = Read(ref reader);
        reader.ReadFileEnd();
        return names;
    }

    private static DeviceNames Read(ref ContentReader reader)
    {
        string? comment = null;
        List<string>? sets = null;
        string? keyboardSet = null;
        string? defaultSet = null;
        List<string>? prompts = null;
        List<DeviceMatch>? matches = null;

        int depth = reader.ReadObjectStart();
        while (reader.ReadNextField(depth, out string field))
        {
            switch (field)
            {
                case "comment":
                    comment = reader.ReadString();
                    break;
                case "sets":
                    sets = ReadSets(ref reader);
                    break;
                case "keyboard_set":
                    keyboardSet = reader.ReadString();
                    break;
                case "default_set":
                    defaultSet = reader.ReadString();
                    break;
                case "prompts":
                    prompts = ReadSets(ref reader);
                    break;
                case "names":
                    matches = ReadMatches(ref reader);
                    break;
                default:
                    throw reader.UnknownField(field);
            }
        }

        return Build(
            ref reader,
            depth,
            reader.Require(comment, depth, "comment"),
            reader.Require(sets, depth, "sets"),
            reader.Require(keyboardSet, depth, "keyboard_set"),
            reader.Require(defaultSet, depth, "default_set"),
            reader.Require(prompts, depth, "prompts"),
            reader.Require(matches, depth, "names"));
    }

    /// <summary>
    /// Checks that each name of a rule is a set of the file, so a lookup always gives a set
    /// that the glyphs of the build hold (T-2).
    /// </summary>
    private static DeviceNames Build(
        ref ContentReader reader,
        int depth,
        string comment,
        List<string> setList,
        string keyboardSet,
        string defaultSet,
        List<string> prompts,
        List<DeviceMatch> matches)
    {
        // An ordinal order, because the default order of .NET follows the culture of the
        // machine (F-39, G-4).
        var sets = new SortedSet<string>(StringComparer.Ordinal);
        for (int index = 0; index < setList.Count; index += 1)
        {
            if (!sets.Add(setList[index]))
            {
                throw reader.RefuseField(
                    depth,
                    $"sets[{index}]",
                    $"the file names the glyph set '{setList[index]}' two times");
            }
        }

        if (sets.Count == 0)
        {
            throw reader.RefuseField(depth, "sets", "the file names no glyph set, and a prompt draws one (D-222)");
        }

        RequireSet(ref reader, depth, sets, "keyboard_set", keyboardSet);
        RequireSet(ref reader, depth, sets, "default_set", defaultSet);
        for (int index = 0; index < matches.Count; index += 1)
        {
            RequireSet(ref reader, depth, sets, $"names[{index}].set", matches[index].Set);
        }

        if (prompts.Count == 0)
        {
            throw reader.RefuseField(depth, "prompts", "the file names no button, and a glyph set draws one for each button (D-222)");
        }

        for (int index = 1; index < prompts.Count; index += 1)
        {
            // The list is short, so the check reads each earlier entry and needs no set.
            for (int earlier = 0; earlier < index; earlier += 1)
            {
                if (string.CompareOrdinal(prompts[index], prompts[earlier]) == 0)
                {
                    throw reader.RefuseField(
                        depth,
                        $"prompts[{index}]",
                        $"the file names the button '{prompts[index]}' two times");
                }
            }
        }

        return new DeviceNames(comment, keyboardSet, defaultSet, sets, prompts, matches);
    }

    private static void RequireSet(
        ref ContentReader reader,
        int depth,
        SortedSet<string> sets,
        string field,
        string set)
    {
        if (!sets.Contains(set))
        {
            throw reader.RefuseField(
                depth,
                field,
                $"the glyph set '{set}' is not in the `sets` list of the file");
        }
    }

    private static List<string> ReadSets(ref ContentReader reader)
    {
        var sets = new List<string>();
        int depth = reader.ReadArrayStart();
        while (reader.ReadNextElement(depth, sets.Count))
        {
            sets.Add(reader.ReadString());
        }

        return sets;
    }

    private static List<DeviceMatch> ReadMatches(ref ContentReader reader)
    {
        var matches = new List<DeviceMatch>();
        int depth = reader.ReadArrayStart();
        while (reader.ReadNextElement(depth, matches.Count))
        {
            matches.Add(ReadMatch(ref reader, depth, matches.Count));
        }

        return matches;
    }

    private static DeviceMatch ReadMatch(ref ContentReader reader, int listDepth, int index)
    {
        string? match = null;
        string? set = null;

        int depth = reader.ReadObjectStart();
        while (reader.ReadNextField(depth, out string field))
        {
            switch (field)
            {
                case "match":
                    match = reader.ReadString();
                    break;
                case "set":
                    set = reader.ReadString();
                    break;
                default:
                    throw reader.UnknownField(field);
            }
        }

        var rule = new DeviceMatch(
            reader.Require(match, depth, "match"),
            reader.Require(set, depth, "set"));

        if (rule.Match.Length == 0)
        {
            throw reader.RefuseField(
                listDepth,
                $"names[{index}].match",
                "the rule matches an empty text, and every name carries one");
        }

        return rule;
    }
}
