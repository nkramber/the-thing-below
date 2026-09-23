using System;
using System.Collections.Generic;
using System.Globalization;
using Godot;
using TheThingBelow.Core.Battles;
using TheThingBelow.Core.Content;
using TheThingBelow.Core.Effects;
using TheThingBelow.Core.Light;
using TheThingBelow.Core.Runs;
using TheThingBelow.Storage;

namespace TheThingBelow.Game.Ui;

/// <summary>
/// The battle on screen: the backdrop and the side view in the world viewport, and the
/// timeline strip, the message line, the command menu, and the status on the frame (D-111,
/// D-213, D-756).
/// </summary>
/// <remarks>
/// The screen draws <see cref="GameRun.BattleView"/> and the event that plays, and it holds no
/// rule (D-100). Every value of a draw comes from a tick count, so a capture of one tick shows
/// the same picture on every run (T-7).
/// <para>
/// The command menu takes the input of the player while a character has the turn, and it
/// gives one intent for each whole choice (D-493, D-827). Every word on screen comes from the
/// string table through the text helper (G-7, D-499).
/// </para>
/// <para>
/// The sprite of each combatant takes the hit flash shader of D-825. The shader never writes
/// the normal map, so the light of PR-56 finds the normal that Godot corrects (D-183).
/// </para>
/// <para>
/// The fight takes the ambient light and the key light of the light setup of the map where it
/// began, at the time of day of that map (D-205, D-442, D-850).
/// </para>
/// </remarks>
public sealed class BattleScreen
{
    /// <summary>The use of the battle drawing of a combatant (D-519, D-828).</summary>
    public const string BattleUse = "battle";

    /// <summary>The use of the attack pose of a character (D-108, D-828).</summary>
    public const string AttackUse = "battle_attack";

    /// <summary>The use of the icon of an element or a status (D-214).</summary>
    public const string IconUse = "icon";

    /// <summary>The content id of the pointer of a target (D-833).</summary>
    public const string PointerContentId = "ui.pointer";

    /// <summary>The use of the pointer drawing (D-833).</summary>
    public const string PointerUse = "pointer";

    /// <summary>The path of the hit flash shader in the Godot project (D-825).</summary>
    public const string FlashShaderPath = "res://shaders/hit_flash.gdshader";

    /// <summary>The name of the shader value that turns the flash on (D-825).</summary>
    public const string FlashAmount = "flash_amount";

    /// <summary>The name of the shader value that holds the color of the flash (D-825).</summary>
    public const string FlashColor = "flash_color";

    /// <summary>The Z index of the backdrop, below every sprite (D-205).</summary>
    private const int BackdropZIndex = -10;

    /// <summary>The margin around the world of a fight that each particle node holds, in art pixels (F-98).</summary>
    private const int BattleWeatherMargin = 64;

    /// <summary>The Z index of the health bars and the pointer, above every sprite.</summary>
    private const int MarkZIndex = 10;

    /// <summary>The width of the label of a damage number, in frame pixels.</summary>
    private const int NumberWidth = 160;

    private readonly UiBase ui;
    private readonly ContentSet content;
    private readonly Node2D world;
    private AmbientLayer weather = null!;

    /// <summary>True for a capture, which seeks each stream to the tick of the frame (D-172).</summary>
    public bool SeekParticles { get; set; }
    private readonly Control layer;
    private readonly Node2D backdrop;
    private readonly List<CombatantNodes> party = [];
    private readonly List<CombatantNodes> enemies = [];
    private readonly TextureRect[] faces = new TextureRect[BattleLayout.StripTurns];
    private readonly Sprite2D pointer;
    private readonly Label message;
    private readonly Label number;
    private readonly HBoxContainer commandRow;
    private readonly List<StatusLine> statusLines = [];
    private readonly Color chosenColor;
    private readonly Color dimColor;
    private readonly CommandMemory memory;
    private readonly BattleEffects pace;
    private readonly SortedDictionary<string, HitBurst> bursts = new(StringComparer.Ordinal);
    private BattleEvent? shownEvent;
    private BattleCommands? commands;
    private bool sentCommand;
    private string shownCommands = string.Empty;

