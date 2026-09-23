using System;
using TheThingBelow.Core.Effects;

namespace TheThingBelow.Game;

/// <summary>The part of the hand-off between the map and a fight that the screen shows (D-522, D-938, D-939).</summary>
public enum HandOffPhase
{
    /// <summary>No transition and no fade plays.</summary>
    None,

    /// <summary>The transition breaks up the map until the view holds its cover color. The events of the fight wait (D-939).</summary>
    Into,

    /// <summary>The fight fades in from the cover color of the transition (D-939).</summary>
    FadeIn,

    /// <summary>The fight ended in a win or a flee, and the map fades in from the back cover. The world waits (D-522, D-938).</summary>
    Back,

    /// <summary>The fade back ended, and the run takes the wait intent on its next tick (D-522).</summary>
    Waiting,
}

/// <summary>
/// The transition into a fight, the fade into the fight, and the fade back to the map, counted in
/// ticks of the run (D-522, D-938, D-939, D-941).
/// </summary>
/// <remarks>
/// The hand-off counts the ticks of the run alone, and never a Godot timer or the frame time, so a
/// faster screen plays the same ticks, and a replay needs nothing from it. The wait intent that ends
/// the fade back sits in the run record (D-493, D-522, G-23).
/// <para>
/// This type holds no Godot value, so a test reads it from the built Game assembly with no engine
/// (D-614).
/// </para>
/// </remarks>
public sealed class ScreenHandOff
{
    /// <summary>The count of parts of a progress, from 0 at the start of a phase to this at its end.</summary>
    public const int ProgressScale = 1000;

    private readonly int fadeTicks;

    /// <summary>Makes a hand-off with no phase.</summary>
    /// <param name="fadeTicks">The ticks of each fade, from the transition table (D-938).</param>
    /// <exception cref="ArgumentOutOfRangeException">The fade holds no tick (T-2).</exception>
    public ScreenHandOff(int fadeTicks)
    {
        ArgumentOutOfRangeException.ThrowIfLessThan(fadeTicks, 1);

        this.fadeTicks = fadeTicks;
    }

    /// <summary>The phase that the screen shows now.</summary>
    public HandOffPhase Phase { get; private set; } = HandOffPhase.None;

    /// <summary>The transition of the fight that runs, or no value before the first fight of the run.</summary>
    public Transition? Transition { get; private set; }

    /// <summary>The tick of the run at which the phase started.</summary>
    public long Since { get; private set; }

    /// <summary>True while the events of the fight wait for the transition (D-939).</summary>
    public bool HoldsEvents => this.Phase == HandOffPhase.Into;

    /// <summary>True while the map shows again, from the start of the fade back until the wait intent ends the fight (D-938).</summary>
    public bool ShowsMapAgain => this.Phase is HandOffPhase.Back or HandOffPhase.Waiting;

    /// <summary>Starts the transition into a fight.</summary>
    /// <param name="transition">The transition that the table gives the fight (D-934).</param>
    /// <param name="tick">The tick of the run that starts the fight.</param>
    /// <exception cref="ArgumentNullException">The transition is null (T-2).</exception>
    /// <exception cref="InvalidOperationException">Another phase plays (T-2).</exception>
    public void StartInto(Transition transition, long tick)
    {
        ArgumentNullException.ThrowIfNull(transition);

        this.RefuseBusy("start the transition into a fight", tick);
        this.Transition = transition;
        this.Start(HandOffPhase.Into, tick);
    }

    /// <summary>Starts the fade back to the map after a win or a flee (D-938).</summary>
    /// <param name="tick">The tick of the run at which the screen played the last event of the fight.</param>
    /// <exception cref="InvalidOperationException">Another phase plays (T-2).</exception>
    public void StartBack(long tick)
    {
        this.RefuseBusy("start the fade back to the map", tick);
        this.Start(HandOffPhase.Back, tick);
    }

    /// <summary>Moves to the next phase when the ticks of a phase ran out.</summary>
    /// <param name="tick">The tick of the run now.</param>
    public void Follow(long tick)
    {
        if (this.Phase == HandOffPhase.Into && this.Elapsed(tick) >= this.TicksOf(HandOffPhase.Into))
        {
            this.Start(HandOffPhase.FadeIn, this.Since + this.TicksOf(HandOffPhase.Into));
        }

        if (this.Phase == HandOffPhase.FadeIn && this.Elapsed(tick) >= this.fadeTicks)
        {
            this.Start(HandOffPhase.None, this.Since + this.fadeTicks);
        }

        if (this.Phase == HandOffPhase.Back && this.Elapsed(tick) >= this.fadeTicks)
        {
            this.Start(HandOffPhase.Waiting, this.Since + this.fadeTicks);
        }
    }

    /// <summary>Ends the hand-off when the fight left the run, after the wait intent (D-522).</summary>
    /// <param name="tick">The tick of the run now.</param>
    public void End(long tick)
    {
        if (this.Phase != HandOffPhase.None)
        {
            this.Start(HandOffPhase.None, tick);
        }
    }

    /// <summary>Gives the part of the phase that ran, from 0 to <see cref="ProgressScale"/>.</summary>
    /// <param name="tick">The tick of the run now.</param>
    /// <returns>The progress. A phase with no length, and the phases <see cref="HandOffPhase.None"/> and <see cref="HandOffPhase.Waiting"/>, give the scale.</returns>
    public int ProgressOf(long tick)
    {
        int ticks = this.TicksOf(this.Phase);
        if (ticks == 0)
        {
            return ProgressScale;
        }

        long elapsed = Math.Clamp(this.Elapsed(tick), 0, ticks);
        return (int)(elapsed * ProgressScale / ticks);
    }

    private int TicksOf(HandOffPhase phase) => phase switch
    {
        HandOffPhase.Into => this.Transition?.Ticks ?? throw new InvalidOperationException("The transition into a fight plays with no transition (T-2)."),
        HandOffPhase.FadeIn or HandOffPhase.Back => this.fadeTicks,
        HandOffPhase.None or HandOffPhase.Waiting => 0,
        _ => throw new ArgumentOutOfRangeException(nameof(phase), phase, "The value names no phase of the hand-off (T-2)."),
    };

    private long Elapsed(long tick) => tick - this.Since;

    private void Start(HandOffPhase phase, long tick)
    {
        this.Phase = phase;
        this.Since = tick;
    }

    private void RefuseBusy(string action, long tick)
    {
        if (this.Phase != HandOffPhase.None)
        {
            throw new InvalidOperationException(
                $"The screen cannot {action} at tick {tick}, because the phase '{this.Phase}' plays since tick {this.Since} (D-522, T-2).");
        }
    }
}
