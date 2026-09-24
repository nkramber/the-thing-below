using System;
using System.Collections.Generic;
using Godot;
using TheThingBelow.Core.Content;
using TheThingBelow.Core.Maps;

namespace TheThingBelow.Game.Ui;

/// <summary>
/// The dungeon map screen: each tile that the party walked at 16 frame pixels, with the doors,
/// the save points, and the party on it (D-567, D-982, D-993). The map action opens it from the
/// walk, and back or the map action closes it (D-986).
/// </summary>
/// <remarks>
/// The screen draws the map into one image when it opens, because a menu pauses the world and
/// no tile changes while it is open (D-162). The image takes the Nearest filter, so each tile
/// keeps its edges at every fit (F-45). The title is the name of the map, and a key under the
/// map names each mark. PR-16 adds the exit and its mark (D-993).
/// </remarks>
public sealed class DungeonMapView : IMenuView
{
    /// <summary>The frame pixels of the inset square of a door, a save point, or the party inside its tile.</summary>
    private const int MarkInset = 3;

    private readonly Control layer;

    /// <summary>Builds the dungeon map screen over the frame.</summary>
    /// <param name="frame">The frame, whose UI layer takes the screen.</param>
    /// <param name="ui">The atlas, the theme, and the text helper.</param>
    /// <param name="party">The party on its map, with the walked tiles.</param>
    /// <exception cref="ArgumentNullException">An argument is null (T-2).</exception>
    /// <exception cref="ArgumentException">The map is larger than 80 by 45 tiles (D-982, T-2).</exception>
    /// <exception cref="InvalidOperationException">Godot made no texture of the map image (T-2, F-45).</exception>
    public DungeonMapView(FrameRoot frame, UiBase ui, MapState party)
    {
        ArgumentNullException.ThrowIfNull(frame);
        ArgumentNullException.ThrowIfNull(ui);
        ArgumentNullException.ThrowIfNull(party);

        DungeonMapLayout layout = DungeonMapLayout.Of(party);
        this.layer = MenuNodes.Layer(frame, ui);

        var box = new FrameBox(
            UiMetrics.EdgePixels,
            UiMetrics.EdgePixels,
            ScreenFit.FrameWidth - (UiMetrics.EdgePixels * 2),
            ScreenFit.FrameHeight - (UiMetrics.EdgePixels * 2));
        MenuNodes.Panel(this.layer, box);
        MenuNodes.Title(this.layer, ui, box, party.Map.Label);

        var colors = new Dictionary<DungeonMapMark, Color>
        {
            [DungeonMapMark.Floor] = ui.Theme.ColorOf("map_floor"),
            [DungeonMapMark.Door] = ui.Theme.ColorOf("map_door"),
            [DungeonMapMark.SavePoint] = ui.Theme.ColorOf("map_save"),
            [DungeonMapMark.Party] = ui.Theme.ColorOf("map_party"),
        };

        var picture = new TextureRect
        {
            TextureFilter = CanvasItem.TextureFilterEnum.Nearest,
            MouseFilter = Control.MouseFilterEnum.Ignore,
        };

        // A texture rect sets its expand mode before its size (F-105).
        picture.ExpandMode = TextureRect.ExpandModeEnum.IgnoreSize;
        picture.Position = new Vector2(layout.Left, layout.Top);
        picture.Size = new Vector2(layout.Width, layout.Height);
        picture.Texture = TextureOf(layout, party.Map, colors);
        this.layer.AddChild(picture);

        this.BuildKey(ui, box, colors);
    }

    /// <inheritdoc/>
    public ViewOutcome Read(InputEvent signal, ScreenFit fit)
    {
        ArgumentNullException.ThrowIfNull(signal);
        ArgumentNullException.ThrowIfNull(fit);

        return signal.IsActionPressed("ui_cancel") ? ViewOutcome.Back : ViewOutcome.Stay;
    }

    /// <inheritdoc/>
    public void Show()
    {
        // The world waits while the screen is open, so the picture of the open stays true (D-162).
    }

    /// <inheritdoc/>
    public void Free() => this.layer.QueueFree();

    /// <summary>Draws each tile of the map into one image: a walked tile fills its square, and a mark fills a smaller square inside it.</summary>
    private static ImageTexture TextureOf(DungeonMapLayout layout, GameMap map, Dictionary<DungeonMapMark, Color> colors)
    {
        Image image = Image.CreateEmpty(layout.Width, layout.Height, false, Image.Format.Rgba8);
        int tile = DungeonMapLayout.TilePixels;
        for (int row = 0; row < map.Height; row += 1)
        {
            for (int column = 0; column < map.Width; column += 1)
            {
                DungeonMapMark mark = layout.MarkAt(new TilePoint(column, row));
                if (mark == DungeonMapMark.None)
                {
                    continue;
                }

                image.FillRect(new Rect2I(column * tile, row * tile, tile, tile), colors[DungeonMapMark.Floor]);
                if (mark != DungeonMapMark.Floor)
                {
                    image.FillRect(
                        new Rect2I((column * tile) + MarkInset, (row * tile) + MarkInset, tile - (MarkInset * 2), tile - (MarkInset * 2)),
                        colors[mark]);
                }
            }
        }

        // Godot reports a failed texture in the log alone, so the check comes right after (F-45, T-2).
        return ImageTexture.CreateFromImage(image) ?? throw new InvalidOperationException(
            $"Godot made no texture of the dungeon map image of '{map.Id.Value}', {layout.Width} by {layout.Height} pixels (F-45, T-2).");
    }

    /// <summary>Adds the key under the map: one square and one word for the party, a door, and a save point.</summary>
    private void BuildKey(UiBase ui, FrameBox box, Dictionary<DungeonMapMark, Color> colors)
    {
        (DungeonMapMark Mark, string Id)[] entries =
        [
            (DungeonMapMark.Party, "menu.map_you"),
            (DungeonMapMark.Door, "menu.map_door"),
            (DungeonMapMark.SavePoint, "menu.map_waystone"),
        ];

        int body = ui.Theme.BodySize;
        int line = MenuLayout.LineOf(body);
        int top = box.Y + box.Height - MenuLayout.Pad - line;
        int left = box.X + MenuLayout.Pad;
        int tile = DungeonMapLayout.TilePixels;
        foreach ((DungeonMapMark mark, string id) in entries)
        {
            this.layer.AddChild(new ColorRect
            {
                Position = new Vector2(left, top + ((line - tile) / 2)),
                Size = new Vector2(tile, tile),
                Color = colors[mark],
                MouseFilter = Control.MouseFilterEnum.Ignore,
            });

            int width = UiMetrics.WidthOf(body, MenuLayout.KeyWordCharacters);
            Label word = MenuNodes.Line(this.layer, left + tile + (tile / 2), top, width, line);
            ui.Text.Put(word, ContentId.Parse(id, StringTable.Path, nameof(DungeonMapView)));
            left += tile + (tile / 2) + width;
        }
    }
}
