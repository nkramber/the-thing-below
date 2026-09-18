using System;

namespace TheThingBelow.Core.Content;

/// <summary>
/// The permanent id of one content entry (D-166). An id is a lowercase kind, a dot, and a
/// lowercase name, such as `enemy.cave_rat` or `item.rusted_key` (D-646).
/// </summary>
/// <remarks>
/// No later entry takes the id of an earlier entry, because a save, a run record, and an
/// art file each hold ids and never a copy of the content (D-166, D-519). The kind is part
/// of the id, so an id in a log line or a crash file says what it points at with no other
/// field.
/// <para>
/// The form is `kind.name`. Each part starts with a letter from `a` to `z`, and the rest of
/// each part is a letter, a digit, or an underscore. No other character is legal.
/// </para>
/// </remarks>
public sealed class ContentId
{
    private readonly int dot;

    private ContentId(string value, int dot)
    {
        this.Value = value;
        this.dot = dot;
    }

    /// <summary>The whole id, such as `enemy.cave_rat`.</summary>
    public string Value { get; }

    /// <summary>The part before the dot, such as `enemy`.</summary>
    public string Kind => this.Value[..this.dot];

    /// <summary>The part after the dot, such as `cave_rat`.</summary>
    public string Name => this.Value[(this.dot + 1)..];

    /// <summary>Reads an id from a content field, and fails on any other form.</summary>
    /// <param name="value">The text of the field.</param>
    /// <param name="file">The path of the content file, for the error (T-2).</param>
    /// <param name="field">The field that holds the text, for the error (T-2).</param>
    /// <returns>The id.</returns>
    /// <exception cref="ContentException">The text does not take the form of D-646.</exception>
    public static ContentId Parse(string value, string file, string field)
    {
        ArgumentNullException.ThrowIfNull(value);
        ArgumentException.ThrowIfNullOrEmpty(file);
        ArgumentException.ThrowIfNullOrEmpty(field);

        int dot = DotOfWellFormedId(value);
        if (dot < 0)
        {
            throw ContentException.ForField(
                file,
                field,
                $"the id '{value}' is not a kind, a dot, and a name in lowercase letters, digits, and underscores (D-646)");
        }

        return new ContentId(value, dot);
    }

    /// <summary>Tells whether the text takes the form of an id (D-646).</summary>
    /// <param name="value">The text to read.</param>
    /// <returns>True when the text is a legal id.</returns>
    public static bool IsWellFormed(string value)
    {
        ArgumentNullException.ThrowIfNull(value);

        return DotOfWellFormedId(value) >= 0;
    }

    /// <summary>Gives the whole id, so a message that reads an id needs no field.</summary>
    /// <returns>The value of the id.</returns>
    public override string ToString() => this.Value;

    /// <summary>
    /// Gives the position of the one dot when the text is a legal id, and -1 otherwise.
    /// </summary>
    /// <remarks>
    /// The check reads the characters itself, because a regular expression in Core would
    /// bring a new dependency for one rule of 12 lines (T-1).
    /// </remarks>
    private static int DotOfWellFormedId(string value)
    {
        int dot = value.IndexOf('.', StringComparison.Ordinal);
        if (dot <= 0 || dot == value.Length - 1)
        {
            return -1;
        }

        if (value.IndexOf('.', dot + 1) >= 0)
        {
            return -1;
        }

        if (!IsWellFormedPart(value.AsSpan(0, dot)) || !IsWellFormedPart(value.AsSpan(dot + 1)))
        {
            return -1;
        }

        return dot;
    }

    private static bool IsWellFormedPart(ReadOnlySpan<char> part)
    {
        if (part.Length == 0 || part[0] is < 'a' or > 'z')
        {
            return false;
        }

        foreach (char letter in part)
        {
            bool legal = letter is (>= 'a' and <= 'z') or (>= '0' and <= '9') or '_';
            if (!legal)
            {
                return false;
            }
        }

        return true;
    }
}
