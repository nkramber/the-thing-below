using System;
using System.Collections.Generic;
using TheThingBelow.Tools.DetLint;
using Xunit;

namespace TheThingBelow.Tests;

/// <summary>
/// DL 8, the text rule of Game (D-499, D-614). The fixtures compile against the Godot assembly
/// of the Game build output, so the rule reads the Godot types and not the words (D-498).
/// </summary>
public sealed class DetLintGodotTextTests
{
    [Fact]
    public void ATextPropertyOutsideTheTextHelperFails()
    {
        IReadOnlyList<LintFinding> findings = DetLintFixture.CheckGame(
            """
            using Godot;
            namespace TheThingBelow.Game;
            public static class Fixture
            {
                public static void Draw(Label label)
                {
                    label.Text = "Enter the mine.";
                }
            }
            """);

        LintFinding finding = Assert.Single(findings);
        Assert.Equal("DL 8", finding.Rule);
        Assert.Equal(DetLintFixture.GamePath, finding.File);
        Assert.Equal(7, finding.Line);
        Assert.Contains("Godot.Label.Text", finding.Detail, StringComparison.Ordinal);
    }

    [Fact]
    public void ATitlePropertyOutsideTheTextHelperFails()
    {
        IReadOnlyList<LintFinding> findings = DetLintFixture.CheckGame(
            """
            using Godot;
            namespace TheThingBelow.Game;
            public static class Fixture
            {
                public static void Name(Window window)
                {
                    window.Title = "The Thing Below";
                }
            }
            """);

        Assert.Equal(["DL 8"], DetLintFixture.RuleIds(findings));
    }

    [Fact]
    public void ADrawCallOfTheCommittedListFails()
    {
        IReadOnlyList<LintFinding> findings = DetLintFixture.CheckGame(
            """
            using Godot;
            namespace TheThingBelow.Game;
            public partial class Fixture : Node2D
            {
                public void Show(Font font)
                {
                    DrawString(font, Vector2.Zero, "Enter the mine.");
                }
            }
            """);

        Assert.Equal(["DL 8"], DetLintFixture.RuleIds(findings));
        Assert.Contains("DrawString", findings[0].Detail, StringComparison.Ordinal);
    }

    /// <summary>
    /// The regression test of finding P3-24. The remap screen showed the English engine name of
    /// each key, and DL 8 read no word `Text` in the name of the call (D-1128).
    /// </summary>
    [Fact]
    public void TheEngineNameOfAKeyFails()
    {
        IReadOnlyList<LintFinding> findings = DetLintFixture.CheckGame(
            """
            using Godot;
            namespace TheThingBelow.Game;
            public static class Fixture
            {
                public static string NameOf(Key key) => OS.GetKeycodeString(key);
            }
            """);

        Assert.Equal(["DL 8"], DetLintFixture.RuleIds(findings));
        Assert.Contains("GetKeycodeString", findings[0].Detail, StringComparison.Ordinal);
    }

    /// <summary>A call that sets a property by its name draws text without a member of the words (D-614).</summary>
    [Fact]
    public void ASetCallWithATextNameFails()
    {
        IReadOnlyList<LintFinding> findings = DetLintFixture.CheckGame(
            """
            using Godot;
            namespace TheThingBelow.Game;
            public static class Fixture
            {
                public static void Draw(Label label)
                {
                    label.Set("text", "Enter the mine.");
                }
            }
            """);

        Assert.Equal(["DL 8"], DetLintFixture.RuleIds(findings));
        Assert.Contains("Set", findings[0].Detail, StringComparison.Ordinal);
    }

    [Fact]
    public void ASetCallWithAnotherNamePasses()
    {
        IReadOnlyList<LintFinding> findings = DetLintFixture.CheckGame(
            """
            using Godot;
            namespace TheThingBelow.Game;
            public static class Fixture
            {
                public static void Move(Node2D node)
                {
                    node.Set("position", Vector2.Zero);
                }
            }
            """);

        Assert.Empty(findings);
    }

    [Fact]
    public void AnItemCallOfAMenuFails()
    {
        IReadOnlyList<LintFinding> findings = DetLintFixture.CheckGame(
            """
            using Godot;
            namespace TheThingBelow.Game;
            public static class Fixture
            {
                public static void Fill(PopupMenu menu)
                {
                    menu.AddItem("Attack");
                }
            }
            """);

        Assert.Equal(["DL 8"], DetLintFixture.RuleIds(findings));
        Assert.Contains("AddItem", findings[0].Detail, StringComparison.Ordinal);
    }

    [Fact]
    public void AMemberThatHoldsNoTextWordPasses()
    {
        // `Texture` holds the letters of `Text` and not the word, so the rule reads it as a
        // longer word and gives no finding (D-614).
        IReadOnlyList<LintFinding> findings = DetLintFixture.CheckGame(
            """
            using Godot;
            namespace TheThingBelow.Game;
            public static class Fixture
            {
                public static void Draw(Sprite2D sprite, Texture2D art)
                {
                    sprite.Texture = art;
                    sprite.TextureFilter = CanvasItem.TextureFilterEnum.Nearest;
                }
            }
            """);

        Assert.Empty(findings);
    }

    [Fact]
    public void ATextPropertyInsideTheTextHelperPasses()
    {
        IReadOnlyList<LintFinding> findings = DetLintFixture.CheckGame(
            $$"""
            using Godot;
            namespace {{GodotTextRule.TextHelperType[..GodotTextRule.TextHelperType.LastIndexOf('.')]}};
            public static class TextHelper
            {
                public static void Draw(Label label, string fromTheStringTable)
                {
                    label.Text = fromTheStringTable;
                }
            }
            """);

        Assert.Empty(findings);
    }

    [Theory]
    [InlineData("Text", true)]
    [InlineData("TooltipText", true)]
    [InlineData("Title", true)]
    [InlineData("DrawString", true)]
    [InlineData("GetKeycodeString", true)]
    [InlineData("Texture", false)]
    [InlineData("TextureFilter", false)]
    [InlineData("Titles", false)]
    public void TheNameRuleReadsAWordAndNotTheLetters(string name, bool drawsText)
    {
        Assert.Equal(drawsText, GodotTextRule.DrawsText(name));
    }
}
