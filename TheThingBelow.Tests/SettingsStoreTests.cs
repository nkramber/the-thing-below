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
    public void ARefusedFileIsKeptAsideAndReplacesAnOlderKeptFile()
    {
        // P2-2 (D-1099): a start keeps a refused file under its own name, and the newest refused
        // file replaces an older one, so the folder never fills.
        Directory.CreateDirectory(this.folder);
        File.WriteAllText(this.store.Path, "{ \"display\": {}, \"format\": 3 }");

        string kept = this.store.SetAside();

        Assert.Equal(Path.Combine(this.folder, SettingsStore.RefusedName), kept);
        Assert.False(this.store.Exists());
        Assert.Equal("{ \"display\": {}, \"format\": 3 }", File.ReadAllText(kept));

        File.WriteAllText(this.store.Path, "second");
        _ = this.store.SetAside();

        Assert.Equal("second", File.ReadAllText(kept));
        Assert.Single(Directory.GetFiles(this.folder));
    }

    [Fact]
    public void AKeepWithNoFileFailsWithThePath()
    {
        Directory.CreateDirectory(this.folder);

        StorageException error = Assert.Throws<StorageException>(() => this.store.SetAside());

        Assert.Equal(this.store.Path, error.Path);
        Assert.Contains(SettingsStore.RefusedName, error.Message, StringComparison.Ordinal);
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
