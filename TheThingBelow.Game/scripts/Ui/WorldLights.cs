using System;
using System.Collections.Generic;
using System.Diagnostics;
using Godot;
using TheThingBelow.Core;
using TheThingBelow.Core.Content;
using TheThingBelow.Core.Light;

namespace TheThingBelow.Game.Ui;

/// <summary>
/// Builds the scene light of the world: the ambient light, each point light, and the one light
/// texture that every point light shares (D-183, D-846, F-46).
/// </summary>
/// <remarks>
/// Godot 2D light fails in silence in three ways, and this class closes each one (F-46):
/// <list type="bullet">
/// <item>A light with no texture joins no frame, so each light takes the built texture, and the
/// build checks it.</item>
/// <item>A light at height 0 gives no light to a flat pixel of a normal map, so each light sets
/// the height of its file, which the reader keeps at 1 or more.</item>
/// <item>One canvas item takes 15 lights at most, and the budget test of Core holds that limit
/// (D-842).</item>
/// </list>
/// <para>
/// An atlas texture or a canvas texture cannot serve as the texture of a light, so the light
/// texture is an image of its own and never a part of the atlas (`area-effects.md` section 7.6).
/// </para>
/// </remarks>
public static class WorldLights
{
    /// <summary>The side of the light texture, in pixels. A light scales it to twice its range.</summary>
    public const int TextureSize = 256;

    /// <summary>The light mask of the floor and the walls: the items that the ground light of a pair lights (D-853).</summary>
    public const int GroundItems = 1;

    /// <summary>The light mask of each figure: the items that the figure light of a pair lights (D-853).</summary>
    public const int FigureItems = 2;

    /// <summary>The occluder mask of each wall (D-852).</summary>
    public const int WallShadows = 1;

    /// <summary>The occluder mask of each figure but the lead (D-853).</summary>
    public const int FigureShadows = 2;

    /// <summary>The occluder mask of the lead, whose carried light it never blocks (D-853).</summary>
    public const int LeadShadows = 4;

    /// <summary>
    /// The power of the fall of a light: 1.5, so a pool of light has a soft edge and a bright
    /// middle, as the HD-2D look of D-849 asks.
    /// </summary>
    public const float LightFalloff = 1.5f;

    /// <summary>The light texture of each falloff that a caller asked for, by that falloff (G-14).</summary>
    private static readonly Dictionary<float, ImageTexture> LightTextureOfFalloff = [];

    /// <summary>The halo texture of each power that a caller asked for, by that power (G-14).</summary>
    private static readonly Dictionary<float, ImageTexture> HaloTextureOfPower = [];

    /// <summary>The count of builds of a light texture in this process, which the smoke session checks (G-14).</summary>
    public static int LightTextureBuilds { get; private set; }

    /// <summary>The count of builds of a halo texture in this process, which the smoke session checks (G-14).</summary>
    public static int HaloTextureBuilds { get; private set; }

    /// <summary>The time of every build of a light texture or a halo texture in this process (G-14).</summary>
    public static TimeSpan TextureBuildTime { get; private set; }

    /// <summary>
    /// Gives the light texture of one falloff: built on the first call, and shared by each later
    /// call, because the texture never changes (G-14).
    /// </summary>
    /// <param name="falloff">The power of the fall with the distance to the edge, such as <see cref="LightFalloff"/>.</param>
    /// <returns>The texture.</returns>
    /// <exception cref="ArgumentOutOfRangeException">The power is not above 0 (T-2).</exception>
    /// <exception cref="InvalidOperationException">Godot made no texture from the image, or freed the shared texture (T-2, F-45).</exception>
    /// <remarks>
    /// Each build set 65,536 pixels, one at a time, in 2.9 ms on an Apple silicon Mac. A map, a
    /// fight, and the spell flash of each fight each built one, so a fight start paid for two.
    /// </remarks>
    public static ImageTexture LightTexture(float falloff)
    {
        ArgumentOutOfRangeException.ThrowIfNegativeOrZero(falloff);

        if (LightTextureOfFalloff.TryGetValue(falloff, out ImageTexture? shared))
        {
            return Live(shared, $"the light texture of the falloff {falloff}");
        }

        ImageTexture built = BuildTexture(falloff);
        LightTextureOfFalloff.Add(falloff, built);
        return built;
    }