    private BattleScreen(UiBase ui, ContentSet content, FrameRoot frame, CommandMemory memory, EffectLevel effects)
    {
        this.ui = ui;
        this.pace = content.Effects.Battle;
        this.Effects = effects;
        this.memory = memory;
        this.content = content;
        this.chosenColor = ui.Theme.ColorOf("text_chosen");
        this.dimColor = ui.Theme.ColorOf("text_dim");

        this.world = new Node2D { YSortEnabled = true };
        frame.World.AddChild(this.world);
        frame.ShowGlow(content.Light.Glow);

        // A fight draws the tilt-shift blur and the vignette as a map does (D-920).
        frame.ShowPasses(content.Light.Passes, content.Palette);
        this.layer = new Control
        {
            Position = Vector2.Zero,
            Size = new Vector2(ScreenFit.FrameWidth, ScreenFit.FrameHeight),
            MouseFilter = Control.MouseFilterEnum.Ignore,
            Theme = ui.Theme.Theme,
        };
        frame.Layer.AddChild(this.layer);

        this.backdrop = this.BuildBackdrop();
        this.pointer = new Sprite2D
        {
            Texture = ui.Atlas.Frame(this.DrawingOf(Parse(PointerContentId), PointerUse), 0),
            Centered = false,
            ZIndex = MarkZIndex,
            Visible = false,
        };
        this.world.AddChild(this.pointer);
        GlowPass.LiftToMarks(this.pointer);

        this.BuildStrip();
        this.message = this.BuildLinePanel(BattleLayout.Message);
        Control commandInside = this.Panel(BattleLayout.Commands);
        this.commandRow = new HBoxContainer { Size = commandInside.Size };
        this.commandRow.AddThemeConstantOverride("separation", ui.Theme.BodySize);
        commandInside.AddChild(this.commandRow);
        this.number = new Label
        {
            HorizontalAlignment = HorizontalAlignment.Center,
            Size = new Vector2(NumberWidth, ui.Theme.BodySize),
            Visible = false,
        };
        this.layer.AddChild(this.number);
    }

    /// <summary>The count of sprites of combatants that the screen built.</summary>
    public int CombatantCount => this.party.Count + this.enemies.Count;

    /// <summary>The count of copies of the backdrop pieces that the screen built (D-518).</summary>
    public int BackdropCopies { get; private set; }

    /// <summary>The command menu of the turn, or no value while no character waits for a command.</summary>
    public BattleCommands? Commands => this.commands;

    /// <summary>Builds the battle screen of one fight into a frame.</summary>
    /// <param name="frame">The frame, with its world viewport and its UI layer.</param>
    /// <param name="ui">The atlas, the theme, and the text helper.</param>
    /// <param name="content">The content set, for the pictures and the strings.</param>
    /// <param name="run">The run, whose view of the fight the screen draws.</param>
    /// <param name="memory">The remembered cursor of the command menu, which lasts the session (D-226).</param>
    /// <param name="effects">The level of the flash and shake reduction of the settings (D-863).</param>
    /// <returns>The screen, which the caller shows on each frame and frees at the end of the fight.</returns>
    /// <exception cref="ArgumentNullException">An argument is null (T-2).</exception>
    /// <exception cref="InvalidOperationException">The run holds no view of a fight, or the shader failed to load (T-2).</exception>
    /// <param name="ambient">The weather that the fight draws, or no value for the weather of the map of the fight (D-205, D-889).</param>
    /// <exception cref="ContentException">The atlas holds no drawing of a combatant, an icon, or the pointer (T-2).</exception>
    public static BattleScreen Build(
        FrameRoot frame,
        UiBase ui,
        ContentSet content,
        GameRun run,
        CommandMemory memory,
        EffectLevel effects,
        AmbientEffect? ambient = null)
    {
        ArgumentNullException.ThrowIfNull(frame);
        ArgumentNullException.ThrowIfNull(memory);
        ArgumentNullException.ThrowIfNull(ui);
        ArgumentNullException.ThrowIfNull(content);
        ArgumentNullException.ThrowIfNull(run);

        BattleView view = run.BattleView ?? throw new InvalidOperationException(
            $"The battle screen builds at tick {run.Tick}, and the run holds no view of a fight (D-532, T-2).");

        var screen = new BattleScreen(ui, content, frame, memory, effects);
        screen.BuildLight(run.Party.Map);

        // The weather of the place plays over the backdrop of the fight (D-205).
        screen.weather = AmbientLayer.Build(
            ambient ?? content.Effects.Ambient.WeatherOf(run.Party.Map.Id),
            content.Palette,
            screen.world);
        Shader flash = LoadFlashShader();
        foreach (ShownCombatant shown in view.Party)
        {
            screen.party.Add(screen.BuildCombatant(shown, flash));
            screen.statusLines.Add(screen.BuildStatusLine());
        }

        foreach (ShownCombatant shown in view.Enemies)
        {
            screen.enemies.Add(screen.BuildCombatant(shown, flash));
        }

        // Every hit file builds its nodes at the start of the fight, so a hit never builds a
        // node while the fight plays (D-182).
        foreach (HitEffect effect in content.Effects.Hits)
        {
            screen.bursts.Add(effect.Id.Value, HitBurst.Build(effect, content.Palette, screen.world));
        }

        screen.PlaceStatusLines();
        screen.Show(run);
        return screen;
    }

