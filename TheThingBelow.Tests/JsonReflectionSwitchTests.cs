using System;
using System.IO;
using System.Xml.Linq;
using Xunit;

namespace TheThingBelow.Tests;

/// <summary>
/// The JSON support of .NET uses reflection by default (F-36). `Directory.Build.props` sets
/// the switch to `false`, so each program of the solution carries it (D-647).
/// </summary>
/// <remarks>
/// The design critic of PR #12 claimed that the switch on a class library reaches no
/// program, because a class library writes no `runtimeconfig.json`. The first test below
/// reads the switch in the running test host, which settles the claim with a measurement.
/// </remarks>
public sealed class JsonReflectionSwitchTests
{
    /// <summary>The name that the build property writes into a `runtimeconfig.json`.</summary>
    private const string SwitchName = "System.Text.Json.JsonSerializer.IsReflectionEnabledByDefault";

    /// <summary>The property of `Directory.Build.props` that sets the switch.</summary>
    private const string PropertyName = "JsonSerializerIsReflectionEnabledByDefault";

    [Fact]
    public void TheSwitchIsOffInThisProgram()
    {
        bool found = AppContext.TryGetSwitch(SwitchName, out bool enabled);

        Assert.True(found, $"The program carries no '{SwitchName}' switch (D-647, F-36).");
        Assert.False(enabled, $"The switch '{SwitchName}' is on, and D-647 sets it off.");
    }

    [Fact]
    public void TheRuntimeConfigOfThisProgramHoldsTheSwitch()
    {
        // The switch reaches a program through its `runtimeconfig.json`. This test reads the
        // file that the build wrote beside the test assembly, which is the evidence of F-36.
        string path = Path.Combine(AppContext.BaseDirectory, "TheThingBelow.Tests.runtimeconfig.json");

        string config = File.ReadAllText(path);

        Assert.Contains(SwitchName, config, StringComparison.Ordinal);
    }

    [Fact]
    public void TheBuildPropertiesSetTheSwitchForEveryProject()
    {
        XDocument properties = XDocument.Load(RepositoryRoot.PathTo("Directory.Build.props"));

        XElement? property = FindProperty(properties, PropertyName);

        Assert.NotNull(property);
        Assert.Equal("false", property.Value);
    }

    private static XElement? FindProperty(XDocument document, string name)
    {
        foreach (XElement element in document.Descendants())
        {
            if (string.Equals(element.Name.LocalName, name, StringComparison.Ordinal))
            {
                return element;
            }
        }

        return null;
    }
}
