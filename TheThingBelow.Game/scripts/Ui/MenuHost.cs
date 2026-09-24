using System;
using System.Collections.Generic;
using Godot;
using TheThingBelow.Core.Content;
using TheThingBelow.Core.Logging;
using TheThingBelow.Core.Runs;
using TheThingBelow.Storage;

namespace TheThingBelow.Game.Ui;

/// <summary>
/// The menu stack of a run on screen: the path of the open windows, one view for each, and the
/// intents that open and close the menu (D-162, D-211, D-986).
/// </summary>
/// <remarks>
/// The menu action opens the main list from the walk, and the map action opens the dungeon map
/// screen. Back closes the window on top, and the menu closes with the last one. The menu
/// action closes every window, and the map action closes the map screen (D-986). The world waits
/// while a window is open, and the host sends the open intent and the close intent of the run
/// (D-162, D-650).
/// <para>
/// The host holds no rule. A choice of the party window becomes the row intent of the run, and
/// a move of a cursor makes no intent (D-493). The settings screen writes and applies its
/// settings through the host of the session when it closes (D-860, D-871).
/// </para>
/// </remarks>
public sealed class MenuHost
{
    private readonly MenuPath path = new();
    private readonly List<IMenuView> views = [];
    private readonly GameRun run;
    private readonly ContentSet content;
    private readonly Func<GameSettings> settings;
    private readonly Action<GameSettings> settingsClosed;
    private readonly Action<IReadOnlyList<LogEntry>> writeLog;
    private FrameRoot frame;
    private UiBase ui;
    private MainList mainList = new();
    private PartyList? partyList;
    private LessonCursor? lessonCursor;
    private GearCursor? gearCursor;
    private ItemCursor? itemCursor;

    /// <summary>Makes the host of the menu of one run, with no window open.</summary>
    /// <param name="frame">The frame, whose UI layer takes each window.</param>
    /// <param name="ui">The atlas, the theme, and the text helper.</param>
    /// <param name="run">The run, which takes the intents of the menu.</param>
    /// <param name="content">The content set, for the string table and the body sizes.</param>
    /// <param name="settings">Gives the settings in use, which the settings screen opens with.</param>
    /// <param name="settingsClosed">Takes the settings of the settings screen when it closes, and writes and applies them (D-860).</param>
    /// <param name="writeLog">Writes the log entries of the menu (D-179).</param>
    /// <exception cref="ArgumentNullException">An argument is null (T-2).</exception>
    public MenuHost(
        FrameRoot frame,
        UiBase ui,
        GameRun run,
        ContentSet content,
        Func<GameSettings> settings,
        Action<GameSettings> settingsClosed,
        Action<IReadOnlyList<LogEntry>> writeLog)
    {
        ArgumentNullException.ThrowIfNull(frame);
        ArgumentNullException.ThrowIfNull(ui);
        ArgumentNullException.ThrowIfNull(run);
        ArgumentNullException.ThrowIfNull(content);
        ArgumentNullException.ThrowIfNull(settings);
        ArgumentNullException.ThrowIfNull(settingsClosed);
        ArgumentNullException.ThrowIfNull(writeLog);

        this.frame = frame;
        this.ui = ui;
        this.run = run;
        this.content = content;
        this.settings = settings;
        this.settingsClosed = settingsClosed;
        this.writeLog = writeLog;
    }

    /// <summary>True while a window is open, so the host takes every input (D-162).</summary>
    public bool IsOpen => this.path.IsOpen;

    /// <summary>The open windows, from the bottom to the top.</summary>
    public MenuPath Path => this.path;

    /// <summary>The view on top, which takes the input.</summary>
    /// <exception cref="InvalidOperationException">No window is open (T-2).</exception>
    public IMenuView Top => this.views.Count > 0
        ? this.views[^1]
        : throw new InvalidOperationException("The menu holds no window, so no view is on top (T-2).");

    /// <summary>Opens the menu with the main list, from the walk (D-211).</summary>
    /// <exception cref="InvalidOperationException">A window is already open (T-2).</exception>
    public void OpenMainList()
    {
        this.mainList = new MainList();
        this.OpenMenu(MenuWindowKind.MainList);
    }

