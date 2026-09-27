using System;
using TheThingBelow.Core.Battles;
using TheThingBelow.Core.Content;
using TheThingBelow.Core.Maps;
using Xunit;

namespace TheThingBelow.Tests;

/// <summary>The load of the harm of a trap, and the refusal of each broken trap line (D-1226, D-1230, D-1231, T-2).</summary>
public sealed class TrapLoadTests
{
    private const string BladeLine = """{ "id": "trap.test_hall_blade", "kind": "trap", "x": 3, "y": 1, "harm": "damage", "share": 2500 }""";

    [Fact]
    public void EachTrapOfTheHallReadsItsHarm()
    {
        GameMap hall = TrapMaps.Hall;

        TrapHarm blade = hall.TrapAt(TrapMaps.Blade)!.Harm!;
        Assert.Equal((TrapHarmKind.Damage, 2500, (StatusKind?)null, (ContentId?)null), (blade.Kind, blade.Share, blade.Status, blade.Group));
        Assert.Equal(StatusKind.Poison, hall.TrapAt(TrapMaps.Needle)!.Harm!.Status);
        Assert.Equal(StatusKind.Silence, hall.TrapAt(TrapMaps.Dust)!.Harm!.Status);
        Assert.Equal("group.one", hall.TrapAt(TrapMaps.Alarm)!.Harm!.Group!.Value);
        Assert.Null(hall.ThingOf(ContentId.Parse("spawn_point.test_hall_start", "test", "spawn"), MapThingKind.SpawnPoint)!.Harm);
    }

    [Theory]
    [InlineData("""{ "id": "trap.test_hall_blade", "kind": "trap", "x": 3, "y": 1 }""", "holds no field 'harm'")]
    [InlineData("""{ "id": "trap.test_hall_blade", "kind": "trap", "x": 3, "y": 1, "harm": "fire" }""", "names the harm 'fire'")]
    [InlineData("""{ "id": "trap.test_hall_blade", "kind": "trap", "x": 3, "y": 1, "harm": "damage" }""", "holds no field 'share'")]
    [InlineData("""{ "id": "trap.test_hall_blade", "kind": "trap", "x": 3, "y": 1, "harm": "damage", "share": 0 }""", "takes the share 0")]
    [InlineData("""{ "id": "trap.test_hall_blade", "kind": "trap", "x": 3, "y": 1, "harm": "damage", "share": 10001 }""", "takes the share 10001")]
    [InlineData("""{ "id": "trap.test_hall_blade", "kind": "trap", "x": 3, "y": 1, "harm": "damage", "share": 50, "status": "poison" }""", "holds the field 'share' alone")]
    [InlineData("""{ "id": "trap.test_hall_blade", "kind": "trap", "x": 3, "y": 1, "harm": "status", "status": "sleep" }""", "names the status 'sleep'")]
    [InlineData("""{ "id": "trap.test_hall_blade", "kind": "trap", "x": 3, "y": 1, "harm": "status" }""", "holds no field 'status'")]
    [InlineData("""{ "id": "trap.test_hall_blade", "kind": "trap", "x": 3, "y": 1, "harm": "encounter" }""", "holds no field 'group'")]
    [InlineData("""{ "id": "trap.test_hall_blade", "kind": "trap", "x": 3, "y": 1, "harm": "encounter", "group": "group.one", "share": 5 }""", "holds the field 'group' alone")]
    public void ABrokenTrapLineFailsTheLoadWithTheTrapAndTheReason(string line, string reason)
    {
        ContentException error = Assert.Throws<ContentException>(() => HallWith(line));

        Assert.Contains("trap.test_hall_blade", error.Message, StringComparison.Ordinal);
        Assert.Contains(reason, error.Message, StringComparison.Ordinal);
    }

    [Fact]
    public void AFieldOfATrapOnAnotherThingFailsTheLoad()
    {
        // D-1226, T-2: a field of a trap on a spawn point is an error, never a silent extra.
        string text = TrapMaps.HallFile.Replace(
            """{ "id": "spawn_point.test_hall_start", "kind": "spawn_point", "x": 1, "y": 1 }""",
            """{ "id": "spawn_point.test_hall_start", "kind": "spawn_point", "x": 1, "y": 1, "harm": "damage" }""",
            StringComparison.Ordinal);

        ContentException error = Assert.Throws<ContentException>(() => TestMaps.Of("test-hall.json", text));

        Assert.Contains("spawn_point.test_hall_start", error.Message, StringComparison.Ordinal);
        Assert.Contains("a field of a trap", error.Message, StringComparison.Ordinal);
    }

    [Fact]
    public void AnEncounterTrapThatNamesAGroupOutsideItsRegionFailsTheContent()
    {
        // D-957, D-1231: the group of an encounter trap lies in the group file of the region of its map.
        GameMap hall = HallWith("""{ "id": "trap.test_hall_blade", "kind": "trap", "x": 3, "y": 1, "harm": "encounter", "group": "group.test_nowhere" }""");

        ContentException error = Assert.Throws<ContentException>(() => TestBattles.Content.RequireGroupsOf(hall));

        Assert.Contains("trap.test_hall_blade", error.Message, StringComparison.Ordinal);
        Assert.Contains("group.test_nowhere", error.Message, StringComparison.Ordinal);
    }

    [Fact]
    public void TheGroupOfTheAlarmOfTheHallPassesTheContent()
    {
        TestBattles.Content.RequireGroupsOf(TrapMaps.Hall);
    }

    [Fact]
    public void AHarmBuiltInCodeRefusesAValueOutsideItsRule()
    {
        // T-2: the builders hold the same rules as the load.
        Assert.Throws<ArgumentOutOfRangeException>(() => TrapHarm.Damage(0));
        Assert.Throws<ArgumentOutOfRangeException>(() => TrapHarm.Put(StatusKind.Sleep));
        Assert.Throws<ArgumentNullException>(() => TrapHarm.Ambush(null!));
        Assert.Throws<ArgumentOutOfRangeException>(() => TrapHarm.NameOf((TrapHarmKind)9));
    }

    private static GameMap HallWith(string blade) =>
        TestMaps.Of("test-hall.json", TrapMaps.HallFile.Replace(BladeLine, blade, StringComparison.Ordinal));
}
