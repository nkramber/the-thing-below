using System;
using System.Text;
using TheThingBelow.Core.Battles;
using TheThingBelow.Core.Content;
using Xunit;

namespace TheThingBelow.Tests;

/// <summary>The reader of the lesson file: the name, the kind, and the forms of each lesson (D-539, D-1026). Each error names the file (T-2).</summary>
public sealed class LessonListTests
{
    [Fact]
    public void EachLessonReadsItsKindAndItsFormsInOrder()
    {
        LessonList list = Read(TestBattles.LessonsFile);

        LessonRecord cinder = list.Lesson(Id("lesson.test_cinder"));
        Assert.Equal((AptitudeKind.Harm, "name.test_cinder"), (cinder.Kind, cinder.Name.Value));
        Assert.Equal(2, cinder.Forms.Count);
        Assert.Equal(("ability.test_cinder", 0, 4, "name.test_cinder", "lesson.test_cinder"), FieldsOf(cinder.Forms[0]));
        Assert.Equal(("ability.test_blaze", 120, 9, "name.test_blaze", "lesson.test_blaze"), FieldsOf(cinder.Forms[1]));
        Assert.True(cinder.IsRite);
        Assert.False(list.Lesson(Id("lesson.test_hew")).IsRite);
        Assert.Equal("lesson.test_hew", list.Ids[0].Value);
    }

    [Fact]
    public void APointTotalOpensEachFormAtOrBelowIt()
    {
        // D-539: each form names the point total that opens it, and the first opens at zero.
        LessonRecord cinder = Read(TestBattles.LessonsFile).Lesson(Id("lesson.test_cinder"));

        Assert.Equal(1, cinder.OpenedAt(0));
        Assert.Equal(1, cinder.OpenedAt(119));
        Assert.Equal(2, cinder.OpenedAt(120));
        Assert.Equal(2, cinder.OpenedAt(5000));
        Assert.Equal(120, cinder.MostPoints);
        Assert.Throws<ArgumentOutOfRangeException>(() => cinder.OpenedAt(-1));
    }

    [Fact]
    public void EveryKindOfD281ReadsByItsName()
    {
        foreach (AptitudeKind kind in Aptitudes.All)
        {
            Assert.True(Aptitudes.TryOf(Aptitudes.NameOf(kind), out AptitudeKind read), $"The kind {kind} reads no name.");
            Assert.Equal(kind, read);
        }

        Assert.Equal(8, Aptitudes.All.Count);
        Assert.False(Aptitudes.TryOf("stealth", out _));
    }

    [Fact]
    public void TheFourRiteKindsAreMendHarmBlightAndBoon()
    {
        // D-281: four kinds for rites and four for drills. Silence stops a rite alone (D-806).
        AptitudeKind[] rites = [AptitudeKind.Mend, AptitudeKind.Harm, AptitudeKind.Blight, AptitudeKind.Boon];
        foreach (AptitudeKind kind in Aptitudes.All)
        {
            Assert.Equal(Array.IndexOf(rites, kind) >= 0, Aptitudes.IsRite(kind));
        }
    }

