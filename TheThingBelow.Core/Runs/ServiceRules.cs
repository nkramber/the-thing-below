using System;
using System.Collections.Generic;
using TheThingBelow.Core.Content;
using TheThingBelow.Core.Logging;
using TheThingBelow.Core.Maps;
using TheThingBelow.Core.Notices;

namespace TheThingBelow.Core.Runs;

/// <summary>The rules of the services of a hub: the open of a service, the rest, and the save (D-1131, D-1132).</summary>
/// <remarks>
/// A confirm while the lead faces the host of a service opens it when its condition holds, and a
/// closed service posts a notice and stays closed (D-543, D-1131). An open service opens the
/// menu, so the world holds and each NPC stands still while its window is open (D-162, D-1139).
/// <para>
/// Core holds no open service in its state. The open service is the service of the host that the
/// lead faces while the menu is open, because the world holds and no body moves then. The rest
/// and the save intents read it there, and each one refuses a request with no open service of its
/// kind (D-1141, T-2).
/// </para>
/// </remarks>
public static class ServiceRules
{
    /// <summary>The source of the ids of this class, for the error of a malformed id (T-2).</summary>
    private const string Source = "TheThingBelow.Core/Runs/ServiceRules.cs";

    /// <summary>The notice of a service whose condition fails (D-543, D-1131).</summary>
    public static readonly ContentId ClosedNotice = ContentId.Parse("notice.service_closed", Source, nameof(ClosedNotice));

    /// <summary>The notice of a rest at a hub (D-390).</summary>
    public static readonly ContentId RestedNotice = ContentId.Parse("notice.rested", Source, nameof(RestedNotice));

    /// <summary>The notice of a save at a hub (D-1132).</summary>
    public static readonly ContentId SavedNotice = ContentId.Parse("notice.saved", Source, nameof(SavedNotice));

    /// <summary>Every notice that these rules post, which the notice file of a build must hold (D-989, T-2).</summary>
    public static readonly IReadOnlyList<ContentId> Notices = [ClosedNotice, RestedNotice, SavedNotice];

    /// <summary>Gives the service of the NPC or the service point that the lead faces (D-1131, D-1142).</summary>
    /// <param name="state">The run.</param>
    /// <returns>The service, or no value when the faced tile holds no host of a service.</returns>
    /// <exception cref="ArgumentNullException">The state is null (T-2).</exception>
    /// <remarks>The NPC that stands on the faced tile wins over a thing, as the confirm reads them (D-1139).</remarks>
    public static MapService? FacedService(RunState state)
    {
        ArgumentNullException.ThrowIfNull(state);

        MapState party = state.Party;
        TilePoint faced = party.LeadAt.Step(party.Facing);
        if (party.Npcs.TryNpcStandingOn(faced, out NpcState? npc))
        {
            return party.Map.ServiceOn(npc!.Npc.Id);
        }

        foreach (MapThing thing in party.Map.ThingsAt(faced))
        {
            if (thing.Kind == MapThingKind.ServicePoint)
            {
                return party.Map.ServiceOn(thing.Id);
            }
        }

        return null;
    }

    /// <summary>Rests the party and the reserve at the open rest service (D-390, D-1135).</summary>
    /// <param name="state">The run.</param>
    /// <param name="context">The seed, the tick, and the intent, for an error (T-2).</param>
    /// <param name="log">The log entries of this tick (D-179).</param>
    /// <exception cref="ArgumentNullException">An argument is null (T-2).</exception>
    /// <exception cref="SimulationException">No rest service is open (D-1141, T-2).</exception>
    /// <remarks>
    /// Each character of the party and of the reserve gets full health and full MP, a downed
    /// character stands again, and poison, blind, and silence end (D-42, D-390, D-970).
    /// </remarks>
    public static void Rest(RunState state, RunContext context, List<LogEntry> log)
    {
        ArgumentNullException.ThrowIfNull(state);
        ArgumentNullException.ThrowIfNull(context);
        ArgumentNullException.ThrowIfNull(log);

        MapService service = RequireOpen(state, ServiceKind.Rest, context);
        state.Characters.RestAtHub();
        log.Add(Entry(state, "the party rested at a hub", service));
        NoticeRules.Post(state, RestedNotice, context, log);
    }