    /// <summary>
    /// The place of the key light over the world viewport, in art pixels: above the middle of
    /// the fight, so each figure takes light on its upper side (D-850).
    /// </summary>
    public static readonly Vector2 KeyLightPlace = new(FrameRoot.WorldWidth / 2, 0);

    /// <summary>
    /// Reads each light of the fight back, and fails on a light that draws nothing (F-46). A
    /// headless session draws nothing, so this check reads the nodes and never the pixels (F-23).
    /// </summary>
    /// <returns>The count of lights.</returns>
    /// <exception cref="InvalidOperationException">A light has no texture or no height, or the fight holds no light (T-2, F-46).</exception>
    public int CheckLights()
    {
        int count = WorldLights.CheckLights(this.world);
        if (count != 1)
        {
            throw new InvalidOperationException(
                $"The fight holds {count} lights, and it takes the one key light of its map (T-2, D-850).");
        }

        return count;
    }

    /// <summary>Builds the ambient light and the key light of the map where the fight began (D-850).</summary>
    private void BuildLight(TheThingBelow.Core.Maps.GameMap map)
    {
        LightSetup setup = this.content.Light.SetupOf(map.Id, map.Time);
        this.world.AddChild(WorldLights.Ambient(this.content.Palette, setup.Ambient));

        // No wall stands in a fight, so the key light casts no shadow (D-850).
        PointLight2D key = WorldLights.Point("key_light", this.content.Palette, setup.Battle, WorldLights.BuildTexture(), WorldLights.GroundItems, 0);
        key.Position = KeyLightPlace;
        this.world.AddChild(key);
    }

    /// <summary>Removes every node of the screen from the frame.</summary>
    public void Free()
    {
        this.world.GetParent()?.RemoveChild(this.world);
        this.world.QueueFree();
        this.layer.GetParent()?.RemoveChild(this.layer);
        this.layer.QueueFree();
    }

    /// <summary>
    /// Reads each particle node of the fight back, and fails on a hit file with no node (T-2).
    /// A headless session draws nothing, so this check reads the nodes and never the pixels
    /// (F-23).
    /// </summary>
    /// <returns>The count of particle nodes of every burst.</returns>
    /// <exception cref="InvalidOperationException">A burst holds no node (T-2).</exception>
    public int CheckBursts()
    {
        int count = 0;
        foreach (HitBurst burst in this.bursts.Values)
        {
            if (burst.NodeCount == 0)
            {
                throw new InvalidOperationException(
                    $"The burst of '{burst.Effect.File}' holds no particle node, and a hit would show nothing (T-2, D-879).");
            }

            count += burst.NodeCount;
        }

        return count;
    }

    /// <summary>The level of the flash and shake reduction, which the settings screen changes (D-863).</summary>
    public EffectLevel Effects { get; set; }

    /// <summary>Draws the fight as the run shows it now (D-532).</summary>
    /// <param name="run">The run.</param>
    /// <exception cref="ArgumentNullException">The run is null (T-2).</exception>
    /// <exception cref="InvalidOperationException">The run holds no view of a fight (T-2).</exception>
    public void Show(GameRun run)
    {
        ArgumentNullException.ThrowIfNull(run);

        this.Draw(run, run.PlayingEvent);
    }

    /// <summary>
    /// Draws the fight of the run with a staged event in place of the event that plays, at the
    /// same ticks. A capture of the screen-test job stages a heavy blow, which no move of the
    /// fixture fight gives before PR-12 (D-172, D-877).
    /// </summary>
    /// <param name="run">The run, which plays an event.</param>
    /// <param name="staged">The event that the screen draws.</param>
    /// <exception cref="ArgumentNullException">An argument is null (T-2).</exception>
    /// <exception cref="InvalidOperationException">The run holds no view of a fight, or plays no event (T-2).</exception>
    public void ShowStaged(GameRun run, BattleEvent staged)
    {
        ArgumentNullException.ThrowIfNull(run);
        ArgumentNullException.ThrowIfNull(staged);

        if (run.PlayingEvent is null)
        {
            throw new InvalidOperationException(
                $"The battle screen stages an event at tick {run.Tick}, and the run plays none, so the event has no ticks (T-2).");
        }

        this.Draw(run, staged);
    }

