using System;
using System.Collections.Generic;
using Godot;

namespace DeckTest;

/// <summary>The kind of an emitter. The four ambient kinds of region one come from D-187.</summary>
public enum AmbientKind
{
    Snow,
    Embers,
    Smoke,
    Dust,
}

/// <summary>
/// Builds the world of the test scene one time, then turns each part of the load on and off.
/// A stage never adds or removes a node, so no stage pays for a node that it did not ask for.
/// </summary>
public sealed class SceneRig
{
    /// <summary>The frame of D-568. Every screen draws this, and the Deck adds black bars.</summary>
    public const int FrameWidth = 1280;
    public const int FrameHeight = 720;

    /// <summary>Godot drops each light past 15 on one canvas item, with no message (F-46).</summary>
    public const int MaxLights = 15;

    public const int MaxEmitters = 16;
    public const int ParticlesForEachEmitter = 512;

    private const int TileSize = 32;

    private readonly List<PointLight2D> _lights = new();
    private readonly List<GpuParticles2D> _emitters = new();

    private readonly CanvasModulate _ambient;
    private readonly ColorRect _fog;
    private readonly ColorRect _crt;
    private readonly ColorRect _transition;
    private readonly WorldEnvironment _environment;
    private readonly ShaderMaterial _transitionMaterial;

    public SceneRig(Node root)
    {
        ArgumentNullException.ThrowIfNull(root);

        var world = new Node2D { Name = "World" };
        root.AddChild(world);

        AddBackdrop(world);
        AddGround(world);
        AddProps(world);
        AddOccluders(world);
        BuildLights(world);
        BuildEmitters(world);

        _ambient = new CanvasModulate { Name = "Ambient", Color = new Color(0.34f, 0.36f, 0.46f) };
        root.AddChild(_ambient);

        _environment = BuildEnvironment();
        root.AddChild(_environment);

        var passes = new CanvasLayer { Name = "Passes", Layer = 1 };
        root.AddChild(passes);

        _fog = AddFullScreenPass(passes, "Fog", "res://shaders/fog.gdshader");
        _crt = AddFullScreenPass(passes, "Crt", "res://shaders/crt.gdshader");
        _transition = AddFullScreenPass(passes, "Transition", "res://shaders/transition.gdshader");
        _transitionMaterial = (ShaderMaterial)_transition.Material;

        SetLightCount(0);
        SetEmitterCount(0);
        SetPasses(crt: false, glow: false, fog: false);
        SetTransition(-1f);
    }

    /// <summary>Turns on the first <paramref name="count"/> lights, and turns off the rest.</summary>
    public void SetLightCount(int count)
    {
        if (count < 0 || count > MaxLights)
        {
            throw new ArgumentOutOfRangeException(nameof(count), count, $"The rig holds 0 to {MaxLights} lights.");
        }

        for (int i = 0; i < _lights.Count; i++)
        {
            _lights[i].Visible = i < count;
        }
    }

    /// <summary>Turns on the first <paramref name="count"/> emitters, and turns off the rest.</summary>
    public void SetEmitterCount(int count)
    {
        if (count < 0 || count > MaxEmitters)
        {
            throw new ArgumentOutOfRangeException(nameof(count), count, $"The rig holds 0 to {MaxEmitters} emitters.");
        }

        for (int i = 0; i < _emitters.Count; i++)
        {
            bool on = i < count;
            _emitters[i].Visible = on;
            _emitters[i].Emitting = on;
        }
    }

    public void SetPasses(bool crt, bool glow, bool fog)
    {
        _crt.Visible = crt;
        _fog.Visible = fog;
        _environment.Environment.GlowEnabled = glow;
    }

    /// <summary>Sets the wipe. A value below zero hides the pass, so it costs nothing.</summary>
    public void SetTransition(float progress)
    {
        if (progress < 0f)
        {
            _transition.Visible = false;
            return;
        }

        _transition.Visible = true;
        _transitionMaterial.SetShaderParameter("progress", Mathf.Clamp(progress, 0f, 1f));
    }

    /// <summary>The count of full-screen passes that the frame draws now.</summary>
    public int ActivePassCount()
    {
        int count = 0;
        if (_crt.Visible)
        {
            count++;
        }

        if (_fog.Visible)
        {
            count++;
        }

        if (_environment.Environment.GlowEnabled)
        {
            count++;
        }

        if (_transition.Visible)
        {
            count++;
        }

        return count;
    }

