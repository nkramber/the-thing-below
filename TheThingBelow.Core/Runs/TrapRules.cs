using System;
using System.Collections.Generic;
using TheThingBelow.Core.Battles;
using TheThingBelow.Core.Content;
using TheThingBelow.Core.Logging;
using TheThingBelow.Core.Maps;
using TheThingBelow.Core.Notices;

namespace TheThingBelow.Core.Runs;

/// <summary>The rules of a trap: when it shows, what it does when it fires, and its disarm (D-386, D-1226 to D-1231).</summary>
/// <remarks>
/// A trap stays hidden. It shows while a character who fights and stands carries a Theft drill,
/// and the lead stands at <see cref="MapRules.TrapShowRange"/> steps or less from it (D-1228). The
/// lead faces a trap that shows and confirms to disarm it. A step onto a trap fires it, whether or
/// not it shows. A trap fires one time, and the memory of the map keeps it spent (D-1229).
/// <para>
/// A damage trap takes its share of full health from each character who fights and stands, and a
/// status trap puts its status on each of them (D-1230). An encounter trap starts a fight at once,
/// in which the enemies act first (D-1231).
/// </para>
/// </remarks>
public static class TrapRules
{
    /// <summary>The source of the ids of this class, for the error of a malformed id (T-2).</summary>
    private const string Source = "TheThingBelow.Core/Runs/TrapRules.cs";

    /// <summary>The notice of a damage trap that fired (D-1230).</summary>
    public static readonly ContentId DamageNotice = ContentId.Parse("notice.trap_damage", Source, nameof(DamageNotice));

    /// <summary>The notice of a trap of poison that fired (D-1230).</summary>
    public static readonly ContentId PoisonNotice = ContentId.Parse("notice.trap_poison", Source, nameof(PoisonNotice));

    /// <summary>The notice of a trap of blind that fired (D-1230).</summary>
    public static readonly ContentId BlindNotice = ContentId.Parse("notice.trap_blind", Source, nameof(BlindNotice));

    /// <summary>The notice of a trap of silence that fired (D-1230).</summary>
    public static readonly ContentId SilenceNotice = ContentId.Parse("notice.trap_silence", Source, nameof(SilenceNotice));

    /// <summary>The notice of a trap that the lead disarmed (D-1228).</summary>
    public static readonly ContentId DisarmNotice = ContentId.Parse("notice.trap_disarmed", Source, nameof(DisarmNotice));

    /// <summary>Every notice that these rules post, which the notice file of a build must hold (D-989, T-2).</summary>
    public static readonly IReadOnlyList<ContentId> Notices = [DamageNotice, PoisonNotice, BlindNotice, SilenceNotice, DisarmNotice];

    /// <summary>Tells whether one trap shows to the party now (D-1228).</summary>
    /// <param name="state">The run.</param>
    /// <param name="trap">A trap of the map that the party stands on.</param>
    /// <returns>
    /// True when the trap is not spent, a character who fights and stands carries a Theft drill,
    /// and the lead stands at <see cref="MapRules.TrapShowRange"/> steps or less from the trap.
    /// </returns>
    /// <exception cref="ArgumentNullException">An argument is null (T-2).</exception>
    /// <exception cref="ArgumentException">The thing is not a trap (T-2).</exception>
    public static bool Shows(RunState state, MapThing trap)
    {
        ArgumentNullException.ThrowIfNull(state);

        return Shows(state.Party, DoorRules.CarriesTheft(state), trap);
    }

    /// <summary>Tells whether one trap shows, from the party on its map and the Theft drill of the party (D-1228).</summary>
    /// <param name="party">The party on the map of the trap.</param>
    /// <param name="theftCarried">True when a character who fights and stands carries a Theft drill, which <see cref="DoorRules.CarriesTheft"/> gives.</param>
    /// <param name="trap">A trap of that map.</param>
    /// <returns>True when the trap is not spent, the Theft drill is carried, and the lead stands at <see cref="MapRules.TrapShowRange"/> steps or less from the trap.</returns>
    /// <exception cref="ArgumentNullException">An argument is null (T-2).</exception>
    /// <exception cref="ArgumentException">The thing is not a trap (T-2).</exception>
    /// <remarks>Game draws each trap from this rule, so the screen and the confirm agree (D-1238).</remarks>
    public static bool Shows(MapState party, bool theftCarried, MapThing trap)
    {
        ArgumentNullException.ThrowIfNull(party);
        RequireTrap(trap);

        return theftCarried
            && !party.Place.IsSpent(trap.Id)
            && MapRules.StepsApart(party.LeadAt, trap.At) <= MapRules.TrapShowRange;
    }

