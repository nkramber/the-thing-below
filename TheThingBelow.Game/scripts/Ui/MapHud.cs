using System;
using System.Collections.Generic;
using Godot;
using TheThingBelow.Core.Battles;
using TheThingBelow.Core.Content;

namespace TheThingBelow.Game.Ui;

/// <summary>
/// The map HUD at the top left of the frame: a face, a health bar, and the icon of each lasting
/// status, for each character who fights (D-212, D-1237, D-1239).
/// </summary>
/// <remarks>
/// The block shows while a character who fights lacks health, is down, or holds poison, blind, or
/// silence, and it fades out when none is. The fade counts the world ticks of the run, so the HUD
/// reads no clock of the engine (T-7). Exact numbers stay in the menu (D-211). The block sits under
/// the notice box, so a notice never covers it (D-221).
/// </remarks>
public sealed class MapHud
{
    /// <summary>The count of world ticks of a fade in or a fade out.</summary>
    public const int FadeTicks = 20;

    /// <summary>The size of a face, in frame pixels. A face draws at 1x, as the map sprites do (D-1239).</summary>
    public const int FacePixels = 32;

    /// <summary>The width of a health bar with its border, in frame pixels.</summary>
    public const int BarWidth = 64;

    /// <summary>The height of a health bar with its border, in frame pixels.</summary>
    public const int BarHeight = 8;

    /// <summary>The size of a status icon, in frame pixels, at 1x (D-214).</summary>
    public const int IconPixels = 16;

    /// <summary>The gap between the face, the bar, and the icons, and between two rows, in frame pixels.</summary>
    public const int Gap = 4;

    /// <summary>The use of a face drawing in the atlas (D-1239).</summary>
    public const string FaceUse = "face";

    private readonly UiBase ui;
    private readonly Control layer;
    private readonly List<HudRow> rows = [];
    private bool wanted;
    private long changedAt;
    private bool started;

    /// <summary>Builds the HUD over the frame, hidden.</summary>
    /// <param name="frame">The frame, whose UI layer takes the HUD.</param>
    /// <param name="ui">The atlas, the theme, and the text helper.</param>
    /// <exception cref="ArgumentNullException">An argument is null (T-2).</exception>
    public MapHud(FrameRoot frame, UiBase ui)
    {
        ArgumentNullException.ThrowIfNull(frame);
        ArgumentNullException.ThrowIfNull(ui);

        this.ui = ui;
        FrameBox notice = MenuLayout.NoticePlace(ui.Theme.BodySize);
        this.layer = new Control
        {
            Position = new Vector2(UiMetrics.EdgePixels, notice.Y + notice.Height + MenuLayout.WindowGap),
            MouseFilter = Control.MouseFilterEnum.Ignore,
            Visible = false,
        };
        frame.Layer.AddChild(this.layer);
    }

    /// <summary>The share of full opacity of the HUD now, from 0 to 1000, for a test and a capture.</summary>
    public int Opacity { get; private set; }

    /// <summary>Tells whether the HUD wants to show for one party (D-1237).</summary>
    /// <param name="party">The party.</param>
    /// <returns>True when a character who fights lacks health, is down, or holds a lasting status.</returns>
    /// <exception cref="ArgumentNullException">The party is null (T-2).</exception>
    public static bool Wanted(PartyState party)
    {
        ArgumentNullException.ThrowIfNull(party);

        foreach (PartyMember member in party.Members)
        {
            if (member.Health < member.Stats.Health || member.Statuses.Count > 0)
            {
                return true;
            }
        }

        return false;
    }

    /// <summary>Draws one frame of the HUD.</summary>
    /// <param name="party">The characters of the run.</param>
    /// <param name="worldTick">The count of world ticks of the run, which the fade counts (D-650).</param>
    /// <param name="hidden">True while a menu, a battle, or a story scene covers the map, which hides the HUD.</param>
    /// <exception cref="ArgumentNullException">The party is null (T-2).</exception>
    /// <exception cref="ContentException">The atlas holds no face of a character or no icon of a status (T-2, D-519).</exception>
    /// <remarks>The first frame takes the wanted state at once, so a capture shows no fade.</remarks>
    public void Show(PartyState party, long worldTick, bool hidden)
    {
        ArgumentNullException.ThrowIfNull(party);

        bool want = Wanted(party);
        if (!this.started)
        {
            this.started = true;
            this.wanted = want;
            this.changedAt = worldTick - FadeTicks;
        }
        else if (want != this.wanted)
        {
            this.wanted = want;
            this.changedAt = worldTick;
        }

        long since = Math.Clamp(worldTick - this.changedAt, 0, FadeTicks);
        int reached = (int)(since * 1000 / FadeTicks);
        this.Opacity = this.wanted ? reached : 1000 - reached;
        if (hidden || this.Opacity == 0)
        {
            this.layer.Visible = false;
            return;
        }

        this.Fill(party.Members);
        this.layer.Modulate = new Color(1f, 1f, 1f, this.Opacity / 1000f);
        this.layer.Visible = true;
    }

