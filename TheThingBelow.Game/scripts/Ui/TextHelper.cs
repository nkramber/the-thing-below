using System;
using System.Collections.Generic;
using System.Text;
using Godot;
using TheThingBelow.Core.Content;

namespace TheThingBelow.Game.Ui;

/// <summary>
/// The one place that puts a player string on screen (G-7, D-499). It takes a string table
/// id, the values to fill in, and the label that shows the text. It never takes a literal.
/// </summary>
/// <remarks>
/// det-lint fails a Godot text property outside this type, under DL 8 (D-499, D-614). Thus
/// every screen of the game reads its words from the string table, and no line of code holds
/// a word of its own.
/// <para>
/// A string can carry a place to fill, such as `{file}`. The caller gives the value for each
/// name, and a name with no value is an error, because a message with `{file}` in it tells
/// the player nothing (T-2).
/// </para>
/// </remarks>
public sealed class TextHelper
{
    /// <summary>The character that opens a place to fill in a string, such as `{file}`.</summary>
    public const char OpenMark = '{';

    /// <summary>The character that closes a place to fill in a string.</summary>
    public const char CloseMark = '}';

    private readonly StringTable strings;

    /// <summary>Makes the helper of one build.</summary>
    /// <param name="strings">The string table of the content set (G-7).</param>
    /// <exception cref="ArgumentNullException">The table is null (T-2).</exception>
    public TextHelper(StringTable strings)
    {
        ArgumentNullException.ThrowIfNull(strings);

        this.strings = strings;
    }

    /// <summary>Puts the text of a string id on a label.</summary>
    /// <param name="place">The label that the player reads.</param>
    /// <param name="id">The string id, such as `ui.title`.</param>
    /// <exception cref="ArgumentNullException">The label or the id is null (T-2).</exception>
    /// <exception cref="ContentException">The table holds no such id, or the text needs a value (T-2).</exception>
    public void Put(Label place, ContentId id) => this.Put(place, id, EmptyValues);

    /// <summary>Puts the text of a string id on a label, with each place filled in.</summary>
    /// <param name="place">The label that the player reads.</param>
    /// <param name="id">The string id, such as `crash.file`.</param>
    /// <param name="values">The value of each place, by its name, such as `file`.</param>
    /// <exception cref="ArgumentNullException">The label, the id, or the values are null (T-2).</exception>
    /// <exception cref="ContentException">
    /// The table holds no such id, a place has no value, or a place has no closing mark (T-2).
    /// </exception>
    public void Put(Label place, ContentId id, IReadOnlyDictionary<string, string> values)
    {
        ArgumentNullException.ThrowIfNull(place);
        ArgumentNullException.ThrowIfNull(id);
        ArgumentNullException.ThrowIfNull(values);

        place.Text = Fill(this.strings.Text(id), id, values);
    }

    /// <summary>Gives the text of a string id with each place filled in.</summary>
    /// <param name="text">The text of the string table entry.</param>
    /// <param name="id">The string id, for each error (T-2).</param>
    /// <param name="values">The value of each place, by its name.</param>
    /// <returns>The text that the player reads.</returns>
    /// <exception cref="ContentException">A place has no value, or a place has no closing mark (T-2).</exception>
    /// <remarks>
    /// The method reads no engine value, so a test drives it with no engine (D-614). It stays
    /// public for that test, and every draw of a string still goes through <see cref="Put(Label, ContentId)"/>.
    /// </remarks>
    public static string Fill(string text, ContentId id, IReadOnlyDictionary<string, string> values)
    {
        ArgumentNullException.ThrowIfNull(text);
        ArgumentNullException.ThrowIfNull(id);
        ArgumentNullException.ThrowIfNull(values);

        int open = text.IndexOf(OpenMark, StringComparison.Ordinal);
        if (open < 0)
        {
            return text;
        }

        StringBuilder filled = new();
        int read = 0;
        while (open >= 0)
        {
            int close = text.IndexOf(CloseMark, open + 1);
            if (close < 0)
            {
                throw ContentException.ForField(
                    StringTable.Path,
                    id.Value,
                    $"the text holds a '{OpenMark}' at character {open} and no '{CloseMark}' after it");
            }

            string name = text[(open + 1)..close];
            if (!values.TryGetValue(name, out string? value))
            {
                throw ContentException.ForField(
                    StringTable.Path,
                    id.Value,
                    $"the text holds the place '{name}', and the caller gave no value for it (T-2)");
            }

            filled.Append(text, read, open - read);
            filled.Append(value);
            read = close + 1;
            open = text.IndexOf(OpenMark, read);
        }

        filled.Append(text, read, text.Length - read);
        return filled.ToString();
    }

    private static readonly IReadOnlyDictionary<string, string> EmptyValues =
        new SortedDictionary<string, string>(StringComparer.Ordinal);
}
