using System;
using Godot;
using TheThingBelow.Core;
using TheThingBelow.Core.Content;
using TheThingBelow.Core.Light;

namespace TheThingBelow.Game.Ui;

/// <summary>
/// The fire of one torch on screen: its flame, its embers, its smoke, and the light that steps
/// between the levels of its file (D-888, D-890, D-891).
/// </summary>
/// <remarks>
/// The light of a torch is a pair of Godot lights (D-853). Each step changes the energy and the
/// range of both, and it moves both and the streams by the jump of the step. A hash of the id
/// and of the step number picks each step, so each torch changes out of step with the next, and
/// each tick shows one picture (T-7, D-172, <see cref="TorchFire"/>).
/// </remarks>
public sealed class TorchFlame
{
    /// <summary>The Z index of the flame of a torch: over each figure, and under the weather (`area-effects.md` section 7.2).</summary>
    public const int FlameZIndex = 2;

    private readonly PointLight2D ground;
    private readonly PointLight2D figures;
    private readonly ParticleStreams streams;
    private readonly float baseEnergy;
    private readonly float baseScale;
    private Vector2 place;

    private TorchFlame(
        string id,
        TorchFire fire,
        PointLight2D ground,
        PointLight2D figures,
        ParticleStreams streams)
    {
        this.Id = id;
        this.Fire = fire;
        this.ground = ground;
        this.figures = figures;
        this.streams = streams;
        this.baseEnergy = ground.Energy;
        this.baseScale = ground.TextureScale;
    }

    /// <summary>The id of the light: the id of the decor piece, or the name of the carried light.</summary>
    public string Id { get; }

    /// <summary>The fire of the file of this torch.</summary>
    public TorchFire Fire { get; }

    /// <summary>The count of particle nodes of this fire.</summary>
    public int NodeCount => this.streams.NodeCount;

    /// <summary>Tells whether every node of this fire stands still, so each ember stays in the world (F-97).</summary>
    public bool NodesStandStill => this.streams.NodesStandStill;

    /// <summary>Shows or hides the streams of this fire, as the switch of the carried light does (D-847).</summary>
    public bool Visible
    {
        set => this.streams.Visible = value;
    }

    /// <summary>Builds the fire of one torch over the pair of lights of that torch.</summary>
    /// <param name="id">The id of the light, which names each node (T-2).</param>
    /// <param name="fire">The fire of the file of the torch.</param>
    /// <param name="lights">The pair of Godot lights of the torch (D-853).</param>
    /// <param name="palette">The palette (D-181).</param>
    /// <param name="parent">The node that takes the particle nodes: the world of the screen.</param>
    /// <returns>The fire on screen.</returns>
    /// <exception cref="ArgumentNullException">An argument is null (T-2).</exception>
    /// <exception cref="ContentException">The palette holds no key of a stream of the fire (T-2).</exception>
    public static TorchFlame Build(
        string id,
        TorchFire fire,
        (PointLight2D Ground, PointLight2D Figures) lights,
        Palette palette,
        Node2D parent)
    {
        ArgumentException.ThrowIfNullOrEmpty(id);
        ArgumentNullException.ThrowIfNull(fire);
        ArgumentNullException.ThrowIfNull(lights.Ground);
        ArgumentNullException.ThrowIfNull(lights.Figures);

        return new TorchFlame(
            id,
            fire,
            lights.Ground,
            lights.Figures,
            ParticleStreams.Build(id, fire.Emitters, palette, lit: false, FlameZIndex, parent));
    }

    /// <summary>Puts the torch at one place, in art pixels of the parent: the place of its light with no jump.</summary>
    /// <param name="point">The place of the light.</param>
    public void MoveTo(Vector2 point) => this.place = point;

    /// <summary>Shows the torch at one tick: its step, its jump, and its streams (D-891).</summary>
    /// <param name="tick">The tick of the run, from 0.</param>
    /// <exception cref="ArgumentOutOfRangeException">The tick is below zero (T-2).</exception>
    public void Show(long tick)
    {
        FlickerStep step = this.Fire.StepAt(this.Id, tick);
        var at = new Vector2(this.place.X + step.JumpX, this.place.Y + step.JumpY);
        float energy = this.baseEnergy * step.Level.Strength / BasisPoints.One;
        float scale = this.baseScale * step.Level.Range / BasisPoints.One;

        this.ground.Position = at;
        this.ground.Energy = energy;
        this.ground.TextureScale = scale;
        this.figures.Position = at;
        this.figures.Energy = energy;
        this.figures.TextureScale = scale;

        this.streams.MoveTo(at);
        this.streams.Seek(tick);
    }
}
