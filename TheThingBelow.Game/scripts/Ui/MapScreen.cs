using System;
using System.Collections.Generic;
using Godot;
using TheThingBelow.Core.Content;
using TheThingBelow.Core.Effects;
using TheThingBelow.Core.Light;
using TheThingBelow.Core.Maps;

namespace TheThingBelow.Game.Ui;

/// <summary>
/// The map on screen: the tiles of the map, the sprite of the lead, the decor pieces, the
/// scene light, and the view that follows it (D-106, D-203, D-667, D-843).
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
/// <para>
/// On a map that is not dark, every live enemy draws at any distance from the party, so no
/// enemy pops in on the screen (D-814). On a dark map, an enemy draws inside the sight of the
/// party alone, and <see cref="SightFade"/> fades each crossing of its edge (D-1062). The sort
/// value of each enemy comes from the front row of its body, so the body draws in front of
/// what it stands before (D-737).
/// </para>
/// <para>
/// Every map sprite sits at the south edge of its front row, and its picture draws up from
/// there (F-94, D-737). The ground layer takes no part in the sort, and it draws below every
/// sprite. A tile that sorted by its cell drew over the feet of a sprite for half of each
/// step north and each step south, because the feet then sit inside a row of tiles (F-95,
/// D-783).
/// </para>
/// <para>
/// The light setup of the map at its time of day gives the ambient light, and each decor piece
/// gives its own light (D-442, D-843). Each wall casts the shadow of its full tile (D-845). The
/// carried light follows the drawn place of the lead on each frame, inside a step too, and no
/// rule reads it (D-847, G-1). The carried light and the torch in the hand draw while the party
/// holds the torch out (D-1064, D-1066).
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

    /// <summary>The use of the map drawing of the lead with the torch in its hand (D-1069).</summary>
    public const string TorchUse = "map_torch";

    /// <summary>The role of the color of the mark of a sight, in the UI style file (D-527).</summary>
    public const string MarkRole = "text_warning";

    /// <summary>The width of the bar and of the dot of the mark, in art pixels (D-208).</summary>
    private const int MarkWidth = 4;

    /// <summary>The height of the bar of the mark, in art pixels.</summary>
    private const int MarkBar = 10;

    /// <summary>The height of the dot of the mark, and the gap above it, in art pixels.</summary>
    private const int MarkDot = 4;

    /// <summary>The Z index of the ground layer, below every sprite and below the mark (F-95, D-783).</summary>
    private const int GroundZIndex = -1;

    /// <summary>The Z index of the mark of a sight: above each figure, each flame, and each layer of fog (D-208, D-885).</summary>
    private const int MarkZIndex = 8;

    /// <summary>The margin around the map and the view that each particle node holds, in art pixels: two tiles (F-98).</summary>
    private const int WeatherMargin = 2 * MapCamera.TilePixels;

    private TileMapLayer ground = null!;
    private Sprite2D lead = null!;
    private Texture2D leadPlain = null!;
    private Texture2D leadTorch = null!;
    private SightFade? fade;
    private int[] shares = [];
    private Sprite2D[] enemies = [];
    private Node2D mark = null!;
    private PointLight2D carriedGround = null!;
    private PointLight2D carriedFigures = null!;
    private CarriedLight carriedPlace = null!;
    private readonly List<TorchFlame> torches = [];
    private TorchFlame carriedFlame = null!;
    private AmbientLayer weather = null!;
    private ShaftPass? shafts;
    private Vector2 view;

    /// <summary>True for a capture, which seeks each stream to the tick of the frame (D-172).</summary>
    public bool SeekParticles { get; set; }

    /// <summary>
    /// Builds the tiles of one map, the sprite of the lead, one sprite for each enemy, and
    /// the mark of a sight (D-208, D-738).
    /// </summary>
    /// <param name="atlas">The pages of the atlas, as textures (D-666).</param>
    /// <param name="theme">The theme, for the color of the mark (D-527).</param>
    /// <param name="party">The party and the enemies on the map (D-528, D-738).</param>
    /// <param name="content">The content set, for the palette, the decor, and the light setup (D-843).</param>
    /// <param name="ambient">The weather of the map, or no value for a map with no weather (D-202, D-889).</param>
    /// <param name="passes">The passes of the HD-2D look, in the mode that the screen shows, for the mode of the light shafts (D-917).</param>
    /// <exception cref="ArgumentNullException">An argument is null (T-2).</exception>
    /// <exception cref="ContentException">
    /// The atlas holds no drawing of a tile, of the lead, of an enemy, or of a decor piece, or
    /// the map has no light setup for its time of day (T-2).
    /// </exception>
    /// <exception cref="InvalidOperationException">A call of the engine made no tile (T-2, F-45).</exception>
    /// <remarks>
    /// The enemies of the map never change while the party stands on it, because the time of
    /// day picks each station at the start of the run (D-743). Thus one sprite serves one
    /// enemy for the whole visit.
    /// </remarks>
    public void Build(GameAtlas atlas, UiTheme theme, MapState party, ContentSet content, AmbientEffect? ambient, Hd2dPasses passes)
    {
        ArgumentNullException.ThrowIfNull(atlas);
        ArgumentNullException.ThrowIfNull(theme);
        ArgumentNullException.ThrowIfNull(party);
        ArgumentNullException.ThrowIfNull(content);
        ArgumentNullException.ThrowIfNull(passes);

        // Godot sorts each canvas item by one Y value, so a character draws in front of what
        // stands behind it (D-206, the external facts of `area-exploration.md`).
        this.YSortEnabled = true;

        this.ground = BuildGround(atlas, party.Map);
        this.AddChild(this.ground);

        ContentId leadId = ContentId.Parse(LeadContentId, AtlasIndex.Path, nameof(LeadContentId));
        this.lead = BuildSprite(atlas, leadId);
        this.leadPlain = this.lead.Texture;
        this.leadTorch = TorchTexture(atlas, leadId);
        this.lead.AddChild(FeetShadow(this.lead, WorldLights.LeadShadows));
        this.AddChild(this.lead);

        IReadOnlyList<PatrolState> patrols = party.Patrols.All;
        this.enemies = new Sprite2D[patrols.Count];
        this.shares = new int[patrols.Count];
        this.fade = null;
        for (int index = 0; index < patrols.Count; index += 1)
        {
            this.enemies[index] = BuildSprite(atlas, patrols[index].Patrol.Id);
            this.enemies[index].AddChild(FeetShadow(this.enemies[index], WorldLights.FigureShadows));
            this.AddChild(this.enemies[index]);
        }

        this.mark = BuildMark(theme);
        this.AddChild(this.mark);

        // The mark draws above the fog, the glow, and the passes, so fog never hides it and it stays sharp (D-208, D-916, D-919).
        GlowPass.LiftToMarks(this.mark);

        this.BuildLight(atlas, party.Map, content);
        this.weather = AmbientLayer.Build(ambient, content.Palette, this);

        // A map with no light shaft draws no shaft pass, so the budget counts none (D-523, D-918).
        this.shafts = ShaftPass.Build(content.Light.DecorOf(party.Map.Id), content.Light, content.Palette, passes, this);
    }

    /// <summary>Puts the party where Core put it, and moves the view (D-203, D-717).</summary>
    /// <param name="party">The party on its map, at the end of the last tick.</param>
    /// <param name="tickPart">The part of the next tick that the frame reached, from 0 to 999 (D-820).</param>
    /// <param name="tick">The tick of the run, which each fade of the dark counts (D-1062).</param>
    /// <param name="torchHeld">True while the party holds the torch out (D-1064).</param>
    /// <exception cref="ArgumentNullException">The party is null (T-2).</exception>
    /// <exception cref="ArgumentOutOfRangeException">The tick is below zero (T-2).</exception>
    /// <remarks>
    /// Every value is a whole art pixel of the world viewport, so no sprite draws between
    /// two pixels and no Godot snap setting is on (D-715).
    /// </remarks>
    public void ShowParty(MapState party, int tickPart, long tick, bool torchHeld)
    {
        ArgumentNullException.ThrowIfNull(party);
        ArgumentOutOfRangeException.ThrowIfNegative(tick);

        int leadX = MapCamera.LeadX(party, tickPart);
        int leadY = MapCamera.LeadY(party, tickPart);
        this.lead.Position = new Vector2(leadX, FeetOf(leadY, 1));
        this.lead.Texture = torchHeld ? this.leadTorch : this.leadPlain;
        this.ShowCarriedLight(torchHeld);
        var carriedAt = new Vector2(leadX + this.carriedPlace.X, FeetOf(leadY, 1) + this.carriedPlace.Y);
        this.carriedGround.Position = carriedAt;
        this.carriedFigures.Position = carriedAt;
        this.carriedFlame.MoveTo(carriedAt);
        this.ShowEnemies(party, tickPart, tick, torchHeld);

        CameraPlace view = MapCamera.Of(party, FrameRoot.WorldWidth, FrameRoot.WorldHeight, tickPart);
        this.view = new Vector2(view.X, view.Y);
        this.Position = new Vector2(-view.X, -view.Y);
    }

    /// <summary>
    /// Shows the fire of each torch and the weather of the map at one tick (D-890, D-891). The
    /// caller draws the party first, because the weather follows the view (D-187).
    /// </summary>
    /// <param name="tick">The tick of the run, from 0.</param>
    /// <param name="seek">True for a capture, which seeks each stream to the tick (D-172). A frame of play runs the streams on the engine.</param>
    /// <exception cref="ArgumentOutOfRangeException">The tick is below zero (T-2).</exception>
    public void ShowWeather(long tick, bool seek)
    {
        ArgumentOutOfRangeException.ThrowIfNegative(tick);

        foreach (TorchFlame torch in this.torches)
        {
            torch.Show(tick, seek);
        }

        this.carriedFlame.Show(tick, seek);
        this.weather.Show(this.view, FrameRoot.WorldWidth, FrameRoot.WorldHeight, tick);
        if (this.shafts is not null)
        {
            this.shafts.Show(this.view, FrameRoot.WorldWidth, FrameRoot.WorldHeight);
        }
    }

    /// <summary>True while the carried light draws, which follows the torch held out (D-847, D-1064).</summary>
    public bool CarriedLightOn => this.carriedGround.Visible;

    /// <summary>True while the lead draws with the torch in its hand (D-1066, D-1069).</summary>
    public bool LeadHoldsTorch => this.lead.Texture == this.leadTorch;

    /// <summary>True when an enemy draws with a share between none and full, inside a fade of the dark (D-1062).</summary>
    public bool ShowsAFade
    {
        get
        {
            foreach (int share in this.shares)
            {
                if (share > 0 && share < SightFade.Full)
                {
                    return true;
                }
            }

            return false;
        }
    }

    /// <summary>
    /// Reads each light of the map back, and fails on a light that draws nothing (F-46). A
    /// headless session draws nothing, so this check reads the nodes and never the pixels (F-23).
    /// </summary>
    /// <returns>The count of lights, the carried light included.</returns>
    /// <exception cref="InvalidOperationException">A light has no texture or no height (T-2, F-46).</exception>
    public string DescribeLights()
    {
        int lights = WorldLights.CheckLights(this);
        string carried = this.CarriedLightOn ? "on" : "off";
        return $"{lights} lights with the carried light {carried}, and {CountOccluders(this)} occluders";
    }

    /// <summary>Tells whether every torch of the map holds its light at its place (D-891, F-99).</summary>
    public bool TorchLightsHoldTheirPlaces
    {
        get
        {
            bool held = this.carriedFlame.LightHoldsItsPlace;
            foreach (TorchFlame torch in this.torches)
            {
                held = held && torch.LightHoldsItsPlace;
            }

            return held;
        }
    }

    /// <summary>Describes the weather of the map and the fire of each torch, for the smoke session (D-187, D-890).</summary>
    /// <returns>The kind of the weather, its motes, its layers of fog, and the nodes of the torches.</returns>
    public string DescribeWeather()
    {
        int flames = this.carriedFlame.NodeCount;
        foreach (TorchFlame torch in this.torches)
        {
            flames += torch.NodeCount;
        }

        bool still = this.carriedFlame.NodesStandStill;
        foreach (TorchFlame torch in this.torches)
        {
            still = still && torch.NodesStandStill;
        }

        if (!still)
        {
            throw new InvalidOperationException(
                "A particle node of the map moved from its parent, and each live particle then rides the view (T-2, F-97).");
        }

        // Godot stops a particle system whose region leaves the screen, so each node holds the
        // whole map. A node with a smaller region goes out as the view moves (F-98).
        var seen = new Rect2(this.view, new Vector2(FrameRoot.WorldWidth, FrameRoot.WorldHeight));
        bool holds = this.carriedFlame.NodesHold(seen);
        float dimmest = this.carriedFlame.Energy;
        foreach (TorchFlame torch in this.torches)
        {
            holds = holds && torch.NodesHold(seen);
            dimmest = Math.Min(dimmest, torch.Energy);
        }

        if (!holds)
        {
            throw new InvalidOperationException(
                $"A particle node of the map holds a region that leaves the view {seen}, and Godot then stops it (T-2, F-98).");
        }

        if (dimmest <= 0f)
        {
            throw new InvalidOperationException(
                $"A torch of the map gives the energy {dimmest}, and every torch burns at each tick (T-2, D-891).");
        }

        string kind = this.weather.Effect is null ? "no weather" : AmbientEffect.NameOf(this.weather.Effect.Kind);
        return $"the weather is {kind} with {this.weather.NodeCount} motes and {this.weather.FogCount} fog layers, "
            + $"and {this.torches.Count} torches with {flames} nodes, the dimmest at {dimmest:0.00} energy";
    }

    /// <summary>
    /// Builds the scene light of the map: the ambient light, each decor piece and its light,
    /// each added light, and the carried light (D-843, D-844, D-847).
    /// </summary>
    private void BuildLight(GameAtlas atlas, GameMap map, ContentSet content)
    {
        LightSetup setup = content.Light.SetupOf(map.Id, map.Time);
        this.AddChild(WorldLights.Ambient(content.Palette, setup.Ambient));

        DecorFile pieces = content.Light.DecorOf(map.Id);
        foreach (DecorPiece piece in pieces.Pieces)
        {
            this.AddChild(PieceSprite(atlas, piece));
        }

        // Each light shaft falls from an opening on its wall, so the map draws the opening too (D-924).
        foreach (DecorPiece shaft in pieces.Shafts)
        {
            this.AddChild(PieceSprite(atlas, shaft));
        }

        foreach (WallShadow wall in WallShadows.Of(map))
        {
            this.AddChild(WallOccluder(wall));
        }

        ImageTexture texture = WorldLights.LightTexture(WorldLights.LightFalloff);
        ImageTexture halo = WorldLights.HaloTexture(GlowPass.HaloPower);
        const int EveryShadow = WorldLights.WallShadows | WorldLights.FigureShadows | WorldLights.LeadShadows;
        DecorFile decor = content.Light.DecorOf(map.Id);
        foreach (MapLight light in content.Light.LightsOf(map.Id, map.Time))
        {
            (PointLight2D ground, PointLight2D figures) = WorldLights.Pair(light.Id.Value, content.Palette, light.Light, texture, EveryShadow);
            ground.Position = new Vector2(light.X, light.Y);
            figures.Position = ground.Position;
            this.AddChild(ground);
            this.AddChild(figures);

            // A light of a decor piece is a fire, and an added light of the setup is not (D-843, D-888).
            if (FireOf(content, decor, light.Id) is TorchFire fire)
            {
                TorchFlame flame = TorchFlame.Build(light.Id.Value, fire, (ground, figures), content.Palette, (content.Light.Glow, halo), WeatherArea(map), this);
                flame.MoveTo(ground.Position);
                this.torches.Add(flame);
            }
        }

        // The lead holds the carried light, so its own feet never shadow it (D-853).
        this.carriedPlace = content.Light.Carried;
        (this.carriedGround, this.carriedFigures) = WorldLights.Pair(
            "carried_light",
            content.Palette,
            this.carriedPlace.Light,
            texture,
            WorldLights.WallShadows | WorldLights.FigureShadows);
        this.AddChild(this.carriedGround);
        this.AddChild(this.carriedFigures);
        this.carriedFlame = TorchFlame.Build(
            CarriedLight.Name,
            this.carriedPlace.Fire,
            (this.carriedGround, this.carriedFigures),
            content.Palette,
            (content.Light.Glow, halo),
            WeatherArea(map),
            this);
        this.ShowCarriedLight(false);
    }

    /// <summary>Turns the carried light on or off, with the torch held out or put away (D-847, D-1064).</summary>
    private void ShowCarriedLight(bool on)
    {
        this.carriedGround.Visible = on;
        this.carriedFigures.Visible = on;
        this.carriedFlame.Visible = on;
    }

    /// <summary>
    /// Gives the region that each particle node of the map holds: the whole map, one view on
    /// each side, and a margin (F-98). Godot stops a particle system whose region leaves the
    /// screen, and each node stands at the north-west corner of the map.
    /// </summary>
    /// <remarks>
    /// A map smaller than the view sits in the middle of it, so the view starts at a negative
    /// pixel (<see cref="MapCamera.AxisOf"/>). The region holds one view on each side, so it
    /// holds the view of every map, however small.
    /// </remarks>
    private static Rect2 WeatherArea(GameMap map)
    {
        int wide = FrameRoot.WorldWidth + WeatherMargin;
        int tall = FrameRoot.WorldHeight + WeatherMargin;
        return new Rect2(
            -wide,
            -tall,
            (map.Width * MapCamera.TilePixels) + (2 * wide),
            (map.Height * MapCamera.TilePixels) + (2 * tall));
    }

    /// <summary>Builds the sprite of one decor piece or of the opening of one light shaft, on its wall (D-844, D-924).</summary>
    private static Sprite2D PieceSprite(GameAtlas atlas, DecorPiece piece)
    {
        AtlasEntry entry = atlas.Index.Entry(piece.Kind, LightContent.MapUse);

        // A piece sorts by the south edge of its wall, as a map sprite sorts by its feet
        // (F-94, D-737). A figure in front of the wall then draws over it.
        return new Sprite2D
        {
            Name = piece.Id.Value,
            Texture = atlas.Frame(entry.Id, 0),
            Centered = false,
            Position = new Vector2(piece.Tile.X * MapCamera.TilePixels, FeetOf(piece.Tile.Y * MapCamera.TilePixels, 1)),
            Offset = new Vector2(0, -entry.Height),

            // A flame and the opening of a shaft give light and take none, so the dark of the
            // ambient light never dims them. The piece is a sprite, so it never glows, and the
            // fire of a torch glows instead (D-188, D-912).
            Material = new CanvasItemMaterial { LightMode = CanvasItemMaterial.LightModeEnum.Unshaded },
        };
    }

    /// <summary>Gives the fire of the decor kind of one light, or no value for a light that no piece holds.</summary>
    private static TorchFire? FireOf(ContentSet content, DecorFile decor, ContentId light)
    {
        foreach (DecorPiece piece in decor.Pieces)
        {
            if (string.CompareOrdinal(piece.Id.Value, light.Value) == 0)
            {
                return content.Light.KindOf(piece.Kind).Fire;
            }
        }

        return null;
    }

    /// <summary>Builds the occluder of one wall from its shape: a rectangle, or an L beside a doorway (D-852).</summary>
    private static LightOccluder2D WallOccluder(WallShadow wall)
    {
        IReadOnlyList<(int X, int Y)> outline = wall.Outline();
        var polygon = new Vector2[outline.Count];
        for (int index = 0; index < outline.Count; index += 1)
        {
            polygon[index] = new Vector2(outline[index].X, outline[index].Y);
        }

        return new LightOccluder2D
        {
            Name = $"wall_{wall.Tile.X}_{wall.Tile.Y}",
            Position = new Vector2(wall.Tile.X * MapCamera.TilePixels, wall.Tile.Y * MapCamera.TilePixels),
            OccluderLightMask = WorldLights.WallShadows,
            Occluder = new OccluderPolygon2D
            {
                Polygon = polygon,
                Closed = true,
            },
        };
    }

    /// <summary>
    /// Builds the occluder at the feet of one figure: a flat hexagon across the middle of the
    /// body, which throws the shadow of the figure away from each light (D-853).
    /// </summary>
    private static LightOccluder2D FeetShadow(Sprite2D sprite, int mask)
    {
        float width = sprite.Texture.GetSize().X;
        float half = width * 0.3f;
        float middle = width / 2f;
        const float Depth = 3f;
        return new LightOccluder2D
        {
            Name = "feet_shadow",
            OccluderLightMask = mask,
            Occluder = new OccluderPolygon2D
            {
                Polygon =
                [
                    new Vector2(middle - half, -Depth),
                    new Vector2(middle - (half / 2f), -2f * Depth),
                    new Vector2(middle + (half / 2f), -2f * Depth),
                    new Vector2(middle + half, -Depth),
                    new Vector2(middle + (half / 2f), 0f),
                    new Vector2(middle - (half / 2f), 0f),
                ],
                Closed = true,
            },
        };
    }

    private static int CountOccluders(Node root)
    {
        int count = 0;
        foreach (Node child in root.GetChildren())
        {
            count += (child is LightOccluder2D ? 1 : 0) + CountOccluders(child);
        }

        return count;
    }

    /// <summary>Puts each enemy where Core put it, and shows the mark of a sight (D-208, D-737).</summary>
    /// <remarks>
    /// The position of a sprite is the north-west pixel of the front row of the body, so
    /// Godot sorts the body by that row (D-737). The offset of the sprite draws the picture
    /// up from there, so a picture taller than one tile covers the whole body.
    /// </remarks>
    private void ShowEnemies(MapState party, int tickPart, long tick, bool torchHeld)
    {
        IReadOnlyList<PatrolState> patrols = party.Patrols.All;
        SightMark? mark = party.Patrols.Mark;
        this.mark.Visible = false;
        int leadX = MapCamera.LeadX(party, tickPart);
        int leadY = MapCamera.LeadY(party, tickPart);
        int range = MapRules.PartySightRange(party.Map, torchHeld) * MapCamera.TilePixels;
        if (party.Map.Dark && this.fade is null)
        {
            this.fade = new SightFade(ClearLines(party), range);
        }

        this.fade?.MoveRange(range, tick);

        for (int index = 0; index < patrols.Count; index += 1)
        {
            PatrolState patrol = patrols[index];
            Sprite2D sprite = this.enemies[index];
            int x = MapCamera.EnemyX(patrol, tickPart);
            int y = MapCamera.EnemyY(patrol, tickPart);

            // On a map that is not dark, every live enemy draws, wherever it stands, so no enemy
            // pops in when the party comes near (D-814). On a dark map, the sight of the party
            // sets the share of each enemy, and the fade of each edge stops a pop-in (D-1062).
            int share = SightFade.Full;
            if (this.fade is not null)
            {
                bool clear = MapSight.Clear(party.Map, party.LeadAt, patrol.Body.Nearest(party.LeadAt));
                share = this.fade.ShareOf(index, clear, SightFade.Reach(leadX, leadY, x, y, patrol.Body.Side), tick);
            }

            bool drawn = !patrol.Dead && share > 0;
            this.shares[index] = patrol.Dead ? 0 : share;
            sprite.Visible = drawn;
            sprite.Modulate = new Color(1, 1, 1, share / (float)SightFade.Full);
            sprite.Position = new Vector2(x, FeetOf(y, patrol.Body.Side));

            // The mark draws with the enemy that saw the party, also while the dark still fades
            // that enemy in, so the warning of a sight never hides (D-720, D-1062).
            if (patrol.Dead || mark is null || string.CompareOrdinal(mark.Enemy.Value, patrol.Patrol.Id.Value) != 0)
            {
                continue;
            }

            this.mark.Visible = true;
            this.mark.Position = new Vector2(
                x + (patrol.Body.Side * MapCamera.TilePixels / 2) - (MarkWidth / 2),
                y - MarkBar - MarkDot - MarkDot);
        }
    }

    /// <summary>Tells for each enemy whether a wall stands between it and the lead now (D-718, D-1062).</summary>
    private static bool[] ClearLines(MapState party)
    {
        IReadOnlyList<PatrolState> patrols = party.Patrols.All;
        bool[] clear = new bool[patrols.Count];
        for (int index = 0; index < patrols.Count; index += 1)
        {
            clear[index] = MapSight.Clear(party.Map, party.LeadAt, patrols[index].Body.Nearest(party.LeadAt));
        }

        return clear;
    }

    /// <summary>Gives the map drawing of the lead with the torch in its hand, which fits the plain drawing (D-1069).</summary>
    /// <exception cref="ContentException">The atlas holds no torch drawing of the lead (T-2).</exception>
    /// <exception cref="InvalidOperationException">The torch drawing differs in size from the plain drawing (T-2).</exception>
    private static Texture2D TorchTexture(GameAtlas atlas, ContentId leadId)
    {
        AtlasEntry plain = atlas.Index.Entry(leadId, MapUse);
        AtlasEntry torch = atlas.Index.Entry(leadId, TorchUse);
        if (torch.Width != plain.Width || torch.Height != plain.Height)
        {
            throw new InvalidOperationException(
                $"The torch drawing of '{leadId.Value}' is {torch.Width} by {torch.Height}, and its map drawing is {plain.Width} by {plain.Height}. One sprite shows both (T-2, D-1069).");
        }

        return atlas.Frame(torch.Id, 0);
    }

    /// <summary>
    /// Gives the sort value of one map sprite: the south edge of the front row of its body
    /// (F-94, D-737).
    /// </summary>
    /// <param name="northPixel">The pixel of the north edge of the anchor tile.</param>
    /// <param name="sideTiles">The count of tiles on one side of the body (D-206).</param>
    /// <returns>The pixel of the south edge of the front row.</returns>
    private static int FeetOf(int northPixel, int sideTiles) =>
        northPixel + (sideTiles * MapCamera.TilePixels);

    /// <summary>Builds the sprite of one thing of the map from its drawing (D-519, D-666).</summary>
    /// <remarks>
    /// The sprite draws its whole picture up from its position, because that position is the
    /// south edge of the front row of the body (F-94, D-737). A picture taller than one tile
    /// then covers the whole body.
    /// </remarks>
    private static Sprite2D BuildSprite(GameAtlas atlas, ContentId content)
    {
        AtlasEntry entry = atlas.Index.Entry(content, MapUse);
        return new Sprite2D
        {
            Texture = atlas.Frame(entry.Id, 0),
            Centered = false,
            Offset = new Vector2(0, -entry.Height),

            // The figure light of each pair lights a figure, so no figure darkens itself (D-853).
            LightMask = WorldLights.FigureItems,
        };
    }

    /// <summary>
    /// Builds the mark that a patrol shows for a beat before an encounter (D-208, D-745).
    /// The mark is a bar and a dot in the warning color of the style file (D-527).
    /// </summary>
    /// <remarks>
    /// The art of the enemies lands in PR-17, and the mark takes its own drawing there
    /// (D-744, section 7.9 of `docs/roadmaps/area-art.md`). Until then two rectangles of the
    /// palette read as a mark at every scale of the frame (D-568).
    /// </remarks>
    private static Node2D BuildMark(UiTheme theme)
    {
        Color color = theme.ColorOf(MarkRole);
        var node = new Node2D
        {
            Visible = false,

            // The mark draws above every body of the map, and Godot sorts by the Y value
            // inside one Z index alone (the external facts of `area-exploration.md`). It also
            // draws above the weather, so fog never hides it (D-208, D-885).
            ZIndex = MarkZIndex,
        };

        node.AddChild(new ColorRect
        {
            Position = new Vector2(0, 0),
            Size = new Vector2(MarkWidth, MarkBar),
            Color = color,
        });
        node.AddChild(new ColorRect
        {
            Position = new Vector2(0, MarkBar + MarkDot),
            Size = new Vector2(MarkWidth, MarkDot),
            Color = color,
        });
        return node;
    }

    /// <summary>
    /// Reads the four Godot defaults of F-51 back, and fails when one of them is wrong
    /// (T-2). A headless session draws nothing, so this check reads the nodes and never the
    /// pixels (F-23).
    /// </summary>
    /// <returns>The four values, for the report of the smoke session.</returns>
    /// <exception cref="InvalidOperationException">The layer or the tile set holds another value (T-2).</exception>
    /// <remarks>
    /// `TileSet.TileSize` and `TileSetAtlasSource.TextureRegionSize` both default to 16 by
    /// 16, and the collisions and the navigation of a layer both default to on (F-51).
    /// </remarks>
    public string DescribeGround()
    {
        TileSet set = this.ground.TileSet;
        var source = (TileSetAtlasSource)set.GetSource(MapTileSet.SourceId);
        Vector2I wanted = new(MapTileSet.TilePixels, MapTileSet.TilePixels);

        Refuse(set.TileSize != wanted, $"the tile size is {set.TileSize}, and it takes {wanted} (D-228, F-51)");
        Refuse(
            source.TextureRegionSize != wanted,
            $"the region size is {source.TextureRegionSize}, and it takes {wanted} (D-667, F-51)");
        Refuse(this.ground.CollisionEnabled, "the collisions of the layer are on, and no rule of Core reads one (G-1, F-51)");
        Refuse(this.ground.NavigationEnabled, "the navigation of the layer is on, and no rule of Core reads it (G-1, F-51)");
        Refuse(this.ground.YSortEnabled, "the layer takes part in the sort, and a tile then draws over a sprite inside a step (F-95, D-783)");
        Refuse(
            this.ground.ZIndex != GroundZIndex,
            $"the layer draws at the Z index {this.ground.ZIndex}, and it takes {GroundZIndex} (F-95, D-783)");
        Refuse(
            this.ground.RenderingQuadrantSize != LightBudget.QuadrantTiles,
            $"the quadrant is {this.ground.RenderingQuadrantSize} tiles, and the budget test counts {LightBudget.QuadrantTiles} (F-46, D-842)");

        return $"tile size {set.TileSize}, region size {source.TextureRegionSize}, "
            + $"collisions {this.ground.CollisionEnabled}, navigation {this.ground.NavigationEnabled}, "
            + $"sort {this.ground.YSortEnabled}, Z index {this.ground.ZIndex}";
    }

    /// <summary>
    /// Reads the sprite of the lead and the sprite of each enemy back, and fails when one of
    /// them holds no picture (T-2, F-45). A headless session draws nothing, so this check
    /// reads the nodes and never the pixels (F-23).
    /// </summary>
    /// <param name="party">The party and the enemies on the map.</param>
    /// <returns>The count of sprites, the count that draws, and the mark.</returns>
    /// <exception cref="ArgumentNullException">The party is null (T-2).</exception>
    /// <exception cref="InvalidOperationException">
    /// A sprite holds no picture of 32 pixels or more, or a live enemy draws no sprite (T-2, D-814).
    /// </exception>
    /// <remarks>
    /// `AtlasTexture` reports an absent page or an empty region in the log alone, so a sprite
    /// with no picture would draw nothing and no check would see it (F-45).
    /// </remarks>
    public string DescribeSprites(MapState party)
    {
        ArgumentNullException.ThrowIfNull(party);

        CheckSprite(this.lead, "the lead");
        int drawn = 0;
        for (int index = 0; index < this.enemies.Length; index += 1)
        {
            PatrolState patrol = party.Patrols.All[index];
            CheckSprite(this.enemies[index], $"the enemy '{patrol.Patrol.Id.Value}'");

            // On a map that is not dark, a live enemy draws at any distance and a dead one never
            // draws (D-814). On a dark map, a live enemy draws while the dark leaves a share of it
            // (D-1062). The bounds below read Core alone and never the shares of the fade, so a
            // fade that hides a live enemy on a lit map, or shows one past the widest sight, fails
            // here (D-1118).
            bool mustDraw = !patrol.Dead && !party.Map.Dark;
            bool mayDraw = !patrol.Dead && (!party.Map.Dark || WithinWidestSight(party, patrol));
            bool visible = this.enemies[index].Visible;
            Refuse(
                (mustDraw && !visible) || (visible && !mayDraw),
                $"the enemy '{patrol.Patrol.Id.Value}' draws {visible}, is dead {patrol.Dead}, and holds the share {this.shares[index]} on a map that is dark {party.Map.Dark} (D-814, D-1062, D-1118)");
            drawn += this.enemies[index].Visible ? 1 : 0;
        }

        string mark = this.mark.Visible ? "a mark" : "no mark";
        return $"the lead at {this.lead.Position}, {this.enemies.Length} enemies with {drawn} drawn, and {mark}";
    }

    /// <summary>
    /// Tells whether an enemy lies inside the widest sight of the party on a dark map: the range
    /// of the torch, one tile of fade past it, and one tile of a step that runs (D-1062, D-1063).
    /// </summary>
    private static bool WithinWidestSight(MapState party, PatrolState patrol)
    {
        TilePoint nearest = patrol.Body.Nearest(party.LeadAt);
        int reach = Math.Max(Math.Abs(nearest.X - party.LeadAt.X), Math.Abs(nearest.Y - party.LeadAt.Y));
        return reach <= MapRules.PartySightRange(party.Map, torchHeld: true) + 2;
    }

    private static void CheckSprite(Sprite2D sprite, string what)
    {
        Texture2D? texture = sprite.Texture;
        Refuse(texture is null, $"{what} holds no picture (F-45)");
        Vector2 size = texture!.GetSize();
        Refuse(
            size.X < MapCamera.TilePixels || size.Y < MapCamera.TilePixels,
            $"{what} holds a picture of {size}, and a map sprite is {MapCamera.TilePixels} pixels or more (D-236, F-45)");
    }

    private static void Refuse(bool broken, string reason)
    {
        if (broken)
        {
            throw new InvalidOperationException($"The tile map of the screen is wrong: {reason} (T-2).");
        }
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

            // The ground is flat, so no tile of it ever stands in front of a sprite. Thus the
            // layer takes no part in the sort, and it draws below every sprite at every pixel
            // of a slide (F-95, D-783). A later tile that stands up takes a layer of its own.
            YSortEnabled = false,
            LightMask = WorldLights.GroundItems,
            ZIndex = GroundZIndex,

            // Godot draws each quadrant as one canvas item, and one canvas item takes 15 lights
            // at most. The budget test of Core counts each quadrant of this size (F-46, D-842).
            RenderingQuadrantSize = LightBudget.QuadrantTiles,
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
