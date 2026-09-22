using System;
using System.Collections.Generic;
using Godot;
using TheThingBelow.Core.Content;
using TheThingBelow.Core.Effects;

namespace TheThingBelow.Game.Ui;

/// <summary>
/// The weather of one place on screen: the streams of particles and the layers of fog of its
/// ambient file (D-187, D-202). The map screen and the battle screen each build one, so the
/// weather of a place plays over its backdrop too (D-205).
/// </summary>
/// <remarks>
/// The streams draw over the figures, and the fog draws over the streams, because fog wraps
/// each figure (D-885). The mark of a sight draws over both, so fog never hides it (D-208).
/// <para>
/// A place with no ambient file builds no node, and every call of <see cref="Show"/> then does
/// nothing (D-202).
/// </para>
/// </remarks>
public sealed class AmbientLayer
{
    /// <summary>The Z index of the motes of the weather: over each figure (`area-effects.md` section 7.2).</summary>
    public const int StreamZIndex = 3;

    /// <summary>The Z index of the lowest layer of fog: over the streams.</summary>
    public const int FogZIndex = 4;

    private readonly List<MoteLayer> motes = [];
    private readonly FogSheets? fogs;

    private AmbientLayer(AmbientEffect? effect, IReadOnlyList<MoteLayer> motes, FogSheets? fogs)
    {
        this.Effect = effect;
        this.motes.AddRange(motes);
        this.fogs = fogs;
    }

    /// <summary>The ambient file that this layer draws, or no value for a place with no weather.</summary>
    public AmbientEffect? Effect { get; }

    /// <summary>The count of motes that the weather drew on the last frame (D-893).</summary>
    public int NodeCount
    {
        get
        {
            int total = 0;
            foreach (MoteLayer layer in this.motes)
            {
                total += layer.MoteCount;
            }

            return total;
        }
    }

    /// <summary>The count of layers of fog, which the budget counts as one full-screen pass each (D-523).</summary>
    public int FogCount => this.fogs?.SheetCount ?? 0;

    /// <summary>Builds the weather of one place under a parent node.</summary>
    /// <param name="effect">The ambient file of the place, or no value for a place with no weather.</param>
    /// <param name="palette">The palette (D-181).</param>
    /// <param name="parent">The node that takes the nodes of the weather: the world of the screen.</param>
    /// <returns>The layer.</returns>
    /// <exception cref="ArgumentNullException">The palette or the parent is null (T-2).</exception>
    /// <exception cref="ContentException">The palette holds no key of the weather (T-2).</exception>
    public static AmbientLayer Build(AmbientEffect? effect, Palette palette, Node2D parent)
    {
        ArgumentNullException.ThrowIfNull(palette);
        ArgumentNullException.ThrowIfNull(parent);

        if (effect is null)
        {
            return new AmbientLayer(null, [], null);
        }

        var motes = new List<MoteLayer>();
        foreach (MoteStream stream in effect.Emitters)
        {
            motes.Add(MoteLayer.Build(effect, stream, palette, StreamZIndex, parent));
        }

        return new AmbientLayer(
            effect,
            motes,
            FogSheets.Build(effect.Id.Value, effect.Fogs, palette, effect.Lit, FogZIndex, parent));
    }

    /// <summary>Shows the weather over one view at one tick.</summary>
    /// <param name="point">The north-west corner of the view, in art pixels of the parent.</param>
    /// <param name="width">The width of the view, in art pixels.</param>
    /// <param name="height">The height of the view, in art pixels.</param>
    /// <param name="tick">The tick of the run, from 0.</param>
    /// <exception cref="ArgumentOutOfRangeException">The tick is below zero, or the view is empty (T-2).</exception>
    public void Show(Vector2 point, int width, int height, long tick)
    {
        foreach (MoteLayer layer in this.motes)
        {
            layer.Show((int)point.X, (int)point.Y, tick);
        }

        this.fogs?.Show(point, width, height, tick);
    }
}
