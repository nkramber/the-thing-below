using System;
using System.Collections.Generic;
using System.Globalization;
using Godot;
using TheThingBelow.Core.Content;
using TheThingBelow.Core.Maps;
using TheThingBelow.Core.Runs;

namespace TheThingBelow.Game.Ui;

/// <summary>
/// The window of a hub service: the rest or the save, leave, and the line of help of the choice
/// under the cursor (D-390, D-1131, D-1132). The rest window and the save window share it.
/// </summary>
/// <remarks>
/// A confirm on the host of a service opened the menu in the rules, so the world holds while the
/// window is open (D-162, D-1131). The window stands at the top left, where the main list stands,
/// so the map stays visible to its right (D-211). Every label takes its text from the string
/// table through the text helper (G-7, D-499).
/// </remarks>
public sealed class ServiceView : IMenuView
{
    private readonly UiBase ui;
    private readonly RunState state;
    private readonly Control layer;
    private readonly Label help;
    private readonly List<Label> lines = [];
    private readonly Color chosenColor;
    private readonly Color dimColor;
    private readonly GoldPanel? gold;
    private Intent? made;

    /// <summary>Builds the window of a service, with the cursor on the service.</summary>
    /// <param name="frame">The frame, whose UI layer takes the window.</param>
    /// <param name="ui">The atlas, the theme, and the text helper.</param>
    /// <param name="state">The state of the run, whose gold a rest reads (D-1156, D-1160).</param>
    /// <param name="choice">The cursor, which the window keeps when the screen builds again.</param>
    /// <exception cref="ArgumentNullException">An argument is null (T-2).</exception>
    /// <remarks>A rest with a price names the price on its line, and the gold of the party stands in a panel under the window (D-1156, D-1160).</remarks>
    public ServiceView(FrameRoot frame, UiBase ui, RunState state, ServiceChoice choice)
    {
        ArgumentNullException.ThrowIfNull(frame);
        ArgumentNullException.ThrowIfNull(ui);
        ArgumentNullException.ThrowIfNull(state);
        ArgumentNullException.ThrowIfNull(choice);

        this.ui = ui;
        this.state = state;
        this.Choice = choice;
        this.chosenColor = ui.Theme.ColorOf("text_chosen");
        this.dimColor = ui.Theme.ColorOf("text_dim");
        this.layer = MenuNodes.Layer(frame, ui);

        int body = ui.Theme.BodySize;
        FrameBox box = MenuLayout.ServiceBox(body);
        MenuNodes.Panel(this.layer, box);
        int line = MenuLayout.LineOf(body);
        int left = box.X + MenuLayout.Pad;
        int inner = box.Width - (MenuLayout.Pad * 2);
        for (int index = 0; index < ServiceChoice.Options.Count; index += 1)
        {
            Label label = MenuNodes.Line(this.layer, left, box.Y + MenuLayout.Pad + (index * line), inner, line);
            ServiceOption option = ServiceChoice.Options[index];
            if (option == ServiceOption.Use && choice.Price is int price && price > 0)
            {
                ui.Text.Put(label, Id("menu.rest_priced"), new Dictionary<string, string>(StringComparer.Ordinal) { ["price"] = price.ToString(CultureInfo.InvariantCulture) });
            }
            else
            {
                ui.Text.Put(label, LabelOf(choice.Kind, option));
            }

            this.lines.Add(label);
        }

        this.help = MenuNodes.Line(this.layer, left, box.Y + MenuLayout.Pad + (ServiceChoice.Options.Count * line), inner, line);
        MenuNodes.Paint(this.help, this.dimColor);
        this.gold = choice.Kind == ServiceKind.Rest ? new GoldPanel(this.layer, ui, state, box) : null;
        this.Show();
    }

    /// <summary>The choices and the cursor.</summary>
    public ServiceChoice Choice { get; }

    /// <summary>Gives the string id of the label of one choice of a service (G-7).</summary>
    /// <param name="kind">The kind of the service.</param>
    /// <param name="option">The choice.</param>
    /// <returns>The id, such as `menu.rest`.</returns>
    /// <exception cref="ArgumentOutOfRangeException">A value names no kind or no choice (T-2).</exception>
    public static ContentId LabelOf(ServiceKind kind, ServiceOption option) => Id(option switch
    {
        ServiceOption.Use => UseIdOf(kind),
        ServiceOption.Leave => "menu.leave",
        _ => throw new ArgumentOutOfRangeException(nameof(option), option, "The window of a service holds no such choice (T-2)."),
    });

