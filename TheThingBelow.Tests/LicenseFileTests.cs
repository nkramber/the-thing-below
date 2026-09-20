using System;
using System.IO;
using System.Linq;
using Xunit;

namespace TheThingBelow.Tests;

/// <summary>
/// The license files that every export carries: the notices of Godot, the Terminus fonts,
/// and the .NET runtime (D-467, D-691).
/// </summary>
public sealed class LicenseFileTests
{
    /// <summary>The folder of the checkout that the export job copies into each build.</summary>
    private const string LicenseFolder = "licenses";

    /// <summary>The file of each notice of D-467, and a phrase that only that notice holds.</summary>
    public static TheoryData<string, string> Notices { get; } = new TheoryData<string, string>
    {
        { "godot.txt", "Godot Engine contributors" },
        { "dotnet.txt", ".NET Foundation and Contributors" },
        { "terminus.txt", "SIL OPEN FONT LICENSE Version 1.1" },
    };

    [Fact]
    public void TheFolderHoldsOneFileForEachNoticeOfD467AndNoOther()
    {
        string[] names = Directory
            .GetFiles(RepositoryRoot.PathTo(LicenseFolder))
            .Select(Path.GetFileName)
            .OfType<string>()
            .Order(StringComparer.Ordinal)
            .ToArray();

        Assert.Equal(["dotnet.txt", "godot.txt", "terminus.txt"], names);
    }

    [Theory]
    [MemberData(nameof(Notices))]
    public void EachFileHoldsItsNotice(string name, string phrase)
    {
        string text = File.ReadAllText(RepositoryRoot.PathTo($"{LicenseFolder}/{name}"));

        Assert.Contains(phrase, text, StringComparison.Ordinal);
    }

    [Theory]
    [MemberData(nameof(Notices))]
    public void EachFileEndsEveryLineWithOneLineFeed(string name, string phrase)
    {
        Assert.NotEmpty(phrase);
        string text = File.ReadAllText(RepositoryRoot.PathTo($"{LicenseFolder}/{name}"));

        // One line ending on every platform, which the `.gitattributes` rule of D-495 sets.
        // A carriage return would give the Windows leg other bytes than the other two legs.
        Assert.DoesNotContain('\r', text);
        Assert.EndsWith("\n", text, StringComparison.Ordinal);
    }

    [Fact]
    public void TheTerminusNoticeNamesTheFontOfD263AndD264()
    {
        string text = File.ReadAllText(RepositoryRoot.PathTo($"{LicenseFolder}/terminus.txt"));

        // The OFL asks for its notice and its license with every copy of the font, and the
        // reserved names are part of the notice (D-263, D-264, D-467).
        Assert.Contains("Reserved Font Name \"Terminus Font\"", text, StringComparison.Ordinal);
        Assert.Contains("Reserved Font Name \"Terminus (TTF)\"", text, StringComparison.Ordinal);
    }
}
