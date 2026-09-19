using System;
using System.Collections.Generic;
using System.Linq;
using Xunit;

namespace TheThingBelow.Tests;

/// <summary>
/// The export presets of PR-54. The game supports three targets and nothing else, and each
/// CI leg exports the build of its own system (D-481, D-482, D-503).
/// </summary>
/// <remarks>
/// A Godot export fails on a wrong option in ways that a build and a test never show, so
/// these tests read the committed preset file and the project settings (F-73, F-74, F-90).
/// </remarks>
public sealed class ExportPresetTests
{
    /// <summary>The preset file of the Godot project, which the export job reads.</summary>
    private const string PresetPath = "TheThingBelow.Game/export_presets.cfg";

    /// <summary>The project file, which holds the import settings of the textures.</summary>
    private const string ProjectPath = "TheThingBelow.Game/project.godot";

    /// <summary>The section of the macOS preset, which is the third preset of the file.</summary>
    private const string MacosOptions = "preset.2.options";

    /// <summary>The preset name and the Godot platform of each target, in the file order.</summary>
    private static readonly (string Name, string Platform)[] TargetRows =
    [
        ("Linux", "Linux"),
        ("Windows", "Windows Desktop"),
        ("macOS", "macOS"),
    ];

    /// <summary>The name and the Godot platform of each target of D-481.</summary>
    public static TheoryData<string, string> Targets { get; } = new TheoryData<string, string>
    {
        { "Linux", "Linux" },
        { "Windows", "Windows Desktop" },
        { "macOS", "macOS" },
    };

    /// <summary>The section of each preset, and the architecture that D-481 or D-482 sets.</summary>
    public static TheoryData<string, string> Architectures { get; } = new TheoryData<string, string>
    {
        { "preset.0.options", "\"x86_64\"" },
        { "preset.1.options", "\"x86_64\"" },
        { MacosOptions, "\"universal\"" },
    };

    [Fact]
    public void TheFileHoldsOnePresetForEachTargetOfD481AndNoOther()
    {
        IReadOnlyDictionary<string, IReadOnlyDictionary<string, string>> sections =
            GodotConfigFile.Read(PresetPath);

        (string Name, string Platform)[] presets = PresetSections(sections)
            .Select(section => (
                Name: GodotConfigFile.ValueOf(sections, section, "name"),
                Platform: GodotConfigFile.ValueOf(sections, section, "platform")))
            .ToArray();

        (string Name, string Platform)[] wanted = TargetRows
            .Select(row => (Name: $"\"{row.Name}\"", Platform: $"\"{row.Platform}\""))
            .ToArray();

        Assert.Equal(wanted, presets);
    }

    [Theory]
    [MemberData(nameof(Targets))]
    public void EachTargetOfD481HasOneRunnablePreset(string name, string platform)
    {
        IReadOnlyDictionary<string, IReadOnlyDictionary<string, string>> sections =
            GodotConfigFile.Read(PresetPath);

        string[] owners = PresetSections(sections)
            .Where(section => GodotConfigFile.ValueOf(sections, section, "name") == $"\"{name}\"")
            .ToArray();

        Assert.True(
            owners.Length == 1,
            $"The file '{PresetPath}' holds {owners.Length} presets named '{name}', and the " +
            $"export job of that leg names one (D-481, T-2).");
        Assert.Equal($"\"{platform}\"", GodotConfigFile.ValueOf(sections, owners[0], "platform"));
        Assert.Equal("true", GodotConfigFile.ValueOf(sections, owners[0], "runnable"));
    }

    [Fact]
    public void NoPresetHoldsAnIncludeFilter()
    {
        IReadOnlyDictionary<string, IReadOnlyDictionary<string, string>> sections =
            GodotConfigFile.Read(PresetPath);

        foreach (string section in PresetSections(sections))
        {
            // The Game assembly carries every content file and each font as a resource, so
            // the export needs no filter and no copy (D-508, F-42, F-73).
            Assert.Equal("\"all_resources\"", GodotConfigFile.ValueOf(sections, section, "export_filter"));
            Assert.Equal("\"\"", GodotConfigFile.ValueOf(sections, section, "include_filter"));
            Assert.Equal("\"\"", GodotConfigFile.ValueOf(sections, section, "exclude_filter"));
        }
    }

    [Theory]
    [MemberData(nameof(Architectures))]
    public void EachPresetTakesTheBinaryFormatOfItsTarget(string section, string architecture)
    {
        IReadOnlyDictionary<string, IReadOnlyDictionary<string, string>> sections =
            GodotConfigFile.Read(PresetPath);

        Assert.Equal(
            architecture,
            GodotConfigFile.ValueOf(sections, section, "binary_format/architecture"));
    }

    [Fact]
    public void TheMacosPresetAndTheProjectTakeTheEtc2AstcFormat()
    {
        IReadOnlyDictionary<string, IReadOnlyDictionary<string, string>> presets =
            GodotConfigFile.Read(PresetPath);
        IReadOnlyDictionary<string, IReadOnlyDictionary<string, string>> project =
            GodotConfigFile.Read(ProjectPath);

        // An export of the universal binary fails with a configuration error when either of
        // these two settings is off. The preset option alone is not enough, because the
        // check reads the import setting of the project (F-74).
        Assert.Equal("true", GodotConfigFile.ValueOf(presets, MacosOptions, "texture_format/etc2_astc"));
        Assert.Equal(
            "true",
            GodotConfigFile.ValueOf(project, "rendering", "textures/vram_compression/import_etc2_astc"));
    }

    [Fact]
    public void TheMacosPresetSignsWithTheCommandOfTheXcodeTools()
    {
        IReadOnlyDictionary<string, IReadOnlyDictionary<string, string>> sections =
            GodotConfigFile.Read(PresetPath);

        // The built-in signer of Godot writes an ad-hoc signature that the kernel refuses,
        // and the exported game dies with signal 9 and no output. Value 3 calls the
        // `codesign` command of the Xcode tools (F-90). PR-79 sets the real identity (D-553).
        Assert.Equal("3", GodotConfigFile.ValueOf(sections, MacosOptions, "codesign/codesign"));
        Assert.Equal("\"-\"", GodotConfigFile.ValueOf(sections, MacosOptions, "codesign/identity"));
        Assert.Equal("0", GodotConfigFile.ValueOf(sections, MacosOptions, "notarization/notarization"));
    }

    /// <summary>Gives the section of each preset, in the order of the file.</summary>
    /// <param name="sections">The sections that <see cref="GodotConfigFile.Read"/> returned.</param>
    /// <returns>The names such as `preset.0`, and never the `preset.0.options` sections.</returns>
    private static IReadOnlyList<string> PresetSections(
        IReadOnlyDictionary<string, IReadOnlyDictionary<string, string>> sections)
    {
        return sections.Keys
            .Where(name => name.StartsWith("preset.", StringComparison.Ordinal))
            .Where(name => !name.EndsWith(".options", StringComparison.Ordinal))
            .ToArray();
    }
}