    private void Draw(GameRun run, BattleEvent? playing)
    {
        BattleView view = run.BattleView ?? throw new InvalidOperationException(
            $"The battle screen draws at tick {run.Tick}, and the run holds no view of a fight (D-532, T-2).");

        this.backdrop.Position = new Vector2(BattleTimes.DriftAt(this.pace, run.Tick), 0);
        this.FollowCommands(run);

        // The shake reads the ticks of the event, and the rest of the picture reads the ticks
        // of the picture, which stand still during the hit-stop (D-876, D-880).
        int ticks = run.PlayingTicks;
        int picture = playing is null ? ticks : BattleTimes.PictureTicks(this.pace, playing, ticks);
        int shake = playing is null ? 0 : BattleTimes.ShakeAt(this.pace, playing, ticks, this.Effects);
        this.world.Position = new Vector2(shake, 0);
        this.weather.Show(Vector2.Zero, FrameRoot.WorldWidth, FrameRoot.WorldHeight, run.Tick);
        for (int slot = 0; slot < view.Party.Count; slot += 1)
        {
            this.ShowCombatant(view, view.Party[slot], this.party[slot], playing, picture);
            this.ShowStatusLine(view.Party[slot], this.statusLines[slot]);
        }

        for (int slot = 0; slot < view.Enemies.Count; slot += 1)
        {
            this.ShowCombatant(view, view.Enemies[slot], this.enemies[slot], playing, picture);
        }

        this.ShowBurst(view, playing, picture, run.Tick - ticks);
        this.ShowMessage(view, playing);
        this.ShowNumber(view, playing, picture);
        this.ShowStrip(run);
        this.ShowCommands(view);
    }

    /// <summary>Shows the burst of the hit that plays, from the hit file of its target, and hides every other burst (D-879).</summary>
    /// <param name="view">The view of the fight.</param>
    /// <param name="playing">The event that plays, or no value.</param>
    /// <param name="picture">The ticks of the picture.</param>
    /// <param name="started">The tick of the run when the event started, which seeds the burst.</param>
    private void ShowBurst(BattleView view, BattleEvent? playing, int picture, long started)
    {
        int? age = playing is null ? null : BattleTimes.BurstAge(this.pace, playing, picture);
        HitBurst? shown = null;
        if (playing is not null && age is int ticks)
        {
            BattleTarget target = playing.Target ?? throw new InvalidOperationException(
                $"The hit of {playing.Actor.Describe()} at tick {started} holds no target (T-2).");
            ShownCombatant struck = view.At(target);
            HitEffect effect = this.content.Effects.HitOf(struck.Id);
            shown = this.bursts[effect.Id.Value];

            // The body of the target: half its height above its feet. The party stands on the
            // right, so a burst on an enemy leaves toward the west (D-832).
            FieldPlace place = BattleLayout.PlaceOf(view, struck);
            CombatantNodes nodes = target.Side == BattleSide.Party ? this.party[target.Slot] : this.enemies[target.Slot];
            var point = new Vector2(place.X, place.Feet - (nodes.Height / 2));
            int away = target.Side == BattleSide.Enemy ? -1 : 1;
            shown.Seek(point, away, unchecked((uint)started), ticks);
        }

        foreach (HitBurst burst in this.bursts.Values)
        {
            if (!ReferenceEquals(burst, shown))
            {
                burst.Hide();
            }
        }
    }

    /// <summary>
    /// Reads one action of the input map while a character has the turn, and gives the
    /// intent of a whole choice (D-493, D-827). A move of the cursor gives no intent.
    /// </summary>
    /// <param name="action">The name of the action, such as `confirm`.</param>
    /// <returns>The intent, or no value.</returns>
    /// <exception cref="ArgumentException">The name is empty (T-2).</exception>
    public Intent? Read(string action)
    {
        ArgumentException.ThrowIfNullOrEmpty(action);

        BattleCommands? open = this.commands;
        if (open is null)
        {
            return null;
        }

        switch (action)
        {
            case InputActions.StepNorth:
            case InputActions.StepWest:
                open.Move(-1);
                return null;
            case InputActions.StepSouth:
            case InputActions.StepEast:
                open.Move(1);
                return null;
            case InputActions.Cancel:
                open.Cancel();
                return null;
            case InputActions.Confirm:
                Intent? made = open.Confirm();
                if (made is not null)
                {
                    this.memory.Keep(open.Actor, open.Action);

                    // The intent reaches the rules on the next tick, and the gate of D-532
                    // stays open until then. A second menu would send a second intent (T-2).
                    this.commands = null;
                    this.sentCommand = true;
                }

                return made;
            default:
                return null;
        }
    }

    /// <summary>Gives the drawing of one thing and one use (D-519).</summary>
    private ContentId DrawingOf(ContentId thing, string use) => this.content.Atlas.Entry(thing, use).Id;

    private static ContentId Parse(string id) => ContentId.Parse(id, AtlasIndex.Path, "battle screen");

    private static Shader LoadFlashShader()
    {
        // The load reports a failure in the log alone, so the result takes a check (T-2).
        Shader? loaded = ResourceLoader.Load<Shader>(FlashShaderPath);
        return loaded ?? throw new InvalidOperationException(
            $"Godot loaded no shader from '{FlashShaderPath}' (D-825, T-2).");
    }

