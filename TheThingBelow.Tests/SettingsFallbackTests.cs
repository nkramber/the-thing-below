using System;
using System.Reflection;
using TheThingBelow.Storage;
using Xunit;

namespace TheThingBelow.Tests;

/// <summary>
/// The field that the message of a refused settings file names (finding P2-2 of the repository
/// review, D-1099, D-1100). A start that refused its settings file quit with no text, and every
/// later start did the same. The start now keeps the file aside, runs on the defaults, and names
/// the field that failed.
/// </summary>
public sealed class SettingsFallbackTests
{
    private const string File = "settings.json";

    [Theory]
    [InlineData("\"format\": 3", "\"format\": 4", "format")]
    [InlineData("\"window\":", "\"windows\":", "windows")]
    public void ARefusalOfTheReaderNamesItsField(string from, string to, string field)
    {
        // A file of a newer build (D-1100), and a field that no format declares.
        string text = SettingsText.Write(SettingsFixtures.Defaults()).Replace(from, to, StringComparison.Ordinal);
        Assert.NotEqual(SettingsText.Write(SettingsFixtures.Defaults()), text);

        StorageException refused = Assert.Throws<StorageException>(() => SettingsText.Read(text, File));

        Assert.Contains(field, FieldOf(refused), StringComparison.Ordinal);
    }

    [Fact]
    public void AValueOutsideItsRangeNamesTheWholeFile()
    {
        // The check of a range keeps no field of the file, so the message names the whole file,
        // and the warning line of the log holds the whole error (D-1099).
        string text = SettingsText.Write(SettingsFixtures.Defaults()).Replace("\"cancel\":", "\"ui_cancel\":", StringComparison.Ordinal);

        StorageException refused = Assert.Throws<StorageException>(() => SettingsText.Read(text, File));

        Assert.Equal("the file", FieldOf(refused));
    }

    [Fact]
    public void AFileWhoseFirstFieldIsNotTheFormatNamesTheFormat()
    {
        StorageException refused = Assert.Throws<StorageException>(() => SettingsText.Read("{\"display\": {}, \"format\": 1}", File));

        Assert.Contains("format", FieldOf(refused), StringComparison.Ordinal);
    }

    [Fact]
    public void BindingsThatLackAnActionNameTheBindingsAndASystemRefusalNamesTheFile()
    {
        Assert.Equal("controls.bindings", FieldOf(new InvalidOperationException("The bindings of the settings lack [torch].")));
        Assert.Equal("the file", FieldOf(StorageException.ForPath(File, "the system refused the read")));
    }

    private static string FieldOf(Exception refused)
    {
        MethodInfo method = GameAssemblyFile.Type("TheThingBelow.Game.Ui.SettingsFallback").GetMethod("FieldOf")!;
        return (string)method.Invoke(null, [refused])!;
    }
}
