using System;
using System.Collections.Generic;
using TheThingBelow.Core.Content;
using TheThingBelow.Core.Logging;
using TheThingBelow.Core.Maps;
using TheThingBelow.Core.Notices;

namespace TheThingBelow.Core.Runs;

/// <summary>The rules of a save point: the confirm that opens the save window, and the save (D-1132, D-1221).</summary>
/// <remarks>
/// A save point stands on a hub and in a dungeon, and the player reads it as a waystone (D-134,
/// D-1222). It is solid, so the lead faces it and confirms (D-1142). The confirm opens the menu and
/// the save window, so the world holds while the window is open (D-162). The save restores nothing
/// (D-1221).
/// <para>
/// Core holds no open save window in its state. The open save point is the save point that the
/// lead faces while the menu is open, because the world holds and no body moves then. The save
/// intent reads it there, and it refuses a request with no faced save point (D-1141, T-2).
/// </para>
/// </remarks>
public static class SavePointRules
{
    /// <summary>The source of the ids of this class, for the error of a malformed id (T-2).</summary>
    private const string Source = "TheThingBelow.Core/Runs/SavePointRules.cs";

    /// <summary>The notice of a save (D-1132).</summary>
    public static readonly ContentId SavedNotice = ContentId.Parse("notice.saved", Source, nameof(SavedNotice));

    /// <summary>Every notice that these rules post, which the notice file of a build must hold (D-989, T-2).</summary>
    public static readonly IReadOnlyList<ContentId> Notices = [SavedNotice];

    /// <summary>Gives the save point that the lead faces (D-1221).</summary>
    /// <param name="state">The run.</param>
    /// <returns>The save point, or no value when the faced tile holds none.</returns>
    /// <exception cref="ArgumentNullException">The state is null (T-2).</exception>
    public static MapThing? FacedSavePoint(RunState state)
    {
        ArgumentNullException.ThrowIfNull(state);

        MapState party = state.Party;
        foreach (MapThing thing in party.Map.ThingsAt(party.LeadAt.Step(party.Facing)))
        {
            if (thing.Kind == MapThingKind.SavePoint)
            {
                return thing;
            }
        }

        return null;
    }

    /// <summary>Asks Game for the slot save at the save point that the lead faces (D-1132, D-1221).</summary>
    /// <param name="state">The run.</param>
    /// <param name="context">The seed, the tick, and the intent, for an error (T-2).</param>
    /// <param name="log">The log entries of this tick (D-179).</param>
    /// <exception cref="ArgumentNullException">An argument is null (T-2).</exception>
    /// <exception cref="SimulationException">No menu is open, a battle holds the run, or the lead faces no save point (T-2).</exception>
    /// <remarks>Core does no file work, so the rule emits a save request, which Game writes after the tick (G-1, D-1132).</remarks>
    public static void Save(RunState state, RunContext context, List<LogEntry> log)
    {
        ArgumentNullException.ThrowIfNull(state);
        ArgumentNullException.ThrowIfNull(context);
        ArgumentNullException.ThrowIfNull(log);

        if (!state.MenuOpen || state.Battle is not null)
        {
            throw new SimulationException("a save request while no menu is open or a battle holds the run, and the save window of a save point makes it (D-1132, D-1221)", context);
        }

        MapThing point = FacedSavePoint(state)
            ?? throw new SimulationException($"a save request, and the lead faces no save point at {state.Party.LeadAt.Step(state.Party.Facing)} (D-1221)", context);
        state.RequestSave(SaveRequestKind.Slot);
        log.Add(new LogEntry(LogLevel.Info, "the party asked for the slot save at a save point", state.Tick, LogSubsystems.Run, [new LogField("save-point", point.Id.Value), new LogField("map", state.Party.Map.Id.Value)]));
        NoticeRules.Post(state, SavedNotice, context, log);
    }

    /// <summary>Opens the menu and the save window from a confirm at a save point (D-1132, D-1221).</summary>
    /// <returns>True, because the open menu holds the world from this tick.</returns>
    internal static bool Open(RunState state, MapThing point, List<LogEntry> log)
    {
        state.SetMenuOpen(true, state.Context($"save-point/{point.Id.Value}"));
        state.AddOpenedSavePoint(point.Id);
        log.Add(new LogEntry(LogLevel.Info, "a save point opened the save window, and the menu holds the world", state.Tick, LogSubsystems.Run, [new LogField("save-point", point.Id.Value), new LogField("map", state.Party.Map.Id.Value)]));
        return true;
    }
}
