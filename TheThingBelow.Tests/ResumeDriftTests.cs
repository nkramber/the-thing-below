using System;
using System.Collections.Generic;
using System.Globalization;
using System.Security.Cryptography;
using System.Text;
using TheThingBelow.Core;
using TheThingBelow.Core.Battles;
using TheThingBelow.Core.Content;
using TheThingBelow.Core.Logging;
using TheThingBelow.Core.Maps;
using TheThingBelow.Core.Runs;
using TheThingBelow.Core.Saves;
using TheThingBelow.Core.Story;
using TheThingBelow.Tools.Content;
using Xunit;

namespace TheThingBelow.Tests;

/// <summary>
/// A resume of a save that another build wrote, after an edit of a map, a story scene, or a
/// party rule (D-1110 to D-1113). Each test writes the stored values on one content, and it
/// resumes them against edited content.
/// </summary>
public sealed class ResumeDriftTests
{
    private const string Route = """
       "routes": [
        { "times": ["dawn", "day", "dusk", "night"], "tiles": [{ "x": 1, "y": 1 }, { "x": 3, "y": 1 }] }
       ]
      """;

    private const string SouthRoute = """
       "routes": [
        { "times": ["dawn", "day", "dusk", "night"], "tiles": [{ "x": 1, "y": 5 }, { "x": 3, "y": 5 }] }
       ]
      """;

    private const string EastPost = """
       "routes": [
        { "times": ["dawn", "day", "dusk", "night"], "tiles": [{ "x": 7, "y": 2 }] }
       ]
      """;

    private const string PauseStart = "{ \"id\": \"step.pause\", ";

    private const string ExtraWaitAndPauseStart = "{ \"id\": \"step.extra\", \"kind\": \"wait\", \"ticks\": 5 },\n  { \"id\": \"step.pause\", ";

    private const string LastStep = "{ \"id\": \"step.ally_leaves\", \"kind\": \"hide\", \"actor\": \"character.test_second\" }";

    private const string LastStepAndCoda = LastStep + ",\n  { \"id\": \"step.coda\", \"kind\": \"wait\", \"ticks\": 2 }";

    /// <summary>
    /// The digest of every party rule number that a save depends on. A change of one refuses
    /// each save of the older content, so the patch that makes it ships a migration (D-1110).
    /// </summary>
    private const string PartyRuleDigest = "465906ad26b50e7b1f35ad3dd515a0a27644183965b2a66a8debd0450c59f03a";

    [Fact]
    public void AnotherBuildPlacesANewEnemyOnItsStationAndLogsIt()
    {
        GameMap before = PatrolMaps.Of(PatrolMaps.Enemy(stations: Route));
        GameMap after = PatrolMaps.Of($"{PatrolMaps.Enemy(stations: Route)},\n{PatrolMaps.Enemy(id: "patrol.two", stations: EastPost)}");
        IReadOnlyList<PatrolValues> stored = MapPatrols.Enter(before).Values();
        ResumeDrift drift = Other();

        MapState party = Resume(after, after.Spawn, stored, null, null, drift);

        Assert.Equal(["patrol.one", "patrol.two"], Ids(party.Patrols));
        Assert.Equal(new TilePoint(7, 2), party.Patrols.All[1].At);
        LogEntry entry = Assert.Single(drift.Entries);
        Assert.Equal(LogLevel.Warning, entry.Level);
        Assert.Contains("the save lacks", entry.Message, StringComparison.Ordinal);
        Assert.Equal("patrol.two", FieldOf(entry, "enemy"));
    }

    [Fact]
    public void ThisBuildStillRefusesASaveWithAnotherEnemyCount()
    {
        // The boundary of D-1111: a snapshot of this build keeps the strict check of D-750.
        GameMap before = PatrolMaps.Of(PatrolMaps.Enemy(stations: Route));
        GameMap after = PatrolMaps.Of($"{PatrolMaps.Enemy(stations: Route)},\n{PatrolMaps.Enemy(id: "patrol.two", stations: EastPost)}");

        ArgumentException error = Assert.Throws<ArgumentException>(
            () => Resume(after, after.Spawn, MapPatrols.Enter(before).Values(), null, null, This()));

        Assert.Contains("holds 1 enemies", error.Message, StringComparison.Ordinal);
    }