    [Theory]
    [InlineData("\"id\": \"lesson.test_hew\"", "\"id\": \"ability.test_hew\"", "lesson")]
    [InlineData("\"kind\": \"blade\"", "\"kind\": \"stealth\"", "the kind 'stealth'")]
    [InlineData("\"kind\": \"blade\", ", "", "absent")]
    [InlineData("\"points\": 60", "\"points\": 0", "opens above the form before it")]
    [InlineData("\"ability\": \"ability.test_hew\", \"points\": 0", "\"ability\": \"ability.test_hew\", \"points\": 5", "a lesson starts at its first form")]
    [InlineData("\"points\": 0, \"mp\": 0, \"name\": \"name.test_hew\"", "\"points\": -1, \"mp\": 0, \"name\": \"name.test_hew\"", "outside 0 to")]
    [InlineData("\"mp\": 4", "\"mp\": 1000", "the MP cost 1000")]
    [InlineData("\"mp\": 4", "\"mp\": -1", "the MP cost -1")]
    [InlineData(", \"description\": \"lesson.test_hew\"", "", "absent")]
    [InlineData("\"ability\": \"ability.test_hew\"", "\"ability\": \"item.test_hew\"", "ability")]
    [InlineData("\"name\": \"name.test_hew\", \"kind\"", "\"name\": \"name.test_hew\", \"tier\": 1, \"kind\"", "unknown field")]
    public void ALessonFileThatBreaksARuleFailsWithTheFile(string from, string to, string reason)
    {
        int at = TestBattles.LessonsFile.IndexOf(from, StringComparison.Ordinal);
        Assert.True(at >= 0, $"The lesson text holds no '{from}'.");
        string text = string.Concat(TestBattles.LessonsFile.AsSpan(0, at), to, TestBattles.LessonsFile.AsSpan(at + from.Length));

        ContentException error = Assert.Throws<ContentException>(() => Read(text));

        Assert.Equal(LessonList.Path, error.File);
        Assert.Contains(reason, error.Message, StringComparison.Ordinal);
    }

    [Fact]
    public void ALessonWithNoFormFails()
    {
        const string text = """{ "comment": "c", "lessons": [{ "id": "lesson.a", "name": "name.a", "kind": "mend", "forms": [] }] }""";

        ContentException error = Assert.Throws<ContentException>(() => Read(text));

        Assert.Contains("holds no form", error.Message, StringComparison.Ordinal);
    }

    [Fact]
    public void ARepeatedIdFailsWithTheId()
    {
        const string lesson = """{ "id": "lesson.a", "name": "name.a", "kind": "mend", "forms": [{ "ability": "ability.a", "points": 0, "mp": 1, "name": "name.a", "description": "lesson.a" }] }""";

        ContentException error = Assert.Throws<ContentException>(() => Read($$"""{ "comment": "c", "lessons": [{{lesson}}, {{lesson}}] }"""));

        Assert.Contains("lesson.a", error.Message, StringComparison.Ordinal);
        Assert.Contains("two times", error.Message, StringComparison.Ordinal);
    }

    [Fact]
    public void AnAbsentIdFailsWithTheFileAndTheId()
    {
        ContentException error = Assert.Throws<ContentException>(() => Read(TestBattles.LessonsFile).Lesson(Id("lesson.absent")));

        Assert.Equal(LessonList.Path, error.File);
        Assert.Contains("lesson.absent", error.Message, StringComparison.Ordinal);
        Assert.False(Read(TestBattles.LessonsFile).Holds(Id("lesson.absent")));
    }

    [Fact]
    public void TheCheckoutLessonFileLoadsWithEachFormOnAnAbilityOfTheCheckout()
    {
        // D-1026: the load of the content set checks each ability that a form names.
        LessonList lessons = LessonList.Read(System.IO.File.ReadAllBytes(RepositoryRoot.PathTo("content/rules/lessons.json")), LessonList.Path);
        AbilityList abilities = AbilityList.Read(System.IO.File.ReadAllBytes(RepositoryRoot.PathTo("content/rules/abilities.json")), AbilityList.Path);

        foreach (LessonRecord lesson in lessons.Records)
        {
            foreach (LessonForm form in lesson.Forms)
            {
                Assert.True(abilities.Holds(form.Ability), $"The form '{form.Ability.Value}' of '{lesson.Id.Value}' names no ability.");
            }
        }
    }

    private static LessonList Read(string text) => LessonList.Read(Encoding.UTF8.GetBytes(text), LessonList.Path);

    private static ContentId Id(string value) => ContentId.Parse(value, "test", "id");

    private static (string, int, int, string, string) FieldsOf(LessonForm form) =>
        (form.Ability.Value, form.Points, form.Mp, form.Name.Value, form.Description.Value);
}
