namespace TheThingBelow.Core;

/// <summary>
/// The version of the simulation rules. Every change of Core behavior raises this number,
/// and the review of that PR confirms the bump (G-17, D-259).
/// </summary>
public static class SimulationVersion
{
    /// <summary>
    /// The current version. PR-4 set the first value, because it held the first Core rules:
    /// the fixed-point math, the streams, and the state hash. PR-5 raised it to 2, because
    /// the content reader, the content ids, and the content hash are Core rules too. PR-6
    /// raised it to 3, because the tick, the intents, and the world of one tick are the
    /// first rules that change a state (G-17). The audit fixes of 2026-09-20 raised it to
    /// 4: the content reader refuses a repeated field, the recorder refuses a tick gap, and
    /// a snapshot refuses an increment that no stream of this build gives.
    /// </summary>
    /// <remarks>
    /// A run record carries this number, and a replay of a record with another number
    /// reports the two numbers and refuses the record (G-5, `RunHeader`). A save carries it
    /// as a label alone: a load reads the snapshot on the rules of this build (D-259). A
    /// change of this number also changes the expected hashes of the identity file (D-504).
    /// </remarks>
    public const int Current = 4;
}