    [Fact]
    public void AnotherBuildMatchesEachEnemyByItsIdWhenTheMapFileMovesIt()
    {
        GameMap before = PatrolMaps.Of($"{PatrolMaps.Enemy(stations: Route)},\n{PatrolMaps.Enemy(id: "patrol.two", stations: EastPost)}");
        GameMap after = PatrolMaps.Of($"{PatrolMaps.Enemy(id: "patrol.two", stations: EastPost)},\n{PatrolMaps.Enemy(stations: Route)}");
        IReadOnlyList<PatrolValues> stored = MapPatrols.Enter(before).Values();
        ResumeDrift drift = Other();

        MapState party = Resume(after, after.Spawn, stored, null, null, drift);

        Assert.Equal(["patrol.two", "patrol.one"], Ids(party.Patrols));
        Assert.Equal(new TilePoint(1, 1), party.Patrols.All[1].At);
        Assert.Empty(drift.Entries);
    }

    [Fact]
    public void AnotherBuildDropsAnEnemyThatTheMapNoLongerPlacesAndEndsItsMark()
    {
        GameMap before = PatrolMaps.Of($"{PatrolMaps.Enemy(stations: Route)},\n{PatrolMaps.Enemy(id: "patrol.two", stations: EastPost)}");
        GameMap after = PatrolMaps.Of(PatrolMaps.Enemy(stations: Route));
        var mark = new SightMark(Id("patrol.two"), 10);
        ResumeDrift drift = Other();

        MapState party = Resume(after, after.Spawn, MapPatrols.Enter(before).Values(), mark, null, drift);

        Assert.Equal(["patrol.one"], Ids(party.Patrols));
        Assert.Null(party.Patrols.Mark);
        Assert.Equal(2, drift.Entries.Count);
        Assert.Contains("no longer places", drift.Entries[0].Message, StringComparison.Ordinal);
        Assert.Contains("the mark ends", drift.Entries[1].Message, StringComparison.Ordinal);
    }

    [Fact]
    public void AnotherBuildRefusesAnEncounterWithAnEnemyThatTheMapNoLongerPlaces()
    {
        GameMap before = PatrolMaps.Of($"{PatrolMaps.Enemy(stations: Route)},\n{PatrolMaps.Enemy(id: "patrol.two", stations: EastPost)}");
        GameMap after = PatrolMaps.Of(PatrolMaps.Enemy(stations: Route));
        var encounter = new MapEncounter(Id("patrol.two"), Id("group.one"), EncounterSide.Party);

        ArgumentException error = Assert.Throws<ArgumentException>(
            () => Resume(after, after.Spawn, MapPatrols.Enter(before).Values(), null, encounter, Other()));

        Assert.Contains("no enemy to end", error.Message, StringComparison.Ordinal);
    }

    [Fact]
    public void AnotherBuildStartsAnEnemyAgainWhenItsEditedRouteTakesNoStoredPlaceAndKeepsItDead()
    {
        GameMap after = PatrolMaps.Of(PatrolMaps.Enemy(stations: SouthRoute));
        var stored = new PatrolValues(Id("patrol.one"), 2, 1, StepDirection.East, null, 0, 1, true, 0, true);
        ResumeDrift drift = Other();

        MapState party = Resume(after, after.Spawn, [stored], null, null, drift);

        PatrolState patrol = Assert.Single(party.Patrols.All);
        Assert.Equal(new TilePoint(1, 5), patrol.At);
        Assert.True(patrol.Dead);
        LogEntry entry = Assert.Single(drift.Entries);
        Assert.Contains("starts on its station again", entry.Message, StringComparison.Ordinal);
    }

    [Fact]
    public void AnotherBuildMovesALeadOffTheSmallerMapToTheSpawnPointAndResizesTheWalkedTiles()
    {
        GameMap before = PatrolMaps.Of(PatrolMaps.Enemy(stations: Route));
        GameMap after = SmallerPatrolMap();
        var lead = new TilePoint(8, 6);
        WalkedTiles walked = WalkedTiles.Empty(before.Width, before.Height);
        walked.Mark(lead);
        walked.Mark(new TilePoint(2, 2));
        ResumeDrift drift = Other();

        MapState party = MapState.Resume(after, new LeadValues(lead, StepDirection.West, StepDirection.West, 3), walked, MapPatrols.Enter(before).Values(), null, null, "the save", drift);

        Assert.Equal(after.Spawn, party.LeadAt);
        Assert.Null(party.Stepping);
        Assert.Equal((6, 8), (party.Walked.Width, party.Walked.Height));
        Assert.True(party.Walked.WasWalked(new TilePoint(2, 2)));
        Assert.True(party.Walked.WasWalked(after.Spawn));
        Assert.Equal(2, drift.Entries.Count);
        Assert.Contains("another size", drift.Entries[0].Message, StringComparison.Ordinal);
        Assert.Contains("lies off the map", drift.Entries[1].Message, StringComparison.Ordinal);
    }

