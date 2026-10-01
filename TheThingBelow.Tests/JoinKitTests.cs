using System;
using System.Collections.Generic;
using System.Text;
using TheThingBelow.Core;
using TheThingBelow.Core.Battles;
using TheThingBelow.Core.Content;
using TheThingBelow.Core.Maps;
using TheThingBelow.Core.Runs;
using TheThingBelow.Core.Shops;
using TheThingBelow.Core.Story;
using TheThingBelow.Tools.Content;
using Xunit;

namespace TheThingBelow.Tests;

/// <summary>
/// The kit of a newcomer (D-1350): the join step gives the join lessons of the record and puts on
/// its join gear, and the load refuses a kit that no run can hold.
/// </summary>
public sealed class JoinKitTests
{
    private const ulong Seed = 20260928;

    private const string SecondNoKit = "\"side_flag\": \"flag.test_second_side\", \"join_lessons\": [], \"join_gear\": []";

    private const string ThirdNoKit = "\"side_flag\": \"flag.test_third_side\", \"join_lessons\": [], \"join_gear\": []";

    private static readonly ContentId Rot = Id("lesson.fixture_rot");

    private static readonly ContentId Quicken = Id("lesson.fixture_quicken");

    private static readonly ContentId Ring = Id("gear.test_resist_ring");

    private static readonly ContentId Shield = Id("gear.test_shield");

    /// <summary>
    /// The fixture of the tests, where the second character joins with the rot and the quicken
    /// and wears the blade and the shield. The two lessons leave the lesson pack, so the player
    /// owns each one time (D-1024).
    /// </summary>
    private static readonly string KitFixture = TestBattles.FixtureFile
        .Replace(SecondNoKit, "\"side_flag\": \"flag.test_second_side\", \"join_lessons\": [\"lesson.fixture_rot\", \"lesson.fixture_quicken\"], \"join_gear\": [\"gear.test_shield\", \"gear.test_resist_ring\"]", StringComparison.Ordinal)
        .Replace("\"lesson.fixture_rot\", \"lesson.fixture_quicken\", \"lesson.fixture_bolt\"]", "\"lesson.fixture_bolt\"]", StringComparison.Ordinal);

    [Fact]
    public void AJoinGivesTheLessonsAndPutsOnTheGearOfTheKit()
    {
        // D-1350: the lessons fill the slots from the first at zero points, and each piece takes
        // the first empty slot of its kind.
        Simulation run = StartWithJoin(TestBattles.Of(KitFixture));

        run.Step([]);

        PartyMember joined = run.State.Characters.Members[1];
        Assert.Equal(TestStory.Ally.Value, joined.Record.Id.Value);
        Assert.Equal([Rot.Value, Quicken.Value], ValuesOf(joined.Slots));
        Assert.Equal(0, joined.PointsOf(Rot));
        Assert.Equal(0, joined.PointsOf(Quicken));
        Assert.Equal([null, Shield.Value, null, null, Ring.Value, null], ValuesOf(joined.Gear));
        Assert.True(run.State.Characters.Owns(Rot));
        Assert.DoesNotContain(Rot.Value, ValuesOf(run.State.Characters.LessonPack));
        Assert.Equal(1, run.State.Characters.OwnedCount(Ring));
        Assert.Equal(0, run.State.Characters.CountOf(Ring));
    }

    [Fact]
    public void ASnapshotAfterTheJoinResumesToTheSameStateHash()
    {
        // D-166, D-563: the snapshot holds the lessons and the gear of each character, so the kit comes back.
        BattleContent content = TestBattles.Of(KitFixture);
        Simulation run = StartWithJoin(content);
        run.Step([]);

        var reader = new ContentReader(Encoding.UTF8.GetBytes(RunSnapshotText.Write(run.Snapshot())), "the test");
        Simulation resumed = Simulation.Resume(Seed, RunSnapshotText.Read(ref reader), TestStory.Map, content, TestBattles.Notices, StoryOf(content), DebugIntentHandlers.None);

        Assert.Equal(run.StateHash(), resumed.StateHash());
        Assert.Equal([Rot.Value, Quicken.Value], ValuesOf(resumed.State.Characters.Members[1].Slots));
        Assert.Equal(Ring.Value, resumed.State.Characters.Members[1].Gear[4]?.Value);
    }

