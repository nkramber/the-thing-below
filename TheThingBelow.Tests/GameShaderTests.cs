using System;
using System.Collections.Generic;
using System.IO;
using System.Text.RegularExpressions;
using TheThingBelow.Core.Effects;
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
        var failures = new List<string>();
        foreach (string file in ShaderFiles())
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

    [Fact]
    public void TheGlowPassNamesABlurShaderAndAnAddShaderThatHoldEachUniform()
    {
        // D-913: the blur reads the exact mean of each cell of the mask, and the pass adds the blur
        // over the world. D-914: the pass cuts the glow into steps over blocks when the file asks.
        Type pass = GameAssemblyFile.Type("TheThingBelow.Game.Ui.GlowPass");
        Assert.Equal("res://shaders/glow_blur.gdshader", (string)pass.GetField("BlurShaderPath")!.GetValue(null)!);
        Assert.Equal("res://shaders/glow_add.gdshader", (string)pass.GetField("AddShaderPath")!.GetValue(null)!);

        string root = Path.Combine(RepositoryRoot.Find(), ShaderFolder);
        string blur = CodeOf(File.ReadAllText(Path.Combine(root, "glow_blur.gdshader")));
        string add = CodeOf(File.ReadAllText(Path.Combine(root, "glow_add.gdshader")));

        // Each cell of the blur is 4 art pixels, the quarter view of the pass.
        Assert.Contains($"const float CELL = {(int)pass.GetField("BlurCell")!.GetValue(null)!}.0;", blur, StringComparison.Ordinal);
        Assert.Contains("render_mode blend_add, unshaded;", add, StringComparison.Ordinal);
        Assert.Contains("floor(level * float(steps)) / float(steps)", add, StringComparison.Ordinal);

        // Game sets each uniform by a name constant, and the file holds each name (D-825).
        foreach (string field in new[] { "IntensityName", "StepsName", "CellSizeName" })
        {
            string name = (string)pass.GetField(field)!.GetValue(null)!;
            Assert.Matches($@"uniform \w+ {name}\b", add);
        }
    }

    [Fact]
    public void NoShaderOfTheGameReadsTheClockOfGodot()
    {
        // F-100, D-172: a shader that reads TIME draws another picture at each capture of one
        // tick. Game gives each shader the tick, or a value of the tick, instead.
        var failures = new List<string>();
        foreach (string file in ShaderFiles())
        {
            if (Regex.IsMatch(CodeOf(File.ReadAllText(file)), @"\bTIME\b"))
            {
                failures.Add(Path.GetFileName(file));
            }
        }

        Assert.True(failures.Count == 0, $"these shaders read TIME: {string.Join(", ", failures)} (F-100)");
    }

    [Fact]
    public void TheFogPassNamesAnUnlitShaderAndALitShaderThatHoldOneFog()
    {
        // D-183, D-897: an unlit fog gives its own light, and a lit fog takes the scene light.
        // The two shaders include one file of the fog, so the two fogs draw the same shapes.
        Type pass = GameAssemblyFile.Type("TheThingBelow.Game.Ui.FogPass");
        Assert.Equal("res://shaders/fog.gdshader", (string)pass.GetField("ShaderPath")!.GetValue(null)!);
        Assert.Equal("res://shaders/fog_lit.gdshader", (string)pass.GetField("LitShaderPath")!.GetValue(null)!);

        string root = Path.Combine(RepositoryRoot.Find(), ShaderFolder);
        string unlit = CodeOf(File.ReadAllText(Path.Combine(root, "fog.gdshader")));
        string lit = CodeOf(File.ReadAllText(Path.Combine(root, "fog_lit.gdshader")));
        string include = "#include \"res://shaders/fog_noise.gdshaderinc\"";
        Assert.Contains("render_mode unshaded;", unlit, StringComparison.Ordinal);
        Assert.DoesNotContain("render_mode", lit, StringComparison.Ordinal);
        Assert.Contains(include, unlit, StringComparison.Ordinal);
        Assert.Contains(include, lit, StringComparison.Ordinal);
    }

    [Fact]
    public void TheFogShaderHoldsEachUniformOfTheFogPassAndTheMostLayers()
    {
        // D-825: Game sets each uniform by a name constant, and the file holds each name. The
        // arrays of the shader hold the most layers that the reader takes (D-898).
        string code = CodeOf(File.ReadAllText(Path.Combine(RepositoryRoot.Find(), ShaderFolder, "fog_noise.gdshaderinc")));
        Type pass = GameAssemblyFile.Type("TheThingBelow.Game.Ui.FogPass");
        string[] fields =
        [
            "LayerCountName", "ColorsName", "OriginXName", "OriginYName", "ScalesName",
            "SeedsName", "FadeFromName", "FadeToName", "StrengthsName", "StepsName", "CellSizesName",
        ];
        foreach (string field in fields)
        {
            string name = (string)pass.GetField(field)!.GetValue(null)!;
            Assert.Matches($@"uniform \w+ {name}\b", code);
        }

        Assert.Contains($"const int MOST_LAYERS = {FogLayer.MostLayers};", code, StringComparison.Ordinal);
    }

    /// <summary>Gives each shader file and each include file of the Game project.</summary>
    private static List<string> ShaderFiles()
    {
        string folder = Path.Combine(RepositoryRoot.Find(), ShaderFolder);
        var files = new List<string>(Directory.GetFiles(folder, "*.gdshader"));
        files.AddRange(Directory.GetFiles(folder, "*.gdshaderinc"));
        files.Sort(StringComparer.Ordinal);
        Assert.NotEmpty(files);
        return files;
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
