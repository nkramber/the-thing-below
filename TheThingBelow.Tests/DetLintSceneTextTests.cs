using System;
using System.Collections.Generic;
using TheThingBelow.Tools.DetLint;
using Xunit;

namespace TheThingBelow.Tests;

/// <summary>DL 9: no text value in a Godot scene file (D-499, G-6, G-7).</summary>
public sealed class DetLintSceneTextTests
{
    [Fact]
    public void ATextValueInASceneFileFails()
    {
        IReadOnlyList<LintFinding> findings = SceneTextRule.Check(
            "TheThingBelow.Game/Fixture.tscn",
            [
                "[gd_scene format=3]",
                string.Empty,
                "[node name=\"Title\" type=\"Label\"]",
                "text = \"Enter the mine.\"",
            ]);

        LintFinding finding = Assert.Single(findings);
        Assert.Equal("DL 9", finding.Rule);
        Assert.Equal(4, finding.Line);
        Assert.Contains("Enter the mine.", finding.Detail, StringComparison.Ordinal);
    }

    [Fact]
    public void ATextValueWithAnEscapedQuoteFails()
    {
        // Godot writes a quote of a string value as `\"`, and the rule reads the whole value
        // to its closing quote (P2-1 of `docs/reviews/pr-23.md`).
        IReadOnlyList<LintFinding> findings = SceneTextRule.Check(
            "TheThingBelow.Game/Fixture.tscn",
            [
                "[node name=\"Title\" type=\"Label\"]",
                "text = \"Say \\\"hello\\\"\"",
            ]);

        LintFinding finding = Assert.Single(findings);
        Assert.Equal("DL 9", finding.Rule);
        Assert.Equal(2, finding.Line);
    }

    /// <summary>Godot writes the text of a menu item under a slash, and the rule reads the last part.</summary>
    [Theory]
    [InlineData("item_0/text = \"Attack\"")]
    [InlineData("popup/item_0/text = \"Attack\"")]
    public void ATextValueOfAnItemFails(string line)
    {
        IReadOnlyList<LintFinding> findings = SceneTextRule.Check(
            "TheThingBelow.Game/Fixture.tscn",
            ["[node name=\"Menu\" type=\"PopupMenu\"]", line]);

        LintFinding finding = Assert.Single(findings);
        Assert.Equal("DL 9", finding.Rule);
        Assert.Equal(2, finding.Line);
    }

    [Fact]
    public void ALayoutValueInASceneFilePasses()
    {
        IReadOnlyList<LintFinding> findings = SceneTextRule.Check(
            "TheThingBelow.Game/Fixture.tscn",
            [
                "[gd_scene load_steps=2 format=3]",
                string.Empty,
                "[ext_resource type=\"Script\" path=\"res://scripts/Boot.cs\" id=\"1_boot\"]",
                string.Empty,
                "[node name=\"Boot\" type=\"Node\"]",
                "script = ExtResource(\"1_boot\")",
                "text = \"\"",
            ]);

        Assert.Empty(findings);
    }

    [Theory]
    [InlineData("text", true)]
    [InlineData("title", true)]
    [InlineData("dialog_text", true)]
    [InlineData("window_title", true)]
    [InlineData("texture", false)]
    [InlineData("name", false)]
    public void TheNameRuleReadsATextProperty(string name, bool holdsText)
    {
        Assert.Equal(holdsText, SceneTextRule.IsTextProperty(name));
    }

    [Fact]
    public void ATextValueAcrossLinesFailsAtTheLineOfItsName()
    {
        // D-1117: Godot writes a line end inside a string value as is, and the rule once read
        // one line alone, so such a value passed.
        IReadOnlyList<LintFinding> findings = SceneTextRule.Check(
            "TheThingBelow.Game/Fixture.tscn",
            [
                "[node name=\"Title\" type=\"Label\"]",
                "text = \"Enter",
                "the mine.\"",
                "texture = \"res://icon.png\"",
            ]);

        LintFinding finding = Assert.Single(findings);
        Assert.Equal(2, finding.Line);
        Assert.Contains("the mine.", finding.Detail, StringComparison.Ordinal);
    }

    [Fact]
    public void ABinarySceneFileFailsWithItsPath()
    {
        LintFinding finding = SceneTextRule.RefuseBinary("TheThingBelow.Game/Screen.scn");

        Assert.Equal(("TheThingBelow.Game/Screen.scn", 1, "DL 9"), (finding.File, finding.Line, finding.Rule));
        Assert.Contains("binary scene or resource", finding.Detail, StringComparison.Ordinal);
    }
}
