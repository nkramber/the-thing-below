using System;
using System.Collections.Generic;

namespace TheThingBelow.Tools.ReviewGate;

/// <summary>One section of a Markdown text, from its heading to the next heading of that level.</summary>
public static class MarkdownSection
{
    /// <summary>Reads the lines of one section.</summary>
    /// <param name="text">The text of the document or the description.</param>
    /// <param name="heading">The heading line of the section, such as `## Verdict`.</param>
    /// <returns>The lines under the heading, or null when the text holds no such heading.</returns>
    public static IReadOnlyList<string>? ReadLines(string text, string heading)
    {
        ArgumentNullException.ThrowIfNull(text);
        ArgumentException.ThrowIfNullOrEmpty(heading);

        List<string> lines = [];
        bool inside = false;
        foreach (string raw in text.Split('\n'))
        {
            string line = raw.TrimEnd('\r');
            if (inside && line.StartsWith("## ", StringComparison.Ordinal))
            {
                break;
            }

            if (inside)
            {
                lines.Add(line);
            }
            else if (string.Equals(line.TrimEnd(), heading, StringComparison.Ordinal))
            {
                inside = true;
            }
        }

        return inside ? lines : null;
    }
}
