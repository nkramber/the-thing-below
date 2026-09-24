using System.Text.RegularExpressions;

namespace TheThingBelow.Tests;

/// <summary>
/// Changes a snapshot line of this build into the shape of an older save format, for a test of
/// the reader of that format (D-166).
/// </summary>
internal static partial class SnapshotLines
{
    /// <summary>
    /// Drops the fields that save format 10 and 11 added: the lessons of each character and the
    /// lesson pack (D-1018, D-1024), then the gear of each character, the gold, and the steals of
    /// a fight (D-1038, D-1043, D-1045). Format 12 dropped the swap place of format 10 (D-1050). A test of an older reader
    /// then reaches the field that it reads.
    /// </summary>
    /// <param name="line">A snapshot line of this build, whose pack holds items alone.</param>
    /// <returns>The line in the shape of save format 9.</returns>
    public static string AsFormatNine(string line)
    {
        string noSlots = CharacterLessons().Replace(line, string.Empty);
        string noLessons = PartyLessons().Replace(noSlots, string.Empty);
        string noGear = CharacterGear().Replace(noLessons, string.Empty);
        string noGold = PartyGold().Replace(noGear, string.Empty);
        return BattleSteals().Replace(noGold, string.Empty);
    }

    // The arrays hold objects with no nested array, so the first `]` ends each one.
    [GeneratedRegex(""","lessons":\{"slot_count":\d+,"slots":\[[^\]]*\],"points":\[[^\]]*\]\}""")]
    private static partial Regex CharacterLessons();

    [GeneratedRegex(""","lesson_pack":\[[^\]]*\]""")]
    private static partial Regex PartyLessons();

    [GeneratedRegex(""","gear":\[[^\]]*\]""")]
    private static partial Regex CharacterGear();

    [GeneratedRegex(""","gold":\d+""")]
    private static partial Regex PartyGold();

    [GeneratedRegex(""","steals":\{"tries":\d+,"taken":\[[^\]]*\]\}""")]
    private static partial Regex BattleSteals();
}
