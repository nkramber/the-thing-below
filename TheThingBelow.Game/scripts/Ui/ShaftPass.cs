using System;
using System.Collections.Generic;
using Godot;
using TheThingBelow.Core;
using TheThingBelow.Core.Content;
using TheThingBelow.Core.Light;

namespace TheThingBelow.Game.Ui;

/// <summary>
/// The light shafts of one map: one rectangle over the view, whose shader draws every shaft of the
/// map in one full-screen pass (D-849, D-918). The pass draws in the overlay view, above the fog
/// and the hit bursts, so a beam lights the fog and never glows (D-916, D-919).
/// </summary>
/// <remarks>
/// At each tick, this class gives the shader the top of each beam in art pixels of the view, and
/// the strength of each beam with its shimmer (D-921). The shader never reads the clock of Godot,
/// so one tick gives one picture in every capture (F-100, D-172).
/// </remarks>
public sealed class ShaftPass
{
    /// <summary>The path of the shader of the light shafts (D-825).</summary>
    public const string ShaderPath = "res://shaders/light_shafts.gdshader";

    /// <summary>The Z index of the pass: above the fog of Z 4 and the hit bursts of Z 5 (D-919).</summary>
    public const int ShaftZIndex = 6;

    /// <summary>The name of the uniform of the count of shafts.</summary>
    public const string ShaftCountName = "shaft_count";

    /// <summary>The name of the uniform of the world column of the view.</summary>
    public const string OriginXName = "origin_x";

    /// <summary>The name of the uniform of the world row of the view.</summary>
    public const string OriginYName = "origin_y";

    /// <summary>The name of the uniform of the column of the top of each beam, in the view.</summary>
    public const string TopXName = "top_x";

    /// <summary>The name of the uniform of the row of the top of each beam, in the view.</summary>
    public const string TopYName = "top_y";

    /// <summary>The name of the uniform of the slant of each beam.</summary>
    public const string SlantsName = "slants";

    /// <summary>The name of the uniform of the length of each beam.</summary>
    public const string LengthsName = "lengths";

    /// <summary>The name of the uniform of the width of each beam.</summary>
    public const string WidthsName = "widths";

    /// <summary>The name of the uniform of the palette color of each beam.</summary>
    public const string ColorsName = "colors";

    /// <summary>The name of the uniform of the strength of each beam at the tick.</summary>
    public const string StrengthsName = "strengths";

    /// <summary>The seed of the hash of the phase of each shimmer. A new value moves the shimmer of every shaft.</summary>
    private const ulong ShimmerSeed = 0x73686166UL;

    private readonly ColorRect rect;
    private readonly ShaderMaterial material;
    private readonly IReadOnlyList<PlacedShaft> shafts;

    private ShaftPass(ColorRect rect, ShaderMaterial material, IReadOnlyList<PlacedShaft> shafts)
    {
        this.rect = rect;
        this.material = material;
        this.shafts = shafts;
    }

    /// <summary>The count of shafts that the one pass draws.</summary>
    public int ShaftCount => this.shafts.Count;

    /// <summary>Builds the pass of the shafts of one map under the world of the map, or nothing for a map with no shaft.</summary>
    /// <param name="decor">The decor file of the map, which places each shaft (D-918).</param>
    /// <param name="light">The light content, which holds each shaft kind.</param>
    /// <param name="palette">The palette, which gives the key of each beam its color (D-181).</param>
    /// <param name="passes">The passes of the file, in the mode that the screen shows (D-917).</param>
    /// <param name="parent">The world of the map.</param>
    /// <returns>The pass, or null for a map with no shaft, which draws no pass (D-523).</returns>
    /// <exception cref="ArgumentNullException">An argument is null (T-2).</exception>
    /// <exception cref="ContentException">No file holds the kind of a shaft, or the palette holds no key of a beam (T-2).</exception>
    /// <exception cref="InvalidOperationException">Godot loaded no shader (T-2, D-825).</exception>
    public static ShaftPass? Build(DecorFile decor, LightContent light, Palette palette, Hd2dPasses passes, Node2D parent)
    {
        ArgumentNullException.ThrowIfNull(decor);
        ArgumentNullException.ThrowIfNull(light);
        ArgumentNullException.ThrowIfNull(palette);
        ArgumentNullException.ThrowIfNull(passes);
        ArgumentNullException.ThrowIfNull(parent);

        if (decor.Shafts.Count == 0)
        {
            return null;
        }

        var placed = new List<PlacedShaft>(decor.Shafts.Count);
        foreach (DecorPiece piece in decor.Shafts)
        {
            ShaftKind kind = light.ShaftOf(piece.Kind);
            placed.Add(new PlacedShaft(
                piece.Id.Value,
                kind,
                checked((piece.Tile.X * MapCamera.TilePixels) + kind.X),
                checked((piece.Tile.Y * MapCamera.TilePixels) + kind.Y)));
        }

        Shader? loaded = ResourceLoader.Load<Shader>(ShaderPath);
        var material = new ShaderMaterial
        {
            // The load reports a failure in the log alone, so the result takes a check (T-2).
            Shader = loaded ?? throw new InvalidOperationException($"Godot loaded no shader from '{ShaderPath}' (D-825, T-2)."),
        };
        LookPasses.SetMode(material, passes);
        SetBeams(material, placed, palette);

        var rect = new ColorRect
        {
            Name = "light_shafts",
            Material = material,
            MouseFilter = Control.MouseFilterEnum.Ignore,
            ZIndex = ShaftZIndex,
        };

        parent.AddChild(rect);
        GlowPass.LiftAboveGlow(rect);
        return new ShaftPass(rect, material, placed);
    }

