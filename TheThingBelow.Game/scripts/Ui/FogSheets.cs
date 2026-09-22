using System;
using System.Collections.Generic;
using Godot;
using TheThingBelow.Core;
using TheThingBelow.Core.Content;
using TheThingBelow.Core.Effects;

namespace TheThingBelow.Game.Ui;

/// <summary>
/// The fog of a weather: one sheet for each layer, tiled over the view and drifting (D-885,
/// D-887). Each sheet draws above the figures, and it counts as one full-screen pass (D-523).
/// </summary>
/// <remarks>
/// A sheet is an image of the text grid of its layer: each cell takes the color of the palette
/// key of the layer, at the strength of its band, and a clear cell takes nothing. The image
/// holds hard edges and one color, so the fog keeps the palette (G-27, D-181).
/// <para>
/// The sprite repeats the image over the view, and the region of the sprite moves by whole art
/// pixels, so no cell edge falls between two pixels (D-230).
/// </para>
/// </remarks>
public sealed class FogSheets
{
    private readonly List<FogSheet> sheets;

    private FogSheets(List<FogSheet> sheets) => this.sheets = sheets;

    /// <summary>The count of sheets: one for each layer of fog, which the budget counts as one pass each (D-523).</summary>
    public int SheetCount => this.sheets.Count;

    /// <summary>Builds one sheet for each layer of fog under a parent node.</summary>
    /// <param name="name">The name of each sheet, which every error names (T-2).</param>
    /// <param name="fogs">The layers, from the lowest.</param>
    /// <param name="palette">The palette, which gives the key of each layer its color (D-181).</param>
    /// <param name="lit">True when the scene light falls on the fog (D-183).</param>
    /// <param name="zIndex">The Z index of the lowest layer. Each later layer takes one more.</param>
    /// <param name="parent">The node that takes the sheets.</param>
    /// <returns>The sheets.</returns>
    /// <exception cref="ArgumentNullException">An argument is null (T-2).</exception>
    /// <exception cref="ContentException">The palette holds no key of a layer (T-2).</exception>
    /// <exception cref="InvalidOperationException">Godot made no texture from the image of a layer (T-2, F-45).</exception>
    public static FogSheets Build(
        string name,
        IReadOnlyList<FogLayer> fogs,
        Palette palette,
        bool lit,
        int zIndex,
        Node2D parent)
    {
        ArgumentException.ThrowIfNullOrEmpty(name);
        ArgumentNullException.ThrowIfNull(fogs);
        ArgumentNullException.ThrowIfNull(palette);
        ArgumentNullException.ThrowIfNull(parent);

        var sheets = new List<FogSheet>();
        for (int index = 0; index < fogs.Count; index += 1)
        {
            FogLayer layer = fogs[index];
            var sprite = new Sprite2D
            {
                Name = $"{name}_fog_{index}",
                Texture = TextureOf(name, layer, palette),
                Centered = false,
                RegionEnabled = true,
                TextureRepeat = CanvasItem.TextureRepeatEnum.Enabled,
                TextureFilter = CanvasItem.TextureFilterEnum.Nearest,
                ZIndex = zIndex + index,
            };

            if (!lit)
            {
                sprite.Material = new CanvasItemMaterial { LightMode = CanvasItemMaterial.LightModeEnum.Unshaded };
            }

            parent.AddChild(sprite);
            sheets.Add(new FogSheet(sprite, layer.DriftX, layer.DriftY));
        }

        return new FogSheets(sheets);
    }

    /// <summary>Puts each sheet over the view at one tick, with its drift.</summary>
    /// <param name="point">The north-west corner of the view, in art pixels of the parent.</param>
    /// <param name="width">The width of the view, in art pixels.</param>
    /// <param name="height">The height of the view, in art pixels.</param>
    /// <param name="tick">The tick of the run, from 0.</param>
    /// <exception cref="ArgumentOutOfRangeException">The tick is below zero, or the view is empty (T-2).</exception>
    public void Show(Vector2 point, int width, int height, long tick)
    {
        ArgumentOutOfRangeException.ThrowIfNegative(tick);
        ArgumentOutOfRangeException.ThrowIfNegativeOrZero(width);
        ArgumentOutOfRangeException.ThrowIfNegativeOrZero(height);

        foreach (FogSheet sheet in this.sheets)
        {
            // The drift is art pixels in each second of 60 ticks, and whole pixels alone (D-230).
            long driftX = sheet.DriftX * tick / HitBurst.StepsPerSecond;
            long driftY = sheet.DriftY * tick / HitBurst.StepsPerSecond;
            sheet.Sprite.Position = point;
            sheet.Sprite.RegionRect = new Rect2(driftX, driftY, width, height);
        }
    }

    private static ImageTexture TextureOf(string name, FogLayer layer, Palette palette)
    {
        if (!palette.TryColorOf(layer.Key, out PaletteColor? found))
        {
            throw ContentException.ForField(Palette.Path, "colors", $"the palette holds no key '{layer.Key}' of the fog '{name}' (D-181)");
        }

        int width = layer.Width * layer.CellSize;
        int height = layer.Height * layer.CellSize;
        var picture = Image.CreateEmpty(width, height, false, Image.Format.Rgba8);
        var clear = new Color(0, 0, 0, 0);
        for (int row = 0; row < height; row += 1)
        {
            for (int column = 0; column < width; column += 1)
            {
                int band = layer.BandAt(column / layer.CellSize, row / layer.CellSize);
                Color color = band == 0
                    ? clear
                    : new Color(
                        found.Red / 255f,
                        found.Green / 255f,
                        found.Blue / 255f,
                        layer.Bands[band - 1] / (float)BasisPoints.One);
                picture.SetPixel(column, row, color);
            }
        }

        // The call reports a failure in the log alone, so the result takes a check (F-45, T-2).
        ImageTexture? texture = ImageTexture.CreateFromImage(picture);
        return texture ?? throw new InvalidOperationException(
            $"Godot made no texture from the fog '{name}', and a sheet with no texture draws nothing (T-2, F-45).");
    }

    /// <summary>One sheet of fog, with the drift of its layer.</summary>
    private sealed record FogSheet(Sprite2D Sprite, int DriftX, int DriftY);
}
