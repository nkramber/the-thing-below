using System;
using System.Collections.Generic;
using Godot;
using TheThingBelow.Core.Content;
using TheThingBelow.Core.Effects;

namespace TheThingBelow.Game.Ui;

/// <summary>
/// The fog of a weather: one rectangle over the view, whose shader draws every layer of the fog
/// in one full-screen pass (D-897, D-898). The fog draws above the figures (D-885).
/// </summary>
/// <remarks>
/// The shader reads a fractal noise at each world pixel, and each layer fades smoothly from clear
/// to its full strength in one palette key, at the pixel size of the art (D-900, D-901, D-181).
/// Where layers overlap, the strongest layer wins (D-899).
/// <para>
/// The shader never reads the clock of Godot. At each tick, this class gives each layer the
/// world pixel of the view with the drift of that layer, in whole art pixels, so one tick gives
/// one picture in every capture (F-100, D-172, D-230).
/// </para>
/// </remarks>
public sealed class FogPass
{
    /// <summary>The path of the shader of a fog that gives its own light (D-183, D-825).</summary>
    public const string ShaderPath = "res://shaders/fog.gdshader";

    /// <summary>The path of the shader of a fog that takes the scene light (D-183, D-825).</summary>
    public const string LitShaderPath = "res://shaders/fog_lit.gdshader";

    /// <summary>The name of the uniform of the count of layers.</summary>
    public const string LayerCountName = "layer_count";

    /// <summary>The name of the uniform of the color of each layer.</summary>
    public const string ColorsName = "colors";

    /// <summary>The name of the uniform of the world column of the view for each layer, with its drift.</summary>
    public const string OriginXName = "origin_x";

    /// <summary>The name of the uniform of the world row of the view for each layer, with its drift.</summary>
    public const string OriginYName = "origin_y";

    /// <summary>The name of the uniform of the noise scale of each layer.</summary>
    public const string ScalesName = "scales";

    /// <summary>The name of the uniform of the seed of each layer.</summary>
    public const string SeedsName = "seeds";

    /// <summary>The name of the uniform of the noise level where each layer starts to show.</summary>
    public const string FadeFromName = "fade_from";

    /// <summary>The name of the uniform of the noise level where each layer is full.</summary>
    public const string FadeToName = "fade_to";

    /// <summary>The name of the uniform of the full strength of each layer.</summary>
    public const string StrengthsName = "strengths";

    private readonly ColorRect rect;
    private readonly ShaderMaterial material;
    private readonly IReadOnlyList<FogLayer> layers;

    private FogPass(ColorRect rect, ShaderMaterial material, IReadOnlyList<FogLayer> layers)
    {
        this.rect = rect;
        this.material = material;
        this.layers = layers;
    }

    /// <summary>The count of layers of the fog, which the one pass draws (D-898).</summary>
    public int LayerCount => this.layers.Count;

    /// <summary>Builds the pass of a fog under a parent node.</summary>
    /// <param name="name">The name of the node, which every error names (T-2).</param>
    /// <param name="fogs">The layers of the fog: 1 to <see cref="FogLayer.MostLayers"/>.</param>
    /// <param name="palette">The palette, which gives the key of each layer its color (D-181).</param>
    /// <param name="lit">True when the scene light falls on the fog (D-183).</param>
    /// <param name="zIndex">The Z index of the fog: over the streams.</param>
    /// <param name="parent">The node that takes the pass.</param>
    /// <returns>The pass.</returns>
    /// <exception cref="ArgumentNullException">An argument is null (T-2).</exception>
    /// <exception cref="ArgumentOutOfRangeException">The fog holds no layer, or more than the shader draws (T-2).</exception>
    /// <exception cref="ContentException">The palette holds no key of a layer (T-2).</exception>
    /// <exception cref="InvalidOperationException">Godot loaded no shader (T-2, D-825).</exception>
    public static FogPass Build(
        string name,
        IReadOnlyList<FogLayer> fogs,
        Palette palette,
        bool lit,
        int zIndex,
        Node2D parent)
    {
        ArgumentException.ThrowIfNullOrEmpty(name);
        ArgumentNullException.ThrowIfNull(fogs);
        ArgumentNullException.ThrowIfNull(palette);
        ArgumentNullException.ThrowIfNull(parent);
        ArgumentOutOfRangeException.ThrowIfZero(fogs.Count);
        ArgumentOutOfRangeException.ThrowIfGreaterThan(fogs.Count, FogLayer.MostLayers);

        var material = new ShaderMaterial { Shader = LoadShader(lit ? LitShaderPath : ShaderPath) };
        SetLayers(material, name, fogs, palette);

        var rect = new ColorRect
        {
            Name = $"{name}_fog",
            Material = material,
            MouseFilter = Control.MouseFilterEnum.Ignore,
            ZIndex = zIndex,
        };

        parent.AddChild(rect);
        return new FogPass(rect, material, fogs);
    }