    /// <summary>Opens the menu with the dungeon map screen, from the walk (D-986).</summary>
    /// <exception cref="InvalidOperationException">A window is already open (T-2).</exception>
    /// <exception cref="ArgumentException">The map is larger than 80 by 45 tiles (D-982, T-2).</exception>
    public void OpenDungeonMap() => this.OpenMenu(MenuWindowKind.DungeonMap);

    /// <summary>Gives one input event to the menu.</summary>
    /// <param name="signal">The event.</param>
    /// <exception cref="ArgumentNullException">The event is null (T-2).</exception>
    /// <exception cref="InvalidOperationException">No window is open (T-2).</exception>
    public void Read(InputEvent signal)
    {
        ArgumentNullException.ThrowIfNull(signal);

        MenuWindowKind top = this.path.Top;

        // The settings screen reads the menu action itself, because a conflict blocks the close
        // (D-862). Every other window closes the whole menu on it (D-162).
        if (top != MenuWindowKind.Settings && signal.IsActionPressed(InputActions.Menu))
        {
            this.CloseAll();
            return;
        }

        if (top == MenuWindowKind.DungeonMap && signal.IsActionPressed(InputActions.Map))
        {
            this.CloseAll();
            return;
        }

        ViewOutcome outcome = this.Top.Read(signal, this.frame.Fit);
        if (outcome == ViewOutcome.Back && top == MenuWindowKind.Settings && signal.IsActionPressed(InputActions.Menu))
        {
            this.Back();
            this.CloseAll();
        }
        else if (outcome == ViewOutcome.Back)
        {
            this.Back();
        }
        else if (outcome == ViewOutcome.Chose)
        {
            this.Choose(top);
        }
    }

    /// <summary>Draws each open window from the state of the run now, such as a row that a tick moved (D-558).</summary>
    public void Show()
    {
        foreach (IMenuView view in this.views)
        {
            view.Show();
        }
    }

    /// <summary>
    /// Builds each open window again on a new frame, as after a change of the fit or the body
    /// size (D-707). Each cursor stays.
    /// </summary>
    /// <param name="built">The new frame.</param>
    /// <param name="shown">The new UI base.</param>
    /// <exception cref="ArgumentNullException">An argument is null (T-2).</exception>
    public void Rebuild(FrameRoot built, UiBase shown)
    {
        ArgumentNullException.ThrowIfNull(built);
        ArgumentNullException.ThrowIfNull(shown);

        // The old frame freed the nodes of each view with it. A settings screen keeps the
        // settings that the player changed, and its cursor starts again on the first row.
        GameSettings? changed = this.views.Count > 0 && this.views[^1] is SettingsView open ? open.Screen.Menu.Settings : null;
        this.frame = built;
        this.ui = shown;
        this.views.Clear();
        foreach (MenuWindowKind kind in this.path.Windows)
        {
            this.views.Add(this.Build(kind, changed));
        }
    }

    private static LogField WindowField(MenuWindowKind kind) => new("window", kind.ToString());

    private void OpenMenu(MenuWindowKind kind)
    {
        this.path.Open(kind);
        this.views.Add(this.Build(kind, null));
        this.run.Queue(Intent.OfPlayer(IntentIds.OpenMenu));
        this.writeLog([new LogEntry(LogLevel.Info, "the menu opened", this.run.Tick, LogSubsystems.Game, [WindowField(kind)])]);
    }

    private void Choose(MenuWindowKind top)
    {
        if (top == MenuWindowKind.MainList)
        {
            MenuWindowKind kind = MainList.WindowOf(this.mainList.Current);
            if (kind == MenuWindowKind.Party)
            {
                this.partyList = new PartyList(this.run.State.Characters.Members.Count);
            }

            if (kind == MenuWindowKind.Lessons)
            {
                this.lessonCursor = new LessonCursor(this.run.State);
            }

            if (kind == MenuWindowKind.Gear)
            {
                this.gearCursor = new GearCursor(this.run.State);
            }

            if (kind == MenuWindowKind.Items)
            {
                this.itemCursor = new ItemCursor(this.run.State);
            }

            this.path.Open(kind);
            this.views.Add(this.Build(kind, null));
            this.writeLog([new LogEntry(LogLevel.Info, "the menu opened a window", this.run.Tick, LogSubsystems.Game, [WindowField(kind)])]);
            return;
        }

        if (top == MenuWindowKind.Party)
        {
            PartyList list = this.partyList ?? throw new InvalidOperationException(
                $"The party window chose at tick {this.run.Tick}, and the host made no cursor for it (T-2).");
            this.run.Queue(list.Choose());
        }

        // The lesson window made the intent of a whole choice: a swap or a cast (D-391, D-1030).
        if (top == MenuWindowKind.Lessons && this.views[^1] is LessonsView lessons && lessons.TakeIntent() is Intent made)
        {
            this.run.Queue(made);
        }

        // The gear window and the item window made the intent of a whole choice (D-1048, D-1049).
        if (top == MenuWindowKind.Gear && this.views[^1] is GearView gear && gear.TakeIntent() is Intent worn)
        {
            this.run.Queue(worn);
        }

        if (top == MenuWindowKind.Items && this.views[^1] is ItemsView items && items.TakeIntent() is Intent used)
        {
            this.run.Queue(used);
        }
    }

