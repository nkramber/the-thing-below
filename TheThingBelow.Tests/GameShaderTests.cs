using System;
using System.Collections.Generic;
using System.IO;
using Xunit;

namespace TheThingBelow.Tests;

/// <summary>The rule of every shader file of the Game project on lit art (D-183, D-825).</summary>
public sealed class GameShaderTests
{
    private const string ShaderFolder = "TheThingBelow.Game/shaders";

    [Fact]
    public void NoShaderOfTheGameWritesTheNormalMap()
    {
        // D-183: Godot corrects the normal of a flipped draw before the shader code, and a
        // write of NORMAL_MAP replaces that correction, so a lit sprite takes a wrong light.
        string[] files = Directory.GetFiles(Path.Combine(RepositoryRoot.Find(), ShaderFolder), "*.gdshader");
        Assert.NotEmpty(files);

        var failures = new List<string>();
        foreach (string file in files)
        {
            if (CodeOf(File.ReadAllText(file)).Contains("NORMAL_MAP", StringComparison.Ordinal))
            {
                failures.Add(Path.GetFileName(file));
            }
        }

        Assert.True(failures.Count == 0, $"these shaders write NORMAL_MAP: {string.Join(", ", failures)} (D-183)");
    }

    [Fact]
    public void TheLayerOfTheMotesNamesAShaderThatGivesTheStrengthOfALightAlone()
    {
        // D-893: a mote takes the strength of each light and never its color, so torchlight
        // never paints it yellow. The owner asked for light gray in light and dark gray in dark.
        string path = (string)GameAssemblyFile.Type("TheThingBelow.Game.Ui.MoteLayer")
            .GetField("LightShaderPath")!
            .GetValue(null)!;

        Assert.Equal("res://shaders/mote_light.gdshader", path);

        string file = Path.Combine(RepositoryRoot.Find(), ShaderFolder, "mote_light.gdshader");
        string code = CodeOf(File.ReadAllText(file));
        Assert.Contains("void light()", code, StringComparison.Ordinal);

        // The light adds the step from the dark gray to the light gray, and it never reads the
        // color of the light, so torchlight never paints a mote yellow (D-893).
        Assert.Contains("LIGHT = vec4((light_color.rgb - dark_color.rgb) * strength, COLOR.a);", code, StringComparison.Ordinal);
        Assert.Contains("COLOR.rgb = dark_color.rgb;", code, StringComparison.Ordinal);

        // Game sets each uniform by a name constant, and the file holds each name (D-825).
        foreach (string field in new[] { "DarkColorName", "LightColorName" })
        {
            string name = (string)GameAssemblyFile.Type("TheThingBelow.Game.Ui.MoteLayer").GetField(field)!.GetValue(null)!;
            Assert.Matches($@"uniform \w+ {name}\b", code);
        }
    }

    /// <summary>Removes each line comment, so a comment that names the member passes.</summary>
    private static string CodeOf(string text)
    {
        var lines = new List<string>();
        foreach (string line in text.Split('\n'))
        {
            if (!line.TrimStart().StartsWith("//", StringComparison.Ordinal))
            {
                lines.Add(line);
            }
        }

        return string.Join('\n', lines);
    }
}