    /// <summary>
    /// Gives the round texture of a glow halo of one power: built on the first call, and shared by
    /// each later call, because the texture never changes (D-1095, G-14).
    /// </summary>
    /// <param name="power">The power of the curve, above 1, such as <see cref="GlowPass.HaloPower"/>.</param>
    /// <returns>The texture, with an alpha of half floats.</returns>
    /// <exception cref="ArgumentOutOfRangeException">The power is not above 1 (T-2).</exception>
    /// <exception cref="InvalidOperationException">Godot made no texture from the image, or freed the shared texture (T-2, F-45).</exception>
    /// <remarks>Each build took 3.1 ms on an Apple silicon Mac, and each map built one.</remarks>
    public static ImageTexture HaloTexture(float power)
    {
        if (HaloTextureOfPower.TryGetValue(power, out ImageTexture? shared))
        {
            return Live(shared, $"the halo texture of the power {power}");
        }

        ImageTexture built = BuildHaloTexture(power);
        HaloTextureOfPower.Add(power, built);
        return built;
    }

    /// <summary>
    /// Gives the light of a glow halo at one distance from its middle, as a part of the light of
    /// its middle (D-1092, D-1095): one less the square of the distance, to a power. The curve
    /// falls slowly near the middle, and it meets 0 at the edge with no slope, so the edge shows
    /// no ring.
    /// </summary>
    /// <param name="distance">The distance from the middle, where 1 is the edge of the circle.</param>
    /// <param name="power">The power of the curve, such as <see cref="GlowPass.HaloPower"/>. A power above 1 meets the edge with no slope.</param>
    /// <returns>The part of the light of the middle, from 0 to 1.</returns>
    /// <exception cref="ArgumentOutOfRangeException">The distance is below 0, or the power is not above 1 (T-2).</exception>
    /// <remarks>
    /// The curve of D-1092 fell to 1% at the edge and then stopped. The world adds light in linear
    /// light, and the sRGB curve of the screen shows 1% of linear light near 10% of the brightness,
    /// so that stop drew a faint ring on a dark wall.
    /// </remarks>
    public static float HaloShareAt(float distance, float power)
    {
        ArgumentOutOfRangeException.ThrowIfNegative(distance);
        if (power <= 1f)
        {
            throw new ArgumentOutOfRangeException(nameof(power), power, "The curve of a halo takes a power above 1, so it meets its edge with no slope (D-1095).");
        }

        return distance >= 1f ? 0f : MathF.Pow(1f - (distance * distance), power);
    }

    /// <summary>Builds the round texture of a glow halo, with the curve of <see cref="HaloShareAt"/> (D-1092, D-1095).</summary>
    /// <param name="power">The power of the curve, above 1.</param>
    /// <returns>The texture, with an alpha of half floats.</returns>
    /// <exception cref="ArgumentOutOfRangeException">The power is not above 1 (T-2).</exception>
    /// <remarks>
    /// An alpha of 8 bits steps by 1/255, and near the edge each step shows as a band on the screen,
    /// because the sRGB curve lifts the faint end of linear light. Half floats hold that end (D-1095).
    /// </remarks>
    /// <exception cref="InvalidOperationException">Godot made no texture from the image (T-2, F-45).</exception>
    private static ImageTexture BuildHaloTexture(float power)
    {
        long start = Stopwatch.GetTimestamp();
        HaloTextureBuilds += 1;
        var picture = Image.CreateEmpty(TextureSize, TextureSize, false, Image.Format.Rgbah);
        float half = TextureSize / 2f;
        for (int y = 0; y < TextureSize; y += 1)
        {
            for (int x = 0; x < TextureSize; x += 1)
            {
                float dx = (x + 0.5f - half) / half;
                float dy = (y + 0.5f - half) / half;
                picture.SetPixel(x, y, new Color(1f, 1f, 1f, HaloShareAt(MathF.Sqrt((dx * dx) + (dy * dy)), power)));
            }
        }

        // The call reports a failure in the log alone, so the result takes a check (F-45, T-2).
        ImageTexture? texture = ImageTexture.CreateFromImage(picture);
        TextureBuildTime += Stopwatch.GetElapsedTime(start);
        return texture ?? throw new InvalidOperationException(
            "Godot made no halo texture from the image, and a halo with no texture draws nothing (T-2, F-46).");
    }