    private Node2D BuildBackdrop()
    {
        // Every fight draws the fixture backdrop until the place art of PR-17 (D-831). The
        // sway shows a strip of the next copy at each edge, so a copy stands on each side.
        ContentId id = ContentId.Parse(ScreenCaptures.FixturePicture, LargePicture.Folder, "battle screen");
        LargePicture picture = this.content.PictureOf(id);
        var holder = new Node2D { ZIndex = BackdropZIndex };
        foreach (int copy in new[] { -1, 0, 1 })
        {
            var view = new PictureView { Position = new Vector2(copy * picture.Width, 0) };
            view.Build(this.ui.Atlas, picture, this.content);
            this.BackdropCopies += view.CopyCount;
            holder.AddChild(view);
        }

        this.world.AddChild(holder);
        return holder;
    }

    private CombatantNodes BuildCombatant(ShownCombatant shown, Shader flash)
    {
        AtlasTexture idle = this.ui.Atlas.Frame(this.DrawingOf(shown.Id, BattleUse), 0);
        AtlasTexture? pose = this.content.Atlas.Draws(shown.Id, AttackUse)
            ? this.ui.Atlas.Frame(this.DrawingOf(shown.Id, AttackUse), 0)
            : null;

        var material = new ShaderMaterial { Shader = flash };
        material.SetShaderParameter(FlashColor, this.ui.Theme.ColorOf("flash"));
        material.SetShaderParameter(FlashAmount, 0.0f);

        int width = (int)idle.Region.Size.X;
        int height = (int)idle.Region.Size.Y;
        var sprite = new Sprite2D
        {
            Texture = idle,
            Centered = false,
            Offset = new Vector2(-(width / 2), -height),
            Material = material,
        };
        this.world.AddChild(sprite);

        var nodes = new CombatantNodes(sprite, material, idle, pose, width, height);
        if (shown.Target.Side == BattleSide.Enemy)
        {
            nodes.Bar = this.BuildBar();
            nodes.Icons = new HBoxContainer();
            nodes.Icons.AddThemeConstantOverride("separation", 0);
            this.layer.AddChild(nodes.Icons);
        }

        return nodes;
    }

    private HealthBar BuildBar()
    {
        var border = new ColorRect
        {
            Color = this.ui.Theme.ColorOf("bar_border"),
            Size = new Vector2(BattleLayout.BarWidth, BattleLayout.BarHeight),
            ZIndex = MarkZIndex,
        };
        var empty = new ColorRect
        {
            Color = this.ui.Theme.ColorOf("bar_empty"),
            Position = Vector2.One,
            Size = new Vector2(BattleLayout.BarWidth - 2, BattleLayout.BarHeight - 2),
        };
        var fill = new ColorRect
        {
            Color = this.ui.Theme.ColorOf("bar_fill"),
            Position = Vector2.One,
            Size = new Vector2(BattleLayout.BarWidth - 2, BattleLayout.BarHeight - 2),
        };

        border.AddChild(empty);
        border.AddChild(fill);
        this.world.AddChild(border);

        // The bar draws above the fog, the glow, and the passes, as the mark does (D-208, D-916, D-919).
        GlowPass.LiftToMarks(border);
        return new HealthBar(border, fill);
    }

    private void BuildStrip()
    {
        Control inside = this.Panel(BattleLayout.Strip);
        for (int turn = 0; turn < this.faces.Length; turn += 1)
        {
            var face = new TextureRect
            {
                Position = new Vector2(turn * (BattleLayout.FaceSize + BattleLayout.FaceGap), 0),
                Size = new Vector2(BattleLayout.FaceSize, BattleLayout.FaceSize),
                ExpandMode = TextureRect.ExpandModeEnum.IgnoreSize,
                // A face keeps whole pixels: a drawing of 32 draws at 1x, one of 64 at half
                // size, and one of 96 at a third (D-236, D-756).
                StretchMode = TextureRect.StretchModeEnum.KeepAspectCentered,
                TextureFilter = CanvasItem.TextureFilterEnum.Nearest,
            };
            this.faces[turn] = face;
            inside.AddChild(face);
        }

        // The mark under the first face names who acts next (D-756).
        inside.AddChild(new ColorRect
        {
            Color = this.ui.Theme.ColorOf("cursor"),
            Position = new Vector2(0, BattleLayout.FaceSize - 2),
            Size = new Vector2(BattleLayout.FaceSize, 2),
        });
    }

    private Label BuildLinePanel(FrameBox box)
    {
        Control inside = this.Panel(box);
        var line = new Label { Size = inside.Size, VerticalAlignment = VerticalAlignment.Center };
        inside.AddChild(line);
        return line;
    }

