using System;
using System.Collections.Generic;
using Godot;
using TheThingBelow.Core.Content;
using TheThingBelow.Core.Story;

namespace TheThingBelow.Game.Ui;

/// <summary>
/// The dialogue box of a story scene: the line at the bottom of the frame, the portrait with the
/// name plate over it above the left end, and the choices above the right end (D-114, D-223, D-1175).
/// </summary>
/// <remarks>
/// <see cref="ScenePlay"/> gives each value from the ticks of the run, so the box reads no clock of
/// the engine (T-7). The line types in silence, and it holds its layout as it types, so no word
/// moves (D-223, D-709). Each text goes through the one text helper (G-7, D-499).
/// </remarks>
public sealed class DialogueBox
{
    private readonly UiBase ui;
    private readonly Control layer;
    private readonly Label line;
    private readonly Control portraitWindow;
    private readonly Label name;
    private readonly TextureRect portrait;
    private readonly Control choiceWindow;
    private readonly Panel choicePanel;
    private readonly List<Label> choices = [];
    private readonly Color chosenColor;
    private readonly int body;
    private ContentId? shownLine;
    private string? shownSpeaker;
    private IReadOnlyList<ChooseOption>? shownOptions;

    /// <summary>Builds the dialogue box over the frame, hidden.</summary>
    /// <param name="frame">The frame, whose UI layer takes the box.</param>
    /// <param name="ui">The atlas, the theme, and the text helper.</param>
    /// <exception cref="ArgumentNullException">An argument is null (T-2).</exception>
    public DialogueBox(FrameRoot frame, UiBase ui)
    {
        ArgumentNullException.ThrowIfNull(frame);
        ArgumentNullException.ThrowIfNull(ui);

        this.ui = ui;
        this.body = ui.Theme.BodySize;
        this.chosenColor = ui.Theme.ColorOf("text_chosen");
        this.layer = MenuNodes.Layer(frame, ui);
        this.layer.Visible = false;
        int lineHeight = MenuLayout.LineOf(this.body);

        FrameBox box = MenuLayout.DialogueBox(this.body);
        MenuNodes.Panel(this.layer, box);
        this.line = MenuNodes.Line(
            this.layer,
            box.X + MenuLayout.NoticePad,
            box.Y + MenuLayout.NoticePad,
            box.Width - (MenuLayout.NoticePad * 2),
            lineHeight * MenuLayout.DialogueLines);
        this.line.AutowrapMode = TextServer.AutowrapMode.WordSmart;
        this.line.VisibleCharactersBehavior = TextServer.VisibleCharactersBehavior.CharsAfterShaping;

        FrameBox place = MenuLayout.PortraitBox(this.body);
        this.portraitWindow = Window(this.layer, place);
        MenuNodes.Panel(this.portraitWindow, new FrameBox(0, 0, place.Width, place.Height));
        this.name = MenuNodes.Line(this.portraitWindow, MenuLayout.NoticePad, MenuLayout.NoticePad, place.Width - (MenuLayout.NoticePad * 2), lineHeight);
        this.portrait = new TextureRect
        {
            Position = new Vector2((place.Width - MenuLayout.PortraitPixels) / 2, MenuLayout.NoticePad + lineHeight + MenuLayout.LineGap),
            ExpandMode = TextureRect.ExpandModeEnum.IgnoreSize,
            Size = new Vector2(MenuLayout.PortraitPixels, MenuLayout.PortraitPixels),
            StretchMode = TextureRect.StretchModeEnum.Scale,
            TextureFilter = CanvasItem.TextureFilterEnum.Nearest,
            MouseFilter = Control.MouseFilterEnum.Ignore,
        };
        this.portraitWindow.AddChild(this.portrait);

        // The window of the choices takes the size of four rows, and each choice sizes it again.
        FrameBox most = MenuLayout.ChoiceBox(this.body, ChooseStep.MostOptions);
        this.choiceWindow = Window(this.layer, most);
        this.choicePanel = new Panel { MouseFilter = Control.MouseFilterEnum.Ignore };
        this.choiceWindow.AddChild(this.choicePanel);
        for (int row = 0; row < ChooseStep.MostOptions; row += 1)
        {
            this.choices.Add(MenuNodes.Line(this.layer, 0, 0, MenuLayout.ChoiceWidth - (MenuLayout.Pad * 2), lineHeight));
        }
    }

    /// <summary>True while the box shows a line.</summary>
    public bool Shown => this.layer.Visible;

