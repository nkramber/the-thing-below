using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Reflection;
using System.Runtime.CompilerServices;
using TheThingBelow.Core.Content;
using TheThingBelow.Tools.Content;
using Xunit;

namespace TheThingBelow.Tests;

/// <summary>
/// The name of a key on the remap screen (G-7, D-1128). The tests read the built Game assembly,
/// because Tests takes no reference to Game (D-614), and they read each key code from the enum
/// of the engine.
/// </summary>
public sealed class KeyNamesTests
{
    private static readonly Lazy<ContentSet> Content =
        new(() => ContentSet.Load(ContentFolder.Read(RepositoryRoot.Find())));

    /// <summary>
    /// The regression test of finding P3-24. On a French layout, the physical key W prints Z.
    /// The screen named the physical key, so it showed W for the key that the player presses as Z.
    /// </summary>
    [Fact]
    public void TheKeyOfAFrenchLayoutShowsItsOwnLetter()
    {
        (string id, IReadOnlyList<(string Place, string Value)> values) = Of(KeyCode("Z"));

        Assert.Equal("settings.binding_key", id);
        Assert.Equal([("key", "Z")], values);
    }

    [Theory]
    [InlineData(0x60, "`")]
    [InlineData(0x7A, "Z")]
    [InlineData(0xE9, "É")]
    [InlineData(0x31, "1")]
    public void AKeyThatPrintsASignShowsTheSignInUpperCase(long code, string sign)
    {
        // The engine named the key of 0x60 "QuoteLeft", an English word outside the table.
        (string id, IReadOnlyList<(string Place, string Value)> values) = Of(code);

        Assert.Equal("settings.binding_key", id);
        Assert.Equal([("key", sign)], values);
    }

    [Theory]
    [InlineData("Space", "settings.key_space")]
    [InlineData("Escape", "settings.key_escape")]
    [InlineData("Enter", "settings.key_enter")]
    [InlineData("Tab", "settings.key_tab")]
    [InlineData("Up", "settings.key_up")]
    [InlineData("Kpenter", "settings.key_keypad_enter")]
    [InlineData("Meta", "settings.key_meta")]
    public void ANamedKeyTakesItsOwnStringId(string key, string expected)
    {
        (string id, IReadOnlyList<(string Place, string Value)> values) = Of(KeyCode(key));

        Assert.Equal(expected, id);
        Assert.Empty(values);
    }

    [Theory]
    [InlineData("F1", "1")]
    [InlineData("F12", "12")]
    [InlineData("F35", "35")]
    public void AFunctionKeyShowsItsNumber(string key, string number)
    {
        (string id, IReadOnlyList<(string Place, string Value)> values) = Of(KeyCode(key));

        Assert.Equal("settings.key_function", id);
        Assert.Equal([("number", number)], values);
    }

    [Theory]
    [InlineData("Kp0", "0")]
    [InlineData("Kp9", "9")]
    public void ADigitOfTheNumberPadShowsItsDigit(string key, string digit)
    {
        (string id, IReadOnlyList<(string Place, string Value)> values) = Of(KeyCode(key));

        Assert.Equal("settings.key_keypad_digit", id);
        Assert.Equal([("digit", digit)], values);
    }

    [Fact]
    public void AKeyWithNoNameShowsItsCode()
    {
        long code = KeyCode("Special") + 0x7FFF;

        (string id, IReadOnlyList<(string Place, string Value)> values) = Of(code);

        Assert.Equal("settings.key_other", id);
        Assert.Equal([("code", code.ToString(System.Globalization.CultureInfo.InvariantCulture))], values);
    }

    /// <summary>Each id of a key name is in the string table, and its places are the places that the code fills (G-7).</summary>
    [Fact]
    public void EachKeyNameIdIsInTheTableWithItsPlaces()
    {
        Dictionary<string, string[]> places = new(StringComparer.Ordinal)
        {
            ["settings.binding_key"] = ["key"],
            ["settings.key_function"] = ["number"],
            ["settings.key_keypad_digit"] = ["digit"],
            ["settings.key_other"] = ["code"],
        };

        IReadOnlyList<string> ids = (IReadOnlyList<string>)KeyNamesType().GetProperty("Ids")!.GetValue(null)!;
        Assert.True(ids.Count > 30, $"The key names hold {ids.Count} ids, so the read broke (T-2).");
        foreach (string id in ids)
        {
            string text = Content.Value.Strings.Text(ContentId.Parse(id, "KeyNamesTests", "id"));
            string[] expected = places.TryGetValue(id, out string[]? named) ? named : [];
            string[] found = text.Split('{').Skip(1).Select(part => part[..part.IndexOf('}', StringComparison.Ordinal)]).ToArray();
            Assert.True(expected.SequenceEqual(found), $"The string '{id}' holds the places [{string.Join(", ", found)}], and the code fills [{string.Join(", ", expected)}].");
            Assert.True(text.Length <= 16, $"The string '{id}' is '{text}', longer than a menu label of 16 characters.");
        }
    }

    private static (string Id, IReadOnlyList<(string Place, string Value)> Values) Of(long code)
    {
        MethodInfo method = KeyNamesType().GetMethod("Of", [typeof(long)])
            ?? throw new InvalidOperationException("The key names hold no 'Of' method (T-2).");
        var result = (ITuple)method.Invoke(null, [code])!;
        return ((string)result[0]!, (IReadOnlyList<(string Place, string Value)>)result[1]!);
    }

    private static Type KeyNamesType() => GameAssemblyFile.Type("TheThingBelow.Game.Ui.KeyNames");

    /// <summary>Reads the code of one key from the enum of the engine, next to the Game assembly.</summary>
    private static long KeyCode(string name)
    {
        string folder = Path.GetDirectoryName(GameAssemblyFile.Load().Location)
            ?? throw new InvalidOperationException("The Game assembly has no folder (T-2).");
        Type key = Assembly.LoadFrom(Path.Combine(folder, "GodotSharp.dll")).GetType("Godot.Key")
            ?? throw new InvalidOperationException("GodotSharp holds no type 'Godot.Key' (T-2).");
        return Convert.ToInt64(Enum.Parse(key, name, ignoreCase: true), System.Globalization.CultureInfo.InvariantCulture);
    }
}
