using System;
using System.Reflection;
using TheThingBelow.Core.Content;
using TheThingBelow.Tools.Content;
using Xunit;

namespace TheThingBelow.Tests;

/// <summary>
/// The text of the UI fits its panel at each body size, and no text falls below the floor of
/// the Steam Deck (D-241, D-459, D-708). The tests read the built Game assembly, because
/// Tests takes no reference to Game (D-614).
/// </summary>
/// <remarks>
/// Terminus is monospace, and the advance of every glyph is half the body size (F-69). The
/// probe of D-621 measured 106 characters across the frame at a body of 24, and 80 at a body
/// of 32, on 2026-09-19 (M-8).
/// </remarks>
public sealed class UiTextLayoutTests
{
    private const string MetricsTypeName = "TheThingBelow.Game.Ui.UiMetrics";
    private const string FitTypeName = "TheThingBelow.Game.Ui.ScreenFit";

    /// <summary>The smallest height in pixels that the rating Verified allows (D-459).</summary>
    private const int DeckTextFloor = 9;

    [Theory]
    [InlineData(24, 106)]
    [InlineData(32, 80)]
    public void TheFrameHoldsTheCountOfCharactersThatTheProbeMeasured(int body, int characters)
    {
        // M-8, from the probe run of 2026-09-19. The frame is 1280 pixels across (D-568).
        Assert.Equal(characters, CharactersAcross(body, FrameWidth()));
    }

    [Fact]
    public void ThePanelHoldsTheLongestStringOfTheTableAtBothBodySizes()
    {
        // Exit tests 4 and 13. Every panel holds its longest string at each body size
        // (D-241, D-708). A panel sits inside the frame with a margin on each side, and the
        // window frame takes a border inside that. PR-62 builds the menu windows that this
        // rule binds, and D-722 removed the demo panel of PR-61.
        ContentSet content = Content();
        int inside = FrameWidth() - (Edge() * 2) - (Border() * 2);

        foreach (string id in content.Strings.Ids)
        {
            int length = content.Strings.Text(ContentId.Parse(id, StringTable.Path, "id")).Length;
            foreach (int body in new[] { content.Style.SmallBody, content.Style.LargeBody })
            {
                Assert.True(
                    WidthOf(body, length) <= inside,
                    $"The string '{id}' is {length} characters. At a body of {body} it needs "
                    + $"{WidthOf(body, length)} pixels, and the panel holds {inside} (D-241, D-708).");
            }
        }
    }

    [Fact]
    public void TheTitleOfTheGameHoldsThePanelAtBothBodySizes()
    {
        // D-708. The title draws at twice the body, so it is the widest line of the panel.
        ContentSet content = Content();
        int inside = FrameWidth() - (Edge() * 2) - (Border() * 2);
        int length = content.Strings.Text(ContentId.Parse("ui.title", StringTable.Path, "id")).Length;

        foreach (int body in new[] { content.Style.SmallBody, content.Style.LargeBody })
        {
            int title = content.Style.TitleSizeOf(body);
            Assert.True(
                WidthOf(title, length) <= inside,
                $"The title is {length} characters. At a title size of {title} it needs "
                + $"{WidthOf(title, length)} pixels, and the panel holds {inside} (D-708).");
        }
    }

    [Fact]
    public void NoTextFallsBelowNinePixelsOnTheDeck()
    {
        // Exit test 8. The Deck shows the frame at 1x, so a body pixel is a device pixel.
        // The rating Verified needs every text at 9 pixels or taller (D-459, D-568).
        ContentSet content = Content();

        Assert.True(
            content.Style.SmallBody >= DeckTextFloor,
            $"The small body is {content.Style.SmallBody} pixels, and the Deck floor is {DeckTextFloor} (D-459).");
        Assert.True(content.Style.LargeBody >= DeckTextFloor);
    }

    [Fact]
    public void TheAdvanceOfAGlyphIsHalfTheBody()
    {
        // F-69. Terminus is monospace, and its advance is half its height.
        Assert.Equal(12, AdvanceOf(24));
        Assert.Equal(16, AdvanceOf(32));
    }

    [Fact]
    public void ABodySizeOfZeroFails()
    {
        // T-2. An absent size is an error, and never a width of zero.
        TargetInvocationException thrown = Assert.Throws<TargetInvocationException>(() => AdvanceOf(0));

        Assert.IsType<ArgumentOutOfRangeException>(thrown.InnerException);
    }

    private static ContentSet Content() => ContentSet.Load(ContentFolder.Read(RepositoryRoot.Find()));

    private static int FrameWidth() =>
        (int)GameAssemblyFile.Type(FitTypeName).GetField("FrameWidth")!.GetValue(null)!;

    private static int Edge() =>
        (int)GameAssemblyFile.Type(MetricsTypeName).GetField("EdgePixels")!.GetValue(null)!;

    private static int Border() =>
        (int)GameAssemblyFile.Type("TheThingBelow.Game.Ui.UiTheme").GetField("FrameEdge")!.GetValue(null)!;

    private static int AdvanceOf(int body) => Call("AdvanceOf", body);

    private static int WidthOf(int body, int characters) => Call("WidthOf", body, characters);

    private static int CharactersAcross(int body, int width) => Call("CharactersAcross", body, width);

    private static int Call(string name, params object[] values) =>
        (int)GameAssemblyFile.Type(MetricsTypeName).GetMethod(name)!.Invoke(null, values)!;
}
