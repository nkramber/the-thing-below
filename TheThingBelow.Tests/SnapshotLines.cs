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
    /// a fight (D-1038, D-1043, D-1045). Format 12 dropped the swap place of format 10 (D-1050), and format 13 added the state of the torch (D-1064). A test of an older reader
    /// then reaches the field that it reads.
    /// </summary>
    /// <param name="line">A snapshot line of this build, whose pack holds items alone.</param>
    /// <returns>The line in the shape of save format 9.</returns>
    public static string AsFormatNine(string line)
    {
        string noSlots = CharacterLessons().Replace(AsFormatTwelve(line), string.Empty);
        string noLessons = PartyLessons().Replace(noSlots, string.Empty);
        string noGear = CharacterGear().Replace(noLessons, string.Empty);
        string noGold = PartyGold().Replace(noGear, string.Empty);
        return BattleSteals().Replace(noGold, string.Empty);
    }

    /// <summary>Drops the state of the torch, which save format 13 added (D-1064), and the NPCs of save format 15.</summary>
    /// <param name="line">A snapshot line of this build.</param>
    /// <returns>The line in the shape of save format 12.</returns>
    public static string AsFormatTwelve(string line) => PartyTorch().Replace(AsFormatFourteen(line), string.Empty);

    /// <summary>Drops the danger count and the encounter stream, which save format 20 added (D-1249), and the time of the map of save format 21.</summary>
    /// <param name="line">A snapshot line of this build.</param>
    /// <returns>The line in the shape of save format 19.</returns>
    public static string AsFormatNineteen(string line) => EncounterStream().Replace(Danger().Replace(AsFormatTwenty(line), string.Empty), string.Empty);

    /// <summary>Drops the time of the map, which save format 21 added (D-1349).</summary>
    /// <param name="line">A snapshot line of this build.</param>
    /// <returns>The line in the shape of save format 20.</returns>
    public static string AsFormatTwenty(string line) => MapTime().Replace(line, "$1");

    /// <summary>Drops the danger count and the encounter stream of save format 20, and the empty memory of the maps, which save format 18 added (D-555).</summary>
    /// <param name="line">A snapshot line of this build, whose memory of the maps is empty. No older format holds one.</param>
    /// <returns>The line in the shape of save format 17.</returns>
    public static string AsFormatSeventeen(string line) => EmptyPlaces().Replace(AsFormatNineteen(line), string.Empty);

    /// <summary>Drops the empty memory of the maps of save format 18, and names the pool of each character `mp`, as each format before save format 17 does (D-1197).</summary>
    /// <param name="line">A snapshot line of this build.</param>
    /// <returns>The line in the shape of save format 16.</returns>
    public static string AsFormatSixteen(string line) => CharacterAp().Replace(AsFormatSeventeen(line), "\"mp\":$1");

    /// <summary>Drops the empty stock, which save format 16 added (D-1152), and names the pool `mp`.</summary>
    /// <param name="line">A snapshot line of this build, whose stock is empty. No older format holds a stock.</param>
    /// <returns>The line in the shape of save format 15.</returns>
    public static string AsFormatFifteen(string line) => EmptyStock().Replace(AsFormatSixteen(line), string.Empty);

    /// <summary>Drops the empty stock of save format 16, and the NPCs of the map, the NPC stream, and the empty reserve, which save format 15 added (D-1136, D-1137).</summary>
    /// <param name="line">A snapshot line of this build, whose reserve and stock are empty. No older format holds either.</param>
    /// <returns>The line in the shape of save format 14.</returns>
    public static string AsFormatFourteen(string line)
    {
        string noReserve = EmptyReserve().Replace(AsFormatFifteen(line), string.Empty);
        return NpcStream().Replace(MapNpcs().Replace(noReserve, string.Empty), string.Empty);
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

    [GeneratedRegex(""","torch_held":(true|false)""")]
    private static partial Regex PartyTorch();

    [GeneratedRegex(""","npcs":\[[^\]]*\]""")]
    private static partial Regex MapNpcs();

    [GeneratedRegex(""","reserve":\[\]""")]
    private static partial Regex EmptyReserve();

    [GeneratedRegex(""","stock":\[\]""")]
    private static partial Regex EmptyStock();

    [GeneratedRegex(""","places":\[\]""")]
    private static partial Regex EmptyPlaces();

    [GeneratedRegex("\"ap\":(\\d+)")]
    private static partial Regex CharacterAp();

    [GeneratedRegex(""",\{"stream":6,"state":"0x[0-9a-f]+","increment":"0x[0-9a-f]+"\}""")]
    private static partial Regex NpcStream();

    [GeneratedRegex(""","danger":\d+""")]
    private static partial Regex Danger();

    [GeneratedRegex(""",\{"stream":7,"state":"0x[0-9a-f]+","increment":"0x[0-9a-f]+"\}""")]
    private static partial Regex EncounterStream();

    // The map writes its time right after its id (D-1349).
    [GeneratedRegex("""("map":\{"id":"[^"]+"),"time":"[a-z]+"(?=,)""")]
    private static partial Regex MapTime();

    [GeneratedRegex(""","steals":\{"tries":\d+,"taken":\[[^\]]*\]\}""")]
    private static partial Regex BattleSteals();
}
