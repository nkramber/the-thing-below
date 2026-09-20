using System;
using System.Collections.Generic;
using System.Reflection;
using TheThingBelow.Core.Content;
using TheThingBelow.Tools.Content;
using Xunit;

namespace TheThingBelow.Tests;

/// <summary>
/// The one helper that puts a player string on screen (G-7, D-499). It takes a string table
/// id and the values to fill in, and it never takes a literal.
/// </summary>
/// <remarks>
/// det-lint fails a Godot text property outside this type, under DL 8 (D-499, D-614). Thus
/// the full name of the type is part of the contract, and the first test pins it.
/// </remarks>
public sealed class TextHelperTests
{
    private const string HelperTypeName = "TheThingBelow.Game.Ui.TextHelper";

    [Fact]
    public void TheHelperCarriesTheNameThatTheLintNames()
    {
        // D-499, D-614. DL 8 names this type, and the lint reads no other one.
        string named = (string)typeof(Tools.DetLint.GodotTextRule)
            .GetField("TextHelperType")!
            .GetValue(null)!;

        Assert.Equal(HelperTypeName, named);
        Assert.NotNull(GameAssemblyFile.Type(HelperTypeName));
    }

    [Fact]
    public void ATextWithNoPlaceReadsAsItIs()
    {
        Assert.Equal("The game stopped.", Fill("The game stopped.", "crash.title", []));
    }

    [Fact]
    public void APlaceTakesItsValue()
    {
        Assert.Equal(
            "It wrote a report: crash-1.json",
            Fill("It wrote a report: {file}", "crash.file", new() { ["file"] = "crash-1.json" }));
    }

    [Fact]
    public void TwoPlacesEachTakeTheirValue()
    {
        Assert.Equal(
            "a to b",
            Fill("{one} to {two}", "test.line", new() { ["one"] = "a", ["two"] = "b" }));
    }

    [Fact]
    public void APlaceWithNoValueFails()
    {
        // T-2. A message that shows `{file}` to the player tells that player nothing.
        ContentException error = Assert.Throws<ContentException>(
            () => Fill("It wrote a report: {file}", "crash.file", []));

        Assert.Equal(StringTable.Path, error.File);
        Assert.Equal("crash.file", error.Field);
        Assert.Contains("'file'", error.Message, StringComparison.Ordinal);
    }

    [Fact]
    public void APlaceWithNoClosingMarkFails()
    {
        // T-2. A text that opens a place and never closes it is a fault of the table.
        ContentException error = Assert.Throws<ContentException>(
            () => Fill("It wrote {file", "crash.file", new() { ["file"] = "one" }));

        Assert.Contains("no '}'", error.Message, StringComparison.Ordinal);
    }

    [Fact]
    public void EveryPlaceOfTheStringTableHasAName()
    {
        // G-7. A place with an empty name could never take a value (T-2).
        StringTable table = ContentSet.Load(ContentFolder.Read(RepositoryRoot.Find())).Strings;

        foreach (string id in table.Ids)
        {
            string text = table.Text(ContentId.Parse(id, StringTable.Path, "id"));
            int open = text.IndexOf('{', StringComparison.Ordinal);
            while (open >= 0)
            {
                int close = text.IndexOf('}', open + 1);
                Assert.True(close > open + 1, $"The string '{id}' holds a place with no name (T-2).");
                open = text.IndexOf('{', close + 1);
            }
        }
    }

    private static string Fill(string text, string id, Dictionary<string, string> values)
    {
        ContentId content = ContentId.Parse(id, StringTable.Path, "id");
        try
        {
            return (string)GameAssemblyFile.Type(HelperTypeName)
                .GetMethod("Fill")!
                .Invoke(null, [text, content, values])!;
        }
        catch (TargetInvocationException thrown) when (thrown.InnerException is not null)
        {
            // The test reads the error of the method, and not the wrapper of reflection (T-2).
            throw thrown.InnerException;
        }
    }
}
