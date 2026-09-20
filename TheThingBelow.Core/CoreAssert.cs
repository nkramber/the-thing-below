using System;

namespace TheThingBelow.Core;

/// <summary>
/// The assertion helper of Core. An assertion stays on in every configuration, a release
/// export included (T-2, G-18).
/// </summary>
/// <remarks>
/// Core never calls `Debug.Assert`. A release export drops every `Debug.Assert` call, so a
/// broken rule would run on and give a wrong result in silence. The methods below hold no
/// `Conditional` attribute, so the compiler keeps every call.
/// </remarks>
public static class CoreAssert
{
    /// <summary>Throws when the condition is false.</summary>
    /// <param name="condition">The condition that the rule expects.</param>
    /// <param name="message">What the rule expected, such as `the party holds one member`.</param>
    /// <param name="context">The seed, the tick, and the ids.</param>
    /// <exception cref="SimulationException">The condition is false.</exception>
    public static void That(bool condition, string message, RunContext context)
    {
        ArgumentException.ThrowIfNullOrEmpty(message);
        ArgumentNullException.ThrowIfNull(context);

        if (!condition)
        {
            throw new SimulationException($"assertion failed: {message}", context);
        }
    }
}
