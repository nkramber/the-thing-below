using System;
using System.Text;
using Godot;
using TheThingBelow.Core;
using TheThingBelow.Core.Content;
using TheThingBelow.Core.Hashing;
using TheThingBelow.Core.Light;

namespace TheThingBelow.Game.Ui;

/// <summary>
/// The glow of the world view: the environment of the world, the view of the world in the frame,
/// the glow halo of each fire and its pulse, and the layer that draws above the glow (D-188,
/// D-910, D-913, D-915, D-916, D-1075).
/// </summary>
/// <remarks>
/// The world draws in HDR 2D, so a pixel keeps linear light above full white. The glow of Godot
/// reads each pixel whose brightest channel passes the threshold of the glow file, and Core keeps
/// every lit sprite and tile below that threshold (F-47). The glow halo of each fire adds a soft
/// round light behind the flame, below the threshold, and it pulses on a slow wave of the tick, so
/// the glow never flickers (D-913). A glow rectangle above the threshold drew as a box over each
/// torch, so the halo took its place (D-1075).
/// <para>
/// The fog, the hit bursts, and the light shafts draw on a layer that the world view never draws.
/// An overlay view with no HDR 2D shares the world and draws that layer alone, above the glow. So
/// the fog blends as it did before HDR 2D, and it never glows (D-916, F-104). Each mark draws on a
/// layer of its own, which a mark view draws above the tilt-shift blur and the vignette (D-919).
/// The UI draws in the frame, above every view, so it never glows (D-210).
/// </para>
/// </remarks>
public static class GlowPass
{
    /// <summary>The path of the shader that turns the linear light of the world into sRGB in the frame (F-103, D-825).</summary>
    public const string ViewShaderPath = "res://shaders/world_view.gdshader";

    /// <summary>The visibility layer of the nodes above the glow: layer 20, which no other node takes (D-916).</summary>
    public const uint AboveGlowLayer = 1u << 19;

    /// <summary>
    /// The visibility layer of the marks: layer 21, which no other node takes. The mark view draws
    /// it above the tilt-shift blur and the vignette, so each mark stays sharp (D-208, D-919).
    /// </summary>
    public const uint MarkLayer = 1u << 20;

    /// <summary>The canvas layers that the world view draws: every layer but the layer above the glow and the layer of the marks.</summary>
    public const uint WorldLayers = uint.MaxValue & ~AboveGlowLayer & ~MarkLayer;

    /// <summary>The cap of the linear light that the glow reads: above the strongest light of 16 that a glow file permits (D-913).</summary>
    public const float LuminanceCap = 32f;

    /// <summary>
    /// The light at the edge of a glow halo, as a part of the light of its middle: 1%. The halo
    /// falls on a slow curve from its middle to that edge, in place of the fast fall of D-1075,
    /// and the edge then ends at no visible step (D-1092).
    /// </summary>
    public const float HaloEdgeShare = 0.01f;

    /// <summary>The seed of the hash of the phase of each pulse. A new value moves the pulse of every glow.</summary>
    private const ulong PulseSeed = 0x676C6F77UL;

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

            // The pulse moves a glow rectangle above 12, and the default cap of the light that the
            // glow reads, 12, would flatten the top of each wave (D-913).
            GlowHdrLuminanceCap = LuminanceCap,

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

    /// <summary>
    /// Puts a node and each node under it on the layer above the glow alone, and adds that layer
    /// to each parent, because a view draws an item only when each parent shares a layer with it
    /// (D-916, F-105).
    /// </summary>
    /// <param name="item">The node, which the caller has added to the world of its screen.</param>
    /// <exception cref="ArgumentNullException">The node is null (T-2).</exception>
    /// <exception cref="InvalidOperationException">The node has no parent yet, so no parent takes the layer (T-2).</exception>
    public static void LiftAboveGlow(CanvasItem item) => Lift(item, AboveGlowLayer);

    /// <summary>
    /// Puts a mark and each node under it on the layer of the marks alone, and adds that layer to
    /// each parent (D-208, D-919, F-105). The mark view draws it above the tilt-shift blur and the
    /// vignette, and above the glow, so each mark stays sharp and never glows.
    /// </summary>
    /// <param name="item">The node, which the caller has added to the world of its screen.</param>
    /// <exception cref="ArgumentNullException">The node is null (T-2).</exception>
    /// <exception cref="InvalidOperationException">The node has no parent yet, so no parent takes the layer (T-2).</exception>
    public static void LiftToMarks(CanvasItem item) => Lift(item, MarkLayer);