    /// <summary>Closes the window on top, and the menu with the last one (D-162, D-211).</summary>
    private void Back()
    {
        MenuWindowKind top = this.path.Top;
        IMenuView view = this.views[^1];
        this.views.RemoveAt(this.views.Count - 1);
        view.Free();
        bool empty = this.path.Back();
        this.writeLog([new LogEntry(LogLevel.Info, "the menu closed a window", this.run.Tick, LogSubsystems.Game, [WindowField(top)])]);

        // The settings apply after the screen left the stack, because a change of the fit
        // builds the frame and each open window again (D-707, D-860).
        if (view is SettingsView settingsView)
        {
            this.settingsClosed(settingsView.Screen.Menu.Settings);
        }

        if (empty)
        {
            this.CloseRun();
        }
    }

    /// <summary>Closes every window, as the menu action and the map action do (D-162, D-986).</summary>
    private void CloseAll()
    {
        if (!this.path.IsOpen)
        {
            return;
        }

        foreach (IMenuView view in this.views)
        {
            view.Free();
        }

        this.views.Clear();
        this.path.CloseAll();
        this.CloseRun();
    }

    /// <summary>
    /// Sends the close intent by its own id. The open intent can still wait in the queue when one
    /// frame opens and closes the menu, and the menu action would then open it twice (T-2).
    /// </summary>
    private void CloseRun()
    {
        this.run.Queue(Intent.OfPlayer(IntentIds.CloseMenu));
        this.writeLog([new LogEntry(LogLevel.Info, "the menu closed", this.run.Tick, LogSubsystems.Game, [])]);
    }

    private IMenuView Build(MenuWindowKind kind, GameSettings? changed) => kind switch
    {
        MenuWindowKind.MainList => new MainListView(this.frame, this.ui, this.mainList),
        MenuWindowKind.Party => new PartyView(this.frame, this.ui, this.run.State, this.partyList ?? throw new InvalidOperationException(
            $"The party window builds at tick {this.run.Tick}, and the host made no cursor for it (T-2).")),
        MenuWindowKind.Lessons => new LessonsView(this.frame, this.ui, this.content.Strings, this.run.State, this.lessonCursor ?? throw new InvalidOperationException(
            $"The lesson window builds at tick {this.run.Tick}, and the host made no cursor for it (T-2).")),
        MenuWindowKind.Gear => new GearView(this.frame, this.ui, this.run.State, this.gearCursor ?? throw new InvalidOperationException(
            $"The gear window builds at tick {this.run.Tick}, and the host made no cursor for it (T-2).")),
        MenuWindowKind.Items => new ItemsView(this.frame, this.ui, this.run.State, this.itemCursor ?? throw new InvalidOperationException(
            $"The item window builds at tick {this.run.Tick}, and the host made no cursor for it (T-2).")),
        MenuWindowKind.Status => new StatusView(this.frame, this.ui, this.content.Strings, this.run.State),
        MenuWindowKind.Log => new LogView(this.frame, this.ui, this.run.State),
        MenuWindowKind.Settings => new SettingsView(
            SettingsScreen.Build(
                this.frame,
                this.ui,
                this.content.Strings,
                changed ?? this.settings(),
                BodySize.DefaultFor(this.frame.Fit.Height, this.content.Style.SmallBody, this.content.Style.LargeBody)),
            InputActions.Menu),
        MenuWindowKind.DungeonMap => new DungeonMapView(this.frame, this.ui, this.run.Party),
        _ => throw new ArgumentOutOfRangeException(nameof(kind), kind, "The menu builds no such window (T-2)."),
    };
}
