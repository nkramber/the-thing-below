namespace TheThingBelow.Core;

/// <summary>
/// The version of the simulation rules. Every change of Core behavior raises this number,
/// and the review of that PR confirms the bump (G-17, D-259).
/// </summary>
public static class SimulationVersion
{
    /// <summary>
    /// The current version. PR-4 sets the first value, because it holds the first Core
    /// rules: the fixed-point math, the streams, and the state hash (G-17).
    /// </summary>
    /// <remarks>
    /// A run record and a snapshot carry this number. A load that reads another number
    /// reports the two numbers and refuses the file (D-259). A change of this number also
    /// changes the expected hashes of the identity file (D-504).
    /// </remarks>
    public const int Current = 1;
}
