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
/// Format 1 is the first format (D-869). Format 2 added the map action, and the step from
/// format 1 adds its default key and button (D-986, D-990). The PR that raises
/// <see cref="SettingsFormat.Current"/> adds a step and a fixture file of the version that it
/// leaves, and <see cref="EveryFormatVersionHasAStoredFile"/> fails a raise with no fixture.
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
    public void TheStoredFileOfFormatOneHoldsTheFixtureDefaultsWithTheMapActionOfItsStep()
    {
        // Exit test 11 of PR-62: format 1 predates the map action, and its step adds the M key
        // and the Back button (D-986, D-990). Every other value stays.
        string path = PathOfFormat(1);

        Assert.Equal(SettingsMigration.FromFormatOne(SettingsFixtures.Defaults()), SettingsText.Read(File.ReadAllText(path), path));
    }

    [Fact]
    public void TheStoredFileOfFormatTwoHoldsTheSameSettingsAsItsMigratedFormatOne()
    {
        // PR-62 wrote format 2 from the migrated file of format 1.
        string one = PathOfFormat(1);
        string two = PathOfFormat(2);

        GameSettings read = SettingsText.Read(File.ReadAllText(two), two);

        Assert.Equal(SettingsText.Read(File.ReadAllText(one), one), read);
        Assert.Equal(
            [InputBinding.OfKey(SettingsMigration.MapKey), InputBinding.OfButton(SettingsMigration.MapButton)],
            read.Controls.Bindings.Of(SettingsMigration.MapAction));
    }

    [Fact]
    public void AFileOfFormatOneThatHoldsTheMapActionFails()
    {
        // T-2: format 1 predates the map action, so a file of that format with it is a fault.
        string path = PathOfFormat(2);
        string text = File.ReadAllText(path).Replace("\"format\": 2", "\"format\": 1", System.StringComparison.Ordinal);

        StorageException error = Assert.Throws<StorageException>(() => SettingsText.Read(text, path));

        Assert.Contains("D-986", error.Message, System.StringComparison.Ordinal);
    }

    [Fact]
    public void TheStepFromFormatOneKeepsABindingOfThePlayerThatTheMapDefaultsMeet()
    {
        // D-862: a player who bound M to the menu in format 1 keeps it, and the settings screen
        // names the conflict until a remap ends it. The step drops no binding.
        GameSettings defaults = SettingsFixtures.Defaults();
        GameSettings bound = defaults with
        {
            Controls = defaults.Controls with
            {
                Bindings = defaults.Controls.Bindings.Rebind("confirm", null, InputBinding.OfKey(SettingsMigration.MapKey)),
            },
        };

        GameSettings migrated = SettingsMigration.FromFormatOne(bound);

        Assert.Contains(InputBinding.OfKey(SettingsMigration.MapKey), migrated.Controls.Bindings.Of("confirm"));
        BindingConflict conflict = Assert.Single(migrated.Controls.Bindings.FindConflicts());
        Assert.Equal(["confirm", "map"], conflict.Actions);
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
