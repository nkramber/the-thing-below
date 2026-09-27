using System;
using System.Collections.Generic;
using TheThingBelow.Core.Battles;
using TheThingBelow.Core.Content;
using TheThingBelow.Core.Logging;
using TheThingBelow.Core.Maps;
using TheThingBelow.Core.Notices;

namespace TheThingBelow.Core.Runs;

/// <summary>The harm of the map: poison and bad air, once each second of the world (D-1234, D-1235).</summary>
/// <remarks>
/// Poison takes its share from each poisoned character, the reserve included (D-1236). Bad air takes
/// its share from each character who fights and stands, while the lead stands on ground under bad
/// air. Both come on each world tick whose count divides by <see cref="MapRules.HarmTicks"/>, so a
/// party that stands still takes the harm too, and an open menu stops it (D-650).
/// <para>
/// A harm can down a character, and a down ends each status of that character (D-392, D-801). When
/// each character who fights is down, the run holds a wipe on the map, even with a healthy reserve
/// (D-397). A down in the reserve never counts toward a wipe.
/// </para>
/// </remarks>
public static class MapHarmRules
{
    /// <summary>The source of the ids of this class, for the error of a malformed id (T-2).</summary>
    private const string Source = "TheThingBelow.Core/Runs/MapHarmRules.cs";

    /// <summary>The notice of a character that the map downed, in the party or in the reserve, while a character who fights still stands (D-392).</summary>
    public static readonly ContentId FellNotice = ContentId.Parse("notice.fell_on_map", Source, nameof(FellNotice));

    /// <summary>Every notice that these rules post, which the notice file of a build must hold (D-989, T-2).</summary>
    public static readonly IReadOnlyList<ContentId> Notices = [FellNotice];

    /// <summary>Applies the harm of this world tick, when the count of world ticks divides by <see cref="MapRules.HarmTicks"/> (D-1234, D-1235).</summary>
    /// <param name="state">The run, with no battle and no story scene.</param>
    /// <param name="log">The log entries of this tick (D-179).</param>
    /// <exception cref="ArgumentNullException">An argument is null (T-2).</exception>
    /// <exception cref="SimulationException">A share passes the range of an `int` (T-2).</exception>
    public static void Tick(RunState state, List<LogEntry> log)
    {
        ArgumentNullException.ThrowIfNull(state);
        ArgumentNullException.ThrowIfNull(log);

        if (state.WorldTick % MapRules.HarmTicks != 0)
        {
            return;
        }

        RunContext context = state.Context("map/harm");
        BattleRules rules = state.BattleContent.Rules;
        int fell = 0;

        foreach (PartyMember member in state.Characters.Members)
        {
            fell += HurtIfPoisoned(state, member, rules.MapPoisonShare, context, log);
        }

        foreach (PartyMember member in state.Characters.Reserve)
        {
            fell += HurtIfPoisoned(state, member, rules.MapPoisonShare, context, log);
        }

        MapState party = state.Party;
        if (party.Map.TileAt(party.LeadAt) == TileKind.BadAir)
        {
            foreach (PartyMember member in state.Characters.Members)
            {
                if (!member.Down)
                {
                    fell += Harm(state, member, rules.BadAirShare, "bad air", context, log);
                }
            }
        }

        if (fell > 0 && !state.MapWiped)
        {
            NoticeRules.Post(state, FellNotice, context, log);
        }

        if (state.MapWiped)
        {
            log.Add(new LogEntry(LogLevel.Info, "each character who fights went down on the map, and the party wiped (D-397)", state.Tick, LogSubsystems.World, [LogField.OfNumber("world-tick", state.WorldTick)]));
        }
    }

    /// <summary>
    /// Takes a share of full health from one character, at least 1 and at most the health that
    /// remains (D-808). A down ends each status of the character (D-801).
    /// </summary>
    /// <param name="member">The character, who stands.</param>
    /// <param name="share">The share, in basis points.</param>
    /// <param name="context">The seed, the tick, and the ids, for an error (T-2).</param>
    /// <returns>The health that the character lost.</returns>
    /// <exception cref="ArgumentNullException">An argument is null (T-2).</exception>
    /// <exception cref="SimulationException">The character is down, or the share passes the range of an `int` (T-2).</exception>
    internal static int Hurt(PartyMember member, int share, RunContext context)
    {
        ArgumentNullException.ThrowIfNull(member);
        ArgumentNullException.ThrowIfNull(context);

        if (member.Down)
        {
            throw new SimulationException($"a harm of the map on '{member.Record.Id.Value}', who is down, and a down character takes no harm (D-36)", context);
        }

        int loss = Math.Min(Math.Max(1, BasisPoints.Apply(member.Stats.Health, share, context)), member.Health);
        member.Health = checked(member.Health - loss);
        if (member.Down)
        {
            member.Statuses = [];
        }

        return loss;
    }

    private static int HurtIfPoisoned(RunState state, PartyMember member, int share, RunContext context, List<LogEntry> log)
    {
        return !member.Down && LessonRules.Holds(member.Statuses, StatusKind.Poison)
            ? Harm(state, member, share, "poison", context, log)
            : 0;
    }

    /// <summary>Hurts one character, logs the harm, and gives 1 when the harm downed the character.</summary>
    private static int Harm(RunState state, PartyMember member, int share, string cause, RunContext context, List<LogEntry> log)
    {
        int loss = Hurt(member, share, context);
        IReadOnlyList<LogField> fields =
        [
            new LogField("character", member.Record.Id.Value),
            new LogField("cause", cause),
            LogField.OfNumber("loss", loss),
            LogField.OfNumber("health", member.Health),
        ];
        if (member.Down)
        {
            log.Add(new LogEntry(LogLevel.Info, "a harm of the map downed a character (D-392)", state.Tick, LogSubsystems.World, fields));
            return 1;
        }

        log.Add(new LogEntry(LogLevel.Debug, "a harm of the map hurt a character", state.Tick, LogSubsystems.World, fields));
        return 0;
    }
}
