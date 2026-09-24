using System;
using System.IO;

namespace TheThingBelow.Tools.Screenplay;

/// <summary>
/// Puts the Screenplay section into the text of a PR description (D-1016). Two marker lines
/// hold the section, so a second run replaces it and leaves the rest of the body intact.
/// </summary>
public static class PullRequestBody
{
    /// <summary>The line that starts the Screenplay section.</summary>
    public const string StartMarker = "<!-- screenplay:start -->";

    /// <summary>The line that ends the Screenplay section.</summary>
    public const string EndMarker = "<!-- screenplay:end -->";

    /// <summary>The most characters that GitHub takes in a PR description (D-1016).</summary>
    public const int MaxCharacters = 65536;

    /// <summary>Gives the body with the section between the two marker lines.</summary>
    /// <param name="body">The text of the PR description.</param>
    /// <param name="section">The Screenplay section, which ends with a line end.</param>
    /// <returns>The new body. A body with no markers gets the section at its end.</returns>
    /// <exception cref="ArgumentNullException">An argument is null (T-2).</exception>
    /// <exception cref="InvalidDataException">
    /// The body holds one marker without the other, a marker two times, or the end marker
    /// first. The new body passes the limit of GitHub (T-2).
    /// </exception>
    /// <remarks>
    /// The count reads the UTF-16 units of .NET, which is never below the count of Unicode
    /// characters, so a body that passes here also passes on GitHub.
    /// </remarks>
    public static string Insert(string body, string section)
    {
        ArgumentNullException.ThrowIfNull(body);
        ArgumentNullException.ThrowIfNull(section);

        string marked = $"{StartMarker}\n{section}{EndMarker}\n";
        int start = OnlyIndex(body, StartMarker);
        int end = OnlyIndex(body, EndMarker);

        string result;
        if (start < 0 && end < 0)
        {
            string separator = body.Length == 0 || body.EndsWith('\n') ? string.Empty : "\n";
            result = body + separator + "\n" + marked;
        }
        else if (start < 0 || end < 0 || end < start)
        {
            throw new InvalidDataException(
                $"The body holds the marker '{(start < 0 ? EndMarker : StartMarker)}' with no partner in order. Correct the markers, or remove both (D-1016).");
        }
        else
        {
            int after = end + EndMarker.Length;
            if (after < body.Length && body[after] == '\n')
            {
                after += 1;
            }

            result = body[..start] + marked + body[after..];
        }

        if (result.Length > MaxCharacters)
        {
            throw new InvalidDataException(
                $"The body holds {result.Length} characters with the Screenplay section, and GitHub takes {MaxCharacters} at most. Split the batch over more PRs (D-1016).");
        }

        return result;
    }

    private static int OnlyIndex(string body, string marker)
    {
        int first = body.IndexOf(marker, StringComparison.Ordinal);
        if (first >= 0 && body.IndexOf(marker, first + marker.Length, StringComparison.Ordinal) >= 0)
        {
            throw new InvalidDataException($"The body holds the marker '{marker}' two times, and the Screenplay section has one start and one end (D-1016).");
        }

        return first;
    }
}
