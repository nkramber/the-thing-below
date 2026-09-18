using System;

namespace TheThingBelow.Core;

/// <summary>
/// The context that a Core error carries inside a run: the seed of the run, the tick, and
/// the ids of the thing that the rule works on (T-2, G-18).
/// </summary>
/// <remarks>
/// This type is a class and not a struct. A default struct value carries no subject, and an
/// error must never report an absent subject as an empty one (T-2).
/// </remarks>
public sealed class RunContext
{
    /// <summary>Makes a context from the seed of the run, the tick, and the ids.</summary>
    /// <param name="seed">The seed that started the run (G-3).</param>
    /// <param name="tick">The tick of the step that runs now. A step outside a run uses 0.</param>
    /// <param name="subject">
    /// The ids of the thing that the rule works on, such as `battle/actor-3`. The text must
    /// have at least one character, because an error with an empty subject names nothing.
    /// </param>
    /// <exception cref="ArgumentException">The subject has no character.</exception>
    /// <exception cref="ArgumentOutOfRangeException">The tick is below zero.</exception>
    public RunContext(ulong seed, long tick, string subject)
    {
        ArgumentException.ThrowIfNullOrEmpty(subject);
        ArgumentOutOfRangeException.ThrowIfNegative(tick);

        this.Seed = seed;
        this.Tick = tick;
        this.Subject = subject;
    }

    /// <summary>The seed that started the run.</summary>
    public ulong Seed { get; }

    /// <summary>The tick of the step that runs now.</summary>
    public long Tick { get; }

    /// <summary>The ids of the thing that the rule works on.</summary>
    public string Subject { get; }

    /// <summary>Gives the context as one line for an error message (T-2).</summary>
    /// <returns>The seed, the tick, and the subject.</returns>
    public string Describe() => $"seed {this.Seed}, tick {this.Tick}, subject {this.Subject}";
}
