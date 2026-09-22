using System;
using System.Collections.Generic;
using Godot;
using TheThingBelow.Core.Content;
using TheThingBelow.Core.Effects;

namespace TheThingBelow.Game.Ui;

/// <summary>
/// The Godot particle nodes of one hit file, built at load from its emitters (D-182, D-875,
/// D-879). No resource file holds an effect (G-6).
/// </summary>
/// <remarks>
/// Each node runs at speed zero, so no frame time of the engine moves a particle. The screen
/// seeks each burst to its age in ticks: a restart with the fixed seed, then the time of that
/// age at a fixed 60 steps a second. Thus one tick of a fight shows the same particles on every
/// run and in every capture (D-172, T-7).
/// <para>
/// Each palette key of an emitter takes a node of its own with one fixed color. A color ramp
/// would sample a texture, and a sample between two keys gives a blend outside the palette
/// (D-181). Each particle has no texture, so it draws as a square of its size (G-27).
/// </para>
/// </remarks>
public sealed class HitBurst
{
    /// <summary>The Z index of a burst: above every figure, and below the health bars and the pointer (`area-effects.md` section 7.2).</summary>
    public const int BurstZIndex = 5;

    /// <summary>The steps of the particle process in one second: one for each tick (D-164).</summary>
    public const int StepsPerSecond = 60;

    private readonly List<BurstNode> nodes;
    private BurstPlace? shown;

    private HitBurst(HitEffect effect, List<BurstNode> nodes)
    {
        this.Effect = effect;
        this.nodes = nodes;
    }

    /// <summary>The hit file of this burst.</summary>
    public HitEffect Effect { get; }

    /// <summary>The count of particle nodes of the burst: one for each palette key of each emitter.</summary>
    public int NodeCount => this.nodes.Count;

    /// <summary>Builds the particle nodes of one hit file under a parent node, hidden.</summary>
    /// <param name="effect">The hit file.</param>
    /// <param name="palette">The palette, which gives each key its color (D-181).</param>
    /// <param name="parent">The node that takes the particle nodes: the world of the battle, so a shake moves them too (D-876).</param>
    /// <returns>The burst.</returns>
    /// <exception cref="ArgumentNullException">An argument is null (T-2).</exception>
    /// <exception cref="ContentException">The palette holds no key of an emitter (T-2).</exception>
    public static HitBurst Build(HitEffect effect, Palette palette, Node2D parent)
    {
        ArgumentNullException.ThrowIfNull(effect);
        ArgumentNullException.ThrowIfNull(palette);
        ArgumentNullException.ThrowIfNull(parent);

        var nodes = new List<BurstNode>();
        foreach (ParticleEmitter emitter in effect.Emitters)
        {
            for (int key = 0; key < emitter.Colors.Count; key += 1)
            {
                int amount = emitter.AmountOf(key);
                if (amount == 0)
                {
                    continue;
                }

                BurstNode node = BuildNode(effect, emitter, ColorOf(effect, palette, emitter.Colors[key]), amount);
                parent.AddChild(node.Particles);
                nodes.Add(node);
            }
        }

        return new HitBurst(effect, nodes);
    }

    /// <summary>Hides every node of the burst.</summary>
    public void Hide()
    {
        if (this.shown is null)
        {
            return;
        }

        this.shown = null;
        foreach (BurstNode node in this.nodes)
        {
            node.Particles.Visible = false;
        }
    }

