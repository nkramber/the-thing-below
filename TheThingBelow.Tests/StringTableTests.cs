using System.Text;
using TheThingBelow.Core.Content;
using Xunit;

namespace TheThingBelow.Tests;

/// <summary>
/// The string table maps a string id to the text that the player reads (G-7, D-167). No C#
/// file holds a player string (D-499).
/// </summary>
public sealed class StringTableTests
{
    private const string File = "strings/en.json";

    [Fact]
    public void TheTableReadsTheTextOfAnId()
    {
        StringTable table = Read(
            """
            {
             "comment": "a note",
             "strings": [
              { "id": "label.lamp", "text": "Tin lamp" },
              { "id": "label.rope", "text": "Wet rope" }
             ]
            }
            """);

        Assert.Equal(2, table.Count);
        Assert.Equal("Tin lamp", table.Text(Id("label.lamp")));
        Assert.Equal("Wet rope", table.Text(Id("label.rope")));
    }

    [Fact]
    public void AnIdThatTheTableLacksFails()
    {
        StringTable table = Read(
            """
            {
             "comment": "a note",
             "strings": [ { "id": "label.lamp", "text": "Tin lamp" } ]
            }
            """);

        ContentException error = Assert.Throws<ContentException>(() => table.Text(Id("label.nail")));

        Assert.Equal(StringTable.Path, error.File);
        Assert.Equal("label.nail", error.Field);
    }

    [Fact]
    public void ARepeatedStringIdFails()
    {
        ContentException error = Assert.Throws<ContentException>(
            () => Read(
                """
                {
                 "comment": "a note",
                 "strings": [
                  { "id": "label.lamp", "text": "Tin lamp" },
                  { "id": "label.lamp", "text": "Tin lamp again" }
                 ]
                }
                """));

        Assert.Equal("strings[1].id", error.Field);
        Assert.Contains("label.lamp", error.Message);
        Assert.Contains("two times", error.Message);
    }

    [Fact]
    public void TheIdsReadInOrdinalOrder()
    {
        // The default order of .NET follows the culture of the machine and the ICU version
        // on it, so every order that reaches a test takes an ordinal comparison (F-39).
        StringTable table = Read(
            """
            {
             "comment": "a note",
             "strings": [
              { "id": "label.rope", "text": "Wet rope" },
              { "id": "label.lamp", "text": "Tin lamp" },
              { "id": "label.nail", "text": "Bent nail" }
             ]
            }
            """);

        Assert.Equal(new[] { "label.lamp", "label.nail", "label.rope" }, table.Ids);
    }

    [Fact]
    public void AStringIdOfAnotherFormFails()
    {
        ContentException error = Assert.Throws<ContentException>(
            () => Read(
                """
                {
                 "comment": "a note",
                 "strings": [ { "id": "Label Lamp", "text": "Tin lamp" } ]
                }
                """));

        Assert.Equal("strings[0].id", error.Field);
        Assert.Contains("D-646", error.Message);
    }

    private static ContentId Id(string value) => ContentId.Parse(value, File, "id");

    private static StringTable Read(string json) => StringTable.Read(Encoding.UTF8.GetBytes(json), File);
}