    /// <summary>Builds one window frame at a box, and gives the control inside its edge.</summary>
    private Control Panel(FrameBox box)
    {
        var panel = new Panel
        {
            Position = new Vector2(box.X, box.Y),
            Size = new Vector2(box.Width, box.Height),
            MouseFilter = Control.MouseFilterEnum.Ignore,
        };
        var inside = new Control
        {
            Position = new Vector2(BattleLayout.PanelEdge, BattleLayout.PanelEdge),
            Size = new Vector2(box.Width - (BattleLayout.PanelEdge * 2), box.Height - (BattleLayout.PanelEdge * 2)),
            MouseFilter = Control.MouseFilterEnum.Ignore,
        };

        panel.AddChild(inside);
        this.layer.AddChild(panel);

        // A container sizes each child to itself, so each row takes the whole inside.
        return inside;
    }

    private StatusLine BuildStatusLine()
    {
        // Half a body between the parts, so a name of 16 characters and the health of the
        // largest stat fit beside four icons (D-241, D-775).
        var row = new HBoxContainer();
        row.AddThemeConstantOverride("separation", this.ui.Theme.BodySize / 2);
        var name = new Label();
        var health = new Label();
        var icons = new HBoxContainer();
        icons.AddThemeConstantOverride("separation", 0);
        row.AddChild(name);
        row.AddChild(health);
        row.AddChild(icons);
        return new StatusLine(row, name, health, icons);
    }

    private void PlaceStatusLines()
    {
        Control inside = this.Panel(BattleLayout.Status);
        var lines = new VBoxContainer { Size = inside.Size };
        lines.AddThemeConstantOverride("separation", 0);
        foreach (StatusLine line in this.statusLines)
        {
            lines.AddChild(line.Row);
        }

        inside.AddChild(lines);
    }

    private void FollowCommands(GameRun run)
    {
        if (!run.TakesBattleCommand)
        {
            this.commands = null;
            this.sentCommand = false;
            return;
        }

        if (this.commands is null && !this.sentCommand)
        {
            this.commands = BattleCommands.Open(run.State, this.memory);
        }
    }

    private void ShowCombatant(BattleView view, ShownCombatant shown, CombatantNodes nodes, BattleEvent? playing, int ticks)
    {
        // A waiting enemy and an enemy that went down hold no place, and they draw nothing (D-758).
        bool visible = BattleLayout.HoldsPlace(shown);
        nodes.Sprite.Visible = visible;
        if (nodes.Bar is HealthBar hidden)
        {
            hidden.Border.Visible = visible;
        }

        if (nodes.Icons is HBoxContainer hiddenIcons)
        {
            hiddenIcons.Visible = visible;
        }

        if (!visible)
        {
            return;
        }

        FieldPlace place = BattleLayout.PlaceOf(view, shown);
        bool acts = playing is not null && playing.Actor == shown.Target && BattleTimes.Poses(this.pace, playing.Kind, ticks);
        int x = place.X + (acts ? BattleLayout.LungeOf(this.pace, shown.Target.Side) : 0);
        nodes.Sprite.Position = new Vector2(x, place.Feet);
        nodes.Sprite.Texture = acts && nodes.Pose is not null ? nodes.Pose : nodes.Idle;

        bool struck = playing is not null && playing.Target == shown.Target && BattleTimes.Flashes(this.pace, playing.Kind, ticks);
        nodes.Material.SetShaderParameter(FlashAmount, struck ? 1.0f : 0.0f);

        // A character who went down stays on the field, dim, until the down pose of PR-17
        // (D-200, D-828).
        nodes.Sprite.Modulate = shown.Place == CombatantPlace.Down ? this.dimColor : Colors.White;

        if (nodes.Bar is HealthBar bar)
        {
            bar.Border.Position = new Vector2(place.X - (BattleLayout.BarWidth / 2), place.Feet + BattleLayout.BarGap);
            bar.Fill.Size = new Vector2(BattleLayout.BarFill(shown.Health, shown.FullHealth), BattleLayout.BarHeight - 2);
        }

        if (nodes.Icons is HBoxContainer icons)
        {
            this.FillIcons(icons, shown.Statuses, nodes);
            int top = (place.Feet + BattleLayout.BarGap + BattleLayout.BarHeight + 1) * FrameRoot.WorldScale;
            icons.Position = new Vector2((place.X * FrameRoot.WorldScale) - (shown.Statuses.Count * BattleLayout.IconSize / 2), top);
        }
    }

    private void ShowStatusLine(ShownCombatant shown, StatusLine line)
    {
        this.FillIcons(line.Icons, shown.Statuses, line);
        string key = $"{shown.Health}/{shown.FullHealth}/{shown.Place}";
        if (string.CompareOrdinal(key, line.ShownText) == 0)
        {
            return;
        }

        line.ShownText = key;
        this.ui.Text.Put(line.Name, BattleMessages.NameIdOf(shown.Id));
        this.ui.Text.Put(line.Health, ContentId.Parse("battle.health", StringTable.Path, "battle screen"), Values(
            ("health", shown.Health.ToString(CultureInfo.InvariantCulture)),
            ("full", shown.FullHealth.ToString(CultureInfo.InvariantCulture))));
        line.Name.Modulate = shown.Place == CombatantPlace.Down ? this.dimColor : Colors.White;
    }

