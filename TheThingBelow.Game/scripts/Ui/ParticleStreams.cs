using System;
using System.Collections.Generic;
using System.Text;
using Godot;
using TheThingBelow.Core.Content;
using TheThingBelow.Core.Hashing;
using TheThingBelow.Core.Light;

namespace TheThingBelow.Game.Ui;

/// <summary>
/// The Godot particle nodes of a list of streams: the weather of a map, or the fire of one
/// torch (D-187, D-890). No resource file holds an effect (G-6).
/// </summary>
/// <remarks>
/// Each node runs at speed zero, as a burst does, so no frame time of the engine moves a
/// particle (<see cref="HitBurst"/>). The build seeks each node one lifetime forward, so the
/// view is full from the first frame. Each later seek asks for the ticks since the last one, so
/// one tick shows the same particles on every run and in every capture (D-172, T-7).
/// <para>
/// Each palette key of a stream takes a node of its own with one fixed color, and each particle
/// draws as a square of its size with hard edges (D-181, G-27).
/// </para>
/// </remarks>
public sealed class ParticleStreams
{
    /// <summary>The seed of the hash of each node name. A new value changes the pattern of every stream.</summary>
    private const ulong NameSeed = 0x73747265616DUL;

    private readonly List<StreamNode> nodes;
    private long shown = -1;

    private ParticleStreams(List<StreamNode> nodes) => this.nodes = nodes;

    /// <summary>The count of particle nodes: one for each palette key of each stream.</summary>
    public int NodeCount => this.nodes.Count;

    /// <summary>
    /// Tells whether every node stands at the place of its parent (F-97). A node that moves
    /// carries each live particle with it, and the weather then rides the view.
    /// </summary>
    public bool NodesStandStill
    {
        get
        {
            foreach (StreamNode node in this.nodes)
            {
                if (node.Particles.Position != Vector2.Zero)
                {
                    return false;
                }
            }

            return true;
        }
    }

    /// <summary>Shows or hides every node of the streams, as the switch of the carried light does (D-847).</summary>
    public bool Visible
    {
        set
        {
            foreach (StreamNode node in this.nodes)
            {
                node.Particles.Visible = value;
            }
        }
    }

    /// <summary>Builds the particle nodes of a list of streams under a parent node.</summary>
    /// <param name="name">The name of each node, which every error names (T-2).</param>
    /// <param name="emitters">The streams, from the file that holds them.</param>
    /// <param name="palette">The palette, which gives each key its color (D-181).</param>
    /// <param name="lit">True when the scene light falls on the particles (D-183).</param>
    /// <param name="zIndex">The Z index of each node.</param>
    /// <param name="parent">The node that takes the particle nodes.</param>
    /// <returns>The streams.</returns>
    /// <exception cref="ArgumentNullException">An argument is null (T-2).</exception>
    /// <exception cref="ContentException">The palette holds no key of a stream (T-2).</exception>
    public static ParticleStreams Build(
        string name,
        IReadOnlyList<StreamEmitter> emitters,
        Palette palette,
        bool lit,
        int zIndex,
        Node2D parent)
    {
        ArgumentException.ThrowIfNullOrEmpty(name);
        ArgumentNullException.ThrowIfNull(emitters);
        ArgumentNullException.ThrowIfNull(palette);
        ArgumentNullException.ThrowIfNull(parent);

        var nodes = new List<StreamNode>();
        for (int index = 0; index < emitters.Count; index += 1)
        {
            StreamEmitter emitter = emitters[index];
            for (int key = 0; key < emitter.Colors.Count; key += 1)
            {
                int amount = emitter.AmountOf(key);
                if (amount == 0)
                {
                    continue;
                }

                StreamNode node = BuildNode(
                    $"{name}_{index}_{key}",
                    emitter,
                    ColorOf(name, palette, emitter.Colors[key]),
                    amount,
                    lit,
                    zIndex);
                parent.AddChild(node.Particles);
                nodes.Add(node);
            }
        }

        return new ParticleStreams(nodes);
    }

    /// <summary>Puts the start box of each stream at one point, in art pixels of the parent.</summary>
    /// <param name="point">The anchor of the streams: the north-west corner of the view, or the place of a light.</param>
    /// <remarks>
    /// The node itself never moves. The start box moves inside it, so each particle that lives
    /// already stays where the world put it, and the view moves past it (F-97).
    /// </remarks>
    public void MoveTo(Vector2 point)
    {
        foreach (StreamNode node in this.nodes)
        {
            Vector2 at = point + node.Offset;
            node.Process.EmissionShapeOffset = new Vector3(at.X, at.Y, 0);
        }
    }