    [Fact]
    public void ThisBuildStillRefusesALeadOffTheMap()
    {
        GameMap after = SmallerPatrolMap();
        WalkedTiles walked = WalkedTiles.Empty(10, 8);
        walked.Mark(new TilePoint(8, 6));

        ArgumentException error = Assert.Throws<ArgumentException>(() => MapState.Resume(
            after, new LeadValues(new TilePoint(8, 6), StepDirection.West, null, 0), walked, null, null, null, "the save", This()));

        Assert.Contains("the lead stands at", error.Message, StringComparison.Ordinal);
    }

    [Fact]
    public void AnotherBuildFindsAStepThatAnInsertedStepMovedAndLogsIt()
    {
        // The reproducer of the review: a wait inserted before the saved step. The index alone
        // resumed in silence on the inserted wait, and the id now finds the saved step.
        StoryContent edited = MeetWith(TestStory.MeetFile.Replace(PauseStart, ExtraWaitAndPauseStart, StringComparison.Ordinal));
        ResumeDrift drift = Other();

        StoryState story = StoryState.Resume(edited, PausedOnTheWait(), TestStory.Map, "the save", drift);

        Assert.Equal(6, story.Step);
        Assert.Equal("step.pause", story.Scene?.StepIds[story.Step].Value);
        Assert.Equal((ScenePhase.Ticks, 3), (story.Phase, story.TicksLeft));
        LogEntry entry = Assert.Single(drift.Entries);
        Assert.Equal("step.pause", FieldOf(entry, "step"));
        Assert.Equal("5", FieldOf(entry, "stored_index"));
        Assert.Equal("6", FieldOf(entry, "index"));
    }

    [Fact]
    public void ThisBuildRefusesAStepIdAtAnotherIndex()
    {
        StoryContent edited = MeetWith(TestStory.MeetFile.Replace(PauseStart, ExtraWaitAndPauseStart, StringComparison.Ordinal));

        ArgumentException error = Assert.Throws<ArgumentException>(
            () => StoryState.Resume(edited, PausedOnTheWait(), TestStory.Map, "the save", This()));

        Assert.Contains("holds that step at index 6", error.Message, StringComparison.Ordinal);
    }

    [Theory]
    [InlineData(SnapshotOrigin.ThisBuild)]
    [InlineData(SnapshotOrigin.OtherBuild)]
    public void AStepIdThatTheStorySceneNoLongerHoldsRefusesTheSave(SnapshotOrigin origin)
    {
        // D-1112: no resume continues at another step, so a deleted step refuses the save.
        StoryContent edited = MeetWith(TestStory.MeetFile.Replace(
            """{ "id": "step.pause", "kind": "wait", "ticks": 5 },""",
            string.Empty,
            StringComparison.Ordinal));

        ArgumentException error = Assert.Throws<ArgumentException>(
            () => StoryState.Resume(edited, PausedOnTheWait(), TestStory.Map, "the save", ResumeDrift.Of(origin, 80)));

        Assert.Contains("'step.pause'", error.Message, StringComparison.Ordinal);
        Assert.Contains("no step of that story scene in this build takes that id", error.Message, StringComparison.Ordinal);
    }

    [Fact]
    public void AnotherBuildEndsAShortenedWaitAtItsNewLength()
    {
        StoryContent edited = MeetWith(TestStory.MeetFile.Replace(
            """{ "id": "step.pause", "kind": "wait", "ticks": 5 }""",
            """{ "id": "step.pause", "kind": "wait", "ticks": 2 }""",
            StringComparison.Ordinal));
        ResumeDrift drift = Other();

        StoryState story = StoryState.Resume(edited, PausedOnTheWait(), TestStory.Map, "the save", drift);

        Assert.Equal((5, 2), (story.Step, story.TicksLeft));
        Assert.Contains("ends at the new length", Assert.Single(drift.Entries).Message, StringComparison.Ordinal);
        Assert.Throws<ArgumentException>(() => StoryState.Resume(edited, PausedOnTheWait(), TestStory.Map, "the save", This()));
    }

