using System;
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

    /// <summary>Builds the light texture: full light at the center, and none at the edge of the circle.</summary>
    /// <returns>The texture.</returns>
    /// <exception cref="InvalidOperationException">Godot made no texture from the image (T-2, F-45).</exception>
    /// <remarks>
    /// The light falls with the distance to its edge to the power 1.5, so a pool of light has a
    /// soft edge and a bright middle, as the HD-2D look of D-849 asks.
    /// </remarks>
    public static ImageTexture BuildTexture()
    {
        var picture = Image.CreateEmpty(TextureSize, TextureSize, false, Image.Format.Rgba8);
        float half = TextureSize / 2f;
        for (int y = 0; y < TextureSize; y += 1)
        {
            for (int x = 0; x < TextureSize; x += 1)
            {
                float dx = (x + 0.5f - half) / half;
                float dy = (y + 0.5f - half) / half;
                float edge = Math.Max(0f, 1f - MathF.Sqrt((dx * dx) + (dy * dy)));
                picture.SetPixel(x, y, new Color(1f, 1f, 1f, MathF.Pow(edge, 1.5f)));
            }
        }

        // The call reports a failure in the log alone, so the result takes a check (F-45, T-2).
        ImageTexture? texture = ImageTexture.CreateFromImage(picture);
        return texture ?? throw new InvalidOperationException(
            "Godot made no light texture from the image, and a light with no texture draws nothing (T-2, F-46).");
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
        };
    }

    /// <summary>Builds one point light (D-183, F-46).</summary>
    /// <param name="id">The id of the piece or the light, which the node takes as its name for each error.</param>
    /// <param name="palette">The palette.</param>
    /// <param name="values">The color, the range, and the height of the light.</param>
    /// <param name="texture">The light texture of <see cref="BuildTexture"/>.</param>
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
    /// <param name="texture">The light texture of <see cref="BuildTexture"/>.</param>
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
