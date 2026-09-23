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
/// A stream runs on the engine in play, because it draws no rule and a frame of play needs no
/// seek (G-1). A capture of the screen test needs one picture for one tick, so it seeks each
/// node instead: a restart with the kept seed, then the time of the age of that tick (D-172,
/// T-7). Godot advances a stream on a request only after a restart, so a request alone holds
/// every particle still (F-100).
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

    /// <summary>
    /// Tells whether every node holds one region of the world, so no stream stops when the view
    /// moves away from the parent (F-98).
    /// </summary>
    /// <param name="area">The region that each node must hold, in art pixels of the parent.</param>
    /// <returns>True when each node holds the region.</returns>
    public bool NodesHold(Rect2 area)
    {
        foreach (StreamNode node in this.nodes)
        {
            if (!node.Particles.VisibilityRect.Encloses(area))
            {
                return false;
            }
        }

        return true;
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
    /// <param name="visible">
    /// The region of the parent that each node holds, in art pixels. Godot stops a particle
    /// system whose region leaves the screen, so this rect holds every place that the start box
    /// of a stream can reach (F-98).
    /// </param>
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
        Rect2 visible,
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
                    zIndex,
                    visible);
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

    /// <summary>Seeks each node to one tick, for a capture of the screen test (D-172).</summary>
    /// <param name="tick">The tick of the run, from 0.</param>
    /// <exception cref="ArgumentOutOfRangeException">The tick is below zero (T-2).</exception>
    /// <remarks>
    /// Each seek starts the node again with its own seed, and it asks for the time of one
    /// lifetime and the age of the tick. The picture then holds particles of every age, and one
    /// tick gives one picture on every run. A request with no restart moves no particle (F-100).
    /// <para>
    /// A frame of play never seeks. The engine moves each particle there, and the node holds the
    /// speed of the engine.
    /// </para>
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
            node.Particles.SpeedScale = 0;
            long warm = node.LifetimeTicks + (tick % node.LifetimeTicks);
            node.Particles.Preprocess = warm / (double)HitBurst.StepsPerSecond;
            node.Particles.Restart(keepSeed: true);
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

    private static StreamNode BuildNode(
        string name,
        StreamEmitter emitter,
        Color color,
        int amount,
        bool lit,
        int zIndex,
        Rect2 visible)
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
            SpeedScale = 1,
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

            // Godot stops a particle system whose region leaves the screen. The node stands at
            // its parent, so the default region of 200 by 200 pixels leaves the screen as soon
            // as the view moves, and every stream of the map goes out (F-98).
            VisibilityRect = visible,
        };

        if (!lit)
        {
            // An unlit stream gives its own light, so the dark of the ambient light never dims it (D-183).
            particles.Material = new CanvasItemMaterial { LightMode = CanvasItemMaterial.LightModeEnum.Unshaded };
        }

        if (emitter.Glow > 0)
        {
            // The stream draws its palette color times its glow, above the glow threshold, so it
            // glows (D-910, D-912).
            particles.SelfModulate = GlowPass.ModulateOf(emitter.Glow);
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