    [Fact]
    public void AJoinWithALessonThatThePlayerOwnsFails()
    {
        // D-1024: the owned lesson set refuses a second copy with an error, here a rot that a chest gave first.
        BattleContent content = TestBattles.Of(KitFixture);
        Simulation run = ResumeWithParty(content, party => party with { LessonPack = [.. party.LessonPack!, Rot] });

        SimulationException error = Assert.Throws<SimulationException>(() => run.Step([]));

        Assert.Contains("the lesson 'lesson.fixture_rot', which the player already owns", error.Message, StringComparison.Ordinal);
        Assert.Contains("D-1024", error.Message, StringComparison.Ordinal);
    }

    [Fact]
    public void AJoinWithAPieceOverItsStackLimitFails()
    {
        // D-1039: the ring stacks to 3, and the pack already holds 3, so the worn ring would be a fourth.
        BattleContent content = TestBattles.Of(KitFixture);
        Simulation run = ResumeWithParty(content, party => party with { Pack = [.. party.Pack, new PackValues(Ring, 3)] });

        SimulationException error = Assert.Throws<SimulationException>(() => run.Step([]));

        Assert.Contains("the piece 'gear.test_resist_ring', and the party would own 4 copies over the stack limit 3", error.Message, StringComparison.Ordinal);
    }

    [Theory]
    [InlineData(
        "\"side_flag\": \"flag.test_marrek_side\", \"join_lessons\": []",
        "\"side_flag\": \"flag.test_marrek_side\", \"join_lessons\": [\"lesson.fixture_rot\"]",
        "join_lessons",
        "is in the start party")]
    [InlineData(
        "\"side_flag\": \"flag.test_marrek_side\", \"join_lessons\": [], \"join_gear\": []",
        "\"side_flag\": \"flag.test_marrek_side\", \"join_lessons\": [], \"join_gear\": [\"gear.test_helm\"]",
        "join_gear",
        "is in the start party")]
    [InlineData(SecondNoKit, "\"side_flag\": \"flag.test_second_side\", \"join_lessons\": [\"lesson.fixture_hew\"], \"join_gear\": []", "join_lessons", "never owns two copies")]
    [InlineData(SecondNoKit, "\"side_flag\": \"flag.test_second_side\", \"join_lessons\": [\"lesson.fixture_salve\"], \"join_gear\": []", "join_lessons", "never owns two copies")]
    [InlineData(SecondNoKit, "\"side_flag\": \"flag.test_second_side\", \"join_gear\": []", "join_lessons", "join_lessons")]
    [InlineData(SecondNoKit, "\"side_flag\": \"flag.test_second_side\", \"join_lessons\": []", "join_gear", "join_gear")]
    [InlineData(SecondNoKit, "\"side_flag\": \"flag.test_second_side\", \"join_lessons\": [], \"join_gear\": [\"item.fixture_draught\"]", "join_gear", "the kind 'gear'")]
    public void AFixtureWithABadKitFailsWithTheFileAndTheField(string from, string to, string field, string named)
    {
        string text = ReplaceFirst(TestBattles.FixtureFile, from, to);

        ContentException error = Assert.Throws<ContentException>(() => BattleFixture.Read(Encoding.UTF8.GetBytes(text), "kit-fixture.json"));

        Assert.Equal("kit-fixture.json", error.File);
        Assert.Contains(field, error.Field + error.Message, StringComparison.Ordinal);
        Assert.Contains(named, error.Message, StringComparison.Ordinal);
    }