    /// <summary>Builds a round texture: full light at the center, and none at the edge of the circle.</summary>
    /// <param name="falloff">The power of the fall with the distance to the edge, such as <see cref="LightFalloff"/>.</param>
    /// <returns>The texture.</returns>
    /// <exception cref="InvalidOperationException">Godot made no texture from the image (T-2, F-45).</exception>
    private static ImageTexture BuildTexture(float falloff)
    {
        long start = Stopwatch.GetTimestamp();
        LightTextureBuilds += 1;
        var picture = Image.CreateEmpty(TextureSize, TextureSize, false, Image.Format.Rgba8);
        float half = TextureSize / 2f;
        for (int y = 0; y < TextureSize; y += 1)
        {
            for (int x = 0; x < TextureSize; x += 1)
            {
                float dx = (x + 0.5f - half) / half;
                float dy = (y + 0.5f - half) / half;
                float edge = Math.Max(0f, 1f - MathF.Sqrt((dx * dx) + (dy * dy)));
                picture.SetPixel(x, y, new Color(1f, 1f, 1f, MathF.Pow(edge, falloff)));
            }
        }

        // The call reports a failure in the log alone, so the result takes a check (F-45, T-2).
        ImageTexture? texture = ImageTexture.CreateFromImage(picture);
        TextureBuildTime += Stopwatch.GetElapsedTime(start);
        return texture ?? throw new InvalidOperationException(
            "Godot made no light texture from the image, and a light with no texture draws nothing (T-2, F-46).");
    }

    /// <summary>Gives a shared texture, and fails when Godot freed it, because a light with a freed texture draws nothing (T-2, F-46).</summary>
    private static ImageTexture Live(ImageTexture shared, string what)
    {
        return GodotObject.IsInstanceValid(shared)
            ? shared
            : throw new InvalidOperationException($"Godot freed {what}, and each light shares it for the whole process (T-2, F-46, G-14).");
    }

    /// <summary>Gives the color of a palette key, at full strength (D-846).</summary>
    /// <param name="palette">The palette.</param>
    /// <param name="color">The key and the strength of the light.</param>
    /// <returns>The palette color.</returns>
    /// <exception cref="ContentException">The palette holds no such key (T-2).</exception>
    public static Color PaletteColorOf(Palette palette, LightColor color)
    {
        ArgumentNullException.ThrowIfNull(palette);
        ArgumentNullException.ThrowIfNull(color);

        if (!palette.TryColorOf(color.Key, out PaletteColor? found))
        {
            throw ContentException.ForField(Palette.Path, "colors", $"the palette holds no key '{color.Key}' that a light names (D-846)");
        }

        return Color.Color8((byte)found.Red, (byte)found.Green, (byte)found.Blue);
    }

    /// <summary>Builds the ambient light: the one canvas modulate of the world (D-846).</summary>
    /// <param name="palette">The palette.</param>
    /// <param name="ambient">The key and the strength of the ambient light.</param>
    /// <returns>The node, which tints each node of its canvas.</returns>
    /// <exception cref="ContentException">The palette holds no such key (T-2).</exception>
    /// <remarks>Godot allows one canvas modulate for each canvas, so each screen of the world builds one (`area-effects.md` section 7.6).</remarks>
    public static CanvasModulate Ambient(Palette palette, LightColor ambient)
    {
        Color full = PaletteColorOf(palette, ambient);
        float strength = ambient.Strength / (float)BasisPoints.One;
        return new CanvasModulate
        {
            Name = "Ambient",
            Color = new Color(full.R * strength, full.G * strength, full.B * strength),

            // The overlay above the glow draws the same world, so the ambient light reaches it too (D-916).
            VisibilityLayer = 1u | GlowPass.AboveGlowLayer,
        };
    }

