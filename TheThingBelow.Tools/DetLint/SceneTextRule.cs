using System;
using System.Collections.Generic;
using System.Text.RegularExpressions;

namespace TheThingBelow.Tools.DetLint;

/// <summary>
/// DL 9: no text value in a Godot scene file. Game data never lives in a scene file, and each
/// player string comes from the string table (D-499, G-6, G-7). The rule reads the text of the
/// file, because a scene file is not C#.
/// </summary>
public static class SceneTextRule
{
    /// <summary>The id of the rule.</summary>
    public const string Id = "DL 9";

    private const string Reason =
        "A scene file holds layout alone. Draw each player string from the string table (D-499, G-6, G-7).";

    // Godot writes a quote inside a string value as `\"`, so the value part reads an escaped
    // character as one unit. A pattern that stops at the first quote misses such a value. A
    // value can hold a line end, which Godot writes as is, so the pattern reads the whole file.
    private static readonly Regex Assignment = new(
        "^[ \\t]*(?<name>[A-Za-z0-9_/]+)[ \\t]*=[ \\t]*\"(?<value>(?:[^\"\\\\]|\\\\.)*)\"[ \\t]*$",
        RegexOptions.CultureInvariant | RegexOptions.Multiline,
        TimeSpan.FromSeconds(1));

    /// <summary>Reads the lines of one scene file and gives each text value that it holds.</summary>
    /// <param name="path">The path of the scene file, for the finding.</param>
    /// <param name="lines">The lines of the file, in order.</param>
    /// <returns>One finding for each property that holds player text.</returns>
    public static IReadOnlyList<LintFinding> Check(string path, IReadOnlyList<string> lines)
    {
        ArgumentException.ThrowIfNullOrEmpty(path);
        ArgumentNullException.ThrowIfNull(lines);

        string text = string.Join('\n', lines);
        List<LintFinding> findings = [];
        foreach (Match match in Assignment.Matches(text))
        {
            string name = match.Groups["name"].Value;
            string value = match.Groups["value"].Value;
            if (value.Length == 0 || !IsTextProperty(name))
            {
                continue;
            }

            findings.Add(new LintFinding(
                path,
                LineOf(text, match.Index),
                Id,
                $"the property `{name}` holds the text \"{value}\". {Reason}"));
        }

        return findings;
    }

    /// <summary>Gives the finding of a binary scene file or resource file, whose text no rule reads (D-825).</summary>
    /// <param name="path">The path of the file, for the finding.</param>
    /// <returns>The finding, at line 1.</returns>
    public static LintFinding RefuseBinary(string path)
    {
        ArgumentException.ThrowIfNullOrEmpty(path);

        return new LintFinding(
            path,
            1,
            Id,
            $"the file is a binary scene or resource, and this rule reads text alone. Write the scene as a `.tscn` file. {Reason}");
    }

    private static int LineOf(string text, int index)
    {
        int line = 1;
        for (int at = 0; at < index; at++)
        {
            if (text[at] == '\n')
            {
                line++;
            }
        }

        return line;
    }

    /// <summary>Tells whether the name of a scene property names player text.</summary>
    /// <param name="name">The name of the property, such as `text`, `dialog_text`, or `item_0/text`.</param>
    /// <returns>True for a text property, and false for a name such as `texture`.</returns>
    /// <remarks>
    /// Godot writes the text of a menu item as `item_0/text` and of a button popup as
    /// `popup/item_0/text`, so the rule reads the last part of the name.
    /// </remarks>
    public static bool IsTextProperty(string name)
    {
        ArgumentException.ThrowIfNullOrEmpty(name);
        string last = name[(name.LastIndexOf('/') + 1)..];
        return last == "text"
            || last == "title"
            || last.EndsWith("_text", StringComparison.Ordinal)
            || last.EndsWith("_title", StringComparison.Ordinal);
    }
}
