using System.Text;
using TheThingBelow.Core.Content;
using Xunit;

namespace TheThingBelow.Tests;

/// <summary>
/// The strict reader refuses an absent field, an unknown field, a wrong type, and a number
/// that is not whole. Every error names the file and the field (D-116, D-177, G-6, T-2).
/// </summary>
public sealed class ContentReaderTests
{
    /// <summary>The path that each error of this class names.</summary>
    private const string File = "rules/fixtures/tools.json";

    [Fact]
    public void AWellFormedFileReads()
    {
        RuleFixture fixture = ReadFixture(
            """
            {
             "comment": "a note",
             "fixtures": [
              { "id": "fixture.lamp", "label": "label.lamp", "weight": 10000 }
             ]
            }
            """);

        Assert.Equal("a note", fixture.Comment);
        RuleFixtureEntry entry = Assert.Single(fixture.Entries);
        Assert.Equal("fixture.lamp", entry.Id.Value);
        Assert.Equal("label.lamp", entry.Label.Value);
        Assert.Equal(10000, entry.Weight);
    }

    [Fact]
    public void AnAbsentFieldOfTheFileFails()
    {
        ContentException error = ReadAndFail("""{ "comment": "a note" }""");

        Assert.Equal(File, error.File);
        Assert.Equal("fixtures", error.Field);
        Assert.Contains("the field is absent", error.Message);
    }

    [Fact]
    public void AnAbsentFieldOfAnElementNamesTheElement()
    {
        ContentException error = ReadAndFail(
            """
            {
             "comment": "a note",
             "fixtures": [
              { "id": "fixture.lamp", "label": "label.lamp", "weight": 1 },
              { "id": "fixture.rope", "weight": 1 }
             ]
            }
            """);

        Assert.Equal("fixtures[1].label", error.Field);
        Assert.Contains("the field is absent", error.Message);
    }

    [Fact]
    public void AnUnknownFieldFails()
    {
        ContentException error = ReadAndFail(
            """
            {
             "comment": "a note",
             "fixtures": [],
             "notes": "a field that no record holds"
            }
            """);

        Assert.Equal("notes", error.Field);
        Assert.Contains("an unknown field", error.Message);
    }

    [Fact]
    public void AnUnknownFieldOfAnElementNamesTheElement()
    {
        ContentException error = ReadAndFail(
            """
            {
             "comment": "a note",
             "fixtures": [
              { "id": "fixture.lamp", "label": "label.lamp", "weight": 1, "colour": "red" }
             ]
            }
            """);

        Assert.Equal("fixtures[0].colour", error.Field);
        Assert.Contains("an unknown field", error.Message);
    }

    [Theory]
    [InlineData("1.5")]
    [InlineData("1.0")]
    [InlineData("1e2")]
    [InlineData("1E2")]
    [InlineData("-2.5")]
    [InlineData("1.5e3")]
    public void ANumberWithAFractionOrAnExponentFails(string number)
    {
        ContentException error = ReadAndFail(
            $$"""
            {
             "comment": "a note",
             "fixtures": [
              { "id": "fixture.lamp", "label": "label.lamp", "weight": {{number}} }
             ]
            }
            """);

        Assert.Equal("fixtures[0].weight", error.Field);
        Assert.Contains(number, error.Message);
        Assert.Contains("fraction or an exponent", error.Message);
    }

    [Theory]
    [InlineData("0")]
    [InlineData("-1")]
    [InlineData("2147483647")]
    [InlineData("-2147483648")]
    public void AWholeNumberReads(string number)
    {
        RuleFixture fixture = ReadFixture(
            $$"""
            {
             "comment": "a note",
             "fixtures": [
              { "id": "fixture.lamp", "label": "label.lamp", "weight": {{number}} }
             ]
            }
            """);

        Assert.Equal(int.Parse(number, System.Globalization.CultureInfo.InvariantCulture), fixture.Entries[0].Weight);
    }

