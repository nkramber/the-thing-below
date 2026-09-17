using System.Reflection;

namespace TheThingBelow.Core;

/// <summary>
/// Names the assembly that holds the simulation. A test reads the reference list of this
/// assembly and fails on an engine, file, network, clock, or OS dependency (G-1, D-100).
/// </summary>
public static class CoreAssembly
{
    /// <summary>The assembly that holds the simulation.</summary>
    public static Assembly Self => typeof(CoreAssembly).Assembly;
}