    /// <summary>Builds one point light (D-183, F-46).</summary>
    /// <param name="id">The id of the piece or the light, which the node takes as its name for each error.</param>
    /// <param name="palette">The palette.</param>
    /// <param name="values">The color, the range, and the height of the light.</param>
    /// <param name="texture">The light texture of <see cref="LightTexture"/>.</param>
    /// <param name="items">The light mask of the items that the light lights.</param>
    /// <param name="shadows">The occluder mask of the shadows that the light takes, or 0 for no shadow.</param>
    /// <returns>The light.</returns>
    /// <exception cref="ContentException">The palette holds no such key (T-2).</exception>
    public static PointLight2D Point(string id, Palette palette, PointLightValues values, Texture2D texture, int items, int shadows)
    {
        ArgumentException.ThrowIfNullOrEmpty(id);
        ArgumentNullException.ThrowIfNull(values);
        ArgumentNullException.ThrowIfNull(texture);

        return new PointLight2D
        {
            Name = id,
            Texture = texture,
            TextureScale = 2f * values.Range / TextureSize,
            Color = PaletteColorOf(palette, values.Color),
            Energy = values.Color.Strength / (float)BasisPoints.One,
            Height = values.Height,
            RangeItemCullMask = items,
            ShadowEnabled = shadows != 0,
            ShadowItemCullMask = shadows,

            // Hard shadows from walls (D-183).
            ShadowFilter = Light2D.ShadowFilterEnum.None,

            // The overlay above the glow draws the same world, so a lit burst or mark takes the light (D-916).
            VisibilityLayer = 1u | GlowPass.AboveGlowLayer,
        };
    }

    /// <summary>
    /// Builds the pair of Godot lights of one source of a map (D-853). The ground light lights
    /// the floor and the walls, and it takes the shadows that the ground mask names. The figure
    /// light lights the figures, and it takes the shadows of the walls alone, so no figure
    /// darkens itself.
    /// </summary>
    /// <param name="id">The id of the piece or the light.</param>
    /// <param name="palette">The palette.</param>
    /// <param name="values">The color, the range, and the height of the source.</param>
    /// <param name="texture">The light texture of <see cref="LightTexture"/>.</param>
    /// <param name="groundShadows">The occluder mask of the shadows that the ground light takes.</param>
    /// <returns>The two lights.</returns>
    /// <exception cref="ContentException">The palette holds no such key (T-2).</exception>
    public static (PointLight2D Ground, PointLight2D Figures) Pair(
        string id,
        Palette palette,
        PointLightValues values,
        Texture2D texture,
        int groundShadows)
    {
        return (
            Point($"{id}_ground", palette, values, texture, GroundItems, groundShadows),
            Point($"{id}_figures", palette, values, texture, FigureItems, WallShadows));
    }

    /// <summary>
    /// Reads each point light under one node back, and fails on a light that draws nothing:
    /// a light with no texture, or a light at height 0 (F-46). A headless session draws nothing,
    /// so this check reads the nodes and never the pixels (F-23).
    /// </summary>
    /// <param name="root">The node that holds the lights.</param>
    /// <returns>The count of lights.</returns>
    /// <exception cref="InvalidOperationException">A light has no texture or no height, and the error names it (T-2).</exception>
    public static int CheckLights(Node root)
    {
        ArgumentNullException.ThrowIfNull(root);

        int count = 0;
        foreach (Node child in root.GetChildren())
        {
            if (child is PointLight2D light)
            {
                if (light.Texture is null)
                {
                    throw new InvalidOperationException(
                        $"The light '{light.Name}' has no texture, and Godot draws no light with none (T-2, F-46).");
                }

                if (light.Height <= 0f)
                {
                    throw new InvalidOperationException(
                        $"The light '{light.Name}' sits at height {light.Height}, and it gives no light to a flat pixel (T-2, F-46).");
                }

                count += 1;
            }

            count += CheckLights(child);
        }

        return count;
    }
}
