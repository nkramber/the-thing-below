namespace TheThingBelow.Core;

/// <summary>
/// Names the assembly that holds the simulation. A test reads the reference list of this
/// assembly and fails on an engine, file, network, clock, or OS dependency (G-1, D-100).
/// </summary>
public static class CoreAssembly
{
    /// <summary>
    /// The name of the assembly that holds the simulation. The member gives the name and
    /// never the `Assembly` object, because Core runs with no reflection (D-614, F-36).
    /// </summary>
    public const string Name = "TheThingBelow.Core";
}