    /// <summary>Puts one icon for each status, and builds the icons again only when the statuses changed (D-830).</summary>
    private void FillIcons(HBoxContainer icons, IReadOnlyList<StatusKind> statuses, IconHolder holder)
    {
        string key = string.Join(",", statuses);
        if (string.CompareOrdinal(key, holder.ShownStatuses) == 0)
        {
            return;
        }

        holder.ShownStatuses = key;
        foreach (Node old in icons.GetChildren())
        {
            icons.RemoveChild(old);
            old.QueueFree();
        }

        foreach (StatusKind status in statuses)
        {
            ContentId thing = ContentId.Parse($"status.{Core.Battles.Statuses.NameOf(status)}", AtlasIndex.Path, "battle screen");
            icons.AddChild(new TextureRect
            {
                Texture = this.ui.Atlas.Frame(this.DrawingOf(thing, IconUse), 0),
                CustomMinimumSize = new Vector2(BattleLayout.IconSize, BattleLayout.IconSize),
                ExpandMode = TextureRect.ExpandModeEnum.IgnoreSize,
                StretchMode = TextureRect.StretchModeEnum.Scale,
                TextureFilter = CanvasItem.TextureFilterEnum.Nearest,
            });
        }
    }

    private void ShowMessage(BattleView view, BattleEvent? playing)
    {
        if (playing is null || ReferenceEquals(playing, this.shownEvent))
        {
            return;
        }

        this.shownEvent = playing;
        if (BattleMessages.Of(playing, view, this.content.Strings) is BattleLine line)
        {
            this.ui.Text.Put(this.message, line.Id, line.Values);
        }
    }

    private void ShowNumber(BattleView view, BattleEvent? playing, int ticks)
    {
        BattleTarget? over = playing is null ? null : NumberTargetOf(playing);
        int? rise = over is null ? null : BattleTimes.NumberRise(this.pace, ticks);
        if (playing is null || over is not BattleTarget target || rise is not int height)
        {
            this.number.Visible = false;
            return;
        }

        ShownCombatant shown = view.At(target);
        CombatantNodes nodes = target.Side == BattleSide.Party ? this.party[target.Slot] : this.enemies[target.Slot];
        FieldPlace place = BattleLayout.PlaceOf(view, shown);
        this.ui.Text.Put(this.number, ContentId.Parse("battle.number", StringTable.Path, "battle screen"), Values(
            ("amount", playing.Amount.ToString(CultureInfo.InvariantCulture))));

        int headTop = (place.Feet - nodes.Height) * FrameRoot.WorldScale;
        this.number.Position = new Vector2(
            (place.X * FrameRoot.WorldScale) - (NumberWidth / 2),
            headTop - this.ui.Theme.BodySize - height);
        this.number.Visible = true;
    }

    /// <summary>Gives the combatant that a number of this event stands over, or no value (D-213).</summary>
    private static BattleTarget? NumberTargetOf(BattleEvent playing) => playing.Kind switch
    {
        BattleEventKind.Hit or BattleEventKind.Absorb or BattleEventKind.Item => playing.Target,
        BattleEventKind.StatusHurt or BattleEventKind.StatusHeal => playing.Actor,
        _ => null,
    };

    private void ShowStrip(GameRun run)
    {
        Battle battle = run.State.Battle ?? throw new InvalidOperationException(
            $"The battle screen draws the strip at tick {run.Tick}, and no battle runs (T-2).");

        IReadOnlyList<BattleTarget> turns = battle.Strip(run.State.BattleContent.Rules, run.State.Context("battle/strip"));
        for (int turn = 0; turn < this.faces.Length; turn += 1)
        {
            TextureRect face = this.faces[turn];
            if (turn >= turns.Count)
            {
                face.Texture = null;
                continue;
            }

            BattleTarget target = turns[turn];
            CombatantNodes nodes = target.Side == BattleSide.Party ? this.party[target.Slot] : this.enemies[target.Slot];
            face.Texture = nodes.Idle;
        }
    }

