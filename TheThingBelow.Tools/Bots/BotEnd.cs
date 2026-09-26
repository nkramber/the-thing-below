using System;

namespace TheThingBelow.Tools.Bots;

/// <summary>How a bot run ended (D-64, D-1179, D-1181, D-1184).</summary>
public enum BotEnd
{
    /// <summary>The goal flag of the bot rules came on.</summary>
    Complete = 1,

    /// <summary>No intent that the state accepts changes the state other than the tick. The job fails.</summary>
    Softlock = 2,

    /// <summary>An error stopped the run. The job fails.</summary>
    Crash = 3,

    /// <summary>The run played its tick budget with no other end.</summary>
    Budget = 4,
}

/// <summary>The names of the ends of a bot run, as the result lines write them.</summary>
public static class BotEnds
{
    /// <summary>Gives the name of an end.</summary>
    /// <param name="end">The end.</param>
    /// <returns>The name, such as `softlock`.</returns>
    /// <exception cref="ArgumentOutOfRangeException">The value names no end (T-2).</exception>
    public static string NameOf(BotEnd end) => end switch
    {
        BotEnd.Complete => "complete",
        BotEnd.Softlock => "softlock",
        BotEnd.Crash => "crash",
        BotEnd.Budget => "budget",
        _ => throw new ArgumentOutOfRangeException(nameof(end), end, "the value names no end of a bot run (D-64)"),
    };

    /// <summary>Tells whether an end fails the bot job (T-2).</summary>
    /// <param name="end">The end.</param>
    /// <returns>True for a softlock and a crash.</returns>
    public static bool Fails(BotEnd end) => end is BotEnd.Softlock or BotEnd.Crash;
}