    /// <summary>Shows the burst at one age, and seeks each node to it (D-172).</summary>
    /// <param name="point">The start of the burst, in art pixels of the world.</param>
    /// <param name="away">One when the burst leaves toward the east, and minus one toward the west: away from the attacker.</param>
    /// <param name="seed">The seed of the burst, from the tick when its event started, so one hit shows one pattern.</param>
    /// <param name="age">The ticks since the blow.</param>
    /// <exception cref="ArgumentOutOfRangeException">The side is not one or minus one, or the age is below zero (T-2).</exception>
    public void Seek(Vector2 point, int away, uint seed, int age)
    {
        if (away != 1 && away != -1)
        {
            throw new ArgumentOutOfRangeException(nameof(away), away, "A burst leaves toward the east (1) or the west (-1) (T-2).");
        }

        ArgumentOutOfRangeException.ThrowIfNegative(age);

        var place = new BurstPlace(point, away, seed, age);
        if (place == this.shown)
        {
            return;
        }

        this.shown = place;
        for (int index = 0; index < this.nodes.Count; index += 1)
        {
            BurstNode node = this.nodes[index];
            GpuParticles2D particles = node.Particles;
            if (age >= node.LifetimeTicks)
            {
                particles.Visible = false;
                continue;
            }

            particles.Position = point;
            node.Process.Direction = DirectionOf(node.Degrees, away);
            particles.Seed = unchecked(seed + (uint)index);
            particles.Visible = true;

            // A restart with the kept seed and a request of the whole age seek the burst from
            // its start, so each tick shows one picture however the frames fall (D-172, T-7).
            particles.Restart(keepSeed: true);
            particles.RequestParticlesProcess(age / (float)StepsPerSecond);
        }
    }

    private static BurstNode BuildNode(HitEffect effect, ParticleEmitter emitter, Color color, int amount)
    {
        var process = new ParticleProcessMaterial
        {
            Direction = DirectionOf(emitter.Direction, 1),
            Spread = emitter.Spread,
            InitialVelocityMin = emitter.SlowestSpeed,
            InitialVelocityMax = emitter.FastestSpeed,
            Gravity = new Vector3(0, emitter.Gravity, 0),
            ScaleMin = emitter.Size,
            ScaleMax = emitter.Size,
            Color = color,
            EmissionShape = ParticleProcessMaterial.EmissionShapeEnum.Box,
            EmissionBoxExtents = new Vector3(emitter.Area, emitter.Area, 0),
        };

        var particles = new GpuParticles2D
        {
            Amount = amount,
            Lifetime = emitter.LifetimeTicks / (double)StepsPerSecond,
            OneShot = true,
            Explosiveness = 1,
            SpeedScale = 0,
            FixedFps = StepsPerSecond,
            Interpolate = false,
            FractDelta = false,
            UseFixedSeed = true,
            LocalCoords = true,
            ProcessMaterial = process,
            ZIndex = BurstZIndex,
            Visible = false,
        };

        if (!effect.Lit)
        {
            // An unlit burst gives its own light, so the scene light never dims it (D-183).
            particles.Material = new CanvasItemMaterial { LightMode = CanvasItemMaterial.LightModeEnum.Unshaded };
        }

        return new BurstNode(particles, process, emitter.Direction, emitter.LifetimeTicks);
    }

    private static Color ColorOf(HitEffect effect, Palette palette, char key)
    {
        if (!palette.TryColorOf(key, out PaletteColor? found))
        {
            throw ContentException.ForField(effect.File, "emitters", $"the palette holds no key '{key}' (D-181)");
        }

        return Color.Color8((byte)found.Red, (byte)found.Green, (byte)found.Blue);
    }

    /// <summary>Gives the direction of a burst: the degrees of the file, turned to face away from the attacker.</summary>
    private static Vector3 DirectionOf(int degrees, int away)
    {
        float radians = Mathf.DegToRad(degrees);
        return new Vector3(Mathf.Cos(radians) * away, Mathf.Sin(radians), 0);
    }

    /// <summary>One particle node, with its process material and the values that each seek reads.</summary>
    private sealed record BurstNode(GpuParticles2D Particles, ParticleProcessMaterial Process, int Degrees, int LifetimeTicks);

    /// <summary>The place, the side, the seed, and the age that the burst shows now.</summary>
    private sealed record BurstPlace(Vector2 Point, int Away, uint Seed, int Age);
}