    [Fact]
    public void TwoKitsWithOneLessonFail()
    {
        // D-1023: each character joins one time, so two kits with the rot would give two copies.
        string text = KitFixture.Replace(ThirdNoKit, "\"side_flag\": \"flag.test_third_side\", \"join_lessons\": [\"lesson.fixture_rot\"], \"join_gear\": []", StringComparison.Ordinal);

        ContentException error = Assert.Throws<ContentException>(() => BattleFixture.Read(Encoding.UTF8.GetBytes(text), "kit-fixture.json"));

        Assert.Equal(("kit-fixture.json", "join_lessons"), (error.File, error.Field));
        Assert.Contains("'character.test_third' joins with the lesson 'lesson.fixture_rot'", error.Message, StringComparison.Ordinal);
    }

    [Theory]
    [InlineData("\"join_lessons\": [\"lesson.test_none\"], \"join_gear\": []", "join_lessons", "holds no such id")]
    [InlineData("\"join_lessons\": [\"lesson.fixture_rot\", \"lesson.fixture_quicken\", \"lesson.fixture_bolt\"], \"join_gear\": []", "join_lessons", "joins with 3 lessons, and it has 2 slots at level 1")]
    [InlineData("\"join_lessons\": [], \"join_gear\": [\"gear.test_none\"]", "join_gear", "holds no such piece")]
    [InlineData("\"join_lessons\": [], \"join_gear\": [\"gear.test_shield\", \"gear.test_shield\"]", "gear.test_shield", "no empty slot of the kind 'off_hand'")]
    public void AKitThatTheOtherFilesRefuseFailsAtTheLoad(string kit, string field, string named)
    {
        // D-44, D-1018, D-1024, D-1350: the battle content checks each id, each slot, and each shop.
        // The bolt leaves the lesson pack, so a kit of three lessons can hold it.
        string fixture = ReplaceFirst(KitFixture, "\"lesson.fixture_purge\", \"lesson.fixture_bolt\"]", "\"lesson.fixture_purge\"]");
        fixture = ReplaceFirst(fixture, "\"join_lessons\": [\"lesson.fixture_rot\", \"lesson.fixture_quicken\"], \"join_gear\": [\"gear.test_shield\", \"gear.test_resist_ring\"]", kit);

        ContentException error = Assert.Throws<ContentException>(() => TestBattles.Of(fixture));

        Assert.Equal((BattleFixture.Path, field), (error.File, error.Field));
        Assert.Contains("character.test_second", error.Message, StringComparison.Ordinal);
        Assert.Contains(named, error.Message, StringComparison.Ordinal);
    }

    [Theory]
    [InlineData("\"join_lessons\": [\"lesson.test_pilfer\"], \"join_gear\": []", "lesson.test_pilfer")]
    [InlineData("\"join_lessons\": [], \"join_gear\": [\"gear.test_blade\"]", "gear.test_blade")]
    public void AKitThingThatAShopStocksFailsAtTheLoad(string kit, string thing)
    {
        // T-2, D-1024, D-1039, D-1350: a buy before the join would give a second copy of the
        // lesson, or a piece past its stack limit, and the join would fail in play.
        string fixture = ReplaceFirst(KitFixture, "\"join_lessons\": [\"lesson.fixture_rot\", \"lesson.fixture_quicken\"], \"join_gear\": [\"gear.test_shield\", \"gear.test_resist_ring\"]", kit);

        ContentException error = Assert.Throws<ContentException>(() => TestBattles.Of(fixture));

        Assert.Equal((ShopList.Path, "shop.test_store"), (error.File, error.Field));
        Assert.Contains($"the stock names '{thing}', and 'character.test_second' joins with it", error.Message, StringComparison.Ordinal);
    }

