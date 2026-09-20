using System;
using TheThingBelow.Core.Content;
using TheThingBelow.Tools.Content;
using Xunit;

namespace TheThingBelow.Tests;

/// <summary>
/// A prompt shows the glyph of the last device that the player touched (D-222, D-711). The
/// tests read the built Game assembly, because Tests takes no reference to Game (D-614).
/// </summary>
/// <remarks>
/// Exit test 7 of PR-61 proves the pick for each of the four sets. Godot reports no
/// controller type, so the name of the pad picks the set through the device table (F-50).
/// PR-78 later overrides the pick with the Steamworks call (D-460, D-553).
/// </remarks>
public sealed class DeviceGlyphTests
{
    private const string DeviceTypeName = "TheThingBelow.Game.Ui.LastDevice";

    [Theory]
    [InlineData("Xbox Wireless Controller", "xbox")]
    [InlineData("Sony DualSense Wireless Controller", "playstation")]
    [InlineData("Sony Interactive Entertainment DUALSHOCK 4", "playstation")]
    [InlineData("Steam Deck Controller", "deck")]
    public void EachPadNameTakesItsGlyphSet(string padName, string set)
    {
        // Exit test 7, with the gamepad half of D-711.
        Assert.Equal(set, SetFor(Devices(), true, padName));
    }

    [Fact]
    public void AKeyEventTakesTheKeyboardSet()
    {
        // D-711. The last key event or mouse event sets the keyboard.
        Assert.Equal("keyboard", SetFor(Devices(), false, string.Empty));
    }

    [Theory]
    [InlineData("Logitech Gamepad F310")]
    [InlineData("")]
    public void APadThatNoRuleNamesTakesTheDefaultSet(string padName)
    {
        // D-711. Every other name gives Xbox, which is the button layout that Godot reports.
        Assert.Equal(Devices().DefaultSet, SetFor(Devices(), true, padName));
        Assert.Equal("xbox", Devices().DefaultSet);
    }

    [Fact]
    public void TheNameOfAPadReadsWithNoCase()
    {
        // D-711. One pad reports `DualSense`, and another reports `SONY DUALSHOCK 4`.
        Assert.Equal("playstation", SetFor(Devices(), true, "SONY DUALSENSE EDGE"));
        Assert.Equal("playstation", SetFor(Devices(), true, "sony dualshock 3"));
    }

    [Fact]
    public void TheDeckRuleWinsOverTheDefault()
    {
        // The rules run in the order of the file, and the first rule that holds wins (D-711).
        Assert.Equal("deck", Devices().Matches[0].Set);
        Assert.Equal("steam deck", Devices().Matches[0].Match);
    }

    [Fact]
    public void EachGlyphSetHoldsTheDrawingOfEachButton()
    {
        // Exit test 7. The content set refuses a build that lacks one of them (D-222, D-711).
        ContentSet content = Content();

        foreach (string set in content.Devices.Sets)
        {
            foreach (string prompt in content.Devices.Prompts)
            {
                string id = DeviceNames.GlyphDrawingId(set, prompt);
                Drawing drawing = content.DrawingOf(ContentId.Parse(id, DeviceNames.Path, set));

                Assert.Equal(16, drawing.Width);
                Assert.Equal(16, drawing.Height);
                Assert.Equal(AtlasPageKind.Ui, drawing.Page);
            }
        }
    }

    [Fact]
    public void EachButtonHasALabelInTheStringTable()
    {
        // G-7. A prompt draws a glyph and a label, and no C# file holds the label.
        ContentSet content = Content();

        foreach (string prompt in content.Devices.Prompts)
        {
            ContentId id = ContentId.Parse($"ui.{prompt}", StringTable.Path, "prompts");
            Assert.True(content.Strings.Contains(id), $"The string table holds no id '{id.Value}' (G-7).");
        }
    }

    [Fact]
    public void TheFileNamesTheFourSets()
    {
        // D-222. The sets cover the keyboard, Xbox, PlayStation, and the Steam Deck.
        Assert.Equal(4, System.Linq.Enumerable.Count(Devices().Sets));
        foreach (string set in new[] { "keyboard", "xbox", "playstation", "deck" })
        {
            Assert.True(Devices().HasSet(set), $"The device table names no set '{set}' (D-222).");
        }
    }

    [Fact]
    public void ARuleThatNamesASetOutsideTheListFails()
    {
        // T-2. A lookup always gives a set whose glyphs the build holds.
        const string body =
            """
            {
             "comment": "a test table",
             "sets": [ "keyboard" ],
             "keyboard_set": "keyboard",
             "default_set": "keyboard",
             "prompts": [ "confirm" ],
             "names": [ { "match": "sony", "set": "playstation" } ]
            }
            """;

        ContentException error = Assert.Throws<ContentException>(
            () => DeviceNames.Read(System.Text.Encoding.UTF8.GetBytes(body), DeviceNames.Path));

        Assert.Equal("names[0].set", error.Field);
        Assert.Contains("playstation", error.Message, StringComparison.Ordinal);
    }

    [Fact]
    public void AGlyphSetWithNoDrawingFails()
    {
        // T-2. A build that lacks one glyph would draw nothing for that button.
        const string body =
            """
            {
             "comment": "a test table",
             "sets": [ "keyboard" ],
             "keyboard_set": "keyboard",
             "default_set": "keyboard",
             "prompts": [ "confirm", "jump" ],
             "names": [ ]
            }
            """;

        using var checkout = ContentCheckout.Copy();
        checkout.WriteRuleFile(DeviceNames.Path, body);

        ContentException error = Assert.Throws<ContentException>(
            () => ContentSet.Load(ContentFolder.Read(checkout.Root)));

        Assert.Equal(DeviceNames.Path, error.File);
        Assert.Contains("ui_glyph_keyboard_jump", error.Message, StringComparison.Ordinal);
    }

    private static ContentSet Content() => ContentSet.Load(ContentFolder.Read(RepositoryRoot.Find()));

    private static DeviceNames Devices() => Content().Devices;

    private static string SetFor(DeviceNames names, bool fromPad, string padName) =>
        (string)GameAssemblyFile.Type(DeviceTypeName)
            .GetMethod("SetFor")!
            .Invoke(null, [names, fromPad, padName])!;
}