    private static void AddBackdrop(Node2D world)
    {
        var backdrop = new Sprite2D
        {
            Name = "Backdrop",
            Texture = TextureFactory.MakeBackdrop(FrameWidth / 2, FrameHeight / 2, seed: 11),
            Centered = false,
            Scale = new Vector2(2f, 2f),
            ZIndex = -100,
        };
        world.AddChild(backdrop);
    }

    private static void AddGround(Node2D world)
    {
        Color[] tints =
        {
            new(0.28f, 0.26f, 0.24f),
            new(0.22f, 0.24f, 0.27f),
            new(0.31f, 0.29f, 0.25f),
            new(0.19f, 0.21f, 0.23f),
        };

        var tileSet = new TileSet { TileSize = new Vector2I(TileSize, TileSize) };
        for (int i = 0; i < tints.Length; i++)
        {
            var source = new TileSetAtlasSource
            {
                Texture = TextureFactory.MakeBevelledTile(TileSize, tints[i], seed: (ulong)(20 + i)),
                TextureRegionSize = new Vector2I(TileSize, TileSize),
            };
            source.CreateTile(Vector2I.Zero);
            tileSet.AddSource(source, i);
        }

        var layer = new TileMapLayer { Name = "Ground", TileSet = tileSet };
        world.AddChild(layer);

        int columns = FrameWidth / TileSize;
        int rows = (FrameHeight / TileSize) + 1;
        var rng = new RandomNumberGenerator { Seed = 7 };

        for (int y = 0; y < rows; y++)
        {
            for (int x = 0; x < columns; x++)
            {
                layer.SetCell(new Vector2I(x, y), rng.RandiRange(0, tints.Length - 1), Vector2I.Zero);
            }
        }
    }

    private static void AddProps(Node2D world)
    {
        var props = new Node2D { Name = "Props" };
        world.AddChild(props);

        var texture = TextureFactory.MakeDomedSprite(TileSize, new Color(0.62f, 0.58f, 0.50f));
        var rng = new RandomNumberGenerator { Seed = 31 };

        for (int i = 0; i < 24; i++)
        {
            props.AddChild(new Sprite2D
            {
                Texture = texture,
                Position = new Vector2(rng.RandfRange(40f, FrameWidth - 40f), rng.RandfRange(40f, FrameHeight - 40f)),
                ZIndex = 1,
            });
        }
    }

    private static void AddOccluders(Node2D world)
    {
        var walls = new Node2D { Name = "Walls" };
        world.AddChild(walls);

        var rng = new RandomNumberGenerator { Seed = 41 };

        for (int i = 0; i < 18; i++)
        {
            float w = rng.RandfRange(48f, 160f);
            float h = rng.RandfRange(24f, 96f);
            var polygon = new OccluderPolygon2D
            {
                Polygon = new[]
                {
                    Vector2.Zero,
                    new Vector2(w, 0f),
                    new Vector2(w, h),
                    new Vector2(0f, h),
                },
            };

            walls.AddChild(new LightOccluder2D
            {
                Occluder = polygon,
                Position = new Vector2(rng.RandfRange(0f, FrameWidth - w), rng.RandfRange(0f, FrameHeight - h)),
            });
        }
    }

    private void BuildLights(Node2D world)
    {
        var holder = new Node2D { Name = "Lights" };
        world.AddChild(holder);

        var texture = TextureFactory.MakeSoftDot(256, falloff: 2.0f);
        var rng = new RandomNumberGenerator { Seed = 53 };

        Color[] tints =
        {
            new(1.00f, 0.72f, 0.38f),
            new(0.55f, 0.78f, 1.00f),
            new(0.85f, 1.00f, 0.72f),
        };

        for (int i = 0; i < MaxLights; i++)
        {
            var light = new PointLight2D
            {
                Texture = texture,
                TextureScale = rng.RandfRange(1.2f, 2.2f),
                // Above one, so the glow of D-188 has light brighter than white to find.
                Energy = rng.RandfRange(1.1f, 1.7f),
                Color = tints[i % tints.Length],
                Position = new Vector2(rng.RandfRange(80f, FrameWidth - 80f), rng.RandfRange(80f, FrameHeight - 80f)),
                ShadowEnabled = true,
                // Hard shadows, as D-183 sets.
                ShadowFilter = Light2D.ShadowFilterEnum.None,
                ZIndex = 5,
            };

            holder.AddChild(light);
            _lights.Add(light);
        }
    }