    /// <summary>Gives the string id of the line of help of one choice of a service (G-7).</summary>
    /// <param name="kind">The kind of the service.</param>
    /// <param name="option">The choice.</param>
    /// <returns>The id, such as `menu.rest_help`.</returns>
    /// <exception cref="ArgumentOutOfRangeException">A value names no kind or no choice (T-2).</exception>
    public static ContentId HelpOf(ServiceKind kind, ServiceOption option) => Id(option switch
    {
        ServiceOption.Use => $"{UseIdOf(kind)}_help",
        ServiceOption.Leave => "menu.leave_help",
        _ => throw new ArgumentOutOfRangeException(nameof(option), option, "The window of a service holds no such choice (T-2)."),
    });

    /// <summary>Takes the intent of the last whole choice, once.</summary>
    /// <returns>The rest intent or the save intent, or no value when the last read made none.</returns>
    public Intent? TakeIntent()
    {
        Intent? taken = this.made;
        this.made = null;
        return taken;
    }

    /// <inheritdoc/>
    public ViewOutcome Read(InputEvent signal, ScreenFit fit)
    {
        ArgumentNullException.ThrowIfNull(signal);
        ArgumentNullException.ThrowIfNull(fit);

        ViewOutcome outcome = this.ReadEvent(signal, fit);
        this.Show();
        return outcome;
    }

    /// <inheritdoc/>
    public void Show()
    {
        bool refused = this.Choice.Refuses(this.state.Characters.Gold);
        for (int index = 0; index < this.lines.Count; index += 1)
        {
            MenuNodes.Paint(this.lines[index], index == this.Choice.Cursor ? this.chosenColor : null);
        }

        this.ui.Text.Put(this.help, refused ? Id("menu.rest_short") : HelpOf(this.Choice.Kind, this.Choice.Current));
        MenuNodes.Paint(this.help, refused ? this.ui.Theme.ColorOf("text_warning") : this.dimColor);
        this.gold?.Show();
    }

    /// <inheritdoc/>
    public void Free() => this.layer.QueueFree();

    private static string UseIdOf(ServiceKind kind) => kind switch
    {
        ServiceKind.Rest => "menu.rest",
        ServiceKind.Save => "menu.save",
        _ => throw new ArgumentOutOfRangeException(nameof(kind), kind, "The window opens for a rest service or a save service (D-1131, T-2)."),
    };

    private static ContentId Id(string value) => ContentId.Parse(value, StringTable.Path, nameof(ServiceView));

    private ViewOutcome ReadEvent(InputEvent signal, ScreenFit fit)
    {
        if (signal is InputEventMouse mouse)
        {
            if (MenuNodes.LineUnder(this.lines, mouse, fit) is not int place)
            {
                return ViewOutcome.Stay;
            }

            this.Choice.Point(place);
            return MenuNodes.IsClick(mouse) ? this.Confirm() : ViewOutcome.Stay;
        }

        if (signal.IsActionPressed("ui_up"))
        {
            this.Choice.Move(-1);
        }
        else if (signal.IsActionPressed("ui_down"))
        {
            this.Choice.Move(1);
        }
        else if (signal.IsActionPressed("ui_accept"))
        {
            return this.Confirm();
        }
        else if (signal.IsActionPressed("ui_cancel"))
        {
            return ViewOutcome.Back;
        }

        return ViewOutcome.Stay;
    }

    /// <summary>
    /// Confirms the choice under the cursor: the service closes the window with its intent, and leave
    /// closes it with none. A rest that the gold cannot pay stays, and the line of help says why (D-1156).
    /// </summary>
    private ViewOutcome Confirm()
    {
        if (this.Choice.Refuses(this.state.Characters.Gold))
        {
            return ViewOutcome.Stay;
        }

        this.made = this.Choice.Confirm();
        return this.made is null ? ViewOutcome.Back : ViewOutcome.Chose;
    }
}
