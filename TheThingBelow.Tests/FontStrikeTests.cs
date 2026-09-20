using System;
using System.Text;
using TheThingBelow.Core.Content;
using TheThingBelow.Tools.Content;
using Xunit;

namespace TheThingBelow.Tests;

/// <summary>
/// The bitmap strikes of the two fonts, and the strike that each size of the UI draws
/// (D-263, D-264, D-710). A size with no strike falls back to the traced outline in silence,
/// and the glyph loses its square pixel (F-49).
/// </summary>
/// <remarks>
/// The strikes of Terminus TTF 4.49.3 are 12, 14, 16, 18, 20, 22, 24, 28, and 32 pixels. A
/// read of both files on 2026-09-19 confirmed the list, and this test pins it (D-710).
/// </remarks>
public sealed class FontStrikeTests
{
    private const string FontsTypeName = "TheThingBelow.Game.Ui.GameFonts";

    private static readonly int[] TerminusStrikes = [12, 14, 16, 18, 20, 22, 24, 28, 32];

    [Theory]
    [InlineData(FontStrikes.BodyPath)]
    [InlineData(FontStrikes.TitlePath)]
    public void EachFontOfTheCheckoutCarriesTheStrikesOfTerminus(string path)
    {
        // D-710. The list comes from the two files of the checkout, and never from a copy.
        FontStrikes strikes = Content().FontOf(path);

        Assert.Equal(TerminusStrikes, strikes.Sizes);
        Assert.Equal(32, strikes.LargestSize);
    }

    [Theory]
    [InlineData(24, 24, 1)]
    [InlineData(32, 32, 1)]
    [InlineData(48, 24, 2)]
    [InlineData(64, 32, 2)]
    public void EachSizeOfTheUiDrawsAWholeMultipleOfAStrike(int wanted, int strike, int scale)
    {
        // D-707, D-710. A title of 48 doubles the 24 bitmap, and a title of 64 doubles 32.
        // The largest bitmap is 32, so every size above it doubles or triples a smaller one.
        (int gave, int times) = StrikeFor(Content().BodyFont, wanted);

        Assert.Equal(strike, gave);
        Assert.Equal(scale, times);
    }

    [Fact]
    public void ASizeWithNoWholeMultipleOfAStrikeFails()
    {
        // F-49. The fallback to the traced outline is silent, so the code refuses the size.
        ContentException error = Assert.Throws<ContentException>(() => StrikeFor(Content().BodyFont, 23));

        Assert.Equal(FontStrikes.BodyPath, error.File);
        Assert.Contains("F-49", error.Message, StringComparison.Ordinal);
        Assert.Contains("12, 14, 16, 18, 20, 22, 24, 28, 32", error.Message, StringComparison.Ordinal);
    }

    [Fact]
    public void ASizeWithNoStrikeFails()
    {
        // T-2. The message names the size and every size that the file carries.
        ContentException error = Assert.Throws<ContentException>(() => Content().BodyFont.RequireSize(26));

        Assert.Contains("26 pixels", error.Message, StringComparison.Ordinal);
        Assert.Contains("D-710", error.Message, StringComparison.Ordinal);
    }

    [Fact]
    public void AFileWithNoBitmapTableFails()
    {
        // D-263. Every font of this game is a pixel font with its own bitmaps.
        byte[] bytes = UiContentFixtures.FontBytes(16);
        Encoding.ASCII.GetBytes("XXXX").CopyTo(bytes, 12);

        ContentException error = Assert.Throws<ContentException>(
            () => FontStrikes.Read(bytes, FontStrikes.BodyPath));

        Assert.Equal(FontStrikes.BodyPath, error.File);
        Assert.Contains("EBLC", error.Message, StringComparison.Ordinal);
        Assert.Contains("F-49", error.Message, StringComparison.Ordinal);
    }

    [Fact]
    public void AStrikeThatIsNotSquareFails()
    {
        // D-230. A square pixel needs the two sides of a strike equal.
        // The last four bytes of the record are the two sides, the bit depth, and the flags.
        byte[] bytes = UiContentFixtures.FontBytes(16);
        bytes[^3] = 20;

        ContentException error = Assert.Throws<ContentException>(
            () => FontStrikes.Read(bytes, FontStrikes.BodyPath));

        Assert.Contains("square pixel", error.Message, StringComparison.Ordinal);
    }

    [Fact]
    public void AStrikeOfMoreThanOneBitFails()
    {
        // D-263. A pixel font of this game is one bit deep, so no glyph carries a gray edge.
        byte[] bytes = UiContentFixtures.FontBytes(16);
        bytes[^2] = 8;

        ContentException error = Assert.Throws<ContentException>(
            () => FontStrikes.Read(bytes, FontStrikes.BodyPath));

        Assert.Contains("8 bits deep", error.Message, StringComparison.Ordinal);
    }

    [Fact]
    public void ACountOfStrikesBeyondTheFileFails()
    {
        // T-2. A count from a file never asks for the memory of that many records.
        byte[] bytes = UiContentFixtures.FontBytes(16);
        bytes[12 + 16 + 4] = 0x7f;

        ContentException error = Assert.Throws<ContentException>(
            () => FontStrikes.Read(bytes, FontStrikes.BodyPath));

        Assert.Contains("bytes", error.Message, StringComparison.Ordinal);
    }

    [Fact]
    public void BothBodySizesHaveTheirOwnStrikeInBothFonts()
    {
        // D-707. Each body size draws its own strike at 1x, so a glyph pixel is one frame
        // pixel. The content set refuses a build where one of them is absent.
        ContentSet content = Content();

        foreach (int body in new[] { content.Style.SmallBody, content.Style.LargeBody })
        {
            Assert.True(content.BodyFont.Has(body), $"The body font carries no bitmap of {body} pixels (D-707).");
            Assert.True(content.TitleFont.Has(body), $"The title font carries no bitmap of {body} pixels (D-707).");
        }
    }

    private static ContentSet Content() => ContentSet.Load(ContentFolder.Read(RepositoryRoot.Find()));

    private static (int Strike, int Scale) StrikeFor(FontStrikes strikes, int wanted)
    {
        try
        {
            object pair = GameAssemblyFile.Type(FontsTypeName)
                .GetMethod("StrikeFor")!
                .Invoke(null, [strikes, wanted])!;

            return ((int)pair.GetType().GetField("Item1")!.GetValue(pair)!,
                (int)pair.GetType().GetField("Item2")!.GetValue(pair)!);
        }
        catch (System.Reflection.TargetInvocationException thrown) when (thrown.InnerException is not null)
        {
            // The test reads the error of the method, and not the wrapper of reflection (T-2).
            throw thrown.InnerException;
        }
    }
}