    /// <summary>Builds the glow halo of one fire, or nothing for a glow of strength 0 (D-912, D-1075).</summary>
    /// <param name="id">The id of the source, which names the node (T-2).</param>
    /// <param name="seed">The glow of the fire of the source.</param>
    /// <param name="palette">The palette, which gives the key its color (D-181).</param>
    /// <param name="halo">The round texture of every halo, from <see cref="WorldLights.BuildHaloTexture"/> with <see cref="HaloEdgeShare"/>.</param>
    /// <param name="parent">The world of the screen.</param>
    /// <returns>The node, or null when the source never glows.</returns>
    /// <exception cref="ArgumentNullException">An argument is null (T-2).</exception>
    /// <exception cref="ContentException">The palette holds no such key (T-2).</exception>
    /// <remarks>
    /// The halo adds its light to the world under it, and the world sorts it by its row, so it
    /// draws over the wall and the floor and under the torch and each figure south of it. Thus
    /// the flame keeps each of its pixels, and no box draws (D-1075).
    /// </remarks>
    public static Sprite2D? BuildSeed(string id, GlowSeed seed, Palette palette, Texture2D halo, Node2D parent)
    {
        ArgumentException.ThrowIfNullOrEmpty(id);
        ArgumentNullException.ThrowIfNull(seed);
        ArgumentNullException.ThrowIfNull(palette);
        ArgumentNullException.ThrowIfNull(halo);
        ArgumentNullException.ThrowIfNull(parent);

        if (seed.Strength == 0)
        {
            return null;
        }

        var sprite = new Sprite2D
        {
            Name = $"{id}_glow",
            Texture = halo,
            Centered = true,
            Scale = new Vector2(seed.Width / (float)halo.GetWidth(), seed.Height / (float)halo.GetHeight()),
            Modulate = WorldLights.PaletteColorOf(palette, new LightColor(seed.Key, seed.Strength)),

            // The glow keeps its own light, and the dark of the ambient light never dims it (D-183).
            // It adds to the world under it, as the spread of the glow did (D-1075).
            Material = new CanvasItemMaterial
            {
                LightMode = CanvasItemMaterial.LightModeEnum.Unshaded,
                BlendMode = CanvasItemMaterial.BlendModeEnum.Add,
            },
        };

        parent.AddChild(sprite);
        return sprite;
    }

    /// <summary>Puts the glow halo of a fire at the place of its light at one tick, with its pulse (D-913, D-1075).</summary>
    /// <param name="sprite">The node of <see cref="BuildSeed"/>.</param>
    /// <param name="seed">The glow of the fire of the source.</param>
    /// <param name="glow">The glow file, which holds the pulse.</param>
    /// <param name="id">The id of the source, which sets the phase of its pulse.</param>
    /// <param name="place">The place of the light, in art pixels of the parent.</param>
    /// <param name="tick">The tick of the run, from 0.</param>
    /// <exception cref="ArgumentNullException">An argument is null (T-2).</exception>
    /// <exception cref="ArgumentOutOfRangeException">The tick is below zero (T-2).</exception>
    /// <remarks>
    /// Godot reads a modulate as sRGB, and it turns it into linear light in HDR 2D (F-103). Thus
    /// the modulate is the sRGB value of the strength times the pulse, and the middle of the halo
    /// draws at that linear light.
    /// </remarks>
    public static void ShowSeed(Sprite2D sprite, GlowSeed seed, Glow glow, string id, Vector2 place, long tick)
    {
        ArgumentNullException.ThrowIfNull(sprite);
        ArgumentNullException.ThrowIfNull(seed);

        float light = PartOf(seed.Strength) * PulseOf(glow, id, tick);
        sprite.Position = new Vector2(place.X + seed.X, place.Y + seed.Y);
        sprite.SelfModulate = new Color(light, light, light).LinearToSrgb();
    }

    /// <summary>Gives the pulse of one glow at one tick: 1 at the top of the wave, and 1 less the depth at the low (D-913).</summary>
    /// <param name="glow">The glow file.</param>
    /// <param name="id">The id of the source, which sets the phase of its pulse.</param>
    /// <param name="tick">The tick of the run, from 0.</param>
    /// <returns>The part of the light of the source that shows.</returns>
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

    private static void Lift(CanvasItem item, uint layer)
    {
        ArgumentNullException.ThrowIfNull(item);

        if (item.GetParent() is not CanvasItem)
        {
            throw new InvalidOperationException($"The node '{item.Name}' has no parent in the world, so it cannot draw in a view above the glow (T-2, F-105).");
        }

        SetLayer(item, layer);
        for (Node? parent = item.GetParent(); parent is CanvasItem above; parent = above.GetParent())
        {
            above.VisibilityLayer |= layer;
        }
    }

    private static void SetLayer(CanvasItem item, uint layer)
    {
        item.VisibilityLayer = layer;
        foreach (Node child in item.GetChildren())
        {
            if (child is CanvasItem below)
            {
                SetLayer(below, layer);
            }
        }
    }

    private static float PartOf(int basisPoints) => basisPoints / (float)BasisPoints.One;
}
