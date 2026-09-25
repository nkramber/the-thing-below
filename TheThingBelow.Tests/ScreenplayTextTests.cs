using System;
using System.Text;
using TheThingBelow.Core.Content;
using TheThingBelow.Core.Story;
using TheThingBelow.Tools.Screenplay;
using Xunit;

namespace TheThingBelow.Tests;

/// <summary>
/// The screenplay text of one story scene: a number on each step, the speaker and the text of
/// each line, and one action line for each other step (D-173, D-1017).
/// </summary>
public sealed class ScreenplayTextTests
{
    /// <summary>The string table of the story fixture, with the name of the ally.</summary>
    public static readonly StringTable Strings = StringTable.Read(
        Encoding.UTF8.GetBytes($$"""{ "comment": "c", "strings": [ {{TestStory.Strings}}, { "id": "name.test_second", "text": "Ossa" } ] }"""),
        StringTable.Path);

    [Fact]
    public void TheCommandPrintsAFixtureSceneWithEachLineInTheOrderOfTheSteps()
    {
        // Exit test 1 of PR-50.
        string text = ScreenplayText.Write(TestStory.Scene(TestStory.MeetFile, "meet"), Strings);

        const string Expected = """
            ### scene.test_meet (`rules/scenes/meet.json`)

            `[0]` *Ossa appears at marker.test_story_door, facing west.*

            `[1]` *The view moves to marker.test_story_door.*

            `[2]` *Ossa walks west 2.*

            `[3]` *The lead faces east.*

            `[4]` **OSSA**
            > You came.

            `[5]` *Pause: 5 ticks.*

            `[6]` *Choice:*
            > 1. Yes. *(sets flag.test_yes)*
            > 2. No. *(sets flag.test_no)*

            `[7]` *Sets flag.test_met.*

            `[8]` *Ossa joins the party.*

            `[9]` *Ossa leaves.*

            """;
        Assert.Equal(Expected.ReplaceLineEndings("\n"), text);
    }

    [Fact]
    public void ALineWithNoSpeakerIsItalicAndTheLeadIsTheLead()
    {
        string text = ScreenplayText.Write(TestStory.Scene(TestStory.FightFile, "fight"), Strings);

        const string Expected = """
            ### scene.test_fight (`rules/scenes/fight.json`)

            `[0]` *(no speaker)*
            > *Steel, in the dark.*

            `[1]` *Battle: group.one.*

            `[2]` *The lead walks east 1.*

            `[3]` **THE LEAD**
            > Move.

            `[4]` *Sets flag.test_done.*

            """;
        Assert.Equal(Expected.ReplaceLineEndings("\n"), text);
    }

    [Fact]
    public void APathPrintsAsRunsOfOneDirection()
    {
        StoryScene scene = TestStory.Scene(
            """{ "comment": "c", "id": "scene.test_walk", "steps": [ { "id": "step.s1", "kind": "move", "actor": "lead", "path": ["north", "north", "east", "north"] } ] }""",
            "walk");

        string text = ScreenplayText.Write(scene, Strings);

        Assert.Contains("`[0]` *The lead walks north 2, east 1, north 1.*", text, StringComparison.Ordinal);
    }

    [Fact]
    public void ALineOfTwoLinesStaysOneQuote()
    {
        StringTable strings = StringTable.Read(
            Encoding.UTF8.GetBytes("""{ "comment": "c", "strings": [ { "id": "line.test_two", "text": "One.\nTwo." } ] }"""),
            StringTable.Path);
        StoryScene scene = TestStory.Scene(
            """{ "comment": "c", "id": "scene.test_two", "steps": [ { "id": "step.s2", "kind": "say", "speaker": "lead", "line": "line.test_two" } ] }""",
            "two");

        string text = ScreenplayText.Write(scene, strings);

        Assert.Contains("**THE LEAD**\n> One.\n> Two.\n", text, StringComparison.Ordinal);
    }

    [Fact]
    public void AStorySceneThatNamesAnAbsentStringIdFailsWithTheStorySceneTheStepAndTheId()
    {
        // Exit test 2 of PR-50 (G-7, T-2).
        StringTable strings = StringTable.Read(
            Encoding.UTF8.GetBytes("""{ "comment": "c", "strings": [ { "id": "name.test_second", "text": "Ossa" }, { "id": "line.test_greet", "text": "You came." }, { "id": "line.test_yes", "text": "Yes." } ] }"""),
            StringTable.Path);

        ContentException error = Assert.Throws<ContentException>(
            () => ScreenplayText.Write(TestStory.Scene(TestStory.MeetFile, "meet"), strings));

        Assert.Contains("rules/scenes/meet.json", error.Message, StringComparison.Ordinal);
        Assert.Contains("scene.test_meet.steps[6].options[1].line", error.Message, StringComparison.Ordinal);
        Assert.Contains("line.test_no", error.Message, StringComparison.Ordinal);
    }

    [Fact]
    public void ACastMemberWithNoNameFailsWithTheStepAndTheNameId()
    {
        StringTable strings = StringTable.Read(
            Encoding.UTF8.GetBytes($$"""{ "comment": "c", "strings": [ {{TestStory.Strings}} ] }"""),
            StringTable.Path);

        ContentException error = Assert.Throws<ContentException>(
            () => ScreenplayText.Write(TestStory.Scene(TestStory.MeetFile, "meet"), strings));

        Assert.Contains("scene.test_meet.steps[0].actor", error.Message, StringComparison.Ordinal);
        Assert.Contains("name.test_second", error.Message, StringComparison.Ordinal);
    }
}
