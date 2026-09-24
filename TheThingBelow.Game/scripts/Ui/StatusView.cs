using System;
using System.Collections.Generic;
using System.Globalization;
using Godot;
using TheThingBelow.Core.Battles;
using TheThingBelow.Core.Content;
using TheThingBelow.Core.Runs;

namespace TheThingBelow.Game.Ui;

/// <summary>
/// The status window: the full sheet of each character in its own column (D-569, D-991). The
/// sheet holds the name, the level, the row, HP, MP, the experience and the experience to the
/// next level, ATK, DEF, SPD, and each status that lasts.
/// </summary>
/// <remarks>
/// The window reads the state of the run and makes no intent. A stat names itself in three
/// characters, as on the battle screen (D-979). A character with no status that lasts shows the
/// word of <c>menu.sound</c> (D-390).
/// </remarks>
public sealed class StatusView : IMenuView
{
    /// <summary>The fixed lines of one column, before the statuses.</summary>
    private const int FixedLines = 10;

    /// <summary>The count of statuses that last past a fight: poison, blind, and silence (D-390).</summary>
    private static readonly int LastingCount = CountLasting();

    private readonly UiBase ui;
    private readonly StringTable strings;
    private readonly RunState state;
    private readonly Control layer;
    private readonly List<List<Label>> columns = [];

    /// <summary>Builds the status window beside the main list.</summary>
    /// <param name="frame">The frame, whose UI layer takes the window.</param>
    /// <param name="ui">The atlas, the theme, and the text helper.</param>
    /// <param name="strings">The string table, which gives the name of each stat (D-979).</param>
    /// <param name="state">The state of the run, which the window reads and never changes.</param>
    /// <exception cref="ArgumentNullException">An argument is null (T-2).</exception>
    public StatusView(FrameRoot frame, UiBase ui, StringTable strings, RunState state)
    {
        ArgumentNullException.ThrowIfNull(frame);
        ArgumentNullException.ThrowIfNull(ui);
        ArgumentNullException.ThrowIfNull(strings);
        ArgumentNullException.ThrowIfNull(state);

        this.ui = ui;
        this.strings = strings;
        this.state = state;
        this.layer = MenuNodes.Layer(frame, ui);

        int body = ui.Theme.BodySize;
        FrameBox box = MenuLayout.TaskBox();
        MenuNodes.Panel(this.layer, box);
        MenuNodes.Title(this.layer, ui, box, Id("menu.status"));

        int line = MenuLayout.LineOf(body);
        int width = (box.Width - (MenuLayout.Pad * 2)) / MenuLayout.StatusColumns;
        int top = MenuLayout.FirstLineTop(body, ui.Theme.TitleSize);
        int most = FixedLines + LastingCount;
        for (int slot = 0; slot < state.Characters.Members.Count; slot += 1)
        {
            List<Label> column = [];
            int left = box.X + MenuLayout.Pad + (slot * width);
            for (int row = 0; row < most; row += 1)
            {
                column.Add(MenuNodes.Line(this.layer, left, top + (row * line), width, line));
            }

            MenuNodes.Paint(column[0], ui.Theme.ColorOf("text_chosen"));
            this.columns.Add(column);
        }

        this.Show();
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
        IReadOnlyList<PartyMember> members = this.state.Characters.Members;
        IReadOnlyList<int> table = this.state.BattleContent.Rules.LevelExperience;
        for (int slot = 0; slot < this.columns.Count; slot += 1)
        {
            this.ShowColumn(this.columns[slot], members[slot], table);
        }
    }

    /// <inheritdoc/>
    public void Free() => this.layer.QueueFree();

    /// <summary>Gives the experience to the next level, or no value at the highest level (D-971, D-972).</summary>
    /// <param name="member">The character.</param>
    /// <param name="table">The total experience of each level, from level 1.</param>
    /// <returns>The experience that the character still needs.</returns>
    public static int? ToNextLevel(PartyMember member, IReadOnlyList<int> table)
    {
        ArgumentNullException.ThrowIfNull(member);
        ArgumentNullException.ThrowIfNull(table);

        return member.Level >= StatCurve.HighestLevel ? null : table[member.Level] - member.Experience;
    }

    private static ContentId Id(string value) => ContentId.Parse(value, StringTable.Path, nameof(StatusView));

    private static int CountLasting()
    {
        int count = 0;
        foreach (StatusKind status in Statuses.All)
        {
            if (Statuses.Lasts(status))
            {
                count += 1;
            }
        }

        return count;
    }

    private static string Number(int value) => value.ToString(CultureInfo.InvariantCulture);

    private static Dictionary<string, string> Values(params (string Key, string Value)[] pairs)
    {
        var values = new Dictionary<string, string>(StringComparer.Ordinal);
        foreach ((string key, string value) in pairs)
        {
            values.Add(key, value);
        }

        return values;
    }

    private void ShowColumn(List<Label> column, PartyMember member, IReadOnlyList<int> table)
    {
        // The attack, the defense, and the speed hold the worn gear, and the gear never changes health or MP (D-1036).
        StatRow full = member.StatsWith(this.state.BattleContent.Gear);
        this.ui.Text.Put(column[0], BattleMessages.NameIdOf(member.Record.Id));
        this.ui.Text.Put(column[1], Id("menu.level"), Values(("level", Number(member.Level))));
        this.ui.Text.Put(column[2], PartyView.RowIdOf(member.Row));
        this.ui.Text.Put(column[3], Id("battle.health"), Values(("health", Number(member.Health)), ("full", Number(full.Health))));
        this.ui.Text.Put(column[4], Id("battle.mp"), Values(("mp", Number(member.Mp)), ("full", Number(full.Mp))));
        this.ui.Text.Put(column[5], Id("menu.experience"), Values(("amount", Number(member.Experience))));
        if (ToNextLevel(member, table) is int next)
        {
            this.ui.Text.Put(column[6], Id("menu.next"), Values(("amount", Number(next))));
        }
        else
        {
            this.ui.Text.Put(column[6], Id("menu.next_none"));
        }

        this.PutStat(column[7], "battle.stat_atk", full.Attack);
        this.PutStat(column[8], "battle.stat_def", full.Defense);
        this.PutStat(column[9], "battle.stat_spd", full.Speed);

        for (int place = 0; place < LastingCount; place += 1)
        {
            Label line = column[FixedLines + place];
            line.Visible = place < member.Statuses.Count || place == 0;
            if (place < member.Statuses.Count)
            {
                this.ui.Text.Put(line, BattleMessages.StatusIdOf(member.Statuses[place]));
            }
            else if (place == 0)
            {
                this.ui.Text.Put(line, Id("menu.sound"));
            }
        }
    }

    private void PutStat(Label line, string statId, int value) =>
        this.ui.Text.Put(line, Id("menu.stat"), Values(("stat", this.strings.Text(Id(statId))), ("value", Number(value))));
}
