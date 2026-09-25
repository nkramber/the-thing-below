using System;
using System.Collections.Generic;
using Godot;
using TheThingBelow.Core;
using TheThingBelow.Core.Content;
using TheThingBelow.Core.Effects;
using TheThingBelow.Core.Light;
using TheThingBelow.Storage;

namespace TheThingBelow.Game.Ui;

/// <summary>
/// The flash of a spell on the battle picture: one point light at the target, one tint over the
/// picture, and the burst of the spell (D-183, D-186, D-878, D-1032). The screen builds one of
/// each at the start of a fight and moves them for each spell.
/// </summary>
/// <remarks>
/// The reduced level plays the light and the tint at a quarter of their strength, and the off
/// level plays neither. The burst plays at each level, because a burst is no flash (D-863).
/// Every value comes from the tick of the flash, so a capture of one tick shows one picture (T-7).
/// </remarks>
public sealed class SpellFlash
{
    /// <summary>The name of the light of a spell, which the check of the lights reads.</summary>
    public const string LightName = "spell_light";

    /// <summary>The Z index of the tint: above every figure and burst, and below the health bars and the pointer.</summary>
    public const int TintZIndex = 8;

    /// <summary>The part of the strength that the reduced level plays, in basis points: a quarter (D-863).</summary>
    public const int ReducedRate = 2500;

    private readonly Palette palette;
    private readonly PointLight2D light;
    private readonly ColorRect tint;
    private readonly SortedDictionary<string, HitBurst> bursts = new(StringComparer.Ordinal);

    private SpellFlash(Palette palette, PointLight2D light, ColorRect tint)
    {
        this.palette = palette;
        this.light = light;
        this.tint = tint;
    }

    /// <summary>The count of particle nodes of every burst of a spell.</summary>
    public int NodeCount
    {
        get
        {
            int count = 0;
            foreach (HitBurst burst in this.bursts.Values)
            {
                count += burst.NodeCount;
            }

            return count;
        }
    }

    /// <summary>Builds the light, the tint, and one burst for each spell under the world of the fight, hidden.</summary>
    /// <param name="effects">The effect content, which holds each spell.</param>
    /// <param name="palette">The palette, which gives each key its color (D-181).</param>
    /// <param name="world">The world of the fight, so a shake moves the flash too (D-876).</param>
    /// <returns>The flash.</returns>
    /// <exception cref="ArgumentNullException">An argument is null (T-2).</exception>
    public static SpellFlash Build(EffectContent effects, Palette palette, Node2D world)
    {
        ArgumentNullException.ThrowIfNull(effects);
        ArgumentNullException.ThrowIfNull(palette);
        ArgumentNullException.ThrowIfNull(world);

        // The light takes its values from each spell as it plays. It starts off, with a range and
        // a height that Godot can draw (F-46).
        var start = new PointLightValues(new LightColor('k', 0), 1, 1);
        PointLight2D light = WorldLights.Point(LightName, palette, start, WorldLights.LightTexture(WorldLights.LightFalloff), WorldLights.GroundItems, 0);
        light.Visible = false;
        world.AddChild(light);

        var tint = new ColorRect
        {
            Size = new Vector2(FrameRoot.WorldWidth, FrameRoot.WorldHeight),
            ZIndex = TintZIndex,
            MouseFilter = Control.MouseFilterEnum.Ignore,
            Visible = false,
        };
        world.AddChild(tint);

        // A tint of the picture never glows (D-916).
        GlowPass.LiftAboveGlow(tint);

        var flash = new SpellFlash(palette, light, tint);
        foreach (SpellEffect spell in effects.Spells)
        {
            flash.bursts.Add(spell.Id.Value, HitBurst.Build(spell.Burst, palette, world));
        }

        return flash;
    }

    /// <summary>Shows one spell at one tick of its flash.</summary>
    /// <param name="spell">The spell.</param>
    /// <param name="point">The body of the target, in art pixels of the world.</param>
    /// <param name="away">One when the burst leaves toward the east, and minus one toward the west.</param>
    /// <param name="seed">The seed of the burst, from the tick when the spell started.</param>
    /// <param name="age">The ticks since the start of the flash, below its length.</param>
    /// <param name="level">The level of the flash and shake setting (D-863).</param>
    /// <exception cref="ArgumentNullException">The spell is null (T-2).</exception>
    /// <exception cref="ArgumentOutOfRangeException">The age is outside the length of the flash (T-2).</exception>
    public void Show(SpellEffect spell, Vector2 point, int away, uint seed, int age, EffectLevel level)
    {
        ArgumentNullException.ThrowIfNull(spell);
        if (age < 0 || age >= spell.LengthTicks)
        {
            throw new ArgumentOutOfRangeException(nameof(age), age, $"The flash '{spell.Id.Value}' lasts {spell.LengthTicks} ticks (T-2).");
        }

        int rate = level switch
        {
            EffectLevel.Full => BasisPoints.One,
            EffectLevel.Reduced => ReducedRate,
            EffectLevel.Off => 0,
            _ => throw new ArgumentOutOfRangeException(nameof(level), level, "The level names no step of the flash setting (D-863, T-2)."),
        };

        long lightStrength = (long)spell.Light.Color.Strength * spell.LightAt(age) / BasisPoints.One * rate / BasisPoints.One;
        this.light.Visible = lightStrength > 0;
        this.light.Position = point;
        this.light.Color = WorldLights.PaletteColorOf(this.palette, spell.Light.Color);
        this.light.Energy = lightStrength / (float)BasisPoints.One;
        this.light.TextureScale = 2f * spell.Light.Range / WorldLights.TextureSize;
        this.light.Height = spell.Light.Height;

        long tintStrength = (long)spell.Tint.Strength * spell.TintAt(age) / BasisPoints.One * rate / BasisPoints.One;
        this.tint.Visible = tintStrength > 0;
        Color color = WorldLights.PaletteColorOf(this.palette, spell.Tint);
        this.tint.Color = new Color(color.R, color.G, color.B, tintStrength / (float)BasisPoints.One);

        HitBurst shown = this.bursts[spell.Id.Value];
        shown.Seek(point, away, seed, age);
        foreach (HitBurst burst in this.bursts.Values)
        {
            if (!ReferenceEquals(burst, shown))
            {
                burst.Hide();
            }
        }
    }

    /// <summary>Hides the light, the tint, and every burst.</summary>
    public void Hide()
    {
        this.light.Visible = false;
        this.tint.Visible = false;
        foreach (HitBurst burst in this.bursts.Values)
        {
            burst.Hide();
        }
    }
}
