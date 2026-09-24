using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using TheThingBelow.Core.Content;
using TheThingBelow.Core.Story;
using TheThingBelow.Tools.Screenplay;
using Xunit;

namespace TheThingBelow.Tests;

/// <summary>
/// The batch of the screenplay: each story scene that the PR changes against its base, and
/// each removed story scene file (D-1015).
/// </summary>
public sealed class ScreenplayBatchTests
{
    private const string MeetPath = "rules/scenes/meet.json";

    private const string FightPath = "rules/scenes/fight.json";

    [Fact]
    public void AStorySceneWithNoChangeStaysOutOfTheBatch()
    {
        // Exit test 4 of PR-50.
        ScreenplayBatch batch = Find(Strings(TestStory.Strings), Scenes(), Strings(TestStory.Strings));

        Assert.Empty(batch.Changed);
        Assert.Empty(batch.Removed);
    }

    [Fact]
    public void AChangedStringPutsItsStorySceneIn()
    {
        // Exit test 4 of PR-50: the fight speaks the ambush line, and the meeting does not.
        string head = TestStory.Strings.Replace("Steel, in the dark.", "Steel, in the black.", StringComparison.Ordinal);

        ScreenplayBatch batch = Find(Strings(head), Scenes(), Strings(TestStory.Strings));

        StoryScene changed = Assert.Single(batch.Changed);
        Assert.Equal("scene.test_fight", changed.Id.Value);
    }

    [Fact]
    public void AChangedOptionStringPutsItsStorySceneIn()
    {
        string head = TestStory.Strings.Replace("\"No.\"", "\"Never.\"", StringComparison.Ordinal);

        ScreenplayBatch batch = Find(Strings(head), Scenes(), Strings(TestStory.Strings));

        StoryScene changed = Assert.Single(batch.Changed);
        Assert.Equal("scene.test_meet", changed.Id.Value);
    }

    [Fact]
    public void ANewStringOfTheBasePutsItsStorySceneIn()
    {
        string baseStrings = TestStory.Strings.Replace("{ \"id\": \"line.test_after\", \"text\": \"Move.\" },", string.Empty, StringComparison.Ordinal);

        ScreenplayBatch batch = Find(Strings(TestStory.Strings), Scenes(), Strings(baseStrings));

        StoryScene changed = Assert.Single(batch.Changed);
        Assert.Equal("scene.test_fight", changed.Id.Value);
    }

    [Fact]
    public void AChangedNameAloneChangesNoStoryScene()
    {
        // D-1015: a speaker name is not a line of the story scene.
        string head = TestStory.Strings + ", { \"id\": \"name.test_second\", \"text\": \"Ossa\" }";
        string baseStrings = TestStory.Strings + ", { \"id\": \"name.test_second\", \"text\": \"Osa\" }";

        ScreenplayBatch batch = Find(Strings(head), Scenes(), Strings(baseStrings));

        Assert.Empty(batch.Changed);
    }

    [Fact]
    public void AChangedFileAndANewFilePutTheirStoryScenesIn()
    {
        List<ContentFile> baseFiles =
        [
            new(MeetPath, Encoding.UTF8.GetBytes(TestStory.MeetFile + " ")),
            new(StringTable.Path, Strings(TestStory.Strings)),
        ];

        ScreenplayBatch batch = ScreenplayBatch.Find(HeadFiles(TestStory.Strings), Scenes(), StringTable.Read(Strings(TestStory.Strings), StringTable.Path), baseFiles);

        Assert.Equal(["scene.test_fight", "scene.test_meet"], batch.Changed.Select(scene => scene.Id.Value));
    }

    [Fact]
    public void ARemovedFileEntersTheBatch()
    {
        List<ContentFile> baseFiles = BaseFiles(TestStory.Strings);
        baseFiles.Add(new ContentFile("rules/scenes/gone.json", Encoding.UTF8.GetBytes("{}")));

        ScreenplayBatch batch = ScreenplayBatch.Find(HeadFiles(TestStory.Strings), Scenes(), StringTable.Read(Strings(TestStory.Strings), StringTable.Path), baseFiles);

        Assert.Empty(batch.Changed);
        Assert.Equal(["rules/scenes/gone.json"], batch.Removed);
    }

    [Fact]
    public void ABaseWithNoStringTableFailsAndNamesTheFile()
    {
        List<ContentFile> baseFiles = [new(MeetPath, Encoding.UTF8.GetBytes(TestStory.MeetFile))];

        ContentException error = Assert.Throws<ContentException>(
            () => ScreenplayBatch.Find(HeadFiles(TestStory.Strings), Scenes(), StringTable.Read(Strings(TestStory.Strings), StringTable.Path), baseFiles));

        Assert.Contains("base:strings/en.json", error.Message, StringComparison.Ordinal);
    }

    private static ScreenplayBatch Find(byte[] headStrings, List<StoryScene> scenes, byte[] baseStrings)
    {
        List<ContentFile> baseFiles =
        [
            new(FightPath, Encoding.UTF8.GetBytes(TestStory.FightFile)),
            new(MeetPath, Encoding.UTF8.GetBytes(TestStory.MeetFile)),
            new(StringTable.Path, baseStrings),
        ];

        return ScreenplayBatch.Find(HeadFiles(TestStory.Strings), scenes, StringTable.Read(headStrings, StringTable.Path), baseFiles);
    }

    private static List<ContentFile> HeadFiles(string strings) =>
    [
        new(FightPath, Encoding.UTF8.GetBytes(TestStory.FightFile)),
        new(MeetPath, Encoding.UTF8.GetBytes(TestStory.MeetFile)),
        new(StringTable.Path, Strings(strings)),
    ];

    private static List<ContentFile> BaseFiles(string strings) => HeadFiles(strings);

    private static List<StoryScene> Scenes() =>
        [TestStory.Scene(TestStory.FightFile, "fight"), TestStory.Scene(TestStory.MeetFile, "meet")];

    private static byte[] Strings(string entries) =>
        Encoding.UTF8.GetBytes($$"""{ "comment": "c", "strings": [ {{entries}} ] }""");
}
