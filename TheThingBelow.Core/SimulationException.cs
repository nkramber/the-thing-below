using System;

namespace TheThingBelow.Core;

/// <summary>
/// The error that a Core rule throws. The message always carries the run context, so no
/// caller must add it again (T-2, G-18).
/// </summary>
public sealed class SimulationException : Exception
{
    /// <summary>Makes an error from a message and the run context.</summary>
    /// <param name="message">What the rule found, such as `a division by zero`.</param>
    /// <param name="context">The seed, the tick, and the ids.</param>
    public SimulationException(string message, RunContext context)
        : base(Describe(message, context))
    {
        this.Context = context;
    }

    /// <summary>Makes an error from a message, the run context, and the error below it.</summary>
    /// <param name="message">What the rule found.</param>
    /// <param name="context">The seed, the tick, and the ids.</param>
    /// <param name="inner">The error that the rule caught, such as an overflow.</param>
    public SimulationException(string message, RunContext context, Exception inner)
        : base(Describe(message, context), inner)
    {
        this.Context = context;
    }

    /// <summary>The seed, the tick, and the ids of the step that failed.</summary>
    public RunContext Context { get; }

    private static string Describe(string message, RunContext context)
    {
        ArgumentException.ThrowIfNullOrEmpty(message);
        ArgumentNullException.ThrowIfNull(context);

        return $"{message} ({context.Describe()})";
    }
}