    private void ShowCommands(BattleView view)
    {
        BattleCommands? open = this.commands;
        this.pointer.Visible = false;
        if (open is null)
        {
            this.ShowCommandRow(string.Empty, []);
            return;
        }

        var entries = new List<(ContentId Id, IReadOnlyDictionary<string, string> Values, bool Allowed)>();
        switch (open.Stage)
        {
            case CommandStage.Action:
                foreach (BattleAction action in BattleCommands.Actions)
                {
                    entries.Add((CommandIdOf(action, open.ActorRow), Values(), open.Allows(action)));
                }

                break;
            case CommandStage.Item:
                foreach (PackValues entry in open.Items)
                {
                    entries.Add((
                        ContentId.Parse("battle.item_entry", StringTable.Path, "battle screen"),
                        Values(
                            ("item", this.content.Strings.Text(BattleMessages.NameIdOf(entry.Item))),
                            ("count", entry.Count.ToString(CultureInfo.InvariantCulture))),
                        true));
                }

                break;
            default:
                BattleTarget pointed = open.PointedTarget ?? throw new InvalidOperationException(
                    "The command menu stands in the target stage and points at no target (T-2).");
                entries.Add((BattleMessages.NameIdOf(view.At(pointed).Id), Values(), true));
                this.ShowPointer(view, pointed);
                break;
        }

        string key = $"{open.Stage}:{open.Cursor}:{string.Join(",", entries.ConvertAll(entry => entry.Id.Value + entry.Allowed))}";
        this.ShowCommandRow(key, entries, open.Stage == CommandStage.Target ? 0 : open.Cursor);
    }

    private void ShowCommandRow(
        string key,
        List<(ContentId Id, IReadOnlyDictionary<string, string> Values, bool Allowed)> entries,
        int cursor = -1)
    {
        if (string.CompareOrdinal(key, this.shownCommands) == 0)
        {
            return;
        }

        this.shownCommands = key;
        foreach (Node old in this.commandRow.GetChildren())
        {
            this.commandRow.RemoveChild(old);
            old.QueueFree();
        }

        for (int index = 0; index < entries.Count; index += 1)
        {
            var label = new Label { VerticalAlignment = VerticalAlignment.Center };
            this.ui.Text.Put(label, entries[index].Id, entries[index].Values);
            if (index == cursor)
            {
                label.AddThemeColorOverride("font_color", this.chosenColor);
            }
            else if (!entries[index].Allowed)
            {
                label.AddThemeColorOverride("font_color", this.dimColor);
            }

            this.commandRow.AddChild(label);
        }
    }

    private void ShowPointer(BattleView view, BattleTarget pointed)
    {
        CombatantNodes nodes = pointed.Side == BattleSide.Party ? this.party[pointed.Slot] : this.enemies[pointed.Slot];
        FieldPlace place = BattleLayout.PlaceOf(view, view.At(pointed));
        int size = (int)this.pointer.Texture.GetSize().X;
        this.pointer.Position = new Vector2(
            place.X - (size / 2),
            place.Feet - nodes.Height - BattleLayout.PointerGap - size);
        this.pointer.Visible = true;
    }

    /// <summary>Gives the label of an action. The step names the move from the row of the actor (D-836).</summary>
    private static ContentId CommandIdOf(BattleAction action, BattleRow row) => ContentId.Parse(
        action switch
        {
            BattleAction.Attack => "battle.command_attack",
            BattleAction.Defend => "battle.command_defend",
            BattleAction.Step => row == BattleRow.Front ? "battle.command_back_up" : "battle.command_step_forward",
            BattleAction.Item => "battle.command_item",
            BattleAction.Flee => "battle.command_flee",
            _ => throw new ArgumentOutOfRangeException(nameof(action), action, $"The action '{action}' has no label (T-2)."),
        },
        StringTable.Path,
        "battle screen");

    private static IReadOnlyDictionary<string, string> Values(params (string Name, string Value)[] values)
    {
        var filled = new SortedDictionary<string, string>(StringComparer.Ordinal);
        foreach ((string name, string value) in values)
        {
            filled.Add(name, value);
        }

        return filled;
    }

    /// <summary>The icons of a combatant, and the statuses that they show now (D-830).</summary>
    private abstract class IconHolder
    {
        public string ShownStatuses { get; set; } = string.Empty;
    }

    private sealed class CombatantNodes(Sprite2D sprite, ShaderMaterial material, AtlasTexture idle, AtlasTexture? pose, int width, int height)
        : IconHolder
    {
        public Sprite2D Sprite { get; } = sprite;

        public ShaderMaterial Material { get; } = material;

        public AtlasTexture Idle { get; } = idle;

        public AtlasTexture? Pose { get; } = pose;

        public int Width { get; } = width;

        public int Height { get; } = height;

        public HealthBar? Bar { get; set; }

        public HBoxContainer? Icons { get; set; }
    }

    private sealed record HealthBar(ColorRect Border, ColorRect Fill);

    private sealed class StatusLine(HBoxContainer row, Label name, Label health, HBoxContainer icons) : IconHolder
    {
        public HBoxContainer Row { get; } = row;

        public Label Name { get; } = name;

        public Label Health { get; } = health;

        public HBoxContainer Icons { get; } = icons;

        public string ShownText { get; set; } = string.Empty;
    }
}