    [Fact]
    public void AnotherBuildResumesTheEndMarkAtTheNewEndOfTheStoryScene()
    {
        StoryContent edited = MeetWith(TestStory.MeetFile.Replace(LastStep, LastStepAndCoda, StringComparison.Ordinal));
        var values = new StoryValues([], new SceneValues(TestStory.Meet, 10, StoryScene.EndStep, ScenePhase.Ready, 0, []), false, false, null);
        ResumeDrift drift = Other();

        StoryState story = StoryState.Resume(edited, values, TestStory.Map, "the save", drift);

        Assert.Equal(11, story.Step);
        Assert.Single(drift.Entries);
        Assert.Equal(10, StoryState.Resume(TestStory.Content, values, TestStory.Map, "the save", This()).Step);
    }

    [Fact]
    public void AnotherBuildStillRefusesAPartyThatAChangedLevelTableBreaks()
    {
        // D-1110: a change of a party rule refuses the save for every build, and the patch that
        // makes one ships a migration. Level 2 with no experience stands for a save of a table
        // where level 2 took less experience.
        Simulation run = TestStory.Start(1);
        RunSnapshot snapshot = run.Snapshot();
        PartySnapshot party = snapshot.Characters!;
        CharacterValues marrek = party.Characters[0];
        GrowthValues growth = marrek.Growth! with { Level = 2 };
        RunSnapshot edited = snapshot with { Characters = party with { Characters = [marrek with { Growth = growth }] } };

        ArgumentException error = Assert.Throws<ArgumentException>(() => Simulation.Resume(
            1, edited, TestStory.Map, TestBattles.Content, TestBattles.Notices, TestStory.Content, DebugIntentHandlers.None, ResumeDrift.Of(SnapshotOrigin.OtherBuild, edited.Tick)));

        Assert.Contains("which gives level 1", error.Message, StringComparison.Ordinal);
    }

    [Fact]
    public void TheHeaderNamesThisBuildOnlyForTheSameSimulationVersionAndContentHash()
    {
        SaveHeader header = SaveHeader.ForThisBuild("hash-a", 1);

        Assert.Equal(SnapshotOrigin.ThisBuild, header.OriginFor("hash-a"));
        Assert.Equal(SnapshotOrigin.OtherBuild, header.OriginFor("hash-b"));
        Assert.Equal(SnapshotOrigin.OtherBuild, (header with { SimulationVersion = SimulationVersion.Current - 1 }).OriginFor("hash-a"));
    }

    [Fact]
    public void ADriftOfThisBuildTakesNoChange()
    {
        InvalidOperationException error = Assert.Throws<InvalidOperationException>(
            () => This().Note(LogSubsystems.World, "a change", LogEntry.NoFields));

        Assert.Contains("strict checks alone", error.Message, StringComparison.Ordinal);
    }

    [Fact]
    public void TheStorySnapshotOfFormatThirteenRefusesAStepId()
    {
        const string Text = """{"flags":[],"paused":false,"entry":false,"scene":{"id":"scene.test_meet","step":5,"step_id":"step.pause","phase":"ticks","ticks_left":3,"actors":[]}}""";
        ContentException error = Assert.Throws<ContentException>(() => ReadStory(Text, 13));

        Assert.Contains("predates it (D-1112)", error.Message, StringComparison.Ordinal);
    }

    [Fact]
    public void TheStorySnapshotOfFormatFourteenNeedsAStepId()
    {
        const string Text = """{"flags":[],"paused":false,"entry":false,"scene":{"id":"scene.test_meet","step":5,"phase":"ticks","ticks_left":3,"actors":[]}}""";
        ContentException error = Assert.Throws<ContentException>(() => ReadStory(Text, 14));

        Assert.Contains("step_id", error.Message, StringComparison.Ordinal);
    }

    [Fact]
    public void ThePartyRuleNumbersThatASaveReadsMatchTheirDigest()
    {
        // D-1110: a change of one of these numbers refuses every save of the older content. The
        // PR that makes one writes a migration of the party for saves of the older content hash,
        // with a fixture save, and then takes the new digest here.
        ContentSet content = ContentSet.Load(ContentFolder.Read(RepositoryRoot.Find()));

        string digest = DigestOf(content.Battle);

        Assert.True(
            string.CompareOrdinal(digest, PartyRuleDigest) == 0,
            $"The party rules that a save reads changed, and each save of the older content now fails to load. Write a migration of the party and a fixture save (D-1110), then set the digest to {digest}.");
    }

