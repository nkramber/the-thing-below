using System;
using System.Collections.Generic;
using System.Globalization;
using Godot;
using TheThingBelow.Core.Content;
using TheThingBelow.Core.Runs;

namespace TheThingBelow.Game.Ui;

/// <summary>
/// The gold of the party, in a small panel under a window of the left column: the main list, the
/// shop menu, or the rest window (D-1160).
/// </summary>
/// <remarks>
/// The panel reads the gold of the run on each show, so a buy, a sale, or a rest shows on the frame
/// after the tick that made it (D-493). The text comes from the string table through the text
/// helper (G-7, D-499).
/// </remarks>
public sealed class GoldPanel
{
    private static readonly ContentId GoldId = ContentId.Parse("menu.gold", StringTable.Path, nameof(GoldPanel));

    private readonly UiBase ui;
    private readonly RunState state;
    private readonly Label line;

    /// <summary>Builds the panel under a window.</summary>
    /// <param name="layer">The layer of the window, which frees the panel with it.</param>
    /// <param name="ui">The atlas, the theme, and the text helper.</param>
    /// <param name="state">The state of the run, whose gold the panel reads.</param>
    /// <param name="above">The place of the window above the panel.</param>
    /// <exception cref="ArgumentNullException">An argument is null (T-2).</exception>
    public GoldPanel(Control layer, UiBase ui, RunState state, FrameBox above)
    {
        ArgumentNullException.ThrowIfNull(layer);
        ArgumentNullException.ThrowIfNull(ui);
        ArgumentNullException.ThrowIfNull(state);

        this.ui = ui;
        this.state = state;
        int body = ui.Theme.BodySize;
        FrameBox box = MenuLayout.GoldBox(above, body);
        MenuNodes.Panel(layer, box);
        this.line = MenuNodes.Line(layer, box.X + MenuLayout.Pad, box.Y + MenuLayout.Pad, box.Width - (MenuLayout.Pad * 2), MenuLayout.LineOf(body));
        this.Show();
    }

    /// <summary>Puts the gold of the run now.</summary>
    public void Show() =>
        this.ui.Text.Put(this.line, GoldId, new Dictionary<string, string>(StringComparer.Ordinal) { ["gold"] = this.state.Characters.Gold.ToString(CultureInfo.InvariantCulture) });
}