    /// <summary>Draws the box for one frame.</summary>
    /// <param name="play">The story scene on screen.</param>
    /// <param name="hidden">True while another screen covers the map, such as a fight (D-999).</param>
    /// <exception cref="ArgumentNullException">The play is null (T-2).</exception>
    /// <exception cref="ContentException">The atlas holds no portrait of the speaker (T-2, D-234).</exception>
    public void Show(ScenePlay play, bool hidden)
    {
        ArgumentNullException.ThrowIfNull(play);

        if (hidden || play.Line is not ContentId shown)
        {
            this.layer.Visible = false;
            this.shownLine = null;
            return;
        }

        if (this.shownLine is null || string.CompareOrdinal(this.shownLine.Value, shown.Value) != 0)
        {
            this.ui.Text.Put(this.line, shown);
            this.shownLine = shown;
            this.ShowSpeaker(play.Speaker);
        }

        this.line.VisibleCharacters = play.Characters ?? -1;
        this.ShowChoices(play);
        this.layer.Visible = true;
    }

    /// <summary>Reads one mouse event over the choices: a move points at a row, and a click picks it (D-219).</summary>
    /// <param name="signal">The event.</param>
    /// <param name="fit">The fit of the frame, which turns the screen pixel into a frame pixel.</param>
    /// <param name="play">The story scene on screen.</param>
    /// <returns>True when the event was a click on a choice, which the host confirms.</returns>
    /// <exception cref="ArgumentNullException">An argument is null (T-2).</exception>
    public bool ReadMouse(InputEvent signal, ScreenFit fit, ScenePlay play)
    {
        ArgumentNullException.ThrowIfNull(signal);
        ArgumentNullException.ThrowIfNull(fit);
        ArgumentNullException.ThrowIfNull(play);

        if (signal is not InputEventMouse mouse || play.Options is not IReadOnlyList<ChooseOption> options)
        {
            return false;
        }

        List<Label> rows = this.choices.GetRange(0, options.Count);
        if (MenuNodes.LineUnder(rows, mouse, fit) is not int row)
        {
            return false;
        }

        play.PointAt(row);
        return MenuNodes.IsClick(mouse);
    }

    /// <summary>Removes every node of the box.</summary>
    public void Free() => this.layer.QueueFree();

    private static Control Window(Control layer, FrameBox place)
    {
        var window = new Control
        {
            Position = new Vector2(place.X, place.Y),
            Size = new Vector2(place.Width, place.Height),
            MouseFilter = Control.MouseFilterEnum.Ignore,
        };
        layer.AddChild(window);
        return window;
    }

    /// <summary>Shows the portrait and the name plate of the speaker, or hides them for a line with no speaker (D-223, D-997).</summary>
    private void ShowSpeaker(SceneActor? speaker)
    {
        if (speaker is null)
        {
            this.portraitWindow.Visible = false;
            this.shownSpeaker = null;
            return;
        }

        string key = speaker.Describe();
        if (string.CompareOrdinal(this.shownSpeaker, key) != 0)
        {
            AtlasEntry entry = this.ui.Atlas.Index.Entry(ScenePlay.ArtIdOf(speaker), ScenePlay.PortraitUse);
            this.portrait.Texture = this.ui.Atlas.Frame(entry.Id, 0);
            this.ui.Text.Put(this.name, ScenePlay.NameIdOf(speaker));
            this.shownSpeaker = key;
        }

        this.portraitWindow.Visible = true;
    }

    /// <summary>Shows the options of a choice, one on each row, with the option under the cursor in the chosen color (D-1175).</summary>
    private void ShowChoices(ScenePlay play)
    {
        IReadOnlyList<ChooseOption>? options = play.Options;
        int count = options?.Count ?? 0;
        this.choiceWindow.Visible = count > 0;
        if (!ReferenceEquals(options, this.shownOptions))
        {
            FrameBox place = MenuLayout.ChoiceBox(this.body, Math.Max(count, 1));
            this.choiceWindow.Position = new Vector2(place.X, place.Y);
            this.choicePanel.Size = new Vector2(place.Width, place.Height);
            int lineHeight = MenuLayout.LineOf(this.body);
            for (int row = 0; row < this.choices.Count; row += 1)
            {
                this.choices[row].Position = new Vector2(place.X + MenuLayout.Pad, place.Y + MenuLayout.Pad + (row * lineHeight));
                this.choices[row].Visible = row < count;
                if (options is not null && row < count)
                {
                    this.ui.Text.Put(this.choices[row], options[row].Line);
                }
            }

            this.shownOptions = options;
        }

        for (int row = 0; row < count; row += 1)
        {
            MenuNodes.Paint(this.choices[row], row == play.Cursor ? this.chosenColor : null);
        }
    }
}