    /// <summary>Asks Game for the slot save at the open save service (D-1132).</summary>
    /// <param name="state">The run.</param>
    /// <param name="context">The seed, the tick, and the intent, for an error (T-2).</param>
    /// <param name="log">The log entries of this tick (D-179).</param>
    /// <exception cref="ArgumentNullException">An argument is null (T-2).</exception>
    /// <exception cref="SimulationException">No save service is open (T-2).</exception>
    /// <remarks>Core does no file work, so the rule emits a save request, which Game writes after the tick (G-1, D-1132).</remarks>
    public static void Save(RunState state, RunContext context, List<LogEntry> log)
    {
        ArgumentNullException.ThrowIfNull(state);
        ArgumentNullException.ThrowIfNull(context);
        ArgumentNullException.ThrowIfNull(log);

        MapService service = RequireOpen(state, ServiceKind.Save, context);
        state.RequestSave(SaveRequestKind.Slot);
        log.Add(Entry(state, "the party asked for the slot save at a hub", service));
        NoticeRules.Post(state, SavedNotice, context, log);
    }

    /// <summary>
    /// Opens one service from a confirm, when its condition holds (D-543, D-1131). A closed service
    /// posts its notice and stays closed. An open service opens the menu, and Game takes it with
    /// <see cref="RunState.TakeOpenedServices"/>.
    /// </summary>
    /// <returns>True when the service opened, which holds the world from this tick.</returns>
    internal static bool Open(RunState state, MapService service, List<LogEntry> log)
    {
        RunContext context = state.Context($"service/{service.Id.Value}");
        if (!service.Condition.Holds(state.Story.Flags))
        {
            log.Add(Entry(state, "the condition of a service failed, and the service stayed closed", service));
            NoticeRules.Post(state, ClosedNotice, context, log);
            return false;
        }

        state.SetMenuOpen(true, context);
        state.AddOpenedService(service);
        log.Add(Entry(state, "a service opened, and the menu holds the world", service));
        return true;
    }

    /// <summary>
    /// Gives the open service of one kind: the menu is open, no battle holds the run, and the lead
    /// faces the host of a service of that kind whose condition holds (D-1131, D-1141).
    /// </summary>
    private static MapService RequireOpen(RunState state, ServiceKind kind, RunContext context)
    {
        string name = ServiceKinds.NameOf(kind);
        if (!state.MenuOpen || state.Battle is not null)
        {
            throw new SimulationException($"a {name} request while no menu is open or a battle holds the run, and the {name} window of a hub service makes it (D-1131, D-1141)", context);
        }

        MapService service = FacedService(state)
            ?? throw new SimulationException($"a {name} request, and the lead faces no host of a service at {state.Party.LeadAt.Step(state.Party.Facing)} (D-1131, D-1141)", context);
        if (service.Kind != kind)
        {
            throw new SimulationException($"a {name} request, and the faced service '{service.Id.Value}' is a {ServiceKinds.NameOf(service.Kind)} service (D-1131, D-1141)", context);
        }

        if (!service.Condition.Holds(state.Story.Flags))
        {
            throw new SimulationException($"a {name} request, and the condition of the service '{service.Id.Value}' keeps it closed (D-543, D-1141)", context);
        }

        return service;
    }

    private static LogEntry Entry(RunState state, string message, MapService service) =>
        new(
            LogLevel.Info,
            message,
            state.Tick,
            LogSubsystems.Run,
            [new LogField("service", service.Id.Value), new LogField("kind", ServiceKinds.NameOf(service.Kind))]);
}