    private void BuildEmitters(Node2D world)
    {
        var holder = new Node2D { Name = "Ambient" };
        world.AddChild(holder);

        var texture = TextureFactory.MakeSoftDot(32, falloff: 1.4f);
        var rng = new RandomNumberGenerator { Seed = 67 };
        var kinds = new[] { AmbientKind.Snow, AmbientKind.Embers, AmbientKind.Smoke, AmbientKind.Dust };

        for (int i = 0; i < MaxEmitters; i++)
        {
            AmbientKind kind = kinds[i % kinds.Length];
            var emitter = new GpuParticles2D
            {
                Name = $"{kind}{i}",
                Texture = texture,
                Amount = ParticlesForEachEmitter,
                Lifetime = 4.0,
                Preprocess = 2.0,
                ProcessMaterial = MakeAmbientMaterial(kind),
                Position = new Vector2(rng.RandfRange(0f, FrameWidth), rng.RandfRange(0f, FrameHeight)),
                VisibilityRect = new Rect2(-FrameWidth, -FrameHeight, FrameWidth * 2, FrameHeight * 2),
                ZIndex = 10,
            };

            holder.AddChild(emitter);
            _emitters.Add(emitter);
        }
    }

    private static ParticleProcessMaterial MakeAmbientMaterial(AmbientKind kind)
    {
        var material = new ParticleProcessMaterial
        {
            EmissionShape = ParticleProcessMaterial.EmissionShapeEnum.Box,
            EmissionBoxExtents = new Vector3(FrameWidth * 0.5f, 64f, 0f),
            ParticleFlagDisableZ = true,
        };

        switch (kind)
        {
            case AmbientKind.Snow:
                material.Direction = new Vector3(0.2f, 1f, 0f);
                material.Spread = 12f;
                material.InitialVelocityMin = 40f;
                material.InitialVelocityMax = 90f;
                material.Gravity = new Vector3(6f, 20f, 0f);
                material.ScaleMin = 0.15f;
                material.ScaleMax = 0.35f;
                material.Color = new Color(0.90f, 0.94f, 1.00f, 0.85f);
                break;

            case AmbientKind.Embers:
                material.Direction = new Vector3(0f, -1f, 0f);
                material.Spread = 30f;
                material.InitialVelocityMin = 30f;
                material.InitialVelocityMax = 80f;
                material.Gravity = new Vector3(0f, -30f, 0f);
                material.ScaleMin = 0.10f;
                material.ScaleMax = 0.25f;
                // Brighter than white, so the glow of D-188 takes it.
                material.Color = new Color(1.60f, 0.70f, 0.25f, 0.95f);
                break;

            case AmbientKind.Smoke:
                material.Direction = new Vector3(0.3f, -1f, 0f);
                material.Spread = 20f;
                material.InitialVelocityMin = 10f;
                material.InitialVelocityMax = 35f;
                material.Gravity = new Vector3(4f, -12f, 0f);
                material.ScaleMin = 0.60f;
                material.ScaleMax = 1.40f;
                material.Color = new Color(0.35f, 0.35f, 0.38f, 0.30f);
                break;

            case AmbientKind.Dust:
                material.Direction = new Vector3(0f, 1f, 0f);
                material.Spread = 45f;
                material.InitialVelocityMin = 5f;
                material.InitialVelocityMax = 20f;
                material.Gravity = new Vector3(2f, 8f, 0f);
                material.ScaleMin = 0.08f;
                material.ScaleMax = 0.18f;
                material.Color = new Color(0.80f, 0.76f, 0.62f, 0.45f);
                break;

            default:
                throw new ArgumentOutOfRangeException(nameof(kind), kind, "The rig has no material for this ambient kind.");
        }

        return material;
    }

    private static WorldEnvironment BuildEnvironment()
    {
        var environment = new Godot.Environment
        {
            BackgroundMode = Godot.Environment.BGMode.Canvas,
            GlowEnabled = false,
            GlowIntensity = 0.9f,
            GlowBloom = 0.15f,
            GlowHdrThreshold = 1.0f,
            GlowBlendMode = Godot.Environment.GlowBlendModeEnum.Additive,
        };

        return new WorldEnvironment { Name = "Glow", Environment = environment };
    }

    private static ColorRect AddFullScreenPass(CanvasLayer parent, string name, string shaderPath)
    {
        var shader = GD.Load<Shader>(shaderPath);
        if (shader is null)
        {
            throw new InvalidOperationException($"The pass {name} has no shader at {shaderPath}.");
        }

        var rect = new ColorRect
        {
            Name = name,
            Material = new ShaderMaterial { Shader = shader },
            Color = new Color(1f, 1f, 1f, 1f),
            MouseFilter = Control.MouseFilterEnum.Ignore,
        };
        rect.SetAnchorsPreset(Control.LayoutPreset.FullRect);
        parent.AddChild(rect);
        return rect;
    }
}
