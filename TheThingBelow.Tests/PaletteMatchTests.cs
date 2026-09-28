using System.Text;
using TheThingBelow.Core.Content;
using TheThingBelow.Tools.Import;
using Xunit;

namespace TheThingBelow.Tests;

/// <summary>The exact color and the nearest color of the palette (D-688, D-1314).</summary>
public sealed class PaletteMatchTests
{
    [Fact]
    public void AnExactColorGivesItsKey()
    {
        var match = new PaletteMatch(DrawingFixtures.Palette());

        bool found = match.TryExact(0xc8, 0x35, 0x3a, out PaletteColor? color);

        Assert.True(found);
        Assert.Equal("x", color!.Key);
    }

    [Fact]
    public void AColorOutsideThePaletteHasNoExactMatch()
    {
        var match = new PaletteMatch(DrawingFixtures.Palette());

        Assert.False(match.TryExact(0xc8, 0x35, 0x3b, out _));
    }

    [Fact]
    public void TheNearestColorHasTheSmallestSquaredRgbDistance()
    {
        var match = new PaletteMatch(DrawingFixtures.Palette());

        // Red c0 30 30 lies 8, 5, and 10 units from x, the red of the palette.
        Assert.Equal("x", match.Nearest(0xc0, 0x30, 0x30).Key);

        // A light gray lies nearer to chalk f2eeea than to snow e8f2f7.
        Assert.Equal("w", match.Nearest(0xf0, 0xee, 0xe8).Key);
    }

    /// <summary>A tie goes to the lower index, whatever the key (D-1314).</summary>
    [Fact]
    public void ATieGoesToTheLowerIndex()
    {
        var blackFirst = new PaletteMatch(TwoColors("000000", "0a0000"));
        var redFirst = new PaletteMatch(TwoColors("0a0000", "000000"));

        // 5 0 0 lies 25 squared units from each color.
        Assert.Equal("a", blackFirst.Nearest(5, 0, 0).Key);
        Assert.Equal("a", redFirst.Nearest(5, 0, 0).Key);
        Assert.Equal("0a0000", redFirst.Nearest(5, 0, 0).Hex);
    }

    /// <summary>Two keys of one color make an exact match a guess (T-2).</summary>
    [Fact]
    public void APaletteWithTwoKeysOfOneColorFails()
    {
        ImportException fault = Assert.Throws<ImportException>(() => new PaletteMatch(TwoColors("123456", "123456")));

        Assert.Contains("'a' and 'b' both hold 123456", fault.Message);
        Assert.Contains(Palette.Path, fault.Message);
    }

    private static Palette TwoColors(string first, string second)
    {
        string body = $$"""
            {
             "comment": "two colors",
             "colors": [
              { "index": 0, "key": "a", "hex": "{{first}}", "name": "first", "height": 0 },
              { "index": 1, "key": "b", "hex": "{{second}}", "name": "second", "height": 0 }
             ]
            }
            """;
        return Palette.Read(Encoding.UTF8.GetBytes(body), Palette.Path);
    }
}
