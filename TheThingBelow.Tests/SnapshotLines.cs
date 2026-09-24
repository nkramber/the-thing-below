using System.Text.RegularExpressions;

namespace TheThingBelow.Tests;

/// <summary>
/// Changes a snapshot line of this build into the shape of an older save format, for a test of
/// the reader of that format (D-166).
/// </summary>
internal static partial class SnapshotLines
{
    /// <summary>
    /// Drops the lessons of each character, the lesson pack, and the swap place, which save
    /// format 10 added (D-1018, D-1024, D-1030). A test of an older reader then reaches the
    /// field that it reads.
    /// </summary>
    /// <param name="line">A snapshot line of this build.</param>
    /// <returns>The line with no lesson field.</returns>
    public static string WithoutLessons(string line)
    {
        string noSlots = CharacterLessons().Replace(line, string.Empty);
        return PartyLessons().Replace(noSlots, string.Empty);
    }

    // The arrays hold objects with no nested array, so the first `]` ends each one.
    [GeneratedRegex(""","lessons":\{"slot_count":\d+,"slots":\[[^\]]*\],"points":\[[^\]]*\]\}""")]
    private static partial Regex CharacterLessons();

    [GeneratedRegex(""","lesson_pack":\[[^\]]*\],"swap_place":(true|false)""")]
    private static partial Regex PartyLessons();
}
