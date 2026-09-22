using System;
using System.IO;
using TheThingBelow.Storage;
using Xunit;

namespace TheThingBelow.Tests;

/// <summary>
/// The settings file beside the folder of the saves (D-860). Every test writes to a folder of
/// its own under the temporary folder of the machine, and no test reads the folder of the
/// person (D-465).
/// </summary>
public sealed class SettingsStoreTests : IDisposable
{
    private readonly string folder;
    private readonly SettingsStore store;

    public SettingsStoreTests()
    {
        this.folder = Path.Combine(Path.GetTempPath(), "the-thing-below-tests", Guid.NewGuid().ToString("n"));
        this.store = new SettingsStore(this.folder);
    }

    /// <summary>Removes the folder of this test.</summary>
    public void Dispose()
    {
        if (Directory.Exists(this.folder))
        {
            Directory.Delete(this.folder, recursive: true);
        }
    }

    [Fact]
    public void TheFileSitsBesideTheFolderOfTheSaves()
    {
        // D-860: a removal of the saves keeps the settings.
        Assert.Equal(Path.Combine(this.folder, "settings.json"), this.store.Path);
        Assert.NotEqual(SaveFolder.SavesName, Path.GetFileName(Path.GetDirectoryName(this.store.Path)));
    }

    [Fact]
    public void AWriteMakesTheFolderAndAReadGivesTheSettings()
    {
        GameSettings settings = SettingsFixtures.Defaults();

        this.store.Write(settings);

        Assert.True(this.store.Exists());
        Assert.Equal(settings, this.store.Read());
    }

    [Fact]
    public void ARemapLastsAcrossANewStore()
    {
        // Exit test 2: Godot saves no remap, so the settings file holds it (F-50).
        GameSettings settings = SettingsFixtures.Defaults();
        ControlBindings remapped = settings.Controls.Bindings
            .Rebind("confirm", InputBinding.OfKey(SettingsFixtures.Enter), InputBinding.OfKey(SettingsFixtures.W + 1));
        this.store.Write(settings with { Controls = settings.Controls with { Bindings = remapped } });

        GameSettings read = new SettingsStore(this.folder).Read();

        Assert.Equal(InputBinding.OfKey(SettingsFixtures.W + 1), read.Controls.Bindings.Of("confirm")[0]);
    }

    [Fact]
    public void AWriteWithAConflictFailsAndKeepsTheOldFile()
    {
        // Exit test 3: a conflict blocks the save (D-862).
        GameSettings settings = SettingsFixtures.Defaults();
        this.store.Write(settings);
        ControlBindings clash = settings.Controls.Bindings
            .Rebind("cancel", InputBinding.OfButton(SettingsFixtures.ButtonB), InputBinding.OfButton(SettingsFixtures.ButtonA));

        InvalidOperationException error = Assert.Throws<InvalidOperationException>(
            () => this.store.Write(settings with { Controls = settings.Controls with { Bindings = clash } }));

        Assert.Contains("cancel and confirm", error.Message, StringComparison.Ordinal);
        Assert.Equal(settings, this.store.Read());
    }

    [Fact]
    public void AReadWithNoFileFailsWithThePath()
    {
        StorageException error = Assert.Throws<StorageException>(() => this.store.Read());

        Assert.Equal(this.store.Path, error.Path);
    }
}
