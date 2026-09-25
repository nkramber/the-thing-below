using System;
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

    [Theory]
    [InlineData("""{ "": 1 }""")]
    [InlineData("""{ ".": 1 }""")]
    [InlineData("""{ "...": 1 }""")]
    public void AFieldNameOfNoCharacterOrOfPointsAloneFailsWithTheFile(string json)
    {
        // Finding P3-28 of the repository review: the empty name raised an argument error with no
        // file, because the name gave no segment of the path (T-2).
        ContentException error = ReadAndFail(json);

        Assert.Equal(File, error.File);
        Assert.Equal(ContentException.WholeFile, error.Field);
        Assert.Contains("a character other than a point", error.Message, StringComparison.Ordinal);
    }

    [Fact]
    public void AnEmptyFieldNameInsideAnElementNamesTheElement()
    {
        ContentException error = ReadAndFail(
            """
            {
             "comment": "a note",
             "fixtures": [{ "": 1 }]
            }
            """);

        Assert.Equal("fixtures[0]", error.Field);
    }

    [Fact]
    public void AFieldNameWithAPointAndALetterStillReadsAsAnUnknownField()
    {
        // The boundary: a point inside a name is a character of that name.
        ContentException error = ReadAndFail("""{ "a.b": 1 }""");

        Assert.Contains("unknown field", error.Message, StringComparison.Ordinal);
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

    [Fact]
    public void AFieldInTheObjectTwoTimesFails()
    {
        // The last value would win in silence, so the reader refuses the second one (G-6).
        ContentException error = ReadAndFail(
            """
            {
             "comment": "a note",
             "comment": "a second note",
             "fixtures": []
            }
            """);

        Assert.Equal(File, error.File);
        Assert.Equal("comment", error.Field);
        Assert.Contains("two times", error.Message);
    }

    [Fact]
    public void AFieldInAnElementTwoTimesNamesTheElement()
    {
        ContentException error = ReadAndFail(
            """
            {
             "comment": "a note",
             "fixtures": [
              { "id": "fixture.lamp", "label": "label.lamp", "weight": 1, "weight": 2 }
             ]
            }
            """);

        Assert.Equal("fixtures[0].weight", error.Field);
        Assert.Contains("two times", error.Message);
    }

    [Fact]
    public void TheSameFieldInTwoElementsReads()
    {
        // Each object holds its own fields, so a second element takes the same names.
        RuleFixture fixture = ReadFixture(
            """
            {
             "comment": "a note",
             "fixtures": [
              { "id": "fixture.lamp", "label": "label.lamp", "weight": 1 },
              { "id": "fixture.rope", "label": "label.rope", "weight": 2 }
             ]
            }
            """);

        Assert.Equal(2, fixture.Entries.Count);
    }

    [Fact]
    public void ALoneSurrogateEscapeFailsWithTheField()
    {
        // `Utf8JsonReader` accepts the form of the escape and refuses its value later, with
        // an error that names no file. The reader adds the file and the field (T-2).
        ContentException error = ReadAndFail(
            """
            {
             "comment": "\ud800",
             "fixtures": []
            }
            """);

        Assert.Equal(File, error.File);
        Assert.Equal("comment", error.Field);
        Assert.Contains("not valid text", error.Message);
    }

    [Fact]
    public void AByteThatIsNotUtf8FailsWithTheField()
    {
        byte[] head = Encoding.UTF8.GetBytes("{ \"comment\": \"a ");
        byte[] tail = Encoding.UTF8.GetBytes(" note\", \"fixtures\": [] }");
        byte[] bytes = [.. head, 0xFF, .. tail];

        ContentException error = Assert.Throws<ContentException>(() => RuleFixture.Read(bytes, File));

        Assert.Equal("comment", error.Field);
        Assert.Contains("not valid text", error.Message);
    }

    [Fact]
    public void AByteOrderMarkFailsWithTheReasonOfTheReader()
    {
        byte[] text = Encoding.UTF8.GetBytes("""{ "comment": "a note", "fixtures": [] }""");
        byte[] bytes = [0xEF, 0xBB, 0xBF, .. text];

        ContentException error = Assert.Throws<ContentException>(() => RuleFixture.Read(bytes, File));

        // The message of the JSON reader names the byte, so a reader of the error sees the
        // mark and not a comment (T-2).
        Assert.Equal(File, error.File);
        Assert.Contains("byte order mark", error.Message);
        Assert.Contains("0xEF", error.Message);
    }

    [Fact]
    public void ANumberThatNoSixtyFourBitValueHoldsFails()
    {
        static void Read()
        {
            var reader = new ContentReader(Encoding.UTF8.GetBytes("""{ "tick": 9223372036854775808 }"""), File);
            int depth = reader.ReadObjectStart();
            reader.ReadNextField(depth, out _);
            reader.ReadLong();
        }

        ContentException error = Assert.Throws<ContentException>(Read);

        Assert.Equal("tick", error.Field);
        Assert.Contains("64-bit", error.Message);
    }

    [Fact]
    public void ATextWhereTheRecordNeedsTrueOrFalseFails()
    {
        static void Read()
        {
            var reader = new ContentReader(Encoding.UTF8.GetBytes("""{ "menu": "yes" }"""), File);
            int depth = reader.ReadObjectStart();
            reader.ReadNextField(depth, out _);
            reader.ReadBoolean();
        }

        ContentException error = Assert.Throws<ContentException>(Read);

        Assert.Equal("menu", error.Field);
        Assert.Contains("true or false", error.Message);
    }

    private static RuleFixture ReadFixture(string json) =>
        RuleFixture.Read(Encoding.UTF8.GetBytes(json), File);

    private static ContentException ReadAndFail(string json) =>
        Assert.Throws<ContentException>(() => ReadFixture(json));
}
