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
    // character as one unit. A pattern that stops at the first quote misses such a value.
    private static readonly Regex Assignment = new(
        "^\\s*(?<name>[A-Za-z0-9_/]+)\\s*=\\s*\"(?<value>(?:[^\"\\\\]|\\\\.)*)\"\\s*$",
        RegexOptions.CultureInvariant,
        TimeSpan.FromSeconds(1));

    /// <summary>Reads the lines of one scene file and gives each text value that it holds.</summary>
    /// <param name="path">The path of the scene file, for the finding.</param>
    /// <param name="lines">The lines of the file, in order.</param>
    /// <returns>One finding for each property that holds player text.</returns>
    public static IReadOnlyList<LintFinding> Check(string path, IReadOnlyList<string> lines)
    {
        ArgumentException.ThrowIfNullOrEmpty(path);
        ArgumentNullException.ThrowIfNull(lines);

        List<LintFinding> findings = [];
        for (int index = 0; index < lines.Count; index++)
        {
            Match match = Assignment.Match(lines[index]);
            if (!match.Success)
            {
                continue;
            }

            string name = match.Groups["name"].Value;
            string value = match.Groups["value"].Value;
            if (value.Length == 0 || !IsTextProperty(name))
            {
                continue;
            }

            findings.Add(new LintFinding(
                path,
                index + 1,
                Id,
                $"the property `{name}` holds the text \"{value}\". {Reason}"));
        }

        return findings;
    }

    /// <summary>Tells whether the name of a scene property names player text.</summary>
    /// <param name="name">The name of the property, such as `text` or `dialog_text`.</param>
    /// <returns>True for a text property, and false for a name such as `texture`.</returns>
    public static bool IsTextProperty(string name)
    {
        ArgumentException.ThrowIfNullOrEmpty(name);
        return name == "text"
            || name == "title"
            || name.EndsWith("_text", StringComparison.Ordinal)
            || name.EndsWith("_title", StringComparison.Ordinal);
    }
}
