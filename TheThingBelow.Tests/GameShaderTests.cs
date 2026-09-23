using System;
using System.Collections.Generic;
using System.IO;
using System.Text.RegularExpressions;
using TheThingBelow.Core.Effects;
using TheThingBelow.Core.Light;
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
    public void TheViewOfTheWorldNamesAShaderThatTurnsLinearLightIntoSrgb()
    {
        // F-103: the world draws in HDR 2D, and Godot gives its linear light to the frame with no
        // conversion, so the world drew too dark. The shader turns each pixel into sRGB, with a
        // clamp at full white, so the light of a glowing source never wraps (D-910).
        string path = (string)GameAssemblyFile.Type("TheThingBelow.Game.Ui.GlowPass")
            .GetField("ViewShaderPath")!
            .GetValue(null)!;

        Assert.Equal("res://shaders/world_view.gdshader", path);

        string code = CodeOf(File.ReadAllText(Path.Combine(RepositoryRoot.Find(), ShaderFolder, "world_view.gdshader")));
        Assert.Contains("shader_type canvas_item;", code, StringComparison.Ordinal);
        Assert.Contains("COLOR = vec4(srgb_of(clamp(world.rgb, 0.0, 1.0)), world.a);", code, StringComparison.Ordinal);
        Assert.Contains("pow(linear_light, vec3(1.0 / 2.4))", code, StringComparison.Ordinal);
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
    public void TheFogPassNamesAnUnlitShaderThatHoldsTheFog()
    {
        // D-183, D-897: the fog gives its own light. D-916: the fog draws above the glow, where no
        // scene light reaches, so the lit fog left, and the shader includes the one file of the fog.
        Type pass = GameAssemblyFile.Type("TheThingBelow.Game.Ui.FogPass");
        Assert.Equal("res://shaders/fog.gdshader", (string)pass.GetField("ShaderPath")!.GetValue(null)!);
        Assert.Null(pass.GetField("LitShaderPath"));

        string root = Path.Combine(RepositoryRoot.Find(), ShaderFolder);
        string unlit = CodeOf(File.ReadAllText(Path.Combine(root, "fog.gdshader")));
        Assert.Contains("render_mode unshaded;", unlit, StringComparison.Ordinal);
        Assert.Contains("#include \"res://shaders/fog_noise.gdshaderinc\"", unlit, StringComparison.Ordinal);
        Assert.False(File.Exists(Path.Combine(root, "fog_lit.gdshader")), "the lit fog shader left with D-916");
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

    [Fact]
    public void EachPassOfTheLookNamesItsShaderAndTheFileOfTheModes()
    {
        // D-917: the tilt-shift blur, the vignette, and the light shafts share the smooth mode and
        // the stepped mode, so each shader includes the one file of the modes.
        Type look = GameAssemblyFile.Type("TheThingBelow.Game.Ui.LookPasses");
        Type shafts = GameAssemblyFile.Type("TheThingBelow.Game.Ui.ShaftPass");
        Assert.Equal("res://shaders/tilt_shift.gdshader", (string)look.GetField("BlurShaderPath")!.GetValue(null)!);
        Assert.Equal("res://shaders/vignette.gdshader", (string)look.GetField("VignetteShaderPath")!.GetValue(null)!);
        Assert.Equal("res://shaders/light_shafts.gdshader", (string)shafts.GetField("ShaderPath")!.GetValue(null)!);

        string root = Path.Combine(RepositoryRoot.Find(), ShaderFolder);
        foreach (string file in new[] { "tilt_shift.gdshader", "vignette.gdshader", "light_shafts.gdshader" })
        {
            string code = CodeOf(File.ReadAllText(Path.Combine(root, file)));
            Assert.Contains("#include \"res://shaders/look_steps.gdshaderinc\"", code, StringComparison.Ordinal);
            Assert.Contains("render_mode unshaded", code, StringComparison.Ordinal);
        }

        string steps = CodeOf(File.ReadAllText(Path.Combine(root, "look_steps.gdshaderinc")));
        foreach (string field in new[] { "SteppedName", "StepsName", "CellSizeName" })
        {
            string name = (string)look.GetField(field)!.GetValue(null)!;
            Assert.Matches($@"uniform \w+ {name}\b", steps);
        }
    }

    [Fact]
    public void TheBlurAndTheVignetteHoldEachUniformOfTheirPasses()
    {
        // D-825: Game sets each uniform by a name constant, and the file holds each name.
        Type look = GameAssemblyFile.Type("TheThingBelow.Game.Ui.LookPasses");
        string root = Path.Combine(RepositoryRoot.Find(), ShaderFolder);
        string blur = CodeOf(File.ReadAllText(Path.Combine(root, "tilt_shift.gdshader")));
        string vignette = CodeOf(File.ReadAllText(Path.Combine(root, "vignette.gdshader")));

        foreach (string field in new[] { "BandName", "RadiusName" })
        {
            Assert.Matches($@"uniform \w+ {(string)look.GetField(field)!.GetValue(null)!}\b", blur);
        }

        foreach (string field in new[] { "ColorName", "StrengthName", "StartName" })
        {
            Assert.Matches($@"uniform \w+ {(string)look.GetField(field)!.GetValue(null)!}\b", vignette);
        }

        // A sharp pixel reads the middle of its own art pixel, so the middle of the view keeps
        // each pixel of the art under the linear filter of the view (D-919).
        Assert.Contains("COLOR = texture(TEXTURE, (art + 0.5) / SCENE);", blur, StringComparison.Ordinal);
    }

    [Fact]
    public void TheShaftShaderAddsLightAndHoldsTheMostShaftsOfAMap()
    {
        // D-918: the arrays of the shader hold the most shafts that the reader takes. The shader
        // writes its light with an alpha of 0 and the blend of premultiplied alpha, so a beam adds
        // light and dims nothing under it (D-919).
        Type pass = GameAssemblyFile.Type("TheThingBelow.Game.Ui.ShaftPass");
        string code = CodeOf(File.ReadAllText(Path.Combine(RepositoryRoot.Find(), ShaderFolder, "light_shafts.gdshader")));
        string[] fields =
        [
            "ShaftCountName", "OriginXName", "OriginYName", "TopXName", "TopYName",
            "SlantsName", "LengthsName", "WidthsName", "ColorsName", "StrengthsName",
        ];
        foreach (string field in fields)
        {
            Assert.Matches($@"uniform \w+ {(string)pass.GetField(field)!.GetValue(null)!}\b", code);
        }

        Assert.Contains($"const int MOST_SHAFTS = {ShaftKind.MostShaftsOnMap};", code, StringComparison.Ordinal);
        Assert.Contains("render_mode unshaded, blend_premul_alpha;", code, StringComparison.Ordinal);
        Assert.Contains("COLOR = vec4(light, 0.0);", code, StringComparison.Ordinal);
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
