using System.Text;
using TheThingBelow.Core.Content;
using Xunit;

namespace TheThingBelow.Tests;

/// <summary>
/// The reader of the palette (D-89, D-181). A repeated key kept the last color in silence in
/// the interim tool, and the reader refuses one with the key and the index (F-20, T-2).
/// </summary>
public sealed class PaletteTests
{
    [Fact]
    public void AWellFormedPaletteReads()
    {
        Palette palette = Read(Body());

        Assert.Equal(2, palette.Colors.Count);
        Assert.Equal("ink", palette.Colors[0].Name);
        Assert.True(palette.TryColorOf('w', out PaletteColor? white));
        Assert.Equal(1, white.Index);
    }

    [Fact]
    public void AColorGivesItsThreeParts()
    {
        Palette palette = Read(Body());

        PaletteColor color = palette.ColorNamed("chalk");

        Assert.Equal(242, color.Red);
        Assert.Equal(238, color.Green);
        Assert.Equal(234, color.Blue);
    }

    [Fact]
    public void TheDotIsTransparentAndBelongsToNoColor()
    {
        Palette palette = Read(Body());

        Assert.False(palette.TryColorOf(Drawing.Transparent, out _));
    }

    [Fact]
    public void ARepeatedKeyFailsWithTheKeyAndTheIndex()
    {
        ContentException error = Fails(Body(second: "k"));

        Assert.Equal("colors[1].key", error.Field);
        Assert.Contains("'k'", error.Message);
        Assert.Contains("index 0", error.Message);
        Assert.Contains("F-20", error.Message);
    }

    [Fact]
    public void AnIndexOutOfOrderFails()
    {
        ContentException error = Fails(Body().Replace("\"index\": 1", "\"index\": 7"));

        Assert.Equal("colors[1].index", error.Field);
        Assert.Contains("color 1 of the file", error.Message);
    }

    [Fact]
    public void AKeyOfMoreThanOneCharacterFails()
    {
        ContentException error = Fails(Body(second: "ww"));

        Assert.Equal("colors[1].key", error.Field);
        Assert.Contains("D-515", error.Message);
    }

    [Fact]
    public void TheDotAsAKeyFails()
    {
        ContentException error = Fails(Body(second: "."));

        Assert.Equal("colors[1].key", error.Field);
        Assert.Contains("transparent", error.Message);
    }

    /// <summary>JSON writes a backslash as two characters, so no key is one (D-515).</summary>
    [Fact]
    public void ABackslashAsAKeyFails()
    {
        ContentException error = Fails(Body(second: "\\\\"));

        Assert.Equal("colors[1].key", error.Field);
        Assert.Contains("two characters", error.Message);
    }

    [Fact]
    public void AColorThatIsNotSixLowercaseDigitsFails()
    {
        ContentException error = Fails(Body().Replace("\"f2eeea\"", "\"F2EEEA\""));

        Assert.Equal("colors[1].hex", error.Field);
        Assert.Contains("lowercase", error.Message);
    }

    [Fact]
    public void ANameThatThePaletteLacksFails()
    {
        Palette palette = Read(Body());

        ContentException error = Assert.Throws<ContentException>(() => palette.ColorNamed("gold"));

        Assert.Equal("gold", error.Field);
        Assert.Contains("no color with this name", error.Message);
    }

    private static Palette Read(string body) => Palette.Read(Encoding.UTF8.GetBytes(body), Palette.Path);

    private static ContentException Fails(string body) =>
        Assert.Throws<ContentException>(() => Read(body));

    private static string Body(string second = "w") =>
        $$"""
        {
         "comment": "a test palette",
         "colors": [
          { "index": 0, "key": "k", "hex": "0b0a0f", "name": "ink" },
          { "index": 1, "key": "{{second}}", "hex": "f2eeea", "name": "chalk" }
         ]
        }
        """;
}
