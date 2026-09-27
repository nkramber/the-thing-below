using System;
using System.Collections.Generic;
using TheThingBelow.Core.Battles;
using TheThingBelow.Core.Content;
using TheThingBelow.Core.Maps;
using TheThingBelow.Core.Runs;
using TheThingBelow.Core.Saves;
using TheThingBelow.Storage;

namespace TheThingBelow.Tools.Bots;

/// <summary>
/// Plays one bot run with no Godot: a seed, a policy, and the content of this build (D-64, D-100).
/// The host of the run acts as Game acts, and it sends each wait intent at once (D-522, D-540).
/// </summary>
/// <remarks>
/// Each tick, the runner asks Core for the intents that the state accepts, checks for a softlock,
/// and steps the one intent that the policy picks (D-493, D-1179). It writes each save that Core
/// asks for through Storage, and after a wipe it reloads the newer save of the run, or it starts
/// the run again from its seed, as Game does (D-1114, D-1181). The run ends as complete when the
/// goal flag comes on, and as budget when it played the tick budget (D-1181, D-1184).
/// <para>
/// A crash is any error of a tick. The runner keeps it in the result with the record, and the
/// bot job fails on it, so no error ends in silence (T-2).
/// </para>
/// </remarks>
public sealed class BotRun
{
    private static readonly IReadOnlyList<Intent> NoIntents = [];

    private readonly ContentSet content;

    private readonly ulong seed;

    private readonly IBotPolicy policy;

    private readonly AcceptedSource accepted;

    private readonly SaveStore saves;

    private readonly BattleTally tally = new();

    private Simulation simulation;

    private RunRecorder recorder;

    private long played;

    private long? goalPlayed;

    private int wipes;

    private BotRun(ContentSet content, ulong seed, IBotPolicy policy, AcceptedSource accepted, SaveStore saves)
    {
        this.content = content;
        this.seed = seed;
        this.policy = policy;
        this.accepted = accepted;
        this.saves = saves;
        this.simulation = this.StartAgain();
        this.recorder = this.NewRecorder();
    }

    /// <summary>Plays one run to its end.</summary>
    /// <param name="content">The content of this build.</param>
    /// <param name="seed">The seed of the run.</param>
    /// <param name="policy">The policy of the run.</param>
    /// <param name="accepted">The source of the accepted intents: the query of Core, or the plant of a test (D-1179).</param>
    /// <param name="saveFolder">The folder of the saves of this run alone. The runner makes it, and the caller removes it.</param>
    /// <returns>The result, with the record of the run.</returns>
    /// <exception cref="ArgumentNullException">An argument is null (T-2).</exception>
    public static BotResult Play(ContentSet content, ulong seed, IBotPolicy policy, AcceptedSource accepted, string saveFolder)
    {
        ArgumentNullException.ThrowIfNull(content);
        ArgumentNullException.ThrowIfNull(policy);
        ArgumentNullException.ThrowIfNull(accepted);
        ArgumentException.ThrowIfNullOrEmpty(saveFolder);

        BotRun run = new(content, seed, policy, accepted, new SaveStore(saveFolder));
        return run.PlayToEnd();
    }

    private BotResult PlayToEnd()
    {
        BotRules rules = this.content.Bots;
        while (true)
        {
            if (this.simulation.State.Story.Flags.IsOn(rules.GoalFlag))
            {
                this.goalPlayed = this.played;
                return this.Result(BotEnd.Complete, null);
            }

            if (this.played >= rules.TickBudget)
            {
                return this.Result(BotEnd.Budget, null);
            }

            // The record of a crash holds the intents of the tick that failed, because the
            // runner records the tick before it steps the rules. A replay then repeats the error
            // (T-7).
            try
            {
                if (this.PlayTick() is string softlock)
                {
                    return this.Result(BotEnd.Softlock, softlock);
                }
            }
            catch (Exception fault)
            {
                return this.Result(BotEnd.Crash, $"{fault.GetType().Name}: {fault.Message}");
            }
        }
    }

