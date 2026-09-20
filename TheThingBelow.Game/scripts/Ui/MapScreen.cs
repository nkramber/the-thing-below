using System;
using Godot;
using TheThingBelow.Core.Content;
using TheThingBelow.Core.Maps;

namespace TheThingBelow.Game.Ui;

/// <summary>
/// The map on screen: the tiles of the map, the sprite of the lead, and the view that
/// follows it (D-106, D-203, D-667).
/// </summary>
/// <remarks>
/// The node draws the state that Core stepped, and it holds no rule of its own (D-100). The
/// lead slides between two tiles across the ticks of the step, and Core keeps the lead on a
/// whole tile (D-203).
/// <para>
/// The view moves from the tick and never from the smoothing of a Godot camera, because that
/// smoothing can run more than once in a frame (F-52). Thus this node sets its own position
/// inside the world viewport, and no `Camera2D` exists (D-717).
/// </para>
/// <para>
/// A layer turns off its collisions and its navigation, because no rule of Core reads them
/// and both default to on (F-51, G-1, G-23).
/// </para>
/// </remarks>
public partial class MapScreen : Node2D
{
    /// <summary>The content id of the character that walks the map (D-267, D-306).</summary>
    /// <remarks>
    /// One sprite walks the map, and it belongs to the lead in the party or in reserve
    /// (D-292, D-306). PR-11 gives the run its party, and the sprite then follows the lead
    /// of that party.
    /// </remarks>
    public const string LeadContentId = "cast.marrek";

    /// <summary>The use that the map drawing of a character serves (D-519).</summary>
    public const string MapUse = "map_front";

    private TileMapLayer ground = null!;
    private Sprite2D lead = null!;

    /// <summary>Builds the tiles of one map and the sprite of the lead.</summary>
    /// <param name="atlas">The pages of the atlas, as textures (D-666).</param>
    /// <param name="map">The map that the party stands on (D-528).</param>
    /// <exception cref="ArgumentNullException">An argument is null (T-2).</exception>
    /// <exception cref="ContentException">The atlas holds no drawing of a tile or of the lead (T-2).</exception>
    /// <exception cref="InvalidOperationException">A call of the engine made no tile (T-2, F-45).</exception>
    public void Build(GameAtlas atlas, GameMap map)
    {
        ArgumentNullException.ThrowIfNull(atlas);
        ArgumentNullException.ThrowIfNull(map);

        // Godot sorts each canvas item by one Y value, so a character draws in front of what
        // stands behind it (D-206, the external facts of `area-exploration.md`).
        this.YSortEnabled = true;

        this.ground = BuildGround(atlas, map);
        this.AddChild(this.ground);

        AtlasEntry entry = atlas.Index.Entry(
            ContentId.Parse(LeadContentId, AtlasIndex.Path, nameof(LeadContentId)),
            MapUse);
        this.lead = new Sprite2D
        {
            Texture = atlas.Frame(entry.Id, 0),
            Centered = false,
        };
        this.AddChild(this.lead);
    }

    /// <summary>Puts the party where Core put it, and moves the view (D-203, D-717).</summary>
    /// <param name="party">The party on its map, at the end of the last tick.</param>
    /// <exception cref="ArgumentNullException">The party is null (T-2).</exception>
    /// <remarks>
    /// Every value is a whole art pixel of the world viewport, so no sprite draws between
    /// two pixels and no Godot snap setting is on (D-715).
    /// </remarks>
    public void ShowParty(MapState party)
    {
        ArgumentNullException.ThrowIfNull(party);

        int leadX = MapCamera.LeadX(party);
        int leadY = MapCamera.LeadY(party);
        this.lead.Position = new Vector2(leadX, leadY);

        CameraPlace view = MapCamera.Of(party, FrameRoot.WorldWidth, FrameRoot.WorldHeight);
        this.Position = new Vector2(-view.X, -view.Y);
    }

    private static TileMapLayer BuildGround(GameAtlas atlas, GameMap map)
    {
        var layer = new TileMapLayer
        {
            TileSet = MapTileSet.Build(atlas),

            // Four Godot defaults meet a tile map, and two of them are these switches
            // (F-51). No rule of Core reads a collision or a navigation mesh (G-1, G-23).
            CollisionEnabled = false,
            NavigationEnabled = false,
            YSortEnabled = true,
        };

        for (int row = 0; row < map.Height; row += 1)
        {
            for (int column = 0; column < map.Width; column += 1)
            {
                TileKind kind = map.TileAt(new TilePoint(column, row));
                layer.SetCell(
                    new Vector2I(column, row),
                    MapTileSet.SourceId,
                    MapTileSet.CellOf(atlas, kind));
            }
        }

        return layer;
    }
}
