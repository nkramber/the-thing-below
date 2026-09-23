using System;
using Godot;
using TheThingBelow.Core;
using TheThingBelow.Core.Light;

namespace TheThingBelow.Game.Ui;

/// <summary>
/// The glow of the world view: the environment of the world, the view of the world in the frame,
/// and the modulate of each source that glows (D-188, D-910, D-911, D-912).
/// </summary>
/// <remarks>
/// The world draws in HDR 2D, so a pixel keeps linear light above full white. The glow of Godot
/// reads each pixel whose brightest channel passes the threshold of the glow file, and Core keeps
/// every lit sprite and tile below that threshold (F-47). A source that glows draws its palette
/// color times its glow, above the threshold.
/// <para>
/// The glow is a smooth bloom, and its colors can leave the palette (D-911). The UI draws in the
/// frame view, outside the world, so it never glows (D-210).
/// </para>
/// </remarks>
public static class GlowPass
{
    /// <summary>The path of the shader that turns the linear light of the world into sRGB in the frame (F-103, D-825).</summary>
    public const string ViewShaderPath = "res://shaders/world_view.gdshader";

    /// <summary>Builds the environment of the world view, with the glow of the file.</summary>
    /// <param name="glow">The glow file.</param>
    /// <returns>The environment.</returns>
    /// <exception cref="ArgumentNullException">The glow is null (T-2).</exception>
    public static Godot.Environment EnvironmentOf(Glow glow)
    {
        ArgumentNullException.ThrowIfNull(glow);

        var environment = new Godot.Environment
        {
            // The canvas background puts the 2D world under the glow of the environment.
            BackgroundMode = Godot.Environment.BGMode.Canvas,
            TonemapMode = Godot.Environment.ToneMapper.Linear,
            GlowEnabled = true,
            GlowHdrThreshold = PartOf(glow.Threshold),
            GlowHdrScale = PartOf(glow.Knee),
            GlowIntensity = PartOf(glow.Intensity),
            GlowStrength = PartOf(glow.Strength),

            // A bloom above 0 makes every pixel glow, and a sprite or a tile with it (F-47).
            GlowBloom = 0f,

            // The glow adds light, and a dark pixel far from a source stays as it was.
            GlowBlendMode = Godot.Environment.GlowBlendModeEnum.Additive,
        };

        for (int level = 0; level < Glow.LevelCount; level += 1)
        {
            environment.SetGlowLevel(level, PartOf(glow.Levels[level]));
        }

        return environment;
    }

    /// <summary>Builds the material of the view of the world in the frame (F-103).</summary>
    /// <returns>The material.</returns>
    /// <exception cref="InvalidOperationException">Godot loaded no shader (T-2, D-825).</exception>
    public static ShaderMaterial ViewMaterial()
    {
        // The load reports a failure in the log alone, so the result takes a check (T-2).
        Shader? loaded = ResourceLoader.Load<Shader>(ViewShaderPath);
        return new ShaderMaterial
        {
            Shader = loaded ?? throw new InvalidOperationException($"Godot loaded no shader from '{ViewShaderPath}' (D-825, T-2)."),
        };
    }

    /// <summary>Gives the modulate that draws a source at its glow (D-912).</summary>
    /// <param name="glow">The linear light of the source, in basis points of its palette color: 1 or more.</param>
    /// <returns>The modulate.</returns>
    /// <exception cref="ArgumentOutOfRangeException">The glow is 0 or less (T-2).</exception>
    /// <remarks>
    /// Godot reads a modulate as sRGB, and it turns it into linear light in HDR 2D (F-103). Thus
    /// the modulate is the sRGB value of the glow, and the source draws at the glow in linear light.
    /// </remarks>
    public static Color ModulateOf(int glow)
    {
        ArgumentOutOfRangeException.ThrowIfNegativeOrZero(glow);

        float part = PartOf(glow);
        return new Color(part, part, part).LinearToSrgb();
    }

    private static float PartOf(int basisPoints) => basisPoints / (float)BasisPoints.One;
}