    /// <summary>Plays one tick, or reloads after a wipe.</summary>
    /// <returns>The description of a softlock, or no value.</returns>
    private string? PlayTick()
    {
        // A battle wipe and a wipe on the map each reload the newer save, as Game does (D-397).
        if (this.simulation.State.Battle is Battle { Outcome: BattleOutcome.Wiped } || this.simulation.State.MapWiped)
        {
            this.ReloadAfterWipe();
            return null;
        }

        IReadOnlyList<Intent> accepted = this.accepted(this.simulation);
        if (SoftlockCheck.Holds(this.content, this.simulation, accepted))
        {
            return $"at tick {this.simulation.Tick}, no intent of the {accepted.Count} that the state accepts changes the state (D-1179): {Describe(accepted)}";
        }

        Intent? chosen = this.policy.Choose(this.simulation.State, accepted);
        IReadOnlyList<Intent> intents = chosen is null ? NoIntents : [chosen];
        this.recorder.Step(this.simulation.Tick + 1, intents);
        _ = this.simulation.Step(intents);
        this.played = checked(this.played + 1);
        this.tally.Read(chosen, this.simulation.TakeBattleEvents());
        foreach (SaveRequestKind kind in this.simulation.TakeSaveRequests())
        {
            this.Save(kind);
        }

        // Game shows the notices and opens the window of each service. The policy reads the
        // state, so the runner drops both lists.
        _ = this.simulation.TakeNotices();
        _ = this.simulation.TakeOpenedServices();
        return null;
    }

    /// <summary>Writes one save through Storage and gives the record a new snapshot, as `GameRun.Save` does (D-651, D-1132).</summary>
    private void Save(SaveRequestKind kind)
    {
        RunSnapshot snapshot = this.simulation.Snapshot();
        this.recorder.Save(snapshot);
        SaveKind file = kind switch
        {
            SaveRequestKind.Slot => SaveKind.Slot,
            SaveRequestKind.Autosave => SaveKind.Autosave,
            _ => throw new ArgumentOutOfRangeException(nameof(kind), kind, "the value names no save request (D-1132)"),
        };
        this.saves.Write(file, new SaveDocument(SaveHeader.ForThisBuild(this.content.Hash, this.seed), snapshot));
    }

    /// <summary>
    /// Reloads the newer save of this run, or starts the run again from its seed when no save of
    /// it exists, as `GameRun.Reload` does (D-776, D-1114, D-1181). The record starts again from
    /// the snapshot of the reload.
    /// </summary>
    private void ReloadAfterWipe()
    {
        this.wipes = checked(this.wipes + 1);
        SaveDocument? slot = this.ReadSave(SaveKind.Slot);
        SaveDocument? autosave = this.ReadSave(SaveKind.Autosave);
        if (SavePick.NewerOf(slot, autosave, this.seed) is SaveDocument save)
        {
            ResumeDrift drift = ResumeDrift.Of(save.Header.OriginFor(this.content.Hash), save.Snapshot.Tick);
            this.simulation = Simulation.Resume(this.seed, save.Snapshot, MapSet.Of(this.content.Maps), this.content.Battle, this.content.Notices, this.content.Story, DebugIntentHandlers.None, drift);
        }
        else
        {
            this.simulation = this.StartAgain();
        }

        this.recorder = this.NewRecorder();
    }

    /// <summary>Starts the run at tick zero on the start map of its seed (D-1185).</summary>
    private Simulation StartAgain() => Simulation.Start(this.seed, MapSet.Of(this.content.Maps), this.content.Bots.StartOf(this.seed), this.content.Battle, this.content.Notices, this.content.Story, DebugIntentHandlers.None);

    private SaveDocument? ReadSave(SaveKind kind) => this.saves.Exists(kind) ? this.saves.Read(kind) : null;

    private RunRecorder NewRecorder() => new(RunHeader.ForThisBuild(this.content.Hash, this.seed), this.simulation.Snapshot());

    private BotResult Result(BotEnd end, string? message) =>
        new(this.seed, this.policy.Kind, end, this.simulation.Tick, this.played, this.goalPlayed, this.wipes, message, this.tally.Battles, this.recorder.Build(), this.simulation.StateHash());

    private static string Describe(IReadOnlyList<Intent> accepted)
    {
        List<string> names = [];
        foreach (Intent intent in accepted)
        {
            names.Add(intent.Describe());
        }

        return names.Count == 0 ? "none" : string.Join(", ", names);
    }
}