    private static string DigestOf(BattleContent battle)
    {
        var text = new StringBuilder();
        text.Append("level_experience");
        foreach (int total in battle.Rules.LevelExperience)
        {
            text.Append(' ').Append(total.ToString(CultureInfo.InvariantCulture));
        }

        text.Append("\nslots");
        for (int level = 1; level <= StatCurve.HighestLevel; level += 1)
        {
            text.Append(' ').Append(battle.Rules.SlotsAt(level).ToString(CultureInfo.InvariantCulture));
        }

        foreach (CharacterRecord character in battle.Fixture.Characters)
        {
            text.Append('\n').Append(character.Id.Value);
            for (int level = 1; level <= StatCurve.HighestLevel; level += 1)
            {
                StatRow row = character.At(level);
                text.Append(' ').Append(row.Health.ToString(CultureInfo.InvariantCulture)).Append('/').Append(row.Mp.ToString(CultureInfo.InvariantCulture));
            }
        }

        foreach (LessonRecord lesson in battle.Lessons.Records)
        {
            text.Append('\n').Append(lesson.Id.Value).Append(' ').Append(lesson.MostPoints.ToString(CultureInfo.InvariantCulture));
        }

        AppendLimits(text, battle, battle.Items.Ids);
        AppendLimits(text, battle, battle.Gear.Ids);
        return Convert.ToHexStringLower(SHA256.HashData(Encoding.UTF8.GetBytes(text.ToString())));
    }

    private static void AppendLimits(StringBuilder text, BattleContent battle, IReadOnlyList<ContentId> ids)
    {
        foreach (ContentId id in ids)
        {
            text.Append('\n').Append(id.Value).Append(' ').Append(battle.LimitOf(id).ToString(CultureInfo.InvariantCulture));
        }
    }

    private static MapState Resume(GameMap map, TilePoint lead, IReadOnlyList<PatrolValues> enemies, SightMark? mark, MapEncounter? encounter, ResumeDrift drift)
    {
        WalkedTiles walked = WalkedTiles.Empty(map.Width, map.Height);
        walked.Mark(lead);
        return MapState.Resume(map, new LeadValues(lead, StepDirection.South, null, 0), walked, enemies, mark, encounter, "the save", drift);
    }

    /// <summary>The map of <see cref="PatrolMaps"/> with its east part cut off: 6 by 8 tiles, and the spawn point at (1, 6).</summary>
    private static GameMap SmallerPatrolMap() => TestMaps.Of(
        "patrol-test.json",
        $$"""
        {
         "comment": "the room of the patrol tests, cut to six columns",
         "id": "map.patrol_test",
         "region": "region.test",
         "label": "label.patrol_test",
         "time": "day",
         "dark": false,
         "terrain": [
          "######",
          "#....#",
          "#....#",
          "#...##",
          "#...##",
          "#....#",
          "#....#",
          "######"
         ],
         "things": [
          { "id": "spawn_point.patrol_test_start", "kind": "spawn_point", "x": 1, "y": 6 }
         ],
         "enemies": [
        {{PatrolMaps.Enemy(stations: Route)}}
         ], "triggers": []
        }
        """);

    /// <summary>The meeting of the story fixture, paused on its wait with three ticks left, as save format 14 holds it.</summary>
    private static StoryValues PausedOnTheWait() => new(
        [],
        new SceneValues(TestStory.Meet, 5, Id("step.pause"), ScenePhase.Ticks, 3, [new ActorValues(TestStory.Ally, new TilePoint(5, 1), StepDirection.West)]),
        true,
        false,
        null);

    private static StoryContent MeetWith(string meet) => StoryContent.Load(
        TestStory.Flags,
        [TestStory.Scene(meet, "meet"), TestStory.Scene(TestStory.FightFile, "fight"), TestStory.Scene(TestStory.VictoryFile, "victory")],
        TestBattles.Content);

    private static List<string> Ids(MapPatrols patrols)
    {
        List<string> ids = [];
        foreach (PatrolState patrol in patrols.All)
        {
            ids.Add(patrol.Patrol.Id.Value);
        }

        return ids;
    }

    private static string FieldOf(LogEntry entry, string name)
    {
        foreach (LogField field in entry.Fields)
        {
            if (string.CompareOrdinal(field.Name, name) == 0)
            {
                return field.Value;
            }
        }

        throw new InvalidOperationException($"The log entry '{entry.Message}' holds no field '{name}'.");
    }

    private static StoryValues ReadStory(string text, int format)
    {
        var reader = new ContentReader(Encoding.UTF8.GetBytes(text), "the save");
        return StorySnapshotText.Read(ref reader, format);
    }

    private static ResumeDrift Other() => ResumeDrift.Of(SnapshotOrigin.OtherBuild, 80);

    private static ResumeDrift This() => ResumeDrift.Of(SnapshotOrigin.ThisBuild, 80);

    private static ContentId Id(string value) => ContentId.Parse(value, "test", "id");
}
