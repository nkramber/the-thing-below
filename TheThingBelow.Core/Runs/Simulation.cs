using System;
using System.Collections.Generic;

namespace TheThingBelow.Core.Runs;

/// <summary>
/// One run of the rules. The host calls <see cref="Step"/> 60 times a second on a fixed
/// step, and Core counts the ticks and reads no clock (D-164, G-3).
/// </summary>
/// <remarks>
/// A step applies the intents of its tick in the order of the list, and then it runs the
/// world (D-168). The tick rises on every step, a step with a menu open included, so the
/// tick is the one time line of a run and no intent needs a second order value (D-650).
/// <para>
/// A debug intent goes to the handlers that the host passed at the start. A host with no
/// handler for that action refuses the intent, and the report names the intent and the tick
/// (D-171, D-260, D-492, T-2).
/// </para>
/// </remarks>
public sealed class Simulation
{
    private readonly DebugIntentHandlers debugHandlers;

    private Simulation(RunState state, DebugIntentHandlers debugHandlers)
    {
        this.State = state;
        this.debugHandlers = debugHandlers;
    }

    /// <summary>The state of the run.</summary>
    public RunState State { get; }

    /// <summary>The count of ticks since the start of the run (D-164).</summary>
    public long Tick => this.State.Tick;

    /// <summary>Starts a run at tick zero.</summary>
    /// <param name="seed">The seed of the run (G-3, G-4).</param>
    /// <param name="debugHandlers">
    /// The extra intent handlers of the host. A release build passes
    /// <see cref="DebugIntentHandlers.None"/> (D-260, D-492).
    /// </param>
    /// <returns>The run.</returns>
    /// <exception cref="ArgumentNullException">The handler set is null (T-2).</exception>
    public static Simulation Start(ulong seed, DebugIntentHandlers debugHandlers)
    {
        ArgumentNullException.ThrowIfNull(debugHandlers);

        return new Simulation(RunState.Start(seed), debugHandlers);
    }

    /// <summary>Starts a run again from a snapshot (D-651).</summary>
    /// <param name="seed">The seed of the run, which the record header holds (G-5).</param>
    /// <param name="snapshot">The snapshot that the record or the save holds.</param>
    /// <param name="debugHandlers">The extra intent handlers of the host (D-260).</param>
    /// <returns>The run, at the tick of the snapshot.</returns>
    /// <exception cref="ArgumentNullException">The snapshot or the handler set is null (T-2).</exception>
    /// <exception cref="ArgumentException">The snapshot is not a state of a run (T-2).</exception>
    public static Simulation Resume(ulong seed, RunSnapshot snapshot, DebugIntentHandlers debugHandlers)
    {
        ArgumentNullException.ThrowIfNull(snapshot);
        ArgumentNullException.ThrowIfNull(debugHandlers);

        return new Simulation(RunState.Resume(seed, snapshot), debugHandlers);
    }

    /// <summary>Runs one tick of the rules.</summary>
    /// <param name="intents">The intents of this tick, in the order that the host made them.</param>
    /// <exception cref="ArgumentNullException">The list or one intent is null (T-2).</exception>
    /// <exception cref="SimulationException">
    /// An intent names no rule of this build, a debug intent has no handler, or a rule
    /// refuses the intent. Every message carries the seed, the tick, and the intent (T-2).
    /// </exception>
    public void Step(IReadOnlyList<Intent> intents)
    {
        ArgumentNullException.ThrowIfNull(intents);

        // The tick rises first, so every error of this step names the tick that the record
        // holds for these intents (D-650, T-2).
        this.State.CountTick();

        foreach (Intent intent in intents)
        {
            ArgumentNullException.ThrowIfNull(intent);
            this.Apply(intent);
        }

        if (!this.State.MenuOpen)
        {
            WorldRules.Step(this.State);
        }
    }

    /// <summary>Stores the whole state of the run (F-10, D-651).</summary>
    /// <returns>The snapshot.</returns>
    public RunSnapshot Snapshot() => this.State.Snapshot();

    /// <summary>Computes the state hash that a replay compares (G-5).</summary>
    /// <returns>The hash of the state.</returns>
    public ulong StateHash() => this.State.StateHash();

    private void Apply(Intent intent)
    {
        RunContext context = this.State.Context($"intent/{intent.Describe()}");

        if (intent.IsDebug)
        {
            if (!this.debugHandlers.TryFind(intent.Action, out DebugIntentHandler? handler))
            {
                throw new SimulationException(
                    $"the debug intent '{intent.Action.Value}' has no handler, and this host passed {this.debugHandlers.Count} handlers (D-260, D-492)",
                    context);
            }

            handler!(this.State, context);
            return;
        }

        if (string.CompareOrdinal(intent.Action.Value, IntentIds.OpenMenu.Value) == 0)
        {
            this.State.SetMenuOpen(true, context);
            return;
        }

        if (string.CompareOrdinal(intent.Action.Value, IntentIds.CloseMenu.Value) == 0)
        {
            this.State.SetMenuOpen(false, context);
            return;
        }

        throw new SimulationException(
            $"the intent '{intent.Action.Value}' names no rule of this build",
            context);
    }
}