    [Theory]
    [InlineData("{ \"thing\": \"lesson.fixture_rot\", \"count\": 1, \"fallback\": \"item.test_root\" }", "lesson.fixture_rot")]
    [InlineData("{ \"thing\": \"gear.test_shield\", \"count\": 1 }", "gear.test_shield")]
    public void AChestThatHoldsAKitThingFailsAtTheLoad(string entry, string thing)
    {
        // T-2, D-1024, D-1039, D-1350: a find before the join would give a second copy of the
        // lesson, or a piece past its stack limit, and the join would fail in play.
        BattleContent content = TestBattles.Of(KitFixture);
        GameMap map = TestMaps.Of("test-vault.json", PartsMaps.VaultFile.Replace("{ \"thing\": \"item.fixture_draught\", \"count\": 4 }", entry, StringComparison.Ordinal));

        ContentException error = Assert.Throws<ContentException>(() => content.RequireThingsOf(map));

        Assert.Equal(("test-vault.json", "chest.test_vault_store"), (error.File, error.Field));
        Assert.Contains($"the chest names '{thing}', and 'character.test_second' joins with it", error.Message, StringComparison.Ordinal);
    }

    [Fact]
    public void KitsThatPassAStackLimitTogetherFailAtTheLoad()
    {
        // D-1039: the shield stacks to 1, and each character joins one time, so two kits of it pass the limit.
        string fixture = KitFixture.Replace(ThirdNoKit, "\"side_flag\": \"flag.test_third_side\", \"join_lessons\": [], \"join_gear\": [\"gear.test_shield\"]", StringComparison.Ordinal);

        ContentException error = Assert.Throws<ContentException>(() => TestBattles.Of(fixture));

        Assert.Equal((BattleFixture.Path, "gear.test_shield"), (error.File, error.Field));
        Assert.Contains("the stack limit is 1", error.Message, StringComparison.Ordinal);
    }

    [Fact]
    public void EveryCharacterOfTheCheckoutStartPartyHoldsNoKit()
    {
        // D-1350: the start lessons and the start gear give the kit of the start party.
        ContentSet content = ContentSet.Load(ContentFolder.Read(RepositoryRoot.Find()));
        foreach (ContentId id in content.Battle.Fixture.StartParty)
        {
            CharacterRecord record = content.Battle.Character(id);
            Assert.Empty(record.JoinLessons);
            Assert.Empty(record.JoinGear);
        }
    }

    private static Simulation StartWithJoin(BattleContent content) =>
        Simulation.Start(Seed, TestStory.Map, content, TestBattles.Notices, StoryOf(content), DebugIntentHandlers.None);

    /// <summary>Starts the run of the join, changes the party of its first snapshot, and resumes from it, so the values pass the checks of a load (D-166).</summary>
    private static Simulation ResumeWithParty(BattleContent content, Func<PartySnapshot, PartySnapshot> change)
    {
        RunSnapshot start = StartWithJoin(content).Snapshot();
        PartySnapshot party = start.Characters ?? throw new InvalidOperationException("The snapshot holds no party.");
        return Simulation.Resume(Seed, start with { Characters = change(party) }, TestStory.Map, content, TestBattles.Notices, StoryOf(content), DebugIntentHandlers.None);
    }

    /// <summary>Gives the story of the tests with a meeting of one step: the second character joins.</summary>
    private static StoryContent StoryOf(BattleContent content)
    {
        StoryScene join = TestStory.Scene(
            """
            {
             "comment": "The second character joins with its kit.",
             "id": "scene.test_meet",
             "steps": [{ "id": "step.s1", "kind": "join", "character": "character.test_second" }]
            }
            """,
            "join");
        return StoryContent.Load(TestStory.Flags, [join, TestStory.Scene(TestStory.FightFile, "fight"), TestStory.Scene(TestStory.VictoryFile, "victory")], content);
    }

    private static string ReplaceFirst(string text, string from, string to)
    {
        int index = text.IndexOf(from, StringComparison.Ordinal);
        Assert.True(index >= 0, $"The text holds no '{from}'.");
        return string.Concat(text.AsSpan(0, index), to, text.AsSpan(index + from.Length));
    }

    private static ContentId Id(string value) => ContentId.Parse(value, "test", "id");

    /// <summary>Gives the text of each id, because a content id compares by reference.</summary>
    private static List<string?> ValuesOf(IReadOnlyList<ContentId?> ids)
    {
        List<string?> values = [];
        foreach (ContentId? id in ids)
        {
            values.Add(id?.Value);
        }

        return values;
    }
}
