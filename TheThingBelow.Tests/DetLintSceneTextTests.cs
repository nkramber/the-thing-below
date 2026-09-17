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
}