    [Fact]
    public void ANumberThatNoThirtyTwoBitValueHoldsFails()
    {
        ContentException error = ReadAndFail(
            """
            {
             "comment": "a note",
             "fixtures": [
              { "id": "fixture.lamp", "label": "label.lamp", "weight": 2147483648 }
             ]
            }
            """);

        Assert.Equal("fixtures[0].weight", error.Field);
        Assert.Contains("does not fit", error.Message);
    }

    [Fact]
    public void AFieldOfTheWrongTypeFails()
    {
        ContentException error = ReadAndFail(
            """
            {
             "comment": 7,
             "fixtures": []
            }
            """);

        Assert.Equal("comment", error.Field);
        Assert.Contains("Number", error.Message);
    }

    [Fact]
    public void AnArrayWhereTheRecordNeedsAnObjectFails()
    {
        ContentException error = ReadAndFail("""[ "a list where the file needs an object" ]""");

        Assert.Equal(ContentException.WholeFile, error.Field);
        Assert.Contains("an object", error.Message);
    }

    [Fact]
    public void AnIdOfAnotherFormFailsWithItsField()
    {
        ContentException error = ReadAndFail(
            """
            {
             "comment": "a note",
             "fixtures": [
              { "id": "Fixture.Lamp", "label": "label.lamp", "weight": 1 }
             ]
            }
            """);

        Assert.Equal("fixtures[0].id", error.Field);
        Assert.Contains("D-646", error.Message);
    }

    [Theory]
    [InlineData("enemy.cave_rat")]
    [InlineData("item.rusted_key")]
    [InlineData("label.lamp")]
    public void AnEntryIdOfAnotherKindFails(string id)
    {
        // The kind of an entry id agrees with the file that holds the entry (D-646). The
        // record owns the kind, so a fixture file holds `fixture.` ids alone.
        ContentException error = ReadAndFail(
            $$"""
            {
             "comment": "a note",
             "fixtures": [
              { "id": "{{id}}", "label": "label.lamp", "weight": 1 }
             ]
            }
            """);

        Assert.Equal(File, error.File);
        Assert.Equal("fixtures[0].id", error.Field);
        Assert.Contains(id, error.Message);
        Assert.Contains(RuleFixture.IdKind, error.Message);
        Assert.Contains("D-646", error.Message);
    }

    [Fact]
    public void AStringIdTakesAKindOfItsOwn()
    {
        // The `label` field points at the string table, so its kind names where the player
        // reads the text and never the kind of this record (G-7, D-646).
        RuleFixture fixture = ReadFixture(
            """
            {
             "comment": "a note",
             "fixtures": [
              { "id": "fixture.lamp", "label": "ui.lamp_name", "weight": 1 }
             ]
            }
            """);

        Assert.Equal("ui", fixture.Entries[0].Label.Kind);
    }

    [Fact]
    public void ACommentInTheFileFails()
    {
        // A comment is a silent place for a rule note that no reader reads, so the reader
        // refuses one (G-6). A balance note goes in a `comment` field or in a document.
        ContentException error = ReadAndFail(
            """
            {
             // a note that no record holds
             "comment": "a note",
             "fixtures": []
            }
            """);

        Assert.Equal(File, error.File);
    }

    [Fact]
    public void ATrailingCommaFails()
    {
        ContentException error = ReadAndFail(
            """
            {
             "comment": "a note",
             "fixtures": [],
            }
            """);

        Assert.Equal(File, error.File);
    }

    [Fact]
    public void ATokenAfterTheContentFails()
    {
        ContentException error = ReadAndFail(
            """
            { "comment": "a note", "fixtures": [] } 7
            """);

        // `Utf8JsonReader` refuses a token after the top-level value itself, and the reader
        // turns that refusal into an error that names the file (T-2).
        Assert.Equal(File, error.File);
    }

    [Fact]
    public void AFileThatEndsEarlyFails()
    {
        ContentException error = ReadAndFail("""{ "comment": "a note", "fixtures": [""");

        Assert.Equal(File, error.File);
    }

    private static RuleFixture ReadFixture(string json) =>
        RuleFixture.Read(Encoding.UTF8.GetBytes(json), File);

    private static ContentException ReadAndFail(string json) =>
        Assert.Throws<ContentException>(() => ReadFixture(json));
}