    /// <summary>Fires the trap on the tile that the lead reached, when the trap is not spent (D-1226, D-1229).</summary>
    /// <param name="state">The run, with the lead on the tile of the trap.</param>
    /// <param name="trap">The trap.</param>
    /// <param name="log">The log entries of this tick (D-179).</param>
    /// <returns>True when the trap started a fight, which holds the world from this tick (D-1231).</returns>
    /// <exception cref="ArgumentNullException">An argument is null (T-2).</exception>
    /// <exception cref="ArgumentException">The thing is not a trap (T-2).</exception>
    /// <exception cref="SimulationException">The lead does not stand on the trap, or a harm breaks a rule (T-2).</exception>
    /// <remarks>A spent trap does nothing, and a log line says so.</remarks>
    public static bool Fire(RunState state, MapThing trap, List<LogEntry> log)
    {
        ArgumentNullException.ThrowIfNull(state);
        RequireTrap(trap);
        ArgumentNullException.ThrowIfNull(log);

        RunContext context = state.Context($"trap/{trap.Id.Value}");
        if (state.Party.LeadAt != trap.At)
        {
            throw new SimulationException($"a fire of the trap at {trap.At}, and the lead stands at {state.Party.LeadAt} (D-1226)", context);
        }

        PlaceState place = state.Party.Place;
        if (place.IsSpent(trap.Id))
        {
            log.Add(Entry(state, LogLevel.Debug, "the lead stepped onto a spent trap", trap));
            return false;
        }

        place.Spend(trap.Id);
        TrapHarm harm = trap.Harm
            ?? throw new InvalidOperationException($"The trap '{trap.Id.Value}' holds no harm, and the load of a map refuses such a trap (D-1226, T-2).");
        switch (harm.Kind)
        {
            case TrapHarmKind.Damage:
                bool downed = HurtEach(state, harm.Share ?? throw MissingValue(trap, TrapHarm.ShareField), context);
                log.Add(Entry(state, LogLevel.Info, "a damage trap fired", trap));
                NoticeRules.Post(state, DamageNotice, context, log);

                // A down that leaves a fighter who stands takes the notice of a down, as a harm of
                // poison or bad air does. The drain of a wipe tells the down of the whole party
                // (D-392, D-397, D-1241).
                if (downed && !state.MapWiped)
                {
                    NoticeRules.Post(state, MapHarmRules.FellNotice, context, log);
                }

                return false;
            case TrapHarmKind.Status:
                StatusKind status = harm.Status ?? throw MissingValue(trap, TrapHarm.StatusField);
                PutOnEach(state, status);
                log.Add(Entry(state, LogLevel.Info, $"a trap of {Statuses.NameOf(status)} fired", trap));
                NoticeRules.Post(state, NoticeOf(status), context, log);
                return false;
            case TrapHarmKind.Encounter:
                // The lead stands on the trap while the fight runs, so a step that started on this
                // tick ends here (D-1231).
                state.Party.EndStep();
                log.Add(Entry(state, LogLevel.Info, "an encounter trap fired", trap));
                BattleTurns.BeginTrap(state, trap.Id, harm.Group ?? throw MissingValue(trap, TrapHarm.GroupField), log);
                return true;
            default:
                throw new SimulationException($"the trap '{trap.Id.Value}' takes the harm {harm.Kind}, which the trap rule does not read (D-1226)", context);
        }
    }

    /// <summary>Disarms a trap that shows, which the lead faces (D-1228).</summary>
    /// <param name="state">The run, with the lead standing and facing the trap.</param>
    /// <param name="trap">The trap.</param>
    /// <param name="log">The log entries of this tick (D-179).</param>
    /// <exception cref="ArgumentNullException">An argument is null (T-2).</exception>
    /// <exception cref="ArgumentException">The thing is not a trap (T-2).</exception>
    /// <exception cref="SimulationException">The trap does not show (T-2).</exception>
    public static void Disarm(RunState state, MapThing trap, List<LogEntry> log)
    {
        ArgumentNullException.ThrowIfNull(state);
        RequireTrap(trap);
        ArgumentNullException.ThrowIfNull(log);

        RunContext context = state.Context($"trap/{trap.Id.Value}");
        if (!Shows(state, trap))
        {
            throw new SimulationException($"a disarm of the trap '{trap.Id.Value}', which does not show, and the lead disarms a trap that shows alone (D-1228)", context);
        }

        state.Party.Place.Spend(trap.Id);
        log.Add(Entry(state, LogLevel.Info, "a Theft drill disarmed a trap", trap));
        NoticeRules.Post(state, DisarmNotice, context, log);
    }

    /// <summary>Hurts each character who fights and stands, and tells whether the harm downed one (D-1230).</summary>
    private static bool HurtEach(RunState state, int share, RunContext context)
    {
        bool downed = false;
        foreach (PartyMember member in state.Characters.Members)
        {
            if (!member.Down)
            {
                _ = MapHarmRules.Hurt(member, share, context);
                downed |= member.Down;
            }
        }

        return downed;
    }

    /// <summary>Puts one status on each character who fights and stands, and keeps the order of D-75.</summary>
    private static void PutOnEach(RunState state, StatusKind status)
    {
        foreach (PartyMember member in state.Characters.Members)
        {
            if (member.Down || LessonRules.Holds(member.Statuses, status))
            {
                continue;
            }

            List<StatusKind> held = [];
            foreach (StatusKind kind in Statuses.All)
            {
                if (kind == status || LessonRules.Holds(member.Statuses, kind))
                {
                    held.Add(kind);
                }
            }

            member.Statuses = held;
        }
    }

    private static ContentId NoticeOf(StatusKind status) => status switch
    {
        StatusKind.Poison => PoisonNotice,
        StatusKind.Blind => BlindNotice,
        StatusKind.Silence => SilenceNotice,
        _ => throw new ArgumentOutOfRangeException(nameof(status), status, "a status trap puts poison, blind, or silence (D-1230)"),
    };

    private static void RequireTrap(MapThing trap)
    {
        ArgumentNullException.ThrowIfNull(trap);

        if (trap.Kind != MapThingKind.Trap)
        {
            throw new ArgumentException($"The thing '{trap.Id.Value}' is a {MapThingKinds.NameOf(trap.Kind)}, and the trap rule reads a trap alone (D-1226, T-2).", nameof(trap));
        }
    }

    private static InvalidOperationException MissingValue(MapThing trap, string field) =>
        new($"The trap '{trap.Id.Value}' holds no value for the field '{field}' of its harm, and the load of a map refuses such a trap (D-1226, T-2).");

    private static LogEntry Entry(RunState state, LogLevel level, string message, MapThing trap) =>
        new(level, message, state.Tick, LogSubsystems.World, [new LogField("trap", trap.Id.Value), new LogField("map", state.Party.Map.Id.Value)]);
}