    /// <summary>Shows the streams at one tick, and seeks each node to it (D-172).</summary>
    /// <param name="tick">The tick of the run, from 0.</param>
    /// <exception cref="ArgumentOutOfRangeException">The tick is below zero (T-2).</exception>
    /// <remarks>
    /// A step forward of one tick or more, and inside one lifetime, asks the node for that
    /// time. Any other step, such as the first one or a jump, starts the node again and seeks it
    /// one lifetime forward, so the picture holds particles of every age.
    /// </remarks>
    public void Seek(long tick)
    {
        ArgumentOutOfRangeException.ThrowIfNegative(tick);

        if (tick == this.shown)
        {
            return;
        }

        foreach (StreamNode node in this.nodes)
        {
            long step = tick - this.shown;
            bool forward = this.shown >= 0 && step > 0 && step <= node.LifetimeTicks;
            if (forward)
            {
                node.Particles.RequestParticlesProcess(step / (float)HitBurst.StepsPerSecond);
                continue;
            }

            // A restart with the kept seed gives the same particles for the same tick (T-7).
            node.Particles.Restart(keepSeed: true);
            long warm = node.LifetimeTicks + (tick % node.LifetimeTicks);
            node.Particles.RequestParticlesProcess(warm / (float)HitBurst.StepsPerSecond);
        }

        this.shown = tick;
    }

    /// <summary>
    /// Gives the seed of one particle node, from its name. Godot gives every node the seed 0
    /// under a fixed seed, so two nodes of one stream would hold the same particles, and two
    /// torches of one kind would hold the same flame.
    /// </summary>
    /// <param name="name">The name of the node, which holds the id of the effect, the emitter, and the key.</param>
    /// <returns>The seed, which is never 0, because Godot reads 0 as no seed.</returns>
    private static uint SeedOf(string name)
    {
        ulong hash = XxHash64.Compute(Encoding.UTF8.GetBytes(name), NameSeed);
        uint seed = (uint)(hash & uint.MaxValue);
        return seed == 0 ? 1 : seed;
    }

    private static StreamNode BuildNode(string name, StreamEmitter emitter, Color color, int amount, bool lit, int zIndex)
    {
        var process = new ParticleProcessMaterial
        {
            Direction = DirectionOf(emitter.Direction),
            Spread = emitter.Spread,
            InitialVelocityMin = emitter.SlowestSpeed,
            InitialVelocityMax = emitter.FastestSpeed,
            Gravity = new Vector3(0, emitter.Gravity, 0),
            ScaleMin = emitter.Size,
            ScaleMax = emitter.Size,
            Color = color,
            EmissionShape = ParticleProcessMaterial.EmissionShapeEnum.Box,
            EmissionBoxExtents = new Vector3(emitter.HalfWidth, emitter.HalfHeight, 0),
        };

        var particles = new GpuParticles2D
        {
            Name = name,
            Amount = amount,
            Lifetime = emitter.LifetimeTicks / (double)HitBurst.StepsPerSecond,
            OneShot = false,
            Explosiveness = 0,
            SpeedScale = 0,
            FixedFps = HitBurst.StepsPerSecond,
            Interpolate = false,
            FractDelta = false,
            UseFixedSeed = true,
            Seed = SeedOf(name),

            // Each particle sits in the world of the parent node, so the view moves past it and
            // never carries it (F-97). The start box moves inside the node, and the node stands
            // still, so a particle that lives already never moves with the view.
            LocalCoords = true,
            ProcessMaterial = process,
            ZIndex = zIndex,
        };

        if (!lit)
        {
            // An unlit stream gives its own light, so the dark of the ambient light never dims it (D-183).
            particles.Material = new CanvasItemMaterial { LightMode = CanvasItemMaterial.LightModeEnum.Unshaded };
        }

        return new StreamNode(particles, process, new Vector2(emitter.X, emitter.Y), emitter.LifetimeTicks);
    }

    private static Color ColorOf(string name, Palette palette, char key)
    {
        if (!palette.TryColorOf(key, out PaletteColor? found))
        {
            throw ContentException.ForField(Palette.Path, "colors", $"the palette holds no key '{key}' of the effect '{name}' (D-181)");
        }

        return Color.Color8((byte)found.Red, (byte)found.Green, (byte)found.Blue);
    }

    /// <summary>Gives the direction of a stream: 0 points east, and 90 points down the screen.</summary>
    private static Vector3 DirectionOf(int degrees)
    {
        float radians = Mathf.DegToRad(degrees);
        return new Vector3(Mathf.Cos(radians), Mathf.Sin(radians), 0);
    }

    /// <summary>One particle node, with its process material, the offset of its start box, and the life of its particles.</summary>
    private sealed record StreamNode(GpuParticles2D Particles, ParticleProcessMaterial Process, Vector2 Offset, int LifetimeTicks);
}
