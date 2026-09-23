using System;
using System.Text;
using Godot;
using TheThingBelow.Core;
using TheThingBelow.Core.Content;
using TheThingBelow.Core.Hashing;
using TheThingBelow.Core.Light;

namespace TheThingBelow.Game.Ui;

/// <summary>
/// The glow pass of the world: the glow of each light source, the materials of the blur and of
/// the pass, and the pulse of each glow (D-188, D-913, D-914).
/// </summary>
/// <remarks>
/// Each fire that glows draws a small rectangle on the glow layer alone. The world view never
/// draws that layer, and a mask view that shares the world draws it alone. A quarter view blurs
/// the mask, and the frame adds the blur over the world, under the UI (D-210). No sprite or tile
/// draws on the glow layer, so none glows (F-47).
/// <para>
/// The pulse of each glow is a slow wave of the tick, with a phase from the id of the source, so
/// one tick gives one picture and no two torches pulse together (D-913, T-7, D-172).
/// </para>
/// </remarks>
public static class GlowPass
{
    /// <summary>The visibility layer of the glow of each source: layer 20, which no other node takes.</summary>
    public const uint GlowLayer = 1u << 19;

    /// <summary>The canvas layers that the world view draws: every layer but the glow layer.</summary>
    public const uint WorldLayers = uint.MaxValue & ~GlowLayer;

    /// <summary>The path of the shader of the blur of the mask (D-825).</summary>
    public const string BlurShaderPath = "res://shaders/glow_blur.gdshader";

    /// <summary>The path of the shader that adds the glow over the world (D-825).</summary>
    public const string AddShaderPath = "res://shaders/glow_add.gdshader";

    /// <summary>The name of the uniform of the intensity of the glow.</summary>
    public const string IntensityName = "intensity";

    /// <summary>The name of the uniform of the count of steps, 0 for a smooth glow.</summary>
    public const string StepsName = "steps";

    /// <summary>The name of the uniform of the block size of a stepped glow.</summary>
    public const string CellSizeName = "cell_size";

    /// <summary>The side of a cell of the blur, in art pixels: the blur view is a quarter of the world view.</summary>
    public const int BlurCell = 4;

    /// <summary>The seed of the hash of the phase of each pulse. A new value moves the pulse of every glow.</summary>
    private const ulong PulseSeed = 0x676C6F77UL;

    /// <summary>Builds the material of the blur of the mask.</summary>
    /// <returns>The material.</returns>
    /// <exception cref="InvalidOperationException">Godot loaded no shader (T-2, D-825).</exception>
    public static ShaderMaterial BlurMaterial() => new() { Shader = LoadShader(BlurShaderPath) };

    /// <summary>Builds the material of the pass that adds the glow over the world.</summary>
    /// <returns>The material, with the default values of the shader until a screen gives the glow file.</returns>
    /// <exception cref="InvalidOperationException">Godot loaded no shader (T-2, D-825).</exception>
    public static ShaderMaterial AddMaterial() => new() { Shader = LoadShader(AddShaderPath) };

    /// <summary>Gives the pass the values of the glow file (D-913, D-914).</summary>
    /// <param name="material">The material of <see cref="AddMaterial"/>.</param>
    /// <param name="glow">The glow file.</param>
    /// <exception cref="ArgumentNullException">An argument is null (T-2).</exception>
    public static void SetValues(ShaderMaterial material, Glow glow)
    {
        ArgumentNullException.ThrowIfNull(material);
        ArgumentNullException.ThrowIfNull(glow);

        material.SetShaderParameter(IntensityName, glow.Intensity / (float)BasisPoints.One);
        material.SetShaderParameter(StepsName, glow.Steps);
        material.SetShaderParameter(CellSizeName, glow.CellSize);
    }

