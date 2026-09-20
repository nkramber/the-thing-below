using System;
using System.Collections.Generic;

namespace TheThingBelow.Core;

/// <summary>
/// The lines of a text file of this project: one object on each line, one line feed after
/// each line, and no carriage return (D-652, T-7). The run record, the save, and the crash
/// file each split their text here, and each one throws its own error on a fault.
/// </summary>
/// <param name="Lines">The lines, with no line ending. The list is empty on a fault.</param>
/// <param name="FaultLine">
/// The number of the line of the fault, from 1. It is 0 when the fault is the whole text, and
/// when the text splits.
/// </param>
/// <param name="Fault">What the split found, or null when the text splits.</param>
public sealed record TextLines(IReadOnlyList<string> Lines, int FaultLine, string? Fault)
{
    /// <summary>Splits a text into its lines.</summary>
    /// <param name="text">The text of the file.</param>
    /// <returns>The lines, or the fault that the text holds.</returns>
    /// <exception cref="ArgumentNullException">The text is null (T-2).</exception>
    public static TextLines Split(string text)
    {
        ArgumentNullException.ThrowIfNull(text);

        if (text.Contains('\r', StringComparison.Ordinal))
        {
            return new TextLines(
                [],
                0,
                "it holds a carriage return, and each line of the file ends with one line feed alone (T-7)");
        }

        string[] parts = text.Split('\n');
        List<string> lines = [];
        for (int index = 0; index < parts.Length; index += 1)
        {
            if (parts[index].Length == 0)
            {
                // The text ends with a line ending, so the split gives one empty part at the
                // end. An empty part anywhere else is a line with no object (T-2).
                if (index == parts.Length - 1)
                {
                    continue;
                }

                return new TextLines([], index + 1, "the line holds no object");
            }

            lines.Add(parts[index]);
        }

        return new TextLines(lines, 0, null);
    }
}