    /// <summary>Puts the pass over the view at one tick, with the shimmer of each beam (D-921).</summary>
    /// <param name="point">The north-west corner of the view, in art pixels of the parent.</param>
    /// <param name="width">The width of the view, in art pixels.</param>
    /// <param name="height">The height of the view, in art pixels.</param>
    /// <param name="tick">The tick of the run, from 0.</param>
    /// <exception cref="ArgumentOutOfRangeException">The tick is below zero, or the view is empty (T-2).</exception>
    public void Show(Vector2 point, int width, int height, long tick)
    {
        ArgumentOutOfRangeException.ThrowIfNegative(tick);
        ArgumentOutOfRangeException.ThrowIfNegativeOrZero(width);
        ArgumentOutOfRangeException.ThrowIfNegativeOrZero(height);

        int viewX = (int)point.X;
        int viewY = (int)point.Y;
        int[] topX = new int[ShaftKind.MostShaftsOnMap];
        int[] topY = new int[ShaftKind.MostShaftsOnMap];
        float[] strengths = new float[ShaftKind.MostShaftsOnMap];
        for (int index = 0; index < this.shafts.Count; index += 1)
        {
            PlacedShaft shaft = this.shafts[index];
            topX[index] = shaft.X - viewX;
            topY[index] = shaft.Y - viewY;
            float shimmer = LightWave.PartOf(shaft.Id, shaft.Kind.ShimmerTicks, shaft.Kind.ShimmerDepth, tick, ShimmerSeed);
            strengths[index] = shaft.Kind.Strength / (float)BasisPoints.One * shimmer;
        }

        // The rectangle moves with the view, and each beam stays on the world (F-97).
        this.rect.Position = new Vector2(viewX, viewY);
        this.rect.Size = new Vector2(width, height);
        this.material.SetShaderParameter(OriginXName, viewX);
        this.material.SetShaderParameter(OriginYName, viewY);
        this.material.SetShaderParameter(TopXName, topX);
        this.material.SetShaderParameter(TopYName, topY);
        this.material.SetShaderParameter(StrengthsName, strengths);
    }

    /// <summary>Gives the shader the values of each beam that never change: the count, the shape, and the color.</summary>
    private static void SetBeams(ShaderMaterial material, IReadOnlyList<PlacedShaft> placed, Palette palette)
    {
        int[] slants = new int[ShaftKind.MostShaftsOnMap];
        int[] lengths = new int[ShaftKind.MostShaftsOnMap];
        int[] widths = new int[ShaftKind.MostShaftsOnMap];
        var colors = new Color[ShaftKind.MostShaftsOnMap];
        for (int index = 0; index < placed.Count; index += 1)
        {
            ShaftKind kind = placed[index].Kind;
            slants[index] = kind.Slant;
            lengths[index] = kind.Length;
            widths[index] = kind.Width;
            colors[index] = LookPasses.ColorOf(palette, kind.Key, kind.File);
        }

        material.SetShaderParameter(ShaftCountName, placed.Count);
        material.SetShaderParameter(SlantsName, slants);
        material.SetShaderParameter(LengthsName, lengths);
        material.SetShaderParameter(WidthsName, widths);
        material.SetShaderParameter(ColorsName, colors);
    }

    /// <summary>One shaft of the map at its place: the id of its piece, its kind, and the top of its beam in world pixels.</summary>
    private sealed record PlacedShaft(string Id, ShaftKind Kind, int X, int Y);
}
