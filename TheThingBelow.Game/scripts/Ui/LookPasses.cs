using System;
using Godot;
using TheThingBelow.Core.Content;
using TheThingBelow.Core.Light;

namespace TheThingBelow.Game.Ui;

/// <summary>
/// The tilt-shift blur and the vignette of the HD-2D look, and the mode that they share with the
/// light shafts (D-849, D-917, D-919, D-920).
/// </summary>
/// <remarks>
/// The frame draws the view of the scene with the blur, and the vignette over it, under the marks
/// and the UI. So the blur and the dark reach the world, the glow, the fog, the hit bursts, and
/// the light shafts, and no mark and no menu (D-208, D-210). Until a screen shows the passes of
/// the file, the blur and the vignette draw nothing, so a screen with no world stays as it was.
/// </remarks>
public static class LookPasses
{
    /// <summary>The path of the shader of the tilt-shift blur (D-825).</summary>
    public const string BlurShaderPath = "res://shaders/tilt_shift.gdshader";

    /// <summary>The path of the shader of the vignette (D-825).</summary>
    public const string VignetteShaderPath = "res://shaders/vignette.gdshader";

    /// <summary>The name of the uniform of the mode: true for the stepped mode (D-917).</summary>
    public const string SteppedName = "stepped";

    /// <summary>The name of the uniform of the count of steps of the stepped mode.</summary>
    public const string StepsName = "steps";

    /// <summary>The name of the uniform of the block size of the stepped mode.</summary>
    public const string CellSizeName = "cell_size";

    /// <summary>The name of the uniform of the height of each band of the blur.</summary>
    public const string BandName = "band";

    /// <summary>The name of the uniform of the radius of the blur at the edge of the view.</summary>
    public const string RadiusName = "radius";

    /// <summary>The name of the uniform of the palette color of the vignette.</summary>
    public const string ColorName = "color";

    /// <summary>The name of the uniform of the strength of the vignette at the corners.</summary>
    public const string StrengthName = "strength";

    /// <summary>The name of the uniform of the start of the dark of the vignette.</summary>
    public const string StartName = "start";

    /// <summary>Builds the material of the view of the scene, with no blur until <see cref="Show"/>.</summary>
    /// <returns>The material.</returns>
    /// <exception cref="InvalidOperationException">Godot loaded no shader (T-2, D-825).</exception>
    public static ShaderMaterial BlurMaterial() => new() { Shader = LoadShader(BlurShaderPath) };

    /// <summary>Builds the material of the vignette, with no dark until <see cref="Show"/>.</summary>
    /// <returns>The material.</returns>
    /// <exception cref="InvalidOperationException">Godot loaded no shader (T-2, D-825).</exception>
    public static ShaderMaterial VignetteMaterial() => new() { Shader = LoadShader(VignetteShaderPath) };

    /// <summary>Gives the blur and the vignette the values of the passes.</summary>
    /// <param name="blur">The material of <see cref="BlurMaterial"/>.</param>
    /// <param name="vignette">The material of <see cref="VignetteMaterial"/>.</param>
    /// <param name="passes">The passes of the file, in the mode that the screen shows.</param>
    /// <param name="palette">The palette, which gives the key of the vignette its color (D-181).</param>
    /// <exception cref="ArgumentNullException">An argument is null (T-2).</exception>
    /// <exception cref="ContentException">The palette holds no key of the vignette (T-2).</exception>
    public static void Show(ShaderMaterial blur, ShaderMaterial vignette, Hd2dPasses passes, Palette palette)
    {
        ArgumentNullException.ThrowIfNull(blur);
        ArgumentNullException.ThrowIfNull(vignette);
        ArgumentNullException.ThrowIfNull(passes);
        ArgumentNullException.ThrowIfNull(palette);

        SetMode(blur, passes);
        blur.SetShaderParameter(BandName, passes.BlurBand);
        blur.SetShaderParameter(RadiusName, passes.BlurRadius);

        SetMode(vignette, passes);
        vignette.SetShaderParameter(ColorName, ColorOf(palette, passes.VignetteKey, Hd2dPasses.Path));
        vignette.SetShaderParameter(StrengthName, passes.VignetteStrength);
        vignette.SetShaderParameter(StartName, passes.VignetteStart);
    }

    /// <summary>Gives a shader of the three passes the mode of the file, with its steps and its block size (D-917).</summary>
    /// <param name="material">The material of a shader that includes `look_steps.gdshaderinc`.</param>
    /// <param name="passes">The passes of the file, in the mode that the screen shows.</param>
    /// <exception cref="ArgumentNullException">An argument is null (T-2).</exception>
    public static void SetMode(ShaderMaterial material, Hd2dPasses passes)
    {
        ArgumentNullException.ThrowIfNull(material);
        ArgumentNullException.ThrowIfNull(passes);

        material.SetShaderParameter(SteppedName, passes.Mode == PassMode.Stepped);
        material.SetShaderParameter(StepsName, passes.Steps);
        material.SetShaderParameter(CellSizeName, passes.CellSize);
    }

    /// <summary>Gives the color of one palette key.</summary>
    /// <param name="palette">The palette.</param>
    /// <param name="key">The key.</param>
    /// <param name="file">The file that names the key, for the error (T-2).</param>
    /// <returns>The color, as Godot takes a color with `source_color`.</returns>
    /// <exception cref="ContentException">The palette holds no such key (T-2).</exception>
    public static Color ColorOf(Palette palette, char key, string file)
    {
        ArgumentNullException.ThrowIfNull(palette);

        if (!palette.TryColorOf(key, out PaletteColor? found))
        {
            throw ContentException.ForField(Palette.Path, "colors", $"the palette holds no key '{key}' of the file '{file}' (D-181)");
        }

        return Color.Color8((byte)found.Red, (byte)found.Green, (byte)found.Blue);
    }

    private static Shader LoadShader(string path)
    {
        // The load reports a failure in the log alone, so the result takes a check (T-2).
        Shader? loaded = ResourceLoader.Load<Shader>(path);
        return loaded ?? throw new InvalidOperationException($"Godot loaded no shader from '{path}' (D-825, T-2).");
    }
}
