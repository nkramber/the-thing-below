using System.Globalization;
using System.IO;
using System.Linq;
using System.Reflection;
using TheThingBelow.Core;
using TheThingBelow.Storage;
using Xunit;

namespace TheThingBelow.Tests;

/// <summary>
/// The stored settings file of each format version (D-570, D-869), and the rule that no
/// setting reaches a run record (T-7).
/// </summary>
/// <remarks>
/// Format 1 is the first format, and the chain of migration steps holds no step yet (D-869).
/// The PR that raises <see cref="SettingsFormat.Current"/> adds a step and a fixture file of
/// the version that it leaves, and <see cref="EveryFormatVersionHasAStoredFile"/> fails a
/// raise with no fixture.
/// </remarks>
public sealed class SettingsFixtureTests
{
    private const string FixtureFolder = "TheThingBelow.Tests/settings";

    [Fact]
    public void EveryFormatVersionHasAStoredFile()
    {
        for (int version = SettingsFormat.Oldest; version <= SettingsFormat.Current; version += 1)
        {
            string path = PathOfFormat(version);
            Assert.True(
                File.Exists(path),
                $"Format version {version} has no stored settings file at '{path}'. A PR that raises " +
                "the format version commits a fixture file of the version that it leaves (D-570).");
        }
    }

    [Fact]
    public void EveryStoredFileReadsWithThisBuild()
    {
        // Exit test 4 (D-869).
        for (int version = SettingsFormat.Oldest; version <= SettingsFormat.Current; version += 1)
        {
            string path = PathOfFormat(version);
            SettingsText.Read(File.ReadAllText(path), path);
        }
    }

    [Fact]
    public void TheStoredFileOfFormatOneHoldsTheFixtureDefaults()
    {
        string path = PathOfFormat(1);

        Assert.Equal(SettingsFixtures.Defaults(), SettingsText.Read(File.ReadAllText(path), path));
    }

    [Fact]
    public void NoSettingReachesARunRecord()
    {
        // Exit test 6. Core writes every run record, and Core takes no reference to Storage,
        // which holds every setting (T-7, G-1, D-860).
        string storage = typeof(GameSettings).Assembly.GetName().Name!;
        AssemblyName[] references = typeof(SimulationVersion).Assembly.GetReferencedAssemblies();

        Assert.DoesNotContain(references, reference => reference.Name == storage);
        Assert.DoesNotContain(
            typeof(SimulationVersion).Assembly.GetTypes(),
            type => type.Name.Contains("Settings", System.StringComparison.Ordinal));
    }

    private static string PathOfFormat(int version) =>
        RepositoryRoot.PathTo($"{FixtureFolder}/format-{version.ToString(CultureInfo.InvariantCulture)}.json");
}