    /// <summary>Puts the pass over the view at one tick, with the drift of each layer.</summary>
    /// <param name="point">The north-west corner of the view, in art pixels of the parent.</param>
    /// <param name="width">The width of the view, in art pixels.</param>
    /// <param name="height">The height of the view, in art pixels.</param>
    /// <param name="tick">The tick of the run, from 0.</param>
    /// <exception cref="ArgumentOutOfRangeException">The tick is below zero, or the view is empty (T-2).</exception>
    /// <exception cref="OverflowException">The drift passes the range of a shader integer (T-2).</exception>
    public void Show(Vector2 point, int width, int height, long tick)
    {
        ArgumentOutOfRangeException.ThrowIfNegative(tick);
        ArgumentOutOfRangeException.ThrowIfNegativeOrZero(width);
        ArgumentOutOfRangeException.ThrowIfNegativeOrZero(height);

        int viewX = (int)point.X;
        int viewY = (int)point.Y;
        int[] originX = new int[FogLayer.MostLayers];
        int[] originY = new int[FogLayer.MostLayers];
        for (int index = 0; index < this.layers.Count; index += 1)
        {
            // The drift is art pixels in each second of 60 ticks, and whole pixels alone (D-230).
            // The shapes move east for a positive drift, so the noise reads the pixel west of the
            // pixel that it draws.
            FogLayer layer = this.layers[index];
            originX[index] = checked((int)(viewX - (layer.DriftX * tick / HitBurst.StepsPerSecond)));
            originY[index] = checked((int)(viewY - (layer.DriftY * tick / HitBurst.StepsPerSecond)));
        }

        // The rectangle moves with the view, and the noise reads world pixels, so the shapes of
        // the fog stay over the world and never ride the view (F-97).
        this.rect.Position = new Vector2(viewX, viewY);
        this.rect.Size = new Vector2(width, height);
        this.material.SetShaderParameter(OriginXName, originX);
        this.material.SetShaderParameter(OriginYName, originY);
    }

    /// <summary>Gives the shader the values of each layer that never change: the color, the noise, and the fade.</summary>
    private static void SetLayers(ShaderMaterial material, string name, IReadOnlyList<FogLayer> fogs, Palette palette)
    {
        var colors = new Color[FogLayer.MostLayers];
        int[] scales = new int[FogLayer.MostLayers];
        int[] seeds = new int[FogLayer.MostLayers];
        int[] fadeFrom = new int[FogLayer.MostLayers];
        int[] fadeTo = new int[FogLayer.MostLayers];
        int[] strengths = new int[FogLayer.MostLayers];
        for (int index = 0; index < fogs.Count; index += 1)
        {
            FogLayer layer = fogs[index];
            colors[index] = ColorOf(name, layer.Key, palette);
            scales[index] = layer.Scale;
            seeds[index] = layer.Seed;
            fadeFrom[index] = layer.From;
            fadeTo[index] = layer.To;
            strengths[index] = layer.Strength;
        }

        material.SetShaderParameter(LayerCountName, fogs.Count);
        material.SetShaderParameter(ColorsName, colors);
        material.SetShaderParameter(ScalesName, scales);
        material.SetShaderParameter(SeedsName, seeds);
        material.SetShaderParameter(FadeFromName, fadeFrom);
        material.SetShaderParameter(FadeToName, fadeTo);
        material.SetShaderParameter(StrengthsName, strengths);
    }

    private static Color ColorOf(string name, char key, Palette palette)
    {
        if (!palette.TryColorOf(key, out PaletteColor? found))
        {
            throw ContentException.ForField(Palette.Path, "colors", $"the palette holds no key '{key}' of the fog '{name}' (D-181)");
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