    /// <summary>Builds the glow of one source on the glow layer, or nothing for a glow of strength 0 (D-912).</summary>
    /// <param name="id">The id of the source, which names the node (T-2).</param>
    /// <param name="seed">The glow of the fire of the source.</param>
    /// <param name="palette">The palette, which gives the key its color (D-181).</param>
    /// <param name="parent">The world of the screen.</param>
    /// <returns>The node, or null when the source never glows.</returns>
    /// <exception cref="ArgumentNullException">An argument is null (T-2).</exception>
    /// <exception cref="ContentException">The palette holds no such key (T-2).</exception>
    public static ColorRect? BuildSeed(string id, GlowSeed seed, Palette palette, Node2D parent)
    {
        ArgumentException.ThrowIfNullOrEmpty(id);
        ArgumentNullException.ThrowIfNull(seed);
        ArgumentNullException.ThrowIfNull(palette);
        ArgumentNullException.ThrowIfNull(parent);

        if (seed.Strength == 0)
        {
            return null;
        }

        Color full = WorldLights.PaletteColorOf(palette, new LightColor(seed.Key, seed.Strength));
        float strength = seed.Strength / (float)BasisPoints.One;
        var rect = new ColorRect
        {
            Name = $"{id}_glow",
            Color = new Color(full.R * strength, full.G * strength, full.B * strength),
            Size = new Vector2(seed.Width, seed.Height),
            MouseFilter = Control.MouseFilterEnum.Ignore,

            // The mask view alone draws this layer, so the world never shows the rectangle.
            VisibilityLayer = GlowLayer,

            // The glow keeps its own color, and the scene light never changes it (D-183).
            Material = new CanvasItemMaterial { LightMode = CanvasItemMaterial.LightModeEnum.Unshaded },
        };

        // A view draws an item only when the item and each parent share a layer with the view.
        // The world of the screen thus takes the glow layer too, and each of its other children
        // keeps the first layer alone, so the mask still draws the glow alone.
        parent.VisibilityLayer |= GlowLayer;
        parent.AddChild(rect);
        return rect;
    }

    /// <summary>Puts the glow of a source at the place of its light at one tick, with its pulse (D-913).</summary>
    /// <param name="rect">The node of <see cref="BuildSeed"/>.</param>
    /// <param name="seed">The glow of the fire of the source.</param>
    /// <param name="glow">The glow file, which holds the pulse.</param>
    /// <param name="id">The id of the source, which sets the phase of its pulse.</param>
    /// <param name="place">The place of the light, in art pixels of the parent.</param>
    /// <param name="tick">The tick of the run, from 0.</param>
    /// <exception cref="ArgumentNullException">An argument is null (T-2).</exception>
    /// <exception cref="ArgumentOutOfRangeException">The tick is below zero (T-2).</exception>
    public static void ShowSeed(ColorRect rect, GlowSeed seed, Glow glow, string id, Vector2 place, long tick)
    {
        ArgumentNullException.ThrowIfNull(rect);
        ArgumentNullException.ThrowIfNull(seed);

        float part = PulseOf(glow, id, tick);
        rect.Position = new Vector2(place.X + seed.X - (seed.Width / 2), place.Y + seed.Y - (seed.Height / 2));
        rect.Modulate = new Color(part, part, part);
    }

    /// <summary>Gives the pulse of one glow at one tick: 1 at the top of the wave, and 1 less the depth at the low (D-913).</summary>
    /// <param name="glow">The glow file.</param>
    /// <param name="id">The id of the source, which sets the phase of its pulse.</param>
    /// <param name="tick">The tick of the run, from 0.</param>
    /// <returns>The part of the glow that shows.</returns>
    /// <exception cref="ArgumentNullException">The glow is null (T-2).</exception>
    /// <exception cref="ArgumentOutOfRangeException">The tick is below zero (T-2).</exception>
    /// <remarks>The wave is a cosine, so the glow swells and fades with no jump, where a torch light steps (D-891).</remarks>
    public static float PulseOf(Glow glow, string id, long tick)
    {
        ArgumentNullException.ThrowIfNull(glow);
        ArgumentException.ThrowIfNullOrEmpty(id);
        ArgumentOutOfRangeException.ThrowIfNegative(tick);

        ulong hash = XxHash64.Compute(Encoding.UTF8.GetBytes(id), PulseSeed);
        long phase = (long)(hash % (ulong)glow.PulseTicks);
        long step = (tick + phase) % glow.PulseTicks;
        float wave = (1f - MathF.Cos(2f * MathF.PI * step / glow.PulseTicks)) / 2f;
        return 1f - (glow.PulseDepth / (float)BasisPoints.One * wave);
    }

    private static Shader LoadShader(string path)
    {
        // The load reports a failure in the log alone, so the result takes a check (T-2).
        Shader? loaded = ResourceLoader.Load<Shader>(path);
        return loaded ?? throw new InvalidOperationException($"Godot loaded no shader from '{path}' (D-825, T-2).");
    }
}