    /// <summary>
    /// Gives the width of the fill of a health bar, in frame pixels. A character with any health
    /// left shows one pixel at least, so a live character never reads as down (D-826).
    /// </summary>
    /// <param name="health">The health now.</param>
    /// <param name="fullHealth">The full health.</param>
    /// <returns>The width, from zero to the inside of the bar.</returns>
    /// <exception cref="ArgumentOutOfRangeException">The full health is not above zero, or the health is outside it (T-2).</exception>
    public static int FillOf(int health, int fullHealth)
    {
        ArgumentOutOfRangeException.ThrowIfLessThan(fullHealth, 1);
        ArgumentOutOfRangeException.ThrowIfNegative(health);
        ArgumentOutOfRangeException.ThrowIfGreaterThan(health, fullHealth);

        int inside = BarWidth - 2;
        return health == 0 ? 0 : Math.Max(1, (int)((long)health * inside / fullHealth));
    }

    /// <summary>Removes every node of the HUD.</summary>
    public void Free() => this.layer.QueueFree();

    private void Fill(IReadOnlyList<PartyMember> members)
    {
        while (this.rows.Count > members.Count)
        {
            this.rows[^1].Root.QueueFree();
            this.rows.RemoveAt(this.rows.Count - 1);
        }

        for (int slot = 0; slot < members.Count; slot += 1)
        {
            if (slot == this.rows.Count)
            {
                this.rows.Add(this.BuildRow(slot));
            }

            this.ShowRow(this.rows[slot], members[slot]);
        }
    }

    private HudRow BuildRow(int slot)
    {
        var root = new Control
        {
            Position = new Vector2(0, slot * (FacePixels + Gap)),
            MouseFilter = Control.MouseFilterEnum.Ignore,
        };
        this.layer.AddChild(root);

        var face = new TextureRect
        {
            Size = new Vector2(FacePixels, FacePixels),
            ExpandMode = TextureRect.ExpandModeEnum.IgnoreSize,
            StretchMode = TextureRect.StretchModeEnum.Scale,
            TextureFilter = CanvasItem.TextureFilterEnum.Nearest,
        };
        root.AddChild(face);

        float barTop = (FacePixels - BarHeight) / 2;
        var border = new ColorRect
        {
            Color = this.ui.Theme.ColorOf("bar_border"),
            Position = new Vector2(FacePixels + Gap, barTop),
            Size = new Vector2(BarWidth, BarHeight),
        };
        border.AddChild(new ColorRect
        {
            Color = this.ui.Theme.ColorOf("bar_empty"),
            Position = Vector2.One,
            Size = new Vector2(BarWidth - 2, BarHeight - 2),
        });
        var fill = new ColorRect
        {
            Color = this.ui.Theme.ColorOf("bar_fill"),
            Position = Vector2.One,
            Size = new Vector2(BarWidth - 2, BarHeight - 2),
        };
        border.AddChild(fill);
        root.AddChild(border);

        var icons = new HBoxContainer
        {
            Position = new Vector2(FacePixels + Gap + BarWidth + Gap, (FacePixels - IconPixels) / 2),
            MouseFilter = Control.MouseFilterEnum.Ignore,
        };
        icons.AddThemeConstantOverride("separation", 0);
        root.AddChild(icons);

        return new HudRow(root, face, fill, icons);
    }

    private void ShowRow(HudRow row, PartyMember member)
    {
        if (row.Shown is null || string.CompareOrdinal(row.Shown.Value, member.Record.Id.Value) != 0)
        {
            row.Face.Texture = this.ui.Atlas.Frame(this.ui.Atlas.Index.Entry(member.Record.Id, FaceUse).Id, 0);
            row.Shown = member.Record.Id;
        }

        // A down character shows an empty gray bar and a dim face (D-1237).
        row.Face.Modulate = member.Down ? new Color(0.45f, 0.45f, 0.45f) : Colors.White;
        row.Fill.Size = new Vector2(FillOf(member.Health, member.Stats.Health), BarHeight - 2);

        string key = string.Join(",", member.Statuses);
        if (string.CompareOrdinal(key, row.ShownStatuses) == 0)
        {
            return;
        }

        row.ShownStatuses = key;
        foreach (Node old in row.Icons.GetChildren())
        {
            row.Icons.RemoveChild(old);
            old.QueueFree();
        }

        foreach (StatusKind status in member.Statuses)
        {
            ContentId thing = ContentId.Parse($"status.{Statuses.NameOf(status)}", AtlasIndex.Path, "map HUD");
            row.Icons.AddChild(new TextureRect
            {
                Texture = this.ui.Atlas.Frame(this.ui.Atlas.Index.Entry(thing, BattleScreen.IconUse).Id, 0),
                CustomMinimumSize = new Vector2(IconPixels, IconPixels),
                ExpandMode = TextureRect.ExpandModeEnum.IgnoreSize,
                StretchMode = TextureRect.StretchModeEnum.Scale,
                TextureFilter = CanvasItem.TextureFilterEnum.Nearest,
            });
        }
    }

    /// <summary>The nodes of one row, and what they show now.</summary>
    private sealed class HudRow(Control root, TextureRect face, ColorRect fill, HBoxContainer icons)
    {
        public Control Root { get; } = root;

        public TextureRect Face { get; } = face;

        public ColorRect Fill { get; } = fill;

        public HBoxContainer Icons { get; } = icons;

        public ContentId? Shown { get; set; }

        public string ShownStatuses { get; set; } = string.Empty;
    }
}
