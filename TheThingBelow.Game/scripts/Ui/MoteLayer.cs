using System;
using System.Collections.Generic;
using Godot;
using TheThingBelow.Core.Content;
using TheThingBelow.Core.Effects;

namespace TheThingBelow.Game.Ui;

/// <summary>
/// The motes of a weather on screen: Game draws each one as a square of the palette, from the
/// pure function of the tick that Core holds (D-187, D-893).
/// </summary>
/// <remarks>
/// Godot advances a particle system about one second at a time, so no long stream of particles
/// could hold its motion or give one picture for one tick (F-100). Each mote of this node comes
/// from the tick alone, so it falls, it sways, it lands, and it lies still, and one tick gives
/// one picture on every run and in every capture (T-7, D-172).
/// <para>
/// Each mote draws with one color of the palette and hard edges, as every effect does (G-27,
/// D-181). A lit weather takes the scene light of the map, and an unlit one gives its own
/// light (D-183).
/// </para>
/// </remarks>
public sealed partial class MoteLayer : Node2D
{
    /// <summary>The path of the shader that gives a mote the strength of a light and never its color (D-825, D-893).</summary>
    public const string LightShaderPath = "res://shaders/mote_light.gdshader";

    /// <summary>The name of the uniform of the color of a mote with no light.</summary>
    public const string DarkColorName = "dark_color";

    /// <summary>The name of the uniform of the color of a mote in full light.</summary>
    public const string LightColorName = "light_color";

    private MoteStream stream = null!;
    private IReadOnlyList<Mote> shown = [];
    private Color drawn;

    /// <summary>The count of motes that this layer drew on the last frame.</summary>
    public int MoteCount => this.shown.Count;

    /// <summary>Builds the layer of one stream of a weather under a parent node.</summary>
    /// <param name="effect">The ambient file of the place.</param>
    /// <param name="stream">The stream of motes that this layer draws.</param>
    /// <param name="palette">The palette, which gives each key its color (D-181).</param>
    /// <param name="zIndex">The Z index of the layer: over each figure.</param>
    /// <param name="parent">The node that takes the layer: the world of the screen.</param>
    /// <returns>The layer.</returns>
    /// <exception cref="ArgumentNullException">An argument is null (T-2).</exception>
    /// <exception cref="ContentException">The palette holds no key of a stream (T-2).</exception>
    public static MoteLayer Build(AmbientEffect effect, MoteStream stream, Palette palette, int zIndex, Node2D parent)
    {
        ArgumentNullException.ThrowIfNull(effect);
        ArgumentNullException.ThrowIfNull(stream);
        ArgumentNullException.ThrowIfNull(palette);
        ArgumentNullException.ThrowIfNull(parent);

        var layer = new MoteLayer
        {
            Name = $"motes_{effect.Id.Value}_{stream.Color}",
            ZIndex = zIndex,
            stream = stream,
        };

        Color lit = ColorOf(effect, palette, stream.Color);
        Color dark = ColorOf(effect, palette, stream.DarkColor);
        if (effect.Lit)
        {
            // A lit mote holds its dark gray with no light and its light gray in full light. It
            // takes the strength of each light and never its color, so torchlight never paints
            // it yellow, and the energy of a torch never pushes it to white (D-183).
            var material = new ShaderMaterial { Shader = LoadLightShader() };
            material.SetShaderParameter(DarkColorName, dark);
            material.SetShaderParameter(LightColorName, lit);
            layer.Material = material;
            layer.drawn = dark;
        }
        else
        {
            // An unlit weather gives its own light, so the dark of the ambient light never dims it (D-183).
            layer.Material = new CanvasItemMaterial { LightMode = CanvasItemMaterial.LightModeEnum.Unshaded };
            layer.drawn = lit;
        }

        parent.AddChild(layer);
        return layer;
    }

    /// <summary>Reads the motes of one view at one tick, and draws them on the next frame.</summary>
    /// <param name="viewX">The column of the north-west corner of the view, in art pixels of the world.</param>
    /// <param name="viewY">The row of the north-west corner of the view, in art pixels of the world.</param>
    /// <param name="tick">The tick of the run, from 0.</param>
    /// <exception cref="ArgumentOutOfRangeException">The tick is below zero (T-2).</exception>
    public void Show(int viewX, int viewY, long tick)
    {
        ArgumentOutOfRangeException.ThrowIfNegative(tick);

        this.shown = AmbientMotes.InView(this.stream, viewX, viewY, tick);
        this.QueueRedraw();
    }

    /// <summary>Draws each mote of the last call of <see cref="Show"/> as a square of its color.</summary>
    public override void _Draw()
    {
        foreach (Mote mote in this.shown)
        {
            this.DrawRect(new Rect2(mote.X, mote.Y, mote.Size, mote.Size), this.drawn, filled: true);
        }
    }

    private static Shader LoadLightShader()
    {
        // The load reports a failure in the log alone, so the result takes a check (T-2).
        Shader? loaded = ResourceLoader.Load<Shader>(LightShaderPath);
        return loaded ?? throw new InvalidOperationException(
            $"Godot loaded no shader from '{LightShaderPath}' (D-825, T-2).");
    }

    private static Color ColorOf(AmbientEffect effect, Palette palette, char key)
    {
        if (!palette.TryColorOf(key, out PaletteColor? found))
        {
            throw ContentException.ForField(Palette.Path, "colors", $"the palette holds no key '{key}' of the weather '{effect.Id.Value}' (D-181)");
        }

        return Color.Color8((byte)found.Red, (byte)found.Green, (byte)found.Blue);
    }

}
